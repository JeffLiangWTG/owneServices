using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Business.UniversalData.Testing;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ManualDataExportProgressFormSendOnButtonPress))]
	sealed class ManualDataExportProgressFormSendOnButtonPressTest : ManualDataExportProgressFormBehaviourTest<ManualDataExportProgressFormSendOnButtonPress>
	{
		[RequiresSTA]
		public override void TestApply()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, "DUCKLING DUCK"))
			{
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

				using (new DummyShipmentDataContextManagerSetUp())
				using (var progressForm = new DummyManualDataExportProgressForm())
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					GetNewBehaviour().Apply(progressForm);

					AssertEquals("progressForm.SendButton.Enabled", true, progressForm.SendButton.Enabled);
					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					AssertMultilineASCIIEquals("notifications",
	@"Processing Dummy Business Object Default
Universal Shipment queued for sending to Organization [EDICUS].
", progressForm.NotificationsTextBox.Text);

					AssertEquals("progressForm.SendButton.Enabled", false, progressForm.SendButton.Enabled);
					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);
				}
			}
		}

		protected override ManualDataExportProgressFormSendOnButtonPress GetNewBehaviour()
		{
			var dummyBO = Factory.New<DummyWithWorkflowAndDataContext>();
			var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			manualDataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			Factory.Save();

			return new ManualDataExportProgressFormSendOnButtonPress(Factory, manualDataExport);
		}
	}
}
