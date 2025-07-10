using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class WorkflowRelationshipTest : TestCaseWithFactory
	{
		#region Applying Templates

		#region Workflow Links between Consols and Shipments

		public void TestConsolForm_WorkflowRelationships_AddShipmentToGrid()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			ForwardingShipment shipment;
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				var loadedConsol = (ForwardingConsol)form.BusinessEntity;

				Application.DoEvents();

				var shipmentGrid = form.FindSingle<ConsolShipmentModuleButtonGrid>();
				AssertEquals("There should be no shipments yet.", 0, shipmentGrid.InnerGrid.List.Count);

				shipmentGrid.InnerGrid.ListManager.AddNew();

				shipment = (ForwardingShipment)shipmentGrid.InnerGrid.ListManager.Current;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_ActualWeight = 20m;
				shipment.JS_ActualVolume = 1m;
				shipment.CreateShipmentJobHeaderWithMutex();
				shipment.JobHeader.JH_GE = department.PK;
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;

				shipmentGrid.InnerGrid.ListManager.EndCurrentEdit();
				Application.DoEvents();

				Assert(!shipment.IsInDatabase);

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = form.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				AssertNoErrors("Errors on the shipment will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", shipment);
				Assert(shipment.IsInDatabase);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestConsolForm_WorkflowRelationships_RemoveShipmentFromGrid()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportCodes.Air;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.JobHeader.JH_GE = department.PK;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;

			consol.Shipments.Add(shipment);

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");

			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);

			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				var loadedConsol = (ForwardingConsol)form.BusinessEntity;
				var loadedShipment = loadedConsol.Factory.Load<CommonShipment>(shipment.PK);

				Application.DoEvents();

				var shipmentGrid = form.FindSingle<ConsolShipmentModuleButtonGrid>();
				AssertEquals("There should be one shipment.", 1, shipmentGrid.InnerGrid.List.Count);

				shipmentGrid.InnerGrid.ListManager.RemoveAt(0);
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
			}

			shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should still be applied.", new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be no link between workflows.", 0, links.Length);

			Factory.Save();
		}

		public void TestConsolForm_WorkflowRelationships_NewButtonOnShipmentGrid()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			ForwardingShipment shipment;
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var consolForm = (ConsolForm)controller.ShowEditForm(consol))
			{
				var loadedConsol = (ForwardingConsol)consolForm.BusinessEntity;

				Application.DoEvents();

				var shipmentGrid = consolForm.FindSingle<ConsolShipmentModuleButtonGrid>();
				AssertEquals("There should be no shipments yet.", 0, shipmentGrid.InnerGrid.List.Count);

				ShipmentForm shipmentForm = null;

				try
				{
					shipmentGrid.FireNewButtonClick();
					Application.DoEvents();

					shipmentForm = (ShipmentForm)shipmentGrid.LastShownZForm;
					shipment = (ForwardingShipment)shipmentForm.BusinessEntity;

					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_ActualWeight = 20m;
					shipment.JS_ActualVolume = 1m;
					shipment.CreateShipmentJobHeaderWithMutex();
					shipment.JobHeader.JH_GE = department.PK;
					shipment.ConsignorPK = consignor.PK;
					shipment.ConsigneePK = consignee.PK;
					shipmentForm.Refresh();
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddOKAnswer();
					var didSave = shipmentForm.FireSaveButton();
					Application.DoEvents();

					AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
					AssertNoErrors("Errors on the shipment will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", shipment);
					AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
					AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
				finally
				{
					shipmentForm?.Dispose();
				}
			}

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestConsolForm_WorkflowRelationships_AttachButtonOnShipmentGrid()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.JobHeader.JH_GE = department.PK;
			shipment.JobHeader.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			shipment.JobHeader.JH_OA_LocalChargesAddr = orgAddress.PK;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";
			shipment.JS_RL_NKDischargePort = "DEFRA";
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Job Workflow", "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be no link between workflows.", 0, links.Length);

			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var consolForm = (ConsolForm)controller.ShowEditForm(consol))
			{
				var loadedConsol = (ForwardingConsol)consolForm.BusinessEntity;
				var loadedShipment = loadedConsol.Factory.Load<CommonShipment>(shipment.PK);

				var shipmentGrid = consolForm.FindSingle<ConsolShipmentModuleButtonGrid>();
				AssertEquals("There should be no shipments yet.", 0, shipmentGrid.InnerGrid.List.Count);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
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
						UnitTestUserNotification.Instance.AddOKAnswer();
						okButton.PerformClick();
						Application.DoEvents();
					}
				});

				var toolStrip = consolForm.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				attachButton.PerformClick();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = consolForm.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				AssertNoErrors("Errors on the shipment will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedShipment);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestConsolForm_WorkflowRelationships_DetachButtonOnShipmentGrid()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.JobHeader.JH_GE = department.PK;
			shipment.JobHeader.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			shipment.JobHeader.JH_OA_LocalChargesAddr = orgAddress.PK;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";
			shipment.JS_RL_NKDischargePort = "DEFRA";
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;

			Factory.Save();

			consol.Shipments.Add(shipment);

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");

			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Job Workflow", "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);

			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var consolForm = (ConsolForm)controller.ShowEditForm(consol))
			{
				var loadedConsol = (ForwardingConsol)consolForm.BusinessEntity;
				var loadedShipment = loadedConsol.Factory.Load<CommonShipment>(shipment.PK);

				var shipmentGrid = consolForm.FindSingle<ConsolShipmentModuleButtonGrid>();
				AssertEquals("There should be one shipments yet.", 1, shipmentGrid.InnerGrid.List.Count);

				UnitTestUserNotification.Instance.AddYesAnswer();
				shipmentGrid.InnerGrid.Select(0);
				Application.DoEvents();

				shipmentGrid.DetachSelectedElement();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = consolForm.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the consol will stop the form from saving and links from being removed, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				AssertNoErrors("Errors on the shipment will stop the form from saving and links from being removed, so fix up any errors listed here please (warnings are okay):", loadedShipment);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				AssertEquals("The user should have been prompted to remove links. SAD!", "Remove Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be no link between workflows.", 0, links.Length);
		}

		public void TestConsolForm_WorkflowRelationships_AttachBookings()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template for Workflow Links";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var bookingTemplate = Factory.New<ProcessTaskTemplate>();
			bookingTemplate.P0_Name = "Booking Template for Workflow Links";
			bookingTemplate.P0_ProcessType = WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode;
			var bookingTemplateWorkflow = helper.CreateWorkflow(bookingTemplate, "Booking Workflow");
			helper.CreateTask(bookingTemplate, bookingTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, bookingTemplateWorkflow);

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

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

			Factory.Save();

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			CommonShipment booking = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "DEFRA";
			booking.ConsigneePK = consignee.PK;
			booking.ConsignorPK = consignor.PK;
			booking.JS_TransportMode = Constants.TransportModes.Air;

			var bookingHeader = helper.GetJobHeaderForParent((IWorkflowProviderCore)booking, Factory, addDefaultProcessHeaderIfNone: false);
			bookingHeader.ApplyTemplateWorkflow(bookingTemplate, bookingTemplateWorkflow);
			var bookingWorkflows = bookingHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Booking Workflow" }, bookingWorkflows.Select(x => x.FH_CompletionStatement));

			var bookingWorkflow = bookingWorkflows.First(x => x.FH_CompletionStatement == "Booking Workflow");
			var links = bookingWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 0, links.Length);

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			using (var consolForm = (ConsolForm)controller.ShowEditForm(consol))
			{
				// we need to reload shipment and consol as we are about to create a link between them on the form factory and saving the factory should save the link as well
				var loadedConsol = (ForwardingConsol)consolForm.BusinessEntity;
				var loadedBooking = loadedConsol.Factory.Load<CommonShipment>(booking.PK);

				ZFormMenuStrategy.AddActionsMenuItem(consolForm, (NoResString)"Attach Bookings For Test", AttachBookingModuleGridHelper_ForTest.NewEventHandler(loadedConsol, loadedBooking));

				var attachBookingItem = FindMenuItem(consolForm, "Actions", "Attach Bookings For Test");
				attachBookingItem.PerformClick();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = consolForm.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				AssertNoErrors("Errors on the booking will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedBooking);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			bookingWorkflow = bookingWorkflows.First(x => x.FH_CompletionStatement == "Booking Workflow");
			links = bookingWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", bookingWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestShipmentForm_WorkflowRelationships_NewConsol()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";
			shipment.JS_RL_NKDischargePort = "DEFRA";
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;

			Factory.Save();

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Job Workflow", "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be no link between workflows.", 0, links.Length);

			ForwardingConsol consol;

			var controller = ZControllerFactory.Create(ControllerIDs.JobShipment);

			using (var shipmentForm = (ShipmentForm)controller.ShowEditForm(shipment))
			{
				var loadedShipment = (ForwardingShipment)shipmentForm.BusinessEntity;

				var additionalDetailTab = shipmentForm.FindAll<ZTabPage>().SingleOrDefault(n => n.CaptionResourceString.Caption == "Additional Detail");
				var tabControl = (ZTabControl)additionalDetailTab.Parent;
				tabControl.SelectedTab = additionalDetailTab;
				Application.DoEvents();

				var additionalDetailsControl = shipmentForm.FindAll<ShipmentAdditionalDetailsControl>().First();
				var consolGrid = additionalDetailsControl.FindAll<ConsolModuleButtonGrid>().First();

				ConsolForm consolForm = null;

				var toolStrip = consolGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var newButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				newButton.PerformClick();
				Application.DoEvents();

				try
				{
					consolForm = (ConsolForm)consolGrid.LastShownZForm;
					consol = (ForwardingConsol)consolForm.BusinessEntity;
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
					consolForm.Refresh();
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddOKAnswer();
					var didSave = consolForm.FireSaveButton();
					Application.DoEvents();

					AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", consol);
					AssertNoErrors("Errors on the shipment will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedShipment);
					AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
					AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
				finally
				{
					consolForm?.Dispose();
				}
			}

			shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Job Workflow", "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().First(x => x.FH_CompletionStatement == "Consol Workflow");
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestShipmentForm_WorkflowRelationships_AttachConsol()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

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

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";
			shipment.JS_RL_NKDischargePort = "DEFRA";
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;

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

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Job Workflow", "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().First(x => x.FH_CompletionStatement == "Consol Workflow");
			var links = conWorkflow.Links.ToArray();
			AssertEquals("There should be no link between workflows.", 0, links.Length);

			var controller = ZControllerFactory.Create(ControllerIDs.JobShipment);

			using (var shipmentForm = (ShipmentForm)controller.ShowEditForm(shipment))
			{
				var loadedShipment = (ForwardingShipment)shipmentForm.BusinessEntity;
				var loadedConsol = loadedShipment.Factory.Load<ForwardingConsol>(consol.PK);

				var additionalDetailTab = shipmentForm.FindAll<ZTabPage>().SingleOrDefault(n => n.CaptionResourceString.Caption == "Additional Detail");
				var tabControl = (ZTabControl)additionalDetailTab.Parent;
				tabControl.SelectedTab = additionalDetailTab;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
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

						UnitTestUserNotification.Instance.AddOKAnswer();
						var okButton = popup.FindAll<ZButton>(button => button.Name == "OK_Button").First();
						okButton.PerformClick();
						Application.DoEvents();
					}
				});

				var additionalDetailsControl = shipmentForm.FindAll<ShipmentAdditionalDetailsControl>().First();
				var consolGrid = additionalDetailsControl.FindAll<ConsolModuleButtonGrid>().First();
				var toolStrip = consolGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				attachButton.PerformClick();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = shipmentForm.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				AssertNoErrors("Errors on the shipment will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedShipment);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links SHP->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			shpWorkflows = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Job Workflow", "Shipment Workflow" }, shpWorkflows.Select(x => x.FH_CompletionStatement));

			conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().First(x => x.FH_CompletionStatement == "Consol Workflow");
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		#endregion

		#region Workflow Links between Consols and Containers

		public void TestConsolForm_WorkflowRelationships_AddContainerToGrid()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ContainerWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template for Workflow Links";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var cntTemplate = Factory.New<ProcessTaskTemplate>();
			cntTemplate.P0_Name = "Container Template for Workflow Links";
			cntTemplate.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			var cntTemplateWorkflow = helper.CreateWorkflow(cntTemplate, "Container Workflow");
			helper.CreateTask(cntTemplate, cntTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, cntTemplateWorkflow);

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
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			ForwardingContainer container;
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				form.SelectContainersTabPage_ForTest();
				Application.DoEvents();

				var loadedConsol = (ForwardingConsol)form.BusinessEntity;

				var containerGrid = form.FindSingle<ZModuleButtonGrid>("JobContainerBoundGrid");
				AssertEquals("There should be a container being edited", 1, containerGrid.InnerGrid.List.Count);

				container = (ForwardingContainer)containerGrid.InnerGrid.ListManager.Current;
				container.JC_ContainerNum = "CNTR001";
				container.JC_RC = Factory.LoadTop1(typeof(RefContainer), new ZQuery(RefContainerSchema.RC_Code, "PM-2H")).PK;
				container.JC_ContainerMode = Constants.ContainerModes.ULD;

				containerGrid.InnerGrid.ListManager.EndCurrentEdit();
				Application.DoEvents();

				Assert(!container.IsInDatabase);

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = form.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the container will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", container);
				AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				Assert("The container should be saved", container.IsInDatabase);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				AssertEquals("The user should have been prompted to create inter-job links. SAD!", "Create Workflow Links CNT->CON", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			var loadedContainer = Factory.Load<ForwardingContainer>(container.PK);
			AssertNotNull(loadedContainer);

			var cntJobHeader = helper.GetJobHeaderForParent(loadedContainer, Factory, addDefaultProcessHeaderIfNone: false);
			var cntWorkflows = cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Container Workflow" }, cntWorkflows.Select(x => x.FH_CompletionStatement));

			var cntWorkflow = cntWorkflows.First(x => x.FH_CompletionStatement == "Container Workflow");
			var links = cntWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the container and the consol.", cntWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the container and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		#endregion

		#endregion

		#region Reapplying Templates

		#region Workflow Links between Consols and Shipments

		public void TestShouldReapplyTemplates_ToCreateWorkflowLinksBetweenConsolsAndShipments_WhenSavingModifiedConsol()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var genericConTemplate = Factory.New<ProcessTaskTemplate>();
			genericConTemplate.P0_Name = "Generic Consol Template";
			genericConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var genericConTemplateWorkflow = helper.CreateWorkflow(genericConTemplate, "Generic Consol Workflow");
			helper.CreateTask(genericConTemplate, genericConTemplateWorkflow);

			var specificConTemplate = Factory.New<ProcessTaskTemplate>();
			specificConTemplate.P0_Name = "Specific Consol Template";
			specificConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			specificConTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JK_RL_NKLoadPort)

			var specificConTemplateWorkflow = helper.CreateWorkflow(specificConTemplate, "Specific Consol Workflow");
			var specificConTemplateTask = helper.CreateTask(specificConTemplate, specificConTemplateWorkflow, description: "Specific Conditional Consol Task");
			specificConTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved consol
			specificConTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(genericConTemplate, genericConTemplateWorkflow, shpTemplateWorkflow);
			helper.CreateDependencyLink(specificConTemplate, specificConTemplateWorkflow, shpTemplateWorkflow);
			helper.CreateDependencyLink(specificConTemplate, genericConTemplateWorkflow, specificConTemplateWorkflow);

			Factory.Save();

			var consolAndShipment = CreateConsolAndAttachedShipment();
			var consol = consolAndShipment.Consol;
			var shipment = consolAndShipment.Shipment;

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Generic Consol Workflow");
			Assert("Precondition", genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Shipment Workflow"));

			consol.JK_RL_NKLoadPort = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			UnitTestUserNotification.Instance.AddOKAnswer(); // confirm creating links in Create Workflow Links dialog
			Factory.Save();
			AssertEquals(false, consol.HasChanges);

			var newFactory = new BusinessObjectFactory();

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Consol Workflow", "Specific Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			shpJobHeader = helper.GetJobHeaderForParent(shipment, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var specificConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Specific Consol Workflow");
			Assert(specificConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Shipment Workflow"));
			Assert(specificConWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Generic Consol Workflow"));
		}

		public void TestShouldReapplyTemplates_ToCreateWorkflowLinksBetweenConsolsAndShipments_WhenSavingModifiedShipment()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var genericShpTemplate = Factory.New<ProcessTaskTemplate>();
			genericShpTemplate.P0_Name = "Generic Shipment Template";
			genericShpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var genericShpTemplateWorkflow = helper.CreateWorkflow(genericShpTemplate, "Generic Shipment Workflow");
			helper.CreateTask(genericShpTemplate, genericShpTemplateWorkflow);

			var specificShpTemplate = Factory.New<ProcessTaskTemplate>();
			specificShpTemplate.P0_Name = "Specific Shipment Template";
			specificShpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			specificShpTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JS_RL_NKOrigin)

			var specificShpTemplateWorkflow = helper.CreateWorkflow(specificShpTemplate, "Specific Shipment Workflow");
			var specificShpTemplateTask = helper.CreateTask(specificShpTemplate, specificShpTemplateWorkflow, description: "Specific Conditional Shipment Task");
			specificShpTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved shipment
			specificShpTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			helper.CreateDependencyLink(genericShpTemplate, conTemplateWorkflow, genericShpTemplateWorkflow);
			helper.CreateDependencyLink(specificShpTemplate, conTemplateWorkflow, specificShpTemplateWorkflow);
			helper.CreateDependencyLink(specificShpTemplate, genericShpTemplateWorkflow, specificShpTemplateWorkflow);

			Factory.Save();

			var consolAndShipment = CreateConsolAndAttachedShipment();
			var consol = consolAndShipment.Consol;
			var shipment = consolAndShipment.Shipment;

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Consol Workflow");
			Assert("Precondition", genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Generic Shipment Workflow"));

			shipment.JS_RL_NKOrigin = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			UnitTestUserNotification.Instance.AddOKAnswer(); // confirm creating links in Create Workflow Links dialog
			Factory.Save();
			AssertEquals(false, shipment.HasChanges);

			var newFactory = new BusinessObjectFactory();

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			shpJobHeader = helper.GetJobHeaderForParent(shipment, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Shipment Workflow", "Specific Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var specificShpWorkflow = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Specific Shipment Workflow");
			Assert(specificShpWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Consol Workflow"));
			Assert(specificShpWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Generic Shipment Workflow"));
		}

		public void TestShouldShowCreateWorkflowLinkDialogOnce_WhenModifyingAndSavingBothConsolAndShipmentResultsInApplyingTheSameTemplatesTwice()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var genericConTemplate = Factory.New<ProcessTaskTemplate>();
			genericConTemplate.P0_Name = "Generic Consol Template";
			genericConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var genericConTemplateWorkflow = helper.CreateWorkflow(genericConTemplate, "Generic Consol Workflow");
			helper.CreateTask(genericConTemplate, genericConTemplateWorkflow);

			var specificConTemplate = Factory.New<ProcessTaskTemplate>();
			specificConTemplate.P0_Name = "Specific Consol Template";
			specificConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			specificConTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JK_RL_NKLoadPort)

			var specificConTemplateWorkflow = helper.CreateWorkflow(specificConTemplate, "Specific Consol Workflow");
			var specificConTemplateTask = helper.CreateTask(specificConTemplate, specificConTemplateWorkflow, description: "Specific Conditional Consol Task");
			specificConTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved consol
			specificConTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(genericConTemplate, genericConTemplateWorkflow, shpTemplateWorkflow);
			helper.CreateDependencyLink(specificConTemplate, specificConTemplateWorkflow, shpTemplateWorkflow);

			Factory.Save();

			var consolAndShipment = CreateConsolAndAttachedShipment();
			var consol = consolAndShipment.Consol;
			var shipment = consolAndShipment.Shipment;

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Generic Consol Workflow");
			Assert("Precondition", genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Shipment Workflow"));

			consol.JK_RL_NKLoadPort = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)
			shipment.JS_ActualWeight = 21m; // some minor changes to shipment not resulting in applying new templates
			AssertEquals(true, shipment.HasChanges);

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddOKAnswer(); // confirm creating links in Create Workflow Links dialog
			UnitTestUserNotification.Instance.AddOKAnswer(); // the second confirmation in case if the dialog is shown twice
			Factory.Save();
			AssertEquals(false, consol.HasChanges);
			AssertEquals(false, shipment.HasChanges);

			var newFactory = new BusinessObjectFactory();

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Consol Workflow", "Specific Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			shpJobHeader = helper.GetJobHeaderForParent(shipment, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var specificConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Specific Consol Workflow");
			Assert(specificConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Shipment Workflow"));

			AssertEquals("Create Workflow Links dialog should not be shown excessively", 1, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.Text == "WorkflowProvidersLinkageUserControl"));
		}

		public void TestShouldShowCreateWorkflowLinkDialogTwice_WhenModifyingAndSavingBothConsolAndShipmentResultsInApplyingDifferentSetsOfTemplates()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var genericConTemplate = Factory.New<ProcessTaskTemplate>();
			genericConTemplate.P0_Name = "Generic Consol Template";
			genericConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var genericConTemplateWorkflow = helper.CreateWorkflow(genericConTemplate, "Generic Consol Workflow");
			helper.CreateTask(genericConTemplate, genericConTemplateWorkflow);

			var specificConTemplate = Factory.New<ProcessTaskTemplate>();
			specificConTemplate.P0_Name = "Specific Consol Template";
			specificConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			specificConTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JK_RL_NKLoadPort)

			var specificConTemplateWorkflow = helper.CreateWorkflow(specificConTemplate, "Specific Consol Workflow");
			var specificConTemplateTask = helper.CreateTask(specificConTemplate, specificConTemplateWorkflow, description: "Specific Conditional Consol Task");
			specificConTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved consol
			specificConTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			var genericShpTemplate = Factory.New<ProcessTaskTemplate>();
			genericShpTemplate.P0_Name = "Generic Shipment Template";
			genericShpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var genericShpTemplateWorkflow = helper.CreateWorkflow(genericShpTemplate, "Generic Shipment Workflow");
			helper.CreateTask(genericShpTemplate, genericShpTemplateWorkflow);

			var specificShpTemplate = Factory.New<ProcessTaskTemplate>();
			specificShpTemplate.P0_Name = "Specific Shipment Template";
			specificShpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			specificShpTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JS_RL_NKOrigin)

			var specificShpTemplateWorkflow = helper.CreateWorkflow(specificShpTemplate, "Specific Shipment Workflow");
			var specificShpTemplateTask = helper.CreateTask(specificShpTemplate, specificShpTemplateWorkflow, description: "Specific Conditional Shipment Task");
			specificShpTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved shipment
			specificShpTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			helper.CreateDependencyLink(genericConTemplate, genericConTemplateWorkflow, genericShpTemplateWorkflow);
			helper.CreateDependencyLink(genericConTemplate, genericConTemplateWorkflow, specificShpTemplateWorkflow);
			helper.CreateDependencyLink(genericShpTemplate, specificConTemplateWorkflow, genericShpTemplateWorkflow);

			Factory.Save();

			var consolAndShipment = CreateConsolAndAttachedShipment();
			var consol = consolAndShipment.Consol;
			var shipment = consolAndShipment.Shipment;

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Generic Consol Workflow");
			Assert("Precondition", genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Generic Shipment Workflow"));

			consol.JK_RL_NKLoadPort = "ADALV"; // modify so that the specific consol template is applicable now (corresponds to P0_LoadPortCountry)
			shipment.JS_RL_NKOrigin = "ADALV"; // modify so that the specific shipment template is applicable now (corresponds to P0_LoadPortCountry)

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddOKAnswer(); // confirm creating links in Create Workflow Links dialog
			UnitTestUserNotification.Instance.AddOKAnswer(); // the second confirmation in case if the dialog is shown twice
			Factory.Save();
			AssertEquals(false, consol.HasChanges);
			AssertEquals(false, shipment.HasChanges);

			var newFactory = new BusinessObjectFactory();

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Consol Workflow", "Specific Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			shpJobHeader = helper.GetJobHeaderForParent(shipment, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Shipment Workflow", "Specific Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Generic Consol Workflow");
			Assert(genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Specific Shipment Workflow"));

			var genericShpWorkflow = shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Generic Shipment Workflow");
			Assert(genericShpWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Specific Consol Workflow"));

			AssertEquals("Create Workflow Links dialog should be shown twice as modification of both consol and shipment results in creating different links", 2, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.Text == "WorkflowProvidersLinkageUserControl"));
		}

		#endregion

		#region Workflow Links between Consols and Containers

		public void TestShouldReapplyTemplates_ToCreateWorkflowLinksBetweenConsolsAndContainers_WhenSavingModifiedConsol()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ContainerWorkflowDescriptorCode);

			var genericConTemplate = Factory.New<ProcessTaskTemplate>();
			genericConTemplate.P0_Name = "Generic Consol Template";
			genericConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var genericConTemplateWorkflow = helper.CreateWorkflow(genericConTemplate, "Generic Consol Workflow");
			helper.CreateTask(genericConTemplate, genericConTemplateWorkflow);

			var specificConTemplate = Factory.New<ProcessTaskTemplate>();
			specificConTemplate.P0_Name = "Specific Consol Template";
			specificConTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			specificConTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JK_RL_NKLoadPort)

			var specificConTemplateWorkflow = helper.CreateWorkflow(specificConTemplate, "Specific Consol Workflow");
			var specificConTemplateTask = helper.CreateTask(specificConTemplate, specificConTemplateWorkflow, description: "Specific Conditional Consol Task");
			specificConTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved consol
			specificConTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			var cntTemplate = Factory.New<ProcessTaskTemplate>();
			cntTemplate.P0_Name = "Container Template";
			cntTemplate.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			var cntTemplateWorkflow = helper.CreateWorkflow(cntTemplate, "Container Workflow");
			helper.CreateTask(cntTemplate, cntTemplateWorkflow);

			helper.CreateDependencyLink(genericConTemplate, genericConTemplateWorkflow, cntTemplateWorkflow);
			helper.CreateDependencyLink(specificConTemplate, specificConTemplateWorkflow, cntTemplateWorkflow);
			helper.CreateDependencyLink(specificConTemplate, genericConTemplateWorkflow, specificConTemplateWorkflow);

			Factory.Save();

			var consolAndContainer = CreateConsolAndAttachedContainer();
			var consol = consolAndContainer.Consol;
			var container = consolAndContainer.Container;

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var cntJobHeader = helper.GetJobHeaderForParent(container, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Container Workflow" }, cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Generic Consol Workflow");
			Assert("Precondition", genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Container Workflow"));

			consol.JK_RL_NKLoadPort = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			UnitTestUserNotification.Instance.AddOKAnswer(); // confirm creating links in Create Workflow Links dialog
			Factory.Save();
			AssertEquals(false, consol.HasChanges);

			var newFactory = new BusinessObjectFactory();

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Consol Workflow", "Specific Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			cntJobHeader = helper.GetJobHeaderForParent(container, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Container Workflow" }, cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var specificConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Specific Consol Workflow");
			Assert(specificConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Container Workflow"));
			Assert(specificConWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Generic Consol Workflow"));
		}

		public void TestShouldReapplyTemplates_ToCreateWorkflowLinksBetweenConsolsAndContainers_WhenSavingModifiedContainer()
		{
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ContainerWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var genericCntTemplate = Factory.New<ProcessTaskTemplate>();
			genericCntTemplate.P0_Name = "Generic Container Template";
			genericCntTemplate.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;

			var genericCntTemplateWorkflow = helper.CreateWorkflow(genericCntTemplate, "Generic Container Workflow");
			helper.CreateTask(genericCntTemplate, genericCntTemplateWorkflow);

			var specificCntTemplate = Factory.New<ProcessTaskTemplate>();
			specificCntTemplate.P0_Name = "Specific Container Template";
			specificCntTemplate.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			specificCntTemplate.P0_SubType2 = "FCL"; // this makes the template specific (corresponds to JC_ContainerMode)

			var specificCntTemplateWorkflow = helper.CreateWorkflow(specificCntTemplate, "Specific Container Workflow");
			var specificCntTemplateTask = helper.CreateTask(specificCntTemplate, specificCntTemplateWorkflow, description: "Specific Conditional Container Task");
			specificCntTemplateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // to ensure the specific template is applicable even to the saved container
			specificCntTemplateTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			helper.CreateDependencyLink(genericCntTemplate, conTemplateWorkflow, genericCntTemplateWorkflow);
			helper.CreateDependencyLink(specificCntTemplate, conTemplateWorkflow, specificCntTemplateWorkflow);
			helper.CreateDependencyLink(specificCntTemplate, genericCntTemplateWorkflow, specificCntTemplateWorkflow);

			Factory.Save();

			var consolAndContainer = CreateConsolAndAttachedContainer();
			var consol = consolAndContainer.Consol;
			var container = consolAndContainer.Container;

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var cntJobHeader = helper.GetJobHeaderForParent(container, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { "Generic Container Workflow" }, cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var genericConWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Consol Workflow");
			Assert("Precondition", genericConWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Generic Container Workflow"));

			container.JC_ContainerMode = "FCL"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			UnitTestUserNotification.Instance.AddOKAnswer(); // confirm creating links in Create Workflow Links dialog
			Factory.Save();
			AssertEquals(false, container.HasChanges);

			var newFactory = new BusinessObjectFactory();

			conJobHeader = helper.GetJobHeaderForParent(consol, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			cntJobHeader = helper.GetJobHeaderForParent(container, newFactory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Generic Container Workflow", "Specific Container Workflow" }, cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var specificCntWorkflow = cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Specific Container Workflow");
			Assert(specificCntWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Consol Workflow"));
			Assert(specificCntWorkflow.Links.Any(l => l.HeaderFrom.FH_CompletionStatement == "Generic Container Workflow"));
		}

		#endregion

		#endregion

		#region Applying Templates in Non Interactive Mode

		public void TestShouldCreateWorkflowLinksWithoutConfirmation_WhenApplyingTemplatesInNonInteractiveMode_IfDefinedByRegistryItem()
		{
			helper.SetAlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates(true);

			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, shpTemplateWorkflow);

			Factory.Save();

			ForwardingConsol consol;
			ForwardingShipment shipment;

			// emulates applying templates in non interactive mode like when applying partial templates as the result of activating triggers and milestones 
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var consolAndShipment = CreateConsolAndAttachedShipment();
				consol = consolAndShipment.Consol;
				shipment = consolAndShipment.Shipment;
			}

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Consol Workflow");
			Assert("The link should be created", conWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Shipment Workflow"));

			AssertEquals("Should not show Create Workflow Links dialog (should not require confirmation)", 0, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.Text == "WorkflowProvidersLinkageUserControl"));
		}

		public void TestShouldNotCreateWorkflowLinks_WhenApplyingTemplatesInNonInteractiveMode_IfDefinedByRegistryItem()
		{
			helper.SetAlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates(false);

			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, shpTemplateWorkflow);

			Factory.Save();

			ForwardingConsol consol;
			ForwardingShipment shipment;

			// emulates applying templates in non interactive mode like when applying partial templates as the result of activating triggers and milestones 
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var consolAndShipment = CreateConsolAndAttachedShipment();
				consol = consolAndShipment.Consol;
				shipment = consolAndShipment.Shipment;
			}

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Consol Workflow", }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var shpJobHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder(new[] { "Shipment Workflow" }, shpJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(h => h.FH_CompletionStatement));

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single(h => h.FH_CompletionStatement == "Consol Workflow");
			Assert("The link should not be created", !conWorkflow.Links.Any(l => l.HeaderTo.FH_CompletionStatement == "Shipment Workflow"));

			AssertEquals("Should not show Create Workflow Links dialog", 0, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.Text == "WorkflowProvidersLinkageUserControl"));
		}

		#endregion

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

		(ForwardingConsol Consol, ForwardingShipment Shipment) CreateConsolAndAttachedShipment()
		{
			var newFactory = new BusinessObjectFactory();

			var department = newFactory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

			var consignor = newFactory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = newFactory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			newFactory.Save();

			ForwardingShipment shipment;
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				Application.DoEvents();

				var shipmentGrid = form.FindSingle<ConsolShipmentModuleButtonGrid>();

				shipmentGrid.InnerGrid.ListManager.AddNew();

				shipment = (ForwardingShipment)shipmentGrid.InnerGrid.ListManager.Current;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_ActualWeight = 20m;
				shipment.JS_ActualVolume = 1m;
				shipment.CreateShipmentJobHeaderWithMutex();
				shipment.JobHeader.JH_GE = department.PK;
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;

				shipmentGrid.InnerGrid.ListManager.EndCurrentEdit();
				Application.DoEvents();

				Assert(!shipment.IsInDatabase);

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.FireSaveButton();
				Application.DoEvents();
			}

			var loadedConsol = Factory.Load<ForwardingConsol>(consol.PK);
			var loadedShipment = Factory.Load<ForwardingShipment>(shipment.PK);

			AssertNotNull(loadedConsol);
			AssertNotNull(loadedShipment);

			return (loadedConsol, loadedShipment);
		}

		(ForwardingConsol Consol, ForwardingContainer Container) CreateConsolAndAttachedContainer()
		{
			var newFactory = new BusinessObjectFactory();

			var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			newFactory.Save();

			ForwardingContainer container;
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				form.SelectContainersTabPage_ForTest();
				Application.DoEvents();

				var containerGrid = form.FindSingle<ZModuleButtonGrid>("JobContainerBoundGrid");
				AssertEquals("There should be a container being edited", 1, containerGrid.InnerGrid.List.Count);

				container = (ForwardingContainer)containerGrid.InnerGrid.ListManager.Current;
				container.JC_ContainerNum = "CNTR001";
				container.JC_RC = Factory.LoadTop1(typeof(RefContainer), new ZQuery(RefContainerSchema.RC_Code, "PM-2H")).PK;
				container.JC_ContainerMode = Constants.ContainerModes.ULD;

				containerGrid.InnerGrid.ListManager.EndCurrentEdit();
				Application.DoEvents();

				Assert(!container.IsInDatabase);

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = form.FireSaveButton();
				Application.DoEvents();
			}

			var loadedConsol = Factory.Load<ForwardingConsol>(consol.PK);
			var loadedContainer = Factory.Load<ForwardingContainer>(container.PK);

			AssertNotNull(loadedConsol);
			AssertNotNull(loadedContainer);

			return (loadedConsol, loadedContainer);
		}
		#endregion Helpers

		#region Setup and Tear Down

		IBMTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();

			helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
		}

		#endregion
	}
}
