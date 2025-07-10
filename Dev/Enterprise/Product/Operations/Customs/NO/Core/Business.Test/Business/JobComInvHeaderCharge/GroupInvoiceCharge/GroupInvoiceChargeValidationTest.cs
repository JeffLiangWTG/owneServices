using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class GroupInvoiceChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateChargeTypeVGEHasAmount()
		{
			var errorMessage = "Value of Goods Exported should be typed in. This will not be part of calculations (of Duties and VAT). The VGE amount is only used as a part of statistical value.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var charge = invoiceGroupHeader.Charges.AddNew();
			charge.J7_ChargeType = NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported;
			charge.J7_Amount = 0m;

			CombineAssertions(() =>
			{
				AssertHasMessageError("Charge VGE can not have amount 0", charge.J7_AmountInfo, errorMessage);

				charge.J7_Amount = 100m;
				AssertNoErrorContaining("With non-zero amount, VGE should not have errors", charge.J7_AmountInfo, errorMessage);
			});
		}

		public void TestChargesWithPercent()
		{
			var errorMessageZeroToHundred = "Percentage should be a value between 0 and 100.";
			var errorMessageYouCantEnterPercentage = "You can't enter a percentage for this charge type.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];

			CombineAssertions(() =>
			{
				AssertPercentageForCharges(NOInvoiceChargeTypesImport.Codes.OtherCharges, canHavePercent: true);
				AssertPercentageForCharges(NOInvoiceChargeTypesImport.Codes.DeductionCharge, canHavePercent: true);
				AssertPercentageForCharges(NOInvoiceChargeTypesImport.Codes.OverseasInsurance, canHavePercent: true);
				AssertPercentageForCharges(NOInvoiceChargeTypesImport.Codes.OverseasFreight, canHavePercent: false);
			});

			void AssertPercentageForCharges(string chargeType, bool canHavePercent)
			{
				var charge = invoiceGroupHeader.Charges.AddNew();
				charge.J7_ChargeType = chargeType;
				charge.J7_Percentage = -10;
				AssertHasError($"{chargeType} negative percentage, error", charge.J7_PercentageInfo, errorMessageZeroToHundred);
				charge.J7_Percentage = 10;
				AssertNoError($"{chargeType} positive percentage, no errors", charge.J7_PercentageInfo, errorMessageZeroToHundred);
				AssertEquals($"{chargeType} can have percentage", !canHavePercent, charge.J7_PercentageInfo.HasError(errorMessageYouCantEnterPercentage));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
