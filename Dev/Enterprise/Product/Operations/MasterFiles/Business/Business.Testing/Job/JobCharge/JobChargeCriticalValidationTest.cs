using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobChargeCriticalValidationTest : CriticalValidationTest<JobCharge>
	{
		JobCharge SavedCharge, SavedPostedCharge;
		AccTransactionLines SavedWIP, SavedAccrual, SavedREVLine, SavedCSTLine;
		AccTransactionHeader SavedARInvoice, SavedAPInvoice;

		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			GetTestCases_Setup();

			Factory.Save();

			Guid linePk = new Guid("b4f093e0-bd07-454d-a1f1-3578dbf7de96");
			foreach (ZString lineType in new[] { TransactionLineTypes.WIP, TransactionLineTypes.Accrual })
			{
				var lineTypeCopy = lineType; // Required to allow C# closure in loop

				foreach (bool fail in new[] { true, false })
				{
					bool failCopy = fail; // Required to allow C# closure in loop

					CreateDefinition_CheckNewRelatedWIPAccrualLineIsNotReversed(result, lineTypeCopy, linePk, failCopy);
					CreateDefinition_CheckOriginalRelatedWIPAccrualLineIsReversed(result, lineTypeCopy, linePk, failCopy);
					CreateDefinition_CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount(result, lineTypeCopy, linePk, failCopy);
					CreateDefinition_CheckNewRelatedWIPAccrualLineOrganizationIsTheSameAsChargeOrganization(result, lineTypeCopy, linePk, failCopy);
				}
			}

			CreateDefinition_CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount(result, TransactionLineTypes.WIP, linePk, false, sellInvoiceCurrency: true);

			foreach (ZString lineType in new[] { TransactionLineTypes.Revenue, TransactionLineTypes.Cost, TransactionLineTypes.UnapprovedCost })
			{
				var lineTypeCopy = lineType; // Required to allow C# closure in loop

				foreach (bool fail in new[] { true, false })
				{
					bool failCopy = fail; // Required to allow C# closure in loop

					CreateDefinition_CheckRelatedTransactionLineHasMatchingAmountAndSign(result, lineTypeCopy, linePk, failCopy);
					if (lineTypeCopy != TransactionLineTypes.UnapprovedCost)
					{
						CreateDefinition_CheckRelatedPostedTransactionLineReferenceIsNotChanged(result, lineTypeCopy, linePk, failCopy);
					}
					CreateDefinition_CheckRelatedTransactionLineHasMatchingTaxCode(result, lineTypeCopy, linePk, failCopy);
				}
			}

			CreateDefinition_CheckRelatedTransactionLineHasMatchingAmountAndSign(result, TransactionLineTypes.Revenue, linePk, false, sellInvoiceCurrency: true);

			return result;
		}

		#region Set up test cases

		void GetTestCases_Setup()
		{
			SavedCharge = CreateJobCharge(Factory);
			SavedWIP = CreateTransactionLine(Factory, TransactionLineTypes.WIP);
			SavedAccrual = CreateTransactionLine(Factory, TransactionLineTypes.Accrual);

			SavedCharge.JR_AL_ARLine = SavedWIP.PK;
			SavedCharge.JR_AL_APLine = SavedAccrual.PK;
			SavedWIP.AL_JH = SavedCharge.JR_JH;
			SavedAccrual.AL_JH = SavedCharge.JR_JH;
			SavedCharge.SetChargeValuesFromLinkedARLineForTests();
			SavedCharge.SetChargeValuesFromLinkedAPLineForTests();

			SavedARInvoice = CreateInvoice(Factory);
			SavedREVLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			SavedREVLine.AL_AH = SavedARInvoice.PK;

			SavedAPInvoice = CreateInvoice(Factory);
			SavedAPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			SavedCSTLine = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			SavedCSTLine.AL_AH = SavedAPInvoice.PK;

			SavedPostedCharge = CreateJobCharge(Factory);
			SavedPostedCharge.JR_AL_ARLine = SavedREVLine.PK;
			SavedPostedCharge.JR_AL_APLine = SavedCSTLine.PK;
			SavedREVLine.AL_JH = SavedPostedCharge.JR_JH;
			SavedCSTLine.AL_JH = SavedPostedCharge.JR_JH;
		}

		void CreateDefinition_CheckNewRelatedWIPAccrualLineIsNotReversed(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckNewRelatedWIPAccrualLineIsNotReversed, type = {0}, linked to JobCharge and is not reversed = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var parent = CreateJobCharge(factory);

					var relatedLine = CreateTransactionLine(factory, lineType, linePk);
					if (lineType == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
					}
					if (lineType == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = relatedLine.PK;
					}
					relatedLine.AL_JH = parent.JR_JH;

					if (shouldFail)
					{
						using (SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
						{
							relatedLine.AL_ReverseDate = ZDateTime.Today;
						}
					}

					return parent;
				}, shouldFail, shouldFail ? CriticalValidationErrorType.JobChargeLinkedLineShouldNotBeReversed_3 : CriticalValidationErrorType.NoError,
				string.Format(@"Currently linked {0} line should not be reversed", lineType),
				string.Format(@"Line: PK = {1}, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = ",
				lineType, linePk)));
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		void CreateDefinition_CheckOriginalRelatedWIPAccrualLineIsReversed(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckOriginalRelatedWIPAccrualLineIsReversed, type = {0}, previously linked to JobCharge and reversed = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var parent = factory.Load<JobCharge>(SavedCharge.PK);

					var arLine = parent.ARLine;
					var apLine = parent.APLine;

					var newLine = CreateTransactionLine(factory, lineType, linePk);

					if (lineType == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = newLine.PK;
						arLine.AL_ReverseDate = shouldFail ? ZDateTime.Empty : ZDateTime.Today;
					}
					if (lineType == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = newLine.PK;
						apLine.AL_ReverseDate = shouldFail ? ZDateTime.Empty : ZDateTime.Today;
					}
					newLine.AL_JH = parent.JR_JH;
					ExceptionReporterTestListener.Instance.Clear();

					return parent;
				}, shouldFail, shouldFail ? CriticalValidationErrorType.JobChargeHasPreviouslyLinkedLineThatMustBeReversed_3 : CriticalValidationErrorType.NoError,
				string.Format(@"Previously linked but currently detached {0} line should be reversed", lineType),
				string.Format(@"Line: PK = {1}, Charge Code = {2}, GL Account = {3}, Type = {0}, OS Amount = 0.0000, Local Amount = 0.0000, GST = 0.0000, Tax Rate = , Tax Class = , Exchange Rate = 1.000000000, Currency = AUD, Post Date = ",
				lineType, lineType == TransactionLineTypes.WIP ? SavedWIP.PK : SavedAccrual.PK, SavedCharge.ChargeCode.AC_Code, SavedCharge.ChargeCode.GLAccountForTesting.AG_AccountNum)));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckOriginalRelatedWIPAccrualLineIsReversed, type = {0}, previously linked to JobCharge but not Original Value and reversed = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var parent = factory.Load<JobCharge>(SavedCharge.PK);

					parent.ARLine.AL_ReverseDate = ZDateTime.Today;
					parent.APLine.AL_ReverseDate = ZDateTime.Today;

					var intermediateLine = CreateTransactionLine(factory, lineType, linePk);
					intermediateLine.AL_JH = parent.JR_JH;

					if (lineType == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = intermediateLine.PK;
					}
					if (lineType == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = intermediateLine.PK;
					}

					var newLine = CreateTransactionLine(factory, lineType, Guid.Empty);
					newLine.AL_JH = parent.JR_JH;

					if (lineType == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = newLine.PK;
					}
					if (lineType == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = newLine.PK;
					}

					intermediateLine.AL_ReverseDate = shouldFail ? ZDateTime.Empty : ZDateTime.Today;
					ExceptionReporterTestListener.Instance.Clear();

					return parent;
				}, shouldFail, shouldFail ? CriticalValidationErrorType.JobChargeHasPreviouslyLinkedLineThatMustBeReversed_3 : CriticalValidationErrorType.NoError,
				string.Format(@"Previously linked but currently detached {0} line should be reversed", lineType),
				string.Format(@"Line: PK = {1}, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = ",
				lineType, linePk)));
		}

		void CreateDefinition_CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail, bool sellInvoiceCurrency = false)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount, type = {0}, amounts the same = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
					job.FillWithValidTestData();

					var parent = CreateJobCharge(factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
					parent.JR_JH = job.PK;
					parent.JR_LocalSellAmt = 10M;
					parent.JR_OSSellAmt = parent.JR_LocalSellAmt;
					parent.JR_LocalCostAmt = 10M;
					parent.JR_OSCostAmt = parent.JR_LocalCostAmt;

					var relatedLine = CreateTransactionLine(factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));

					relatedLine.AL_AH = ZGuid.Empty;
					relatedLine.AL_RX_NKTransactionCurrency = "AUD";
					if (lineType == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
						if (!(shouldFail || sellInvoiceCurrency))
						{
							relatedLine.AL_LineAmount = -10M;
						}
						if (sellInvoiceCurrency)
						{
							parent.JR_RX_NKSellInvoiceCurrency = "USD";
							Assert("Precondition: BillInInvoiceCurrency", parent.BillInInvoiceCurrency);
							Assert("Precondition: BillInInvoiceCurrencyWithLocalSellCurrency", parent.BillInInvoiceCurrencyWithLocalSellCurrency);
						}
						// No CFX
						Assert(parent.JR_LineCFX.IsEmpty);
						AssertEquals(false, parent.IsApplyCFX);
					}
					if (lineType == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = relatedLine.PK;
						if (!shouldFail)
						{
							relatedLine.AL_LineAmount = 10M;
						}
					}
					relatedLine.AL_JH = parent.JR_JH;

					return parent;
				}, shouldFail, shouldFail ? CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13 : CriticalValidationErrorType.NoError,
				string.Format(@"Related {0} amount is not the same as charge amount", lineType),
				"Charge: PK = 1e62b4e5-84bc-48c5-af62-b2182aa37281",
				string.Format(@"Line: PK = fcb2c006-ca45-4106-94a0-35227aaae854, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 4caede15-eccf-4b28-a850-7aff85959630, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.",
				lineType)));
		}

		void CreateDefinition_CheckNewRelatedWIPAccrualLineOrganizationIsTheSameAsChargeOrganization(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail)
		{
			var chargeOrgType = lineType == TransactionLineTypes.WIP ? "Sell" : "Cost";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckNewRelatedWIPAccrualLineOrganizationIsTheSameAsChargeOrganization, type = {0}, organizations the same = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var org = factory.NewWithPrimaryKey<OrgHeader>(new Guid("571ddba2-fd12-41af-a6a2-06020532b2f2"));
					org.CompanyData.OB_IsDebtor = true;
					org.CompanyData.OB_IsCreditor = true;
					org.OH_Code = "XVBQP68SIYXQ";

					var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
					job.FillWithValidTestData();

					var parent = CreateJobCharge(factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
					parent.JR_JH = job.PK;
					parent.JR_LocalSellAmt = 10M;
					parent.JR_OSSellAmt = parent.JR_LocalSellAmt;

					parent.JR_LocalCostAmt = 10M;
					parent.JR_OSCostAmt = parent.JR_LocalCostAmt;

					var relatedLine = CreateTransactionLine(factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
					relatedLine.AL_AH = ZGuid.Empty;
					relatedLine.AL_RX_NKTransactionCurrency = "AUD";
					relatedLine.AL_LineAmount = lineType == TransactionLineTypes.WIP ? -10m : 10m;
					relatedLine.AL_OSAmount = relatedLine.AL_LineAmount;
					relatedLine.AL_OH = org.PK;

					if (lineType == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
						if (shouldFail)
						{
							relatedLine.AL_OH = ZGuid.Empty;
						}
					}
					if (lineType == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = relatedLine.PK;
						relatedLine.AL_LineAmount = parent.JR_LocalCostAmt;
						relatedLine.AL_OSAmount = parent.JR_OSCostAmt;
						if (shouldFail)
						{
							relatedLine.AL_OH = ZGuid.Empty;
						}
					}

					relatedLine.AL_JH = parent.JR_JH;

					parent.JR_OH_SellAccount = org.PK;
					parent.JR_OH_CostAccount = org.PK;

					return parent;
				}, shouldFail, shouldFail ? CriticalValidationErrorType.JobChargeOrganisationIsNotSameAsLineOne_5 : CriticalValidationErrorType.NoError,
				string.Format(@"Related {0} organization is not the same as charge organization", lineType),
				"Charge: PK = 1e62b4e5-84bc-48c5-af62-b2182aa37281",
				string.Format(@"Line: PK = fcb2c006-ca45-4106-94a0-35227aaae854, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = {1}, Local Amount = {1}, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 4caede15-eccf-4b28-a850-7aff85959630, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.",
				lineType,
				lineType == TransactionLineTypes.WIP ? -10 : 10),
				$@"WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: 1e62b4e5-84bc-48c5-af62-b2182aa37281, Charge {chargeOrgType} Organization: 00000000-0000-0000-0000-000000000000, Line Organization: 571ddba2-fd12-41af-a6a2-06020532b2f2, Old Organization: 00000000-0000-0000-0000-000000000000
   at",
				"WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb: There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
				));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case1()
		{
			AssertOnSavingCheck(CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory), new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, all details are equal"));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case2()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			string mismatchedInfo = "Invoice Number:    7775    |    111";

			string expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_APInvoiceNum = "7775";
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Invoice Number is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, @"Apportion Split Charge invoice details are not equal to parent consol cost invoice details",
				mismatchedInfo,
				expectedMessage1,
				expectedMessage2,
				expectedMessage3
				));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case3()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			string mismatchedInfo = "Invoice Number:    7775    |    111";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = No, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, false);
			charge.JR_APInvoiceNum = "7775";
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Invoice Number is not equal, No Link From Charge To Invoice", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case4()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Invoice Date:    18-Sep-1971 12:00:00.0000    |    05-Jan-2010 12:00:00.0000";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_APInvoiceDate = ZDateTime.BrettsBirthday;
			ExceptionReporterTestListener.Instance.Clear();
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Invoice Date is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details",
				mismatchedInfo,
				expectedMessage1,
				expectedMessage2,
				expectedMessage3
				));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case5()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Payment Date:    18-Sep-1971 12:00:00.0000    |    10-Jan-2010 12:00:00.0000";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_PaymentDate = ZDateTime.BrettsBirthday;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Payment Date is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case6()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Creditor:    TEST    |    XVBQP68SIYXQ";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_OH_CostAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge.CostAccount.OH_Code = "TEST";
			var oldValue = charge.JR_AT_CostGSTRate;
			charge.JR_AT_CostGSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;

			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Creditor is not equal",
				true,
				CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
				"Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo,
				expectedMessage1,
				expectedMessage2,
				expectedMessage3,
				"ChargeSetsCreditorDifferentToConsolCost:\r\nCharge account",
				$"Charge's Tax Rate is changed from {oldValue} to {charge.JR_AT_CostGSTRate}. ConsolCost's Tax Rate is {oldValue}.",
				"ApportionSplitChargeCostAmountIsSetWithZero: "));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case7()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Supplier Cost Reference:    XYZ    |    ABC";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_CostReference = "XYZ";
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Supplier Cost Reference is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case8()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "GST Rate:    TAX1    |    <empty>";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			var differentTaxRate = Factory.New<AccTaxRate>();
			differentTaxRate.AT_Code = "TAX1";
			charge.JR_AT_CostGSTRate = differentTaxRate.PK;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Tax code is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case9()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Tax Class:    MSG1    |    <empty>";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			var differentTaxClass = Factory.New<AccInvMsg>();
			differentTaxClass.A9_Code = "MSG1";
			charge.JR_A9_CostVATClass = differentTaxClass.PK;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Tax class is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case10()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Supply Type:    LOC    |    ";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_CostSupplyType = "LOC";
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Supply type is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case11()
		{
			string userErrorMessage = "Apportion Split Charge invoice details are not equal to parent consol cost invoice details.\r\nPlease go to related consol form, click 'Job Invoicing > Synchronize Cost Invoice Details' menu item and try again.";

			var mismatchedInfo = "Supply Type:    LOC    |    ";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);

			SuspendCriticalValidationAttribute.IsActive = true;
			charge.JR_CostSupplyType = "LOC";
			Factory.Save();
			SuspendCriticalValidationAttribute.IsActive = false;

			charge.JR_APInvoiceNum = "222";
			var consolCost = Factory.Load(ObjectFactory.GetType<IJobConsolCost>(), charge.JR_E6);
			consolCost[JobConsolCostSchema.Constants.E6_InvoiceNum] = "222";

			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, dismatched data without changes", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges, userErrorMessage, mismatchedInfo));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_Case12()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var mismatchedInfo = "Tax Branch:    TB1    |    ";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
