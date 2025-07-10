using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module.Testing
{
	public abstract class JobSailingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestHiddenFilter

		public void TestHiddenFilter()
		{
			JobSailing airSailing = NewSailing(Constants.TransportModes.Road, "AUBNE", "SGSIN");
			JobSailing railSailing = NewSailing(Constants.TransportModes.Road, "AUBNE", "SGSIN");
			JobSailing roadSailing = NewSailing(Constants.TransportModes.Road, "AUBNE", "SGSIN");
			JobSailing seaSailing = NewSailing(Constants.TransportModes.Road, "AUBNE", "SGSIN");

			Factory.Save();

			JobSailing[] sailings;

			sailings = Factory.Load<JobSailing>(Strip.Filter);

			Assert("should have matched atleast one sailing", sailings.Length > 0);

			foreach (JobSailing sailing in sailings)
			{
				AssertEquals("should only find sailings with the correct transport mode", TransportMode, sailing.Voyage.JV_AirSeaRoad);
			}
		}

		JobSailing NewSailing(ZString transportMode, ZString load, ZString discharge)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		#endregion

		#region TestCharterFilter

		public void TestCharterFilter()
		{
			JobSailing charteredSailing = CreateSailing(true);
			JobSailing nonCharteredSailing = CreateSailing(false);

			Factory.Save();

			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Status"]).Property = "";

			((ModuleTextFilter)filter["Charter"]).Property = FreightConstants.CharterFilter.All;
			((ModuleTextFilter)filter["Charter"]).IsActive = true;
			sailings.Load(filter.Filter);
			AssertEquals(true, sailings.Contains(charteredSailing));
			AssertEquals(true, sailings.Contains(nonCharteredSailing));

			((ModuleTextFilter)filter["Charter"]).Property = FreightConstants.CharterFilter.CharterOnlyCode;
			sailings.Load(filter.Filter);
			AssertEquals(true, sailings.Contains(charteredSailing));
			AssertEquals(false, sailings.Contains(nonCharteredSailing));

			((ModuleTextFilter)filter["Charter"]).Property = FreightConstants.CharterFilter.NonCharterOnlyCode;
			sailings.Load(filter.Filter);
			AssertEquals(false, sailings.Contains(charteredSailing));
			AssertEquals(true, sailings.Contains(nonCharteredSailing));

			((ModuleTextFilter)filter["Charter"]).Property = "";
			sailings.Load(filter.Filter);
			AssertEquals(true, sailings.Contains(charteredSailing));
			AssertEquals(true, sailings.Contains(nonCharteredSailing));
		}

		JobSailing CreateSailing(bool isChartered)
		{
			var vessel = RefVessel.LookupVesselByName("Blah", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Blah";
			}

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportMode;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = (isChartered ? "CHA" : "NCH");
			voyage.JV_IsChartered = isChartered;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion

		#region TestNumberFilter

		public void TestNumberFilter()
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = JobSailingFilterBusinessObject.StatusFilterTypes.All;
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			((ModuleTextFilter)filter[filter.BookingRefLabel]).Property = "";
			((ModuleTextFilter)filter[filter.BookingRefLabel]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter[filter.BookingRefLabel]).Property = "9500";

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter[filter.BookingRefLabel]).IsActive = false;
			((ModuleTextFilter)filter[filter.ReferenceNumberLabel]).IsActive = true;
			((ModuleTextFilter)filter[filter.ReferenceNumberLabel]).Property = "SA0000001";

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));
		}

		#endregion

		#region TestShipStatusFilter

		public void TestShipStatusFilter()
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Status"]).Property = "CURRENT";

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter["Status"]).Property = "ARRIVED";

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));
		}

		#endregion

		#region TestDateFilter

		void TestDateFilter(string dateProperty, ZDateTime startFilterDate, ZDateTime endFilterDate)
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Status"]).IsActive = true;

			((ModuleDateFilter)filter[dateProperty]).Property1 = new ZDateTime();
			((ModuleDateFilter)filter[dateProperty]).Property2 = new ZDateTime();
			((ModuleDateFilter)filter[dateProperty]).PropertySearch = "Date range";
			((ModuleDateFilter)filter[dateProperty]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleDateFilter)filter[dateProperty]).Property1 = startFilterDate;
			((ModuleDateFilter)filter[dateProperty]).Property2 = endFilterDate;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));
		}

		public void TestDateLoadETDFilter()
		{
			TestDateFilter("Load Port ETD", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
		}

		public void TestDateLoadATDFilter()
		{
			TestDateFilter("Load Port ATD", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
		}

		public void TestDateLoadSTDFilter()
		{
			TestDateFilter("Load Port STD", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
		}

		public void TestDateLoadETAFilter()
		{
			TestDateFilter("Load Port ETA", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(8));
		}

		public void TestDateLoadATAFilter()
		{
			TestDateFilter("Load Port ATA", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(2));
		}

		public void TestDateLoadSTAFilter()
		{
			TestDateFilter("Load Port ETA", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(8));
		}

		public void TestDateDischargeETAFilter()
		{
			TestDateFilter("Discharge Port ETA", ZDateTime.Now.AddDays(-6), ZDateTime.Now.AddDays(8));
		}

		public void TestDateDischargeATAFilter()
		{
			TestDateFilter("Discharge Port ATA", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(10));
		}

		public void TestDateDischargeSTAFilter()
		{
			TestDateFilter("Discharge Port STA", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(10));
		}

		#endregion

		#region TestShowPublishedFilter

		public void TestShowPublishedFilter()
		{
			Assert("Precondition", !Globals.IsWeb);
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			((ModuleFlagsFilter)filter["Show Unpublished"]).Property0 = true;
			((ModuleFlagsFilter)filter["Show Unpublished"]).IsActive = true;

			try
			{
				Globals.IsWeb = true;
				sailings.Load(filter.Filter);
				Assert("Expect published sailing to be in collection", sailings.Contains(Sailing1.PK));
				Assert("Expect unpublished sailing not to be in collection because it is web", !sailings.Contains(Sailing2.PK));
			}
			finally
			{
				Globals.IsWeb = false;
			}

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleFlagsFilter)filter["Show Unpublished"]).Property0 = false;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));
		}

		#endregion

		#region TestOrgFilter

		public void TestOrgFilter()
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			Assert("Precondition: not a web user", !Globals.IsWeb);
			Assert("Line filter is Guid filter when not a web user", filter["Line"] is ModuleGuidFilter);

			((ModuleTextFilter)filter["Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			((ModuleGuidFilter)filter["Line"]).Property = new ZGuid();
			((ModuleGuidFilter)filter["Line"]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleGuidFilter)filter["Line"]).Property = Sailing1.JX_JV_OH_Line;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));

			Globals.IsWeb = true;
			try
			{
				Sailing2.JX_IsPublished = ZBool.True;
				Factory.Save();

				filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();
				Assert("Line filter is Text filter", filter["Line"] is ModuleTextFilter);
				((ModuleTextFilter)filter["Status"]).Property = "ALL";
				((ModuleTextFilter)filter["Status"]).IsActive = true;
				((ModuleTextFilter)filter["Line"]).IsActive = true;

				sailings.Load(filter.Filter);
				Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
				Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

				((ModuleTextFilter)filter["Line"]).Property = "1st";

				sailings.Load(filter.Filter);
				Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
				Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));

				filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();
				filter.UserIsLoggedIn = true;

				Assert("Line filter is Text filter even when user is logged in", filter["Line"] is ModuleTextFilter);

				AssertEquals("Displayed description should be \"Carrier\", as \"Line\" is ambiguous for localization purposes", "Carrier", filter["Line"].LocalizedDescription);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion

		#region TestPortCountryFilter

		public void TestPortCountryFilter()
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			((ModuleLocationFilter)filter["Load / Discharge"]).Property1 = "";
			((ModuleLocationFilter)filter["Load / Discharge"]).Property2 = "";
			((ModuleLocationFilter)filter["Load / Discharge"]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleLocationFilter)filter["Load / Discharge"]).Property1 = Origin1.JA_RL_NKPortOfLoading;
			((ModuleLocationFilter)filter["Load / Discharge"]).Property2 = Destination1.JB_RL_NKPortOfDischarge;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));
		}

		#endregion

		#region TestLoadAndDischargePortFilters
		public void TestLoadAndDischargePortFilters()
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert("Filter type is ModuleLocationFilter when it's not web", filter["Load / Discharge"] is ModuleLocationFilter);
			Globals.IsWeb = true;
			try
			{
				Sailing2.JX_IsPublished = ZBool.True;
				Factory.Save();

				RefUNLOCO loco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, Origin1.JA_RL_NKPortOfLoading));
				RefUNLOCO loco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, Destination1.JB_RL_NKPortOfDischarge));
				Assert("Precondition", loco1.RL_PortName.Length > 1);
				Assert("Precondition", loco2.RL_PortName.Length > 1);

				filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();
				filter.UserIsLoggedIn = true;
				Assert("Filter type is ModuleLocationFilter when user is logged in", filter["Load / Discharge"] is ModuleLocationFilter);
				filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();
				filter.UserIsLoggedIn = false;

				Assert("Filter type is ModuleTextFilter when user is not logged in", filter["Load Port"] is ModuleTextFilter);
				Assert("Filter type is ModuleTextFilter when user is not logged in", filter["Discharge Port"] is ModuleTextFilter);
				((ModuleTextFilter)filter["Status"]).Property = "ALL";
				((ModuleTextFilter)filter["Status"]).IsActive = true;

				((ModuleTextFilter)filter["Load Port"]).IsActive = true;
				((ModuleTextFilter)filter["Discharge Port"]).IsActive = true;
				((ModuleTextFilter)filter["Load Port"]).Property = "";
				((ModuleTextFilter)filter["Discharge Port"]).Property = "";

				sailings.Load(filter.Filter);
				Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
				Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

				((ModuleTextFilter)filter["Load Port"]).Property = loco1.RL_PortName.Substring(0, loco1.RL_PortName.Length - 1);

				sailings.Load(filter.Filter);
				Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
				Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));

				((ModuleTextFilter)filter["Load Port"]).Property = "";
				((ModuleTextFilter)filter["Discharge Port"]).Property = loco2.RL_PortName.Substring(0, loco2.RL_PortName.Length - 1);

				sailings.Load(filter.Filter);
				Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
				Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}
		#endregion

		#region TestServiceStringFilter
		public void TestServiceStringFilter()
		{
			JobSailingFilterBusinessObject filter = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject();

			((ModuleTextFilter)filter["Status"]).Property = JobSailingFilterBusinessObject.StatusFilterTypes.All;
			((ModuleTextFilter)filter["Status"]).IsActive = true;
			((ModuleTextFilter)filter[filter.ServiceStringLabel]).Property = "";
			((ModuleTextFilter)filter[filter.ServiceStringLabel]).IsActive = true;

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter[filter.ServiceStringLabel]).Property = "this is the first sailing";

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to not be in collection", !sailings.Contains(Sailing2.PK));

			((ModuleTextFilter)filter[filter.ServiceStringLabel]).Property = "this is the second sailing";

			sailings.Load(filter.Filter);
			Assert("Expect sailing 1 to not be in collection", !sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));
		}
		#endregion

		#region Implementation

		protected JobSailing Sailing1, Sailing2;
		protected JobVoyage Voyage1, Voyage2;
		protected VoyageDestination Destination1, Destination2;
		protected VoyageOrigin Origin1, Origin2;
		protected OrgHeader ShipLine1, ShipLine2;
		protected JobSailingCollection sailings;

		protected override void SetUp()
		{
			base.SetUp();
			InitializeValues();
		}

		void InitializeValues()
		{
			Sailing1 = Factory.New<JobSailing>();
			Sailing1.JX_UniqueReference = "SA0000001";
			Voyage1 = Factory.New<JobVoyage>();
			Destination1 = Factory.New<VoyageDestination>();
			Origin1 = Factory.New<VoyageOrigin>();
			Voyage1.JV_AirSeaRoad = TransportMode;
			Destination1.JB_JV = Voyage1.PK;
			Origin1.JA_JV = Voyage1.PK;
			Sailing1.JX_JA = Origin1.PK;
			Sailing1.JX_JB = Destination1.PK;
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(-30);
			Origin1.JA_A_DEP = ZDateTime.Now.AddDays(-25);
			Origin1.JA_S_DEP = ZDateTime.Now.AddDays(-30);
			Origin1.JA_E_ARV = ZDateTime.Now.AddDays(-20);
			Origin1.JA_A_ARV = ZDateTime.Now.AddDays(-15);
			Origin1.JA_S_ARV = ZDateTime.Now.AddDays(-20);
			Destination1.JB_E_ARV = ZDateTime.Now.AddDays(-26);
			Destination1.JB_A_ARV = ZDateTime.Now.AddDays(-21);
			Destination1.JB_A_ARV = ZDateTime.Now.AddDays(-26);
			Sailing1.JX_ServiceString = "this is the first sailing";

			Sailing2 = Factory.New<JobSailing>();
			Sailing2.JX_UniqueReference = "SA0000002";
			Voyage2 = Factory.New<JobVoyage>();
			Destination2 = Factory.New<VoyageDestination>();
			Origin2 = Factory.New<VoyageOrigin>();
			Voyage2.JV_AirSeaRoad = TransportMode;
			Destination2.JB_JV = Voyage2.PK;
			Origin2.JA_JV = Voyage2.PK;
			Sailing2.JX_JA = Origin2.PK;
			Sailing2.JX_JB = Destination2.PK;
			Origin2.JA_E_DEP = ZDateTime.Now;
			Origin2.JA_A_DEP = ZDateTime.Now.AddDays(2);
			Origin2.JA_S_DEP = ZDateTime.Now;
			Origin2.JA_E_ARV = ZDateTime.Now.AddDays(5);
			Origin2.JA_A_ARV = ZDateTime.Now.AddDays(-2);
			Origin2.JA_S_ARV = ZDateTime.Now.AddDays(5);
			Destination2.JB_E_ARV = ZDateTime.Now.AddDays(3);
			Destination2.JB_A_ARV = ZDateTime.Now.AddDays(6);
			Destination2.JB_S_ARV = ZDateTime.Now.AddDays(3);
			Sailing2.JX_ServiceString = "this is the second sailing";

			Sailing1.JX_ReservedMasterBill = "1200";
			Sailing2.JX_ReservedMasterBill = "9500";

			Sailing1.JX_IsPublished = ZBool.True;
			Sailing2.JX_IsPublished = ZBool.False;

			Origin1.JA_RL_NKPortOfLoading = "AUSYD";
			Destination1.JB_RL_NKPortOfDischarge = "USLAX";
			Origin2.JA_RL_NKPortOfLoading = "NZAKL";
			Destination2.JB_RL_NKPortOfDischarge = "JPTYO";

			var vessel1 = RefVessel.LookupVesselByName("ARAFURA", Factory).First();
			var vessel2 = RefVessel.LookupVesselByName("BOTANY BAY", Factory).First();
			Voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			Voyage2.JV_RV_NKVessel = vessel2.RV_FK;

			Voyage1.JV_VoyageFlight = "4999";
			Voyage2.JV_VoyageFlight = "8777";

			ShipLine1 = Factory.New<OrgHeader>();
			ShipLine1.OH_Code = "SHIP1";
			ShipLine1.OH_IsShippingLine = ZBool.True;
			ShipLine1.OH_FullName = "1st ShipLine";
			Voyage1.JV_OH_Line = ShipLine1.PK;

			ShipLine2 = Factory.New<OrgHeader>();
			ShipLine2.OH_Code = "SHIP2";
			ShipLine2.OH_FullName = "2nd ShipLine";
			ShipLine2.OH_IsShippingLine = ZBool.True;
			Voyage2.JV_OH_Line = ShipLine2.PK;

			Factory.Save();

			sailings = new JobSailingCollection(Factory);
		}

		public ZQuery GetSubGroupQuery(ModuleTextFilter filter)
		{
			ZQuery query = filter.Query;

			if (!query.IsEmpty)
			{
				for (var sub = filter.SubGroup; sub != null && sub is ModuleFilterSubGroup s; sub = sub.Parent)
				{
					query = s.GetSubQuery(query);
				}
			}
			return query;
		}

		#region Strip

		public JobSailingFilterBusinessObject Strip
		{
			get { return strip ?? (strip = (JobSailingFilterBusinessObject)GetNewFilterStripBusinessObject()); }
		}
		JobSailingFilterBusinessObject strip;

		#endregion

		#endregion

		protected abstract ZString TransportMode { get; }
	}
}
