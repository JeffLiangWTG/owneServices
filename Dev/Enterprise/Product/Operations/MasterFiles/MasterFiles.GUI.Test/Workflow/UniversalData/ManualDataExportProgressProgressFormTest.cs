using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Business.UniversalData.Testing;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Workflow.UniversalData.Testing
{
	[TestedType(typeof(ManualDataExportProgressForm))]
	sealed class ManualDataExportProgressProgressFormTest : ZFormBasherTest
	{
		public void TestModes()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!"))
			{
				AssertNotNull(DummyWorkflowDescriptor.Instance);

				var dummyBO = Factory.New<DummyWithWorkflow>();
				var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				manualDataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
				manualDataExport.EventCode = Events.Authorised.Code;
				Factory.Save();

				using (var form = new ManualDataExportProgressForm())
				{
					new ManualDataExportProgressFormSendOnShow(manualDataExport).Apply(form);

					var textBox = (ZTextBox)form.Controls.Find("notificationsTextBox", true)[0];
					var sendButton = (ZButton)form.Controls.Find("sendButton", true)[0];
					var closeButton = (ZButton)form.Controls.Find("closeButton", true)[0];
					form.Shown += (s, e) =>
					{
						AssertEquals(false, sendButton.Visible);
						AssertEquals(true, closeButton.Visible);
					};

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(form);

					AssertMultilineASCIIEquals("notifications",
	@"Processing Dummy Business Object Default
Universal Event queued for sending to Organization [EDICUS].
", textBox.Text);
				}

				using (var form = new ManualDataExportProgressForm())
				{
					new ManualDataExportProgressFormSendOnButtonPress(Factory, manualDataExport).Apply(form);

					var textBox = (ZTextBox)form.Controls.Find("notificationsTextBox", true)[0];
					var sendButton = (ZButton)form.Controls.Find("sendButton", true)[0];
					var closeButton = (ZButton)form.Controls.Find("closeButton", true)[0];
					form.Show();
					AssertEquals("no notifications as event hasn't been sent yet", string.Empty, textBox.Text);
					AssertEquals(true, sendButton.Visible);
					AssertEquals(true, sendButton.Enabled);
					AssertEquals(true, closeButton.Visible);
					AssertEquals(true, closeButton.Enabled);

					sendButton.PerformClick();

					AssertMultilineASCIIEquals("event sent on click",
	@"Processing Dummy Business Object Default
Universal Event queued for sending to Organization [EDICUS].
", textBox.Text);
					AssertEquals(true, closeButton.Enabled);
					AssertEquals(true, sendButton.Visible);
					AssertEquals(false, sendButton.Enabled);
					AssertEquals(true, closeButton.Visible);
					AssertEquals(true, closeButton.Enabled);
				}
			}
		}

		public void TestNotifications()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!"))
			{
				var dummyBO = Factory.New<DummyWithWorkflow>();
				var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				manualDataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
				manualDataExport.EventCode = Events.Authorised.Code;
				Factory.Save();

				using (var form = new ManualDataExportProgressForm())
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					new ManualDataExportProgressFormSendOnShow(manualDataExport).Apply(form);

					var textBox = (ZTextBox)form.Controls.Find("notificationsTextBox", true)[0];
					var closeButton = (ZButton)form.Controls.Find("closeButton", true)[0];

					AssertMultilineASCIIEquals("notifications", string.Empty, textBox.Text);
					AssertEquals(false, closeButton.Enabled);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(form);

					AssertMultilineASCIIEquals("notifications",
	@"Processing Dummy Business Object Default
Universal Event queued for sending to Organization [EDICUS].
", textBox.Text);
					AssertEquals(true, closeButton.Enabled);
				}
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			ZFormModaliser.ShowDialogsInTest = false;
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ManualDataExportProgressForm();

			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			new ManualDataExportProgressFormSendOnButtonPress(Factory, manualDataExport).Apply(form);

			return form;
		}

		#endregion
	}
}
