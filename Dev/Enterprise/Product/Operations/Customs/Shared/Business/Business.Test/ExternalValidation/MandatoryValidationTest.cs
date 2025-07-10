using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MandatoryValidationTest : TestCaseWithFactory
	{
		public void TestExchangeRateValidation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ExportDate = new ZDateTime(2019, 2, 2, 19, 42, 03);

			var invoice = declaration.Invoices.AddNew();
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ABC";
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2019, 2, 2);
			rate.RE_ExpiryDate = new ZDateTime(2019, 2, 2);
			rate.RE_SellRate = 0.541m;

			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;

			AssertEquals("Exchange rate", 0.541m, invoice.JZ_InvoiceCurrExRate);
			AssertNoWarningContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no exchange rate in the database for the valuation date");

			declaration.JE_ExportDate = new ZDateTime(2019, 2, 3, 19, 42, 03);
			invoice.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertHasWarningContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no exchange rate in the database for the valuation date");
		}

		public void TestExchangeRateFallBackWarning()
		{
			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);

			RefCurrency foreignCurrency = Factory.New<RefCurrency>();
			foreignCurrency.RX_Code = "~~~";

			RefExchangeRate exchangeRate = foreignCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_SellRate = 0.5m;
			exchangeRate.RE_StartDate = new ZDateTime(2005, 1, 1);
			exchangeRate.RE_ExpiryDate = exchangeRate.RE_StartDate;

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = exchangeRate.RE_StartDate.AddDays(1);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;

			var validation = new ExternalMessageValidation(invoice);

			using (invoice.SuspendValidationTesting())
			{
				validation.ValidateExchangeRateExist((RefCurrencyCurrencyConverter)invoice.CurrencyConverter, invoice.JZ_RX_NKInvoice_CurrencyInfo);
			}

			var bizObj = Factory.New<DummyBusinessObject>();

			validation.ValidateExchangeRateExist((RefCurrencyCurrencyConverter)invoice.CurrencyConverter, bizObj.Z0_CodeInfo, 2);
			AssertEquals("Fall back is not warned", false, bizObj.Z0_CodeInfo.HasWarnings());

			testDec.JE_ExportDate = exchangeRate.RE_StartDate.AddDays(2);
			validation.ValidateExchangeRateExist((RefCurrencyCurrencyConverter)invoice.CurrencyConverter, bizObj.Z0_DescriptionInfo, 2);
			AssertEquals("Fall back is not warned", false, bizObj.Z0_DescriptionInfo.HasWarnings());
		}

		public void TestExchangeRateValidDateOfExportError()
		{
			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);
			var foreignCurrency = Factory.New<RefCurrency>();
			foreignCurrency.RX_Code = "~~~";

			var exchangeRate = foreignCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_SellRate = 0.5m;
			exchangeRate.RE_StartDate = new ZDateTime(2005, 1, 1);
			exchangeRate.RE_ExpiryDate = exchangeRate.RE_StartDate;

			var testDec = Factory.New<JobDeclarationForTest>();
			testDec.JE_ExportDate = exchangeRate.RE_StartDate.AddDays(1);
			var invoice = testDec.Invoices.AddNew();

			AssertNoErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "A valid date of export is required to calculate exchange rates.");
			testDec.IsValidDateOfValuation = false;
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			AssertHasMessageError(invoice.JZ_RX_NKInvoice_CurrencyInfo, "A valid date of export is required to calculate exchange rates.");
		}

		public void TestCheckEntered()
		{
			TestDeclaration testDec = Factory.New<TestDeclaration>();
			AssertEquals(false, testDec.HasMessageErrors);
			testDec.JE_AddInfo = "ES";
			AssertEquals(false, testJobDeclaration.HasMessageErrors);

			AssertEquals(false, testDec.HasMessageErrors);
			testDec.JE_AddInfo = "";
			AssertEquals(true, testDec.HasMessageErrors);
		}

		public void TestInvalidCharacters()
		{
			TestDeclaration testDec = Factory.New<TestDeclaration>();
			var info = testDec.IllegalCharactersTestPropertyInfo;
			info.Value = (ZString)"ABC & 123";

			AssertNoWarningContaining(info, "Contains illegal Characters: ");
			// because of needing a matching pair ...
			AssertNoWarningContaining(info, "Contains illegal Characters: [6] Tab");
			AssertNoWarningContaining(info, "Contains illegal Characters: [5] Enter");
			AssertNoWarningContaining(info, "Contains illegal Characters: [6] Tab, [10] Enter");

			info.Value = (ZString)@"ABC &	123";
			AssertHasWarningContaining(info, "Contains illegal Characters: [6] Tab");

			info.Value = (ZString)@"ABC 
& 123";
			AssertHasWarningContaining(info, "Contains illegal Characters: [5] Enter");

			info.Value = (ZString)@"ABC &	123
456";
			AssertHasWarningContaining(info, "Contains illegal Characters: [6] Tab, [10] Enter");

			// because of needing a matching pair ...
			AssertHasWarningContaining(info, "Contains illegal Characters: ");

			AssertNoWarningContaining(info, "...");
			info.Value = (ZString)@"	1	2	3	4	5	6	7";
			AssertNoWarningContaining(info, "...");
			info.Value = (ZString)@"	1	2	3	4	5	6	7	8";
			AssertHasWarningContaining(info, "...");
		}

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override JobDeclarationValidation GetNewValidation()
			{
				return new ValidationForTestDeclaration(this);
			}

			public new ValidationForTestDeclaration Validation
			{
				get { return (ValidationForTestDeclaration)GetNewValidation(); }
			}

			public new class Schema : BaseJobDeclaration.Schema
			{
				public const string IllegalCharactersTestProperty = "IllegalCharactersTestProperty";
			}

			public ZString IllegalCharactersTestProperty
			{
				get { return illegalCharactersTestProperty; }
				set
				{
					illegalCharactersTestProperty = value;
					Validation.ValidateIllegalCharactersTestProperty();
				}
			}
			ZString illegalCharactersTestProperty;

			public ZPropertyInfo IllegalCharactersTestPropertyInfo
			{
				get { return GetZPropertyInfo(Schema.IllegalCharactersTestProperty); }
			}
		}

		class ValidationForTestDeclaration : BaseJobDeclarationValidation
		{
			public ValidationForTestDeclaration(BaseJobDeclaration dec) : base(dec)
			{
			}

			protected TestDeclaration TestDec
			{
				get { return (TestDeclaration)base.Parent; }
			}

			protected override void CheckJE_AddInfo()
			{
				base.CheckJE_AddInfo();
				new ExternalMessageValidation(TestDec).CheckEntered(TestDec.JE_AddInfoInfo, "TestMessageError");
			}

			public void ValidateIllegalCharactersTestProperty()
			{
				ValidateCalculatedProperty(TestDec.IllegalCharactersTestPropertyInfo);
			}

			protected void CheckIllegalCharactersTestProperty()
			{
				ExternalMessageValidation.CheckForIllegalCharacters(TestDec.IllegalCharactersTestPropertyInfo);
			}
		}

		class JobDeclarationForTest : BaseJobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZDateTime DateOfValuation => IsValidDateOfValuation ? base.DateOfValuation : ZDateTime.Invalid;
			public bool IsValidDateOfValuation = true;
		}

		#region Implementation

		BaseJobDeclaration testJobDeclaration;

		protected override void SetUp()
		{
			base.SetUp();
			testJobDeclaration = BaseJobDeclaration.New(Factory);
		}

		#endregion
	}
}
