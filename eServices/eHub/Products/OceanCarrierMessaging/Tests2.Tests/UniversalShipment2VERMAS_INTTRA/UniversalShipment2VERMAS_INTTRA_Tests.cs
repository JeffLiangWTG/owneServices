using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2VERMAS_INTTRA;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class UniversalShipment2VERMAS_INTTRA_Tests
	{
		const string filePath = "UniversalShipment2VERMAS_INTTRA.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2Vermas_INTTRA()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "INTMSG", isDirect:"true");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "INTMSG", "C00678281_FOOO4134134", "true");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "INTMSG");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string msgID, string previousSubscribeConsolRef = "", string isDirect = "false")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
			};

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA_VM").Repeat.Any();
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("88").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.BGM", "@maxlength", "14")).Return("358").Repeat.Any();
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "INTTRA")).Return("CGWS").Repeat.Any();

			if (isDirect == "true")
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "INTTRA", "@recipientId", "TESTSENDER__1", "@ST_ID", msgID, "@value", "C00678281_FOOO4134134")).Return("C00678281_FOOO4134134").Repeat.Any();
			}
			else
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "INTTRA", "@recipientId", "TESTSENDER__1", "@ST_ID", msgID, "@value", "C00678281_FOOO4134134")).Return("").Repeat.Any();
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "CGWS")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "88", "Verified Gross Container Weight", "DocumentName")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "88", "ForwardingConsol", "ForwardingType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "88", "Container", "SubMessageType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "88", "C00678281_FOOO4134134")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "88")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "C00678281_FOOO4134134", "88", "InterchangeNum")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "INT00000000088", "88", "InterchangeNum")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "INT00000000088", "C00678281_FOOO4134134", "VERMAS")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, "INTTRA", "TESTSENDER__1", "C00678281_FOOO4134134", "INT00000000088", "VERMAS")).Repeat.Any();

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "MAEU")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ")).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", "INTTRA")).Return("INTTRA").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", "INTTRA")).Return("INTID").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "INTTRA")).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", "INTTRA")).Return("INT").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "40G0")).Return("MAPPED40").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "42G0")).Return("MAPPED42").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "22G0")).Return("MAPPED22").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Default Interface Name", "Interface Name", "INTTRA_VM")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_VM", "MAEU")).Return("MAEU1").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "INTTRA_VM", "MAEU")).Return("MAEU").Repeat.Any();

			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VERMAS_CGWS_88")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "88")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "88")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true")).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true")).Repeat.Any();

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalShipment2VERMAS_INTTRA>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2Vermas_ThrowException()
		{
			var input = filePath + "Test4_input.xml";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteAssertException<UniversalShipment2VERMAS_INTTRA>(input, "ContainerCollection did not contain a Container.");

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}
	}
}
