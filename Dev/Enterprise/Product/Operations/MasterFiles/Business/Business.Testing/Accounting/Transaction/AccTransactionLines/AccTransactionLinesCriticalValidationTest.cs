using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccTransactionLinesCriticalValidationTest : CriticalValidationTest<AccTransactionLines>
	{
		public void TestCheckReverseDate_IReverseDateValidation()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = transaction.PK;
			line.AL_LineType = TransactionLineTypes.Revenue;

			var validationMock = new Mock<IReverseDateValidation>();
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			countryFactoryMock.As<IInstanceProvider<IReverseDateValidation>>().Setup(x => x.Get()).Returns(validationMock.Object);
			var factoryMock = new Mock<IGlobalAccountingCountryFactory>();
			factoryMock.Setup(c => c.GetCountryFactory(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				validationMock.Setup(x => x.ValidateReverseDate(It.IsAny<AccTransactionLines>())).Returns((ResourceString)null);
				AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods("DO NOT report error: InvalidReverseDate"));

				validationMock.Reset();
				validationMock.Setup(x => x.ValidateReverseDate(It.IsAny<AccTransactionLines>())).Returns(ResString.GetMultilingualString("Test", "Dummy Error"));
				AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods("Report error: InvalidReverseDate", true, CriticalValidationErrorType.InvalidReverseDate, "Dummy Error"));
			}
		}

		public void TestLedgerHasChangedByDataRefreshBusError()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
			transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var transactionInAnotherFactory = anotherFactory.Load<AccTransactionHeader>(transaction.PK);
			transactionInAnotherFactory.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionInAnotherFactory.AH_TransactionType = TransactionTypes.Invoice; //currently there is not case when ledger can be changed without transaction type.
			anotherFactory.Save();

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = transaction.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods("Ledger is changed.", true, CriticalValidationErrorType.TransactionHeaderWasCriticallyChangedByDataRefreshBus_2,
				"Transaction was critically changed by this user during another operation. Please close this screen as this operation is not valid any more.",
				"Standard validation must catch this case and doesn't not allow to save.",
				$"Header: PK = {transaction.PK}"));
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestErrorReportingForAssociatedJobChargesForReversedWIPACR()
		{
			ExceptionReporterTestListener.Instance.Clear();
			var line1 = GetLine(Factory, true, TransactionLineTypes.WIP, true, true);
			var line2 = GetLine(Factory, true, TransactionLineTypes.Accrual, true, true);

			line1.AL_ReverseDate = ZDateTime.Empty;
			line2.AL_ReverseDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("No error", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			line1.AL_ReverseDate = new ZDateTime(2006, 05, 05);
			try
			{
				Factory.Save();
				Fail("Saving should be terminated with exception");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Error message is reported for WIP line", "reversed WIP should not be linked to a Job Charge", ex.Message);
			}

			string errorMessage = ExceptionReporterTestListener.Instance[0].Message;
			AssertContains("Error message is reported for WIP line", "A reversed WIP has an associated job charge.", errorMessage);
			ExceptionReporterTestListener.Instance.Clear();

			line2.AL_ReverseDate = new ZDateTime(2006, 05, 05);
			try
			{
				Factory.Save();
				Fail("Saving should be terminated with exception");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Error message about previous Critical Validation error", "A critical validation error has occurred. Please close the form, then try again.", ex.Message);
			}

			AssertEquals("No further Error reports", 0, ExceptionReporterTestListener.Instance.Count);
		}

		#region General Ledger Account can not be empty on Transaction Line

		public void TestErrorReportingAL_AGShouldNotBeEmpty_Accrual()
		{
			AssertTransactionLineWithNoGLAccount(TransactionLineTypes.Accrual);
		}

		public void TestErrorReportingAL_AGShouldNotBeEmpty_Cost()
		{
			AssertTransactionLineWithNoGLAccount(TransactionLineTypes.Cost, true);
		}

		public void TestErrorReportingAL_AGShouldNotBeEmpty_Revenue()
		{
			AssertTransactionLineWithNoGLAccount(TransactionLineTypes.Revenue, true);
		}

		public void TestErrorReportingAL_AGShouldNotBeEmpty_WIP()
		{
			AssertTransactionLineWithNoGLAccount(TransactionLineTypes.WIP);
		}

		void AssertTransactionLineWithNoGLAccount(ZString lineType, bool isHeaderRequired = false)
		{
			var cmtChargeCode = CommentChargeCode;  //calls on saving so must be done first

			var lineToBeSaved = GetLine(Factory, true, lineType, true);
			lineToBeSaved.AL_AC = Env.Registry.FreightChargeCode;

			var noExceptionTestCase = new TestCaseDefinition_ForSeparateTestsMethods("Expected no exceptions, line should be fine to save");
			AssertOnSavingCheck(lineToBeSaved, noExceptionTestCase);

			Factory.Save();
			lineToBeSaved.AL_AG = ZGuid.Empty;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Line without GL Account",
				true, CriticalValidationErrorType.TransactionLineWithoutGLAccountAndChargeCode_2,
				"The system has attempted to create a transaction line of type ", lineType,
				@"but was not successful due to one of the following reasons:
1.The transaction line does not have a GL account specified.
2.The transaction line does not have a charge code specified.
3.The transaction line has a charge code specified but GL account is missing in the charge code configuration.

Please try the following steps to identify and/or resolve the issue:
1. Run File > Validate All menu and check if any validation error is shown.
2. If you are reversing a transaction, check if any of the original transaction line is missing a GL Account value.
3. Check the GL Account setup of the charge code used in Maintain > Account > Charge Codes."
				);

			AssertOnSavingCheck(lineToBeSaved, testCase);

			var line1 = GetLine(Factory, true, lineType, true);
			line1.AL_AG = ZGuid.Empty;

			AssertOnSavingCheck(line1, testCase);
			ExceptionReporterTestListener.Instance.Clear();

			var line2 = GetLine(Factory, true, lineType, true);
			line2.AL_AC = Env.Registry.FreightChargeCode;
			line2.AL_AG = ZGuid.Empty;

			AssertOnSavingCheck(line2, testCase);
			ExceptionReporterTestListener.Instance.Clear();

			line2.AL_AG = line2.ChargeCode.AC_AG_CostAccount;

			noExceptionTestCase = new TestCaseDefinition_ForSeparateTestsMethods("Expected no exception as line now has GL account");
			AssertOnSavingCheck(line2, noExceptionTestCase);

			var line3 = GetLine(Factory, true, lineType, true);
			line3.AL_AC = cmtChargeCode.PK;
			line3.AL_AG = ZGuid.Empty;

			noExceptionTestCase = new TestCaseDefinition_ForSeparateTestsMethods("Expected no exception for lines with a comment charge code");
			AssertOnSavingCheck(line3, noExceptionTestCase);

			var line4 = GetLine(Factory, true, lineType, true);
			line4.AL_AC = Env.Registry.FreightChargeCode;
			line4.AL_AG = ZGuid.Empty;

			var anotherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, line4.AL_GC)).PK;

			if (isHeaderRequired)
			{
				line4.TransactionHeader.AH_GB = anotherBranch;
			}
			line4.AL_GB = anotherBranch;

			noExceptionTestCase = new TestCaseDefinition_ForSeparateTestsMethods("Expected no exception when lines belong to another company's branch");
			AssertOnSavingCheck(line4, noExceptionTestCase);

			var line5 = GetLine(Factory, true, lineType, true);
			line5.AL_AC = ZGuid.Empty;
			line5.AL_AG = ZGuid.Empty;
			AssertOnSavingCheck(line5, testCase);
		}

		#endregion

		#region TestLineWithoutChargeWhenChargeIsNotForSavingCase1

		public void TestLineWithoutChargeWhenChargeIsNotForSavingCase1_Rev()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase1(TransactionLineTypes.Revenue);
		}

		public void TestLineWithoutChargeWhenChargeIsNotForSavingCase1_Cost()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase1(TransactionLineTypes.Cost);
		}

		void TestLineWithoutChargeWhenChargeIsNotForSavingCase1(string lineType)
		{
			var messagePrefix = string.Format("Line type: {0}. ", lineType);
			var line = GetLine(Factory, true, lineType);
			var charge = GetLineLinkedCharge(line, Factory.New<ChargeNotForSaveLikeApportionment>());
			AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionLines>>(messagePrefix, () => Factory.Save());
			AssertEquals(messagePrefix + "Exception count", 1, ExceptionReporterTestListener.Instance.Count);
			var exception = ExceptionReporterTestListener.Instance[0].InnerException;

			AssertContains("User Message", "Cost, Revenue or Unapproved Cost transaction line that does not have a related job charge that can be saved", exception.Message);
			AssertContainsInOrder(messagePrefix + "Exception message", exception.Message, "Line: PK =", "Charge is not in database. Charge: PK =");
			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region TestLineWithoutChargeWhenChargeIsNotForSavingCase2

		public virtual void TestLineWithoutChargeWhenChargeIsNotForSavingCase2_Rev()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase2(TransactionLineTypes.Revenue);
		}

		public virtual void TestLineWithoutChargeWhenChargeIsNotForSavingCase2_Cost()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase2(TransactionLineTypes.Cost);
		}

		void TestLineWithoutChargeWhenChargeIsNotForSavingCase2(string lineType)
		{
			var messagePrefix = string.Format("Line type: {0}. ", lineType);
			var savedCharge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			ReleaseFactory();

			var chargeNotToSaveLoaded = Factory.Load<ChargeNotForSaveLikeApportionment>(savedCharge.PK);
			var line = GetLine(Factory, true, lineType);
			var charge = GetLineLinkedCharge(line, chargeNotToSaveLoaded);
			AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionLines>>(messagePrefix, () => Factory.Save());
			AssertEquals(messagePrefix + "Exception count", 1, ExceptionReporterTestListener.Instance.Count);
			var exception = ExceptionReporterTestListener.Instance[0].InnerException;

			AssertContains("User Message", @"Cost, Revenue or Unapproved Cost transaction line that does not have a related job charge that can be saved", exception.Message);
			AssertContainsInOrder(messagePrefix + "Exception message", exception.Message, "Line: PK =", "Charge is in database. Charge: PK =");
			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region TestLineWithoutChargeWhenChargeIsNotForSavingCase3

		public void TestLineWithoutChargeWhenChargeIsNotForSavingCase3_Rev()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase3(TransactionLineTypes.Revenue);
		}

		public void TestLineWithoutChargeWhenChargeIsNotForSavingCase3_Cost()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase3(TransactionLineTypes.Cost);
		}

		void TestLineWithoutChargeWhenChargeIsNotForSavingCase3(string lineType)
		{
			var line = GetLine(Factory, true, lineType);
			var charge = GetLineLinkedCharge(line);
			Factory.Save();

			ReleaseFactory();
			var chargeNotToSaveLoaded = Factory.Load<ChargeNotForSaveLikeApportionment>(charge.PK);
			var lineLoaded = Factory.Load<AccTransactionLines>(line.PK);
			lineLoaded.AL_Desc = "Changed description";
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestLineWithoutChargeWhenChargeIsNotForSavingCase4

		public void TestLineWithoutChargeWhenChargeIsNotForSavingCase4_Rev()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase4(TransactionLineTypes.Revenue);
		}

		public void TestLineWithoutChargeWhenChargeIsNotForSavingCase4_Cost()
		{
			TestLineWithoutChargeWhenChargeIsNotForSavingCase4(TransactionLineTypes.Cost);
		}

		void TestLineWithoutChargeWhenChargeIsNotForSavingCase4(string lineType)
		{
			var factory = new BusinessObjectFactory();
			var line = GetLine(factory, true, lineType, false, true, 0m, "INV1");

			Assert("Precondition", !line.IsInDatabase);
			var messagePrefix = string.Format("Line type: {0}. ", lineType);
			AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionLines>>(messagePrefix + "Even if object is not going to be saved it's still error to change posted line link", () => factory.Save());
			AssertEquals(messagePrefix + "Exception count", 1, ExceptionReporterTestListener.Instance.Count);
			var exception = ExceptionReporterTestListener.Instance[0].InnerException;

			AssertContains("User Message", string.Format("{0} transaction line that does not have a related job charge", lineType), exception.Message);
			AssertContainsInOrder(messagePrefix + "Exception message", exception.Message, "Line: PK =");
			ExceptionReporterTestListener.Instance.Clear();

			var line2 = GetLine(Factory, true, lineType);
			var charge = GetLineLinkedCharge(line2);
			line2.Factory.Save();

			ReleaseFactory();

			var chargeLoaded = Factory.Load<JobCharge>(charge.PK);
			var lineLoaded = Factory.Load<AccTransactionLines>(line.PK);
			lineLoaded.AL_Desc = "Changed description";
			if (lineType == TransactionLineTypes.Cost)
			{
				chargeLoaded.JR_AL_APLine = ZGuid.Empty;
			}
			else
			{
				chargeLoaded.JR_AL_ARLine = ZGuid.Empty;
			}

			Assert("Precondition", line2.IsInDatabase);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestCFXLineWithoutJobCharge

		public void TestCFXLineWithoutJobChargeCost()
		{
			AssertCFXLineWithoutJobCharge(TransactionLineTypes.Cost);
		}

		public void TestCFXLineWithoutJobChargeRevenue()
		{
			AssertCFXLineWithoutJobCharge(TransactionLineTypes.Revenue);
		}

		public void TestCFXLineWithoutJobChargeUnapprovedCost()
		{
			AssertCFXLineWithoutJobCharge(TransactionLineTypes.UnapprovedCost);
		}

		void AssertCFXLineWithoutJobCharge(string lineType)
		{
			AccTransactionLines line = GetLine(Factory, true, lineType);
			line.TransactionHeader.AH_Ledger = LedgerTypes.JobCosting;
			line.TransactionHeader.AH_TransactionType = TransactionTypes.Journal;
			line.AL_LineAmount = 100M;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CFX Line without Job Charge", true,
				GetTransactionLineWithoutChargeCriticalValidationType(lineType),
				string.Format("{0} transaction line that does not have a related job charge", lineType),
				string.Format("Line: PK =", "Charge Code = FRT, GL Account = 1010.20.10, Type = {0}, OS Amount = 0, Local Amount = 100, GST = 0,", lineType));
			AssertOnSavingCheck(line, testCase);
		}

		CriticalValidationErrorType GetTransactionLineWithoutChargeCriticalValidationType(string lineType)
		{
			switch (lineType)
			{
				case TransactionLineTypes.Cost:
					return CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5;
				case TransactionLineTypes.Revenue:
					return CriticalValidationErrorType.RevenueTransactionLineWithoutJobCharge_6;
				case TransactionLineTypes.UnapprovedCost:
					return CriticalValidationErrorType.UnapprovedCostTransactionLineWithoutJobCharge_4;
				default:
					return CriticalValidationErrorType.JobTransactionLineWithoutJobCharge_3;
			}
		}

		#endregion

		#region TestCFXLineWithoutJobChargeToSave

		public void TestCFXLineWithoutJobChargeToSave()
		{
			AccTransactionLines line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			line.TransactionHeader.AH_Ledger = LedgerTypes.JobCosting;
			line.TransactionHeader.AH_TransactionType = TransactionTypes.Journal;
			line.AL_LineAmount = 100M;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			JobCharge charge = GetLineLinkedCharge(line);
			ZDataUtils.SetShouldRowBeSaved(((IBusinessObjectInternals)charge).Row, false);
			AssertEquals("Charge should not be saved", false, ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)charge).Row));

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CFX Line without Job Charge to save", true,
				CriticalValidationErrorType.JobTransactionLineWithoutJobChargeToSave_3,
				"Cost, Revenue or Unapproved Cost transaction line that does not have a related job charge that can be saved.",
				"Line: PK =", "Charge Code = FRT, GL Account = 1010.20.10, Type = REV, OS Amount = 0, Local Amount = 100, GST = 0,",
				"Charge is not in database. Charge: PK =", "harge Code = FRT, Charge Code Type = MRG, Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0,");
			AssertOnSavingCheck(line, testCase);
		}

		#endregion

		#region Cost or Revenue transaction line local amount must equal related job charge local amount

		public void TestCFXLineWithAmountDifferentToChargeAmount()
		{
			AccTransactionLines line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			line.TransactionHeader.AH_Ledger = LedgerTypes.JobCosting;
			line.TransactionHeader.AH_TransactionType = TransactionTypes.Journal;
			line.AL_OSAmount = line.AL_LineAmount = 10M;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			JobCharge charge = GetLineLinkedCharge(line);
			charge.JR_LocalCostAmt = 100M;
			charge.JR_LocalSellAmt = 100M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CFX Line does not check its Amount against Charge Amounts.");
			AssertOnSavingCheck(line, testCase);
		}

		public void TestLineFieldNotEqualChargeFiled()
		{
			foreach (bool fail in new[] { true, false })
			{
				foreach (string transactionType in new[] { TransactionLineTypes.Revenue, TransactionLineTypes.Cost, TransactionLineTypes.UnapprovedCost })
				{
					AssertIncorrectLineAmount(transactionType, fail);
					AssertIncorrectLineTaxCode(transactionType, fail);
					AssertIncorrectLineTaxClass(transactionType, fail);
				}
			}

			AssertIncorrectLineAmount(TransactionLineTypes.Revenue, false, sellInvoiceCurrency: true);
		}

		void AssertIncorrectLineAmount(string transactionType, bool fail, bool sellInvoiceCurrency = false)
		{
			bool isSell = transactionType == TransactionLineTypes.Revenue;
			AccTransactionLines line = GetLine(Factory, true, transactionType);
			line.AL_RevRecognitionType = "IMM";

			JobCharge charge = GetLineLinkedCharge(line);
			TestCaseDefinition_ForSeparateTestsMethods testCase;

			if (isSell)
			{
				line.AL_OSAmount = line.AL_LineAmount = 100M;
				charge.JR_LocalSellAmt = fail || sellInvoiceCurrency ? -100M : 100M;
				if (sellInvoiceCurrency)
				{
					charge.JR_RX_NKSellInvoiceCurrency = "USD";
					Assert("Precondition: BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
					Assert("Precondition: BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);
				}

				testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					string.Format("Incorrect Amount for {0} line. Fail: {1}{2}", transactionType, fail, sellInvoiceCurrency ? " because of Sell Invoice Currency" : ""),
					fail,
					CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
					"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
					"\r\nCharge amount: -100, line amount: 100.",
					"\r\nLine: PK =",
					"\r\nCharge: PK =",
					"\r\nHeader: PK ="
				);
			}
			else
			{
				line.AL_OSAmount = line.AL_LineAmount = -100M;
				charge.JR_LocalCostAmt = fail ? -100M : 100M;

				SetConsolCostLinkedCharge(charge);

				testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					string.Format("Incorrect Amount for {0} line. Fail: {1}", transactionType, fail),
					fail,
					CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
					"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
					"\r\nCharge amount: -100, line amount: 100.",
					"\r\nLine: PK =",
					"\r\nCharge: PK =",
					"\r\nJob Consol Cost:\r\n\tPK =",
					"\r\nHeader: PK ="
				);
			}

			AssertOnSavingCheck(line, testCase);
		}

		void AssertIncorrectLineTaxCode(string transactionType, bool fail)
		{
			AssertIncorrectLineTaxCode(transactionType, fail, useEmptyLineTax: false, useEmptyChargeTax: false);
			AssertIncorrectLineTaxCode(transactionType, fail, useEmptyLineTax: true, useEmptyChargeTax: false);
			AssertIncorrectLineTaxCode(transactionType, fail, useEmptyLineTax: false, useEmptyChargeTax: true);
		}

		void AssertIncorrectLineTaxCode(string transactionType, bool fail, bool useEmptyLineTax, bool useEmptyChargeTax)
		{
			bool isSell = transactionType == TransactionLineTypes.Revenue;
			var taxRateForLine = Factory.New<AccTaxRate>();
			taxRateForLine.AT_Code = "TAX1";
			var taxRateForCharge = Factory.New<AccTaxRate>();
			taxRateForCharge.AT_Code = "TAX2";

			var taxRateForLinePK = useEmptyLineTax ? ZGuid.Empty : taxRateForLine.PK;
			var taxRateForChargePK = useEmptyChargeTax ? ZGuid.Empty : taxRateForCharge.PK;

			AccTransactionLines line = GetLine(Factory, true, transactionType);
			line.AL_RevRecognitionType = "IMM";
			line.AL_AT = taxRateForLinePK;

			JobCharge charge = GetLineLinkedCharge(line);
			if (isSell)
			{
				line.AL_OSAmount = line.AL_LineAmount = 100M;
				charge.JR_LocalSellAmt = 100M;
				charge.JR_AT_SellGSTRate = fail ? taxRateForChargePK : taxRateForLinePK;
			}
			else
			{
				line.AL_OSAmount = line.AL_LineAmount = -100M;
				charge.JR_LocalCostAmt = 100M;
				charge.JR_AT_CostGSTRate = fail ? taxRateForChargePK : taxRateForLinePK;

				SetConsolCostLinkedCharge(charge);
			}
			TestCaseDefinition_ForSeparateTestsMethods testCase;
			if (isSell)
			{
				testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					string.Format("Incorrect Tax Code for {0} line. Fail: {1}", transactionType, fail),
					fail,
					CriticalValidationErrorType.TransactionLineTaxCodeNotEqualJobChargeTaxCode_5,
					"Cost, Revenue or Unapproved Cost transaction tax code that does not equal related job charge tax code.",
					string.Format("\r\nCharge tax code: {0}, line tax code: {1}.", useEmptyChargeTax ? "<empty>" : "TAX2", useEmptyLineTax ? "<empty>" : "TAX1"),
					"\r\nLine: PK =",
					"\r\nCharge: PK =",
					"TransactionLineHasChangedAfterItIsPosted_TaxId: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1"
				);
			}
			else
			{
				testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					string.Format("Incorrect Tax Code for {0} line. Fail: {1}", transactionType, fail),
					fail,
					CriticalValidationErrorType.TransactionLineTaxCodeNotEqualJobChargeTaxCode_5,
					"Cost, Revenue or Unapproved Cost transaction tax code that does not equal related job charge tax code.",
					string.Format("\r\nCharge tax code: {0}, line tax code: {1}.", useEmptyChargeTax ? "<empty>" : "TAX2", useEmptyLineTax ? "<empty>" : "TAX1"),
					"\r\nLine: PK =",
					"\r\nCharge: PK =",
					"\r\nJob Consol Cost:\r\n\tPK =",
					"TransactionLineHasChangedAfterItIsPosted_TaxId: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1"
				);
			}
			AssertOnSavingCheck(line, testCase);
		}

		void AssertIncorrectLineTaxClass(string transactionType, bool fail)
		{
			AssertIncorrectLineTaxClass(transactionType, fail, useEmptyLineTax: false, useEmptyChargeTax: false);
			AssertIncorrectLineTaxClass(transactionType, fail, useEmptyLineTax: true, useEmptyChargeTax: false);
			AssertIncorrectLineTaxClass(transactionType, fail, useEmptyLineTax: false, useEmptyChargeTax: true);
		}

		void AssertIncorrectLineTaxClass(string transactionType, bool fail, bool useEmptyLineTax, bool useEmptyChargeTax)
		{
			bool isSell = transactionType == TransactionLineTypes.Revenue;
			var taxClassForLine = Factory.New<AccInvMsg>();
			taxClassForLine.A9_Code = "MSG1";

			var taxClassForCharge = Factory.New<AccInvMsg>();
			taxClassForCharge.A9_Code = "MSG2";

			var taxClassForLinePK = useEmptyLineTax ? ZGuid.Empty : taxClassForLine.PK;
			var taxClassForChargePK = useEmptyChargeTax ? ZGuid.Empty : taxClassForCharge.PK;

			AccTransactionLines line = GetLine(Factory, true, transactionType);
			line.AL_RevRecognitionType = "IMM";
			line.AL_A9_VATClass = taxClassForLinePK;

			JobCharge charge = GetLineLinkedCharge(line);
			if (isSell)
			{
				line.AL_OSAmount = line.AL_LineAmount = 100M;
				charge.JR_LocalSellAmt = 100M;
				charge.JR_A9_SellVATClass = fail ? taxClassForChargePK : taxClassForLinePK;
			}
			else
			{
				line.AL_OSAmount = line.AL_LineAmount = -100M;
				charge.JR_LocalCostAmt = 100M;
				charge.JR_A9_CostVATClass = fail ? taxClassForChargePK : taxClassForLinePK;

				SetConsolCostLinkedCharge(charge);
			}

			TestCaseDefinition_ForSeparateTestsMethods testCase;
			if (isSell)
			{
				testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					string.Format("Incorrect Tax Class for {0} line. Fail: {1}", transactionType, fail),
					fail,
					CriticalValidationErrorType.TransactionLineTaxClassNotEqualJobChargeTaxClass_4,
					"Cost, Revenue or Unapproved Cost transaction tax class that does not equal related job charge tax class.",
					string.Format("\r\nCharge tax class: {0}, line tax class: {1}.", useEmptyChargeTax ? "<empty>" : "MSG2", useEmptyLineTax ? "<empty>" : "MSG1"),
					"\r\nLine: PK =",
					"\r\nCharge: PK =",
					"TransactionLineHasChangedAfterItIsPosted_TaxMessage: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1"
				);
			}
			else
			{
				testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					string.Format("Incorrect Tax Class for {0} line. Fail: {1}", transactionType, fail),
					fail,
					CriticalValidationErrorType.TransactionLineTaxClassNotEqualJobChargeTaxClass_4,
					"Cost, Revenue or Unapproved Cost transaction tax class that does not equal related job charge tax class.",
					string.Format("\r\nCharge tax class: {0}, line tax class: {1}.", useEmptyChargeTax ? "<empty>" : "MSG2", useEmptyLineTax ? "<empty>" : "MSG1"),
					"\r\nLine: PK =",
					"\r\nCharge: PK =",
					"\r\nJob Consol Cost:\r\n\tPK =",
					"TransactionLineHasChangedAfterItIsPosted_TaxMessage: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1"
				);
			}
			AssertOnSavingCheck(line, testCase);
		}

		#endregion

		public void TestJobTransactionLineWithoutJobChargeDefaultType()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = "TRY";
			var validation = new AccTransactionLinesCriticalValidation(line);

			var result = validation.GetTransactionLineWithoutJobChargeCriticalValidationResult("");
			AssertEquals("If not CST, REV or UA then defaults to generic JobTransactionLineWithoutJobCharge error", result.ErrorType, CriticalValidationErrorType.JobTransactionLineWithoutJobCharge_3);
			AssertEquals("Correct error message", result.UserFriendlyErrorMessage, "TRY transaction line that does not have a related job charge.");
		}

		public void TestJobTransactionLineWithoutJobChargeWithDetailedInfo()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_JobNum = job1.PK.ToString().Substring(1, JobHeaderSchema.JH_JobNum.MaxLength).ToUpper();
			//var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			//job2.JH_JobNum = job2.PK.ToString().Substring(1, JobHeaderSchema.JH_JobNum.MaxLength).ToUpper();

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = job1.PK;
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = job1.PK;
			Factory.Save();

			var line = GetLine(Factory, false, TransactionLineTypes.Cost);
			line.AL_JH = job1.PK;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobTransactionLineWithoutJobChargeWithDetailedInfo", true,
				CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5,
				"CST transaction line that does not have a related job charge",
				"Query:WHERE JR_JH = ",
				"Header: PK = " + line.AL_AH.ToString(),
				"Line: PK = " + line.PK.ToString(),
				"Job: Job Number = " + job1.JH_JobNum,
				"Charge: PK = " + charge1.PK.ToString(),
				"Charge: PK = " + charge2.PK.ToString());

			AssertOnSavingCheck(line, testCase);
		}

		public void TestCheckAccTransactionLineBranchBelongToAccTransactionHeaderCompany()
		{
			AccTransactionLines line = GetLine(Factory, false, TransactionLineTypes.Revenue);
			GlbBranch differentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, line.AL_GC));
			line.AL_GB = differentBranch.PK;
			var expectedMessage = string.Format("Line: PK = {0}, Charge Code = FRT, GL Account = 1010.20.10, Type = REV, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {1}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.", line.PK, line.TransactionHeader.PK);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction line branch does not belong to transaction header company", true, CriticalValidationErrorType.AccTransactionLineBranchDoesNotBelongToAccTransactionHeaderCompany_2, "Transaction line branch does not belong to transaction header company", expectedMessage);
			AssertOnSavingCheck(line, testCase);
		}

		public void TestExtraDeveloperMsg_TransactionLineTaxClassNotEqualJobChargeTaxClass()
		{
			var taxClassForLine = Factory.New<AccInvMsg>();
			taxClassForLine.A9_Code = "MSG1";

			var taxClassForCharge = Factory.New<AccInvMsg>();
			taxClassForCharge.A9_Code = "MSG2";

			var taxClassForLinePK = taxClassForLine.PK;
			var taxClassForChargePK = taxClassForCharge.PK;

			AccTransactionLines line = GetLine(Factory, true, TransactionLineTypes.Cost);
			line.AL_RevRecognitionType = "IMM";
			line.AL_A9_VATClass = taxClassForLinePK;

			JobCharge charge = GetLineLinkedCharge(line);

			line.AL_LineAmount = -100M;
			charge.JR_LocalCostAmt = 100M;
			charge.JR_A9_CostVATClass = taxClassForChargePK;

			SetConsolCostLinkedCharge(charge);

			TestCaseDefinition_ForSeparateTestsMethods testCase;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				string.Format("Incorrect Tax Class for {0} line. Fail: {1}", TransactionLineTypes.Cost, true),
				true,
				CriticalValidationErrorType.TransactionLineTaxClassNotEqualJobChargeTaxClass_4,
				"Cost, Revenue or Unapproved Cost transaction tax class that does not equal related job charge tax class.",
				string.Format("\r\nCharge tax class: {0}, line tax class: {1}.", false ? "<empty>" : "MSG2", false ? "<empty>" : "MSG1"),
				"\r\nLine: PK =",
				"\r\nCharge: PK =",
				"\r\nJob Consol Cost:\r\n\tPK =",
				"TransactionLineHasChangedAfterItIsPosted_TaxMessage: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1"
			);

			AssertOnSavingCheck(line, testCase);
		}

		public void TestExtraDeveloperMsg_TransactionLineTaxIdHasChangedAfterItIsPosted_CST()
			=> AssertExtraDeveloperMsg_TransactionLineTaxIdHasChangedAfterItIsPosted(TransactionLineTypes.Cost, expectedHasStackTrace: true);

		public void TestExtraDeveloperMsg_TransactionLineTaxIdHasChangedAfterItIsPosted_REV()
			=> AssertExtraDeveloperMsg_TransactionLineTaxIdHasChangedAfterItIsPosted(TransactionLineTypes.Revenue, expectedHasStackTrace: false);

		void AssertExtraDeveloperMsg_TransactionLineTaxIdHasChangedAfterItIsPosted(string transactionType, bool expectedHasStackTrace)
		{
			var lineInDb = PrepareLineInDb(transactionType);
			lineInDb.AL_AT = ZGuid.Empty;

			var expectedInfosForNoRecord = new[] {
				"TransactionLineHasChangedAfterItIsPosted_TaxId: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1",
			};
			var expectedInfosForHasRecord = new [] {
				"TransactionLineHasChangedAfterItIsPosted_TaxId:\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)",
				"at Enterprise.MasterFiles.Business.AccTransactionLines.<>c.<set_AL_AT>",
				"AccTransactionLinesCriticalValidationTest.cs"
			};
			var expectedTechDetailsInThisOrderIntoErrorMessage = expectedHasStackTrace
				? expectedInfosForHasRecord
				: expectedInfosForNoRecord;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				$"TransactionLineHasChangedAfterItIsPosted_TaxId stack trace for {transactionType}",
				true,
				CriticalValidationErrorType.TransactionLineTaxCodeNotEqualJobChargeTaxCode_5,
				"Cost, Revenue or Unapproved Cost transaction tax code that does not equal related job charge tax code.",
				expectedTechDetailsInThisOrderIntoErrorMessage
			);

			AssertOnSavingCheck(lineInDb, testCase);
		}

		public void TestExtraDeveloperMsg_TransactionLineTaxMessageHasChangedAfterItIsPosted_CST()
			=> AssertExtraDeveloperMsg_TransactionLineTaxMessageHasChangedAfterItIsPosted(TransactionLineTypes.Cost, expectedHasStackTrace: true);

		public void TestExtraDeveloperMsg_TransactionLineTaxMessageHasChangedAfterItIsPosted_REV()
			=> AssertExtraDeveloperMsg_TransactionLineTaxMessageHasChangedAfterItIsPosted(TransactionLineTypes.Revenue, expectedHasStackTrace: false);

		void AssertExtraDeveloperMsg_TransactionLineTaxMessageHasChangedAfterItIsPosted(string transactionType, bool expectedHasStackTrace)
		{
			var lineInDb = PrepareLineInDb(transactionType);
			lineInDb.AL_A9_VATClass = ZGuid.Empty;

			var expectedInfosForNoRecord = new[] {
				"TransactionLineHasChangedAfterItIsPosted_TaxMessage: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1"
			};
			var expectedInfosForHasRecord = new[] {
				"TransactionLineHasChangedAfterItIsPosted_TaxMessage:\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)",
				"at Enterprise.MasterFiles.Business.AccTransactionLines.<>c.<set_AL_A9_VATClass>",
				"AccTransactionLinesCriticalValidationTest.cs"
			};
			var expectedTechDetailsInThisOrderIntoErrorMessage = expectedHasStackTrace
				? expectedInfosForHasRecord
				: expectedInfosForNoRecord;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				$"TransactionLineHasChangedAfterItIsPosted_TaxMessage stack trace for {transactionType}",
				true,
				CriticalValidationErrorType.TransactionLineTaxClassNotEqualJobChargeTaxClass_4,
				"Cost, Revenue or Unapproved Cost transaction tax class that does not equal related job charge tax class.",
				expectedTechDetailsInThisOrderIntoErrorMessage
			);

			AssertOnSavingCheck(lineInDb, testCase);
		}

		AccTransactionLines PrepareLineInDb(string transactionType)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "TaxMsg01";
			Factory.Save();

			var line = GetLine(Factory, true, transactionType);
			line.AL_RevRecognitionType = "IMM";
			line.AL_A9_VATClass = taxMsg.PK;
			line.AL_AT = taxRate.PK;

			var charge = GetLineLinkedCharge(line);
			charge.JR_A9_CostVATClass = taxMsg.PK;

			var isSell = transactionType == TransactionLineTypes.Revenue;
			if (isSell)
			{
				line.AL_OSAmount = line.AL_LineAmount = 100M;
				charge.JR_LocalSellAmt = 100M;
			}
			else
			{
				line.AL_OSAmount = line.AL_LineAmount = -100M;
				charge.JR_LocalCostAmt = 100M;
			}
			line.TransactionHeader.AH_InvoiceAmount = line.AL_LineAmount;
			line.TransactionHeader.AH_OutstandingAmount = line.AL_LineAmount;
			Factory.Save();

			return line;
		}

		public void TestExtraDeveloperMsg_TransactionLineSupplyTypeNotEqualJobChargeSupplyType()
		{
			AccTransactionLines line = GetLine(Factory, true, TransactionLineTypes.Cost);
			line.AL_RevRecognitionType = "IMM";
			line.AL_SupplyType = "LOX";

			JobCharge charge = GetLineLinkedCharge(line);

			line.AL_LineAmount = -100M;
			charge.JR_LocalCostAmt = 100M;
			charge.JR_CostSupplyType = "LOC";

			SetConsolCostLinkedCharge(charge);

			TestCaseDefinition_ForSeparateTestsMethods testCase;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				string.Format("Incorrect Supply Type for {0} line. Fail: {1}", TransactionLineTypes.Cost, true),
				true,
				CriticalValidationErrorType.TransactionLineSupplyTypeNotEqualJobChargeSupplyType_2,
				"Cost, Revenue or Unapproved Cost transaction line supply type that does not equal related job charge supply type.",
				string.Format("\r\nCharge supply type: LOC, line supply type: LOX.")
			);

			AssertOnSavingCheck(line, testCase);
		}

		public void TestExtraDeveloperMsg_TransactionLineTaxBranchNotEqualToTransactionHeaderTaxBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			AccTransactionLines line = GetLine(Factory, true, TransactionLineTypes.Cost);
			line.AL_GB_TaxBranch = GlbBranch.CurrentBranch.PK;

			JobCharge charge = GetLineLinkedCharge(line);
			charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			line.TransactionHeader.AH_GB_TaxBranch = branch.PK;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				string.Format("Tax branch in transaction header and transaction line should keep the same."),
				true,
				CriticalValidationErrorType.TransactionLineTaxBranchNotEqualToTransactionHeaderTaxBranch,
				"Transaction Line tax branch does not equal to transaction header tax branch.",
				$"\r\nTransaction header tax branch: {line.TransactionHeader.AH_GB_TaxBranch}, transaction line tax branch: {line.AL_GB_TaxBranch}."
				);

			AssertNotEquals("Precondition", line.AL_GB_TaxBranch, line.TransactionHeader.AH_GB_TaxBranch);
			AssertOnSavingCheck(line, testCase);
		}

		public void TestFCBAdjustmentJournalLineIsValidatedWhenLineTransactionCurrencyIsLocal()
		{
			var line = GetLine(Factory, false, TransactionTypes.GLStandardJournal, false, true, 0m, "FCB001");
			line.TransactionHeader.AH_ReceiptType = ReceiptTypes.ForeignCurrencyBalance;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_OSAmount = 0m;
			line.AL_LineAmount = 100m;

			var expectedMessage = string.Format("Line: PK = {0}, Charge Code = FRT, GL Account = {1}, Type = GJL, OS Amount = 0, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {2}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.", line.PK, line.GLHeader.AG_AccountNum, line.TransactionHeader.PK);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1.", true, CriticalValidationErrorType.TransactionLineLocalAmountNotEqualForeignWithExRate_5, "Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1.", expectedMessage);
			AssertEquals(line.Company.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
			AssertOnSavingCheck(line, testCase);
		}

		public void TestFCBAdjustmentJournalLineIsNotValidatedWhenLineTransactionCurrencyIsForeign()
		{
			var line = GetLine(Factory, false, TransactionTypes.GLStandardJournal, false, true, 0m, "FCB001");
			line.TransactionHeader.AH_ReceiptType = ReceiptTypes.ForeignCurrencyBalance;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			line.AL_ExchangeRate = 1m;
			line.AL_OSAmount = 0m;
			line.AL_LineAmount = 100m;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1.", false, CriticalValidationErrorType.TransactionLineLocalAmountNotEqualForeignWithExRate_5, "Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1.", "");
			AssertNotEquals(line.Company.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
			AssertOnSavingCheck(line, testCase);
		}

		public void TestTransactionLineLocalAmountNotEqualForeignExRate1WithAPInvoice()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = transaction.PK;
			line.AL_AC = Env.Registry.FreightChargeCode;
			line.AL_ExchangeRate = 1m;
			line.AL_OSAmount = 98m;
			line.AL_LineAmount = 100m;
			line.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.UnitedStates;

			var testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("Test Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1 with different currency pair.");
			AssertNotEquals("Currency should be unequal", line.Company.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
			AssertOnSavingCheck(line, testCase1);

			line.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.Australia;

			var expectedMessage = string.Format("Line: PK = {0}, Charge Code = FRT, GL Account = {1}, Type = WIP, OS Amount = 98, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {2}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.", line.PK, line.GLHeader.AG_AccountNum, line.TransactionHeader.PK);
			var testCase2 = new TestCaseDefinition_ForSeparateTestsMethods("Test Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1 with same currency pair.", true, CriticalValidationErrorType.TransactionLineLocalAmountNotEqualForeignWithExRate_5, "Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1.", expectedMessage);
			AssertEquals("Currency should be equal", line.Company.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
			AssertOnSavingCheck(line, testCase2);
		}

		public void TestRevenueTransactionLineWithoutJobChargeHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			charge.Delete();

			string expectedErrMsg = "REV transaction line that does not have a related job charge";
			string expectedStackTraceMsg = "A stack trace should appear here";
			var collectorService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			collectorService.AddInfoWhenAllowed(line.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueTransactionLineWithoutJobCharge, () => expectedStackTraceMsg, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

			try
			{
				Factory.Save();
				Fail("Critical Validation should prevent saving");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Critical Validation Error", expectedErrMsg, ex.Message);
				AssertContains("Critical Validation Error", expectedStackTraceMsg, ex.DeveloperErrorMessage);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			#region Transaction line that does not have a related job charge or transaction header

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan REV Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Revenue), true, CriticalValidationErrorType.RevenueTransactionLineWithoutJobCharge_6, "REV transaction line that does not have a related job charge", "Line: PK ="));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan CST Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Cost), true, CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5, "CST transaction line that does not have a related job charge", "Line: PK ="));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan UCST Line", factory1 => GetLine(factory1, true, TransactionLineTypes.UnapprovedCost), true, CriticalValidationErrorType.UnapprovedCostTransactionLineWithoutJobCharge_4, "UCT transaction line that does not have a related job charge", "Line: PK ="));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("REV Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Revenue, true)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CST Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Cost, true)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("UCT Line", factory1 => GetLine(factory1, true, TransactionLineTypes.UnapprovedCost, true)));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan REV Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Revenue, true, false), true, CriticalValidationErrorType.TransactionLineWithoutTransactionHeader_2, "Cost, Revenue or Unapproved Cost transaction line that does not have a related transaction header", "Line: PK ="));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan CST Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Cost, true, false), true, CriticalValidationErrorType.TransactionLineWithoutTransactionHeader_2, "Cost, Revenue or Unapproved Cost transaction line that does not have a related transaction header", "Line: PK ="));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan UCT Line", factory1 => GetLine(factory1, true, TransactionLineTypes.UnapprovedCost, true, false), true, CriticalValidationErrorType.TransactionLineWithoutTransactionHeader_2, "Cost, Revenue or Unapproved Cost transaction line that does not have a related transaction header", "Line: PK ="));

			result.Add(GetOrphanCFXCases(TransactionLineTypes.Cost));
			result.Add(GetOrphanCFXCases(TransactionLineTypes.Revenue));
			result.Add(GetOrphanCFXCases(TransactionLineTypes.UnapprovedCost));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CFX Line", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Revenue);
				line.TransactionHeader.AH_Ledger = LedgerTypes.JobCosting;
				line.TransactionHeader.AH_TransactionType = TransactionTypes.Journal;

				JobCharge charge = GetLineLinkedCharge(line);

				return line;
			}));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("ACR Line", factory1 => GetLine(factory1, true, TransactionLineTypes.Accrual)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("WIP Line", factory1 => GetLine(factory1, true, TransactionLineTypes.WIP)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Orphan REV Line with no job", factory1 => GetLine(factory1, false, TransactionLineTypes.Revenue)));

			result.Add(GetLineWithoutChargeChargeDeletedCases(TransactionLineTypes.Cost));
			result.Add(GetLineWithoutChargeChargeDeletedCases(TransactionLineTypes.Revenue));
			result.Add(GetLineWithoutChargeChargeDeletedCases(TransactionLineTypes.UnapprovedCost));

			result.Add(GetLineWithoutChargePostedChargeDeletedCases(TransactionLineTypes.Cost));
			result.Add(GetLineWithoutChargePostedChargeDeletedCases(TransactionLineTypes.Revenue));
			result.Add(GetLineWithoutChargePostedChargeDeletedCases(TransactionLineTypes.UnapprovedCost));

			#endregion

			#region Transaction line for reversed transaction linked to job charge

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Reversed REV Line",
				(BusinessObjectFactory factory1) =>
				{
					AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Revenue);
					line.TransactionHeader.AH_IsCancelled = true;
					return line;
				}));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Reversed CST Line",
				(BusinessObjectFactory factory1) =>
				{
					AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Cost);
					line.TransactionHeader.AH_IsCancelled = true;
					return line;
				}));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Reversed UCST Line",
				(BusinessObjectFactory factory1) =>
				{
					AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.UnapprovedCost);
					line.TransactionHeader.AH_IsCancelled = true;
					return line;
				}));

			#endregion

			#region Cost or Revenue transaction line that has more than one related job charge

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("REV Line referenced by two charge records",
				(BusinessObjectFactory factory1) =>
				{
					AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Revenue, true);
					JobCharge secondLinkedCharge = GetLineLinkedCharge(line);
					return line;
				}, true, CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_LineSide_5, "Cost, Revenue or Unapproved Cost transaction line that has more than one related job charge", "Line: PK =", "Charge: PK =", "Charge: PK ="));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CST Line referenced by two charge records",
				(BusinessObjectFactory factory1) =>
				{
					AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Cost, true);
					JobCharge secondLinkedCharge = GetLineLinkedCharge(line);
					return line;
				}, true, CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_LineSide_5, "Cost, Revenue or Unapproved Cost transaction line that has more than one related job charge", "Line: PK =", "Charge: PK =", "Charge: PK ="));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("UCST Line referenced by two charge records",
				(BusinessObjectFactory factory1) =>
				{
					AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.UnapprovedCost, true);
					JobCharge secondLinkedCharge = GetLineLinkedCharge(line);
					return line;
				}, true, CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_LineSide_5, "Cost, Revenue or Unapproved Cost transaction line that has more than one related job charge", "Line: PK =", "Charge: PK =", "Charge: PK ="));

			#endregion

			#region AL_LineAmount + AL_GSTVAT != AL_OSAmount when ALExchangeRate = 1

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("New Line with AL_LineAmount + AL_GSTVAT != AL_OSAmount when ALExchangeRate = 1", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Cost);
				JobCharge charge = GetLineLinkedCharge(line);
				line.AL_ExchangeRate = 1m;
				line.AL_OSAmount = -110m;
				line.AL_GSTVAT = -5m;
				line.AL_LineAmount = -100m;
				line.AL_RevRecognitionType = "IMM";
				charge.JR_LocalCostAmt = -line.AL_LineAmount;
				Assert("Pre-condition: AL_LineAmount + AL_GSTVAT != AL_OSAmount", line.AL_LineAmount + line.AL_GSTVAT != line.AL_OSAmount);

				return line;
			}, true, CriticalValidationErrorType.TransactionLineLocalAmountNotEqualForeignWithExRate_5, "Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1", "Line: PK ="));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("New Line with AL_LineAmount + AL_GSTVAT == AL_OSAmount when ALExchangeRate = 1", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				line.AL_ExchangeRate = 1m;
				line.AL_OSAmount = -110m;
				line.AL_GSTVAT = -10m;
				line.AL_LineAmount = -100m;
				line.AL_RevRecognitionType = "IMM";
				AssertEquals("Pre-condition: AL_LineAmount + AL_GSTVAT == AL_OSAmount", line.AL_LineAmount + line.AL_GSTVAT, line.AL_OSAmount);

				return line;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Saved Line modified to AL_LineAmount + AL_GSTVAT != AL_OSAmount when ALExchangeRate = 1", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				line.AL_ExchangeRate = 1m;
				line.AL_OSAmount = -110m;
				line.AL_GSTVAT = -10m;
				line.AL_LineAmount = -100m;
				line.AL_RevRecognitionType = "IMM";
				AssertEquals("Pre-condition: AL_LineAmount + AL_GSTVAT == AL_OSAmount", line.AL_LineAmount + line.AL_GSTVAT, line.AL_OSAmount);

				factory1.Save();
				line.AL_GSTVAT = -5m;
				Assert("Pre-condition: AL_LineAmount + AL_GSTVAT != AL_OSAmount", line.AL_LineAmount + line.AL_GSTVAT != line.AL_OSAmount);

				return line;
			}, true, CriticalValidationErrorType.TransactionLineLocalAmountNotEqualForeignWithExRate_5, "Local Line Amount and Tax that are not equal to the Foreign Currency Line Amount and Tax when the exchange rate is 1", "Line: PK ="));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Saved Line with AL_LineAmount + AL_GSTVAT != AL_OSAmount when ALExchangeRate = 1", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				line.AL_ExchangeRate = 1m;
				line.AL_OSAmount = -110m;
				line.AL_GSTVAT = -10m;
				line.AL_LineAmount = -100m;
				line.AL_RevRecognitionType = "IMM";
				AssertEquals("Pre-condition: AL_LineAmount + AL_GSTVAT == AL_OSAmount", line.AL_LineAmount + line.AL_GSTVAT, line.AL_OSAmount);

				factory1.Save();

				string sqlText = "UPDATE dbo.AccTransactionLines SET AL_GSTVAT = @LocalGST, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = @pk";
				using (DbCommand command = Db.Connection.Command(sqlText))
				{
					command.AddParameterBasedOnDbColumn("@LocalGST", -5m, AccTransactionLinesSchema.AL_GSTVAT);
					command.AddParameterBasedOnDbColumn("@pk", line.PK.ToGuid(), AccTransactionLinesSchema.PK);
					command.ExecuteNonQuery();
				}
				line.Reload();
				Assert("Pre-condition: AL_LineAmount + AL_GSTVAT != AL_OSAmount", line.AL_LineAmount + line.AL_GSTVAT != line.AL_OSAmount);

				return line;
			}));

			#endregion

			#region Cost or Revenue transaction line organizationmust equal related job charge local amount

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Incorrect Organization WIP Line", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.WIP);
				JobCharge charge = GetLineLinkedCharge(line);
				line.AL_OH = ZGuid.NewZGuid();
				return line;
			}, true, CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10, "Transaction line organization does not equal related job charge organization."));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Correct Organization WIP Line", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.WIP);
				JobCharge charge = GetLineLinkedCharge(line);
				return line;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Incorrect Organization Accrual Line", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				JobCharge charge = GetLineLinkedCharge(line);
				line.AL_OH = ZGuid.NewZGuid();
				return line;
			}, true, CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10, "Transaction line organization does not equal related job charge organization."));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Correct Organization Accrual Line", (BusinessObjectFactory factory1) =>
			{
				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				JobCharge charge = GetLineLinkedCharge(line);
				return line;
			}));

			#endregion

			#region WIP/Accrual must have creditor

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Accrual must have creditor code when registry is set to true.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetAccrualMustHaveCreditorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				line.AL_LineAmount = 10M;
				line.AL_RevRecognitionType = "IMM";

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_LocalCostAmt = 100M;

				return line;
			}, true, CriticalValidationErrorType.AccrualMustHaveCreditor_1, @"Accrual without a creditor. You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Accrual must have creditor code when registry is set to true - no job charge.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetAccrualMustHaveCreditorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				BusinessObject shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

				var job = factory1.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f"));
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_JobNum = "234232";
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = "JS";

				AccTransactionLines line = (AccTransactionLines)factory1.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
				line.AL_GB = GlbBranch.CurrentBranch.PK;
				line.AL_GE = GlbDepartment.CurrentDepartment.PK;
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
				line.AL_JH = job.PK;

				return line;
			}, true, CriticalValidationErrorType.AccrualMustHaveCreditor_1, @"Accrual without a creditor. You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code",

