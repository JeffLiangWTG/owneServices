using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class DummyEnterpriseBusinessObjectWithWorkflow : DummyEnterpriseBusinessObject, IWorkflowProvider, IJobNumber
	{
		public DummyEnterpriseBusinessObjectWithWorkflow(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region IWorkflowProvider Members

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
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

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (_workflowItems == null)
				{
					_workflowItems = this.GetOrCreateProcessTaskCollection(() => new DummyEnterpriseBusinessObjectProcessTaskCollection(this));
					RegisterEditableChildObject(_workflowItems);
				}
				return _workflowItems;
			}
		}
		ProcessTaskCollection _workflowItems;

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IWorkflowProviderCore Members

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, Z0_Code, ZString.Empty);
			return result;
		}

		public ZString WorkflowType
		{
			get { return new DummyEnterpriseBusinessObjectWorkflowDescriptor().Code; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return Z0_Description; }
		}

		#endregion
	}
}
