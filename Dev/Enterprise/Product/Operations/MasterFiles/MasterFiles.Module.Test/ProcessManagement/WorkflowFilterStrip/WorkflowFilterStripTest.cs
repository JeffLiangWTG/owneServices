using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowFilterStripTest : TestCaseWithFactory
	{
		public void TestHandlesWorkflowModuleFilter()
		{
			AssertProvidesControlsFor(new WorkflowModuleFilter("Workflow Test", typeof(ProcessTasks), WorkflowModuleFilterTypes.MilestoneDate));
		}

		public void TestHandlesWorkflowModuleTextFilter()
		{
			AssertProvidesControlsFor(new WorkflowModuleTextFilter("Workflow Text Test", GetSomeFilter, new CodeDescriptionPairList(), typeof(ProcessTasks), "string value"));
		}

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (WorkflowFilterStrip strip = new WorkflowFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(WorkflowFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(WorkflowFilterStrip).GetMethod(
	"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);

			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		ZQuery GetSomeFilter(ZString str)
		{
			return new ZDBOnlySubQuery(typeof(DummyBusinessObject), ProcessTasksSchema.P9_ParentID);
		}

		#endregion
	}
}
