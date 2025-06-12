using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2VERMAS_NGBEDI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class UniversalShipment2VERMAS_NGBEDITests
	{
		const string filePath = "UniversalShipment2VERMAS_NGBEDI.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2VERMAS_NGBEDI()
		{
			AssertMapping1("Test1_input.xml", "Test1_output.xml");
			AssertMapping2("Test1_output.xml", "Test1_output(Cleanup).xml");
			AssertMapping1("Test2_input.xml", "Test2_output.xml");
		}

		void AssertMapping1(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("NGBEDI_VM1");
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "A01", "NGBEDI")).Return("NGB");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_VM1", "TESTSENDER__1", "C00001106", "MAEU0494852"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.NGBEDI.VERMAS", "@maxlength", "14")).Return("3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "NGBEDI_VM1", "MAEU")).Return("AAAA");
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "ContainerTypeToISOCode", "NGBEDI Code", "22G0")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22G0")).Return("");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NGBMSG", "NGBEDI_VM1", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));
			mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "EVGM_NGB_3"));

			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Shipping Agent Code", "Output Code", "TESTSENDER__1", "")).Return("NGBEDI").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("NGBEDI", "NGBEDI", "NGBEDI Provider Configuration", "Shipping Agent Code", "Output Code", "TESTSENDER__1", "C1C1")).Return("C1C1").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>() { 
					{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
					{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
					{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
				};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalShipment2VERMAS_NGBEDI>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		void AssertMapping2(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<VERMAS_NGBEDI2VERMAS_NGBEDI_CharCleanup>(input, expectedOutput);
		}
	}
}
