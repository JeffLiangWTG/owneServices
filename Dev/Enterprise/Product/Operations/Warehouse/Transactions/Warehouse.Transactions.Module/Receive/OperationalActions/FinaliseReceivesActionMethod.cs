using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class FinaliseReceivesActionMethod : FinalizeDocketActionMethod<WhsReceive>
	{
		public FinaliseReceivesActionMethod()
			: base(new ZGuid("910f0950-5269-40eb-8f8b-18db0631784e"))
		{
		}

		protected override FinalizeDocketsActionMethodApplicator<WhsReceive> NewApplicatorCore(BusinessObjectFactory factory)
		{
			return new FinaliseReceivesActionMethodApplicator(factory);
		}

		protected override string GetOperationalActionName() => Res.GetString("2711616f-dcc4-4d18-8761-52098d9bdce0", "Finalize Receives");
	}
}
