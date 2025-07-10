using System;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Registry.Business.InvoiceRemittanceCustomisationElement;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRemittanceConfiguration))]
	sealed class InvoiceRemittanceConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCodeMaxLength()
		{
			AssertEquals(3, BizObj.CodeInfo.MaxLength);
		}

		public void TestCodeReadOnly()
		{
			PreparedUsedConfiguration();

			AssertEquals(true, BizObj.Code_ReadOnly);
		}

		public void TestDelete()
		{
			PreparedUsedConfiguration();

			AssertEquals(false, BizObj.CanDelete);
			AssertEquals("This code is already used thus cannot be deleted.", BizObj.ReasonForNotAbleToDelete);
		}

		void PreparedUsedConfiguration()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_InvoicePaymentReferenceCode = "AAA";
			Factory.Save();

			BizObj.Code = "AAA";
			BizObj.Description = "Test";
			BizObj.DebtorLocation = "ALL";
		}

		public void TestCalculateInvoiceRemittanceReference()
		{
			BizObj.Elements[ElementNames.CustomCode1].Include = true;
			BizObj.Elements[ElementNames.CustomCode1].DigitCode = "XX";
			BizObj.Elements[ElementNames.CustomCode1].Order = 1;

			BizObj.Elements[ElementNames.InvoiceTransactionReference].Include = true;
			BizObj.Elements[ElementNames.InvoiceTransactionReference].DigitCode = "8";
			BizObj.Elements[ElementNames.InvoiceTransactionReference].Order = 2;

			var referenceNumber = BizObj.GetInvoiceRemittanceReference(new DummyInvoiceRemittanceConfiguration());
			AssertEquals("XX00010035", referenceNumber);
		}

		public void TestCalculateInvoiceRemittanceReferenceWithMod97()
		{
			BizObj.Elements[ElementNames.CustomCode1].Include = true;
			BizObj.Elements[ElementNames.CustomCode1].Order = 1;
			BizObj.Elements[ElementNames.CustomCode1].DigitCode = "RF";
			BizObj.Elements[ElementNames.CustomCode1].CheckDigit = ElementNames.CheckDigit1;

			BizObj.Elements[ElementNames.CheckDigit1].Include = true;
			BizObj.Elements[ElementNames.CheckDigit1].Order = 2;
			BizObj.Elements[ElementNames.CheckDigit1].DigitCode = "2";
			BizObj.Elements[ElementNames.CheckDigit1].CheckDigitAlgorithm = CheckDigitAlgorithm.MOD97;

			BizObj.Elements[ElementNames.InvoiceTransactionReference].Include = true;
			BizObj.Elements[ElementNames.InvoiceTransactionReference].Order = 3;
			BizObj.Elements[ElementNames.InvoiceTransactionReference].DigitCode = "8";
			BizObj.Elements[ElementNames.InvoiceTransactionReference].CheckDigit = ElementNames.CheckDigit2;

			BizObj.Elements[ElementNames.CheckDigit2].Include = true;
			BizObj.Elements[ElementNames.CheckDigit2].Order = 4;
			BizObj.Elements[ElementNames.CheckDigit2].DigitCode = "1";
			BizObj.Elements[ElementNames.CheckDigit2].CheckDigit = ElementNames.CheckDigit1;
			BizObj.Elements[ElementNames.CheckDigit2].CheckDigitAlgorithm = CheckDigitAlgorithm.Algorithm731;

			var referenceNumber = BizObj.GetInvoiceRemittanceReference(new DummyInvoiceRemittanceConfiguration());
			AssertEquals("RF71000100353", referenceNumber);

			referenceNumber = BizObj.GetInvoiceRemittanceReference(new DummyInvoiceRemittanceConfiguration { InvoiceTransactionReference = "165" });
			AssertEquals("RF091656", referenceNumber);
		}

		public void TestReferenceNumberTotalLength()
		{
			BizObj.Elements[ElementNames.CustomCode1].Include = true;
			BizObj.Elements[ElementNames.CustomCode1].DigitCode = "ABCD";

			BizObj.Elements[ElementNames.InvoiceTransactionReference].Include = true;
			BizObj.Elements[ElementNames.InvoiceTransactionReference].DigitCode = "8";

			var length = BizObj.ReferenceNumberTotalLength;
			AssertEquals("length should be length('ABCD') + 8 = 12.", 12, length);
		}

		public void TestGetInvoiceTotalInLocalCurrencyDigitCode()
		{
			BizObj.Elements[ElementNames.InvoiceTotalInLocalCurrency].Include = true;
			BizObj.Elements[ElementNames.InvoiceTotalInLocalCurrency].DigitCode = "8";

			var result = BizObj.GetInvoiceTotalInLocalCurrencyDigitCode("12345");
			AssertEquals("00012345", result);

			result = BizObj.GetInvoiceTotalInLocalCurrencyDigitCode("123456789");
			AssertEquals("23456789", result);

			BizObj.Elements[ElementNames.InvoiceTotalInLocalCurrency].DigitCode = "ab";
			result = BizObj.GetInvoiceTotalInLocalCurrencyDigitCode("12345");
			AssertEquals("", result);
		}

		public void TestGetInvoiceTotalInInvoiceCurrencyDigitCode()
		{
			BizObj.Elements[ElementNames.InvoiceTotalInInvoiceCurrency].Include = true;
			BizObj.Elements[ElementNames.InvoiceTotalInInvoiceCurrency].DigitCode = "8";

			var result = BizObj.GetInvoiceTotalInInvoiceCurrencyDigitCode("12345");
			AssertEquals("00012345", result);

			result = BizObj.GetInvoiceTotalInInvoiceCurrencyDigitCode("123456789");
			AssertEquals("23456789", result);

			BizObj.Elements[ElementNames.InvoiceTotalInInvoiceCurrency].DigitCode = "ab";
			result = BizObj.GetInvoiceTotalInInvoiceCurrencyDigitCode("12345");
			AssertEquals("", result);
		}

		class DummyInvoiceRemittanceConfiguration : IInvoiceRemittance
		{
			public ZString InvoiceNumber => throw new NotImplementedException();

			public ZString InvoiceTotalInLocalCurrency => throw new NotImplementedException();

			public ZString InvoiceTotalInInvoiceCurrency => throw new NotImplementedException();

			public ZString Message => throw new NotImplementedException();

			public ZString BillerCode => throw new NotImplementedException();

			public ZString BillerAccountNumber => throw new NotImplementedException();

			public ZString DebtorOrganizationCode => throw new NotImplementedException();

			public ZString DebtorClientNumber => throw new NotImplementedException();

			public ZString InvoiceTransactionReference { get; set; } = "00010035";

			public ZString InvoiceNumberInNumeric => throw new NotImplementedException();
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new InvoiceRemittanceConfiguration BizObj
		{
			get
			{
				var obj = (InvoiceRemittanceConfiguration)base.BizObj;
				obj.CurrentFallbackLevel = NewFallbackLevel();
				return obj;
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new InvoiceRemittanceConfiguration();
		}

		#endregion
	}
}
