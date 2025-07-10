using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyFlattenedCollection : NonPersistentBusinessObjectCollection<DummyFlattened>
	{
		public DummyFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyFlattened(Factory);
		}
	}
}
