using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.Testing
{
	public class LocalCountryCustomsInterfaceIntegrationProviderTest : TestCaseWithFactory
	{
		public void TestSubmit()
		{
			SubmitEdiInterchange(EDIInterchangeTransportTypeList.Codes.eHub, EDIInterchangeStatusList.Codes.eHubQueued);
			SetUp();
			customsInterface.InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.eAdaptor;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				SubmitEdiInterchange(EDIInterchangeTransportTypeList.Codes.eAdaptor, EDIInterchangeStatusList.Codes.eAdaptorQueued);
			}
		}

		public void SubmitEdiInterchange(string transportType, string status)
		{
			var licenseKeyId = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			CombineAssertions(() =>
			{
				AssertEquals($"Execute Result for {customsInterface.InterfaceType}", "Submit Succeeded.", integrationProvider.Execute(jobDeclaration));
				AssertEquals(1, jobDeclaration.Messages.Count);
				var message = jobDeclaration.Messages[0];
				AssertNotNull(message);
				AssertEquals($"Expected EM_ApplicationCode for {customsInterface.InterfaceType}", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals($"Expected EM_Status for {customsInterface.InterfaceType}", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals($"Expected EM_ReceiveTransmit for {customsInterface.InterfaceType}", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals($"Expected EM_GB for {customsInterface.InterfaceType}", GlbBranch.CurrentBranch.PK, message.EM_GB);
				var interchange = message.Interchange;
				AssertNotNull(interchange);
				AssertEquals($"Expected EI_ApplicationCode for {customsInterface.InterfaceType}", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals($"Expected EI_InterchangeType for {customsInterface.InterfaceType}", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals($"Expected EI_From for {customsInterface.InterfaceType}", licenseKeyId, interchange.EI_From);
				AssertEquals($"Expected EI_To for {customsInterface.InterfaceType}", customsInterface.RecipientID, interchange.EI_To);
				AssertEquals($"Expected EI_Status for {customsInterface.InterfaceType}", status, interchange.EI_Status);
				AssertEquals($"Expected EI_TransportType for {customsInterface.InterfaceType}", transportType, interchange.EI_TransportType);
				AssertEquals($"Expected EI_ReceiveTransmit for {customsInterface.InterfaceType}", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertNotContains($"Expected in EI_BodyText for {customsInterface.InterfaceType}", "<?xml version=\"1.0\" encoding=\"utf-8\"?>", interchange.EI_BodyText);
				AssertContains($"Expected in EI_BodyText for {customsInterface.InterfaceType}", $"<SenderID>{licenseKeyId}</SenderID>", interchange.EI_BodyText);
				AssertContains($"Expected in EI_BodyText for {customsInterface.InterfaceType}", "<RecipientID>123</RecipientID>", interchange.EI_BodyText);
				AssertEquals($"Expected EI_GB for {customsInterface.InterfaceType}", GlbBranch.CurrentBranch.PK, interchange.EI_GB);
			});
		}

		BaseJobDeclaration jobDeclaration;
		LocalCountryCustomsInterface customsInterface;
		LocalCountryCustomsInterfaceIntegrationProvider integrationProvider;

		protected override void SetUp()
		{
			base.SetUp();
			customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			setLocalCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			jobDeclaration = Factory.New<BaseJobDeclaration>();
			integrationProvider = new LocalCountryCustomsInterfaceIntegrationProvider();
		}

		protected override void TearDown()
		{
			base.TearDown();
			setLocalCountryCustomsInterface.Dispose();
		}

		IDisposable setLocalCountryCustomsInterface;
	}
}
