using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	abstract class CurrencyConvertion_of_ApportionedCharges_Tests_Parent : TestCaseWithFactory
	{
		protected CurrencyConvertion_of_ApportionedCharges_Tests_Parent(decimal invoiceAmount, decimal sumTotalADD, decimal sumTotalOFT, string currencyCodeADD, decimal exchangeRateADD, string currencyCodeOFT, decimal exchangeRateOFT)
		{
			this.invoiceAmount = invoiceAmount;
			this.sumTotalADD = sumTotalADD;
			this.sumTotalOFT = sumTotalOFT;
			this.currencyCodeADD = currencyCodeADD;
			this.exchangeRateADD = exchangeRateADD;
			this.currencyCodeOFT = currencyCodeOFT;
			this.exchangeRateOFT = exchangeRateOFT;
		}

		[TestDate(2020, 03, 27, 03, 04, 05)]
		public void TestCurrencyConvertion_of_ApportionedCharges()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				SetupExchangeRate(currencyCodeADD, exchangeRateADD);
				SetupExchangeRate(currencyCodeOFT, exchangeRateOFT);

				invoice = Factory.New<JobComInvoiceHeader>();
				invoice.JZ_InvoiceAmount = invoiceAmount;
				invoice.JZ_RX_NKInvoice_Currency = currencyCodeADD;
				invoice.JZ_InvoiceDate = processDate;
				invoice.JZ_ValuationDateOverride = processDate;

				invoice.GroupCharges.AddNew("ADD", sumTotalADD, currencyCodeADD);
				invoice.GroupCharges.AddNew("OFT", sumTotalOFT, currencyCodeOFT);

				CreateInvoiceLines();
				PerformSanityCheckOnApportionedCharges();

				foreach (JobComInvoiceLine invLine in invoice.InvoiceLines)
				{
					var docJobComInvoiceLine = DocJobComInvoiceLine.New(invLine, Factory);
					var amount = docJobComInvoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInForeignCurrency;

					var sLineNo = string.Format(CultureInfo.InvariantCulture, "{0}", invLine.JI_LineNo);
					var assertMsg = $"For invoice line {sLineNo}, the calculated Non-Dutiable charge amount must be 0.00 (zero value decimal).";
					AssertEquals(assertMsg, 0.00m, amount);
				}
			}
		}

		protected abstract void CreateInvoiceLines();

		void SetupExchangeRate(string currencyCode, decimal exchangeRate)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCurrencySchema.RX_Code, currencyCode);
			var currencyEUR = Factory.LoadTop1<RefCurrency>(query);
			currencyEUR.ExchangeRates.RemoveAllFromRelationship();

			int total = currencyEUR.ExchangeRates.Count;
			AssertEquals("Prerequisite: Clear all existing exchange rates.", 0, total);

			RefExchangeRate exRate = currencyEUR.ExchangeRates.AddNew();
			exRate.RE_StartDate = new ZDateTime(2020, 3, 26);
			exRate.RE_ExpiryDate = new ZDateTime(2020, 3, 29);
			exRate.RE_SellRate = exchangeRate;
			exRate.RE_ExRateType = "CUS";
		}

		void SetChargePropertiesFor_ADD_Charge(BaseInvoiceLineApportionedCharge charge)
		{
			charge.J7_AdjustedCharge = false;
			charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge.J7_DistributeBy = "VAL";
			charge.J7_ExchangeRate = exchangeRateADD;
			charge.J7_FullOrPartialApportionment = "PAA";
			charge.J7_IsApportionedCharge = true;
			charge.J7_IsCalculated = false;
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_Percentage = 0.00m;
			charge.J7_PrepaidCollect = "PPD";
		}

		void SetChargePropertiesFor_OFT_Charge(BaseInvoiceLineApportionedCharge charge)
		{
			charge.J7_AdjustedCharge = false;
			charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge.J7_DistributeBy = "VAL";
			charge.J7_ExchangeRate = exchangeRateOFT;
			charge.J7_FullOrPartialApportionment = "PAA";
			charge.J7_IsApportionedCharge = true;
			charge.J7_IsCalculated = false;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_Percentage = 0.00m;
			charge.J7_PrepaidCollect = "CCX";
		}

		protected void AddInvoiceLine(decimal linePrice, decimal chargeADD, decimal chargeOFT)
		{
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_LinePrice = linePrice;

			invLine.JI_CustomDate1 = processDate;
			invLine.JI_CustomDate2 = processDate;
			invLine.JI_CustomDate3 = processDate;
			invLine.JI_CustomDate4 = processDate;
			invLine.JI_CustomDate5 = processDate;

			SetChargePropertiesFor_ADD_Charge(invLine.ApportionedCharges.AddNew("ADD", chargeADD, currencyCodeADD));
			SetChargePropertiesFor_OFT_Charge(invLine.ApportionedCharges.AddNew("OFT", chargeOFT, currencyCodeOFT));
		}

		void PerformSanityCheckOnApportionedCharges()
		{
			var invLines = invoice.InvoiceLines.ToList().Cast<JobComInvoiceLine>().ToList();

			var sumPrice = 0.0m;
			invLines.ForEach(x => sumPrice += x.JI_LinePrice);
			AssertEquals("Sanity check on sum of line prices.", invoice.InvoiceAmount.Amount, sumPrice);
			AssertEquals("Sanity check on expected sum of line prices.", invoiceAmount, sumPrice);

			var chargeList = invLines.SelectMany(x => x.ApportionedCharges.Cast<BaseInvoiceLineApportionedCharge>().ToList()).ToList();

			var sum_ADD = SumAllMatchingCharges(chargeList, "ADD", currencyCodeADD);
			var sum_OFT = SumAllMatchingCharges(chargeList, "OFT", currencyCodeOFT);

			var groupCharges = invoice.GroupCharges.Cast<ApportionedCharge>().ToList();

			var invoice_ADD = groupCharges.FirstOrDefault(x => x.J7_ChargeType == "ADD" && x.J7_RX_NKCurrency == currencyCodeADD).J7_Amount;
			var invoice_OFT = groupCharges.FirstOrDefault(x => x.J7_ChargeType == "OFT" && x.J7_RX_NKCurrency == currencyCodeOFT).J7_Amount;

			AssertEquals("Sanity check on sum of ADD line charges.", invoice_ADD, sum_ADD);
			AssertEquals("Sanity check on sum of OFT line charges.", invoice_OFT, sum_OFT);

			AssertEquals("Sanity check on expected sum of ADD line charges.", sumTotalADD, sum_ADD);
			AssertEquals("Sanity check on expected sum of OFT line charges.", sumTotalOFT, sum_OFT);

			invLines.ForEach(x => CheckInvoiceLineRatios(invoice.JZ_InvoiceAmount, invoice_ADD, invoice_OFT, x));
		}

		decimal SumAllMatchingCharges(List<BaseInvoiceLineApportionedCharge> chargeList, string chargeCode, string currencyCode)
		{
			var sumTotal = 0.0m;
			chargeList.Where(x => x.J7_ChargeType == chargeCode && x.J7_RX_NKCurrency == currencyCode).ToList().Select(y => y.J7_Amount).ForEach(x => sumTotal += x);
			return sumTotal;
		}

		void CheckInvoiceLineRatios(decimal invoiceAmount, decimal invoice_ADD, decimal invoice_OFT, JobComInvoiceLine invLine)
		{
			var lineCharges = invLine.ApportionedCharges.ToList().Cast<BaseInvoiceLineApportionedCharge>().ToList();

			var charge_ADD = lineCharges.FirstOrDefault(x => x.J7_ChargeType == "ADD" && x.J7_RX_NKCurrency == currencyCodeADD).J7_Amount;
			var charge_OFT = lineCharges.FirstOrDefault(x => x.J7_ChargeType == "OFT" && x.J7_RX_NKCurrency == currencyCodeOFT).J7_Amount;

			var expectedRatio = decimal.Round(invLine.JI_LinePrice / invoiceAmount, 2);
			var ratio_ADD = decimal.Round(charge_ADD / invoice_ADD, 2);
			var ratio_OFT = decimal.Round(charge_OFT / invoice_OFT, 2);

			var sLineNo = string.Format(CultureInfo.InvariantCulture, "{0}", invLine.JI_LineNo);
			var assertMsg = $"For invoice line number {sLineNo}, Check Apportioned Ratio of line charge ";

			AssertEquals(assertMsg + "ADD", expectedRatio, ratio_ADD);
			AssertEquals(assertMsg + "OFT", expectedRatio, ratio_OFT);
		}

		#region Instance Variables:

		JobComInvoiceHeader invoice;

		readonly decimal invoiceAmount;
		readonly decimal sumTotalADD;
		readonly decimal sumTotalOFT;

		readonly string currencyCodeADD;
		readonly decimal exchangeRateADD;

		readonly string currencyCodeOFT;
		readonly decimal exchangeRateOFT;

		readonly ZDateTime processDate = new ZDateTime(2020, 3, 27);

		#endregion Instance Variables.
	}

	sealed class CurrencyConvertion_of_ApportionedCharges_in_JPY_Tests : CurrencyConvertion_of_ApportionedCharges_Tests_Parent
	{
		public CurrencyConvertion_of_ApportionedCharges_in_JPY_Tests() : base(19557.75m, 250.00m, 39660.0m, "EUR", 0.051537m, "JPY", 6.154134m)
		{
		}

		protected override void CreateInvoiceLines()
		{
			AddInvoiceLine(2080.00m, 26.59m, 4218.0m);
			AddInvoiceLine(2142.50m, 27.39m, 4345.0m);
			AddInvoiceLine(8141.25m, 104.06m, 16510.0m);
			AddInvoiceLine(444.00m, 5.68m, 900.0m);
			AddInvoiceLine(145.75m, 1.86m, 295.0m);
			AddInvoiceLine(825.75m, 10.56m, 1675.0m);
			AddInvoiceLine(485.00m, 6.20m, 983.0m);
			AddInvoiceLine(449.50m, 5.75m, 911.0m);
			AddInvoiceLine(4332.25m, 55.37m, 8785.0m);
			AddInvoiceLine(307.75m, 3.93m, 624.0m);
			AddInvoiceLine(204.00m, 2.61m, 414.0m);
		}
	}

	sealed class CurrencyConvertion_of_ApportionedCharges_in_USD_Tests : CurrencyConvertion_of_ApportionedCharges_Tests_Parent
	{
		public CurrencyConvertion_of_ApportionedCharges_in_USD_Tests() : base(19557.75m, 250.00m, 368.21m, "EUR", 0.051537m, "USD", 0.057137m)
		{
		}

		protected override void CreateInvoiceLines()
		{
			AddInvoiceLine(2080.00m, 26.59m, 39.16m);
			AddInvoiceLine(2142.50m, 27.39m, 40.34m);
			AddInvoiceLine(8141.25m, 104.06m, 153.28m);
			AddInvoiceLine(444.00m, 5.68m, 8.36m);
			AddInvoiceLine(145.75m, 1.86m, 2.74m);
			AddInvoiceLine(825.75m, 10.56m, 15.55m);
			AddInvoiceLine(485.00m, 6.20m, 9.13m);
			AddInvoiceLine(449.50m, 5.75m, 8.46m);
			AddInvoiceLine(4332.25m, 55.37m, 81.56m);
			AddInvoiceLine(307.75m, 3.93m, 5.79m);
			AddInvoiceLine(204.00m, 2.61m, 3.84m);
		}
	}
}
