using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.Universal.BusinessObjects.ZZRefCusCodeListCombined
{
	public static class CusRefPackLoaderHelper
	{
		public static void MessageErrorIfNeeded(string countryCode, string commercialUQ, ZDateTime date, ZPropertyInfo zPropertyInfoForWarning, BusinessObjectFactory factory)
		{
			if (countryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				return; // Disable this for ZA?  OK then. 
			}

			var customsUqList = RefCusCodeListTypes.GetCachedList(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, date);
			var refPacks = new CusRefPacks.Loader(factory).Load(commercialUQ, countryCode, RPTypeList.Codes.AllAreas, customsUqList.GetAllCodes());
			if (refPacks == null || !customsUqList.ContainsCode(refPacks.RP_CustomsPack))
			{
				if (customsUqList.ContainsCode(commercialUQ))
				{
					// I've kept this block here for clarity and to explain this assumption.					
					// If there is no EXPLICIT map, but the code selected in the GUI does exist in the country's list, e.g. PKG for package in Fiji, we do not need to warn. 
					// This ASSUMES that whoever has loaded the records into RefPacks omitted the pointless conversions (e.g. PKG-->PKG) ONLY after checking that their descriptions/meanings matched. 
					// If their descriptions did not match, a mapping should have been added.
				}
				else
				{
					zPropertyInfoForWarning.AddMessageError(Res.GetString("A2AA9A98-5E54-4663-88F7-BF59A573F9C6", "Package type {0} does not map to a Customs package type for country {1}. Please add a mapping via Maintain > Customs > Customs Files > Packs Conversion.", commercialUQ, countryCode));
				}
			}
		}
	}
}
