using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceLineLookups))]
	abstract class JobComInvoiceLineLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		public void TestNatureOfTransactionList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness,
				"01",
				"Eşyanın İade Edilmesi - Geri Gelen Eşya",
				ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness,
				"02",
				"İade Edilen Eşyanın Değiştirilmesi",
				ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1));

			Factory.Save();

			var list = lookups.NatureOfTransactionList;
			list.Load();
			CombineAssertions(() =>
			{
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "01"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "02"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "Eşyanın İade Edilmesi - Geri Gelen Eşya"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "İade Edilen Eşyanın Değiştirilmesi"));
				AssertSame("Cached", list, lookups.NatureOfTransactionList);
			});
		}

		public void TestPrimaryPreferenceList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var trId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, parent: trId);

			var cusPref1 = helper.CreatePreferenceForCountry("AKCT", "Türkiye-AB Avrupa Kömür Çelik Topluluğu Ürünleri STA’sı", Core.Constants.CountryCodes.Turkey);
			var cusPref2 = helper.CreatePreferenceForCountry("D8", "D-8 Üyesi Devletler Arasında Tercihli Ticaret Anlaşması", Core.Constants.CountryCodes.Turkey);
			var cusPref3 = helper.CreatePreferenceForCountry("EAZG", "En Az Gelişmiş Ülkeler", Core.Constants.CountryCodes.Turkey);
			var cusPref4 = helper.CreatePreferenceForCountry("500", "500Description", "CDS");
			Factory.Save();

			var date1 = ZDateTime.MinSmallDateTimeValue;
			var date2 = ZDateTime.MaxSmallDateTimeValue;
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "TEST1", date1, date2);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Germany, date1.Date, date2.Date);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "AKCT, D8, EAZG", ((CodeDescriptionPairList)lookups.PrimaryPreferenceList).CodesAsString);

				invoiceLine.JI_Tariff = "999999999";
				AssertEquals("No UniveralTariff and get preference count", "AKCT, D8, EAZG", ((CodeDescriptionPairList)lookups.PrimaryPreferenceList).CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			lookups = GetLookups();
		}
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected JobComInvoiceLineLookups lookups;

		protected abstract string MessageType { get; }

		protected abstract JobComInvoiceLineLookups GetLookups();
	}
}
