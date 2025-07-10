using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.Reports
{
	public class USRegionDistrictPortCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.US.IUSRegionDistrictPortCollectionProvider
	{
		public USRegionDistrictPortCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(BusinessObjectFactory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.RegionDistrictPort;

		public override int MaxLength => 4;
	}
}
