using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Business.UniversalData.Testing;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ManualDataExportProgressFormSendOnShow))]
	sealed class ManualDataExportFormSendOnShowTest : ManualDataExportProgressFormBehaviourTest<ManualDataExportProgressFormSendOnShow>
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

					AssertEquals("progressForm.SendButton.Visible", false, progressForm.SendButton.Visible);
					AssertEquals("progressForm.CloseButton.Enabled", false, progressForm.CloseButton.Enabled);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					AssertMultilineASCIIEquals("notifications",
	@"Processing Dummy Business Object Default
Universal Shipment queued for sending to Organization [EDICUS].
", progressForm.NotificationsTextBox.Text);

					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);
				}
			}
		}

		public void TestApply_Saves()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, "DUCKLING DUCK"))
			{
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

				using (new DummyShipmentDataContextManagerSetUp())
				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var dummyBO = Factory.New<DummyWithWorkflowAndDataContext>();
					var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
					manualDataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
					Factory.Save();
					var didSave = false;
					Factory.Saved += (s, p) => didSave = true;

					var obj = new ManualDataExportProgressFormSendOnShow(manualDataExport);
					obj.Apply(progressForm);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);
					progressForm.SendButton.PerformClick();

					AssertEquals(true, didSave);
				}
			}
		}

		[RequiresSTA]
		public void TestApply_Saves_HandleFailure()
		{
			var failureMessage = "IT DID NOT WORK OK?!?!?!?";
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, "DUCKLING DUCK"))
			{
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

				using (new DummyShipmentDataContextManagerSetUp())
				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var dummyBO = Factory.New<DummyWithWorkflowAndDataContext>();
					var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
					manualDataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
					Factory.Save();
					bool hasExploded = false;
					Factory.Saved += (s, p) =>
					{
						if (!hasExploded)
						{
							hasExploded = true;
							throw new InvalidOperationException(failureMessage);
						}
					};

					var obj = new ManualDataExportProgressFormSendOnShow(manualDataExport);
					obj.Apply(progressForm);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);
					progressForm.SendButton.PerformClick();

					AssertContains(failureMessage, progressForm.NotificationsTextBox.Text);
					ErrorReporter.Clear();
				}
			}
		}

		[RequiresSTA]
		public void TestManualDataExport_FactoryHasDisposableService()
		{
			using (var tempCommunicationsMode = ManualDataExportTest.TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "DUCKLING DUCK"))
			{
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;

				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var dummyBO = Factory.New<DummyWithWorkflow>();
					var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
					dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Forwarder;
					dataExport.EventCode = Events.AuthorisedCode;
					Factory.Save();
					bool isDisposed = false;
					void OnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
					{
						Factory.SubscribeForDispose(new DisposableAction(() => isDisposed = true));
					}
					Factory.Saved += OnSaved;

					var obj = new ManualDataExportProgressFormSendOnShow(dataExport);
					obj.Apply(progressForm);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);
					progressForm.SendButton.PerformClick();

					AssertEquals(true, isDisposed);
					AssertEquals("No error report for not having Disposable Service", 0, ErrorReporter.TotalErrorCount);

					Factory.Saved -= OnSaved;
				}
			}
		}

		protected override ManualDataExportProgressFormSendOnShow GetNewBehaviour()
		{
			var dummyBO = Factory.New<DummyWithWorkflowAndDataContext>();
			var manualDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			manualDataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			Factory.Save();

			return new ManualDataExportProgressFormSendOnShow(manualDataExport);
		}
	}
}
