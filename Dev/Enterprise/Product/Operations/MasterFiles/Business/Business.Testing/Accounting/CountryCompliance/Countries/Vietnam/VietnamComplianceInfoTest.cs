using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing.Vietnam
{
	[TestedType(typeof(VietnamComplianceInfo))]
	public class VietnamComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => CountryCodes.VietNam;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "EXI", "TXI" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "EXI";

		protected override string ExpectedComplianceSubTypeDescription => "VN Export Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "H\u00d3A \u0110\u01a0N XU\u1ea4T KH\u1ea8U";

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "TXI", "EXI" };

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedComplianceRules => @"VN,TXI,AR,INV,TXN,ALL,OTO,VN,,,1,,,,,,,
VN,TXI,AR,INV,TXN,ALL,OTO,,,,1,,,,,,,
VN,TXI,AR,INV,TNE,ALL,OTO,VN,,,2,,,,,,,
VN,TXI,AR,INV,TNE,ALL,OTO,,,,2,,,,,,,";

		protected override bool ExpectedShouldAutoSetEReportingComplianceDateValue => true;

		#region IComplianceInfoEInvoicingGUIActionProvider

		public void TestGetEligibleInvoices()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionProvider;

			var transactions = new List<AccTransactionHeader>();

			var invoiceWithTXI = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithTXI.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithTXI.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			transactions.Add(invoiceWithTXI);

			var invoiceWithEXI = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithEXI.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithEXI.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI;
			transactions.Add(invoiceWithEXI);

			var invoiceReversedWithEXI = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceReversedWithEXI.AH_TransactionType = TransactionTypes.Invoice;
			invoiceReversedWithEXI.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI;
			invoiceReversedWithEXI.AH_IsCancelled = true;
			transactions.Add(invoiceReversedWithEXI);

			var invoiceWithXXX = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithXXX.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithXXX.AH_ComplianceSubType = "XXX";
			transactions.Add(invoiceWithXXX);

			var creditNoteWithTXINotCancelled = Factory.NewWithValidTestData<AccTransactionHeader>();
			creditNoteWithTXINotCancelled.AH_TransactionType = TransactionTypes.CreditNote;
			creditNoteWithTXINotCancelled.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			creditNoteWithTXINotCancelled.AH_IsCancelled = false;
			transactions.Add(creditNoteWithTXINotCancelled);

			var creditNoteWithEmptyeComplianceSubTypeCancelled = Factory.NewWithValidTestData<AccTransactionHeader>();
			creditNoteWithEmptyeComplianceSubTypeCancelled.AH_TransactionType = TransactionTypes.CreditNote;
			creditNoteWithEmptyeComplianceSubTypeCancelled.AH_IsCancelled = true;
			transactions.Add(creditNoteWithEmptyeComplianceSubTypeCancelled);

			var eligibleInvoices = complianceInfo.GetEligibleInvoices(transactions);

			AssertEquals(4, eligibleInvoices.Count());
			Assert(eligibleInvoices.Contains(invoiceWithTXI));
			Assert(eligibleInvoices.Contains(invoiceWithEXI));
			Assert(eligibleInvoices.Contains(creditNoteWithTXINotCancelled));
			Assert(eligibleInvoices.Contains(creditNoteWithEmptyeComplianceSubTypeCancelled));
		}

		public void TestExistActiveDocumentRequestPivot()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionProvider;
			var lastSentDate = ZDateTime.UtcNow;
			AssertEquals("Last sent date is not past 1 hour so we cannot re send document request.", true, complianceInfo.ExistActiveDocumentRequestPivot(lastSentDate));

			lastSentDate = ZDateTime.UtcNow.AddHours(-2);
			AssertEquals("Last sent date has past 1 hour so we cannot re send document request.", false, complianceInfo.ExistActiveDocumentRequestPivot(lastSentDate));
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public void TestComplianceInfoEInvoicingGUIActionDocumentRequest()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionDocumentRequest;

			AssertEquals("Request e-Invoice Copy", complianceInfo.DocumentRequestMenuName);
			AssertEquals(@"Your request for a copy of the tax invoice is being processed.
Please Note:
- A request for a copy of the tax invoice can only be made for new INV, INV amendment with CRD, canceled INV.
- A new request for a copy of the tax invoice is allowed where response from an existing request has not been received after 60 minutes from request submission.", complianceInfo.DocumentRequestActionInformation);
		}

		#endregion

		public void TestGetIComplianceSubTypeAndNumberUpdateRules()
		{
			AssertEquals(true, ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode)?.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed);
		}

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(VietnamComplianceInfo.RuleSetCodes.TXIExcludingNotReportableCharges, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(VietnamComplianceInfo.RuleSetCodes.TXIExcludingNotReportableCharges));
			Assert(ruleSet.ContainsCode(VietnamComplianceInfo.RuleSetCodes.TXIExcludingNotReportableAndExcludedFromTaxBaseCharges));
		}
	}
}
