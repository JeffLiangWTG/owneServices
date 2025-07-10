using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrganisationHasSalesRelationFilter))]
	sealed class OrganisationHasSalesRelationFilterTest : HasSalesRelationFilterTestCase<OrganisationHasSalesRelationFilter>
	{
		#region Constructor

		public override void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BoolProperty", true, Filter.BoolProperty);
				AssertEquals("TypeProperty", SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, Filter.TypeProperty);
			});
		}

		#endregion

		#region Clear / IsEmpty

		public override void TestClear()
		{
			Filter.BoolProperty = false;
			Filter.TypeProperty = "OPP";

			Filter.Clear();

			CombineAssertions(() =>
			{
				AssertEquals("BoolProperty", true, Filter.BoolProperty);
				AssertEquals("TypeProperty", ZString.Empty, Filter.TypeProperty);
			});
		}

		#endregion

		#region Query

		public override void TestGetQuery()
		{
			#region Test Data

			TestCaseHelper.ClearTable(RelatedActivityPivotSchema.Constants.TableName);

			var testOrgs = Factory.Load<OrgHeader>(new ZQuery { MaximumRows = 4 });
			AssertEquals(4, testOrgs.Length);
			var org1 = testOrgs[0];
			var org2 = testOrgs[1];
			var org3 = testOrgs[2];

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_OH = org1.PK;
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OH = org3.PK;

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org2.PK;
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_OH = org2.PK;

			Factory.Save();
			var initialOrgCount = Factory.GetDatabaseCount(typeof(OrgHeader));

			#endregion

			var filter = new OrganisationHasSalesRelationFilter(ZString.Empty, ZString.Empty, OrgHeaderSchema.PK);

			filter.IsActive = true;

			AssertHasSalesRelationFilterResults(filter, true, "", Array.Empty<OrgHeader>(), testOrgs); // no orgs shouldnt be there 
			AssertHasSalesRelationFilterResults(filter, true, SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, new[] { org1, org2, org3 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.CampaignManagement, Array.Empty<OrgHeader>());
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.Communication, new[] { org2 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.InquiryManager, new[] { org2 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.OpportunityManager, new[] { org1, org3 });

			AssertHasSalesRelationFilterResults(filter, false, "", Array.Empty<OrgHeader>(), testOrgs);
			AssertHasSalesRelationFilterResults(filter, false, SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, new[] { org1, org2, org3 }, testOrgs);
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.CampaignManagement, Array.Empty<OrgHeader>(), testOrgs);
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.Communication, new[] { org2 }, testOrgs);
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.InquiryManager, new[] { org2 }, testOrgs);
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.OpportunityManager, new[] { org1, org3 }, testOrgs);
		}

		public override void TestQueryGetsRefreshedWhenFilterIsReused()
		{
			var testOrgs = Factory.Load<OrgHeader>(new ZQuery { MaximumRows = 4 });
			AssertEquals(4, testOrgs.Length);
			var org1 = testOrgs[0];
			var org2 = testOrgs[1];
			var org3 = testOrgs[2];
			var org4 = testOrgs[3];

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity4.P8_OH = org1.PK;

			var campaign1 = Factory.New<IGlbCompanyCampaign>();
			campaign1.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var campaign2 = Factory.New<IGlbCompanyCampaign>();
			campaign2.G0_CampaignName = ZGuid.NewZGuid().ToString();

			(campaign1 as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity1);
			(campaign2 as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity2);
			(campaign2 as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity4);

			/*
			 *  CAM1     CAM2      OP3
			 *  |        |  \
			 *  |        |   \
			 *  OP1     OP2  OP4
			 * 
			 * */

			Factory.Save();

			var filter = new OrganisationHasSalesRelationFilter(ZString.Empty, ZString.Empty, OrgHeaderSchema.PK);
			filter.IsActive = true;
			filter.BoolProperty = ZBool.True;
			filter.TypeProperty = RelatableActivityTypeList.Codes.CampaignManagement;

			var customSqlFilter = new ModuleSQLFilter("Custom SQL Filter", typeof(OrgHeader));
			customSqlFilter.IsActive = true;
			customSqlFilter.Property1 = "OH_PK IN (";
			var i = 0;
			foreach (var org in testOrgs)
			{
				customSqlFilter.Property1 += "'" + org.PK + (i < testOrgs.Length - 1 ? "', " : "'");
				i++;
			}
			customSqlFilter.Property1 += ")";

			var filterCombiner = new ModuleFilterCombiner();
			Func<ZQuery> getQuery = () => filterCombiner.GetCombinedFilter(new[] { (ModuleFilter)filter, customSqlFilter });

			var collection = Factory.Load<OrgHeader>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, Array.Empty<OrgHeader>(), collection);

			filter.TypeProperty = RelatableActivityTypeList.Codes.OpportunityManager;
			collection = Factory.Load<OrgHeader>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { org1 }, collection);

			filter.BoolProperty = false;
			collection = Factory.Load<OrgHeader>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { org2, org3, org4 }, collection);
		}

		#endregion

		#region Implementation

		void AssertHasSalesRelationFilterResults(HasSalesRelationFilter hasSalesRelationFilter, ZBool boolProperty, ZString typeProperty, OrgHeader[] expectedResult)
		{
			var organisationHasSalesRelationFilter = (OrganisationHasSalesRelationFilter)hasSalesRelationFilter;
			organisationHasSalesRelationFilter.BoolProperty = boolProperty;
			organisationHasSalesRelationFilter.TypeProperty = typeProperty;
			var filterCombiner = new ModuleFilterCombiner();
			var query = filterCombiner.GetCombinedFilter(new[] { organisationHasSalesRelationFilter });
			var actualResult = Factory.Load<OrgHeader>(query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, expectedResult, actualResult);
		}

		void AssertHasSalesRelationFilterResults(HasSalesRelationFilter hasSalesRelationFilter, ZBool boolProperty, ZString typeProperty, OrgHeader[] unexpectedResult, OrgHeader[] testOrgs)
		{
			var customSqlFilter = new ModuleSQLFilter("Custom SQL Filter", typeof(OrgHeader));
			customSqlFilter.IsActive = true;
			customSqlFilter.Property1 = "OH_PK IN (";
			var i = 0;
			foreach (var org in testOrgs)
			{
				customSqlFilter.Property1 += "'" + org.PK + (i < testOrgs.Length - 1 ? "', " : "'");
				i++;
			}
			customSqlFilter.Property1 += ")";

			var organisationHasSalesRelationFilter = (OrganisationHasSalesRelationFilter)hasSalesRelationFilter;
			organisationHasSalesRelationFilter.BoolProperty = boolProperty;
			organisationHasSalesRelationFilter.TypeProperty = typeProperty;
			var filterCombiner = new ModuleFilterCombiner();
			var query = filterCombiner.GetCombinedFilter(new[] { (ModuleFilter)organisationHasSalesRelationFilter, customSqlFilter });
			var actualResult = Factory.Load<OrgHeader>(query);
			AssertEquals(testOrgs.Length - unexpectedResult.Length, actualResult.Length);
			foreach (var org in unexpectedResult)
			{
				Assert("Org with sales relation should not be returned by filter", !actualResult.Contains(org));
			}
		}

		protected override OrganisationHasSalesRelationFilter GetNewModuleFilter()
		{
			var filter = new OrganisationHasSalesRelationFilter(ZString.Empty, ZString.Format("moo"), OrgHeaderSchema.PK);
			return filter;
		}

		#endregion
	}
}
