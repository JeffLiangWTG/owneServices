using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.ArgentinaOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ArgentinaComplianceInfo : CountryComplianceInfo,
		IComplianceRegistryDefaultProvider,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeTaxRegistrationTypeRuleProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		ITaxMessagesGroupProvider,
		IBankAccountValidation,
		IEquivalentComplianceSubTypeProvider,
		IQRCodeDataProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Argentina;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.CUIT;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.BOL,
				ComplianceSubTypes.CUS,
				ComplianceSubTypes.OCZ,
				ComplianceSubTypes.OXZ,
				ComplianceSubTypes.PCA,
				ComplianceSubTypes.PCB,
				ComplianceSubTypes.PCC,
				ComplianceSubTypes.PDA,
				ComplianceSubTypes.PDB,
				ComplianceSubTypes.PDC,
				ComplianceSubTypes.PXA,
				ComplianceSubTypes.PXB,
				ComplianceSubTypes.PXC,
				ComplianceSubTypes.QCA,
				ComplianceSubTypes.QCB,
				ComplianceSubTypes.QCC,
				ComplianceSubTypes.QCM,
				ComplianceSubTypes.QCZ,
				ComplianceSubTypes.QDA,
				ComplianceSubTypes.QDB,
				ComplianceSubTypes.QDC,
				ComplianceSubTypes.QDM,
				ComplianceSubTypes.QXA,
				ComplianceSubTypes.QXB,
				ComplianceSubTypes.QXC,
				ComplianceSubTypes.QXM,
				ComplianceSubTypes.QXZ,
				ComplianceSubTypes.SPA,
				ComplianceSubTypes.SPB,
				ComplianceSubTypes.TCA,
				ComplianceSubTypes.TCB,
				ComplianceSubTypes.TCC,
				ComplianceSubTypes.TCE,
				ComplianceSubTypes.TCM,
				ComplianceSubTypes.TDA,
				ComplianceSubTypes.TDB,
				ComplianceSubTypes.TDC,
				ComplianceSubTypes.TDE,
				ComplianceSubTypes.TDM,
				ComplianceSubTypes.TXA,
				ComplianceSubTypes.TXB,
				ComplianceSubTypes.TXC,
				ComplianceSubTypes.TXE,
				ComplianceSubTypes.TXM,
				ComplianceSubTypes.XCL
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string BOL = "BOL";
			public const string CUS = "CUS";
			public const string OCZ = "OCZ";
			public const string OXZ = "OXZ";
			public const string PCA = "PCA";
			public const string PCB = "PCB";
			public const string PCC = "PCC";
			public const string PDA = "PDA";
			public const string PDB = "PDB";
			public const string PDC = "PDC";
			public const string PXA = "PXA";
			public const string PXB = "PXB";
			public const string PXC = "PXC";
			public const string QCA = "QCA";
			public const string QCB = "QCB";
			public const string QCC = "QCC";
			public const string QCM = "QCM";
			public const string QCZ = "QCZ";
			public const string QDA = "QDA";
			public const string QDB = "QDB";
			public const string QDC = "QDC";
			public const string QDM = "QDM";
			public const string QXA = "QXA";
			public const string QXB = "QXB";
			public const string QXC = "QXC";
			public const string QXM = "QXM";
			public const string QXZ = "QXZ";
			public const string SPA = "SPA";
			public const string SPB = "SPB";
			public const string TCA = "TCA";
			public const string TCB = "TCB";
			public const string TCC = "TCC";
			public const string TCE = "TCE";
			public const string TCM = "TCM";
			public const string TDA = "TDA";
			public const string TDB = "TDB";
			public const string TDC = "TDC";
			public const string TDE = "TDE";
			public const string TDM = "TDM";
			public const string TXA = "TXA";
			public const string TXB = "TXB";
			public const string TXC = "TXC";
			public const string TXE = "TXE";
			public const string TXM = "TXM";
			public const string XCL = "XCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString BOL => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|BOL", "Other Vouchers / Invoices");
			public static MultilingualString CUS => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|CUS", "66 Import Clearance");
			public static MultilingualString OCZ => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|OCZ", "90 Other documents - Credit Notes");
			public static MultilingualString OXZ => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|OXZ", "99 Other Documents");
			public static MultilingualString PCA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PCA", "203 Type A Credit Note - {0}", "MiPyME");
			public static MultilingualString PCB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PCB", "208 Type B Credit Note - {0}", "MiPyME");
			public static MultilingualString PCC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PCC", "213 Type C Credit Note - {0}", "MiPyME");
			public static MultilingualString PDA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PDA", "202 Type A Debit Note - {0}", "MiPyME");
			public static MultilingualString PDB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PDB", "207 Type B Debit Note - {0}", "MiPyME");
			public static MultilingualString PDC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PDC", "212 Type C Debit Note - {0}", "MiPyME");
			public static MultilingualString PXA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PXA", "201 Type A Invoice - {0}", "MiPyME");
			public static MultilingualString PXB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PXB", "206 Type B Invoice - {0}", "MiPyME");
			public static MultilingualString PXC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|PXC", "211 Type C Invoice - {0}", "MiPyME");
			public static MultilingualString QCA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QCA", "112 Type A Credit Note Ticket");
			public static MultilingualString QCB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QCB", "113 Type B Credit Note Ticket");
			public static MultilingualString QCC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QCC", "114 Type C Credit Note Ticket");
			public static MultilingualString QCM => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QCM", "119 Type M Credit Note Ticket");
			public static MultilingualString QCZ => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QCZ", "110 Credit Note Ticket");
			public static MultilingualString QDA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QDA", "115 Type A Debit Note Ticket");
			public static MultilingualString QDB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QDB", "116 Type B Debit Note Ticket");
			public static MultilingualString QDC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QDC", "117 Type C Debit Note Ticket");
			public static MultilingualString QDM => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QDM", "120 Type M Debit Note Ticket");
			public static MultilingualString QXA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QXA", "81 Type A Ticket");
			public static MultilingualString QXB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QXB", "82 Type B Ticket");
			public static MultilingualString QXC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QXC", "111 Type C Ticket");
			public static MultilingualString QXM => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QXM", "118 Type M Ticket");
			public static MultilingualString QXZ => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|QXZ", "83 Ticket");
			public static MultilingualString SPA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|SPA", "17 Type A - Public Services");
			public static MultilingualString SPB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|SPB", "18 Type B - Public Services");
			public static MultilingualString TCA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TCA", "03 Type A Credit Note");
			public static MultilingualString TCB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TCB", "08 Type B Credit Note");
			public static MultilingualString TCC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TCC", "13 Type C Credit Note");
			public static MultilingualString TCE => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TCE", "21 Type E Credit Note");
			public static MultilingualString TCM => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TCM", "53 Type M Credit Note");
			public static MultilingualString TDA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TDA", "02 Type A Debit Note");
			public static MultilingualString TDB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TDB", "07 Type B Debit Note");
			public static MultilingualString TDC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TDC", "12 Type C Debit Note");
			public static MultilingualString TDE => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TDE", "20 Type E Debit Note");
			public static MultilingualString TDM => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TDM", "52 Type M Debit Note");
			public static MultilingualString TXA => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXA", "01 Type A Invoice");
			public static MultilingualString TXB => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXB", "06 Type B Invoice");
			public static MultilingualString TXC => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXC", "11 Type C Invoice");
			public static MultilingualString TXE => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXE", "19 Type E Invoice");
			public static MultilingualString TXM => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXM", "51 Type M Invoice");
			public static MultilingualString XCL => ResString.GetMultilingualString("ARComplianceSubTypeCodeList|XCL", "Reimbursement / Disbursement / Excluded Supply");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string BOL = "BOLETA";
			public const string CUS = "DESPACHO DE IMPORTACIÓN";
			public const string OCZ = "OTROS COMPROBANTES - DOCUMENTOS EXCEPTUADOS - NOTAS DE CRÉDITO";
			public const string OXZ = "OTROS COMPROBANTES QUE NO CUMPLEN O ESTÁN EXCEPTUADOS DE LA R.G. 1415 Y SUS MODIF";
			public const string PCA = "NOTA DE CRÉDITO ELECTRÓNICA MiPyME (FCE) A";
			public const string PCB = "NOTA DE CRÉDITO ELECTRÓNICA MiPyME (FCE) B";
			public const string PCC = "NOTA DE CRÉDITO ELECTRÓNICA MiPyME (FCE) C";
			public const string PDA = "NOTA DE DÉBITO ELECTRÓNICA MiPyME (FCE) A";
			public const string PDB = "NOTA DE DÉBITO ELECTRÓNICA MiPyME (FCE) B";
			public const string PDC = "NOTA DE DÉBITO ELECTRÓNICA MiPyME (FCE) C";
			public const string PXA = "FACTURA DE CRÉDITO ELECTRÓNICA MiPyME (FCE) A";
			public const string PXB = "FACTURA DE CRÉDITO ELECTRÓNICA MiPyME (FCE) B";
			public const string PXC = "FACTURA DE CRÉDITO ELECTRÓNICA MiPyME (FCE) C";
			public const string QCA = "TIQUE NOTA DE CRÉDITO A";
			public const string QCB = "TIQUE NOTA DE CRÉDITO B";
			public const string QCC = "TIQUE NOTA DE CRÉDITO C";
			public const string QCM = "TIQUE NOTA DE CRÉDITO M";
			public const string QCZ = "TIQUE NOTA DE CRÉDITO";
			public const string QDA = "TIQUE NOTA DE DÉBITO A";
			public const string QDB = "TIQUE NOTA DE DÉBITO B";
			public const string QDC = "TIQUE NOTA DE DÉBITO C";
			public const string QDM = "TIQUE NOTA DE DÉBITO M";
			public const string QXA = "TIQUE FACTURA A";
			public const string QXB = "TIQUE FACTURA B";
			public const string QXC = "TIQUE FACTURA C";
			public const string QXM = "TIQUE FACTURA M";
			public const string QXZ = "TIQUE";
			public const string SPA = "LIQUIDACIÓN DE SERVICIOS PÚBLICOS CLASE A";
			public const string SPB = "LIQUIDACIÓN DE SERVICIOS PÚBLICOS CLASE B";
			public const string TCA = "NOTA DE CRÉDITO A";
			public const string TCB = "NOTA DE CRÉDITO B";
			public const string TCC = "NOTA DE CRÉDITO C";
			public const string TCE = "NOTA DE CRÉDITO E";
			public const string TCM = "NOTA DE CRÉDITO M";
			public const string TDA = "NOTA DE DÉBITO A";
			public const string TDB = "NOTA DE DÉBITO B";
			public const string TDC = "NOTA DE DÉBITO C";
			public const string TDE = "NOTA DE DÉBITO E";
			public const string TDM = "NOTA DE DÉBITO M";
			public const string TXA = "FACTURA TIPO A";
			public const string TXB = "FACTURA TIPO B";
			public const string TXC = "FACTURA TIPO C";
			public const string TXE = "FACTURA TIPO E";
			public const string TXM = "FACTURA TIPO M";
			public const string XCL = "DOCUMENTO DE REEMBOLSO";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string BOL = "Other Documents. Not Reported. Used when recording Accounts Payable transactions equivalent to ‘receipts’ and other documents supporting payable transactions with no right to claim input tax credit. No amount of VAT is recorded against the transaction. This is a document type that would not be reported to AFIPUsed in payable to record payables documents equivalent to ‘receipts’ and other documents without right to claim input tax credit.";
			public const string CUS = "Other Documents. AFIP Document Code 66. Used in Payables. Document issued by a customs broker/importer.";
			public const string OCZ = "Other Documents. AFIP Document Code 90. This compliance Sub Type is used for Credit Note transactions that cannot be categorized with the other document types, for example, bank statements.";
			public const string OXZ = "Other Documents. AFIP Document Code 99. This compliance Sub Type is used for Invoice transactions that cannot be categorized with the other document types, for example, bank statements.";
			public const string PCA = "MiPyME document. AFIP Document Code 203.  Tipo A MiPyME Credit Note.";
			public const string PCB = "MiPyME document. AFIP Document Code 208.  Tipo B MiPyME Credit Note.";
			public const string PCC = "MiPyME document. AFIP Document Code 213.  Tipo C MiPyME Credit Note.";
			public const string PDA = "MiPyME document. AFIP Document Code 202. Tipo A MiPyME Amending Invoice.";
			public const string PDB = "MiPyME document. AFIP Document Code 207. Tipo B MiPyME Amending Invoice.";
			public const string PDC = "MiPyME document. AFIP Document Code 212. Tipo C MiPyME Amending Invoice.";
			public const string PXA = "MiPyME document. AFIP Document Code 201. Tipo A MiPyME Invoice. Note about MiPyME Regime: Specific Small and Medium businesses registered to participate in the Factura de Crédito Invoicing regime must issue this class of invoice when billing specific \"large taxpayer\" organizations.";
			public const string PXB = "MiPyME document. AFIP Document Code 206. Tipo B MiPyME Invoice.";
			public const string PXC = "MiPyME document. AFIP Document Code 211. Tipo C MiPyME Invoice.";
			public const string QCA = "Ticket Documents. AFIP Document Code 112. Used in Payables when a document of this class is received.";
			public const string QCB = "Ticket Documents. AFIP Document Code 113. Used in Payables when a document of this class is received.";
			public const string QCC = "Ticket Documents. AFIP Document Code 114. Used in Payables when a document of this class is received.";
			public const string QCM = "Ticket Documents. AFIP Document Code 119. Used in Payables when a document of this class is received.";
			public const string QCZ = "Ticket Documents. AFIP Document Code 110. Used in Payables when a document of this class is received.";
			public const string QDA = "Ticket Documents. AFIP Document Code 115. Used in Payables when a document of this class is received.";
			public const string QDB = "Ticket Documents. AFIP Document Code 116. Used in Payables when a document of this class is received.";
			public const string QDC = "Ticket Documents. AFIP Document Code 117. Used in Payables when a document of this class is received.";
			public const string QDM = "Ticket Documents. AFIP Document Code 120. Used in Payables when a document of this class is received";
			public const string QXA = "Ticket Documents. AFIP Document Code 81. Companies that are \"controladores fiscales\" have a special printer to issue their invoices called ‘Tique’. Sub Types in this category will be used to record certain types of Payables transaction when the relevant fiscal document is received from the supplier.";
			public const string QXB = "Ticket Documents. AFIP Document Code 82. Used in Payables when a document of this class is received.";
			public const string QXC = "Ticket Documents. AFIP Document Code 111. Used in Payables when a document of this class is received.";
			public const string QXM = "Ticket Documents. AFIP Document Code 118. Used in Payables when a document of this class is received.";
			public const string QXZ = "Ticket Documents. AFIP Document Code 83. Used in Payables when a document of this class is received.";
			public const string SPA = "Other Documents. AFIP Document Code 17. Used in Payables. Invoices issued by electrical/water/gas service providers (public services) to IVA Responsible customers.";
			public const string SPB = "Other Documents. AFIP Document Code 18. Used in Payables. Invoices issued by electrical/water/gas service providers (public services) to IVA Not Responsible customers.";
			public const string TCA = "General Sub type. AFIP Document Code 3. Credit Note amending usually a parent Factura Tipo A transaction.";
			public const string TCB = "General Sub type. AFIP Document Code 8. Credit Note issued usually amending a Compliance subtype “Tipo B” Factura";
			public const string TCC = "General Sub type. AFIP Document Code 13. Credit Note issued usually amending a Compliance subtype “Tipo C” Factura.";
			public const string TCE = "General Sub type. AFIP Document Code 21. Credit Note issued amending a Compliance subtype “Tipo E” Factura.";
			public const string TCM = "General Sub type. AFIP Document Code 53. Credit Note issued usually amending a Compliance subtype “Tipo M” Factura.";
			public const string TDA = "General Sub type. AFIP Document Code 2. Amending Invoice issued usually against a parent Factura Tipo A transaction, or a Stand Alone Debit Note recording charges that do not require a Factura.";
			public const string TDB = "General Sub type. AFIP Document Code 7. Amending Invoice issued usually against a parent Factura Tipo B transaction, or a Stand Alone Debit Note recording charges that do not require a Factura.";
			public const string TDC = "General Sub type. AFIP Document Code 12.  Amending Invoice issued usually against a parent Factura Tipo C transaction, or a Stand Alone Debit Note recording charges that do not require a Factura.";
			public const string TDE = "General Sub type. AFIP Document Code 20. Amending Invoice issued against a parent Factura Tipo E transaction.";
			public const string TDM = "General Sub type. AFIP Document Code 52.Amending Invoice issued usually against a parent Factura Tipo M transaction, or a Stand Alone Debit Note recording charges that do not require a Factura.";
			public const string TXA = "General Sub type. AFIP Document Code 1. Invoice issued by an IVA Responsible taxpayer to another IVA Responsible taxpayer. Note about General sub Types: Tipo A, B, C, E and M Factura, Debit Notes and Credit Notes are the most commonly issued and received document types.";
			public const string TXB = "General Sub type. AFIP Document Code 6. Invoice issued by an IVA Responsible taxpayer to IVA Exempt, Monotributista, Consumidor Final or any other ‘IVA Not Responsible’ customer.";
			public const string TXC = "General Sub type. AFIP Document Code 11. Invoice issued by an IVA Responsible Monotributo taxpayer.";
			public const string TXE = "General Sub type. AFIP Document Code 19. Invoice issued for exportations or deemed exportations (eg. Tierra del Fuego).";
			public const string TXM = "General Sub type. AFIP Document Code 51. Document issued by IVA Responsible taxpayer obliged to issue \"Tipo M\" invoices to IVA Responsible taxpayers instead of Tipo A documents.";
			public const string XCL = "Other Documents. Used to formally identify Payables \"Reimbursement / Disbursement / Excluded Supply\" transactions. May also be used for Receivables Invoices (depending on your business policy, to issue “Documento de Reembolso” when an Invoice transaction only contains “Exclude” Tax ID charges. Transactions with XCL will NOT be sent to AFIP.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType BOL => new ComplianceSubType(ComplianceSubTypeCodes.BOL, () => ComplianceSubTypeDescriptions.BOL, () => ComplianceSubTypeLocalDescriptions.BOL, () => ComplianceSubTypeInternalImplemenationNote.BOL);
			public static ComplianceSubType CUS => new ComplianceSubType(ComplianceSubTypeCodes.CUS, () => ComplianceSubTypeDescriptions.CUS, () => ComplianceSubTypeLocalDescriptions.CUS, () => ComplianceSubTypeInternalImplemenationNote.CUS, LedgerOfUse.AP);
			public static ComplianceSubType OCZ => new ComplianceSubType(ComplianceSubTypeCodes.OCZ, () => ComplianceSubTypeDescriptions.OCZ, () => ComplianceSubTypeLocalDescriptions.OCZ, () => ComplianceSubTypeInternalImplemenationNote.OCZ, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType OXZ => new ComplianceSubType(ComplianceSubTypeCodes.OXZ, () => ComplianceSubTypeDescriptions.OXZ, () => ComplianceSubTypeLocalDescriptions.OXZ, () => ComplianceSubTypeInternalImplemenationNote.OXZ, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PCA => new ComplianceSubType(ComplianceSubTypeCodes.PCA, () => ComplianceSubTypeDescriptions.PCA, () => ComplianceSubTypeLocalDescriptions.PCA, () => ComplianceSubTypeInternalImplemenationNote.PCA, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType PCB => new ComplianceSubType(ComplianceSubTypeCodes.PCB, () => ComplianceSubTypeDescriptions.PCB, () => ComplianceSubTypeLocalDescriptions.PCB, () => ComplianceSubTypeInternalImplemenationNote.PCB, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType PCC => new ComplianceSubType(ComplianceSubTypeCodes.PCC, () => ComplianceSubTypeDescriptions.PCC, () => ComplianceSubTypeLocalDescriptions.PCC, () => ComplianceSubTypeInternalImplemenationNote.PCC, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType PDA => new ComplianceSubType(ComplianceSubTypeCodes.PDA, () => ComplianceSubTypeDescriptions.PDA, () => ComplianceSubTypeLocalDescriptions.PDA, () => ComplianceSubTypeInternalImplemenationNote.PDA, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PDB => new ComplianceSubType(ComplianceSubTypeCodes.PDB, () => ComplianceSubTypeDescriptions.PDB, () => ComplianceSubTypeLocalDescriptions.PDB, () => ComplianceSubTypeInternalImplemenationNote.PDB, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PDC => new ComplianceSubType(ComplianceSubTypeCodes.PDC, () => ComplianceSubTypeDescriptions.PDC, () => ComplianceSubTypeLocalDescriptions.PDC, () => ComplianceSubTypeInternalImplemenationNote.PDC, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PXA => new ComplianceSubType(ComplianceSubTypeCodes.PXA, () => ComplianceSubTypeDescriptions.PXA, () => ComplianceSubTypeLocalDescriptions.PXA, () => ComplianceSubTypeInternalImplemenationNote.PXA, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PXB => new ComplianceSubType(ComplianceSubTypeCodes.PXB, () => ComplianceSubTypeDescriptions.PXB, () => ComplianceSubTypeLocalDescriptions.PXB, () => ComplianceSubTypeInternalImplemenationNote.PXB, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PXC => new ComplianceSubType(ComplianceSubTypeCodes.PXC, () => ComplianceSubTypeDescriptions.PXC, () => ComplianceSubTypeLocalDescriptions.PXC, () => ComplianceSubTypeInternalImplemenationNote.PXC, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QCA => new ComplianceSubType(ComplianceSubTypeCodes.QCA, () => ComplianceSubTypeDescriptions.QCA, () => ComplianceSubTypeLocalDescriptions.QCA, () => ComplianceSubTypeInternalImplemenationNote.QCA, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType QCB => new ComplianceSubType(ComplianceSubTypeCodes.QCB, () => ComplianceSubTypeDescriptions.QCB, () => ComplianceSubTypeLocalDescriptions.QCB, () => ComplianceSubTypeInternalImplemenationNote.QCB, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType QCC => new ComplianceSubType(ComplianceSubTypeCodes.QCC, () => ComplianceSubTypeDescriptions.QCC, () => ComplianceSubTypeLocalDescriptions.QCC, () => ComplianceSubTypeInternalImplemenationNote.QCC, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType QCM => new ComplianceSubType(ComplianceSubTypeCodes.QCM, () => ComplianceSubTypeDescriptions.QCM, () => ComplianceSubTypeLocalDescriptions.QCM, () => ComplianceSubTypeInternalImplemenationNote.QCM, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType QCZ => new ComplianceSubType(ComplianceSubTypeCodes.QCZ, () => ComplianceSubTypeDescriptions.QCZ, () => ComplianceSubTypeLocalDescriptions.QCZ, () => ComplianceSubTypeInternalImplemenationNote.QCZ, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType QDA => new ComplianceSubType(ComplianceSubTypeCodes.QDA, () => ComplianceSubTypeDescriptions.QDA, () => ComplianceSubTypeLocalDescriptions.QDA, () => ComplianceSubTypeInternalImplemenationNote.QDA, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QDB => new ComplianceSubType(ComplianceSubTypeCodes.QDB, () => ComplianceSubTypeDescriptions.QDB, () => ComplianceSubTypeLocalDescriptions.QDB, () => ComplianceSubTypeInternalImplemenationNote.QDB, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QDC => new ComplianceSubType(ComplianceSubTypeCodes.QDC, () => ComplianceSubTypeDescriptions.QDC, () => ComplianceSubTypeLocalDescriptions.QDC, () => ComplianceSubTypeInternalImplemenationNote.QDC, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QDM => new ComplianceSubType(ComplianceSubTypeCodes.QDM, () => ComplianceSubTypeDescriptions.QDM, () => ComplianceSubTypeLocalDescriptions.QDM, () => ComplianceSubTypeInternalImplemenationNote.QDM, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QXA => new ComplianceSubType(ComplianceSubTypeCodes.QXA, () => ComplianceSubTypeDescriptions.QXA, () => ComplianceSubTypeLocalDescriptions.QXA, () => ComplianceSubTypeInternalImplemenationNote.QXA, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QXB => new ComplianceSubType(ComplianceSubTypeCodes.QXB, () => ComplianceSubTypeDescriptions.QXB, () => ComplianceSubTypeLocalDescriptions.QXB, () => ComplianceSubTypeInternalImplemenationNote.QXB, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QXC => new ComplianceSubType(ComplianceSubTypeCodes.QXC, () => ComplianceSubTypeDescriptions.QXC, () => ComplianceSubTypeLocalDescriptions.QXC, () => ComplianceSubTypeInternalImplemenationNote.QXC, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QXM => new ComplianceSubType(ComplianceSubTypeCodes.QXM, () => ComplianceSubTypeDescriptions.QXM, () => ComplianceSubTypeLocalDescriptions.QXM, () => ComplianceSubTypeInternalImplemenationNote.QXM, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType QXZ => new ComplianceSubType(ComplianceSubTypeCodes.QXZ, () => ComplianceSubTypeDescriptions.QXZ, () => ComplianceSubTypeLocalDescriptions.QXZ, () => ComplianceSubTypeInternalImplemenationNote.QXZ, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType SPA => new ComplianceSubType(ComplianceSubTypeCodes.SPA, () => ComplianceSubTypeDescriptions.SPA, () => ComplianceSubTypeLocalDescriptions.SPA, () => ComplianceSubTypeInternalImplemenationNote.SPA, LedgerOfUse.AP);
			public static ComplianceSubType SPB => new ComplianceSubType(ComplianceSubTypeCodes.SPB, () => ComplianceSubTypeDescriptions.SPB, () => ComplianceSubTypeLocalDescriptions.SPB, () => ComplianceSubTypeInternalImplemenationNote.SPB, LedgerOfUse.AP);
			public static ComplianceSubType TCA => new ComplianceSubType(ComplianceSubTypeCodes.TCA, () => ComplianceSubTypeDescriptions.TCA, () => ComplianceSubTypeLocalDescriptions.TCA, () => ComplianceSubTypeInternalImplemenationNote.TCA, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TCB => new ComplianceSubType(ComplianceSubTypeCodes.TCB, () => ComplianceSubTypeDescriptions.TCB, () => ComplianceSubTypeLocalDescriptions.TCB, () => ComplianceSubTypeInternalImplemenationNote.TCB, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TCC => new ComplianceSubType(ComplianceSubTypeCodes.TCC, () => ComplianceSubTypeDescriptions.TCC, () => ComplianceSubTypeLocalDescriptions.TCC, () => ComplianceSubTypeInternalImplemenationNote.TCC, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TCE => new ComplianceSubType(ComplianceSubTypeCodes.TCE, () => ComplianceSubTypeDescriptions.TCE, () => ComplianceSubTypeLocalDescriptions.TCE, () => ComplianceSubTypeInternalImplemenationNote.TCE, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TCM => new ComplianceSubType(ComplianceSubTypeCodes.TCM, () => ComplianceSubTypeDescriptions.TCM, () => ComplianceSubTypeLocalDescriptions.TCM, () => ComplianceSubTypeInternalImplemenationNote.TCM, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TDA => new ComplianceSubType(ComplianceSubTypeCodes.TDA, () => ComplianceSubTypeDescriptions.TDA, () => ComplianceSubTypeLocalDescriptions.TDA, () => ComplianceSubTypeInternalImplemenationNote.TDA, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TDB => new ComplianceSubType(ComplianceSubTypeCodes.TDB, () => ComplianceSubTypeDescriptions.TDB, () => ComplianceSubTypeLocalDescriptions.TDB, () => ComplianceSubTypeInternalImplemenationNote.TDB, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TDC => new ComplianceSubType(ComplianceSubTypeCodes.TDC, () => ComplianceSubTypeDescriptions.TDC, () => ComplianceSubTypeLocalDescriptions.TDC, () => ComplianceSubTypeInternalImplemenationNote.TDC, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TDE => new ComplianceSubType(ComplianceSubTypeCodes.TDE, () => ComplianceSubTypeDescriptions.TDE, () => ComplianceSubTypeLocalDescriptions.TDE, () => ComplianceSubTypeInternalImplemenationNote.TDE, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TDM => new ComplianceSubType(ComplianceSubTypeCodes.TDM, () => ComplianceSubTypeDescriptions.TDM, () => ComplianceSubTypeLocalDescriptions.TDM, () => ComplianceSubTypeInternalImplemenationNote.TDM, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXA => new ComplianceSubType(ComplianceSubTypeCodes.TXA, () => ComplianceSubTypeDescriptions.TXA, () => ComplianceSubTypeLocalDescriptions.TXA, () => ComplianceSubTypeInternalImplemenationNote.TXA, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXB => new ComplianceSubType(ComplianceSubTypeCodes.TXB, () => ComplianceSubTypeDescriptions.TXB, () => ComplianceSubTypeLocalDescriptions.TXB, () => ComplianceSubTypeInternalImplemenationNote.TXB, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXC => new ComplianceSubType(ComplianceSubTypeCodes.TXC, () => ComplianceSubTypeDescriptions.TXC, () => ComplianceSubTypeLocalDescriptions.TXC, () => ComplianceSubTypeInternalImplemenationNote.TXC, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXE => new ComplianceSubType(ComplianceSubTypeCodes.TXE, () => ComplianceSubTypeDescriptions.TXE, () => ComplianceSubTypeLocalDescriptions.TXE, () => ComplianceSubTypeInternalImplemenationNote.TXE, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXM => new ComplianceSubType(ComplianceSubTypeCodes.TXM, () => ComplianceSubTypeDescriptions.TXM, () => ComplianceSubTypeLocalDescriptions.TXM, () => ComplianceSubTypeInternalImplemenationNote.TXM, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TipoABE)
			{
				AddComplianceSubTypeAttributionRulesForTipoABE(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TipoABExcludedSupply)
			{
				AddComplianceSubTypeAttributionRulesForTipoABExcludedSupply(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TipoMB)
			{
				AddComplianceSubTypeAttributionRulesForTipoMB(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TipoMBExcludedSupply)
			{
				AddComplianceSubTypeAttributionRulesForTipoMBExcludedSupply(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TipoC)
			{
				AddComplianceSubTypeAttributionRulesForTipoC(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TipoCExcludedSupply)
			{
				AddComplianceSubTypeAttributionRulesForTipoCExcludedSupply(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForTipoABE(collection);
				AddComplianceSubTypeAttributionRulesForTipoABExcludedSupply(collection);
				AddComplianceSubTypeAttributionRulesForTipoMB(collection);
				AddComplianceSubTypeAttributionRulesForTipoMBExcludedSupply(collection);
				AddComplianceSubTypeAttributionRulesForTipoC(collection);
				AddComplianceSubTypeAttributionRulesForTipoCExcludedSupply(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("ArgentinaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForTipoABE(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ExcludedRegistrationCode = OrgCusCodes.MIP;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ExcludedRegistrationCode = OrgCusCodes.MIP;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.RequiredRegistrationCode = OrgCusCodes.MIP;
			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = ComplianceSubTypeCodes.TXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PDA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PCA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.RequiredRegistrationCode = OrgCusCodes.MIP;
			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = ComplianceSubTypeCodes.TXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PDB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABE;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PCB;
		}

		void AddComplianceSubTypeAttributionRulesForTipoABExcludedSupply(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ExcludedRegistrationCode = OrgCusCodes.MIP;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ExcludedRegistrationCode = OrgCusCodes.MIP;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.RequiredRegistrationCode = OrgCusCodes.MIP;
			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = ComplianceSubTypeCodes.TXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PDA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PCA;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.RequiredRegistrationCode = OrgCusCodes.MIP;
			configuration.ThresholdApplies = true;
			configuration.SubTypeThresholdNotMet = ComplianceSubTypeCodes.TXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PDB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PXB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.PDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.RuleSetCode = RuleSetCodes.TipoABExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoABExcludedSupply;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PCB;
		}

		void AddComplianceSubTypeAttributionRulesForTipoMB(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMB;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMB;
		}

		void AddComplianceSubTypeAttributionRulesForTipoMBExcludedSupply(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDM;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCM;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDB;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoMBExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoMBExcludedSupply;
		}

		void AddComplianceSubTypeAttributionRulesForTipoC(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoC;
		}

		void AddComplianceSubTypeAttributionRulesForTipoCExcludedSupply(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TDC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TDC;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.TCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.Argentina;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TipoCExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCExcludedSupply;
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TipoABE, RuleSetDescriptions.TipoABE);
			rulesetList.AddPair(RuleSetCodes.TipoABExcludedSupply, RuleSetDescriptions.TipoABExcludedSupply);
			rulesetList.AddPair(RuleSetCodes.TipoMB, RuleSetDescriptions.TipoMB);
			rulesetList.AddPair(RuleSetCodes.TipoMBExcludedSupply, RuleSetDescriptions.TipoMBExcludedSupply);
			rulesetList.AddPair(RuleSetCodes.TipoC, RuleSetDescriptions.TipoC);
			rulesetList.AddPair(RuleSetCodes.TipoCExcludedSupply, RuleSetDescriptions.TipoCExcludedSupply);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TipoABE;
		}

		public static class RuleSetCodes
		{
			public const string TipoABE = "1";
			public const string TipoABExcludedSupply = "2";
			public const string TipoMB = "3";
			public const string TipoMBExcludedSupply = "4";
			public const string TipoC = "5";
			public const string TipoCExcludedSupply = "6";
		}

		#region SuppressResourceStringsCheckRegion

		static class RuleSetDescriptions
		{
			public const string TipoABE = "Tipo A, B & E";
			public const string TipoABExcludedSupply = "Tipo A & B & Excluded Supply as XCL";
			public const string TipoMB = "Type M and B";
			public const string TipoMBExcludedSupply = "Type M, B & Excluded supply as XCL";
			public const string TipoC = "Type C";
			public const string TipoCExcludedSupply = "Type C & Excluded supply as XCL";
		}

		#endregion

		#endregion

		#region ComplianceSubTypeTaxRegistration

		bool IComplianceSubTypeTaxRegistrationTypeRuleProvider.IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header)
		{
			return (rule.TaxRegistrationType == TaxRegistrationTypeCodes.Recoverable && IsOrganizationTaxRecoverable(header))
					|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.NotRecoverable && !IsOrganizationTaxRecoverable(header));
		}

		bool IsOrganizationTaxRecoverable(OrgHeader header) => (header != null &&
				(header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.IVR, Constants.CountryCodes.Argentina) != null ||
				header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.IVI, Constants.CountryCodes.Argentina) != null ||
				header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.IVM, Constants.CountryCodes.Argentina) != null));

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => EnvProxy.Instance.IsProductionSystem ? new ZDate(2022, 10, 1) : new ZDate(2022, 8, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Argentina;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);
		}

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.TXA,
				ComplianceSubTypeCodes.TDA,
				ComplianceSubTypeCodes.TCA,
				ComplianceSubTypeCodes.TXB,
				ComplianceSubTypeCodes.TDB,
				ComplianceSubTypeCodes.TCB,
				ComplianceSubTypeCodes.TXC,
				ComplianceSubTypeCodes.TDC,
				ComplianceSubTypeCodes.TCC,
				ComplianceSubTypeCodes.TXE,
				ComplianceSubTypeCodes.TDE,
				ComplianceSubTypeCodes.PXA,
				ComplianceSubTypeCodes.PDA,
				ComplianceSubTypeCodes.PCA,
				ComplianceSubTypeCodes.PXB,
				ComplianceSubTypeCodes.PDB,
				ComplianceSubTypeCodes.TCE,
				ComplianceSubTypeCodes.PXC,
				ComplianceSubTypeCodes.PDC,
				ComplianceSubTypeCodes.PCC,
				ComplianceSubTypeCodes.TXM,
				ComplianceSubTypeCodes.TDM,
				ComplianceSubTypeCodes.TCM,
				ComplianceSubTypeCodes.PCB
			};
		}

		#endregion

		#region ITaxMessagesGroupProvider
		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N1, Description = TaxMessageGroupDescriptions.N1, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N1 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N2, Description = TaxMessageGroupDescriptions.N2, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N2 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N3, Description = TaxMessageGroupDescriptions.N3, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N3 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N4, Description = TaxMessageGroupDescriptions.N4, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N4 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N5, Description = TaxMessageGroupDescriptions.N5, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N5 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N6, Description = TaxMessageGroupDescriptions.N6, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N6 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N8, Description = TaxMessageGroupDescriptions.N8, Bool = false, RelatedItemCode = TaxMessageGroupCodes.N8 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N9, Description = TaxMessageGroupDescriptions.N9, Bool = false, RelatedItemCode = TaxMessageGroupCodes.N9 }
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string N1 = "1";
			public const string N2 = "2";
			public const string N3 = "3";
			public const string N4 = "4";
			public const string N5 = "5";
			public const string N6 = "6";
			public const string N8 = "8";
			public const string N9 = "9";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString N1 => (NoResString)"No Gravado";
			public static MultilingualString N2 => (NoResString)"Exento";
			public static MultilingualString N3 => (NoResString)"IVA 0%";
			public static MultilingualString N4 => (NoResString)"IVA 10,5%";
			public static MultilingualString N5 => (NoResString)"IVA 21%";
			public static MultilingualString N6 => (NoResString)"IVA 27%";
			public static MultilingualString N8 => (NoResString)"IVA 5%";
			public static MultilingualString N9 => (NoResString)"IVA 2,5%";
		}

		#endregion

		#endregion

		#region IBankAccountValidation

		void IBankAccountValidation.ValidateFullAccountNumber(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty && !Regex.IsMatch(propertyInfo.Value.ToString(), @"^[0-9]{22}$"))
			{
				propertyInfo.AddError(Res.GetString("5e9ba088-fc63-4ccb-a82e-8e43afcbf054", "Unique account number must be numeric and 22 characters long"));
			}
		}

		#endregion

		#region ComplianceSubType AFIP Equivalent Code

		string IEquivalentComplianceSubTypeProvider.GetEquivalentComplianceSubType(ZString complianceSubType)
		{
			return AFIPEquivalents.DocumentTypeEquivalentCodes.EquivalentCodes.ContainsKey(complianceSubType) ? AFIPEquivalents.DocumentTypeEquivalentCodes.EquivalentCodes[complianceSubType] : string.Empty;
		}

		#endregion

		#region QRCodeData

		string IQRCodeDataProvider.GetTransactionQRCodeString(ITransactionQRCodeDataProvider transactionData)
		{
			return (new ArgentinaQRCodeDataProvider() as IQRCodeDataProvider).GetTransactionQRCodeString(transactionData);
		}

		#endregion

		protected override string GetComplianceSequencePrefixErrorMessage() => Res.GetString("0B2B5DA9-552C-421E-8FE0-51137193B67A", @"Please enter a number between 1 and 99998");

		protected override string GetComplianceSequencePrefixRegex() => @"^(0{0,4}[1-9]|0{0,3}[1-9][0-9]|0{0,2}[1-9][0-9][0-9]|0{0,1}[1-9][0-9][0-9][0-9]|(?![9]{5})+([1-9][0-9][0-9][0-9][0-9]))$";

		#region IComplianceRegistryDefaultProvider

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate
				: AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue(proposedValue, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, CountryCode)
				: ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode, alternateErrorMessage: ErrorMessageForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(proposedValue));

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? string.Empty
				: ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode, alternateErrorMessage: ErrorMessageForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(proposedValue));

		static string ErrorMessageForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue)
			=> Res.GetString("0A528936-2CB4-4EC2-BD5D-13936C8612F0", "'{0}' is not valid for country/region '{1}' when registry '{2}' is not enabled.", proposedValue, Constants.CountryCodes.Argentina, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Caption);

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion
	}
}
