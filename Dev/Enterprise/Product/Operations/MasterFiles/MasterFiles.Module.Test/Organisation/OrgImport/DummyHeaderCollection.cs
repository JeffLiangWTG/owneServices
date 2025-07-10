using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyHeaderCollection : NonPersistentBusinessObjectCollection<DummyHeader>
	{
		public DummyHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyHeader(Factory);
		}
	}
}
