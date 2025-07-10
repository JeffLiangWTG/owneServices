using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class CommonFilterLookupsTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestEntryStatusListIntegratedCountryCommonCode()
		{
			CombineAssertions("Integrated", () =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
				{
					SetUp();
					ModuleTextFilter filter = (ModuleTextFilter)filterBizObj["Shipment Type"];
					filter.IsActive = true;
					filter.Property = JobMessageTypeList.Codes.Export;
					var list = lookups.EntryStatusList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					AssertEquals("EXP List Count", 2, list.Count);
					Assert("SUB Code", list.ContainsCode("SUB"));
					Assert("ACK Code", list.ContainsCode("ACK"));
					AssertEquals("SUB Description", "Submitted", list.GetDescriptionFromCode("SUB"));
					AssertEquals("ACK Description", "Acknowledged", list.GetDescriptionFromCode("ACK"));
					filter.Property = JobMessageTypeList.Codes.Import;
					list = lookups.EntryStatusList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					AssertEquals("IMP List Count", 2, list.Count);
					Assert("SUB Code", list.ContainsCode("SUB"));
					Assert("ACK Code", list.ContainsCode("ACK"));
					AssertEquals("SUB Description", "Submitted", list.GetDescriptionFromCode("SUB"));
					AssertEquals("ACK Description", "Acknowledged", list.GetDescriptionFromCode("ACK"));
					filter.Property = "";
					list = lookups.EntryStatusList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					AssertEquals("BOTH List Count", 2, list.Count);
					Assert("SUB Code", list.ContainsCode("SUB"));
					Assert("ACK Code", list.ContainsCode("ACK"));
					AssertEquals("SUB Description", "Submitted", list.GetDescriptionFromCode("SUB"));
					AssertEquals("ACK Description", "Acknowledged", list.GetDescriptionFromCode("ACK"));
				}
			});
		}

		public void TestControllingAgents()
		{
			var controllingAgents = lookups.ControllingAgents;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, controllingAgents.IsLoaded);
				AssertType<ControllingAgentCollection>(controllingAgents);
			});
		}

		public void TestFilterControllingAgents()
		{
			var filterControllingAgents = lookups.FilterControllingAgents;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, filterControllingAgents.IsLoaded);
				AssertType<ControllingAgentCollection>(filterControllingAgents);
			});
		}

		public void TestControllingCustomers()
		{
			var controllingCustomers = lookups.ControllingCustomers;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, controllingCustomers.IsLoaded);
				AssertType<ControllingCustomerCollection>(controllingCustomers);
			});
		}

		public void TestFilterControllingCustomers()
		{
			var filterControllingCustomers = lookups.FilterControllingCustomers;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, filterControllingCustomers.IsLoaded);
				AssertType<ControllingCustomerCollection>(filterControllingCustomers);
			});
		}

		public void TestConsignors()
		{
			var consignors = lookups.Consignors;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, consignors.IsLoaded);
				AssertType<ConsignorCollection>(consignors);
			});
		}

		public void TestFilterConsignors()
		{
			var filterConsignors = lookups.FilterConsignors;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, filterConsignors.IsLoaded);
				AssertType<ConsignorCollection>(filterConsignors);
			});
		}

		public void TestConsignees()
		{
			var consignees = lookups.Consignees;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, consignees.IsLoaded);
				AssertType<ConsigneeCollection>(consignees);
			});
		}

		public void TestFilterConsignees()
		{
			var filterConsignees = lookups.FilterConsignees;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, filterConsignees.IsLoaded);
				AssertType<ConsigneeCollection>(filterConsignees);
			});
		}

		public void TestVesselList()
		{
			var vesselList = lookups.VesselList;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, ((IBusinessObjectCollection)vesselList).IsLoaded);
				AssertType<RefVesselCollection>(vesselList);
			});
		}

		public void TestLocationList()
		{
			var locationList = lookups.LocationList;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, locationList.IsLoaded);
				AssertType<LocationCollection>(locationList);
			});
		}

		public void TestStaffList()
		{
			var staffList = lookups.StaffList;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, ((IBusinessObjectCollection)staffList).IsLoaded);
				AssertType<GlbStaffCollection>(staffList);
			});
		}

		public void TestBranchList()
		{
			var branchList = lookups.BranchList;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, branchList.IsLoaded);
				AssertType<GlbBranchCollection>(branchList);
			});
		}

		public void TestCartageList()
		{
			var cartageList = lookups.CartageList;
			CombineAssertions(() =>
			{
				AssertEquals("Collection Not Loaded", false, cartageList.IsLoaded);
				AssertType<LocalTransportCollection>(cartageList);
			});
		}

		protected abstract FilterStripBusinessObject GetFilterStripBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = GetFilterStripBusinessObject();
			lookups = new CommonFilterLookups(filterBizObj);
		}

		FilterStripBusinessObject filterBizObj;
		CommonFilterLookups lookups;
	}
}
