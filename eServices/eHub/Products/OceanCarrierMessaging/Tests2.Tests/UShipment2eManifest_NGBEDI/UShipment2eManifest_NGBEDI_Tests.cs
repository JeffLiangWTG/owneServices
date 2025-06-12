using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2eManifest_NGBEDI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class UShipment2eManifest_NGBEDI_Tests
	{
		const string filePath = "UShipment2eManifest_NGBEDI.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UShipment2eManifest_NGBEDI()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
			AssertMapping("Test2_input.xml", "Test2_output.xml");
			AssertMapping("Test3_input.xml", "Test3_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UShipment2eManifest_NGBEDI_IsNewFormMessage()
		{
			AssertMapping("Test4_NewForm_input.xml", "Test4_NewForm_LongLabel_output.xml", true, true);
			AssertMapping("Test4_NewForm_input.xml", "Test4_NewForm_ShortLabel_output.xml", false, true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TesteManifest2eManifestCharCleanup()
		{
			var input = filePath + "Test3Cleanup_input.xml";
			var expectedOutput = filePath + "Test3Cleanup_output.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<eManifest2eManifest_CharCleanup>(input, expectedOutput);
		}

		void AssertMapping(string inputFile, string expectedOutputFile, bool useLongReference = false, bool isNewFormMessage = false)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NGBEDI_EM1");
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "NGBEDI")).Return("ESPS");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EM1", "TESTSENDER__1", "3", "eManifest", "DocumentName")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EM1", "TESTSENDER__1", "3", "ForwardingShipment", "ForwardingType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_EM1", "TESTSENDER__1", "3", "Shipment", "SubMessageType")).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.NGBEDI.eManifest", "@maxlength", "14")).Return("3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "NGBEDI_EM1", "SHP_CCC111")).Return("SCAC001").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "NGBEDI_EM1", "INTT")).Return("INTT").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Release Type", "Output Code", "NGBEDI_EM1", "BOL")).Return("BOL").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R1")).Return("22R1").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("45R0").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R1")).Return("45R1").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PLT")).Return("PLT").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "NGBEDI_EM1")).Return(useLongReference ? "true" : "false").Repeat.Any();
			if (!isNewFormMessage)
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "AU", "1")).Return("AAA").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "AU", "2")).Return("ABN").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "AU", "3")).Return("").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "1")).Return("AAA").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "2")).Return("USC").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "3")).Return("").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "1")).Return("CNE").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "2")).Return("USC").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "3")).Return("").Repeat.Any();

				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "1")).Return("").Repeat.Any();

				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "US", "1")).Return("CNE Label").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "US", "2")).Return("NotifyParty Label").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "CN", "2")).Return("USC Label").Repeat.Any();
				mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "CN", "AU", "2")).Return("Australia Business Number").Repeat.Any();
			}

			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Shipping Agent Code", "Output Code", "TESTSENDER__1", "")).Return("DHLNGB").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Shipping Agent Code", "Output Code", "TESTSENDER__1", "XXXX")).Return("ZZZZZ").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "22R0")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "22R1")).Return("22XX").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "45R0")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "45R1")).Return("45XX").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Package Type", "NGBEDI Code", "PLT")).Return("PT").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Package Type", "NGBEDI Code", "UPC")).Return("up").Repeat.Any();
			mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "EMF_ESPS_3"));

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2eManifest_NGBEDI>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}