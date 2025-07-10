using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public sealed class TTIRegistrationNumberValidator
	{
		public static ZString Validate(ZString value)
		{
			var result = ZString.Empty;

			if (value.Length > AutoUSTTBLineAddInfo.Schema.US_NumberForIRCMaxLength)
			{
				result = TTIRegistrationNumberMaxLength;
			}
			else if (!IsValidRegistrationNumber(value))
			{
				result = TTIRegistrationNumberCorrectFormat;
			}
			return result;
		}

		static bool IsValidRegistrationNumber(ZString vaule)
		{
			return Regex.IsMatch(vaule, @"^(DSP|BW|BWN|BR)[\-][A-Z]{2}[\-]\S+$", RegexOptions.IgnoreCase);
		}

		public const string TTIRegistrationNumberMaxLength = "Maximum length of Registration Number for Code Type of 'TTI' is only 15.";
		public const string TTIRegistrationNumberCorrectFormat = "TTB IRC Registration Number should be in the format, it must start with DSP,BW,BWN,or BR followed by a dash '-', next two characters are ALPHA followed by a dash '-',then any combination of letters, number or punctuation. Such as DSP-CA-90210,BR-MA-SAM-1.";
	}
}
