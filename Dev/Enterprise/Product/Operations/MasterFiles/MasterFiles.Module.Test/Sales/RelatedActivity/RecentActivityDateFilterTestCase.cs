using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class RecentActivityDateFilterTestCase<FilterT> : ModuleFilterTestCase<FilterT> where FilterT : RecentActivityDateFilter
	{
		#region IsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.SalesRelationActivity; }
		}

		#endregion

		#region Clear / IsEmpty

		public void TestClear()
		{
			Filter.PropertySearch = ModuleDateFilter.Future;
			Filter.Property1 = new ZDateTime(2013, 1, 25);
			Filter.Property2 = new ZDateTime(2014, 4, 13);
			Filter.TypeProperty = "OPP";

			Filter.Clear();

			CombineAssertions(() =>
				{
					AssertEquals("PropertySearch", ModuleDateFilter.SpecifiedDateRange, Filter.PropertySearch);
					AssertEquals("Property1", ZDateTime.Empty, Filter.Property1);
					AssertEquals("Property2", ZDateTime.Empty, Filter.Property2);
					AssertEquals("TypeProperty", ZString.Empty, Filter.TypeProperty);
				});
		}

		public void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			Filter.TypeProperty = ZString.Empty;

			AssertEquals(true, Filter.IsEmpty);

			Filter.Property1 = new ZDateTime(2013, 1, 25);
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2014, 4, 13);
			AssertEquals(false, Filter.IsEmpty);

			Filter.TypeProperty = "XXX";
			AssertEquals(true, Filter.IsEmpty);
		}

		#endregion

		#region Query

		[TestDate(2014, 1, 1)]
		public virtual void TestGetQuery()
		{
			#region Test Data

			var opportunity1 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 8, 1));
			var opportunity2 = NewWithValidTestDataLastEditTime<OrgOpportunity>(new ZDateTime(2014, 9, 1));
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
			var communication = NewWithValidTestDataLastEditTime<OrgSalesCall>(new ZDateTime(2014, 9, 1));

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

			var filterCombiner = new ModuleFilterCombiner();

			Filter.Property1 = new ZDateTime(2014, 9, 1);
			Filter.Property2 = new ZDateTime(2014, 9, 5);
			Filter.TypeProperty = "ANY";
			var expectedResult = new BusinessObject[]
				{
					opportunity1,
					opportunity2,
					opportunity6
				};
			var actualResult = Factory.Load<OrgOpportunity>(filterCombiner.GetCombinedFilter(new[] { Filter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			Filter.Property1 = new ZDateTime(2014, 10, 1);
			Filter.Property2 = ZDateTime.Empty;
			Filter.TypeProperty = "ANY";
			expectedResult = new BusinessObject[]
				{
					opportunity3,
					opportunity4,
					opportunity5
				};
			actualResult = Factory.Load<OrgOpportunity>(filterCombiner.GetCombinedFilter(new[] { Filter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = new ZDateTime(2014, 8, 1);
			Filter.TypeProperty = "ANY";
			expectedResult = System.Array.Empty<BusinessObject>();
			actualResult = Factory.Load<OrgOpportunity>(filterCombiner.GetCombinedFilter(new[] { Filter }));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			Filter.Property1 = new ZDateTime(2014, 9, 1);
			Filter.Property2 = new ZDateTime(2014, 9, 5);
			Filter.TypeProperty = "CAM";
			expectedResult = System.Array.Empty<BusinessObject>();
			actualResult = Factory.Load<OrgOpportunity>(filterCombiner.GetCombinedFilter(new[] { Filter }));
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

		#endregion
	}
}
