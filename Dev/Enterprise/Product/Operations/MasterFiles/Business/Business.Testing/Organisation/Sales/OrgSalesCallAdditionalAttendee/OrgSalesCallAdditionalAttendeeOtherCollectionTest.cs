using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallAdditionalAttendeeOtherCollection))]
	sealed class OrgSalesCallAdditionalAttendeeOtherCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			return new OrgSalesCallAdditionalAttendeeOtherCollection(salesCall);
		}
	}
}
