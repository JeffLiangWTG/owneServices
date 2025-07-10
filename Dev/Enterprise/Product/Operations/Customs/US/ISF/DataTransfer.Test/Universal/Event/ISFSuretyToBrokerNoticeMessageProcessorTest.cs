using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Testing
{
	sealed class ISFSuretyToBrokerNoticeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSendAcknowledgementReport()
		{
			var eventXmlText = @"
<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>ISF0000002</Key>
              <Type>USImporterSecurityFiling</Type>
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
</UniversalEvent>
";
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			ISFRegistry.Instance.ImporterSecurityFilingMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());
			var header = Factory.NewWithValidTestData<CusISFHeader>();
			header.BF_JobReference = "ISF0000002";
			header.BF_CustomsReference = "XJ5-11114444458";
			Factory.Save();
			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var finder = new ISFEventParentFinder(Factory, new ISFHeaderDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusISFHeader;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "ISF0000002", relatedObj.BF_JobReference);
			var processor = new ISFSuretyToBrokerNoticeMessageProcessor(relatedObj, xmlEvent as Event);
			processor.SendAcknowledgementReport();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Body.Contains("ISF0000002"));
			AssertNotNull(email);
			var eamilBody = email.Body;
			Assert(eamilBody.Contains("ISF0000002"));
			Assert(eamilBody.Contains("You are not registered with eHub. Contact WTG to register"));
			Assert(eamilBody.Contains("eBond Message to Surety Agent"));
		}
	}
}
