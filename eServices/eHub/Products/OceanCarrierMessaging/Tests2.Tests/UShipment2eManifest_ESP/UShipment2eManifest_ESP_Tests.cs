using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2eManifest_ESP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class UShipment2eManifest_ESP_Tests
	{
		string filePathIFTMBF = "UShipment2eManifest_ESP.TestFiles_IFTMBF.";
		string filePathIFTCPS = "UShipment2eManifest_ESP.TestFiles_IFTCPS.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2eManifest_ESP_IFTMBF()
		{
			AssertMapping(filePathIFTMBF, "Test1_input.xml", "Test1_output.xml", "IFTMBF", "SHL_CCC001", "");
			AssertMapping(filePathIFTMBF, "Test2_input.xml", "Test2_output.xml", "IFTMBF", "INTT", "XXXX");
			AssertMapping(filePathIFTMBF, "Test4_input.xml", "Test4_output.xml", "IFTMBF", "INTT", "");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2eManifest_ESP_IFTMBF_IsNewFormMessage()
		{
			AssertMapping(filePathIFTMBF, "Test5_NewForm_input.xml", "Test5_NewForm_ShortLabel_output.xml", "IFTCPS", "SHL_CCC001", "", isNewFormMessage: true);
			AssertMapping(filePathIFTMBF, "Test5_NewForm_input.xml", "Test5_NewForm_LongLabel_output.xml", "IFTCPS", "SHL_CCC001", "", useLongReference: true, isNewFormMessage: true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2eManifest_ESP_IFTCPS()
		{
			AssertMapping(filePathIFTCPS, "Test1_input.xml", "Test1_output.xml", "IFTCPS", "SHL_CCC001", "");
			AssertMapping(filePathIFTCPS, "Test2_input.xml", "Test2_output.xml", "IFTCPS", "INTT", "XXXX");
			AssertMapping(filePathIFTCPS, "Test4_input.xml", "Test4_output.xml", "IFTCPS", "INTT", "");
			AssertMapping(filePathIFTCPS, "Test5_Summary_input.xml", "Test5_Summary_output.xml", "IFTCPS", "INTT", "", "CODE", "TRUE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TesteManifest2eManifestCharCleanup()
		{
			AssertTestCharCleanup(filePathIFTMBF, "Test3Cleanup_input.xml", "Test3Cleanup_output.xml");
			AssertTestCharCleanup(filePathIFTCPS, "Test3Cleanup_input.xml", "Test3Cleanup_output.xml");
		}

		void AssertTestCharCleanup(string filePath, string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<eManifest2eManifest_CharCleanup>(input, expectedOutput);
		}

		void AssertMapping(string filePath, string inputFile, string expectedOutputFile, string messageFormat, string carrierSCAC, string handlingAgentC1CCode, string paymentLocation = "NAME", string isSummary = "FALSE", bool useLongReference = false, bool isNewFormMessage = false)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("EASIPASS_EM1");
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "EASIPASS")).Return("ESPS");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_EM1", "TESTSENDER__1", "3", "eManifest", "DocumentName")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_EM1", "TESTSENDER__1", "3", "ForwardingShipment", "ForwardingType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_EM1", "TESTSENDER__1", "3", "Shipment", "SubMessageType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_EM1", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.EASIPASS.eManifest", "@maxlength", "14")).Return("3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "EASIPASS_EM1", "SHL_CCC001")).Return("SCAC001").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "EASIPASS_EM1", "INTT")).Return("INTT").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Release Type", "Output Code", "EASIPASS_EM1", "BOL")).Return("BOL").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R1")).Return("22R1").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("45R0").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R1")).Return("45R1").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "ContainerTypeToISOCode", "EASIPASS Code", "22R0")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "ContainerTypeToISOCode", "EASIPASS Code", "22R1")).Return("22XX").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "ContainerTypeToISOCode", "EASIPASS Code", "45R0")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "ContainerTypeToISOCode", "EASIPASS Code", "45R1")).Return("45XX").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Package Type", "EASIPASS Code", "PLT")).Return("PT").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Package Type", "EASIPASS Code", "UPC")).Return("up").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Handling Agent", "Output Code", "SHL_CCC001", "")).Return("MAEK").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Handling Agent", "Output Code", "INTT", "XXXX")).Return("ZZZZ").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Handling Agent", "Output Code", "INTT", "")).Return("").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "eManifest Format", "Format", carrierSCAC, handlingAgentC1CCode)).Return(messageFormat).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Cargo Details Format (EM)", "Is Summary", carrierSCAC, handlingAgentC1CCode)).Return(isSummary).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Payment Location (EM)", "Location Value", carrierSCAC, handlingAgentC1CCode)).Return(paymentLocation).Repeat.Any();

			if (!isNewFormMessage)
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "AU", "1")).Return("").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "1")).Return("AAA").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "2")).Return("EIN").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "3")).Return("AEO").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "4")).Return("").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "1")).Return("AAA").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "2")).Return("USC").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "3")).Return("").Repeat.Any();

				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "1")).Return("").Repeat.Any();

				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "CN", "2")).Return("USC Label").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "US", "2")).Return("CNE Label").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "US", "3")).Return("NotifyParty Label").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "AU", "1")).Return("Australia Business Number").Repeat.Any();
			}
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "EASIPASS_EM1")).Return(useLongReference ? "true" : "false").Repeat.Any();

			mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", messageFormat + "_eMF_ESPS_3"));
			mockContextAccessor.Stub(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", messageFormat));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UShipment2eManifest_ESP>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}