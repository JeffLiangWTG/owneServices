using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business
{
	public static class CusITARENCodeConstants
	{
		public static CodeDescriptionPairList GetITARExemptionNumberCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ITARExemptionNumberCodeList", () =>
			{
				var intendedUseCodeList = new CodeDescriptionPairList();
				ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, ZDateTime.Now).ForEach(x => intendedUseCodeList.AddPairIfNotExist(x.ZZD_Code, x.ZZD_Description));
				intendedUseCodeList.Sort();
				return intendedUseCodeList;
			});
		}

		public const string ExemptionPrefixThatRequiringImportEntryNo = "123.4";
	}
}
