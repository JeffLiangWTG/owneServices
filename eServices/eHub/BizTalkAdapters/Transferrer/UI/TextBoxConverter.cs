using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.UI
{
	public class TextBoxConverter : StringConverter
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
			if (typeof(string) == destinationType && value is string text)
			{
				if (string.IsNullOrWhiteSpace(text))
					return string.Empty;
				else
					using (var rdr = new StringReader(text))
						return rdr.ReadLine();
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
