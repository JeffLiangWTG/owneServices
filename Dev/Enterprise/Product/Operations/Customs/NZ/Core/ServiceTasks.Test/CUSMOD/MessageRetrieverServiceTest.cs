using System;
using System.Collections.Generic;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.NZ.ServiceTasks.CUSMOD.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	sealed class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestRetrieveMessage()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "AAA";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var branch = company.Branches.AddNew();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00001234Z");
			Factory.Save();

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESCRE</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"">20130328175823</IssueDateTime>
    <FunctionalReferenceID>14</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>83563537</ID>
        <FunctionalReferenceID>X00001004</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130328175823</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_GB = branch.PK;
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000044";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var logger = new LoggerForTesting();
			var serviceTask = new MessageRetrieverService();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();

			var message = interchange.ContainedMessages[0];
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(EDIMessage.ApplicationCodes.NewZealandCustoms, message.EM_ApplicationCode);
			AssertContains("<FunctionalReferenceID>X00001004</FunctionalReferenceID>", message.EM_MessageText);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
				new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					"NZ Customs Inbound Interchanges",
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.NZCustoms),
				};
			}
		}
	}
}
