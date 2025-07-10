using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class WorkflowRelationshipTest : TestCaseWithFactory
	{
		public void TestQuickBooking_WorkflowRelationships_ConsolidateAction()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template for Workflow Links";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template for Workflow Links";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, shpTemplateWorkflow);

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var booking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			booking.ClientPK = localClient.PK;
			booking.Booking.ConsignorPK = consignor.PK;
			booking.Booking.ConsigneePK = consignee.PK;
			booking.TransportMode = Constants.TransportModes.Air;
			booking.TryLoadOrCreateJob();
			booking.Job.JH_GE = department.PK;
			booking.Job.JH_GB = GlbBranch.CurrentBranch.PK;
			booking.ETA = DateTime.Today;
			booking.ETD = DateTime.Today;
			booking.LoadPort = "AUSYD";
			booking.DischargePort = "DEFRA";

			Factory.Save();

			ForwardingConsol consol;

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);

			using (var bookingForm = (QuotedBookingForm)controller.ShowEditForm(booking))
			{
				var attachBookingItem = FindMenuItem(bookingForm, "Actions", "Consolidate");

				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();

				bookingForm.FireSaveButton();
				bookingForm.FireSaveButton();
				attachBookingItem.PerformClick();
				Application.DoEvents();

				using (var consolForm = Application.OpenForms.OfType<ConsolForm>().Single())
				{
					consol = (ForwardingConsol)consolForm.BusinessEntity;
					consol.Transports[0].JW_TransportType = "FL1";
					consol.Transports[0].JW_VoyageFlight = "AA123";
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddYesAnswer();
					UnitTestUserNotification.Instance.AddOKAnswer();
					var didSave = consolForm.FireSaveButton();
					Application.DoEvents();

					AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", consol);
					AssertNoErrors("Errors on the booking will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", booking);
					AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
					AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
			}

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().First(x => x.FH_CompletionStatement == "Consol Workflow");
			var links = conWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestQuickBooking_WorkflowRelationships_AttachConsol()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template for Workflow Links";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template for Workflow Links";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, shpTemplateWorkflow);

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var booking = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			booking.ClientPK = localClient.PK;
			booking.Booking.ConsignorPK = consignor.PK;
			booking.Booking.ConsigneePK = consignee.PK;
			booking.TransportMode = Constants.TransportModes.Air;
			booking.TryLoadOrCreateJob();
			booking.Job.JH_GE = department.PK;
			booking.Job.JH_GB = GlbBranch.CurrentBranch.PK;
			booking.ETA = DateTime.Today;
			booking.ETD = DateTime.Today;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().First(x => x.FH_CompletionStatement == "Consol Workflow");
			var links = conWorkflow.Links.ToArray();
			AssertEquals("There should be no links between workflows.", 0, links.Length);

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);

			using (var bookingForm = (QuotedBookingForm)controller.ShowEditForm(booking))
			{
				var openedBooking = (QuotedBooking)bookingForm.BusinessEntity;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					if (dialog is EmbeddedModulePopup popup)
					{
						popup.Show();
						Application.DoEvents();
						var filterControl = popup.FindSingle<ZFilterStripControl>();
						filterControl.ResetFilterStrips();
						filterControl.FirePerformSearch();
						Application.DoEvents();

						filterControl.Grid.Select(0);
						Application.DoEvents();

						var okButton = popup.FindAll<ZButton>(button => button.Name == "OK_Button").First();
						okButton.PerformClick();
						Application.DoEvents();
					}
				});

				var attachBookingItem = FindMenuItem(bookingForm, "Actions", "Add to Existing Consol");
				attachBookingItem.PerformClick();
				Application.DoEvents();

				using (var consolForm = Application.OpenForms.OfType<ConsolForm>().Single())
				{
					var openedConsol = (ForwardingConsol)consolForm.BusinessEntity;

					UnitTestUserNotification.Instance.AddOKAnswer();
					var didSave = consolForm.FireSaveButton();
					Application.DoEvents();

					AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", openedConsol);
					AssertNoErrors("Errors on the booking will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", openedBooking);
					AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
					AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
			}

			var newFactory = new BusinessObjectFactory();

			var jobConShipLnk = newFactory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, consol.PK));
			AssertEquals(1, jobConShipLnk.Length);

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().First(x => x.FH_CompletionStatement == "Consol Workflow");
			links = conWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		#region Helpers
		static MenuItem FindMenuItem(Form form, params string[] path)
		{
			return FindMenuItem(form.Menu.MenuItems, path, 0);
		}

		static MenuItem FindMenuItem(Menu.MenuItemCollection items, string[] path, int index)
		{
			MenuItem next = null;

			foreach (MenuItem item in items)
			{
				var text = item.Text.Replace("&", "");
				if (text == path[index])
				{
					next = item;
					break;
				}
			}

			if (next == null)
			{
				throw new ArgumentException(string.Format("cant find '{0}'", path[index]));
			}

			return (index + 1 == path.Length) ? next : FindMenuItem(next.MenuItems, path, index + 1);
		}
		#endregion Helpers
	}
}
