using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Schemas;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMBF_STHEAST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMBF_STHEAST_Tests
  {
    const string filePath = "UniversalShipment2IFTMBF_STHEAST.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2IFTMBF_STHEAST()
    {
      AssertMapping1("Test1_input.xml", "Test1_output.xml");
      AssertMapping1("Test2_input.xml", "Test2_output.xml");
      AssertMapping1("Test3_input.xml", "Test3_output.xml");
      AssertMapping1("Test4_input.xml", "Test4_output.xml");
      AssertMapping1("Test5_input.xml", "Test5_output.xml");
      AssertMapping1("Test6_input_GroupingMethod_SHP.xml", "Test6_output_GroupingMethod_SHP.xml");

      AssertMapping2("Test1_output.xml", "Test1_output(Cleanup).xml");
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
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SOUTHEAST");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "SOUTHEAST")).Return("STHE");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEID", "SOUTHEAST", "TESTSENDER__1", "STHE")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "3", "C00001136")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "Shipping Order", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "C00001136", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "3", "Shipping Order", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("STEMSG", "SOUTHEAST", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SOUTHEAST", "@recipientId", "TESTSENDER__1", "@ST_ID", "STEMSG", "@value", "C00001136")).Return("C00001136").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.SOUTHEAST.UNH1", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "SOUTHEAST", "EASI")).Return("EASI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Release Type", "Output Code", "SOUTHEAST", "BOL")).Return("BOL").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("45R0").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Package Type", "Output Code", "PLT")).Return("PX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Package Type", "Output Code", "PKG")).Return("PX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Container Type", "Output Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Container Type", "Output Code", "45R0")).Return("45R0").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Carrier Code", "Output Code", "EASI", "XXXX")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Carrier Code", "Output Code", "ONEY", "XXXX")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Carrier Code", "Output Code", "OOLU", "XXXX")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Carrier Code", "Output Code", "OOLU", "C1ST")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Carrier Code", "Output Code", "INTT", "")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Receiver ID", "Output Code", "XXXX", "CNNGB")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Receiver ID", "Output Code", "XXXX", "CNSHA")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Receiver ID", "Output Code", "C1ST", "CNNGB")).Return("ZZZZ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SOUTHEAST", "SOUTHEAST", "SOUTHEAST Provider Configuration", "Receiver ID", "Output Code", "", "AUSYD")).Return("ZZZZ").Repeat.Any();

      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_SO_STHE_3"));

      var extensionObjects = new Dictionary<string, object>() {
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMBF_STHEAST>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<SIFTMBF_SOUTHEAST>(expectedOutput, ErrorWhileList);

    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
        {
          "MarksAndNos"
        };
      }
    }
    void AssertMapping2(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.Execute<IFTMBF2IFTMBF_CharCleanup>(input, expectedOutput);
    }
  }
}
