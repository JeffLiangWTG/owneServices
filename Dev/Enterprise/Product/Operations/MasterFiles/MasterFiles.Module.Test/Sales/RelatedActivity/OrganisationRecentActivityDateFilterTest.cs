using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrganisationRecentActivityDateFilter))]
	sealed class OrganisationRecentActivityDateFilterTest : RecentActivityDateFilterTestCase<OrganisationRecentActivityDateFilter>
	{
		#region Query

		[TestDate(2014, 1, 1)]
		public override void TestGetQuery()
		{
			#region Test Data

			var testOrgs = Factory.Load<OrgHeader>(new ZQuery { MaximumRows = 4 });
			AssertEquals(4, testOrgs.Length);
			var org1 = testOrgs[0];
			var org2 = testOrgs[1];
			var org3 = testOrgs[2];
			var org4 = testOrgs[3];

			var opportunity1 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 8, 1));
			opportunity1.P8_OH = org1.PK;
			var opportunity2 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 9, 1));
			opportunity2.P8_OH = org3.PK;
			var opportunity3 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 10, 1));
			var opportunity4 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 9, 1));
			var opportunity5 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 12, 1));
			var opportunity6 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 9, 1));

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			TestDateAttribute.Date = new ZDateTime(2014, 6, 1).ToDateTime();
			Factory.Save();

			var inquiry = NewWithValidTestDataLastEditTime<SalesEnquiry>(new ZDateTime(2014, 7, 1));
			inquiry.O1_OH_ConvertedToQualifiedLead = org2.PK;
			var communication = NewWithValidTestDataLastEditTime<OrgSalesCall>(new ZDateTime(2014, 9, 1));
			communication.OQ_OH = org2.PK;

			(campaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity1);
			(campaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity2);
			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			opportunity3.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			opportunity4.RelatedChildActivityPivotCollection.AddNewPivot(opportunity5);

			/*
			 * CAM      OP4   OP6
			 * |  \      |
			 * |   \     |
			 * OP1 OP2  OP5
			 * |
			 * |
			 * INQ OP3
			 * |   /
			 * |  /
			 * COM
			 * 
			 * */

			Factory.Save();

			#endregion

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

			Filter.Property1 = new ZDateTime(2014, 9, 1);
			Filter.Property2 = new ZDateTime(2014, 9, 5);
			Filter.TypeProperty = "ANY";
			var expectedResult = new BusinessObject[]
				{
					org1,
					org2
				};
			var actualResult = Factory.Load<OrgHeader>(filterCombiner.GetCombinedFilter(new[] { (ModuleFilter)Filter, customSqlFilter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			Filter.Property1 = new ZDateTime(2014, 10, 1);
			Filter.Property2 = ZDateTime.Empty;
			Filter.TypeProperty = "ANY";
			expectedResult = new BusinessObject[]
				{
					org2,
					org3
				};
			actualResult = Factory.Load<OrgHeader>(filterCombiner.GetCombinedFilter(new[] { (ModuleFilter)Filter, customSqlFilter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			Filter.Property1 = new ZDateTime(2014, 7, 1);
			Filter.Property2 = new ZDateTime(2014, 12, 1);
			Filter.TypeProperty = "OPP";
			expectedResult = new BusinessObject[]
				{
					org1,
					org3,
					org2
				};
			actualResult = Factory.Load<OrgHeader>(filterCombiner.GetCombinedFilter(new[] { (ModuleFilter)Filter, customSqlFilter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			Filter.Property1 = new ZDateTime(2014, 7, 1);
			Filter.Property2 = new ZDateTime(2014, 12, 1);
			Filter.TypeProperty = "INQ";
			expectedResult = new BusinessObject[]
				{
					org1,
					org2
				};
			actualResult = Factory.Load<OrgHeader>(filterCombiner.GetCombinedFilter(new[] { (ModuleFilter)Filter, customSqlFilter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
		}

		#endregion

		#region Implementation

		T NewWithValidTestDataLastEditTime<T>(ZDateTime lastEditTime)
			where T : BusinessObject
		{
			TestDateAttribute.Date = lastEditTime.ToDateTime();
			var result = Factory.NewWithValidTestData<T>();
			Factory.Save();
			return result;
		}

		protected override OrganisationRecentActivityDateFilter GetNewModuleFilter()
		{
			var filter = new OrganisationRecentActivityDateFilter(ZString.Empty, ZString.Format("moo"), OrgHeaderSchema.PK);
			return filter;
		}

		#endregion
	}
}