@"Job: Job Number = 234232, PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Parent Table Code = JS, Parent Table Name = , ControllerID = , Shipment Incoterm = , Local Client Code = , Local Client Address = , Overseas Agent Code = , Overseas Agent Address = , Is In DB = No, Has Changes = Yes.

Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = , Type = ACR, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.

Job Charge: Not Set"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Accrual must have creditor code when registry is set to true - extented.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetAccrualMustHaveCreditorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				BusinessObject shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

				var job = factory1.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f"));
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_JobNum = "234232";
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = "JS";

				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				var jobCharge = (JobCharge)factory1.New(typeof(JobCharge), new Guid("d85faec8-f85d-4a99-ae49-5b6923d1ec68"));
				jobCharge.JR_JH = job.PK;
				jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
				jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				jobCharge.JR_AC = Env.Registry.FreightChargeCode;
				jobCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				jobCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				AccTransactionHeader invoice = factory1.NewWithPrimaryKey<AccTransactionHeader>(new Guid("e66a7fed-b46b-4322-bdf3-616d341cf2ed"));
				invoice.FillWithValidTestData();
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				invoice.AH_InvoiceAmount = -100m;
				invoice.AH_OutstandingAmount = 0M;

				AccTransactionLines line = (AccTransactionLines)factory1.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
				line.AL_GB = GlbBranch.CurrentBranch.PK;
				line.AL_GE = GlbDepartment.CurrentDepartment.PK;
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
				line.AL_JH = job.PK;

				jobCharge.JR_AL_APLine = line.PK;

				return line;
			}, true, CriticalValidationErrorType.AccrualMustHaveCreditor_1, @"Accrual without a creditor. You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code",

