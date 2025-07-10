using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	public sealed class DummyBusinessObjectWithWorkflow : DummyBusinessObject, IWorkflowProvider
	{
		public DummyBusinessObjectWithWorkflow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IWorkflowProvider Members

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			throw new NotImplementedException();
		}

		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		public ProcessTaskCollection WorkflowItems
		{
			get { return this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection(this)); }
		}

		#endregion

		#region IWorkflowProviderCore Members

		public CargoWise.Integration.IColumnValueRanker GetTemplateSelectionCriteria()
		{
			throw new NotImplementedException();
		}

		public ZString WorkflowType
		{
			get { return "DUM"; }
		}

		public Logs Logs => null;

		public BusinessObjectFactory LogsFactory => null;

		#endregion
	}
}
