using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallTradeLanePivotCollection))]
	sealed class OrgSalesCallTradeLanePivotCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgSalesCallTradeLanePivotCollection>
	{
		protected override OrgSalesCallTradeLanePivotCollection GetCollectionToTest()
		{
			OrgSalesCall salesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			return new OrgSalesCallTradeLanePivotCollection(salesCall);
		}
	}
}
