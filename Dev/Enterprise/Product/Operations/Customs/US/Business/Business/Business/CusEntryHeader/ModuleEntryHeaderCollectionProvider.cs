using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public class ModuleEntryHeaderCollectionProvider : CollectionProvider, Integration.Customs.US.IUSModuleEntryHeaderCollectionProvider
	{
		public ModuleEntryHeaderCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new ModuleEntryHeaderCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(CusEntryHeader)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new ModuleEntryHeaderCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EntryHeader;
	}
}
