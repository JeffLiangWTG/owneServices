using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementFetchStrategyTest : TestCaseWithFactory
	{
		#region FetchForView

		public void TestFetchForView_Opportunity()
		{
			var properties = new[] { "Opportunity+P8_OpportunityID" };
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgOpportunitySchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		public void TestFetchForView_StatusDescription()
		{
			var properties = new[] { "StatusDescription" };
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgCommissionCalculationQueueSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		public void TestFetchForView_Customer()
		{
			var properties = new[] { "Customer+OH_Code" };
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		#endregion

		#region Implementation

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testAgreements = viewFactory.Load<OrgCommissionAgreement>(new ZQuery(OrgCommissionAgreementSchema.PK, testAgreementPks));
			viewFactory.ResetDatabaseLoadCount();

			foreach (var testAgreement in testAgreements)
			{
				testAgreement.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testAgreement in testAgreements)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testAgreement[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testAgreementPks = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
				agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
				if (i < 5)
				{
					testAgreementPks.Add(agreement.PK);
				}
				else
				{
					agreement.Approve();
					var draft = agreement.CreateDraft();
					testAgreementPks.Add(draft.PK);
				}
			}

			Factory.Save();
		}

		List<ZGuid> testAgreementPks;

		#endregion
	}
}
