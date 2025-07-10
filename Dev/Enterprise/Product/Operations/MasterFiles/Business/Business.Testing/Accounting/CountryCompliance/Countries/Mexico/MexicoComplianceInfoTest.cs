using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MexicoComplianceInfo))]
	public class MexicoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Mexico;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "OTR", "TCR", "TDR", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCR", "TDR", "TXI", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "OTR", "TCR", "TDR", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TDR", "TXI", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "OTR", "TDR", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "OTR", "TCR", "XCL" };

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "TXI", "TDR", "TCR" };

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> { { "OTR", LedgerOfUse.AP } };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "TCR", TransactionTypeOfUse.CRD }, { "TDR", TransactionTypeOfUse.INV }, { "TXI", TransactionTypeOfUse.INV },
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCR";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Credit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de Crédito";

		protected override string ExpectedComplianceRules => @"MX,TXI,AR,INV,TID,ALL,OTO,,,,,,,,,,,
MX,TCR,AR,CRD,TID,ALL,OTO,,,,,,,,,,,
MX,TCR,AR,CRD,TID,ALL,ARO,,TDR,,,,,,,,,
MX,TCR,AR,CRD,TID,ALL,ARO,,TXI,,,,,,,,,
MX,TDR,AR,INV,TID,ALL,ARO,,TXI,,,,,,,,,
MX,XCL,AR,INV,EXL,ALL,OTO,,,,,,,,,,,
MX,XCL,AR,INV,EXL,ALL,ARO,,XCL,,,,,,,,,
MX,XCL,AR,CRD,EXL,ALL,OTO,,,,,,,,,,,
MX,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,,,,,,,,
MX,TDR,AR,INV,TID,ALL,ARO,,TCR,,,,,,,,,
MX,TXI,AP,INV,TID,ALL,OTO,,,,,,,,,,,
MX,TCR,AP,CRD,TID,ALL,OTO,,,,,,,,,,,
MX,TCR,AP,CRD,TID,ALL,ARO,,TDR,,,,,,,,,
MX,TCR,AP,CRD,TID,ALL,ARO,,TXI,,,,,,,,,
MX,XCL,AP,INV,EXL,ALL,OTO,,,,,,,,,,,
MX,XCL,AP,INV,EXL,ALL,ARO,,XCL,,,,,,,,,
MX,XCL,AP,CRD,EXL,ALL,OTO,,,,,,,,,,,
MX,XCL,AP,CRD,EXL,ALL,ARO,,XCL,,,,,,,,,
MX,TDR,AP,INV,TID,ALL,ARO,,TCR,,,,,,,,,";

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2023, 11, 1) : new ZDate(2023, 4, 1);

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("RET Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("RET Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("RET Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
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

		public void TestGetIComplianceSubTypeAndNumberUpdateRules()
		{
			AssertEquals(true, ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode)?.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed);
		}

		public void TestGetITransactionAuthorisationRecordProvider()
		{
			var result = new ZBlob();
			AssertEquals(Convert.ToBase64String(result), ObjectFactory.Get<ICountryComplianceFactory>().GetTransactionAuthorisationRecordProvider(CountryCode)?.GetDecodedAuthorationData(result));
		}

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Mexico;

		public void TestGetEquivalentComplianceSubType()
		{
			var equivalentComplianceProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode);

			AssertEquals("I - Ingreso", EquivalentComplianceSubtype(MexicoComplianceInfo.ComplianceSubTypeCodes.TXI));
			AssertEquals("I - Ingreso", EquivalentComplianceSubtype(MexicoComplianceInfo.ComplianceSubTypeCodes.TDR));
			AssertEquals("E - Egreso", EquivalentComplianceSubtype(MexicoComplianceInfo.ComplianceSubTypeCodes.TCR));
			AssertEquals(MexicoComplianceInfo.ComplianceSubTypeCodes.OTR, EquivalentComplianceSubtype(MexicoComplianceInfo.ComplianceSubTypeCodes.OTR));
			AssertEquals(string.Empty, EquivalentComplianceSubtype(string.Empty));

			string EquivalentComplianceSubtype(string complianceSubtype)
			{
				return equivalentComplianceProvider.GetEquivalentComplianceSubType(complianceSubtype);
			}
		}

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;
	}
}
