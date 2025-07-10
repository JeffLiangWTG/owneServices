using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class ImportCollectionInfoForDummyFlattened : ImportCollectionInfoImpl
	{
		public ImportCollectionInfoForDummyFlattened(IBusinessObjectCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<DummyFlattened>(DummyFlattened.Schema.FlatHeaderString));
			Add(new ImportPropertyInfoImpl<DummyFlattened>(DummyFlattened.Schema.FlatHeaderDate));
			Add(new ImportPropertyInfoImpl<DummyFlattened>(DummyFlattened.Schema.FlatChild1Int));
			Add(new ImportPropertyInfoImpl<DummyFlattened>(DummyFlattened.Schema.FlatChild1Date));
			Add(new ImportPropertyInfoImpl<DummyFlattened>(DummyFlattened.Schema.FlatChild2Int));
			Add(new ImportPropertyInfoImpl<DummyFlattened>(DummyFlattened.Schema.FlatChild2Date));
		}
	}
}
