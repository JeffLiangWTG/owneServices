using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.ICS2HelperTest
{
  [TestClass]
  public class ICS2HelperTest
  {
    ICS2Helper Helper => helper ?? (helper = new ICS2Helper());
    ICS2Helper helper;

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestClearAndAddConsigneeCountryAndState()
    {
      Helper.AddConsigneeCountryAndState("BE", "ANT");
      Helper.AddConsigneeCountryAndState("AU", "NSW");
      Helper.AddConsigneeCountryAndState("GB", "BFS");

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().Count, 3);

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().ToArray()[0], "BE-ANT");
      Assert.AreEqual(Helper.GetConsigneeCountryAndState().ToArray()[1], "AU-NSW");
      Assert.AreEqual(Helper.GetConsigneeCountryAndState().ToArray()[2], "GB-BFS");

      Helper.ClearConsigneeCountriesAndStates();

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().Count, 0);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIsConsigneeCountryAndStateInEUNorwaySwitzerlandNorthernIreland()
    {
      Helper.AddConsigneeCountryAndState("AU", "NSW");

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().Count, 1);
      Assert.AreEqual(Helper.IsConsigneeCountryAndStateInEUNorwaySwitzerlandNorthernIreland(), false);

      Helper.AddConsigneeCountryAndState("GB", "LON");

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().Count, 2);
      Assert.AreEqual(Helper.IsConsigneeCountryAndStateInEUNorwaySwitzerlandNorthernIreland(), false);

      Helper.AddConsigneeCountryAndState("GB", "BFS");

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().Count, 3);
      Assert.AreEqual(Helper.IsConsigneeCountryAndStateInEUNorwaySwitzerlandNorthernIreland(), true);

      Helper.ClearConsigneeCountriesAndStates();

      Helper.AddConsigneeCountryAndState("BE", "ANT");

      Assert.AreEqual(Helper.GetConsigneeCountryAndState().Count, 1);
      Assert.AreEqual(Helper.IsConsigneeCountryAndStateInEUNorwaySwitzerlandNorthernIreland(), true);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetPOBox()
    {
      Assert.AreEqual("P.O. Box 888", Helper.GetPOBox("111 Main St, P.O. Box 888, Springfield, IL 62704"));
      Assert.AreEqual("PO Box 888", Helper.GetPOBox("111 Main St, PO Box 888, Toronto, ON M5H 2N2"));
      Assert.AreEqual("Post Office Box 888", Helper.GetPOBox("111 Main St, Post Office Box 888, London, UK SW1A 1AA"));
      Assert.AreEqual("POB 888", Helper.GetPOBox("111 Main St, POB 888, Manchester, UK M1 1AE"));
      Assert.AreEqual("PO Box 888", Helper.GetPOBox("111 Main St, PO Box 888, Sydney, NSW 2000"));
      Assert.AreEqual("PO Box 888", Helper.GetPOBox("2/111 Main St, PO Box 888, Wellington, 6011"));
      Assert.AreEqual("POBOX 888", Helper.GetPOBox("111 Main St, POBOX 888, Vancouver, BC V5K 0A1"));
      Assert.AreEqual("p.o.box 888", Helper.GetPOBox("111 Main St, p.o.box 888, Toronto, ON M5H 2N2"));
      Assert.AreEqual("p.o box 888", Helper.GetPOBox("Unit 2, 111 Main St, p.o box 888, London, UK SW1A 1AA"));
      Assert.AreEqual("P.O. BOX 888", Helper.GetPOBox("111 Main St, P.O. BOX 888, Vancouver, BC V5K 0A1"));
      Assert.AreEqual(string.Empty, Helper.GetPOBox("111 Main St, No PO Box here, Vancouver, BC V5K 0A1"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCountryIsInEuropeUnion()
    {
      var helper = new ICS2Helper();

      Assert.AreEqual(27, Constants.CountriesInEuropeanUnion.Count);
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Austria));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Belgium));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Bulgaria));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Croatia));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Cyprus));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.CzechRepublic));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Denmark));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Estonia));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Finland));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.France));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Germany));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Greece));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Hungary));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Ireland));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Italy));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Latvia));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Lithuania));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Luxembourg));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Malta));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Netherlands));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Poland));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Portugal));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Romania));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Slovakia));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Slovenia));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Spain));
      Assert.IsTrue(helper.CountryIsInEuropeUnion(Constants.CountryCodes.Sweden));

      Assert.IsFalse(helper.CountryIsInEuropeUnion(Constants.CountryCodes.UnitedKingdom));
      Assert.IsFalse(helper.CountryIsInEuropeUnion(Constants.CountryCodes.UnitedStates));
      Assert.IsFalse(helper.CountryIsInEuropeUnion(Constants.CountryCodes.China));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIsNorthernIreland()
    {
      InitialiseCodeMapsTestingContext();
      var helper = new ICS2Helper();

      Assert.IsFalse(helper.IsNorthernIreland(string.Empty));
      Assert.IsFalse(helper.IsNorthernIreland("NZAKL"));
      Assert.IsTrue(helper.IsNorthernIreland("XXXXX"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIsICS2Ports()
    {
      //ICS2 Rules: Ports must be located in the European Union, Norway, Switzerland, or Northern Ireland.
      InitialiseCodeMapsTestingContext();
      var helper = new ICS2Helper();

      Assert.IsFalse(helper.IsICS2Port(string.Empty));
      Assert.IsFalse(helper.IsICS2Port("GBXXX"));         //EuropeUnion = false
      Assert.IsFalse(helper.IsICS2Port("USLAX"));         //EuropeUnion = false
      Assert.IsTrue(helper.IsICS2Port("XXXXX"));
      Assert.IsTrue(helper.IsICS2Port("NOXXX"));          //Norway
      Assert.IsTrue(helper.IsICS2Port("CHXXX"));          //Switzerland
    }

    void InitialiseCodeMapsTestingContext()
    {
      #region TestData

      var ctx = new CodeMapsTestingContext();

      var eHubClient_SI = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };
      ctx.eHubClients.Add(eHubClient_SI);

      var transSet_SI = new eHubTransformationSet { TS_Name = "OCM System Configuration", eHubClient_Recipient = eHubClient_SI };
      ctx.eHubTransformationSets.Add(transSet_SI);

      var codeSet_UNLOCOLookup = new eHubCodeSet { CS_Name = "UNLOCO_Lookup", eHubClient_Sender = eHubClient_SI, eHubClient_Recipient = eHubClient_SI, eHubTransformationSet = transSet_SI };
      var codeSetResult1_UNLOCOLookup = new eHubCodeSetResult { eHubCodeSet = codeSet_UNLOCOLookup, CR_Order = 1, CR_Name = "AdministrativeRegion" };

      var UNLOCOLookup_CodeMapKey1 = new eHubCodeMapKey { eHubCodeSet = codeSet_UNLOCOLookup, CK_Order = 1, CK_Key1Value = "XXXXX" };
      var UNLOCOLookup_CodeMapKey2 = new eHubCodeMapKey { eHubCodeSet = codeSet_UNLOCOLookup, CK_Order = 3, CK_Key1Value = "%", };

      var UNLOCOLookup_codeMapValue1 = new eHubCodeMapValue { eHubCodeMapKey = UNLOCOLookup_CodeMapKey1, eHubCodeSetResult = codeSetResult1_UNLOCOLookup, CV_OutputCode = "Northern Ireland" };
      var UNLOCOLookup_codeMapValue2 = new eHubCodeMapValue { eHubCodeMapKey = UNLOCOLookup_CodeMapKey2, eHubCodeSetResult = codeSetResult1_UNLOCOLookup, CV_OutputCode = string.Empty };

      ctx.eHubCodeMapKeys.Add(UNLOCOLookup_CodeMapKey1);
      ctx.eHubCodeMapKeys.Add(UNLOCOLookup_CodeMapKey2);

      ctx.eHubCodeMapValues.Add(UNLOCOLookup_codeMapValue1);
      ctx.eHubCodeMapValues.Add(UNLOCOLookup_codeMapValue2);

      var transformAccessor = new TransformAccessor();
      transformAccessor.SetCodeMapsTestingContext(ctx);

      #endregion
    }
  }
}