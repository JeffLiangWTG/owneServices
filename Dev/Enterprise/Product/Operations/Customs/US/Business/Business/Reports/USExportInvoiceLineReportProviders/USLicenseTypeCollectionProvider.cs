using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.Reports
{
	sealed class USLicenseTypeCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.US.IUSLicenseTypeCollectionProvider
	{
		public USLicenseTypeCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return AddInfoJobComInvoiceLineLookups.GetUS_LicenseType_ListForEXPCollection(BusinessObjectFactory, ZDateTime.Today);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		public override int MaxLength => AutoUSAddInfo.Schema.US_LicenseTypeMaxLength;
	}
}
