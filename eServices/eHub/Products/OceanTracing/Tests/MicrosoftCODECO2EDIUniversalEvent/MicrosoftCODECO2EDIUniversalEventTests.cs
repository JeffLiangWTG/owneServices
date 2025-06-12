using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanTracing.Transforms.MicrosoftCODECO2EDIUniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanTracing.Tests
{
	[TestClass]
	public class MicrosoftCODECO2EDIUniversalEvent_Tests
	{
		const string filePath = "MicrosoftCODECO2EDIUniversalEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMicrosoftCODECO2EDIUniversalEvent()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "CW1Code", "34", "GIN", "At Port");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "NZPORT", "34", "GIN", "At Port");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "NZPORT", "36", "GOU", "At Other Port");

			AssertMapping("Test1_input.xml", "Test1_output.xml", "CW1Code", "34", "GIN", "At Port", unbSenderID: "UNB_NZPORT");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "NZPORT", "34", "GIN", "At Port", unbSenderID: "UNB_NZPORT");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "NZPORT", "36", "GOU", "At Other Port", unbSenderID: "UNB_NZPORT");
			AssertMapping("Test3_input.xml", "Test3_output_defaultInterface.xml", "NZPORT", "36", "GOU", "At Other Port",  unbSenderID: "UNB_NZPORT", "40GP");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMicrosoftCODECO2EDIUniversalEvent_ThrowException()
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
			mapTester.ExecuteAssertException<CODECO2EDIUniversalEvent>(input, exceptionMessage);
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string dataProvider, string eventCode, string eventType, string eventReference, string unbSenderID = "", string containerType = "")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			var codeMappingPortCode = !string.IsNullOrEmpty(unbSenderID) ? unbSenderID : "NZPOE";
			var eventTypePortCode = !string.IsNullOrEmpty(dataProvider) ? dataProvider : unbSenderID;

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
				mockCodeMapper.Expect(x => x.GetRecipientCode(unbSenderID, unbSenderID, $"{unbSenderID} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "2210")).Return("20GP").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(unbSenderID, unbSenderID, $"{unbSenderID} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "45G0")).Return(containerType).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(unbSenderID, unbSenderID, $"{unbSenderID} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "0000")).Return(mappedContainerType).Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(unbSenderID, unbSenderID, $"{unbSenderID} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "4532")).Return(containerType).Repeat.Any();
			}
			else
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode(dataProvider, dataProvider, $"{dataProvider} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "0000")).Return("00GP").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(dataProvider, dataProvider, $"{dataProvider} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "4532")).Return("").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(dataProvider, dataProvider, $"{dataProvider} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "45G0")).Return("").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode(dataProvider, dataProvider, $"{dataProvider} System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "2210")).Return("20GP").Repeat.Any();
			}

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "0000")).Return("00GP").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "4532")).Return("4532").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "ISOCodeToContainerType", "ediEnterprise Code", "45G0")).Return("40GP").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Event Type", eventTypePortCode, eventCode)).Return(eventType).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Event Parameters", eventTypePortCode, eventCode)).Return("|Facility=CY").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Event Reference", eventTypePortCode, eventCode)).Return(eventReference).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Event Type", "Is Estimate", eventTypePortCode, eventCode)).Return("false").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Ownership", "ediEnterprise Code", codeMappingPortCode, "")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Movement Type", "ediEnterprise Code", codeMappingPortCode, "2")).Return("Export").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OCEAN_CONTAINER_TRACING", "OCEAN_CONTAINER_TRACING", "OCT System Configuration", "Container Mode", "ediEnterprise Code", codeMappingPortCode, "")).Return("").Repeat.Any();
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
			mapTester.Execute<CODECO2EDIUniversalEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}
	}
}
