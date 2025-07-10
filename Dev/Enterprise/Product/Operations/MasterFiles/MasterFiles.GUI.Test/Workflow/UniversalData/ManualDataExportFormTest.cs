using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Business.UniversalData.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Workflow.UniversalData.Testing
{
	[TestedType(typeof(ManualDataExportForm))]
	sealed class ManualDataExportFormTest : ZFormBasherTest
	{
		public void TestSendAndCloseFunctionality()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!"))
			{
				ZFormModaliser.ShowDialogsInTest = true;

				var dummyBO = Factory.New<DummyWithWorkflow>();
				Factory.Save();

				using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalEvent))
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					int formClosedCount = 0;
					form.FormClosed += delegate
					{ formClosedCount++; };
					form.Show();
					form.SendAndCloseButton.PerformClick();
					AssertEquals("Last Message Shown", @"
Error You cannot send a Universal Event without filling in all mandatory fields.

Please fix all validation errors and try again.
".Trim(), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Form Closed Count", 0, formClosedCount);
					AssertNull(lastShownNotifications);

					form.dataExportBO.Calc_RecipientType = "ORP";
					form.dataExportBO.EventCode = "ATH";

					form.SendAndCloseButton.PerformClick();

					AssertMultilineASCIIEquals("notifications",
	@"Processing Dummy Business Object Default
Universal Event queued for sending to Organization [EDICUS].
", lastShownNotifications);
					AssertEquals("Form Closed Count", 1, formClosedCount);
				}
			}
		}

		public void TestSendAndCloseFunctionality_BizoFromMainFormFactory()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!"))
			{
				ZFormModaliser.ShowDialogsInTest = true;

				var dummyBO = new BusinessObjectFactory().New<DummyWithWorkflow>();
				dummyBO.Factory.Save();

				using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalEvent))
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					form.Show();
					form.dataExportBO.Calc_RecipientType = "ORP";
					form.dataExportBO.EventCode = "ATH";
					form.SendAndCloseButton.PerformClick();

					AssertContains("notifications", "Universal Event queued", lastShownNotifications);
					AssertEquals("All changes and Save should happen within this form by its own factory", false, dummyBO.HasChanges);
				}
			}
		}

		public void TestSendAndCloseFunctionality_withUniversalDataBussCommunicationMode()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!"))
			{
				ZFormModaliser.ShowDialogsInTest = true;

				var dummyBO = Factory.New<DummyWithWorkflow>();
				Factory.Save();

				var communicationModes = new List<IEDICommunicationsMode>();
				var mode = new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } };
				communicationModes.AddRange(mode);

				using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalEvent, communicationModes))
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					int formClosedCount = 0;
					form.FormClosed += delegate
					{ formClosedCount++; };
					form.Show();
					form.SendAndCloseButton.PerformClick();
					AssertEquals("Last Message Shown", @"
Error You cannot send a Universal Event without filling in all mandatory fields.

Please fix all validation errors and try again.
".Trim(), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Form Closed Count", 0, formClosedCount);
					AssertNull(lastShownNotifications);

					form.dataExportBO.Calc_RecipientType = "ORP";
					form.dataExportBO.EventCode = "ATH";

					form.SendAndCloseButton.PerformClick();

					AssertMultilineASCIIEquals("notifications",
@"Processing Dummy Business Object Default
Universal Event sent internally.
", lastShownNotifications);
					AssertEquals("Form Closed Count", 1, formClosedCount);
				}
			}
		}

		void GatherNotifications(object sender, EventArgs e)
		{
			var notificationsForm = sender as ManualDataExportProgressForm;
			var notificationsTextBox = (ZTextBox)notificationsForm.Controls.Find("notificationsTextBox", true)[0];
			lastShownNotifications = notificationsTextBox.Text;
			notificationsForm.Closed -= GatherNotifications;
		}

		public void TestTitleEqualDataTypeName()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var manualDataExportUniversalEvent = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			Factory.Save();

			using (var form = new ManualDataExportProgressForm(manualDataExportUniversalEvent.DataTypeName))
			{
				new ManualDataExportProgressFormSendOnShow(manualDataExportUniversalEvent).Apply(form);

				var zChildTitle = (ZLabel)form.Controls.Find("zChildTitle", true)[0];

				AssertContains(manualDataExportUniversalEvent.DataTypeName, zChildTitle.Text);
			}

			var manualDataExportUniversalShipment = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			Factory.Save();

			using (var form = new ManualDataExportProgressForm(manualDataExportUniversalShipment.DataTypeName))
			{
				new ManualDataExportProgressFormSendOnShow(manualDataExportUniversalShipment).Apply(form);

				var zChildTitle = (ZLabel)form.Controls.Find("zChildTitle", true)[0];

				AssertContains(manualDataExportUniversalShipment.DataTypeName, zChildTitle.Text);
			}
		}

		public void TestChangeVisibility()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!"))
			{
				ZFormModaliser.ShowDialogsInTest = true;

				var dummyBO = Factory.New<DummyWithWorkflow>();
				Factory.Save();
				var eventFormOriginalHeight = 0;
				var shipmentFormOriginalHeight = 0;

				using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalEvent))
				{
					form.Show();
					AssertEquals("UniversalEvent form.RecipientTypeDropEdit.Visible", false, form.RecipientTypeDropEdit.Visible);
					AssertEquals("UniversalEvent form.RecipientPKGuidFindBox.Visible", false, form.RecipientPKGuidFindBox.Visible);
					AssertEquals("UniversalEvent form.RecipientServiceDropEdit.Visible", false, form.RecipientServiceDropEdit.Visible);
					eventFormOriginalHeight = form.Size.Height;

					form.dataExportBO.Calc_RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
					AssertEquals("UniversalEvent form.RecipientTypeDropEdit.Visible", false, form.RecipientTypeDropEdit.Visible);
					AssertEquals("UniversalEvent form.RecipientPKGuidFindBox.Visible", false, form.RecipientPKGuidFindBox.Visible);
					AssertEquals("UniversalEvent form.RecipientServiceDropEdit.Visible", false, form.RecipientServiceDropEdit.Visible);
					AssertEquals("UniversalEvent form height", eventFormOriginalHeight, form.Size.Height);

					form.dataExportBO.Calc_RecipientType = MessageRecipientPartyTypeList.SpecialCodes.Other;
					AssertEquals("UniversalEvent form.RecipientTypeDropEdit.Visible", true, form.RecipientTypeDropEdit.Visible);
					AssertEquals("UniversalEvent form.RecipientPKGuidFindBox.Visible", true, form.RecipientPKGuidFindBox.Visible);
					AssertEquals("UniversalEvent form.RecipientServiceDropEdit.Visible", false, form.RecipientServiceDropEdit.Visible);
					AssertEquals("UniversalEvent form height", eventFormOriginalHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(44), form.Size.Height);
				}

				using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalShipment))
				{
					form.Show();
					AssertEquals("UniversalShipment form.RecipientTypeDropEdit.Visible", false, form.RecipientTypeDropEdit.Visible);
					AssertEquals("UniversalShipment form.RecipientPKGuidFindBox.Visible", false, form.RecipientPKGuidFindBox.Visible);
					AssertEquals("UniversalShipment form.RecipientServiceDropEdit.Visible", true, form.RecipientServiceDropEdit.Visible);
					shipmentFormOriginalHeight = form.Size.Height;
					AssertEquals("UniversalShipment form height to fit in Recipient Service Drop Edit.", shipmentFormOriginalHeight, eventFormOriginalHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(23));

					form.dataExportBO.Calc_RecipientType = MessageRecipientPartyTypeList.SpecialCodes.Other;
					AssertEquals("UniversalShipment form.RecipientTypeDropEdit.Visible", true, form.RecipientTypeDropEdit.Visible);
					AssertEquals("UniversalShipment form.RecipientPKGuidFindBox.Visible", true, form.RecipientPKGuidFindBox.Visible);
					AssertEquals("UniversalShipment form.RecipientServiceDropEdit.Visible", true, form.RecipientServiceDropEdit.Visible);
					AssertEquals("UniversalShipment form height", shipmentFormOriginalHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(44), form.Size.Height);
				}
			}
		}

		public void TestFormCaption()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalEvent))
			{
				form.Show();
				AssertEquals("form.Text", "Manually Send XML Universal Event", form.Text);
			}

			using (var form = new ManualDataExportForm(Factory, dummyBO, UniversalDataType.UniversalShipment))
			{
				form.Show();
				AssertEquals("form.Text", "Manually Send XML Universal Shipment", form.Text);
			}
		}

		#region Implementation

		string lastShownNotifications;

		protected override void SetUp()
		{
			base.SetUp();

			lastShownNotifications = null;
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var notificationsForm = form as ManualDataExportProgressForm;

				if (notificationsForm != null)
				{
					notificationsForm.Closed += GatherNotifications;
				}
			});
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
		}

		protected override Form GetFormToBashCore()
		{
			return new ManualDataExportForm(Factory, Factory.New<DummyWithWorkflow>(), UniversalDataType.UniversalEvent);
		}

		#endregion
	}
}
