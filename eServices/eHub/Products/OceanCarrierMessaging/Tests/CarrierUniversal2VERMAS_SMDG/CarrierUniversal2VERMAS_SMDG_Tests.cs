using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2VERMAS_SMDG;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2VERMAS_SMDG_Tests
  {
    const string filePath = "CarrierUniversal2VERMAS_SMDG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2EVG()
    {
      AssertMapping("EVG_Test1_input.xml", "EVG_Test1_output.xml", "EVERGREEN_VM", "EVG", "EVERGREEN", "CGWS", "EVGID", "EVGMSG", "EVERGREEN", "EVRG");
      AssertMapping("EVG_Test1_input.xml", "EVG_Test1_output.xml", "EVERGREEN_VM", "EVG", "EVERGREEN", "CGWS", "EVGID", "EVGMSG", "EVERGREEN", "EVRG", partySenderIdentifier: "");
      AssertMapping("EVERGREEN.Test1_input.xml", "EVERGREEN.Test1_output.xml", "EVERGREEN_VM", "EVG", "EVERGREEN", "CGWS", "EVGID", "EVGMSG", "EVERGREEN", "EVRG");
      AssertMapping("EVERGREEN.Test1_input.xml", "EVERGREEN.Test1_output.xml", "EVERGREEN_VM", "EVG", "EVERGREEN", "CGWS", "EVGID", "EVGMSG", "EVERGREEN", "EVRG", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2COSCO()
    {
      AssertMapping("COSCO_Test1_input.xml", "COSCO_Test1_output.xml", "COSCO_VM", "COS", "COSCO", "CGWS", "COSID", "COSMSG", "COSCO", "HLAG");
      AssertMapping("COSCO_Test1_input.xml", "COSCO_Test1_output.xml", "COSCO_VM", "COS", "COSCO", "CGWS", "COSID", "COSMSG", "COSCO", "HLAG", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2MAERSK()
    {
      AssertMapping("MAERSK_Test1_input.xml", "MAERSK_Test1_output.xml", "MAERSK_VM", "MAE", "MAERSK", "CGWS", "MAEID", "MAEMSG", "MAEU", "MAEU", "true");
      AssertMapping("MAERSK_Test1_input.xml", "MAERSK_Test1_output.xml", "MAERSK_VM", "MAE", "MAERSK", "CGWS", "MAEID", "MAEMSG", "MAEU", "MAEU", "true", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2YANGMING()
    {
      AssertMapping("YANGMING_Test1_input.xml", "YANGMING_Test1_output.xml", "YANGMING_VM", "YML", "MAERSK", "CGWS", "YMLID", "YMLMSG", "YANGMING", "HLAG");
      AssertMapping("YANGMING_Test1_input.xml", "YANGMING_Test1_output.xml", "YANGMING_VM", "YML", "MAERSK", "CGWS", "YMLID", "YMLMSG", "YANGMING", "HLAG", partySenderIdentifier: "");
      AssertMapping("YANGMING.Test1_input.xml", "YANGMING.Test1_output.xml", "YANGMING_VM", "YML", "MAERSK", "CGWS", "YMLID", "YMLMSG", "YANGMING", "HLAG");
      AssertMapping("YANGMING.Test1_input.xml", "YANGMING.Test1_output.xml", "YANGMING_VM", "YML", "MAERSK", "CGWS", "YMLID", "YMLMSG", "YANGMING", "HLAG", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2CMACGM()
    {
      AssertMapping("CMACGM_Test1_input.xml", "CMACGM_Test1_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "CMACGM", "CMDU");
      AssertMapping("CMACGM_Test1_input.xml", "CMACGM_Test1_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "CMACGM", "CMDU", partySenderIdentifier: "");
      AssertMapping("CMACGM.Test1_input.xml", "CMACGM.Test1_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "CMACGM", "CMDU");
      AssertMapping("CMACGM.Test1_input.xml", "CMACGM.Test1_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "CMACGM", "CMDU", partySenderIdentifier: "");
      AssertMapping("CMACGM.Test2_input.xml", "CMACGM.Test2_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "CMACGM", "CMDU");
      AssertMapping("CMACGM.Test2_input.xml", "CMACGM.Test2_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "CMACGM", "CMDU", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2CARGOSMART()
    {
      AssertMapping("CARGOSMART_Test1_input.xml", "CARGOSMART_Test1_output.xml", "CARGOSMART_VM", "CGS", "CARGOSMART", "CGWS", "CGSID", "CGSMSG", "CARGOSMART", "OOLU");
      AssertMapping("CARGOSMART_Test1_input.xml", "CARGOSMART_Test1_output.xml", "CARGOSMART_VM", "CGS", "CARGOSMART", "CGWS", "CGSID", "CGSMSG", "CARGOSMART", "OOLU", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2PORTRIX()
    {
      AssertMapping("PRTX_Test1_input.xml", "PRTX_Test1_output.xml", "PORTRIX_VM", "POR", "PORTRIX", "CGWS", "PORID", "PORMSG", "PORTRIX", "PRTX");
      AssertMapping("PRTX_Test1_input.xml", "PRTX_Test1_output.xml", "PORTRIX_VM", "POR", "PORTRIX", "CGWS", "PORID", "PORMSG", "PORTRIX", "PRTX", partySenderIdentifier: "");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2CMACGM_CannotFindUNB3()
    {
      var isThrownException = false;
      try
      {
        AssertMapping("CMACGM_Test1_input.xml", "CMACGM_Test1_output.xml", "CMACGM_VM", "CMA", "CMACGM", "CGWS", "CMAID", "CMAMSG", "", "CMDU");
      }
      catch (ArgumentException ex)
      {
        isThrownException = true;
        Assert.AreEqual("Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:CMACGM_VM], [SCAC:CMDU])",
            ex.Message.Trim());
      }
      Assert.IsTrue(isThrownException);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string recipientID, string carrierCode, string carrierName, string carrierID, string id, string msgID, string combinedSCACCarrierName, string scac, string sendAgentReference = "", string partySenderIdentifier = "CW1")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var partySenderIdentifierForContextProperty = string.IsNullOrEmpty(partySenderIdentifier) ? "CARGOWISE" : partySenderIdentifier;

      var index = recipientID.IndexOf("_");
      var serviceProvider = index >= 0
                          ? recipientID.Substring(0, index)
                          : recipientID;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + carrierName + ".UNH1", "@maxlength", "14")).Return("88");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", carrierName)).Return(carrierID);
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(id, carrierName, "TESTSENDER__1", carrierID));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, carrierName, "TESTSENDER__1", "88", "C00678281"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, carrierName, "TESTSENDER__1", carrierCode + "00000000088", "C00678281", "VERMAS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, carrierName, "TESTSENDER__1", "C00678281", carrierCode + "00000000088", "VERMAS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, carrierName, "TESTSENDER__1", carrierCode + "00000000088", "88", "InterchangeNum"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(msgID, carrierName, "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "88"));
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", carrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", msgID, "@value", "C00678281", "@referenceType", "VERMAS")).Return("");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", carrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", msgID, "@value", "C00678281")).Return("");

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifierForContextProperty));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", combinedSCACCarrierName));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "88"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "88"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VERMAS_" + carrierID + "_88"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(carrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return(id).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return(carrierCode).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMVERMAS", "OCMVERMAS", "OCM VERMAS Configuration", "Defaults", "Send Agent Reference", recipientID)).Return(sendAgentReference).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", recipientID, scac)).Return(combinedSCACCarrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", recipientID, scac)).Return(partySenderIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CAR")).Return("CA").Repeat.Any();

      switch (scac)
      {
        case "HLAG":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "HLAA")).Return("HLAG").Repeat.Any();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "COLA")).Return("COLO").Repeat.Any();
          break;
        case "EVRG":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "EVRA")).Return("EVRG").Repeat.Any();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "COLA")).Return("COLO").Repeat.Any();
          break;
        case "MAEU":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "MAEA")).Return("MAEU").Repeat.Any();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "HLAA")).Return("HLAG").Repeat.Any();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "COLA")).Return("COLO").Repeat.Any();
          break;
        case "CMDU":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "CMDA")).Return("CMDU").Repeat.Any();
          break;
        case "PRTX":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "PRTX")).Return("PRTX").Repeat.Any();
          break;
        case "OOLU":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "OOLU")).Return("OOLU").Repeat.Any();
          break;
      }

      mockOCMHelper.Expect(x => x.GetServiceProvider(recipientID)).Return(serviceProvider).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CarrierUniversal2VERMAS_SMDG>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