@"Job: Job Number = 234232, PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Parent Table Code = JS, Parent Table Name = , ControllerID = , Shipment Incoterm = , Local Client Code = , Local Client Address = , Overseas Agent Code = , Overseas Agent Address = , Is In DB = No, Has Changes = Yes.

Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = , Type = ACR, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.

Default Creditor: Not Set, Job Parent: Not Set", "Charge: PK = d85faec8-f85d-4a99-ae49-5b6923d1ec68"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Accrual for a different Company is not required to have creditor code when registry is set to true for the current Company.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetAccrualMustHaveCreditorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				line.AL_OSAmount = line.AL_LineAmount = 10M;
				line.AL_RevRecognitionType = "IMM";
				line.AL_GB = factory1.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_LocalCostAmt = 100M;

				return line;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("WIP must have debtor code when registry is set to true.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetWIPMustHaveDebtorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.WIP);
				line.AL_OSAmount = line.AL_LineAmount = 10M;
				line.AL_RevRecognitionType = "IMM";

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_LocalSellAmt = 100M;

				return line;
			}, true, CriticalValidationErrorType.WIPMustHaveDebtor_1, @"WIP without a debtor. You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("WIP must have debtor code when registry is set to true - no job charge.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetWIPMustHaveDebtorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				BusinessObject shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

				var job = factory1.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f"));
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_JobNum = "234232";
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = "JS";

				AccTransactionLines line = (AccTransactionLines)factory1.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
				line.AL_GB = GlbBranch.CurrentBranch.PK;
				line.AL_GE = GlbDepartment.CurrentDepartment.PK;
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
				line.AL_JH = job.PK;

				return line;
			}, true, CriticalValidationErrorType.WIPMustHaveDebtor_1, @"WIP without a debtor. You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code",

