using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class EntryLinesProviderTest : TestCaseWithFactory
	{
		public void TestCusEntryImportLineItemMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var lines = declaration.EntryLines.ToArray();

				CombineAssertions("Lines Case | Import 1.Case", () =>
				{
					AssertEquals("Type", "KN", lines[0].Type);
					AssertEquals("Pieces", 2m, lines[0].Pieces);
					AssertEquals("QuantityUnit", "C62", lines[0].QuantityUnit);
					AssertEquals("ReturnToCountryOfOrigin", "EVET", lines[0].ReturnToCountryOfOrigin);
					AssertEquals("SecondaryProcess", "EVET", lines[0].SecondaryProcess);
					AssertEquals("LineNo", "1", lines[0].LineNo);
					AssertEquals("Quantity", 200m, lines[0].Quantity);
					AssertEquals("1st VatRatio", "KDV20", lines[0].VatRatio);
					AssertEquals("2rd VatRatio", "KDV10", lines[1].VatRatio);
					AssertEquals("UsedGoods", "K1", lines[0].UsedGoods);
					AssertEquals("Description44", "ek bilgi 1 ek bilgi 2 ek bilgi 3", lines[0].Description44);
					AssertEquals("ProducerRegNo", "8899665544", lines[0].ProducerRegNo);
					AssertEquals("DomesticOther", 2m, lines[0].DomesticOther);
					AssertEquals("DomesticBank", 4m, lines[0].DomesticBank);
					AssertEquals("DomesticStoring", 6m, lines[0].DomesticStoring);
					AssertEquals("DomesticDischarge", 8m, lines[0].DomesticDischarge);
					AssertEquals("DomesticPort", 10m, lines[0].DomesticPort);
					AssertEquals("DomesticCulture", 12m, lines[0].DomesticCulture);
					AssertEquals("DomesticKkdf", 14m, lines[0].DomesticKkdf);
					AssertEquals("TotalDomesticExpensesIncludingVAT", 16m, lines[0].TotalDomesticExpensesIncludingVAT);
					AssertEquals("DomesticEnvironment", 18m, lines[0].DomesticEnvironment);
					AssertEquals("DomesticOtherDescription", "Domestic Explanation", lines[0].DomesticOtherDescription);
					AssertEquals("ExemptionDescription", ZString.Empty, lines[0].ExemptionDescription);
					AssertEquals("ReferenceDate", ZString.Empty, lines[0].ReferenceDate);
					AssertEquals("OverseasCommission", 20m, lines[0].OverseasCommission);
					AssertEquals("OverseasDemurrage", 40m, lines[0].OverseasDemurrage);
					AssertEquals("OverseasRoyalty", 60m, lines[0].OverseasRoyalty);
					AssertEquals("OverseasInterest", 80m, lines[0].OverseasInterest);
					AssertEquals("OverseasOther", 100m, lines[0].OverseasOther);
					AssertEquals("OverseasCommissionCurrency", "USD", lines[0].OverseasCommissionCurrency);
					AssertEquals("OverseasDemurrageCurrency", "USD", lines[0].OverseasDemurrageCurrency);
					AssertEquals("OverseasRoyaltyCurrency", "USD", lines[0].OverseasRoyaltyCurrency);
					AssertEquals("OverseasInterestCurrency", "USD", lines[0].OverseasInterestCurrency);
					AssertEquals("OverseasOtherCurrency", "USD", lines[0].OverseasOtherCurrency);
					AssertEquals("OverseasOtherDescription", "Overseas Explanation", lines[0].OverseasOtherDescription);
					AssertEquals("QualificationOfLine", "11", lines[0].QualificationOfLine);
					AssertEquals("EntranceAndExitPurpose", "01", lines[0].EntranceAndExitPurpose);
					AssertEquals("EntranceAndExitPurposeDescription", "descOfPurpose", lines[0].EntranceAndExitPurposeDescription);
					AssertEquals("STMCityCode", "34", lines[0].STMCityCode);
					AssertEquals("GoodsComeBackReason", "21", lines[0].GoodsComeBackReason);
					AssertEquals("GoodsComeBackReasonDescription", "descOfReturningGoodsReason", lines[0].GoodsComeBackReasonDescription);
					AssertEquals("OrderType", "3", lines[0].OrderType);
				});

				var entryLine = declaration.EntryLines.ToArray()[0];
				CombineAssertions("Lines Case | Import 2.Case", () =>
				{
					AssertEquals("Vehicle count", 4, entryLine.CusVehicle.Count);
					AssertEquals(nameof(entryLine.Tariff), "660320000000", entryLine.Tariff);
					AssertEquals(nameof(entryLine.ManufacturerCompanyInfo), "EVET", entryLine.ManufacturerCompanyInfo);
					AssertEquals(nameof(entryLine.LineOrderNo), 1, entryLine.LineOrderNo);
					AssertEquals(nameof(entryLine.CountryOfOrigin), "003", entryLine.CountryOfOrigin);
					AssertEquals(nameof(entryLine.GrossWeight), 200.00m, entryLine.GrossWeight);
					AssertEquals(nameof(entryLine.NetWeight), 180.00m, entryLine.NetWeight);
					AssertEquals(nameof(entryLine.ComplementaryDimensionsUnit), "C62", entryLine.ComplementaryDimensionsUnit);
					AssertEquals(nameof(entryLine.StatisticalQuantity), 200.00m, entryLine.StatisticalQuantity);
					AssertEquals(nameof(entryLine.InternationalAgreement), "AT", entryLine.InternationalAgreement);
					AssertEquals(nameof(entryLine.PerceptionUnit1), "AYR", entryLine.PerceptionUnit1);
					AssertEquals(nameof(entryLine.PerceptionQuantity1), 180.0000m, entryLine.PerceptionQuantity1);
					AssertEquals(nameof(entryLine.PerceptionUnit2), "KFO", entryLine.PerceptionUnit2);
					AssertEquals(nameof(entryLine.PerceptionQuantity2), 180.0000m, entryLine.PerceptionQuantity2);
					AssertEquals(nameof(entryLine.Exemptions1), "AHSKA", entryLine.Exemptions1);
					AssertEquals(nameof(entryLine.Exemptions2), "BSİZ", entryLine.Exemptions2);
					AssertEquals(nameof(entryLine.Exemptions3), "AMBLI", entryLine.Exemptions3);
					AssertEquals(nameof(entryLine.Exemptions4), "BTC", entryLine.Exemptions4);
					AssertEquals(nameof(entryLine.Exemptions5), "BSGDCULKE", entryLine.Exemptions5);
					AssertEquals(nameof(entryLine.PerceptionUnit3), "GSM", entryLine.PerceptionUnit3);
					AssertEquals(nameof(entryLine.PerceptionQuantity3), 145.0000m, entryLine.PerceptionQuantity3);
					AssertEquals(nameof(entryLine.TypeOfDelivery), ZString.Empty, entryLine.TypeOfDelivery);
					AssertEquals(nameof(entryLine.AdditionalCode), "7012", entryLine.AdditionalCode);
					AssertEquals(nameof(entryLine.Feature), "01", entryLine.Feature);
					AssertEquals(nameof(entryLine.InvoiceAmount), 3200m, entryLine.InvoiceAmount);
					AssertEquals(nameof(entryLine.InvoiceAmountCurrency), "EUR", entryLine.InvoiceAmountCurrency);
					AssertEquals(nameof(entryLine.BorderPassFee), 0.00m, entryLine.BorderPassFee);
					AssertEquals(nameof(entryLine.FreightAmount), 200.00m, entryLine.FreightAmount);
					AssertEquals(nameof(entryLine.FreightAmountCurrency), "USD", entryLine.FreightAmountCurrency);
					AssertEquals(nameof(entryLine.StatisticalValue), ZDecimal.Zero, entryLine.StatisticalValue);
					AssertEquals(nameof(entryLine.InsuranceValue), 60.00m, entryLine.InsuranceValue);
					AssertEquals(nameof(entryLine.InsuranceValueCurrency), "USD", entryLine.InsuranceValueCurrency);
					AssertEquals(nameof(entryLine.TariffDescription), string.Empty, entryLine.TariffDescription);
					AssertEquals(nameof(entryLine.CommercialDescription), "Şemsiye", entryLine.CommercialDescription);
					AssertEquals(nameof(entryLine.Brand), "ADDR", entryLine.Brand);
					AssertEquals(nameof(entryLine.Number), "1111 2222", entryLine.Number);
				});
			}
		}

		public void TestCusEntryExportLineItemMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetExportProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entryLine = declaration.EntryLines.ToArray()[0];
				CombineAssertions("Lines Case | Export Empty Vehicle", () =>
				{
					AssertEquals("Vehicle count", 1, entryLine.CusVehicle.Count);
				});

				headerJobDeclaration = helper.GetExportProviderHeader();
				helper.AddInvoiceLineVehicle(headerJobDeclaration.InvoiceLines[0], headerJobDeclaration.InvoiceLines[1]);

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				AssertGreaterThanOrEqualTo(declaration.EntryLines.Count, 1);

				entryLine = declaration.EntryLines.ToArray()[0];
				CombineAssertions("Lines Case | Export not Empty Vehicle", () =>
				{
					AssertEquals("Vehicle count", 4, entryLine.CusVehicle.Count);
					AssertEquals(nameof(entryLine.VatRatio), ZString.Empty, entryLine.VatRatio);
					AssertEquals(nameof(entryLine.Tariff), "660320000000", entryLine.Tariff);
					AssertEquals(nameof(entryLine.ManufacturerCompanyInfo), "EVET", entryLine.ManufacturerCompanyInfo);
					AssertEquals(nameof(entryLine.LineOrderNo), 1, entryLine.LineOrderNo);
					AssertEquals(nameof(entryLine.CountryOfOrigin), "003", entryLine.CountryOfOrigin);
					AssertEquals(nameof(entryLine.GrossWeight), 200.00m, entryLine.GrossWeight);
					AssertEquals(nameof(entryLine.NetWeight), 180.00m, entryLine.NetWeight);
					AssertEquals(nameof(entryLine.ComplementaryDimensionsUnit), "C62", entryLine.ComplementaryDimensionsUnit);
					AssertEquals(nameof(entryLine.StatisticalQuantity), 200.00m, entryLine.StatisticalQuantity);
					AssertEquals(nameof(entryLine.InternationalAgreement), string.Empty, entryLine.InternationalAgreement);
					AssertEquals(nameof(entryLine.PerceptionUnit1), string.Empty, entryLine.PerceptionUnit1);
					AssertEquals(nameof(entryLine.PerceptionQuantity1), 0.0000m, entryLine.PerceptionQuantity1);
					AssertEquals(nameof(entryLine.PerceptionUnit2), string.Empty, entryLine.PerceptionUnit2);
					AssertEquals(nameof(entryLine.PerceptionQuantity2), 0.0000m, entryLine.PerceptionQuantity2);
					AssertEquals(nameof(entryLine.Exemptions1), "AHSKA", entryLine.Exemptions1);
					AssertEquals(nameof(entryLine.Exemptions2), "BSİZ", entryLine.Exemptions2);
					AssertEquals(nameof(entryLine.Exemptions3), "AMBLI", entryLine.Exemptions3);
					AssertEquals(nameof(entryLine.Exemptions4), "BTC", entryLine.Exemptions4);
					AssertEquals(nameof(entryLine.Exemptions5), "BSGDCULKE", entryLine.Exemptions5);
					AssertEquals(nameof(entryLine.PerceptionUnit3), string.Empty, entryLine.PerceptionUnit3);
					AssertEquals(nameof(entryLine.PerceptionQuantity3), 0.0000m, entryLine.PerceptionQuantity3);
					AssertEquals(nameof(entryLine.TypeOfDelivery), "FOB", entryLine.TypeOfDelivery);
					AssertEquals(nameof(entryLine.AdditionalCode), string.Empty, entryLine.AdditionalCode);
					AssertEquals(nameof(entryLine.Feature), "01", entryLine.Feature);
					AssertEquals(nameof(entryLine.InvoiceAmount), 3200m, entryLine.InvoiceAmount);
					AssertEquals(nameof(entryLine.InvoiceAmountCurrency), "EUR", entryLine.InvoiceAmountCurrency);
					AssertEquals(nameof(entryLine.BorderPassFee), 0.00m, entryLine.BorderPassFee);
					AssertEquals(nameof(entryLine.FreightAmount), 200.00m, entryLine.FreightAmount);
					AssertEquals(nameof(entryLine.FreightAmountCurrency), "USD", entryLine.FreightAmountCurrency);
					AssertEquals(nameof(entryLine.StatisticalValue), 3814.97m, entryLine.StatisticalValue);
					AssertEquals(nameof(entryLine.InsuranceValue), 60.00m, entryLine.InsuranceValue);
					AssertEquals(nameof(entryLine.InsuranceValueCurrency), "USD", entryLine.InsuranceValueCurrency);
					AssertEquals(nameof(entryLine.TariffDescription), string.Empty, entryLine.TariffDescription);
					AssertEquals(nameof(entryLine.CommercialDescription), "Şemsiye", entryLine.CommercialDescription);
					AssertEquals(nameof(entryLine.Brand), "ADDR", entryLine.Brand);
					AssertEquals(nameof(entryLine.Number), "1111 2222", entryLine.Number);
				});
			}
		}

		public void TestCusEntryLineItemSubMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entryLine = declaration.EntryLines.ToArray()[0];

				CombineAssertions(() =>
				{
					AssertEquals("ComplementaryInfo", 0, entryLine.ComplementaryInfo.Count);
					AssertEquals("DeclarationOpeningAndClosingInfo", 1, entryLine.DeclarationOpeningAndClosingInfo.Count);
					AssertEquals("CusVehicle", 4, entryLine.CusVehicle.Count);
					AssertEquals("ContainerInfo", 2, entryLine.ContainerInfo.Count);
					AssertEquals("TaxImmunitys", 6, entryLine.TaxImmunitys.Count);
					AssertEquals("AviationFuelTypes", 1, entryLine.AviationFuelTypes.Count);
					AssertEquals("PaymentTypes", 1, entryLine.PaymentTypes.Count);
				});
			}
		}
	}
}
