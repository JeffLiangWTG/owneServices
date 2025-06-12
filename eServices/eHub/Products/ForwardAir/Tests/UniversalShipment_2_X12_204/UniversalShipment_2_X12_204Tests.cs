using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.X12.Schemas;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.ForwardAir.Transforms.UniversalShipment_2_X12_204;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardAir.Tests
{
  [TestClass]
  public class UniversalShipment_2_X12_204Tests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2X12_204()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_PUShipPickup_input.xml", "Test3_PUShipPickup_output.xml");
      AssertMapping("Test4_PUShipETD_input.xml", "Test4_PUShipETD_output.xml");
      AssertMapping("Test5_PUTransFCLFCL_input.xml", "Test5_PUTransFCLFCL_output.xml");
      AssertMapping("Test6_PUTransFTLLCL_input.xml", "Test6_PUTransFTLLCL_output.xml");

      AssertMapping("Test7_PUTransLCLLCL_input.xml", "Test7_PUTransLCLLCL_output.xml");
      AssertMapping("Test8_PUTransLCLFCL_input.xml", "Test8_PUTransLCLFCL_output.xml");
      AssertMapping("Test9_PUConsolETD_input.xml", "Test9_PUConsolETD_output.xml");

      AssertMapping("Test10_AirportShipPickup_input.xml", "Test10_AirportShipPickup_output.xml");
      AssertMapping("Test11_AirportShipETD_input.xml", "Test11_AirportShipETD_output.xml");
      AssertMapping("Test12_AirportConsolETD_input.xml", "Test12_AirportConsolETD_output.xml");
      AssertMapping("Test13_MultipleCustomerNumbers_input.xml", "Test13_MultipleCustomerNumbers_output.xml");
      AssertMapping("Test14_InsuranceValueBlank_input.xml", "Test14_InsuranceValueBlank_output.xml");
      AssertMapping("Test15_InsuranceValueZero_input.xml", "Test15_InsuranceValueZero_output.xml");

      AssertMapping("Test16_InsuranceValueGreaterThanZero_input.xml", "Test16_InsuranceValueGreaterThanZero_output.xml");
      AssertMapping("Test17_InsuranceValueGreaterThanZero_MultiSubShipment_input.xml", "Test17_InsuranceValueGreaterThanZero_MultiSubShipment_output.xml");
      AssertMapping("Test18_Single_Door_Pickup_input.xml", "Test18_Single_Door_Pickup_output.xml");
      AssertMapping("Test19_Single_Door_Delivery_input.xml", "Test19_Single_Door_Delivery_output.xml");
      AssertMapping("Test20_Multiple_Shipments_input.xml", "Test20_Multiple_Shipments_output.xml");

      //error Exception Case
      AssertMapping("Test1_input.xml", "Test1_output.xml", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string senderID = "CARW")
    {
      var input = "UniversalShipment_2_X12_204.TestFiles." + inputFile;
      var expectedOutput = "UniversalShipment_2_X12_204.TestFiles." + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("yyyyMMdd"))).Return("20210401");

      mockContextAccessor.Expect(x => x.SetContextProperty("ST02", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "000000111")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA05", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "02"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA06", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CARW"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA07", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "02"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA08", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "FWDN"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA12", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "00400"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA15", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "T"));
      mockContextAccessor.Expect(x => x.SetContextProperty("ISA16", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ":"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS01", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "SM"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS02", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "CARW"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "FWDN"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS04", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "20210401"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS07", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "X"));
      mockContextAccessor.Expect(x => x.SetContextProperty("GS08", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "004010"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyName", "http://schemas.microsoft.com/Edi/PropertySchema", "Override"));
      mockContextAccessor.Expect(x => x.SetContextProperty(Arg<string>.Is.Equal("OverrideFilename"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Send 204", "Client Details", "Forward Air ID", "HYEDUSUAT")).Return(senderID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORAIRCMH", "FORAIRCMH", "Forward Air - Send 204", "Client Details", "Production Indicator", "HYEDUSUAT")).Return("T");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardAir.Transforms.UniversalShipment_2_X12_204.ST02", "@padlength", "9")).Return("000000111");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetIATAfromUNLOCO", "@IATACode", "@UNLOCOCode", "USORD")).Return("ORD").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetIATAfromUNLOCO", "@IATACode", "@UNLOCOCode", "USPHX")).Return("PHX").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetIATAfromUNLOCO", "@IATACode", "@UNLOCOCode", "CAYVR")).Return("YVR").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetIATAfromUNLOCO", "@IATACode", "@UNLOCOCode", "USLAX")).Return("LAX").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      if (senderID == "")
      {
        string expectedMessage = "Your company isn't registered in eHub for ForwardAir Messaging. Please contact your WiseTechGlobal rep. Sender Code: HYEDUSUAT";

        try
        {
          mapTester.ExecuteCompiled<UniversalShipment_2_X12_204>(input, expectedOutput);

          Assert.Fail("An ApplicationExeption was expected, but wasn't thrown");
        }
        catch (Exception ex)
        {
          var actualException = ex.InnerException ?? ex;
          if (!(actualException is ApplicationException))
          {
            Assert.Fail(String.Format("Map UniversalShipment_2_X12_204 threw a TargetInvocationException, but it did not contain an expected inner ApplicationException. Exception caught: {0}", ex));
          }

          Assert.AreEqual(expectedMessage, actualException.Message);
        }
      }
      else
      {
        mapTester.ExecuteCompiled<UniversalShipment_2_X12_204>(input, expectedOutput);

        mockContextAccessor.VerifyAllExpectations();
        mockCodeMapper.VerifyAllExpectations();
        mockDataModelAccessor.VerifyAllExpectations();
        mockDateMapper.VerifyAllExpectations();

        AssertSchema(expectedOutput);
      }
    }

    void AssertSchema(string expectedOutput)
    {
      var errorWhiteList = new List<string>();
      errorWhiteList.Add("The actual length is not equal to the specified length");
      errorWhiteList.Add("The actual length is less than the MinLength value");
      errorWhiteList.Add("SubShipment ConsigneePickupDeliveryAddress");
      errorWhiteList.Add("SubShipment ConsignorPickupDeliveryAddress");

      SchemaValidator.ValidateSchema<X12_00401_204>(expectedOutput, errorWhiteList); 
    }
  }
}
