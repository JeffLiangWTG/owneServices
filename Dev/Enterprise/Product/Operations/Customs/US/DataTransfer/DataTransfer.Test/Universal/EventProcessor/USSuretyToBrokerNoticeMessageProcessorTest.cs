using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class USSuretyToBrokerNoticeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetLogParentsForEventUsingContext()
		{
			CreateJobDeclarationForSendMessage();
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());
			Factory.Save();
			const string incomingEvent = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>B00158390</Key>
              <Type>CustomsDeclaration</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2014-09-09T09:30:10</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <Reason>You are not registered with eHub. Contact WTG to register.</Reason>
          <MessageType>eBond Message to Surety Agent</MessageType>
        </EventParameters>
      </Event>
</UniversalEvent>";
			var logger = new TestErrorLogger();
			var subscriber = new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as JobDeclaration;
			AssertNotNull(relatedObj);
			AssertEquals("B00158390", relatedObj.JE_DeclarationReference);
			Assert(SuretyToBrokerNoticeMessageProcessorHelper.IsSuretyToBrokerNoticeMessage(xmlEvent as Event));
			var processor = new USSuretyToBrokerNoticeMessageProcessor(xmlEvent as Event, relatedObj);
			processor.SendAcknowledgementReport();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Body.Contains("B00158390"));
			AssertNotNull(email);
			var eamilBody = email.Body;
			Assert(eamilBody.Contains("B00158390 / XJ5-C123457-8"));
			Assert(eamilBody.Contains("You are not registered with eHub. Contact WTG to register"));
			Assert(eamilBody.Contains("eBond Message to Surety Agent"));
		}

		JobDeclaration CreateJobDeclarationForSendMessage()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_DeclarationReference = "B00158390";
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";
			Factory.Save();
			return declaration;
		}
	}
}
