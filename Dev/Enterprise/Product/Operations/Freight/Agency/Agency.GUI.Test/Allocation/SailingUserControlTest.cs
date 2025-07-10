using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class SailingUserControlTest : BaseAgencyTest
	{
		public void TestCreateSailingWithoutLoadDischarge()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_A_BKD = ZDateTime.Now;
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("No ports specified, error message should show", "An Origin & Destination or Load & Discharge must exist before a new Sailing can be created.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("Origin and Destination ports specified, sailing should fallback to these, no error message expected", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ClearShipmentPorts(shipment);
				shipment.JS_RL_NKOrigin = "AUSYD";
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("Not enough ports specified, error message should show", "An Origin & Destination or Load & Discharge must exist before a new Sailing can be created.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ClearShipmentPorts(shipment);
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_NKDischargePort = "USLAX";
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("Discharge specified and Load should fallback from Origin, no error message expected", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void ClearShipmentPorts(AgencyShipment shipment)
		{
			shipment.JS_RL_NKOrigin = ZString.Empty;
			shipment.JS_RL_NKDestination = ZString.Empty;
			shipment.JS_NKLoadPort = ZString.Empty;
			shipment.JS_NKDischargePort = ZString.Empty;
		}

		public void TestSelectSailing()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ZGuid.Empty;
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
				Application.DoEvents();
				AssertNoExceptionThrown(form.Control.PerformClickSelectSailingForTest);
			}
		}

		[GuiTest]
		public void TestEditSailing()
		{
			ZDateTime now = ZDateTime.Now;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Voyage";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			sailing.Origin.JA_E_DEP = now.AddDays(10);
			sailing.Destination.JB_E_ARV = now.AddDays(15);
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
				Application.DoEvents();
				shipment.Sailings.Add(sailing);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickEditSailingForTest();
				AssertEquals("Should be Information message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Please save this Booking before attempting to edit the Sailing."));
			}

			Factory.Save();
			var shipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			using (FormForTestingControl form = new FormForTestingControl(shipment2))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickEditSailingForTest();
				AssertNull("Should be NULL", form.Control.LastCreatedControllerForTesting);
				AssertEquals("Should be Information message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Unable to edit Sailing Schedule because it was not created. Use \"Create Sailing\" or \"Select Sailing\" buttons."));
				shipment2.Sailings.Add(sailing);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickEditSailingForTest();
				AssertEquals("Should be JobSeaVoyage controller", ControllerIDs.JobSeaVoyage, form.Control.LastCreatedControllerForTesting.ID);
				AssertEquals("Should NOT be Information message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Unable to edit Sailing Schedule because it was not created. Use \"Create Sailing\" or \"Select Sailing\" buttons."));
				AssertEquals("Should NOT be Information message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Please save this Booking before attempting to edit the Sailing."));
				AssertNotNull(form.Control.LastCreatedControllerForTesting.LastShownForm);
				form.Control.LastCreatedControllerForTesting.LastShownForm.Dispose();
			}
		}

		[GuiTest]
		public void TestChangingSailingWhenPosted()
		{
			ZDateTime now = ZDateTime.Now;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = principal.PK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			sailing.Origin.JA_E_DEP = now.AddDays(10);
			sailing.Destination.JB_E_ARV = now.AddDays(15);
			shipment.Sailings.Add(sailing);
			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			AccTransactionLines lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_ARLine = lines.PK;
			lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_APLine = lines.PK;
			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("Should be Information message", "Unable to create/select/clear Sailing Schedule because job has been invoiced.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickSelectSailingForTest();
				AssertEquals("Should be Information message", "Unable to create/select/clear Sailing Schedule because job has been invoiced.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickClearSailingForTest();
				AssertEquals("Should be Information message", "Unable to create/select/clear Sailing Schedule because job has been invoiced.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickEditSailingForTest();
				AssertEquals("Should be Information message", "Please save this Booking before attempting to edit the Sailing.", UnitTestUserNotification.Instance.LastMessage.Text);
				charge.APLine.AL_LineType = TransactionLineTypes.Accrual;
				((IJobInvoicingPlugIn)shipment).InvoicingSupporter.PostedStateChanged();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("Should NOT be Information message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickSelectSailingForTest();
				AssertEquals("Should NOT be Information message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickClearSailingForTest();
				AssertEquals("Should NOT be Information message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickEditSailingForTest();
				AssertEquals("Should be Information message", "Please save this Booking before attempting to edit the Sailing.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[GuiTest]
		public void TestAllowChangeSailingWhenPosted()
		{
			var now = ZDateTime.Now;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = principal.PK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_E_DEP = now.AddDays(10);
			sailing.Destination.JB_E_ARV = now.AddDays(15);
			shipment.Sailings.Add(sailing);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			var lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_ARLine = lines.PK;
			lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_APLine = lines.PK;
			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			using (LinerAgencyDataRegistry.Instance.AllowSailingChangeWhenInvoiceIsPosted.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickCreateSailingForTest();
				AssertEquals("Information message not shown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickSelectSailingForTest();
				AssertEquals("Information message not shown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Control.PerformClickClearSailingForTest();
				AssertEquals("Information message not shown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestImportExportSailing()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ZGuid.Empty;
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should show the export panel for non-imports", true, form.ExportPanel.Visible);
				AssertEquals("Should hide the import panel for non-imports", false, form.ImportPanel.Visible);
				shipment.JS_JX = ImportSailing.PK;
				AssertEquals("Should hide the export panel for imports", false, form.ExportPanel.Visible);
				AssertEquals("Should show the import panel for imports", true, form.ImportPanel.Visible);
				shipment.JS_JX = ExportSailing.PK;
				AssertEquals("Should show the export panel for non-imports", true, form.ExportPanel.Visible);
				AssertEquals("Should hide the import panel for non-imports", false, form.ImportPanel.Visible);
			}
		}

		#region FormForTestingControl
		class FormForTestingControl : ZForm
		{
			public FormForTestingControl(AgencyShipment shipment) : base(shipment)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Control = new SailingUserControl();
				Control.Dock = DockStyle.Fill;
				this.Controls.Add(Control);
				ImportPanel = (ZPanel)typeof(SailingUserControl).GetField("ImportPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Control);
				ExportPanel = (ZPanel)typeof(SailingUserControl).GetField("ExportPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Control);
			}

			public SailingUserControl Control;
			public ZPanel ImportPanel;
			public ZPanel ExportPanel;
		}
		#endregion
	}
}
