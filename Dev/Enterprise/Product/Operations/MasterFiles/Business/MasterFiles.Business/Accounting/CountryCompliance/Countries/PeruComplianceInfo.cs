using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.PeruOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PeruComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Peru;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.IGV;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.CAE,
				ComplianceSubTypes.CMA,
				ComplianceSubTypes.DSB,
				ComplianceSubTypes.HON,
				ComplianceSubTypes.MQR,
				ComplianceSubTypes.NCD,
				ComplianceSubTypes.NCR,
				ComplianceSubTypes.NXI,
				ComplianceSubTypes.OTR,
				ComplianceSubTypes.SSP,
				ComplianceSubTypes.TBC,
				ComplianceSubTypes.TBO,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXI,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string CAE = "CAE";
			public const string CMA = "CMA";
			public const string DSB = "DSB";
			public const string HON = "HON";
			public const string MQR = "MQR";
			public const string NCD = "NCD";
			public const string NCR = "NCR";
			public const string NXI = "NXI";
			public const string OTR = "OTR";
			public const string SSP = "SSP";
			public const string TBC = "TBC";
			public const string TBO = "TBO";
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string TXI = "TXI";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString CAE { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|CAE", "AIR TRANSPORT INVOICE"); } }
			public static MultilingualString CMA { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|CMA", "RATED SEA BILL"); } }
			public static MultilingualString DSB { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|DSB", "DISBURSEMENT"); } }
			public static MultilingualString HON { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|HON", "PROFESSIONAL SERVICES INVOICE"); } }
			public static MultilingualString MQR { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|MQR", "CASH REGISTER RECEIPT"); } }
			public static MultilingualString NCD { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|NCD", "FOREIGN SUPPLIER DEBIT NOTE"); } }
			public static MultilingualString NCR { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|NCR", "FOREIGN SUPPLIER CREDIT NOTE"); } }
			public static MultilingualString NXI { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|NXI", "FOREIGN SUPPLIER INVOICE"); } }
			public static MultilingualString OTR { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|OTR", "OTHERS"); } }
			public static MultilingualString SSP { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|SSP", "UTILITY BILLS"); } }
			public static MultilingualString TBC { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|TBC", "CREDIT NOTE - NO INPUT TAX CREDIT"); } }
			public static MultilingualString TBO { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|TBO", "TAX INVOICE - NO INPUT TAX CREDIT"); } }
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|TCD", "TAX DEBIT NOTE"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|TCR", "TAX CREDIT NOTE"); } }
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("PEComplianceSubTypeCodeList|TXI", "TAX INVOICE"); } }
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string CAE = "CARTA DE PORTE AERO NACIONAL";
			public const string CMA = "CONOCIMIENTO DE EMBARQUE MARITIMA";
			public const string DSB = "DISBURSEMENT";
			public const string HON = "RECIBO POR HONORARIOS";
			public const string MQR = "TICKET O CINTA EMITIDO POR MAQUINA REGISTRADORA";
			public const string NCD = "NOTA DE DEBITO - NO DOMICILIADO";
			public const string NCR = "NOTA DE CREDITO - NO DOMICILIADO";
			public const string NXI = "COMPROBANTE - NO DOMICILIADO";
			public const string OTR = "OTROS";
			public const string SSP = "RECIBO POR SERVICIOS PUBLICOS";
			public const string TBC = "NOTA DE CRÉDITO DE BOLETA DE VENTA";
			public const string TBO = "BOLETA DE VENTA";
			public const string TCD = "NOTA DE DEBITO";
			public const string TCR = "NOTA DE CREDITO";
			public const string TXI = "FACTURA";
		}
		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string CAE = "Used in both Receivables and Payables to sub-classify Amending/reversing Invoice (INV) transactions.";
			public const string CMA = "PE Fiscal Document Code 6. Used in Payables when the Peru supplier has issued a Carta de Porte Aéreo por el Servicio de Transporte de Carga Aérea Invoice document.";
			public const string DSB = "PE Fiscal Document Code 21. Used in Payables when a Rated Sea Bill of Lading is used as the accounts payable “invoice” document.";
			public const string HON = "Not a PE Fiscal Document. Used in both Receivables and Payables to sub-classify Invoices and Credit Note transactions recorded for disbursement / reimbursement purposes.";
			public const string MQR = "PE Fiscal Document Code 12. Used in Payables when the Peru supplier has issued a Ticket o Cinta Emitido por Máquina Registradora document.";
			public const string NCD = "Not a PE Fiscal Document. Used in Payables when recording Invoices and Credit Note transactions received from Foreign (not Peru) suppliers.";
			public const string NCR = "Not a PE Fiscal Document. Used in Payables when recording Invoices and Credit Note transactions received from Foreign (not Peru) suppliers.";
			public const string NXI = "Not a PE Fiscal Document. Used in Payables when recording Invoices and Credit Note transactions received from Foreign (not Peru) suppliers.";
			public const string OTR = "No PE Fiscal Document Code. Used in Payables to record expenses when the Peru supplier has issued an ‘invoice’ that does not fall into other compliance sub types defined by the law.";
			public const string SSP = "PE Fiscal Document Code 14. Used in Payables to record utility bills such as electricity, telecommunication services or running water bills.";
			public const string TBC = "PE Fiscal Document Code 7. Used in Receivables and Payables to sub-classify amending credit notes that amend a Boleta de Venta (TBO).  This Document type does not give right to Input Tax Credit.";
			public const string TBO = "PE Fiscal Document Code 3. Used in both Receivables and Payables to sub-classify Invoices when the recipient does not have a PE RUC tax number. The Boleta de venta can contain tax but does not give right to Input Tax Credit.";
			public const string TCD = "PE Fiscal Document Code 8. Used in both Receivables and Payables to sub-classify Amending/reversing Invoice transactions.";
			public const string TCR = "PE Fiscal Document Code 7. Used in both Receivables and Payables to sub-classify Credit Note transactions.";
			public const string TXI = "PE Fiscal Document Code 1. Used in both Receivables and Payables to sub-classify original Invoice transactions.";
		}
		#endregion
		static class ComplianceSubTypes
		{
			public static ComplianceSubType CAE => new ComplianceSubType(ComplianceSubTypeCodes.CAE, () => ComplianceSubTypeDescriptions.CAE, () => ComplianceSubTypeLocalDescriptions.CAE, () => ComplianceSubTypeInternalImplemenationNote.CAE);
			public static ComplianceSubType CMA => new ComplianceSubType(ComplianceSubTypeCodes.CMA, () => ComplianceSubTypeDescriptions.CMA, () => ComplianceSubTypeLocalDescriptions.CMA, () => ComplianceSubTypeInternalImplemenationNote.CMA);
			public static ComplianceSubType DSB => new ComplianceSubType(ComplianceSubTypeCodes.DSB, () => ComplianceSubTypeDescriptions.DSB, () => ComplianceSubTypeLocalDescriptions.DSB, () => ComplianceSubTypeInternalImplemenationNote.DSB);
			public static ComplianceSubType HON => new ComplianceSubType(ComplianceSubTypeCodes.HON, () => ComplianceSubTypeDescriptions.HON, () => ComplianceSubTypeLocalDescriptions.HON, () => ComplianceSubTypeInternalImplemenationNote.HON);
			public static ComplianceSubType MQR => new ComplianceSubType(ComplianceSubTypeCodes.MQR, () => ComplianceSubTypeDescriptions.MQR, () => ComplianceSubTypeLocalDescriptions.MQR, () => ComplianceSubTypeInternalImplemenationNote.MQR);
			public static ComplianceSubType NCD => new ComplianceSubType(ComplianceSubTypeCodes.NCD, () => ComplianceSubTypeDescriptions.NCD, () => ComplianceSubTypeLocalDescriptions.NCD, () => ComplianceSubTypeInternalImplemenationNote.NCD);
			public static ComplianceSubType NCR => new ComplianceSubType(ComplianceSubTypeCodes.NCR, () => ComplianceSubTypeDescriptions.NCR, () => ComplianceSubTypeLocalDescriptions.NCR, () => ComplianceSubTypeInternalImplemenationNote.NCR);
			public static ComplianceSubType NXI => new ComplianceSubType(ComplianceSubTypeCodes.NXI, () => ComplianceSubTypeDescriptions.NXI, () => ComplianceSubTypeLocalDescriptions.NXI, () => ComplianceSubTypeInternalImplemenationNote.NXI);
			public static ComplianceSubType OTR => new ComplianceSubType(ComplianceSubTypeCodes.OTR, () => ComplianceSubTypeDescriptions.OTR, () => ComplianceSubTypeLocalDescriptions.OTR, () => ComplianceSubTypeInternalImplemenationNote.OTR);
			public static ComplianceSubType SSP => new ComplianceSubType(ComplianceSubTypeCodes.SSP, () => ComplianceSubTypeDescriptions.SSP, () => ComplianceSubTypeLocalDescriptions.SSP, () => ComplianceSubTypeInternalImplemenationNote.SSP);
			public static ComplianceSubType TBC => new ComplianceSubType(ComplianceSubTypeCodes.TBC, () => ComplianceSubTypeDescriptions.TBC, () => ComplianceSubTypeLocalDescriptions.TBC, () => ComplianceSubTypeInternalImplemenationNote.TBC);
			public static ComplianceSubType TBO => new ComplianceSubType(ComplianceSubTypeCodes.TBO, () => ComplianceSubTypeDescriptions.TBO, () => ComplianceSubTypeLocalDescriptions.TBO, () => ComplianceSubTypeInternalImplemenationNote.TBO);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
		}

		#endregion
	}
}
