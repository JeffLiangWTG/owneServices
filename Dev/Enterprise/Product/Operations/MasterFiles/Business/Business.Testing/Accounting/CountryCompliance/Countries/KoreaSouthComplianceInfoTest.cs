using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(KoreaSouthComplianceInfo))]
	public class KoreaSouthComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.KoreaSouth;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "101", "102", "201", "202", "301", "401", "201", "202", "401" };

		protected override string ExpectedComplianceRules => @"KR,101,AR,INV,TNE,ALL,OTO,,,,1,,,,,,,
KR,102,AR,INV,ATZ,ALL,OTO,,,,1,,,,,,,
KR,201,AR,CRD,TNE,ALL,ARO,,101,,1,,,,,,,
KR,202,AR,CRD,ATZ,ALL,ARO,,102,,1,,,,,,,
KR,301,AR,INV,EXT,ALL,OTO,,,,1,,,,,,,
KR,401,AR,CRD,EXT,ALL,ARO,,301,,1,,,,,,,
KR,201,AR,INV,TNE,ALL,ARO,,101,,1,,,,,,,
KR,202,AR,INV,ATZ,ALL,ARO,,102,,1,,,,,,,
KR,401,AR,INV,EXT,ALL,ARO,,301,,1,,,,,,,";

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "101", "102", "201", "202", "301", "401", "201", "202", "401" };

		protected override string[] ExpectedPayablesComplianceSubTypes => Array.Empty<string>();

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "101", "102", "301", "201", "202", "401" };

		protected override string[] ExpectedReceivablesOriginalInvoiceComplianceSubTypes => new string[] { "101", "102", "301" };

		protected override string[] ExpectedReceivablesAmendingInvoiceComplianceSubTypes => new string[] { "201", "202", "401" };

		protected override string[] ExpectedReceivablesReversalInvoiceComplianceSubTypes => Array.Empty<string>();

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => Array.Empty<string>();

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "201", "202", "401" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => Array.Empty<string>();

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
			{ "101", LedgerOfUse.AR },
			{ "102", LedgerOfUse.AR },
			{ "201", LedgerOfUse.AR },
			{ "202", LedgerOfUse.AR },
			{ "301", LedgerOfUse.AR },
			{ "401", LedgerOfUse.AR }
		};

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse[]> ExpectedComplianceTransactionTypesOfUse => new Dictionary<string, TransactionTypeOfUse[]>
		{
			{ "101", new TransactionTypeOfUse[] { TransactionTypeOfUse.INV } },
			{ "102", new TransactionTypeOfUse[] { TransactionTypeOfUse.INV } },
			{ "201", new TransactionTypeOfUse[] { TransactionTypeOfUse.INV, TransactionTypeOfUse.CRD } },
			{ "202", new TransactionTypeOfUse[] { TransactionTypeOfUse.INV, TransactionTypeOfUse.CRD } },
			{ "301", new TransactionTypeOfUse[] { TransactionTypeOfUse.INV } },
			{ "401", new TransactionTypeOfUse[] { TransactionTypeOfUse.INV, TransactionTypeOfUse.CRD } }
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "101";

		protected override string ExpectedComplianceSubTypeDescription => "Original Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "일반 세금계산서";

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.KoreaSouth;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override bool ExpectedShouldAutoSetEReportingComplianceDateValue => true;

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderReferenceSchema.Constants.AH1_Reference;

		#region IComplianceInfoEInvoicingGUIActionQueuePendingInvoice

		public void TestQueuePendingInvoiceMenuName()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals("Authorize and Send", complianceInfo.QueuePendingInvoiceMenuName);
		}

		public void TestPivotStatusesEligibleForQueuing()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals("PivotStatusesEligibleForQueuing only have one element", 1, complianceInfo.PivotStatusesEligibleForQueuing.Length);
			AssertEquals("PivotStatusesEligibleForQueuing only have Pending", EInvoicingPivotState.Pending, complianceInfo.PivotStatusesEligibleForQueuing.FirstOrDefault());
		}

		public void TestPivotStatusesEligibleForQueuingErrorMessage()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals("You can only authorize and send transactions of which the E-Reporting status is 'PEN - Pending'.", complianceInfo.PivotStatusesEligibleForQueuingErrorMessage);
		}

		#endregion

		#region IComplianceInfoEInvoicingRequeueHandler

		public void TestIsPivotRequeueRestricted()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingRequeueHandler;

			AssertEquals("Should be restricted when state is SNT", true, complianceInfo.IsPivotRequeueRestricted(EInvoicingPivotState.Sent, out var warningMessage));
			AssertEquals("Re-queuing transactions with 'SNT - Sent' status may cause duplicate invoicing. Do you really want to re-queue?", warningMessage);

			AssertEquals("Should not be restricted when state is PEN", false, complianceInfo.IsPivotRequeueRestricted(EInvoicingPivotState.Pending, out warningMessage));
			AssertEquals("Should have no error message when state is PEN", null, warningMessage);
		}

		#endregion

		#region IComplianceSubTypeValidation

		public void TestMessageForComplianceSubTypeValidation()
		{
			var complianceSubTypeValidation = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceSubTypeValidation;
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.OH_Category = OrgConstants.Category.Business;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_OH = org.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "ARINV001";

			AssertEquals(ZString.Empty, complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
			AssertEquals(ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(transaction));

			transaction.AH_ComplianceSubType = "202";

			AssertEquals(ZString.Empty, complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
			AssertEquals(ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(transaction));

			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock();

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals("You have selected a Compliance Sub Type manually. Please note that incorrect Compliance Sub Type allocation may result in a failure E-Reporting submission.", complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
				AssertEquals("The Compliance Sub Type of the Invoice should be '101', '102' or '301'.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(transaction));

				transaction.AH_ComplianceSubType = "101";
				AssertEquals(ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(transaction));

				transaction.AH_OriginalTransactionNum = "TESTINV001";
				transaction.AH_ComplianceSubType = "201";
				AssertEquals(ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(transaction));
			}
		}

		public void TestWarningMessageForComplianceSubTypeValidation()
		{
			var complianceSubTypeValidation = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceSubTypeValidation;
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.OH_Category = OrgConstants.Category.Business;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_OH = org.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "ARINV001";
			transaction.AH_ComplianceSubType = "202";

			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock();

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals("You have selected a Compliance Sub Type manually. Please note that incorrect Compliance Sub Type allocation may result in a failure E-Reporting submission.", complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
				AssertEquals("The Compliance Sub Type of the Invoice should be '101', '102' or '301'.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(transaction));

				transaction.AH_ComplianceSubTypeInfo.AddWarningWithoutValidationCheck("Test Warning");
				AssertEquals("You have selected a Compliance Sub Type manually. Please note that incorrect Compliance Sub Type allocation may result in a failure E-Reporting submission.", complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));

				transaction.AH_ComplianceSubTypeInfo.AddErrorWithoutValidationCheck("Test Error");
				AssertEquals(ZString.Empty, complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
			}
		}

		#endregion

		#region IComplianceSubTypeGUIProvider

		public void TestComplianceSubTypeGUIProvider()
		{
			var guiProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceSubTypeGUIProvider;

			AssertEquals(true, guiProvider.ShouldClearComplianceSubType(ZGuid.Empty));

			CombineAssertions("AR", () =>
			{
				AssertEquals("Readonly if Amend with CreditNote", true, guiProvider.ComplianceSubTypeIsReadOnly(true, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, false));
				AssertEquals("Readonly if Amend with Invoice", true, guiProvider.ComplianceSubTypeIsReadOnly(true, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, false));
				AssertEquals("Readonly if CreditNote with empty original transation ", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, true));
				AssertEquals("Readonly if normal CreditNote", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, true));
				AssertEquals("Not readonly if normal Invoice", false, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, true));
				AssertEquals("Readonly if credit note with original transaction", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, false));
				AssertEquals("Readonly if Invoice with original transaction", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, false));
			});

			CombineAssertions("AP", () =>
			{
				AssertEquals("Readonly if Amend with CreditNote", true, guiProvider.ComplianceSubTypeIsReadOnly(true, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, false));
				AssertEquals("Readonly if Amend with Invoice", true, guiProvider.ComplianceSubTypeIsReadOnly(true, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, false));
				AssertEquals("Readonly if CreditNote with empty original transation ", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, true));
				AssertEquals("Readonly if normal CreditNote", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, true));
				AssertEquals("Readonly if normal Invoice", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, true));
				AssertEquals("Readonly if credit note with original transaction", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, false));
				AssertEquals("Readonly if Invoice with original transaction", true, guiProvider.ComplianceSubTypeIsReadOnly(false, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, false));
			});
		}

		#endregion

		#region IComplianceSubTypeTaxInvoiceRulePrecedenceProvider

		public void TestIComplianceSubTypeTaxInvoiceRulePrecedenceProvider()
		{
			var precedenceProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceSubTypeTaxInvoiceRulePrecedenceProvider;
			var precedenceList = precedenceProvider.ComplianceSubTypeTaxInvoiceRulePrecedenceList();

			AssertEquals(4, precedenceList.Length);

			CombineAssertions(() =>
			{
				AssertEquals(TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount, precedenceList[0]);
				AssertEquals(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL, precedenceList[1]);
				AssertEquals(TaxInvoiceRuleCodes.All, precedenceList[2]);
				AssertEquals(ZString.Empty, precedenceList[3]);
			});
		}

		#endregion

		public void TestInvalidRuleSetCodeErrorReport()
		{
			CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode).SetComplianceSubTypeAttributionRuleConfigurations(null, "999");

			AssertEquals("Incorrect Rule Set Code: 999.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
