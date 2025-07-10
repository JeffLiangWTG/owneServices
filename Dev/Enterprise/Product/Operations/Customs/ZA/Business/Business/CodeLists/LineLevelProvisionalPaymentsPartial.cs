using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public static class ProvisionalPaymentTypesHelper
	{
		internal static IEnumerable<ZString> GetTypesForRateType(ZString rateType)
		{
			switch (rateType)
			{
				case Universal.Constants.RateTypes.Penalty:
					return new ZString[] { LineLevelProvisionalPayments.Codes.FOR, LineLevelProvisionalPayments.Codes.PEN };
				case Universal.Constants.RateTypes.ProvisionalPayment:
					return new ZString[] { LineLevelProvisionalPayments.Codes.PPA, LineLevelProvisionalPayments.Codes.PPC, LineLevelProvisionalPayments.Codes.PPG, LineLevelProvisionalPayments.Codes.PPR, LineLevelProvisionalPayments.Codes.PPT, HeaderLevelProvisionalPayments.Codes.PPE };
				default:
					return System.Array.Empty<ZString>();
			}
		}

		internal static CodeDescriptionPairList GetFullProvisionalPaymentTypeList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ProvisionalPaymentTypesHelper.GetFullProvisionalPaymentTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(factory.GetCachedValue<HeaderLevelProvisionalPayments>());
				result.AddRange(factory.GetCachedValue<LineLevelProvisionalPayments>());
				result.SortByDescriptionAndCombineIfSameCode();
				return result;
			});
		}

		internal static IEnumerable<string> GetAllProvisionalPaymentTypes(this BusinessObjectFactory factory)
		{
			return factory.GetFullProvisionalPaymentTypeList().GetAllCodes();
		}
	}
}
