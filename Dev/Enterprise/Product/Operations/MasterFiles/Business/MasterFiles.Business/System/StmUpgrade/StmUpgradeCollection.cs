using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmUpgradeCollection : BusinessObjectCollection<StmUpgrade>
	{
		public StmUpgradeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public StmUpgradeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
