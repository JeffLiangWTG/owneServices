using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffSecurityCollection : NonPersistentBusinessObjectCollection<GlbStaffSecurity>
	{
		public GlbStaffSecurityCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlbStaffSecurity();
		}
	}
}
