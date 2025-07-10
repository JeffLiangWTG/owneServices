using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.TradeSingleWindow.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Core;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;

	public class UnsolicitedMessageProcessorTest : TestCaseWithFactory
	{
		public void TestUnsolicitedResponse()
		{
			var unsolicitedDOGroup = Factory.New<GlbGroup>();
			unsolicitedDOGroup.GG_Code = "USR";
			unsolicitedDOGroup.GG_Desc = "Unsolicited Delivery Order Responses";
			var staff = unsolicitedDOGroup.Staff.AddNew();
			staff.GS_Code = "TST";
			staff.GS_FullName = "John Tester";
			staff.GS_LoginName = "JT";
			staff.GS_EmailAddress = "JohnTester@testingCompany.com";
			Factory.Save();

			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = UnsolicitedDeliveryOrderResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);

			ZString responseOutputExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for TSW Unsolicited Delivery Order: 

An unsolicited Delivery Order message has been received from New Zealand Customs.
The message is for the: Submitter.
Below are the details contained in the message:

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Entry Number    : 73218058
Message No      : 3158

Message Status  : (819) Delivery Order Herewith
                : method of Payment as specified.

Amount Returned : $502.99
Terms           : Client Deferred

Delivery Instructions
---------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)
";
			AssertEquals(responseOutputExpected, nzcMessage.EM_MessageInterpretation);

			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for TSW Unsolicited Delivery Order: ");
			AssertNotNull("Should have found the email generated from processing this message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@testingCompany.com", email.CCRecipients[0].Email);
		}

		const string UnsolicitedDeliveryOrderResponse =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20150925161034</IssueDateTime>
    <FunctionalReferenceID>3158</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_CA7C4566-8C86-4E79-B77D-7874B1B34BC3"" filename=""IM1_Delivery_Order-73218058-2015-09-25-161045713.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161034</AcceptanceDateTime>
        <FunctionalReferenceID>73884726</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>502.99</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161034</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161034</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
	}
}
