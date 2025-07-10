using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCompanyICompanyTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals("PK", CompanyGuid, TestCompany.PK);
			AssertEquals("LocalCurrency", CurrencyCode, TestCompany.LocalCurrency.Code);
			AssertEquals("Code", "XXX", TestCompany.Code);
			AssertEquals("Name", "TestName", TestCompany.Name);
			AssertEquals("BusinessRegNo1", "TestRegNo1", TestCompany.BusinessRegNo1);
			AssertEquals("BusinessRegNo2", "TestRegNo2", TestCompany.BusinessRegNo2);

			AssertEquals("IsReciprocal", true, TestCompany.IsReciprocal);
			AssertEquals("IsGSTRegistered", true, TestCompany.IsGSTRegistered);
			AssertEquals("IsGSTCashBasis", true, TestCompany.IsGSTCashBasis);
			AssertEquals("IsWHTRegistered", false, TestCompany.IsWHTRegistered);
			AssertEquals("IsWHTCashBasis", false, TestCompany.IsWHTCashBasis);

			AssertEquals("OrganisationPK", Guid.Empty, TestCompany.OrganisationPK);
			AssertEquals("Fax", "8888 8888", TestCompany.Fax);

			AssertNotNull("ExchangeRate", TestCompany.ExchangeRate);
		}

		public void TestIsDemoCompany()
		{
			Assert("Test Company is not Demo", !TestCompany.IsDemoCompany);

			DataRow demoCompanyRow = SetupCompanyRow(CompanyGuid, CurrencyCode, Guid.Empty, "DEM");
			ICompany demCompany = new GlbCompany(Factory, demoCompanyRow);
			Assert("Dem Company is a Demo company", demCompany.IsDemoCompany);
		}

		public void TestDateTimeFormat()
		{
			DataRow demoCompanyRow = SetupCompanyRow(CompanyGuid, CurrencyCode, Guid.Empty, "DEM");
			demoCompanyRow[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "PH"; //Philippines
			ICompany demCompany = new GlbCompany(Factory, demoCompanyRow);
			AssertEquals("DateTimeFormat", CountryDateTimeFormat.US, demCompany.DateTimeFormat);

			demCompany = new GlbCompany(Factory, demoCompanyRow);
			demoCompanyRow[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "AU"; //Australia
			AssertEquals("DateTimeFormat", CountryDateTimeFormat.Other, demCompany.DateTimeFormat);

			demCompany = new GlbCompany(Factory, demoCompanyRow);
			demoCompanyRow[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "US"; //USA
			AssertEquals("DateTimeFormat", CountryDateTimeFormat.US, demCompany.DateTimeFormat);

			demCompany = new GlbCompany(Factory, demoCompanyRow);
			demoCompanyRow[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "JP"; //JAPAN
			AssertEquals("DateTimeFormat", CountryDateTimeFormat.Japan, demCompany.DateTimeFormat);

			demCompany = new GlbCompany(Factory, demoCompanyRow);
			demoCompanyRow[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "HU"; //Hungary
			AssertEquals("DateTimeFormat", CountryDateTimeFormat.Other, demCompany.DateTimeFormat);
		}

		public void TestShowCodeAtCompanyAndBranchName()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();

			company.GC_Code = "TCO";
			company.GC_Name = "Test Company";

			Env.Registry.ShowCodeAtCompanyAndBranchName = true;
			AssertEquals(company.HumanReadableNameForRegistry, company.HumanReadableShortcutName);

			Env.Registry.ShowCodeAtCompanyAndBranchName = false;
			AssertEquals(company.HumanReadableNameForRegistry, company.GC_Name);
		}

		#region Implementation

		ICompany TestCompany;
		Guid CompanyGuid;
		string CurrencyCode;

		protected override void SetUp()
		{
			CompanyGuid = Guid.NewGuid();
			CurrencyCode = "USD";

			DataRow companyRow = SetupCompanyRow(CompanyGuid, CurrencyCode, Guid.Empty, "XXX");
			TestCompany = new GlbCompany(Factory, companyRow);
			base.SetUp();
		}

		DataRow SetupCompanyRow(Guid companyGuid, string currencyCode, Guid orgProxyGuid, string companyCode)
		{
			var factory = new BusinessObjectFactory();
			var company = factory.New(ObjectFactory.GetType("IGlbCompany"));

			DataRow companyRow = ((INeedRow)company).Row;
			companyRow[GlbCompanySchema.PK.Name] = companyGuid;
			companyRow[GlbCompanySchema.GC_Address1.Name] = "TestAddress";
			companyRow[GlbCompanySchema.GC_Code.Name] = companyCode;
			companyRow[GlbCompanySchema.GC_Name.Name] = "TestName";
			companyRow[GlbCompanySchema.GC_RX_NKLocalCurrency.Name] = currencyCode;
			companyRow[GlbCompanySchema.GC_BusinessRegNo.Name] = "TestRegNo1";
			companyRow[GlbCompanySchema.GC_BusinessRegNo2.Name] = "TestRegNo2";
			companyRow[GlbCompanySchema.GC_Phone.Name] = "123";
			companyRow[GlbCompanySchema.GC_IsReciprocal.Name] = true;
			companyRow[GlbCompanySchema.GC_IsGSTRegistered.Name] = true;
			companyRow[GlbCompanySchema.GC_IsGSTCashBasis.Name] = true;
			companyRow[GlbCompanySchema.GC_IsWHTRegistered.Name] = false;
			companyRow[GlbCompanySchema.GC_IsWHTCashBasis.Name] = false;
			companyRow[GlbCompanySchema.GC_StartDate.Name] = new DateTime(2002, 1, 1);
			companyRow[GlbCompanySchema.GC_OH_OrgProxy.Name] = orgProxyGuid;
			companyRow[GlbCompanySchema.GC_CustomsRegistrationNo.Name] = "123";
			companyRow[GlbCompanySchema.GC_Fax.Name] = "8888 8888";
			companyRow[GlbCompanySchema.GC_WebAddress.Name] = "www.edi.com.au";
			companyRow[GlbCompanySchema.GC_RN_NKCountryCode.Name] = EnvProxy.Instance.CurrentCompany.Country.Code;

			return companyRow;
		}

		#endregion
	}
}
