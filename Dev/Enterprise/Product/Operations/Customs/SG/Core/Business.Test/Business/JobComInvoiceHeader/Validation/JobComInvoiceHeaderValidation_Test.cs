using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class JobComInvoiceHeaderValidation_Test : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public override void TestValidateJZ_CU_RelatedHouseBill()
		{
			var houseBill = InvoiceHeader.JobDeclaration.Bills.AddNew();
			InvoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			var entry1 = InvoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			var entry2 = InvoiceHeader.JobDeclaration.ActiveEntryHeaders.AddNew();
			InvoiceHeader.Validation.ValidateJZ_CU_RelatedHouseBill();
			Assert(!InvoiceHeader.JZ_CU_RelatedHouseBillInfo.HasMessageErrors());
		}

		public void TestRefreshJE_Calc_InvoicesCountValidationWhenAnInvoiceIsAttached()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			for (int index = 0; index < 20; index++)
			{
				declaration.Invoices.AddNew();
			}

			JobComInvoiceHeader standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_JE = declaration.PK;
			AssertHasError(declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
			declaration.Invoices[0].Delete();
			AssertNoError(declaration.JE_Calc_InvoicesCountInfo, Customs.SG.V4.Business.JobDeclarationValidation.Only20InvoicesAreAllowed);
		}

		public void TestInvoiceNumber()
		{
			Validation.ValidateJZ_InvoiceNumber();
			AssertEquals(true, InvoiceHeader.JZ_InvoiceNumberInfo.HasMessageErrors());
			InvoiceHeader.JZ_InvoiceNumber = "123";
			Validation.ValidateJZ_InvoiceNumber();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceNumberInfo.HasMessageErrors());
		}

		public void TestInvoiceDate()
		{
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			Validation.ValidateJZ_InvoiceDate();
			AssertEquals(true, InvoiceHeader.JZ_InvoiceDateInfo.HasMessageErrors());
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
			Validation.ValidateJZ_InvoiceDate();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceDateInfo.HasMessageErrors());
		}

		public void TestInvoiceCurrency()
		{
			Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertEquals(false, InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			InvoiceHeader.JZ_InvoiceAmount = 10m;
			Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertEquals(true, InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertEquals(false, InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "NEW";
			InvoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertEquals(true, InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasMessageErrors());
		}

		public void TestSupplier()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(false, InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			OrgHeader supplier = Factory.New<OrgHeader>();
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(true, InvoiceHeader.JZ_OH_SupplierInfo.HasErrors());
			supplier.OH_IsConsignor = true;
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(false, InvoiceHeader.JZ_OH_SupplierInfo.HasErrors());
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Validation.ValidateJZ_OH_Supplier();
			AssertEquals(false, InvoiceHeader.JZ_OH_SupplierInfo.HasMessageErrors());
		}

		public void TestIncoTerm()
		{
			InvoiceHeader.JZ_IncoTerm = "XXX";
			Validation.ValidateJZ_IncoTerm();
			AssertEquals(true, InvoiceHeader.JZ_IncoTermInfo.HasMessageErrors());
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CFR;
			Validation.ValidateJZ_IncoTerm();
			AssertEquals(false, InvoiceHeader.JZ_IncoTermInfo.HasMessageErrors());
		}

		public void TestInvoiceAmount()
		{
			InvoiceHeader.JZ_InvoiceAmount = -1;
			Validation.ValidateJZ_InvoiceAmount();
			AssertEquals(true, InvoiceHeader.JZ_InvoiceAmountInfo.HasErrors());
			InvoiceHeader.JZ_InvoiceAmount = 1;
			Validation.ValidateJZ_InvoiceAmount();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceAmountInfo.HasErrors());
		}

		#region overrides
		protected override Type GetTypeForTest()
		{
			return typeof(JobComInvoiceHeaderValidation_OUT);
		}

		public override void TestValidateBalance()
		{
			var dec = JobDeclaration.New(Factory);
			var invoice = dec.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100;
			invoice.JZ_InvoiceAmount = 1000m;
			dec.ResumeApportionment();
			AssertHasWarning(invoice.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
			invoice.JZ_InvoiceAmount = 100m;
			dec.ResumeApportionment();
			AssertNoWarning(invoice.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
			invoice.JZ_InvoiceAmount = 0m;
			dec.ResumeApportionment();
			AssertHasWarning(invoice.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
		}

		public override void TestValidateJZ_InvoiceNumber()
		{
			Assert(true);
		}

		public override void TestValidateJZ_OH_Supplier()
		{
			Assert(true);
		}

		public override void TestValidateAbsenceOfOFTOrONS()
		{
			Assert(true);
		}

		public override void TestValidateJZ_MessageType()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			Assert("Initially no errors", !invoice.JZ_MessageTypeInfo.HasErrors());
			invoice.JZ_MessageType = "";
			Assert("In error", invoice.JZ_MessageTypeInfo.HasErrors());
			invoice.JZ_MessageType = "ZZZ";
			Assert("In error", invoice.JZ_MessageTypeInfo.HasErrors());
			invoice.JZ_MessageType = MessageTypeCodeList.Codes.COO;
			Assert("OK", !invoice.JZ_MessageTypeInfo.HasErrors());
			invoice.JZ_MessageType = MessageTypeCodeList.Codes.OUT;
			Assert("OK", !invoice.JZ_MessageTypeInfo.HasErrors());
			invoice.JZ_JE = Declaration.PK;
			invoice.Validation.ValidateJZ_MessageType();
			Assert("No error when attached to a declaration", !invoice.JZ_MessageTypeInfo.HasErrors());
		}

		#endregion
		#region Implementation
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		new JobDeclaration declaration;
		#endregion
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		new JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceHeaderValidation Validation
		{
			get
			{
				return InvoiceHeader.Validation;
			}
		}
		#endregion
	}
}
