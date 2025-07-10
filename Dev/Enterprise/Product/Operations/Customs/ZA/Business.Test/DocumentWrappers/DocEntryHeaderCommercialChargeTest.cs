using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class DocEntryHeaderCommercialChargeTest : TestCaseWithFactory
	{
		public void TestWrapping_of_InvoiceLineApportionedCharge()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var invoiceHeader = Factory.New<JobComInvoiceHeader>();
				invoiceHeader.JZ_InvoiceNumber = "INV01";
				invoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

				var listCharges = new List<BaseInvoiceLineApportionedCharge>()
				{
					CreateApportionedCharge_01(invoiceLine),
					CreateApportionedCharge_02(invoiceLine),
					CreateApportionedCharge_03(invoiceLine)
				};

				foreach (var charge in listCharges)
				{
					var wrapper = DocEntryHeaderCommercialCharge.New(charge, Factory);

					AssertEquals(charge.InvoiceLine.InvoiceNumber, wrapper.InvoiceNumber);
					AssertEquals(charge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight, wrapper.IsFreight);
					AssertEquals(charge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance, wrapper.IsInsurance);
					AssertEquals(charge.J7_Amount, wrapper.ChargeAmount);
					AssertEquals(charge.J7_RX_NKCurrency, wrapper.CurrencyCode);
					AssertEquals(charge.J7_ExchangeRate, wrapper.ExchangeRate);
					AssertEquals(charge.J7_IsDutiable, wrapper.IsDutiable);
					AssertEquals(!charge.J7_IsNotIncludedInInvoice, wrapper.IsIncludedInLines);
				}
			}
		}

		BaseInvoiceLineApportionedCharge CreateApportionedCharge_01(BaseJobComInvoiceLine invoiceLine)
		{
			var charge = invoiceLine.ApportionedCharges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 100;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_ExchangeRate = 0.16;
			charge.J7_IsDutiable = true;
			charge.J7_IsNotIncludedInInvoice = false;
			return charge;
		}

		BaseInvoiceLineApportionedCharge CreateApportionedCharge_02(BaseJobComInvoiceLine invoiceLine)
		{
			var charge = invoiceLine.ApportionedCharges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_Amount = 200;
			charge.J7_RX_NKCurrency = "GBP";
			charge.J7_ExchangeRate = 0.14;
			charge.J7_IsDutiable = false;
			charge.J7_IsNotIncludedInInvoice = true;
			return charge;
		}

		BaseInvoiceLineApportionedCharge CreateApportionedCharge_03(BaseJobComInvoiceLine invoiceLine)
		{
			var charge = invoiceLine.ApportionedCharges.AddNew();
			charge.J7_ChargeType = "OTH";
			charge.J7_Amount = 300;
			charge.J7_RX_NKCurrency = "JPY";
			charge.J7_ExchangeRate = 2.76;
			charge.J7_IsDutiable = false;
			charge.J7_IsNotIncludedInInvoice = false;
			return charge;
		}

		public void TestAddAmount()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var invoiceHeader = Factory.New<JobComInvoiceHeader>();
				invoiceHeader.JZ_InvoiceNumber = "INV01";
				invoiceHeader.JZ_InvoiceDate = ZDateTime.Now;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

				var listCharges = new List<BaseInvoiceLineApportionedCharge>()
				{
					CreateApportionedCharge_01(invoiceLine),
					CreateApportionedCharge_02(invoiceLine),
					CreateApportionedCharge_03(invoiceLine)
				};

				foreach (var charge in listCharges)
				{
					var originalApportionedChargeAmount = charge.J7_Amount;
					var wrapper = DocEntryHeaderCommercialCharge.New(charge, Factory);
					wrapper.AddAdditionalAmountInChargeCurrency(100);

					AssertEquals(originalApportionedChargeAmount, charge.J7_Amount);
					AssertEquals(originalApportionedChargeAmount + 100, wrapper.ChargeAmount);
				}
			}
		}
	}
}