";
			string expectedMessage3 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			charge.JR_GB_CostTaxBranch = Factory.New<GlbBranch>().PK;
			charge.CostTaxBranch.GB_Code = "TB1";
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Tax branch is not equal", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3));
		}

		public void TestCheckChargeAndConsolCostInvoiceDetailsAreEqual_ConsolCostIsNotSaved()
		{
			var expectedMessage = @"Consol cost is not saved:
Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost";

			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);

			charge.JR_APInvoiceNum = "222";
			var consolCost = Factory.Load(ObjectFactory.GetType<IJobConsolCost>(), charge.JR_E6);
			consolCost[JobConsolCostSchema.Constants.E6_InvoiceNum] = "222";

			ZDataUtils.SetShouldRowBeSaved(((IBusinessObjectInternals)consolCost).Row, false);
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeAndConsolCostInvoiceDetailsAreEqual, related consol cost is not saved", true, CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", expectedMessage));
		}

		public void TestCheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount_Case1()
		{
			AssertOnSavingCheck(CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, false), new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, non-zero JR_OSCostAmt"));
		}

		public void TestCheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount_Case2()
		{
			var charge = CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, false);
			charge.JR_OSCostAmt = 0m;
			charge.JR_LocalCostAmt = 0m;
			charge.ParentConsolCost[Enterprise.ZArchitecture.Schema.JobConsolCostSchema.Constants.E6_OSCostAmount] = 0m;
			charge.ParentConsolCost[Enterprise.ZArchitecture.Schema.JobConsolCostSchema.Constants.E6_LocalCostAmount] = 0m;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, zero JR_OSCostAmt with zero E6_OSCostAmount"));
		}

		public void TestCheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount_Case3()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = No, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e, Type = ";

			var charge = CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, false);
			charge.JR_OSCostAmt = 0m;
			charge.JR_LocalCostAmt = 0m;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, zero JR_OSCostAmt", true, CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11, "Apportion Split Charge should not have 0 Cost Amount", expectedMessage1, expectedMessage2));
		}

		public void TestCheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount_Case4()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = No, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e, Type = ";

			var charge = CreateJobChargeLinkedToJob(Factory);
			CreateConsolCostLinkedToCharge(Factory, false, charge, false);
			charge.JR_OSCostAmt = 0M;
			charge.JR_LocalCostAmt = 0m;
			charge.HasChanges = false;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, zero JR_OSCostAmt", true, CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11, "Apportion Split Charge should not have 0 Cost Amount", expectedMessage1, expectedMessage2, "IsSavedByFactory Evaluation Info:"));
		}

		public void TestCheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount_Case5()
		{
			string expectedMessage1 = "Job Consol Cost:\r\n\tPK = a12986b4-036c-4cb2-a0e7-0c96438a46dd\r\n\tType = JobConsolCost";

			var expectedMessage2 =
@"	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = No, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e, Type = ";

			var charge = CreateJobChargeLinkedToJob(Factory);
			CreateConsolCostLinkedToCharge(Factory, false, charge, false);
			charge.JR_OSCostAmt = 0M;
			charge.JR_LocalCostAmt = 0m;
			charge.HasChanges = false;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, zero JR_OSCostAmt", true, CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11, "Apportion Split Charge should not have 0 Cost Amount", expectedMessage1, expectedMessage2, "IsSavedByFactory Evaluation Info:"));
		}

		public void TestCheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount_TrackingZeroExchangeRate()
		{
			Factory.NameForDebugging = Enterprise.MasterFiles.Integration.MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName;

			var chargeSeq1 = CreateJobChargeLinkedToJob(Factory);
			CreateConsolCostLinkedToCharge(Factory, false, chargeSeq1, false);
			chargeSeq1.ParentConsolCost[JobConsolCostSchema.E6_RX_NKCurrency.Name] = "USD";
			chargeSeq1.ParentConsolCost[JobConsolCostSchema.E6_ExchangeRate.Name] = 0;
			chargeSeq1.ParentConsolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10;
			chargeSeq1.ParentConsolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 0;
			chargeSeq1.JR_RX_NKCostCurrency = "USD";
			chargeSeq1.JR_LocalCostAmt = 0m;
			chargeSeq1.JR_OSCostAmt = 0M;
			AssertOnSavingCheck(chargeSeq1
				, new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, zero JR_OSCostAmt"
				, true, CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11
				, "Apportion Split Charge should not have 0 Cost Amount"
				, "JobConsolCostExchangeRateChangeToZero: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."));

			var charge = CreateJobChargeLinkedToJob(Factory, 2);
			CreateConsolCostLinkedToCharge(Factory, false, charge, false, Guid.NewGuid(), Guid.NewGuid());
			var consolCost = charge.ParentConsolCost;
			consolCost[JobConsolCostSchema.E6_RX_NKCurrency.Name] = "USD";
			consolCost[JobConsolCostSchema.E6_ExchangeRate.Name] = 0;
			consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = 10;
			consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 0;

			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_LocalCostAmt = 0m;
			charge.JR_OSCostAmt = 0M;

			var consolCostAmountCallStack = $@"
