using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseGroupInvoiceChargeValidationTestCase : TestCaseWithFactory
	{
		public void TestValidateExchangeRateForCurrency()
		{
			RefCurrency foreignCurrency = Factory.New<RefCurrency>();
			foreignCurrency.RX_Code = "XXX";

			RefExchangeRate rateOn1Jan = foreignCurrency.ExchangeRates.AddNew();
			rateOn1Jan.RE_ExRateType = "CUS";
			rateOn1Jan.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateOn1Jan.RE_ExpiryDate = new ZDateTime(2005, 1, 1);
			rateOn1Jan.RE_SellRate = 0.7m;

			RefExchangeRate rateOn2Jan = foreignCurrency.ExchangeRates.AddNew();
			rateOn2Jan.RE_ExRateType = "CUS";
			rateOn2Jan.RE_StartDate = new ZDateTime(2005, 1, 2);
			rateOn2Jan.RE_ExpiryDate = new ZDateTime(2005, 1, 2);
			rateOn2Jan.RE_SellRate = 0.8m;

			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.DateOfValuation).Returns(new ZDateTime(2004, 12, 21));
			var declaration = mockDeclaration.Object;

			var groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = "XXX";
			AssertHasMessageErrorContaining(groupCharge.J7_RX_NKCurrencyInfo, "There is no valid exchange rate for this currency for ");

			mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.DateOfValuation).Returns(new ZDateTime(2005, 1, 3));
			declaration = mockDeclaration.Object;
			groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = "XXX";
			AssertNoMessageErrorContaining(groupCharge.J7_RX_NKCurrencyInfo, "There is no valid exchange rate for this currency for ");
			AssertHasWarningContaining(groupCharge.J7_RX_NKCurrencyInfo, "There is no exchange rate in the database for the valuation date");

			mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.DateOfValuation).Returns(new ZDateTime(2005, 1, 2));
			declaration = mockDeclaration.Object;
			groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = "XXX";
			AssertNoWarningContaining(groupCharge.J7_RX_NKCurrencyInfo, "There is no exchange rate in the database for the valuation date");
		}

		public void TestValidateDistributeBy()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseGroupInvoiceCharge oFT1 = topGroup.Charges.AddNew();
			oFT1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT1.J7_Amount = 100m;
			oFT1.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			BaseJobComInvoiceHeader invoice = topGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			oFT1.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;
			AssertHasMessageError(oFT1.J7_DistributeByInfo, "There are invoice lines that don't have a volume. The apportionment of this charge won't be correct for the invoice lines.");
		}

		public void TestCheckIfThereAreInvoicesToBeApportioned()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			topGroup.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			AssertEquals("There is one invoice", 1, testDec.Invoices.Count);

			BaseGroupInvoiceCharge subGroupOFT = subGroup.Charges.AddNew();
			subGroupOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			subGroupOFT.J7_Amount = 100m;
			subGroupOFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			AssertHasMessageErrorContaining(subGroupOFT.J7_ChargeTypeInfo, "charges because the current group contains no invoices");

			BaseJobComInvoiceHeader invoice = subGroup.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			subGroupOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertNoMessageErrorContaining(subGroupOFT.J7_ChargeTypeInfo, "charges because the current group contains no invoices");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);
			subGroupOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertHasMessageErrorContaining(subGroupOFT.J7_ChargeTypeInfo, "charges because all invoices in the group have the charge assigned");

			subGroupOFT.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			subGroupOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertNoMessageErrorContaining(subGroupOFT.J7_ChargeTypeInfo, "charges because all invoices in the group have the charge assigned");
		}

		public void TestCheckGroupChargeBalance()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
				topGroup.JobComInvoiceHeaders.AddNew();

				BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
				AssertEquals("There is one invoice", 1, testDec.Invoices.Count);

				BaseGroupInvoiceCharge subGroupOFT = subGroup.Charges.AddNew();
				subGroupOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				subGroupOFT.J7_Amount = 100m;
				subGroupOFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

				subGroupOFT.J7_Amount = 100m;
				testDec.ResumeApportionment();
				subGroupOFT.Validation.ValidateJ7_Amount();
				AssertHasWarningContaining(subGroupOFT.J7_AmountInfo, "does not add up to this group charge");

				BaseJobComInvoiceHeader invoice = subGroup.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();

				subGroupOFT.Validation.ValidateJ7_Amount();
				AssertNoWarningContaining(subGroupOFT.J7_AmountInfo, "does not add up to this group charge");

				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, testDec.LocalCurrencyCode);
				testDec.ResumeApportionment();
				subGroupOFT.Validation.ValidateJ7_Amount();
				AssertHasWarningContaining(subGroupOFT.J7_AmountInfo, "does not add up to this group charge");

				subGroupOFT.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
				testDec.ResumeApportionment();
				subGroupOFT.Validation.ValidateJ7_Amount();
				AssertNoWarningContaining(subGroupOFT.J7_AmountInfo, "does not add up to this group charge");
			}
		}
	}
}
