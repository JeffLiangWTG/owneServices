using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class UnAssignAllLinesActionMethodApplicator<T> : LinesAssignerActionMethodApplicator<T>
		where T : BusinessObject, IMasterStaffAssigner
	{
		protected UnAssignAllLinesActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{ }

		#region AssignTargetsToUsers

		protected override void AssignTargetsToUsers(IOperationalActionSectionLog log, IEnumerable<T> targets)
		{
			log.SetSectionProgressMax(targets.Count());

			foreach (T target in targets)
			{
				var assigner = (IMasterStaffAssigner)target;
				string formattedMessage = MessageHeader + " {0} {1}";
				var logControllerLink = new LogControllerLink(GetJobNo(target), ControllerID, target.PK);

				if (assigner.IsJobAssignable)
				{
					if (assigner.CanAssignOrUnAssignAnyLines)
					{
						foreach (var line in assigner.Lines)
						{
							if (line.CanAssignOrUnAssignLine())
							{
								line.UnAssignLine(VerifiedBy);
							}
						}

						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, formattedMessage, logControllerLink, Res.GetString("96ede43b-67f2-4723-b6cb-7da6eadd4bbe", "- All lines have been un-assigned successfully."));
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, formattedMessage, logControllerLink, NoAssignLinesErrorMessage);
					}
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, formattedMessage, logControllerLink, GetErrorMessage(target));
				}

				log.BumpSectionProgress();
			}
		}

		#endregion

		protected virtual string NoAssignLinesErrorMessage => Res.GetString("6b8968fa-0e3d-48b8-adf5-86fda14fb9f8", "- No Lines were un-assigned because there are no assigned lines.");

		public override AssignAllLinesApplicatorValidation<T> Validation => new UnassignAllLinesApplicatorValidation<T>(this);
	}
}
