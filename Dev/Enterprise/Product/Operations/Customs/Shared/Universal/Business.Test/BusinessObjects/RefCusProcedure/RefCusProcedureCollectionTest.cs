using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedureCollection))]
	class RefCusProcedureCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusProcedureCollection>
	{
		public void TestCountrySpecificLoading()
		{
			CombineAssertions(() =>
			{
				var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure1.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
				testCusProcedure1.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
				var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure2.ZZ6_StartDate = new ZDateTime(2017, 1, 1);
				testCusProcedure2.ZZ6_EndDate = new ZDateTime(2017, 12, 31);
				Factory.Save();
				var testCollection = new RefCusProcedureCollection(Factory, CoreConstants.CountryCodes.Australia, ZDateTime.Today);
				AssertEquals("No Procedure for AU", 0, testCollection.Count);
				testCollection = new RefCusProcedureCollection(Factory, CoreConstants.CountryCodes.UnitedStates, ZDateTime.Today);
				AssertEquals("No Procedure for US", 0, testCollection.Count);
				testCollection = new RefCusProcedureCollection(Factory, CoreConstants.CountryCodes.Italy, new ZDateTime(2016, 6, 8));
				AssertEquals("One Procedure for Italy", 1, testCollection.Count);
				AssertEquals("One Procedure for Italy", testCusProcedure1.PK, testCollection[0].PK);
				testCollection = new RefCusProcedureCollection(Factory, CoreConstants.CountryCodes.Italy, new ZDateTime(2017, 6, 8));
				AssertEquals("One Procedure for Italy", 1, testCollection.Count);
				AssertEquals("One Procedure for Italy", testCusProcedure2.PK, testCollection[0].PK);
			}

			);
		}

		public void TestFilterConstants()
		{
			AssertEquals("Procedure Code", RefCusProcedureCollection.FilterConstants.ProcedureCode);
		}

		public void TestLoadCustomsProcedureCodesForCountryAndShipmentType()
		{
			CombineAssertions(() =>
			{
				var testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_ProcedureCode = "11";
				testCusProcedure.ZZ6_PreviousProcedureCode = string.Empty;
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.UnitedKingdom;
				testCusProcedure.ZZ6_ShipmentType = "IMP";
				testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_PreviousProcedureCode = string.Empty;
				testCusProcedure.ZZ6_ProcedureCode = "11";
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure.ZZ6_ShipmentType = "EXP";
				testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_ProcedureCode = "11";
				testCusProcedure.ZZ6_PreviousProcedureCode = "12";
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure.ZZ6_ShipmentType = "EXP";
				testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_ProcedureCode = "13";
				testCusProcedure.ZZ6_PreviousProcedureCode = "";
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure.ZZ6_ShipmentType = "EXW";
				testCusProcedure.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
				testCusProcedure.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
				testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_ProcedureCode = "13";
				testCusProcedure.ZZ6_PreviousProcedureCode = "";
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure.ZZ6_ShipmentType = "EXW";
				testCusProcedure.ZZ6_StartDate = new ZDateTime(2017, 1, 1);
				testCusProcedure.ZZ6_EndDate = new ZDateTime(2017, 12, 31);
				Factory.Save();
				var testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, CoreConstants.CountryCodes.UnitedKingdom, "IMP", ZDateTime.Today);
				AssertEquals("1 Procedure for GB in IMP", 1, testCollection.Count);
				testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, CoreConstants.CountryCodes.UnitedKingdom, "EXP", ZDateTime.Today);
				AssertEquals("0 Procedure for GB in EXP", 0, testCollection.Count);
				testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, CoreConstants.CountryCodes.Italy, "IMP", ZDateTime.Today);
				AssertEquals("0 Procedure for IT in IMP", 0, testCollection.Count);
				testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, CoreConstants.CountryCodes.Italy, "EXP", ZDateTime.Today);
				AssertEquals("1 Procedure for IT in EXP", 1, testCollection.Count);
				testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, CoreConstants.CountryCodes.Italy, "EXW", new ZDateTime(2016, 6, 8));
				AssertEquals("1 Procedure for IT in EXW", 1, testCollection.Count);
				AssertEquals("1 Procedure for IT in EXW", new ZDateTime(2016, 1, 1), testCollection[0].ZZ6_StartDate);
				testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, CoreConstants.CountryCodes.Italy, "EXW", new ZDateTime(2017, 6, 8));
				AssertEquals("1 Procedure for IT in EXW", 1, testCollection.Count);
				AssertEquals("1 Procedure for IT in EXW", new ZDateTime(2017, 1, 1), testCollection[0].ZZ6_StartDate);
				testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountry(Factory, CoreConstants.CountryCodes.Italy, new ZDateTime(2017, 6, 8));
				AssertEquals("2 Procedure for IT", 2, testCollection.Count);
			}

			);
		}

		public void TestLoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode()
		{
			CombineAssertions(() =>
			{
				var testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure.ZZ6_ProcedureCode = "11";
				testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure.ZZ6_PreviousProcedureCode = "12";
				testCusProcedure.ZZ6_ShipmentType = "AAA";
				var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure1.ZZ6_ProcedureCode = "11";
				testCusProcedure1.ZZ6_PreviousProcedureCode = "40";
				testCusProcedure1.ZZ6_ShipmentType = "BBB";
				testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure1.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
				testCusProcedure1.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
				var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure2.ZZ6_ProcedureCode = "11";
				testCusProcedure2.ZZ6_PreviousProcedureCode = "40";
				testCusProcedure2.ZZ6_ShipmentType = "BBB";
				testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure2.ZZ6_StartDate = new ZDateTime(2017, 1, 1);
				testCusProcedure2.ZZ6_EndDate = new ZDateTime(2017, 12, 31);
				Factory.Save();
				var testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "AAA", "11", new ZDateTime(2016, 6, 8));
				AssertEquals("1 Procedure for Italy AAA", 1, testCollection.Count);
				AssertEquals("CPC - AAA", "11", testCollection[0].ZZ6_ProcedureCode);
				AssertEquals("PPC - AAA", "12", testCollection[0].ZZ6_PreviousProcedureCode);
				testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "BBB", "11", new ZDateTime(2016, 6, 8));
				AssertEquals("1 Procedure for Italy BBB", 1, testCollection.Count);
				AssertEquals("CPC - BBB", "11", testCollection[0].ZZ6_ProcedureCode);
				AssertEquals("PPC - BBB", "40", testCollection[0].ZZ6_PreviousProcedureCode);
				AssertEquals("PPC - BBB", testCusProcedure1.PK, testCollection[0].PK);
				testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "11", new ZDateTime(2016, 6, 8));
				AssertEquals("2 Procedure for Italy", 2, testCollection.Count);
				AssertArrayEqualsByElements(new RefCusProcedure[] { testCusProcedure, testCusProcedure1 }, testCollection.ToArray());
			}

			);
		}

		public void TestLoadPreviousProceduresCodesForCountryShipmentTypeProcedureCodeEmptyConcession()
		{
			var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = "11";
			testCusProcedure1.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure1.ZZ6_ShipmentType = "BBB";
			testCusProcedure1.ZZ6_Concession = string.Empty;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure1.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure1.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = "11";
			testCusProcedure2.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure2.ZZ6_ShipmentType = "BBB";
			testCusProcedure2.ZZ6_Concession = "1V1";
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure2.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure2.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			Factory.Save();
			var testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "BBB", "11", new ZDateTime(2016, 6, 8));
			AssertEquals("2 Procedure for Italy BBB (with and without concession)", 2, testCollection.Count);
			testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "BBB", "11", new ZDateTime(2016, 6, 8), true);
			AssertEquals("1 Procedure for Italy BBB (without concession)", 1, testCollection.Count);
		}

		public void TestLoadPreviousProceduresCodesForCountryShipmentTypeProcedureCodeWithCategory()
		{
			var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = "11";
			testCusProcedure1.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure1.ZZ6_ShipmentType = "BBB";
			testCusProcedure1.ZZ6_Concession = string.Empty;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure1.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure1.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			testCusProcedure1.ZZ6_Category = "1";
			var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = "11";
			testCusProcedure2.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure2.ZZ6_ShipmentType = "BBB";
			testCusProcedure2.ZZ6_Concession = "1V1";
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure2.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure2.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			testCusProcedure2.ZZ6_Category = "2";
			Factory.Save();
			var testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "BBB", "11", new ZDateTime(2016, 6, 8), false, null);
			AssertEquals("2 Procedure for Italy BBB without category", 2, testCollection.Count);
			testCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, CoreConstants.CountryCodes.Italy, "BBB", "11", new ZDateTime(2016, 6, 8), false, "1");
			AssertEquals("1 Procedure for Italy BBB with category", 1, testCollection.Count);
		}

		public void TestLoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode()
		{
			var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = "11";
			testCusProcedure1.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure1.ZZ6_ShipmentType = "BBB";
			testCusProcedure1.ZZ6_Concession = string.Empty;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure1.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure1.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = "11";
			testCusProcedure2.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure2.ZZ6_ShipmentType = "BBB";
			testCusProcedure2.ZZ6_Concession = "1V1";
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure2.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure2.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			var testCusProcedure3 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure3.ZZ6_ProcedureCode = "11";
			testCusProcedure3.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure3.ZZ6_ShipmentType = "BBB";
			testCusProcedure3.ZZ6_Concession = "AA1";
			testCusProcedure3.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure3.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure3.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			Factory.Save();
			var testCollection = RefCusProcedureCollection.LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(Factory,
				CoreConstants.CountryCodes.Italy, "BBB", "11", "40", new ZDateTime(2016, 6, 8));
			AssertEquals("2 Concessions for Italy BBB - 11 - 40", 2, testCollection.Count);
		}

		public void TestLoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCodeWithCategory()
		{
			var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = "11";
			testCusProcedure1.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure1.ZZ6_ShipmentType = "BBB";
			testCusProcedure1.ZZ6_Concession = string.Empty;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure1.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure1.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			testCusProcedure1.ZZ6_Category = "1";
			var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = "11";
			testCusProcedure2.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure2.ZZ6_ShipmentType = "BBB";
			testCusProcedure2.ZZ6_Concession = "1V1";
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure2.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure2.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			testCusProcedure2.ZZ6_Category = "2";
			var testCusProcedure3 = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure3.ZZ6_ProcedureCode = "11";
			testCusProcedure3.ZZ6_PreviousProcedureCode = "40";
			testCusProcedure3.ZZ6_ShipmentType = "BBB";
			testCusProcedure3.ZZ6_Concession = "AA1";
			testCusProcedure3.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure3.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure3.ZZ6_EndDate = new ZDateTime(2016, 12, 31);
			testCusProcedure3.ZZ6_Category = "1";
			Factory.Save();

			var testCollection = RefCusProcedureCollection.LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(Factory,
				CoreConstants.CountryCodes.Italy, "BBB", "11", "40", new ZDateTime(2016, 6, 8));
			AssertEquals("2 Concessions for Italy BBB without category", 2, testCollection.Count);

			testCollection = RefCusProcedureCollection.LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(Factory,
				CoreConstants.CountryCodes.Italy, "BBB", "11", "40", new ZDateTime(2016, 6, 8), "1");
			AssertEquals("1 Concessions for Italy BBB - 11 - 40", 1, testCollection.Count);
		}

		public void TestGetDescriptionFromCodeShouldUseCorrectCountryCode()
		{
			var itProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			itProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			itProcedure.ZZ6_ProcedureCode = "40";
			itProcedure.ZZ6_Description = "40 From IT";
			var gbProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			gbProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.UnitedKingdom;
			gbProcedure.ZZ6_ProcedureCode = "40";
			gbProcedure.ZZ6_Description = "40 From GB";
			Factory.Save();
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CoreConstants.CountryCodes.Italy))
			{
				var collection = new RefCusProcedureCollection(Factory, CoreConstants.CountryCodes.UnitedKingdom, ZDateTime.Now);
				var description = collection.GetDescriptionFromCode("40");
				AssertEquals("It should return the GB code because we specify the country code as GB explicitly.", "40 From GB", description);
			}
		}

		public void TestLoadCustomsProcedureCodesForCountryAndShipmentTypeAndGroup()
		{
			var pattern1 = "11******,12******";
			var pattern2 = "**12****";
			var pattern3 = "**11****";

			CombineAssertions(() =>
			{
				var testCusProcedure1 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure1.ZZ6_ShipmentType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testCusProcedure1.ZZ6_Group = $"{pattern1},{pattern2}";

				var testCusProcedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure2.ZZ6_ShipmentType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testCusProcedure2.ZZ6_Group = pattern3;

				var testCusProcedure3 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure3.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure3.ZZ6_ShipmentType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testCusProcedure3.ZZ6_Group = pattern2;

				var testCusProcedure4 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure4.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.UnitedKingdom;
				testCusProcedure4.ZZ6_ShipmentType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testCusProcedure4.ZZ6_Group = pattern2;

				var testCusProcedure5 = Factory.NewWithValidTestData<RefCusProcedure>();
				testCusProcedure5.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
				testCusProcedure5.ZZ6_ShipmentType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testCusProcedure5.ZZ6_Group = pattern2;
				testCusProcedure5.ZZ6_StartDate = new ZDateTime(2019, 1, 1);
				testCusProcedure5.ZZ6_EndDate = new ZDateTime(2019, 12, 31);
				Factory.Save();

				var testCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentTypeAndGroup(Factory, CoreConstants.CountryCodes.Italy, Common.Shared.SharedJobMessageTypeList.Codes.Import, ZDateTime.Today, new[] { pattern2, pattern3 });
				var completeFilter = testCollection.CompleteFilter;
				AssertEquals("Contains pattern2", true, testCusProcedure1.MatchesFilter(completeFilter));
				AssertEquals("Matched with pattern3", true, testCusProcedure2.MatchesFilter(completeFilter));
				AssertEquals("Unmatched ShipmentType", false, testCusProcedure3.MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroup", false, testCusProcedure4.MatchesFilter(completeFilter));
				AssertEquals("Unmatched Date", false, testCusProcedure5.MatchesFilter(completeFilter));
			});
		}

		protected override RefCusProcedureCollection GetCollectionToTest()
		{
			return new RefCusProcedureCollection(Factory, string.Empty, ZDateTime.Today);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(CoreConstants.CountryCodes.Italy);
			helper.CreateNewOrGetExistingDataGrouping(CoreConstants.CountryCodes.UnitedKingdom);
		}
	}
}
