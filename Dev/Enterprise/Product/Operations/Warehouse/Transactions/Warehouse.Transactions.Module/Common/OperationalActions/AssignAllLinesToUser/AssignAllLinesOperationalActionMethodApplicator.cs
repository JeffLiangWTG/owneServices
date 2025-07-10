using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class AssignAllLinesOperationalActionMethodApplicator<T> : LinesAssignerActionMethodApplicator<T>
		where T : BusinessObject, IMasterStaffAssigner
	{
		protected AssignAllLinesOperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{ }

		#region Validation

		public override AssignAllLinesApplicatorValidation<T> Validation => new AssignAllLinesApplicatorValidation<T>(this);

		#endregion

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
								line.AssignLine(VerifiedBy);
							}
						}

						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, formattedMessage, new Object[] { logControllerLink, Res.GetString("88f20d10-a1f1-48ab-b872-f34885848200", "- All lines have been assigned successfully.") });
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, formattedMessage, new Object[] { logControllerLink, UnassignErrorMessage });
					}
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, formattedMessage, new Object[] { logControllerLink, GetErrorMessage(target) });
				}

				log.BumpSectionProgress();
			}
		}

		#endregion

		protected virtual string UnassignErrorMessage { get { return Res.GetString("501552e4-4098-44bb-9da3-e6a11db2d775", "- No Lines were assigned because there are no unassigned lines."); } }
	}
}