[USD]E6_ExchangeRate had been changed to zero
   at System.Environment.GetStackTrace";

			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods(
				"CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount, zero JR_OSCostAmt"
				, true
				, CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11
				, "Apportion Split Charge should not have 0 Cost Amount"
				, consolCostAmountCallStack
				));
		}

		public void TestOldZeroCostAmountApportionedChargesAreNotAffectedByCriticalValidation()
		{
			var charge = CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, false);
			Factory.Save();

			DbCommand command = ((IDbConnected)Factory).Connection.Command(string.Format(@"
update dbo.JobCharge 
set 
	{0} = 0,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP' 
where 
	{1} = @ChargePK", JobChargeSchema.Constants.JR_OSCostAmt, JobChargeSchema.Constants.PK));
			command.AddParameterBasedOnDbColumn("@ChargePK", charge.PK.ToGuid(), JobChargeSchema.PK);
			command.ExecuteNonQuery();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToConsolCostInvoiceHasNonZeroCostAmount should not validate an old Apportioned Charge with 0 Cost Amount");
			AssertOnSavingCheck(charge, testCase);
		}

		void CreateDefinition_CheckRelatedTransactionLineHasMatchingAmountAndSign(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail, bool sellInvoiceCurrency = false)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckRelatedTransactionLineHasMatchingAmountAndSign Local Amount, type = {0}, different currencies, same amount = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					JobHeader job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
					job.FillWithValidTestData();

					var parent = CreateJobCharge(factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
					parent.JR_JH = job.PK;
					parent.JR_LocalSellAmt = 10M;
					parent.JR_OSSellAmt = parent.JR_LocalSellAmt;
					parent.JR_LocalCostAmt = 10M;
					parent.JR_OSCostAmt = parent.JR_LocalCostAmt;

					var relatedLine = CreateTransactionLine(factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
					relatedLine.AL_JH = parent.JR_JH;
					relatedLine.AL_RX_NKTransactionCurrency = "USD";
					relatedLine.AL_ExchangeRate = 1.1m;
					if (lineType == TransactionLineTypes.Revenue)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
						if (!(shouldFail || sellInvoiceCurrency))
						{
							relatedLine.AL_OSAmount = 11M;
							relatedLine.AL_LineAmount = 10M;
						}
						if (sellInvoiceCurrency)
						{
							parent.JR_RX_NKSellInvoiceCurrency = "USD";
							Assert("Precondition: BillInInvoiceCurrency", parent.BillInInvoiceCurrency);
							Assert("Precondition: BillInInvoiceCurrencyWithLocalSellCurrency", parent.BillInInvoiceCurrencyWithLocalSellCurrency);
						}
					}
					else
					{
						parent.JR_AL_APLine = relatedLine.PK;
						if (!shouldFail)
						{
							relatedLine.AL_OSAmount = -11M;
							relatedLine.AL_LineAmount = -10M;
						}
					}

					return parent;
				}, shouldFail, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				string.Format("Related {0} amount is not the same as charge amount", lineType),
				"Charge: PK = ",
				"Line: PK = "
				));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckRelatedTransactionLineHasMatchingAmountAndSign OS Amount, type = {0}, same currency, same amounts = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
					job.FillWithValidTestData();

					var parent = CreateJobCharge(factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
					parent.FillWithValidTestData();
					parent.JR_JH = job.PK;
					parent.JR_RX_NKSellCurrency = "USD";
					parent.JR_OSSellAmt = 10M;
					var exchangeRates = (BusinessObjectCollection)parent.Job["ExchangeRates"];
					var exRate = exchangeRates.First(); //there is only one exchange rate
					exRate["JF_BaseRate"] = 0.9m;
					parent.JR_LocalSellAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(parent.JR_OSSellAmt, parent.JR_OSSellExRate);
					parent.JR_AT_SellGSTRate = TaxRate.PK;
					parent.JR_RX_NKCostCurrency = "USD";
					parent.JR_OSCostAmt = 10M;
					parent.JR_OSCostExRate = 0.9M;
					parent.JR_LocalCostAmt = Env.CurrentCompany.ExchangeRate.ForeignToLocal(parent.JR_OSCostAmt, parent.JR_OSCostExRate);
					parent.JR_AT_CostGSTRate = TaxRate.PK;

					var relatedLine = CreateTransactionLine(factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
					relatedLine.FillWithValidTestData();
					relatedLine.AL_JH = parent.JR_JH;
					relatedLine.AL_RX_NKTransactionCurrency = "USD";
					relatedLine.AL_ExchangeRate = 0.9m;
					relatedLine.AL_AT = TaxRate.PK;
					if (lineType == TransactionLineTypes.Revenue)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
						if (!shouldFail)
						{
							relatedLine.AL_OSAmount = 11M;
							relatedLine.AL_LineAmount = 11.11M;
							relatedLine.AL_GSTVAT = 0.11M;
						}
					}
					else
					{
						parent.JR_AL_APLine = relatedLine.PK;
						if (!shouldFail)
						{
							relatedLine.AL_OSAmount = -11M;
							relatedLine.AL_LineAmount = -11.11M;
							relatedLine.AL_GSTVAT = -0.11M;
						}
					}

					return parent;
				}, shouldFail, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				string.Format(@"Related {0} amount is not the same as charge amount", lineType),
				"Charge: PK = ",
				"Line: PK = "
				));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckRelatedTransactionLineHasMatchingAmountAndSign Local Amount after saving, type = {0}, same currency, same amount = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var parent = factory.Load<JobCharge>(SavedPostedCharge.PK);

					if (lineType == TransactionLineTypes.Revenue)
					{
						if (shouldFail)
						{
							parent.JR_LocalSellAmt = 20M;
							parent.JR_OSSellAmt = 20M;
						}
					}
					else
					{
						if (shouldFail)
						{
							parent.JR_LocalCostAmt = 20M;
							parent.JR_OSCostAmt = 20M;
						}
					}

					return parent;
				}, shouldFail, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				string.Format("Related {0} amount is not the same as charge amount", lineType == TransactionLineTypes.Revenue ? "REV" : "CST"),
				lineType == TransactionLineTypes.Revenue ? nameof(CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount) : nameof(CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount),
				string.Format(
@"{0} has been changed from 0.0000 to 20 after the {1} posted.
   at System.Environment.GetStackTrace",
				lineType == TransactionLineTypes.Revenue ? "JR_LocalSellAmt" : "JR_LocalCostAmt",
				lineType == TransactionLineTypes.Revenue ? "revenue" : "cost"
				)));
		}

		void CreateDefinition_CheckRelatedPostedTransactionLineReferenceIsNotChanged(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckRelatedPostedTransactionLineReferenceIsNotChanged, type = {0}, invoice reversed = {1}", lineType, !shouldFail),
				(BusinessObjectFactory factory) =>
				{
					var parent = factory.Load<JobCharge>(SavedPostedCharge.PK);

					if (lineType == TransactionLineTypes.Revenue)
					{
						((IBusinessObjectInternals)parent).Row[JobChargeSchema.Constants.JR_AL_ARLine] = System.DBNull.Value;
						factory.Load<AccTransactionHeader>(SavedARInvoice.PK).AH_IsCancelled = !shouldFail;
					}
					else
					{
						parent.APLine.AL_LineType = lineType;
						var header = factory.Load<AccTransactionHeader>(SavedAPInvoice.PK);
						var ledgerTransactionType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLedgerTransactionType(lineType);
						header.AH_Ledger = ledgerTransactionType.Item1;
						header.AH_TransactionType = ledgerTransactionType.Item2;
						factory.Save();

						((IBusinessObjectInternals)parent).Row[JobChargeSchema.Constants.JR_AL_APLine] = System.DBNull.Value;
						header.AH_IsCancelled = !shouldFail;
					}
					parent.HasChanges = true;

					return parent;
				}, shouldFail, CriticalValidationErrorType.JobChargeReferenceToPostedTransactionLineCannotBeChanged_3,
				string.Format(@"Posted {0} line reference can't be changed", lineType),
				"Charge: PK = ",
				string.Format("	Fields with changes: {0} ({1}, 00000000-0000-0000-0000-000000000000).",
									lineType == TransactionLineTypes.Revenue ? "JR_AL_ARLine" : "JR_AL_APLine",
									lineType == TransactionLineTypes.Revenue ? SavedREVLine.PK : SavedCSTLine.PK),
				"Line: PK = "
				));
		}

		void CreateDefinition_CheckRelatedTransactionLineHasMatchingTaxCode(List<TestCaseDefinitionWithDelegate_Obsolete> result, ZString lineType, Guid linePk, bool shouldFail)
		{
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckRelatedTransactionLineHasMatchingTaxCode, type = {0}, same tax codes = {1}", lineType, !shouldFail),
				(factory) =>
				{
					var taxRate1 = factory.New<AccTaxRate>();
					taxRate1.AT_Code = "TAX1";
					var taxRate2 = factory.New<AccTaxRate>();
					taxRate2.AT_Code = "TAX2";

					var parent = CreateJobCharge(factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
					var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
					job.FillWithValidTestData();
					parent.JR_JH = job.PK;
					parent.JR_LocalSellAmt = parent.JR_LocalCostAmt = 10M;
					parent.JR_OSSellAmt = parent.JR_OSCostAmt = 10M;
					parent.JR_AT_CostGSTRate = parent.JR_AT_SellGSTRate = taxRate1.PK;

					var consol = factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new Guid("83eaacb0-a592-4000-b7c2-bd4164008b9b"));
					consol.FillWithValidTestData();

					var consolCost = factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("f3c0243f-d792-449c-90f5-5f4a2770831c"));
					consolCost[JobConsolCostSchema.E6_AC_ChargeCode.Name] = parent.JR_AC;
					consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10m;
					consolCost[JobConsolCostSchema.E6_AT_TaxRate.Name] = taxRate1.PK;
					consolCost.FillWithValidTestData();

					parent.JR_E6 = consolCost.PK;

					var invoice = CreateInvoice(factory);
					invoice.AH_TransactionType = TransactionTypes.Invoice;
					var relatedLine = CreateTransactionLine(factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
					relatedLine.AL_AH = invoice.PK;
					relatedLine.AL_JH = parent.JR_JH;
					relatedLine.AL_AT = shouldFail ? taxRate2.PK : taxRate1.PK;

					if (lineType == TransactionLineTypes.Revenue)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
						relatedLine.AL_LineAmount = relatedLine.AL_OSAmount = 10M;
						consolCost[JobConsolCostSchema.Constants.E6_AH_ARInvoice] = invoice.PK;
					}
					else
					{
						parent.JR_AL_APLine = relatedLine.PK;
						relatedLine.AL_LineAmount = relatedLine.AL_OSAmount = -10M;
						consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice] = invoice.PK;
					}

					return parent;
				}, shouldFail, CriticalValidationErrorType.JobChargeTaxCodeNotEqualRelatedLineTaxCode_7,
				String.Format(@"Related {0} line tax code and class are not the same as charge tax code and class", lineType),
				"Charge: PK = ",
				"Job Consol Cost:\r\n\tPK = ",
				"Line: PK = "
			));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("CheckRelatedTransactionLineHasMatchingTaxCode, type = {0}, same tax codes = {1}", lineType, !shouldFail),
				(factory) =>
				{
					var taxRate1 = factory.New<AccTaxRate>();
					taxRate1.AT_Code = "TAX1";
					var taxClass1 = factory.New<AccInvMsg>();
					taxClass1.A9_Code = "MSG1";
					var taxClass2 = factory.New<AccInvMsg>();
					taxClass2.A9_Code = "MSG2";

					var parent = CreateJobCharge(factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
					var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
					job.FillWithValidTestData();
					parent.JR_JH = job.PK;
					parent.JR_LocalSellAmt = parent.JR_LocalCostAmt = 10M;
					parent.JR_OSSellAmt = parent.JR_OSCostAmt = 10M;
					parent.JR_AT_CostGSTRate = parent.JR_AT_SellGSTRate = taxRate1.PK;
					parent.JR_A9_CostVATClass = parent.JR_A9_SellVATClass = taxClass1.PK;

					var consol = factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new Guid("e531827d-2c4c-4d17-a5fa-6fe3875b471c"));
					consol.FillWithValidTestData();

					var consolCost = factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("359f6157-1354-46a4-a0fc-18cf02f3a048"));
					consolCost[JobConsolCostSchema.E6_AC_ChargeCode.Name] = parent.JR_AC;
					consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10m;
					consolCost[JobConsolCostSchema.E6_AT_TaxRate.Name] = taxRate1.PK;
					consolCost[JobConsolCostSchema.E6_A9_VATClass.Name] = taxClass1.PK;
					consolCost.FillWithValidTestData();

					parent.JR_E6 = consolCost.PK;

					var invoice = CreateInvoice(factory);
					var relatedLine = CreateTransactionLine(factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
					relatedLine.AL_AH = invoice.PK;
					relatedLine.AL_JH = parent.JR_JH;
					relatedLine.AL_AT = taxRate1.PK;
					relatedLine.AL_A9_VATClass = shouldFail ? taxClass2.PK : taxClass1.PK;

					if (lineType == TransactionLineTypes.Revenue)
					{
						parent.JR_AL_ARLine = relatedLine.PK;
						relatedLine.AL_LineAmount = relatedLine.AL_OSAmount = 10M;
						consolCost[JobConsolCostSchema.Constants.E6_AH_ARInvoice] = invoice.PK;
					}
					else
					{
						parent.JR_AL_APLine = relatedLine.PK;
						relatedLine.AL_LineAmount = relatedLine.AL_OSAmount = -10M;
						consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice] = invoice.PK;
					}

					return parent;
				}, shouldFail, CriticalValidationErrorType.JobChargeTaxCodeNotEqualRelatedLineTaxCode_7,
				string.Format(@"Related {0} line tax code and class are not the same as charge tax code and class", lineType),
				"Charge: PK = ",
				"Job Consol Cost:\r\n\tPK = ",
				"Line: PK = "
			));
		}

		#endregion

		public void TestCollectedInfo_CheckRelatedTransactionLineHasMatchingTaxCode()
		{
			var taxRate1 = Factory.New<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			var taxRate2 = Factory.New<AccTaxRate>();
			taxRate2.AT_Code = "TAX2";

			var jobCharge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AT_CostGSTRate = taxRate1.PK;

			var consol = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol.FillWithValidTestData();

			var consolCost = Factory.New(ObjectFactory.GetType<IJobConsolCost>());
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode.Name] = jobCharge.JR_AC;
			consolCost[JobConsolCostSchema.E6_AT_TaxRate.Name] = taxRate1.PK;
			consolCost.FillWithValidTestData();
			jobCharge.JR_E6 = consolCost.PK;

			var invoice = CreateInvoice(Factory);
			var relatedLine = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			relatedLine.AL_AH = invoice.PK;
			relatedLine.AL_JH = jobCharge.JR_JH;
			relatedLine.AL_AT = taxRate2.PK;

			consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice] = invoice.PK;
			jobCharge.JR_AL_APLine = relatedLine.PK;

			AssertNotEquals(relatedLine.AL_AT, jobCharge.JR_AT_CostGSTRate);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedTransactionLineHasMatchingTaxCode, Tax Rate is not equal",
							true,
							CriticalValidationErrorType.JobChargeTaxCodeNotEqualRelatedLineTaxCode_7,
							"Related CST line tax code and class are not the same as charge tax code and class",
							"JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal: ");

			AssertOnSavingCheck(jobCharge, testCase);
		}

		public void TestCheckRelatedTransactionLineHasMatchingAmountAndSign_CollectedInfoIsAlwaysReported()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = CreateJobCharge(Factory);
			jobCharge.JR_JH = job.PK;

			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;

			var invoiceLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			invoiceLine.AL_AH = invoice.PK;
			invoiceLine.AL_OSAmount = invoiceLine.AL_LineAmount = 20m;
			jobCharge.JR_AL_ARLine = invoiceLine.PK;

			Factory.ServiceContainer.RemoveService<CriticalValidationInfoCollectorService>();

			var infoCollectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull($"Precondition: {nameof(infoCollectorService)}", infoCollectorService);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedTransactionLineHasMatchingAmountAndSign, Job Charge and related Line Amount is not equal",
							true,
							CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
							"Related REV amount is not the same as charge amount.",
							"JobChargeLocalSellAmtNotEqualRelatedLineAmount: There was no attempt to collect any data.",
							"JobChargeOSSellAmtNotEqualRelatedLineOSAmount: There was no attempt to collect any data.",
							"JobChargeOSSellExRateNotEqualRelatedLineExRate: There was no attempt to collect any data.",
							"TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There was no attempt to collect any data.",
							"TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There was no attempt to collect any data.",
							"JobChargeOSCostGSTAmountChangedWhenCostPosted: There was no attempt to collect any data.");

			AssertOnSavingCheck(jobCharge, testCase);
		}

		public void TestCheckRelatedTransactionLineHasMatchingAmountAndSign_CollectedInfo()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateNotEqualRelatedLineExRate);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostGSTAmountChangedWhenCostPosted);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = CreateJobCharge(Factory);
			jobCharge.JR_JH = job.PK;

			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;

			var invoiceLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			invoiceLine.AL_AH = invoice.PK;
			invoiceLine.AL_OSAmount = invoiceLine.AL_LineAmount = 20m;
			jobCharge.JR_AL_ARLine = invoiceLine.PK;

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount, () => "Test error message reported when charge OS sell amount and related line amount are different.");
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateNotEqualRelatedLineExRate, () => "Test error message reported when charge OS sell exchange rate and related line exchange rate are different.");
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(invoiceLine.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge, () => "Test error message reported when line OS amount and related charge OS sell amount are different.");
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostGSTAmountChangedWhenCostPosted, () => "Test error message reported when OSCostGSTAmount is changed after cost is posted.");

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedTransactionLineHasMatchingAmountAndSign, Job Charge and related Line Amount is not equal",
							true,
							CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
							"Related REV amount is not the same as charge amount.",
							"JobChargeLocalSellAmtNotEqualRelatedLineAmount:",
							"JobChargeOSSellAmtNotEqualRelatedLineOSAmount:\r\nTest error message reported when charge OS sell amount and related line amount are different.",
							"JobChargeOSSellExRateNotEqualRelatedLineExRate:\r\nTest error message reported when charge OS sell exchange rate and related line exchange rate are different.",
							"TransactionLineAmountChangeWhenItsLinkedToPostedCharge:",
							"TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge:\r\nTest error message reported when line OS amount and related charge OS sell amount are different.",
							"JobChargeOSCostGSTAmountChangedWhenCostPosted:\r\nTest error message reported when OSCostGSTAmount is changed after cost is posted.");

			AssertOnSavingCheck(jobCharge, testCase);
		}

		[TestDate(2025, 02, 02)]
		public void TestCollectedInfoDetails_WhenJobChargeAmountNotEqualRelatedLineAmount()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = "00001000";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var journal = CreateInvoice(Factory);

			journal.AH_Ledger = LedgerTypes.JobCosting;
			journal.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.AutoJobRevenueJournal;

			var line = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			line.AL_AH = journal.PK;

			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_OSAmount = -50m;
			line.AL_ExchangeRate = 0.5m;
			line.AL_LineAmount = -100m;

			var charge = CreateJobCharge(Factory);
			charge.JR_JH = job.PK;
			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			charge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AL_APLine = line.PK;
			charge.APLine.AL_AH = journal.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();

			charge.JR_OSCostAmt = 43m;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckRelatedTransactionLineHasMatchingAmountAndSign, Job Charge and related Line Amount is not equal",
				true, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				"Related CST amount is not the same as charge amount.",
				$"Charge: PK = {charge.PK.ToString()}",
				"InternalFields: InternalJob = 00001000, InternalBranch = BNE, InternalDepartment = BRN",
				$"Line: PK = {line.PK.ToString()}",
				$"Header: PK = {journal.PK.ToString()}, Ledger = JC, Transaction Type = JRJ, Invoice Date = 02-Feb-25 00:00:00, Post Date = 02-Feb-25 00:00:00, Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = TEST_TRANSCATIONNUM, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None."
				);
			AssertOnSavingCheck(charge, testCase);
		}

		#region Data Refresh Bus Update Tests

		public void TestChargeIsSavedSuccessfullyWhileHavingSkipDataRefreshBusUpdateBusinessContexts()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Assert(!charge.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithoutSkippingDataRefreshBusUpdateContexts");
			AssertAfterSavingCheck(charge, testCase);

			charge = Factory.NewWithValidTestData<JobCharge>();
			using (new DisposableAction(() => charge.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange), () => charge.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange)))
			{
				Assert(charge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

				var exceptedErrorMessage = new string[]
				{
					"Job charge was modified by this user during another operation.",
					"Job charge skipped data refresh bus update, but it was saved successfully.",
					$"PK = {charge.PK}",
					"Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateDueToAnyChange)",
					"Properties:"
				}.Union(GetJobChargePropertyBlocks(charge)).Append("Charge in db: Not found").ToArray();
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithSkipDataRefreshBusUpdateDueToAnyChangeContext", true,
					CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_4,
					CriticalValidationMessageTemplate.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
					exceptedErrorMessage);
				AssertAfterSavingCheck(charge, testCase);
			}
		}

		public void TestChargeIsSavedSuccessfullyWhileHavingSkipDataRefreshBusUpdateBusinessContextsWithStackTrace()
		{
			var chargePublisher = Factory.NewWithValidTestData<JobCharge>();
			var chargeSubscriber = Factory.NewWithValidTestData<JobCharge>();
			chargeSubscriber.JR_Desc = "Original Description";
			Factory.Save();

			chargeSubscriber.JR_Desc = "Test Description";
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(chargeSubscriber.PK, CriticalValidationInfoCollectorServiceKeyType.DataRefreshBusUpdateSkipped);
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, chargeSubscriber, chargePublisher, new[] { chargeSubscriber.JR_DescInfo });

			Assert(chargeSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

			var exceptedErrorMessage = new string[]
			{
				"Job charge was modified by this user during another operation.",
				"Job charge skipped data refresh bus update, but it was saved successfully.",
				$"PK = {chargeSubscriber.PK}",
				"Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateDueToAnyChange)",
				"Subscriber:",
				$"Publisher: 	PK = {chargePublisher.PK}",
				"Properties:"
			}.Union(GetJobChargePropertyBlocks(chargePublisher))
			.Append("StackTrace:")
			.Append("Fields with changes: JR_Desc (Original Description, Test Description)",
					$"Charge in db: 	PK = {chargeSubscriber.PK}",
					"Properties:",
					"JR_Desc = Original Description")
			.ToArray();
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithSkipDataRefreshBusUpdateDueToAnyChangeContext", true,
				CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_4,
				CriticalValidationMessageTemplate.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
				exceptedErrorMessage);
			AssertAfterSavingCheck(chargeSubscriber, testCase);
		}

		public void TestChargeIsDeletedAndHasSkippedDataRefreshBusUpdateBusinessContextsAndFactoryIsSavedSuccessfully()
		{
			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			charge1.Delete();
			Assert(!charge1.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithoutSkippingDataRefreshBusUpdateContexts");
			AssertAfterSavingCheck(charge1, testCase);

			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			using (new DisposableAction(() => charge2.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange), () => charge2.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange)))
			{
				var properties = GetJobChargePropertyBlocks(charge2);
				charge2.Delete();
				Assert(charge2.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

				var exceptedErrorMessage = new string[]
				{
					"Job charge was modified by this user during another operation.",
					"Job charge skipped data refresh bus update, but it was saved successfully.",
				}.ToArray();
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithSkipDataRefreshBusUpdateDueToAnyChangeContext", true,
					CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost,
					CriticalValidationMessageTemplate.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
					exceptedErrorMessage);
				AssertAfterSavingCheck(charge2, testCase);
			}

			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			using (charge3.SetTempContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted))
			{
				var properties = GetJobChargePropertyBlocks(charge3);
				charge3.Delete();
				Assert(charge3.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted));

				var exceptedErrorMessage = new string[]
				{
					"Job charge was modified by this user during another operation.",
					"Job charge skipped data refresh bus update, but it was saved successfully.",
				}.ToArray();
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("ChargeWithSkipDataRefreshBusUpdateDueRowIsDeletedContext", true,
					CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost,
					CriticalValidationMessageTemplate.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
					exceptedErrorMessage);
				AssertAfterSavingCheck(charge3, testCase);
			}
		}

		string[] GetJobChargePropertyBlocks(JobCharge charge)
		{
			return new string[]
			{
				$@" JR_A9_CostVATClass = {charge.JR_A9_CostVATClass}
 JR_A9_SellVATClass = {charge.JR_A9_SellVATClass}
 JR_AB = {charge.JR_AB}",
				$@" JR_AgentDeclaredCostAmt = {charge.JR_AgentDeclaredCostAmt}",
				$@" JR_AgentDeclaredSellAmt = {charge.JR_AgentDeclaredSellAmt}",
				$@" JR_AK = {charge.JR_AK}
 JR_AL_APLine = {charge.JR_AL_APLine}
 JR_AL_ARLine = {charge.JR_AL_ARLine}
 JR_AL_CFXLine = {charge.JR_AL_CFXLine}
 JR_APDocumentReceivedDate = {charge.JR_APDocumentReceivedDate}
 JR_APInvoiceDate = {charge.JR_APInvoiceDate}
 JR_APInvoiceNum = {charge.JR_APInvoiceNum}
 JR_APLinePostingStatus = {charge.JR_APLinePostingStatus}
 JR_APNumberOfSupportingDocuments = {charge.JR_APNumberOfSupportingDocuments}",
				$@" JR_ARLinePostingStatus = {charge.JR_ARLinePostingStatus}
 JR_ARNumberOfSupportingDocuments = {charge.JR_ARNumberOfSupportingDocuments}
 JR_AT_CostGSTRate = {charge.JR_AT_CostGSTRate}
 JR_AT_SellGSTRate = {charge.JR_AT_SellGSTRate}
 JR_AW_CostWHTRate = {charge.JR_AW_CostWHTRate}
 JR_AW_SellWHTRate = {charge.JR_AW_SellWHTRate}",
				$@" JR_ChargeType = {charge.JR_ChargeType}
 JR_ChequeNo = {charge.JR_ChequeNo}",
				$@" JR_CostGovtChargeCode = {charge.JR_CostGovtChargeCode}",
				$@" JR_CostPlaceOfSupply = {charge.JR_CostPlaceOfSupply}
 JR_CostPlaceOfSupplyType = {charge.JR_CostPlaceOfSupplyType}
 JR_CostRated = {charge.JR_CostRated}
 JR_CostRatingOverride = {charge.JR_CostRatingOverride}
 JR_CostRatingOverrideComment = {charge.JR_CostRatingOverrideComment}
 JR_CostReference = {charge.JR_CostReference}
 JR_CostSupplyType = {charge.JR_CostSupplyType}
 JR_CostTaxDate = {charge.JR_CostTaxDate}
 JR_DeclaredOSCostAmt = {charge.JR_DeclaredOSCostAmt}
 JR_Desc = {charge.JR_Desc}
 JR_DisplaySequence = {charge.JR_DisplaySequence}
 JR_E6 = {charge.JR_E6}
 JR_E6_GatewaySellHeader = {charge.JR_E6_GatewaySellHeader}
 JR_EstimatedCost = {charge.JR_EstimatedCost}
 JR_EstimatedRevenue = {charge.JR_EstimatedRevenue}",
				$@" JR_GE_InternalDept = {charge.JR_GE_InternalDept}
 JR_InvoiceType = {charge.JR_InvoiceType}",
				$@" JR_IsCostTaxAmountOverridden = {charge.JR_IsCostTaxAmountOverridden}
 JR_IsIncludedInProfitShare = {charge.JR_IsIncludedInProfitShare}",
				$@" JR_JH_InternalJob = {charge.JR_JH_InternalJob}",
				$@" JR_JR_RevenueLine = {charge.JR_JR_RevenueLine}
 JR_LineCFX = {charge.JR_LineCFX}
 JR_LineType = {charge.JR_LineType}
 JR_LocalCostAmt = {charge.JR_LocalCostAmt}",
				$@" JR_LocalSellAmt = {charge.JR_LocalSellAmt}
 JR_MarginPercentage = {charge.JR_MarginPercentage}
 JR_OA_SellInvoiceAddress = {charge.JR_OA_SellInvoiceAddress}
 JR_OC_SellInvoiceContact = {charge.JR_OC_SellInvoiceContact}
 JR_OH_CostAccount = {charge.JR_OH_CostAccount}
 JR_OH_SellAccount = {charge.JR_OH_SellAccount}
 JR_OP_Product = {charge.JR_OP_Product}
 JR_OrderReference = {charge.JR_OrderReference}
 JR_OSCostAmt = {charge.JR_OSCostAmt}",
				$@" JR_OSCostExRate = {charge.JR_OSCostExRate}
 JR_OSCostGSTAmt = {charge.JR_OSCostGSTAmt}",
				$@" JR_OSCostWHTAmt = {charge.JR_OSCostWHTAmt}
 JR_OSSellAmt = {charge.JR_OSSellAmt}",
				$@" JR_OSSellExRate = {charge.JR_OSSellExRate}",
				$@" JR_OSSellWHTAmt = {charge.JR_OSSellWHTAmt}
 JR_PaymentDate = {charge.JR_PaymentDate}
 JR_PaymentType = {charge.JR_PaymentType}
 JR_PreventInvoicePrintGrouping = {charge.JR_PreventInvoicePrintGrouping}
 JR_ProductQuantity = {charge.JR_ProductQuantity}
 JR_ProFormaCost = {charge.JR_ProFormaCost}
 JR_ProFormaRevenue = {charge.JR_ProFormaRevenue}",
				$@" JR_RX_NKCostCurrency = {charge.JR_RX_NKSellCurrency}
 JR_RX_NKSellCurrency = {charge.JR_RX_NKSellCurrency}
 JR_RX_NKSellInvoiceCurrency = {charge.JR_RX_NKSellInvoiceCurrency}",
				$@" JR_SellGovtChargeCode = {charge.JR_SellGovtChargeCode}",
				$@" JR_SellPlaceOfSupply = {charge.JR_SellPlaceOfSupply}
 JR_SellPlaceOfSupplyType = {charge.JR_SellPlaceOfSupplyType}
 JR_SellRated = {charge.JR_SellRated}
 JR_SellRatingOverride = {charge.JR_SellRatingOverride}
 JR_SellRatingOverrideComment = {charge.JR_SellRatingOverrideComment}
 JR_SellReference = {charge.JR_SellReference}
 JR_SellSupplyType = {charge.JR_SellSupplyType}
 JR_SellTaxDate = {charge.JR_SellTaxDate}"
			};
		}

		public void TestChargeHasSkippedDataRefreshBusUpdateAndFactorySaveThrowConcurrencyError_SkipDataRefreshBusUpdateRegsitryIsAnyChange()
		{
			AssertCriticalErrorNotReportedWhenFactorySaveThrowConcurrencyError(false);
		}

		void AssertCriticalErrorNotReportedWhenFactorySaveThrowConcurrencyError(bool isDeletedCharge)
		{
			var subscriberFactory = new BusinessObjectFactory();
			var subscriberCharge = subscriberFactory.NewWithValidTestData<JobCharge>();
			subscriberFactory.Save();

			var publisherFactory = new BusinessObjectFactory();
			var publisherCharge = publisherFactory.Load<JobCharge>(subscriberCharge.PK);

			subscriberCharge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			publisherCharge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			publisherFactory.Save();
			subscriberCharge.RunPreSaveValidation();

			Assert(subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));

			if (isDeletedCharge)
			{
				subscriberCharge.Delete();
			}

			try
			{
				subscriberFactory.Save();
			}
			catch (OnSavingCriticalCheckException)
			{
				Fail("Did not expect a critical check exception as factory should throw concurrency error");
			}
			catch (ZSaveConcurrencyException ex)
			{
				var rowState = isDeletedCharge ? "Deleted" : "Modified";
				var expectedErrorMessage = FormattableString.Invariant($@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: JobCharge
PK: {subscriberCharge.PK}
RowState: {rowState}");
				AssertContains(expectedErrorMessage, ex.Message);
			}
		}

		#endregion

		public void TestCheckChargeExchangeRateWithExistingWrongDataWithoutChange()
		{
			var noErrorTestCase = new TestCaseDefinition_ForSeparateTestsMethods("Should not report existing wrong data");
			var charge = CreateJobCharge(Factory);
			Factory.Save();

			((IDbConnected)Factory).Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobCharge
SET
	JR_OSCostExRate = 0.66,
	JR_OSSellExRate = 0.77,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge.PK));
			charge.Reload();

			AssertNotEquals(1m, charge.JR_OSCostExRate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKCostCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKSellCurrency);
			Assert(!charge.JR_OSCostExRateInfo.HasChanges);
			Assert(!charge.JR_OSSellExRateInfo.HasChanges);
			Assert(charge.IsInDatabase);

			AssertOnSavingCheck(charge, noErrorTestCase);
		}

		public void TestCheckChargeAmountExceedMaximumAllowedAmount()
		{
			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				var charge = CreateJobCharge(Factory);
				charge.JR_JH = jobHeader.PK;

				Assert("Precondition: JR_LocalCostAmt is less than registry setting.", Math.Abs(charge.JR_LocalCostAmt) < maximumAllowedLineAmount);
				Assert("Precondition: JR_LocalSellAmt is less than registry setting.", Math.Abs(charge.JR_LocalSellAmt) < maximumAllowedLineAmount);

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Charge amount is not greater than registry setting.");
				AssertOnSavingCheck(charge, testCase);

				charge.JR_LocalCostAmt = 51M;

				Assert("Precondition: JR_LocalCostAmt is greater than registry setting.", Math.Abs(charge.JR_LocalCostAmt) > maximumAllowedLineAmount);

				var maximumAllowedLineAmountString = registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals());
				var expectedMessage = $"The job charge amount must be between -{maximumAllowedLineAmountString} and {maximumAllowedLineAmountString} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("Charge Amount is greater than registry setting.", true, CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, expectedMessage);
				AssertOnSavingCheck(charge, testCase);

				charge.JR_LocalCostAmt = 49M;
				charge.JR_LocalSellAmt = 52M;

				Assert("Precondition: JR_LocalCostAmt is less than registry setting.", Math.Abs(charge.JR_LocalCostAmt) < maximumAllowedLineAmount);
				Assert("Precondition: JR_LocalSellAmt is greater than registry setting.", Math.Abs(charge.JR_LocalSellAmt) > maximumAllowedLineAmount);

				AssertOnSavingCheck(charge, testCase);
			}
		}

		public void TestCheckChargeAmountExceedMaximumAllowedAmount_InDB()
		{
			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			var maximumAllowedLineAmountString = registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals());
			var expectedMessage = $"The job charge amount must be between -{maximumAllowedLineAmountString} and {maximumAllowedLineAmountString} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var newFactory1 = new BusinessObjectFactory();
				var jobHeader1 = newFactory1.NewJobWithValidTestDataForTesting<JobHeader>();
				var charge1 = CreateJobCharge(newFactory1);
				charge1.JR_JH = jobHeader1.PK;

				newFactory1.Save();

				Assert("Precondition: IsInDatabase is true", charge1.IsInDatabase);
				Assert("Precondition: JR_LocalCostAmtInfo HasChanges is false", !charge1.JR_LocalCostAmtInfo.HasChanges);
				Assert("Precondition: JR_LocalSellAmtInfo HasChanges is false", !charge1.JR_LocalSellAmtInfo.HasChanges);

				var testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("Charge is in DB, JR_LocalCostAmtInfo and JR_LocalSellAmtInfo have no changes.");
				AssertOnSavingCheck(charge1, testCase1);

				charge1.JR_LocalCostAmt = 51M;

				Assert("Precondition: JR_LocalCostAmtInfo HasChanges is true", charge1.JR_LocalCostAmtInfo.HasChanges);
				Assert("Precondition: JR_LocalCostAmt is greater than registry setting.", Math.Abs(charge1.JR_LocalCostAmt) > maximumAllowedLineAmount);

				testCase1 = new TestCaseDefinition_ForSeparateTestsMethods("JR_LocalCostAmtInfo has changes and JR_LocalCostAmt is greater than registry setting.", true, CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, expectedMessage);
				AssertOnSavingCheck(charge1, testCase1);
			}
		}

		public void TestCheckChargeSellExchangeRateFromAnotherCompanyWithDifferentCurrency()
		{
			Action<JobCharge, decimal> setExChangeRateProperly = (chrg, rate) =>
			{
				var exRateWrap = chrg.GetType().GetProperty("RevenueExchangeRate").GetValue(chrg);
				var method = exRateWrap.GetType().GetMethod("SetBuyRate_ForTestOnly");
				method.Invoke(exRateWrap, new object[] { rate });
			};

			var companyACurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var companyBQuery = new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, companyACurrency);
			var companyB = Factory.LoadTop1<GlbCompany>(companyBQuery);
			JobCharge charge;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, companyB.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var companyBFactory = new BusinessObjectFactory();
				var jobHeader = companyBFactory.NewJobWithValidTestDataForTesting<JobHeader>();
				charge = CreateJobCharge(companyBFactory);
				charge.JR_JH = jobHeader.PK;
				charge.JR_RX_NKSellCurrency = companyACurrency;
				//need to use reflection as the code is in Accounting we can't refer this project to
				setExChangeRateProperly(charge, 1.2m);
				companyBFactory.Save();

				AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKSellCurrency);
				AssertNotEquals(1m, charge.JR_OSSellExRate);
			}

			var chargeReloaded = Factory.Load<JobCharge>(charge.PK);
			chargeReloaded.JR_RX_NKSellCurrency = "USD";
			setExChangeRateProperly(chargeReloaded, 1.3m);

			AssertEquals(companyB.PK, chargeReloaded.Company.PK);
			AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, chargeReloaded.JR_RX_NKSellCurrency);
			AssertNotEquals(1m, chargeReloaded.JR_OSSellExRate);//for a local currency it is not possible to set ex rate to other than 1m
			Assert(chargeReloaded.JR_OSSellExRateInfo.HasChanges);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeSellExchangeRate - No Errors");
			AssertOnSavingCheck(chargeReloaded, testCase);
		}

		public void TestCheckChargeCostExchangeRateFromAnotherCompanyWithDifferentCurrency()
		{
			var companyACurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var companyBQuery = new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, companyACurrency);
			var companyB = Factory.LoadTop1<GlbCompany>(companyBQuery);
			JobCharge charge;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, companyB.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var companyBFactory = new BusinessObjectFactory();
				var jobHeader = companyBFactory.NewJobWithValidTestDataForTesting<JobHeader>();
				charge = CreateJobCharge(companyBFactory);
				charge.JR_JH = jobHeader.PK;
				charge.JR_RX_NKCostCurrency = companyACurrency;
				charge.JR_OSCostExRate = 1.2m;
				companyBFactory.Save();

				AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKCostCurrency);
				AssertNotEquals(1m, charge.JR_OSCostExRate);
			}

			var chargeReloaded = Factory.Load<JobCharge>(charge.PK);
			chargeReloaded.JR_OSCostExRate = 1.3m;

			AssertEquals(companyB.PK, chargeReloaded.Company.PK);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, chargeReloaded.JR_RX_NKCostCurrency);
			AssertNotEquals(1m, chargeReloaded.JR_OSCostExRate);
			Assert(chargeReloaded.JR_OSCostExRateInfo.HasChanges);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeCostExchangeRate - No Errors");
			AssertOnSavingCheck(chargeReloaded, testCase);
		}

		public void TestCheckChargeSellExchangeRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				var charge = CreateJobCharge(Factory);
				charge.JR_JH = jobHeader.PK;

				AssertEquals("AUD", charge.JR_RX_NKSellCurrency);
				AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				AssertEquals(1m, charge.JR_OSSellExRate);
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeSellExchangeRate - No Errors");
				AssertOnSavingCheck(charge, testCase);

				charge.JR_RX_NKSellCurrency = "USD";
				var exchangeRateUSD = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == "USD");
				AssertNotNull(exchangeRateUSD);
				exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);
				AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKSellCurrency);
				AssertEquals(0.5m, charge.JR_OSSellExRate);
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeSellExchangeRate - No Errors");
				AssertOnSavingCheck(charge, testCase);

				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_OSSellExRate = 1.2m;
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKSellCurrency);
				Assert(charge.JR_OSSellExRate != 1m);
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("When Local Company currency is equals to the charge sell currency, the sell exchange rate must be 1.", true,
					CriticalValidationErrorType.JobChargeOSSellExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeSellCurrency_4,
					"Invalid charge sell exchange rate. The job charge sell exchange rate should be 1.0 when local company currency is equals to the job charge sell currency.");
				AssertOnSavingCheck(charge, testCase);
			}
		}

		public void TestCheckChargeCostExchangeRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				var charge = CreateJobCharge(Factory);
				charge.JR_JH = jobHeader.PK;

				AssertEquals("AUD", charge.JR_RX_NKCostCurrency);
				AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				AssertEquals(1m, charge.JR_OSCostExRate);
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeCostExchangeRate - No Errors");
				AssertOnSavingCheck(charge, testCase);

				charge.JR_RX_NKCostCurrency = "USD";
				var exchangeRateUSD = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == "USD");
				AssertNotNull(exchangeRateUSD);
				exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);
				AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKCostCurrency);
				AssertEquals(0.5m, charge.JR_OSCostExRate);
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeCostExchangeRate - No Errors");
				AssertOnSavingCheck(charge, testCase);

				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_OSCostExRate = 1.2m;
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKCostCurrency);
				Assert(charge.JR_OSCostExRate != 1m);
				testCase = new TestCaseDefinition_ForSeparateTestsMethods("When Local Company currency is equals to the charge cost currency, the cost exchange rate must be 1.", true,
					CriticalValidationErrorType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency_9,
					"Invalid charge cost exchange rate. The job charge cost exchange rate should be 1.0 when local company currency is equals to the job charge cost currency.");
				AssertOnSavingCheck(charge, testCase);
			}
		}

		public void TestPostedChargeDeleted()
		{
			var savedARInvoice = CreateInvoice(Factory);
			savedARInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			savedARInvoice.AH_TransactionType = TransactionTypes.Invoice;
			var savedREVLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			savedREVLine.AL_AH = savedARInvoice.PK;

			var savedAPInvoice = CreateInvoice(Factory);
			savedAPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			savedAPInvoice.AH_TransactionType = TransactionTypes.Invoice;
			var savedCSTLine = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			savedCSTLine.AL_AH = savedAPInvoice.PK;

			var savedREVPostedCharge = CreateJobCharge(Factory);
			savedREVPostedCharge.JR_AL_ARLine = savedREVLine.PK;
			savedREVLine.AL_JH = savedREVPostedCharge.JR_JH;

			var savedCSTPostedCharge = CreateJobCharge(Factory);
			savedCSTPostedCharge.JR_AL_APLine = savedCSTLine.PK;
			savedCSTLine.AL_JH = savedCSTPostedCharge.JR_JH;

			var savedCharge = CreateJobCharge(Factory);

			var savedWIP = CreateTransactionLine(Factory, TransactionLineTypes.WIP);
			var savedAccrual = CreateTransactionLine(Factory, TransactionLineTypes.Accrual);
			savedCharge.JR_AL_ARLine = savedWIP.PK;
			savedCharge.JR_AL_APLine = savedAccrual.PK;

			Factory.Save();

			foreach (ZString type in new[] { TransactionLineTypes.Revenue, TransactionLineTypes.Cost })
			{
				foreach (bool fail in new[] { true, false })
				{
					ReleaseFactory();

					JobCharge parent;
					if (type == TransactionLineTypes.Revenue)
					{
						parent = Factory.Load<JobCharge>(savedREVPostedCharge.PK);
						parent.JR_AL_ARLine = ZGuid.Empty;
						Factory.Load<AccTransactionHeader>(savedARInvoice.PK).AH_IsCancelled = !fail;
					}
					else
					{
						parent = Factory.Load<JobCharge>(savedCSTPostedCharge.PK);
						parent.APLine.AL_LineType = type;

						var header = Factory.Load<AccTransactionHeader>(savedAPInvoice.PK);
						var ledgerTransactionType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLedgerTransactionType(type);
						header.AH_Ledger = ledgerTransactionType.Item1;
						header.AH_TransactionType = ledgerTransactionType.Item2;

						Factory.Save();

						parent.JR_AL_APLine = ZGuid.Empty;
						header.AH_IsCancelled = !fail;
					}
					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Posted Charge deleted, type = {0}, posted = {1}", type, fail),
						fail, fail ? CriticalValidationErrorType.JobChargeRelatedToPostedTransactionLineCannotBeDeleted_4 : CriticalValidationErrorType.NoError,
						string.Format(@"Charge deleted with posted {0}", type),
						$"PK = {parent.PK}",
						"Business Contexts",
						"Properties:",
						"JobChargeConstructorStackTrace:",
						"JobChargeDeleteStackTrace:");

					AssertDeletedObjectOnSavingCheck(parent, testCase);
				}
			}
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2014, 2, 1)]
		public void TestPreviouslyLinkedWIPAccrualNotReversedForDeletedCharge()
		{
			var savedCharge = CreateJobCharge(Factory);

			var savedWIP = CreateTransactionLine(Factory, TransactionLineTypes.WIP);
			var savedAccrual = CreateTransactionLine(Factory, TransactionLineTypes.Accrual);
			savedCharge.JR_AL_ARLine = savedWIP.PK;
			savedCharge.JR_AL_APLine = savedAccrual.PK;
			savedCharge.SetChargeValuesFromLinkedAPLineForTests();
			savedCharge.SetChargeValuesFromLinkedARLineForTests();

			Factory.Save();

			foreach (ZString type in new[] { TransactionLineTypes.Accrual, TransactionLineTypes.WIP })
			{
				foreach (bool fail in new[] { true, false })
				{
					ReleaseFactory();

					bool isAR = type == TransactionLineTypes.WIP;
					JobCharge parent = Factory.Load<JobCharge>(savedCharge.PK);
					var line = Factory.Load<AccTransactionLines>(isAR ? parent.JR_AL_ARLine : parent.JR_AL_APLine);
					var otherLine = Factory.Load<AccTransactionLines>(isAR ? parent.JR_AL_APLine : parent.JR_AL_ARLine);

					parent.JR_AL_ARLine = ZGuid.Empty;
					parent.JR_AL_APLine = ZGuid.Empty;
					ExceptionReporterTestListener.Instance.Clear();

					parent.Delete();

					if (fail)
					{
						line.AL_ReverseDate = ZDateTime.Empty;
					}

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Charge deleted with previously linked {0} which is {1}", type, fail ? "not reversed" : "reversed"),
						fail, fail ? CriticalValidationErrorType.JobChargeRelatedWIP_ACR_MustBeReversedWhenChargeIsDeleted_3 : CriticalValidationErrorType.NoError,
						string.Format(@"Charge deleted with not reversed {0}", type),
						string.Format(@"Charge original values: PK = {1}, Job PK = {2}, Charge Code = {3}, Cost Account = , OS Cost Amount = 0.0000, Local Cost Amount = 0.0000, OS Cost Exchange Rate = 1.000000000, OS Cost GST Amount = 0.0000, OS Cost WHT Amount = 0.0000, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = {4}, AP Line = {6}, Sell Account = , OS Sell Exchange Rate = 1.000000000, OS Sell Amount = 0.0000, Local Sell Amount = 0.0000, OS Sell WHT Amount = 0.0000, Is Revenue Posted = {5}, AR Line = {7}, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0.000, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = Yes.
Line: PK = {9}, Charge Code = {3}, GL Account = {10}, Type = {0}, OS Amount = 0.0000, Local Amount = 0.0000, GST = 0.0000, Tax Rate = , Tax Class = , Exchange Rate = 1.000000000, Currency = AUD, Post Date = 01-Feb-14 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {8}, Job PK = {2}, Organization = , Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = Yes.",
						type,
						savedCharge.PK,
						savedCharge.JR_JH,
						savedCharge.ChargeCode.AC_Code,
						"No",
						"No",
						savedAccrual.PK,
						savedWIP.PK,
						"00000000-0000-0000-0000-000000000000",
						type == TransactionLineTypes.WIP ? savedWIP.PK : savedAccrual.PK,
						savedCharge.ChargeCode.GLAccountForTesting.AG_AccountNum
						));

					AssertDeletedObjectOnSavingCheck(parent, testCase);

					ReleaseFactory();

					parent = Factory.Load<JobCharge>(savedCharge.PK);
					line = Factory.Load<AccTransactionLines>(isAR ? parent.JR_AL_ARLine : parent.JR_AL_APLine);
					otherLine = Factory.Load<AccTransactionLines>(isAR ? parent.JR_AL_APLine : parent.JR_AL_ARLine);

					line.AL_ReverseDate = ZDateTime.Now;
					otherLine.AL_ReverseDate = ZDateTime.Now;

					var newLine = CreateTransactionLine(Factory, type, Guid.Empty);
					newLine.AL_JH = parent.JR_JH;

					if (type == TransactionLineTypes.WIP)
					{
						parent.JR_AL_ARLine = newLine.PK;
					}
					if (type == TransactionLineTypes.Accrual)
					{
						parent.JR_AL_APLine = newLine.PK;
					}

					testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Charge deleted with previously linked, but not the Original Value,  {0} which is {1}", type, fail ? "not reversed" : "reversed"),
						fail, fail ? CriticalValidationErrorType.JobChargeRelatedWIP_ACR_MustBeReversedWhenChargeIsDeleted_3 : CriticalValidationErrorType.NoError,
						string.Format(@"Charge deleted with not reversed {0}", type),
						string.Format(
@"Charge original values: PK = {1}, Job PK = {2}, Charge Code = {3}, Cost Account = , OS Cost Amount = 0.0000, Local Cost Amount = 0.0000, OS Cost Exchange Rate = 1.000000000, OS Cost GST Amount = 0.0000, OS Cost WHT Amount = 0.0000, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = {4}, AP Line = {6}, Sell Account = , OS Sell Exchange Rate = 1.000000000, OS Sell Amount = 0.0000, Local Sell Amount = 0.0000, OS Sell WHT Amount = 0.0000, Is Revenue Posted = {5}, AR Line = {7}, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0.000, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = Yes.
Line: PK = {9}, Charge Code = , GL Account = 1010101010, Type = {0}, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = {2}, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.",
						type,
						savedCharge.PK,
						savedCharge.JR_JH,
						savedCharge.ChargeCode.AC_Code,
						"No",
						"No",
						savedAccrual.PK,
						savedWIP.PK,
						parent.JR_JH,
						newLine.PK
						));

					parent.Delete();

					if (fail)
					{
						newLine.AL_ReverseDate = ZDateTime.Empty;
					}

					AssertDeletedObjectOnSavingCheck(parent, testCase);
				}
			}
		}

		[TestDate(2014, 2, 1)]
		public void TestLinkedWIPAccrualNotReversedForDeletedCharge2()
		{
			var savedCharge = CreateJobCharge(Factory);

			var savedWIP = CreateTransactionLine(Factory, TransactionLineTypes.WIP);
			var savedAccrual = CreateTransactionLine(Factory, TransactionLineTypes.Accrual);
			savedCharge.JR_AL_ARLine = savedWIP.PK;
			savedCharge.JR_AL_APLine = savedAccrual.PK;
			savedCharge.SetChargeValuesFromLinkedAPLineForTests();
			savedCharge.SetChargeValuesFromLinkedARLineForTests();

			Factory.Save();

			foreach (ZString type in new[] { TransactionLineTypes.Accrual, TransactionLineTypes.WIP })
			{
				foreach (bool fail in new[] { true, false })
				{
					ReleaseFactory();

					JobCharge parent;

					bool isAR = type == TransactionLineTypes.WIP;

					parent = Factory.Load<JobCharge>(savedCharge.PK);
					var line = Factory.Load<AccTransactionLines>(isAR ? parent.JR_AL_ARLine : parent.JR_AL_APLine);
					var otherLine = Factory.Load<AccTransactionLines>(isAR ? parent.JR_AL_APLine : parent.JR_AL_ARLine);

					parent.Delete();

					if (fail)
					{
						line.AL_ReverseDate = ZDateTime.Empty;
					}

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Charge deleted with linked {0} which is {1}", type, fail ? "not reversed" : "reversed"),
						fail, fail ? CriticalValidationErrorType.JobChargeRelatedWIP_ACR_MustBeReversedWhenChargeIsDeleted_3 : CriticalValidationErrorType.NoError,
						string.Format(@"Charge deleted with not reversed {0}", type),
						string.Format(@"Charge original values: PK = {1}, Job PK = {2}, Charge Code = {3}, Cost Account = , OS Cost Amount = 0.0000, Local Cost Amount = 0.0000, OS Cost Exchange Rate = 1.000000000, OS Cost GST Amount = 0.0000, OS Cost WHT Amount = 0.0000, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = {4}, AP Line = {6}, Sell Account = , OS Sell Exchange Rate = 1.000000000, OS Sell Amount = 0.0000, Local Sell Amount = 0.0000, OS Sell WHT Amount = 0.0000, Is Revenue Posted = {5}, AR Line = {7}, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0.000, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = Yes.
Line: PK = {9}, Charge Code = {3}, GL Account = {10}, Type = {0}, OS Amount = 0.0000, Local Amount = 0.0000, GST = 0.0000, Tax Rate = , Tax Class = , Exchange Rate = 1.000000000, Currency = AUD, Post Date = 01-Feb-14 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {8}, Job PK = {2}, Organization = , Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = Yes.",
						type,
						savedCharge.PK,
						savedCharge.JR_JH,
						savedCharge.ChargeCode.AC_Code,
						"No",
						"No",
						savedAccrual.PK,
						savedWIP.PK,
						"00000000-0000-0000-0000-000000000000",
						type == TransactionLineTypes.WIP ? savedWIP.PK : savedAccrual.PK,
						savedCharge.ChargeCode.GLAccountForTesting.AG_AccountNum
						));

					AssertDeletedObjectOnSavingCheck(parent, testCase);
				}
			}
		}

		#region TestCheckNewChargeLinkedToWIPAccrualLineAlreadyInDataBase

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2016, 11, 16)]
		public void TestCheckNewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges()
		{
			var parent = CreateJobCharge(Factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			parent.JR_JH = job.PK;
			var relatedWIPLine = CreateTransactionLine(Factory, TransactionLineTypes.WIP, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
			var relatedACRLine = CreateTransactionLine(Factory, TransactionLineTypes.Accrual, new Guid("684f6900-562a-450d-8845-b14087c27ae4"));
			parent.JR_AL_ARLine = relatedWIPLine.PK;
			parent.JR_AL_APLine = relatedACRLine.PK;
			parent.SetChargeValuesFromLinkedARLineForTests();
			parent.SetChargeValuesFromLinkedAPLineForTests();

			var jobCharge = relatedACRLine.LoadRelatedJobCharge();

			Factory.Save();

			var testCaseForWIP = new TestCaseDefinition_ForSeparateTestsMethods("NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges",
				true, CriticalValidationErrorType.NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges_3, "The new charge linked to a WIP Line or ACR Line which already in database and referred by other charges.",
				string.Format(CultureInfo.InvariantCulture, @"
Related WIP
Line: PK = fcb2c006-ca45-4106-94a0-35227aaae854, Charge Code = 6GQVHOGKXL, GL Account = {0}, Type = WIP, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 16-Nov-16 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 4caede15-eccf-4b28-a850-7aff85959630, Organization = , Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = No.", relatedWIPLine.GLHeader.AG_AccountNum),
"Charge: PK = 7d54bde3-93b6-4b13-a32f-c93784747e8b",
@"Other charges
Charge: PK = 1e62b4e5-84bc-48c5-af62-b2182aa37281");

			var testCaseForACR = new TestCaseDefinition_ForSeparateTestsMethods("NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges",
				true, CriticalValidationErrorType.NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges_3, "The new charge linked to a WIP Line or ACR Line which already in database and referred by other charges.",
				string.Format(CultureInfo.InvariantCulture, @"
Related ACR
Line: PK = 684f6900-562a-450d-8845-b14087c27ae4, Charge Code = 6GQVHOGKXL, GL Account = {0}, Type = ACR, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 16-Nov-16 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 4caede15-eccf-4b28-a850-7aff85959630, Organization = , Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = No.", relatedACRLine.GLHeader.AG_AccountNum),
"Charge: PK = 7d54bde3-93b6-4b13-a32f-c93784747e8b",
@"Other charges
Charge: PK = 1e62b4e5-84bc-48c5-af62-b2182aa37281");

			AssertCheckNewChargeLinkedToWIPAccrualLineAlreadyInDataBase(relatedWIPLine, testCaseForWIP, jobCharge);
			AssertCheckNewChargeLinkedToWIPAccrualLineAlreadyInDataBase(relatedACRLine, testCaseForACR, jobCharge);
		}

		void AssertCheckNewChargeLinkedToWIPAccrualLineAlreadyInDataBase(AccTransactionLines line, TestCaseDefinition_ForSeparateTestsMethods testCase, JobCharge jobCharge)
		{
			var otherCharge = CreateJobCharge(Factory, new Guid("7d54bde3-93b6-4b13-a32f-c93784747e8b"));
			otherCharge.JR_JH = line.AL_JH;

			if (line.AL_LineType == TransactionLineTypes.WIP)
			{
				otherCharge.JR_AL_ARLine = line.PK;
			}
			else
			{
				otherCharge.JR_AL_APLine = line.PK;
			}

			AssertOnSavingCheck(otherCharge, testCase);
			if (line.AL_LineType == TransactionLineTypes.Accrual)
			{
				jobCharge.APLine.AL_ReverseDate = ZDateTime.Today;
				jobCharge.JR_AL_APLine = ZGuid.Empty;
			}
			if (line.AL_LineType == TransactionLineTypes.WIP)
			{
				jobCharge.ARLine.AL_ReverseDate = ZDateTime.Today;
				jobCharge.JR_AL_ARLine = ZGuid.Empty;
			}
			otherCharge.Delete();
		}

		#endregion

		public void TestCheckOtherChargesLinkedToTheSameRevenueTransactionLine_OtherChargeLinkedByJR_AL_ARLine()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(false, TransactionLineTypes.Revenue, true);
		}

		public void TestCheckOtherChargesLinkedToTheSameRevenueTransactionLine_OtherChargeLinkedByJR_AL_APLine()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(false, TransactionLineTypes.Revenue, false);
		}

		public void TestCheckOtherChargesLinkedToTheSameWIPTransactionLine_OtherChargeLinkedByJR_AL_APLine()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(false, TransactionLineTypes.WIP, false);
		}

		public void TestCheckOtherChargesLinkedToTheSameCostTransactionLine_OtherChargeLinkedByJR_AL_ARLine()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(false, TransactionLineTypes.Cost, true);
		}

		public void TestCheckOtherChargesLinkedToTheSameAccrualTransactionLine_OtherChargeLinkedByJR_AL_ARLine()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(false, TransactionLineTypes.Accrual, true);
		}

		public void TestCheckOtherChargesLinkedToTheSameCostTransactionLine_OtherChargeLinkedByJR_AL_APLine()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(false, TransactionLineTypes.Cost, false);
		}

		public void TestCheckOtherChargesLinkedToTheSameRevenueTransactionLine_OtherChargeLinkedByJR_AL_ARLine_WithNewTransactionLineInMemory()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(true, TransactionLineTypes.Revenue, true);
		}

		public void TestCheckOtherChargesLinkedToTheSameRevenueTransactionLine_OtherChargeLinkedByJR_AL_APLine_WithNewTransactionLineInMemory()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(true, TransactionLineTypes.Revenue, false);
		}

		public void TestCheckOtherChargesLinkedToTheSameWIPTransactionLine_OtherChargeLinkedByJR_AL_APLine_WithNewTransactionLineInMemory()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(true, TransactionLineTypes.WIP, false);
		}

		public void TestCheckOtherChargesLinkedToTheSameCostTransactionLine_OtherChargeLinkedByJR_AL_ARLine_WithNewTransactionLineInMemory()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(true, TransactionLineTypes.Cost, true);
		}

		public void TestCheckOtherChargesLinkedToTheSameAccrualTransactionLine_OtherChargeLinkedByJR_AL_ARLine_WithNewTransactionLineInMemory()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(true, TransactionLineTypes.Accrual, true);
		}

		public void TestCheckOtherChargesLinkedToTheSameCostTransactionLine_OtherChargeLinkedByJR_AL_APLine_WithNewTransactionLineInMemory()
		{
			AssertCheckOtherChargesLinkedToTheSameTransactionLine(true, TransactionLineTypes.Cost, false);
		}

		public void AssertCheckOtherChargesLinkedToTheSameTransactionLine(bool hasNewTransactionLineInMemory, ZString lineType, bool isOtherChargeLinkedByJR_AL_ARLine)
		{
			var parent = CreateJobCharge(Factory, new Guid("1e62b4e5-84bc-48c5-af62-b2182aa37281"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			parent.JR_JH = job.PK;

			var relatedLine = CreateTransactionLine(Factory, lineType, new Guid("fcb2c006-ca45-4106-94a0-35227aaae854"));
			if (lineType == TransactionLineTypes.WIP || lineType == TransactionLineTypes.Revenue)
			{
				parent.JR_AL_ARLine = relatedLine.PK;
			}
			else if (lineType == TransactionLineTypes.Accrual || lineType == TransactionLineTypes.Cost)
			{
				parent.JR_AL_APLine = relatedLine.PK;
			}
			relatedLine.AL_JH = parent.JR_JH;
			relatedLine.AL_AC = parent.JR_AC;
			parent.SetChargeValuesFromLinkedARLineForTests();

			if (lineType == TransactionLineTypes.Revenue || lineType == TransactionLineTypes.Cost)
			{
				var invoice = CreateInvoice(Factory);
				invoice.AH_Ledger = lineType == TransactionLineTypes.Revenue ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				relatedLine.AL_AH = invoice.PK;
			}

			if (!hasNewTransactionLineInMemory)
			{
				Factory.Save();
			}

			var otherCharge = CreateJobCharge(Factory, new Guid("7d54bde3-93b6-4b13-a32f-c93784747e8b"));
			otherCharge.JR_JH = job.PK;
			otherCharge.JR_AC = parent.JR_AC;
			if (isOtherChargeLinkedByJR_AL_ARLine)
			{
				otherCharge.JR_AL_ARLine = relatedLine.PK;
			}
			else
			{
				otherCharge.JR_AL_APLine = relatedLine.PK;
			}

			var testCase = hasNewTransactionLineInMemory
				? new TestCaseDefinition_ForSeparateTestsMethods("CheckOtherChargesLinkedToTheSameTransactionLine with new invoice line in memory")
				: new TestCaseDefinition_ForSeparateTestsMethods("CheckOtherChargesLinkedToTheSameTransactionLine",
				true, CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_ChargeSide_4, "Other charges related to the same Transaction line",
				"Charge: PK = 7d54bde3-93b6-4b13-a32f-c93784747e8b",
				@"Other charges:
Charge: PK = 1e62b4e5-84bc-48c5-af62-b2182aa37281");

			AssertOnSavingCheck(otherCharge, testCase);
		}

		#region TestCheckChargeLinkedToPostedConsolCostIsCostPosted

		public void TestCheckChargeLinkedToPostedConsolCostIsCostPosted_ChargeNotInDb()
		{
			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			Assert("Precondition: charge cost is posted.", charge.IsCostPosted);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("All posted");
			AssertOnSavingCheck(charge, testCase);
			AssertOnSavingCheckForAnotherCriticalValidationType((IJobConsolCost)charge.ParentConsolCost, testCase);

			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_AL_APLine] = DBNull.Value; //we can't use related bizo property setter as our production code prevents such invalid changes, but we want check we will have an error in this case

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Not saved charge is posted, so consol cost validation should report an error and not this validation.");
			AssertOnSavingCheck(charge, testCase);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Not saved charge is posted, so consol cost validation should report an error.", true,
				CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4, "Non Cost posted Apportion Split Charge is linked to posted Consol Cost.",
				"Job Consol Cost:\r\n\tPK = ", "Charge with incorrect data:", "JobConsolCost Reloaded:", "Problem Charge Reloaded:");
			AssertOnSavingCheckForAnotherCriticalValidationType((IJobConsolCost)charge.ParentConsolCost, testCase);
		}

		public void TestCheckChargeLinkedToPostedConsolCostIsCostPosted_ChargeIsInDb()
		{
			AssertChargeLinkedToPostedConsolCostIsCostPosted_ChargeIsInDb(false);
		}

		public void TestCheckChargeLinkedToPostedConsolCostIsCostPosted_ChargeIsInDb_ButCosolCostPostedInThisSession()
		{
			AssertChargeLinkedToPostedConsolCostIsCostPosted_ChargeIsInDb(true);
		}

		void AssertChargeLinkedToPostedConsolCostIsCostPosted_ChargeIsInDb(bool isConsolCostPostedInThisFactory)
		{
			var charge = CreateJobChargeLinkedToJob(Factory, 1);
			var charge2 = CreateJobChargeLinkedToJob(Factory, 2);
			charge2.JR_AC = charge.JR_AC;
			Factory.Save();

			var anotherFactory = isConsolCostPostedInThisFactory ? Factory : new BusinessObjectFactory { RefreshEnabled = false };
			CreateConsolCostLinkedToCharge(anotherFactory, true, anotherFactory.Load<JobCharge>(charge.PK), true);
			if (!isConsolCostPostedInThisFactory)
			{
				using (SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
				{
					anotherFactory.Save();

					Assert("Precondition: charge cost is not posted.", !charge.IsCostPosted);
					Assert("Precondition: charge AP line is not empty.", !charge.JR_AL_APLine.IsEmpty);

					charge.APLine.AL_ReverseDate = ZDateTime.Now;
					charge.JR_AL_APLine = ZGuid.Empty;
					Assert("Precondition: charge AP line has changes.", charge.JR_AL_APLineInfo.HasChanges);

					var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Charge AP Line field value was not changed from cost line PK and so our production code done nothing wrong.");
					AssertOnSavingCheck(charge, testCase);

					charge.Reload();
				}
			}

			Assert("Precondition: charge cost is posted.", charge.IsCostPosted);
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_AL_APLine] = DBNull.Value; //we can't use related bizo property setter as our production code prevents such invalid changes, but we want check we will have an error in this case
			Assert("Precondition: charge AP line has changes.", charge.JR_AL_APLineInfo.HasChanges);

			if (isConsolCostPostedInThisFactory)
			{
				AssertNoErrorForUnpostedChargeButHasErrosForLinkedConsolCostPostedInThisFactory(charge);
			}
			else
			{
				AssertErrorForUnpostedChargeLinkedToPostedConsolCost(charge);
			}

			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 4m;
			charge2.JR_OSCostAmt = charge2.JR_LocalCostAmt = 6m;
			charge2.JR_E6 = charge.ParentConsolCost.PK;
			if (isConsolCostPostedInThisFactory)
			{
				AssertErrorForLinkedChargeButErrorForConsolCostPostedInThisFactory(charge2, charge);
			}
			else
			{
				AssertErrorForChargeLinkedToPostedConsolCost(charge2);
			}
		}

		void AssertNoErrorForUnpostedChargeButHasErrosForLinkedConsolCostPostedInThisFactory(JobCharge charge)
		{
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Cost APLine was reset to null, when consol cost is still posted, but consol cost is posted here and so it should report it.");
			AssertOnSavingCheck(charge, testCase);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Consol cost is posted here and so it should report that linked change is not posted.", true,
				CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4, "Non Cost posted Apportion Split Charge is linked to posted Consol Cost.",
				"Job Consol Cost:\r\n\tPK =", "Charge with incorrect data:", "JobConsolCost Reloaded:", "Problem Charge Reloaded:");
			AssertOnSavingCheckForAnotherCriticalValidationType((IJobConsolCost)charge.ParentConsolCost, testCase);
		}

		void AssertErrorForUnpostedChargeLinkedToPostedConsolCost(JobCharge charge)
		{
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Cost APLine was reset to null, when consol cost is still posted.", true,
				CriticalValidationErrorType.JobChargeReferenceToPostedTransactionLineCannotBeChanged_3, "Posted CST line reference can't be changed", "Charge: PK = ", "Line: PK = ");
			AssertOnSavingCheck(charge, testCase);
		}

		void AssertErrorForLinkedChargeButErrorForConsolCostPostedInThisFactory(JobCharge charge, JobCharge initialCharge)
		{
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
							"Job Charge Invoice Details not equal Consol Cost will fail critical validation",
							true,
							CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
							CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_JobChargeErrorMessage(string.Empty));
			AssertOnSavingCheck(charge, testCase);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("consol cost is posted here and so it should report an error.", true,
				CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details",
				"Mismatched fields are listed below -->", "Job Consol Cost:\r\n\tPK =", "Charge with incorrect data:");
			AssertOnSavingCheckForAnotherCriticalValidationType((IJobConsolCost)charge.ParentConsolCost, testCase);

			//populate posting details just to test non posted charge linked to posted consol cost error from consol cost side for this case
			//otherwise another critical validation asserted above happens before this one
			charge.JR_APInvoiceNum = initialCharge.JR_APInvoiceNum;
			charge.JR_APInvoiceDate = initialCharge.JR_APInvoiceDate;
			charge.JR_OH_CostAccount = initialCharge.JR_OH_CostAccount;
			charge.JR_PaymentDate = initialCharge.JR_PaymentDate;
			charge.JR_CostReference = initialCharge.JR_CostReference;
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("consol cost is posted here and so it should report that linked change is not posted.", true,
				CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4, "Non Cost posted Apportion Split Charge is linked to posted Consol Cost.",
				"Job Consol Cost:\r\n\tPK = ", "Charge with incorrect data:", "JobConsolCost Reloaded:", "Problem Charge Reloaded:");
			AssertOnSavingCheckForAnotherCriticalValidationType((IJobConsolCost)charge.ParentConsolCost, testCase);
		}

		void AssertErrorForChargeLinkedToPostedConsolCost(JobCharge charge)
		{
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
							"Job Charge Invoice Details not equal Consol Cost will fail critical validation",
							true,
							CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
							CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_JobChargeErrorMessage(string.Empty));
			AssertOnSavingCheck(charge, testCase);

			charge.JR_APInvoiceNum = (ZString)charge.ParentConsolCost[JobConsolCostSchema.E6_InvoiceNum.Name];
			charge.JR_APInvoiceDate = (ZDateTime)charge.ParentConsolCost[JobConsolCostSchema.E6_InvoiceDate.Name];
			charge.JR_OH_CostAccount = (ZGuid)charge.ParentConsolCost[JobConsolCostSchema.E6_OH_Creditor.Name];
			charge.JR_PaymentDate = (ZDateTime)charge.ParentConsolCost[JobConsolCostSchema.E6_PaymentDate.Name];
			charge.JR_CostReference = (ZString)charge.ParentConsolCost[JobConsolCostSchema.E6_CostReference.Name];

			var expectedMessage = $@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {charge.Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10.0000, GST is Overridden = Yes, OS GST Amount = 0.0000, Exchange Rate = 0.000000000, Local Cost Amount = 10.0000, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
Apportionment Charges (2):";
			var expectedMessage2 = "Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			var expectedMessage3 = "Charge: PK = 1946d803-fd83-4370-b58d-64ea57330c75";
			var expectedMessage4 = @"Charge with incorrect data:
Charge: PK = 1946d803-fd83-4370-b58d-64ea57330c75";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Not posted charge was linked to posted consol cost.", true, CriticalValidationErrorType.JobChargeLinkedToPostedConsolCostIsNotCostPosted_3, "Non Cost posted Apportion Split Charge is linked to posted Consol Cost", expectedMessage, expectedMessage2, expectedMessage3, expectedMessage4);
			AssertOnSavingCheck(charge, testCase);
		}

		#endregion

		public void TestCheckChargeLinkedToPostedConsolCostIsPostedToSameAPInvoice()
		{
			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToPostedConsolCostIsPostedToSameAPInvoice, all posted to same Invoice");
			AssertOnSavingCheck(charge, testCase);

			charge.APLine.AL_AH = ZGuid.NewZGuid();

			string expectedMessage = Invariant(
$@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {charge.Factory._Instance}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6d935954-0e05-4fab-a93b-4f344ed529de, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
");
			string expectedMessage2 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToPostedConsolCostIsCostPosted, AL_AH is not equal to E6_AH_APInvoice", true, CriticalValidationErrorType.JobChargeLinkedToPostedConsolCostIsPostedToDifferentInvoice_4, "Consol Cost and linked Apportion Split Charge are posted to different AP Invoices", expectedMessage, expectedMessage2);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckChargeLinkedToPostedConsolCostIsPostedToSameAPInvoiceForJRJ()
		{
			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);

			charge.APLine.AL_AH = ZGuid.NewZGuid();

			var revJournal = CreateInvoice(Factory);

			revJournal.AH_Ledger = LedgerTypes.JobCosting;
			revJournal.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			revJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
			charge.APLine.AL_AH = revJournal.PK;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToPostedConsolCostIsCostPosted");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckChargeLinkedToUnpostedConsolCostIsNotCostPosted()
		{
			var charge = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckCheckChargeLinkedToPostedConsolCostIsCostPosted, all posted");
			AssertOnSavingCheck(charge, testCase);

			var consol = Factory.Load(ObjectFactory.GetType<IJobConsolCost>(), charge.JR_E6);
			consol[JobConsolCostSchema.Constants.E6_AH_APInvoice] = ZGuid.Empty;

			string expectedMessage = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 10, GST is Overridden = No, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 10, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = , Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e", charge.Factory._Instance);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToPostedConsolCostIsCostPosted, Consol is not posted", true, CriticalValidationErrorType.JobChargeLinkedToUnpostedConsolCostIsCostPosted_3, "Cost posted Apportion Split Charge is linked to unposted Consol Cost", expectedMessage);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestChargeAPLineLinkedToAutoJobRevenueJournal()
		{
			// Simulate an Auto Job Revenue Journal
			var revJournal = CreateInvoice(Factory);
			revJournal.AH_Ledger = LedgerTypes.JobCosting;
			revJournal.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			revJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
			// With two Revenue Lines
			var revOnAP = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			var revOnAR = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			revOnAP.AL_AH = revJournal.PK;
			revOnAR.AL_AH = revJournal.PK;

			revOnAP.AL_RX_NKTransactionCurrency = "USD";
			revOnAP.AL_OSAmount = -50m;
			revOnAP.AL_ExchangeRate = 0.5m;
			revOnAP.AL_LineAmount = -100m;

			revOnAR.AL_RX_NKTransactionCurrency = "USD";
			revOnAR.AL_OSAmount = 50m;
			revOnAR.AL_ExchangeRate = 0.5m;
			revOnAR.AL_LineAmount = 100m;

			var chargeAP = CreateJobCharge(Factory);
			var chargeAR = CreateJobCharge(Factory);
			// Set JobCharge AP Line Link to Journal's Revenue Line
			chargeAP.JR_AL_APLine = revOnAP.PK;
			chargeAR.JR_AL_ARLine = revOnAR.PK;
			chargeAP.SetChargeValuesFromLinkedAPLineForTests();
			chargeAR.SetChargeValuesFromLinkedARLineForTests();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToAutoJobRevenueJournal - No Errors");
			AssertOnSavingCheck(chargeAP, testCase);

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			Assert(((ZGuid)consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice]).IsEmpty);
			chargeAP.JR_E6 = consolCost.PK;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToUnpostedConsolCostIsNotCostPosted - No Errors");
			AssertOnSavingCheck(chargeAP, testCase);
			// Consol Cost clean up as we are going to save Factory later
			chargeAP.JR_E6 = ZGuid.Empty;
			consolCost.Delete();

			chargeAP.JR_OSCostAmt = 33m;
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToAutoJobRevenueJournal - Not Equal Amounts Error", true, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				"Related REV amount is not the same as charge amount.",
				"Charge: PK = " + chargeAP.PK.ToString(), ", OS Cost Amount = 33, Local Cost Amount = 66, OS Cost Exchange Rate = 0.5,",
				"InternalFields: InternalJob = , InternalBranch = , InternalDepartment = ",
				"Line: PK = " + revOnAP.PK.ToString(), ", Type = REV, OS Amount = -50, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0.5, Currency = USD,",
				"Header: PK = " + revJournal.PK.ToString(), ", Ledger = JC, Transaction Type = JRJ");
			AssertOnSavingCheck(chargeAP, testCase);

			chargeAP.JR_OSCostAmt = 50m;
			chargeAP.JR_AT_CostGSTRate = TaxRate.PK;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToAutoJobRevenueJournal - Not Equal Tax Rates Error", true, CriticalValidationErrorType.JobChargeTaxCodeNotEqualRelatedLineTaxCode_7,
				"Related REV line tax code and class are not the same as charge tax code and class.",
				"Charge: PK = " + chargeAP.PK.ToString(), ", OS Cost Amount = 50, Local Cost Amount = 100, OS Cost Exchange Rate = 0.5,", ", Is Cost Posted = Yes,", ", Cost GST Rate = GST,",
				"Line: PK = " + revOnAP.PK.ToString(), ", Type = REV, OS Amount = -50, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0.5, Currency = USD,",
				"Header: PK = ");
			AssertOnSavingCheck(chargeAP, testCase);

			chargeAP.JR_AT_CostGSTRate = ZGuid.Empty;
			Factory.Save();

			((IBusinessObjectInternals)chargeAP).Row[JobChargeSchema.Constants.JR_AL_APLine] = System.DBNull.Value;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToAutoJobRevenueJournal - Link To Posted Transaction Cannot Be Changed", true,
				CriticalValidationErrorType.JobChargeReferenceToPostedTransactionLineCannotBeChanged_3,
				"Posted REV line reference can't be changed.",
				"Charge: PK = " + chargeAP.PK.ToString(), ", OS Cost Amount = 50, Local Cost Amount = 100, OS Cost Exchange Rate = 0.5,", ", Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000,",
				"Line: PK = " + revOnAP.PK.ToString(), ", Type = REV, OS Amount = -50, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0.5, Currency = USD,");
			AssertOnSavingCheck(chargeAP, testCase);

			revJournal.AH_IsCancelled = true;
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToAutoJobRevenueJournal - No Errors (Link to Reversed Transaction Line Can Be Cleared)");
			AssertOnSavingCheck(chargeAP, testCase);

			chargeAP.JR_AL_APLine = revOnAP.PK;
			revJournal.AH_IsCancelled = false;
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToAutoJobRevenueJournal -  - Charge Cannot BE Deleted While Referring Line Of Posted Auto Job Revenue Journal", true,
				CriticalValidationErrorType.JobChargeRelatedToPostedTransactionLineCannotBeDeleted_4,
				"Charge deleted with posted REV.",
				$"PK = {chargeAP.PK}",
				"Business Contexts",
				"Properties:",
				"JobChargeConstructorStackTrace:",
				"JobChargeDeleteStackTrace");
			AssertDeletedObjectOnSavingCheck(chargeAP, testCase);
		}

		public void TestChargeLineLinkedToJobRevenueJournal()
		{
			// Simulate an Job Revenue Journal
			var revJournal = CreateInvoice(Factory);
			revJournal.AH_Ledger = LedgerTypes.JobCosting;
			revJournal.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			revJournal.AH_TransactionCategory = Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
			// With two Lines AR -> CST  AP -> REV , AR-> CST only appears in AUTO JRJ
			var revOnAP = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			var revOnAR = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			revOnAP.AL_AH = revJournal.PK;
			revOnAR.AL_AH = revJournal.PK;

			revOnAP.AL_RX_NKTransactionCurrency = "USD";
			revOnAP.AL_OSAmount = -50m;
			revOnAP.AL_ExchangeRate = 0.5m;
			revOnAP.AL_LineAmount = -100m;

			revOnAR.AL_RX_NKTransactionCurrency = "USD";
			revOnAR.AL_OSAmount = 50m;
			revOnAR.AL_ExchangeRate = 0.5m;
			revOnAR.AL_LineAmount = 100m;

			var chargeAP = CreateJobCharge(Factory);
			var chargeAR = CreateJobCharge(Factory);
			// Set JobCharge AP Line Link to Journal's Revenue Line
			chargeAP.JR_AL_APLine = revOnAP.PK;
			chargeAR.JR_AL_ARLine = revOnAR.PK;
			chargeAP.SetChargeValuesFromLinkedAPLineForTests();
			chargeAR.SetChargeValuesFromLinkedARLineForTests();
			Factory.Save();

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount);

			chargeAP.JR_OSCostAmt = 43m;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeAPLineLinkedToJobRevenueJournal - Not Equal Amounts Error", true, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				"Related REV amount is not the same as charge amount.",
				"Charge: PK = " + chargeAP.PK.ToString(),
				", OS Cost Amount = 43, Local Cost Amount = 86, OS Cost Exchange Rate = 0.5,",
				"InternalFields: InternalJob = , InternalBranch = , InternalDepartment = ",
				"Line: PK = " + revOnAP.PK.ToString(),
				", Type = REV, OS Amount = -50, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0.5, Currency = USD,",
				"Header: PK = " + revJournal.PK.ToString(), ", Ledger = JC, Transaction Type = JRJ",
				@"JobChargeLocalCostAmtNotEqualRelatedLineAmount:

