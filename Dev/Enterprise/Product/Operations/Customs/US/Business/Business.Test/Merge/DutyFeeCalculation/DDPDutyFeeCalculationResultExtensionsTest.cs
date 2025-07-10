using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.DDPDisbursementCalculation.Testing
{
	sealed class DDPDutyFeeCalculationResultExtensionsTest : TestCaseWithFactory
	{
		public void TestSetOrGetDutyFeeCalculationResult()
		{
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "AAA", GetDDPResultData(10m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine2, "AAA", GetDDPResultData(15m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "BBB", GetDDPResultData(20m));

			AssertEquals(10m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine1, "AAA"));
			AssertEquals(15m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine2, "AAA"));

			AssertEquals(20m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine1, "BBB"));
			AssertEquals(0m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine2, "BBB"));

			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, USCustomsChargeTypeList.Codes.DisbursementCharge, GetDDPResultData(0m));

			Dictionary<string, DDPCalculationResultData> chargesAndDuty;
			dutyFeeCalculationResult.TryGetValue(invoiceLine1, out chargesAndDuty);

			AssertNotNull(chargesAndDuty);
			Assert("Existence of DDD is important for DDP calculation even if the amount is zero", chargesAndDuty.ContainsKey(USCustomsChargeTypeList.Codes.DisbursementCharge));
		}

		public void TestRemoveChargesExcept()
		{
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "AAA", GetDDPResultData(10m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine2, "AAA", GetDDPResultData(15m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "BBB", GetDDPResultData(20m));

			dutyFeeCalculationResult.RemoveChargesExcept(invoiceLine1, new string[] { "AAA" });

			AssertEquals(10m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine1, "AAA"));
			AssertEquals(15m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine2, "AAA"));

			AssertEquals(0m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine1, "BBB"));
			AssertEquals(0m, dutyFeeCalculationResult.GetChargeOrFeeAmount(invoiceLine2, "BBB"));
		}

		public void TestGetTotalAmountForAChargeType()
		{
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "AAA", GetDDPResultData(10m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine2, "AAA", GetDDPResultData(15m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "BBB", GetDDPResultData(20m));

			AssertEquals(25m, dutyFeeCalculationResult.GetTotalAmounts("AAA"));
			AssertEquals(20m, dutyFeeCalculationResult.GetTotalAmounts("BBB"));
		}

		public void TestGetTotalAmountForInvoiceLine()
		{
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "AAA", GetDDPResultData(10m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine2, "AAA", GetDDPResultData(15m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "BBB", GetDDPResultData(20m));

			AssertEquals(30m, dutyFeeCalculationResult.GetTotalAmounts(invoiceLine1));
			AssertEquals(15m, dutyFeeCalculationResult.GetTotalAmounts(invoiceLine2));
		}

		public void TestGetTotalAmountForInvoiceHeader()
		{
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "AAA", GetDDPResultData(10m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine2, "AAA", GetDDPResultData(15m));
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine1, "BBB", GetDDPResultData(20m));

			JobComInvoiceHeader diffInvoice = invoiceLine1.Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine3 = diffInvoice.JobComInvoiceLines.AddNew();
			dutyFeeCalculationResult.SetDutyFeeCalculationResult(invoiceLine3, "BBB", GetDDPResultData(1m));

			AssertEquals(45m, dutyFeeCalculationResult.GetTotalAmounts(invoiceLine1.InvoiceHeader));
			AssertEquals(1m, dutyFeeCalculationResult.GetTotalAmounts(diffInvoice));
		}

		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;

		Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> dutyFeeCalculationResult;

		protected override void SetUp()
		{
			base.SetUp();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine2 = declaration.InvoiceLines.AddNew();

			dutyFeeCalculationResult = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>();
		}

		DDPCalculationResultData GetDDPResultData(decimal amount)
		{
			return new DDPCalculationResultData(amount, new FeeCalculationInternalData(amount, 0m));
		}
	}

	class DDPCustomsValuesExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetCustomsValues()
		{
			CustomsValues value1 = new CustomsValues();
			value1.SupCustomsValue = 1m;
			value1.CustomsValue = 2m;
			customsValues.Add(invoiceLine1, value1);

			CustomsValues value2 = new CustomsValues();
			value2.SupCustomsValue = 3m;
			value2.CustomsValue = 4m;
			customsValues.Add(invoiceLine2, value2);

			AssertEquals(1m, customsValues.GetCustomsValue(invoiceLine1, true));
			AssertEquals(2m, customsValues.GetCustomsValue(invoiceLine1, false));
			AssertEquals(3m, customsValues.GetCustomsValue(invoiceLine2, true));
			AssertEquals(4m, customsValues.GetCustomsValue(invoiceLine2, false));
		}

		public void TestGetTotalCustomsValues()
		{
			CustomsValues value1 = new CustomsValues();
			value1.SupCustomsValue = 1m;
			value1.CustomsValue = 2m;
			customsValues.Add(invoiceLine1, value1);

			CustomsValues value2 = new CustomsValues();
			value2.SupCustomsValue = 3m;
			value2.CustomsValue = 4m;
			customsValues.Add(invoiceLine2, value2);

			AssertEquals(3m, customsValues.GetTotalCustomsValue(invoiceLine1));
			AssertEquals(7m, customsValues.GetTotalCustomsValue(invoiceLine2));
		}

		public void TestGetCustomsValue()
		{
			invoiceLine1.US_SupTariff = "9802004040";
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.US_98GoodsValue = 5m;

			CustomsValues result = DDPCustomsValuesExtensionMethods.GetCustomsValues(invoiceLine1);

			AssertEquals(100m, result.CustomsValue);
			AssertEquals(5m, result.SupCustomsValue);
		}

		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;

		Dictionary<JobComInvoiceLine, CustomsValues> customsValues;

		protected override void SetUp()
		{
			base.SetUp();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine2 = declaration.InvoiceLines.AddNew();

			customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>();
		}
	}
}
