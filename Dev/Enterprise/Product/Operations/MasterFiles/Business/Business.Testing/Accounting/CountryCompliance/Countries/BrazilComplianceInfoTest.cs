using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(BrazilComplianceInfo))]
	sealed class BrazilComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Brazil;

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType
			=> new[] { BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, BrazilComplianceInfo.ComplianceSubTypeCodes.CNS };

		protected override string ExpectedComplianceRules =>
@"BR,CNS,AR,CRD,TXX,ALL,ARO,,NFS,,1,Cancellations Only,,,,,,ISS
BR,CNS,AR,CRD,TXX,ALL,ARO,,NFS,,1,Cancellations Only,,,,,ISS,
BR,XNC,AR,CRD,TXX,ALL,ARO,,XND,,1,Cancellations Only,,,,,,ISS
BR,XNC,AR,CRD,TXX,ALL,ARO,,XNC,,1,Cancellations Only,,,,,,ISS
BR,XND,AR,INV,TXX,ALL,ARO,,XNC,,1,Cancellations Only,,,,,,ISS
BR,XND,AR,INV,TXX,ALL,ARO,,XND,,1,Cancellations Only,,,,,,ISS
BR,NFS,AR,INV,TXX,ALL,ALL,,,,2,NFSe and Cancellations,,,,,ISS,
BR,XND,AR,INV,TXX,ALL,OTO,,,,2,NFSe and Cancellations,,,,,,ISS
BR,CNS,AR,CRD,TXX,ALL,ARO,,NFS,,2,NFSe and Cancellations,,,,,ISS,
BR,XNC,AR,CRD,TXX,ALL,ARO,,XND,,2,NFSe and Cancellations,,,,,,ISS
BR,XNC,AR,CRD,TXX,ALL,ARO,,XNC,,2,NFSe and Cancellations,,,,,,ISS
BR,XND,AR,INV,TXX,ALL,ARO,,XNC,,2,NFSe and Cancellations,,,,,,ISS
BR,XND,AR,INV,TXX,ALL,ARO,,XND,,2,NFSe and Cancellations,,,,,,ISS";

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "IMF";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "IM";

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? ZDate.Empty : new ZDate(2021, 4, 1);

		#region IComplianceSubTypeCodeProvider

		protected override string[] ExpectedComplianceSubTypes => new string[] { "NFS", "NFE", "CNS", "CNE", "XND", "XNC" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "NFS", "NFE", "CNS", "CNE", "XND", "XNC" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "NFS", "NFE", "CNS", "CNE", "XND", "XNC" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "NFS", "NFE", "XND" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "NFS", "NFE", "XND" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "CNS", "CNE", "XNC" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "CNS", "CNE", "XNC" };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse>
		{
			{ "CNE", TransactionTypeOfUse.CRD },
			{ "CNS", TransactionTypeOfUse.CRD },
			{ "XNC", TransactionTypeOfUse.CRD },
			{ "XND", TransactionTypeOfUse.INV },
			{ "NFE", TransactionTypeOfUse.INV },
			{ "NFS", TransactionTypeOfUse.INV }
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "CNS";

		protected override string ExpectedComplianceSubTypeDescription => "CANCELLATION OF ELECTRONIC INVOICE FOR SERVICES";

		protected override string ExpectedComplianceSubTypeLocalDescription => "CANCELAMENTO DE NOTA FISCAL DE SERVIÇOS ELETRÔNICA";

		#endregion

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = BrazilOrgCusCodeInfo.OrgCusCodes.CMT;

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "CMT";

			AssertEquals(expected, result);
		}

		public override void TestGetRecipientLocalBusinessReg2NumberCodeType()
		{
			var countryComplianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, countryComplianceInfo.GetRecipientLocalBusinessReg2NumberCodeType());
		}

		public override void TestGetRecipientLocalBusinessReg2Heading()
		{
			var countryComplianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("CPF", countryComplianceInfo.GetRecipientLocalBusinessReg2Heading());
		}

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Brazil()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(BrazilComplianceInfo.RuleSetCodes.CancellationsOnly, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(BrazilComplianceInfo.RuleSetCodes.CancellationsOnly));
			Assert(ruleSet.ContainsCode(BrazilComplianceInfo.RuleSetCodes.NFSeAndCancellations));
		}

		#endregion

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Brazil;

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), $"'GVT' is not valid for country/region 'BR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), string.Empty },
			};

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), $"'GVT' is not valid for country/region 'BR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), string.Empty },
			};
	}
}
