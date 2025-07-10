using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MalaysiaComplianceInfo))]
	public class MalaysiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Malaysia;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "01", "02", "03", "11", "12" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "01", "02", "03" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "01", "03" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "02" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "11", "12" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "11" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "12" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "01";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Fakta cukai";

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "01", "02", "03", "11", "12" };

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Malaysia;

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse>
		{
			{ "01", TransactionTypeOfUse.INV },
			{ "02", TransactionTypeOfUse.CRD },
			{ "03", TransactionTypeOfUse.INV },
			{ "11", TransactionTypeOfUse.INV },
			{ "12", TransactionTypeOfUse.CRD },
		};

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
			{ "01", LedgerOfUse.AR },
			{ "02", LedgerOfUse.AR },
			{ "03", LedgerOfUse.AR },
			{ "11", LedgerOfUse.AP },
			{ "12", LedgerOfUse.AP },
		};

		protected override string ExpectedComplianceRules =>
@"MY,01,AR,INV,TNE,ALL,OTO,,,,1,Tax Invoice,,,,,,
MY,02,AR,CRD,TNE,ALL,ARO,,01,,1,Tax Invoice,,,,,,
MY,03,AR,INV,TNE,ALL,ARO,,01,,1,Tax Invoice,,,,,,
MY,11,AP,INV,RVS,ALL,OTO,,,,1,Tax Invoice,,,,,,
MY,12,AP,CRD,RVS,ALL,ALL,,,,1,Tax Invoice,,,,,,
MY,01,AR,INV,TNE,ALL,OTO,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,02,AR,CRD,TNE,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,03,AR,INV,TNE,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,01,AR,INV,NOT,ALL,OTO,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,02,AR,CRD,NOT,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,03,AR,INV,NOT,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,11,AP,INV,RVS,ALL,OTO,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,12,AP,CRD,RVS,ALL,ALL,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
";

		protected override bool ExpectedShouldAutoSetEReportingComplianceDateValue => true;

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Malaysia()
		{
			var ruleSetProvider = ComplianceInfo as IComplianceSubTypeRulesWithMultipleRuleSetProvider;
			AssertEquals(MalaysiaComplianceInfo.RuleSetCodes.DefaultMalaysiaRuleSet, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(MalaysiaComplianceInfo.RuleSetCodes.DefaultMalaysiaRuleSet));
		}

		public void TestComplianceSubTypeAttributionRuleSetMalaysiaRuleSet2IncludingNotReportableTransacion()
		{
			var expect = @"
MY,01,AR,INV,TNE,ALL,OTO,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,02,AR,CRD,TNE,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,03,AR,INV,TNE,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,01,AR,INV,NOT,ALL,OTO,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,02,AR,CRD,NOT,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,03,AR,INV,NOT,ALL,ARO,,01,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,11,AP,INV,RVS,ALL,OTO,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,
MY,12,AP,CRD,RVS,ALL,ALL,,,,2,Tax Invoice and Not Reportable Invoice,,,,,,";
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection, MalaysiaComplianceInfo.RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion);
			AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expect);
		}

		[TestDate(2005, 1, 2, 14, 30, 45, 28)]
		[TestDateIncremental]
		public void TestExistActiveDocumentRequestPivot()
		{
			var ruleSetProvider = ComplianceInfo as IComplianceInfoEInvoicingGUIActionProvider;
			var time = ZDateTime.UtcNow.AddHours(-1);
			Assert(ruleSetProvider.ExistActiveDocumentRequestPivot(time.AddSeconds(1)));
			Assert(!ruleSetProvider.ExistActiveDocumentRequestPivot(time));
		}

		public void TestIComplianceInfoEInvoicingGUIActionDocumentRequest()
		{
			var complianceInfo = ComplianceInfo as IComplianceInfoEInvoicingGUIActionDocumentRequest;
			AssertEquals("Request e-Invoice Document", complianceInfo.DocumentRequestMenuName);
			AssertEquals(@"Your request for a copy of the tax invoice is being processed.
Please Note:
- A request for a copy of the tax invoice can only be made for an Invoice or Credit Note in which E-Reporting Status = SUC.
- A new request for a copy of the tax invoice is allowed where response from an existing request has not been received after 60 minutes from request submission.", complianceInfo.DocumentRequestActionInformation);
		}

		#region IComplianceInfoEInvoicingGUIActionStatusRequest

		public void TestIComplianceInfoEInvoicingGUIActionStatusRequest_StatusRequestMenuName()
		{
			AssertEquals("Request e-Invoice Status", (ComplianceInfo as IComplianceInfoEInvoicingGUIActionStatusRequest).StatusRequestMenuName);
		}

		public void TestIComplianceInfoEInvoicingGUIActionStatusRequest_StatusRequestActionInformation()
		{
			var expect = @"Your request is being processed.
Please note:
1.Only transactions that have an e-Reporting status of 'DLV - Delivered' or 'IMP - In Processing' or 'FAL - Failed' with E-Reporting Govt # are eligible for 'Request e-Invoice Status'.
2.A new request is allowed when response from an existing request has not been received after 30 minutes from request submission.";
			AssertEquals(expect, (ComplianceInfo as IComplianceInfoEInvoicingGUIActionStatusRequest).StatusRequestActionInformation);
		}

		#endregion

		MalaysiaComplianceInfo ComplianceInfo => TestObject as MalaysiaComplianceInfo;

		#region IComplianceSubTypeValidation

		public void TestErrorMessageForComplianceSubTypeValidation()
		{
			var arInvoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice1.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice1.AH_ComplianceSubType = "111";

			var arInvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice2.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice2.AH_ComplianceSubType = "01";

			var arInvoice3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice3.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice3.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice3.AH_ComplianceSubType = "03";

			var arInvoice4 = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice4.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice4.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice4.AH_ComplianceSubType = ZString.Empty;

			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(CountryCode);
			AssertEquals("Enter Compliance Sub Type 01 for Tax Invoice.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice1));
			AssertEquals(ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice2));
			AssertEquals("Enter Compliance Sub Type 01 for Tax Invoice.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice3));
			AssertEquals(ZString.Empty, complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice4));
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

		#region IComplianceInfoEInvoicingGUIActionProvider

		public void TestGetEligibleInvoices()
		{
			var provider = ComplianceInfo as IComplianceInfoEInvoicingGUIActionProvider;

			var invoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = TransactionTypes.Invoice;

			var invoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice2.AH_TransactionType = TransactionTypes.CreditNote;

			var adjustmentNote = Factory.NewWithValidTestData<AccTransactionHeader>();
			adjustmentNote.AH_Ledger = LedgerTypes.AccountsReceivable;
			adjustmentNote.AH_TransactionType = TransactionTypes.AdjustmentNote;

			AssertContainsExactElementsInAnyOrder(new AccTransactionHeader[] { invoice1, invoice2 }, provider.GetEligibleInvoices(new AccTransactionHeader[] { invoice1, invoice2, adjustmentNote }));

			invoice1.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice2.AH_Ledger = LedgerTypes.AccountsPayable;
			adjustmentNote.AH_Ledger = LedgerTypes.AccountsPayable;

			AssertContainsExactElementsInAnyOrder(new AccTransactionHeader[] { invoice1, invoice2 }, provider.GetEligibleInvoices(new AccTransactionHeader[] { invoice1, invoice2, adjustmentNote }));
		}

		protected override bool EnableEInvoicingForAPTransaction() => true;

		#endregion

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderSchema.Constants.AH_GovernmentAllocatedID;
	}
}
