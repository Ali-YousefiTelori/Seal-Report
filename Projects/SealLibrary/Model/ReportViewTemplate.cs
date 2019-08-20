﻿//
// Copyright (c) Seal Report, Eric Pfirsch (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.IO;
using Seal.Helpers;
using RazorEngine.Templating;

namespace Seal.Model
{
    /// <summary>
    /// A ReportViewTemplate defines how a view is parsed and rendered.
    /// </summary>
    public class ReportViewTemplate
    {
        public const string ReportName = "Report";
        public const string ModelName = "Model";
        public const string ModelDetailName = "Model Detail";
        public const string DataTableName = "Data Table";
        public const string PageTableName = "Page Table";
        public const string ChartNVD3Name = "Chart NVD3";
        public const string ChartJSName = "Chart JS";
        public const string ChartPlotlyName = "Chart Plotly";
        public const string ModelContainerName = "Model Container";

        /// <summary>
        /// Name of the view template
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Current file path of the template
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Path of the configuration file for the template
        /// </summary>
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// Parameters defined for the template 
        /// </summary>
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        /// <summary>
        /// Allowed parent template names
        /// </summary>
        public List<string> ParentNames { get; set; } = new List<string>();

        /// <summary>
        /// True if the template is for a report model
        /// </summary>
        public bool ForReportModel { get; set; } = false;

        /// <summary>
        /// Text of the template
        /// </summary>
        public string Text
        {
            get
            {
                string result = "";
                try
                {
                    StreamReader sr = new StreamReader(FilePath);
                    result = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
                return result;
            }
        }

        /// <summary>
        /// Current template configuration text
        /// </summary>
        public string Configuration = "";

        /// <summary>
        /// Current errors
        /// </summary>
        public string Error { get; set; } = "";

        /// <summary>
        /// List of partial templates path
        /// </summary>
        public List<string> PartialTemplatesPath { get; set; } = new List<string>();

        /// <summary>
        /// Returns a partial template path from a given name
        /// </summary>
        public string GetPartialTemplatePath(string name)
        {
            return Path.Combine(Path.GetDirectoryName(FilePath), name + ".partial.cshtml");
        }

        /// <summary>
        /// Returns a partial template text from a given name
        /// </summary>
        public string GetPartialTemplateText(string name)
        {
            return File.ReadAllText(GetPartialTemplatePath(name));
        }

        /// <summary>
        /// Initialize the template from a file
        /// </summary>
        public bool Init(string path)
        {
            FilePath = path;
            LastModification = File.GetLastWriteTime(path);
            ConfigurationPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".config.cshtml");
            if (!File.Exists(ConfigurationPath)) return false;

            LastConfigModification = File.GetLastWriteTime(ConfigurationPath);
            Configuration = File.ReadAllText(ConfigurationPath);
            //load partial templates related
            PartialTemplatesPath.Clear();
            foreach (var partialPath in Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".*.partial.cshtml"))
            {
                PartialTemplatesPath.Add(partialPath);
            }

            IsParsed = false;

            return true;
        }

        /// <summary>
        /// Returns a list of ReportViewTemplate from a given folder
        /// </summary>
        public static List<ReportViewTemplate> LoadTemplates(string templateFolder)
        {
            List<ReportViewTemplate> viewTemplates = new List<ReportViewTemplate>();
            //Templates
            foreach (var path in Directory.GetFiles(templateFolder, "*.cshtml"))
            {
                if (path.EndsWith(".config.cshtml") || path.EndsWith(".partial.cshtml")) continue;
                ReportViewTemplate template = new ReportViewTemplate();
                if (template.Init(path)) viewTemplates.Add(template);
            }
            return viewTemplates;
        }

        /// <summary>
        /// Clear the template configuration
        /// </summary>
        public void ClearConfiguration()
        {
            Parameters.Clear();
            ParentNames.Clear();
            ForReportModel = false;
        }

        /// <summary>
        /// Flag for optimization, by default the template is not parsed...until it is used
        /// </summary>
        public bool IsParsed = false; 

        /// <summary>
        /// Last modification date time
        /// </summary>
        public DateTime LastModification;

        /// <summary>
        /// Last modfication of the configuration file
        /// </summary>
        public DateTime LastConfigModification;

        /// <summary>
        /// True if the template or its configuration is modified
        /// </summary>
        public bool IsModified
        {
            get
            {
                return LastModification != File.GetLastWriteTime(FilePath) || LastConfigModification != File.GetLastWriteTime(ConfigurationPath);
            }
        }

        /// <summary>
        /// Parse the current configuration and initialize the parameters
        /// </summary>
        public void ParseConfiguration()
        {
            //Parse the configuration file to init the view template
            try
            {
                string key = key = string.Format("TPLCFG:{0}_{1}", ConfigurationPath, File.GetLastWriteTime(ConfigurationPath).ToString("s"));
                Error = "";
                ClearConfiguration();
                RazorHelper.CompileExecute(Configuration, this);
                IsParsed = true;
            }
            catch (TemplateCompilationException ex)
            {
                Error = Helper.GetExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                Error = string.Format("Unexpected error got when parsing template configuration.\r\n{0}", ex.Message);
            }
        }
    }
}
