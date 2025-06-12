using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2SIFTMBF_ESP;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2SIFTMBF_ESPTests
  {
    const string filePath = "UniversalShipment2SIFTMBF_ESP.TestFiles.";
    ContextAccessor mockContextAccessor;
    CodeMapper mockCodeMapper;
    DateMapper mockDateMapper;
    Dictionary<string, object> extensionObjects;

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2SIFTMBF_ESP()
    {
      AssertMapping1("Test1_input.xml", "Test1_output.xml", "ONEY", "XXXX", "", " BOOKING ");
      AssertMapping1("Test2_input.xml", "Test2_output.xml", "OOLU", "XXXX", "", "88888888");
      AssertMapping1("Test3_input.xml", "Test3_output.xml", "EASI", "XXXX", "PIER-PIER", "BOOKING");
      AssertMapping1("Test4_input.xml", "Test4_output.xml", "OOLU", "C1ST", "", "BOOKING");
      AssertMapping1("Test5_input.xml", "Test5_output.xml", "OOLU", "C1ST", "", "BOOKING", "TRUE", "CODE");
      AssertMapping1("Test6_input.xml", "Test6_output.xml", "APLU", "XXXX", "", "BOOKING", maxSegment: "6");
      AssertMapping1("Test7_input.xml", "Test7_output.xml", "GOSU", "XXXX", "", " BOOKING ");
      AssertMapping1("Test8_input.xml", "Test8_output.xml", "LNLU", "XXXX", "", " BOOKING ");
      AssertMapping1("Test9_input.xml", "Test9_output.xml", "ONEY", "", "", " BOOKING ");
      AssertMapping1("Test10_input.xml", "Test10_output.xml", "MAEU", "XXXX", "", " BOOKING ", maxSegment:"9");
      AssertMapping1("Test11_input_GroupingMethod_SHP.xml", "Test11_output_GroupingMethod_SHP.xml", "ONEY", "", "", "BOOKING");
      AssertMapping1("Test12_input_IsSummary_GroupingMethod_SHP.xml", "Test12_output_IsSummary_GroupingMethod_SHP.xml", "ONEY", "", "", "BOOKING", isSummary: "TRUE");
    }

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestSIFTMBF_ESP2SIFTMBF_ESP_CharCleanup()
    {
      AssertMapping2("Test1_output.xml", "Test1_output(Cleanup).xml");
    }

    void AssertMapping1(string inputFile, string expectedOutputFile, string carrierSCAC, string c1cCode, string deliveryTerm, string fileDescription, string isSummary = "FALSE", string paymentLocation = "NAME", string maxSegment = "5")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("EASIPASS_SO1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "EASIPASS")).Return("ESPS");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_SO1", "TESTSENDER__1", "3", "Shipping Order", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_SO1", "TESTSENDER__1", "3", "ForwardingShipment", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_SO1", "TESTSENDER__1", "3", "Shipment", "SubMessageType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ESPMSG", "EASIPASS_SO1", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.EASIPASS.eManifest", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "EASIPASS_SO1", "EASI")).Return("EASI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Release Type", "Output Code", "EASIPASS_SO1", "BOL")).Return("BOL").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("45R0").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Delivery Term", "Carrier Term", carrierSCAC, c1cCode, "DTD")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Delivery Term", "Carrier Term", carrierSCAC, c1cCode, "PTD")).Return(deliveryTerm).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Package Type", "EASIPASS Code", "PLT")).Return("PX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Package Type", "EASIPASS Code", "UPC")).Return("up").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Package Type", "EASIPASS Code", "PKG")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "ContainerTypeToISOCode", "EASIPASS Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "ContainerTypeToISOCode", "EASIPASS Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Booking Agent", "Output Code", carrierSCAC, c1cCode)).Return("ZZZZ").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Payment Location (SO)", "Location Value", carrierSCAC, c1cCode)).Return(paymentLocation).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Cargo Details Format (SO)", "Is Summary", carrierSCAC, c1cCode)).Return(isSummary).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "Address Format", "Max Segment", carrierSCAC, c1cCode)).Return(maxSegment).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "File Description", "File Description", "TESTSENDER__1", carrierSCAC, c1cCode, "CNNGB")).Return(fileDescription).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("EASIPASS", "EASIPASS", "EASIPASS Provider Configuration", "File Description", "File Description", "TESTSENDER__1", carrierSCAC, c1cCode, "CNSHA")).Return(fileDescription).Repeat.Any();

      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_SO_ESPS_3"));

      extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2SIFTMBF_ESP>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }

    void AssertMapping2(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.Execute<SIFTMBF_ESP2SIFTMBF_ESP_CharCleanup>(input, expectedOutput);
    }
  }
}
