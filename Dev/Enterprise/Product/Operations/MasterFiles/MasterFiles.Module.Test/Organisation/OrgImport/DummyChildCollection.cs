using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyChildCollection : NonPersistentBusinessObjectCollection<DummyChild>
	{
		public DummyChildCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyChild(Factory);
		}
	}
}
