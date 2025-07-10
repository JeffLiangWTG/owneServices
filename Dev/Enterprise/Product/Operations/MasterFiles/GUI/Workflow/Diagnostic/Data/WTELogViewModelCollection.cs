using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class WTELogViewModelCollection : NonPersistentBusinessObjectCollection<WTELogViewModel>
	{
		public WTELogViewModelCollection(ProcessTask task)
		{
			this.task = task;
		}

		public override void Load()
		{
			if (task != null)
			{
				if (!task.IsNonPersistedRepresentationOfTemplateTrigger)
				{
					var query = new ZQuery(StmALogSchema.SL_Parent, task.PK)
						.AddToFilter(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.Events.WorkflowTriggerEventCode);
					Populate(query, task);
				}
				else
				{
					var triggerLinkQuery = new ZQuery(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, task.P9_ParentTemplateID);
					triggerLinkQuery.AddToFilter(ProcessJobTriggerLinkSchema.P9L_ParentId, task.P9_ParentID);
					var triggerLinks = task.Factory.Load<IProcessJobTriggerLink>(triggerLinkQuery);

					foreach (var triggerLink in triggerLinks)
					{
						var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.Events.WorkflowTriggerEventCode)
							.AddToFilter(StmALogSchema.SL_Parent, triggerLinks.Select(s => s.Identifier).ToArray());

						Populate(query, triggerLink);
					}
				}
			}
		}

		void Populate(ZQuery query, IBaseTrigger trigger)
		{
			query.OrderBy = StmALogSchema.SL_EventTime.Name + OrderByClause.Descending;
			query.MaximumRows = 200;

			foreach (var log in task.Factory.Load<StmALog>(query))
			{
				Add(new WTELogViewModel(trigger, log));
			}
		}

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject() => new WTELogViewModel(task, null);

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		#endregion

		#region Implementation

		readonly ProcessTask task;

		#endregion
	}
}
