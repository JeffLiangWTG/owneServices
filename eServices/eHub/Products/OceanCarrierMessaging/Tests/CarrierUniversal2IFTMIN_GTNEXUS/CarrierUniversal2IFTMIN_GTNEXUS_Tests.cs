using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN_GTNEXUS;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMIN_GTNEXUS_Tests
  {
    const string filePath = "CarrierUniversal2IFTMIN_GTNEXUS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void CarrierUniversal2IFTMIN_GTNEXUS()
    {
      AssertMapping("Test1_input.xml", "Test1_cleanup.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_cleanup.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_cleanup.xml", "Test3_output.xml", payableElseWhere: "A", payableElseWhereDescription: "ELSEWHERE");
      AssertMapping("Test4_input.xml", "Test4_cleanup.xml", "Test4_output.xml");
      AssertMapping("Test5_input.xml", "Test5_cleanup.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_cleanup.xml", "Test6_output.xml", isSummary: "TRUE");
      AssertMapping("Test7_input.xml", "Test7_cleanup.xml", "Test7_output.xml", isSummary: "TRUE");
      AssertMapping("Test8_input.xml", "Test8_cleanup.xml", "Test8_output.xml");
      AssertMapping("Test9_input.xml", "Test9_cleanup.xml", "Test9_output.xml");
      AssertMapping("Test10_input.xml", "Test10_cleanup.xml", "Test10_output.xml");
      AssertMapping("Test10_input.xml", "Test10_cleanup.xml", "Test10_output_RFFGN1.xml", numberOfSegment: "1");
      AssertMapping("Test10_input.xml", "Test10_cleanup.xml", "Test10_output_RFFGN4.xml", numberOfSegment: "4");
      AssertMapping("Test11_Brazil_summary_input.xml", "Test11_Brazil_summary_cleanup.xml", "Test11_Brazil_summary_output.xml", isSummary: "TRUE");
      AssertMapping("Test12_Brazil_input.xml", "Test12_Brazil_cleanup.xml", "Test12_Brazil_output.xml");
      AssertMapping("Test13_SHP_GroupingMethod_input.xml", "Test13_SHP_GroupingMethod_cleanup.xml", "Test13_SHP_GroupingMethod_output.xml");
      AssertMapping("Test14_input.xml", "Test14_cleanup.xml", "Test14_output.xml");
      AssertMapping("Test15_input.xml", "Test15_cleanup.xml", "Test15_output.xml");
      AssertMapping("Test16_DNG_GroupingMethod_input.xml", "Test16_DNG_GroupingMethod_cleanup.xml", "Test16_DNG_GroupingMethod_output.xml");
      AssertMapping("Test17_BBK_input.xml", "Test17_BBK_cleanup.xml", "Test17_BBK_output.xml");
      AssertMapping("Test18_ROR_input.xml", "Test18_ROR_cleanup.xml", "Test18_ROR_output.xml");
    }

    void AssertMapping(string inputFile, string cleanupFile, string expectedOutputFile, string isSummary = "FALSE", string numberOfSegment = "2", string payableElseWhere = "", string payableElseWhereDescription = "")
    {
      var input = filePath + inputFile;
      var cleanup = filePath + cleanupFile;
      var expectedOutput = filePath + expectedOutputFile;
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.BGM", "@maxlength", "14")).Return("29").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "42G0")).Return("48T8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "40G0")).Return("48T8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "45R1")).Return("48T8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "ContainerTypeToISOCode", "GTNEXUS Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00001007", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00001007")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "TESTSENDER__1", "@ST_ID", "GTNMSG", "@value", "C00001008", "@referenceType", "JobNumber")).Return("xxx123").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "GTNEXUS")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GNXID", "GTNEXUS", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "C00001007", "JobNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001007", "GTN0000000029", "JobNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "10"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "10", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "10", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "Shipment", "SubMessageType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "Shipment", "SubMessageType")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GTNEXUS_SI1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.GTNEXUS.UNH1", "@maxlength", "14")).Return("10");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001007", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001008", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001007", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "C00001008", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "GTN0000000029", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GTNMSG", "GTNEXUS", "TESTSENDER__1", "xxx123", "ForwardingConsol", "ForwardingType")).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "10"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "us2A")).Return("us20").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "ca1A")).Return("ca18").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "CMDA")).Return("CMDU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "ANLA")).Return("ANLC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "ARKU")).Return("ARKU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "MSCU")).Return("MSCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "GTNEXUS_SI1", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "NumberOfSegment", "GTNEXUS_SI1")).Return(numberOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "Inc.RegulatingCountry", "GTNEXUS_SI1")).Return("true").Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "PLT")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "PKG")).Return("P_O").Repeat.Any();
      // ISO
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      // default
      mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Package Type", "GTNEXUS Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("GTNEXUS_SI", "GTNEXUS_SI", "Shipping Instruction IFTMIN to GTNEXUS", "Defaults", "Package Type")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "GTNEXUS")).Return(isSummary).Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("GTNEXUS_SI1")).Return("GTNEXUS").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("GTNEXUS")).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription("GTNEXUS")).Return(payableElseWhereDescription).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "GTNEXUS", "GTN0000000029", "C00001007")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "GTNEXUS", "xxx123", "C00001008")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
            };

      MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly());
      MapTester mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester1.Execute<CU2CUniveralISO8859>(input, cleanup);
      mapTester2.Execute<CarrierUniversal2IFTMIN_GTNEXUS>(cleanup, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
