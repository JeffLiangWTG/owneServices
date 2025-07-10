using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccTaxRate;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionLines))]
	public class AccTransactionLinesTestCase : EnterpriseBusinessObjectTestCase
	{
		public void TestWIPACROrganizationNotEqualToRelatedChargeForNewLine_Accrual()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Accrual;
			line.AL_JH = ZGuid.NewZGuid();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_OH_CostAccount = ZGuid.NewZGuid();
			charge.JR_AL_APLine = line.PK;

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			line.AL_OH = ZGuid.NewZGuid();
			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {charge.JR_OH_CostAccount}, Line Organization: {line.AL_OH}, Old Organization: {ZGuid.Empty}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line and cnahge have different orgs", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			line.AL_OH = line.AL_OH;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line sets the same org as before", collectedInfo, infoNotCollected);

			line.AL_OH = charge.JR_OH_CostAccount;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line sets the same org as on charge", collectedInfo, infoNotCollected);
		}

		public void TestWIPACROrganizationNotEqualToRelatedChargeForNewLine_WIP()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_JH = ZGuid.NewZGuid();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_OH_SellAccount = ZGuid.NewZGuid();
			charge.JR_AL_ARLine = line.PK;

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			line.AL_OH = ZGuid.NewZGuid();
			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {charge.JR_OH_SellAccount}, Line Organization: {line.AL_OH}, Old Organization: {ZGuid.Empty}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line and charge have different orgs", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			line.AL_OH = line.AL_OH;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line sets the same org as before", collectedInfo, infoNotCollected);

			line.AL_OH = charge.JR_OH_SellAccount;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line sets the same org as on charge", collectedInfo, infoNotCollected);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var line = (AccTransactionLines)GetNewBusinessObjectForDeleteTest(Factory);
			var lineType = line.AL_LineTypeInfo.OriginalValue;
			if (lineType.IsEmpty
				|| line.AL_LineType == TransactionLineTypes.WIP
				|| line.AL_LineType == TransactionLineTypes.Accrual
				|| line.AL_LineType == TransactionLineTypes.Revenue
				|| (line.AL_LineType == TransactionLineTypes.Cost))
			{
				Assert("Should not be deleted", true);
			}
			else
			{
				base.TestSaveAndDeleteBusinessObject();
			}
		}

		public void TestAL_TotalTaxExpenseAmount()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AH = transaction.PK;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var taxExpenses = transactionLine.GetTaxExpenses();
			Assert(taxExpenses.IsNullOrEmpty());
			AssertEquals(0m, transactionLine.AL_TotalTaxExpenseAmount);
			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessorCrossAssembly>();
			ObjectFactory.Substitute("ITaxProcessor", taxProcessorMock.Object);

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, transactionLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(2.05m)), (ZDate.Today.AddDays(-2), new ZDecimal(4.02m)) });
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Never);

			taxExpenses = transactionLine.GetTaxExpenses();
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Once);
			AssertContainsExactElementsInAnyOrder<(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)>(new[] { (ZDate.Today, new ZDecimal(2.05m)), (ZDate.Today.AddDays(-2), new ZDecimal(4.02m)) }, taxExpenses);
			AssertEquals(6.07m, transactionLine.AL_TotalTaxExpenseAmount);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Exactly(2));
			Assert(transactionLine.IsTaxExpense);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Exactly(3));

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, transactionLine.PK)).Returns(Array.Empty<(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)>());
			taxExpenses = transactionLine.GetTaxExpenses();
			Assert(taxExpenses.IsNullOrEmpty());
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Exactly(4));
			AssertEquals(0m, transactionLine.AL_TotalTaxExpenseAmount);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Exactly(5));
			Assert(!transactionLine.IsTaxExpense);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Exactly(6));

			transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AH = transaction.PK;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, transactionLine.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(2.05m)), (ZDate.Today.AddDays(-2), new ZDecimal(4.02m)) });
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Never);
			taxExpenses = transactionLine.GetTaxExpenses();
			Assert(taxExpenses.IsNullOrEmpty());
			AssertEquals(0m, transactionLine.AL_TotalTaxExpenseAmount);
			Assert(!transactionLine.IsTaxExpense);
			taxProcessorMock.Verify(t => t.GetTaxExpenses(Factory, transactionLine.PK), Times.Never);
		}

		public void TestConcurrencyPolicyForAL_RevRecognitionType()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AH = transaction.PK;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var transactionLineInFactory1 = factory1.Load<AccTransactionLines>(transactionLine.PK);
			var transactionLineInFactory2 = factory2.Load<AccTransactionLines>(transactionLine.PK);

			transactionLineInFactory1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate;
			transactionLineInFactory2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedDepartureDate;

			factory1.Save();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory2.Save();
				Fail("Expected a ZSaveConcurrencyException for AL_RevRecognitionType strict concurrency policy.");
			}
			catch (ZSaveConcurrencyException e)
			{
				AssertContains(AutoAccTransactionLines.Schema.AL_RevRecognitionType, e.Message);
			}
		}

		public void TestAL_TaxDateReadOnly()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			line.AL_AT = ZGuid.Empty;
			Assert(line.AL_TaxDateInfo.ReadOnly);

			line.AL_AT = ZGuid.Invalid;
			Assert(line.AL_TaxDateInfo.ReadOnly);

			line.AL_AT = ZGuid.Missing;
			Assert(line.AL_TaxDateInfo.ReadOnly);

			line.AL_AT = ZGuid.BrettsGuid;
			Assert(!line.AL_TaxDateInfo.ReadOnly);
		}

		public void TestSetTaxDateSafe()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			line.SetTaxDateSafe(ZDate.Today);
			AssertEquals(ZDate.Empty, line.AL_TaxDate);

			line.SetTaxDateSafe(ZDate.Invalid);
			AssertEquals(ZDate.Empty, line.AL_TaxDate);

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			line.AL_AT = taxRate.PK;
			line.SetTaxDateSafe(ZDate.Today);
			AssertEquals(ZDate.Today, line.AL_TaxDate);

			line.SetTaxDateSafe(ZDate.Invalid);
			AssertEquals(ZDate.Today, line.AL_TaxDate);

			line.SetTaxDateSafe(ZDate.BrettsBirthday);
			AssertEquals(ZDate.BrettsBirthday, line.AL_TaxDate);

			line.SetTaxDateSafe(ZDate.Empty);
			AssertEquals(ZDate.Today, line.AL_TaxDate);
		}

		public void TestAL_ATSetsAL_TaxDate()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var taxRate2 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);

			var line = GetNewBusinessObject() as AccTransactionLines;
			var expectedDate = ZDate.BrettsBirthday;
			line.AL_TaxDate = expectedDate;
			line.AL_AT = taxRate.PK;
			AssertNotEquals("Precondition: expectedDate", ZDate.Today, expectedDate);
			AssertEquals(expectedDate, line.AL_TaxDate);

			line.AL_AT = taxRate2.PK;
			AssertEquals(expectedDate, line.AL_TaxDate);

			line.AL_AT = ZGuid.Empty;
			AssertEquals(ZDate.Empty, line.AL_TaxDate);

			line.AL_AT = taxRate.PK;
			AssertEquals(ZDate.Today, line.AL_TaxDate);

			line.AL_AT = taxRate2.PK;
			AssertEquals(ZDate.Today, line.AL_TaxDate);

			line.AL_AT = ZGuid.Empty;
			AssertEquals(ZDate.Empty, line.AL_TaxDate);
		}

		#region TestAL_TaxRateNumeratorDenominator

		public void TestAL_TaxRateNumeratorDenominator_UsingCache()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(new BusinessObjectFactory() { RefreshEnabled = false });
			var rate = taxRate.SetRate_ForTestOnly(20, 8);
			var extraRate = taxRate.SetExtraRate_ForTestOnly(5, 3);
			taxRate.Factory.Save();

			var line = GetNewBusinessObject() as AccTransactionLines;
			line.AL_TaxDate = ZDate.Today.AddDays(1);
			line.AL_AT = taxRate.PK;
			AssertTaxRates(line, 20, 8, 5, 3);

			rate.ZAT_RateNumerator *= 2;
			rate.ZAT_RateDenominator *= 2;
			rate.ZAT_EndDate = ZDate.BrettsBirthday;
			extraRate.ZAT_RateNumerator *= 2;
			extraRate.ZAT_RateDenominator *= 2;
			extraRate.ZAT_EndDate = ZDate.BrettsBirthday;
			taxRate.Factory.Save();
			line.AL_TaxDate = ZDate.Today.AddDays(-10);
			AssertTaxRates(line, 20, 8, 5, 3);

			line.AL_TaxDate = ZDate.Today.AddMonths(-3);
			AssertTaxRates(line, 20, 8, 5, 3);

			line.AL_TaxDate = ZDate.BrettsBirthday;
			AssertTaxRates(line, 20, 8, 5, 3);
		}

		public void TestAL_TaxRateNumeratorDenominator_Setting()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			AssertTaxRates(line, 0, 1, 0, 1);

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);

			var taxDate = ZDate.Today.AddDays(-1);
			taxRate.SetRate_ForTestOnly(20, 8, taxDate, taxDate);

			taxDate = ZDate.Today;
			taxRate.SetRate_ForTestOnly(10, 2, taxDate, taxDate);

			taxDate = ZDate.Today.AddDays(1);
			taxRate.SetRate_ForTestOnly(11, 3, taxDate, taxDate);

			taxDate = ZDate.Today;
			taxRate.SetExtraRate_ForTestOnly(5, 4, taxDate, taxDate);

			taxDate = ZDate.Today.AddDays(1);
			taxRate.SetExtraRate_ForTestOnly(6, 7, taxDate, taxDate);

			taxDate = ZDate.Today.AddDays(2);
			taxRate.SetExtraRate_ForTestOnly(9, 13, taxDate, taxDate);

			line.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertTaxRates(line, 0, 1, 0, 1);

			line.AL_AT = taxRate.PK;
			AssertTaxRates(line, 11, 3, 6, 7);

			line.AL_TaxDate = ZDate.Today;
			AssertTaxRates(line, 10, 2, 5, 4);

			line.AL_TaxDate = ZDate.Today.AddDays(2);
			AssertTaxRates(line, 0, 1, 9, 13);

			line.AL_TaxDate = ZDate.Today.AddDays(-1);
			AssertTaxRates(line, 20, 8, 0, 1);
		}

		public void TestAL_TaxRateForReverseOrSuspendedTax()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			var taxRateRVS = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRateRVS.AT_Type = AccTaxRate.Types.ReverseRated;
			var taxDate = ZDate.Today;
			taxRateRVS.SetRate_ForTestOnly(11, 3, taxDate, taxDate);
			taxRateRVS.SetExtraRate_ForTestOnly(6, 7, taxDate, taxDate);
			line.AL_AT = taxRateRVS.PK;
			AssertTaxRates(line, 11, 3, 6, 7);
			AssertEquals("RVS tax: " + nameof(AccTransactionLines.AL_TaxRateCalc), taxRateRVS.GetRate(taxDate), line.AL_TaxRateCalc);
			AssertEquals("RVS tax: " + nameof(AccTransactionLines.AL_TaxRateCalc), 0m, line.AL_TaxRateCalc);
			AssertEquals("RVS tax: " + nameof(AccTransactionLines.AL_TaxRateCalc_Raw), 3.6666666666666666666666666667m, line.AL_TaxRateCalc_Raw);
			AssertEquals("RVS tax: " + nameof(AccTransactionLines.AL_TaxExtraRateCalc), taxRateRVS.GetExtraRate_ForTestOnly(), line.AL_TaxExtraRateCalc);
			AssertEquals("RVS tax: " + nameof(AccTransactionLines.AL_TaxExtraRateCalc), 0.8571428571428571428571428571m, line.AL_TaxExtraRateCalc);

			var taxRateSUS = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRateSUS.AT_Type = AccTaxRate.Types.Suspended;
			taxRateSUS.SetRate_ForTestOnly(10, 2, taxDate, taxDate);
			taxRateSUS.SetExtraRate_ForTestOnly(5, 4, taxDate, taxDate);
			line.AL_AT = taxRateSUS.PK;
			AssertTaxRates(line, 10, 2, 5, 4);
			AssertEquals("SUS tax: " + nameof(AccTransactionLines.AL_TaxRateCalc), taxRateSUS.GetRate(taxDate), line.AL_TaxRateCalc);
			AssertEquals("SUS tax: " + nameof(AccTransactionLines.AL_TaxRateCalc), 0m, line.AL_TaxRateCalc);
			AssertEquals("SUS tax: " + nameof(AccTransactionLines.AL_TaxRateCalc_Raw), 5m, line.AL_TaxRateCalc_Raw);
			AssertEquals("SUS tax: " + nameof(AccTransactionLines.AL_TaxExtraRateCalc), taxRateSUS.GetExtraRate_ForTestOnly(), line.AL_TaxExtraRateCalc);
			AssertEquals("SUS tax: " + nameof(AccTransactionLines.AL_TaxExtraRateCalc), 1.25m, line.AL_TaxExtraRateCalc);
		}

		static void AssertTaxRates(AccTransactionLines line, ZInt rateNum, ZInt rateDenom, ZInt extraRateNum, ZInt extraRateDenom)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(AccTransactionLines.AL_TaxRateNumerator), rateNum, line.AL_TaxRateNumerator);
				AssertEquals(nameof(AccTransactionLines.AL_TaxRateDenominator), rateDenom, line.AL_TaxRateDenominator);
				AssertEquals(nameof(AccTransactionLines.AL_TaxExtraRateNumerator), extraRateNum, line.AL_TaxExtraRateNumerator);
				AssertEquals(nameof(AccTransactionLines.AL_TaxExtraRateDenominator), extraRateDenom, line.AL_TaxExtraRateDenominator);
			});
		}

		#endregion

		public void TestAL_TaxRateNumeratorDenominator_Readonly()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			Assert(nameof(AccTransactionLines.AL_TaxRateNumerator), line.AL_TaxRateNumeratorInfo.ReadOnly);
			Assert(nameof(AccTransactionLines.AL_TaxRateDenominator), line.AL_TaxRateDenominatorInfo.ReadOnly);
			Assert(nameof(AccTransactionLines.AL_TaxExtraRateNumerator), line.AL_TaxExtraRateNumeratorInfo.ReadOnly);
			Assert(nameof(AccTransactionLines.AL_TaxExtraRateDenominator), line.AL_TaxExtraRateDenominatorInfo.ReadOnly);
		}

		public void TestAL_TaxRateCalc()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc), 0m, line.AL_TaxRateCalc);
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc_Raw), line.AL_TaxRateCalc, line.AL_TaxRateCalc_Raw);

			line.AL_TaxRateNumerator = 10;
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc), 0m, line.AL_TaxRateCalc);
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc_Raw), 10m, line.AL_TaxRateCalc_Raw);

			line.AL_AT = Factory.New<AccTaxRate>().PK;
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc), 0m, line.AL_TaxRateCalc);
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc_Raw), line.AL_TaxRateCalc, line.AL_TaxRateCalc_Raw);

			line.AL_TaxRateNumerator = 10;
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc), 10m, line.AL_TaxRateCalc);
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc_Raw), line.AL_TaxRateCalc, line.AL_TaxRateCalc_Raw);

			line.AL_TaxRateDenominator = 7;
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc), 1.4285714285714285714285714286m, line.AL_TaxRateCalc);
			AssertEquals(nameof(AccTransactionLines.AL_TaxRateCalc_Raw), line.AL_TaxRateCalc, line.AL_TaxRateCalc_Raw);
		}

		public void TestAL_TaxExtraRateCalc()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			AssertEquals(0m, line.AL_TaxExtraRateCalc);

			line.AL_TaxExtraRateNumerator = 10;
			AssertEquals(0m, line.AL_TaxExtraRateCalc);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			line.AL_AT = taxRate.PK;
			line.AL_TaxExtraRateNumerator = 10;
			AssertEquals(10m, line.AL_TaxExtraRateCalc);

			line.AL_TaxExtraRateDenominator = 7;
			AssertEquals(1.4285714285714285714285714286m, line.AL_TaxExtraRateCalc);

			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			AssertEquals(0m, line.AL_TaxExtraRateCalc);

			line.AL_TaxRateNumerator = 100;
			AssertEquals(142.85714285714285714285714286m, line.AL_TaxExtraRateCalc);
		}

		public void TestGetEffectiveExtraRate()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			line.AL_AT = taxRate.PK;
			line.AL_TaxRateNumerator = 10;
			line.AL_TaxExtraRateNumerator = 4;
			taxRate.AT_Type = Types.Rated;
			taxRate.AT_ExtraTaxRateType = ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			AssertEquals(0.4m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.ChinaInputVATClaimed;
			AssertEquals(4.1666666666666666666666666700m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.VATRetention;
			AssertEquals(-4m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.VATRetentionFraction;
			AssertEquals(-40m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.VATRemittedByCustomer;
			AssertEquals(-10m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.StateGST;
			taxRate.AT_Type = Types.ReverseRated;
			AssertEquals(0m, line.GetEffectiveExtraRate());

			taxRate.AT_Type = Types.Exempt;
			AssertEquals(4m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			AssertEquals(4m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;
			AssertEquals(4m, line.GetEffectiveExtraRate());

			taxRate.AT_ExtraTaxRateType = ExtraTypes.QuebecQST;
			AssertEquals(4.4m, line.GetEffectiveExtraRate());
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccTransactionLines()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;

			var localList = new List<string>
				{
					nameof(line.AL_LineAmount),
					nameof(line.AL_GSTVAT),
					nameof(line.AL_WithholdingTax)
				};

			var osList = new List<string>
				{
					nameof(line.AL_OSAmount)
				};

			var exList = new List<string>
				{
					nameof(line.AL_ExchangeRate)
				};

			var rateList = new List<string>
				{
					nameof(AccTransactionLines.AL_TaxRateCalc),
					nameof(AccTransactionLines.AL_TaxRateCalc_Raw),
					nameof(AccTransactionLines.AL_TaxExtraRateCalc)
				};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckLocalCurrency(localList, nameof(line.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(line.CurrencyDecimals), nameof(line.AL_RX_NKTransactionCurrency), line);
			tester.CheckExchangeRate(exList, nameof(line.ExchangeRateDecimals));
			tester.CheckSetter(rateList, nameof(line.TaxRateDecimals));
		}

		public void TestTransactionLineTypeIsCompatibleWithTransactionHeader()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			AssertNotNull(line);
			if (!line.GetType().IsAssignableFrom(typeof(AccTransactionLines)))
			{
				Assert(!line.AL_LineType.IsEmpty);
				var compatibleHeaders = new AccTransactionLinesCompatibilityMatrixTestHelper().LineHeaderCompatibilityMatrix[line.AL_LineType];
				AssertNotNull(compatibleHeaders);
				if (line.AL_LineType != TransactionLineTypes.WIP && line.AL_LineType != TransactionLineTypes.Accrual)
				{
					Assert(compatibleHeaders.Count > 0);
				}
			}
		}

		public void TestAL_ExchangeRate_WhenLocalCompanyCurrencyEqualsJobChargeCostCurrency_ShouldReportDevError()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			var invoiceAP = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceAP.AH_Ledger = LedgerTypes.AccountsPayable;
			invoiceAP.AH_TransactionType = TransactionTypes.Invoice;
			invoiceAP.AH_PostedToEFT = false;
			invoiceAP.AH_ExchangeRate = 1m;
			invoiceAP.AH_RX_NKTransactionCurrency = "AUD";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRateNumerator_ForTestOnly(0);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Cost;
			charge.JR_AL_APLine = line.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_AH = invoiceAP.PK;
			line.AL_JH = job.PK;

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineExRateShouldBeOneWhenLocalCompanyCurrencyEqualsLineCurrency);

			line.AL_ExchangeRate = 1.1m;
			charge.JR_OSCostExRate = line.AL_ExchangeRate;

			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("AUD", line.AL_RX_NKTransactionCurrency);

			line.AL_ExchangeRate = 1.2m;
			charge.JR_OSCostExRate = line.AL_ExchangeRate;

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException<JobCharge>>(() => Factory.Save());
			AssertContains("AL_ExchangeRate = 1.1, AL_ExchangeRate Old Value = 0, AL_RX_NKTransactionCurrency = AUD", ex.DeveloperErrorMessage);
			AssertContains("AL_ExchangeRate = 1.2, AL_ExchangeRate Old Value = 1.1, AL_RX_NKTransactionCurrency = AUD", ex.DeveloperErrorMessage);
			AssertContains("AH_RX_NKTransactionCurrency = AUD, AH_PostedToEFT = N, AH_ExchangeRate = 1", ex.DeveloperErrorMessage);
			ErrorReporter.Instance.Clear();
		}

		public void TestTransactionCurrency()
		{
			var line = GetNewBusinessObject() as AccTransactionLines;
			line.AL_RX_NKTransactionCurrency = ZString.Empty;
			AssertNull(line.TransactionCurrency);
			var expectedCode = "AUD";
			line.AL_RX_NKTransactionCurrency = expectedCode;
			AssertEquals(expectedCode, line.TransactionCurrency.RX_Code);

			expectedCode = "USD";
			line.AL_RX_NKTransactionCurrency = expectedCode;
			AssertEquals(expectedCode, line.TransactionCurrency.RX_Code);
			BusinessObjectFactory.StartLogging();
			try
			{
				Action<int, Action> assertLoads = (loadCount, loadData) =>
				{
					var prevLoads = BusinessObjectFactory.DebugLogCount;
					loadData();
					AssertEquals(loadCount, BusinessObjectFactory.DebugLogCount - prevLoads);
				};
				assertLoads(0, () => AssertEquals(expectedCode, line.TransactionCurrency.RX_Code));
				assertLoads(0, () => AssertEquals("Cached value is used.", expectedCode, line.TransactionCurrency.RX_Code));
				assertLoads(1, () => AssertEquals("Test validity check: factory.load for the same code still increase count.", expectedCode, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, line.AL_RX_NKTransactionCurrency).RX_Code));
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}

			line.AL_RX_NKTransactionCurrency = "XXX";
			AssertNull(line.TransactionCurrency);
		}

		public void TestGLPosted()
		{
			AccTransactionLines testLine = GetNewBusinessObject() as AccTransactionLines;

			((INeedRow)testLine).Row[AccTransactionLines.Schema.AL_PostToGL] = "M";
			((INeedRow)testLine).Row[AccTransactionLines.Schema.AL_ReverseToGL] = "M";
			AssertEquals("M", testLine.AL_PostToGL);
			AssertEquals("M", testLine.AL_ReverseToGL);

			((INeedRow)testLine).Row[AccTransactionLines.Schema.AL_PostToGL] = "Y";
			((INeedRow)testLine).Row[AccTransactionLines.Schema.AL_ReverseToGL] = "Y";
			AssertEquals("Y", testLine.AL_PostToGL);
			AssertEquals("Y", testLine.AL_ReverseToGL);

			((INeedRow)testLine).Row[AccTransactionLines.Schema.AL_PostToGL] = "N";
			((INeedRow)testLine).Row[AccTransactionLines.Schema.AL_ReverseToGL] = "N";
			AssertEquals("N", testLine.AL_PostToGL);
			AssertEquals("N", testLine.AL_ReverseToGL);
		}

		#region TestJobNumber

		public void TestJobNumber()
		{
			AccTransactionLines testLine = Factory.New<AccTransactionLines>();

			AssertEquals("Job should be null initially", null, testLine.Job);
			AssertEquals("JobNumber for null Job should be empty string", "", testLine.JobNumber);

			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_JobNum = "S000010001";
			// JobHeader is not valid with empty JH_Status.
			testJob.JH_Status = JobHeaderStatus.Working.Code;
			testLine.AL_JH = testJob.PK;

			AssertEquals("Job does not match expected TestJob", testJob, testLine.Job);
			AssertEquals("JobNumber does not match expected TestJob number", testJob.JH_JobNum, testLine.JobNumber);
		}

		#endregion TestJobNumber

		#region TestOnSavingDateDifference

		public virtual void TestOnSavingDateDifference()
		{
			try
			{
				ErrorReporter.Clear();

				AccTransactionLines testLine = GetNewBusinessObject() as AccTransactionLines;
				testLine.AL_GB = GlbBranch.CurrentBranch.PK;
				testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
				testLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
				testLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

				AccTransactionLines testLine2 = GetNewBusinessObject() as AccTransactionLines;
				testLine2.AL_GB = GlbBranch.CurrentBranch.PK;
				testLine2.AL_GE = GlbDepartment.CurrentDepartment.PK;
				testLine2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
				testLine2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
				testLine.AL_ReverseDate = ZDateTime.Empty;
				testLine2.AL_ReverseDate = ZDateTime.Empty;
				Factory.Save();

				AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);

				testLine.AL_ReverseDate = new ZDateTime(2000, 1, 1);
				testLine2.AL_ReverseDate = new ZDateTime(2000, 1, 1);
				Factory.Save();

				AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();

				ZDateTime newValue = ZDateTime.Empty;
				testLine.AL_ReverseDate = newValue;
				Factory.Save();
				AssertEquals("The error created here should not be reported until the object is saved", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();

				newValue = new ZDateTime(2007, 6, 22);
				testLine.AL_ReverseDate = newValue;
				testLine.AL_Desc += "this is for saving";
				Factory.Save();

				string errorMessage = testLine.ResetReverseDateErrorMessageTrap(newValue);
				AssertEquals("Should report all new errors since last save", 1, ErrorReporter.TotalErrorCount);
				AssertResetReverseDateErrorMessageHasCorrectData(testLine, errorMessage);
				var wipMessage = ErrorReporter.LastMessageReported;
				AssertEquals("Error message is reported for WIP line", true, ErrorReporter.LastMessageReported.Contains("WIP"));
				ErrorReporter.Clear();

				testLine2.AL_ReverseDate = newValue;
				testLine2.AL_Desc += "this is for saving";
				Factory.Save();

				string errorMessage2 = testLine.ResetReverseDateErrorMessageTrap(newValue);
				AssertResetReverseDateErrorMessageHasCorrectData(testLine2, errorMessage2);
				AssertNotEquals("Error message is reported again for ACR line", wipMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				if (testLine.IsSavedByFactory)
				{
					newValue = testLine.AL_ReverseDate.AddDays(1);
					testLine.AL_ReverseDate = newValue;
					testLine.AL_Desc += "line needs to have changes to save and report";
					string errorMessage3 = testLine.ResetReverseDateErrorMessageTrap(newValue);
					Factory.Save();
					AssertEquals("Has error", errorMessage3, ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}

				Factory.RefreshEnabled = false;
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var lineInNewFactory = newFactory.Load<AccTransactionLines>(testLine.PK);
				ConcurrencyInfo.SetConcurrencyPolicy(testLine, nameof(AccTransactionLines.AL_Desc), ConcurrencyPolicy.Strict);
				ConcurrencyInfo.SetConcurrencyPolicy(lineInNewFactory, nameof(AccTransactionLines.AL_Desc), ConcurrencyPolicy.Strict);
				lineInNewFactory.AL_Desc += "line needs to have changes to save and report";
				testLine.AL_Desc += "line needs to have changes to save and report";
				testLine.AL_ReverseDate = new ZDateTime(2009, 2, 25);
				newFactory.Save();
				try
				{
					Factory.Save();
					Fail("Should fail concurrency check");
				}
				catch (ZSaveConcurrencyException ex)
				{
					AssertContains("Save Aborted Due to Concurrency Check", ex.Message);
				}
				AssertEquals("Should not report if concurrency check failed", 0, ErrorReporter.TotalErrorCount);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		protected void AssertResetReverseDateErrorMessageHasCorrectData(AccTransactionLines line, string message)
		{
			var charge = line.LoadRelatedJobCharge();
			if (charge?.Job != null)
			{
				AssertContains(charge.JR_InvoiceType, message);
				if (charge.Job.Parent is IJobInvoicingPlugIn plugIn)
				{
					AssertContains(plugIn.GetType().Name, message);
					AssertContains(plugIn.InvoicingSupporter?.ConsumerType?.Code, message);
				}
			}
			AssertContains(charge?.GetJobChargeInfo(), message);
			AssertContains("at Enterprise.MasterFiles.Business.AccTransactionLines.ResetReverseDateErrorMessageTrap(ZDateTime newValue)", message);
		}

		#endregion TestOnSavingDateDifference

		public void TestSavingReverseDateDependsOnWipAccrualReversingBusinessContext()
		{
			var originalValue = ZDateTime.Today;
			var newValue = ZDateTime.Today.AddDays(-1);
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line.AL_ReverseDate = originalValue;
			Factory.Save();

			line.RemoveContext(BusinessContext.WipAccrualReversing);

			Assert("PreCondition", line.AL_ReverseDate.IsValid);
			Assert("PreCondition", line.AL_ReverseDateInfo.OriginalValue.IsValid);
			Assert("PreCondition", !line.HasContext(BusinessContext.WipAccrualReversing));
			AssertNotEquals("PreCondition", newValue.Date, ((ZDateTime)line.AL_ReverseDateInfo.OriginalValue).Date);

			line.AL_ReverseDate = newValue;
			AssertEquals("Reverse date should not be saved", originalValue, line.AL_ReverseDate);
			ErrorReporter.Clear();

			line.SetContext(BusinessContext.WipAccrualReversing);
			Assert("PreCondition", line.HasContext(BusinessContext.WipAccrualReversing));

			line.AL_ReverseDate = newValue;
			AssertEquals("Reverse date should be saved", newValue, line.AL_ReverseDate);
		}

		#region TestLocalDecimals

		public void TestLocalDecimals()
		{
			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				AccTransactionLines testLine = GetNewBusinessObject() as AccTransactionLines;
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals("Decimals should be 0", 0, testLine.LocalDecimals);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				AssertEquals("Decimals should be 2", 2, testLine.LocalDecimals);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		#endregion

		#region TestCurrencyDecimals

		public void TestCurrencyDecimals()
		{
			AccTransactionLines testLine = GetNewBusinessObject() as AccTransactionLines;
			testLine.AL_RX_NKTransactionCurrency = string.Empty;
			AssertNull(testLine.TransactionCurrency);
			AssertEquals("Should be 2 when no currency specified", 2, testLine.CurrencyDecimals);

			testLine.AL_RX_NKTransactionCurrency = "IDR";
			AssertEquals("Decimals should be 0", 0, testLine.CurrencyDecimals);
			testLine.AL_RX_NKTransactionCurrency = "USD";
			AssertEquals("Decimals should be 2", 2, testLine.CurrencyDecimals);
		}

		#endregion

		#region General Ledger Account defaults from Charge Code

		public void TestGeneralLedgerAccountDefaultsFromChargeCode()
		{
			SetUpGeneralLedgerAccountsAndChargeCodes();

			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Accrual;

			AssertEquals("Pre-condition: cannot default as there's not enough information", ZGuid.Empty, line.AL_AG);

			line.AL_AC = marginChargeCode.PK;

			AssertEquals("ACR GL should be defaulted from the charge code even there is no department set", accrualAccount.PK, line.AL_AG);

			line.AL_GE = Env.CurrentDepartment.PK;

			AssertEquals("ACR GL should be defaulted from the charge code as the line type is ACR", accrualAccount.PK, line.AL_AG);

			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AG = ZGuid.Empty;
			line.AL_AC = ZGuid.Empty;

			line.AL_AC = marginChargeCode.PK;

			AssertEquals("Now the line type is cost, expected the CST account to be defaulted", costAccount.PK, line.AL_AG);

			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AG = ZGuid.Empty;
			line.AL_GE = ZGuid.Empty;

			line.AL_GE = Env.CurrentDepartment.PK;

			AssertEquals("Setting the department should also default the most appropriate GL account from the Charge Code", revenueAccount.PK, line.AL_AG);

			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_AG = ZGuid.Empty;
			line.AL_AC = ZGuid.Empty;

			line.AL_AC = commentChargeCode.PK;

			AssertEquals("No GL Accounts can be defaulted from a comment charge code", ZGuid.Empty, line.AL_AG);

			line.AL_AC = marginChargeCode.PK;

			AssertEquals(wipAccount.PK, line.AL_AG);
		}

		void SetUpGeneralLedgerAccountsAndChargeCodes()
		{
			var factory = new BusinessObjectFactory();
			accrualAccount = factory.NewWithValidTestData<AccGLHeader>();
			costAccount = factory.NewWithValidTestData<AccGLHeader>();
			revenueAccount = factory.NewWithValidTestData<AccGLHeader>();
			wipAccount = factory.NewWithValidTestData<AccGLHeader>();
			factory.Save();

			marginChargeCode = factory.NewWithValidTestData<AccChargeCode>();
			marginChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			marginChargeCode.AC_Code = "MRGALLGL";
			marginChargeCode.AC_AG_AccrualAccount = accrualAccount.PK;
			marginChargeCode.AC_AG_CostAccount = costAccount.PK;
			marginChargeCode.AC_AG_RevenueAccount = revenueAccount.PK;
			marginChargeCode.AC_AG_WIPAccount = wipAccount.PK;

			commentChargeCode = factory.NewWithValidTestData<AccChargeCode>();
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			commentChargeCode.AC_Code = "CMTNOGL";
			factory.Save();
		}

		AccGLHeader accrualAccount;
		AccGLHeader costAccount;
		AccGLHeader revenueAccount;
		AccGLHeader wipAccount;
		AccChargeCode marginChargeCode;
		AccChargeCode commentChargeCode;

		public void TestGLAccountFromChargeCode()
		{
			SetUpGeneralLedgerAccountsAndChargeCodes();

			var factory = marginChargeCode.Factory;

			var header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			var line = factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AH = header.PK;
			line.FillWithValidTestData();

			AssertEquals("Pre-condition: cannot default as there's not enough information", ZGuid.Empty, line.AL_AG);

			line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			var department2 = factory.NewWithValidTestData<GlbDepartment>();
			var department3 = factory.NewWithValidTestData<GlbDepartment>();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.OB_IsDebtor = true;

			var glPostingOverride1 = marginChargeCode.GLPostingOverrides.AddNew();
			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			glPostingOverride1.Y1_GE = department1.PK;
			glPostingOverride1.Y1_AG_CST = factory.NewWithValidTestData<AccGLHeader>().PK;

			var glPostingOverride2 = marginChargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.ThirdParty;
			glPostingOverride2.Y1_GE = department2.PK;
			glPostingOverride2.Y1_AG_CST = factory.NewWithValidTestData<AccGLHeader>().PK;

			var glPostingOverride3 = marginChargeCode.GLPostingOverrides.AddNew();
			glPostingOverride3.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			glPostingOverride3.Y1_AG_CST = factory.NewWithValidTestData<AccGLHeader>().PK;

			var glPostingOverride4 = marginChargeCode.GLPostingOverrides.AddNew();
			glPostingOverride4.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			glPostingOverride4.Y1_GE = department2.PK;
			glPostingOverride4.Y1_AG_CST = factory.NewWithValidTestData<AccGLHeader>().PK;

			factory.Save();

			line.AL_AC = marginChargeCode.PK;
			line.AL_OH = orgHeader.PK;

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";
			line.AL_GE = department2.PK;
			AssertEquals(glPostingOverride4.Y1_AG_CST, line.AL_AG);

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";
			line.AL_GE = department1.PK;
			AssertEquals(glPostingOverride3.Y1_AG_CST, line.AL_AG);

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";
			line.AL_GE = ZGuid.Empty;
			AssertEquals(glPostingOverride3.Y1_AG_CST, line.AL_AG);

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			line.AL_GE = department2.PK;
			AssertEquals(glPostingOverride2.Y1_AG_CST, line.AL_AG);

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			line.AL_GE = department1.PK;
			AssertEquals(glPostingOverride1.Y1_AG_CST, line.AL_AG);

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			line.AL_GE = department3.PK;
			AssertEquals(costAccount.PK, line.AL_AG);

			line.AL_AG = ZGuid.Empty;
			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			line.AL_GE = ZGuid.Empty;
			AssertEquals(costAccount.PK, line.AL_AG);
		}

		#endregion

		public void TestCriticalValidationSupport()
		{
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			Assert(((ISupportCriticalValidation)line).CriticalValidation is AccTransactionLinesCriticalValidation);
		}

		public void TestHaveConstructorStackTrace()
		{
			IHaveConstructorStackTrace hasTrace = BusinessObject as IHaveConstructorStackTrace;
			AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);

			AssertNull("Should be no ConstructorStackTrace by default", hasTrace.ConstructorStackTrace);

			StackTrace trace = new StackTrace();
			hasTrace.ConstructorStackTrace = trace;

			AssertEquals("Should be assigned StackTrace", trace, hasTrace.ConstructorStackTrace);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				hasTrace = this.GetNewBusinessObject() as IHaveConstructorStackTrace;
				AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);
				AssertNotNull("Should have ConstructorStackTrace", hasTrace.ConstructorStackTrace);
				AssertContains("Trace should be as expected", trace.ToString(), hasTrace.ConstructorStackTrace.ToString());
			}
		}

		public void TestResetWIPReverseDateErrorMessage()
		{
			AccTransactionLines wIPLine = (AccTransactionLines)Factory.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			wIPLine.AL_GB = GlbBranch.CurrentBranch.PK;
			wIPLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			wIPLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			wIPLine.AL_ReverseDate = new ZDateTime(2006, 5, 21);
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "1010101010";
			wIPLine.AL_AG = glHeader.PK;

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job.FillWithValidTestData();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "1234";
			// JobHeader is not valid with empty JH_Status.
			job.JH_Status = JobHeaderStatus.Working.Code;
			wIPLine.AL_JH = job.PK;
			Factory.Save();

			JobCharge charge = (JobCharge)Factory.New(typeof(JobCharge), new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			AccChargeCode chargeCode = (AccChargeCode)Factory.New(typeof(AccChargeCode), new Guid("dfc0eded-f8dc-4650-9066-a4e8a6ba33a0"));
			chargeCode.FillWithValidTestData();
			chargeCode.AC_Code = "FRTZ";
			charge.JR_JH = wIPLine.AL_JH;
			charge.JR_GB = wIPLine.AL_GB;
			charge.JR_GE = wIPLine.AL_GE;
			charge.JR_AL_ARLine = wIPLine.PK;
			charge.JR_AC = chargeCode.PK;

			charge.SetAmountsFromLinkedLinesForTests();

			ErrorReporter.Clear();
			wIPLine.AL_ReverseDate = new ZDateTime(2007, 6, 22);
			wIPLine.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertEquals("No changes to AL_ReverseDate", new ZDateTime(2006, 5, 21), wIPLine.AL_ReverseDate);
			AssertEquals("DeveloperException should been reported, with WIP being reversed again in saving", 1, ErrorReporter.TotalErrorCount);
			string errorMessage = ErrorReporter.LastMessageReported;
			ErrorReporter.Clear();

			AssertContains("Error message has line and job charge info", GetWIP_ACR_ExpectedErrorMessage(TransactionLineTypes.WIP), errorMessage);
		}

		public void TestResetACRReverseDateErrorMessage()
		{
			AccTransactionLines aCRLine = (AccTransactionLines)Factory.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			aCRLine.AL_GB = GlbBranch.CurrentBranch.PK;
			aCRLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			aCRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			aCRLine.AL_ReverseDate = new ZDateTime(2006, 5, 21);
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "1010101010";
			aCRLine.AL_AG = glHeader.PK;

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job.FillWithValidTestData();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "1234";
			// JobHeader is not valid with empty JH_Status.
			job.JH_Status = JobHeaderStatus.Working.Code;
			aCRLine.AL_JH = job.PK;
			Factory.Save();

			JobCharge charge = (JobCharge)Factory.New(typeof(JobCharge), new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			AccChargeCode chargeCode = (AccChargeCode)Factory.New(typeof(AccChargeCode), new Guid("dfc0eded-f8dc-4650-9066-a4e8a6ba33a0"));
			chargeCode.FillWithValidTestData();
			chargeCode.AC_Code = "FRTZ";
			charge.JR_JH = aCRLine.AL_JH;
			charge.JR_GB = aCRLine.AL_GB;
			charge.JR_GE = aCRLine.AL_GE;
			charge.JR_AL_APLine = aCRLine.PK;
			charge.JR_AC = chargeCode.PK;

			charge.SetAmountsFromLinkedLinesForTests();

			ErrorReporter.Clear();
			aCRLine.AL_ReverseDate = new ZDateTime(2007, 6, 22);
			aCRLine.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertEquals("No changes to AL_ReverseDate", new ZDateTime(2006, 5, 21), aCRLine.AL_ReverseDate);
			AssertEquals("DeveloperException should been reported, with ACR being reversed again in saving", 1, ErrorReporter.TotalErrorCount);
			string errorMessage = ErrorReporter.LastMessageReported;
			ErrorReporter.Clear();

			AssertContains("Error message has line and job charge info", GetWIP_ACR_ExpectedErrorMessage(TransactionLineTypes.Accrual), errorMessage);
		}

		public void TestResetREVReverseDateErrorMessage()
		{
			var rEVLine = Factory.NewWithPrimaryKey<AccTransactionLines>(new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "1010101010";
			rEVLine.AL_AG = glHeader.PK;
			rEVLine.AL_GB = GlbBranch.CurrentBranch.PK;
			rEVLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			rEVLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			rEVLine.AL_ReverseDate = new ZDateTime(2006, 5, 21);

			var header = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("889092c1-03f7-45de-b334-3a486168bac3"));
			header.AH_TransactionNum = "00001001";
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_GE = GlbDepartment.CurrentDepartment.PK;
			header.AH_InvoiceDate = new ZDateTime(2007, 6, 22);
			// AccTransactionHeader is not valid with empty AH_TransactionType.
			header.AH_TransactionType = TransactionTypes.Invoice;
			// AccTransactionHeader is not valid with empty AH_Ledger.
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			rEVLine.AL_AH = header.PK;

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job.FillWithValidTestData();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "1234";
			// JobHeader is not valid with empty JH_Status.
			job.JH_Status = JobHeaderStatus.Working.Code;
			rEVLine.AL_JH = job.PK;

			var charge = Factory.NewWithPrimaryKey<JobCharge>(new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			var chargeCode = Factory.NewWithPrimaryKey<AccChargeCode>(new Guid("dfc0eded-f8dc-4650-9066-a4e8a6ba33a0"));
			chargeCode.FillWithValidTestData();
			chargeCode.AC_Code = "FRTZ";
			charge.JR_JH = rEVLine.AL_JH;
			charge.JR_GB = rEVLine.AL_GB;
			charge.JR_GE = rEVLine.AL_GE;
			charge.JR_AL_ARLine = rEVLine.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellExRate = 0;

			Factory.Save();

			charge.SetAmountsFromLinkedLinesForTests();

			ErrorReporter.Clear();
			rEVLine.AL_ReverseDate = new ZDateTime(2007, 6, 22);
			rEVLine.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertEquals("No changes to AL_ReverseDate", new ZDateTime(2006, 5, 21), rEVLine.AL_ReverseDate);
			AssertEquals("DeveloperException should been reported", 1, ErrorReporter.TotalErrorCount);
			string errorMessage = ErrorReporter.LastMessageReported;
			ErrorReporter.Clear();

			AssertContains("Error message has line and job charge info", GetREV_CST_ExpectedErrorMessage(TransactionLineTypes.Revenue), errorMessage);
		}

		public void TestResetCSTReverseDateErrorMessage()
		{
			var cSTLine = Factory.NewWithPrimaryKey<AccTransactionLines>(new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			cSTLine.AL_GB = GlbBranch.CurrentBranch.PK;
			cSTLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			cSTLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			cSTLine.AL_ReverseDate = new ZDateTime(2006, 5, 21);
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "1010101010";
			cSTLine.AL_AG = glHeader.PK;

			var header = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("889092c1-03f7-45de-b334-3a486168bac3"));
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_GE = GlbDepartment.CurrentDepartment.PK;
			header.AH_InvoiceDate = new ZDateTime(2007, 6, 22);
			// AccTransactionHeader is not valid with empty AH_TransactionType.
			header.AH_TransactionType = TransactionTypes.Invoice;
			// AccTransactionHeader is not valid with empty AH_Ledger.
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionNum = "00001000";
			cSTLine.AL_AH = header.PK;

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job.FillWithValidTestData();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "1234";
			// JobHeader is not valid with empty JH_Status.
			job.JH_Status = JobHeaderStatus.Working.Code;
			cSTLine.AL_JH = job.PK;

			var charge = Factory.NewWithPrimaryKey<JobCharge>(new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			var chargeCode = Factory.NewWithPrimaryKey<AccChargeCode>(new Guid("dfc0eded-f8dc-4650-9066-a4e8a6ba33a0"));
			chargeCode.FillWithValidTestData();
			chargeCode.AC_Code = "FRTZ";
			charge.JR_JH = cSTLine.AL_JH;
			charge.JR_GB = cSTLine.AL_GB;
			charge.JR_GE = cSTLine.AL_GE;
			charge.JR_AL_APLine = cSTLine.PK;
			charge.JR_AC = chargeCode.PK;

			Factory.Save();

			charge.SetAmountsFromLinkedLinesForTests();

			ErrorReporter.Clear();
			cSTLine.AL_ReverseDate = new ZDateTime(2007, 6, 22);
			cSTLine.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			AssertEquals("No changes to AL_ReverseDate", new ZDateTime(2006, 5, 21), cSTLine.AL_ReverseDate);
			AssertEquals("DeveloperException should been reported", 1, ErrorReporter.TotalErrorCount);
			string errorMessage = ErrorReporter.LastMessageReported;
			ErrorReporter.Clear();

			AssertContains("Error message has line and job charge info", GetREV_CST_ExpectedErrorMessage(TransactionLineTypes.Cost), errorMessage);
		}

		public void TestResetUAUCTReverseDateNoError()
		{
			var uCTLine = Factory.NewWithPrimaryKey<AccTransactionLines>(new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			uCTLine.AL_GB = GlbBranch.CurrentBranch.PK;
			uCTLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			uCTLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.UnapprovedCost;
			uCTLine.AL_ReverseDate = new ZDateTime(2006, 5, 21);
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "1010101010";
			uCTLine.AL_AG = glHeader.PK;

			var header = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("889092c1-03f7-45de-b334-3a486168bac3"));
			header.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_GE = GlbDepartment.CurrentDepartment.PK;
			header.AH_InvoiceDate = new ZDateTime(2007, 6, 22);
			// AccTransactionHeader is not valid with empty AH_TransactionType.
			header.AH_TransactionType = TransactionTypes.UAInvoice;
			header.AH_TransactionNum = "00001000";
			uCTLine.AL_AH = header.PK;

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job.FillWithValidTestData();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "1234";
			// JobHeader is not valid with empty JH_Status.
			job.JH_Status = JobHeaderStatus.Working.Code;
			uCTLine.AL_JH = job.PK;

			var charge = Factory.NewWithPrimaryKey<JobCharge>(new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			var chargeCode = Factory.NewWithPrimaryKey<AccChargeCode>(new Guid("dfc0eded-f8dc-4650-9066-a4e8a6ba33a0"));
			chargeCode.FillWithValidTestData();
			chargeCode.AC_Code = "FRTZ";
			charge.JR_JH = uCTLine.AL_JH;
			charge.JR_GB = uCTLine.AL_GB;
			charge.JR_GE = uCTLine.AL_GE;
			charge.JR_AL_APLine = uCTLine.PK;
			charge.JR_AC = chargeCode.PK;

			Factory.Save();

			charge.SetAmountsFromLinkedLinesForTests();

			ErrorReporter.Clear();
			uCTLine.AL_ReverseDate = new ZDateTime(2007, 6, 22);
			AssertEquals("AL_ReverseDate was set", new ZDateTime(2007, 6, 22), uCTLine.AL_ReverseDate);
			AssertEquals("DeveloperException should not been reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestSetAL_GB()
		{
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_GB = GlbBranch.CurrentBranch.PK;

			AssertEquals(false, line.AL_GCInfo.HasErrors());
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, line.AL_GC);

			line.AL_GC = ZGuid.Empty;
			Assert(line.AL_GCInfo.HasErrors());
			AssertEquals("Error Message", "Please enter a Company.", line.AL_GCInfo.GetErrors().First().Message);

			line.AL_GC = ZGuid.NewZGuid();
			Assert(line.AL_GCInfo.HasErrors());
			AssertEquals("Error Message", "The Company you entered doesn't match Current Branch.", line.AL_GCInfo.GetErrors().First().Message);
		}

		public virtual void TestRoundAmountToLocalDecimals()
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineAmount = 10.568123656m;
			line.AL_GSTVAT = 2.468123456m;
			line.AL_WithholdingTax = 2.457321m;

			AssertEquals("Precondition", 2, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			AssertEquals("Should be rounded", 10.57m, line.AL_LineAmount);
			AssertEquals("Should be rounded", 2.47m, line.AL_GSTVAT);
			AssertEquals("Should be rounded", 2.46m, line.AL_WithholdingTax);
		}

		string GetWIP_ACR_ExpectedErrorMessage(ZString lineType, bool isPostDate = false)
		{
			bool isSell = lineType == TransactionLineTypes.WIP;

			return string.Format(@"{0} has {1} Date 21-May-06 00:00:00 changed to 22-Jun-07 00:00:00. {0} has the Enterprise.MasterFiles.Business.AccTransactionLines type and PK: 6c1c3709-8498-4ea3-95ba-fee57c41b00c.
{0} other details: Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = {2}, Reverse Date = {3}, Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 084172b9-3656-41d5-b18c-1a73179a8f9b, Organization = , Revenue Recognition Type = , Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = No.",
				lineType,
				isPostDate ? "Post" : "Reverse",
				isPostDate ? "21-May-06 00:00:00" : "",
				isPostDate ? "" : "21-May-06 00:00:00");
		}

		string GetREV_CST_ExpectedErrorMessage(ZString lineType, bool isPostDate = false)
		{
			bool isSell = lineType == TransactionLineTypes.Revenue;

			return string.Format(@"{0} has {1} Date 21-May-06 00:00:00 changed to 22-Jun-07 00:00:00. {0} has the Enterprise.MasterFiles.Business.AccTransactionLines type and PK: 6c1c3709-8498-4ea3-95ba-fee57c41b00c.
{0} other details: Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = {2}, Reverse Date = {3}, Post To GL = N, Reverse To GL = N, Header PK = 889092c1-03f7-45de-b334-3a486168bac3, Job PK = 084172b9-3656-41d5-b18c-1a73179a8f9b, Organization = , Revenue Recognition Type = , Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = No.",
				lineType,
				isPostDate ? "Post" : "Reverse",
				isPostDate ? "21-May-06 00:00:00" : "",
				isPostDate ? "" : "21-May-06 00:00:00");
		}

		public void TestOnRateChangedCallCount()
		{
			var accTransactionLinesChild = Factory.NewWithValidTestData<AccTransactionLinesChild>();
			var hitCount = 0;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxRateNumerator = 18;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxRateDenominator = 6;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxExtraRateNumerator = 12;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxExtraRateDenominator = 3;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxRateNumerator = accTransactionLinesChild.AL_TaxRateNumerator;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxRateDenominator = accTransactionLinesChild.AL_TaxRateDenominator;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxExtraRateNumerator = accTransactionLinesChild.AL_TaxExtraRateNumerator;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_TaxExtraRateDenominator = accTransactionLinesChild.AL_TaxExtraRateDenominator;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(new BusinessObjectFactory());
			taxRate.SetRate_ForTestOnly(8, 4, ZDate.Today, ZDate.Today.AddDays(1));
			taxRate.Factory.Save();
			accTransactionLinesChild.AL_AT = taxRate.PK;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);

			taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(new BusinessObjectFactory());
			taxRate.AT_Code = "DUMMYCODE";
			taxRate.SetRate_ForTestOnly(8, 4, ZDate.Today, ZDate.Today.AddDays(10));
			taxRate.Factory.Save();

			accTransactionLinesChild.AL_AT = taxRate.PK;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			var newDate = ZDate.Today.AddDays(10);
			AssertNotEquals("Precondition: " + nameof(AccTransactionLines.AL_TaxDate), newDate, accTransactionLinesChild.AL_TaxDate);
			accTransactionLinesChild.AL_TaxDate = newDate;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);
		}

		public void TestOnRateChangedCalledWhenRateIsChangedButRawRateNot()
		{
			var taxRate = CreateRate("RAT", AccTaxRate.Types.Rated);
			var taxRate_CAP = CreateRate("CAP", AccTaxRate.Types.CapitalRated);
			var taxRate_RVS = CreateRate("RVS", AccTaxRate.Types.ReverseRated);

			var accTransactionLinesChild = Factory.NewWithValidTestData<AccTransactionLinesChild>();
			var hitCount = 0;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);

			accTransactionLinesChild.AL_AT = taxRate.PK;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);
			var prevAL_TaxRateCalc_Raw = accTransactionLinesChild.AL_TaxRateCalc_Raw;
			var prevAL_TaxExtraRateCalc = accTransactionLinesChild.AL_TaxExtraRateCalc;
			var prevAL_TaxRateCalc = accTransactionLinesChild.AL_TaxRateCalc;

			accTransactionLinesChild.AL_AT = taxRate_CAP.PK;
			AssertEquals(hitCount, accTransactionLinesChild.MethodCallCount);
			AssertEquals("Postcondition: AL_TaxRateCalc", prevAL_TaxRateCalc, accTransactionLinesChild.AL_TaxRateCalc);
			AssertEquals("Postcondition: AL_TaxRateCalc_Raw", prevAL_TaxRateCalc_Raw, accTransactionLinesChild.AL_TaxRateCalc_Raw);
			AssertEquals("Postcondition: AL_TaxExtraRateCalc", prevAL_TaxExtraRateCalc, accTransactionLinesChild.AL_TaxExtraRateCalc);

			accTransactionLinesChild.AL_AT = taxRate_RVS.PK;
			AssertEquals(++hitCount, accTransactionLinesChild.MethodCallCount);
			AssertNotEquals("Postcondition: AL_TaxRateCalc", prevAL_TaxRateCalc, accTransactionLinesChild.AL_TaxRateCalc);
			AssertEquals("Postcondition: AL_TaxRateCalc_Raw", prevAL_TaxRateCalc_Raw, accTransactionLinesChild.AL_TaxRateCalc_Raw);
			AssertEquals("Postcondition: AL_TaxExtraRateCalc", prevAL_TaxExtraRateCalc, accTransactionLinesChild.AL_TaxExtraRateCalc);

			AccTaxRate CreateRate(ZString code, ZString type)
			{
				var rate = AccTaxRate.CreateTaxRate_ForTestOnly(new BusinessObjectFactory());
				rate.AT_Code = code;
				rate.AT_Type = type;
				rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
				rate.SetRate_ForTestOnly(7, 3, ZDate.Today, ZDate.Today.AddDays(1));
				rate.SetExtraRate_ForTestOnly(17, 13, ZDate.Today, ZDate.Today.AddDays(1));
				rate.Factory.Save();

				return rate;
			}
		}

		[TestDate(2018, 09, 25, 20, 49, 10)]
		public void TestSetReverseDateBeforeUnlinkChargeSuspenderIsSuspended()
		{
			var skipReportingWhenReversedWIPACRLinkedToJobChargeAttribute = SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.IsActive;
			try
			{
				SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.IsActive = false;
				var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("cd81332b-231f-4bc7-aea8-5423fc735aa0"));
				job.FillWithValidTestData();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "1234";
				job.JH_Status = JobHeaderStatus.Working.Code;
				Factory.Save();

				JobCharge charge = (JobCharge)Factory.New(typeof(JobCharge), new Guid("9620a13e-7edf-479a-a17c-750a0eaab63e"));
				AccChargeCode chargeCode = (AccChargeCode)Factory.New(typeof(AccChargeCode), new Guid("b5eb1d81-bc9e-4514-b4ec-59211cfc3294"));
				chargeCode.FillWithValidTestData();
				chargeCode.AC_Code = "FRTZ";
				charge.JR_JH = job.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				charge.JR_AC = chargeCode.PK;
				charge.JR_LocalCostAmt = 100;
				charge.JR_LocalSellAmt = 100;
				Factory.Save();

				var acrLine = charge.APLine;
				AssertEquals(acrLine.AL_LineType, ZArchitecture.Core.TransactionLineTypes.Accrual);
				AssertNotNull(acrLine.LoadRelatedJobCharge());

				ErrorReporter.Clear();
				acrLine.AL_ReverseDate = ZDateTime.Today;

				AssertEquals(ErrorReporter.TotalErrorCount, 1);
				string errorMessage = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();

				AssertContains("Error message has line and job charge info", "A reversed ACR/WIP has an associated job charge.", errorMessage);
				AssertNotNull(acrLine.LoadRelatedJobCharge());

				using (acrLine.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					acrLine.AL_ReverseDate = ZDateTime.Today.AddDays(1);
					AssertEquals(ErrorReporter.TotalErrorCount, 0);
				}

				AssertEquals(ErrorReporter.TotalErrorCount, 1);
				errorMessage = ErrorReporter.LastMessageReported;
				AssertContains("Error message has line and job charge info", "A reversed ACR/WIP has an associated job charge.", errorMessage);
				ErrorReporter.Clear();

				using (acrLine.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					acrLine.AL_ReverseDate = ZDateTime.Today.AddDays(2);
					acrLine.LoadRelatedJobCharge().JR_AL_APLine = ZGuid.Empty;
					AssertEquals(ErrorReporter.TotalErrorCount, 0);
				}
				AssertEquals(ErrorReporter.TotalErrorCount, 0);

				var wipLine = charge.ARLine;
				AssertEquals(wipLine.AL_LineType, ZArchitecture.Core.TransactionLineTypes.WIP);
				AssertNotNull(wipLine.LoadRelatedJobCharge());

				ErrorReporter.Clear();
				wipLine.AL_ReverseDate = ZDateTime.Today;

				AssertEquals(ErrorReporter.TotalErrorCount, 1);
				errorMessage = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();

				AssertContains("Error message has line and job charge info", "A reversed ACR/WIP has an associated job charge.", errorMessage);
				AssertNotNull(wipLine.LoadRelatedJobCharge());

				using (wipLine.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					wipLine.AL_ReverseDate = ZDateTime.Today.AddDays(1);
					AssertEquals(ErrorReporter.TotalErrorCount, 0);
				}

				AssertEquals(ErrorReporter.TotalErrorCount, 1);
				errorMessage = ErrorReporter.LastMessageReported;
				AssertContains("Error message has line and job charge info", "A reversed ACR/WIP has an associated job charge.", errorMessage);
				ErrorReporter.Clear();

				using (wipLine.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					wipLine.AL_ReverseDate = ZDateTime.Today.AddDays(2);
					wipLine.LoadRelatedJobCharge().JR_AL_ARLine = ZGuid.Empty;
					AssertEquals(ErrorReporter.TotalErrorCount, 0);
				}
				AssertEquals(ErrorReporter.TotalErrorCount, 0);
			}
			finally
			{
				SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.IsActive = skipReportingWhenReversedWIPACRLinkedToJobChargeAttribute;
			}
		}

		public void TestCheckJobIsNotEmptyWhenChargeCodeRequiredIt()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			transactionLine.AL_JH = ZGuid.Empty;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			transactionLine.AL_AC = chargeCode.PK;

			var chargeTypesShouldHaveJobs = new[] { Constants.ChargeType.Disbursement, Constants.ChargeType.ManualJobAccrual, Constants.ChargeType.Margin, Constants.ChargeType.Revenue };
			var chargeTypesMightNotHaveJobs = new[] { Constants.ChargeType.Comment, Constants.ChargeType.Overhead, Constants.ChargeType.NonAccrual };

			foreach (var chargeType in chargeTypesShouldHaveJobs)
			{
				chargeCode.AC_ChargeType = chargeType;
				Assert(!transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());
			}

			foreach (var chargeType in chargeTypesMightNotHaveJobs)
			{
				chargeCode.AC_ChargeType = chargeType;
				Assert(transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());
			}

			chargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			Assert("Precondition : Make sure the check result is false before testing anything else", !transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());

			transactionLine.AL_JH = ZGuid.NewZGuid();
			Assert(transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());
			transactionLine.AL_JH = ZGuid.Empty;

			transactionLine.AL_AC = ZGuid.Empty;
			Assert(transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());
			transactionLine.AL_AC = chargeCode.PK;

			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			Assert(transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());
			transactionLine.AL_LineType = TransactionLineTypes.Cost;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionLine.AL_AH = transaction.PK;
			chargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			Factory.Save();

			Assert("Precondition : line should be in database", transactionLine.IsInDatabase);
			chargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			Assert(transactionLine.CheckJobIsNotEmptyWhenChargeCodeRequiredIt());
		}

		public void TestIsAutoLogged()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionType = TransactionTypes.Invoice;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AH = transaction.PK;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, transactionLine.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.AddedARecordToTheSystemCode, AutoEvents.EditedARecordCode });
			var logs = Factory.Load<StmALog>(query);

			logs = new BusinessObjectFactory().Load<StmALog>(query);
			AssertEquals("No create log is found", 0, logs.Length);

			transactionLine.AL_Desc = "changed description";
			Factory.Save();

			logs = new BusinessObjectFactory().Load<StmALog>(query);
			AssertEquals("No edit log is found", 0, logs.Length);
		}

		#region IEInvoicingEligibilityLiteTransactionLine

		public void TestIEInvoicingEligibilityLiteTransactionLine_ChargePK()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;
			var agGuid = ZGuid.NewZGuid();
			var acGuid = ZGuid.NewZGuid();

			transactionLine.AL_AG = ZGuid.Empty;
			transactionLine.AL_AC = ZGuid.Empty;
			AssertEquals("ChargePK should be empty when AC and AG FKs are both null", ZGuid.Empty, asEligibilty.ChargePK);

			transactionLine.AL_AG = agGuid;
			transactionLine.AL_AC = ZGuid.Empty;
			AssertEquals("ChargePK should use AG when AC is null", agGuid, asEligibilty.ChargePK);

			transactionLine.AL_AG = ZGuid.Empty;
			transactionLine.AL_AC = acGuid;
			AssertEquals("ChargePK should use AC when AG is null", acGuid, asEligibilty.ChargePK);

			transactionLine.AL_AG = agGuid;
			transactionLine.AL_AC = acGuid;
			AssertEquals("ChargePK should prefer AC over AG", acGuid, asEligibilty.ChargePK);
		}

		public void TestIEInvoicingEligibilityLiteTransactionLine_ChargeCode()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;

			var cc = Factory.NewWithValidTestData<AccChargeCode>();

			AssertEquals("Precondition: empty AL_AC", ZGuid.Empty, transactionLine.AL_AC);

			AssertEquals("ChargeCode should be empty when AL_AC not set", string.Empty, asEligibilty.ChargeCode);

			transactionLine.AL_AC = cc.PK;
			AssertEquals("ChargeCode should be same as underlying BizObject", cc.AC_Code, asEligibilty.ChargeCode);

			transactionLine.AL_AC = cc.PK;
			cc.AC_Code = "ChangedCC";
			AssertEquals("ChargeCode should be same as underlying BizObject", "ChangedCC", asEligibilty.ChargeCode);
		}

		public void TestIEInvoicingEligibilityLiteTransactionLine_ChargeType()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;

			var cc = Factory.NewWithValidTestData<AccChargeCode>();
			cc.AC_ChargeType = Constants.ChargeType.Revenue;

			AssertEquals("Precondition: empty AL_AC", ZGuid.Empty, transactionLine.AL_AC);

			AssertEquals("ChargeCode should be empty when AL_AC not set", string.Empty, asEligibilty.ChargeCode);

			transactionLine.AL_AC = cc.PK;
			AssertEquals("ChargeType should be same as underlying BizObject", cc.AC_ChargeType, asEligibilty.ChargeType);

			transactionLine.AL_AC = cc.PK;
			cc.AC_ChargeType = Constants.ChargeType.Comment;
			AssertEquals("ChargeType should be same as underlying BizObject", Constants.ChargeType.Comment, asEligibilty.ChargeType);
		}

		public void TestIEInvoicingEligibilityLiteTransactionLine_TaxType()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;

			AssertNull("Precondition: null TaxRate", transactionLine.TaxRate);
			Assert("Precondition: null TaxRate FK", transactionLine.AL_AT.IsEmpty);

			AssertEquals("TaxType should be blank when TaxRate is null", ZString.Empty, asEligibilty.TaxType);

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.AT_Code = "TESTRATE";
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			transactionLine.AL_AT = taxRate.PK;

			AssertEquals("TaxType should be AT_Type RAT", AccTaxRate.Types.Rated, asEligibilty.TaxType);

			taxRate.AT_Type = "___";
			AssertEquals("TaxType should be AT_Type ___", "___", asEligibilty.TaxType);
		}

		public void TestIEInvoicingEligibilityLiteTransactionLine_LineAmount()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;

			var lineAmountsToTest = (IReadOnlyCollection<decimal>)new decimal[] { 0m, 1m, -1m };

			foreach (var amount in lineAmountsToTest)
			{
				transactionLine.AL_LineAmount = amount;
				AssertEquals("LineAmount should be same as underlying BizObject", amount, asEligibilty.LineAmount);
			}
		}

		public void TestIEInvoicingEligibilityLiteTransactionLine_OSAmount()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;

			var lineAmountsToTest = (IReadOnlyCollection<decimal>)new decimal[] { 0m, 1m, -1m };

			foreach (var amount in lineAmountsToTest)
			{
				transactionLine.AL_OSAmount = amount;
				AssertEquals("OSAmount should be same as underlying BizObject", amount, asEligibilty.OSAmount);
			}
		}

		public void TestIEInvoicingEligibilityLiteTransactionLine_GSTVATAmount()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var asEligibilty = (IEInvoicingEligibilityLiteTransactionLine)transactionLine;

			var gstVATAmountsToTest = (IReadOnlyCollection<decimal>)new decimal[] { 0m, 0.1m, -1 };

			foreach (var amount in gstVATAmountsToTest)
			{
				transactionLine.AL_GSTVAT = amount;
				AssertEquals("GSTVATAmount should be same as underlying BizObject", amount, asEligibilty.GSTVATAmount);
			}
		}

		#endregion
	}
}
