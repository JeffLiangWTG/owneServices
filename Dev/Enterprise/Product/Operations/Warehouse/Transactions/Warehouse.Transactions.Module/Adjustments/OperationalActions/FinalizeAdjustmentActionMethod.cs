using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class FinaliseAdjustmentsActionMethod : FinalizeDocketActionMethod<WhsAdjustment>
	{
		public FinaliseAdjustmentsActionMethod()
			: base(new ZGuid("f159a111-1dee-4a34-8487-0d70fb3dd8dc"))
		{
		}

		protected override FinalizeDocketsActionMethodApplicator<WhsAdjustment> NewApplicatorCore(BusinessObjectFactory factory)
		{
			return new FinaliseAdjustmentsActionMethodApplicator(factory);
		}

		protected override string GetOperationalActionName() => Res.GetString("a7f93bfb-0c93-4e4b-ae5b-85f90122c0cb", "Finalize Adjustments");
	}
}
