using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	public class NZCurrencyConverterWithTestExchangeRatesTest : TestCaseWithFactory
	{
		public void TestDateForRateGetsSetFromJE_ExchangeRateDate()
		{
			Declaration.JE_EDITransmitDate = new ZDateTime(2004, 1, 1);
			Converter.GetExchangeRate(currency);
			AssertEquals("Converter.DateForRate", new ZDateTime(2004, 1, 1), Converter.DateForRate);

			Declaration.JE_EDITransmitDate = ZDateTime.Empty;
			Converter.GetExchangeRate(currency);
			AssertEquals("Converter.DateForRate", Declaration.CachedTodaysDate, Converter.DateForRate);
		}

		public void TestBasicRateExistsAndNotExists()
		{
			AssertEquals("Exchange Rate is there", TestRate, Converter.GetExchangeRate(currency));
			exchangeRate.Delete();
			AssertEquals("No exchange rate for this currency", 0.00m, Converter.GetExchangeRate(currency));
		}

		public void TestBeforeAndAfterDate()
		{
			AssertEquals("Exchange Rate is there", TestRate, Converter.GetExchangeRate(currency));
			Declaration.JE_EDITransmitDate = new ZDateTime(2003, 11, 13);
			AssertEquals("No exchange rate for this date", 0.00m, Converter.GetExchangeRate(currency));
			Declaration.JE_EDITransmitDate = new ZDateTime(2003, 11, 16);
			AssertEquals("No exchange rate for this date", 0.00m, Converter.GetExchangeRate(currency));
			Declaration.JE_EDITransmitDate = new ZDateTime(2003, 11, 14);
			AssertEquals("Exchange Rate is there", TestRate, Converter.GetExchangeRate(currency));
		}

		public void TestConvertForeignToLocal()
		{
			Money foreignAmount = new Money(1000.00m, currency);
			Money localAmount = Converter.ConvertRounded(foreignAmount, Converter.LocalCurrency);
			AssertEquals("Conversion of $1000 @ .70", new ZDecimal(1428.57), localAmount.Amount);
			AssertEquals("Money Currency", Converter.LocalCurrency.RX_Code, localAmount.Currency.Code);
		}

		public void TestConvertLocalToForeign()
		{
			Money localAmount = new Money(1428.57M, Converter.LocalCurrency);
			Money foreignAmount = Converter.ConvertRounded(localAmount, currency);
			AssertEquals("Conversion of $1428.57 @ 1 / .70", 1000.00m, foreignAmount.Amount);
			AssertEquals("Money Currency", currency.RX_Code, foreignAmount.Currency.Code);
		}

		public void TestLocalToLocal()
		{
			Money localAmount1 = new Money(1000.00m, Converter.LocalCurrency);
			Money localAmount2 = Converter.ConvertRounded(localAmount1, Converter.LocalCurrency);
			AssertEquals("Conversion of $1000 @ 1", localAmount1.Amount, localAmount2.Amount);
			AssertEquals("Money Currency", Converter.LocalCurrency.PK, localAmount2.Currency.PK);
		}

		public void TestForeignToForeign()
		{
			Money foreignAmount1 = new Money(1000, currency);
			Money foreignAmount2 = Converter.ConvertRounded(foreignAmount1, currency);
			AssertEquals("Conversion of $1000 @ .70 > @ 1/.70", foreignAmount1.Amount, foreignAmount2.Amount);
			AssertEquals("Money Currency", foreignAmount1.Currency, foreignAmount2.Currency);
		}

		public void TestTestModeAltersExchangeRateBehaviourInUnitTestEnvironment()
		{
			IsInTestMode = true;
			currency.RX_Code = "AUD";
			AssertEquals("Exchange Rate is there", 0.90m, Converter.GetExchangeRate(currency));
		}

		public void TestLocalCurrency()
		{
			AssertEquals("Just to make sure the tests are running in the right country....", "NZD", Converter.LocalCurrency.RX_Code);
		}

		[ExpectNoExceptions]
		public void TestCollectionModification()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var header = declaration.CusEntryHeader;
			var entryLine = header.MergedLines.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			header.CH_IsActive = false;
			var milestone = declaration.WorkflowItems.Milestones.AddNew();
			var workflowDescriptor = new Customs.Business.JobDeclarationWorkflowDescriptor();
			var recipientPartyTypeList = new MessageRecipientPartyTypeList(workflowDescriptor.SupportedMessageRecipientParties(declaration.WorkflowItems.AddNew(), declaration));
			var notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			var processor = workflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			IsInTestMode = false;
			SetupTestCurrencyAndExchangeRate();
		}

		protected const decimal TestRate = 0.70m;
		protected RefCurrency currency;
		protected RefExchangeRate exchangeRate;

		#region SetupTestCurrencyAndExchangeRate
		void SetupTestCurrencyAndExchangeRate()
		{
			currency = Factory.New<RefCurrency>();
			currency.RX_Code = "XXX";
			currency.RX_Desc = "Description";
			currency.RX_IsSystem = true;
			currency.RX_SubUnitName = "SubUnit";
			currency.RX_SubUnitRatio = 100;
			currency.RX_Symbol = "X";
			currency.RX_UnitName = "XXX";

			exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_GC = Env.CurrentCompany.PK;
			exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.RE_StartDate = new ZDateTime(2003, 11, 14);
			exchangeRate.RE_ExpiryDate = new ZDateTime(2003, 11, 15);
			exchangeRate.RE_SellRate = TestRate;
			exchangeRate.RE_ExRateType = "CUS";
		}
		#endregion

		#region Converter
		NZCurrencyConverterWithTestExchangeRates fConverter;
		protected NZCurrencyConverterWithTestExchangeRates Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new NZCurrencyConverterWithTestExchangeRates(Declaration);
				}
				return fConverter;
			}
		}
		#endregion

		#region Declaration
		JobDeclaration fDeclaration;
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					fDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
					fDeclaration.JE_EDITransmitDate = new ZDateTime(2003, 11, 14);
				}
				return fDeclaration;
			}
		}
		#endregion

		#region IsInTestMode
		protected bool IsInTestMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.Value; }
			set { NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}
		#endregion

		#endregion
	}
}
