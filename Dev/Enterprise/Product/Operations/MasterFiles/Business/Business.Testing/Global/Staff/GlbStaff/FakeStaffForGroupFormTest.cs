using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FakeStaffForGroupForm))]
	sealed class FakeStaffForGroupFormTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(FakeStaffForGroupForm));
		}

		public void TestAddToAllUsersGroup()
		{
			FakeStaffForGroupForm staff = Factory.New<FakeStaffForGroupForm>();
			AssertEquals("Staff should not be in AllUsers group", 0, staff.Groups.Count);
		}

		public void TestSave()
		{
			FakeStaffForGroupForm fakeStaff = Factory.New<FakeStaffForGroupForm>();
			GlbStaff realStaff = Factory.New<GlbStaff>();
			realStaff.GS_Code = "ZAC";
			Factory.Save();

			Assert("Fake Staff should not be saved", !fakeStaff.IsInDatabase);
			Assert("Real Staff should be saved", realStaff.IsInDatabase);
		}
	}
}
