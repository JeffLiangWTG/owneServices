using System.Linq;
using CargoWise.EntityFramework.Testing;
using FluentAssertions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgProfitShareDetailsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocationsAllowsRegions()
		{
			Assert("Regions are not allowed in the location lookups of a profit share agreement", Lookups.Locations.AllowZones);
		}

		public void TestFreightModesList()
		{
			AssertEquals("FreightModeList count", 18, Lookups.FreightModes.Count);
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("ALL"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("AIR"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("LCL"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("FCL"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("ULD"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("LSE"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("SEA"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("RAI"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("ROA"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("BLK"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("LQD"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("BBK"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("ROR"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("FTL"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("LTL"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("OTH"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("BCN"));
			Assert("FreightModeList", Lookups.FreightModes.ContainsCode("GRP"));
		}

		public void TestFreightModesDescription()
		{
			AssertEquals("All Transport Modes", Lookups.FreightModes["ALL"].Description);
			AssertEquals("Air (ULD, LSE, BCN)", Lookups.FreightModes["AIR"].Description);
			AssertEquals("Unit Load Device (Air)", Lookups.FreightModes["ULD"].Description);
			AssertEquals("Loose (Air)", Lookups.FreightModes["LSE"].Description);
			AssertEquals("Sea (FCL, LCL, BLK, LQD, BBK, ROR, GRP, BCN)", Lookups.FreightModes["SEA"].Description);
			AssertEquals("Rail (FCL, LCL, BLK, LQD, BBK, GRP, BCN)", Lookups.FreightModes["RAI"].Description);
			AssertEquals("Road (FCL, LCL, FTL, LTL, GRP, BCN)", Lookups.FreightModes["ROA"].Description);
			AssertEquals("Full Container Load (Sea/Rail/Road)", Lookups.FreightModes["FCL"].Description);
			AssertEquals("Less Container Load (Sea/Rail/Road)", Lookups.FreightModes["LCL"].Description);
			AssertEquals("Bulk (Sea/Rail/Road)", Lookups.FreightModes["BLK"].Description);
			AssertEquals("Liquid (Sea/Rail)", Lookups.FreightModes["LQD"].Description);
			AssertEquals("Break Bulk (Sea/Rail)", Lookups.FreightModes["BBK"].Description);
			AssertEquals("Roll On/Roll Off (Sea)", Lookups.FreightModes["ROR"].Description);
			AssertEquals("Full Truck Load (Road)", Lookups.FreightModes["FTL"].Description);
			AssertEquals("Less Truck Load (Road)", Lookups.FreightModes["LTL"].Description);
			AssertEquals("Other (Air/Sea/Rail/Road)", Lookups.FreightModes["OTH"].Description);
			AssertEquals("Groupage/Freight All Kinds (Sea/Rail/Road)", Lookups.FreightModes["GRP"].Description);
			AssertEquals("Buyer’s Consolidation (Air/Sea/Rail/Road)", Lookups.FreightModes["BCN"].Description);
		}

		public void TestOrgOverrideTypesList()
		{
			AssertEquals("OrgOverrideTypesList count", 7, Lookups.OrgOverrideTypes.Count);
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("LOC"));
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("CNE"));
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("CNR"));
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("PUA"));
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("IBR"));
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("EBR"));
			Assert("OrgOverrideTypesList", Lookups.OrgOverrideTypes.ContainsCode("ALL"));
		}

		public void TestJobTypesList()
		{
			Lookups.JobTypes.ToArray()
				.Select(x => (x.Code, x.Description))
				.Should().BeEquivalentTo(new[]
				{
					("", ""),
					("SHP", "Shipment"),
					("GCN", "Gateway Consol"),
				});

			Assert("FluentAssertions is used.", true);
		}

		public void TestGatewayAgentTypesList()
		{
			Lookups.GatewayAgentTypes.ToArray()
				.Select(x => (x.Code, x.Description))
				.Should().BeEquivalentTo(new[]
				{
					("", ""),
					("SGW", "Sending Agent is GTT/GTA"),
					("RGW", "Receiving Agent is GTT/GTA"),
					("BGW", "Both Sending and Receiving Agents are GTT/GTA"),
				});

			Assert("FluentAssertions is used.", true);
		}

		public void TestGatewayProfitApportionmentMethodList()
		{
			Lookups.GatewayProfitApportionmentMethods.ToArray()
				.Select(x => (x.Code, x.Description))
				.Should().BeEquivalentTo(new[]
				{
					("", ""),
					("SHP", "Shipment"),
					("CHG", "Chargeable Units"),
					("GWT", "Gross Weight"),
					("GVT", "Gross Volume"),
				});

			Assert("FluentAssertions is used.", true);
		}

		public void TestAgreementTypesList()
		{
			Lookups.AgreementTypes.ToArray()
				.Select(x => (x.Code, x.Description))
				.Should().BeEquivalentTo(new[]
				{
					("PCF", "Prepaid and Collect Freight Charges"),
					("FRT", "Freight Charge Only"),
					("CLF", "Collect Freight"),
					("ORG", "Origin Charges"),
					("DST", "Destination Charges"),
					("ALL", "All Charges"),
					("FOR", "Prepaid and Collect Freight and Origin"),
					("FDS", "Prepaid and Collect Freight and Destination"),
					("CUS", "Custom List of Charge Codes from Registry"),
					("USR", "User Defined List of Charge Codes"),
				});

			Assert("FluentAssertions is used.", true);
		}

		OrgProfitShareDetailsLookups Lookups
		{
			get { return lookups ?? (lookups = new OrgProfitShareDetailsLookups(Details)); }
		}
		OrgProfitShareDetailsLookups lookups;

		OrgProfitShareDetails Details
		{
			get { return details ?? (details = Factory.NewWithValidTestData<OrgProfitShareDetails>()); }
		}
		OrgProfitShareDetails details;
	}
}
