using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallAdditionalAttendeeStaffCollection))]
	sealed class OrgSalesCallAdditionalAttendeeStaffCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var additionalStaff = salesCall.AdditionalAttendeesStaff.AddNew();
			AssertEquals(GlbStaffSchema.Constants.Prefix, additionalStaff.O6_AttendeeTableCode);
			AssertEquals(true, additionalStaff.O6_ReceiverReminder);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			return new OrgSalesCallAdditionalAttendeeStaffCollection(salesCall);
		}
	}
}
