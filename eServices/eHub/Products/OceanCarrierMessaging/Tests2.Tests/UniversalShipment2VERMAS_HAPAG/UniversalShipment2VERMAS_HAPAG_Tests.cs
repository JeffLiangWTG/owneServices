using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2VERMAS_HAPAG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2VERMAS_HAPAG_Tests
  {
    const string filePath = "UniversalShipment2VERMAS_HAPAG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2VERMAS_HAPAG()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "HAPAG_LLOYD")).Return("HLAG");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HAPAG_LLOYD_VM2");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "HLAG")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "88"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "88"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VERMAS_HLAG_88"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB4_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "20160311"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB4_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "0851"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "HAPAG_LLOYD"));

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmm")).Return("201603110851").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.HAPAG_LLOYD.UNH1", "@maxlength", "14")).Return("88").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.HAPAG_LLOYD.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HAPAG_LLOYD", "@recipientId", "TESTSENDER__1", "@ST_ID", "HAPMSG", "@value", "C00001106_CONT1111111")).Return("HPA0000002").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "HAPAG_LLOYD_VM2", "HLAG")).Return("HAPAG_LLOYD");

      mockCodeMapper.Stub(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "42G0")).Return("42G0").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "HLAG"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "88", "C00001106_CONT1111111"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HLA0000000001", "C00001106_CONT1111111"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "C00001106_CONT1111111", "HLA0000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "88"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "88", "Verified Gross Container Weight", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "88", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "88", "Container", "SubMessageType")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2VERMAS_HAPAG>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