@"Job: Job Number = 234232, PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Parent Table Code = JS, Parent Table Name = , ControllerID = , Shipment Incoterm = , Local Client Code = , Local Client Address = , Overseas Agent Code = , Overseas Agent Address = , Is In DB = No, Has Changes = Yes.

Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = , Type = WIP, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.

Job Charge: Not Set"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("WIP must have debtor code when registry is set to true - extented.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetWIPMustHaveDebtorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				BusinessObject shipment = factory1.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

				var job = factory1.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f"));
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_JobNum = "234232";
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = "JS";

				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				var jobCharge = (JobCharge)factory1.New(typeof(JobCharge), new Guid("d85faec8-f85d-4a99-ae49-5b6923d1ec68"));
				jobCharge.JR_JH = job.PK;
				jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
				jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				jobCharge.JR_AC = Env.Registry.FreightChargeCode;
				jobCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				jobCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				AccTransactionHeader invoice1 = factory1.NewWithPrimaryKey<AccTransactionHeader>(new Guid("e66a7fed-b46b-4322-bdf3-616d341cf2ed"));
				invoice1.FillWithValidTestData();
				invoice1.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice1.AH_TransactionType = TransactionTypes.Invoice;
				invoice1.AH_InvoiceAmount = -100m;
				invoice1.AH_OutstandingAmount = 0M;

				AccTransactionLines line1 = (AccTransactionLines)factory1.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
				line1.AL_GB = GlbBranch.CurrentBranch.PK;
				line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
				line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
				line1.AL_JH = job.PK;
				line1.AL_AH = invoice1.PK;

				AccTransactionHeader invoice2 = factory1.NewWithPrimaryKey<AccTransactionHeader>(new Guid("33cac2bc-2c5a-4c1f-9753-78d5f4af7a7a"));
				invoice2.FillWithValidTestData();
				invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice2.AH_TransactionType = TransactionTypes.Invoice;
				invoice2.AH_InvoiceAmount = -100m;
				invoice2.AH_OutstandingAmount = 0M;

				AccTransactionLines line2 = (AccTransactionLines)factory1.New(typeof(AccTransactionLines), new Guid("E66A7FED-B46B-4322-BDF3-616D341CF2ED"));
				line2.AL_GB = GlbBranch.CurrentBranch.PK;
				line2.AL_GE = GlbDepartment.CurrentDepartment.PK;
				line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
				line2.AL_JH = job.PK;

				jobCharge.JR_AL_APLine = line1.PK;
				jobCharge.JR_AL_ARLine = line2.PK;

				var org = factory1.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "TSLCL1";
				org.OH_IsCreditor = false;
				org.OH_IsDebtor = false;

				return line2;
			}, true, CriticalValidationErrorType.WIPMustHaveDebtor_1, @"WIP without a debtor. You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code",

@"Job: Job Number = 234232, PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Parent Table Code = JS, Parent Table Name = , ControllerID = , Shipment Incoterm = , Local Client Code = , Local Client Address = , Overseas Agent Code = , Overseas Agent Address = , Is In DB = No, Has Changes = Yes.

Line: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Charge Code = , GL Account = , Type = WIP, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.

