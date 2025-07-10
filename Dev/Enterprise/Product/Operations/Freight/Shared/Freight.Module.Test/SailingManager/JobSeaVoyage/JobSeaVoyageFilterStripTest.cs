using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobSeaVoyageFilterStrip))]
	sealed class JobSeaVoyageFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestBaseFilter()
		{
			Voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			Voyage2.JV_AirSeaRoad = Constants.TransportModes.Air;
			Factory.Save();

			AssertMatches("base filter", FilterStrip.Filter, Voyage1);
		}

		public void TestTradeLaneFilter()
		{
			OrgHeader principal1 = Factory.New<OrgHeader>();
			OrgHeader principal2 = Factory.New<OrgHeader>();
			principal1.OH_Code = "Principal1";
			principal2.OH_Code = "Principal2";
			principal1.OH_IsShippingProvider = true;
			principal2.OH_IsShippingProvider = true;
			principal1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			principal2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();
			tradeLane1.EJ_Code = "STL";
			tradeLane2.EJ_Code = "ATL";
			tradeLane1.EJ_OH_RelatedOrg = principal1.PK;
			tradeLane2.EJ_OH_RelatedOrg = principal2.PK;
			JobTradeLaneVoyage tradeLaneVoyage1 = Factory.New<JobTradeLaneVoyage>();
			JobTradeLaneVoyage tradeLaneVoyage2 = Factory.New<JobTradeLaneVoyage>();
			tradeLaneVoyage1.NB_EJ = tradeLane1.PK;
			tradeLaneVoyage2.NB_EJ = tradeLane2.PK;
			tradeLaneVoyage1.NB_JV = Voyage1.PK;
			tradeLaneVoyage2.NB_JV = Voyage2.PK;
			tradeLaneVoyage1.NB_OH = principal1.PK;
			tradeLaneVoyage2.NB_OH = principal2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[JobSeaVoyageFilterStrip.Descriptions.TradeLane];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			AssertMatches("empty filter", FilterStrip.Filter, Voyage1, Voyage2);

			filter.Property = tradeLane1.PK;
			AssertMatches("Voyage1", FilterStrip.Filter, Voyage1);
		}

		public void TestVoyageVessel()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "VesselA";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "VesselB";
			var vessel3 = Factory.NewWithValidTestData<RefVessel>();
			vessel3.RV_Name = "VesselBlack";

			Voyage1.JV_VoyageFlight = "001";
			Voyage1.JV_RV_NKVessel = vessel1.RV_FK;

			Voyage2.JV_VoyageFlight = "001";
			Voyage2.JV_RV_NKVessel = vessel2.RV_FK;

			Voyage3.JV_VoyageFlight = "002";
			Voyage3.JV_RV_NKVessel = vessel3.RV_FK;

			Voyage4.JV_VoyageFlight = "";
			Voyage4.JV_RV_NKVessel = "";

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterStrip[JobSeaVoyageFilterStrip.Descriptions.VoyageVessel];

			filter.Property = "";
			filter.Vessel = "";
			AssertMatches("empty filter", filter.Query, Voyage1, Voyage2, Voyage3, Voyage4);

			filter.VoyageFlightNo = "001";
			AssertMatches("voyage", filter.Query, Voyage1, Voyage2);

			filter.Vessel = "VesselB";
			AssertMatches("voyage + vessel", filter.Query, Voyage2);

			filter.Property = "";
			AssertMatches("vessel", filter.Query, Voyage2, Voyage3);

			filter.VoyageFlightNo = "";
			filter.Vessel = "VesselBl";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertMatches("empty filter", filter.Query, Voyage3);

			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertMatches("empty filter", filter.Query, Voyage1, Voyage2, Voyage3);

			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertMatches("empty filter", filter.Query, Voyage4);
		}

		public void TestVoyageVessel_MaxLength()
		{
			var filter = (VoyageVesselModuleFilter)FilterStrip[JobSeaVoyageFilterStrip.Descriptions.VoyageVessel];
			AssertEquals(filter.MaxLength, JobVoyageSchema.JV_VoyageFlight.MaxLength);
		}

		public void TestCarrier()
		{
			OrgHeader carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			Voyage1.JV_OH_Line = carrier1.PK;
			Voyage2.JV_OH_Line = carrier2.PK;
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[JobSeaVoyageFilterStrip.Descriptions.Carrier];

			filter.Property = ZGuid.Empty;
			AssertMatches("empty", filter.Query, Voyage1, Voyage2);

			filter.Property = Voyage1.JV_OH_Line;
			AssertMatches("carrier1", filter.Query, Voyage1);
		}

		#region Implementation

		void AssertMatches(string message, ZQuery filter, params JobVoyage[] expectedVoyages)
		{
			ZQuery combinedFilter = new ZQuery();
			combinedFilter.AddToFilter(filter);
			combinedFilter.AddToFilter(JobVoyageSchema.PK, voyagePKList);

			AssertContainsExactElementsInAnyOrder(message,
				BusinessObjectEqualityComparer<JobVoyage>.IgnoreFactoryComparer,
				(v) => v.JV_SendersMessageReference,
				expectedVoyages,
				Factory.Load<JobVoyage>(combinedFilter));
		}

		JobVoyage Voyage1
		{
			get
			{
				if (voyage1 == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel1";

					voyage1 = Factory.New<JobVoyage>();
					voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
					voyage1.JV_RV_NKVessel = vessel.RV_FK;
					voyage1.JV_VoyageFlight = "Voyage1";
					voyage1.JV_SendersMessageReference = "Voyage1";
					voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyagePKList.Add(voyage1.PK);
				}
				return voyage1;
			}
		}
		JobVoyage voyage1;

		JobVoyage Voyage2
		{
			get
			{
				if (voyage2 == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel2";

					voyage2 = Factory.New<JobVoyage>();
					voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
					voyage2.JV_RV_NKVessel = vessel.RV_FK;
					voyage2.JV_VoyageFlight = "Voyage2";
					voyage2.JV_SendersMessageReference = "Voyage2";
					voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyagePKList.Add(voyage2.PK);
				}
				return voyage2;
			}
		}
		JobVoyage voyage2;

		JobVoyage Voyage3
		{
			get
			{
				if (voyage3 == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel3";

					voyage3 = Factory.New<JobVoyage>();
					voyage3.JV_AirSeaRoad = Constants.TransportModes.Sea;
					voyage3.JV_RV_NKVessel = vessel.RV_FK;
					voyage3.JV_VoyageFlight = "Voyage3";
					voyage3.JV_SendersMessageReference = "Voyage1";
					voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyagePKList.Add(voyage3.PK);
				}
				return voyage3;
			}
		}
		JobVoyage voyage3;

		JobVoyage Voyage4
		{
			get
			{
				if (voyage4 == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel4";

					voyage4 = Factory.New<JobVoyage>();
					voyage4.JV_AirSeaRoad = Constants.TransportModes.Sea;
					voyage4.JV_RV_NKVessel = vessel.RV_FK;
					voyage4.JV_VoyageFlight = "Voyage4";
					voyage4.JV_SendersMessageReference = "Voyage4";
					voyage4.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage4.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyagePKList.Add(voyage4.PK);
				}
				return voyage4;
			}
		}
		JobVoyage voyage4;

		readonly List<ZGuid> voyagePKList = new List<ZGuid>();

		JobSeaVoyageFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new JobSeaVoyageFilterStrip();
				}

				return filterStrip;
			}
		}
		JobSeaVoyageFilterStrip filterStrip;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobSeaVoyageFilterStrip();
		}

		#endregion
	}
}
