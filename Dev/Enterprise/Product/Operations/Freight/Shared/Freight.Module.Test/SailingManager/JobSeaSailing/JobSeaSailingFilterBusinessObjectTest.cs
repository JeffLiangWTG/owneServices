using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobSeaSailingFilterBusinessObject))]
	sealed class JobSeaSailingFilterBusinessObjectTest : JobSailingFilterBusinessObjectTest
	{
		#region IsArchivedFilter

		public void TestIsArchivedFilter()
		{
			var voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage1.JV_IsActive = true;
			voyage1.GenerateSailings();

			var voyage2 = Factory.NewWithValidTestData<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage2.JV_IsActive = false;
			voyage2.GenerateSailings();

			var sailing1 = voyage1.Sailings.First();
			var sailing2 = voyage2.Sailings.First();

			Factory.Save();

			var filterBO = new JobSeaSailingFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBO[JobSeaSailingFilterBusinessObject.SeaFilterTypes.IncludeArchived];
			filter.IsActive = true;
			filter.Property0 = false;

			sailings.Load(filter.Query);
			Assert("Non-archived sailing should show.", sailings.Contains(sailing1));
			Assert("Archived sailing should not show.", !sailings.Contains(sailing2));

			filter.Property0 = true;

			sailings.Load(filter.Query);
			Assert("Non-archived sailing should show.", sailings.Contains(sailing1));
			Assert("Archived sailing should show.", sailings.Contains(sailing2));
		}

		#endregion

		#region TestTradeLaneFilter

		public void TestTradeLaneFilter()
		{
			OrgHeader principal1 = Factory.New<OrgHeader>();
			principal1.OH_Code = "Principal1";
			principal1.OH_IsShippingProvider = true;
			principal1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			OrgHeader principal2 = Factory.New<OrgHeader>();
			principal2.OH_Code = "Principal2";
			principal2.OH_IsShippingProvider = true;
			principal2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();
			tradeLane1.EJ_Code = "FTL";
			tradeLane2.EJ_Code = "STL";
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

			JobSeaSailingFilterBusinessObject filter = new JobSeaSailingFilterBusinessObject();
			((ModuleGuidFilter)filter[JobSeaSailingFilterBusinessObject.SeaFilterTypes.TradeLane]).Property = ZGuid.Empty;
			((ModuleGuidFilter)filter[JobSeaSailingFilterBusinessObject.SeaFilterTypes.TradeLane]).IsActive = true;

			sailings.Load(filter.Filter);

			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 to be in collection", sailings.Contains(Sailing2.PK));

			((ModuleGuidFilter)filter[JobSeaSailingFilterBusinessObject.SeaFilterTypes.TradeLane]).Property = tradeLane1.PK;
			sailings.Load(filter.Filter);

			Assert("Expect sailing 1 to be in collection", sailings.Contains(Sailing1.PK));
			Assert("Expect sailing 2 not to be in collection", !sailings.Contains(Sailing2.PK));
		}

		#endregion

		#region TestVesselFilter

		public void TestVesselAndVoyageFilter()
		{
			var filterStrip = new JobSeaSailingFilterBusinessObject();

			((ModuleTextFilter)filterStrip["Status"]).Property = "ALL";
			((ModuleTextFilter)filterStrip["Status"]).IsActive = true;

			var filter = (VoyageVesselModuleFilter)filterStrip["Voyage # and Vessel"];
			filter.IsActive = true;
			filter.Property = "";
			filter.Vessel = "";
			filter.VoyageFlightNo = "";

			sailings.Load(filterStrip.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Sailing1, Sailing2 }, sailings);

			filter.Vessel = Voyage1.JV_RV_NKVessel;
			filter.VoyageFlightNo = Voyage1.JV_VoyageFlight;

			sailings.Load(filterStrip.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Sailing1 }, sailings);

			sailings.Load(filterStrip.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Sailing1 }, sailings);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = Voyage1.JV_VoyageFlight.Substring(2);

			sailings.Load(filterStrip.Filter);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<JobSailing>(), sailings);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			sailings.Load(filterStrip.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Sailing1 }, sailings);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobSeaSailingFilterBusinessObject();
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Sea; }
		}

		#endregion
	}
}
