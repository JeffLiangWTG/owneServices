using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(ISalesRelationActivity), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class SalesRelationActivityTestCase<TSalesActivity> : RelatableActivityTestCase<TSalesActivity>
		where TSalesActivity : BusinessObject, ISalesRelationActivity
	{
		#region FetchStrategy

		public void TestFetchStrategy_FetchForView_RecentActivityDate()
		{
			if (TableSchema == null)
			{
				Assert("FetchForView not required if no table schema", true);
				return;
			}

			AddActivitiesForFetchStrategyTest();

			AssertFetchForViewDbHits(new[] { "SalesRelationModel.RecentActivityDate" });
		}

		public void TestFetchStrategy_FetchForView_HasSalesRelation()
		{
			if (TableSchema == null)
			{
				Assert("FetchForView not required if no table schema", true);
				return;
			}

			AddActivitiesForFetchStrategyTest();

			AssertFetchForViewDbHits(new[] { "SalesRelationModel.HasSalesRelation" });
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames)
		{
			var viewFactory = new BusinessObjectFactory();
			var testActivities = viewFactory.Load<TSalesActivity>(new ZQuery(TableSchema.PK, testActivityPks));
			viewFactory.ResetDatabaseLoadCount();

			foreach (var testActivity in testActivities)
			{
				testActivity.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testActivity in testActivities)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testActivity[columnName];
				}
			}

			AssertEquals(string.Format(@"ViewSalesRelationActivityData table hit count should be 1.

If this fails, you may need to add the following line to the FetchForViewCore of [{0}]'s fetch strategy:
	SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(Factory, (ISalesRelationActivity)BusinessObject, columns);
", typeof(TSalesActivity).FullName), 1, viewFactory.GetTableHitCount(ViewSalesRelationActivityDataSchema.Constants.TableName));
		}

		void AddActivitiesForFetchStrategyTest()
		{
			testActivityPks = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var activity = GetNewActivity();
				var parentInquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				var childInquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				var grandchildInquiry = Factory.NewWithValidTestData<SalesEnquiry>();

				activity.RelatedParentActivityPivotCollection.AddNewPivot(parentInquiry);
				activity.RelatedChildActivityPivotCollection.AddNewPivot(childInquiry);
				childInquiry.RelatedChildActivityPivotCollection.AddNewPivot(grandchildInquiry);

				testActivityPks.Add(activity.PK);
			}

			Factory.Save();
		}

		List<ZGuid> testActivityPks;

		#endregion

		#region Recent Activity Date

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestRecentActivityDate()
		{
			GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 600;
			GlbBranch.CurrentBranch.HomePort.TimeZoneSet.DaylightSavingZones.DeleteAll();

			var activity = GetNewActivity();
			Factory.Save();
			if (!activity.SalesRelationModel.RecentActivityDate.IsEmpty)
			{
				AssertEquals("Should be in local time", new ZDateTime(2000, 1, 1, 10, 0, 0), activity.SalesRelationModel.RecentActivityDate);
			}
			else
			{
				Assert(true);
			}

			var childActivity = GetNewActivity();
			childActivity.RelatedParentActivityPivotCollection.AddActivity(activity);
			Factory.Save();
			if (!childActivity.SalesRelationModel.RecentActivityDate.IsEmpty)
			{
				AssertEquals("Should be in local time", new ZDateTime(2000, 1, 1, 10, 0, 0), childActivity.SalesRelationModel.RecentActivityDate);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Implementation

		protected abstract ITableSchema TableSchema { get; }

		#endregion
	}
}
