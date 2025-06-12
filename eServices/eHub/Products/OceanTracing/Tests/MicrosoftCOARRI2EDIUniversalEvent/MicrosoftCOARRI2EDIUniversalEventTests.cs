using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanTracing.Transforms.MicrosoftCOARRI2EDIUniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanTracing.Tests
{
	[TestClass]
	public class MicrosoftCOARRI2EDIUniversalEvent_Tests
	{
		const string filePath = "MicrosoftCOARRI2EDIUniversalEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMicrosoftCOARRI2EDIUniversalEvent()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "CW1Code", "46", "FLO", "At Port");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "CW1Code", "44", "FUL", "At Container Terminal");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "NZPORT", "44", "FUL", "At Container Terminal");
			AssertMapping("Test4_input.xml", "Test4_output.xml", "NZPORT", "98", "FUL", "At Container Terminal");
			AssertMapping("Test5_input.xml", "Test5_output.xml", "NZPORT", "270", "FLO", "At Port");

			AssertMapping("Test1_input.xml", "Test1_output.xml", "CW1Code", "46", "FLO", "At Port", unbSenderID: "UNB_NZPORT");
			AssertMapping("Test1_input.xml", "Test1_output_defaultInterface.xml", "CW1Code", "46", "FLO", "At Port", unbSenderID: "UNB_NZPORT", containerType: "40GP");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "CW1Code", "44", "FUL", "At Container Terminal", unbSenderID: "UNB_NZPORT");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "NZPORT", "44", "FUL", "At Container Terminal", unbSenderID: "UNB_NZPORT");
			AssertMapping("Test4_input.xml", "Test4_output.xml", "NZPORT", "98", "FUL", "At Container Terminal", unbSenderID: "UNB_NZPOE");
			AssertMapping("Test5_input.xml", "Test5_output.xml", "NZPORT", "270", "FLO", "At Port", "NZPORT");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMicrosoftCOARRI2EDIUniversalEvent_ThrowException()
		{
			var exceptionMessage = "Could not found matching PartyReceiverID in the Client Registration Lookup.(Client Registration: OCT, SenderID:[UNB2:NZPORT] RecipientID: [UNB3:PAC])";

			var input = filePath + "Test1_input.xml";

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NZPOE").Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("NZPORT").Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("PAC").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("NZPORT", "PAC", "OCT")).Return("").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("NZPOE", "PAC", "OCT")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Default Interface", "DefaultInterfaceID", "NZPORT")).Return("").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteAssertException<COARRI2EDIUniversalEvent>(input, exceptionMessage);
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string dataProvider, string eventCode, string eventType, string eventReference, string unbSenderID = "", string containerType = "")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var codeMappingPortCode = !string.IsNullOrEmpty(unbSenderID) ? unbSenderID : "NZPOE";
			var eventTypePortCode = !string.IsNullOrEmpty(dataProvider) ? dataProvider : unbSenderID;
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NZPOE").Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(dataProvider).Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("PAC").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("NZPOE", "PAC", "OCT")).Return("HYEBNEUAT").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(dataProvider, "PAC", "OCT")).Return("").Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEBNEUAT")).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Default Interface", "DefaultInterfaceID", dataProvider)).Return(unbSenderID).Repeat.Any();

			if (!string.IsNullOrEmpty(unbSenderID))
			{
				var mappedContainerType = !string.IsNullOrEmpty(containerType) ? containerType : "";
				mockCodeMapper.Expect(x => x.GetRecipientCode(unbSenderID, unbSenderID, $"{unbSenderID} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "0000")).Return(mappedContainerType).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(unbSenderID, unbSenderID, $"{unbSenderID} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "4532")).Return("").Repeat.Any();
			}
			else
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode(dataProvider, dataProvider, $"{dataProvider} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "0000")).Return("00GP").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(dataProvider, dataProvider, $"{dataProvider} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "4532")).Return("").Repeat.Any();
			}

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "0000")).Return("00GP").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "4532")).Return("45GP").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Event Type", eventTypePortCode, eventCode)).Return(eventType).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Event Parameters", eventTypePortCode, eventCode)).Return("|Facility=CY").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Event Reference", eventTypePortCode, eventCode)).Return(eventReference).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Is Estimate", eventTypePortCode, eventCode)).Return("false").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Ownership", "ediEnterprise Code", codeMappingPortCode, "")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Ownership", "ediEnterprise Code", codeMappingPortCode, "2")).Return("Carrier Owned").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Movement Type", "ediEnterprise Code", codeMappingPortCode, "2")).Return("Export").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Mode", "ediEnterprise Code", codeMappingPortCode, "")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Mode", "ediEnterprise Code", codeMappingPortCode, "3")).Return("CY/CY").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Unit of Measurement", "ediEnterprise Code", codeMappingPortCode, "KGM")).Return("KG").Repeat.Any();

			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2020-06-01T09:30:10").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<COARRI2EDIUniversalEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}
	}
}
