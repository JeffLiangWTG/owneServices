using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DummyForDeletedJobTest : DummyBusinessObject, IWorkflowProvider
	{
		public ZString WorkflowType
		{
			get
			{
				WorkflowTypeAccessed?.Invoke(this, EventArgs.Empty);
				return DummyWorkflowDescriptor.Instance.Code;
			}
		}

		public event EventHandler WorkflowTypeAccessed;

		#region Irrelevant things

		public DummyForDeletedJobTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			WorkflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection(this));
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			throw new NotImplementedException();
		}

		public Logs Logs { get; }
		public BusinessObjectFactory LogsFactory { get; }
		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			throw new NotImplementedException();
		}

		public IProcessHeaderCollection Workflows { get; }
		public ProcessTaskCollection WorkflowItems { get; }

		#endregion
	}
}
