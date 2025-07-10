using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CancelReceivesActionMethodApplicator : CancelDocketsActionMethodApplicator<WhsReceive>
	{
		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			CancelDockets(log, targets.Cast<WhsReceive>());
		}

		public CancelReceivesActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("6506A586-EDC5-483F-95F9-071B6384B2AB", "Cancel Receives"), factory)
		{
		}
	}
}
