using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ProcessTaskFilterStripTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestSelectItemFromPopup_WhenPopupClosed_ShouldUpdateFilterPropertyImmediately()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var enquiry = Factory.New<SalesEnquiry>();
			var jobHeader = helper.GetJobHeaderForParent(enquiry, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "The workflow");

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Workflow");
				var filter = (WorkflowJobModuleFilter)strip.CurrentModuleFilter;

				using (var moduleForm = (ZForm)module.ShowPopup())
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					filterControl.AddFilterStrip(strip);
					var findBox = filterControl.FindSingle<ProcessTaskFilterStrip.FindBoxForWorkflowJobFilterStrip>();
					EmbeddedModulePopup popup = null;

					try
					{
						findBox.PopupButton.PerformClick();

						var openForms = ZApplication.GetOpenForms();
						popup = openForms.OfType<EmbeddedModulePopup>().Single(x => x.Text.StartsWith("Job Workflows"));
						var processHeaderFilterControl = popup.FindSingle<ZFilterStripCommonControl>();
						processHeaderFilterControl.FirePerformSearch();
						popup.Module_ForTest.DisplayGrid.SelectSingleElementByPK(workflow.PK);
						popup.ExposedOKButtonForTesting.PerformClick();
					}
					finally
					{
						popup?.Dispose();
					}

					AssertEquals("The filter's Property property should have been set as soon as the popup was closed, and yet...", workflow.PK, filter.Property);
				}
			}
		}

		[RequiresSTA]
		public void TestTasksStaffCanDoFilterStrip()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("TasksStaffCanDo");
				var filter = (TasksStaffCanDoModuleFilter)strip.CurrentModuleFilter;

				using (var moduleForm = (ZForm)module.ShowPopup())
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					var filterStrip = filterControl.AddFilterStrip(strip);

					AssertCloseEnough("The filter strip should be double the normal height of a filter strip.", ControlDpiScalingHelper.ScaleToCurrentDpiY(44), filterStrip.Height);

					var modeBox = filterStrip.FindSingleOrDefault<ZDropEdit>("TasksStaffCanDoModeDropEdit");
					AssertNotNull(modeBox);
					AssertEquals(nameof(filter.Mode), modeBox.BindTo);
				}
			}
		}

		[RequiresSTA]
		public void TestDateFilterStripHeight()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Actual Start");

				using (var moduleForm = (ZForm)module.ShowPopup())
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					var filterStrip = filterControl.AddFilterStrip(strip);

					AssertCloseEnough("The filter strip should have the normal height of a filter strip.", ControlDpiScalingHelper.ScaleToCurrentDpiY(22), filterStrip.Height);
				}
			}
		}
	}
}
