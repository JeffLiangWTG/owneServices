using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.Reports
{
	public class USCFDAProductNumberCollectionProvider : CollectionProvider,
		Integration.Customs.US.IUSFDAProductNumberCollectionProvider
	{
		public USCFDAProductNumberCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new ZZRefCusCodeListCombinedCollection(BusinessObjectFactory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode, ZDateTime.Today);
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			var countryCode = Core.Constants.CountryCodes.UnitedStates;
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			var effectiveDate = ZDateTime.Today;

			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(BusinessObjectFactory, countryCode, listType, effectiveDate);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(countryCode), false));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(listType), false));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", effectiveDate));
			return collection;
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;
	}
}