JR_LocalCostAmt has been changed from 100 to 86 after the cost posted.
   at");
			AssertOnSavingCheck(chargeAP, testCase);

			chargeAR.JR_OSSellAmt = 43m;
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeARLineLinkedToJobRevenueJournal - Not Equal Amounts Error", true, CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				"Related CST amount is not the same as charge amount.",
				"Charge: PK = " + chargeAR.PK.ToString(),
				", OS Sell Exchange Rate = 0.5, OS Sell Amount = 43, Local Sell Amount = 86,",
				"InternalFields: InternalJob = , InternalBranch = , InternalDepartment = ",
				"Line: PK = " + revOnAR.PK.ToString(),
				", Type = CST, OS Amount = 50, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0.5, Currency = USD,",
				"Header: PK = " + revJournal.PK.ToString(), ", Ledger = JC, Transaction Type = JRJ",
				@"JobChargeLocalSellAmtNotEqualRelatedLineAmount:

JR_LocalSellAmt has been changed from 100 to 86 after the revenue posted.
   at");
			AssertOnSavingCheck(chargeAR, testCase);
		}

		public void TestJobChargeAmountNotEqualRelatedLineAmount_TransactionLineAmountChangeWhenItsLinkedToPostedCharge_TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge()
		{
			var charge = CreateJobCharge(Factory);
			Factory.Save();

			var journal = CreateInvoice(Factory);
			journal.AH_Ledger = LedgerTypes.JobCosting;
			journal.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
			var journalLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			journalLine.AL_AH = journal.PK;

			journalLine.AL_RX_NKTransactionCurrency = "USD";
			journalLine.AL_OSAmount = -50m;
			journalLine.AL_ExchangeRate = 0.5m;
			journalLine.AL_LineAmount = -100m;
			journalLine.AL_JH = charge.JR_JH;

			charge.JR_AL_ARLine = journalLine.PK;
			charge.SetChargeValuesFromLinkedARLineForTests();

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);

			journalLine.AL_OSAmount = -40m;
			journalLine.AL_LineAmount = -110m;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods(
				"Test line's extra info for critical validation JobChargeAmountNotEqualRelatedLineAmount.",
				true,
				CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				"Related REV amount is not the same as charge amount.",
				"Charge: PK = " + charge.PK.ToString(),
				"InternalFields: InternalJob = , InternalBranch = , InternalDepartment = ",
				"Line: PK = " + journalLine.PK.ToString(),
				$"Header: PK = {journal.PK.ToString()}, Ledger = JC, Transaction Type = JRJ",
				@"TransactionLineAmountChangeWhenItsLinkedToPostedCharge:
AL_LineAmount has been changed from -100 to -110 after Job charge creation.
JobCharge LocalAmount: -100

StackTrace:",
				@"TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge:
AL_OSAmount has been changed from -50 to -40 after Job charge creation.
JobCharge OSAmount: -50

StackTrace:"
			));
		}

		public void TestJR_LocalSellAmtSet_JobChargeLocalSellAmtNotEqualRelatedLineAmountWhenARLineNotInDataBase()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = CreateJobCharge(Factory);
			jobCharge.JR_JH = job.PK;

			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;

			var invoiceLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			invoiceLine.AL_AH = invoice.PK;
			invoiceLine.AL_OSAmount = invoiceLine.AL_LineAmount = 20m;
			jobCharge.JR_AL_ARLine = invoiceLine.PK;

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount);
			jobCharge.JR_LocalSellAmt = 10m;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeARLineLinkedToJobRevenueJournal - Not Equal Amounts Error",
				true,
				CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
				"Related REV amount is not the same as charge amount.",
				"Charge: PK = " + jobCharge.PK.ToString(),
				"InternalFields: InternalJob = , InternalBranch = , InternalDepartment = ",
				"Line: PK = " + invoiceLine.PK.ToString(),
				//$"Header: PK = {invoice.PK.ToString()}, Ledger = AR, Transaction Type = INV",
				@"JobChargeLocalSellAmtNotEqualRelatedLineAmount:

