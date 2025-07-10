using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.ArgentinaOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ArgentinaComplianceInfo))]
	sealed class ArgentinaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Argentina;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "BOL", "CUS", "OCZ", "OXZ", "PCA", "PCB", "PCC", "PDA", "PDB", "PDC", "PXA", "PXB", "PXC", "QCA", "QCB", "QCC", "QCM", "QCZ", "QDA", "QDB", "QDC", "QDM", "QXA", "QXB", "QXC", "QXM", "QXZ", "SPA", "SPB", "TCA", "TCB", "TCC", "TCE", "TCM", "TDA", "TDB", "TDC", "TDE", "TDM", "TXA", "TXB", "TXC", "TXE", "TXM", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "BOL", "PCA", "PCB", "PCC", "PDA", "PDB", "PDC", "PXA", "PXB", "PXC", "TCA", "TCB", "TCC", "TCE", "TCM", "TDA", "TDB", "TDC", "TDE", "TDM", "TXA", "TXB", "TXC", "TXE", "TXM", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "BOL", "CUS", "OCZ", "OXZ", "PCA", "PCB", "PCC", "PDA", "PDB", "PDC", "PXA", "PXB", "PXC", "QCA", "QCB", "QCC", "QCM", "QCZ", "QDA", "QDB", "QDC", "QDM", "QXA", "QXB", "QXC", "QXM", "QXZ", "SPA", "SPB", "TCA", "TCB", "TCC", "TCE", "TCM", "TDA", "TDB", "TDC", "TDE", "TDM", "TXA", "TXB", "TXC", "TXE", "TXM", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "BOL", "PDA", "PDB", "PDC", "PXA", "PXB", "PXC", "TDA", "TDB", "TDC", "TDE", "TDM", "TXA", "TXB", "TXC", "TXE", "TXM", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "BOL", "CUS", "OXZ", "PDA", "PDB", "PDC", "PXA", "PXB", "PXC", "QDA", "QDB", "QDC", "QDM", "QXA", "QXB", "QXC", "QXM", "QXZ", "SPA", "SPB", "TDA", "TDB", "TDC", "TDE", "TDM", "TXA", "TXB", "TXC", "TXE", "TXM", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "BOL", "PCA", "PCB", "PCC", "TCA", "TCB", "TCC", "TCE", "TCM", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "BOL", "CUS", "OCZ", "PCA", "PCB", "PCC", "QCA", "QCB", "QCC", "QCM", "QCZ", "SPA", "SPB", "TCA", "TCB", "TCC", "TCE", "TCM", "XCL" };

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "TXA", "TDA", "TCA", "TXB", "TDB", "TCB", "TXC", "TDC", "TCC", "TXE", "TDE", "PXA", "PDA", "PCA", "PXB", "PDB", "TCE", "PXC", "PDC", "PCC", "TXM", "TDM", "TCM", "PCB" };

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
			{ "QCZ", LedgerOfUse.AP }, { "QXC", LedgerOfUse.AP }, { "QCA", LedgerOfUse.AP }, { "QCB", LedgerOfUse.AP },
			{ "QCC", LedgerOfUse.AP }, { "QDA", LedgerOfUse.AP }, { "QDB", LedgerOfUse.AP }, { "QDC", LedgerOfUse.AP },
			{ "QXM", LedgerOfUse.AP }, { "QCM", LedgerOfUse.AP }, { "QDM", LedgerOfUse.AP }, { "SPA", LedgerOfUse.AP },
			{ "SPB", LedgerOfUse.AP }, { "CUS", LedgerOfUse.AP }, { "QXA", LedgerOfUse.AP }, { "QXB", LedgerOfUse.AP },
			{ "QXZ", LedgerOfUse.AP }, { "OCZ", LedgerOfUse.AP }, { "OXZ", LedgerOfUse.AP }
		};

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse>
		{
			{ "OCZ", TransactionTypeOfUse.CRD }, { "OXZ", TransactionTypeOfUse.INV }, { "PCA", TransactionTypeOfUse.CRD }, { "PCB", TransactionTypeOfUse.CRD },
			{ "PCC", TransactionTypeOfUse.CRD }, { "PDA", TransactionTypeOfUse.INV }, { "PDB", TransactionTypeOfUse.INV }, { "PDC", TransactionTypeOfUse.INV },
			{ "PXA", TransactionTypeOfUse.INV }, { "PXB", TransactionTypeOfUse.INV }, { "PXC", TransactionTypeOfUse.INV }, { "QCA", TransactionTypeOfUse.CRD },
			{ "QCB", TransactionTypeOfUse.CRD }, { "QCC", TransactionTypeOfUse.CRD }, { "QCM", TransactionTypeOfUse.CRD }, { "QCZ", TransactionTypeOfUse.CRD },
			{ "QDA", TransactionTypeOfUse.INV }, { "QDB", TransactionTypeOfUse.INV }, { "QDC", TransactionTypeOfUse.INV }, { "QDM", TransactionTypeOfUse.INV },
			{ "QXA", TransactionTypeOfUse.INV }, { "QXB", TransactionTypeOfUse.INV }, { "QXC", TransactionTypeOfUse.INV }, { "QXM", TransactionTypeOfUse.INV },
			{ "QXZ", TransactionTypeOfUse.INV }, { "TCA", TransactionTypeOfUse.CRD }, { "TCB", TransactionTypeOfUse.CRD }, { "TCC", TransactionTypeOfUse.CRD },
			{ "TCE", TransactionTypeOfUse.CRD }, { "TCM", TransactionTypeOfUse.CRD }, { "TDA", TransactionTypeOfUse.INV }, { "TDB", TransactionTypeOfUse.INV },
			{ "TDC", TransactionTypeOfUse.INV }, { "TDE", TransactionTypeOfUse.INV }, { "TDM", TransactionTypeOfUse.INV }, { "TXA", TransactionTypeOfUse.INV },
			{ "TXB", TransactionTypeOfUse.INV }, { "TXC", TransactionTypeOfUse.INV }, { "TXE", TransactionTypeOfUse.INV }, { "TXM", TransactionTypeOfUse.INV }
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCB";

		protected override string ExpectedComplianceSubTypeDescription => "08 Type B Credit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "NOTA DE CRÉDITO B";

		protected override Func<ComplianceSubTypeAttributionRuleConfiguration, string>[] ExpectedComplianceRuleFields =>
			new Func<ComplianceSubTypeAttributionRuleConfiguration, string>[]
			{
				x => x.Country,
				x => x.SubType,
				x => x.LedgerType,
				x => x.InvoiceType,
				x => x.TaxInvoiceRule,
				x => x.DisbursementRule,
				x => x.OriginalRule,
				x => x.OrganisationLocation,
				x => x.ParentTransactionSubType,
				x => x.TaxRegistrationType,
				x => x.RuleSetCode,
				x => x.RuleSetDescription,
				x => x.SelfBillingRule,
				x => x.TaxRegistrationLocationRule,
				x => x.VATGroupRule,
				x => x.TaxIDCode,
				x => x.RequiredTaxSystem,
				x => x.ExcludedTaxSystem,
				x => x.RequiredRegistrationCode,
				x => x.ExcludedRegistrationCode,
				x => x.ThresholdApplies ? "Y" : "N",
				x => x.SubTypeThresholdNotMet,
			};

		protected override string ExpectedComplianceRules => @"AR,TXA,AR,INV,TID,ALL,OTO,AR,,REC,1,Tipo A, B & E,,,,,,,,MIP,N,
AR,TXB,AR,INV,TID,ALL,OTO,AR,,NOT,1,Tipo A, B & E,,,,,,,,MIP,N,
AR,TDA,AR,INV,TID,ALL,ARO,AR,TXA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,TDA,AR,INV,TID,ALL,ARO,AR,TCA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,TCA,AR,CRD,TID,ALL,ARO,AR,TXA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,TCA,AR,CRD,TID,ALL,ARO,AR,TDA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,AR,TXB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,AR,TCB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,AR,TXB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,AR,TDB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TCA,AR,CRD,TID,ALL,OTO,AR,,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,OTO,AR,,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TXE,AR,INV,TXX,ALL,OTO,,,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TDE,AR,INV,TXX,ALL,ARO,,TXE,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TCE,AR,CRD,TXX,ALL,ARO,,TXE,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,TCE,AR,CRD,TXX,ALL,OTO,,,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,OTO,,,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,ARO,,XCL,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,OTO,,,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,ARO,,XCL,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,OTO,,,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,ARO,,XCL,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,OTO,,,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,ARO,,XCL,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,PXA,AR,INV,TID,ALL,OTO,AR,,REC,1,Tipo A, B & E,,,,,,,MIP,,Y,TXA
AR,PCA,AR,CRD,TID,ALL,ARO,AR,PXA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,PCA,AR,CRD,TID,ALL,ARO,AR,PDA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,PDA,AR,INV,TID,ALL,ARO,AR,PXA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,PDA,AR,INV,TID,ALL,ARO,AR,PCA,REC,1,Tipo A, B & E,,,,,,,,,N,
AR,PXB,AR,INV,TID,ALL,OTO,AR,,NOT,1,Tipo A, B & E,,,,,,,MIP,,Y,TXB
AR,PCB,AR,CRD,TID,ALL,ARO,AR,PXB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,PCB,AR,CRD,TID,ALL,ARO,AR,PDB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,PDB,AR,INV,TID,ALL,ARO,AR,PXB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,PDB,AR,INV,TID,ALL,ARO,AR,PCB,NOT,1,Tipo A, B & E,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,OTO,AR,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,OTO,AR,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,OTO,,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,OTO,,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TXA,AR,INV,TID,ALL,OTO,AR,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,MIP,N,
AR,TDA,AR,INV,TID,ALL,ARO,AR,TCA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TCA,AR,CRD,TID,ALL,ARO,AR,TDA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TCA,AR,CRD,TID,ALL,OTO,AR,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TXB,AR,INV,TID,ALL,OTO,,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,MIP,N,
AR,TDB,AR,INV,TID,ALL,ARO,,TCB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,,TDB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,OTO,,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,OTO,,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,ARO,,XCL,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,OTO,,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,INV,EXL,ALL,ARO,,XCL,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,OTO,,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,ARO,,XCL,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,OTO,,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AP,CRD,EXL,ALL,ARO,,XCL,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,ARO,AR,XCL,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,ARO,AR,XCL,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,ARO,,XCL,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,ARO,,XCL,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TCA,AR,CRD,TID,ALL,ARO,AR,TXA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,,TXB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TDA,AR,INV,TID,ALL,ARO,AR,TXA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,,TXB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PXA,AR,INV,TID,ALL,OTO,AR,,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,MIP,,Y,TXA
AR,PCA,AR,CRD,TID,ALL,ARO,AR,PXA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PCA,AR,CRD,TID,ALL,ARO,AR,PDA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PDA,AR,INV,TID,ALL,ARO,AR,PXA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PDA,AR,INV,TID,ALL,ARO,AR,PCA,REC,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PXB,AR,INV,TID,ALL,OTO,AR,,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,MIP,,Y,TXB
AR,PCB,AR,CRD,TID,ALL,ARO,AR,PXB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PCB,AR,CRD,TID,ALL,ARO,AR,PDB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PDB,AR,INV,TID,ALL,ARO,AR,PXB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,PDB,AR,INV,TID,ALL,ARO,AR,PCB,NOT,2,Tipo A & B & Excluded Supply as XCL,,,,,,,,,N,
AR,TXM,AR,INV,TID,ALL,OTO,AR,,REC,3,Type M and B,,,,,,,,,N,
AR,TDM,AR,INV,TID,ALL,ARO,AR,TCM,REC,3,Type M and B,,,,,,,,,N,
AR,TDM,AR,INV,TID,ALL,ARO,AR,TXM,REC,3,Type M and B,,,,,,,,,N,
AR,TCM,AR,CRD,TID,ALL,ARO,AR,TXM,REC,3,Type M and B,,,,,,,,,N,
AR,TCM,AR,CRD,TID,ALL,ARO,AR,TDM,REC,3,Type M and B,,,,,,,,,N,
AR,TCM,AR,CRD,TID,ALL,OTO,AR,,REC,3,Type M and B,,,,,,,,,N,
AR,TXB,AR,INV,TID,ALL,OTO,,,NOT,3,Type M and B,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,,TCB,NOT,3,Type M and B,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,,TXB,NOT,3,Type M and B,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,,TXB,NOT,3,Type M and B,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,,TDB,NOT,3,Type M and B,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,OTO,,,NOT,3,Type M and B,,,,,,,,,N,
AR,TXM,AR,INV,TID,ALL,OTO,AR,,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TDM,AR,INV,TID,ALL,ARO,AR,TCM,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TDM,AR,INV,TID,ALL,ARO,AR,TXM,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TCM,AR,CRD,TID,ALL,ARO,AR,TXM,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TCM,AR,CRD,TID,ALL,ARO,AR,TDM,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TCM,AR,CRD,TID,ALL,OTO,AR,,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TXB,AR,INV,TID,ALL,OTO,,,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,,TCB,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TDB,AR,INV,TID,ALL,ARO,,TXB,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,,TXB,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,ARO,,TDB,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TCB,AR,CRD,TID,ALL,OTO,,,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,OTO,AR,,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,OTO,AR,,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,OTO,,,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,OTO,,,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,ARO,AR,XCL,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,ARO,AR,XCL,REC,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,ARO,,XCL,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,ARO,,XCL,NOT,4,Type M, B & Excluded supply as XCL,,,,,,,,,N,
AR,TXC,AR,INV,TID,ALL,OTO,,,NOT,5,Type C,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,,TCC,NOT,5,Type C,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,,TXC,NOT,5,Type C,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,,TXC,NOT,5,Type C,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,,TDC,NOT,5,Type C,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,OTO,,,NOT,5,Type C,,,,,,,,,N,
AR,TXC,AR,INV,TID,ALL,OTO,AR,,REC,5,Type C,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,AR,TCC,REC,5,Type C,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,AR,TXC,REC,5,Type C,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,AR,TXC,REC,5,Type C,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,AR,TDC,REC,5,Type C,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,OTO,AR,,REC,5,Type C,,,,,,,,,N,
AR,TXC,AR,INV,TID,ALL,OTO,,,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,,TCC,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,,TXC,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,,TXC,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,,TDC,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,OTO,,,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TXC,AR,INV,TID,ALL,OTO,AR,,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,AR,TCC,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TDC,AR,INV,TID,ALL,ARO,AR,TXC,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,AR,TXC,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,ARO,AR,TDC,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,TCC,AR,CRD,TID,ALL,OTO,AR,,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,OTO,AR,,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,OTO,AR,,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,OTO,,,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,OTO,,,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,ARO,AR,XCL,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,ARO,AR,XCL,REC,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,CRD,EXL,ALL,ARO,,XCL,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,
AR,XCL,AR,INV,EXL,ALL,ARO,,XCL,NOT,6,Type C & Excluded supply as XCL,,,,,,,,,N,";

		protected override string ExpectedComplianceSequencePrefixErrorMessage => "Please enter a number between 1 and 99998";

		protected override string ExpectedComplianceSequencePrefixRegex => @"^(0{0,4}[1-9]|0{0,3}[1-9][0-9]|0{0,2}[1-9][0-9][0-9]|0{0,1}[1-9][0-9][0-9][0-9]|(?![9]{5})+([1-9][0-9][0-9][0-9][0-9]))$";

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Argentina()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(ArgentinaComplianceInfo.RuleSetCodes.TipoABE, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(6, ruleSet.Count);
			Assert(ruleSet.ContainsCode(ArgentinaComplianceInfo.RuleSetCodes.TipoABE));
			Assert(ruleSet.ContainsCode(ArgentinaComplianceInfo.RuleSetCodes.TipoABExcludedSupply));
			Assert(ruleSet.ContainsCode(ArgentinaComplianceInfo.RuleSetCodes.TipoMB));
			Assert(ruleSet.ContainsCode(ArgentinaComplianceInfo.RuleSetCodes.TipoMBExcludedSupply));
			Assert(ruleSet.ContainsCode(ArgentinaComplianceInfo.RuleSetCodes.TipoC));
			Assert(ruleSet.ContainsCode(ArgentinaComplianceInfo.RuleSetCodes.TipoCExcludedSupply));
		}

		public void TestIsAssignableFrom()
		{
			var isAssignableFrom = typeof(IBankAccountValidation).IsAssignableFrom(typeof(ArgentinaComplianceInfo));
			AssertEquals("ArgentinaComplianceInfo must implement IBankAccountValidation interface.", isAssignableFrom, true);
		}

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "1", Description = (NoResString)"No Gravado", Bool = true, RelatedItemCode = "1" },
				new CodeDescriptionBoolRelatedItem() { Code = "2", Description = (NoResString)"Exento", Bool = true, RelatedItemCode = "2" },
				new CodeDescriptionBoolRelatedItem() { Code = "3", Description = (NoResString)"IVA 0%", Bool = true, RelatedItemCode = "3" },
				new CodeDescriptionBoolRelatedItem() { Code = "4", Description = (NoResString)"IVA 10,5%", Bool = true, RelatedItemCode = "4" },
				new CodeDescriptionBoolRelatedItem() { Code = "5", Description = (NoResString)"IVA 21%", Bool = true, RelatedItemCode = "5" },
				new CodeDescriptionBoolRelatedItem() { Code = "6", Description = (NoResString)"IVA 27%", Bool = true, RelatedItemCode = "6" },
				new CodeDescriptionBoolRelatedItem() { Code = "8", Description = (NoResString)"IVA 5%", Bool = false, RelatedItemCode = "8" },
				new CodeDescriptionBoolRelatedItem() { Code = "9", Description = (NoResString)"IVA 2,5%", Bool = false, RelatedItemCode = "9" }
			};

		public void TestGetEquivalentComplianceSubType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("1", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA));
				AssertEquals("2", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA));
				AssertEquals("3", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA));
				AssertEquals("6", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB));
				AssertEquals("7", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB));
				AssertEquals("8", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB));
				AssertEquals("11", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC));
				AssertEquals("12", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDC));
				AssertEquals("13", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCC));
				AssertEquals("19", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE));
				AssertEquals("20", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE));
				AssertEquals("21", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE));
				AssertEquals("51", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXM));
				AssertEquals("52", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM));
				AssertEquals("53", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM));
				AssertEquals("201", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA));
				AssertEquals("202", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA));
				AssertEquals("203", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA));
				AssertEquals("206", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB));
				AssertEquals("207", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB));
				AssertEquals("208", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB));
				AssertEquals("211", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC));
				AssertEquals("212", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC));
				AssertEquals("213", ObjectFactory.Get<ICountryComplianceFactory>().GetIEquivalentComplianceSubTypeProvider(CountryCode).GetEquivalentComplianceSubType(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC));
			});
		}

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Argentina;

		public void TestIsOrganizationTaxRecoverable()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var rule = new ComplianceSubTypeAttributionRuleConfiguration();
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;

			var builder = new ArgentinaComplianceInfo() as IComplianceSubTypeTaxRegistrationTypeRuleProvider;
			var result = false;

			var orgCusCodes = from field in typeof(OrgCusCodes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
							  let codes = (string)field.GetValue(null)
							  select codes;

			foreach (var code in orgCusCodes)
			{
				orgHeader.CustomsCodes.RemoveAll();
				orgHeader.CustomsCodes.AddNew(code, "1111111", CountryCode);
				result = builder.IsTaxRegistrationTypeRuleApplicable(rule, orgHeader);

				if (code == OrgCusCodes.IVI || code == OrgCusCodes.IVM || code == OrgCusCodes.IVR)
				{
					Assert(result);
				}
				else
				{
					Assert(!result);
				}
			}
		}

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate;

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true), "Cannot change default value for country/region 'AR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true), "Cannot change default value for country/region 'AR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true), "Cannot change default value for country/region 'AR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), $"'GVT' is not valid for country/region 'AR' when registry '{AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Caption}' is not enabled." },
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
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), $"'GVT' is not valid for country/region 'AR' when registry '{AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Caption}' is not enabled." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), string.Empty },
			};

		#region E-Reporting Compliance Date

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2022, 10, 1) : new ZDate(2022, 8, 1);

		#endregion
	}
}
