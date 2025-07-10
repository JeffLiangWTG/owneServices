using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.Business
{
	public static class QuotaDispositionCodeList
	{
		public static IEnumerable<CodeDescriptionPair> GetQuotaDispositionCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("QuotaDispositionCodeList", () =>
			{
				var codeList = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USQuotaDispositions,
					ZDateTime.Today);
				return codeList.Select(s => new CodeDescriptionPair()
				{
					Code = s.ZZD_Code,
					Description = s.ZZD_Description
				});
			});
		}
	}
}
