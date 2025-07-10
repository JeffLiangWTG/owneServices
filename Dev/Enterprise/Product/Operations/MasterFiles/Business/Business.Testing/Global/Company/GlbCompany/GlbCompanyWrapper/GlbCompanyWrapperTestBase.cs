using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbCompanyWrapper))]
	public abstract class GlbCompanyWrapperTest<T> : NonPersistentBusinessObjectTestCase
			where T : GlbCompanyWrapper
	{
		public void TestDefault()
		{
			AssertEquals(Company.PK, Wrapper.PK);
			AssertEquals(GlbCompanySchema.Constants.TableName, Wrapper.TableName);
			AssertEquals(GlbCompanySchema.Constants.Prefix, Wrapper.TablePrefix);
			AssertEquals(false, Wrapper.SupportsClone());
			AssertEquals(false, Wrapper.IsInDatabase);

			Factory.Save();
			AssertEquals(Company.PK, Wrapper.PK);
			AssertEquals(GlbCompanySchema.Constants.TableName, Wrapper.TableName);
			AssertEquals(GlbCompanySchema.Constants.Prefix, Wrapper.TablePrefix);
			AssertEquals(false, Wrapper.SupportsClone());
			AssertEquals(true, Wrapper.IsInDatabase);
		}

		public void TestGetGlbExternalPassword()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "A12";
			company1.GC_RN_NKCountryCode = CompanyCountryCode;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "A12";
			company2.GC_RN_NKCountryCode = CompanyCountryCode;

			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = "ABC";
			extPassword1.GP_GC = company1.PK;
			extPassword1.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_PasswordType = "DEF";
			extPassword2.GP_GC = company1.PK;
			extPassword2.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-11);
			var wrapper = GetWrapper(company1);

			var extPassword3 = Factory.New<GlbExternalPassword>();
			extPassword3.GP_PasswordType = "ABC";
			extPassword3.GP_GC = company2.PK;
			extPassword3.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			var wrapper2 = GetWrapper(company2);

			var extPassword4 = Factory.New<GlbExternalPassword>();
			extPassword4.GP_PasswordType = "GHI";
			extPassword4.GP_GC = ZGuid.Empty;
			extPassword4.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-13);

			var extPassword5 = Factory.New<GlbExternalPassword>();
			extPassword4.GP_PasswordType = "JKL";
			extPassword4.GP_GC = company2.PK;
			extPassword4.GP_GS = Factory.NewWithValidTestData<GlbStaff>().PK;
			extPassword4.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-13);

			AssertEquals(extPassword1, wrapper.GetGlbExternalPassword<GlbExternalPassword>("ABC"));
			AssertEquals(extPassword2, wrapper.GetGlbExternalPassword<GlbExternalPassword>("DEF"));
			AssertEquals(extPassword3, wrapper2.GetGlbExternalPassword<GlbExternalPassword>("ABC"));
			AssertNull(wrapper2.GetGlbExternalPassword<GlbExternalPassword>("DEF"));
			AssertNull(wrapper.GetGlbExternalPassword<GlbExternalPassword>("GHI"));
			AssertNull(wrapper2.GetGlbExternalPassword<GlbExternalPassword>("GHI"));
			AssertNull(wrapper.GetGlbExternalPassword<GlbExternalPassword>("JKL"));
			AssertNull(wrapper2.GetGlbExternalPassword<GlbExternalPassword>("JKL"));
		}

		public void TestGetGlbExternalPasswordOrCreateNew()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "A12";
			company1.GC_RN_NKCountryCode = CompanyCountryCode;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "A12";
			company2.GC_RN_NKCountryCode = CompanyCountryCode;

			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = "ABC";
			extPassword1.GP_GC = company1.PK;
			extPassword1.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_PasswordType = "DEF";
			extPassword2.GP_GC = company1.PK;
			extPassword2.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-11);
			var wrapper = GetWrapper(company1);

			var extPassword3 = Factory.New<GlbExternalPassword>();
			extPassword3.GP_PasswordType = "ABC";
			extPassword3.GP_GC = company2.PK;
			extPassword3.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			var wrapper2 = GetWrapper(company2);

			var extPassword4 = Factory.New<GlbExternalPassword>();
			extPassword4.GP_PasswordType = "GHI";
			extPassword4.GP_GC = ZGuid.Empty;
			extPassword4.GP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-13);

			AssertEquals(extPassword1, wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("ABC"));
			AssertEquals(extPassword2, wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("DEF"));
			AssertEquals(extPassword3, wrapper2.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("ABC"));

			AssertWrapper(wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("GHI"), "GHI", company1.PK);
			AssertWrapper(wrapper2.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("GHI"), "GHI", company2.PK);
			AssertWrapper(wrapper2.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("DEF"), "DEF", company2.PK);
			AssertWrapper(wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>("JKL"), "JKL", company1.PK);
		}

		protected void AssertWrapper(GlbExternalPassword extPassword, ZString passwordType, ZGuid companyPk)
		{
			AssertEquals(passwordType, extPassword.GP_PasswordType);
			AssertEquals(companyPk, extPassword.GP_GC);
		}

		#region Implementation

		#region GlbExternalPassword

		protected T Wrapper => wrapper ?? (wrapper = GetWrapper(Company));
		T wrapper;

		protected T GetWrapper(GlbCompany company) => GlbCompanyWrapper.GetWrapper<T>(company);

		#endregion

		#region GlbCompany

		protected GlbCompany Company
		{
			get
			{
				if (glbCompany == null)
				{
					glbCompany = Factory.New<GlbCompany>();
					glbCompany.GC_Code = "XCN";
					glbCompany.GC_RN_NKCountryCode = CompanyCountryCode;
				}
				return glbCompany;
			}
		}
		GlbCompany glbCompany;

		protected virtual ZString CompanyCountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Wrapper;
		}

		#endregion
	}
}
