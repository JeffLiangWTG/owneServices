using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2UniversalEvent_Tests
  {
    const string filePath = "UniversalShipment2UniversalEvent.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2UniversalEvent_INTTRA()
    {
      AssertMapping("Test1_SI_input.xml", "Test1_SI_output.xml", "CARGOSMART_BK1", "", "CONT1", "SHIPPING INSTRUCTION", "", subType: MessageRefSubscriptionType.JobNumber);
      AssertMapping("Test2_BK_input.xml", "Test2_BK_output.xml", "", "CGSMSG", "Booking Request", subType: MessageRefSubscriptionType.InterchangeNum);
      AssertMapping("Test3_eMainfest_input.xml", "Test3_eMainfest_output.xml", "CARGOSMART", "CGSMSG", "YMLU9383943", "eManifest", "973808566/B", subType: MessageRefSubscriptionType.IFTMBF);
      AssertMapping("Test4_VM_MultipleContainer_input.xml", "Test4_VM_MultipleContainer_output.xml", "CARGOSMART", "CGSMSG", "CONT1,CONT3", "Verified Gross Container Weight", "S00001001", "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2", subType: MessageRefSubscriptionType.VERMAS);
      AssertMapping("Test5_VM_input.xml", "Test5_VM_output.xml", "CARGOSMART", "CGSMSG", "CONT1", "Verified Gross Container Weight", "S00001001", "http://www.cargowise.com/Schemas/Universal/2011/11", subType: MessageRefSubscriptionType.VERMAS);
      AssertMapping("Test6_eMainfest_MultipleSubshipment_input.xml", "Test6_eMainfest_MultipleSubshipment_output.xml", "CARGOSMART", "CGSMSG", "YMLU9383943", "eManifest", "973808566/A,973808566/B");
      AssertMapping("Test7_ISN_input.xml", "Test7_ISN_output.xml", "HAPAG_LLOYD_BK1", "CGSMSG", "YMLU9383943", "eManifest", "973808566/A", subscribedMessageReference: string.Empty);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string senderId, string messageSenderId, string containerNumbers = "", string documentName = "", string soNumbers = "", string nameSpace = "", MessageRefSubscriptionType subType = MessageRefSubscriptionType.Default, string subscribedMessageReference = "C03078216")
    {

      var stringHelper = new StringHelper();

      if (stringHelper.CharCount(senderId, "_") == 2)
      {
        senderId = stringHelper.GetIndexOfValue(senderId, "_", 2);
      }
      else if (senderId.IndexOf('_') > -1)
      {
        senderId = senderId.Substring(0, senderId.IndexOf('_'));
      }

      var messageRefereneceValue = "C00676795";
      var messageRefereneceValues = new List<string>();
      if (documentName == "eManifest")
      {
        foreach (var soNumber in soNumbers.Split(','))
        {
          messageRefereneceValues.Add(messageRefereneceValue + "_" + soNumber);
        }
      }
      else if (nameSpace == "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2")
      {
        foreach (var containerNumber in containerNumbers.Split(','))
        {
          messageRefereneceValues.Add(messageRefereneceValue + "_" + containerNumber);
        }
      }
      else
      {
        messageRefereneceValues.Add(messageRefereneceValue);
      }

      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("CARGOWISE", "")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderId)).Return(messageSenderId).Repeat.Any();

      var msgID = messageSenderId;
      if (msgID == "")
      {
        msgID = "CW1MSG";
      }

      var msgRefValue = GetMessageRefSubscriptionTypeList(subType, subscribedMessageReference);
      foreach (var value in messageRefereneceValues)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", value, "@referenceType", "JobNumber")).Return(msgRefValue[MessageRefSubscriptionType.JobNumber.ToString()]).Repeat.AtLeastOnce();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", value, "@referenceType", "InterchangeNum")).Return(msgRefValue[MessageRefSubscriptionType.InterchangeNum.ToString()]).Repeat.AtLeastOnce();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", value, "@referenceType", "IFTMBF")).Return(msgRefValue[MessageRefSubscriptionType.IFTMBF.ToString()]).Repeat.AtLeastOnce();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", value, "@referenceType", "IFTMIN")).Return(msgRefValue[MessageRefSubscriptionType.IFTMIN.ToString()]).Repeat.AtLeastOnce();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", value, "@referenceType", "VERMAS")).Return(msgRefValue[MessageRefSubscriptionType.VERMAS.ToString()]).Repeat.AtLeastOnce();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", value)).Return(msgRefValue[MessageRefSubscriptionType.Default.ToString()]).Repeat.AtLeastOnce();
      }
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", msgID, "@value", "9e238311-2885-4cd5-82da-150af33e65a7")).Return("88").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2UniversalEvent>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }


    [TestMethod]
    public void TestSubscriptionTypeList()
    {
      var msgRefList = GetMessageRefSubscriptionTypeList(MessageRefSubscriptionType.IFTMBF, "AAAAA");
      Assert.AreEqual("", msgRefList[MessageRefSubscriptionType.JobNumber.ToString()]);
      Assert.AreEqual("", msgRefList[MessageRefSubscriptionType.InterchangeNum.ToString()]);
      Assert.AreEqual("AAAAA", msgRefList[MessageRefSubscriptionType.IFTMBF.ToString()]);
      Assert.AreEqual("", msgRefList[MessageRefSubscriptionType.IFTMIN.ToString()]);
      Assert.AreEqual("", msgRefList[MessageRefSubscriptionType.VERMAS.ToString()]);
      Assert.AreEqual("", msgRefList[MessageRefSubscriptionType.Default.ToString()]);
    }

    Dictionary<string, string> GetMessageRefSubscriptionTypeList(MessageRefSubscriptionType type, string value)
    {
      var subscriptionTypeList = new Dictionary<string, string>()
      {
        {MessageRefSubscriptionType.JobNumber.ToString(), ""},
        {MessageRefSubscriptionType.InterchangeNum.ToString(), ""},
        {MessageRefSubscriptionType.IFTMBF.ToString(), ""},
        {MessageRefSubscriptionType.IFTMIN.ToString(), ""},
        {MessageRefSubscriptionType.VERMAS.ToString(), ""},
        {MessageRefSubscriptionType.Default.ToString(), ""}
      };
      subscriptionTypeList[type.ToString()] = value;

      return subscriptionTypeList;
    }

    enum MessageRefSubscriptionType
    {
      JobNumber = 1,
      InterchangeNum = 2,
      IFTMBF = 3,
      IFTMIN = 4,
      VERMAS = 5,
      Default = 6
    }
  }
}
