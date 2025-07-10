using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationLookups : AutoWhsCycleCountLocationLookups
	{
		public WhsCycleCountLocationLookups(AutoWhsCycleCountLocation parent) : base(parent)
		{
		}

		public override ProcessTaskCollection Tasks => Factory.GetCachedValue("WhsCycleCountLocationLookups|Tasks", () => new ProcessTaskCollection(Factory, ZQuery.NoResultQuery));
	}
}
