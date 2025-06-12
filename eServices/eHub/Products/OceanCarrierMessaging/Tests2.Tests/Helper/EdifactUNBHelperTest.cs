using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.EdifactUNBHelperTest
{
  [TestClass]
  public class EdifactUNBHelperTest
  {
    CodeMapper MockCodeMapper => mockCodeMapper ?? (mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>());
    CodeMapper mockCodeMapper;

    EdifactUNBHelper Helper => helper ?? (helper = new EdifactUNBHelper(MockCodeMapper));
    EdifactUNBHelper helper;

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestErrorHandling()
    {
      Assert.AreEqual(string.Empty, Helper.GetSyntaxIdentifier(null));
      Assert.AreEqual(string.Empty, Helper.GetSyntaxIdentifier(string.Empty));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetteers()
    {
      const string unb = "UNB+UNOC+MSCU:ZZZ+WISETECH:ZZZ+250409:0745+670'#####";

      Assert.AreEqual(string.Empty, Helper.GetSyntaxIdentifier(null));
      Assert.AreEqual(string.Empty, Helper.GetSyntaxIdentifier(string.Empty));

      Assert.AreEqual("UNOC",                 Helper.GetSyntaxIdentifier(unb));
      Assert.AreEqual("MSCU",                 Helper.GetSender(unb));
      Assert.AreEqual("ZZZ",                  Helper.GetSenderCode(unb));
      Assert.AreEqual("WISETECH",             Helper.GetRecipient(unb));
      Assert.AreEqual("ZZZ",                  Helper.GetRecipientCode(unb));
      Assert.AreEqual("250409",               Helper.GetDate(unb));
      Assert.AreEqual("0745",                 Helper.GetTime(unb));
      Assert.AreEqual("2025-04-09T07:45:00",  Helper.GetDateTime(unb));
      Assert.AreEqual("670",                  Helper.GetInterchangeIdentifier(unb));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestInterchangeIdentifierFallback()
    {
      const string unb = "UNB+UNOC+MSCU:ZZZ+WISETECH:ZZZ+250409:0745+670$$ABC'#####";
      const string counter = "1234";
      MockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.OCMCNF.UNB", "@maxlength", "14")).Return(counter).Repeat.Once();

      Assert.AreEqual(counter, Helper.GetInterchangeIdentifier(unb));
    }
  }
}