Default Debtor: Not Set",
"Charge: PK = d85faec8-f85d-4a99-ae49-5b6923d1ec68",
@"Posted Cost:
Cost Header: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Ledger = AP, Transaction Type = INV, Invoice Date = 04-Mar-04 00:00:00, Post Date = , Invoice Amount = -100, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 0CBSU6DGVDNI2TP1RBLLN7XKNXVXACC4CNGTGR, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Cost Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = , Type = CST, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes."));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("WIP  for a different Company is not required to have creditor code when registry is set to true for the current Company.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetWIPMustHaveDebtorCode(GlbCompany.CurrentCompany.PK.ToGuid(), true);

				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.WIP);
				line.AL_OSAmount = line.AL_LineAmount = 10M;
				line.AL_RevRecognitionType = "IMM";
				line.AL_GB = factory1.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_LocalSellAmt = 100M;

				return line;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Accrual doesn't have to have creditor code when registry is set to false.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetAccrualMustHaveCreditorCode(GlbCompany.CurrentCompany.PK.ToGuid(), false);

				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.Accrual);
				line.AL_OSAmount = line.AL_LineAmount = 10M;
				line.AL_RevRecognitionType = "IMM";

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_LocalCostAmt = 100M;

				return line;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("WIP doesn't have to have debtor code when registry is set to false.", (BusinessObjectFactory factory1) =>
			{
				ObjectFactory.Get<IAccounting>().SetWIPMustHaveDebtorCode(GlbCompany.CurrentCompany.PK.ToGuid(), false);

				AccTransactionLines line = GetLine(factory1, true, TransactionLineTypes.WIP);
				line.AL_OSAmount = line.AL_LineAmount = 10M;
				line.AL_RevRecognitionType = "IMM";

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_LocalSellAmt = 100M;

				return line;
			}));

			#endregion

			#region Job related line must have revenue recognition type

			foreach (var lineType in typeof(TransactionLineTypes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetValue(null)))
			{
				string lineType1 = lineType;

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Job related {0} line has not revenue recognition type and amount is zero.", lineType), (BusinessObjectFactory factory1) =>
				{
					var line = GetLine(factory1, true, lineType1, true, true, 0M);
					line.AL_RevRecognitionType = "";

					AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
					AssertNotEquals("Precondition: AL_JH is not empty.", ZGuid.Empty, line.AL_JH);

					return line;
				}));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Job related {0} line has not revenue recognition type and job is empty.", lineType), (BusinessObjectFactory factory1) =>
				{
					var line = GetLine(factory1, true, lineType1, true, true, 10M);
					line.AL_AC = CommentChargeCode.PK;
					line.AL_JH = ZGuid.Empty;
					line.AL_RevRecognitionType = "";

					AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
					AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);

					return line;
				}));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Job related {0} line has not revenue recognition type and charge code is empty.", lineType), (BusinessObjectFactory factory1) =>
				{
					var line = GetLine(factory1, true, lineType1, true, true, 10M);
					line.AL_AC = ZGuid.Empty;
					line.AL_RevRecognitionType = "";

					AssertEquals("Precondition: AL_AC is empty.", ZGuid.Empty, line.AL_AC);
					AssertNotEquals("Precondition: AL_JH is not empty.", ZGuid.Empty, line.AL_JH);

					return line;
				}));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Job related {0} line has revenue recognition type.", lineType), (BusinessObjectFactory factory1) =>
				{
					var line = GetLine(factory1, true, lineType1, true, true, 10M);
					line.AL_RevRecognitionType = "DEP";

					AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
					AssertNotEquals("Precondition: AL_JH is not empty.", ZGuid.Empty, line.AL_JH);

					return line;
				}));
			}

			#endregion

			#region Line charges which are other than comment/overhead/nonaccrual must have job attached

			foreach (var lineType in typeof(TransactionLineTypes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetValue(null)))
			{
				string lineType1 = lineType;
				foreach (var chargeType in chargeTypesMightNotHaveJobs)
				{
					string chargeType1 = chargeType;
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("{0} line which has charge with charge type {1} might not have job attached.", lineType, chargeType), (BusinessObjectFactory factory1) =>
					{
						var line = GetLine(factory1, true, lineType1, true, true, 10M);
						line.AL_AC = ChargeCodeDictionary[chargeType1].PK;
						line.AL_JH = ZGuid.Empty;

						AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
						AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);

						return line;
					}));
				}

				if (lineType == TransactionLineTypes.Cost || lineType == TransactionLineTypes.UnapprovedCost)
				{
					foreach (var chargeType in chargeTypesShouldHaveJobs)
					{
						result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("{0} line which has charge of charge type {1} must have job attached.", lineType, chargeType), (BusinessObjectFactory factory1) =>
						{
							string chargeType1 = chargeType;
							var line = GetLine(factory1, true, lineType1, true, true, 10M);
							line.AL_RevRecognitionType = "DEP";
							line.AL_AC = ChargeCodeDictionary[chargeType1].PK;
							line.AL_JH = ZGuid.Empty;
							AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
							AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);
							Assert("Precondition: line is not in db.", !line.IsInDatabase);
							Assert("Precondition: AL_JH does not have changes", !line.AL_JHInfo.HasChanges);
							Assert("Precondition: AL_AC does not have changes", !line.AL_ACInfo.HasChanges);

							return line;
						}, true, CriticalValidationErrorType.TransactionLineHasNoJobWhenRequiredByChargeCode_2, string.Format("Cost or Unapproved Cost transaction line that does not have a Job, but the line Charge Code '{0}' requires it.", ChargeCodeDictionary[chargeType].AC_Code), "Line: PK ="));
					}
				}

				if (lineType == TransactionLineTypes.UnapprovedCost)
				{
					foreach (var chargeType in chargeTypesShouldHaveJobs)
					{
						string chargeType1 = chargeType;
						result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Created by eNett import {0} line which has charge of charge type {1} that requires job can have no job attached.", lineType, chargeType), (BusinessObjectFactory factory1) =>
						{
							var line = GetLine(factory1, true, lineType1, true, true, 10M, "INVENETT" + lineType1 + chargeType1);
							line.AL_RevRecognitionType = "DEP";
							line.AL_AC = ChargeCodeDictionary[chargeType1].PK;
							line.AL_JH = ZGuid.Empty;
							line.TransactionHeader.IsCreatedByENett = true;

							AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
							AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);
							Assert("Precondition: line is not in db.", !line.IsInDatabase);
							Assert("Precondition: AL_JH does not have changes", !line.AL_JHInfo.HasChanges);
							Assert("Precondition: AL_AC does not have changes", !line.AL_ACInfo.HasChanges);

							return line;
						}));

						result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Created by eNett import {0} line which has charge of charge type {1} that requires job must have a job when converted to CST.", lineType, chargeType), (BusinessObjectFactory factory1) =>
						{
							var line = GetLine(factory1, true, lineType1, true, true, 10M, "INVENETTUAC" + lineType1 + chargeType1);
							line.AL_RevRecognitionType = "DEP";
							line.AL_AC = ChargeCodeDictionary[chargeType1].PK;
							line.AL_JH = ZGuid.Empty;
							AssertEquals("Precondition: AL_LineType must be UAC.", TransactionLineTypes.UnapprovedCost, line.AL_LineType);

							line.TransactionHeader.IsCreatedByENett = true;
							factory1.Save();

							line.AL_LineType = TransactionLineTypes.Cost;

							Assert("Precondition: line is in db as we want AL_LineType to have changes.", line.IsInDatabase);
							Assert("Precondition: AL_LineType does have changes", line.AL_LineTypeInfo.HasChanges);
							AssertEquals("Precondition: AL_LineType must be UAC.", TransactionLineTypes.Cost, line.AL_LineType);
							AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
							AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);
							Assert("Precondition: AL_JH does not have changes", !line.AL_JHInfo.HasChanges);
							Assert("Precondition: AL_AC does not have changes", !line.AL_ACInfo.HasChanges);

							return line;
						}, true, CriticalValidationErrorType.TransactionLineHasNoJobWhenRequiredByChargeCode_2, string.Format("Cost or Unapproved Cost transaction line that does not have a Job, but the line Charge Code '{0}' requires it.", ChargeCodeDictionary[chargeType1].AC_Code), "Line: PK ="));
					}
				}
			}

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Line is in database and AL_JH has changes - {0} line which has charge with charge type {1} must have job attached.", TransactionLineTypes.Cost, Constants.ChargeType.Margin), (BusinessObjectFactory factory1) =>
			{
				var line = GetLine(factory1, true, TransactionLineTypes.Cost, true, true, 10M, "INV123A");
				line.AL_RevRecognitionType = "DEP";

				factory1.Save();

				line.AL_JH = ZGuid.Empty;

				AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
				AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);
				Assert("Precondition: line is in db.", line.IsInDatabase);
				Assert("Precondition: AL_JH has changes", line.AL_JHInfo.HasChanges);
				Assert("Precondition: AL_AC does not have changes", !line.AL_ACInfo.HasChanges);

				return line;
			}, true, CriticalValidationErrorType.TransactionLineHasNoJobWhenRequiredByChargeCode_2, "Cost or Unapproved Cost transaction line that does not have a Job, but the line Charge Code 'FRT' requires it", "Line: PK ="));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Line is in database and AL_AC has changes - {0} line which has charge with charge type {1} must have job attached.", TransactionLineTypes.Cost, Constants.ChargeType.ManualJobAccrual), (BusinessObjectFactory factory1) =>
			{
				var line = GetLine(factory1, false, TransactionLineTypes.Cost, true, true, 10M, "INV123B");
				line.AL_RevRecognitionType = "DEP";
				line.AL_AC = CommentChargeCode.PK;

				factory1.Save();

				line.AL_AC = MJAChargeCode.PK;

				AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
				AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);
				Assert("Precondition: line is in db.", line.IsInDatabase);
				Assert("Precondition: AL_JH does not have changes", !line.AL_JHInfo.HasChanges);
				Assert("Precondition: AL_AC has changes", line.AL_ACInfo.HasChanges);

				return line;
			}, true, CriticalValidationErrorType.TransactionLineHasNoJobWhenRequiredByChargeCode_2, string.Format("Cost or Unapproved Cost transaction line that does not have a Job, but the line Charge Code '{0}' requires it", MJAChargeCode.AC_Code), "Line: PK ="));

			#endregion

			#region Transaction line is not compatible with the transaction header

			var validLineHeadersPairs = new AccTransactionLinesCompatibilityMatrixTestHelper().LineHeaderCompatibilityMatrix;

			foreach (KeyValuePair<string, List<Tuple<string, string>>> entry in validLineHeadersPairs)
			{
				var lineType = entry.Key;
				foreach (var tuple in entry.Value)
				{
					var ledger = tuple.Item1;
					var transactionType = tuple.Item2;
					if (!string.IsNullOrEmpty(ledger) && !string.IsNullOrEmpty(transactionType))
					{
						result.Add(
							new TestCaseDefinitionWithDelegate_Obsolete(
								string.Format("Check Valid combination of Transaction Line with his Header: lineType={0} ledger={1} transactionType={2}", lineType, ledger, transactionType),
								factory =>
								{
									var line = GetLine(Factory, true, lineType, false, false);
									var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
									transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
									transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
									transactionHeader.AH_InvoiceDate = DateTime.Today;
									transactionHeader.AH_TransactionNum = "INV0123";
									transactionHeader.AH_InvoiceAmount = 0m;
									transactionHeader.AH_OSTotal = 0m;
									transactionHeader.AH_OutstandingAmount = 0m;
									transactionHeader.AH_Ledger = ledger;
									transactionHeader.AH_TransactionType = transactionType;
									line.AL_AH = transactionHeader.PK;
									if (transactionType == TransactionTypes.GLAutoJournal || transactionType == TransactionTypes.GLReversingJournal)
									{
										transactionHeader.AH_DueDate = DateTime.Today.AddDays(1);
									}
									GetLineLinkedCharge(line);
									return line;
								},
								false, CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
								"This transaction line is not compatible with the transaction header",
								FormattableString.Invariant($"Transaction Line Type {lineType} is not compatible with Transaction Header Ledger {ledger} and Transaction Type {transactionType}"),
								"Header: PK = ", FormattableString.Invariant($@"Ledger = {ledger}, Transaction Type = {transactionType}, Invoice Date = "), "Post Date = , Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = INV0123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes.",
								"Line: PK = ", "Charge Code = FRT, GL Account = 1010.20.10, Type = CST, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = ", "Job PK = ", "Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes."
							)
						);
					}
				}
			}

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					string.Format("Check valid combination of WIP"),
					factory =>
					{
						var line = GetLine(Factory, true, TransactionLineTypes.WIP);
						return line;
					},
					false, CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
					"This transaction line is not compatible with the transaction header"
				)
			);

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					string.Format("Check valid combination of ACR"),
					factory =>
					{
						var line = GetLine(Factory, true, TransactionLineTypes.Accrual);
						return line;
					},
					false, CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
					"This transaction line is not compatible with the transaction header"
				)
			);

			result.Add(
				new TestCaseDefinitionWithDelegate_Obsolete(
					string.Format("Check Wrong combination of Transaction Line with this Header"),
					factory =>
					{
						var lineType = TransactionLineTypes.Cost;
						var ledger = LedgerTypes.IncompleteTransactions;
						var transactionType = TransactionTypes.IncompleteInvoice;
						var criticalValidationShouldFailed = validLineHeadersPairs[lineType].Any(x => x.Item1 == ledger && x.Item2 == transactionType);
						Assert(!criticalValidationShouldFailed);
						var line = GetLine(Factory, true, lineType, true, true);
						var transactionHeader = Factory.Load<AccTransactionHeader>(line.AL_AH);
						transactionHeader.AH_Ledger = ledger;
						transactionHeader.AH_TransactionType = transactionType;
						return line;
					},
					true, CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
					"This transaction line is not compatible with the transaction header",
					FormattableString.Invariant($"Transaction Line Type {TransactionLineTypes.Cost} is not compatible with Transaction Header Ledger {LedgerTypes.IncompleteTransactions} and Transaction Type {TransactionTypes.IncompleteInvoice}"),
					"Header: PK = ", FormattableString.Invariant($@"Ledger = {LedgerTypes.IncompleteTransactions}, Transaction Type = {TransactionTypes.IncompleteInvoice}, Invoice Date = "), "Post Date = , Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = INV123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.",
					"Line: PK = ", "Charge Code = FRT, GL Account = 1010.20.10, Type = CST, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = ", "Job PK = ", "Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes."
				)
			);

			#endregion

			return result;
		}

		TestCaseDefinitionWithDelegate_Obsolete GetOrphanCFXCases(string lineType)
		{
			return new TestCaseDefinitionWithDelegate_Obsolete("Orphan CFX Line", (BusinessObjectFactory factory) =>
			{
				AccTransactionLines line = GetLine(factory, true, lineType);
				line.TransactionHeader.AH_Ledger = LedgerTypes.JobCosting;
				line.TransactionHeader.AH_TransactionType = TransactionTypes.Journal;

				JobCharge charge = GetLineLinkedCharge(line);
				charge.JR_AL_CFXLine = Guid.Empty;
				charge.JR_AL_APLine = charge.JR_AL_ARLine = line.PK;

				return line;
			}, true, GetTransactionLineWithoutChargeCriticalValidationType(lineType), string.Format("{0} transaction line that does not have a related job charge", lineType), "Line: PK =");
		}

		TestCaseDefinitionWithDelegate_Obsolete GetLineWithoutChargeChargeDeletedCases(string lineType)
		{
			return new TestCaseDefinitionWithDelegate_Obsolete("Line Without Charge: Charge created but deleted then", (BusinessObjectFactory factory) =>
			{
				var line = AccTransactionLinesCriticalValidationTest.GetLine(factory, true, lineType);
				var charge = AccTransactionLinesCriticalValidationTest.GetLineLinkedCharge(line);
				charge.Delete();

				Assert(!line.IsInDatabase);
				return line;
			}, true, GetTransactionLineWithoutChargeCriticalValidationType(lineType), string.Format("{0} transaction line that does not have a related job charge", lineType), "Line: PK =");
		}
		TestCaseDefinitionWithDelegate_Obsolete GetLineWithoutChargePostedChargeDeletedCases(string lineType)
		{
			return new TestCaseDefinitionWithDelegate_Obsolete("Line Without Charge: Posted charge loaded and deleted", (BusinessObjectFactory factory) =>
			{
				var line = GetLine(new BusinessObjectFactory(), true, lineType);
				var charge = GetLineLinkedCharge(line);
				line.Factory.Save();

				var chargeLoaded = factory.Load<JobCharge>(charge.PK);
				chargeLoaded.Delete();

				Assert(line.IsInDatabase);
				return factory.Load<AccTransactionLines>(line.PK);
			}, false, GetTransactionLineWithoutChargeCriticalValidationType(lineType), string.Format("{0} transaction line that does not have a related job charge", lineType), "Line: PK =");
		}

		[TestDate(2009, 1, 1)]
		public void TestJobRelatedLineMustHaveRevenueRecognitionType()
		{
			foreach (bool createD3Record in new bool[] { false, true })
			{
				int headerCount = 0;
				foreach (var lineType in typeof(TransactionLineTypes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetValue(null)))
				{
					var line = GetLine(Factory, true, lineType, true, true, 10M);
					line.AL_RevRecognitionType = "IMM";

					if (line.AL_AH.IsValid)
					{
						headerCount++;
					}

					var date = ZDateTime.Today;

					if (createD3Record)
					{
						var methodInfo = line.Job.GetType().GetMethod("GetOrCreateJobChargeRevRecognition", BindingFlags.NonPublic | BindingFlags.Instance);
						methodInfo.Invoke(line.Job, new object[] { new ZString("JCL"), ZDateTime.Today });
					}

					AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
					AssertNotEquals("Precondition: AL_JH is not empty.", ZGuid.Empty, line.AL_JH);

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format(CultureInfo.InvariantCulture, "Job related {0} line has no revenue recognition type.", lineType));
					AssertOnSavingCheck(line, testCase);

					line.AL_RevRecognitionType = "";
					var messageForRevenueRecognitionTypeFromJobDuringPreSaveValidation = "Message for RevenueRecognitionTypeFromJobDuringPreSaveValidation";
					var messageForRevenueRecognitionTypeFromJobDuringLineCreation = "Message for RevenueRecognitionTypeFromJobDuringLineCreation";
					var messageForRevRecognitionTypeSetFromValidToEmpty = "Message for RevenueRecognitionTypeSetFromValidToEmpty";
					CriticalValidationInfoCollectorService.GetOrCreateService(line.Factory).AddInfoOnceWhenAllowed(line.AL_AC, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation, () => messageForRevenueRecognitionTypeFromJobDuringPreSaveValidation, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
					CriticalValidationInfoCollectorService.GetOrCreateService(line.Factory).AddInfoOnceWhenAllowed(line.AL_AC, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringLineCreation, () => messageForRevenueRecognitionTypeFromJobDuringLineCreation, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
					CriticalValidationInfoCollectorService.GetOrCreateService(line.Factory).AddInfoOnceWhenAllowed(line.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeSetFromValidToEmpty, () => messageForRevRecognitionTypeSetFromValidToEmpty, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

					var expectedRevRecognitionMessage = string.Empty;
					if (createD3Record)
					{
						var methodInfo = line.Job.GetType().GetProperty("RevenueRecognitionCollection", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
						var bizOs = (methodInfo.GetValue(line.Job, null) as IEnumerable).Cast<BusinessObject>().ToArray();
						var pk = new ZGuid(bizOs[0][JobChargeRevRecognitionSchema.PK]);
						expectedRevRecognitionMessage += string.Format(CultureInfo.InvariantCulture, "PK: {0}, D3_JH: {1}, D3_RecognitionType : JCL, D3_RecognitionDate : {2}, Is In DB = No, Has Changes = Yes", pk, line.Job.PK, date.ToAUString());
					}
					else
					{
						expectedRevRecognitionMessage += "No D3 record is created for this Job";
					}

					var expectedMessage = string.Empty;
					if (line.AL_AH.IsValid)
					{
						expectedMessage = string.Format(CultureInfo.InvariantCulture, @"Line: PK = {0}, Charge Code = {1}, GL Account = 1010.20.10, Type = {2}, OS Amount = 10, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {3}, Job PK = {4}, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.

Header: PK = {5}, Ledger = {6}, Transaction Type = {7}, Invoice Date = {8}, Post Date = , Invoice Amount = 10, GST Amount = 0, OS Total = 10, Exchange Rate = 1, Currency = , Outstanding Amount = 10, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = {9}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line: PK = {0}, Charge Code = {1}, GL Account = 1010.20.10, Type = {2}, OS Amount = 10, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {3}, Job PK = {4}, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.

Job: Job Number = {10}, PK = {11}, Parent Table Code = Z0, Parent Table Name = , ControllerID = , Shipment Incoterm = , Local Client Code = , Local Client Address = , Overseas Agent Code = , Overseas Agent Address = , Is In DB = No, Has Changes = Yes.
Revenue Recognition info:

",
															line.PK, line.ChargeCode.AC_Code, lineType, line.AL_AH, line.AL_JH,
															line.AL_AH, line.TransactionHeader.AH_Ledger, line.TransactionHeader.AH_TransactionType, date.ToAUString(), line.TransactionHeader.AH_TransactionNum,
															line.Job.JH_JobNum, line.AL_JH);
					}
					else
					{
						expectedMessage = string.Format(CultureInfo.InvariantCulture, @"Line: PK = {0}, Charge Code = {1}, GL Account = 1010.20.10, Type = {2}, OS Amount = 10, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {3}, Job PK = {4}, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Job: Job Number = {5}, PK = {6}, Parent Table Code = Z0, Parent Table Name = , ControllerID = , Shipment Incoterm = , Local Client Code = , Local Client Address = , Overseas Agent Code = , Overseas Agent Address = , Is In DB = No, Has Changes = Yes.
Revenue Recognition info:

",
													line.PK, line.ChargeCode.AC_Code, lineType, line.AL_AH, line.AL_JH,
													line.Job.JH_JobNum, line.AL_JH);
					}

					expectedMessage += expectedRevRecognitionMessage;
					var expectedMessageWhenJobRelatedLineHasNoRevenueRecognitionType = $@"

RevenueRecognitionTypeFromJobDuringPreSaveValidation:
{messageForRevenueRecognitionTypeFromJobDuringPreSaveValidation}

RevenueRecognitionTypeFromJobDuringLineCreation:
{messageForRevenueRecognitionTypeFromJobDuringLineCreation}

Post Save Revenue Recognition Details:
{line.Job.GetRevenueRecognitionDetails(line.ChargeCode)}

RevenueRecognitionTypeSetFromValidToEmpty:
{messageForRevRecognitionTypeSetFromValidToEmpty}";

					testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format(CultureInfo.InvariantCulture, "Job related {0} line has not revenue recognition type.", lineType), true, CriticalValidationErrorType.JobTransactionLineWithoutRevenueRecognitionType_7, "Job related transaction line that does not have revenue recognition type.", new[] { expectedMessage, expectedMessageWhenJobRelatedLineHasNoRevenueRecognitionType });
					AssertOnSavingCheck(line, testCase);
				}
				AssertEquals("line with header should be created to test header info", 3, headerCount);
			}
		}

		public void TestTransactionLineAmountExceedMaximumAllowedAmount()
		{
			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			var expectedMessage = $"The transaction line amount exceed the maximum allowed amount {registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

			foreach (var lineType in typeof(TransactionLineTypes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetValue(null)))
			{
				var line = GetLine(Factory, true, lineType, true, true, 49M);
				var hasHeader = line.AL_AH.IsValid;
				if (hasHeader)
				{
					line.TransactionHeader.AH_IsCancelled = false;
				}
				line.AL_RevRecognitionType = "IMM";

				using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
				{
					Assert("Precondition: IsInDatabase is false", !line.IsInDatabase);
					Assert("Precondition: AH_IsCancelled is false", !line.TransactionHeader?.AH_IsCancelled ?? true);
					Assert("Precondition: AL_LineAmount is less than registry setting.", Math.Abs(line.AL_LineAmount) < maximumAllowedLineAmount);
					Assert("Precondition: AL_GSTVAT is less than registry setting.", Math.Abs(line.AL_GSTVAT) < maximumAllowedLineAmount);

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Line amount is not greater than registry setting.");
					AssertOnSavingCheck(line, testCase);

					line.AL_RX_NKTransactionCurrency = "CNY";
					line.AL_LineAmount = 51M;
					AssertNotEquals("Precondition: AL_LineAmount is greater than registry setting.", Math.Abs(line.AL_LineAmount) > maximumAllowedLineAmount);

					if (hasHeader)
					{
						testCase = new TestCaseDefinition_ForSeparateTestsMethods("Line amount is greater than registry setting.", true, CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount, expectedMessage);
						AssertOnSavingCheck(line, testCase);
					}
					else
					{
						testCase = new TestCaseDefinition_ForSeparateTestsMethods("Line without header (WIP, ACR), although line amount is greater than registry setting.");
						AssertOnSavingCheck(line, testCase);
					}

					line.AL_LineAmount = 49M;
					line.AL_GSTVAT = 52M;

					Assert("Precondition: AL_LineAmount is less than registry setting.", Math.Abs(line.AL_LineAmount) < maximumAllowedLineAmount);
					Assert("Precondition: AL_GSTVAT is greater than registry setting.", Math.Abs(line.AL_GSTVAT) > maximumAllowedLineAmount);
					AssertOnSavingCheck(line, testCase);
				}

				if (hasHeader)
				{
					using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new MaximumAllowedTransactionAmount() { MaximumAllowedLineAmount = 1M, MaximumAllowedHeaderAmount = 1M }))
					{
						var line1 = GetLine(Factory, true, lineType, true, true, 49M);
						line1.TransactionHeader.AH_IsCancelled = true;
						line1.AL_RevRecognitionType = "IMM";

						Assert("Precondition: AH_IsCancelled is true", line1.TransactionHeader.AH_IsCancelled);

						var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction header is cancelled.");
						AssertOnSavingCheck(line1, testCase);
					}
				}
			}
		}

		public void TestTransactionLineAmountExceedMaximumAllowedAmount_InDB()
		{
			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			var expectedMessage = $"The transaction line amount exceed the maximum allowed amount {registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var newFactory1 = new BusinessObjectFactory();
				var line1 = GetLine(newFactory1, true, TransactionLineTypes.Cost, true, true);
				newFactory1.Save();

				Assert("Precondition: IsInDatabase is true", line1.IsInDatabase);
				Assert("Precondition: AL_GSTVATInfo HasChanges is false", !line1.AL_GSTVATInfo.HasChanges);
				Assert("Precondition: AL_LineAmountInfo HasChanges is false", !line1.AL_LineAmountInfo.HasChanges);

				var testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("Line is in DB and AL_GSTVATInfo,AL_LineAmountInfo have no changes.");
				AssertOnSavingCheck(line1, testCase1);

				line1.AL_GSTVAT = 52M;
				Assert("Precondition: AL_GSTVATInfo HasChanges is true", line1.AL_GSTVATInfo.HasChanges);

				testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("AL_GSTVATInfo HasChanges and AL_GSTVAT is greater than registry setting.", true, CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount, expectedMessage);
				AssertOnSavingCheck(line1, testCase1);

				var newFactory2 = new BusinessObjectFactory();
				var line2 = GetLine(newFactory2, true, TransactionLineTypes.Revenue, true, true);
				newFactory2.Save();

				Assert("Precondition: IsInDatabase is true", line2.IsInDatabase);
				Assert("Precondition: AL_GSTVATInfo HasChanges is false", !line2.AL_GSTVATInfo.HasChanges);
				Assert("Precondition: AL_LineAmountInfo HasChanges is false", !line2.AL_LineAmountInfo.HasChanges);

				var testCase2 = new TestCaseDefinition_ForSeparateTestsMethods("Line is in DB and AL_GSTVATInfo,AL_LineAmountInfo have no changes.");
				AssertOnSavingCheck(line2, testCase2);

				line2.AL_LineAmount = 53M;
				Assert("Precondition: AL_LineAmountInfo HasChanges is true", line2.AL_LineAmountInfo.HasChanges);

				testCase2 = new TestCaseDefinition_ForSeparateTestsMethods("AL_LineAmountInfo HasChanges and AL_LineAmount is greater than registry setting.", true, CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount, expectedMessage);
				AssertOnSavingCheck(line2, testCase2);
			}
		}

		public void TestReverseAPCreditNoteWithChargeTypeChange()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();

			line.AL_AH = transaction.PK;
			line.AL_LineType = TransactionLineTypes.Cost;

			line.AL_AC = chargeCode.PK;
			Assert(AccChargeCode.IsValidInAP(chargeCode.AC_ChargeType, false));

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;

			var helper = ObjectFactory.Get<IAccTransactionHeaderCriticalValidationHelper>();
			helper.SetIsReversalOfOriginalTransaction_ForTestOnly(transaction);

			AssertNotEquals("Precondition: AL_AC is not empty.", ZGuid.Empty, line.AL_AC);
			AssertEquals("Precondition: AL_JH is empty.", ZGuid.Empty, line.AL_JH);
			Assert("Precondition: line is not in db.", !line.IsInDatabase);
			Assert("Precondition: AL_JH does not have changes", !line.AL_JHInfo.HasChanges);
			Assert("Precondition: AL_AC does not have changes", !line.AL_ACInfo.HasChanges);
			Assert("Precondition: MRG should return false", !AccChargeCode.IsValidInAP(chargeCode.AC_ChargeType, false));

			AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail any critical validation."));
		}

		public void TestCheckCompatibleWhenTransactionHeaderInDBAndTransactionTypeOrLedgerHasChages()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line.AL_AH = transaction.PK;
			transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			transaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
			line.AL_LineType = TransactionLineTypes.Cost;

			Assert("Precondition: Header is in db.", !transaction.IsInDatabase);
			AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods(
				"Should Check Wrong combination of Transaction Line with this Header When Transaction Is Not In DB", true,
				CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
				"This transaction line is not compatible with the transaction header"));

			var newTransaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			newTransaction.AH_TransactionType = TransactionTypes.Invoice;
			newTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			line.AL_AH = newTransaction.PK;
			Factory.Save();

			line.AL_AH = transaction.PK;

			Assert("Precondition: Header is in db.", transaction.IsInDatabase);
			AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail any critical validation."));

			transaction.AH_TransactionType = TransactionTypes.Journal;
			Assert(transaction.AH_TransactionTypeInfo.HasChanges);
			AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods(
				"Should Check Wrong combination of Transaction Line with this Header When Transaction Is In DB And TransactionType Has Changes", true,
				CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
				"This transaction line is not compatible with the transaction header"));

			transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			transaction.AH_Ledger = LedgerTypes.JobCosting;
			Assert(!transaction.AH_TransactionTypeInfo.HasChanges);
			Assert(transaction.AH_LedgerInfo.HasChanges);
			AssertOnSavingCheck(line, new TestCaseDefinition_ForSeparateTestsMethods(
				"Should Check Wrong combination of Transaction Line with this Header When Transaction Is In DB And Ledger Has Changes", true,
				CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4, "This transaction line is not compatible with the transaction header"));
		}

		public void TestJobChargeLocalSellAmtChangeWhenPostingReceivableChargesHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			var collectorService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			collectorService.AddInfoWhenAllowed(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtChangeWhenPostingReceivableCharges, () => "test");
			Factory.SetContext(BusinessContext.PostingReceivableChargesForFactoryLevel);

			charge.JR_LocalSellAmt = 99M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeLocalSellAmtChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			charge.JR_LocalSellAmt = 100M;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeLocalSellAmtChangeWhenPostingReceivableCharges:",
				"JR_LocalSellAmt = 100, JR_LocalSellAmt Old Value = 99",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
		}

		public void TestJobChargeSellCurrencyChangeWhenPostingReceivableChargesHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			charge.JR_RX_NKSellCurrency = "CNY";
			charge.JR_LocalSellAmt = 100M;

			Factory.SetContext(BusinessContext.PostingReceivableChargesForFactoryLevel);

			charge.JR_RX_NKSellCurrency = "USD";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeSellCurrencyChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			charge.JR_RX_NKSellCurrency = "AUD";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeSellCurrencyChangeWhenPostingReceivableCharges:",
				"JR_RX_NKSellCurrency = AUD, JR_RX_NKSellCurrency Old Value = USD",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
		}

		public void TestJobChargeSellCurrencyChangeWhenPostingChargesFromConsolHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			charge.JR_RX_NKSellCurrency = "CNY";
			charge.JR_LocalSellAmt = 100M;
			var regisItem = ObjectFactory.Get<IAccounting>().Registry.ProfitShareChargeCode;
			charge.JR_AC = (regisItem as ZArchitecture.Environment.ChargeCodeRegistryItem).Value;

			Factory.SetContext(BusinessContext.PostingChargesFromConsol);

			charge.JR_RX_NKSellCurrency = "USD";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeSellCurrencyChangeWhenPostingOverseasAgentChargesFromConsol: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			charge.JR_RX_NKSellCurrency = "AUD";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeSellCurrencyChangeWhenPostingOverseasAgentChargesFromConsol:",
				"JR_RX_NKSellCurrency = AUD, JR_RX_NKSellCurrency Old Value = USD",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingChargesFromConsol);
		}

		public void TestJobChargeOSSellExRateChangeWhenPostingReceivableChargesHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			charge.JR_RX_NKSellCurrency = "CNY";
			charge.JR_LocalSellAmt = 100M;

			Factory.SetContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel);

			charge.JR_OSSellExRate = 2M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeOSSellExRateChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			charge.JR_OSSellExRate = 3M;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"JobChargeOSSellExRateChangeWhenPostingReceivableCharges:",
				"JR_OSSellExRate = 3, JR_OSSellExRate Old Value = 2",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
		}

		public void TestTransactionLineAmountChangeWhenPostingReceivableChargesHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			line.AL_RevRecognitionType = "IMM";

			Factory.SetContext(BusinessContext.PostingReceivableChargesForFactoryLevel);

			line.AL_LineAmount = 100M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"TransactionLineAmountChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			line.AL_LineAmount = 99M;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"TransactionLineAmountChangeWhenPostingReceivableCharges:",
				"AL_LineAmount = 99, AL_LineAmount Old Value = 100",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
		}

		public void TestTransactionLineCurrencyChangeWhenPostingReceivableChargesHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			line.AL_RevRecognitionType = "IMM";
			line.AL_RX_NKTransactionCurrency = "CNY";
			line.AL_LineAmount = 100M;

			Factory.SetContext(BusinessContext.PostingReceivableChargesForFactoryLevel);

			line.AL_RX_NKTransactionCurrency = "USD";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"TransactionLineCurrencyChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			line.AL_RX_NKTransactionCurrency = "AUD";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"TransactionLineCurrencyChangeWhenPostingReceivableCharges:",
				"AL_RX_NKTransactionCurrency = AUD, AL_RX_NKTransactionCurrency Old Value = USD",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
		}

		public void TestTransactionLineExChangeRateChangeWhenPostingReceivableChargesHasStackTrace()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var charge = GetLineLinkedCharge(line);

			line.AL_RevRecognitionType = "IMM";
			line.AL_RX_NKTransactionCurrency = "CNY";
			line.AL_LineAmount = 100M;

			Factory.SetContext(BusinessContext.PostingReceivableChargesForFactoryLevel);

			line.AL_ExchangeRate = 2M;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"TransactionLineExChangeRateChangeWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(line, testCase);

			line.AL_ExchangeRate = 3M;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Amount for Rev Line",
				true,
				CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
				"Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount",
				"TransactionLineExChangeRateChangeWhenPostingReceivableCharges:",
				"AL_ExchangeRate = 3, AL_ExchangeRate Old Value = 2",
				"at"
			);

			AssertOnSavingCheck(line, testCase);

			Factory.RemoveContext(BusinessContext.PostingReceivableChargesForFactoryLevel);
		}

		public void TestTransactionLineNegativeAmountNotAllowedOnAccountReceivableTransactions()
		{
			var testDataItems = new[]
			{
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSAmount = 5, FactorySave = false, ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSAmount = 5, FactorySave = false, ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSAmount = -5, FactorySave = false, ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSAmount = -5, FactorySave = false, ExpectedToFail = true },
				new { TransactionLineType = TransactionLineTypes.Cost, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSAmount = -5, FactorySave = false, ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Accrual, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSAmount = -5, FactorySave = false, ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSAmount = -5, FactorySave = true, ExpectedToFail = false },
			};

			var transactionNumber = 0;
			foreach (var codeGroup in testDataItems.GroupBy(testDataItem => testDataItem.Code))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
				using (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codeGroup.Key))
				{
					foreach (var testDataItem in codeGroup)
					{
						var line = GetLine(Factory, true, testDataItem.TransactionLineType, localAmount: testDataItem.OSAmount, transactionNumber: "I" + (++transactionNumber).ToString("D4"));
						line.AL_ExchangeRate = 1M;
						line.AL_OSAmount = testDataItem.OSAmount;
						line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

						GetLineLinkedCharge(line);

						if (testDataItem.FactorySave)
						{
							using (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code))
							{
								Factory.Save();
							}
						}

						var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
							"Negative Charges on Accounts Receivable Transactions",
							testDataItem.ExpectedToFail,
							CriticalValidationErrorType.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed,
							"To comply with tax regulations in Australia, you are not allowed to post AR INV with negative amounts on any lines.");

						AssertOnSavingCheck(line, testCase);
					}
				}
			}
		}

		public void TestTransactionLineNegativeAmountNotAllowedOnAccountReceivableTransactions_NegativeChargesAllowed()
		{
			var testDataItems = new[]
			{
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code, OSAmount = -5, IsDebtorVATIsNotApplicable = true,	FactorySave = false,	ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code, OSAmount = -5, IsDebtorVATIsNotApplicable = false,	FactorySave = false,	ExpectedToFail = true },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code, OSAmount = -5, IsDebtorVATIsNotApplicable = false,	FactorySave = true,		ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Revenue, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code, OSAmount = 5,  IsDebtorVATIsNotApplicable = false,	FactorySave = false,	ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Cost,	  AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code, OSAmount = -5, IsDebtorVATIsNotApplicable = false,	FactorySave = false,	ExpectedToFail = false },
				new { TransactionLineType = TransactionLineTypes.Accrual, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code, OSAmount = -5, IsDebtorVATIsNotApplicable = false,	FactorySave = false,	ExpectedToFail = false },
			};

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var transactionNumber = 0;
			foreach (var codeGroup in testDataItems.GroupBy(testDataItem => testDataItem.Code))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
				using (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codeGroup.Key))
				{
					foreach (var testDataItem in codeGroup)
					{
						var line = GetLine(Factory, true, testDataItem.TransactionLineType, localAmount: testDataItem.OSAmount, transactionNumber: "I" + (++transactionNumber).ToString("D4"), organization: testDataItem.IsDebtorVATIsNotApplicable ? org1 : org2);
						line.AL_ExchangeRate = 1M;
						line.AL_OSAmount = testDataItem.OSAmount;
						line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

						GetLineLinkedCharge(line);

						if (testDataItem.FactorySave)
						{
							using (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code))
							{
								Factory.Save();
							}
						}

						var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
							"Negative Charges on Accounts Receivable Transactions",
							testDataItem.ExpectedToFail,
							CriticalValidationErrorType.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed,
							"To comply with tax regulations in Australia, you are not allowed to post AR INV with negative amounts on any lines.");

						AssertOnSavingCheck(line, testCase);
					}
				}
			}
		}

		public void TestTransactionLineWithInvalidTaxIdAndTaxMessageMapping()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "TaxMsg01";
			Factory.Save();

			var config = AccountingTestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, taxRate, taxMsg));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var line = GetLine(Factory, false, TransactionLineTypes.Revenue, localAmount: 190m);
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = ZGuid.Empty;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Invalid Tax ID and Tax Message Mapping",
				true,
				CriticalValidationErrorType.TransactionLineTaxIdAndTaxMessageMappingInvalid_1,
				"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.",
				"Line Type=REV, Tax ID=TaxRate01, Tax Message=");

			AssertOnSavingCheck(line, testCase);
		}

		public void TestTransactionLineWithInvalidTaxIdAndTaxMessageMapping_LineIsInDatabase()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "TaxMsg01";
			Factory.Save();

			var line = GetLine(Factory, false, TransactionLineTypes.Revenue, localAmount: 190m);
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = ZGuid.Empty;
			Factory.Save();

			var config = AccountingTestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, taxRate, taxMsg));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			AssertNotEquals("Pre-condition", TransactionLineTypes.UnapprovedCost, line.AL_LineTypeInfo.OriginalValue);
			AssertEquals(true, line.IsInDatabase);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Non-UCT Line already saved should skip Tax ID and Tax Message Mapping validation");

			AssertOnSavingCheck(line, testCase);
		}

		public void TestTransactionLineWithInvalidTaxIdAndTaxMessageMapping_PostUnapprovedCost()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "TaxMsg01";
			Factory.Save();

			var line = GetLine(Factory, true, TransactionLineTypes.UnapprovedCost, localAmount: 190m);
			var charge = GetLineLinkedCharge(line);
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = ZGuid.Empty;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			charge.JR_AT_CostGSTRate = line.AL_AT;
			Factory.Save();

			var config = AccountingTestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate, taxMsg));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			line.AL_LineType = TransactionLineTypes.Cost;

			AssertEquals("Pre-condition", TransactionLineTypes.UnapprovedCost, line.AL_LineTypeInfo.OriginalValue);
			AssertEquals(true, line.IsInDatabase);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Invalid Tax ID and Tax Message Mapping for posted unapporved cost line",
				true,
				CriticalValidationErrorType.TransactionLineTaxIdAndTaxMessageMappingInvalid_1,
				"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.",
				"Line Type=CST, Tax ID=TaxRate01, Tax Message=");

			AssertOnSavingCheck(line, testCase);
		}

		public void TestWIPOrganisationDoesNotMatchOneOnJobChargeErrorMessage()
			=> TestWIPOrganisationDoesNotMatchOneOnJobChargeErrorMessage(false);

		public void TestWIPOrganisationDoesNotMatchOneOnJobChargeErrorMessageWhenWIPAlreadyInDatabase()
			=> TestWIPOrganisationDoesNotMatchOneOnJobChargeErrorMessage(true);

		void TestWIPOrganisationDoesNotMatchOneOnJobChargeErrorMessage(bool wipIsAlreadyInDb)
		{
			var orgOld1 = Factory.NewWithValidTestData<OrgHeader>();
			orgOld1.OH_Code = "Old1";
			orgOld1.OH_IsDebtor = true;

			var orgOld2 = Factory.NewWithValidTestData<OrgHeader>();
			orgOld2.OH_Code = "Old2";
			orgOld2.OH_IsDebtor = true;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TEST1";
			org1.OH_IsDebtor = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TEST2";
			org2.OH_IsDebtor = true;

			var line = GetLine(Factory, true, TransactionLineTypes.WIP);
			JobCharge charge = GetLineLinkedCharge(line);
			if (wipIsAlreadyInDb)
			{
				Factory.Save();
			}

			line.AL_OH = orgOld1.PK;
			charge.JR_OH_SellAccount = orgOld2.PK;

			var notCollectedInfo_Key = "WIPACROrganizationNotEqualToRelatedChargeForNewLine";
			var notCollectedInfoInDb_Key = "WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb";
			var prefix = wipIsAlreadyInDb ? "WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb:" : "WIPACROrganizationNotEqualToRelatedChargeForNewLine:";
			var notCollectedInfo_WIPACROrganizationIsNotSet_Key = "WIPACROrganizationIsNotSet";
			var message1 = $@"
RelatedCharge PK: {charge.PK}, Charge Sell Organization: 00000000-0000-0000-0000-000000000000, Line Organization: {orgOld1.PK}, Old Organization: 00000000-0000-0000-0000-000000000000
   at";
			var message2 = $@"
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {orgOld2.PK}, Line Organization: {orgOld1.PK}, Old Organization: 00000000-0000-0000-0000-000000000000
   at";
			var message3 = $@"
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {orgOld2.PK}, Line Organization: {org1.PK}, Old Organization: {orgOld1.PK}
   at";
			var message4 = $@"
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {org2.PK}, Line Organization: {org1.PK}, Old Organization: {orgOld2.PK}
   at";

			var wipacr_extraMessage = $@"
Is Debtor For Company: Yes
Branch Company PK: {GlbCompany.CurrentCompany.PK}
Org PK: {orgOld2.PK}
";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Organization WIP Line",
				true,
				CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
				"Transaction line organization does not equal related job charge organization",
				$@"Transaction line organization does not equal related job charge organization.

	PK = {line.PK}",
				"Properties:",
				$@"RelatedCharge:
	PK = {charge.PK}",
				"Properties:",
				wipIsAlreadyInDb ? notCollectedInfo_Key : prefix + message1,
				wipIsAlreadyInDb ? "" : message2,
				wipIsAlreadyInDb ? notCollectedInfoInDb_Key : "",
				notCollectedInfo_WIPACROrganizationIsNotSet_Key,
				wipacr_extraMessage
			);

			AssertOnSavingCheck(line, testCase);

			line.AL_OH = org1.PK;
			charge.JR_OH_SellAccount = org2.PK;

			wipacr_extraMessage = $@"
