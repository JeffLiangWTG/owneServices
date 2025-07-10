using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module.Testing
{
	sealed class WorkflowFilterStripWithRoutingSupportTest : TestCaseWithFactory
	{
		public void TestHandlesWorkflowModuleFilter()
		{
			AssertProvidesControlsFor(new WorkflowModuleFilterWithRoutingSupport("Workflow Test", typeof(ProcessTasks), WorkflowModuleFilterTypes.MilestoneDate));
		}

		public void TestDatesToFilterVisbleForWorkflowModuleFilter()
		{
			AssertDatesToFilterDropEditVisible(new WorkflowModuleFilterWithRoutingSupport("Workflow Test", typeof(ProcessTasks), WorkflowModuleFilterTypes.MilestoneDate), true);
			AssertDatesToFilterDropEditVisible(new WorkflowModuleFilterWithRoutingSupport("Workflow Test", typeof(ProcessTasks), WorkflowModuleFilterTypes.MilestoneLastCompleted), false);
			AssertDatesToFilterDropEditVisible(new WorkflowModuleFilterWithRoutingSupport("Workflow Test", typeof(ProcessTasks), WorkflowModuleFilterTypes.MilestoneNext), false);
		}

		public void TestHandlesWorkflowModuleTextFilter()
		{
			AssertProvidesControlsFor(new WorkflowModuleTextFilterWithRoutingSupport("Workflow Text Test", GetSomeFilter, (new CodeDescriptionPairList()), typeof(ProcessTasks), ZString.Empty, null, "string value"));
		}

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (var strip = new WorkflowFilterStripWithRoutingSupport())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		void AssertDatesToFilterDropEditVisible(ModuleFilter filter, bool visible)
		{
			using (var strip = new WorkflowFilterStripWithRoutingSupport())
			{
				var control = GetCurrentFilterControls(strip, filter).FirstOrDefault() as WorkflowFilterStripControlWithRoutingSupport;
				AssertNotNull(control);
				AssertEquals(visible, control.DatesToFilterDropEdit.Visible);
				control.Dispose();
			}
		}

		Control[] GetCurrentFilterControls(WorkflowFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(WorkflowFilterStripWithRoutingSupport).GetMethod(
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
