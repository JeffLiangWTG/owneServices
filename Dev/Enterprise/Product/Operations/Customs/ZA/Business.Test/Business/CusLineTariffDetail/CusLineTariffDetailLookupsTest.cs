using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusLineTariffDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQuantityUnitList()
		{
			var tariffDetail = Factory.New<CusLineTariffDetail>();
			AssertSame("Is cached", Factory.GetCachedValue<QuantityCodeList>(), tariffDetail.Lookups.QuantityUnitList);
		}

		public void TestTariffTypeList()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInvoiceLine = testDeclaration.InvoiceLines.AddNew();
			var testInvoiceTariffDetail = testInvoiceLine.CusLineTariffDetails.AddNew();
			var testLookup = testInvoiceTariffDetail.Lookups;
			var query = new ZQuery(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			query.OrderBy = RefCusTariffTypeSchema.Constants.ZZI_TariffType;
			var selectedTariffTypes = Factory.Load<RefCusTariffType>(query);
			var tariffTypeList = testLookup.TariffTypeList;
			AssertSame("Is cached", tariffTypeList, testLookup.TariffTypeList);
			AssertEquals("Type Count", selectedTariffTypes.Length - 1, tariffTypeList.Count);
			AssertEquals("no 1P1", false, tariffTypeList.ContainsCode(UniversalReferenceConstants.CusTariffCode.Schedule1Part1));
		}

		public void TestTariffCollection_UsingWildcardSearchingForNonSche1()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
			var tariffType12B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			var tariffType13A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			var tariffType2P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			var tariffType2P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P2");
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType3P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P2");
			var tariffType4P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			var tariffType4P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P2");
			var tariffType5P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P1");
			var tariffType5P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P2");
			var tariffType6P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var tariffType6P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P2");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
			var tariffType1PK = tariff1P1.ZZ1_ZZI_TariffType;
			var tariff12A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1010201010", startDate, endDate);
			var tariff12B = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1020201010", startDate, endDate);
			var tariff13A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType13A.PK, "1030201010", startDate, endDate);
			var tariff2P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P1.PK, "2040201010", startDate, endDate);
			var tariff2P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P2.PK, "2050201010", startDate, endDate);
			var tariff3P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3060201010", startDate, endDate);
			var tariff3P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P2.PK, "3070201010", startDate, endDate);
			var tariff4P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4080201010", startDate, endDate);
			var tariff4P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "4090201010", startDate, endDate);
			var tariff5P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P1.PK, "5100201010", startDate, endDate);
			var tariff5P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P2.PK, "5110201010", startDate, endDate);
			var tariff6P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6120201010", startDate, endDate);
			var tariff6P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P2.PK, "6130201010", startDate, endDate);
			var relationship01 = helper.CreateTariffRelationship(tariff12A.PK, tariffType1PK, "101010");
			var relationship02 = helper.CreateTariffRelationship(tariff12B.PK, tariffType1PK, "101010");
			var relationship03 = helper.CreateTariffRelationship(tariff13A.PK, tariffType1PK, "101010");
			var relationship04 = helper.CreateTariffRelationship(tariff2P1.PK, tariffType1PK, "101010");
			var relationship05 = helper.CreateTariffRelationship(tariff2P2.PK, tariffType1PK, "101010");
			var relationship06 = helper.CreateTariffRelationship(tariff3P1.PK, tariffType1PK, "101010");
			var relationship07 = helper.CreateTariffRelationship(tariff3P2.PK, tariffType1PK, "101010");
			var relationship08 = helper.CreateTariffRelationship(tariff4P1.PK, tariffType1PK, "101010");
			var relationship09 = helper.CreateTariffRelationship(tariff4P2.PK, tariffType1PK, "101010");
			var relationship10 = helper.CreateTariffRelationship(tariff5P1.PK, tariffType1PK, "101010");
			var relationship11 = helper.CreateTariffRelationship(tariff5P2.PK, tariffType1PK, "101010");
			var relationship12 = helper.CreateTariffRelationship(tariff6P1.PK, tariffType1PK, "101010");
			var relationship13 = helper.CreateTariffRelationship(tariff6P2.PK, tariffType1PK, "101010");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1010101010";
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "1";
			AssertMatchesFilter(t12A: true, t12B: true, t13A: true);
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertMatchesFilter(t12A: true);
			tariffDetail.BZ_Type = "12B";
			AssertMatchesFilter(t12B: true);
			tariffDetail.BZ_Type = "13A";
			AssertMatchesFilter(t13A: true);
			tariffDetail.BZ_Type = "13B";
			AssertMatchesFilter();
			tariffDetail.BZ_Type = "2";
			AssertMatchesFilter(t2P1: true, t2P2: true);
			tariffDetail.BZ_Type = "2P1";
			AssertMatchesFilter(t2P1: true, t2P2: true);
			tariffDetail.BZ_Type = "2P2";
			AssertMatchesFilter(t2P1: true, t2P2: true);
			tariffDetail.BZ_Type = "2P3";
			AssertMatchesFilter(t2P1: true, t2P2: true);
			tariffDetail.BZ_Type = "3";
			AssertMatchesFilter(t3P1: true, t3P2: true);
			tariffDetail.BZ_Type = "3P1";
			AssertMatchesFilter(t3P1: true, t3P2: true);
			tariffDetail.BZ_Type = "3P2";
			AssertMatchesFilter(t3P1: true, t3P2: true);
			tariffDetail.BZ_Type = "3P3";
			AssertMatchesFilter(t3P1: true, t3P2: true);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "4";
			AssertMatchesFilter(t4P1: true, t4P2: true);
			tariffDetail.BZ_Type = "4P1";
			AssertMatchesFilter(t4P1: true, t4P2: true);
			tariffDetail.BZ_Type = "4P2";
			AssertMatchesFilter(t4P1: true, t4P2: true);
			tariffDetail.BZ_Type = "4P3";
			AssertMatchesFilter(t4P1: true, t4P2: true);
			void AssertMatchesFilter(bool t12A = false, bool t12B = false, bool t13A = false, bool t2P1 = false, bool t2P2 = false, bool t3P1 = false, bool t3P2 = false, bool t4P1 = false, bool t4P2 = false, bool t5P1 = false, bool t5P2 = false, bool t6P1 = false, bool t6P2 = false)
			{
				CombineAssertions("Tariff Detail Type: " + tariffDetail.BZ_Type, () =>
				{
					var filter = GetFilter(tariffDetail.Lookups.TariffCollection);
					AssertEquals(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, t12A, tariff12A.MatchesFilter(filter));
					AssertEquals("12B", t12B, tariff12B.MatchesFilter(filter));
					AssertEquals("13A", t13A, tariff13A.MatchesFilter(filter));
					AssertEquals("2P1", t2P1, tariff2P1.MatchesFilter(filter));
					AssertEquals("2P2", t2P2, tariff2P2.MatchesFilter(filter));
					AssertEquals("3P1", t3P1, tariff3P1.MatchesFilter(filter));
					AssertEquals("3P2", t3P2, tariff3P2.MatchesFilter(filter));
					AssertEquals("4P1", t4P1, tariff4P1.MatchesFilter(filter));
					AssertEquals("4P2", t4P2, tariff4P2.MatchesFilter(filter));
					AssertEquals("5P1", t5P1, tariff5P1.MatchesFilter(filter));
					AssertEquals("5P2", t5P2, tariff5P2.MatchesFilter(filter));
					AssertEquals("6P1", t6P1, tariff6P1.MatchesFilter(filter));
					AssertEquals("6P2", t6P2, tariff6P2.MatchesFilter(filter));
				});
			}
		}

		public void TestTariffCollection()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
			var tariffType2P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P2");
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType4P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			var tariffType5P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P1");
			var tariffType6P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
			var tariff12A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1020101010", startDate, endDate);
			var tariff2P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P2.PK, "2020101010", startDate, endDate);
			var tariff3P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3020101010", startDate, endDate);
			var tariff4P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4020101010", startDate, endDate);
			var tariff5P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P1.PK, "5020101010", startDate, endDate);
			var tariff6P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6020101010", startDate, endDate);
			var tariff12A2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1030101010", startDate, endDate);
			var tariff2P22 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P2.PK, "2030101010", startDate, endDate);
			var tariff3P12 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3030101010", startDate, endDate);
			var tariff4P12 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4030101010", startDate, endDate);
			var tariffType1PK = tariff1P1.ZZ1_ZZI_TariffType;
			var relationship1 = helper.CreateTariffRelationship(tariff12A.PK, tariffType1PK, "101010");
			var relationship2 = helper.CreateTariffRelationship(tariff2P2.PK, tariffType1PK, "101010");
			var relationship3 = helper.CreateTariffRelationship(tariff3P1.PK, tariffType1PK, "101010");
			var relationship4 = helper.CreateTariffRelationship(tariff4P1.PK, tariffType1PK, "101010");
			var relationship5 = helper.CreateTariffRelationship(tariff5P1.PK, tariffType1PK, "101010");
			var relationship6 = helper.CreateTariffRelationship(tariff6P1.PK, tariffType1PK, "101010");
			var relationship7 = helper.CreateTariffRelationship(tariff4P12.PK, tariffType1PK, "101010");
			var relationship8 = helper.CreateTariffRelationship(tariff3P12.PK, tariff12A.ZZ1_ZZI_TariffType, "102010");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "4P1";
			MatchesFilter(t4P1: true, t4P12: true);
			tariffDetail.BZ_Type = "5P1";
			MatchesFilter(t5P1: true);
			tariffDetail.BZ_Type = "6P1";
			MatchesFilter(t6P1: true);
			tariffDetail.BZ_Type = "3P1";
			MatchesFilter(t3P1: true);
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			MatchesFilter(t12A: true);
			tariffDetail.BZ_Type = "2P2";
			MatchesFilter(t2P2: true);
			tariffDetail.BZ_Type = "3P1";
			MatchesFilter(t3P1: true);
			var tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail2.BZ_Tariff = "1020101010";
			invoiceLine.CusLineTariffDetails.Where(x => x != tariffDetail && x != tariffDetail2).DeleteAll();
			tariffDetail.BZ_Type = "3P1";
			MatchesFilter(t3P1: true, t3P12: true);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "4P1";
			MatchesFilter(t4P1: true, t4P12: true);
			void MatchesFilter(bool t1P1 = false, bool t12A = false, bool t2P2 = false, bool t3P1 = false, bool t4P1 = false, bool t5P1 = false, bool t6P1 = false, bool t12A2 = false, bool t2P22 = false, bool t3P12 = false, bool t4P12 = false)
			{
				CombineAssertions("Tariff Detail Type: " + tariffDetail.BZ_Type, () =>
				{
					var filter = GetFilter(tariffDetail.Lookups.TariffCollection);
					AssertEquals(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, t1P1, tariff1P1.MatchesFilter(filter));
					AssertEquals("12B", t12A, tariff12A.MatchesFilter(filter));
					AssertEquals("2P2", t2P2, tariff2P2.MatchesFilter(filter));
					AssertEquals("3P1", t3P1, tariff3P1.MatchesFilter(filter));
					AssertEquals("4P1", t4P1, tariff4P1.MatchesFilter(filter));
					AssertEquals("5P1", t5P1, tariff5P1.MatchesFilter(filter));
					AssertEquals("6P1", t6P1, tariff6P1.MatchesFilter(filter));
					AssertEquals("12A v2", t12A2, tariff12A2.MatchesFilter(filter));
					AssertEquals("2P2 v2", t2P22, tariff2P22.MatchesFilter(filter));
					AssertEquals("3P1 v2", t3P12, tariff3P12.MatchesFilter(filter));
					AssertEquals("4P1 v2", t4P12, tariff4P12.MatchesFilter(filter));
				});
			}
		}

		public void TestTariffRestrictionFilter()
		{
			var tariffType6P1 = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDateTomorrow = ZDateTime.Today.AddDays(2);
			var tariff6P1 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6020101010", startDate, endDateTomorrow);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "6#";
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "3#";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "6P1";
			var tariffCollection = tariffDetail.Lookups.TariffCollection;
			AssertEquals(false, tariffCollection.FilterBusinessObjectDefaults.ContainsDefaultFor(Customs.Universal.Constants.RefCusTariffFilters.TariffRestriction + ":Property"));
			invoiceLine.JI_CEI = entryInstruction2.PK;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "3P1";
			tariffCollection = tariffDetail.Lookups.TariffCollection;
			AssertEquals(false, tariffCollection.FilterBusinessObjectDefaults.ContainsDefaultFor(Customs.Universal.Constants.RefCusTariffFilters.TariffRestriction + ":Property"));
		}

		ZQuery GetFilter(ChildTariffViewCollection tariffCollection)
		{
			var filter = tariffCollection.CompleteFilter;
			var tariffQuery = new ZDBOnlyQuery(typeof(TariffView));
			if (tariffCollection.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>().Any(x => x.Key.StartsWith(Customs.Universal.Constants.RefCusTariffFilters.TariffRestriction)))
			{
				tariffQuery.AddFilterAndZSQLParameterCollection((ZString)tariffCollection.FilterBusinessObjectDefaults[Customs.Universal.Constants.RefCusTariffFilters.TariffRestriction + ":Property"].Value, new ZSqlParameterCollection());
			}

			filter.AddToFilter(tariffQuery);
			return filter;
		}

		protected override void SetUp()
		{
			base.SetUp();
			universalTestHelper = new ZAUniversalReferenceTestDataHelper(Factory);
		}

		ZAUniversalReferenceTestDataHelper universalTestHelper;
	}
}
