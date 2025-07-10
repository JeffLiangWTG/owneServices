using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusPermitHeader.Loader))]
	sealed class BaseCusPermitHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var appliesToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var permit = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, org.PK, "NO123", startDate.Date, endDate.Date, "BTH", "IMP");
			permit.CPH_OA_AppliesTo = appliesToOrg.MainAddress.PK;

			Factory.Save();

			var permitHeader = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO123", org.PK, ZDateTime.Today, "", "", "", null, appliesToOrg.PK);
			AssertEquals(permitHeader.PK, permit.PK);
		}

		public void TestLoad_ShouldNotLoadGuarantee()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var appliesToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var guarantee = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			guarantee.CPH_OH_PermitHolder = org.PK;
			guarantee.CPH_Number = "NO123";
			guarantee.CPH_StartDate = startDate.Date;
			guarantee.CPH_EndDate = endDate.Date;
			guarantee.CPH_QtyValIndicator = "BTH";
			guarantee.CPH_Type = "IMP";

			Factory.Save();

			var permitHeader = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO123", org.PK, ZDateTime.Today, "", "", "", null, appliesToOrg.PK);
			AssertNull("We should not load guarantee.", permitHeader);
		}

		public void TestLoadByNumber()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			permit.CPH_Number = "PER001";
			AssertEquals("Should not return any permit if the permit number is empty.", false, new BaseCusPermitHeader.Loader(Factory).LoadByNumber(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty).Any());
			AssertSame("Should load the permits.", permit, new BaseCusPermitHeader.Loader(Factory).LoadByNumber(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "PER001").Single());

			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			guarantee.CPH_Number = "GUA001";
			AssertEquals("Should not load the guarantees.", false, new BaseCusPermitHeader.Loader(Factory).LoadByNumber(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "GUA001").Any());
		}

		public void TestLoadByRules()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var permit = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, org.PK, "NO123", startDate.Date, endDate.Date, "BTH", "IMP");
			var tariffRule1 = helper.CreatePermitRule(permit, "TAR", "1110", "1120");
			var tariffRule2 = helper.CreatePermitRule(permit, "TAR", "1130", "1140");
			Factory.Save();

			CombineAssertions("Permit rules of the same type are OR'ed together", () =>
			{
				var rules1 = new Dictionary<ZString, ZString>() { { "TAR", "1112" } };
				var rules2 = new Dictionary<ZString, ZString>() { { "TAR", "1132" } };

				var permitHeader1 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "", org.PK, startDate.Date, "", "", "", rules1);
				var permitHeader2 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "", org.PK, startDate.Date, "", "", "", rules2);
				AssertEquals("Matched tariffRule1", permit.PK, permitHeader1.PK);
				AssertEquals("Matched tariffRule2", permit.PK, permitHeader2.PK);
			});

			var countryRule1 = helper.CreatePermitRule(permit, "COO", "AU", "");
			var countryRule2 = helper.CreatePermitRule(permit, "COO", "ZA", "");
			Factory.Save();

			CombineAssertions("Permit rules of different types are AND'ed together", () =>
			{
				var rules1 = new Dictionary<ZString, ZString>() { { "TAR", "1112" } };
				var rules2 = new Dictionary<ZString, ZString>() { { "TAR", "1132" }, { "COO", "AU" } };

				var permitHeader1 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "", org.PK, startDate.Date, "", "", "", rules1);
				var permitHeader2 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "", org.PK, startDate.Date, "", "", "", rules2);
				AssertNull("Missing countryRule", permitHeader1);
				AssertEquals("Matched tariffRule2 + countryRule1", permit.PK, permitHeader2.PK);
			});
		}

		public void TestLoadFilterByDate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var actualPermit1 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, org.PK, "NO001", startDate.Date, endDate.Date, "BTH", "IMP");
			var actualPermit2 = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, org.PK, "NO002", startDate.Date, ZDate.Empty, "BTH", "IMP");

			Factory.Save();

			var loadedPermit1 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO001", org.PK, startDate.AddHours(13));
			var loadedPermit2 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO001", org.PK, endDate.AddHours(13));
			var loadedPermit3 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO001", org.PK, endDate.AddHours(25));
			var loadedPermit4 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO001", org.PK, ZDateTime.Empty);
			var loadedPermit5 = new BaseCusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO002", org.PK, endDate.AddHours(25));

			CombineAssertions("Permit loading", () =>
			{
				AssertNotNull("Permit1 not loaded", loadedPermit1);
				AssertNotNull("Permit2 not loaded", loadedPermit2);
				AssertNull("Permit3 should not be loaded", loadedPermit3);
				AssertNotNull("Permit4 not loaded", loadedPermit4);
				AssertNotNull("Permit5 not loaded", loadedPermit5);
			});

			CombineAssertions("Permit Data", () =>
			{
				AssertEquals("Permit1 Same as start date", actualPermit1.PK, loadedPermit1.PK);
				AssertEquals("Permit2 Same as end date", actualPermit1.PK, loadedPermit2.PK);
				AssertEquals("Permit4 No Date filter", actualPermit1.PK, loadedPermit4.PK);
				AssertEquals("Permit5 No end date", actualPermit2.PK, loadedPermit5.PK);
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BaseCusPermitHeader.Loader(Factory);
		}
	}
}
