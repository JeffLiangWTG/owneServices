using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ItalyComplianceInfo))]
	sealed class ItalyComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Italy;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "API", "APS", "APV", "ARE", "ARI", "ARN", "ARS", "ARV", "G26", "G28", "INI", "INT", "XAP", "XCL", "XLP" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "ARE", "ARI", "ARN", "ARS", "ARV", "G26", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "API", "APS", "APV", "INI", "INT", "G28", "XAP", "XLP" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "ARE", "ARI", "ARN", "ARS", "ARV", "G26", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "API", "APS", "APV", "INI", "INT", "G28", "XAP", "XLP" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "ARE", "ARI", "ARN", "ARS", "ARV", "G26", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "API", "APS", "APV", "INI", "INT", "G28", "XAP", "XLP" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "API";

		protected override string ExpectedComplianceSubTypeDescription => "AP Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Acquisti - Fattura / Nota di Credito";

		protected override string ExpectedComplianceRules =>
@"IT,ARI,AR,INV,TID,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,CRD,TID,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARS,AR,INV,TID,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,ARS,AR,CRD,TID,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,API,AP,INV,TXR,ALL,ALL,IT,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,API,AP,CRD,TXR,ALL,ALL,IT,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,NEU,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,NEU,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,INI,AP,INV,RVS,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INI,AP,CRD,RVS,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INT,AP,INV,RVS,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,INT,AP,CRD,RVS,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,IT,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,IT,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,NEU,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,NEU,,,1,Vendite ARI ARS, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,INV,TID,NDB,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,CRD,TID,NDB,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARS,AR,INV,TID,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,ARS,AR,CRD,TID,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,API,AP,INV,TXR,ALL,ALL,IT,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,API,AP,CRD,TXR,ALL,ALL,IT,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,NEU,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,NEU,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,INI,AP,INV,RVS,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INI,AP,CRD,RVS,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INT,AP,INV,RVS,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,INT,AP,CRD,RVS,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,IT,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,IT,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,NEU,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,NEU,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XCL,AR,INV,TID,DSB,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XCL,AR,CRD,TID,DSB,ALL,,,,2,Vendite ARI ARS XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,INV,TID,ALL,ALL,IT,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,CRD,TID,ALL,ALL,IT,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARE,AR,INV,TID,ALL,ALL,EUX,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARE,AR,CRD,TID,ALL,ALL,EUX,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARN,AR,INV,TID,ALL,ALL,NEU,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARN,AR,CRD,TID,ALL,ALL,NEU,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARS,AR,INV,TID,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,ARS,AR,CRD,TID,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,API,AP,INV,TXR,ALL,ALL,IT,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,API,AP,CRD,TXR,ALL,ALL,IT,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,NEU,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,NEU,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,INI,AP,INV,RVS,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INI,AP,CRD,RVS,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INT,AP,INV,RVS,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,INT,AP,CRD,RVS,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,IT,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,IT,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,NEU,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,NEU,,,3,Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,INV,TID,NDB,ALL,IT,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARI,AR,CRD,TID,NDB,ALL,IT,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARE,AR,INV,TID,NDB,ALL,EUX,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARE,AR,CRD,TID,NDB,ALL,EUX,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARN,AR,INV,TID,NDB,ALL,NEU,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARN,AR,CRD,TID,NDB,ALL,NEU,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,ARS,AR,INV,TID,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,ARS,AR,CRD,TID,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,API,AP,INV,TXR,ALL,ALL,IT,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,API,AP,CRD,TXR,ALL,ALL,IT,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,NEU,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,NEU,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,APS,AP,INV,RVS,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,APS,AP,CRD,RVS,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,SBI,,EVG,,,
IT,INI,AP,INV,RVS,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INI,AP,CRD,RVS,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,IT,EVG,,,
IT,INT,AP,INV,RVS,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,INT,AP,CRD,RVS,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,EUX,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,IT,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,IT,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,INV,EXL,ALL,ALL,NEU,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XAP,AP,CRD,EXL,ALL,ALL,NEU,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XCL,AR,INV,TID,DSB,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
IT,XCL,AR,CRD,TID,DSB,ALL,,,,4,Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP,STD,,EVG,,,
";

		protected override ZDate ExpectedEInvoicingComplianceDate => new ZDate(2019, 1, 1);

		protected override ZDate ExpectedEInvoicingComplianceDateForPayables => new ZDate(2022, 7, 1);

		protected override string ExpectedDefaultEInvoicingSubmitPivotStatusForPayables => Core.Constants.EInvoicingPivotState.Pending;

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse>
		{
			{ "API",LedgerOfUse.AP },
			{ "APS",LedgerOfUse.AP },
			{ "APV",LedgerOfUse.AP },
			{ "INI",LedgerOfUse.AP },
			{ "INT",LedgerOfUse.AP },
			{ "G28",LedgerOfUse.AP },
			{ "XAP",LedgerOfUse.AP },
			{ "XLP",LedgerOfUse.AP },
			{ "ARE",LedgerOfUse.AR },
			{ "ARI",LedgerOfUse.AR },
			{ "ARN",LedgerOfUse.AR },
			{ "ARS",LedgerOfUse.AR },
			{ "ARV",LedgerOfUse.AR },
			{ "G26",LedgerOfUse.AR },
			{ "XCL",LedgerOfUse.AR }
		};

		public void TestComplianceSubTypeDependencyConfigurationForItaly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				AssertEquals(2, collection.Count);
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[0], "G26", "ARI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[1], "G28", "API");
			}

			void AssertComplianceSubTypeDependencyConfigurationCollection(ComplianceSubTypeDependencyConfiguration item, string expectedChildSubType, string expectedParentSubType)
			{
				AssertEquals("Country", "IT", item.Country);
				AssertEquals("Child Sub Type", expectedChildSubType, item.ChildSubType);
				AssertEquals("Parent Sub Type", expectedParentSubType, item.ParentSubType);
			}
		}

		public void TestComplianceSubTypeAttributionRuleSetCollection()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(ItalyComplianceInfo.RuleSetCodes.Default, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(4, ruleSet.Count);
			Assert(ruleSet.ContainsCode(ItalyComplianceInfo.RuleSetCodes.Default));
			Assert(ruleSet.ContainsCode(ItalyComplianceInfo.RuleSetCodes.DefaultWithDisbursementAR));
			Assert(ruleSet.ContainsCode(ItalyComplianceInfo.RuleSetCodes.DefaultWithARByCountry));
			Assert(ruleSet.ContainsCode(ItalyComplianceInfo.RuleSetCodes.DefaultWithARByCountryAndDisbursement));
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_Default()
		{
			var expected = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			const string billRule = "STD";
			const string disbRule = "ALL";
			const string origRule = "ALL";
			const string vatRule = "EVG";
			const string setCode = "1";
			const string setDesc = "Vendite ARI ARS, Acquisti API APS INI INT XAP";

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expected);
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_DefaultWithDisbursementAR()
		{
			var expected = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			const string billRule = "STD";
			const string disbRule = "ALL";
			const string origRule = "ALL";
			const string vatRule = "EVG";
			const string setCode = "2";
			const string setDesc = "Vendite ARI ARS XCL, Acquisti API APS INI INT XAP";

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: "DSB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: "DSB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var ruleSet = new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ItalyComplianceInfo.RuleSetCodes.DefaultWithDisbursementAR);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, ruleSet);

				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expected);
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_DefaultWithARByCountry()
		{
			var expected = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			const string billRule = "STD";
			const string disbRule = "ALL";
			const string origRule = "ALL";
			const string vatRule = "EVG";
			const string setCode = "3";
			const string setDesc = "Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP";

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARE", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARE", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var ruleSet = new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ItalyComplianceInfo.RuleSetCodes.DefaultWithARByCountry);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, ruleSet);

				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expected);
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_DefaultWithARByCountryAndDisbursement()
		{
			var expected = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			const string billRule = "STD";
			const string disbRule = "ALL";
			const string origRule = "ALL";
			const string vatRule = "EVG";
			const string setCode = "4";
			const string setDesc = "Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP";

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARI", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARE", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARE", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: "NDB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "ARS", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "API", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TXR", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "APS", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: "SBI", originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INI", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "INT", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "RVS", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, taxRegistrationLocationRule: "EUX");

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: CountryCode);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XAP", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", disbursementRule: disbRule, selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc, organisationLocation: "NEU");

			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", disbursementRule: "DSB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);
			AddComplianceSubTypeAttributionRule(expected, country: CountryCode, subType: "XCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", disbursementRule: "DSB", selfBillingRule: billRule, originalRule: origRule, vatGroupRule: vatRule, ruleSetCode: setCode, ruleSetDescription: setDesc);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var ruleSet = new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ItalyComplianceInfo.RuleSetCodes.DefaultWithARByCountryAndDisbursement);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, ruleSet);

				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expected);
			}
		}

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("SPV Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("SPV Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("SPV Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("IVA Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("IVA Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("IVA Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "N0", Description = (NoResString)"No Natura", Bool = true },
				new CodeDescriptionBoolRelatedItem() { Code = "N1", Description = (NoResString)"Escluse ex art.15", Bool = true, RelatedItemCode = "N1" },
				new CodeDescriptionBoolRelatedItem() { Code = "N2", Description = (NoResString)"Non soggette", Bool = true, RelatedItemCode = "N2" },
				new CodeDescriptionBoolRelatedItem() { Code = "N21", Description = (NoResString)"Non soggette ad IVA ai sensi degli artt. da 7 a 7-septies del DPR 633/72", Bool = true, RelatedItemCode = "N2.1" },
				new CodeDescriptionBoolRelatedItem() { Code = "N22", Description = (NoResString)"Non soggette - altri casi", Bool = true, RelatedItemCode = "N2.2" },
				new CodeDescriptionBoolRelatedItem() { Code = "N3", Description = (NoResString)"Non imponibili", Bool = true, RelatedItemCode = "N3" },
				new CodeDescriptionBoolRelatedItem() { Code = "N31", Description = (NoResString)"Non imponibili - esportazioni", Bool = true, RelatedItemCode = "N3.1" },
				new CodeDescriptionBoolRelatedItem() { Code = "N32", Description = (NoResString)"Non imponibili - cessioni intracomunitarie", Bool = true, RelatedItemCode = "N3.2" },
				new CodeDescriptionBoolRelatedItem() { Code = "N33", Description = (NoResString)"Non imponibili - cessioni verso San Marino", Bool = true, RelatedItemCode = "N3.3" },
				new CodeDescriptionBoolRelatedItem() { Code = "N34", Description = (NoResString)"Non imponibili - operazioni assimilate alle cessioni all'esportazione", Bool = true, RelatedItemCode = "N3.4" },
				new CodeDescriptionBoolRelatedItem() { Code = "N35", Description = (NoResString)"Non imponibili - a seguito di dichiarazioni d'intento", Bool = true, RelatedItemCode = "N3.5" },
				new CodeDescriptionBoolRelatedItem() { Code = "N36", Description = (NoResString)"Non imponibili - altre operazioni che non concorrono alla formazione del plafond", Bool = true, RelatedItemCode = "N3.6" },
				new CodeDescriptionBoolRelatedItem() { Code = "N4", Description = (NoResString)"Esente", Bool = true, RelatedItemCode = "N4" },
				new CodeDescriptionBoolRelatedItem() { Code = "N5", Description = (NoResString)"Regime del margine/IVA non esposta in fattura", Bool = true, RelatedItemCode = "N5" },
				new CodeDescriptionBoolRelatedItem() { Code = "N6", Description = (NoResString)"Reverse charge", Bool = true, RelatedItemCode = "N6" },
				new CodeDescriptionBoolRelatedItem() { Code = "N61", Description = (NoResString)"Reverse charge - cessione di rottami e altri materiali di recupero", Bool = true, RelatedItemCode = "N6.1" },
				new CodeDescriptionBoolRelatedItem() { Code = "N62", Description = (NoResString)"Reverse charge - cessione di oro e argento puro", Bool = true, RelatedItemCode = "N6.2" },
				new CodeDescriptionBoolRelatedItem() { Code = "N63", Description = (NoResString)"Reverse charge - subappalto nel settore edile", Bool = true, RelatedItemCode = "N6.3" },
				new CodeDescriptionBoolRelatedItem() { Code = "N64", Description = (NoResString)"Reverse charge - cessione di fabbricati", Bool = true, RelatedItemCode = "N6.4" },
				new CodeDescriptionBoolRelatedItem() { Code = "N65", Description = (NoResString)"Reverse charge - cessione di telefoni cellulari", Bool = true, RelatedItemCode = "N6.5" },
				new CodeDescriptionBoolRelatedItem() { Code = "N66", Description = (NoResString)"Reverse charge - cessione di prodotti elettronici", Bool = true, RelatedItemCode = "N6.6" },
				new CodeDescriptionBoolRelatedItem() { Code = "N67", Description = (NoResString)"Reverse charge - prestazioni comparto edile e settori connessi", Bool = true, RelatedItemCode = "N6.7" },
				new CodeDescriptionBoolRelatedItem() { Code = "N68", Description = (NoResString)"Reverse charge - operazioni settore energetico", Bool = true, RelatedItemCode = "N6.8" },
				new CodeDescriptionBoolRelatedItem() { Code = "N69", Description = (NoResString)"Reverse charge - altri casi", Bool = true, RelatedItemCode = "N6.9" },
				new CodeDescriptionBoolRelatedItem() { Code = "N7", Description = (NoResString)"IVA assolta in altro Stato UE", Bool = true, RelatedItemCode = "N7" }
			};

		public override void TestEInvoiceEliglibleComplianceSubTypes()
		{
			var complianceInfoEInvoice = CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(CountryCode);
			AssertNotNull(complianceInfoEInvoice);
			Assert(complianceInfoEInvoice.GetEligibleComplianceSubTypeListForEInvoicing().Count == 0);
		}

		#region IComplianceInfoEInvoicingGUIActionQueuePendingInvoice

		public void TestQueuePendingInvoiceMenuName()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals(ZString.Empty, complianceInfo.QueuePendingInvoiceMenuName);
		}

		public void TestPivotStatusesEligibleForQueuing()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals("PivotStatusesEligibleForQueuing only have two element", 2, complianceInfo.PivotStatusesEligibleForQueuing.Length);
			AssertEquals("PivotStatusesEligibleForQueuing have Pending", true, complianceInfo.PivotStatusesEligibleForQueuing.Contains(EInvoicingPivotState.Pending));
			AssertEquals("PivotStatusesEligibleForQueuing have AwaitingReview", true, complianceInfo.PivotStatusesEligibleForQueuing.Contains(EInvoicingPivotState.AwaitingReview));
		}

		public void TestPivotStatusesEligibleForQueuingErrorMessage()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionQueuePendingInvoice;
			AssertEquals("You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending' or 'AWA - Awaiting Review'.", complianceInfo.PivotStatusesEligibleForQueuingErrorMessage);
		}

		#endregion
	}
}
