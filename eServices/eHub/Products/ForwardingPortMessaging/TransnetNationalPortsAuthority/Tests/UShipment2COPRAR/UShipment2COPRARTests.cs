using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Transforms.UShipment2COPRAR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Tests
{
  [TestClass]
  public class UShipment2COPRARTests
  {
    const string filePath = "UShipment2COPRAR.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UShipment2COPRAR()
    {
      AssertMapping("Test01_Export_inputTNP.xml", "Test01_Export_outputTNP.xml", "JNB", "C00678967", "Cargo Dues - Export");
      AssertMapping("Test02_Export_inputTNP_ZA.xml", "Test02_Export_outputTNP_ZA.xml", "JNB", "C00678967", "Cargo Dues - Export");
      AssertMapping("Test03_Discharge_input.xml", "Test03_Discharge_output.xml", "JNB", "C00678967", "Cargo Dues - Discharge Coastwise");
      AssertMapping("Test04_Import_input_BBK.xml", "Test04_Import_output_BBK.xml", "JNB", "C00678967", "Cargo Dues - Import (Quotation)");
      AssertMapping("Test05_Import_input_FCL.xml", "Test05_Import_output_FCL.xml", "JNB", "C00678967", "Cargo Dues - Import");
      AssertMapping("Test06_ImportRoadLeg_input.xml", "Test06_ImportRoadLeg_output.xml", "DUR", "C00001625", "Cargo Dues - Import");
      AssertMapping("Test07_Load_input.xml", "Test07_Load_output.xml", "JNB", "C00678967", "Cargo Dues - Load Coastwise");
      AssertMapping("Test08_ExportWTH_input.xml", "Test08_ExportWTH_output.xml", "JNB", "C00678967", "Cargo Dues - Export");
      AssertMapping("Test09_ExportCN_input.xml", "Test09_ExportCN_output.xml", "DUR", "C00678967", "Cargo Dues - Export");
      AssertMapping("Test10_Export_input.xml", "Test10_Export_output.xml", "JNB", "C00678967", "Cargo Dues - Export");

      AssertMapping("Test11_Import_input_AddInfo.xml", "Test11_Import_output_AddInfo.xml", "JNB", "C00678967", "Cargo Dues - Import");
      AssertMapping("Test12_Export_input_AddInfo.xml", "Test12_Export_output_AddInfo.xml", "JNB", "C00678967", "Cargo Dues - Export");
    }

    private void AssertMapping(string inputFile, string expectedOutputFile, string branch, string consolID, string documentName)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Transforms.UShipment2COPRAR.BGM", "@maxlength", "14")).Return("88");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "TESTSENDER__1", "@ST_ID", "NPAMSG", "@value", consolID + "_" + documentName, "@referenceType", "BGM")).Return("200001").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", branch, "TNPA")).Return("TNPAClient");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAID", "TNPA", "TESTSENDER__1", "TNPAClient"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", consolID, "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", consolID, "88", "88"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Cargo Dues - Export", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Cargo Dues - Discharge Coastwise", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Cargo Dues - Load Coastwise", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Cargo Dues - Import (Quotation)", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Cargo Dues - Import", "DocumentName")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "APP", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "ORG", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "AMD", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "WTH", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Export", "Direction")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Import", "Direction")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Discharge Coastwise", "Direction")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "Load Coastwise", "Direction")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "ZADUR", "Port")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "ZACPT", "Port")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", "88", "ZAELS", "Port")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NPAMSG", "TNPA", "TESTSENDER__1", consolID + "_" + documentName, "88", "BGM")).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB2_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "TNPAClient"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "00000000000088"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "00000000000088"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB7", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "COPRAR"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB10", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "NPAIG1.0"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNA6", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "'"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNA6Suffix", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "\r\n"));

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockContextAccessor },
            };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2COPRAR>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
