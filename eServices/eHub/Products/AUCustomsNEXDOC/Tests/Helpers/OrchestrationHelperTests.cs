using System;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.AUCustomsNEXDOC.Helpers;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Xml;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Tests.Helpers
{
	[TestClass]
	public class OrchestrationHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetServiceType()
		{
			Assert.AreEqual("rexsubmission", OrchestrationHelper.GetServiceType("OrderRex"));
			Assert.AreEqual("rexsubmission", OrchestrationHelper.GetServiceType("LodgeRex"));
			Assert.AreEqual("rexsubmission", OrchestrationHelper.GetServiceType("AmendRex"));
			Assert.AreEqual("rexsubmission", OrchestrationHelper.GetServiceType("WithdrawalRex"));
			Assert.AreEqual("rexsubmission", OrchestrationHelper.GetServiceType("CancelRex"));
			Assert.AreEqual("modifyCertificate", OrchestrationHelper.GetServiceType("ReissueCertificate"));
			Assert.AreEqual("modifyCertificate", OrchestrationHelper.GetServiceType("ReplaceCertificate"));
			Assert.AreEqual("readCertificate", OrchestrationHelper.GetServiceType("ReadCertificate"));
			Assert.AreEqual("customs", OrchestrationHelper.GetServiceType("TransferRexEDN"));
			Assert.AreEqual("customs", OrchestrationHelper.GetServiceType("CancelRexEDN"));
			Assert.AreEqual("readrex", OrchestrationHelper.GetServiceType("ReadRex"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetRecipient()
		{
			var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
			SetupTestData(mockeHubTransactionsContext);
			var subscription = OrchestrationHelper.GetSubscription("Certificate Notification: REX11111111", "NEXDOCS", "NEXDOCS",
				new NoOpLogger());
			var subscription2 = OrchestrationHelper.GetSubscription("Certificate Notification: REX2222222 (Exporter Reference: LBG20190304)", "NEXDOCSTest", "NEXDOCSTest",
				new NoOpLogger());
			Assert.AreEqual("282dcc66-a19c-4cb2-a9e5-be5f85b8d002", subscription.Recipient);
			Assert.AreEqual("e2d61c71-183f-4740-86b3-ba571d7316fb", subscription.Sender);
			Assert.AreEqual("Job1", subscription.Reference);
			Assert.AreEqual("REX11111111", subscription.Value);
			Assert.AreEqual("282dcc66-a19c-4cb2-a9e5-be5f85b8d002", subscription2.Recipient);
			Assert.AreEqual("55f4237b-7949-4164-9583-490c32c46caf", subscription2.Sender);
			Assert.AreEqual("Job2", subscription2.Reference);
			Assert.AreEqual("REX2222222", subscription2.Value);
			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUnsolicitedNotification()
		{
			var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
			SetupTestData(mockeHubTransactionsContext);
			var subscription = OrchestrationHelper.GetSubscription("Certificate Notification: REX00000000", "NEXDOCS", "NEXDOCS",
				new NoOpLogger());
			Assert.AreEqual("e2d61c71-183f-4740-86b3-ba571d7316fb", subscription.Recipient);
			Assert.AreEqual("e2d61c71-183f-4740-86b3-ba571d7316fb", subscription.Sender);
			Assert.AreEqual("Unsolicited Notification", subscription.Reference);
			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestGetREXNumber()
		{
			var testStrings = SetupTestStrings();
			Assert.AreEqual("REX2222222", OrchestrationHelper.GetRexNumber(testStrings["titleValidWithRexAndJob"]));
			Assert.AreEqual("REX11111111", OrchestrationHelper.GetRexNumber(testStrings["titleValidWithRex"]));
			Assert.AreEqual("", OrchestrationHelper.GetRexNumber(testStrings["titleInvalid"]));
		}

		[TestMethod]
		public void TestGetJobNumber()
		{
			var testStrings = SetupTestStrings();
			Assert.AreEqual("WPR20190304", OrchestrationHelper.GetJobNumber(testStrings["titleValidWithRexAndJob"]));
			Assert.AreEqual("", OrchestrationHelper.GetJobNumber(testStrings["titleValidWithRex"]));
			Assert.AreEqual("", OrchestrationHelper.GetJobNumber(testStrings["titleInvalid"]));
		}

		[TestMethod]
		public void TestGetEDNNumber()
		{
			var testStrings = SetupTestStrings();
			Assert.AreEqual("AAAAAAAKY", OrchestrationHelper.GetEDNNumber(testStrings["textValidInRightSequence"]));
			Assert.AreEqual("LBG", OrchestrationHelper.GetEDNNumber(testStrings["textValidInWrongSequence"]));
			Assert.AreEqual("", OrchestrationHelper.GetEDNNumber(testStrings["textInvalidNoEDN"]));
			Assert.AreEqual("", OrchestrationHelper.GetEDNNumber(testStrings["blank"]));
		}

		[TestMethod]
		public void TestExtractConsigneeInformation()
		{
			#region Input XML
			var inputXML = @"<soapenv:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <S:Header xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
    <!-- The header content was removed for this sample  -->
  </S:Header>
  <S:Body xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
    <ns1:ReadCertificateResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadCertificateSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
      <ns1:superCertificates>
        <ns1:consignee>
          <ns0:consigneeName>ACE TEX PLASTIC SDN BHD</ns0:consigneeName>
          <ns0:consigneeAddress>
            <ns0:streetAddress>
              <ns0:streetLine>45 JALAN KAPAR</ns0:streetLine>
              <ns0:streetLine>WONDERBOOM</ns0:streetLine>
            </ns0:streetAddress>
            <ns0:city>KLANG</ns0:city> 
            <ns0:state>10</ns0:state>
            <ns0:country>MY</ns0:country>
            <ns0:postalCode>41400</ns0:postalCode>
          </ns0:consigneeAddress>
        </ns1:consignee>
        <ns1:departureDate>2021-05-24</ns1:departureDate>
        <ns1:certificates>
          <ns1:certificate>
            <ns1:certificatePayloads>
              <ns1:certificatePayload outputType=""PAPER"" copyType=""PREVIEW"" paperType=""UNSECURE"">JVBERi0xL...0YK</ns1:certificatePayload>
            </ns1:certificatePayloads>
          </ns1:certificate>
        </ns1:certificates>
      </ns1:superCertificates>
    </ns1:ReadCertificateResponse>
  </S:Body>
</soapenv:Envelope>
";
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(inputXML);
			#endregion
			#region Expected Output
			var expectedOutput = @"
            <Context>
                <Type>consigneeName</Type>
                <Value>ACE TEX PLASTIC SDN BHD</Value>
            </Context>
            <Context>
                <Type>consigneeStreetAddress1</Type>
                <Value>45 JALAN KAPAR</Value>
            </Context>
            <Context>
                <Type>consigneeStreetAddress2</Type>
                <Value>WONDERBOOM</Value>
            </Context>
            <Context>
                <Type>consigneeCity</Type>
                <Value>KLANG</Value>
            </Context>
            <Context>
                <Type>consigneeState</Type>
                <Value>10</Value>
            </Context>
            <Context>
                <Type>consigneeCountry</Type>
                <Value>MY</Value>
            </Context>
            <Context>
                <Type>consigneePostalCode</Type>
                <Value>41400</Value>
            </Context>
            <Context>
                <Type>consigneePhoneNumber</Type>
                <Value></Value>
            </Context>
            <Context>
                <Type>consigneeRepresentative</Type>
                <Value></Value>
            </Context>
            <Context>
                <Type>departureDate</Type>
                <Value>2021-05-24</Value>
            </Context>";
			#endregion
			Assert.AreEqual(expectedOutput, OrchestrationHelper.ExtractConsigneeInformation(xmlDoc));
		}

		[TestMethod]
		public void TestExtractCertificatePayload()
		{
			#region Input XML
			var inputXML_Preview = @"<soapenv:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <S:Header xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
    <!-- The header content was removed for this sample  -->
  </S:Header>
  <S:Body xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
    <ns1:ReadCertificateResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadCertificateSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
      <ns1:superCertificates>
        <ns1:consignee>
          <ns0:consigneeName>ACE TEX PLASTIC SDN BHD</ns0:consigneeName>
          <ns0:consigneeAddress>
            <ns0:streetAddress>
              <ns0:streetLine>45 JALAN KAPAR</ns0:streetLine>
              <ns0:streetLine>WONDERBOOM</ns0:streetLine>
            </ns0:streetAddress>
            <ns0:city>KLANG</ns0:city> 
            <ns0:state>10</ns0:state>
            <ns0:country>MY</ns0:country>
            <ns0:postalCode>41400</ns0:postalCode>
          </ns0:consigneeAddress>
        </ns1:consignee>
        <ns1:departureDate>2021-05-24</ns1:departureDate>
        <ns1:certificates>
          <ns1:certificate>
            <ns1:certificatePayloads>
              <ns1:certificatePayload outputType=""PAPER"" copyType=""PREVIEW"" paperType=""UNSECURE"">JVBERi0xL...0YK</ns1:certificatePayload>
            </ns1:certificatePayloads>
          </ns1:certificate>
          <ns1:certificate>
            <ns1:certificatePayloads>
              <ns1:certificatePayload outputType=""PAPER"" copyType=""PREVIEW"" paperType=""UNSECURE"">Blah...0YK</ns1:certificatePayload>
            </ns1:certificatePayloads>
          </ns1:certificate>
        </ns1:certificates>
      </ns1:superCertificates>
    </ns1:ReadCertificateResponse>
  </S:Body>
</soapenv:Envelope>
";
			XmlDocument xmlDoc_Preview = new XmlDocument();
			xmlDoc_Preview.LoadXml(inputXML_Preview);

			var inputXML_Original = @"<soapenv:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <S:Header xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
    <!-- The header content was removed for this sample  -->
  </S:Header>
  <S:Body xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
    <ns1:ReadCertificateResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadCertificateSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
      <ns1:superCertificates>
        <ns1:consignee>
          <ns0:consigneeName>ACE TEX PLASTIC SDN BHD</ns0:consigneeName>
          <ns0:consigneeAddress>
            <ns0:streetAddress>
              <ns0:streetLine>45 JALAN KAPAR</ns0:streetLine>
              <ns0:streetLine>WONDERBOOM</ns0:streetLine>
            </ns0:streetAddress>
            <ns0:city>KLANG</ns0:city> 
            <ns0:state>10</ns0:state>
            <ns0:country>MY</ns0:country>
            <ns0:postalCode>41400</ns0:postalCode>
          </ns0:consigneeAddress>
        </ns1:consignee>
        <ns1:departureDate>2021-05-24</ns1:departureDate>
        <ns1:certificates>
          <ns1:certificate>
            <ns1:certificateNumber>certNumber1</ns1:certificateNumber>
            <ns1:certificatePayloads>
              <ns1:certificatePayload outputType=""PAPER"" copyType=""ORIGINAL"" paperType=""UNSECURE"">JVVERi0xL...0YK</ns1:certificatePayload>
            </ns1:certificatePayloads>
          </ns1:certificate>
          <ns1:certificate>
            <ns1:certificateNumber>certNumber2</ns1:certificateNumber>
            <ns1:certificatePayloads>
              <ns1:certificatePayload outputType=""PAPER"" copyType=""ORIGINAL"" paperType=""UNSECURE"">Blah...0YK</ns1:certificatePayload>
            </ns1:certificatePayloads>
          </ns1:certificate>
        </ns1:certificates>
      </ns1:superCertificates>
    </ns1:ReadCertificateResponse>
  </S:Body>
</soapenv:Envelope>
";
			XmlDocument xmlDoc_Original = new XmlDocument();
			xmlDoc_Original.LoadXml(inputXML_Original);
			#endregion
			#region Expected Output
			var expectedOutput_Preview = @"
                <AttachedDocument>
                    <FileName>REX001_1.pdf</FileName>
                    <Type>
                        <Code>QPP</Code>
                        <Description>Quarantine Print Preview</Description>
                    </Type>
                    <ImageData>JVBERi0xL...0YK</ImageData>
                    <IsPublished>false</IsPublished>
                </AttachedDocument>
                <AttachedDocument>
                    <FileName>REX001_2.pdf</FileName>
                    <Type>
                        <Code>QPP</Code>
                        <Description>Quarantine Print Preview</Description>
                    </Type>
                    <ImageData>Blah...0YK</ImageData>
                    <IsPublished>false</IsPublished>
                </AttachedDocument>";

			var expectedOutput_Original = @"
                <AttachedDocument>
                    <FileName>certNumber1.pdf</FileName>
                    <Type>
                        <Code>QRP</Code>
                        <Description>Quarantine Remote Print</Description>
                    </Type>
                    <ImageData>JVVERi0xL...0YK</ImageData>
                    <IsPublished>true</IsPublished>
                </AttachedDocument>
                <AttachedDocument>
                    <FileName>certNumber2.pdf</FileName>
                    <Type>
                        <Code>QRP</Code>
                        <Description>Quarantine Remote Print</Description>
                    </Type>
                    <ImageData>Blah...0YK</ImageData>
                    <IsPublished>true</IsPublished>
                </AttachedDocument>";
			#endregion
			Assert.AreEqual(expectedOutput_Preview, OrchestrationHelper.ExtractCertificatePayload(xmlDoc_Preview, "PREVIEW", rexNumber: "REX001"));
			Assert.AreEqual(expectedOutput_Original, OrchestrationHelper.ExtractCertificatePayload(xmlDoc_Original, "ORIGINAL"));
		}

		[TestMethod]
		public void TestExtractXmlValue()
		{
			#region Input XML
			var inputXML = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"" xmlns:ns2=""http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0"">
	<soapenv:Header>
		<nexauth:authTokens xmlns:nexauth=""http://agriculture.gov.au/header/auth"">
			<vendorToken>70909cd0ec7e4adea332f45bbb793b91</vendorToken>
			<clientGroupToken>2D363634353139333033383736303730</clientGroupToken>
			<clientToken>289e391a9a504f63a7d4c43dc2552c9f</clientToken>
		</nexauth:authTokens>
		<wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">
			<wsse:UsernameToken>
				<wsse:Username>f6a33212f43e41d79ba21e05d907d8f0</wsse:Username>
				<wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">Password1!</wsse:Password>
			</wsse:UsernameToken>
		</wsse:Security>
	</soapenv:Header>
	<soapenv:Body>
		<ReadCertificate xmlns=""http://agriculture.gov.au/nexdoc/ReadCertificateSoap_1.0"">
			<identification>
				<ns1:rexNumber>REX6000004587</ns1:rexNumber>
			</identification>
		</ReadCertificate>
	</soapenv:Body>
</soapenv:Envelope>
";
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(inputXML);
			#endregion
			Assert.AreEqual("REX6000004587", OrchestrationHelper.ExtractXmlValue(xmlDoc, "//*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='ReadCertificate']/*[local-name()='identification']/*[local-name()='rexNumber']"));
		}

		[TestMethod]
		public void TestGetClientToken_ReturnOnlyValidToken()
		{
			var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
			OrchestrationHelper.GetContext = () => mockeHubTransactionsContext;
			var registrationType = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG002", RT_Description = "Registration 2" };
			var clientSystem = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var clientSystemRegistration1 = new eHubClientSystemRegistration { CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClientSystem = clientSystem, eHubRegistrationType = registrationType, CD_Code = "REG001", CD_Flag1 = 0 };
			var clientSystemRegistration2 = new eHubClientSystemRegistration { CD_PK = new Guid("{10000000-FFFF-1111-1111-000000000000}"), eHubClientSystem = clientSystem, eHubRegistrationType = registrationType, CD_Code = "REG002", CD_Flag1 = 1 };
			var clientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>() { clientSystemRegistration1, clientSystemRegistration2 };
			mockeHubTransactionsContext.Stub(x => x.eHubClientSystemRegistrations).Return(clientSystemRegistrations);

			var clientToken = OrchestrationHelper.GetClientToken("CLIJEY001", "REG002", new NoOpLogger());

			Assert.AreEqual("REG002", clientToken);
		}

		[TestMethod]
		public void TestInvalidateClientToken_UpdateTokenToInvalid()
		{
			var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
			OrchestrationHelper.GetContext = () => mockeHubTransactionsContext;
 			var clientSystemRegistration1 = new eHubClientSystemRegistration() {CD_Code = "REG001", CD_Flag1 = 1};
			var clientSystemRegistration2 = new eHubClientSystemRegistration() { CD_Code = "REG002", CD_Flag1 = 1 };
			var clientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>() { clientSystemRegistration1, clientSystemRegistration2};
			mockeHubTransactionsContext.Stub(x => x.eHubClientSystemRegistrations).Return(clientSystemRegistrations);
			mockeHubTransactionsContext.Expect(x => x.SaveChanges()).Return(1).Repeat.Once();

			OrchestrationHelper.InvalidateClientToken("REG001", new NoOpLogger());

			Assert.AreEqual((byte)0, clientSystemRegistration1.CD_Flag1);
			Assert.AreEqual((byte)1, clientSystemRegistration2.CD_Flag1);
		}

		[TestMethod]
		public void TestCheckExceptionForContent()
		{
			var innerException = new Exception("<message>Client token provided is not valid.</message>");
			var outerException = new Exception("An error occurred while processing the message, refer to the details section for more information", innerException);

			var result = OrchestrationHelper.CheckExceptionForContent(outerException, "Client token provided is not valid.");

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void TestShouldFilterOutError_ReturnsTrue_WhenPatternExists()
		{
			string errorMessage = "System.ServiceModel.CommunicationException: An error (The request was aborted: The request was canceled.) occurred while transmitting data over the HTTP channel";

			bool result = OrchestrationHelper.ShouldFilterOutError(errorMessage);

			Assert.IsTrue(result, "The error message should match the pattern and return true.");
		}

		[TestMethod]
		public void TestShouldFilterOutError_ReturnsFalse_WhenPatternDoesNotExist()
		{
			string errorMessage = "error message that does not match the pattern.";

			bool result = OrchestrationHelper.ShouldFilterOutError(errorMessage);

			Assert.IsFalse(result, "The error message does not match any patterns, so it should return false.");
		}

		[TestMethod]
		public void TestShouldFilterOutError_ReturnsFalse_WhenErrorMessageIsEmpty()
		{
			string errorMessage = string.Empty;

			bool result = OrchestrationHelper.ShouldFilterOutError(errorMessage);

			Assert.IsFalse(result, "An empty error message should return false.");
		}

		[TestMethod]
		public void TestShouldFilterOutError_ReturnsFalse_WhenErrorMessageIsNull()
		{
			string errorMessage = null;

			bool result = OrchestrationHelper.ShouldFilterOutError(errorMessage);

			Assert.IsFalse(result, "A null error message should return false.");
		}

		private static void SetupTestData(eHubTransactionsContext mockeHubTransactionsContext)
		{

			OrchestrationHelper.GetContext = () => mockeHubTransactionsContext;
			var customs = new eHubClient() { CC_PK = Guid.Parse("e2d61c71-183f-4740-86b3-ba571d7316fb"), CC_ID = "NEXDOCS", CC_FriendlyName = "AU Customs NEXDOCS Production" };
			var transformationSet = new eHubTransformationSet() { TS_Name = "NEXDOCS Credentials", eHubClient_Recipient = customs };
			var codeSet = new eHubCodeSet() { CS_Name = "Credentials", eHubClient_Sender = customs, eHubClient_Recipient = customs, eHubTransformationSet = transformationSet, CS_Key1Name = "Type" };
			var codeSetResult = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Token", CR_Order = 1 };
			var codeSetResult2 = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Password", CR_Order = 2 };
			var codeMapKey = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_Order = 1, CK_Key1Value = "Installation" };
			var codeMapKe2 = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_Order = 2, CK_Key1Value = "Vendor" };
			var codeMapValue = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey, eHubCodeSetResult = codeSetResult, CV_OutputCode = "111" };
			var codeMapValue2 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey, eHubCodeSetResult = codeSetResult2, CV_OutputCode = "222" };
			var codeMapValue3 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKe2, eHubCodeSetResult = codeSetResult, CV_OutputCode = "333" };

			var customsTest = new eHubClient() { CC_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CC_ID = "NEXDOCSTest", CC_FriendlyName = "AU Customs NEXDOCS Test" };
			var transformationSetTest = new eHubTransformationSet() { TS_Name = "NEXDOCS Credentials", eHubClient_Recipient = customsTest };
			var codeSetTest = new eHubCodeSet() { CS_Name = "Credentials", eHubClient_Sender = customsTest, eHubClient_Recipient = customsTest, eHubTransformationSet = transformationSetTest, CS_Key1Name = "Type" };
			var codeSetResultTest = new eHubCodeSetResult() { eHubCodeSet = codeSetTest, CR_Name = "Token", CR_Order = 1 };
			var codeSetResult2Test = new eHubCodeSetResult() { eHubCodeSet = codeSetTest, CR_Name = "Password", CR_Order = 2 };
			var codeMapKeyTest = new eHubCodeMapKey() { eHubCodeSet = codeSetTest, CK_Order = 1, CK_Key1Value = "Installation" };
			var codeMapKe2Test = new eHubCodeMapKey() { eHubCodeSet = codeSetTest, CK_Order = 2, CK_Key1Value = "Vendor" };
			var codeMapValueTest = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKeyTest, eHubCodeSetResult = codeSetResultTest, CV_OutputCode = "444" };
			var codeMapValue2Test = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKeyTest, eHubCodeSetResult = codeSetResult2Test, CV_OutputCode = "555" };
			var codeMapValue3Test = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKe2Test, eHubCodeSetResult = codeSetResultTest, CV_OutputCode = "666" };

			var client = new eHubClient() { CC_PK = System.Guid.Parse("282dcc66-a19c-4cb2-a9e5-be5f85b8d002"), CC_ID = "AAABBBCCC", CC_FriendlyName = "Test Clinet" };
			var subscriptionType = new eHubSubscriptionType() { ST_PK = new Guid(), ST_ID = "NEXDOC", ST_Name = "AU Customs NEXDOC" };
			var subscriptionValue = new eHubSubscriptionValue() { SV_PK = new Guid(), eHubSubscriptionType = subscriptionType, eHubClient_Provider = customs, eHubClient_Subscriber = client, SV_Value = "REX11111111", SV_Reference = "Job1", SV_ReferenceType = "CustomsDeclaration" };
			var subscriptionValue2 = new eHubSubscriptionValue() { SV_PK = new Guid(), eHubSubscriptionType = subscriptionType, eHubClient_Provider = customsTest, eHubClient_Subscriber = client, SV_Value = "REX2222222", SV_Reference = "Job2", SV_ReferenceType = "CustomsDeclaration" };

			var ehubClients = new TestDbSet<eHubClient>() { client, customs, customsTest };
			var transfromationSets = new TestDbSet<eHubTransformationSet>() { transformationSet, transformationSetTest };
			var codeSets = new TestDbSet<eHubCodeSet>() { codeSet, codeSetTest };
			var setResults = new TestDbSet<eHubCodeSetResult>() { codeSetResult, codeSetResult2, codeSetResultTest, codeSetResult2Test };
			var codeMapKeys = new TestDbSet<eHubCodeMapKey>() { codeMapKey, codeMapKeyTest, codeMapKe2Test, codeMapKe2 };
			var codeMapValues = new TestDbSet<eHubCodeMapValue>() { codeMapValue, codeMapValueTest, codeMapValue2, codeMapValue3, codeMapValue2Test, codeMapValue3Test };
			var subscriptionTypes = new TestDbSet<eHubSubscriptionType>() { subscriptionType };
			var subscriptionValues = new TestDbSet<eHubSubscriptionValue>() { subscriptionValue, subscriptionValue2 };


			mockeHubTransactionsContext.Stub(x => x.eHubClients).Return(ehubClients);
			mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets).Return(transfromationSets);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeSets).Return(codeSets);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeSetResults).Return(setResults);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeMapKeys).Return(codeMapKeys);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeMapValues).Return(codeMapValues);
			mockeHubTransactionsContext.Stub(x => x.eHubSubscriptionTypes).Return(subscriptionTypes);
			mockeHubTransactionsContext.Stub(x => x.eHubSubscriptionValues).Return(subscriptionValues);
		}

		private static Dictionary<string, string> SetupTestStrings()
		{
			var testStrings = new Dictionary<string, string>();
			testStrings.Add("titleValidWithRexAndJob", "Certificate Notification: REX2222222 (Exporter Reference: WPR20190304)");
			testStrings.Add("titleValidWithRex", "Certificate Notification: REX11111111");
			testStrings.Add("titleInvalid", "Random Rubbish");
			testStrings.Add("textValidInRightSequence", @">&lt;p&gt;Response from ICS for REX0000026468 (Exporter Reference: B60002138).&lt;/p&gt;
&lt;p&gt;EDN: AAAAAAAKY&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;No warning messages.&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;No error messages.&lt;/p&gt;");
			testStrings.Add("textValidInWrongSequence", @">&lt;p&gt;Response from ICS for REX0000026468 (Exporter Reference: B60002138).&lt;/p&gt;
&lt;p&gt;CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;EDN: LBG&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;No warning messages.&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;No error messages.&lt;/p&gt;");
			testStrings.Add("textInvalidNoEDN", @">&lt;p&gt;Response from ICS for REX0000026468 (Exporter Reference: B60002138).&lt;/p&gt;
&lt;p&gt;CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;No warning messages.&lt;/p&gt;
&lt;p/&gt;
&lt;p&gt;No error messages.&lt;/p&gt;");
			testStrings.Add("blank", "");
			return testStrings;
		}

	}
}
