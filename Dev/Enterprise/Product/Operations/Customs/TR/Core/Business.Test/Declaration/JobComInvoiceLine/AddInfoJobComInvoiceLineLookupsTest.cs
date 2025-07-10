using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUsedGoodsCodeList()
		{
			var list = invoiceLine.Lookups.UsedGoodsCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "K1, K2, K3", list.CodesAsString);
				AssertSame(Factory.GetCachedValue<UsedGoodsCodeList>(), list);
			});
		}

		public void TestReturningGoodsReasonCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, "0", "Diğer (Açıklama Belirtiniz)", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, "10", "Kimyasal ve teknolojik olaylar (Radyasyon ve hava kirliliği gibi)", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, "1", "Sözleşme hükümlerine uygun olmaması", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, "2", "Nakliye sırasında hasar görmesi", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, "3", "Eşyanın tabiatından kaynaklı hasar", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var list = invoiceLine.Lookups.ReturningGoodsReasonCodeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("Code list", new[] { "0", "1", "10", "2", "3" }, list.Select(x => x.ZZD_Code));
				AssertEquals("Cached", list, invoiceLine.Lookups.ReturningGoodsReasonCodeList);
			});
		}

		public void TestExportUnionPackageCodeList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TREUP", "TREUP");

			helper.CreateCusCodeList("TR", "TREUP", "A0001", "TREUP Test 1", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "TREUP", "A0002", "TREUP Test 1", yesterday, tomorrow);

			Factory.Save();
			var list = invoiceLine.Lookups.ExportUnionPackageCodeList;
			list.Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0002"));
		}

		public void TestThreadCodeList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TREUT", "TREUT");

			helper.CreateCusCodeList("TR", "TREUT", "A0001", "TREUT Test 1", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "TREUT", "A0002", "TREUT Test 1", yesterday, tomorrow);

			Factory.Save();
			var list = invoiceLine.Lookups.ThreadCodeList;
			list.Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0002"));
		}

		public void TestExportUnionAdditionalTariffList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Turkey, TariffTypes.ExportUnionAdditional).PK;
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffTypePK, "A088000101", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			Factory.Save();

			var secondaryPreferences = invoiceLine.Lookups.ExportUnionAdditionalTariffList;
			AssertSame("Cached", TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, TariffTypes.ExportUnionAdditional, ZDateTime.Today), secondaryPreferences);
			AssertEquals("Tariff Matches", true, tariff.MatchesFilter(secondaryPreferences.CompleteFilter));
		}

		public void TestEntryExitPurposeCodeList()
		{
			var list = invoiceLine.Lookups.EntryExitPurposeCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "01, 02, 03, 04, 05", list.CodesAsString);
				AssertSame(Factory.GetCachedValue<EntryExitPurposeCodeList>(), list);
			});
		}

		public void TestPaymentCodeList()
		{
			var list = invoiceLine.Lookups.InvoicePaymentCodeList;
			CombineAssertions("InvoicePaymentCodeList", () =>
			{
				AssertEquals("CodeAsString", "1, 2, 3, 4, 6, 7, 11, 12, 13, 14, 15, 16, 17", list.CodesAsString);
				AssertSame(Factory.GetCachedValue<InvoicePaymentCodeList>(), list);
			});
		}

		public void TestPriceTypeList()
		{
			var list = invoiceLine.Lookups.PriceTypeList;
			CombineAssertions("PriceTypeList", () =>
			{
				AssertEquals("CodeAsString", "01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 58, 77, 88, 99", list.CodesAsString);
				AssertSame(Factory.GetCachedValue<PriceTypeList>(), list);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			this.invoiceLine = invoiceLine.AddInfo;
		}
		AddInfoJobComInvoiceLine invoiceLine;
	}
}
