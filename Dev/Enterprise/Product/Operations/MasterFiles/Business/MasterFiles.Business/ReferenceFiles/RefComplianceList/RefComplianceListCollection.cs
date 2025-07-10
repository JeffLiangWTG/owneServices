using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceListCollection : ActiveBusinessObjectCollection<RefComplianceList>
	{
		public RefComplianceListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefComplianceListCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
