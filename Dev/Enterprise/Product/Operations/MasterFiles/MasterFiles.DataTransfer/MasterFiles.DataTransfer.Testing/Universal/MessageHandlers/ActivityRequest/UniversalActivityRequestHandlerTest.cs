using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class ActivityRequestHandlerTest : TestCaseWithFactory
	{
		public void TestCreateRequestAndResponseMessage()
		{
			var handler = new UniversalActivityRequestHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = (EDIMessage)handler.CreateRequestMessage();
			AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, request.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Receive, request.EM_ReceiveTransmit);
			AssertEquals(EDIMessageTypeList.Codes.XMS, request.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest, request.EM_MessageSubType);
			AssertEquals(EDIMessageStatusList.Codes.Recognised, request.EM_Status);

			var response = (EDIMessage)handler.CreateResponseMessage();
			AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, response.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, response.EM_ReceiveTransmit);
			AssertEquals(EDIMessageTypeList.Codes.XMS, response.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalResponse, response.EM_MessageSubType);
			AssertEquals(EDIMessageStatusList.Codes.Sent, response.EM_Status);
		}

		public void TestProcess()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var xml = FormattableString.Invariant($@"<UniversalActivityRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
				  <ActivityRequest>
				    <DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>WorkItem</Type>
				          <Key>WI12345678</Key>
				        </DataTarget>
				      </DataTargetCollection>
			
				      <Company>
				        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				        <Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
				      </Company>
				      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
				      <ServerID>{registrationKey.ServerCode}</ServerID>
				    </DataContext>
				  </ActivityRequest>
				</UniversalActivityRequest>");

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalActivityRequestHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using (var processingResult = handler.Process(request))
			{
				AssertNotNull(processingResult);
				AssertEquals("ERR", processingResult.Status);
				AssertContains("There is no business object matching the criteria.", xmlSessionTracker.ToString());
			}

			var workItem = (BusinessObject)Factory.New<IWorkItem>();
			workItem.FillWithValidTestData();
			workItem[WorkItemSchema.WKI_WorkItemNumber] = "WI12345678";
			workItem[WorkItemSchema.WKI_Summary] = "And that's the way the news goes.";

			Factory.Save();

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using (var processingResult = handler.Process(request, responseMessageSaver))
			{
				AssertResponseMessageLinkedToParentByDexEvent(workItem, (IEDIMessage)response);

				processingResult.ResponseMessageText.Position = 0;
				AssertContains(@"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WorkItem</Type>
          <Key>WI12345678</Key>
        </DataSource>
      </DataSourceCollection>", new StreamReader(processingResult.ResponseMessageText).ReadToEnd());
			}
		}

		void AssertResponseMessageLinkedToParentByDexEvent(BusinessObject parent, IEDIMessage responseMessage)
		{
			var logParent = new BusinessObjectFactory().Load(parent.GetType(), parent.PK) as IStmALogParent;
			var exportLog = logParent.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNotNull("Expecting a DEX event", exportLog);
			AssertEquals("DEX event should be linked to response message", responseMessage.PK, exportLog.RelatedEDIMessage.Message.PK);
		}
	}
}
