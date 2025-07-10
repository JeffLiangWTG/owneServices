using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ChileComplianceInfo))]
	sealed class ChileComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Chile;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "BHE", "BOL", "DCD", "DCR", "DEX", "DXI", "TCD", "TCR", "TEX", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "DCD", "DCR", "DEX", "DXI", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "BHE", "BOL", "DCD", "DCR", "DEX", "DXI", "TCD", "TCR", "TEX", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "DCD", "DEX", "DXI", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "BHE", "BOL", "DCD", "DEX", "DXI", "TCD", "TEX", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "DCR", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "DCR", "TCR", "XCL" };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "BHE", TransactionTypeOfUse.INV }, { "BOL", TransactionTypeOfUse.INV }, { "DCD", TransactionTypeOfUse.INV }, { "DCR", TransactionTypeOfUse.CRD },
			{ "DEX", TransactionTypeOfUse.INV }, { "DXI", TransactionTypeOfUse.INV }, { "TCD", TransactionTypeOfUse.INV }, { "TCR", TransactionTypeOfUse.CRD },
			{ "TEX", TransactionTypeOfUse.INV }, { "TXI", TransactionTypeOfUse.INV }, { "XCL", TransactionTypeOfUse.ALL }
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "BHE";

		protected override string ExpectedComplianceSubTypeDescription => "Electronic Simplified Fee Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Boleta de Honorarios Electr\u00f3nica";

		protected override string ExpectedComplianceRules => @"CL,DXI,AR,INV,TXA,ALL,OTO,,,,,,,,,,,
CL,DCR,AR,CRD,TXA,ALL,OTO,,,,,,,,,,,
CL,DCR,AR,CRD,TXA,ALL,ARO,,DXI,,,,,,,,,
CL,DCR,AR,CRD,TXA,ALL,ARO,,DCD,,,,,,,,,
CL,DCD,AR,INV,TXA,ALL,ARO,,DXI,,,,,,,,,
CL,DCD,AR,INV,TXA,ALL,ARO,,DCR,,,,,,,,,
CL,DEX,AR,INV,TXX,ALL,OTO,,,,,,,,,,,
CL,DCR,AR,CRD,TXX,ALL,OTO,,,,,,,,,,,
CL,DCR,AR,CRD,TXX,ALL,ARO,,DEX,,,,,,,,,
CL,DCR,AR,CRD,TXX,ALL,ARO,,DCD,,,,,,,,,
CL,DCD,AR,INV,TXX,ALL,ARO,,DEX,,,,,,,,,
CL,DCD,AR,INV,TXX,ALL,ARO,,DCR,,,,,,,,,
CL,XCL,AR,INV,EXL,ALL,OTO,,,,,,,,,,,
CL,XCL,AR,INV,EXL,ALL,ARO,,XCL,,,,,,,,,
CL,XCL,AR,CRD,EXL,ALL,OTO,,,,,,,,,,,
CL,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,,,,,,,,
CL,XCL,AP,INV,EXL,ALL,ALL,,,,,,,,,,,
CL,XCL,AP,CRD,EXL,ALL,ALL,,,,,,,,,,,";

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse
			=> new Dictionary<string, LedgerOfUse> {
				{ "BHE", LedgerOfUse.AP },
				{ "BOL", LedgerOfUse.AP },
				{ "TCD", LedgerOfUse.AP },
				{ "TCR", LedgerOfUse.AP },
				{ "TEX", LedgerOfUse.AP },
				{ "TXI", LedgerOfUse.AP },
			};

		#region ICountryComplianceElectronicInvoiceEligibleSubType

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "DXI", "DCR", "DCD", "DEX" };

		#endregion

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Chile;

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		public void TestGetIComplianceSubTypeAndNumberUpdateRules()
		{
			var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode);
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();

			AssertEquals(true, complianceSubTypeAndNumberUpdateRules.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed);

			AssertEquals("Default: should be empty", ZString.Empty, complianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(Array.Empty<AccTransactionHeader>()));

			Assert("Default: should be true", complianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(transactionHeader));

			AssertEquals("Default: should be false", false, complianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(transactionHeader));
		}
	}
}
