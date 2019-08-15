﻿//
// Copyright (c) Seal Report, Eric Pfirsch (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using Seal.Converter;
using Seal.Forms;
using System.Xml.Serialization;
using DynamicTypeDescriptor;
using System.Data.OleDb;
using System.Data.Odbc;
using Seal.Helpers;
using System.Windows.Forms;
using System.Data.Common;
using System.Data;

namespace Seal.Model
{
    /// <summary>
    /// A MetaConnection defines a connection to a database
    /// </summary>
    public class MetaConnection : RootComponent
    {
        /// <summary>
        /// Current MetaSource
        /// </summary>
        [XmlIgnore]
        public MetaSource Source = null;

        #region Editor

        static string PasswordKey = "1awéàèüwienyjhdl+256()$$";

        protected override void UpdateEditorAttributes()
        {
            if (_dctd != null)
            {
                //Disable all properties
                foreach (var property in Properties) property.SetIsBrowsable(false);
                //Then enable
                GetProperty("Name").SetIsBrowsable(true);
                GetProperty("DatabaseType").SetIsBrowsable(true);
                GetProperty("DateTimeFormat").SetIsBrowsable(true);
                if (IsEditable) GetProperty("ConnectionString").SetIsBrowsable(true);
                else GetProperty("ConnectionString2").SetIsBrowsable(true);
                GetProperty("UserName").SetIsBrowsable(true);
                if (IsEditable) GetProperty("ClearPassword").SetIsBrowsable(true);
                
                GetProperty("Information").SetIsBrowsable(true);
                GetProperty("Error").SetIsBrowsable(true);
                GetProperty("HelperCheckConnection").SetIsBrowsable(true);
                if (IsEditable) GetProperty("HelperCreateFromExcelAccess").SetIsBrowsable(true);

                GetProperty("Information").SetIsReadOnly(true);
                GetProperty("Error").SetIsReadOnly(true);
                GetProperty("HelperCheckConnection").SetIsReadOnly(true);
                if (IsEditable) GetProperty("HelperCreateFromExcelAccess").SetIsReadOnly(true);

                GetProperty("DateTimeFormat").SetIsReadOnly(!IsEditable || DatabaseType == DatabaseType.MSAccess || DatabaseType == DatabaseType.MSExcel);

                TypeDescriptor.Refresh(this);
            }
        }
        #endregion

        /// <summary>
        /// Create a basic connection into a source
        /// </summary>
        public static MetaConnection Create(MetaSource source)
        {
            return new MetaConnection() { Name = "connection", GUID = Guid.NewGuid().ToString(), Source = source };
        }

        /// <summary>
        /// The name of the connection
        /// </summary>
        [DefaultValue(null)]
        [DisplayName("Name"), Description("The name of the connection."), Category("Definition"), Id(1, 1)]
        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The type of the source database
        /// </summary>
        [DefaultValue(DatabaseType.Standard)]
        [DisplayName("Database type"), Description("The type of the source database."), Category("Definition"), Id(2, 1)]
        [TypeConverter(typeof(NamedEnumConverter))]
        public DatabaseType DatabaseType { get; set; } = DatabaseType.Standard;

        /// <summary>
        /// OLEDB Connection string used to connect to the database
        /// </summary>
        [DefaultValue(null)]
        [DisplayName("Connection string"), Description("OLEDB Connection string used to connect to the database. The string can contain the keyword " + Repository.SealRepositoryKeyword + " to specify the repository root folder."), Category("Definition"), Id(3, 1)]
        [Editor(typeof(ConnectionStringEditor), typeof(UITypeEditor))]
        public string ConnectionString { get; set; }


        /// <summary>
        /// Property Helper for editor
        /// </summary>
        [DefaultValue(null)]
        [DisplayName("Connection string"), Description("OLEDB Connection string used to connect to the database."), Category("Definition"), Id(3, 1)]
        [XmlIgnore]
        public string ConnectionString2
        {
            get { return ConnectionString; }
        }

        /// <summary>
        /// The date time format used to build date restrictions in the SQL WHERE clauses. This is not used for MS Access database (Serial Dates).
        /// </summary>
        [DefaultValue("yyyy-MM-dd HH:mm:ss")]
        [DisplayName("Date Time format"), Description("The date time format used to build date restrictions in the SQL WHERE clauses. This is not used for MS Access database (Serial Dates)."), Category("Definition"), Id(4, 1)]
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Full Connection String with user name and password
        /// </summary>
        [Browsable(false)]
        public string FullConnectionString
        {
            get {
                string result = Helper.GetOleDbConnectionString(ConnectionString, UserName, ClearPassword);
                return Source.Repository.ReplaceRepositoryKeyword(result); 
            }
        }

