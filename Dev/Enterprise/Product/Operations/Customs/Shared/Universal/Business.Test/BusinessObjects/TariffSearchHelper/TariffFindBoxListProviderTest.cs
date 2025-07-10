using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Universal.Testing
{
	public class TariffFindBoxListProviderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.Command("delete from RefDatabase_RefCusTariff").ExecuteNonQuery();
		}

		UniversalReferenceTestDataHelper helper;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		public void TestNearestMatch()
		{
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var tariffTypeABC = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "ABC");
			Factory.Save();
			var tariff = Helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeABC.PK, "112000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A.B.C");
			var tariff2 = Helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeABC.PK, "112001", ZDateTime.Today.AddYears(1), ZDateTime.Today.AddYears(3), "DESC TARIFF 2", compositeKey: "A.B.C");
			var tariff3 = Helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeABC.PK, "112002", ZDateTime.Today.AddYears(3), ZDateTime.Today.AddYears(4), "DESC TARIFF 3", compositeKey: "A.B.C");
			var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today, null, null);
			AssertEquals("112000", ((IFindBoxListProvider)listProvider).NearestMatch("1120", true, -1).Item1);
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today.AddYears(2), null, null);
			AssertEquals("112001", ((IFindBoxListProvider)listProvider).NearestMatch("1120", true, -1).Item1);
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", null, null, null);
			AssertEquals("112002", ((IFindBoxListProvider)listProvider).NearestMatch("1120", true, -1).Item1);
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today, null, new TariffFormatterForTesting());
			AssertEquals("112000", ((IFindBoxListProvider)listProvider).NearestMatch("11.20", true, -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var tariffTypeABC = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "ABC");
			var tariffTypeTST = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var sorghumFlourTariff = Helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeABC.PK, "110290301", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "SORGHUM FLOUR", compositeKey: "A.B.C");
			Helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeABC.PK, "110290301", ZDateTime.Today.AddYears(1), ZDateTime.Today.AddYears(3), "SORGHUM FLOUR 2", compositeKey: "A.B.C");
			Helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeABC.PK, "110290301", ZDateTime.Today.AddYears(3), ZDateTime.Today.AddYears(4), "SORGHUM FLOUR 3", compositeKey: "A.B.C");
			var otherTariff = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "1102901", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "OTHER", compositeKey: "A.B");
			var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "", () => ZDateTime.Today, null, null);
			AssertEquals("SORGHUM FLOUR", ((IFindBoxListProvider)listProvider).DescriptionFromCode("110290301"));
			AssertEquals(ZString.Empty, ((IFindBoxListProvider)listProvider).DescriptionFromCode("110290"));
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "", () => ZDateTime.Today.AddYears(2), null, null);
			AssertEquals("SORGHUM FLOUR 2", ((IFindBoxListProvider)listProvider).DescriptionFromCode("110290301"));
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "", null, null, null);
			AssertEquals("SORGHUM FLOUR 3", ((IFindBoxListProvider)listProvider).DescriptionFromCode("110290301"));
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "", () => ZDateTime.Today, null, new TariffFormatterForTesting());
			AssertEquals("SORGHUM FLOUR", ((IFindBoxListProvider)listProvider).DescriptionFromCode("1102.90.30 1"));
			AssertEquals(ZString.Empty, ((IFindBoxListProvider)listProvider).DescriptionFromCode("1102.90"));
		}

		public void TestGetBusinessObjectFromCode()
		{
			var tariffTypeABC = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "ABC");
			Factory.Save();
			var sorghumFlourTariff = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeABC.PK, "110290301", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "SORGHUM FLOUR", compositeKey: "A.B.C");
			var sorghumFlourTariff2 = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeABC.PK, "110290301", ZDateTime.Today.AddYears(1), ZDateTime.Today.AddYears(3), "SORGHUM FLOUR 2", compositeKey: "A.B.C");
			var sorghumFlourTariff3 = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeABC.PK, "110290301", ZDateTime.Today.AddYears(3), ZDateTime.Today.AddYears(4), "SORGHUM FLOUR 3", compositeKey: "A.B.C");
			var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today, null, null);
			AssertEquals("Business object", sorghumFlourTariff.PK, ((IFindBoxListProvider)listProvider).GetBusinessObjectFromCode("110290301").PK);
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today.AddYears(2), null, null);
			AssertEquals("Business object", sorghumFlourTariff2.PK, ((IFindBoxListProvider)listProvider).GetBusinessObjectFromCode("110290301").PK);
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", null, null, null);
			AssertEquals("Business object", sorghumFlourTariff3.PK, ((IFindBoxListProvider)listProvider).GetBusinessObjectFromCode("110290301").PK);
			listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today, null, new TariffFormatterForTesting());
			AssertEquals("Business object", sorghumFlourTariff.PK, ((IFindBoxListProvider)listProvider).GetBusinessObjectFromCode("1102.90.30 1").PK);
		}

		public void TestCodeFromPrimaryKey()
		{
			var tariffTypeABC = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "ABC");
			Factory.Save();
			var sorghumFlourTariff = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeABC.PK, "110290301", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "SORGHUM FLOUR", compositeKey: "A.B.C");
			var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today, null, null);
			AssertEquals("Tariff code", "110290301", ((IFindBoxListProvider)listProvider).CodeFromPrimaryKey(sorghumFlourTariff.PK));
		}

		public void TestDescriptionFromPrimaryKey()
		{
			var tariffTypeABC = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "ABC");
			Factory.Save();
			var sorghumFlourTariff = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeABC.PK, "110290301", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "SORGHUM FLOUR", compositeKey: "A.B.C");
			var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "ABC", () => ZDateTime.Today, null, null);
			AssertEquals("Tariff description", "SORGHUM FLOUR", ((IFindBoxListProvider)listProvider).DescriptionFromPrimaryKey(sorghumFlourTariff.PK));
		}

		public void TestLoadManualTariff()
		{
			var dataGrouping = Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var tariffTypeTST = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "111000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1");
			var tariff2 = Helper.CreateManualTariff(Core.Constants.CountryCodes.Eritrea, "TST", "222000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2");
			var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Eritrea, "TST", () => ZDateTime.Today, null, null);
			var list = (TariffViewCollection)((IFindBoxListProvider)listProvider).List;
			list.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "111000", "222000" }, list.Select(x => x.ZZ1_TariffCode));
		}

		public void TestShowNearestMatchDesciption()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var chTariffTypePK = UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Switzerland, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "04069099000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "cheese");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "04069099001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "cheese emmental");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "04069099002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "cheese appenzell");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "84061000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "steam turbines");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "8406100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "steam turbines extra");

			Factory.Save();

			CombineAssertions(() =>
			{
				var listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Switzerland, Constants.TariffTypes.Export, () => ZDateTime.Today, null, new TariffFormatterForTesting(), true);

				AssertEquals("Find Description no match", ZString.Empty, ((IFindBoxListProvider)listProvider).DescriptionFromCode("99999999999"));
				AssertEquals("Find Description exact match", "cheese emmental", ((IFindBoxListProvider)listProvider).DescriptionFromCode("04069099001"));
				AssertEquals("Find Description nearest match", "cheese", ((IFindBoxListProvider)listProvider).DescriptionFromCode("04069099"));

				listProvider = new TariffFindBoxListProvider(Core.Constants.CountryCodes.Switzerland, Constants.TariffTypes.Export, () => ZDateTime.Today, null, new TariffFormatterForTesting(), false);

				AssertEquals("Find Description no match", ZString.Empty, ((IFindBoxListProvider)listProvider).DescriptionFromCode("99999999999"));
				AssertEquals("Find Description exact match", "cheese emmental", ((IFindBoxListProvider)listProvider).DescriptionFromCode("04069099001"));
				AssertEquals("Find Description nearest match", ZString.Empty, ((IFindBoxListProvider)listProvider).DescriptionFromCode("04069099"));
			});
		}

		class TariffFormatterForTesting : ITariffFormatter
		{
			public ZString DisplayFormat(ZString unformattedTariff)
			{
				return unformattedTariff;
			}

			public ZString Format(ZString unformattedTariff)
			{
				return unformattedTariff.KeepNumericCharacters();
			}
		}
	}
}
