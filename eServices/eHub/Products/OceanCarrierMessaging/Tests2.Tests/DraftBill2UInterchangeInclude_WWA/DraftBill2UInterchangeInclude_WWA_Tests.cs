using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.DraftBill2UI_WWA;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class DraftBill2UInterchangeInclude_WWA_Tests
  {
    const string filePath = "DraftBill2UInterchangeInclude_WWA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void DraftBill2UInterchangeInclude_WWA()
    {
      AssertMapping("Test1_input.xml", "Test1_output_CLD.xml", subscribedShipmentType:"CLD");
      AssertMapping("Test1_input.xml", "Test1_output_STD.xml", subscribedShipmentType: "STD");
      AssertMapping("Test2_input.xml", "Test2_output.xml", subscribedJobNumber:"");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribedJobNumber = "C01329220", string subscribedShipmentType = "STD")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      var sourceParty = "WWA";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceParty);
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEUAT001"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", sourceParty, "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "WWA0000219252", "@referenceType", "JobNumber")).Return(subscribedJobNumber);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", sourceParty, "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "WWA0000219252", "@referenceType", "ForwardingType")).Return("ForwardingConsol");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "WWA", "@ST_ID", "WWAMSG", "@value", "WWA0000219252")).Return("HYEUAT001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderID", sourceParty, "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "WWA0000219252", "@referenceType", "ShipmentType")).Return(subscribedShipmentType).Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss")).Return("2022-02-02T16:03:38").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE");
      mockOCMHelper.Expect(x => x.IsCoLoad("STD")).Return("FALSE");

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<DraftBill2UInterchangeInclude_WWA>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}