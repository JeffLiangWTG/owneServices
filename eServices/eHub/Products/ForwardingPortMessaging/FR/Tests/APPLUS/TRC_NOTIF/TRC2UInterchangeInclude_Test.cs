using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
	[TestClass]
	public class TRC2UInterchangeInclude_Test
	{
		const string filePath = "APPLUS.TRC_NOTIF.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTRC2UInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test_output_Empty.xml", "MGI", "REF_TRACING_01", "TRC - Export", "CW1", deliveryMode:"0");
			AssertMapping("Test2_input.xml", "Test_output_Empty.xml", "SOGET", "REF_TRACING_02", "TRC - Import", "CW1", deliveryMode: "0");
			AssertMapping("Test1_input.xml", "Test1_output_ContainerTracking.xml", "MGI", "REF_TRACING_01", "TRC - Export", "CONTAINER_TRACKING", deliveryMode: "0");
			AssertMapping("Test2_input.xml", "Test2_output_ContainerTracking.xml", "SOGET", "REF_TRACING_02", "TRC - Import", "CONTAINER_TRACKING", deliveryMode: "0");

			AssertMapping("Test1_input.xml", "Test1_output.xml", "MGI", "REF_TRACING_01", "TRC - Export", "CW1", deliveryMode: "1");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "SOGET", "REF_TRACING_02", "TRC - Import", "CW1", deliveryMode: "1");
			AssertMapping("Test1_input.xml", "Test_output_Empty.xml", "MGI", "REF_TRACING_01", "TRC - Export", "CONTAINER_TRACKING", deliveryMode: "1");
			AssertMapping("Test2_input.xml", "Test_output_Empty.xml", "SOGET", "REF_TRACING_02", "TRC - Import", "CONTAINER_TRACKING", deliveryMode: "1");

			AssertMapping("Test1_input.xml", "Test1_output.xml", "MGI", "REF_TRACING_01", "TRC - Export", "CW1", deliveryMode: "2");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "SOGET", "REF_TRACING_02", "TRC - Import", "CW1", deliveryMode: "2");
			AssertMapping("Test1_input.xml", "Test1_output_ContainerTracking.xml", "MGI", "REF_TRACING_01", "TRC - Export", "CONTAINER_TRACKING", deliveryMode: "2");
			AssertMapping("Test2_input.xml", "Test2_output_ContainerTracking.xml", "SOGET", "REF_TRACING_02", "TRC - Import", "CONTAINER_TRACKING", deliveryMode: "2");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string messageRef, string expectedDocumentName, string destinationParty, string deliveryMode)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.AtLeastOnce();

			var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
			var interfaceName = serviceProvider + " System Configuration";

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID).Repeat.Any();

			var recipientID = "CW1";
			if (destinationParty == "CONTAINER_TRACKING")
			{
				recipientID = "B52FR1PRO";
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@value", messageRef, "@ST_ID", serviceProviderMSGID)).Return(recipientID).Repeat.Any();
			}
			
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "JobNumber")).Return("C00100099_1_2").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "OperationPort")).Return("FRFRX").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "DocumentName")).Return(expectedDocumentName).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "WayBillNumber")).Return("OOLU2704933160").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "BookingConfirmationReference")).Return("2704933160").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", messageRef, "@referenceType", "Carrier")).Return("OOLU").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "TRC Event Delivery Method", "Method", recipientID)).Return(deliveryMode).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Event Type", "Vu à bord", "Export")).Return("FLU").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Event Type", "Vu à bord", "Import")).Return("ARV").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Event Reference", "Vu à bord", "Export")).Return("|TYP=Export").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Event Reference", "Vu à bord", "Import")).Return("|TYP=Import").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Is Estimate", "Vu à bord", "Export")).Return("false").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Is Estimate", "Vu à bord", "Import")).Return("true").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Event Parameters", "Vu à bord", "Export")).Return("|Department=Terminal|Type=Booking is confirmed and container has been allocated").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "Tracing Event Type", "Event Parameters", "Vu à bord", "Import")).Return("|Department=Terminal|Type=Booking is confirmed and container has been allocated").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, interfaceName, "ContainerTypeToISOCode", serviceProvider + " Code", "2200")).Return("22G0").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<TRC2UInterchangeInclude>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}
	}
}
