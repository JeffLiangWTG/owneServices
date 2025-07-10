using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

static class SupplementaryCodeHelper
{
	public static CodeDescriptionPairList GetCodeListBasedOnPackageType(CodeDescriptionPairList codeList, JobComInvoiceLine invLine)
	{
		var factory = invLine.Factory;
		var tariff = invLine.JI_Tariff;
		var packageType = invLine.JI_PackageType;

		return factory.GetCachedValue($"NO.SupplementaryCodeHelper.CodeList.{tariff}{packageType}", () =>
		{
			var noCodeList = new CodeDescriptionPairList();
			var rateCodeDescriptionDictionary = invLine.UniversalTariff?.FilteredRates.DistinctBy(r => r.RateCode)
					.ToDictionary(r => r.RateCode, v => v.RateDescription) ?? new Dictionary<ZString, ZString>();

			foreach (ICodeDescription code in codeList)
			{
				if (CanAddCodeToNOAdditionalCodeList(code, packageType))
				{
					noCodeList.AddPair(code.Code, rateCodeDescriptionDictionary.TryGetValue(code.Code, out var rateDescription) ? rateDescription : code.Description);
				}
			}

			return noCodeList;
		});
	}

	static bool CanAddCodeToNOAdditionalCodeList(ICodeDescription additionalCode, ZString packageType)
	{
		var code = additionalCode.Code;
		return packageType.IsEmpty || ((!code.StartsWith(RateCodeStartsWithM) && !code.StartsWith(RateCodeStartsWithG)) || (code.Length > 1 && code[1].ToString() == packageType));
	}

	const string RateCodeStartsWithM = "M";
	const string RateCodeStartsWithG = "G";
}
