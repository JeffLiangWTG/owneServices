using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions
{
	public class BatchMarkAsClosedActionMethodApplicator : OperationalActionMethodApplicator
	{
		public BatchMarkAsClosedActionMethodApplicator(BusinessObjectFactory factory)
			: base("Batch Mark As Closed Operational Action", factory)
		{ }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			foreach (var bizObj in targets)
			{
				if (bizObj is CusInBondHeader cusInBondHeader)
				{
					var jobLink = cusInBondHeader.GetInBondHeaderIdLink();
					foreach (var moveHeader in cusInBondHeader.MovementHeaders.OfType<CusInBondMoveHeader>())
					{
						if (moveHeader.BM_InBondClosedDate.IsEmpty)
						{
							moveHeader.CloseInBond();
							log.NotifyFormat(OperationalActionLogErrorLevel.Success, "In-Bond Movement {0} in Job {1} has been closed manually.", moveHeader.InBondNumber, jobLink);
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "In-Bond Movement {0} in Job {1} has already been closed.", moveHeader.InBondNumber, jobLink);
						}
					}
				}
				else if (bizObj is USInBondMoveHeader uSInBondMoveHeader)
				{
					var jobLink = uSInBondMoveHeader.GetInBondMovementIdLink();
					var moveHeader = uSInBondMoveHeader.MoveHeader;
					if (moveHeader.BM_InBondClosedDate.IsEmpty)
					{
						moveHeader.CloseInBond();
						log.NotifyFormat(OperationalActionLogErrorLevel.Success, "In-Bond Movement {0}: has been closed manually.", new object[] { jobLink });
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "In-Bond Movement {0}: has already been closed.", new object[] { jobLink });
					}
				}
			}
		}
	}
}
