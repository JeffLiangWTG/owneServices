using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module.Organisation.CommissionAgreement;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgOpportunityFilterBusinessObject))]
	sealed class OrgOpportunityFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestOverallDispositionFilter()
		{
			var list = new OpportunityStatusCollection();

			list.Add("CRT", (NoResString)"Current", false);
			list.Add("LOS", (NoResString)"Lost", true);
			list.Add("ABD", (NoResString)"Abandoned", true);
			list.Add("SUS", (NoResString)"Suspended", false);
			list.Add("WON", (NoResString)"Won", true);
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var oppCRT = Factory.NewWithValidTestData<OrgOpportunity>();
			oppCRT.P8_Status = "CRT";
			var oppLOS = Factory.NewWithValidTestData<OrgOpportunity>();
			oppLOS.P8_Status = "LOS";
			var oppABD = Factory.NewWithValidTestData<OrgOpportunity>();
			oppABD.P8_Status = "ABD";
			var oppSUS = Factory.NewWithValidTestData<OrgOpportunity>();
			oppSUS.P8_Status = "SUS";
			var oppWON = Factory.NewWithValidTestData<OrgOpportunity>();
			oppWON.P8_Status = "WON";

			Factory.Save();

			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			filterBizObj["Overall Disposition"].IsActive = true;
			((ModuleTextFilter)filterBizObj["Overall Disposition"]).Property = "OPN";
			ZQuery query1 = new ZQuery();
			query1.AddToFilter(filterBizObj.Filter);

			OrgOpportunityCollection collection1 = new OrgOpportunityCollection(Factory);
			collection1.Load(query1);
			AssertCollectionContains(oppCRT, collection1);
			AssertCollectionContains(oppSUS, collection1);

			((ModuleTextFilter)filterBizObj["Overall Disposition"]).Property = "CLS";
			ZQuery query2 = new ZQuery();
			query2.AddToFilter(filterBizObj.Filter);

			OrgOpportunityCollection collection2 = new OrgOpportunityCollection(Factory);
			collection2.Load(query2);
			AssertCollectionContains(oppWON, collection2);
			AssertCollectionContains(oppABD, collection2);
			AssertCollectionContains(oppLOS, collection2);
		}

		public void TestCurrentFilter()
		{
			OrganisationsDataRegistry.Instance.CurrentLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CurrentABC");

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityID = "O00000001";
			opp1.P8_DiscountAmount = 0;
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OpportunityID = "O00000002";
			opp2.P8_DiscountAmount = 5;
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OpportunityID = "O00000003";
			opp3.P8_DiscountAmount = 10;

			Factory.Save();

			var opps = new OrgOpportunityCollection(Factory);
			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Current"].GetType());
			var currentFilter = (ModuleNumberRangeFilter)filterBizObj["Current"];
			AssertEquals("CurrentABC", currentFilter.MultilingualDescription.ToString());
			AssertEquals((byte)0, currentFilter.Decimals);
			currentFilter.IsActive = true;

			currentFilter.Property1 = 5;
			currentFilter.Property2 = 15;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(opp => opp.P8_OpportunityID, new[] { opp2, opp3 }, opps.Cast<OrgOpportunity>());

			currentFilter.Property1 = ZDecimal.Zero;
			currentFilter.Property2 = 5;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(opp => opp.P8_OpportunityID, new[] { opp1, opp2 }, opps.Cast<OrgOpportunity>());
		}

		public void TestPotentialFilter()
		{
			OrganisationsDataRegistry.Instance.PotentialLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PotentialDEF");
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityID = "O00000001";
			opp1.P8_RentalMultiplier = 0;
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OpportunityID = "O00000002";
			opp2.P8_RentalMultiplier = 5;
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OpportunityID = "O00000003";
			opp3.P8_RentalMultiplier = 10;

			Factory.Save();

			var opps = new OrgOpportunityCollection(Factory);
			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleNumberRangeFilter), filterBizObj["Potential"].GetType());
			var potentialFilter = (ModuleNumberRangeFilter)filterBizObj["Potential"];
			AssertEquals("PotentialDEF", potentialFilter.MultilingualDescription.ToString());
			AssertEquals((byte)0, potentialFilter.Decimals);
			potentialFilter.IsActive = true;

			potentialFilter.Property1 = 5;
			potentialFilter.Property2 = 15;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(opp => opp.P8_OpportunityID, new[] { opp2, opp3 }, opps.Cast<OrgOpportunity>());

			potentialFilter.Property1 = ZDecimal.Zero;
			potentialFilter.Property2 = 5;
			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(opp => opp.P8_OpportunityID, new[] { opp1, opp2 }, opps.Cast<OrgOpportunity>());
		}

		public void TestOpportunityIDFilter()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityID = "O00000001";
			opp1.P8_RentalMultiplier = 0;
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OpportunityID = "O00000012";
			opp2.P8_RentalMultiplier = 5;
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OpportunityID = "O00000013";
			opp3.P8_RentalMultiplier = 10;

			Factory.Save();

			var opps = new OrgOpportunityCollection(Factory);
			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			AssertEquals(typeof(ModuleFountainFilter), filterBizObj["Opportunity ID"].GetType());

			var opportunityIDFilter = (ModuleFountainFilter)filterBizObj["Opportunity ID"];
			AssertEquals(true, opportunityIDFilter.IsCommon);

			opportunityIDFilter.Property = "O0000001";
			opportunityIDFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			opportunityIDFilter.IsActive = true;

			opps.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(opp => opp.P8_OpportunityID, new[] { opp2, opp3 }, opps.Cast<OrgOpportunity>());
		}

		public void TestRegisteringCompanyFilter()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			var companyC = Factory.NewWithValidTestData<GlbCompany>();

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityID = "O0090001";
			opp1.P8_GC = companyA.PK;
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_GC = companyA.PK;
			opp2.P8_OpportunityID = "O0090002";
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_GC = companyB.PK;
			opp3.P8_OpportunityID = "O0090003";

			Factory.Save();

			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			AssertType(typeof(ModuleGuidFilter), filterBizObj["Company"]);

			var filter = (ModuleGuidFilter)filterBizObj["Company"];
			filter.IsActive = true;

			filter.Property = companyA.PK;
			AssertContainsExactElementsInAnyOrder(
				opp => opp.P8_OpportunityID,
				new[] { opp1, opp2 },
				Factory.Load<OrgOpportunity>(filter.Query));

			filter.Property = companyB.PK;
			AssertContainsExactElementsInAnyOrder(
				opp => opp.P8_OpportunityID,
				new[] { opp3 },
				Factory.Load<OrgOpportunity>(filter.Query));
		}

		public void TestLocationFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_RL_NKClosestPort = "USLAX";
			OrgOpportunity opp1 = org1.SalesOpportunities.AddNew();

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_RL_NKClosestPort = "AUSYD";
			OrgOpportunity opp2 = org2.SalesOpportunities.AddNew();

			Factory.Save();

			OrgOpportunityCollection opps = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			((ModuleNkFilter)filterBizo["Location"]).IsActive = true;
			((ModuleNkFilter)filterBizo["Location"]).Property = "US";
			opps.Load(filterBizo.Filter);
			AssertEquals(1, opps.Count);
			AssertEquals(opp1.PK, opps[0].PK);

			opps = new OrgOpportunityCollection(Factory);
			((ModuleNkFilter)filterBizo["Location"]).Property = "AU";
			opps.Load(filterBizo.Filter);
			AssertEquals(1, opps.Count);
			AssertEquals(opp2.PK, opps[0].PK);

			opps = new OrgOpportunityCollection(Factory);
			((ModuleNkFilter)filterBizo["Location"]).Property = "";
			opps.Load(filterBizo.Filter);
			AssertEquals(2, opps.Count);
		}

		public void TestLocationFilter_LoginCountryRestriction()
		{
			OrgOpportunityFilterBusinessObject filterBizO = new OrgOpportunityFilterBusinessObject();
			ModuleNkFilter locationFilter = (ModuleNkFilter)filterBizO["Location"];
			AssertEquals("Default visibility", FilterVisibility.Visible, locationFilter.Visibility);
			AssertEquals("", locationFilter.DefaultProperty);
			AssertNull("PropertyValidation should be null if search is not restricted", locationFilter.PropertyValidation);

			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = false;
			filterBizO = new OrgOpportunityFilterBusinessObject();
			locationFilter = (ModuleNkFilter)filterBizO["Location"];
			AssertEquals(FilterVisibility.AlwaysVisible, locationFilter.Visibility);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, locationFilter.DefaultProperty);
			AssertNotNull(locationFilter.PropertyValidation);
			AssertNoErrors(locationFilter.PropertyInfo);

			locationFilter.Property = "AUBNE";
			AssertNoErrors(locationFilter.PropertyInfo);

			locationFilter.Property = "JPTYO";
			string expectedError = @"Your current security rights only allow you to view opportunities relating to organizations based in your current login country/region (AU).
