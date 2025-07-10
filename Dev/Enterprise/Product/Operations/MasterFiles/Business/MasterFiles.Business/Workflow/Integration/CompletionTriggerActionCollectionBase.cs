using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CompletionTriggerActionBaseCollection<TTriggerAction, TTrigger> : ActiveBusinessObjectCollection<TTriggerAction>
		where TTriggerAction : BusinessObject, ITriggerAction
		where TTrigger : BusinessObject, IBaseTrigger
	{
		protected CompletionTriggerActionBaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected CompletionTriggerActionBaseCollection(TTrigger master, SchemaGuidColumn fkColumn)
			: base(master.Factory, master, GetQuery(master), fkColumn)
		{
			this.master = master;
		}

		readonly TTrigger master;

		static ZQuery GetQuery(TTrigger parent)
		{
			return parent.IsMilestone() || parent.IsTrigger() ? new ZQuery() : ZQuery.NoResultQuery;
		}

		public bool ContainsTriggerType(string triggerType)
		{
			return this.Any(a => a.ActionType == triggerType);
		}

		protected override void OnAdded(TTriggerAction businessObject)
		{
			base.OnAdded(businessObject);

			if (!master.IsTrigger() && !master.IsMilestone())
			{
				ErrorReporter.ReportOnce("Attempted to add a trigger action to a record that doesn't support them. Use WorkflowItems.Triggers.AddNew or WorkflowItems.Milestones.AddNew to create the parent trigger or milestone.");
			}
		}
	}
}
