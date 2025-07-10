using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public sealed class DummyParent : DummyBusinessObject
	{
		public DummyParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyWithWorkflowCollection DummyWithWorkflows
		{
			get
			{
				if (fDummyWithWorkflows == null)
				{
					fDummyWithWorkflows = new DummyWithWorkflowCollection(Factory);
				}
				return fDummyWithWorkflows;
			}
		}

		DummyWithWorkflowCollection fDummyWithWorkflows;
	}
}
