using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RelatedOrgSalesCallCollection))]
	sealed class RelatedOrgSalesCallCollectionTest : ActiveBusinessObjectCollectionTestCase<RelatedOrgSalesCallCollection>
	{
		public void TestFilter()
		{
			var opportunityA = (IRelatableActivity)Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunityB = (IRelatableActivity)Factory.NewWithValidTestData<OrgOpportunity>();
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var communication1 = header.SalesCalls.AddNew();
			communication1.OQ_CallSummary = "communication1";

			var communication2 = header.SalesCalls.AddNew();
			communication2.OQ_CallSummary = "communication2";

			var communication3 = header.SalesCalls.AddNew();
			communication3.OQ_CallSummary = "communication3";

			var pivotA1 = Factory.New<ViewRelatedActivityPivot>();
			pivotA1.ParentActivity = opportunityA;
			pivotA1.ChildActivity = communication1;

			var pivot2A = Factory.New<ViewRelatedActivityPivot>();
			pivot2A.ParentActivity = communication2;
			pivot2A.ChildActivity = opportunityA;

			var pivotB3 = Factory.New<ViewRelatedActivityPivot>();
			pivotB3.ParentActivity = opportunityB;
			pivotB3.ChildActivity = communication3;
			Factory.Save();

			var collection = new RelatedOrgSalesCallCollection(opportunityA);
			AssertContainsExactElementsInAnyOrder(x => x.OQ_CallSummary, new[] { communication1, communication2 }, collection);

			var communication4 = Factory.New<OrgSalesCall>();
			communication4.OQ_CallSummary = "communication4";

			var pivotA4 = Factory.New<ViewRelatedActivityPivot>();
			pivotA4.ParentActivity = opportunityA;
			pivotA4.ChildActivity = communication4;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(x => x.OQ_CallSummary, new[] { communication1, communication2, communication4 }, collection);
		}

		#region Implementation

		protected override RelatedOrgSalesCallCollection GetCollectionToTest()
		{
			return new RelatedOrgSalesCallCollection(ParentOpportunity);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OH = header.PK;
			Factory.Save();

			return orgSalesCall;
		}

		IRelatableActivity parentOpportunity;
		IRelatableActivity ParentOpportunity
		{
			get { return parentOpportunity ?? (parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>()); }
		}

		#endregion
	}
}
