using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.Universal
{
	class RateFormulaHumanReadableVisitorData
	{
		public RateFormulaHumanReadableVisitorData(
			string currencyCode,
			IDictionary<string, string> unitOfMeasureList,
			IDictionary<string, string> countrySpecificList,
			IDictionary<string, string> additionalInformationList,
			IDictionary<string, string> meursingExpressionList
		)
		{
			CurrencyCode = Argument.NotNull(currencyCode, "currencyCode");
			UnitOfMeasureList = Argument.NotNull(unitOfMeasureList, "unitOfMeasureList");
			CountrySpecificList = Argument.NotNull(countrySpecificList, "countrySpecificList");
			AdditionalInformationList = Argument.NotNull(additionalInformationList, "additionalInformationList");
			MeursingExpressionList = Argument.NotNull(meursingExpressionList, "meursingExpressionList");
		}
		internal readonly string CurrencyCode;

		internal IDictionary<string, string> UnitOfMeasureList { get; }
		internal IDictionary<string, string> CountrySpecificList { get; }
		internal IDictionary<string, string> AdditionalInformationList { get; }
		internal IDictionary<string, string> MeursingExpressionList { get; }
	}
}
