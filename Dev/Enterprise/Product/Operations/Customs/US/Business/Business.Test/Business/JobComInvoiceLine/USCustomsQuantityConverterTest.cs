using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCustomsQuantityConverterTest : TestCaseWithFactory
	{
		public void TestCalculateCountrySpecificQuantity()
		{
			SetupInvoiceLineAndTariff();
			invoiceLine.JI_CustomsQuantity = 0m;
			invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Dozen;
			invoiceLine.JI_InvoiceQuantity = 18m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Pieces;
			AssertEquals(1.5m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 17m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Packs;
			AssertEquals(1.416667m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 17m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Pieces;
			AssertEquals(1.416667m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 18m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Packs;
			AssertEquals(1.5m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 18m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(1.5m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 28m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Pieces;
			AssertEquals(2.333333m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 17m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(1.416667m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 48m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Pieces;
			AssertEquals(4m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 6m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(0.5m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 43m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Packs;
			AssertEquals(3.583333m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 5.95m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(0.495833m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 29.99m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Packs;
			AssertEquals(2.499167m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 30m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(2.5m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 5.99m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Packs;
			AssertEquals(0.499167m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 42m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(3.5m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 795m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Packs;
			AssertEquals(66.25m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(0.083333m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 3m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(0.25m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(0.416667m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Number;
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCalculateCountrySpecificQuantityWithTaxCodeAndQuantityIsRequired()
		{
			SetupInvoiceLineAndTariff();
			invoiceLine.US_TaxCode = "018";

			CombineAssertions(() =>
			{
				tariff.UE_DutyComputationCode = "1";
				invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#01 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "2";
				invoiceLine.US_TaxRateS = "";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#02 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "3";
				invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#03 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "4";
				invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#04 Not Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "5";
				invoiceLine.US_TaxRateS = "";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#05 Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "6";
				invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#06 Not Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "7";
				invoiceLine.US_TaxRateS = "";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#07 Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "C";
				invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#08 Not Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "F";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#09 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "K";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#10 Not Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);
			});
		}

		public void TestCalculateCountrySpecificQuantityWithTariff22()
		{
			SetupInvoiceLineAndTariff();

			invoiceLine.JI_InvoiceQuantity = 2322m;
			AssertEquals("Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

			tariff.UE_Tariff = "2200111100";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Dozen;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Pieces;
			invoiceLine.JI_InvoiceQuantity = 2310m;
			AssertEquals("Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCalculateCountrySpecificQuantityWithVISANo()
		{
			SetupInvoiceLineAndTariff();
			invoiceLine.US_VisaNo = "1234";

			CombineAssertions(() =>
			{
				tariff.UE_DutyComputationCode = "7";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#01 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "0";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#02 Not Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "1";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#03 Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "2";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#04 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "3";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#05 Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "4";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#06 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "5";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#07 Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_DutyComputationCode = "6";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#08 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);
			});
		}

		public void TestCalculateCountrySpecificQuantityWithEmptyTextile()
		{
			SetupInvoiceLineAndTariff();
			invoiceLine.US_TextileCategoryNo = ZString.Empty;

			// When Duty Computation Code is 1/3/4/6.
			CombineAssertions(() =>
			{
				tariff.UE_Column1RateSpecific = 1.05m;
				tariff.UE_DutyComputationCode = "1";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#01 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 0.05m;
				tariff.UE_DutyComputationCode = "3";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#02 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 1.05m;
				tariff.UE_DutyComputationCode = "4";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#03 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 0.05m;
				tariff.UE_DutyComputationCode = "6";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#04 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 1.05m;
				tariff.UE_DutyComputationCode = "6";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#05 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 0.05m;
				tariff.UE_DutyComputationCode = "4";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#06 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 1.05m;
				tariff.UE_DutyComputationCode = "3";
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#07 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				tariff.UE_Column1RateSpecific = 0.05m;
				tariff.UE_DutyComputationCode = "1";
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#08 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);
			});

			// When Duty Computation Code is C.
			CombineAssertions(() =>
			{
				tariff.UE_DutyComputationCode = "C";

				invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
				tariff.UE_Column1RateSpecific = 1.05m;
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#01 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				invoiceLine.US_SelectedRateType = RateTypeList.Codes.Secondary;
				tariff.UE_Column1RateAdValorem = 0.05m;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#02 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);

				invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
				tariff.UE_Column1RateSpecific = 0.05m;
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#03 Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				invoiceLine.US_SelectedRateType = RateTypeList.Codes.Secondary;
				tariff.UE_Column1RateAdValorem = 1.05m;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#04 Not Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);
			});

			// When Duty Computation Code is F.
			CombineAssertions(() =>
			{
				tariff.UE_DutyComputationCode = "F";
				tariff.UE_Column1RateSpecific = 0.05m;
				tariff.UE_Column1RateOther = 0.5m;

				invoiceLine.JI_CustomsSecondQuantity = 2m;
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#01 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				invoiceLine.JI_CustomsSecondQuantity = 1m;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#02 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);
			});

			// When Duty Computation Code is K.
			CombineAssertions(() =>
			{
				tariff.UE_DutyComputationCode = "K";
				tariff.UE_Column1RateSpecific = 1.05m;
				tariff.UE_Column1RateOther = 0.95m;
				tariff.UE_Column1RateAdValorem = 1m;

				invoiceLine.JI_CustomsSecondQuantity = 100m;
				invoiceLine.JI_InvoiceQuantity = 2310m;
				AssertEquals("#01 Not Rounded.", 192.5m, invoiceLine.JI_CustomsQuantity);

				invoiceLine.JI_CustomsSecondQuantity = 99m;
				invoiceLine.JI_InvoiceQuantity = 2322m;
				AssertEquals("#02 Rounded.", 193.5m, invoiceLine.JI_CustomsQuantity);
			});
		}

		void SetupInvoiceLineAndTariff()
		{
			invoiceLine = Factory.New<JobComInvoiceLine>();
			tariff = Factory.New<USCTariff>();

			tariff.UE_Tariff = "1100110011";
			tariff.UE_DateFrom = new ZDateTime(2000, 01, 01);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 0m;
			invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Dozen;
			invoiceLine.JI_InvoiceUQ = ABIUnitOfMeasureList.Codes.Pieces;
			invoiceLine.US_TextileCategoryNo = "GT5";
		}

		JobComInvoiceLine invoiceLine;
		USCTariff tariff;
	}
}
