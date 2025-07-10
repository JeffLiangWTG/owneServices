using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MandatoryChargesHandlerTest : TestCaseWithFactory
	{
		public void TestFillRecommendedCharges()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			AssertEquals("One invoice with OFT recommended charge in Group Charges", true, groupHeader.JobComInvoiceHeaders.HasInvoicesWithRecommendedChargeInGroupCharges(CustomsChargeCodeProvider.OverseasFreight));
			AssertEquals("One invoice with ONS recommended charge in Group Charges", true, groupHeader.JobComInvoiceHeaders.HasInvoicesWithRecommendedChargeInGroupCharges(CustomsChargeCodeProvider.OverseasInsurance));

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("One invoice with OFT recommended charge in Charges", false, groupHeader.JobComInvoiceHeaders.HasInvoicesWithRecommendedChargeInGroupCharges(CustomsChargeCodeProvider.OverseasFreight));
			AssertEquals("One invoice with ONS recommended charge in Charges", false, groupHeader.JobComInvoiceHeaders.HasInvoicesWithRecommendedChargeInGroupCharges(CustomsChargeCodeProvider.OverseasInsurance));
		}

		public void TestAutoCreateChargesBasedOnIncoterms()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("OFT in GroupCharges should be removed", 0, invoice.GroupHeader.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).Length);
			AssertEquals("ONS in GroupCharges should be removed", 0, invoice.GroupHeader.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).Length);
			AssertEquals("OFT is there", 1, invoice.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).Length);
			AssertEquals("ONS is there", 1, invoice.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).Length);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("OFT is there", 1, invoice.GroupHeader.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).Length);
			AssertEquals("ONS is there", 1, invoice.GroupHeader.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).Length);
		}

		public void TestDeleteZeroAmountChargesForPercentage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseInvoiceCharge chargeWithPercentage = invoice.Charges.AddNew();
			chargeWithPercentage.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			chargeWithPercentage.J7_Percentage = 10m;

			BaseInvoiceCharge chargeWithoutAmountCurrency = invoice.Charges.AddNew();
			chargeWithoutAmountCurrency.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;

			invoice.JZ_IncoTerm = "EXW";
			AssertEquals("ChargeWithPercentage should stay", false, chargeWithPercentage.IsDeleted);
			AssertEquals("ChargeWithPercentage still part of Invoice Charges", true, invoice.Charges.Contains(chargeWithPercentage));

			AssertEquals("ChargeWithoutAmountCurrency is deleted by system", true, chargeWithoutAmountCurrency.IsDeleted);
			AssertEquals("invoice charge should not contain the charge", false, invoice.Charges.Contains(chargeWithoutAmountCurrency));
		}
	}
}