        /// <summary>
        /// SQLServer Connection String
        /// </summary>
        public string SQLServerConnectionString
        {
            get
            {
                OleDbConnectionStringBuilder builder = new System.Data.OleDb.OleDbConnectionStringBuilder(FullConnectionString);
                string result = string.Format("Server={0};Database={1};", builder["Data Source"], builder["Initial Catalog"], builder["User ID"], builder["Password"]);
                result += (builder.ContainsKey("User ID") ? string.Format("User Id={0};Password={1};", builder["User ID"], builder["Password"]) : "Trusted_Connection=True;");
                return result;
            }
        }

        /// <summary>
        /// User name used to connect to the database
        /// </summary>
        [DisplayName("User name"), Description("User name used to connect to the database."), Category("Security"), Id(1, 2)]
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
        public bool ShouldSerializePassword() { return !string.IsNullOrEmpty(Password); }

        /// <summary>
        /// Password in clear text
        /// </summary>
        [DisplayName("User password"), PasswordPropertyText(true), Description("Password used to connect to the database."), Category("Security"), Id(2, 2)]
        [XmlIgnore]
        public string ClearPassword
        {
            get {
                try
                {
                    return CryptoHelper.DecryptTripleDES(Password, PasswordKey);
                }
                catch (Exception ex)
                {
                    Error = "Error during password decryption:" + ex.Message;
                    TypeDescriptor.Refresh(this);
                    return Password;
                }
            }
            set {
                try
                {
                    Password = CryptoHelper.EncryptTripleDES(value, PasswordKey);
                }
                catch(Exception ex)
                {
                    Error = "Error during password encryption:" + ex.Message;
                    TypeDescriptor.Refresh(this);
                    Password = value;
                }
            }
        }

        /// <summary>
        /// True if the connection is editable
        /// </summary>
        [XmlIgnore]
        public bool IsEditable = true;

        [XmlIgnore]
        private DbConnection DbConnection
        {
            get
            {
                return Helper.DbConnectionFromConnectionString(FullConnectionString);
            }
        }

        /// <summary>
        /// Returns an open DbConnection object
        /// </summary>
        public DbConnection GetOpenConnection()
        {
            try
            {
                DbConnection connection = DbConnection;
                connection.Open();
                if (DatabaseType == DatabaseType.Oracle)
                {
                    try
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = "alter session set nls_date_format='yyyy-mm-dd hh24:mi:ss'";
                        command.ExecuteNonQuery();
                    }
                    catch { }
                }

                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error opening database connection:\r\n{0}", ex.Message));
            }
        }

        /// <summary>
        /// Check the current connection
        /// </summary>
        public void CheckConnection()
        {
            Cursor.Current = Cursors.WaitCursor;
            Error = "";
            Information = "";
            try
            {
                DbConnection connection = DbConnection;
                connection.Open();
                connection.Close();
                Information = "Database connection checked successfully.";
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Information = "Error got when checking the connection.";
            }
            Information = Helper.FormatMessage(Information);
            UpdateEditorAttributes();
            Cursor.Current = Cursors.Default;
        }

        #region Helpers
        /// <summary>
        /// Editor Helper: Check the database connection
        /// </summary>
        [Category("Helpers"), DisplayName("Check connection"), Description("Check the database connection."), Id(1, 10)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperCheckConnection
        {
            get { return "<Click to check database connection>"; }
        }

        /// <summary>
        /// Editor Helper: Helper to create a connection string to query an Excel workbook or a MS Access database
        /// </summary>
        [Category("Helpers"), DisplayName("Create connection from Excel or MS Access"), Description("Helper to create a connection string to query an Excel workbook or a MS Access database."), Id(1, 10)]
        [Editor(typeof(HelperEditor), typeof(UITypeEditor))]
        public string HelperCreateFromExcelAccess
        {
            get { return "<Click to create a connection from an Excel or a MS Access file>"; }
        }

        /// <summary>
        /// Last information message when the enum list has been refreshed
        /// </summary>
        [XmlIgnore, Category("Helpers"), DisplayName("Information"), Description("Last information message when the enum list has been refreshed."), Id(2, 10)]
        [EditorAttribute(typeof(InformationUITypeEditor), typeof(UITypeEditor))]
        public string Information { get; set; }

        /// <summary>
        /// Last error message
        /// </summary>
        [XmlIgnore, Category("Helpers"), DisplayName("Error"), Description("Last error message."), Id(3, 10)]
        [EditorAttribute(typeof(ErrorUITypeEditor), typeof(UITypeEditor))]
        public string Error { get; set; }

        #endregion
    }
}
