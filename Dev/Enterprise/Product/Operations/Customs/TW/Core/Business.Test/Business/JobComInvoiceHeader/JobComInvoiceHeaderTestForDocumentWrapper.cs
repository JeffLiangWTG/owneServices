using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceHeaderTestForDocumentWrapper : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override ZDecimal ExpectedIncludedTotalInInvoiceCurr => 72m;

		public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			var overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasFreightCharge.J7_IsIncludedInITOT = false;
			var overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasInsuranceCharge.J7_IsIncludedInITOT = false;

			AssertEquals(100m, invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			var overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasFreightCharge.J7_IsIncludedInITOT = false;
			var overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasInsuranceCharge.J7_IsIncludedInITOT = false;

			AssertEquals(100m, invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			var overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasFreightCharge.J7_IsIncludedInITOT = false;
			var overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasInsuranceCharge.J7_IsIncludedInITOT = false;

			AssertEquals(600m, invoice.NonDutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			var overseasFreightCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasFreightCharge.J7_IsIncludedInITOT = false;
			var overseasInsuranceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);
			overseasInsuranceCharge.J7_IsIncludedInITOT = false;

			AssertEquals(600m, invoice.NonDutiableChargesNotIncludedInLines.Amount);
		}

		#region Implementation
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion
	}
}
