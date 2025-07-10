using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class MDMAdminPanelAddressCollection : BusinessObjectCollection<MDMAdminPanelAddressView>
	{
		public MDMAdminPanelAddressCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public MDMAdminPanelAddressCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
