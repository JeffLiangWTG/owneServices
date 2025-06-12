using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Bolero;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Bolero_Tests
  {
    const string filePath = "Bolero.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestBolero()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "C00001001-v1", "CARGOWISE", "1", "CARGOWISE", "C00001001-v1");
      AssertMapping("Test1_input.xml", "Test1_output.xml", "C00001001", "CARGOWISE", recipientID: "CARGOWISE", referenceNumber: "C00001001");
      AssertMapping("Test1_input.xml", "Test1_NonCW1_output.xml", "C00001001", recipientID: "WTLDAUILA", referenceNumber: "pd-230619-ebl_2", nonCW1: true);
      AssertMapping("Test3_input.xml", "Test3_output.xml", "C00001001-v1", "CARGOWISE", "1", "CARGOWISE", "C00001001-v1");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "C00001001", "CARGOWISE", recipientID: "CARGOWISE", referenceNumber: "C00001001");
      AssertMapping("Test3_input.xml", "Test3_NonCW1_output.xml", "C00001001", recipientID: "WTLDAUILA", referenceNumber: "pd-230619-ebl_2", nonCW1: true);
      AssertMapping("Test4_input.xml", "Test4_NonCW1_output.xml", "C00001001", recipientID: "WTLDAUILA", referenceNumber: "pd-230619-ebl_2", nonCW1: true);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestBoleros_RecipientIDNotFound()
    {
      var isThrownException = false;
      try
      {
        AssertMapping("Test2_input.xml", "Test2_output.xml", "C00001001");
      }
      catch (ArgumentException ex)
      {
        isThrownException = true;
        Assert.AreEqual("Could not found matching RecipientID", ex.Message.Trim());
      }
      Assert.IsTrue(isThrownException);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolNo, string subscriber = "", string subscribedCounter = "", string recipientID = "", string referenceNumber = "", bool nonCW1 = false)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptionsByValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Any();

      if (nonCW1)
      {
        mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return("").Repeat.Any();
      }
      else
      {
        mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return(subscriber).Repeat.Any();
        mockSubscriptionHelper.Expect(x => x.GetProvider()).Return("BOLERO_EAD").Repeat.Any();

        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "BOLERO_EAD", "@recipientId", "", "@ST_ID", "OCMMSG", "@value", "CMA0000002412", "@referenceType", "JobNumber")).Return(consolNo).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "BOLERO_EAD", "@recipientId", "", "@ST_ID", "OCMMSG", "@value", "CMA0000002412", "@referenceType", "Client_Job")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "BOLERO_EAD", "@recipientId", "", "@ST_ID", "OCMMSG", "@value", "C2301962977_DFOCN0SGH", "@referenceType", "JobNumber")).Return("C00001002").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "BOLERO_EAD", "@recipientId", "", "@ST_ID", "OCMMSG", "@value", "C2301962977_DFOCN0SGH", "@referenceType", "Client_Job")).Return("C00001003").Repeat.Any();
      }

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("BOLERO_EAD").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "BOLERO_EAD", "@recipientId", "", "@ST_ID", "EBLCNT", "@value", referenceNumber)).Return(subscribedCounter).Repeat.Any();

      mockOCMHelper.Expect(x => x.GetShippingLineSCAC("C1NT")).Return("NPLX").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetShippingLineSCAC("C1CM")).Return("CMDU").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetShippingLineSCAC("C1XP")).Return("").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetShippingLineSCAC(string.Empty)).Return(string.Empty).Repeat.Any();

      var counter = (subscribedCounter == "" ? 1 : (int.Parse( subscribedCounter) + 1)).ToString();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EBLCNT", "BOLERO_EAD", recipientID, referenceNumber, counter)).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EBLMSG", "BOLERO_EAD", recipientID, referenceNumber, "pd-230619-ebl_1", counter)).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EBLMSG", "BOLERO_EAD", recipientID, referenceNumber, "", counter)).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        {"http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper}
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UInterchange2UInterchange>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockSubscriptionHelper.VerifyAllExpectations();
    }
  }
}