using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Workflow.Test
{
	[TestedType(typeof(WorkflowDiagnosticViewModel))]
	sealed class WorkflowDiagnosticViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowDiagnosticViewModel(null);
		}
	}
}
