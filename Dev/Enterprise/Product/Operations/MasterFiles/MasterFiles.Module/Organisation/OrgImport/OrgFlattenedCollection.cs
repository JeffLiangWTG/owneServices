using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public class OrgFlattenedCollection : NonPersistentBusinessObjectCollection<OrgFlattened>
	{
		public OrgFlattenedCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgFlattened();
		}
	}
}
