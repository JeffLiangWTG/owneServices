using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas2.D95B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanTracing.Transforms.EDIUniversalShipment2COPRAR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class EDIUniversalShipment2COPRARTest
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2COPRAR_Original()
    {
      const string sourceFile = "EDIUniversalShipment2COPRAR.TestFiles.Universal_Original.xml";
      const string expectedFile = "EDIUniversalShipment2COPRAR.TestFiles.COPRAR_Original.xml";
      TestMapping(sourceFile, expectedFile, "NZAKL Discharge", "ShippingPort_POAL_COPRAR_Discharge", "POAL Discharge", "MRN123");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2COPRAR_Replacement()
    {
      const string sourceFile = "EDIUniversalShipment2COPRAR.TestFiles.Universal_Replacement.xml";
      const string expectedFile = "EDIUniversalShipment2COPRAR.TestFiles.COPRAR_Replacement.xml";
      TestMapping(sourceFile, expectedFile, "NZAKL Discharge", "ShippingPort_POAL_COPRAR_Discharge", "POAL Discharge", "MRN123", "DOC123");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2COPRAR_Cancellation()
    {
      const string sourceFile = "EDIUniversalShipment2COPRAR.TestFiles.Universal_Cancellation.xml";
      const string expectedFile = "EDIUniversalShipment2COPRAR.TestFiles.COPRAR_Cancellation.xml";
      TestMapping(sourceFile, expectedFile, "NZAKL Discharge", "ShippingPort_POAL_COPRAR_Discharge", "POAL Discharge", "MRN123", "DOC123");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2COPRAR_Load()
    {
      const string sourceFile = "EDIUniversalShipment2COPRAR.TestFiles.Universal_Load.xml";
      const string expectedFile = "EDIUniversalShipment2COPRAR.TestFiles.COPRAR_Load.xml";
      TestMapping(sourceFile, expectedFile, "NZAKL Load", "ShippingPort_POAL_COPRAR_Load", "POAL Load", "MRN123");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2COPRAR_DIMSegment()
    {
      const string sourceFile = "EDIUniversalShipment2COPRAR.TestFiles.Universal_DIMSegment.xml";
      const string expectedFile = "EDIUniversalShipment2COPRAR.TestFiles.COPRAR_DIMSegment.xml";
      TestMapping(sourceFile, expectedFile, "NZAKL Discharge", "ShippingPort_POAL_COPRAR_Discharge", "POAL Discharge", "MRN123");
    }

    void TestMapping(string sourceFile, string expectedFile, string portKey, string destinationParty, string ediId, params string[] nums)
    {
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COPRAR", "Ports", "DestinationParty", portKey)).Return(destinationParty);
      mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COPRAR", "Ports", "EdiIdentifier", portKey)).Return(ediId);

      foreach (var num in nums)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanTracing.Transforms.COPRAR", "@maxlength", "14")).Return(num);
      }

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDNZIKB");
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", destinationParty));

      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyName", "http://schemas.microsoft.com/Edi/PropertySchema", destinationParty));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB2_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "HYE"));

      var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
            };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.Execute<EDIUniversalShipment2COPRAR>(sourceFile, expectedFile);
      mockContextAccessor.VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<EFACT_D95B_COPRAR>(expectedFile, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>
        {
          "The element 'EFACT_D95B_COPRAR' in namespace 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006' has invalid child element 'BGM'. List of possible elements expected: 'BGM' in namespace 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006'."
        };
      }
    }
  }
}
