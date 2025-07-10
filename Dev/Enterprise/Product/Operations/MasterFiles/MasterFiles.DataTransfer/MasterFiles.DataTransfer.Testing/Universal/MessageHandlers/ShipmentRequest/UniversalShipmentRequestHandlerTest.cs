using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ValidationRule = Enterprise.UniversalDataBuss.DataObjects.ValidationRule;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalShipmentRequestHandlerTest : TestCaseWithFactory
	{
		public void TestCreateRequestAndResponseMessage()
		{
			var handler = new UniversalShipmentRequestHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = (EDIMessage)handler.CreateRequestMessage();
			AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, request.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Receive, request.EM_ReceiveTransmit);
			AssertEquals(EDIMessageTypeList.Codes.XMS, request.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest, request.EM_MessageSubType);
			AssertEquals(EDIMessageStatusList.Codes.Recognised, request.EM_Status);

			var response = (EDIMessage)handler.CreateResponseMessage();
			AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, response.EM_ApplicationCode);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, response.EM_ReceiveTransmit);
			AssertEquals(EDIMessageTypeList.Codes.XMS, response.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalResponse, response.EM_MessageSubType);
		}

		public void TestProcess()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var xml = string.Format(CultureInfo.InvariantCulture, @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingConsol</Type>
			          <Key>C12345678</Key>
			        </DataTarget>
			      </DataTargetCollection>
			
			      <Company>
			        <Code>{0}</Code>
			        <Name>{1}</Name>
			      </Company>
			      <EnterpriseID>{2}</EnterpriseID>
			      <ServerID>{3}</ServerID>
			    </DataContext>
			  </ShipmentRequest>
			</UniversalShipmentRequest>
			", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name, registrationKey.EnterpriseCode, registrationKey.ServerCode);

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalShipmentRequestHandler(xmlSessionTracker);
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

			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol.FillWithValidTestData();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C12345678";
			consol[JobConsolSchema.JK_MasterBillNum] = "M1234";
			Factory.Save();

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using (var processingResult = handler.Process(request, responseMessageSaver))
			{
				AssertResponseMessageLinkedToParentByDexEvent(consol, (IEDIMessage)response);

				processingResult.ResponseMessageText.Position = 0;
				AssertContains(@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C12345678</Key>
        </DataSource>
      </DataSourceCollection>", new StreamReader(processingResult.ResponseMessageText).ReadToEnd());
			}
		}

		public void TestResponseMessageWithValidationRuleCollection()
		{
			var xml =
	@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key />
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			var rule1 = new ValidationRule() { Code = "R001", Sequence = 1, MessageLog = "Validate Qty greater than 9", Result = "ERROR" };
			var rule2 = new ValidationRule() { Code = "R002", Sequence = 4, MessageLog = "DG Category not permitted", Result = "WARNING" };
			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			xmlSessionTracker.ValidationRuleCollection = new List<ValidationRule> { rule1, rule2 };
			var handler = new UniversalShipmentImportHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request, responseMessageSaver))
			{
				AssertContains($@"<ValidationRuleCollection>
    <ValidationRule>
      <Code>{rule1.Code}</Code>
      <Sequence>{rule1.Sequence}</Sequence>
      <MessageLog>{rule1.MessageLog}</MessageLog>
      <Result>{rule1.Result}</Result>
    </ValidationRule>
    <ValidationRule>
      <Code>{rule2.Code}</Code>
      <Sequence>{rule2.Sequence}</Sequence>
      <MessageLog>{rule2.MessageLog}</MessageLog>
      <Result>{rule2.Result}</Result>
    </ValidationRule>
  </ValidationRuleCollection>", new StreamReader(processingResult.FullResponseMessageText).ReadToEnd());
			}
		}

		public void TestUnhandledExceptionsAreReported()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var xml = string.Format(CultureInfo.InvariantCulture, @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingConsol</Type>
			          <Key>C12345678</Key>
			        </DataTarget>
			      </DataTargetCollection>
			
			      <Company>
			        <Code>{0}</Code>
			        <Name>{1}</Name>
			      </Company>
			      <EnterpriseID>{2}</EnterpriseID>
			      <ServerID>{3}</ServerID>
			    </DataContext>
			  </ShipmentRequest>
			</UniversalShipmentRequest>
			", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name, registrationKey.EnterpriseCode, registrationKey.ServerCode);

			var handler = new UniversalShipmentRequestHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			((BusinessObject)consol).FillWithValidTestData();
			consol.JK_UniqueConsignRef = "C12345678";
			Factory.Save();

			var exception = new InvalidOperationException("boop");
			handler.ExceptionToThrow = exception;

			handler.Process(request);
			request.Save();
			AssertEquals(exception, ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestResponseMessageEncodingIsUTF8WithoutBOM()
		{
			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalShipmentRequestHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using (var processingResult = handler.Process(request, responseMessageSaver))
			{
				processingResult.FullResponseMessageText.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(processingResult.FullResponseMessageText, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
				{
					reader.Peek();
					Assert(reader.CurrentEncoding == Encoding.UTF8);
				}

				processingResult.FullResponseMessageText.Seek(0, SeekOrigin.Begin);
				var buffer = new byte[3];
				var readCount = processingResult.FullResponseMessageText.Read(buffer, 0, 3);
				var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
				AssertNotEquals(utf8Bom, buffer);
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
