using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.BizTalkAdapters.Common.UI
{
    public class PrivateKeyTextBoxConverter : TextBoxConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is string)
            {
                string text = (string)value;
                if (String.IsNullOrWhiteSpace(text))
                    return String.Empty;
                else
                {
                    var keyRegex = new Regex(@"^-+ *BEGIN (?<keyName>\w+( \w+)* PRIVATE KEY)", RegexOptions.Compiled | RegexOptions.Multiline);
                    var match = keyRegex.Match(text);
                    if (match.Success)
                        return match.Groups["keyName"].Value;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
