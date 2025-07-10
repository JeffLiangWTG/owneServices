using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class USMLCategoryCodes
	{
		public static CodeDescriptionPairList GetCachedPreECRList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("US_USMLCategoryCodesPreECR", // 'US_YesNoList' is not a database field
			delegate
			{
				var result = new USMLCategoryCodes();
				result.RemoveCode(USMLCategoryCodes.Codes.GasTurbineEnginesAndAssociatedEquipment);
				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedPostECRList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("US_USMLCategoryCodesPostECR", // 'US_YesNoList' is not a database field
			delegate
			{
				var result = new USMLCategoryCodes();
				return result;
			});
		}
	}
}
