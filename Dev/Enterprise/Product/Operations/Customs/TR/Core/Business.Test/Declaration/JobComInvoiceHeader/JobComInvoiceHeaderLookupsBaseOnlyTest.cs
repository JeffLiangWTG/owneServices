using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderLookups))]
	class JobComInvoiceHeaderLookupsBaseOnlyTest : JobComInvoiceHeaderLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceHeaderLookups GetLookups() => new JobComInvoiceHeaderLookups(invoice);

		public void TestJZ_IncoTerm_List()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default List", 11, lookups.JZ_IncoTerm_List.Count);
				AssertType<TRIncotermCodeList>("Type", lookups.JZ_IncoTerm_List);
			});
		}

		public void TestRelationCodeList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(RelationCodeList.Codes.Y, RelationCodeList.Descriptions.Y),
				new CodeDescriptionPair(RelationCodeList.Codes.N, RelationCodeList.Descriptions.N),
			}, lookups.RelationCodeList);
		}

		public void TestValuationCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var trGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusCodeType("TRNOB", "TR Valuation Code List");
			helper.CreateCusCodeType("TRMON", "Non-TR Valuation Code List");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var today = ZDateTime.Today;
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRNOB", "43", "Valid Item 1", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRNOB", "45", "Valid Item 2", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRMON", "43", "Invalid Code Type", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRNOB", "46", "Invalid Date", yesterday, yesterday);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TRNOB", "43", "Wrong Country", today, tomorrow);

			Factory.Save();

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRNOB", "44", "Valid Item 3", today, tomorrow);

			Factory.Save();

			var list = lookups.ValuationCodeList;

			AssertEquals(3, list.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Code 43 should exist", true, lookups.ValuationCodeList.ContainsCode("43"));
				AssertEquals("Description should be Valid Item 1 to ensure the correct type", "Valid Item 1", lookups.ValuationCodeList.GetDescriptionFromCode("43"));
				AssertEquals("Description should be Valid Item 2 to ensure the correct type", "Valid Item 2", lookups.ValuationCodeList.GetDescriptionFromCode("45"));
				AssertEquals("First Item", "43", (list[0] as ICodeDescription).Code);
				AssertEquals("Second Item", "44", (list[1] as ICodeDescription).Code);
				AssertEquals("Third Item", "45", (list[2] as ICodeDescription).Code);
			});

			var list1 = lookups.ValuationCodeList;
			var list2 = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().Lookups.ValuationCodeList;

			AssertSame("Is Cached", list1, list2);
		}
	}
}
