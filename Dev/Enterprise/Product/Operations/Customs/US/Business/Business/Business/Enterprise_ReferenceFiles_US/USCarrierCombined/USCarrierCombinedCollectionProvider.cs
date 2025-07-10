using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCarrierCombinedCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.US.IUSCarrierCombinedCollectionProvider
	{
		public USCarrierCombinedCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new USCarrierCombinedCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(USCarrierCombined)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new USCarrierCombinedCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.Carrier;

		public override int MaxLength => USCarrierCombinedSchema.UI_Code.MaxLength;
	}
}
