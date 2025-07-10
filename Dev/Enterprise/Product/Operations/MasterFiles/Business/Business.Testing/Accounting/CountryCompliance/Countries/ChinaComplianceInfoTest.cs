using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ChinaComplianceInfo))]
	public class ChinaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.China;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "ETA", "ETB", "TXA", "TXB", "FDA", "FDB" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "ETB";

		protected override string ExpectedComplianceSubTypeDescription => "Electronic General VAT Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "电子增值税普通发票";

		protected override string ExpectedComplianceRules => @"CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,1,TXA, TXB,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,1,TXA, TXB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,1,TXA, TXB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,1,TXA, TXB,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,1,TXA, TXB,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,1,TXA, TXB,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,1,TXA, TXB,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,2,TXA, ETB,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,2,TXA, ETB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,2,TXA, ETB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,2,TXA, ETB,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,2,TXA, ETB,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,2,TXA, ETB,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,2,TXA, ETB,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,3,TXA, TXB, ETB,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,3,TXA, TXB, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,3,TXA, TXB, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,3,TXA, TXB, ETB,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,3,TXA, TXB, ETB,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,3,TXA, TXB, ETB,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,3,TXA, TXB, ETB,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,4,TXA, ETB, TXB,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,4,TXA, ETB, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,4,TXA, ETB, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,4,TXA, ETB, TXB,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,4,TXA, ETB, TXB,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,4,TXA, ETB, TXB,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,4,TXA, ETB, TXB,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,5,TXA, TXB, ETA,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,5,TXA, TXB, ETA,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,5,TXA, TXB, ETA,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,5,TXA, TXB, ETA,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,5,TXA, TXB, ETA,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,5,TXA, TXB, ETA,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,5,TXA, TXB, ETA,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,6,TXA, ETB, ETA,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,6,TXA, ETB, ETA,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,6,TXA, ETB, ETA,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,6,TXA, ETB, ETA,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,6,TXA, ETB, ETA,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,6,TXA, ETB, ETA,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,6,TXA, ETB, ETA,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,7,TXA, TXB, ETA, ETB,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,7,TXA, TXB, ETA, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,7,TXA, TXB, ETA, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,7,TXA, TXB, ETA, ETB,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,7,TXA, TXB, ETA, ETB,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,7,TXA, TXB, ETA, ETB,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,7,TXA, TXB, ETA, ETB,,,,,,
