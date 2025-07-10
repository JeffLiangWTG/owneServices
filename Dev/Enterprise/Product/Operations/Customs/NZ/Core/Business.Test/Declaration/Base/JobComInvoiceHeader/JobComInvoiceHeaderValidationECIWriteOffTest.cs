using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.Types;
	using NUnit.Framework;

	public class JobComInvoiceHeaderValidationECIWriteOffTest : Customs.Business.Testing.InvoiceHeaderValidationTest
	{
		[TestDate(2050, 1, 1)]
		public void TestInvalidExchangeRateMessageErrorDoesntShowUpOnAnECIWriteOff()
		{
			declaration.JE_EDITransmitDate = new ZDateTime(2050, 1, 1);
			invoiceHeader.JZ_InvoiceAmount = 100m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			declaration.RunPreSaveValidation();
			AssertNoMessageErrors(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);
		}

		public void TestNoWarningWhenCIFEqualsFOB()
		{
			declaration.RunPreSaveValidation();
			AssertNoWarnings(invoiceHeader.JZ_Calc_CIFAmountInfo);
		}

		public void TestCheckJZ_Calc_Balance()
		{
			invoiceHeader.JZ_InvoiceAmount = 600m;
			Assert("Should not check for ECI Write-offs", !invoiceHeader.JZ_Calc_BalanceInfo.HasMessageErrors());
		}

		public void TestValidateJZ_CU_RelatedHouseBill()
		{
			var houseBill = invoiceHeader.JobDeclaration.Bills.AddNew();
			invoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();

			var entry1 = invoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			var entry2 = invoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			invoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			Assert(!invoiceHeader.JZ_CU_RelatedHouseBillInfo.HasMessageErrors());
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}
	}
}
