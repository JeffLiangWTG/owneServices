using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2VERMAS_EMAIL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2VERMAS_EMAIL_Tests
  {
    const string filePath = "CarrierUniversal2VERMAS_EMAIL.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Vermas_EMAIL()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", (codeMapper, contextAccessor) =>
      {
        codeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL_VM", "CARRIER_EMAIL_VM", "Verified Gross Container Weight VERMAS to Carrier", "Email Lookup", "Email", "DKCPH", "DK", "INTT")).Return("email@email.com");
        contextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "email@email.com")).Repeat.Once();
      });

      try
      {
        AssertMapping("Test1_input.xml", "Test1_output.xml", (codeMapper, contextAccessor) =>
        {
          codeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL_VM", "CARRIER_EMAIL_VM", "Verified Gross Container Weight VERMAS to Carrier", "Email Lookup", "Email", "DKCPH", "DK", "INTT")).Return("");
        });
        Assert.Fail("Expect Exception should throw.");
      }
      catch (Exception ex)
      {
        var actualException = ex.InnerException ?? ex;
        Assert.AreEqual("Unable to find matching email for [Port:DKCPH], [Country:DK], [Carrier:INTT]", actualException.Message);
      }
    }

    void AssertMapping(string inputFile, string expectedOutputFile, Action<CodeMapper, ContextAccessor> callback)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var recipientID = "CARRIER_EMAIL_VM";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
      mockDateMapper.Expect(x => x.CurrentDateTime("dd-MMM-yyyy HH:mm")).Return("04-Jul-2014 05:43");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VERMAS_TESTSENDER__1_C00678281"));
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "COLA")).Return("COLO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "INTA")).Return("INTT").Repeat.Any();
      callback(mockCodeMapper, mockContextAccessor);

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CAR")).Return("CA").Repeat.Twice();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2VERMAS_EMAIL>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}