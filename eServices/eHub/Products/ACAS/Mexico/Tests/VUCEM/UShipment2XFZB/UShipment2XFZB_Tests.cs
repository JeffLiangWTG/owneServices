using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ACAS.MX.VUCEM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.ACAS.MX.Tests
{
  [TestClass]
  public class UShipment2XFZB_Tests
  {
    const string filePath = "VUCEM.UShipment2XFZB.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2XFZB_CCT()
    {
      AssertMapping("Test1_input_ORG.xml", "Test1_output_ORG.xml", "ORG", "", true);
      AssertMapping("Test2_input_WTH.xml", "Test2_output_WTH.xml", "WTH", "FZB0000007", false);
      AssertMapping("Test3_input_AMD.xml", "Test3_output_AMD.xml", "AMD", "FZB0000007", false);
      AssertMapping("Test4_input_ORG.xml", "Test4_output_ORG.xml", "ORG", "", false);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string purpose, string previousJobNumber, bool shipmentIdDifferentFromWayBillNumber)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      var serviceProvider = "ACAS_MX";
      var destinationParty = serviceProvider + "_XFZB1";
      var serviceProviderMSGID = "ACASMX";
      var serviceProviderPrefix = "FZB";
      var formattedInterchangeNumber = ""; 

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "ACAS_" + serviceProvider + "_MXCUU"));


      mockCodeMapper.Expect(x => x.GetRecipientCode("ACASMX", "ACASMX", "ACAS System Configuration", "Port Settings", "Name", serviceProvider + "_XFZB1")).Return(serviceProvider).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "BEL", "ACAS_MX")).Return("VUCEM001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_MX", "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "S00001004", "@referenceType", "XFZB")).Return(previousJobNumber).Repeat.Any();
      if (string.IsNullOrEmpty(previousJobNumber))
      {
        formattedInterchangeNumber = serviceProviderPrefix + "0000001";
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "S00001004", "XFZB"));
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "S00001004", formattedInterchangeNumber, "XFZB"));
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.MX.VUCEM.Interchange", "@maxlength", "7")).Return("1");
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "1"));
        mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VUCEM_XFZB_TESTSENDER_1")).Repeat.Any();
      }
      else
      {
        formattedInterchangeNumber = serviceProviderPrefix + "0000007";
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.MX.VUCEM.Interchange", "@maxlength", "7")).Return("7");
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "7"));
        mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VUCEM_XFZB_TESTSENDER_7")).Repeat.Any();
      }
      
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "ForwardingShipment", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, purpose, "Purpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "House AWB (MX)", "DocumentName"));
      if (shipmentIdDifferentFromWayBillNumber)
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "S00001111", "FZB-HWB"));
      }

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "MXCUU", ""));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "VUCEM001"));
      

      var extensionObjects = new Dictionary<string, object>() {
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UShipment2XFZB>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
