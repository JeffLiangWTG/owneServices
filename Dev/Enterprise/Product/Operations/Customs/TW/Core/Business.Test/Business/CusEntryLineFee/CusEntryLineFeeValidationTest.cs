using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryLineFeeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCF_BaseValue()
		{
			entryLineFee.CF_BaseValue = 0;
			AssertHasMessageErrorContaining(entryLineFee.CF_BaseValueInfo, "You have not entered a value.");
			entryLineFee.CF_BaseValue = -1;
			AssertHasMessageErrorContaining(entryLineFee.CF_BaseValueInfo, "value cannot be negative.");
			entryLineFee.CF_BaseValue = 1;
			AssertNoMessageErrorContaining(entryLineFee.CF_BaseValueInfo, "You have not entered a value.");
		}

		public void TestCheckCF_ChargeAmount()
		{
			entryLineFee.CF_ChargeAmount = -1;
			AssertHasMessageErrorContaining(entryLineFee.CF_ChargeAmountInfo, "Charge Amount cannot be negative.");
			entryLineFee.CF_ChargeAmount = 1;
			AssertNoMessageErrorContaining(entryLineFee.CF_ChargeAmountInfo, "Charge Amount cannot be negative.");
		}

		public void TestCheckCF_ChargeType()
		{
			entryLineFee.CF_ChargeType = ZString.Empty;
			AssertHasMessageErrorContaining(entryLineFee.CF_ChargeTypeInfo, "You have not entered a Charge Type.");
			entryLineFee.CF_ChargeType = "INV";
			AssertHasMessageErrorContaining(entryLineFee.CF_ChargeTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCF_RateOverrideReasonCode()
		{
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			AssertNoNotifications(entryLineFee.CF_RateOverrideReasonCodeInfo);
			entryLineFee.CF_RateOverrideReasonCode = "INV";
			AssertHasMessageErrorContaining(entryLineFee.CF_RateOverrideReasonCodeInfo, "The code you have selected is not in the list.");
			entryLineFee.CF_RateOverrideReasonCode = "ADD";
			AssertNoMessageErrorContaining(entryLineFee.CF_RateOverrideReasonCodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCF_MethodOfPayment()
		{
			entryLineFee.CF_Rate = 1m;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			AssertHasErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, MandatoryValidation.MustBeEntered);
			entryLineFee.CF_MethodOfPayment = "ZZ";
			AssertHasMessageErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, "The code you have selected is not in the list.");
			AssertNoErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, MandatoryValidation.MustBeEntered);
			entryLineFee.CF_MethodOfPayment = "CAS";
			AssertNoMessageErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, "The code you have selected is not in the list.");
			entryLineFee.CF_Rate = 0m;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			AssertNoErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCF_Rate()
		{
			entryLineFee.CF_Rate = 1;
			AssertNoMessageErrorContaining(entryLineFee.CF_RateInfo, "Tax Rate allows only 5 decimal places.");
			entryLineFee.CF_Rate = 1.999999;
			AssertHasMessageErrorContaining(entryLineFee.CF_RateInfo, "Tax Rate allows only 5 decimal places.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.CustomsEntryHeaders.AddNew();
			var invoiceHeader = dec.Invoices.AddNew();
			var entryInstruction = dec.CusEntryInstruction;
			entryInstruction.CEI_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "XXX";
			invoiceLine.JI_Tariff = "A";
			dec.DoMerge();
			entryLine = dec.CustomsEntryHeaders[0].MergedLines[0];
			entryLineFee = entryLine.Fees.AddNew();
		}

		JobDeclaration dec;
		CusEntryLine entryLine;
		CusEntryLineFee entryLineFee;
	}
}
