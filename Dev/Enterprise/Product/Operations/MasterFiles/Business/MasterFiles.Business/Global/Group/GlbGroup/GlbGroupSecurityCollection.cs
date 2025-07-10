using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupSecurityCollection : NonPersistentBusinessObjectCollection<GlbGroupSecurity>
	{
		public GlbGroupSecurityCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlbGroupSecurity();
		}
	}
}
