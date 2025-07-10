using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Registry;

	public class PreferentialCountryGroupCodeListTest : TestCaseWithFactory
	{
		public void TestGetListFor()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ZDateTime dateForDutyRate = new ZDateTime(2009, 11, 21);
				NZCCountry unitedStates = Factory.LoadTop1<NZCCountry>(new ZQuery(NZCCountrySchema.U9_Code, Enterprise.Core.Constants.CountryCodes.UnitedStates));
				PreferentialCountryGroupCodeList codeList = PreferentialCountryGroupCodeList.GetListFor(unitedStates.U9_Code, dateForDutyRate, Factory);
				AssertEquals("codeList.Count", 0, codeList.Count);

				NZCCountry australia = Factory.LoadTop1<NZCCountry>(new ZQuery(NZCCountrySchema.U9_Code, Enterprise.Core.Constants.CountryCodes.Australia));
				codeList = PreferentialCountryGroupCodeList.GetListFor(australia.U9_Code, dateForDutyRate, Factory);
				AssertEquals("codeList.Count", 1, codeList.Count);
				Assert(codeList.Contains("AU", "AUSTRALIA"));

				NZCCountry thailand = Factory.LoadTop1<NZCCountry>(new ZQuery(NZCCountrySchema.U9_Code, Enterprise.Core.Constants.CountryCodes.Thailand));
				codeList = PreferentialCountryGroupCodeList.GetListFor("TH", dateForDutyRate, Factory);
				AssertEquals("codeList.Count", 2, codeList.Count);
				Assert(codeList.Contains("TH", "THAILAND"));
				Assert(codeList.Contains("LDC", "LESS DEVELOPED COUNTRIES"));
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);

				var codeList = PreferentialCountryGroupCodeList.GetListFor("AA", ZDateTime.Today, Factory);
				AssertEquals("codeList.Count", 0, codeList.Count);

				codeList = PreferentialCountryGroupCodeList.GetListFor("AU", ZDateTime.Today, Factory);
				AssertEquals("codeList.Count", 1, codeList.Count);
				Assert("Contains trade group 'AU'", codeList.Contains("AU", "Australia"));

				codeList = PreferentialCountryGroupCodeList.GetListFor("US", ZDateTime.Today, Factory);
				AssertEquals("codeList.Count", 1, codeList.Count);
				Assert("Contains trade group 'US'", codeList.Contains("US", "United States"));

				codeList = PreferentialCountryGroupCodeList.GetListFor("US", ZDateTime.Today, Factory, "N");
				AssertEquals("codeList.Count", 0, codeList.Count);
			}
		}

		public void TestDateForDutyRateOutOfCountryGroupInterval()
		{
			ZDateTime dateForDutyRate = new ZDateTime(2006, 4, 30);
			NZCCountry singapore = Factory.LoadTop1<NZCCountry>(new ZQuery(NZCCountrySchema.U9_Code, Enterprise.Core.Constants.CountryCodes.Singapore));
			PreferentialCountryGroupCodeList codeList = PreferentialCountryGroupCodeList.GetListFor(singapore.U9_Code, dateForDutyRate, Factory);
			AssertEquals("codeList.Count", 1, codeList.Count);
			Assert(codeList.Contains("SG", "SINGAPORE"));

			dateForDutyRate = new ZDateTime(2006, 5, 1);
			codeList = PreferentialCountryGroupCodeList.GetListFor(singapore.U9_Code, dateForDutyRate, Factory);
			AssertEquals("codeList.Count", 2, codeList.Count);
			Assert(codeList.Contains("SG", "SINGAPORE"));
			Assert(codeList.Contains("TPA", "TRANS-PACIFIC AGREEMENT"));
		}
	}

	static class ExtensionMethodsForTesting
	{
		internal static bool Contains(this PreferentialCountryGroupCodeList codeList, string code, string description)
		{
			return codeList.ContainsCode(code) && codeList.GetDescriptionFromCode(code).Equals(description);
		}
	}
}
