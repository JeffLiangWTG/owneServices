using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffBRCharacteristic))]
	sealed class RefCusTariffBRCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRefCusTariffBRCharacteristic_LoadCollections()
		{
			var characteristic = CreateRefCusTariffBRCharacteristic(Factory, Core.Constants.CountryCodes.Brazil, "HSN", "56049000");

			Factory.New<RefCusTariffBRCharacteristicAttribute>().ZB3_ZB1_Characteristic = characteristic.PK;
			Factory.New<RefCusTariffBRCharacteristicAttribute>().ZB3_ZB1_Characteristic = characteristic.PK;
			Factory.New<RefCusTariffBRCharacteristicValue>().ZB2_ZB1_Characteristic = characteristic.PK;
			Factory.New<RefCusTariffBRCharacteristicValue>().ZB2_ZB1_Characteristic = characteristic.PK;

			AssertEquals("RefCusTariffBRCharacteristicValueCollection count should be ", 2, characteristic.Values.Count);
			AssertEquals("RefCusTariffBRCharacteristicAttributeCollection count should be ", 2, characteristic.Attributes.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateRefCusTariffBRCharacteristic(Factory, Core.Constants.CountryCodes.Brazil, "HSN", "56049000");
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		public static RefCusTariffBRCharacteristic CreateRefCusTariffBRCharacteristic(BusinessObjectFactory factory, ZString countryCode, ZString tariffType, ZString tariffCode,
			string characteristicType = "NCM", string code = "AA", string style = Constants.ProfileQuestion.AnswerDataTypes.String)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(countryCode, tariffType).PK;
			factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffTypePK, tariffCode, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			factory.Save();

			return helper.CreateRefCusTariffBRCharacteristic(tariff, characteristicType, code, style);
		}
	}

	[TestedType(typeof(RefCusTariffBRCharacteristic.Loader))]
	class RefCusTariffBRCharacteristicLoaderTest : LoaderTestCase
	{
		public void TestLoadCharacteristics()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN", "NCM");
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "56049000", compositeKey: "56.04", ensureDataGroupingExists: false);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "57049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

			var nomenclatureGroup1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "5604", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56.04", "NCM", ensureDataGroupingExists: false);
			var nomenclatureGroup2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "56", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56", "NCM", ensureDataGroupingExists: false);

			helper.CreateRefCusTariffBRCharacteristic(tariff1, "NVE", "T1", isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff1, "NVE", "T2", startDate: ZDate.Today.AddDays(-15), endDate: ZDate.Today.AddDays(-5));
			helper.CreateRefCusTariffBRCharacteristic(tariff1, "NVE", "T3", startDate: ZDate.Today.AddDays(5), endDate: ZDate.Today.AddDays(15));
			helper.CreateRefCusTariffBRCharacteristic(tariff2, "NVE", "T4");
			helper.CreateRefCusTariffBRCharacteristic(tariff1, "NCM", "T5");
			helper.CreateRefCusTariffBRCharacteristic(tariff1, "NVE", "T6", isExport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff1, "NVE", "T7");

			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup1, "NCM", "N1");
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup1, "NVE", "N2");
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup2, "NVE", "N3", isExport: false);
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup2, "NVE", "N4", isImport: false);

			Factory.Save();

			var loader = new RefCusTariffBRCharacteristic.Loader(Factory);
			var brCharacteristics = loader.LoadCharacteristics(tariff1, "NVE", CharacteristicDirection.Import, ZDateTime.Today, true);
			AssertContainsExactElementsInExactOrder(new string[] { "N2", "N3", "T6", "T7" }, brCharacteristics.Select(x => x.ZB1_Code));

			brCharacteristics = loader.LoadCharacteristics(tariff1, "NVE", CharacteristicDirection.Import, ZDateTime.Today, false);
			AssertContainsExactElementsInExactOrder(new string[] { "T6", "T7" }, brCharacteristics.Select(x => x.ZB1_Code));

			brCharacteristics = loader.LoadCharacteristics(tariff1, "NVE", CharacteristicDirection.Export, ZDateTime.Today, true);
			AssertContainsExactElementsInExactOrder(new string[] { "N2", "N4", "T1", "T7" }, brCharacteristics.Select(x => x.ZB1_Code));

			brCharacteristics = loader.LoadCharacteristics(tariff1, "NVE", CharacteristicDirection.Export, ZDateTime.Today, false);
			AssertContainsExactElementsInExactOrder(new string[] { "T1", "T7" }, brCharacteristics.Select(x => x.ZB1_Code));

			brCharacteristics = loader.LoadCharacteristics(tariff1, "NVE", CharacteristicDirection.Both, ZDateTime.Today, true);
			AssertContainsExactElementsInExactOrder(new string[] { "N2", "N3", "N4", "T1", "T6", "T7" }, brCharacteristics.Select(x => x.ZB1_Code));

			brCharacteristics = loader.LoadCharacteristics(tariff1, "NVE", CharacteristicDirection.Both, ZDateTime.Today, false);
			AssertContainsExactElementsInExactOrder(new string[] { "T1", "T6", "T7" }, brCharacteristics.Select(x => x.ZB1_Code));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new RefCusTariffBRCharacteristic.Loader(Factory);
	}
}
