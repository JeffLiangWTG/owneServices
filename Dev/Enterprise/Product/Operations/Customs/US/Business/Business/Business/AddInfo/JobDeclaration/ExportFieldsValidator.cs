using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	static class ExportFieldsValidator
	{
		public static void ValidateFTZIndicatorFormat(ZPropertyInfo propertyInfo)
		{
			var ftzInd = (ZString)propertyInfo.Value;
			int ftzIndInt;
			int.TryParse(ftzInd.SubstringSafe(0, 3), out ftzIndInt);

			if (!Regex.IsMatch(ftzInd, "[0-9]{3}(?:[a-z0-9]{4}|[a-z0-9]{6})$", RegexOptions.IgnoreCase) || ftzIndInt <= 0 || ftzIndInt > 286)
			{
				propertyInfo.AddMessageError(InvalidFTZIndicatorFormat);
			}
		}
		internal const string InvalidFTZIndicatorFormat = @"The FT Zone must be 7 or 9 positions 
When using the 7 position format, the FT Zone should be structured as follows: 
Position 1-3: General Purpose Zone (NNN – 3 numeric)
Position 4-5: Subzone (XX – 2 alphanumeric characters)
Position 6-7: Site (XX – 2 alphanumeric characters) 

When using the 9 position format, the FT Zone should be structured as follows: 
Position 1-3: General Purpose Zone (NNN – 3 numeric)
Position 4-6: Subzone (XXX – 3 alphanumeric characters)
Position 7-9: Site (XXX – 3 alphanumeric characters) 

Insert zeros when there is no subzone or site and leading zero(s) when the General Purpose zone is less than 3 numerics and when the subzone or site is 1 alphanumeric character for reporting 7 position FTZ Zones or 1 or 2 alphanumeric characters for reporting 9 position FTZ Zones.
";

		public static void ValidateOriginalTINFormat(ZPropertyInfo propertyInfo)
		{
			var value = (ZString)propertyInfo.Value;
			if (!Regex.IsMatch(value, @"^[X]\d{14}$", RegexOptions.IgnoreCase))
			{
				propertyInfo.AddMessageError(InvalidOriginalTINFormat);
				return;
			}

			ZDateTime validFormatValue;
			var result = ZDateTime.TryParseExact(value.SubstringSafe(1, 8), out validFormatValue, "yyyyMMdd");
			if (!result)
			{
				propertyInfo.AddMessageError(InvalidOriginalTINFormat);
			}
		}
		internal const string InvalidOriginalTINFormat = @"The ITN is a 15-position alphanumeric string that begins with an 'X' followed by the 8-position date 
that AES accepted the data and a 6-position AES generated number. The ITN format is:
XYYYYMMDDnnnnnn";
	}
}
