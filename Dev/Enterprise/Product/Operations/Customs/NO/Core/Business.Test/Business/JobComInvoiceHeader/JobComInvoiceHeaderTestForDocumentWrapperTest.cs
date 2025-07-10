using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class JobComInvoiceHeaderTestForDocumentWrapperTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		#region Overseas carges (OFT/ONS) are dutiable in NO

		public override void TestNonDutiableChargesNotIncludedInLinesWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(1000m, invoice.NonDutiableChargesNotIncludedInLines.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(1000m, invoice.NonDutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(500m, invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
		}

		public override void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsDutiable = false;

			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(500m, invoice.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency.Amount);
		}

		public override void TestDutiableChargesNotIncludedInLinesWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;

			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(1500m, invoice.DutiableChargesNotIncludedInLines.Amount);
		}

		public override void TestDutiableChargesNotIncludedInLinesInLocalCurrencyWorksCorrectly()
		{
			invoice.Charges.RemoveAll();
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;

			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, invoice.JobDeclaration.LocalCurrencyCode);
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300m, invoice.JobDeclaration.LocalCurrencyCode);

			AssertEquals(1500m, invoice.DutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
		}

		public override void TestCheckingValueOfJZ_Calc_ConversionFactor()
		{
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddYears(10), 0.5m, uSDCurrency, "CUS");

			invoice.JZ_InvoiceAmount = 10_000m;
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoice.JZ_IncoTerm = "CIF";

			invoice.Charges.RemoveAll();
			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500m);
			oFT.J7_IsIncludedInITOT = true;
			invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

			//Line Total : 10000 - 50 as ONS is not included in lines
			//FOB amount in USD: 10000 (OFT/ONS is dutiable)
			ZDecimal expectedValue = invoice.ConvertToLocalAmountRounded(10_000, uSDCurrency).Amount / (10_000 - 50.0m);
			AssertEquals(ZArchitecture.Core.Utilities.Round(expectedValue, 4), ZArchitecture.Core.Utilities.Round(invoice.JZ_Calc_ConversionFactor, 4));
		}

		#endregion
	}
}
