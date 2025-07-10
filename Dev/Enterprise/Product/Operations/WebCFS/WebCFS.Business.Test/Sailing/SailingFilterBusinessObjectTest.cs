using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;
using JobSailing = Enterprise.Freight.Business.JobSailing;
using JobVoyage = Enterprise.Freight.Business.JobVoyage;

namespace Enterprise.WebCFS.Business.Testing
{
	[TestedType(typeof(SailingFilterBusinessObject))]
	public class SailingFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		#region TestCharterFilter

		public void TestCharterFilter()
		{
			JobSailing charteredSailing = CreateSailing(true);
			JobSailing nonCharteredSailing = CreateSailing(false);

			Factory.Save();
			Filter.ShipStatus = "";

			Filter.CharterStatus = FreightConstants.CharterFilter.All;
			Collection.Load(Filter.Filter);
			AssertEquals(true, Collection.Contains(charteredSailing));
			AssertEquals(true, Collection.Contains(nonCharteredSailing));

			Filter.CharterStatus = FreightConstants.CharterFilter.CharterOnlyCode;
			Collection.Load(Filter.Filter);
			AssertEquals(true, Collection.Contains(charteredSailing));
			AssertEquals(false, Collection.Contains(nonCharteredSailing));

			Filter.CharterStatus = FreightConstants.CharterFilter.NonCharterOnlyCode;
			Collection.Load(Filter.Filter);
			AssertEquals(false, Collection.Contains(charteredSailing));
			AssertEquals(true, Collection.Contains(nonCharteredSailing));

			Filter.CharterStatus = "";
			Collection.Load(Filter.Filter);
			AssertEquals(true, Collection.Contains(charteredSailing));
			AssertEquals(true, Collection.Contains(nonCharteredSailing));
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
			voyage.JV_VoyageFlight = (isChartered ? "Charter" : "NonCharter");
			voyage.JV_IsChartered = isChartered;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion

		#region TestResultsAreRestrictedToTransportMode

		public void TestResultsAreRestrictedToTransportMode()
		{
			Filter.ShipStatus = "ALL";

			Collection.Load(Filter.Filter);
			Assert("Collection should include sailings of relevant transport mode", Collection.Contains(Sailing1.PK));
			Assert("Collection should include sailings of relevant transport mode", Collection.Contains(Sailing2.PK));
			Assert("Collection should not include other transport modes", !Collection.Contains(AlternateTransportModeSailing.PK));
		}

		protected ZString TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		#endregion

		#region TestNumberFilter

		public void TestNumberFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.NumberFilter = SailingFilterBusinessObject.NumberFilterTypes.None;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.NumberFilter = SailingFilterBusinessObject.NumberFilterTypes.ReservedMasterBill;
			Filter.Number = "9500";

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));
		}

		#endregion

		#region TestShipStatusFilter

		public void TestShipStatusFilter()
		{
			Filter.ShipStatus = "ALL";

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.ShipStatus = "CURRENT";

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.ShipStatus = "ARRIVED";

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		#endregion

		#region TestDateFilter

		public void TestDateFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.JX_DateFilterType = SailingFilterBusinessObject.DateFilterTypes.None;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_DateFilterType = SailingFilterBusinessObject.DateFilterTypes.ETD;
			Filter.JX_FromDate = ZDateTime.Now.AddDays(-1);
			Filter.JX_ToDate = ZDateTime.Now.AddDays(1);

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_DateFilterType = SailingFilterBusinessObject.DateFilterTypes.ETA;
			Filter.JX_FromDate = ZDateTime.Now.AddDays(-28);
			Filter.JX_ToDate = ZDateTime.Now.AddDays(-26);

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		#endregion

		#region TestShowPublishedFilter

		public void TestShowPublishedFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.JX_IsPublished = ZBool.True;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_IsPublished = ZBool.False;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		#endregion

		#region TestOrgFilter

		public void TestOrgFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.OrgFilter = SailingFilterBusinessObject.OrgFilterTypes.None;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.OrgFilter = SailingFilterBusinessObject.OrgFilterTypes.Line;
			Filter.JV_OH_Line = Sailing1.JX_JV_OH_Line;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));

			AssertEquals("Displayed description should be \"Carrier\", as \"Line\" is ambiguous for localization purposes", "Carrier", Filter.OrgFilter_List[1].MultilingualDescription);
		}

		#endregion

		#region TestPortFilter

		public void TestPortFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.JX_PortFilterType = SailingFilterBusinessObject.PortFilterTypes.None;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_PortFilterType = SailingFilterBusinessObject.PortFilterTypes.LoadDischarge;
			Filter.JX_RL_NKPort1 = Origin1.JA_RL_NKPortOfLoading;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));

			Filter.JX_RL_NKPort2 = Destination2.JB_RL_NKPortOfDischarge;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification alters desired class name")]
		public void TestPortFilter_DoesNotExceedMaxLengths()
		{
			var expectedPort1 = new string('A', VoyageOrigin.Schema.JA_RL_NKPortOfLoadingMaxLength);
			var expectedPort2 = new string('B', VoyageDestination.Schema.JB_RL_NKPortOfDischargeMaxLength);

			Filter.JX_RL_NKPort1 = expectedPort1 + "A";
			Filter.JX_RL_NKPort2 = expectedPort2 + "B";

			var expectedSubFilter1 = $"JA_RL_NKPortOfLoading = '{expectedPort1}'";
			var expectedSubFilter2 = $"JB_RL_NKPortOfDischarge = '{expectedPort2}'";

			var filterText = Filter.Filter.LiteralTextADO;
			AssertContains(expectedSubFilter1, filterText);
			AssertContains(expectedSubFilter2, filterText);
		}

		#endregion

		#region TestCountryFilter

		public void TestCountryFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.JX_CountryFilterType = SailingFilterBusinessObject.CountryFilterTypes.None;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_CountryFilterType = SailingFilterBusinessObject.CountryFilterTypes.LoadDischarge;
			Filter.JX_RN_Country1 = Sailing1.JX_Calc_LoadCountry;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));

			Filter.JX_RN_Country2 = Sailing2.JX_Calc_DischargeCountry;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 not to be in collection", !Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		#endregion

		#region TestVesselFilter

		public void TestVesselFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.JX_JV_NKVessel = "";

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_JV_NKVessel = Voyage1.JV_RV_NKVessel;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification alters desired class name")]
		public void TestVesselFilter_DoesNotExceedMaxLengths()
		{
			var expectedVessel = new string('A', JobVoyage.Schema.JV_RV_NKVesselMaxLength);

			Filter.JX_JV_NKVessel = expectedVessel + "A";

			var expectedSubFilter1 = $"JV_RV_NKVessel = '{expectedVessel}'";
			AssertContains(expectedSubFilter1, Filter.Filter.LiteralTextADO);
		}

		public void TestVesselListValidation()
		{
			Filter.JX_JV_NKVessel = "Non-existant vessel name";
			if (ExpectVesselListValidation)
			{
				AssertHasErrors("JX_JV_NKVessel must be an existing vessel", Filter.JX_JV_NKVesselInfo);
			}
			else
			{
				AssertNoErrors("JX_JV_NKVessel may be free text", Filter.JX_JV_NKVesselInfo);
			}

			Filter.JX_JV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			AssertNoErrors("No error because JX_JV_NKVessel has an existing vessel name", Filter.JX_JV_NKVesselInfo);
		}

		protected virtual bool ExpectVesselListValidation
		{
			get { return true; }
		}

		#endregion

		#region TestVoyageNoFilter

		public void TestVoyageNoFilter()
		{
			Filter.ShipStatus = "ALL";
			Filter.JX_JV_VoyageFlight = "";

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", Collection.Contains(Sailing2.PK));

			Filter.JX_JV_VoyageFlight = Voyage1.JV_VoyageFlight;

			Collection.Load(Filter.Filter);
			Assert("Expect sailing 1 to be in collection", Collection.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !Collection.Contains(Sailing2.PK));
		}

		#endregion

		public void TestFilter_DoesNotExceedMaxLengths()
		{
			var expectedVoyageFlight = new string('A', AutoJobVoyage.Schema.JV_VoyageFlightMaxLength);

			Filter.JX_JV_VoyageFlight = expectedVoyageFlight + "A";

			var expectedSubFilter = $"JV_VoyageFlight = '{expectedVoyageFlight}'";
			AssertContains(expectedSubFilter, Filter.Filter.LiteralTextADO);
		}

		#region Implementation

		protected WebFilterBusinessObjectFactory FilterFactory = new WebFilterBusinessObjectFactory(new BusinessObjectFactory());
		protected SailingFilterBusinessObject Filter;
		protected JobSailingCollection Collection;

		protected JobSailing Sailing1, Sailing2, AlternateTransportModeSailing;
		protected JobVoyage Voyage1, Voyage2, AlternateTransportModeVoyage;
		protected VoyageDestination Destination1, Destination2, AlternateTransportModeDestination;
		protected VoyageOrigin Origin1, Origin2, AlternateTransportModeOrigin;

		protected OrgHeader ShipLine1, ShipLine2;

		protected override void SetUp()
		{
			base.SetUp();

			Sailing1 = Factory.New<JobSailing>();
			Voyage1 = Factory.New<JobVoyage>();
			Destination1 = Factory.New<VoyageDestination>();
			Origin1 = Factory.New<VoyageOrigin>();
			Destination1.JB_JV = Voyage1.PK;
			Origin1.JA_JV = Voyage1.PK;
			Sailing1.JX_JA = Origin1.PK;
			Sailing1.JX_JB = Destination1.PK;
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(-30);
			Destination1.JB_E_ARV = ZDateTime.Now.AddDays(-27);
			Voyage1.JV_AirSeaRoad = TransportMode;

			Sailing2 = Factory.New<JobSailing>();
			Voyage2 = Factory.New<JobVoyage>();
			Destination2 = Factory.New<VoyageDestination>();
			Origin2 = Factory.New<VoyageOrigin>();
			Destination2.JB_JV = Voyage2.PK;
			Origin2.JA_JV = Voyage2.PK;
			Sailing2.JX_JA = Origin2.PK;
			Sailing2.JX_JB = Destination2.PK;
			Origin2.JA_E_DEP = ZDateTime.Now;
			Destination2.JB_E_ARV = ZDateTime.Now.AddDays(3);
			Voyage2.JV_AirSeaRoad = TransportMode;

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
			Voyage1.JV_OH_Line = ShipLine1.PK;

			ShipLine2 = Factory.New<OrgHeader>();
			ShipLine2.OH_Code = "SHIP2";
			ShipLine2.OH_IsShippingLine = ZBool.True;
			Voyage2.JV_OH_Line = ShipLine2.PK;

			AlternateTransportModeSailing = Factory.New<JobSailing>();
			AlternateTransportModeVoyage = Factory.New<JobVoyage>();
			AlternateTransportModeDestination = Factory.New<VoyageDestination>();
			AlternateTransportModeOrigin = Factory.New<VoyageOrigin>();
			AlternateTransportModeDestination.JB_JV = AlternateTransportModeVoyage.PK;
			AlternateTransportModeOrigin.JA_JV = AlternateTransportModeVoyage.PK;
			AlternateTransportModeSailing.JX_JA = AlternateTransportModeOrigin.PK;
			AlternateTransportModeSailing.JX_JB = AlternateTransportModeDestination.PK;
			AlternateTransportModeOrigin.JA_E_DEP = ZDateTime.Now.AddDays(-30);
			AlternateTransportModeDestination.JB_E_ARV = ZDateTime.Now.AddDays(-27);
			AlternateTransportModeVoyage.JV_AirSeaRoad = (TransportMode == Core.Constants.TransportModes.Air) ? Core.Constants.TransportModes.Road : Core.Constants.TransportModes.Air;

			Factory.Save();

			Filter = (SailingFilterBusinessObject)FilterFactory.New(GetExpectedBusinessObjectType());
			Filter.ResetToDefaultValues();
			Filter.JX_IsPublished = true;

			Collection = new JobSailingCollection(Factory);
		}

		#endregion
	}
}
