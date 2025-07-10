using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompany.Loader))]
	sealed class GlbCompanyLoaderTest : LoaderTestCase
	{
		public void TestLoadCompanies()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			carltonAndUnitedFosters.GC_Code = "CUF";

			var loader = (GlbCompany.Loader)GetNewLoaderToTest();
			AssertContainsExactElementsInAnyOrder(Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true)), loader.LoadCompanies());
			AssertContainsExactElementsInAnyOrder(new[] { youngs, charlesWells }, loader.LoadCompanies(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals(0, loader.LoadCompanies(Core.Constants.CountryCodes.France).Length);
		}

		public void TestLoadCompanies_Inactive()
		{
			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			charlesWells.GC_IsActive = false;

			var loader = (GlbCompany.Loader)GetNewLoaderToTest();
			AssertContainsExactElementsInAnyOrder(new[] { youngs, charlesWells }, loader.LoadCompanies(Core.Constants.CountryCodes.UnitedKingdom, activeCompaniesOnly: false));
			AssertContainsExactElementsInAnyOrder(new[] { youngs }, loader.LoadCompanies(Core.Constants.CountryCodes.UnitedKingdom, activeCompaniesOnly: true));
		}

		public void TestLoadCompanies_DemoCompany()
		{
			var demCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.DemoCompanyCode);
			demCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var loader = (GlbCompany.Loader)GetNewLoaderToTest();
			AssertContainsExactElementsInAnyOrder(new[] { demCompany }, loader.LoadCompanies(Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(0, loader.LoadCompanies(Core.Constants.CountryCodes.UnitedStates, false, true).Length);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new GlbCompany.Loader(Factory);
	}
}
