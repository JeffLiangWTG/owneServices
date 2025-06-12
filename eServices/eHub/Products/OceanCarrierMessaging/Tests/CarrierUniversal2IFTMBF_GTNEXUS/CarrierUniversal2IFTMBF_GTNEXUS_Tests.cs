using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF_GTNEXUS;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversalShipment2IFTMBF_GTNEXUS_Tests
  {
    const string filePath = "CarrierUniversal2IFTMBF_GTNEXUS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_GTNEXUS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test1_input.xml", "Test1_output_MultiPickupDropOff.xml", multiPickupDelivery:"TRUE");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml", isSummary: "TRUE");
      AssertMapping("Test4_input_TransportModes.xml", "Test4_output_TransportModes.xml");
      AssertMapping("Test7_SHP_GroupingMethod_input.xml", "Test7_SHP_GroupingMethod_output.xml");
      AssertMapping("Test8_DNG_GroupingMethod_NoShipmentsAttached_input.xml", "Test8_DNG_GroupingMethod_NoShipmentsAttached_output.xml");
      AssertMapping("Test9_DNG_GroupingMethod_input.xml", "Test9_DNG_GroupingMethod_output.xml");
      AssertMapping("Test10_BBK_input.xml", "Test10_BBK_output.xml");
      AssertMapping("Test11_ROR_input.xml", "Test11_ROR_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_GTNEXUS_IsOutOfGaugeBeTrue()
    {
      AssertMapping("Test5_input.xml", "Test5_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_GTNEXUS_IsOutOfGaugeBeFalse()
    {
      AssertMapping("Test6_IsOutOfGaugeBeFalse_input.xml", "Test6_IsOutOfGaugeBeFalse_output.xml");
    }

    public void AssertMapping(string inputFile, string expectedOutputFile, string isSummary = "FALSE", string multiPickupDelivery = "FALSE")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.BGM", "@maxlength", "14")).Return("29").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "48K8")).Return("48T8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00001007", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00001007")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00001008", "@referenceType", "JobNumber")).Return("xxx123").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "GTNEXUS")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GNXID", "GTNEXUS", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "C00001007", "JobNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001007", "GTNV000000029", "JobNumber")).Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GTNEXUS_BK1").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.UNH1", "@maxlength", "14")).Return("10");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "10"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001007", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001008", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001007", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001008", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001008", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "Booking Request", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "Booking Request", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "Shipment", "SubMessageType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "Shipment", "SubMessageType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTNV000000029", "10", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "10", "InterchangeNum")).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "10"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_BK1", "BLAA")).Return("BLAH").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_BK1", "MSCU")).Return("MSCU").Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "PKG")).Return("PKG").Repeat.Any();
      // ISO
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      // default
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Defaults", "Package Type")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "GTNEXUS")).Return(isSummary).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider("GTNEXUS_BK1")).Return("GTNEXUS").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "GTNEXUS", "GTNV000000029", "C00001007")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "GTNEXUS", "xxx123", "C00001008")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.Execute<CarrierUniversal2IFTMBF_GTNEXUS>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
    }
  }
}
