﻿//
// Copyright (c) Seal Report, Eric Pfirsch (sealreport@gmail.com), http://www.sealreport.org.
// This code is licensed under GNU General Public License version 3, http://www.gnu.org/licenses/gpl-3.0.en.html.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Seal.Model;
using System.Globalization;
using Seal.Helpers;

namespace Seal.Converter
{
    public class SecurityProviderConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true; //true means show a combobox
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true; //true will limit to list. false will show the list, but allow free-form entry
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] choices = new string[] { "No Provider" };
            SealSecurity security = context.Instance as SealSecurity;
            if (security != null)
            {
                choices = (from s in security.Providers select s.Name).ToArray();
            }

            return new StandardValuesCollection(choices);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (context != null)
            {
                SealSecurity security = context.Instance as SealSecurity;
                if (security != null && value != null)
                {
                    SecurityProvider provider = security.Providers.FirstOrDefault(i => i.Name == value.ToString());
                    if (provider != null) return provider.Name;
                }
            }
            return base.ConvertTo(context, culture, value, destType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            SealSecurity security = context.Instance as SealSecurity;
            if (security != null && value != null)
            {
                SecurityProvider provider = security.Providers.FirstOrDefault(i => i.Name == value.ToString());
                if (provider != null) return provider.Name;
            }
            return base.ConvertFrom(context, culture, value);
        }

    }

}
