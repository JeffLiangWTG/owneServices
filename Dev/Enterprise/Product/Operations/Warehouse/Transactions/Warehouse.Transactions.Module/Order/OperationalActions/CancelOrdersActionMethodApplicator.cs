using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CancelOrdersActionMethodApplicator : CancelDocketsActionMethodApplicator<WhsOrder>
	{
		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			CancelDockets(log, targets.Cast<WhsOrder>());
		}

		public CancelOrdersActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("4D446AF1-DE53-42DD-ABDF-69FDB690525A", "Cancel Orders"), factory)
		{
		}
	}
}
