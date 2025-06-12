using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UShipment2UInterchangeInclude_Tests
  {
    const string filePath = "IFTMCS.UShipment2UInterchangeInclude.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void Test_UShipment2UInterchangeInclude()
    {
      AssertMapping("Test1_MAA_input.xml", "Test1_MAA_output.xml", "CARGOSMART", false, "subscriberValue", "eHubClientRegistrationIDValue", "C00001005");
      AssertMapping("Test2_ThrowException_input.xml", "Test2_ThrowException_input.xml", "CARGOSMART", false, "", "", "C00001005");
      AssertMapping("Test3_NoDocumentaryOverride_input.xml", "Test3_NoDocumentaryOverride_output.xml", "CARGOSMART", false, "subscriberValue", "eHubClientRegistrationIDValue", "");

      AssertMapping("Test1_MAA_input.xml", "Test1_MAA_output_ContainerTracking.xml", "CARGOSMART", false, "subscriberValue", "eHubClientRegistrationIDValue", "C00001005", "CONTAINER_TRACKING");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string senderID, bool isDirect, string subscriber, string eHubClientRegistrationID, string consolID, string destinationParty = "HYEBNEUAT")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;


      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", subscriber)).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return("XXXMSG");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "XXXMSG", "@value", consolID)).Return(destinationParty).Repeat.Once();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2024-03-08T11:11:11.111");


      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      Exception exception = null;
      try
      {
        mapTester.Execute<UShipment2UInterchangeInclude>(input, expectedOutput);
        mockCodeMapper.VerifyAllExpectations();
      }
      catch (Exception ex)
      {
        if (subscriber == string.Empty && eHubClientRegistrationID == string.Empty)
        {
          exception = ex;
        }
        else
        {
          throw;
        }
      }

      if (subscriber == string.Empty && eHubClientRegistrationID == string.Empty)
      {
        Assert.AreEqual("Unable to resolve recipient Id, message rejected.", exception.Message);
      }
    }
  }
}