If you think this is incorrect, please contact your system administrator.";
			AssertHasError(locationFilter.PropertyInfo, expectedError);

			locationFilter.Property = "ID";
			AssertHasError(locationFilter.PropertyInfo, expectedError);

			locationFilter.Property = "AUSYD";
			AssertNoErrors(locationFilter.PropertyInfo);
		}

		public void TestSourceDetailsFilter()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			opportunity1.P8_SourceDetails = "UTS";
			var opportunity2 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			opportunity2.P8_SourceDetails = "UWS";
			var opportunity3 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			opportunity3.P8_SourceDetails = "UNSW";
			var opportunity4 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			opportunity4.P8_SourceDetails = "TAFE";

			Factory.Save();

			var collection = new OrgOpportunityCollection(Factory);
			var filterBizo = new OrgOpportunityFilterBusinessObject();
			var sourceDetailsFilter = (ModuleTextFilter)filterBizo["Source Details"];

			sourceDetailsFilter.IsActive = true;
			sourceDetailsFilter.Property = "";
			collection.Load(filterBizo.Filter);
			AssertEquals(4, collection.Count);

			sourceDetailsFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			sourceDetailsFilter.Property = "W";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opportunity2, collection);
			AssertCollectionContains(opportunity3, collection);

			sourceDetailsFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			sourceDetailsFilter.Property = "U";
			collection.Load(filterBizo.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(opportunity1, collection);
			AssertCollectionContains(opportunity2, collection);
			AssertCollectionContains(opportunity3, collection);

			sourceDetailsFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opportunity4, collection);
		}

		public void TestRelatedTradeLanesFilter()
		{
			OrgOpportunity opportunity1 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			LinkOpportunityWithTradeLane(opportunity1, "AUSYD", "USLAX", Core.Constants.Sales.Mode.Export);
			LinkOpportunityWithTradeLane(opportunity1, "AUMEL", "USCHI", Core.Constants.Sales.Mode.Import);

			OrgOpportunity opportunity2 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			LinkOpportunityWithTradeLane(opportunity2, "AUSYD", "DEHAM", Core.Constants.Sales.Mode.Import);

			OrgOpportunity opportunity3 = Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew();
			LinkOpportunityWithTradeLane(opportunity3, "AUBNE", "USLAX", Core.Constants.Sales.Mode.Export);

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();
			ModuleTextFilter directionFilter = (ModuleTextFilter)filterBizo["Sales Monthly Trade Mode"];

			directionFilter.IsActive = true;
			directionFilter.Property = "";
			collection.Load(filterBizo.Filter);
			AssertEquals(3, collection.Count);

			directionFilter.Property = Core.Constants.Sales.Mode.Export;
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(opportunity1));
			AssertEquals(true, collection.Contains(opportunity3));

			directionFilter.Property = Core.Constants.Sales.Mode.Import;
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(opportunity1));
			AssertEquals(true, collection.Contains(opportunity2));

			filterBizo = new OrgOpportunityFilterBusinessObject();
			ModuleLocationFilter locationFilter = (ModuleLocationFilter)filterBizo["Sales Trade Lane Origin / Destination"];
			locationFilter.IsActive = true;
			locationFilter.Property1 = "";
			locationFilter.Property2 = "";
			collection.Load(filterBizo.Filter);
			AssertEquals(3, collection.Count);

			locationFilter.Property1 = "AUSYD";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(opportunity1));
			AssertEquals(true, collection.Contains(opportunity2));

			locationFilter.Property1 = "AUBNE";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(true, collection.Contains(opportunity3));

			locationFilter.Property1 = "";
			locationFilter.Property2 = "USLAX";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(opportunity1));
			AssertEquals(true, collection.Contains(opportunity3));

			locationFilter.Property1 = "AUSYD";
			locationFilter.Property2 = "DEHAM";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(true, collection.Contains(opportunity2));
		}

		void LinkOpportunityWithTradeLane(OrgOpportunity opportunity, string originCode, string destinationCode, string mode)
		{
			var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, originCode);
			var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, destinationCode);

			OrgSales tradeLane = opportunity.Header.SalesCollection.AddNew();
			tradeLane.OW_OriginID = origin.PK;
			tradeLane.OW_DestinationID = destination.PK;

			if (mode == Core.Constants.Sales.Mode.Export)
			{
				tradeLane.OW_OH_Supplier = opportunity.Header.PK;
			}
			else if (mode == Core.Constants.Sales.Mode.Import)
			{
				tradeLane.OW_OH_Buyer = opportunity.Header.PK;
			}

			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane);
			foreach (var pair in opportunity.TradeProfileDescriptionList)
			{
				pair.Value = true;
			}
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestEstimatedCloseDateFilter()
		{
			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_EstimatedCloseDate = new ZDateTime(2010, 11, 26);
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_EstimatedCloseDate = new ZDateTime(2010, 10, 15);
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_EstimatedCloseDate = new ZDateTime(2010, 11, 14);

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleDateFilter filter = (ModuleDateFilter)filterBizo["Estimated Close Date"];

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2010, 11, 26, 10, 0, 0);
			filter.Property2 = new ZDateTime(2010, 11, 26, 10, 0, 0);
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);

			filter.Property1 = new ZDateTime(2010, 11, 1);
			filter.Property2 = new ZDateTime(2010, 12, 1);
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp3, collection);
		}

		public void TestCloseCertaintyFilter()
		{
			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_CloseCertainty = new ZByte(50);
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_CloseCertainty = new ZByte(80);
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_CloseCertainty = new ZByte(25);

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)filterBizo["Close Certainty"];

			filter.IsActive = true;
			filter.Property1 = 25;
			filter.Property2 = 25;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp3, collection);

			filter.Property1 = 0;
			filter.Property2 = 50;
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp3, collection);

			filter.IsActive = false;
		}

		public void TestProductTypeFilter()
		{
			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_PackageType = "AAA";
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_PackageType = "BBB";
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_PackageType = "CCC";

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleTextFilter filter = (ModuleTextFilter)filterBizo["Product Type"];

			filter.IsActive = true;
			filter.Property = "BBB";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp2, collection);

			filter.Property = "AAA";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);
		}

		public void TestProductTypeFilter_Description()
		{
			var filterBizo1 = new OrgOpportunityFilterBusinessObject();
			AssertEquals(OrganisationsDataRegistry.Instance.ProductTypeLabel.DefaultValue, filterBizo1["Product Type"].MultilingualDescription);

			OrganisationsDataRegistry.Instance.ProductTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Product Type");
			var filterBizo2 = new OrgOpportunityFilterBusinessObject();
			AssertEquals("My Product Type", filterBizo2["Product Type"].MultilingualDescription);
		}

		public void TestEstimatedValueFilter()
		{
			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_EstimatedValue = 50m;
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_EstimatedValue = 100m;
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_EstimatedValue = 120m;

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)filterBizo["Total Estimated Value (p.a)"];

			filter.IsActive = true;
			filter.Property1 = 50m;
			filter.Property2 = 100m;
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);

			filter.Property1 = 120m;
			filter.Property2 = 120m;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp3, collection);
		}

		public void TestAssignedOfficeFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_Address1 = "some address 1";
			orgAddress1.OA_OH = org1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_Address1 = "some address 2";
			orgAddress2.OA_OH = org2.PK;

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OA_AssignedOffice = orgAddress1.PK;

			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OA_AssignedOffice = orgAddress2.PK;

			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizo["Assigned Office"];

			filter.IsActive = true;
			filter.Property = org1.PK;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertEquals(orgAddress1.OA_Address1, opp1.AssignedOffice.OA_Address1);

			filter.Property = org2.PK;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp2, collection);
			AssertEquals(orgAddress2.OA_Address1, opp2.AssignedOffice.OA_Address1);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp3, collection);
			AssertEquals(Guid.Empty, opp3.P8_OA_AssignedOffice);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);
		}

		public void TestValueTypeFilter()
		{
			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			OrgOpportunityValue value11 = opp1.ValueItems.AddNew();
			value11.PV_RevenueType = "AA1";

			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			OrgOpportunityValue value21 = opp2.ValueItems.AddNew();
			value21.PV_RevenueType = "AA1";
			OrgOpportunityValue value22 = opp2.ValueItems.AddNew();
			value22.PV_RevenueType = "BA2";

			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			OrgOpportunityValue value31 = opp3.ValueItems.AddNew();
			value31.PV_RevenueType = "CA1";
			OrgOpportunityValue value32 = opp3.ValueItems.AddNew();
			value32.PV_RevenueType = "BA2";

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleTextFilter filter = (ModuleTextFilter)filterBizo["Value Type"];

			filter.IsActive = true;
			filter.Property = "AA1";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);

			filter.Property = "BA2";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp2, collection);
			AssertCollectionContains(opp3, collection);

			filter.Property = "CA1";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp3, collection);
		}

		public void TestReferringOrgFilter()
		{
			OrgHeader sourceOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader sourceOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH_ReferringOrganisation = sourceOrg1.PK;
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH_ReferringOrganisation = sourceOrg2.PK;

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizo["Referring Organization"];
			filter.Property = sourceOrg1.PK;
			filter.IsActive = true;

			collection.Load(filterBizo.Filter);
			Assert("Expect collection to contain opp1", collection.Contains(opp1));
			Assert("Expect collection to not contain opp2", !collection.Contains(opp2));

			filter.Property = sourceOrg2.PK;
			collection.Load(filterBizo.Filter);
			Assert("Expect collection not to contain opp1", !collection.Contains(opp1));
			Assert("Expect collection to contain opp2", collection.Contains(opp2));
		}

		public void TestClientSizeFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_CMClientSize = "GOL";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MiscServ.OM_CMClientSize = "PLA";

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org1.PK;
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org2.PK;

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleTextFilter filter = (ModuleTextFilter)filterBizo["Client Size"];

			filter.IsActive = true;
			filter.Property = "PLA";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp2, collection);

			filter.Property = "GOL";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);
		}

		public void TestTradeProductFilter()
		{
			IOrgSalesProduct forwarding = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			IOrgSalesProduct brokerage = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			IOrgSalesProduct transport = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales1 = org1.SalesCollection.AddNew();
			sales1.OW_MP_Product = forwarding.Identifier;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales2 = org2.SalesCollection.AddNew();
			sales2.OW_MP_Product = brokerage.Identifier;

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales3 = org3.SalesCollection.AddNew();
			sales3.OW_MP_Product = transport.Identifier;

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org1.PK;
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org2.PK;
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales3);
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OH = org3.PK;
			opp3.AssociatedTradeLanesPivots.AddPivotFor(sales3);

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleTextFilter filter = (ModuleTextFilter)filterBizo["Sales Trade Lane Product"];

			filter.IsActive = true;
			filter.Property = SystemDefinedSalesProductList.Codes.ForwardingShipment;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);

			filter.Property = SystemDefinedSalesProductList.Codes.CustomsBrokerage;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp2, collection);

			filter.Property = SystemDefinedSalesProductList.Codes.Transport;
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp2, collection);
			AssertCollectionContains(opp3, collection);
		}

		public void TestTradeStatusFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales1 = org1.SalesCollection.AddNew();
			OrgTradeDetail trade11 = sales1.TradeDetails.AddNew();
			trade11.PA_Status = "NON";
			OrgTradeDetail trade12 = sales1.TradeDetails.AddNew();
			trade12.PA_Status = "SHP";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales2 = org2.SalesCollection.AddNew();
			OrgTradeDetail trade21 = sales2.TradeDetails.AddNew();
			trade21.PA_Status = "SHP";

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales3 = org3.SalesCollection.AddNew();
			OrgTradeDetail trade31 = sales3.TradeDetails.AddNew();
			trade31.PA_Status = "QTE";

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org1.PK;
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org2.PK;
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OH = org3.PK;
			opp3.AssociatedTradeLanesPivots.AddPivotFor(sales3);

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleTextFilter filter = (ModuleTextFilter)filterBizo["Sales Trade Lane Status"];

			filter.IsActive = true;
			filter.Property = "NON";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);

			filter.Property = "SHP";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);

			filter.Property = "QTE";
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp3, collection);
		}

		public void TestTradeCommodityFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales1 = org1.SalesCollection.AddNew();
			OrgTradeDetail trade11 = sales1.TradeDetails.AddNew();
			trade11.ProspectDetail.PAP_RH_NKCommodityCode = "ALUM";
			OrgTradeDetail trade12 = sales1.TradeDetails.AddNew();
			trade12.ProspectDetail.PAP_RH_NKCommodityCode = "CHBF";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales2 = org2.SalesCollection.AddNew();
			OrgTradeDetail trade21 = sales2.TradeDetails.AddNew();
			trade21.ProspectDetail.PAP_RH_NKCommodityCode = "CHBF";

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgSales sales3 = org3.SalesCollection.AddNew();
			OrgTradeDetail trade31 = sales3.TradeDetails.AddNew();
			trade31.ProspectDetail.PAP_RH_NKCommodityCode = "ALUM";

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org1.PK;
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org2.PK;
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OH = org3.PK;
			opp3.AssociatedTradeLanesPivots.AddPivotFor(sales3);

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleNkFilter filter = (ModuleNkFilter)filterBizo["Sales Trade Lane Commodity"];

			filter.IsActive = true;
			filter.Property = "ALUM";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp3, collection);

			filter.Property = "CHBF";
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);

			filter.Property = "ABBT";
			collection.Load(filterBizo.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestTradeLaneFilterSubGroup()
		{
			var forwarding = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var brokerage = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var transport = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var uschi = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USCHI");
			var deham = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEHAM");
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			OrgSales sales11 = org1.SalesCollection.AddNew();
			sales11.OW_MP_Product = forwarding.Identifier;
			sales11.OW_OriginID = ausyd.PK;
			sales11.OW_DestinationID = uslax.PK;
			sales11.OW_OH_Buyer = org1.PK;
			OrgTradeDetail trade11 = sales11.TradeDetails.AddNew();
			trade11.PA_Status = "CNF";
			OrgTradeDetail trade12 = sales11.TradeDetails.AddNew();
			OrgSales sales12 = org1.SalesCollection.AddNew();
			sales12.OW_MP_Product = brokerage.Identifier;
			sales12.OW_OriginID = aumel.PK;
			sales12.OW_DestinationID = uschi.PK;
			sales12.OW_OH_Supplier = org1.PK;
			OrgTradeDetail trade13 = sales12.TradeDetails.AddNew();
			trade13.PA_Status = "QTE";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = true;
			OrgSales sales2 = org2.SalesCollection.AddNew();
			sales2.OW_MP_Product = forwarding.Identifier;
			sales2.OW_OriginID = ausyd.PK;
			sales2.OW_DestinationID = deham.PK;
			sales2.OW_OH_Supplier = org2.PK;
			OrgTradeDetail trade21 = sales2.TradeDetails.AddNew();
			trade21.PA_Status = "QTE";
			OrgTradeDetail trade22 = sales2.TradeDetails.AddNew();
			OrgTradeDetail trade23 = sales2.TradeDetails.AddNew();

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			OrgSales sales3 = org3.SalesCollection.AddNew();
			sales3.OW_MP_Product = transport.Identifier;
			sales3.OW_OriginID = aubne.PK;
			sales3.OW_DestinationID = uslax.PK;
			sales3.OW_OH_Supplier = org3.PK;
			OrgTradeDetail trade31 = sales3.TradeDetails.AddNew();
			trade31.PA_Status = "CNF";

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org1.PK;
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales11);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales12);
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org2.PK;
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OH = org3.PK;
			opp3.AssociatedTradeLanesPivots.AddPivotFor(sales3);

			Factory.Save();

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleTextFilter productfilter = (ModuleTextFilter)filterBizo["Sales Trade Lane Product"];
			ModuleTextFilter directionFilter = (ModuleTextFilter)filterBizo["Sales Monthly Trade Mode"];
			ModuleLocationFilter locationFilter = (ModuleLocationFilter)filterBizo["Sales Trade Lane Origin / Destination"];
			ModuleTextFilter statusfilter = (ModuleTextFilter)filterBizo["Sales Trade Lane Status"];

			directionFilter.IsActive = true;
			locationFilter.IsActive = false;
			productfilter.IsActive = true;
			statusfilter.IsActive = false;

			productfilter.Property = SystemDefinedSalesProductList.Codes.ForwardingShipment;
			directionFilter.Property = Core.Constants.Sales.Mode.Export;
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Export, Product: SHP", 1, collection.Count);
			AssertCollectionContains(opp2, collection);

			directionFilter.Property = Core.Constants.Sales.Mode.Import;
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP", 1, collection.Count);
			AssertCollectionContains(opp1, collection);

			locationFilter.IsActive = true;
			locationFilter.Property1 = "AUSYD";
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP, Origin: AUSYD", 1, collection.Count);
			AssertCollectionContains(opp1, collection);

			locationFilter.Property2 = "USLAX";
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP, Origin: AUSYD, Dest: USLAX", 1, collection.Count);
			AssertCollectionContains(opp1, collection);

			directionFilter.IsActive = true;
			locationFilter.IsActive = true;
			productfilter.IsActive = false;

			directionFilter.Property = Core.Constants.Sales.Mode.Import;
			locationFilter.Property1 = "AUMEL";
			locationFilter.Property2 = "USCHI";
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Import, Origin: AUMEL, Dest: USCHI", 0, collection.Count);

			productfilter.IsActive = true;
			directionFilter.Property = Core.Constants.Sales.Mode.Export;
			productfilter.Property = SystemDefinedSalesProductList.Codes.CustomsBrokerage;
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Export, Product: BRK, Origin: AUMEL, Dest: USCHI", 1, collection.Count);
			AssertCollectionContains(opp1, collection);

			statusfilter.IsActive = true;
			statusfilter.Property = "QTE";
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Export, Product: BRK, Origin: AUMEL, Dest: USCHI, Status: QTE", 1, collection.Count);
			AssertCollectionContains(opp1, collection);

			statusfilter.Property = "NON";
			collection.Load(filterBizo.Filter);
			AssertEquals("Trade Mode: Export, Product: BRK, Origin: AUMEL, Dest: USCHI, Status: NON", 0, collection.Count);

			directionFilter.IsActive = false;
			locationFilter.IsActive = true;
			productfilter.IsActive = false;
			statusfilter.IsActive = true;
			locationFilter.Property1 = "AUSYD";
			locationFilter.Property2 = "";
			statusfilter.Property = "QTE";
			collection.Load(filterBizo.Filter);
			AssertEquals("Origin: AUSYD, Status: QTE", 1, collection.Count);
			AssertCollectionContains(opp2, collection);
		}

		public void TestLastQuotedDateFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			BusinessObject quote1a = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote1a[RatingHeaderSchema.TH_QuoteNumber] = "00221000";
			quote1a[RatingHeaderSchema.TH_OH] = org1.PK;
			quote1a[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 5, 1);
			BusinessObject quote1b = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote1b[RatingHeaderSchema.TH_QuoteNumber] = "00221001";
			quote1b[RatingHeaderSchema.TH_OH] = org1.PK;
			quote1b[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 10, 9);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			BusinessObject quote2a = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote2a[RatingHeaderSchema.TH_QuoteNumber] = "00221002";
			quote2a[RatingHeaderSchema.TH_OH] = org2.PK;
			quote2a[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 11, 1);

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			BusinessObject quote3a = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote3a[RatingHeaderSchema.TH_QuoteNumber] = "00221003";
			quote3a[RatingHeaderSchema.TH_OH] = org3.PK;
			quote3a[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 11, 30);

			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org1.PK;
			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org2.PK;
			OrgOpportunity opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_OH = org3.PK;
			OrgOpportunity opp4 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp4.P8_OH = org4.PK;

			Factory.Save();

			AssertEquals(new ZDateTime(2011, 10, 9), opp1.Header.LastQuotedDate);
			AssertEquals(new ZDateTime(2011, 11, 1), opp2.Header.LastQuotedDate);
			AssertEquals(new ZDateTime(2011, 11, 30), opp3.Header.LastQuotedDate);
			AssertEquals(ZDateTime.Empty, opp4.Header.LastQuotedDate);

			OrgOpportunityCollection collection = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterBizo = new OrgOpportunityFilterBusinessObject();

			ModuleDateFilter filter = (ModuleDateFilter)filterBizo["Last Quoted Date"];
			filter.IsActive = true;

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 5, 1);
			filter.Property2 = new ZDateTime(2011, 5, 1);
			collection.Load(filterBizo.Filter);
			AssertEquals(0, collection.Count);

			filter.Property1 = new ZDateTime(2011, 10, 9);
			filter.Property2 = new ZDateTime(2011, 10, 9);
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);

			filter.Property1 = new ZDateTime(2011, 11, 1);
			filter.Property2 = new ZDateTime(2011, 11, 30);
			collection.Load(filterBizo.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp2, collection);
			AssertCollectionContains(opp3, collection);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			collection.Load(filterBizo.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);
			AssertCollectionContains(opp3, collection);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.Load(filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp4, collection);
		}

		public void TestSalesTeam()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();

			GlbStaff staffA = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staffB = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staffC = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staffD = Factory.NewWithValidTestData<GlbStaff>();

			GlbGroup teamX = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup teamY = Factory.NewWithValidTestData<GlbGroup>();

			GlbGroupLink linkStaffA = Factory.NewWithValidTestData<GlbGroupLink>();
			GlbGroupLink linkStaffB = Factory.NewWithValidTestData<GlbGroupLink>();
			GlbGroupLink linkStaffC = Factory.NewWithValidTestData<GlbGroupLink>();

			staffA.GS_Code = "AAA";
			staffB.GS_Code = "BBB";
			staffC.GS_Code = "CCC";
			staffD.GS_Code = "DDD";

			teamX.GG_Code = "XXX";
			teamX.GG_Desc = "Sales team X";
			teamX.GG_IsSales = true;
			teamY.GG_Code = "YYY";
			teamY.GG_Desc = "Sales team Y";
			teamY.GG_IsSales = true;

			linkStaffA.GK_GS = staffA.PK;
			linkStaffA.GK_GG = teamX.PK;

			linkStaffB.GK_GS = staffB.PK;
			linkStaffB.GK_GG = teamX.PK;

			linkStaffC.GK_GS = staffC.PK;
			linkStaffC.GK_GG = teamY.PK;

			OrgOpportunity opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_OH = org1.PK;
			opportunity1.P8_GS_NKPrimarySalesPerson = staffA.GS_Code;

			OrgOpportunity opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OH = org1.PK;
			opportunity2.P8_GS_NKPrimarySalesPerson = staffB.GS_Code;

			OrgOpportunity opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity3.P8_OH = org1.PK;
			opportunity3.P8_GS_NKPrimarySalesPerson = staffC.GS_Code;

			OrgOpportunity opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity4.P8_OH = org1.PK;

			OrgOpportunity opportunity5 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity5.P8_OH = org1.PK;
			opportunity5.P8_GS_NKPrimarySalesPerson = staffD.GS_Code;
			Factory.Save();

			OrgOpportunityCollection opportunities = new OrgOpportunityCollection(Factory);
			OrgOpportunityFilterBusinessObject filterOpportunity = new OrgOpportunityFilterBusinessObject();
			opportunities.Load();
			OrgOpportunity one = opportunities[0];
			ModuleNkFilter filter = (ModuleNkFilter)filterOpportunity["Sales Team"];

			filter.IsActive = true;
			filter.Property = "GGG";
			opportunities.Load(filterOpportunity.Filter);
			AssertEquals(0, opportunities.Count);

			filter.Property = "XXX";
			opportunities.Load(filterOpportunity.Filter);
			AssertEquals(2, opportunities.Count);
			AssertCollectionContains(opportunity1, opportunities);
			AssertCollectionContains(opportunity2, opportunities);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			opportunities.Load(filterOpportunity.Filter);
			AssertEquals(2, opportunities.Count);
			AssertCollectionContains(opportunity4, opportunities);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			opportunities.Load(filterOpportunity.Filter);
			AssertEquals(3, opportunities.Count);
			AssertCollectionNotContains(opportunity4, opportunities);
		}

		public void TestCreatedFromInquiry()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			SalesEnquiry inquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			OrgOpportunity opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org.PK;
			opp1.P8_O1_Enquiry = inquiry.PK;

			OrgOpportunity opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org.PK;

			Factory.Save();

			var filters = new OrgOpportunityFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filters["Created From Inquiry"];
			filter.IsActive = true;
			filter.Property0 = true;

			var collection = new OrgOpportunityCollection(Factory);
			collection.Load(filters.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(opp1, collection);

			filter.Property0 = false;
			collection.Load(filters.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(opp1, collection);
			AssertCollectionContains(opp2, collection);
		}

		public void TestCommissionAgreementsModuleFilter_Security()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			var strip = filterBizObj.FilterStrips.AddNew("Commission Agreements");
			var filter = (CommissionAgreementsModuleFilter)strip.CurrentModuleFilter;
			AssertNotNull(filter);

			var oldValue = Env.Security.CommissionAgreementView.IsAllowed;

			try
			{
				// CASE A: no filtering, allowed

				Env.Security.CommissionAgreementView.IsAllowed = true;
				AssertNoErrors("Case A", filter.SelectedFiltersDescriptionInfo);

				// CASE B: no filtering, not allowed

				Env.Security.CommissionAgreementView.IsAllowed = false;
				AssertNoErrors("Case B", filter.SelectedFiltersDescriptionInfo);

				// CASE C: filtering is set, not allowed

				var nestedFilter = filter.SelectedFilters.AddGuidFilterStrip(CommissionAgreementFilterBusinessObject.FilterDescription.PartyIsRecipient);
				filter.Validation.ValidateSelectedFiltersDescription();

				AssertNotNull(nestedFilter);
				AssertEquals("Organization in Wolf Pack", nestedFilter.MultilingualDescription);
				AssertHasError("Case C", filter.SelectedFiltersDescriptionInfo, Env.Security.CommissionAgreementView.ErrorMessageForNotAllowed);

				// CASE C2: two filter strips, not allowed

				var strip2 = filterBizObj.FilterStrips.AddNew("Commission Agreements");
				var filter2 = (CommissionAgreementsModuleFilter)strip2.CurrentModuleFilter;

				var nestedFilter2 = filter2.SelectedFilters.AddGuidFilterStrip(CommissionAgreementFilterBusinessObject.FilterDescription.PartyIsRecipient);
				filter2.Validation.ValidateSelectedFiltersDescription();

				AssertNotNull(nestedFilter2);
				AssertEquals("Organization in Wolf Pack", nestedFilter2.MultilingualDescription);
				AssertHasError("Case C2 - first strip", filter.SelectedFiltersDescriptionInfo, Env.Security.CommissionAgreementView.ErrorMessageForNotAllowed);
				AssertHasError("Case C2 - second strip", filter2.SelectedFiltersDescriptionInfo, Env.Security.CommissionAgreementView.ErrorMessageForNotAllowed);

				// CASE D: filtering is set, allowed

				Env.Security.CommissionAgreementView.IsAllowed = true;
				filter.Validation.ValidateSelectedFiltersDescription();
				AssertNoErrors("Case D", filter.SelectedFiltersDescriptionInfo);
			}
			finally
			{
				// restoring the previous rights
				Env.Security.CommissionAgreementView.IsAllowed = oldValue;
			}
		}

		public void TestOrganisationEntityInWolfPackFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var proxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var proxyCompany = Factory.NewWithValidTestData<GlbCompany>();
			proxyCompany.GC_OH_OrgProxy = proxyOrg.PK;
			var proxyBranch = Factory.NewWithValidTestData<GlbBranch>();
			proxyBranch.GB_GC = proxyCompany.PK;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = proxyBranch.PK;

			var oppA = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementA = oppA.CommissionAgreements.AddNew();
			agreementA.FillWithValidTestData();
			var recipientA1 = agreementA.Recipients.AddNew();
			recipientA1.CAR_OH_Party = org1.PK;
			var recipientA2 = agreementA.Recipients.AddNew();
			recipientA2.CAR_OH_Party = org2.PK;

			var oppB = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementB = oppB.CommissionAgreements.AddNew();
			agreementB.FillWithValidTestData();
			var recipientB1 = agreementB.Recipients.AddNew();
			recipientB1.CAR_OH_Party = org1.PK;

			var oppC = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementC = oppC.CommissionAgreements.AddNew();
			agreementC.FillWithValidTestData();
			var recipientC3 = agreementC.Recipients.AddNew();
			recipientC3.CAR_GS_NKStaff = staff.GS_Code;

			Factory.Save();

			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			var filter = (CommissionAgreementsModuleFilter)filterBizObj["Commission Agreements"];
			AssertNotNull(filter);
			AssertEquals("Commission Agreements", filter.MultilingualDescription);
			AssertEquals("Commission Agreement", filter.Category.Description);
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var nestedFilter = filter.SelectedFilters.AddGuidFilterStrip(CommissionAgreementFilterBusinessObject.FilterDescription.PartyIsRecipient);

			AssertNotNull(nestedFilter);
			AssertEquals("Organization in Wolf Pack", nestedFilter.MultilingualDescription);

			//org1

			nestedFilter.Property = org1.PK;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementA,
					agreementB,
				},
				Factory.Load<OrgCommissionAgreement>(filter.SelectedFilters.Filter));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					oppA,
					oppB,
				},
				Factory.Load<OrgOpportunity>(filterBizObj.Filter));

			//org2

			nestedFilter.Property = org2.PK;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementA,
				},
				Factory.Load<OrgCommissionAgreement>(filter.SelectedFilters.Filter));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					oppA,
				},
				Factory.Load<OrgOpportunity>(filterBizObj.Filter));

			//proxyOrg

			nestedFilter.Property = proxyOrg.PK;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementC,
				},
				Factory.Load<OrgCommissionAgreement>(filter.SelectedFilters.Filter));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					oppC,
				},
				Factory.Load<OrgOpportunity>(filterBizObj.Filter));
		}

		public void TestStaffEntityInWolfPackFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var oppA = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementA = oppA.CommissionAgreements.AddNew();
			agreementA.FillWithValidTestData();
			var recipientA1 = agreementA.Recipients.AddNew();
			recipientA1.CAR_GS_NKStaff = staff1.GS_Code;
			var recipientA2 = agreementA.Recipients.AddNew();
			recipientA2.CAR_GS_NKStaff = staff2.GS_Code;

			var oppB = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementB = oppB.CommissionAgreements.AddNew();
			agreementB.FillWithValidTestData();
			var recipientB1 = agreementB.Recipients.AddNew();
			recipientB1.CAR_GS_NKStaff = staff1.GS_Code;

			var oppC = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementC = oppC.CommissionAgreements.AddNew();
			agreementC.FillWithValidTestData();
			var recipientC3 = agreementC.Recipients.AddNew();
			recipientC3.CAR_GS_NKStaff = staff3.GS_Code;

			Factory.Save();

			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			var filter = (CommissionAgreementsModuleFilter)filterBizObj["Commission Agreements"];
			AssertNotNull(filter);
			AssertEquals("Commission Agreements", filter.MultilingualDescription);
			AssertEquals("Commission Agreement", filter.Category.Description);
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var nestedFilter = filter.SelectedFilters.AddNkFilterStrip(CommissionAgreementFilterBusinessObject.FilterDescription.StaffIsRecipient);
			AssertNotNull(nestedFilter);
			AssertEquals("Staff in Wolf Pack", nestedFilter.MultilingualDescription);
			nestedFilter.IsActive = true;

			//staff1

			nestedFilter.Property = staff1.GS_Code;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementA,
					agreementB
				},
				Factory.Load<OrgCommissionAgreement>(filter.SelectedFilters.Filter));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					oppA,
					oppB
				},
				Factory.Load<OrgOpportunity>(filterBizObj.Filter));

			//staff2

			nestedFilter.Property = staff2.GS_Code;
			Thread.Sleep(100);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementA
				},
				Factory.Load<OrgCommissionAgreement>(filter.SelectedFilters.Filter));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					oppA
				},
				Factory.Load<OrgOpportunity>(filterBizObj.Filter));

			//staff3

			nestedFilter.Property = staff3.GS_Code;
			Thread.Sleep(100);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementC
				},
				Factory.Load<OrgCommissionAgreement>(filter.SelectedFilters.Filter));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					oppC
				},
				Factory.Load<OrgOpportunity>(filterBizObj.Filter));
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new OrgOpportunityFilterBusinessObject().ModuleFilters;

			AssertNull(collection["Custom1 String"]);
			AssertNull(collection["Custom1 Integer"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["Custom2 Boolean"]);
			AssertNull(collection["Custom2 Datetime"]);
			AssertNull(collection["Unrelated Custom String"]);

			PrepareTemplatesWithCustomFields();

			collection = new OrgOpportunityFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), collection["Custom1 String"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), collection["Custom1 Integer"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), collection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), collection["Custom2 Boolean"].GetType());
			AssertEquals(typeof(ModuleDateFilter), collection["Custom2 Datetime"].GetType());
			AssertNull(collection["Unrelated Custom String"]);
		}

		void PrepareTemplatesWithCustomFields()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = OpportunityWorkflowDescriptor.WorkflowTypeCode;

			var template1Definition1 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition1.XC_Name = "Custom1 String";
			template1Definition1.XC_Type = AddOnColumnDataType.Codes.String;

			var template1Definition2 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition2.XC_Name = "Custom1 Integer";
			template1Definition2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = OpportunityWorkflowDescriptor.WorkflowTypeCode;

			var template2Definition1 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition1.XC_Name = "Custom2 Boolean";
			template2Definition1.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var template2Definition2 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition2.XC_Name = "Custom2 Datetime";
			template2Definition2.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var unrelatedTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			unrelatedTemplate.P0_ProcessType = "ZZZ";

			var unrelatedTemplateDefinition = unrelatedTemplate.GenCustomColumnDefinitions.AddNew();
			unrelatedTemplateDefinition.XC_Name = "Unrelated Custom String";
			unrelatedTemplateDefinition.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<OrgOpportunity>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.OpportunityManagementCRMSecurity);
		}

		#endregion

		public void TestValueAnalysisModuleFilters()
		{
			var filterBizObj = new OrgOpportunityFilterBusinessObject();
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOpp,
				ModuleIDs.ValueAnalysisForwardingOpp,
				ModuleIDs.ValueAnalysisLinerAgencyOpp,
				ModuleIDs.ValueAnalysisTransportOpp,
				ModuleIDs.ValueAnalysisWarehouseOpp
			};
			foreach (var module in modules)
			{
				var filter = filterBizObj[module.Description];
				AssertNotNull(module.Description, filter);
				AssertEquals("ValueAnalysisModuleFilter", filter.GetType().Name);
				AssertEquals("Value Analysis", (string)filter.Category.Description);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			var opportunities = Factory.Load<OrgOpportunity>(new ZQuery());
			foreach (var opportunity in opportunities)
			{
				opportunity.Delete();
			}
			Factory.Save();
			base.SetUp();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgOpportunityFilterBusinessObject();
		}

		#endregion
	}
}
