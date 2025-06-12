using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2BR_VANGUARD;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2BR_VANGUARD_Tests
  {
    const string filePath = "CarrierUniversal2BR_VANGUARD.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2BR_VANGUARD()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", payableElseWhere: "A");
      AssertMapping("Test3_input_CoLoad.xml", "Test3_output_CoLoad.xml", "CLD");
      AssertMapping("Test4_input.xml", "Test4_output.xml", isSummary: "TRUE");
      AssertMapping("Test5_input_GroupingMethod.xml", "Test5_output_GroupingMethod.xml", "AGT");
      AssertMapping("Test6_input_ColoaderFlag.xml", "Test6_output_ColoaderFlag.xml", "DRT");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string shipmentType = "", string isSummary = "FALSE", string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("edi_test_prod").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VGD_BK1").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTime("yyyy-MM-dd")).Return("2019-05-15").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTime("HH:mm:ss")).Return("15:23:11").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VANGUARD", "@recipientId", "edi_test_prod", "@ST_ID", "VGDMSG", "@value", "TEST30975", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "11", "TEST30975")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "11")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("edi_test_prod", "BN1", "VANGUARD")).Return("CGWS");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.VANGUARD", "@maxlength", "14")).Return("11").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "ContainerTypeToISOCode", "VANGUARD Code", "22G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "ContainerTypeToISOCode", "VANGUARD Code", "45R1")).Return("45R1").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "Sender ID", "Sender")).Return("edi_cargowise_prod");
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "Receiver ID", "Receiver")).Return("vanguard");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "C03078216", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "C03078216", shipmentType, "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "C03078216", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "C03078216", "3.0.0", "FormVersion")).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VGD_BR_0000000011"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "edi_test_prod", "VANGUARD")).Return(isSummary).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("DRT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("VGD_BK1")).Return("VGD").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("VGD")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("edi_test_prod", "CGWS", "VGD", "C03078216", "TEST30975")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CarrierUniversal2BR_VANGUARD>(input, expectedOutput);
    }
  }
}
