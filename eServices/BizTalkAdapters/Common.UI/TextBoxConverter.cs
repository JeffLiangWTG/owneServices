using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.Common.UI
{
    public class TextBoxConverter : System.ComponentModel.StringConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is string)
            {
                string text = (string)value;
                if (String.IsNullOrWhiteSpace(text))
                    return String.Empty;
                else
                    using (var rdr = new StringReader(text))
                        return rdr.ReadLine();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
