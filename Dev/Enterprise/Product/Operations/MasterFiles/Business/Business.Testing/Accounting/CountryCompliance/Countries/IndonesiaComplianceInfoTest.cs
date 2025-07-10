using System;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(IndonesiaComplianceInfo))]
	sealed class IndonesiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Indonesia;

		#region IComplianceSubTypeCodeProvider

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "ID Govt Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Faktur Pajak";

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "NIT";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT NITKU";

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT NPWP";

		#endregion

		protected override string ExpectedComplianceRules => @"ID,TXI,AR,INV,AMT,ALL,ALL,,,,1,TXI,,,,,,
ID,T01,AR,INV,STI,ALL,ALL,,,,2,T01,T04,,,,PPN, CAPPPN,,
ID,T04,AR,INV,STI,ALL,ALL,,,,2,T01,T04,,,,PPN1,,
ID,T01,AR,INV,STI,ALL,ALL,,,,3,T01,T05,,,,PPN, CAPPPN,,
ID,T05,AR,INV,STI,ALL,ALL,,,,3,T01,T05,,,,PPN1,,";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestComplianceSubTypeAttributionRuleSet_Indonesia()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(IndonesiaComplianceInfo.RuleSetCodes.TXI, ruleSetProvider.GetDefaultRuleSet());

			var ruleSets = ruleSetProvider.GetRuleSet();
			AssertEquals(3, ruleSets.Count);
			Assert(ruleSets.ContainsCode(IndonesiaComplianceInfo.RuleSetCodes.TXI));
			Assert(ruleSets.ContainsCode(IndonesiaComplianceInfo.RuleSetCodes.T01T04));
			Assert(ruleSets.ContainsCode(IndonesiaComplianceInfo.RuleSetCodes.T01T05));
		}

		public void TestComplianceSubTypeDependencyConfigurationForIndonesia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				AssertEquals(9, collection.Count);
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[0], "ID", "T01", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[1], "ID", "T02", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[2], "ID", "T03", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[3], "ID", "T04", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[4], "ID", "T05", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[5], "ID", "T06", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[6], "ID", "T07", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[7], "ID", "T08", "TXI");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[8], "ID", "T09", "TXI");
			}

			void AssertComplianceSubTypeDependencyConfigurationCollection(ComplianceSubTypeDependencyConfiguration item, string expectedCountry, string expectedChildSubType, string expectedParentSubType)
			{
				AssertEquals("Country", expectedCountry, item.Country);
				AssertEquals("Child Sub Type", expectedChildSubType, item.ChildSubType);
				AssertEquals("Parent Sub Type", expectedParentSubType, item.ParentSubType);
			}
		}
	}
}