JR_LocalSellAmt has been changed from 0 to 10 after the revenue posted.
   at");
			AssertOnSavingCheck(jobCharge, testCase);
		}

		public void TestChargeCFXLine()
		{
			var parent = CreateJobCharge(Factory);
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.FillWithValidTestData();
			parent.JR_JH = job.PK;

			var revenueLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			parent.JR_AL_ARLine = revenueLine.PK;
			revenueLine.AL_JH = parent.JR_JH;
			revenueLine.AL_AC = parent.JR_AC;
			parent.SetChargeValuesFromLinkedARLineForTests();

			var invoice = CreateInvoice(Factory);
			revenueLine.AL_AH = invoice.PK;

			var cfxLine = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			parent.JR_AL_CFXLine = cfxLine.PK;
			cfxLine.AL_JH = parent.JR_JH;
			cfxLine.AL_AC = parent.JR_AC;
			cfxLine.AL_OSAmount = cfxLine.AL_LineAmount = -10m;

			var cfxJournal = CreateInvoice(Factory);
			cfxJournal.AH_Ledger = LedgerTypes.JobCosting;
			cfxJournal.AH_TransactionType = TransactionTypes.Journal;
			cfxLine.AL_AH = cfxJournal.PK;

			Factory.Save();

			parent.JR_AL_CFXLine = ZGuid.Empty;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCFXLine - Link Cannot Be Changed While CFX Is Posted", true, CriticalValidationErrorType.JobChargeReferenceToPostedTransactionLineCannotBeChanged_3,
				"Posted REV line reference can't be changed.",
				"Charge: PK = " + parent.PK.ToString(), "Is Revenue Posted = Yes, AR Line = " + revenueLine.PK.ToString() + ", CFX Line = 00000000-0000-0000-0000-000000000000,",
				"Line: PK = " + cfxLine.PK.ToString(), ", Type = REV, OS Amount = -10, Local Amount = -10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD,", ", Header PK = " + cfxJournal.PK.ToString());
			AssertOnSavingCheck(parent, testCase);

			cfxJournal.AH_IsCancelled = true;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCFXLine - No Errors (Link to Reversed CFX Line Can Be Cleared)");
			AssertOnSavingCheck(parent, testCase);

			parent.JR_AL_CFXLine = cfxLine.PK;
			cfxJournal.AH_IsCancelled = false;
			// Reverse REV linked to ARLine
			parent.JR_AL_ARLine = ZGuid.Empty;
			invoice.AH_IsCancelled = true;
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCFXLine - Charge Cannot BE Deleted While Referring Posted CFX", true, CriticalValidationErrorType.JobChargeRelatedToPostedTransactionLineCannotBeDeleted_4,
				"Charge deleted with posted REV.",
				$"PK = {parent.PK}",
				"Business Contexts",
				"Properties:",
				"JobChargeConstructorStackTrace:",
				"JobChargeDeleteStackTrace:");
			AssertDeletedObjectOnSavingCheck(parent, testCase);
		}

		public void TestChargeOSAndLocalSellAmount()
		{
			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			Factory.Save();

			charge.JR_LocalSellAmt = 50;
			charge.JR_OSSellExRate = 1m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_OSSellAmt] = decimal.Zero;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellAmount", true, CriticalValidationErrorType.JobChargeOsSellAmountNotEqualLocalSellAmount_8,
				"OS sell amount should be same as local sell amount when Local Currency is used.", "Charge: PK = " + charge.PK.ToString(), ", OS Sell Exchange Rate = " + charge.JR_OSSellExRate, ", OS Sell Amount = " + charge.JR_OSSellAmt + ", Local Sell Amount = " + charge.JR_LocalSellAmt);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestJobChargeNegativeRevenueIsNotPermitted()
		{
			var testDataItems = new[]
			{
				new { CriticalCheckShouldFail = false, AllowNegativeRevenueChargesOnJob = true, OSSellAmt = 50m, IsInDatabase = false, HasChanges = false },
				new { CriticalCheckShouldFail = false, AllowNegativeRevenueChargesOnJob = true, OSSellAmt = -50m, IsInDatabase = false, HasChanges = false },
				new { CriticalCheckShouldFail = false, AllowNegativeRevenueChargesOnJob = false, OSSellAmt = 50m, IsInDatabase = false, HasChanges = false },
				new { CriticalCheckShouldFail = true, AllowNegativeRevenueChargesOnJob = false, OSSellAmt = -50m, IsInDatabase = false, HasChanges = false },
				new { CriticalCheckShouldFail = false, AllowNegativeRevenueChargesOnJob = false, OSSellAmt = -50m, IsInDatabase = true, HasChanges = false },
				new { CriticalCheckShouldFail = true, AllowNegativeRevenueChargesOnJob = false, OSSellAmt = -50m, IsInDatabase = true, HasChanges = true },
			};

			var someOtherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			charge.JR_OSSellExRate = 1m;

			foreach (var registrySettingGroup in testDataItems.GroupBy(testDataItem => testDataItem.AllowNegativeRevenueChargesOnJob))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(someOtherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registrySettingGroup.Key))
				{
					foreach (var testDataItem in registrySettingGroup)
					{
						if (testDataItem.IsInDatabase)
						{
							charge.JR_OSSellAmt = testDataItem.OSSellAmt + (testDataItem.HasChanges ? 1 : 0);

							using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
							{
								Factory.Save();
							}
						}

						charge.JR_OSSellAmt = testDataItem.OSSellAmt;

						charge.JR_GC = someOtherCompany.PK;
						AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods(
							"TestNegativeRevenueChargesOnJobWhenNotPermitted",
							false,
							CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted,
							CriticalValidationMessageTemplate.JobChargeNegativeRevenueIsNotPermittedErrorMessage));

						charge.JR_GC = Env.CurrentCompanyPK;
						AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods(
							"TestNegativeRevenueChargesOnJobWhenNotPermitted",
							testDataItem.CriticalCheckShouldFail,
							CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted,
							CriticalValidationMessageTemplate.JobChargeNegativeRevenueIsNotPermittedErrorMessage));

						if (testDataItem.IsInDatabase)
						{
							charge = CreateJobCharge(Factory);
							job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
							job.FillWithValidTestData();
							charge.JR_JH = job.PK;
							charge.JR_OSSellExRate = 1m;
						}
					}
				}
			}
		}

		public void TestChargeOSAndLocalSellAmount_ShouldHaveSameSign_ForceOS()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			var charge = CreateJobCharge(Factory);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKSellCurrency = "USD";
			var exchangeRateUSD = ((IExchangeRateSourceBase)job).FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			charge.JR_LocalSellAmt = 50;
			charge.JR_OSSellExRate = 1m;
			decimal osAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_OSSellAmt] = osAmount;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellAmount", true, CriticalValidationErrorType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount_5,
				string.Format("OS Sell Amount and Local Sell Amount should have the same sign.\r\nOS Sell Amount: {0}, Local Sell Amount: {1}.", osAmount, charge.JR_LocalSellAmt));
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestChargeOSAndLocalSellAmount_ShouldHaveSameSign_ForceLocal()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			var charge = CreateJobCharge(Factory);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKSellCurrency = "USD";
			var exchangeRateUSD = ((IExchangeRateSourceBase)job).FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			charge.JR_OSSellAmt = 50;
			charge.JR_OSSellExRate = 1m;
			var localAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_LocalSellAmt] = localAmount;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellAmount", true, CriticalValidationErrorType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount_5,
				string.Format("OS Sell Amount and Local Sell Amount should have the same sign.\r\nOS Sell Amount: {0}, Local Sell Amount: {1}.", charge.JR_OSSellAmt, localAmount));
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestSaveWhenNonDecimalPlacesLocalSellAmountIsConvertedFromButNotEqualToOsSellAmount()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";

			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();

			job.Company.GC_RX_NKLocalCurrency = "JPY";

			charge.JR_JH = job.PK;

			Factory.Save();

			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_LocalSellAmt = 123m;
			charge.JR_OSSellExRate = 1m;
			charge.JR_OSSellAmt = 123.4m;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellAmount", false, CriticalValidationErrorType.JobChargeOsSellAmountNotEqualLocalSellAmount_8,
				"OS sell amount should be same as local sell amount when Local Currency is used.", "Charge: PK = " + charge.PK.ToString(), ", OS Sell Exchange Rate = " + charge.JR_OSSellExRate, ", OS Sell Amount = " + charge.JR_OSSellAmt + ", Local Sell Amount = " + charge.JR_LocalSellAmt);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestChargeOSAndLocalSellAmountCheckUsesChargeCompanyToGetLocalCurrency()
		{
			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			Factory.Save();

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));

			charge.JR_OSSellAmt = 50m;
			charge.JR_RX_NKSellCurrency = otherCompany.GC_RX_NKLocalCurrency;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_LocalSellAmt] = 25m;
			AssertNotEquals(charge.JR_LocalSellAmt, charge.JR_OSSellAmt);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellAmount uses correct Local Currency");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertOnSavingCheck(charge, testCase);
			}
		}

		public void TestChargeOSAndLocalCostAmountCheckUsesChargeCompanyToGetLocalCurrency()
		{
			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			Factory.Save();

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));

			charge.JR_OSCostAmt = 50m;
			charge.JR_RX_NKCostCurrency = otherCompany.GC_RX_NKLocalCurrency;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_LocalCostAmt] = 25m;
			AssertNotEquals(charge.JR_LocalCostAmt, charge.JR_OSCostAmt);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCostAmount uses correct Local Currency");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertOnSavingCheck(charge, testCase);
			}
		}

		public void TestChargeOSAndLocalCostAmount()
		{
			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			Factory.Save();

			charge.JR_LocalCostAmt = 50m;
			charge.JR_OSCostExRate = 1m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_OSCostAmt] = decimal.Zero;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCostAmount", true, CriticalValidationErrorType.JobChargeOsCostAmountNotEqualLocalCostAmount_4,
				"OS cost amount should be same as local cost amount when Local Currency is used.", "Charge: PK = " + charge.PK.ToString(), ", OS Cost Amount = " + charge.JR_OSCostAmt + ", Local Cost Amount = " + charge.JR_LocalCostAmt, ", OS Cost Exchange Rate = " + charge.JR_OSCostExRate);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckOSAndLocalAmountSign_CollectedInfoIsAlwaysReported()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = CreateJobCharge(Factory);
			jobCharge.JR_JH = job.PK;

			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;

			var invoiceLine = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			invoiceLine.AL_AH = invoice.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.AL_OSAmount = 50m;
			invoiceLine.AL_ExchangeRate = 0.5m;
			invoiceLine.AL_LineAmount = 100m;

			jobCharge.JR_AL_APLine = invoiceLine.PK;
			jobCharge.SetChargeValuesFromLinkedAPLineForTests();

			jobCharge.JR_OSCostAmt = 100m;
			jobCharge.JR_OSCostExRate = 1m;
			((IBusinessObjectInternals)jobCharge).Row[JobChargeSchema.Constants.JR_LocalCostAmt] = -100m;
			AssertNotEquals(Math.Sign(jobCharge.JR_OSCostAmt), Math.Sign(jobCharge.JR_LocalCostAmt));

			Factory.ServiceContainer.RemoveService<CriticalValidationInfoCollectorService>();

			var infoCollectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull($"Precondition: {nameof(infoCollectorService)}", infoCollectorService);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckOSAndLocalAmountSign, OS and Local Amounts signs not matching",
							true,
							CriticalValidationErrorType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount_3,
							"OS Cost Amount and Local Cost Amount should have the same sign",
							"JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount: There was no attempt to collect any data.",
							"CarryForwardChargeAmountWithDifferentSigns: There was no attempt to collect any data.");

			AssertOnSavingCheck(jobCharge, testCase);
		}

		public void TestChargeOsSellAmountNotMatchingSignOfLocalSellAmount()
		{
			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			Factory.Save();

			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_LocalSellAmt = 100;
			charge.JR_OSSellExRate = -0.5m;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellExRate", true, CriticalValidationErrorType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount_5,
				@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: OS Sell Amount and Local Sell Amount should have the same sign.
OS Sell Amount: 100, Local Sell Amount: -200.", "JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount");
			AssertOnSavingCheck(charge, testCase);

			charge.JR_OSSellExRate = -1m;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeSellExRate", true, CriticalValidationErrorType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount_5,
				@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: OS Sell Amount and Local Sell Amount should have the same sign.
OS Sell Amount: 100, Local Sell Amount: -100.", "get_StackTrace()");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestChargeOSAndLocalCostAmount_ShouldHaveSameSign_ForceOS()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			var charge = CreateJobCharge(Factory);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKCostCurrency = "USD";
			var exchangeRateUSD = ((IExchangeRateSourceBase)job).FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			var line = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			charge.JR_AL_APLine = line.PK;
			line.AL_JH = charge.JR_JH;
			line.AL_AC = charge.JR_AC;

			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_OSAmount = 50m;
			line.AL_ExchangeRate = 0.5m;
			line.AL_LineAmount = 100m;

			charge.SetChargeValuesFromLinkedAPLineForTests();
			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			line.AL_AH = invoice.PK;
			Factory.Save();

			charge.JR_LocalCostAmt = 50m;
			charge.JR_OSCostExRate = 1m;
			decimal osAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_OSCostAmt] = osAmount;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCostAmount", true, CriticalValidationErrorType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount_3,
				string.Format("OS Cost Amount and Local Cost Amount should have the same sign.\r\nOS Cost Amount: {0}, Local Cost Amount: {1}.", osAmount, charge.JR_LocalCostAmt),
				"Developer Details (Critical Validation Failure):",
				"OS Cost Amount and Local Cost Amount should have the same sign.",
				"OS Cost Amount: -50, Local Cost Amount: 25.0.",
				"Charge with incorrect data:",
				$"Charge: PK = {charge.PK}, Type = Charge, Charge Type = , Job PK = {job.PK}, Job Number = {job.JH_JobNum}, Charge Code = {line.ChargeCode.AC_Code}, Charge Code Type = , Cost Account = , OS Cost Amount = -50, Local Cost Amount = 25.0, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = Yes, AP Line = {line.PK}, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = USD, Sell Currency = USD, Is In DB = Yes, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 0, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.",
				"Fields with changes: JR_AgentDeclaredCostAmt (-50, 25.0), JR_LocalCostAmt (-100, 25.0), JR_OSCostExRate (0.5, 1).",
				$"Line: PK = {line.PK}, Charge Code = {line.ChargeCode.AC_Code}, GL Account = {line.GLHeader.AG_AccountNum}, Type = CST, OS Amount = 50, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0.5, Currency = USD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {invoice.PK}, Job PK = {job.PK}, Organization = , Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = No.",
				"JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount:",
				"JR_LocalCostAmt has been changed from -100 to 50.",
				"get_StackTrace()");
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestChargeOSAndLocalCostAmount_ShouldHaveSameSign_ForceLocal()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.CarryForwardChargeAmountWithDifferentSigns);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			var charge = CreateJobCharge(Factory);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKCostCurrency = "USD";
			var exchangeRateUSD = ((IExchangeRateSourceBase)job).FirstOrDefault(x => x.CurrencyCode == "USD");
			AssertNotNull(exchangeRateUSD);
			exchangeRateUSD.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			var line = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			charge.JR_AL_APLine = line.PK;
			line.AL_JH = charge.JR_JH;
			line.AL_AC = charge.JR_AC;

			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_OSAmount = 50m;
			line.AL_ExchangeRate = 0.5m;
			line.AL_LineAmount = 100m;

			charge.SetChargeValuesFromLinkedAPLineForTests();
			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			line.AL_AH = invoice.PK;
			Factory.Save();

			charge.JR_OSCostAmt = 50m;
			charge.JR_OSCostExRate = 1m;
			var localAmount = -50m;
			((IBusinessObjectInternals)charge).Row[JobChargeSchema.Constants.JR_LocalCostAmt] = localAmount;

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(charge.PK, CriticalValidationInfoCollectorServiceKeyType.CarryForwardChargeAmountWithDifferentSigns, () => "Test Error Message reported when Carry Forward Charge Amount With Different Signs.");

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCostAmount", true, CriticalValidationErrorType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount_3,
				string.Format("OS Cost Amount and Local Cost Amount should have the same sign.\r\nOS Cost Amount: {0}, Local Cost Amount: {1}.", charge.JR_OSCostAmt, localAmount),
				"Developer Details (Critical Validation Failure):",
				"OS Cost Amount and Local Cost Amount should have the same sign.",
				"OS Cost Amount: 50, Local Cost Amount: -50.",
				"Charge with incorrect data:",
				$"Charge: PK = {charge.PK}",
				"Fields with changes: JR_AgentDeclaredCostAmt (-50, 50), JR_LocalCostAmt (-100, -50), JR_OSCostAmt (-50, 50), JR_OSCostExRate (0.5, 1).",
				$"Line: PK = {line.PK}",
				"JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount:\r\nJR_OSCostAmt has been changed from -50 to 50.",
				"CarryForwardChargeAmountWithDifferentSigns:\r\nTest Error Message reported when Carry Forward Charge Amount With Different Signs.");
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestSaveWhenNonDecimalPlacesLocalCostAmountIsConvertedFromButNotEqualToOsCostAmount()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";

			var charge = CreateJobCharge(Factory);
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.FillWithValidTestData();
			job.Company.GC_RX_NKLocalCurrency = "JPY";

			charge.JR_JH = job.PK;

			Factory.Save();

			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_LocalCostAmt = 123m;
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSCostAmt = 123.4m;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestChargeCostAmount", false, CriticalValidationErrorType.JobChargeOsCostAmountNotEqualLocalCostAmount_4,
				"OS cost amount should be same as local cost amount when Local Currency is used.", "Charge: PK = " + charge.PK.ToString(), ", OS Cost Amount = " + charge.JR_OSCostAmt + ", Local Cost Amount = " + charge.JR_LocalCostAmt, ", OS Cost Exchange Rate = " + charge.JR_OSCostExRate);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestNewChargeIsAddedToClosedJobHeaderThrowsException()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = CreateJobCharge(Factory);

			job.JH_Status = JobHeaderStatus.Closed.Code;
			charge.JR_JH = job.PK;

			ErrorReporter.Clear();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Should not save newly created charge(s) on a closed job",
				true,
				CriticalValidationErrorType.JobChargeLinkedToClosedJob,
				CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobErrorMessage,
				"Newly created job charge is being saved on a closed job.",
				"\r\n\r\nJob Status HasChanges: False (CLS)",
				"\r\nUser: E",
				"\r\nUser can re-open jobs: True, True",
				"\r\nJobChargeCreatedOnClosedJobStackTrace:",
				"\r\nJobChargeConstructorStackTrace:",
				"\r\nReopeningJobStackTrace:");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckChargeLinkedToClosedJob_CollectedInfoIsAlwaysReported()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.Closed.Code;

			var jobCharge = CreateJobCharge(Factory);
			jobCharge.JR_JH = job.PK;

			Factory.ServiceContainer.RemoveService<CriticalValidationInfoCollectorService>();

			var infoCollectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull($"Precondition: {nameof(infoCollectorService)}", infoCollectorService);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToClosedJob, New job Charge is being created on a closed job",
							true,
							CriticalValidationErrorType.JobChargeLinkedToClosedJob,
							"Newly created job charge is being saved on a closed job.",
							"Job Status HasChanges:",
							"\r\nUser: E",
							"\r\nUser can re-open jobs:",
							"\r\nLast Job Opened Time:",
							"\r\nLast Job Closed Time:",
							"\r\nLast Job Closed User:",
							"\r\n\r\nJobCharge:",
							"\r\n\r\nJobChargeCreatedOnClosedJobStackTrace: There was no attempt to collect any data.",
							"\r\n\r\nJobChargeConstructorStackTrace: There was no attempt to collect any data.",
							"\r\n\r\nReopeningJobStackTrace: There was no attempt to collect any data.");

			AssertOnSavingCheck(jobCharge, testCase);
		}

		public void TestCheckChargeLinkedToClosedJob_WhenCreatingProfitShareCharge()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.Closed.Code;

			var jobCharge = CreateJobCharge(Factory);
			jobCharge.JR_JH = job.PK;

			Factory.SetContext(BusinessContext.CreatingProfitShareCharge);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToClosedJob, New job Charge is being created on a closed job",
							true,
							CriticalValidationErrorType.JobChargeLinkedToClosedJobWhenCreatingProfitShareCharges,
							CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobWhenCreatingProfitShareChargesErrorMessage);

			AssertOnSavingCheck(jobCharge, testCase);
		}

		public void TestNewChargeIsAddedToReopenedJobHeaderDoesNotThrowException()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = CreateJobCharge(Factory);

			job.JH_Status = JobHeaderStatus.Closed.Code;
			charge.JR_JH = job.PK;
			job.JH_Status = JobHeaderStatus.Working.Code;

			ErrorReporter.Clear();

			var message = "Should not have thrown an exception because the job was re-opened before being saved. We don't care about this situation";
			AssertNoExceptionThrown(message, () => Factory.Save());
		}

		public void TestNewChargeIsAddedToSavedClosedJobHeaderThrowsException()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var charge = CreateJobCharge(Factory);
			charge.JR_JH = job.PK;
			ErrorReporter.Clear();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Should not save newly created charge(s) on a closed job",
				true,
				CriticalValidationErrorType.JobChargeLinkedToClosedJob,
				CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobErrorMessage,
				"Newly created job charge is being saved on a closed job.",
				"\r\nJob Status HasChanges: False (CLS)",
				"\r\nUser: E",
				"\r\nUser can re-open jobs: True, True",
				"\r\n\r\nJobChargeCreatedOnClosedJobStackTrace:",
				"\r\n\r\nJobChargeConstructorStackTrace:",
				"\r\n\r\nReopeningJobStackTrace:");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestNewChargeIsAddedToWorkingJobHeaderDoesNotThrowException()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = CreateJobCharge(Factory);

			charge.JR_JH = job.PK;
			job.JH_Status = JobHeaderStatus.Closed.Code;

			ErrorReporter.Clear();

			var message = "Should not have thrown an exception because the job was not closed at the time of being attached to the charge";
			AssertNoExceptionThrown(message, () => Factory.Save());
		}

		public void TestSaveWhenJobHeaderIsInactive()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();
			job.MarkAsInactive();

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			ErrorReporter.Clear();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckChargeLinkedToInactiveJobIsNotPermitted",
				true,
				CriticalValidationErrorType.JobChargeLinkedToInactiveJob,
				CriticalValidationMessageTemplate.JobChargeLinkedToInactiveJobErrorMessage);
			AssertOnSavingCheck(charge, testCase);
		}

		public void TestDeletedChargeShouldNotBeUpdatedByPostedAPInvcoieThroughDataRefreshBus()
		{
			var testFactory = new BusinessObjectFactory();
			var charge = CreateJobCharge(testFactory);
			var job = testFactory.NewJobForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			testFactory.Save();
			charge.Delete();

			var chargeCopy = Factory.Load<JobCharge>(charge.PK);
			var line = CreateTransactionLine(Factory, TransactionLineTypes.Cost);
			chargeCopy.JR_AL_APLine = line.PK;
			line.AL_JH = chargeCopy.JR_JH;
			line.AL_AC = chargeCopy.JR_AC;

			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_OSAmount = 50m;
			line.AL_ExchangeRate = 0.5m;
			line.AL_LineAmount = 100m;

			chargeCopy.SetChargeValuesFromLinkedAPLineForTests();
			var invoice = CreateInvoice(Factory);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			line.AL_AH = invoice.PK;
			Factory.Save();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Should not fail on Deleted Charge which was AP Posted in the same session as we skip Data Refresh.");
			AssertDeletedObjectOnSavingCheck(charge, testCase);
			AssertExceptionThrown<ZSaveConcurrencyException>("Should fail with Concurrency Exception instead", () => { testFactory.Save(); });
		}

		public void TestDeletedChargeShouldNotBeUpdatedByPostedARInvcoieThroughDataRefreshBus()
		{
			var testFactory = new BusinessObjectFactory();
			var charge = CreateJobCharge(testFactory);
			var job = testFactory.NewJobForTesting<JobHeader>();
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			testFactory.Save();
			charge.Delete();

			var chargeCopy = Factory.Load<JobCharge>(charge.PK);
			var line = CreateTransactionLine(Factory, TransactionLineTypes.Revenue);
			chargeCopy.JR_AL_ARLine = line.PK;
			line.AL_JH = chargeCopy.JR_JH;
			line.AL_AC = chargeCopy.JR_AC;

			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_OSAmount = 50m;
			line.AL_ExchangeRate = 0.5m;
			line.AL_LineAmount = 100m;

			chargeCopy.SetChargeValuesFromLinkedARLineForTests();
			var invoice = CreateInvoice(Factory);
			line.AL_AH = invoice.PK;
			Factory.Save();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Should not fail on Deleted Charge which was AR Posted in the same session as we skip Data Refresh.");
			AssertDeletedObjectOnSavingCheck(charge, testCase);
			AssertExceptionThrown<ZSaveConcurrencyException>("Should fail with Concurrency Exception instead", () => { testFactory.Save(); });
		}

		public void TestCheckJobChargeMoreThanOneFieldLinkToTheSameLine_NewCharge()
		{
			var charge = CreateJobCharge(Factory, new Guid("febb3f1c-0a28-4fd2-9709-8ba94e0195fe"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			var relatedLine = CreateTransactionLine(Factory, TransactionLineTypes.WIP, new Guid("209870e1-5458-45c6-81ed-075c098a9d3d"));
			charge.JR_AL_ARLine = relatedLine.PK;
			charge.JR_AL_APLine = relatedLine.PK;
			relatedLine.AL_JH = charge.JR_JH;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobCharge is not in database and has more than one field link to the same line.",
				true, CriticalValidationErrorType.JobChargeMoreThanOneFieldLinkToTheSameLine, "The AP Line and AR Line of a Job Charge can not be the same transaction line.",
				"JR_AL_APLine and JR_AL_ARLine of the Job Charge are linked to the same transaction line.",
				"JobCharge Info:\tPK = febb3f1c-0a28-4fd2-9709-8ba94e0195fe",
				"Transaction Line Info:\tPK = 209870e1-5458-45c6-81ed-075c098a9d3d");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckJobChargeMoreThanOneFieldLinkToTheSameLine_ChargeIsInDB_APLineHasChanges()
		{
			var charge = CreateJobCharge(Factory, new Guid("febb3f1c-0a28-4fd2-9709-8ba94e0195fe"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			var relatedLine1 = CreateTransactionLine(Factory, TransactionLineTypes.WIP, new Guid("209870e1-5458-45c6-81ed-075c098a9d3d"));
			var relatedLine2 = CreateTransactionLine(Factory, TransactionLineTypes.Accrual, new Guid("24e15fd5-fb03-4da7-bf26-532cdaa9fdb7"));
			charge.JR_AL_ARLine = relatedLine1.PK;
			charge.JR_AL_APLine = relatedLine2.PK;
			relatedLine1.AL_JH = charge.JR_JH;
			relatedLine1.AL_AC = charge.JR_AC;
			relatedLine2.AL_JH = charge.JR_JH;
			relatedLine2.AL_AC = charge.JR_AC;

			Factory.Save();

			ReverseWIPAccrual(relatedLine2, charge);
			charge.JR_AL_APLine = relatedLine1.PK;
			Assert("JR_AL_APLine has changes.", charge.JR_AL_APLineInfo.HasChanges);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobCharge has more than one field link to the same line when charge is in database and JR_AL_APLine has changes.",
				true, CriticalValidationErrorType.JobChargeMoreThanOneFieldLinkToTheSameLine, "The AP Line and AR Line of a Job Charge can not be the same transaction line.",
				"JR_AL_APLine and JR_AL_ARLine of the Job Charge are linked to the same transaction line.",
				"JobCharge Info:\tPK = febb3f1c-0a28-4fd2-9709-8ba94e0195fe",
				"Transaction Line Info:\tPK = 209870e1-5458-45c6-81ed-075c098a9d3d");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckJobChargeMoreThanOneFieldLinkToTheSameLine_ChargeIsInDB_ARLineHasChanges()
		{
			var charge = CreateJobCharge(Factory, new Guid("febb3f1c-0a28-4fd2-9709-8ba94e0195fe"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			var relatedLine1 = CreateTransactionLine(Factory, TransactionLineTypes.WIP, new Guid("209870e1-5458-45c6-81ed-075c098a9d3d"));
			var relatedLine2 = CreateTransactionLine(Factory, TransactionLineTypes.Accrual, new Guid("24e15fd5-fb03-4da7-bf26-532cdaa9fdb7"));
			charge.JR_AL_ARLine = relatedLine1.PK;
			charge.JR_AL_APLine = relatedLine2.PK;
			relatedLine1.AL_JH = charge.JR_JH;
			relatedLine1.AL_AC = charge.JR_AC;
			relatedLine2.AL_JH = charge.JR_JH;
			relatedLine2.AL_AC = charge.JR_AC;

			Factory.Save();

			ReverseWIPAccrual(relatedLine1, charge);
			charge.JR_AL_ARLine = relatedLine2.PK;
			Assert("JR_AL_ARLine has changes.", charge.JR_AL_ARLineInfo.HasChanges);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobCharge has more than one field link to the same line when charge is in database and JR_AL_ARLine has changes.",
				true, CriticalValidationErrorType.JobChargeMoreThanOneFieldLinkToTheSameLine, "The AP Line and AR Line of a Job Charge can not be the same transaction line.",
				"JR_AL_APLine and JR_AL_ARLine of the Job Charge are linked to the same transaction line.",
				"JobCharge Info:\tPK = febb3f1c-0a28-4fd2-9709-8ba94e0195fe",
				"Transaction Line Info:\tPK = 24e15fd5-fb03-4da7-bf26-532cdaa9fdb7");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckJobChargeMoreThanOneFieldLinkToTheSameLine_ChargeIsInDB_BothARAPLinesHaveChanges()
		{
			var charge = CreateJobCharge(Factory, new Guid("febb3f1c-0a28-4fd2-9709-8ba94e0195fe"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			var relatedLine1 = CreateTransactionLine(Factory, TransactionLineTypes.WIP, new Guid("209870e1-5458-45c6-81ed-075c098a9d3d"));
			var relatedLine2 = CreateTransactionLine(Factory, TransactionLineTypes.Accrual, new Guid("24e15fd5-fb03-4da7-bf26-532cdaa9fdb7"));
			charge.JR_AL_ARLine = relatedLine1.PK;
			charge.JR_AL_APLine = relatedLine2.PK;
			relatedLine1.AL_JH = charge.JR_JH;
			relatedLine1.AL_AC = charge.JR_AC;
			relatedLine2.AL_JH = charge.JR_JH;
			relatedLine2.AL_AC = charge.JR_AC;

			Factory.Save();

			ReverseWIPAccrual(relatedLine1, charge);
			ReverseWIPAccrual(relatedLine2, charge);
			var relatedLine3 = CreateTransactionLine(Factory, TransactionLineTypes.Accrual, new Guid("7679601c-b1bb-4d4a-bd5a-b7f645400027"));
			relatedLine3.AL_JH = charge.JR_JH;
			relatedLine3.AL_AC = charge.JR_AC;
			charge.JR_AL_ARLine = relatedLine3.PK;
			charge.JR_AL_APLine = relatedLine3.PK;
			Assert("JR_AL_ARLine has changes.", charge.JR_AL_ARLineInfo.HasChanges);
			Assert("JR_AL_APLine has changes.", charge.JR_AL_APLineInfo.HasChanges);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobCharge has more than one field link to the same line when charge is in database and both JR_AL_APLine and JR_AL_ARLine have changes.",
				true, CriticalValidationErrorType.JobChargeMoreThanOneFieldLinkToTheSameLine, "The AP Line and AR Line of a Job Charge can not be the same transaction line.",
				"JR_AL_APLine and JR_AL_ARLine of the Job Charge are linked to the same transaction line.",
				"JobCharge Info:\tPK = febb3f1c-0a28-4fd2-9709-8ba94e0195fe",
				"Transaction Line Info:\tPK = 7679601c-b1bb-4d4a-bd5a-b7f645400027");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckJobChargeMoreThanOneFieldLinkToTheSameLine_ChargeIsInDB_ChargeHasNoChange()
		{
			var charge = CreateJobCharge(Factory, new Guid("febb3f1c-0a28-4fd2-9709-8ba94e0195fe"));
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("4caede15-eccf-4b28-a850-7aff85959630"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;

			var relatedLine = CreateTransactionLine(Factory, TransactionLineTypes.WIP, new Guid("209870e1-5458-45c6-81ed-075c098a9d3d"));
			charge.JR_AL_ARLine = relatedLine.PK;
			charge.JR_AL_APLine = relatedLine.PK;
			relatedLine.AL_JH = charge.JR_JH;
			relatedLine.AL_AC = charge.JR_AC;

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobCharge has more than one field link to the same line when charge is in database but has no change.");

			AssertOnSavingCheck(charge, testCase);
		}

		public void TestCheckJobChargeHasInvalidProFormaCostOrProFormaRevenue_CollectedInfoIsAlwaysReported()
		{
			var chargeWithRatingJobHeader = CreateJobChargeWithRatingJobHeader();
			chargeWithRatingJobHeader.JR_ProFormaCost = false;
			ErrorReporter.Clear();

			Factory.ServiceContainer.RemoveService<CriticalValidationInfoCollectorService>();

			var infoCollectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull($"Precondition: {nameof(infoCollectorService)}", infoCollectorService);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckJobChargeHasInvalidProFormaCostOrProFormaRevenue, Job Charge Proforma Cost or Revenue is invalid",
				true, CriticalValidationErrorType.InvalidJobChargeProFormaCostOrProFormaRevenue,
				"Invalid Job Charge Proforma-cost or Proforma-revenue. Proforma-cost or Proforma-revenue should be true for Rating Header, otherwise, they should be false.",
				"JobChargeJR_ProFormaCostSetterCallStack: There was no attempt to collect any data.",
				"JobChargeJR_ProFormaRevenueSetterCallStack: There was no attempt to collect any data.",
				"JobChargeConstructorStackTrace: There was no attempt to collect any data.");

			AssertOnSavingCheck(chargeWithRatingJobHeader, testCase);
		}

		public void TestCheckJobChargeHasInvalidProFormaCostOrProFormaRevenue_CollectedInfo()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaCostSetterCallStack);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaRevenueSetterCallStack);

			var chargeWithRatingJobHeader = CreateJobChargeWithRatingJobHeader();

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(chargeWithRatingJobHeader.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaCostSetterCallStack, () => "Test error message reported when JR_ProformaCost is not valid.");
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(chargeWithRatingJobHeader.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaRevenueSetterCallStack, () => "Test error message reported when JR_ProformaRevenue is not valid.");

			chargeWithRatingJobHeader.JR_ProFormaCost = false;
			ErrorReporter.Clear();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckJobChargeHasInvalidProFormaCostOrProFormaRevenue, Job Charge Proforma Cost or Revenue is invalid",
				true, CriticalValidationErrorType.InvalidJobChargeProFormaCostOrProFormaRevenue,
				"Invalid Job Charge Proforma-cost or Proforma-revenue. Proforma-cost or Proforma-revenue should be true for Rating Header, otherwise, they should be false.",
				"JobChargeJR_ProFormaCostSetterCallStack:\r\nTest error message reported when JR_ProformaCost is not valid.",
				"JobChargeJR_ProFormaRevenueSetterCallStack:\r\nTest error message reported when JR_ProformaRevenue is not valid.",
				"JobChargeConstructorStackTrace:");

			AssertOnSavingCheck(chargeWithRatingJobHeader, testCase);
		}

		#region Implementation

		AccTaxRate TaxRate;
		AccGLHeader GLHeader;

		protected override void SetUp()
		{
			base.SetUp();

			TaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			GLHeader.AG_AccountNum = "1010101010";
			Factory.Save();
		}

		JobCharge CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(BusinessObjectFactory factory)
		{
			return CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(factory, true);
		}

		JobCharge CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(BusinessObjectFactory factory, bool shouldBePosted)
		{
			var charge = CreateJobChargeLinkedToJob(factory);

			CreateConsolCostLinkedToCharge(factory, shouldBePosted, charge);

			return charge;
		}

		JobCharge CreateJobChargeLinkedToJob(BusinessObjectFactory factory, int chargeNumber = 1)
		{
			var chargePKs = new[] { new Guid("f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e"), new Guid("1946d803-fd83-4370-b58d-64ea57330c75") };
			var jobPKs = new[] { new Guid("7dd1291f-ed46-4c34-8714-5d35e717cda5"), new Guid("957a86ed-60d0-4fee-ba61-25d6603cb366") };

			var charge = CreateJobCharge(factory, chargePKs[chargeNumber - 1]);
			var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(jobPKs[chargeNumber - 1]);
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10m;

			return charge;
		}

		void CreateConsolCostLinkedToCharge(BusinessObjectFactory factory, bool shouldBePosted, JobCharge charge, bool saveConsolCost = false, Guid? consolCostPK = null, Guid? consolPK = null)
		{
			var consolCost = factory.New(ObjectFactory.GetType<IJobConsolCost>(), consolCostPK ?? new Guid("a12986b4-036c-4cb2-a0e7-0c96438a46dd"));
			var consol = factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), consolPK ?? new Guid("40b5446d-6066-4ed1-b9c1-8ca588a2a43d"));
			consol.FillWithValidTestData();
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode.Name] = charge.JR_AC;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID.Name] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode.Name] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10m;
			consolCost.FillWithValidTestData();

			charge.JR_E6 = consolCost.PK;

			if (saveConsolCost)
			{
				consolCost.Factory.Save();
			}

			if (shouldBePosted)
			{
				var invoice = CreateInvoice(factory, new Guid("6d935954-0e05-4fab-a93b-4f344ed529de"));
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				var line = CreateTransactionLine(factory, TransactionLineTypes.Cost, new Guid("684f6900-562a-450d-8845-b14087c27ae4"));
				line.AL_OSAmount = line.AL_LineAmount = -10m;
				line.AL_AH = invoice.PK;
				if (charge.APLine != null)
				{
					using (SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
					{
						charge.APLine.AL_ReverseDate = ZDateTime.Today;
					}
				}
				charge.JR_AL_APLine = line.PK;
				consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice] = invoice.PK;
			}

			charge.JR_APInvoiceNum = "111";
			charge.JR_APInvoiceDate = new ZDateTime(2010, 01, 05);
			charge.JR_OH_CostAccount = factory.NewWithValidTestData<OrgHeader>().PK;
			charge.JR_PaymentDate = new ZDateTime(2010, 01, 10);
			charge.JR_CostReference = "ABC";

			consolCost[JobConsolCostSchema.E6_InvoiceNum.Name] = charge.JR_APInvoiceNum;
			consolCost[JobConsolCostSchema.E6_InvoiceDate.Name] = charge.JR_APInvoiceDate;
			consolCost[JobConsolCostSchema.E6_PaymentDate.Name] = charge.JR_PaymentDate;
			consolCost[JobConsolCostSchema.E6_OH_Creditor.Name] = charge.JR_OH_CostAccount;
			consolCost[JobConsolCostSchema.E6_CostReference.Name] = charge.JR_CostReference;
		}

		AccTransactionHeader CreateInvoice(BusinessObjectFactory factory)
		{
			return CreateInvoice(Factory, Guid.Empty);
		}

		AccTransactionHeader CreateInvoice(BusinessObjectFactory factory, Guid pk)
		{
			var invoice = pk == Guid.Empty ? factory.NewWithValidTestData<AccTransactionHeader>() : factory.NewWithPrimaryKey<AccTransactionHeader>(pk);
			invoice.AH_TransactionNum = "TEST_TRANSCATIONNUM";
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_InvoiceDate = ZDateTime.Now;

			return invoice;
		}

		AccTransactionLines CreateTransactionLine(BusinessObjectFactory factory, ZString lineType)
		{
			return CreateTransactionLine(factory, lineType, Guid.Empty);
		}

		AccTransactionLines CreateTransactionLine(BusinessObjectFactory factory, ZString lineType, Guid pk)
		{
			var line = pk == Guid.Empty ? factory.NewWithValidTestData<AccTransactionLines>() : factory.NewWithPrimaryKey<AccTransactionLines>(pk);
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_ExchangeRate = 1m;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_LineType = lineType;
			line.AL_AG = GLHeader.PK;
			line.FillWithValidTestData();

			return line;
		}

		JobCharge CreateJobCharge(BusinessObjectFactory factory)
		{
			return CreateJobCharge(factory, Guid.Empty);
		}

		JobCharge CreateJobCharge(BusinessObjectFactory factory, Guid pk)
		{
			var charge = pk == Guid.Empty ? factory.NewWithValidTestData<JobCharge>() : factory.NewWithPrimaryKey<JobCharge>(pk);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.FillWithValidTestData();

			return charge;
		}

		void ReverseWIPAccrual(AccTransactionLines line, JobCharge charge)
		{
			if (line.AL_LineType == TransactionLineTypes.Accrual || line.AL_LineType == TransactionLineTypes.WIP)
			{
				using (line.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					using (line.GetValidationSuspender())
					{
						line.AL_ReverseDate = ZDateTime.Now;
					}
					if (line.AL_LineType == TransactionLineTypes.Accrual)
					{
						charge.JR_AL_APLine = ZGuid.Empty;
					}
					else
					{
						charge.JR_AL_ARLine = ZGuid.Empty;
					}
				}
			}
		}

		JobCharge CreateJobChargeWithRatingJobHeader()
		{
			var ratingJobHeader = Factory.NewJobForTesting<JobHeader>();
			ratingJobHeader.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = ratingJobHeader.PK;
			return jobCharge;
		}

		#endregion
	}
}
