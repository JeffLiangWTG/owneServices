using System;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.Helper
{
  [TestClass]
  public class TransportLegHelperTest
  {
    [TestMethod]
    public void TestTransportLegHelper_DoNotPromoteToMain_ByVesselVoyage()
    {
			AssertLegTypeByVesselVoyage("WATER 001", "VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("water 001", "VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("BARGE 001", "VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("FERRY 001", "VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "WATER VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "water VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "BARGE VY01", "Sea", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "FERRY VY01", "Sea", "PreCarriage");

      AssertLegTypeByVesselVoyage("WATER 001", "VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("water 001", "VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("BARGE 001", "VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("FERRY 001", "VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "WATER VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "water VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "BARGE VY01", "Sea", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "FERRY VY01", "Sea", "OnForwarding");

      AssertLegTypeByVesselVoyage("WATER 001", "VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("water 001", "VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("BARGE 001", "VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("FERRY 001", "VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "WATER VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "water VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "BARGE VY01", "InlandWaterway", "PreCarriage");
      AssertLegTypeByVesselVoyage("vessel name", "FERRY VY01", "InlandWaterway", "PreCarriage");

      AssertLegTypeByVesselVoyage("WATER 001", "VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("water 001", "VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("BARGE 001", "VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("FERRY 001", "VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "WATER VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "water VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "BARGE VY01", "InlandWaterway", "OnForwarding");
      AssertLegTypeByVesselVoyage("vessel name", "FERRY VY01", "InlandWaterway", "OnForwarding");
    }

    void AssertLegTypeByVesselVoyage(string vesselName, string voyage, string transportMode, string expectedLegType)
    {
      var helper = new TransportLegHelper();
      helper.AddTransportLeg("1", expectedLegType, transportMode, "NZAKL", "Auckland", "AUSYD", "Sydney", vesselName, voyage);

      var newTransportLegs = helper.NewTransportLegs();
      helper.CalculateTransportLegs();

      Assert.AreEqual(expectedLegType, newTransportLegs[0].LegType);
    }

    [TestMethod]
    public void TestTransportLegHelper_PreCarriage()
    {
      var helper = new TransportLegHelper();
      helper.AddTransportLeg("1", "PreCarriage", "Sea", "NZAKL", "Auckland", "AUSYD", "Sydney", "Vessel Name 001", "");
      helper.AddTransportLeg("2", "PreCarriage", "Road", "AUSYD", "Sydney", "AUMEL", "Melbourne", "", "");
      helper.AddTransportLeg("3", "PreCarriage", "InlandWaterway", "AUMEL", "Melbourne", "AUALP", "Abbel Point", "", "");
      helper.AddTransportLeg("4", "Main", "Sea", "AUMEL", "Melbourne", "SGSIN", "Singapore", "Vessel Name 002", "VG02");

      var newTransportLegs = helper.NewTransportLegs();
      helper.CalculateTransportLegs();
      Assert.AreEqual(4, newTransportLegs.Count);

      Assert.AreEqual("Main", newTransportLegs[0].LegType);
      Assert.AreEqual("NZAKL", newTransportLegs[0].PortOfLoadingCode);
      Assert.AreEqual("AUSYD", newTransportLegs[0].PortOfDischargeCode);
      Assert.AreEqual("Sea", newTransportLegs[0].TransportMode);

      Assert.AreEqual("PreCarriage", newTransportLegs[1].LegType);
      Assert.AreEqual("AUSYD", newTransportLegs[1].PortOfLoadingCode);
      Assert.AreEqual("AUMEL", newTransportLegs[1].PortOfDischargeCode);
      Assert.AreEqual("Road", newTransportLegs[1].TransportMode);

      Assert.AreEqual("PreCarriage", newTransportLegs[2].LegType);
      Assert.AreEqual("AUMEL", newTransportLegs[2].PortOfLoadingCode);
      Assert.AreEqual("AUALP", newTransportLegs[2].PortOfDischargeCode);
      Assert.AreEqual("InlandWaterway", newTransportLegs[2].TransportMode);

      Assert.AreEqual("Main", newTransportLegs[3].LegType);
      Assert.AreEqual("AUMEL", newTransportLegs[3].PortOfLoadingCode);
      Assert.AreEqual("SGSIN", newTransportLegs[3].PortOfDischargeCode);
      Assert.AreEqual("Sea", newTransportLegs[3].TransportMode);
    }

    [TestMethod]
    public void TestTransportLegHelper_OnForwarding()
    {
      var helper = new TransportLegHelper();
      helper.AddTransportLeg("1", "OnForwarding", "Sea", "SGSIN", "Singapore", "MYKUL", "Kuala Lumpur", "Vessel Name 003", "065N");
      helper.AddTransportLeg("2", "OnForwarding", "Rail", "MYKUL", "Kuala Lumpur", "HKHKG", "Hong Kong", "", "");
      helper.AddTransportLeg("3", "OnForwarding", "InlandWaterway", "HKHKG", "Hong Kong", "AUALP", "Abbel Point", "", "");

      var newTransportLegs = helper.NewTransportLegs();
      helper.CalculateTransportLegs();
      Assert.AreEqual(3, newTransportLegs.Count);

      Assert.AreEqual("Main", newTransportLegs[0].LegType);
      Assert.AreEqual("SGSIN", newTransportLegs[0].PortOfLoadingCode);
      Assert.AreEqual("MYKUL", newTransportLegs[0].PortOfDischargeCode);
      Assert.AreEqual("Sea", newTransportLegs[0].TransportMode);

      Assert.AreEqual("OnForwarding", newTransportLegs[1].LegType);
      Assert.AreEqual("MYKUL", newTransportLegs[1].PortOfLoadingCode);
      Assert.AreEqual("HKHKG", newTransportLegs[1].PortOfDischargeCode);
      Assert.AreEqual("Rail", newTransportLegs[1].TransportMode);

      Assert.AreEqual("OnForwarding", newTransportLegs[2].LegType);
      Assert.AreEqual("HKHKG", newTransportLegs[2].PortOfLoadingCode);
      Assert.AreEqual("AUALP", newTransportLegs[2].PortOfDischargeCode);
      Assert.AreEqual("InlandWaterway", newTransportLegs[2].TransportMode);
    }

    [TestMethod]
    public void TestTransportLegHelper()
    {
      var helper = new TransportLegHelper();
      helper.AddTransportLeg("1", "PreCarriage", "Sea", "NZAKL", "Auckland", "AUSYD", "Sydney", "Vessel Name 001", "VG01");
      helper.AddTransportLeg("2", "PreCarriage", "Road", "AUSYD", "Sydney", "AUMEL", "Melbourne", "", "");
      helper.AddTransportLeg("3", "PreCarriage", "InlandWaterway", "AUMEL", "Melbourne", "AUALP", "Abbel Point", "", "");
      helper.AddTransportLeg("4", "Main", "Sea", "AUALP", "Abbel Point", "SGSIN", "Singapore", "Vessel Name 002", "VG02");
      helper.AddTransportLeg("5", "OnForwarding", "Sea", "SGSIN", "Singapore", "MYKUL", "Kuala Lumpur", "Vessel Name 003", "065N");
      helper.AddTransportLeg("6", "OnForwarding", "Rail", "MYKUL", "Kuala Lumpur", "HKHKG", "Hong Kong", "", "");
      helper.AddTransportLeg("7", "OnForwarding", "InlandWaterway", "HKHKG", "Hong Kong", "AUALP", "Abbel Point", "", "");

      Assert.AreEqual(7, helper.Count());
      helper.CalculateTransportLegs();

      var newTransportLegs = helper.NewTransportLegs();
      Assert.AreEqual(7, newTransportLegs.Count);

      Assert.AreEqual("PreCarriage", newTransportLegs[0].LegType);
      Assert.AreEqual("AUSYD", newTransportLegs[0].PortOfLoadingCode);
      Assert.AreEqual("AUMEL", newTransportLegs[0].PortOfDischargeCode);
      Assert.AreEqual("Road", newTransportLegs[0].TransportMode);

      Assert.AreEqual("PreCarriage", newTransportLegs[1].LegType);
      Assert.AreEqual("AUMEL", newTransportLegs[1].PortOfLoadingCode);
      Assert.AreEqual("AUALP", newTransportLegs[1].PortOfDischargeCode);
      Assert.AreEqual("InlandWaterway", newTransportLegs[1].TransportMode);

      Assert.AreEqual("Main", newTransportLegs[2].LegType);
      Assert.AreEqual("NZAKL", newTransportLegs[2].PortOfLoadingCode);
      Assert.AreEqual("AUSYD", newTransportLegs[2].PortOfDischargeCode);
      Assert.AreEqual("Sea", newTransportLegs[2].TransportMode);

      Assert.AreEqual("Main", newTransportLegs[3].LegType);
      Assert.AreEqual("AUALP", newTransportLegs[3].PortOfLoadingCode);
      Assert.AreEqual("SGSIN", newTransportLegs[3].PortOfDischargeCode);
      Assert.AreEqual("Sea", newTransportLegs[3].TransportMode);

      Assert.AreEqual("Main", newTransportLegs[4].LegType);
      Assert.AreEqual("SGSIN", newTransportLegs[4].PortOfLoadingCode);
      Assert.AreEqual("MYKUL", newTransportLegs[4].PortOfDischargeCode);
      Assert.AreEqual("Sea", newTransportLegs[4].TransportMode);

      Assert.AreEqual("OnForwarding", newTransportLegs[5].LegType);
      Assert.AreEqual("MYKUL", newTransportLegs[5].PortOfLoadingCode);
      Assert.AreEqual("HKHKG", newTransportLegs[5].PortOfDischargeCode);
      Assert.AreEqual("Rail", newTransportLegs[5].TransportMode);

      Assert.AreEqual("OnForwarding", newTransportLegs[6].LegType);
      Assert.AreEqual("HKHKG", newTransportLegs[6].PortOfLoadingCode);
      Assert.AreEqual("AUALP", newTransportLegs[6].PortOfDischargeCode);
      Assert.AreEqual("InlandWaterway", newTransportLegs[6].TransportMode);
    }
  }
}
