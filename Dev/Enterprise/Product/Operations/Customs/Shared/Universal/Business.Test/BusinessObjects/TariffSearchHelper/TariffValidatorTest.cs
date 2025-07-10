using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class TariffValidatorTest : TestCaseWithFactory
	{
		#region Test Tariff / Nomenclature lookup/validation
		[ExpectException(typeof(ArgumentException))]
		public void TestValidatorThrowsArgumentException()
		{
			var tariffValidator = new TariffValidator();
			var validationData = tariffValidator.Validate("ZA", "29.1612.30", false);
		}

		public void TestValidTariff()
		{
			CreateZZTariffData(Factory);
			var tariffValidator = new TariffValidator();
			var validationData = tariffValidator.Validate("ZA", "29161230", false);
			AssertEquals("Tariff 29161230 for ZA is a valid ref tariffView", true, validationData.IsValid);
			AssertEquals("NearestNomenclature returns value provided for a valid ref tariffView", "29161230", validationData.NearestNomenclature);
			AssertEquals("Tariff 29161230 Description", "BUTYL ACRYLATE", validationData.Description);
			AssertEquals("Tariff 29161230 DataGroupingName", "Universal Customs", validationData.DataGroupingName);
			AssertEquals("Tariff 29161230 EndDate", new ZDateTime(2079, 06, 06), validationData.EndDate);
			validationData = tariffValidator.Validate("ZA", "030325", false);
			AssertEquals("Tariff 030325 for ZA is a valid ref tariffView", true, validationData.IsValid);
			AssertEquals("NearestNomenclature returns the value provided for a valid ref tariffView", "030325", validationData.NearestNomenclature);
			AssertEquals("Tariff 030325 should return the current Description", "CARP (CYPRINUS SPP., CARASSIUS SPP., CTENOPHARYNGODON IDELLUS, HYPOPHT HALMICHTHYS SPP., CIRRHINUS SPP., MYLOPHARYNGODON PICEUS, CATLA CATLA, LABEO SPP., OSTEOCHILUS HASSELTI, LEPTOBARBUS HOEVENI, MEGALOBRAMA SP P.)", validationData.Description);
			AssertEquals("Tariff 030325 DataGroupingName", "Universal Customs", validationData.DataGroupingName);
			AssertEquals("Tariff 030325 EndDate should return the current end date.", new ZDateTime(2079, 06, 06), validationData.EndDate);
			validationData = tariffValidator.Validate("ZA", "29161230", true);
			AssertEquals("Using find neareast should still find exact match for Tariff (29161230 for ZA) if it exists a valid ref tariffView", true, validationData.IsValid);
			AssertEquals("NearestNomenclature returns value provided for a valid ref tariffView", "29161230", validationData.NearestNomenclature);
			AssertEquals("Tariff 29161230 Description", "BUTYL ACRYLATE", validationData.Description);
			AssertEquals("Tariff 29161230 DataGroupingName", "Universal Customs", validationData.DataGroupingName);
			AssertEquals("Tariff 29161230 EndDate", new ZDateTime(2079, 06, 06), validationData.EndDate);
		}

		public void TestInvalidTariff()
		{
			var validationData = new TariffValidator().Validate("ZA", "29161115", false);
			AssertEquals("Tariff 29161115 for ZA is not a valid ref tariffView", false, validationData.IsValid);
			AssertEquals("NearestNomenclature should be blank", ZString.Empty, validationData.NearestNomenclature);
			AssertEquals("Tariff Description should be blank", ZString.Empty, validationData.Description);
			AssertEquals("Tariff DataGroupingName should be blank", ZString.Empty, validationData.DataGroupingName);
		}

		public void TestValidNomeclature()
		{
			CreateZZTariffData(Factory);
			var tariffValidator = new TariffValidator();
			var validationData = tariffValidator.Validate("ZA", "29161240", true);
			AssertEquals("Tariff 291612 for ZA is the nearest valid nomenclature ref and therefore should be produce valid response here.", true, validationData.IsValid);
			AssertEquals("Nomenclature 291612 Description", "Alpha Bravo", validationData.Description);
			AssertEquals("Nomenclature 291612 DataGroupingName", "Universal Customs", validationData.DataGroupingName);
			AssertEquals("Nomenclature 291612 EndDate", ZDateTime.UtcToday, validationData.EndDate);
		}

		public void TestInvalidNomenclature()
		{
			var validationData = new TariffValidator().Validate("CN", "29161115", false);
			AssertEquals("Tariff 29161115 for CN is not a valid ref tariffView", false, validationData.IsValid);
			AssertEquals("NearestNomenclature should be blank", ZString.Empty, validationData.NearestNomenclature);
			AssertEquals("Tariff Description should be blank", ZString.Empty, validationData.Description);
			AssertEquals("Tariff DataGroupingName should be blank", ZString.Empty, validationData.DataGroupingName);
			AssertEquals("Nomenclature EndDate should be empty", ZDateTime.Empty, validationData.EndDate);
			validationData = new TariffValidator().Validate("IN", "29161230", false);
			AssertEquals("Tariff 29161230 is valid but not for Indonesia therefore this is not a valid ref tariffView for this purpose", false, validationData.IsValid);
			AssertEquals("NearestNomenclature should be blank", ZString.Empty, validationData.NearestNomenclature);
			AssertEquals("Tariff Description should be blank", ZString.Empty, validationData.Description);
			AssertEquals("Tariff DataGroupingName should be blank", ZString.Empty, validationData.DataGroupingName);
			AssertEquals("Nomenclature EndDate should be empty", ZDateTime.Empty, validationData.EndDate);
		}

		public void TestInvalidTariffWithFindNearest()
		{
			CreateZZTariffData(Factory);
			var validationData = new TariffValidator().Validate("ZA", "29161115", true);
			AssertEquals("Tariff 291611 for ZA is the nearest valid nomenclature ref and therefore should be produce valid response here.", true, validationData.IsValid);
			AssertEquals("Nomenclature 291611 Description", "Acrylic acid and its salts:", validationData.Description);
			AssertEquals("Nomenclature 291611 DataGroupingName", "Universal Customs", validationData.DataGroupingName);
			AssertEquals("Nomenclature 291611 EndDate", ZDateTime.UtcToday.AddYears(5), validationData.EndDate);
		}

		public void TestInvalidNomenclatureWithFindNearest()
		{
			var validationData = new TariffValidator().Validate("CN", "29161115", true);
			AssertEquals("Tariff 29161115 for CN is not a valid ref tariffView", false, validationData.IsValid);
			AssertEquals("NearestNomenclature should be blank", ZString.Empty, validationData.NearestNomenclature);
			AssertEquals("Tariff Description should be blank", ZString.Empty, validationData.Description);
			AssertEquals("Tariff DataGroupingName should be blank", ZString.Empty, validationData.DataGroupingName);
			AssertEquals("Nomenclature EndDate should be empty", ZDateTime.Empty, validationData.EndDate);
			validationData = new TariffValidator().Validate("IN", "29161230", true);
			AssertEquals("Tariff 29161230 is valid but not for Indonesia & IN does not belong to any nomenclature group there this is not a valid ref tariffView for this purpose", false, validationData.IsValid);
			AssertEquals("NearestNomenclature should be blank", ZString.Empty, validationData.NearestNomenclature);
			AssertEquals("Tariff Description should be blank", ZString.Empty, validationData.Description);
			AssertEquals("Tariff DataGroupingName should be blank", ZString.Empty, validationData.DataGroupingName);
			AssertEquals("Nomenclature EndDate should be empty", ZDateTime.Empty, validationData.EndDate);
		}

		public void TestHasCountryDataSet()
		{
			Assert("No Country Data Set", !TariffSearchHelper.GetHasCountryDataSet(Factory, Core.Constants.CountryCodes.SouthAfrica));
			CreateZZTariffData(Factory);
			Assert("Has Country Data Set", TariffSearchHelper.GetHasCountryDataSet(Factory, Core.Constants.CountryCodes.SouthAfrica));
		}

		static void CreateZZTariffData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var zzCountry = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			zzCountry.ZZZ_Description = "Universal Customs";
			var zaCountry = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			zaCountry.ZZZ_Description = "South Africa";
			zaCountry.ZZZ_ZZZ_Grouping = zzCountry.PK;
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AntiDumping, "AntiDumping");
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "NGT";
			factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "29161230", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "BUTYL ACRYLATE");
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "030325", new ZDateTime(2012, 01, 01), new ZDateTime(2016, 12, 31, 23, 59, 00), "CARP (CYPRINUS CARPIO, CARASSIUS CARASSIUS, CTENOPHARYNGODON IDELLUS, HYPOPHTHALMICHTHYS SPP., CIRRHINUS SPP., MYLOPHARYNGODON PICEUS)");
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "030325", new ZDateTime(2017, 01, 01), new ZDateTime(2079, 06, 06), "CARP (CYPRINUS SPP., CARASSIUS SPP., CTENOPHARYNGODON IDELLUS, HYPOPHT HALMICHTHYS SPP., CIRRHINUS SPP., MYLOPHARYNGODON PICEUS, CATLA CATLA, LABEO SPP., OSTEOCHILUS HASSELTI, LEPTOBARBUS HOEVENI, MEGALOBRAMA SP P.)");
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "291612", ZDateTime.BrettsBirthday, ZDateTime.UtcToday, "Alpha Bravo", "99...99.99", "NGT");
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "291611", ZDateTime.BrettsBirthday, ZDateTime.UtcToday.AddYears(5), "Acrylic acid and its salts:", "06.29.07.16.1.1", "NGT");
			factory.Save();
		}
		#endregion
	}
}
