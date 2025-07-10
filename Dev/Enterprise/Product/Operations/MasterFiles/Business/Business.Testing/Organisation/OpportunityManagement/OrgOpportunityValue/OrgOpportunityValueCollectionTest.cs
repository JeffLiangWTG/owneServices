using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityValueCollection))]
	public class OrgOpportunityValueCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			return opp.ValueItems;
		}

		public void TestTotalValue()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue value1 = opp.ValueItems.AddNew();
			OrgOpportunityValue value2 = opp.ValueItems.AddNew();

			AssertEquals(0m, opp.ValueItems.TotalValue);

			value1.PV_Value = 50m;
			AssertEquals(50m, opp.ValueItems.TotalValue);

			value1.PV_Value = 100m;
			AssertEquals(100m, opp.ValueItems.TotalValue);

			value2.PV_Value = 200m;
			AssertEquals(300m, opp.ValueItems.TotalValue);

			opp.ValueItems.RemoveAndDelete(value2);
			AssertEquals(100m, opp.ValueItems.TotalValue);
		}
	}
}
