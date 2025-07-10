using System;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequest))]
	public class WorkRequestRelatedItemTest : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => "AAA - Mrs Sullivan always planned to leave everything to her cats.";
		protected override string ExpectedSelectionCriterion2 => "BBB - But sometimes, plans need a helping paw.";
		protected override string ExpectedSelectionCriterion3 => "CCC - What are the kitties to do, but buckle together and work as a team.";
		protected override string ExpectedSelectionCriterion4 => "DDD - This fall sparks will fly between one guy who can't get a break...";
		protected override string ExpectedSelectionCriterion5 => "EEE - And nine cats who break all the rules.";

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			return GetPopulatedWorkRequestForTest(Factory);
		}

		public static WorkRequest GetPopulatedWorkRequestForTest(BusinessObjectFactory factory)
		{
			var workRequest = factory.NewWithValidTestData<WorkRequest>();

			workRequest.WKR_Summary = "Last Will and Testameow";

			workRequest.WKR_SelectionCriteria1 = "AAA";
			workRequest.WKR_SelectionCriteria2 = "BBB";
			workRequest.WKR_SelectionCriteria3 = "CCC";
			workRequest.WKR_SelectionCriteria4 = "DDD";
			workRequest.WKR_SelectionCriteria5 = "EEE";

			var client = ProcessMgmtTestHelper.CreateOrganizationAndContact(factory, "Weekend at Dead Cat Lady's House 2", "Mrs Sullivan");
			workRequest.WKR_OC_Client = client.PK;

			return workRequest;
		}

		protected override Type ExpectedPivotCollectionType => typeof(WorkItemRequestLinkCollection);

		protected override void SetUp()
		{
			base.SetUp();

			ProcessMgmtTestHelper.SetMrsSullivanRelatedSelectionCriteriaValues();
		}
	}

	[TestedType(typeof(WorkRequest))]
	public class WorkRequestRelatedItemSourceTest : WorkTaskRelatedItemSourceTestCase
	{
	}
}
