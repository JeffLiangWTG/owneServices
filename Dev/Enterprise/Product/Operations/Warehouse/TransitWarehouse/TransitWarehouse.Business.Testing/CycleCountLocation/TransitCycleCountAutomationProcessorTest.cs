using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.TransitWarehouseCycleCountAutomation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitCycleCountAutomationProcessorTest : ScheduledRuleProcessorTest
	{
		#region TestLoadInputFacts

		public void TestLoadInputFacts()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");
			location1.WLV_LocationStatus = LocationStatus.Codes.Held;
			location1.WLV_WA_PickingArea = area.PK;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_WA_PickingArea = area.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();
			AssertEquals("There are 9 locations", 9, locationFacts.Length);

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationTypeCode", "RNO", locationFact1.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact1.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact1.AreaName);
				AssertEquals("RowName", "RO1", locationFact1.RowName);
				AssertEquals("LocationString", "RO1-1-1-1", locationFact1.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Held, locationFact1.LocationStatus);
				AssertEquals("Column", 1, locationFact1.Column);
				AssertEquals("Level", 1, locationFact1.Level);
				AssertEquals("Tray", 1, locationFact1.Tray);

				AssertEquals("LocationTypeCode", "RNO", locationFact2.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact2.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact2.AreaName);
				AssertEquals("RowName", "RO1", locationFact2.RowName);
				AssertEquals("LocationString", "RO1-1-1-2", locationFact2.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Normal, locationFact2.LocationStatus);
				AssertEquals("Column", 1, locationFact2.Column);
				AssertEquals("Level", 1, locationFact2.Level);
				AssertEquals("Tray", 2, locationFact2.Tray);
			});
		}

		public void TestLoadInputFacts_TwoWarehouses()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W02");
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var row2 = Helper.CreateRow(whs2, "RO2", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");
			var location3 = row2.Locations.Single(l => l.WLV_LocationString == "RO2-1-1-1");
			var location4 = row2.Locations.Single(l => l.WLV_LocationString == "RO2-1-1-2");
			location1.WLV_LocationStatus = LocationStatus.Codes.Held;
			location1.WLV_WA_PickingArea = area.PK;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_WA_PickingArea = area.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			AssertEquals("Precondition: wh1 has 10 locations", 10, data.Whs1.Rows.Sum(r => r.Locations.Count));
			AssertEquals("Precondition: wh2 has 8 locations", 8, whs2.Rows.Sum(r => r.Locations.Count));

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();
			AssertEquals("There are 9 locations", 9, locationFacts.Length);
			AssertEquals("No locations from wh2", 0, locationFacts.Where(l => row2.Locations.Any(l2 => l2.PK == l.PK)).Count());

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationTypeCode", "RNO", locationFact1.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact1.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact1.AreaName);
				AssertEquals("RowName", "RO1", locationFact1.RowName);
				AssertEquals("LocationString", "RO1-1-1-1", locationFact1.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Held, locationFact1.LocationStatus);
				AssertEquals("Column", 1, locationFact1.Column);
				AssertEquals("Level", 1, locationFact1.Level);
				AssertEquals("Tray", 1, locationFact1.Tray);

				AssertEquals("LocationTypeCode", "RNO", locationFact2.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact2.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact2.AreaName);
				AssertEquals("RowName", "RO1", locationFact2.RowName);
				AssertEquals("LocationString", "RO1-1-1-2", locationFact2.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Normal, locationFact2.LocationStatus);
				AssertEquals("Column", 1, locationFact2.Column);
				AssertEquals("Level", 1, locationFact2.Level);
				AssertEquals("Tray", 2, locationFact2.Tray);
			});
		}

		public void TestLoadInputFacts_UsesSmartParameterisation()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");
			Factory.Save();

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();

			using (TestConnection.TrackExecutedCommands())
			{
				var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
				var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

				AssertEquals("Exclude default DockDoor location", 9, locationFacts.Length);

				var selectCommand = TestConnection.ExecutedCommands.SingleOrDefault(c =>
					c.Contains("CROSS APPLY dbo.WhsLocationIndexForSort(WLV_Column, WLV_Level, WLV_Tray, WR_Levels, WR_Trays) as LocationIndexForSort"));
				AssertContains("Smart parameterisation should be used.", "WLV_WW_Whs = @WhsPK_NOHISTOGRAM", selectCommand);
			}
		}

		public void TestLoadInputFacts_RCNConsignor()
		{
			TestRCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.Consignor), "LCE");
		}

		public void TestLoadInputFacts_RCNConsignee()
		{
			TestRCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.Consignee), "CED");
		}

		public void TestLoadInputFacts_RCNBookingParty()
		{
			TestRCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.BookingParty), "BKD");
		}

		public void TestLoadInputFacts_RCNBillToParty()
		{
			TestRCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.BillToParty), "CRB");
		}

		public void TestLoadInputFacts_RCNConsignorPickup()
		{
			TestRCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.ConsignorPickup), "CRG");
		}

		void TestRCNLoadInputFacts(string name, string addressType)
		{
			var date = ZDateTimeOffset.Today;
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", data.Whs1.PK, location.PK);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "PKG1", TransitWarehouseStatuses.Codes.Putaway, rtu, location: location);
			var orgHeader = Helper.CreateClient("OrgCode", "OrgName");
			Helper.CreateJobDocAddress(rcn, addressType, orgHeader.MainAddress.PK);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();
			AssertEquals("There is 1 location", 1, locationFacts.Length);

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location.PK).Single();
			switch (name)
			{
				case nameof(ITransitCycleCountLocationFact.Consignor):
					AssertEquals(nameof(ITransitCycleCountLocationFact.Consignor), "OrgCode", locationFact1.Consignor.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.Consignee):
					AssertEquals(nameof(ITransitCycleCountLocationFact.Consignee), "OrgCode", locationFact1.Consignee.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.BookingParty):
					AssertEquals(nameof(ITransitCycleCountLocationFact.BookingParty), "OrgCode", locationFact1.BookingParty.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.BillToParty):
					AssertEquals(nameof(ITransitCycleCountLocationFact.BillToParty), "OrgCode", locationFact1.BillToParty.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.ConsignorPickup):
					AssertEquals(nameof(ITransitCycleCountLocationFact.ConsignorPickup), "OrgCode", locationFact1.ConsignorPickup.Fact.Code);
					break;
			}
		}

		public void TestLoadInputFacts_UNLOCO()
		{
			var date = ZDateTimeOffset.Today;
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK, unloco: "AULTL");
			var unloco = Helper.CreateUNLOCO("AULTL");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", data.Whs1.PK, location.PK);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "PKG1", TransitWarehouseStatuses.Codes.Putaway, rtu, location: location);
			var orgHeader = Helper.CreateClient("OrgCode", "OrgName");
			Helper.CreateJobDocAddress(rcn, "CRG", orgHeader.MainAddress.PK);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();
			AssertEquals("There is 1 location", 1, locationFacts.Length);

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location.PK).Single();
			AssertEquals(nameof(ITransitCycleCountLocationFact.DestinationUNLOCO), "AULTL", locationFact1.DestinationUNLOCO.Fact.UNLOCO);
		}

		public void TestLoadInputFacts_DCNConsignor()
		{
			TestDCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.Consignor), "LCE");
		}

		public void TestLoadInputFacts_DCNConsignee()
		{
			TestDCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.Consignee), "CED");
		}

		public void TestLoadInputFacts_DCNBookingParty()
		{
			TestDCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.BookingParty), "BKD");
		}

		public void TestLoadInputFacts_DCNBillToParty()
		{
			TestDCNLoadInputFacts(nameof(ITransitCycleCountLocationFact.BillToParty), "CRB");
		}

		void TestDCNLoadInputFacts(string name, string addressType)
		{
			var date = ZDateTimeOffset.Today;
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", data.Whs1.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", data.Whs1.PK, location.PK);
			Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Package, "PKG1", TransitWarehouseStatuses.Codes.Putaway, rtu, dcn, location: location);
			var orgHeader = Helper.CreateClient("OrgCode", "OrgName");
			Helper.CreateJobDocAddress(dcn, addressType, orgHeader.MainAddress.PK);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();
			AssertEquals("There is 1 location", 1, locationFacts.Length);

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location.PK).Single();
			switch (name)
			{
				case nameof(ITransitCycleCountLocationFact.Consignor):
					AssertEquals(nameof(ITransitCycleCountLocationFact.Consignor), "OrgCode", locationFact1.Consignor.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.Consignee):
					AssertEquals(nameof(ITransitCycleCountLocationFact.Consignee), "OrgCode", locationFact1.Consignee.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.BookingParty):
					AssertEquals(nameof(ITransitCycleCountLocationFact.BookingParty), "OrgCode", locationFact1.BookingParty.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.BillToParty):
					AssertEquals(nameof(ITransitCycleCountLocationFact.BillToParty), "OrgCode", locationFact1.BillToParty.Fact.Code);
					break;
				case nameof(ITransitCycleCountLocationFact.ConsignorPickup):
					AssertEquals(nameof(ITransitCycleCountLocationFact.ConsignorPickup), "OrgCode", locationFact1.ConsignorPickup.Fact.Code);
					break;
			}
		}

		public void TestLoadInputFacts_ExcludeVoidLocations()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Void;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one VOID location and default DockDoor location", 8, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's VOID", locationFact1);
		}

		public void TestLoadInputFacts_ExcludeDockDoorLocations()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.DDL);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one DockDoor location and default Dockdoor location", 8, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's of LocationClass DockDoor", locationFact1);
		}

		public void TestLoadInputFacts_ExcludePackingStationLocations()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var locationType1 = Helper.CreateLocationType("NO1", "Location Type 1", false, 0, LocationClasses.Codes.PST);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one PackingStation location and default Dockdoor location", 8, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's of LocationClass DockDoor", locationFact1);
		}

		public void TestLoadInputFacts_ExcludePackingConsolidationLocations()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var packingConsolidationlocationType = Helper.CreateLocationType("CON", "Consloidation", false, 0, LocationClasses.Codes.CON);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = packingConsolidationlocationType.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one Packing Consolidation location and default Dockdoor location", 8, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's of LocationClass DockDoor", locationFact1);
		}

		public void TestLoadInputFacts_ExcludeLocation_HasWICEndTimeIsNull()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;

			Helper.CreateCycleCountLocation(location1, CycleCountLocationStatuses.Codes.NotStarted, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one location which has CycleCountLocation where WCL_EndTime is null", 8, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it has a cycle count location which WCL_EndTime is null", locationFact1);
		}

		public void TestLoadInputFacts_ExcludeLocation_HasOpenVariance_WCL_WL_Location()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;

			var ccLocation = Helper.CreateCycleCountLocation(location1, CycleCountLocationStatuses.Codes.Completed, ZDateTimeOffset.Today, ZDateTimeOffset.Today, ZDateTimeOffset.Today);
			ccLocation.WIC_GS_NKAssignedTo = "DDD";
			Helper.CreateCycleCountLocationVariance(ccLocation, varianceQty: 1);

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one location which has an open variance", 8, locationFacts.Length);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it has an open variance by WCL_WL_Location", locationFact1);
		}

		public void TestLoadInputFacts_NotExcludeLocation_HasOpenVariance_WCL_WL_ExpectedLocation()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Held;
			location1.WLV_WA_PickingArea = area.PK;
			location2.WLV_WLT_LocationType = locationType1.PK;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_WA_PickingArea = area.PK;

			var ccLocation1 = Helper.CreateCycleCountLocation(location1, CycleCountLocationStatuses.Codes.Completed, ZDateTimeOffset.Today, ZDateTimeOffset.Today, ZDateTimeOffset.Today);
			ccLocation1.WIC_GS_NKAssignedTo = "DDD";
			var variance = Helper.CreateCycleCountLocationVariance(ccLocation1, varianceQty: 1);
			variance.WIV_WL_ExpectedStockLocation = location2.PK;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();

			AssertEquals("Should not exclude location2 which has open variance by WCC_WL_ExpectedStockLocation", 8, locationFacts.Length);

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it has an open variance by WCL_WL_Location", locationFact1);

			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationTypeCode", "NO1", locationFact2.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact2.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact2.AreaName);
				AssertEquals("RowName", "RO1", locationFact2.RowName);
				AssertEquals("LocationString", "RO1-1-1-2", locationFact2.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Normal, locationFact2.LocationStatus);
				AssertEquals("Column", 1, locationFact2.Column);
				AssertEquals("Level", 1, locationFact2.Level);
				AssertEquals("Tray", 2, locationFact2.Tray);
			});
		}

		public void TestLoadInputFacts_LocationAttribs()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var date1 = ZDateTimeOffset.Today;
			var date2 = new ZDateTimeOffset(2023, 2, 14);
			var date3 = new ZDateTimeOffset(2023, 1, 14);
			var date4 = new ZDateTimeOffset(2023, 1, 1);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("NO2", LocationClasses.Codes.HPL);
			var row = Helper.CreateRow(data.Whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(data.Whs1, "AE1");

			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");

			location1.WLV_CycleCountPathSequence = 1;
			location1.WLV_CycleCountLastPerformed = date1;
			location1.WLV_LastInventoryChangeDate = date2;

			location2.WLV_CycleCountPathSequence = 2;
			location2.WLV_CycleCountLastPerformed = date3;
			location2.WLV_LastInventoryChangeDate = date4;

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "TWC";
			ruleSet.PRS_Name = "TWC-TEST";
			ruleSet.PRS_Description = "TWC-TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			Factory.Save();

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var locationFacts = inputFacts.OfType<ITransitCycleCountLocationFact>().ToArray();
			AssertEquals("There are 8 locations", 9, locationFacts.Length);

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("CycleCountPathSequence", 1, locationFact1.CycleCountPathSequence);
				AssertEquals("CycleCountLastPerformedDate", date1.ToDateTime(), locationFact1.CycleCountLastPerformedDate);
				AssertEquals("InventoryLastChangedDate", date2.ToDateTime(), locationFact1.InventoryLastChangedDate);

				AssertEquals("CycleCountPathSequence", 2, locationFact2.CycleCountPathSequence);
				AssertEquals("CycleCountLastPerformedDate", date3.ToDateTime(), locationFact2.CycleCountLastPerformedDate);
				AssertEquals("InventoryLastChangedDate", date4.ToDateTime(), locationFact2.InventoryLastChangedDate);
			});
		}

		#endregion

		#region TestProcessResults

		public void TestProcessResults_SuccessfullyCreatingTask()
		{
			var loc1 = (PK: Guid.NewGuid(), LocString: "A-1-1");
			var loc2 = (PK: Guid.NewGuid(), LocString: "A-1-2");
			var loc3 = (PK: Guid.NewGuid(), LocString: "A-2-1");

			var cycleCountFact1 = new TransitCycleCountTaskFact(loc1.PK, loc1.LocString, 3);
			var cycleCountFact2 = new TransitCycleCountTaskFact(loc2.PK, loc2.LocString, 1);
			var cycleCountFact3 = new TransitCycleCountTaskFact(loc3.PK, loc3.LocString, 9);
			var cycleCountFacts = new IFact[]
			{
				cycleCountFact1,
				cycleCountFact2,
				cycleCountFact3,
			};

			var createCycleCountLocMock = new Mock<IWhsItemCycleCountLocationCreator>();
			createCycleCountLocMock.
				Setup(m =>
					m.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsItemCycleCountLocationInfo>>()))
				.Returns((BusinessObjectFactory f, IEnumerable<WhsItemCycleCountLocationInfo> infos) =>
				{
					var task1 = f.New<WhsItemCycleCountLocation>();
					task1.WIC_WL_Location = loc1.PK;
					var task2 = f.New<WhsItemCycleCountLocation>();
					task2.WIC_WL_Location = loc2.PK;
					var task3 = f.New<WhsItemCycleCountLocation>();
					task3.WIC_WL_Location = loc3.PK;
					return new[] { task1, task2, task3 };
				});

			var notificationsMock = new Mock<INotifications>();
			var processor = new TransitCycleCountAutomationProcessor(createCycleCountLocMock.Object);
			processor.ProcessResults(Factory, new ProductionRulesEngineResult(cycleCountFacts), notificationsMock.Object, new CancellationToken());

			VerifyNoErrors(notificationsMock);

			var cycleCountLocs = Factory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("3 Cycle Count Locations should exist", 3, cycleCountLocs.Length);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc1.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc2.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc3.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == GetInformationMessageForNothingProcessed())), Times.Never);

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
					new WhsItemCycleCountLocationInfo(loc1.PK, 3),
					new WhsItemCycleCountLocationInfo(loc2.PK, 1),
					new WhsItemCycleCountLocationInfo(loc3.PK, 9),
			};
			createCycleCountLocMock.Verify(c => c.CreateCycleCountLocations(Factory, cycleCountInfos), Times.Once);
		}

		public void TestProcessResults_SkippingLocationAsTaskAlreadyExists()
		{
			var loc1 = (PK: Guid.NewGuid(), LocString: "B-1-1");
			var loc2 = (PK: Guid.NewGuid(), LocString: "B-2-1");
			var cycleCountFact1 = new TransitCycleCountTaskFact(loc1.PK, loc1.LocString, 2);
			var cycleCountFact2 = new TransitCycleCountTaskFact(loc2.PK, loc2.LocString, 4);
			var cycleCountFacts = new IFact[]
			{
				cycleCountFact1,
				cycleCountFact2,
			};

			var createCycleCountLocMock = new Mock<IWhsItemCycleCountLocationCreator>();
			createCycleCountLocMock.
				Setup(m =>
					m.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsItemCycleCountLocationInfo>>()))
				.Returns((BusinessObjectFactory f, IEnumerable<WhsItemCycleCountLocationInfo> infos) =>
				{
					var task1 = f.New<WhsItemCycleCountLocation>();
					task1.WIC_WL_Location = loc1.PK;
					return new[] { task1 };
				});

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor(createCycleCountLocMock.Object);
			processor.ProcessResults(Factory, new ProductionRulesEngineResult(cycleCountFacts), notificationsMock.Object, new CancellationToken());

			VerifyNoErrors(notificationsMock);

			var cycleCountLocs = Factory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("1 Cycle Count Locations should exist", 1, cycleCountLocs.Length);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc1.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Skipped creating Cycle Count Task for Location: {loc2.LocString} as one already exists.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == GetInformationMessageForNothingProcessed())), Times.Never);

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
					new WhsItemCycleCountLocationInfo(loc1.PK, 2),
					new WhsItemCycleCountLocationInfo(loc2.PK, 4),
			};
			createCycleCountLocMock.Verify(c => c.CreateCycleCountLocations(Factory, cycleCountInfos), Times.Once);
		}

		public void TestProcessResults_DBHits()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 5, 20);
			Factory.Save();

			var facts = new List<TransitCycleCountTaskFact>();
			for (var i = 1; i <= 10; i++)
			{
				var location1 = data.Whs1.FindLocation($"A-1-{i}");
				facts.Add(new TransitCycleCountTaskFact(location1.PK.ToGuid(), location1.WLV_LocationString, 3));

				var location2 = data.Whs1.FindLocation($"A-2-{i}");
				facts.Add(new TransitCycleCountTaskFact(location2.PK.ToGuid(), location2.WLV_LocationString, 4));

				var location3 = data.Whs1.FindLocation($"A-3-{i}");
				facts.Add(new TransitCycleCountTaskFact(location3.PK.ToGuid(), location3.WLV_LocationString, 3));
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsItemCycleCountLocationSchema.Constants.TableName, 1 },
			};

			var testFactory = new BusinessObjectFactory();
			var processor = GetProcessor(new WhsItemCycleCountLocationCreator());
			var notificationsMock = new Mock<INotifications>();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, testFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				processor.ProcessResults(testFactory, new ProductionRulesEngineResult(facts), notificationsMock.Object, new CancellationToken());
				testFactory.Save();
			}

			VerifyNoErrors(notificationsMock);

			var cycleCountLocs = Factory.Load<WhsItemCycleCountLocation>(new ZQuery());
			AssertEquals("30 Cycle Count Locations should exist", 30, cycleCountLocs.Length);
		}

		#endregion

		#region Implementation

		protected override string GetInformationMessageForNothingProcessed()
			=> "No cycle count tasks were created in this run.";

		protected override GuidRegistryItem GetErrorContactGroupRegistryItem()
			=> WarehouseDataRegistry.Instance.CycleCountingAutomationFailureNotificationGroup;

		static TransitCycleCountAutomationProcessor GetProcessor(
			IWhsItemCycleCountLocationCreator cycleCountLocationCreator = null)
		{
			return new TransitCycleCountAutomationProcessor(cycleCountLocationCreator ?? Mock.Of<IWhsItemCycleCountLocationCreator>());
		}

		protected override IScheduledRuleProcessor GetProcessor() => new TransitCycleCountAutomationProcessor(new WhsItemCycleCountLocationCreator());

		#endregion
	}
}
