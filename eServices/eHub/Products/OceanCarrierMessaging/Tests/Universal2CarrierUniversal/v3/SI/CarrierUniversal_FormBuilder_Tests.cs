using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v3;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  public partial class Universal2CarrierUniversal_V3_Tests
  {
    [TestClass]
    public class Universal2CarrierUniversal_V3_SI_Tests
    {
      const string filePath = "Universal2CarrierUniversal.v3.SI.TestFiles.";

      [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
      public void TestCarrierUniversal_FormBuilder_v3_SI()
      {
        AssertMapping("Test01_input_UniversalShipment.xml", "Test01_output_CarrierUniversal.xml");
        AssertMapping("Test02_NewFormMessage_input_UniversalShipment.xml", "Test02_NewFormMessage_output_CarrierUniversal.xml");

        //FormVersion = 1.0.0
        AssertMapping("Test12_U2CU_SI_tempControl_packingLine_input.xml", "Test12_U2CU_SI_tempControl_packingLine_output.xml");
			}

      void AssertMapping(string inputFile, string expectedOutputFile, string destinationParty = "INTTRA")
      {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
        var mockContextAccessor = MockRepository.GeneratePartialMock<ContextAccessor>();

        mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "STD")).Return("FALSE").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "", "1")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "", "1")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "US", "1")).Return("XXX").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "US", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "US", "1")).Return("YYY").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "US", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "AU", "1")).Return("ZZZ").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "AU", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "AU", "1")).Return("AAA").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "AU", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "CN", "1")).Return("ACN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "CN", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "1")).Return("...").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "2")).Return("UCN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "3")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "ZA", "1")).Return("UZA").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "ZA", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "CN", "1")).Return("SSS").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "CN", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "CN", "3")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "", "1")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "ZA", "1")).Return("").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("NZ"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("NZ"), Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CA"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CA"), Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "INTTRA")).Return("false").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "CMACGM")).Return("true").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "US", "US", "1")).Return("USXXXXX").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "US", "AU", "1")).Return("ZZZZZZZ").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "US", "CN", "2")).Return("UCNNNNN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "US", "1")).Return("USXXXXX_LONG").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "AU", "1")).Return("ZZZZZZZ_LONG").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "CN", "2")).Return("UCNNNNN_LONG").Repeat.Any();

        var extensionObjects = new Dictionary<string, object>()
        {
          {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
          {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
        };

        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.Execute<Universal2CarrierUniversal_SI>(input, expectedOutput);

        mockCodeMapper.VerifyAllExpectations();
        mockContextAccessor.VerifyAllExpectations();

      }
    }
  }
}
