using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.NZ;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(NZGlbStaffWrapper))]
	public class GlbStaffWrapperTest : Enterprise.MasterFiles.Business.Testing.GlbStaffWrapperTest<NZGlbStaffWrapper>
	{
		public void TestIGlbStaffWrapperMembers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = NZGlbStaffWrapper.Get(staff);
			INZGlbStaffWrapper iWrapper = wrapper;
			AssertEquals(wrapper.NZBPassword, iWrapper.NZBPassword);
		}

		public void TestNZBPassword()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_NZ>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff1.PK;

			Factory.Save();

			var wrapper1 = NZGlbStaffWrapper.Get(staff1);
			AssertEquals(password.PK, wrapper1.NZBPassword.PK);

			var wrapper2 = NZGlbStaffWrapper.Get(staff2);
			var password2 = wrapper2.NZBPassword;
			Assert(!password2.IsInDatabase);
			Assert(!password2.HasChanges);
			AssertEquals(staff2.PK, password2.GP_GS);
			AssertEquals(GlbCompany.CurrentCompany.PK, password2.GP_GC);
			AssertEquals(PasswordTypesList.Codes.NZB, password2.GP_PasswordType);
		}

		protected override NZGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
		{
			return NZGlbStaffWrapper.Get(staff);
		}
	}
}
