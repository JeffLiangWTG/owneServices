using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountWave : AutoWhsCycleCountWave, IWorkflowProvider
	{
		public WhsCycleCountWave(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsCycleCountWaveProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.WhsCycleCountWaveWorkflowDescriptorCode;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => new ColumnValueRanker();

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			WorkflowItems.RemoveAndDeleteAll();
		}

		#endregion
	}
}
