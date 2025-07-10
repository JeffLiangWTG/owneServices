using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract partial class JobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckJZ_MarksAndNumbersIsWesternEuropeanIfRequired()
		{
			var invoice = Factory.New<TestInvoice>();
			invoice.JZ_MarksAndNumbers = "XX1";
			var targetInfo = invoice.JZ_MarksAndNumbersInfo;
			AssertNoError(targetInfo, EnglishCharactersValidation.GetNotificationMessage(targetInfo));

			invoice.JZ_MarksAndNumbers = "你好";
			AssertHasError(targetInfo, EnglishCharactersValidation.GetNotificationMessage(targetInfo));
		}

		public virtual void TestValidateJZ_CU_RelatedHouseBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
				Bill houseBill = testDec.Bills.AddNew();
				invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
				AssertNoMessageError(invoice.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);
				CusEntryHeader entry1 = testDec.ActiveEntryHeaders.AddNew();
				invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
				AssertNoMessageError(invoice.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);
				CusEntryHeader entry2 = testDec.ActiveEntryHeaders.AddNew();
				invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
				AssertHasMessageError(invoice.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);
				invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
				AssertNoMessageError(invoice.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);
			}
		}

		public void TestCheckJZ_GB()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader standaloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_GB = ZGuid.Empty;
			standaloneInvoice.JZ_GB = ZGuid.Empty;

			AssertHasErrorContaining(standaloneInvoice.JZ_GBInfo, InvoiceHeaderValidation.InvalidBranch);
			AssertNoErrorContaining(invoice.JZ_GBInfo, InvoiceHeaderValidation.InvalidBranch);

			standaloneInvoice.JZ_GB = ZGuid.Invalid;
			AssertHasErrorContaining(standaloneInvoice.JZ_GBInfo, InvoiceHeaderValidation.InvalidBranch);
			AssertNoErrorContaining(invoice.JZ_GBInfo, InvoiceHeaderValidation.InvalidBranch);

			standaloneInvoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			AssertNoErrorContaining(standaloneInvoice.JZ_GBInfo, InvoiceHeaderValidation.InvalidBranch);
		}

		public virtual void TestValidateBalance()
		{
			var line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			declaration.ResumeApportionment();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_Calc_BalanceInfo, "total of all invoice lines");
			invoiceHeader.JZ_InvoiceAmount = 100m;
			declaration.ResumeApportionment();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_Calc_BalanceInfo, "total of all invoice lines");
			invoiceHeader.JZ_InvoiceAmount = 0m;
			declaration.ResumeApportionment();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_Calc_BalanceInfo, "total of all invoice lines");
		}

		public virtual void TestValidateAbsenceOfOFTOrONS()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.RunPreSaveValidation();
			AssertHasWarning(invoice.JZ_Calc_CIFAmountInfo, "The CIF amount is the same as the FOB amount and you have not entered any Overseas Freight or Insurance.");

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			invoice.RunPreSaveValidation();
			AssertNoWarning(invoice.JZ_Calc_CIFAmountInfo, "The CIF amount is the same as the FOB amount and you have not entered any Overseas Freight or Insurance.");
		}

		public virtual void TestValidateJZ_InvoiceNumber()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_InvoiceNumber = "";
			Assert("In error", invoice.JZ_InvoiceNumberInfo.HasErrors());

			invoice.JZ_JE = declaration.PK;
			Assert("No error when attached to a declaration", !invoice.JZ_InvoiceNumberInfo.HasErrors());
		}

		public virtual void TestValidateJZ_MessageType()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);

			invoice.JZ_MessageType = "";
			AssertHasErrorContaining(invoice.JZ_MessageTypeInfo, MandatoryValidation.MustBeEntered);

			invoice.JZ_MessageType = "ZZZ";
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(invoice.JZ_MessageTypeInfo, ListValidation.InvalidCodeError);

			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, ListValidation.InvalidCodeError);

			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, ListValidation.InvalidCodeError);

			invoice.JZ_JE = declaration.PK;
			((InvoiceHeaderValidation)invoice.Validation).ValidateJZ_MessageType();
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(invoice.JZ_MessageTypeInfo, ListValidation.InvalidCodeError);
		}

		public virtual void TestValidateJZ_OH_Buyer()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			Assert("Initially no errors", !invoice.JZ_OH_BuyerInfo.HasErrors());
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.Validation.ValidateJZ_OH_Buyer();
			Assert("In error", invoice.JZ_OH_BuyerInfo.HasErrors());

			invoice.JZ_JE = declaration.PK;
			invoice.Validation.ValidateJZ_OH_Buyer();
			Assert("No error when attached to a declaration", !invoice.JZ_OH_BuyerInfo.HasErrors());
		}

		public virtual void TestValidateJZ_OH_Supplier()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			BaseJobDeclaration declaration = invoiceHeader.JobDeclaration;
			AssertNotNull("Precondition: invoiceHeader.JobDeclaration", declaration);
			invoiceHeader.JZ_JE = ZGuid.Empty;
			new FakeDeclarationCreatorForInvoice(invoiceHeader);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasErrorContaining(invoiceHeader.JZ_OH_SupplierInfo, MandatoryValidation.MustBeEntered);
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertNoErrorContaining(invoiceHeader.JZ_OH_SupplierInfo, MandatoryValidation.MustBeEntered);

			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasError(invoiceHeader.JZ_OH_SupplierInfo, InvoiceHeaderValidation.ErrorCannotUseMISCOnCommercialInvoiceHeader);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);
		}

		protected virtual BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			return invoiceHeader;
		}

		public void TestValidationObject()
		{
			AssertEquals(GetTypeForTest(), GetInvoiceHeader().Validation.GetType());
		}

		#region Implementation

		protected BaseJobDeclaration declaration;
		protected BaseJobComInvoiceHeader invoiceHeader;
		protected InvoiceHeaderValidation validation;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = BaseJobDeclaration.New(Factory);
			invoiceHeader = declaration.Invoices.AddNew();
		}

		protected abstract Type GetTypeForTest();

		protected virtual string GetValidSupplierCode()
		{
			return "";
		}

		#endregion
	}
}