Is Debtor For Company: Yes
Branch Company PK: {GlbCompany.CurrentCompany.PK}
Org PK: {org2.PK}
";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Organization WIP Line",
				true,
				CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
				"Transaction line organization does not equal related job charge organization.",
				$@"Transaction line organization does not equal related job charge organization.

	PK = {line.PK}",
				"Properties:",
				$@"RelatedCharge:
	PK = {charge.PK}",
				"Properties:",
				wipIsAlreadyInDb ? notCollectedInfo_Key : prefix + message1,
				wipIsAlreadyInDb ? "" : message2,
				wipIsAlreadyInDb ? "" : message3,
				wipIsAlreadyInDb ? "" : message4,
				wipIsAlreadyInDb ? prefix + message3 : notCollectedInfoInDb_Key,
				wipIsAlreadyInDb ? message4 : "",
				notCollectedInfo_WIPACROrganizationIsNotSet_Key,
				wipacr_extraMessage
			);

			AssertOnSavingCheck(line, testCase);

			charge.JR_OH_SellAccount = ZGuid.Empty;

			wipacr_extraMessage = $@"
Is Debtor For Company: 
Branch Company PK: {GlbCompany.CurrentCompany.PK}
Org PK: {ZGuid.Empty}
";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Organization WIP Line",
				true,
				CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
				"Transaction line organization does not equal related job charge organization.",
				$@"Transaction line organization does not equal related job charge organization.

	PK = {line.PK}",
				"Properties:",
				$@"RelatedCharge:
	PK = {charge.PK}",
				"Properties:",
				wipIsAlreadyInDb ? notCollectedInfo_Key : prefix + message1,
				wipIsAlreadyInDb ? "" : message2,
				wipIsAlreadyInDb ? "" : message3,
				wipIsAlreadyInDb ? "" : message4,
				wipIsAlreadyInDb ? prefix + message3 : notCollectedInfoInDb_Key,
				wipIsAlreadyInDb ? message4 : "",
				notCollectedInfo_WIPACROrganizationIsNotSet_Key,
				wipacr_extraMessage
			);

			AssertOnSavingCheck(line, testCase);
		}

		public void TestACROrganisationDoesNotMatchOneOnJobChargeErrorMessage()
			=> TestACROrganisationDoesNotMatchOneOnJobChargeErrorMessage(false);

		public void TestACROrganisationDoesNotMatchOneOnJobChargeErrorMessageWhenACRAlreadyInDatabase()
			=> TestACROrganisationDoesNotMatchOneOnJobChargeErrorMessage(true);

		void TestACROrganisationDoesNotMatchOneOnJobChargeErrorMessage(bool acrIsAlreadyInDb)
		{
			var orgOld1 = Factory.NewWithValidTestData<OrgHeader>();
			orgOld1.OH_Code = "Old1";
			orgOld1.OH_IsCreditor = true;

			var orgOld2 = Factory.NewWithValidTestData<OrgHeader>();
			orgOld2.OH_Code = "Old2";
			orgOld2.OH_IsCreditor = true;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TEST1";
			org1.OH_IsCreditor = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TEST2";
			org2.OH_IsCreditor = true;

			var line = GetLine(Factory, true, TransactionLineTypes.Accrual);
			JobCharge charge = GetLineLinkedCharge(line);
			if (acrIsAlreadyInDb)
			{
				Factory.Save();
			}

			line.AL_OH = orgOld1.PK;
			charge.JR_OH_CostAccount = orgOld2.PK;

			var notCollectedInfo_Key = "WIPACROrganizationNotEqualToRelatedChargeForNewLine";
			var notCollectedInfoInDb_Key = "WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb";
			var prefix = acrIsAlreadyInDb ? "WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb:" : "WIPACROrganizationNotEqualToRelatedChargeForNewLine:";
			var notCollectedInfo_WIPACROrganizationIsNotSet_Key = "WIPACROrganizationIsNotSet";
			var message1 = $@"
RelatedCharge PK: {charge.PK}, Charge Cost Organization: 00000000-0000-0000-0000-000000000000, Line Organization: {orgOld1.PK}, Old Organization: 00000000-0000-0000-0000-000000000000
   at";
			var message2 = $@"
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {orgOld2.PK}, Line Organization: {orgOld1.PK}, Old Organization: 00000000-0000-0000-0000-000000000000
   at";
			var message3 = $@"
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {orgOld2.PK}, Line Organization: {org1.PK}, Old Organization: {orgOld1.PK}
   at";
			var message4 = $@"
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {org2.PK}, Line Organization: {org1.PK}, Old Organization: {orgOld2.PK}
   at";

			var wipacr_extraMessage = $@"
Is Creditor For Company: Yes
Branch Company PK: {GlbCompany.CurrentCompany.PK}
Org PK: {orgOld2.PK}
";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Organization ACR Line",
				true,
				CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
				"Transaction line organization does not equal related job charge organization",
				$@"Transaction line organization does not equal related job charge organization.

	PK = {line.PK}",
				"Properties:",
				$@"RelatedCharge:
	PK = {charge.PK}",
				"Properties:",
				acrIsAlreadyInDb ? notCollectedInfo_Key : prefix + message1,
				acrIsAlreadyInDb ? "" : message2,
				acrIsAlreadyInDb ? notCollectedInfoInDb_Key : "",
				acrIsAlreadyInDb ? notCollectedInfo_WIPACROrganizationIsNotSet_Key : notCollectedInfo_WIPACROrganizationIsNotSet_Key,
				wipacr_extraMessage
			);

			AssertOnSavingCheck(line, testCase);

			line.AL_OH = org1.PK;
			charge.JR_OH_CostAccount = org2.PK;

			wipacr_extraMessage = $@"
