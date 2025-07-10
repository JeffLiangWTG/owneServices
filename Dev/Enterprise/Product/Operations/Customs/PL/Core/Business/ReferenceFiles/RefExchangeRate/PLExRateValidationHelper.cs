using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public class PLExRateValidationHelper : Integration.Customs.Shared.IExRateValidationHelper
{
	public PLExRateValidationHelper() { }

	public void CheckIfAsPublishedIsInRightFormat(ZDecimal exRate, ZString asPublished, ZPropertyInfo asPublishedInfo)
	{
		if (!string.IsNullOrEmpty(asPublished))
		{
			var currentCultureInfo = Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture;
			if (!decimal.TryParse(asPublished, NumberStyles.Any, currentCultureInfo, out decimal decimalAsPublished))
			{
				asPublishedInfo.AddMessageError(Res.GetString("A7F24823-DD22-463B-B6C0-4A71734EA880", "Invalid value. Please insert correct decimal value or leave this field empty.\r\nMake sure using ',' (COMMA) instead of '.' (DOT) for decimal separator."));
			}
			else if (decimalAsPublished / 1 != exRate &&
					decimalAsPublished / 100 != exRate &&
					decimalAsPublished / 10000 != exRate)
			{
				asPublishedInfo.AddWarning(Res.GetString("7200BD79-5797-45CF-A6BB-6980E1A9B060", "Invalid As Published value. Correct value should contain multiplied exchange rate by 1/100/10000"));
			}
		}
	}
}
