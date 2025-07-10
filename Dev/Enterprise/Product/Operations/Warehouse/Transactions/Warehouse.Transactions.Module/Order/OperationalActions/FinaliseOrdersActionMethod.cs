using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class FinaliseOrdersActionMethod : FinalizeDocketActionMethod<WhsOrder>
	{
		public FinaliseOrdersActionMethod()
			: base(new ZGuid("1F1A72D8-300E-4476-8211-F7795775272D"))
		{
		}

		protected override FinalizeDocketsActionMethodApplicator<WhsOrder> NewApplicatorCore(BusinessObjectFactory factory)
		{
			return new FinaliseOrdersActionMethodApplicator(factory);
		}

		protected override string GetOperationalActionName() => Res.GetString("fa3db675-f712-49bf-9467-d8c8c36766ba", "Finalize Orders");
	}
}
