using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingVoyagesFilter))]
	sealed class VesselRoutingVoyagesFilterTestOneStop : VesselRoutingVoyagesFilterTestBase
	{
		public override void TestAllPorts()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage1");

			NewJobVesselSchedule("AUBNE", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage2");

			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage3");
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(32), "Lloyds", "Voyage3");

			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage4");
			NewJobVesselSchedule("AUBNE", ZDateTime.Today.AddDays(32), "Lloyds", "Voyage4");

			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage5");
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(32), "Lloyds", "Voyage5");
			Factory.Save();

			FilterBusinessObject.E9_LloydsNumber = "Lloyds";
			FilterBusinessObject.E9_Port = "AUSYD";
			FilterBusinessObject.E9_DateFrom = new ZDate(2020, 1, 1);
			AssertFilterMatches("Searching by both load and discharge ports", "Voyage1", "Voyage3", "Voyage5");
		}

		public override void TestPerformSearch()
		{
			JobVesselRouting foreignPort = NewJobVesselRouting("MYPKG", "Lloyds", "Voyage");
			JobVesselRouting decoyForeignPort = NewJobVesselRouting("SGSIN", "Decoy", "Voyage");

			JobVesselSchedule domesticPort1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage");
			JobVesselSchedule domesticPort2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(2), "Lloyds", "Voyage");

			JobVesselSchedule decoyDomesticPort1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Decoy", "Voyage");
			JobVesselSchedule decoyDomesticPort2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(2), "Decoy", "Voyage");
			Factory.Save();

			FilterBusinessObject.E9_LloydsNumber = "Lloyds";
			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.All;

			FilterBusinessObject.PerformSearch();
			AssertEquals("Should find the correct voyage", 1, FilterBusinessObject.Voyages.Count);
			AssertEquals("Should find the correct voyage", "Voyage", FilterBusinessObject.Voyages[0].E8_Voyage);
			AssertEquals("Should find all 5 port pairs", 5, FilterBusinessObject.Voyages[0].PortPairs.Count);
		}

		public void TestPerformSearchWithDataFromDifferentDataProviders()
		{
			JobVesselSchedule domesticPort1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage1");
			JobVesselSchedule domesticPort2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(2), "Lloyds", "Voyage1");
			JobVesselSchedule domesticPort3 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddYears(1), "Lloyds", "Voyage2");

			JobVesselRouting foreignPort1 = NewJobVesselRouting("MYPKG", "Lloyds", "Voyage1");
			JobVesselRouting foreignPort2 = NewJobVesselRouting("CNSHA", "Lloyds", "Voyage1");
			JobVesselRouting foreignPort3 = NewJobVesselRouting("GBLON", "Lloyds", "Voyage2");
			JobVesselRouting foreignPort4 = NewJobVesselRouting("DKCPH", "Lloyds", "Voyage2");

			JobVesselSchedule decoyDomesticPort1 = NewJobVesselSchedule("DEBRE", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage1", FreightConstants.VesselDataProviders.DBH);
			JobVesselSchedule decoyDomesticPort2 = NewJobVesselSchedule("DEHAM", ZDateTime.Today.AddDays(2), "Lloyds", "Voyage1", FreightConstants.VesselDataProviders.DBH);
			JobVesselSchedule decoyDomesticPort3 = NewJobVesselSchedule("DECUX", ZDateTime.Today.AddYears(1).AddDays(2), "Lloyds", "Voyage2", FreightConstants.VesselDataProviders.DBH);

			JobVesselRouting decoyForeignPort1 = NewJobVesselRouting("MYPKG", "Lloyds", "Voyage1", decoyDomesticPort1.PK);
			JobVesselRouting decoyForeignPort2 = NewJobVesselRouting("CNSHA", "Lloyds", "Voyage1", decoyDomesticPort1.PK);
			JobVesselRouting decoyForeignPort3 = NewJobVesselRouting("GBLON", "Lloyds", "Voyage2", decoyDomesticPort2.PK);
			JobVesselRouting decoyForeignPort4 = NewJobVesselRouting("DKCPH", "Lloyds", "Voyage2", decoyDomesticPort2.PK);

			Factory.Save();

			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.EstimatedDeparture;
			FilterBusinessObject.E9_DateFrom = ZDateTime.Today.AddDays(1);
			FilterBusinessObject.E9_DateTo = new ZDateTime(2079, 1, 1);
			FilterBusinessObject.E9_LloydsNumber = "Lloyds";
			FilterBusinessObject.E9_DataProvider = "1ST";
			FilterBusinessObject.PerformSearch();

			AssertEquals("Number of AU Export voyages", 2, FilterBusinessObject.Voyages.Count);
			AssertEquals("Data Provider", FilterBusinessObject.Voyages[0].E8_DataProvider, FreightConstants.VesselDataProviders.OneStop);
			AssertEquals("Data Provider", FilterBusinessObject.Voyages[1].E8_DataProvider, FreightConstants.VesselDataProviders.OneStop);
			AssertEquals("E8_EV_PK", FilterBusinessObject.Voyages[1].E8_EV_PK, domesticPort3.PK);
			AssertEquals("Number of Port Pairs", 9, FilterBusinessObject.Voyages[0].PortPairs.Count);
			AssertEquals("Number of Port Pairs", 4, FilterBusinessObject.Voyages[1].PortPairs.Count);
		}

		public override void TestE9_Carrier()
		{
			JobVesselSchedule domesticPort1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Voyage");
			domesticPort1.EV_LineOperator = "OP2";
			domesticPort1.EV_OperatorsDescription = "Operator Description 1";

			JobVesselSchedule domesticPort2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(2), "Voyage");
			domesticPort2.EV_LineOperator = "OP1";
			domesticPort2.EV_OperatorsDescription = "Operator Description 2";

			JobVesselSchedule decoyDomesticPort1 = NewJobVesselSchedule("AUSYD", new ZDateTime(2025, 1, 1), "Decoy");
			decoyDomesticPort1.EV_LineOperator = "OP2";
			decoyDomesticPort1.EV_OperatorsDescription = "Operator Description 2";

			JobVesselSchedule decoyDomesticPort2 = NewJobVesselSchedule("AUSYD", new ZDateTime(2025, 1, 2), "Decoy");
			decoyDomesticPort2.EV_LineOperator = "OP2";
			decoyDomesticPort2.EV_OperatorsDescription = "Operator Description 2";

			Factory.Save();

			FilterBusinessObject.E9_Carrier = "OP1";
			AssertFilterMatches("Should find the correct voyage from line operator code", "Voyage");

			FilterBusinessObject.E9_Carrier = "Operator Description 1";
			AssertFilterMatches("Should find the correct voyage from line operator description", "Voyage");

			FilterBusinessObject.E9_Carrier = "Non-existant port operator";
			AssertFilterMatches("Should not find any voyages from a non-existant port operator", System.Array.Empty<string>());
		}

		public override void TestE9_Voyage()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Now, "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Now, "DcoyVoyage");
			Factory.Save();

			FilterBusinessObject.E9_Voyage = "Voyage";
			AssertFilterMatches("Should match on the correct voyage", "Voyage");
		}

		public override void TestE9_DataProvider()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Now, "235325", "Voyage1", FreightConstants.VesselDataProviders.DAKOSY);
			NewJobVesselSchedule("AUMEL", ZDateTime.Now, "235325", "Voyage2", FreightConstants.VesselDataProviders.DBH);
			Factory.Save();

			FilterBusinessObject.E9_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			AssertFilterMatches("Should match on the correct data provider", "Voyage1");
		}

		public override void TestFilterByImportExport()
		{
			JobVesselSchedule port1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Lloyds", "");
			port1.EV_ShipOperatorVoyageIn = "ImpVoyage";
			port1.EV_ShipOperatorVoyageOut = "ExpVoyage";

			JobVesselSchedule port2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(32), "Lloyds", "");
			port2.EV_ShipOperatorVoyageIn = "ImpVoyage";
			port2.EV_ShipOperatorVoyageOut = "ExpVoyage";

			Factory.Save();
			FilterBusinessObject.E9_LloydsNumber = "Lloyds";

			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.EstimatedArrival;
			AssertFilterMatches("Searching by import voyage produces all port pairs with an in voyage matching", "ImpVoyage");
			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.EstimatedDeparture;
			AssertFilterMatches("Searching by export voyage produces all port pairs with an out voyage matching", "ExpVoyage");
		}

		#region Lloyds Number and Vessel

		public override void TestLloydsNumber_MatchOnVesselName()
		{
			JobVesselSchedule voyage = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Voyage");
			voyage.EV_ShipName = "VesselName";
			voyage.EV_IMOLloydsNumber = "Lloyds";
			JobVesselSchedule decoyVoyage = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "DcoyVoyage");
			decoyVoyage.EV_ShipName = "DecoyVesselName";
			decoyVoyage.EV_IMOLloydsNumber = "Lloyds";
			Factory.Save();

			FilterBusinessObject.E9_RV_NKVesselName = "VesselName";
			AssertFilterMatches("Should match on E9_RV_NKVesselName", "Voyage");
		}

		public override void TestValidateE9_Port()
		{
			FilterBusinessObject.E9_Port = "XXXXX";
			AssertHasErrors("Invalid port code", FilterBusinessObject.E9_PortInfo);
		}

		public override void TestPerformSearch_ValidatesLloydsNumbers()
		{
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Voyage");
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(2), "Voyage");
			Factory.Save();

			FilterBusinessObject.E9_LloydsNumber = "Lloyds";
			FilterBusinessObject.PerformSearch();
			VesselRoutingPortPair portPair = FilterBusinessObject.Voyages[0].PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "MYPKG";
			portPair.E9_RL_NKDischargePort = "AUSYD";
			portPair.E9_IsSelected = true;
			AssertHasWarnings("Expected warning on E8_LloydsNumber as the voyage is selected and no vessel with that lloyds number is registered", FilterBusinessObject.Voyages[0].E8_LloydsNumberInfo);
		}

		public override void TestVesselName_MatchOnRefVesselVesselName()
		{
			RefVessel.RV_Name = "VesselName";
			RefVessel.RV_LloydsNumber = "Lloyds";

			JobVesselSchedule voyage = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Voyage");
			voyage.EV_ShipName = "DecoyVesselName";
			voyage.EV_IMOLloydsNumber = "Lloyds";
			JobVesselSchedule decoyVoyage = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "DcoyVoyage");
			decoyVoyage.EV_ShipName = "DecoyVesselName";
			decoyVoyage.EV_IMOLloydsNumber = "DcyLlds";

			NewJobVesselRouting("GBLON", "Lloyds", "Voyage", ZDateTime.Today.AddDays(1), voyage.PK);
			Factory.Save();

			FilterBusinessObject.E9_RV_NKVesselName = "VesselName";
			AssertFilterMatches("Should match on RV_Name='Vessel Name Criteria', then RV_LloydsNumber=E9_LloydsNumber", "Voyage");
		}

		#endregion

		public override void TestLoadDischargePorts()
		{
			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage1");

			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage2");

			NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage3");
			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(32), "Lloyds", "Voyage3");

			NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Lloyds", "Voyage4");
			NewJobVesselSchedule("AUPER", ZDateTime.Today.AddDays(32), "Lloyds", "Voyage4");

			Factory.Save();
			FilterBusinessObject.E9_LloydsNumber = "Lloyds";

			FilterBusinessObject.E9_Port = "AUMEL";
			FilterBusinessObject.E9_DateFrom = ZDateTime.Today.AddDays(1);
			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.EstimatedArrival;
			AssertFilterMatches("Searching by load port", "Voyage1", "Voyage3");

			FilterBusinessObject.E9_Port = "AUPER";
			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.EstimatedDeparture;
			AssertFilterMatches("Searching by discharge port", "Voyage2", "Voyage4");

			FilterBusinessObject.E9_Port = "AUSYD";
			FilterBusinessObject.E9_DateType = VesselRoutingVoyagesFilter.Constants.Dates.All;
			AssertFilterMatches("Searching by both load and discharge ports", "Voyage3", "Voyage4");
		}

		protected override void AssertDateFilter(ZString dateFilterType, SchemaDateTimeColumn jobVesselScheduleDateProperty)
		{
			FilterBusinessObject.E9_DateType = dateFilterType;
			FilterBusinessObject.E9_DateFrom = ZDateTime.Today.AddDays(2);
			FilterBusinessObject.E9_DateTo = ZDateTime.Today.AddDays(3);

			JobVesselSchedule port = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), "Voyage");
			JobVesselSchedule decoyPort = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(9), "Decoy");

			port[jobVesselScheduleDateProperty] = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertFilterMatches("Out of bounds", System.Array.Empty<string>());

			port[jobVesselScheduleDateProperty] = ZDateTime.Today.AddDays(2);
			Factory.Save();
			AssertFilterMatches("Lower boundary", "Voyage");

			port[jobVesselScheduleDateProperty] = ZDateTime.Today.AddDays(3);
			Factory.Save();
			AssertFilterMatches("Upper boundary", "Voyage");

			port[jobVesselScheduleDateProperty] = ZDateTime.Today.AddDays(4);
			Factory.Save();
			AssertFilterMatches("Out of bounds", System.Array.Empty<string>());

			port.Delete();
			decoyPort.Delete();
			Factory.Save();
		}

		protected override string DataProvider { get { return FreightConstants.VesselDataProviders.OneStop; } }
		protected override string[] ExpectedVoyages_TestE9_LineOperator { get { return new string[] { "Voyage" }; } }
	}
}
