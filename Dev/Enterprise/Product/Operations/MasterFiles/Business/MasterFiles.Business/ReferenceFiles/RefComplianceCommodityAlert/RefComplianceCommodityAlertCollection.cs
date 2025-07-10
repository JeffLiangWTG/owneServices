using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceCommodityAlertCollection : ActiveBusinessObjectCollection<RefComplianceCommodityAlert>
	{
		public RefComplianceCommodityAlertCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefComplianceCommodityAlertCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
