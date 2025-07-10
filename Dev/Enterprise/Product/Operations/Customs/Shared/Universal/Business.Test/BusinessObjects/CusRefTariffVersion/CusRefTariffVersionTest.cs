using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTariffVersion))]
	class CusRefTariffVersionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCRT_RN_NKCountryCode_ReadOnly()
		{
			AssertEquals(true, Factory.New<CusRefTariffVersion>().CRT_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestCRT_RN_NKCountryCode_Caption()
		{
			AssertEquals("Country/Region", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusRefTariffVersion>().CRT_RN_NKCountryCodeInfo).Caption);
		}

		public void TestCodeProperty()
		{
			var version = Factory.New<CusRefTariffVersion>();
			version.CRT_Version = "TTT";
			AssertEquals("TTT", ((ICodeDescription)version).Code);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Factory.New<CusRefTariffVersion>().CRT_RN_NKCountryCode);
		}

		public void TestCRT_Code_ReadOnly()
		{
			var version = Factory.New<CusRefTariffVersion>();
			version.CRT_Version = "TTT";
			version.CRT_Description = "TTT desc";
			version.CRT_EffectiveDate = new ZDate(2021, 3, 11);
			AssertEquals(false, version.CRT_VersionInfo.ReadOnly);
			Factory.Save();
			AssertEquals(true, version.CRT_VersionInfo.ReadOnly);
		}
	}

	[TestedType(typeof(CusRefTariffVersion.Loader))]
	class CusRefTariffVersionLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var version1 = Factory.New<CusRefTariffVersion>();
			version1.CRT_Version = "V1";
			version1.CRT_EffectiveDate = ZDate.Today;
			var version2 = Factory.New<CusRefTariffVersion>();
			version2.CRT_Version = "V2";
			version2.CRT_EffectiveDate = ZDate.Today.AddDays(5);

			CombineAssertions(() =>
			{
				AssertEquals("no country code", null, CusRefTariffVersion.Loader.Load(Factory, ZString.Empty, ZDate.Today));
				AssertEquals("invalid effective date", null, CusRefTariffVersion.Loader.Load(Factory, GlbCompany.CurrentCompany.Country.Code, ZDate.Invalid));
				AssertEquals("only version1 and version2 not effective yet", version1.PK, CusRefTariffVersion.Loader.Load(Factory, GlbCompany.CurrentCompany.Country.Code, ZDate.Today).PK);
				AssertEquals("version1 not effective while version2 takes into effect", version2.PK, CusRefTariffVersion.Loader.Load(Factory, GlbCompany.CurrentCompany.Country.Code, ZDate.Today.AddDays(6)).PK);
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusRefTariffVersion.Loader(Factory);
	}
}
