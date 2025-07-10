using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCCountryCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.US.IUSCCountryCollectionProvider
	{
		public USCCountryCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new USCCountryCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.Country;

		public override int MaxLength => USCCountrySchema.UC_Code.MaxLength;
	}
}
