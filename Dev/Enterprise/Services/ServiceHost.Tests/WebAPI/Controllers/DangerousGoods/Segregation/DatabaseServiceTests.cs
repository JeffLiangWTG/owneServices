using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation
{
	class DatabaseServiceTests : TestCaseWithFactory
	{
		public void TestFind_ShouldReturnItemsAndSubstances()
		{
			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstance1.DG_UNNO = "2910";

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_DG = undgSubstance1.PK;

			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstance2.DG_UNNO = "2911";

			var undgDataItem2 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem2.DI_DG = undgSubstance2.PK;

			var databaseService = new DatabaseService(Factory);
			var (classificationData1, substance1, errorMessage1) = databaseService.Find(undgDataItem1.PK.ToGuid(), UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
			var (classificationData2, substance2, errorMessage2) = databaseService.Find(undgDataItem2.PK.ToGuid(), UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
			AssertEquals(undgDataItem1.DI_QuantityClassification, classificationData1.QuantityClassification);
			AssertEquals(undgSubstance1.PK, substance1.PK);
			AssertEquals(undgDataItem2.DI_QuantityClassification, classificationData2.QuantityClassification);
			AssertEquals(undgSubstance2.PK, substance2.PK);
			AssertEquals(string.Empty, errorMessage1);
			AssertEquals(string.Empty, errorMessage2);
		}

		public void TestFind_ShouldFailIfTheStandardDoesNotMatch()
		{
			var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstance1.DG_UNNO = "2910";

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_DG = undgSubstance1.PK;

			var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstance2.DG_UNNO = "2911";

			var undgDataItem2 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem2.DI_DG = undgSubstance2.PK;

			var databaseService = new DatabaseService(Factory);
			var (classificationData1, substance1, errorMessage1) = databaseService.Find(undgDataItem1.PK.ToGuid(), UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			var (classificationData2, substance2, errorMessage2) = databaseService.Find(undgDataItem2.PK.ToGuid(), UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			AssertNull(classificationData1);
			AssertNull(substance1);
			AssertNull(classificationData2);
			AssertNull(substance2);
			AssertEquals($"Cannot find UNDG substance for standard {UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO} and UNDG data item with PK = {undgDataItem1.PK}", errorMessage1);
			AssertEquals($"Cannot find UNDG substance for standard {UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO} and UNDG data item with PK = {undgDataItem2.PK}", errorMessage2);
		}

		public void TestFindSubstanceFromUNDGDataItemDTO_ShouldReturnSubstanceThatMatchesStandard()
		{
			var undgSubstance1 = TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "1111", undgClass: "1", undgCode: "codeA", undgVariant: "vA", undgStandard: "IMO");
			var undgSubstance2 = TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "2222", undgClass: "2", undgCode: "codeB", undgVariant: "vB", undgStandard: "IAT");

			var undgDataItemDto = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
					new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
				}
			};

			var databaseService = new DatabaseService(Factory);
			var (classificationDataReturned, substance, errorMessage) = databaseService.Find(undgDataItemDto, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
			AssertEquals(undgDataItemDto.UNDGClassificationData, classificationDataReturned);
			AssertEquals(undgSubstance2.DG_UNNO, substance.DG_UNNO);
			AssertEquals(undgSubstance2.DG_Variant, substance.DG_Variant);
			AssertEquals(undgSubstance2.DG_Standard, substance.DG_Standard);
			AssertEquals(string.Empty, errorMessage);
		}

		public void TestFindSubstanceFromUNDGDataItemDTO_ShouldFailIfTheStandardDoesNotMatch()
		{
			TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "1111", undgClass: "1", undgCode: "codeA", undgVariant: "vA", undgStandard: "IAT");
			TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "2222", undgClass: "2", undgCode: "codeB", undgVariant: "vB", undgStandard: "IAT");
			TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "3333", undgClass: "3", undgCode: "codeC", undgVariant: "vC", undgStandard: "IAT");

			var undgDataItemDto = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
					new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
					new UNDGSubstanceDTO { Unno = "3333", Variant = "vC", Standard = "IAT" },
				}
			};

			var databaseService = new DatabaseService(Factory);
			var (classificationDataReturned, substance, errorMessage) = databaseService.Find(undgDataItemDto, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			AssertNull(substance);
			AssertEquals($"Cannot find UNDG substance for standard {UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO}", errorMessage);
		}

		public void TestFindSubstanceFromUNDGDataItemDTO_ShouldFailIfNoSubstanceIsFoundAgainstDTO()
		{
			TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "1111", undgClass: "1", undgCode: "codeA", undgVariant: "vA", undgStandard: "IAT");
			TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "2222", undgClass: "2", undgCode: "codeB", undgVariant: "vB", undgStandard: "IAT");
			TestHelper.CreateUNDGSubstance(factory: Factory, undgUNNOCode: "3333", undgClass: "3", undgCode: "codeC", undgVariant: "vC", undgStandard: "IAT");

			var undgDataItemDto = new UNDGDataItemDTO
			{
				UNDGSubstanceDTOs = new[]
				{
					new UNDGSubstanceDTO { Unno = "9999", Variant = "vX", Standard = "IAT" },
				}
			};

			var databaseService = new DatabaseService(Factory);
			var (classificationDataReturned, substance, errorMessage) = databaseService.Find(undgDataItemDto, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
			AssertNull(substance);
			AssertEquals("Cannot find UNDG substance with UNNO = 9999, Variant = vX, Standard = IAT", errorMessage);
		}
	}
}
