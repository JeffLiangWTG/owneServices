using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyWithReadOnlyActionField : DummyWithWorkflow, IWorkflowProvider
	{
		public DummyWithReadOnlyActionField(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		[ActionField(ReadOnly = true)]
		public override ZString Z0_Description
		{
			get => base.Z0_Description;
			set => base.Z0_Description = value;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return GetWorkflowType(); }
		}

		protected override string GetWorkflowType()
		{
			return "DUM";
		}

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		public override DummyProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItemsForReadOnly == null)
				{
					workflowItemsForReadOnly = this.GetOrCreateProcessTaskCollection(GetNewWorkflowItemsForReadOnly);
					RegisterEditableChildObject(workflowItemsForReadOnly);
				}
				return workflowItemsForReadOnly;
			}
		}
		DummyProcessTaskCollectionForReadOnly workflowItemsForReadOnly;

		DummyProcessTaskCollectionForReadOnly GetNewWorkflowItemsForReadOnly()
		{
			return new DummyProcessTaskCollectionForReadOnly(this);
		}
	}
}