CN,TXA,AR,INV,TXA,ALL,ALL,,,REC,8,TXA, ETB, ETA, TXB,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,8,TXA, ETB, ETA, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,8,TXA, ETB, ETA, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,8,TXA, ETB, ETA, TXB,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,8,TXA, ETB, ETA, TXB,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,8,TXA, ETB, ETA, TXB,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,8,TXA, ETB, ETA, TXB,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,9,ETA, TXB,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,9,ETA, TXB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,9,ETA, TXB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,9,ETA, TXB,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,9,ETA, TXB,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,9,ETA, TXB,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,9,ETA, TXB,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,10,ETA, ETB,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,10,ETA, ETB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,10,ETA, ETB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,10,ETA, ETB,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,10,ETA, ETB,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,10,ETA, ETB,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,10,ETA, ETB,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,11,ETA, TXB, ETB,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,11,ETA, TXB, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,11,ETA, TXB, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,11,ETA, TXB, ETB,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,11,ETA, TXB, ETB,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,11,ETA, TXB, ETB,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,11,ETA, TXB, ETB,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,12,ETA, ETB, TXB,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,12,ETA, ETB, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,12,ETA, ETB, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,12,ETA, ETB, TXB,,,,,,
CN,ETA,AP,INV,TXA,ALL,ALL,,,REC,12,ETA, ETB, TXB,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,12,ETA, ETB, TXB,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,12,ETA, ETB, TXB,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,13,ETA, TXB, TXA,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,13,ETA, TXB, TXA,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,13,ETA, TXB, TXA,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,13,ETA, TXB, TXA,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,13,ETA, TXB, TXA,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,13,ETA, TXB, TXA,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,13,ETA, TXB, TXA,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,14,ETA, ETB, TXA,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,14,ETA, ETB, TXA,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,14,ETA, ETB, TXA,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,14,ETA, ETB, TXA,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,14,ETA, ETB, TXA,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,14,ETA, ETB, TXA,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,14,ETA, ETB, TXA,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,15,ETA, TXB, TXA, ETB,,,,,,
CN,TXB,AR,INV,TXX,ALL,ALL,,,REC,15,ETA, TXB, TXA, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,NOT,15,ETA, TXB, TXA, ETB,,,,,,
CN,TXB,AR,INV,TXN,ALL,ALL,,,DNI,15,ETA, TXB, TXA, ETB,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,15,ETA, TXB, TXA, ETB,,,,,,
CN,ETB,AP,INV,TXX,ALL,ALL,,,REC,15,ETA, TXB, TXA, ETB,,,,,,
CN,ETB,AP,INV,TXN,ALL,ALL,,,NOT,15,ETA, TXB, TXA, ETB,,,,,,
CN,ETA,AR,INV,TXA,ALL,ALL,,,REC,16,ETA, ETB, TXA, TXB,,,,,,
CN,ETB,AR,INV,TXX,ALL,ALL,,,REC,16,ETA, ETB, TXA, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,NOT,16,ETA, ETB, TXA, TXB,,,,,,
CN,ETB,AR,INV,TXN,ALL,ALL,,,DNI,16,ETA, ETB, TXA, TXB,,,,,,
CN,TXA,AP,INV,TXA,ALL,ALL,,,REC,16,ETA, ETB, TXA, TXB,,,,,,
CN,TXB,AP,INV,TXX,ALL,ALL,,,REC,16,ETA, ETB, TXA, TXB,,,,,,
CN,TXB,AP,INV,TXN,ALL,ALL,,,NOT,16,ETA, ETB, TXA, TXB,,,,,,
CN,FDA,AR,INV,TXA,ALL,ALL,,,REC,17,FDA, FDB,,,,,,
CN,FDB,AR,INV,TXX,ALL,ALL,,,REC,17,FDA, FDB,,,,,,
CN,FDB,AR,INV,TXN,ALL,ALL,,,NOT,17,FDA, FDB,,,,,,
CN,FDB,AR,INV,TXN,ALL,ALL,,,DNI,17,FDA, FDB,,,,,,
CN,FDA,AP,INV,TXA,ALL,ALL,,,REC,17,FDA, FDB,,,,,,
CN,FDB,AP,INV,TXX,ALL,ALL,,,REC,17,FDA, FDB,,,,,,
CN,FDB,AP,INV,TXN,ALL,ALL,,,NOT,17,FDA, FDB,,,,,,
CN,FDA,AR,INV,TXA,ALL,ALL,,,REC,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDB,AR,INV,TXX,ALL,ALL,,,REC,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDB,AR,INV,TXN,ALL,ALL,,,NOT,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDB,AR,INV,TXN,ALL,ALL,,,DNI,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDB,AR,INV,TXA,ALL,ALL,,,FOR,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDA,AP,INV,TXA,ALL,ALL,,,REC,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDB,AP,INV,TXX,ALL,ALL,,,REC,18,FDA, FDB, FDB Foreign Debtor,,,,,,
CN,FDB,AP,INV,TXN,ALL,ALL,,,NOT,18,FDA, FDB, FDB Foreign Debtor,,,,,,
";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override bool ExpectedShouldAutoSetEReportingComplianceDateValue => true;

		public void TestIComplianceDocumentInfo_ShouldDisplayComplianceDocumentDate()
		{
			var complianceDocumentInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceDocumentInfo;

			AssertEquals(true, complianceDocumentInfo.ShouldDisplayComplianceDocumentDate());
		}

		public void TestCnComplianceSubTypeContainsETA()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			Assert(AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCode().ContainsCode(ChinaComplianceInfo.ComplianceSubTypeCodes.ETA));
			Assert(AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCodeInLocalLanguage().ContainsCode(ChinaComplianceInfo.ComplianceSubTypeCodes.ETA));
		}

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_China()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(ChinaComplianceInfo.RuleSetCodes.TXATXB, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(18, ruleSet.Count);
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXATXB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXAETB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXATXBETB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXAETBTXB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXATXBETA));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXAETBETA));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXATXBETAETB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.TXAETBETATXB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETATXB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETAETB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETATXBETB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETAETBTXB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETATXBTXA));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETAETBTXA));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETATXBTXAETB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.ETAETBTXATXB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.FDAFDB));
			Assert(ruleSet.ContainsCode(ChinaComplianceInfo.RuleSetCodes.FDAFDBFDBForeignDebtor));
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("VAT Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("VAT Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("VAT Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public void TestComplianceInfoEInvoicingGUIActionDocumentRequest()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionDocumentRequest;

			AssertEquals("Request e-Invoice Status", complianceInfo.DocumentRequestMenuName);
			AssertEquals(@"Your request is being processed.
Please Note:
- A new request is allowed where response from an existing request has not been received after 60 minutes from request submission.", complianceInfo.DocumentRequestActionInformation);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public void TestGetComplianceSubTypesSort()
		{
			var codeDescriptionPairList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode);
			AssertEquals(6, codeDescriptionPairList.Count);
			AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, codeDescriptionPairList[0].Code);
			AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.ETB, codeDescriptionPairList[1].Code);
			AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.FDA, codeDescriptionPairList[2].Code);
			AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.FDB, codeDescriptionPairList[3].Code);
			AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, codeDescriptionPairList[4].Code);
			AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, codeDescriptionPairList[5].Code);
		}

		public void TestGetEligibleInvoices()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionProvider;

			var transactions = new List<AccTransactionHeader>();

			var invoiceWithEAT = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithEAT.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithEAT.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithEAT.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
			transactions.Add(invoiceWithEAT);

			var invoiceWithETB = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithETB.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithETB.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithETB.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			transactions.Add(invoiceWithETB);
 
			var invoiceWithTXA = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithTXA.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithTXA.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithTXA.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transactions.Add(invoiceWithTXA);

			var invoiceWithFDA = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithFDA.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithFDA.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithFDA.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			transactions.Add(invoiceWithFDA);

			var invoiceWithFDB = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithFDB.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithFDB.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithFDB.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			transactions.Add(invoiceWithFDB);

			var invoiceWithTXB = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithTXB.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithTXB.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithTXB.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			transactions.Add(invoiceWithTXB);

			var invoiceReversedWithTXB = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceReversedWithTXB.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceReversedWithTXB.AH_TransactionType = TransactionTypes.Invoice;
			invoiceReversedWithTXB.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			invoiceReversedWithTXB.AH_IsCancelled = true;
			transactions.Add(invoiceReversedWithTXB);

			var invoiceWithXXX = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithXXX.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceWithXXX.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithXXX.AH_ComplianceSubType = "XXX";
			transactions.Add(invoiceWithXXX);

			var creditNoteWithETANotCancelled = Factory.NewWithValidTestData<AccTransactionHeader>();
			creditNoteWithETANotCancelled.AH_TransactionType = TransactionTypes.CreditNote;
			creditNoteWithETANotCancelled.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
			creditNoteWithETANotCancelled.AH_IsCancelled = false;
			transactions.Add(creditNoteWithETANotCancelled);

			var apInvoiceWithEAT = Factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoiceWithEAT.AH_Ledger = LedgerTypes.AccountsPayable;
			apInvoiceWithEAT.AH_TransactionType = TransactionTypes.Invoice;
			apInvoiceWithEAT.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
			transactions.Add(apInvoiceWithEAT);

			var eligibleInvoices = complianceInfo.GetEligibleInvoices(transactions);

			AssertEquals(6, eligibleInvoices.Count());
			Assert(eligibleInvoices.Contains(invoiceWithEAT));
			Assert(eligibleInvoices.Contains(invoiceWithETB));
			Assert(eligibleInvoices.Contains(invoiceWithTXA));
			Assert(eligibleInvoices.Contains(invoiceWithTXB));
			Assert(eligibleInvoices.Contains(invoiceWithFDA));
			Assert(eligibleInvoices.Contains(invoiceWithFDB));
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

		#region IComplianceSubTypeAndNumberUpdateRules

		public void TestIsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed()
		{
			AssertEquals(false, ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode).IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed);
		}

		public void TestGetErrorMessageForARComplianceSubTypeAndNumberUpdateForSingleTransaction()
		{
			AssertErrorMessageForGetErrorMessageForARComplianceSubTypeAndNumberUpdate(false);
		}

		public void TestGetErrorMessageForARComplianceSubTypeAndNumberUpdateForMultipleTransaction()
		{
			AssertErrorMessageForGetErrorMessageForARComplianceSubTypeAndNumberUpdate(true);
		}

		void AssertErrorMessageForGetErrorMessageForARComplianceSubTypeAndNumberUpdate(bool shouldHasMultipleTransaction)
		{
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.OH_Category = OrgConstants.Category.Business;
			debtor.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_OH = debtor.PK;
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_TransactionNum = "ARINV001";
			transactionHeader.AH_SystemCreateTimeUtc = DateTime.Now;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AT = vat.PK;

			var transactionHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader2.AH_OH = debtor.PK;
			transactionHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader2.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader2.AH_TransactionNum = "ARINV002";
			transactionHeader2.AH_SystemCreateTimeUtc = DateTime.Now;

			var transactionLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine2.AL_AH = transactionHeader2.PK;
			transactionLine2.AL_AT = vat.PK;

			var transactionHeadersList = new List<AccTransactionHeader>() { transactionHeader };

			var nonTaxApplicableErrorMessage = "You cannot update the compliance sub type as this is a non tax invoice.";
			var noVATNumberErrorMessage = "You cannot update the compliance sub type as this invoice debtor does not have a valid VAT registration number.";
			var hasPivotErrorMessage = "You cannot update the Compliance Sub Type as the Invoice is currently in the process of E-Reporting.";

			if (shouldHasMultipleTransaction)
			{
				transactionHeadersList.Add(transactionHeader2);

				var debtor2 = Factory.NewWithValidTestData<OrgHeader>();
				debtor2.CompanyData.OB_IsDebtor = true;
				debtor2.OH_Category = OrgConstants.Category.Business;
				debtor2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				debtor2.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

				var vatCustomsCode = debtor2.CustomsCodes.AddNew();
				vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
				vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vatCustomsCode.OK_CustomsRegNo = "1111";

				var transactionHeader3 = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionHeader3.AH_OH = debtor2.PK;
				transactionHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionHeader3.AH_TransactionType = TransactionTypes.Invoice;
				transactionHeader3.AH_TransactionNum = "ARINV003";

				var transactionLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
				transactionLine3.AL_AH = transactionHeader3.PK;
				transactionLine3.AL_AT = vat.PK;

				transactionHeadersList.Add(transactionHeader3);

				nonTaxApplicableErrorMessage = @"AR Invoice ARINV001:
You cannot update the compliance sub type as this is a non tax invoice.
AR Invoice ARINV002:
You cannot update the compliance sub type as this is a non tax invoice.";
				noVATNumberErrorMessage = @"AR Invoice ARINV001:
You cannot update the compliance sub type as this invoice debtor does not have a valid VAT registration number.
AR Invoice ARINV002:
You cannot update the compliance sub type as this invoice debtor does not have a valid VAT registration number.";
				hasPivotErrorMessage = @"AR Invoice ARINV001:
You cannot update the Compliance Sub Type as the Invoice is currently in the process of E-Reporting.
AR Invoice ARINV002:
You cannot update the Compliance Sub Type as the Invoice is currently in the process of E-Reporting.";
			}

			var transactionHeaders = transactionHeadersList.ToArray();
			var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, "Test"))
				{
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today))
					{
						transactionLine.AL_AT = Guid.Empty;
						transactionLine2.AL_AT = Guid.Empty;

						AssertEquals("AR invoice 1 does not has tax id.", false, transactionLine.AL_AT.IsValid);
						AssertEquals("AR invoice 2 does not has tax id.", false, transactionLine2.AL_AT.IsValid);
						AssertEquals($"When AR invoice does not has tax id, there will have error message {nonTaxApplicableErrorMessage}.", nonTaxApplicableErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						transactionLine.AL_AT = vat.PK;
						transactionLine2.AL_AT = vat.PK;

						AssertEquals("AR invoice 1 has tax id.", true, transactionLine.AL_AT.IsValid);
						AssertEquals("AR invoice 2 has tax id.", true, transactionLine2.AL_AT.IsValid);
						AssertEquals("Debtor does not has VAT number.", false, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
						AssertEquals($"When Debtor does not has VAT number, there will have error message '{noVATNumberErrorMessage}'.", noVATNumberErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						var vatCustomsCode = debtor.CustomsCodes.AddNew();
						vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
						vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
						vatCustomsCode.OK_CustomsRegNo = "1111";

						AssertEquals("Debtor has VAT number.", true, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
						AssertEquals("There has no error message.", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						var pivotPK1 = InsertAccEInvoicingTransactionPivots(transactionHeader.PK.ToGuid(), transactionHeader.Company.PK.ToGuid(), CountryCodes.China);
						var pivotPK2 = InsertAccEInvoicingTransactionPivots(transactionHeader2.PK.ToGuid(), transactionHeader2.Company.PK.ToGuid(), CountryCodes.China);

						var pivotType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix);
						AssertEquals("transaction has pivot.", true, transactionHeader.Factory.Exists(pivotType, new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK), false));
						AssertEquals("transaction2 has pivot.", true, transactionHeader2.Factory.Exists(pivotType, new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader2.PK), false));
						AssertEquals($"When transaction has pivot, there will have error message '{hasPivotErrorMessage}'.", hasPivotErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						UpdateAccEInvoicingTransactionPivotStatus(pivotPK1, EInvoicingPivotState.BatchedWithError);
						UpdateAccEInvoicingTransactionPivotStatus(pivotPK2, EInvoicingPivotState.Failed);
						AssertNotContains($"No error when pivot status is BER or FAL.", hasPivotErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						UpdateAccEInvoicingTransactionPivotStatus(pivotPK1, EInvoicingPivotState.Discarded);
						UpdateAccEInvoicingTransactionPivotStatus(pivotPK2, EInvoicingPivotState.Discarded);
						AssertNotContains($"No error when pivot status is DCD.", hasPivotErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						InsertAccEInvoicingTransactionPivots(transactionHeader.PK.ToGuid(), transactionHeader.Company.PK.ToGuid(), CountryCodes.China, actionType: EInvoicingPivotActionType.StatusCheck);
						InsertAccEInvoicingTransactionPivots(transactionHeader2.PK.ToGuid(), transactionHeader2.Company.PK.ToGuid(), CountryCodes.China, actionType: EInvoicingPivotActionType.StatusCheck);
						AssertNotContains($"No error when pivot status is DCD.", hasPivotErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));
					}
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(1).ToDateTime()))
					{
						transactionLine.AL_AT = Guid.Empty;
						transactionLine2.AL_AT = Guid.Empty;

						AssertEquals("AR invoice 1 does not has tax id.", false, transactionLine.AL_AT.IsValid);
						AssertEquals("AR invoice 2 does not has tax id.", false, transactionLine2.AL_AT.IsValid);
						AssertEquals($"When AR invoice does not has tax id, there will have error message {nonTaxApplicableErrorMessage}.", nonTaxApplicableErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						transactionLine.AL_AT = vat.PK;
						transactionLine2.AL_AT = vat.PK;
						debtor.CustomsCodes.RemoveAll();
						AssertEquals("AR invoice has tax id.", true, debtor.CompanyData.IsARTaxApplicable);
						AssertEquals("Debtor does not has VAT number.", false, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
						AssertEquals($"When Debtor does not has VAT number, there will have error message '{noVATNumberErrorMessage}'.", noVATNumberErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

						var vatCustomsCode = debtor.CustomsCodes.AddNew();
						vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
						vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
						vatCustomsCode.OK_CustomsRegNo = "1111";

						AssertEquals("Debtor has VAT number.", true, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
						AssertEquals("There has no error message.", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));
					}
				}
				using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, string.Empty))
				{
					transactionLine.AL_AT = Guid.Empty;
					transactionLine2.AL_AT = Guid.Empty;

					AssertEquals("AR invoice 1 does not has tax id.", false, transactionLine.AL_AT.IsValid);
					AssertEquals("AR invoice 2 does not has tax id.", false, transactionLine2.AL_AT.IsValid);
					AssertEquals($"When AR invoice does not has tax id, there will have error message {nonTaxApplicableErrorMessage}.", nonTaxApplicableErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

					transactionLine.AL_AT = vat.PK;
					transactionLine2.AL_AT = vat.PK;
					debtor.CustomsCodes.RemoveAll();
					AssertEquals("AR invoice has tax id.", true, debtor.CompanyData.IsARTaxApplicable);
					AssertEquals("Debtor does not has VAT number.", false, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
					AssertEquals($"When Debtor does not has VAT number, there will have error message '{noVATNumberErrorMessage}'.", noVATNumberErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

					var vatCustomsCode = debtor.CustomsCodes.AddNew();
					vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
					vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
					vatCustomsCode.OK_CustomsRegNo = "1111";

					AssertEquals("Debtor has VAT number.", true, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
					AssertEquals("There has no error message.", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));
				}
			}
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				transactionLine.AL_AT = Guid.Empty;
				transactionLine2.AL_AT = Guid.Empty;

				AssertEquals("AR invoice 1 does not has tax id.", false, transactionLine.AL_AT.IsValid);
				AssertEquals("AR invoice 2 does not has tax id.", false, transactionLine2.AL_AT.IsValid);
				AssertEquals($"When AR invoice does not has tax id, there will have error message {nonTaxApplicableErrorMessage}.", nonTaxApplicableErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

				transactionLine.AL_AT = vat.PK;
				transactionLine2.AL_AT = vat.PK;
				debtor.CustomsCodes.RemoveAll();
				AssertEquals("AR invoice has tax id.", true, debtor.CompanyData.IsARTaxApplicable);
				AssertEquals("Debtor does not has VAT number.", false, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
				AssertEquals($"When Debtor does not has VAT number, there will have error message '{noVATNumberErrorMessage}'.", noVATNumberErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

				var vatCustomsCode = debtor.CustomsCodes.AddNew();
				vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
				vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vatCustomsCode.OK_CustomsRegNo = "1111";

				AssertEquals("Debtor has VAT number.", true, debtor.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
				AssertEquals("There has no error message.", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));
			}
		}

		public void TestIsComplianceNumberAllowed()
		{
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			debtor.OH_Category = OrgConstants.Category.Business;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_OH = debtor.PK;
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_TransactionNum = "ARINV001";
			transactionHeader.AH_GSTAmount = 10;
			transactionHeader.AH_SystemCreateTimeUtc = DateTime.UtcNow;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AT = vat.PK;

			var vatCustomsCode = debtor.CustomsCodes.AddNew();
			vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
			vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCustomsCode.OK_CustomsRegNo = "1111";

			var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("EnableEInvoicingFunctionality is false.", false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Compliance Number allow update when EnableEInvoicingFunctionality is false.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, ZString.Empty);
			AssertEquals("EnableEInvoicingFunctionality is true.", true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("ChinaEInvoicingCredentials is empty.", ZString.Empty, AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty));
			AssertEquals("Compliance Number allow update when ChinaEInvoicingCredentials is empty.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));

			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, "test123");
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, transactionHeader.AH_SystemCreateTimeUtc.AddDays(1).ToDateTime());
			AssertEquals("ChinaEInvoicingCredentials is not empty.", "test123", AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty));
			Assert("Transaction create date is less than E-Reporting Compliance Date.", transactionHeader.AH_SystemCreateTimeUtc < AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Compliance Number allow update when Transaction create date is less than E-Reporting Compliance Date.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));

			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, transactionHeader.AH_SystemCreateTimeUtc.AddDays(-1).ToDateTime());
			Assert("Transaction create date is big than E-Reporting Compliance Date.", transactionHeader.AH_SystemCreateTimeUtc >= AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Compliance Number allow update when Transaction don't have pivot.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));

			InsertAccEInvoicingTransactionPivots(transactionHeader.PK.ToGuid(), transactionHeader.Company.PK.ToGuid(), CountryCodes.China);
			Assert("Transaction create date is big than E-Reporting Compliance Date.", transactionHeader.Factory.Exists(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix), new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK), false));
			AssertEquals("Compliance Number not allow update when Transaction has pivot.", false, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));
		}

		public void TestIsComplianceDocDateAllowed()
		{
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			debtor.OH_Category = OrgConstants.Category.Business;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_OH = debtor.PK;
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_TransactionNum = "ARINV001";
			transactionHeader.AH_GSTAmount = 10;
			transactionHeader.AH_SystemCreateTimeUtc = DateTime.UtcNow;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AT = vat.PK;

			var vatCustomsCode = debtor.CustomsCodes.AddNew();
			vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
			vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCustomsCode.OK_CustomsRegNo = "1111";

			var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("EnableEInvoicingFunctionality is false.", false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Compliance Doc Date allow update when EnableEInvoicingFunctionality is false.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(transactionHeader));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, ZString.Empty);
			AssertEquals("EnableEInvoicingFunctionality is true.", true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("ChinaEInvoicingCredentials is empty.", ZString.Empty, AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty));
			AssertEquals("Compliance Doc Date allow update when ChinaEInvoicingCredentials is empty.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(transactionHeader));

			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, "test123");
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, transactionHeader.AH_SystemCreateTimeUtc.AddDays(1).ToDateTime());
			AssertEquals("ChinaEInvoicingCredentials is not empty.", "test123", AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty));
			Assert("Transaction create date is less than E-Reporting Compliance Date.", transactionHeader.AH_SystemCreateTimeUtc < AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Compliance Doc Date allow update when Transaction create date is less than E-Reporting Compliance Date.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));

			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, transactionHeader.AH_SystemCreateTimeUtc.AddDays(-1).ToDateTime());
			Assert("Transaction create date is big than E-Reporting Compliance Date.", transactionHeader.AH_SystemCreateTimeUtc >= AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Compliance Doc Date allow update when Transaction don't have pivot.", true, complianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(transactionHeader));

			InsertAccEInvoicingTransactionPivots(transactionHeader.PK.ToGuid(), transactionHeader.Company.PK.ToGuid(), CountryCodes.China);
			Assert("Transaction create date is big than E-Reporting Compliance Date.", transactionHeader.Factory.Exists(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix), new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK), false));
			AssertEquals("Compliance Number not allow update when Transaction has pivot.", false, complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));
		}

		public void TestGetErrorMessageForARComplianceSubTypeAndNumberUpdateForTransaction_PendingPivot()
		{
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.OH_Category = OrgConstants.Category.Business;

			var vatCustomsCode = debtor.CustomsCodes.AddNew();
			vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
			vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCustomsCode.OK_CustomsRegNo = "1111";

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_OH = debtor.PK;
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_TransactionNum = "ARINV001";

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AT = vat.PK;

			var transactionHeaders = new[] { transactionHeader };

			InsertAccEInvoicingTransactionPivots(transactionHeader.PK.ToGuid(), transactionHeader.Company.PK.ToGuid(), CountryCodes.China, status: EInvoicingPivotState.Pending);

			var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode);
			var hasPivotErrorMessage = "You cannot update the Compliance Sub Type as the Invoice is currently in the process of E-Reporting.";
			var pivotType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix);
			var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK);
			query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Pending);
			AssertEquals("transaction don't have not pending pivot.", false, transactionHeader.Factory.Exists(pivotType, query, false));
			AssertEquals($"When transaction has pending pivot, there will not have error message '{hasPivotErrorMessage}'.", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));
		}

		Guid InsertAccEInvoicingTransactionPivots(Guid parentPK, Guid companyPK, string countryCode, string parentTableCode = "AH", string status = "QUE", string actionType = EInvoicingPivotActionType.Submit)
		{
			var sQL = @"INSERT INTO dbo.AccEInvoicingTransactionPivot (AIP_PK, AIP_GC, AIP_RN_NKCountryCode, AIP_ParentID, AIP_ParentTableCode, AIP_Status, AIP_ActionType) 
							VALUES (@PK, @Company, @CountryCode, @ParentID, @ParentTableCode, @Status, @ActionType)";
			var pk = Guid.NewGuid();
			using (var cmd = TestConnection.Command(sQL))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameterBasedOnDbColumn("@CountryCode", countryCode, AccEInvoicingTransactionPivotSchema.AIP_RN_NKCountryCode);
				cmd.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, parentPK);
				cmd.AddParameterBasedOnDbColumn("@ParentTableCode", parentTableCode, AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode);
				cmd.AddParameterBasedOnDbColumn("@Status", status, AccEInvoicingTransactionPivotSchema.AIP_Status);
				cmd.AddParameterBasedOnDbColumn("@ActionType", actionType, AccEInvoicingTransactionPivotSchema.AIP_ActionType);
				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		void UpdateAccEInvoicingTransactionPivotStatus(Guid pk, string status)
		{
			var sQL = @"UPDATE dbo.AccEInvoicingTransactionPivot SET AIP_Status = @Status, AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = 'TST' WHERE AIP_PK = @PK";
			using (var cmd = TestConnection.Command(sQL))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameterBasedOnDbColumn("@Status", status, AccEInvoicingTransactionPivotSchema.AIP_Status);
				cmd.ExecuteNonQuery();
			}
		}

		public void TestTaxIdErrorMessage_CMTCharge()
		{
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.OH_Category = OrgConstants.Category.Business;

			var vatCustomsCode = debtor.CustomsCodes.AddNew();
			vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
			vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCustomsCode.OK_CustomsRegNo = "1111";

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_OH = debtor.PK;
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_TransactionNum = "ARINV001";

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;

			var transactionHeaders = new[] { transactionHeader };

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, transactionHeader.Branch.PK.ToGuid(), Guid.Empty, "Test");

			var nonTaxApplicableErrorMessage = "You cannot update the compliance sub type as this is a non tax invoice.";
			var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode);

			AssertEquals($"When AR invoice does not has tax id and is not comment charge, there will have error message {nonTaxApplicableErrorMessage}.", nonTaxApplicableErrorMessage, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));

			var commentCharge = Factory.NewWithValidTestData<AccChargeCode>();
			commentCharge.AC_ChargeType = ChargeType.Comment;
			transactionLine.AL_AC = commentCharge.PK;

			AssertEquals($"When AR invoice does not has tax id and is comment charge, there will have not error message {nonTaxApplicableErrorMessage}.", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(transactionHeaders));
		}

		#endregion

		#region IComplianceSubTypeValidation

		public void TestErrorMessageForComplianceSubTypeValidation()
		{
			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.OH_Category = OrgConstants.Category.Business;

			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_OH = org.PK;
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_TransactionNum = "ARINV001";
			arInvoice.AH_GSTAmount = 0;
			arInvoice.AH_ComplianceSubType = "ETB";

			var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine.AL_AH = arInvoice.PK;
			arInvoiceLine.AL_AT = vat.PK;

			var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_OH = org.PK;
			apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			apInvoice.AH_TransactionType = TransactionTypes.Invoice;
			apInvoice.AH_TransactionNum = "ARINV001";
			apInvoice.AH_GSTAmount = 0;
			apInvoice.AH_ComplianceSubType = "ETB";

			var apInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apInvoiceLine.AL_AH = apInvoice.PK;
			apInvoiceLine.AL_AT = vat.PK;

			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(CountryCode);

			arInvoiceLine.AL_AT = Guid.Empty;
			apInvoiceLine.AL_AT = Guid.Empty;

			AssertEquals("AR invoice does not has tax id.", false, arInvoiceLine.AL_AT.IsValid);
			AssertEquals("AP invoice does not has tax id.", false, apInvoiceLine.AL_AT.IsValid);
			AssertEquals($"When AR invoice does not has tax id, there will have error message 'You cannot assign a compliance sub type to the invoice as this is a non tax invoice.'.", "You cannot assign a compliance sub type to the invoice as this is a non tax invoice.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals($"When AP invoice does not has tax id, there will not have error message.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			arInvoiceLine.AL_AT = vat.PK;
			apInvoiceLine.AL_AT = vat.PK;

			AssertEquals("AR invoice has tax id.", true, arInvoiceLine.AL_AT.IsValid);
			AssertEquals("AR invoice has tax id.", true, apInvoiceLine.AL_AT.IsValid);
			AssertEquals("Debtor/Creditor does not has VAT number.", false, org.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
			AssertEquals($"When Debtor does not has VAT number for AR invoice, there has no error message because compliance subtype is ETB.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals($"When Creditor does not has VAT number for AP invoice, there will not have error message.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			var vatCustomsCode = org.CustomsCodes.AddNew();
			vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.China;
			vatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCustomsCode.OK_CustomsRegNo = "1111";

			arInvoice.AH_ComplianceSubType = "ETB";
			apInvoice.AH_ComplianceSubType = "ETB";

			AssertEquals("Debtor/Creditor has VAT number.", true, org.CustomsCodes.Any(x => ((OrgCusCode)x).OK_RN_NKCodeCountry == CountryCodes.China && ((OrgCusCode)x).OK_CodeType == OrgCusCode.CodeTypes.VATCode && !string.IsNullOrEmpty(((OrgCusCode)x).OK_CustomsRegNo)));
			AssertEquals("AR Invoice Tax amount is 0.", 0m, arInvoice.AH_GSTAmount);
			AssertEquals("AP Invoice Tax amount is 0.", 0m, apInvoice.AH_GSTAmount);
			AssertEquals("There has no error message because compliance subtype is ETB.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals("There has no error message because compliance subtype is ETB.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			arInvoice.AH_ComplianceSubType = "ETA";
			apInvoice.AH_ComplianceSubType = "ETA";

			AssertEquals($"When Debtor does not has VAT number for AR invoice, there will have error message 'You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.'.", "You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals($"When Creditor does not has VAT number  for AP invoice, there will not have error message.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			arInvoice.AH_ComplianceSubType = "TXA";
			apInvoice.AH_ComplianceSubType = "TXA";

			AssertEquals($"When Debtor does not has VAT number for AR invoice, there will have error message 'You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.'.", "You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals($"When Creditor does not has VAT number for AP invoice, there will not have error message.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			arInvoice.AH_ComplianceSubType = "FDA";
			apInvoice.AH_ComplianceSubType = "FDA";

			AssertEquals($"When Debtor does not has VAT number for AR invoice, there will have error message 'You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.'.", "You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals($"When Creditor does not has VAT number for AP invoice, there will not have error message.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			arInvoice.AH_GSTAmount = 10;
			apInvoice.AH_GSTAmount = 10;
			AssertNotEquals("AR invoice Tax amount is not 0.", 0, arInvoice.AH_GSTAmount);
			AssertNotEquals("AP invoice Tax amount is not 0.", 0, apInvoice.AH_GSTAmount);
			AssertEquals("There has no error message for AR invoice.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
			AssertEquals("There has no error message for AP invoice.", ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(apInvoice));

			vatCustomsCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			AssertEquals($"When Debtor does not has VAT number and country/Region is not CN for AR invoice, there will have error message 'You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as this invoice debtor does not have a valid VAT registration number.'.", "You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as this invoice debtor does not have a valid VAT registration number.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));
		}

		public void TestWarningMessageForComplianceSubTypeValidation()
		{
			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(CountryCode);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.OH_Category = OrgConstants.Category.Business;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_OH = org.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "ARINV001";

			AssertEquals(ZString.Empty, complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
		}

		#endregion

		#region IComplianceDocumentStatusProvider

		public void TestComplianceDocumentStatusProvider()
		{
			var complianceDocumentStatusProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceDocumentStatusProvider(CountryCode);

			AssertNotNull(complianceDocumentStatusProvider);

			var complianceDocumentStatusTypes = complianceDocumentStatusProvider.GetComplianceDocumentStatusTypes();

			AssertEquals(8, complianceDocumentStatusTypes.Count);
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDD));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDN));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDR));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDV));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDP));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDF));
			Assert(complianceDocumentStatusTypes.Contains(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CRR));
		}

		public void TestGetComplianceDocumentStatus()
		{
			var complianceDocumentStatusProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceDocumentStatusProvider(CountryCode);
			AssertNotNull(complianceDocumentStatusProvider);

			var complianceDocumentStatusType = complianceDocumentStatusProvider.GetComplianceDocumentStatus(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDD.Code);
			AssertEquals("CDD - Compliance Document Record Deleted", complianceDocumentStatusType);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueuePendingInvoice

		public void TestQueuePendingInvoiceMenuName()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals("Authorize And Send", complianceInfo.QueuePendingInvoiceMenuName);
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
			AssertEquals("You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending'.", complianceInfo.PivotStatusesEligibleForQueuingErrorMessage);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueueReversedTransaction

		public void TestRejectReQueueForReversedTransaction()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueueReversedTransaction;
			AssertEquals("RejectReQueueForReversedTransaction should be true", true, complianceInfo.RejectReQueueForReversedTransaction(string.Empty));
		}

		#endregion

		public void TestComplianceSubTypeDictionary()
		{
			AssertEquals("TXA", ChinaComplianceInfo.ComplianceSubTypeDictionary["01"]);
			AssertEquals("TXB", ChinaComplianceInfo.ComplianceSubTypeDictionary["02"]);
			AssertEquals("ETA", ChinaComplianceInfo.ComplianceSubTypeDictionary["11"]);
			AssertEquals("ETB", ChinaComplianceInfo.ComplianceSubTypeDictionary["12"]);
			AssertEquals("FDA", ChinaComplianceInfo.ComplianceSubTypeDictionary["13"]);
			AssertEquals("FDB", ChinaComplianceInfo.ComplianceSubTypeDictionary["14"]);
		}

		public void TestGetTaxRegistrationTypeList()
		{
			var list = CountryComplianceFactory.GetIComplianceSubTypeAdditionalTaxRegistrationTypeListProvider(CountryCode)?.GetTaxRegistrationTypeList();
			AssertEquals("ChinaComplianceInfo.GetTaxRegistrationTypeList", 2, list.Count);
			Assert(list.ContainsCode("DNI"));
			Assert(list.ContainsCode("FOR"));
		}

		public void TestIsOrganizationIsTaxRegistrationTypeRuleApplicable()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var rule = new ComplianceSubTypeAttributionRuleConfiguration();

			var builder = new ChinaComplianceInfo() as IComplianceSubTypeTaxRegistrationTypeRuleProvider;

			orgHeader.CustomsCodes.RemoveAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1111111", CountryCode);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.VAG, "1111111", CountryCode);

			rule.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			var result = builder.IsTaxRegistrationTypeRuleApplicable(rule, orgHeader);

			Assert(orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			Assert(orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.ChinaCodeTypes.VAG));
			Assert($"the tax registration type {TaxRegistrationTypeCodes.Recoverable} is applicable when its orgHeader code has {OrgCusCode.CodeTypes.VATCode} and {OrgCusCode.ChinaCodeTypes.VAG}", result);

			orgHeader.CustomsCodes.RemoveAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1111111", CountryCode);

			rule.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			result = builder.IsTaxRegistrationTypeRuleApplicable(rule, orgHeader);

			Assert(orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode));
			Assert(!orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.ChinaCodeTypes.VAG));
			Assert($"the tax registration type {TaxRegistrationTypeCodes.Recoverable} is applicable when its orgHeader code has {OrgCusCode.CodeTypes.VATCode} and no {OrgCusCode.ChinaCodeTypes.VAG}", result);

			orgHeader.CustomsCodes.RemoveAll();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			result = builder.IsTaxRegistrationTypeRuleApplicable(rule, orgHeader);
			Assert(!orgHeader.CustomsCodes.Cast<OrgCusCode>().Any());
			Assert($"the tax registration type {TaxRegistrationTypeCodes.Individual} is applicable when orgHeader don't have CustomsCodes and its orgHeader category is NAT", result);

			orgHeader.OH_Category = OrgConstants.Category.Business;
			orgHeader.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.ForeignOrganisation;
			result = builder.IsTaxRegistrationTypeRuleApplicable(rule, orgHeader);
			Assert(!orgHeader.CustomsCodes.Cast<OrgCusCode>().Any());
			AssertNotEquals("Organization is not China", CountryCodes.China, orgHeader.CountryCode);
			AssertEquals(OrgConstants.Category.Business, orgHeader.OH_Category);
			Assert($"the tax registration type {TaxRegistrationTypeCodes.ForeignOrganisation} is applicable when orgHeader don't have CustomsCodes its orgHeader category is BUS and orgHeader is not China", result);
		}
	}
}
