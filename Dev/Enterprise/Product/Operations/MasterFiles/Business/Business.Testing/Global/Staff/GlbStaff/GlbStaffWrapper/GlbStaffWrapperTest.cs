using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbStaffWrapper))]
	public abstract class GlbStaffWrapperTest<T> : NonPersistentBusinessObjectTestCase
			where T : GlbStaffWrapper
	{
		public void TestDefault()
		{
			AssertEquals(Staff.PK, Wrapper.PK);
			AssertEquals(GlbStaffSchema.Constants.TableName, Wrapper.TableName);
			AssertEquals(GlbStaffSchema.Constants.Prefix, Wrapper.TablePrefix);
			AssertEquals(false, Wrapper.SupportsClone());
			AssertEquals(false, Wrapper.IsInDatabase);

			Factory.Save();
			AssertEquals(Staff.PK, Wrapper.PK);
			AssertEquals(GlbStaffSchema.Constants.TableName, Wrapper.TableName);
			AssertEquals(GlbStaffSchema.Constants.Prefix, Wrapper.TablePrefix);
			AssertEquals(false, Wrapper.SupportsClone());
			AssertEquals(true, Wrapper.IsInDatabase);
		}

		public void TestGetGlbExternalPassword()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "A12";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "A12";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "S$3";
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordType = "ABC";
			extPassword1.GP_GC = company1.PK;
			extPassword1.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_GS = staff.PK;
			extPassword2.GP_PasswordType = "ABC";
			extPassword2.GP_GC = company2.PK;
			extPassword2.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			var extPassword3 = Factory.New<GlbExternalPassword>();
			extPassword3.GP_GS = staff.PK;
			extPassword3.GP_PasswordType = "ABC";
			extPassword3.GP_GC = ZGuid.Empty;
			extPassword3.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-11);
			var extPassword4 = Factory.New<GlbExternalPassword>();
			extPassword4.GP_GS = staff.PK;
			extPassword4.GP_PasswordType = "DEF";
			extPassword4.GP_GC = company1.PK;
			extPassword4.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-12);
			var extPassword5 = Factory.New<GlbExternalPassword>();
			extPassword5.GP_GS = ZGuid.Empty;
			extPassword5.GP_PasswordType = "GHI";
			extPassword5.GP_GC = company1.PK;
			extPassword5.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-13);
			var wrapper = CreateNewWrapper(staff);
			AssertEquals(extPassword3, wrapper.GetGlbExternalPassword<GlbExternalPassword>("ABC"));
			AssertEquals(extPassword1, wrapper.GetGlbExternalPassword<GlbExternalPassword>("ABC", company1.PK));
			AssertEquals(extPassword2, wrapper.GetGlbExternalPassword<GlbExternalPassword>("ABC", company2.PK));
			AssertNull(wrapper.GetGlbExternalPassword<GlbExternalPassword>("GHI", company2.PK));
			AssertNull(wrapper.GetGlbExternalPassword<GlbExternalPassword>("GHI", company1.PK));
			AssertNull(wrapper.GetGlbExternalPassword<GlbExternalPassword>("DEF", company2.PK));
			AssertEquals(extPassword4, wrapper.GetGlbExternalPassword<GlbExternalPassword>("DEF", company1.PK));
			AssertNull(wrapper.GetGlbExternalPassword<GlbExternalPassword>("JKL"));
		}

		public void GetGlbExternalPasswordOrCreateNew()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "A12";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "A12";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "S$3";
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordType = "ABC";
			extPassword1.GP_GC = company1.PK;
			extPassword1.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_GS = staff.PK;
			extPassword2.GP_PasswordType = "ABC";
			extPassword2.GP_GC = company2.PK;
			extPassword2.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			var extPassword3 = Factory.New<GlbExternalPassword>();
			extPassword3.GP_GS = staff.PK;
			extPassword3.GP_PasswordType = "ABC";
			extPassword3.GP_GC = ZGuid.Empty;
			extPassword3.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-11);
			var extPassword4 = Factory.New<GlbExternalPassword>();
			extPassword4.GP_GS = staff.PK;
			extPassword4.GP_PasswordType = "DEF";
			extPassword4.GP_GC = company1.PK;
			extPassword4.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-12);
			var extPassword5 = Factory.New<GlbExternalPassword>();
			extPassword5.GP_GS = ZGuid.Empty;
			extPassword5.GP_PasswordType = "GHI";
			extPassword5.GP_GC = company1.PK;
			extPassword5.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-13);
			var wrapper = CreateNewWrapper(staff);
			AssertEquals(extPassword3, wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("ABC"));
			AssertEquals(extPassword1, wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("ABC", company1.PK));
			AssertEquals(extPassword2, wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("ABC", company2.PK));
			AssertWrapper(wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("GHI", company2.PK), "GHI", company2.PK);
			AssertWrapper(wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("GHI", company1.PK), "GHI", company1.PK);
			AssertWrapper(wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("DEF", company2.PK), "DEF", company2.PK);
			AssertEquals(extPassword4, wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("DEF", company1.PK));
			AssertWrapper(wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("JKL"), "JKL", ZGuid.Empty);
		}

		protected void AssertWrapper(GlbExternalPassword glbExternalPassword, ZString passwordType, ZGuid companyPK)
		{
			AssertEquals(passwordType, glbExternalPassword.GP_PasswordType);
			AssertEquals(companyPK, glbExternalPassword.GP_GC);
		}

		#region Implementation

		#region GlbExternalPassword

		protected T Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = CreateNewWrapper(Staff);
				}
				return wrapper;
			}
		}
		T wrapper;

		protected abstract T CreateNewWrapper(GlbStaff staff);

		#endregion

		#region GlbStaff

		protected GlbStaff Staff
		{
			get
			{
				if (glbStaff == null)
				{
					glbStaff = Factory.New<GlbStaff>();
					glbStaff.GS_Code = "ZAC";
				}
				return glbStaff;
			}
		}
		GlbStaff glbStaff;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Wrapper;
		}

		#endregion
	}
}
