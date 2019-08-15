﻿//
// Copyright (c) Seal Report, Eric Pfirsch (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using DynamicTypeDescriptor;
using Seal.Converter;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Seal.Model
{
    /// <summary>
    /// A Dashboard Widget is a report View published for Dashboards
    /// </summary>
    public class DashboardWidget : RootEditor
    {
        #region Editor

        protected override void UpdateEditorAttributes()
        {
            if (_dctd != null)
            {
                TypeDescriptor.Refresh(this);
            }
        }

        public override void InitEditor()
        {
            base.InitEditor();
        }

        #endregion

        /// <summary>
        /// Name
        /// </summary>
        public override string ToString()
        {
            return _name;
        }

        private string _name = "";

        /// <summary>
        /// Unique identifier
        /// </summary>
        [Browsable(false)]
        public string GUID { get; set; }

        /// <summary>
        /// The widget name
        /// </summary>
        [DisplayName("Name"), Description("The widget name."), Id(1, 1)]
        public string Name { get => _name;
            set {
                _name = value;
                //Create guid for the first time
                if (!string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(GUID))
                {
                    GUID = Guid.NewGuid().ToString();
                }
            }
        }

        /// <summary>
        /// Description of the widget
        /// </summary>
        [DisplayName("Description"), Description("Description of the widget."), Id(2, 1)]
        public string Description { get; set; }

        /// <summary>
        /// Tag used to define the security of the Dashboard Manager (Widgets of the Security Groups defined in the Web Security)
        /// </summary>
        [DisplayName("Security tag"), Description("Tag used to define the security of the Dashboard Manager (Widgets of the Security Groups defined in the Web Security)."), Id(3, 1)]
        public string Tag { get; set; }

        /// <summary>
        /// If true, the widget may modify dynamically the name, icon or color after the execution (e.g. set the color in red if no record in the model)
        /// </summary>
        [DisplayName("Is dynamic"), Description("If true, the widget may modify dynamically the name, icon or color after the execution (e.g. set the color in red if no record in the model)."), Id(4, 1)]
        [DefaultValue(false)]
        public bool Dynamic { get; set; } = false;

        /// <summary>
        /// CSS class defining the icon of the widget header
        /// </summary>
        [DisplayName("Icon class"), Description("CSS class defining the icon of the widget header."), Id(5, 1)]
        [TypeConverter(typeof(WidgetIconClassConverter))]
        [DefaultValue("glyphicon glyphicon-info-sign")]
        public string Icon { get; set; } = "glyphicon glyphicon-info-sign";

        /// <summary>
        /// CSS class defining the background color of the widget header
        /// </summary>
        [DisplayName("Color class"), Description("CSS class defining the background color of the widget header."), Id(6, 1)]
        [TypeConverter(typeof(WidgetColorClassConverter))]
        [DefaultValue("default")]
        public string Color { get; set; } = "default";

        /// <summary>
        /// Width of the widget in pixels. If 0, the widget will use the size of the inner HTML generated.
        /// </summary>
        [DisplayName("Width"), Description("Width of the widget in pixels. If 0, the widget will use the size of the inner HTML generated."), Id(7, 1)]
        [DefaultValue(0)]
        public int Width { get; set; } = 0;

        /// <summary>
        /// Height of the widget in pixels. If 0, the widget will use the size of the inner HTML generated.
        /// </summary>
        [DisplayName("Height"), Description("Height of the widget in pixels. If 0, the widget will use the size of the inner HTML generated."), Id(8, 1)]
        [DefaultValue(0)]
        public int Height { get; set; } = 0;

        /// <summary>
        /// If true, the widget name is a link to execute the full report
        /// </summary>
        [DisplayName("Allow report execution"), Description("If true, the widget name is a link to execute the full report."), Id(9, 1)]
        [DefaultValue(true)]
        public bool Exec { get; set; } = true;

        /// <summary>
        /// Number of seconds before the widget is re-executed. If -1, the rate of the root view is used (defined in property 'Options: Auto-Refresh (seconds)'). A value of 0 means no refresh.
        /// </summary>
        [DisplayName("Auto-Refresh (seconds)"), Description("Number of seconds before the widget is re-executed. If -1, the rate of the root view is used (defined in property 'Options: Auto-Refresh (seconds)'). A value of 0 means no refresh."), Id(10, 1)]
        [DefaultValue(-1)]
        public int Refresh { get; set; } = -1;

        /// <summary>
        /// The XML to insert in a dashboard definition file to show this widget
        /// </summary>
        [XmlIgnore]
        [DisplayName("Dashboard XML"), Description("The XML to insert in a dashboard definition file to show this widget."), Id(11, 1)]
        public string XML
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? "" : string.Format("<DashboardItem><WidgetGUID>{0}</WidgetGUID></DashboardItem>", GUID);
            }
        }

        /// <summary>
        /// True if the widget is published
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public bool IsPublished
        {
            get { return !string.IsNullOrEmpty(_name);  }
        }

        //Run-time
        /// <summary>
        /// Current report name
        /// </summary>
        [XmlIgnore]
        public string ReportName;

        /// <summary>
        /// Current report path
        /// </summary>
        [XmlIgnore]
        public string ReportPath;

        /// <summary>
        /// Last modification date time
        /// </summary>
        [XmlIgnore]
        public DateTime LastModification;
    }
}
