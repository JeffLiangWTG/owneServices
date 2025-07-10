using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.TR;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRGlbStaffWrapper))]
	public class TRGlbStaffWrapperTest : Enterprise.MasterFiles.Business.Testing.GlbStaffWrapperTest<TRGlbStaffWrapper>
	{
		public void TestIGlbStaffWrapperMembers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = TRGlbStaffWrapper.Get(staff);
			ITRGlbStaffWrapper iWrapper = wrapper;
			AssertEquals(wrapper.TRBPassword, iWrapper.TRBPassword);
		}

		public void TestTRBPassword()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_TR>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff1.PK;

			Factory.Save();

			var wrapper1 = TRGlbStaffWrapper.Get(staff1);
			AssertEquals(password.PK, wrapper1.TRBPassword.PK);

			var wrapper2 = TRGlbStaffWrapper.Get(staff2);
			var password2 = wrapper2.TRBPassword;
			Assert(!password2.IsInDatabase);
			Assert(!password2.HasChanges);
			AssertEquals(staff2.PK, password2.GP_GS);
			AssertEquals(GlbCompany.CurrentCompany.PK, password2.GP_GC);
			AssertEquals(PasswordTypesList.Codes.TRK, password2.GP_PasswordType);
		}

		protected override TRGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
		{
			return TRGlbStaffWrapper.Get(staff);
		}
	}
}
