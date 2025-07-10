using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceLineChargeForPercentageTest : TestCaseWithFactory
	{
		public void TestCalculateAmountBasedOnPercentage()
		{
			var lineCharge = invoiceLine.Charges.AddNew();
			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			lineCharge.J7_Percentage = 10m;
			AssertEquals("Amount is calculated", 1000m, lineCharge.J7_Amount);
			AssertEquals("Currency is populated", invoice.Invoice_Currency.RX_Code, lineCharge.J7_RX_NKCurrency);
		}

		public void TestDefaultedPercentageInApportionedCharges()
		{
			var dIS = invoice.Charges.AddNew();
			dIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			dIS.J7_Percentage = 20m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice line percentage populated in apportioned charges", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("Invoice line percentage", 20m, invoiceLine.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Invoice line Discount amount is calculated", 2000m, invoiceLine.ApportionedCharges.GetCharge(dIS.ChargeKey).Amount);
			AssertEquals("Invoice line discount currency is populated", invoice.Invoice_Currency.RX_Code, invoiceLine.ApportionedCharges.GetCharge(dIS.ChargeKey).Currency.Code);
		}

		public void TestAddLineAfterPercentageAddedDefaultsThePercentageInNewLine()
		{
			var invoiceDIS = invoice.Charges.AddNew();
			invoiceDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			invoiceDIS.J7_Percentage = 20m;

			invoiceLine.JI_LinePrice = 7000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice line2 has a percentage populated", 1, invoiceLine2.ApportionedCharges.Count);
			AssertEquals("Invoice line2 percentage", 20m, invoiceLine2.ApportionedCharges[0].J7_Percentage);
		}

		public void TestChargeOverridenInLinesAsAmountWhenInvoiceHasPercentage()
		{
			invoiceLine.JI_LinePrice = 7000m;
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;

			BaseInvoiceCharge invoiceDIS = invoice.Charges.AddNew();
			invoiceDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			invoiceDIS.J7_Percentage = 20m;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line1 Percentage", 20m, invoiceLine.ApportionedCharges[0].J7_Percentage);
			AssertEquals("PreCondition:Line2 Percentage", 20m, invoiceLine2.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line1 Percentage Amount", 1400m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("Line2 Percentage amount", 600m, invoiceLine2.ApportionedCharges[0].J7_Amount);

			BaseInvoiceLineCharge line1DIS = invoiceLine.Charges.AddNew();
			line1DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			line1DIS.J7_Amount = 100m;
			line1DIS.J7_RX_NKCurrency = invoice.Invoice_Currency.RX_Code;
			testDec.ResumeApportionment();
			AssertEquals("Line1 Percentage still stays", 20m, invoiceLine.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line2 Percentage", 20m, invoiceLine2.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line1 Percentage Amount", 100m, invoiceLine.Charges[0].J7_Amount);
		}

		public void TestChargeOverridenInLinesAsPercentageWhenInvoiceHasAmount()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				invoiceLine.JI_LinePrice = 7000m;
				BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 3000m;

				BaseInvoiceCharge invoiceDIS = invoice.Charges.AddNew();
				invoiceDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				invoiceDIS.J7_Amount = 1000m;
				invoiceDIS.J7_RX_NKCurrency = invoice.Invoice_Currency.RX_Code;
				testDec.ResumeApportionment();
				AssertEquals("PreCondition:Line1 is apportioned", 700m, invoiceLine.ApportionedCharges[0].J7_Amount);
				AssertEquals("PreCondition:Line2 is apportioned", 300m, invoiceLine2.ApportionedCharges[0].J7_Amount);

				BaseInvoiceLineCharge line1DIS = invoiceLine.Charges.AddNew();
				line1DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				line1DIS.J7_Percentage = 5m;
				testDec.ResumeApportionment();
				AssertEquals("Line1 DIS amount", 350m, invoiceLine.Charges[0].J7_Amount);
				AssertEquals("Line1 Apportioned DIS amount", 700m, invoiceLine.ApportionedCharges[0].J7_Amount);
				AssertEquals("Line2 DIS amount", 300m, invoiceLine2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestChargeOverridenInLinesAsAmountWhenInvoiceHasAmount()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				invoiceLine.JI_LinePrice = 7000m;
				BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 3000m;

				BaseInvoiceCharge invoiceDIS = invoice.Charges.AddNew();
				invoiceDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				invoiceDIS.J7_Amount = 1000m;
				invoiceDIS.J7_RX_NKCurrency = invoice.Invoice_Currency.RX_Code;
				testDec.ResumeApportionment();
				AssertEquals("PreCondition:Line1 is apportioned", 700m, invoiceLine.ApportionedCharges[0].J7_Amount);
				AssertEquals("PreCondition:Line2 is apportioned", 300m, invoiceLine2.ApportionedCharges[0].J7_Amount);

				BaseInvoiceLineCharge line1DIS = invoiceLine.Charges.AddNew();
				line1DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				line1DIS.J7_Amount = 400m;
				line1DIS.J7_RX_NKCurrency = invoice.Invoice_Currency.RX_Code;
				testDec.ResumeApportionment();
				AssertEquals("Line1 DIS amount", 400m, invoiceLine.Charges[0].J7_Amount);
				AssertEquals("Line2 DIS amount", 600m, invoiceLine2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestChargesCalculatedOnPercentageAggregatedToParentThatHasPercentage()
		{
			invoiceLine.JI_LinePrice = 6000m;
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoiceLine.Declaration.LocalCurrencyCode);

			BaseInvoiceCharge invoiceDIS = invoice.Charges.AddNew();
			invoiceDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			invoiceDIS.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("InvoiceLine DIS calculated", 600m, invoiceLine.ApportionedCharges.GetCharge(invoiceDIS.ChargeKey).Amount);
			AssertEquals("InvoiceLine2 DIS calculated", 300m, invoiceLine2.ApportionedCharges.GetCharge(invoiceDIS.ChargeKey).Amount);
			AssertEquals("Invoice DIS calculated in invoice charge, not in apportioned charges", 0m, invoice.GroupCharges.GetCharge(invoiceDIS.ChargeKey).Amount);
			AssertEquals("Invoice DIS calculated in Invoice Charge", 900m, invoiceDIS.J7_Amount);
		}

		public void TestChargesCalculatedOnPercentageAggregatedToParentThatHasPercentage2()
		{
			invoiceLine.JI_LinePrice = 6000m;
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoiceLine.Declaration.LocalCurrencyCode);

			BaseGroupInvoiceCharge groupDIS = topGroup.Charges.AddNew();
			groupDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			groupDIS.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice line 1 DIS amount", 600m, invoiceLine.ApportionedCharges.GetCharge(groupDIS.ChargeKey).Amount);
			AssertEquals("Invoice line 2 DIS amount", 300m, invoiceLine2.ApportionedCharges.GetCharge(groupDIS.ChargeKey).Amount);
			AssertEquals("Invoice DIS amount", 900m, invoice.GroupCharges.GetCharge(groupDIS.ChargeKey).Amount);
		}

		public void TestAssignZeroAmountAndCurrencyIfPercentageOverridenInLineLevel()
		{
			BaseInvoiceCharge invCharge = invoice.Charges.AddNew();
			invCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			invCharge.J7_Percentage = 5m;
			invCharge.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("Amount is readonly", true, invCharge.J7_AmountInfo.ReadOnly);
			AssertEquals("Currency is readonly", true, invCharge.J7_RX_NKCurrencyInfo.ReadOnly);

			AssertEquals("Precondition: Line is defaulted with the percentage", 5m, invoiceLine.ApportionedCharges[0].J7_Percentage);
			AssertEquals("PreCondition: Amount is calculated against percentage", 500m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition: Currency is calculated", invoice.JobDeclaration.LocalCurrencyCode, invoiceLine.ApportionedCharges[0].J7_RX_NKCurrency);

			BaseInvoiceLineCharge lineCharge = invoiceLine.Charges.AddNew();
			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			lineCharge.J7_Percentage = 10m;
			lineCharge.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("Amount is calculated against the percentage", 1000m, lineCharge.J7_Amount);

			AssertEquals("Amount calculated against percentage is cleared as line overrode this charge", 500m, invCharge.J7_Amount);
			AssertEquals("Commission Percentage amount from line", 1000m, invoice.GroupCharges.GetCharge(invCharge.ChargeKey).Amount);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader topGroup;
		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			topGroup = testDec.JobComInvoiceGroupHeaders[0];
			invoice = topGroup.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000;
		}

		#endregion
	}
}
