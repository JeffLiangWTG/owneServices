using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgOpportunityValueLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValueTypes()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			OrgOpportunityValue value = opp.ValueItems.AddNew();
			AssertEquals(ExpectedNumberOfValueItems, value.Lookups.ValueTypes.Count);
		}

		protected virtual int ExpectedNumberOfValueItems
		{
			get { return 7; }
		}
	}
}