Is Creditor For Company: Yes
Branch Company PK: {GlbCompany.CurrentCompany.PK}
Org PK: {org2.PK}
";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Organization ACR Line",
				true,
				CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
				"Transaction line organization does not equal related job charge organization.",
				$@"Transaction line organization does not equal related job charge organization.

	PK = {line.PK}",
				"Properties:",
				$@"RelatedCharge:
	PK = {charge.PK}",
				"Properties:",
				acrIsAlreadyInDb ? notCollectedInfo_Key : prefix + message1,
				acrIsAlreadyInDb ? "" : message2,
				acrIsAlreadyInDb ? "" : message3,
				acrIsAlreadyInDb ? "" : message4,
				acrIsAlreadyInDb ? prefix + message3 : notCollectedInfoInDb_Key,
				acrIsAlreadyInDb ? message4 : "",
				notCollectedInfo_WIPACROrganizationIsNotSet_Key,
				wipacr_extraMessage
			);

			AssertOnSavingCheck(line, testCase);

			charge.JR_OH_CostAccount = ZGuid.Empty;

			wipacr_extraMessage = $@"
Is Creditor For Company: 
Branch Company PK: {GlbCompany.CurrentCompany.PK}
Org PK: {ZGuid.Empty}
";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Incorrect Organization ACR Line",
				true,
				CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
				"Transaction line organization does not equal related job charge organization.",
				$@"Transaction line organization does not equal related job charge organization.

	PK = {line.PK}",
				"Properties:",
				$@"RelatedCharge:
	PK = {charge.PK}",
				"Properties:",
				acrIsAlreadyInDb ? notCollectedInfo_Key : prefix + message1,
				acrIsAlreadyInDb ? "" : message2,
				acrIsAlreadyInDb ? "" : message3,
				acrIsAlreadyInDb ? "" : message4,
				acrIsAlreadyInDb ? prefix + message3 : notCollectedInfoInDb_Key,
				acrIsAlreadyInDb ? message4 : "",
				notCollectedInfo_WIPACROrganizationIsNotSet_Key,
				wipacr_extraMessage
			);

			AssertOnSavingCheck(line, testCase);
		}

		public void TestCheckCostLineReferencedByMultipleJobCharges_LinkedByDifferentFields()
		{
			AssertCheckLineReferencedByMultipleJobCharges_LinkedByDifferentFields(TransactionLineTypes.Cost, false);
		}

		public void TestCheckRevenueLineReferencedByMultipleJobCharges_LinkedByDifferentFields()
		{
			AssertCheckLineReferencedByMultipleJobCharges_LinkedByDifferentFields(TransactionLineTypes.Revenue, false);
		}

		public void TestCheckCostLineReferencedByMultipleJobCharges_LinkedByDifferentFields_LineIsInDB()
		{
			AssertCheckLineReferencedByMultipleJobCharges_LinkedByDifferentFields(TransactionLineTypes.Cost, true);
		}

		public void TestCheckRevenueLineReferencedByMultipleJobCharges_LinkedByDifferentFields_LineIsInDB()
		{
			AssertCheckLineReferencedByMultipleJobCharges_LinkedByDifferentFields(TransactionLineTypes.Revenue, true);
		}

		void AssertCheckLineReferencedByMultipleJobCharges_LinkedByDifferentFields(ZString lineType, bool isLineInDB)
		{
			var line = GetLine(Factory, true, lineType, true);

			if (isLineInDB)
			{
				Factory.Save();
			}

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			GetChargeToLink(line, jobCharge);
			switch (line.AL_LineType)
			{
				case TransactionLineTypes.Cost:
					jobCharge.JR_AL_ARLine = line.PK;
					break;
				case TransactionLineTypes.Revenue:
					jobCharge.JR_AL_APLine = line.PK;
					break;
				default:
					Fail("Only support Cost / Revenue");
					break;
			}
			jobCharge.SetAmountsFromLinkedLinesForTests();

			var testCase = isLineInDB
				? new TestCaseDefinition_ForSeparateTestsMethods("Line is in database and is referenced by more than one charge records linking by different fields.")
				: new TestCaseDefinition_ForSeparateTestsMethods("Line is not saved yet and is referenced by more than one charge records linking by different fields.",
					true,
					CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_LineSide_5,
					"Cost, Revenue or Unapproved Cost transaction line that has more than one related job charge.",
					"Line: PK =",
					"Charge: PK =",
					"Charge: PK =");

			AssertOnSavingCheck(line, testCase);
		}

		public void TestCheckCostLineReferencedByMultipleJobCharges_LinkedByTheSameField_LineIsInDB()
		{
			AssertCheckLineReferencedByMultipleJobCharges_LinkedByTheSameField(TransactionLineTypes.Cost, true);
		}

		public void TestCheckRevenueLineReferencedByMultipleJobCharges_LinkedByTheSameField_LineIsInDB()
		{
			AssertCheckLineReferencedByMultipleJobCharges_LinkedByTheSameField(TransactionLineTypes.Revenue, true);
		}

		void AssertCheckLineReferencedByMultipleJobCharges_LinkedByTheSameField(ZString lineType, bool isLineInDB)
		{
			var line = GetLine(Factory, true, lineType, true);

			if (isLineInDB)
			{
				Factory.Save();
			}

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			GetChargeToLink(line, jobCharge);
			switch (line.AL_LineType)
			{
				case TransactionLineTypes.Cost:
					jobCharge.JR_AL_APLine = line.PK;
					break;
				case TransactionLineTypes.Revenue:
					jobCharge.JR_AL_ARLine = line.PK;
					break;
				default:
					Fail("Only support Cost / Revenue");
					break;
			}
			jobCharge.SetAmountsFromLinkedLinesForTests();

			var testCase = isLineInDB
				? new TestCaseDefinition_ForSeparateTestsMethods("Line is in database and is referenced by more than one charge records linking by different fields.")
				: new TestCaseDefinition_ForSeparateTestsMethods("Line is not saved yet and is referenced by more than one charge records linking by different fields.",
					true,
					CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_LineSide_5,
					"Cost, Revenue or Unapproved Cost transaction line that has more than one related job charge.",
					"Line: PK =",
					"Charge: PK =",
					"Charge: PK =");

			AssertOnSavingCheck(line, testCase);
		}

		public void TestShouldReportNoChargeCriticalValidationWhenCostIsLinkedByJR_AL_ARLine()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Cost);
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			GetChargeToLink(line, jobCharge);
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.SetAmountsFromLinkedLinesForTests();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CST transaction line that does not have a related job charge.",
				true,
				CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5,
				"CST transaction line that does not have a related job charge.",
				"Line: PK =");

			AssertOnSavingCheck(line, testCase);
		}

		public void TestShouldReportNoChargeCriticalValidationWhenRevenueIsLinkedByJR_AL_APLine()
		{
			var line = GetLine(Factory, true, TransactionLineTypes.Revenue);
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			GetChargeToLink(line, jobCharge);
			jobCharge.JR_AL_APLine = line.PK;
			jobCharge.SetAmountsFromLinkedLinesForTests();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("REV transaction line that does not have a related job charge.",
				true,
				CriticalValidationErrorType.RevenueTransactionLineWithoutJobCharge_6,
				"REV transaction line that does not have a related job charge.",
				"Line: PK =");

			AssertOnSavingCheck(line, testCase);
		}

		#region Implementation

		readonly string[] chargeTypesShouldHaveJobs = new[] { Constants.ChargeType.Disbursement, Constants.ChargeType.ManualJobAccrual, Constants.ChargeType.Margin, Constants.ChargeType.Revenue };
		readonly string[] chargeTypesMightNotHaveJobs = new[] { Constants.ChargeType.Comment, Constants.ChargeType.Overhead, Constants.ChargeType.NonAccrual };

		AccChargeCode commentChargeCode;
		AccChargeCode CommentChargeCode
		{
			get
			{
				if (commentChargeCode == null)
				{
					commentChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
					Factory.Save();
				}
				return commentChargeCode;
			}
		}

		AccChargeCode overheadChargeCode;
		AccChargeCode OverheadChargeCode
		{
			get
			{
				if (overheadChargeCode == null)
				{
					overheadChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					overheadChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
					Factory.Save();
				}
				return overheadChargeCode;
			}
		}

		AccChargeCode nonAccrualChargeCode;
		AccChargeCode NonAccrualChargeCode
		{
			get
			{
				if (nonAccrualChargeCode == null)
				{
					nonAccrualChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					nonAccrualChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
					Factory.Save();
				}
				return nonAccrualChargeCode;
			}
		}

		AccChargeCode disbursementChargeCode;
		AccChargeCode DisbursementChargeCode
		{
			get
			{
				if (disbursementChargeCode == null)
				{
					disbursementChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					disbursementChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
					Factory.Save();
				}
				return disbursementChargeCode;
			}
		}

		AccChargeCode mjaChargeCode;
		AccChargeCode MJAChargeCode
		{
			get
			{
				if (mjaChargeCode == null)
				{
					mjaChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					mjaChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
					Factory.Save();
				}
				return mjaChargeCode;
			}
		}

		AccChargeCode marginChargeCode;
		AccChargeCode MarginChargeCode
		{
			get
			{
				if (marginChargeCode == null)
				{
					marginChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					marginChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
					Factory.Save();
				}
				return marginChargeCode;
			}
		}

		AccChargeCode revenueChargeCode;
		AccChargeCode RevenueChargeCode
		{
			get
			{
				if (revenueChargeCode == null)
				{
					revenueChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					revenueChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
					Factory.Save();
				}
				return revenueChargeCode;
			}
		}

		Dictionary<string, AccChargeCode> chargeCodeDictionary;
		Dictionary<string, AccChargeCode> ChargeCodeDictionary
		{
			get
			{
				if (chargeCodeDictionary == null)
				{
					chargeCodeDictionary = new Dictionary<string, AccChargeCode>();
					chargeCodeDictionary.Add(Constants.ChargeType.Comment, CommentChargeCode);
					chargeCodeDictionary.Add(Constants.ChargeType.Overhead, OverheadChargeCode);
					chargeCodeDictionary.Add(Constants.ChargeType.NonAccrual, NonAccrualChargeCode);
					chargeCodeDictionary.Add(Constants.ChargeType.Disbursement, DisbursementChargeCode);
					chargeCodeDictionary.Add(Constants.ChargeType.ManualJobAccrual, MJAChargeCode);
					chargeCodeDictionary.Add(Constants.ChargeType.Margin, MarginChargeCode);
					chargeCodeDictionary.Add(Constants.ChargeType.Revenue, RevenueChargeCode);
				}

				return chargeCodeDictionary;
			}
		}

		internal protected static AccTransactionLines GetLine(BusinessObjectFactory factory, bool hasJob, string lineType, bool hasCharge = false, bool hasHeader = true, decimal localAmount = 0M, string transactionNumber = "INV123", OrgHeader organization = null)
		{
			AccTransactionHeader header = null;
			if (hasHeader && (lineType == TransactionLineTypes.Cost || lineType == TransactionLineTypes.Revenue || lineType == TransactionLineTypes.UnapprovedCost || lineType == TransactionTypes.GLStandardJournal))
			{
				header = factory.New<AccTransactionHeader>();
				switch (lineType)
				{
					case TransactionLineTypes.Cost:
						header.AH_Ledger = LedgerTypes.AccountsPayable;
						header.AH_TransactionType = TransactionTypes.Invoice;
						break;
					case TransactionLineTypes.Revenue:
						header.AH_Ledger = LedgerTypes.AccountsReceivable;
						header.AH_TransactionType = TransactionTypes.Invoice;
						break;
					case TransactionLineTypes.UnapprovedCost:
						header.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
						header.AH_TransactionType = TransactionTypes.UAInvoice;
						break;
					case TransactionTypes.GLStandardJournal:
						header.AH_Ledger = LedgerTypes.General;
						header.AH_TransactionType = TransactionTypes.GLStandardJournal;
						break;
				}
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_GE = GlbDepartment.CurrentDepartment.PK;
				header.AH_InvoiceDate = ZDateTime.Today;
				header.AH_TransactionNum = transactionNumber;
				header.AH_InvoiceAmount = localAmount;
				header.AH_OSTotal = localAmount;
				header.AH_OutstandingAmount = localAmount;

				if (organization != null)
				{
					header.AH_OH = organization.PK;
				}
			}
			AccTransactionLines line = factory.New<AccTransactionLines>();
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AC = Env.Registry.FreightChargeCode;
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			line.AL_ExchangeRate = 1m;
			line.AL_LineAmount = localAmount;
			line.AL_OSAmount = line.AL_LineAmount;

			if (header != null)
			{
				line.AL_AH = header.PK;
			}
			line.AL_LineType = lineType;

			if (hasJob)
			{
				var job = factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_JobNum = job.PK.ToString().Substring(1, JobHeaderSchema.JH_JobNum.MaxLength).ToUpper(); // To avoid duplicate Job Numbers
				line.AL_JH = job.PK;

				if (hasCharge)
				{
					GetLineLinkedCharge(line);
				}
			}

			line.AL_AG = line.ChargeCode.AC_AG_CostAccount;

			return line;
		}

		internal static JobCharge GetLineLinkedCharge(AccTransactionLines line, JobCharge chargeToLink = null)
		{
			var charge = GetChargeToLink(line, chargeToLink);
			if (line.TransactionHeader != null && line.TransactionHeader.AH_TransactionType == TransactionTypes.Journal)
			{
				charge.JR_AL_CFXLine = line.PK;
			}
			else
			{
				if (line.AL_LineType == TransactionLineTypes.Cost || line.AL_LineType == TransactionLineTypes.UnapprovedCost || line.AL_LineType == TransactionLineTypes.Accrual)
				{
					charge.JR_AL_APLine = line.PK;
					charge.SetAmountsFromLinkedLinesForTests();
				}
				if (line.AL_LineType == TransactionLineTypes.Revenue || line.AL_LineType == TransactionLineTypes.WIP)
				{
					charge.JR_AL_ARLine = line.PK;
					charge.SetAmountsFromLinkedLinesForTests();
				}
			}
			return charge;
		}

		internal static JobCharge GetChargeToLink(AccTransactionLines line, JobCharge chargeToLink = null)
		{
			var charge = chargeToLink ?? line.Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_GB = line.AL_GB;
			charge.JR_GE = line.AL_GE;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			return charge;
		}

		internal static void SetConsolCostLinkedCharge(JobCharge charge)
		{
			charge.JR_E6 = ZGuid.NewZGuid();
			charge.Factory.New(ObjectFactory.GetType<IJobConsolCost>(), charge.JR_E6.ToGuid());
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;

		#endregion
	}
}
