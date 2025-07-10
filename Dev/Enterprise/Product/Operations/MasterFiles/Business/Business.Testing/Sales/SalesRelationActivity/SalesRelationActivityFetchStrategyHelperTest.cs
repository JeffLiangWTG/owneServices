using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesRelationActivityFetchStrategyHelperTest : TestCaseWithFactory
	{
		#region FetchForView

		public void TestFetchForView_RecentActivityDate()
		{
			var columnNames = new[] { "SalesRelationModel.RecentActivityDate" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(ViewSalesRelationActivityDataSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNames, expectedDbHits);
		}

		public void TestFetchForView_HasSalesRelation()
		{
			var columnNames = new[] { "SalesRelationModel.HasSalesRelation" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(ViewSalesRelationActivityDataSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNames, expectedDbHits);
		}

		#endregion

		#region Implementation

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testOpportunities = viewFactory.Load<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.PK, testOpportunityPks));
			viewFactory.ResetDatabaseLoadCount();

			foreach (var testOpportunity in testOpportunities)
			{
				SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(viewFactory, testOpportunity, viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testOpportunity in testOpportunities)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testOpportunity[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testOpportunityPks = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				var parentInquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				var childInquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				var grandchildInquiry = Factory.NewWithValidTestData<SalesEnquiry>();

				opportunity.RelatedParentActivityPivotCollection.AddNewPivot(parentInquiry);
				opportunity.RelatedChildActivityPivotCollection.AddNewPivot(childInquiry);
				childInquiry.RelatedChildActivityPivotCollection.AddNewPivot(grandchildInquiry);

				testOpportunityPks.Add(opportunity.PK);
			}

			Factory.Save();
		}

		List<ZGuid> testOpportunityPks;

		#endregion
	}
}
