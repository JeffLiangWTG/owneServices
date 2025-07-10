using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using static Enterprise.Customs.PL.Business.Constants;
using AdditionalInfo = Enterprise.Customs.PL.Business.Declaration.AdditionalInfo;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;
using JobComInvoiceHeader = Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader;
using JobComInvoiceLine = Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.PL.Business.Declaration.JobDeclaration;
using PreviousDocument = Enterprise.Customs.PL.Business.Declaration.PreviousDocument;
using SupportingDocument = Enterprise.Customs.PL.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.PL.Business;

public static class Utils
{
	public static ZBool IsSimplifiedDeclaration(this CusEntryInstruction entryInstruction) => Constants.EntryInstructionSubStyle.SimplifiedDeclarationSubStyleList(entryInstruction.Factory).Contains(entryInstruction.CEI_SubStyle);

	public static ZBool IsNormalDeclaration(this CusEntryInstruction entryInstruction) => Constants.EntryInstructionSubStyle.NormalDeclarationSubStyleList(entryInstruction.Factory).Contains(entryInstruction.CEI_SubStyle);

	public static ZBool IsUnderSimplifiedProcedure(this CusEntryInstruction entryInstruction) => entryInstruction.CEI_SubStyle == SubStyleCodes.Z;

	internal static bool HasAuthorizationUsageCode(this CusSupportingInfo parent, ZString agcCode)
		=> parent?.Parent switch
		{
			JobComInvoiceLine invoiceLine => HasAuthorizationUsageCode(invoiceLine.EntryInstruction, agcCode),
			JobComInvoiceHeader header => HasAuthorizationUsageCode(header.CusEntryInstructions.Cast<CusEntryInstruction>(), agcCode),
			CusEntryInstruction entryInstruction => HasAuthorizationUsageCode(entryInstruction, agcCode),
			JobDeclaration declaration => HasAuthorizationUsageCode(declaration.CustomsEntryInstructions, agcCode),
			_ => false
		};

	static bool HasAuthorizationUsageCode(IEnumerable<CusEntryInstruction> entryInstructions, ZString agcCode) => entryInstructions != null
																												&& entryInstructions.Any(x => HasAuthorizationUsageCode(x, agcCode));

	static bool HasAuthorizationUsageCode(CusEntryInstruction instruction, ZString agcCode) => instruction is CusEntryInstruction
																								&& instruction.HasAuthorisationUsageCode(agcCode);

	internal static bool HasProceduresStartingWith(this CusSupportingInfo parent, IReadOnlyCollection<ZString> procedures)
		=> parent?.Parent switch
		{
			JobComInvoiceLine invoiceLine => HasProceduresStartingWith(invoiceLine.JI_Procedure, procedures),
			JobComInvoiceHeader header => header.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => HasProceduresStartingWith(invoiceLine.JI_Procedure, procedures)),
			CusEntryInstruction entryInstruction => entryInstruction.InvoiceLines.Any(invoiceLine => HasProceduresStartingWith(invoiceLine.JI_Procedure, procedures)),
			JobDeclaration declaration => declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => HasProceduresStartingWith(invoiceLine.JI_Procedure, procedures)),
			_ => false
		};

	static bool HasProceduresStartingWith(ZString procedure, IReadOnlyCollection<ZString> procedures) => procedures.Any(x => procedure.StartsWith(x));

	internal static bool HasOfficeOfPresentationPL(this CusSupportingInfo parent)
		=> parent?.Parent switch
		{
			JobComInvoiceLine invoiceLine => invoiceLine.Declaration?.HasOfficeOfPresentationPL ?? false,
			JobComInvoiceHeader header => header.JobDeclaration?.HasOfficeOfPresentationPL ?? false,
			CusEntryInstruction entryInstruction => entryInstruction.JobDeclaration?.HasOfficeOfPresentationPL ?? false,
			JobDeclaration declaration => declaration.HasOfficeOfPresentationPL,
			_ => false
		};

	internal static bool HasPreviousDocumentCodes(this IPreviousDocumentsProvider provider, IReadOnlyCollection<ZString> previousDocumentCodes) =>
		provider?.PreviousDocuments.Cast<PreviousDocument>().Any(x => previousDocumentCodes.Contains(x.CSI_Code)) ?? false;

	internal static bool HasPreviousDocumentCode(this IPreviousDocumentsProvider provider, string previousDocumentCodes) =>
		provider?.PreviousDocuments.Cast<PreviousDocument>().Any(x => previousDocumentCodes == x.CSI_Code) ?? false;

	internal static bool HasAdditionalInformationCode(this AdditionalInfo additionalInfo, ZString csiType, ZString csiCode) =>
		additionalInfo != null && additionalInfo.CSI_SubType == csiType && additionalInfo.CSI_Code == csiCode;

	internal static bool HasAdditionalInformationCode(this IAdditionalInfosProvider provider, ZString csiType, ZString csiCode) =>
		provider?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => HasAdditionalInformationCode(x, csiType, csiCode)) ?? false;

	internal static bool HasSupportingDocumentCode(this ISupportingDocumentsProvider provider, IReadOnlyCollection<ZString> supportingDocumentCodes) =>
		provider?.SupportingDocuments.Cast<SupportingDocument>().Any(x => supportingDocumentCodes.Contains(x.CSI_Code)) ?? false;

	internal static bool HasSupportingDocumentCode(this ISupportingDocumentsProvider provider, ZString supportingDocumentCode) =>
		provider?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == supportingDocumentCode) ?? false;

	internal static bool HasSupportingDocumentCodeWithReferenceNumber(this ISupportingDocumentsProvider provider, ZString supportingDocumentCode) =>
		provider?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == supportingDocumentCode && !x.CSI_ReferenceNumber.IsEmpty) ?? false;

	public static bool HasSupportingDocumentCodes(this CusSupportingInfo parentInfo, IReadOnlyCollection<ZString> supportingDocumentCsiCodes)
		=> parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine =>
				invoiceLine.HasSupportingDocumentCode(supportingDocumentCsiCodes)
				|| invoiceLine.Declaration.HasSupportingDocumentCode(supportingDocumentCsiCodes)
				|| invoiceLine.EntryInstruction.HasSupportingDocumentCode(supportingDocumentCsiCodes)
				|| invoiceLine.InvoiceHeader.HasSupportingDocumentCode(supportingDocumentCsiCodes),

			JobComInvoiceHeader invoice =>
				invoice.HasSupportingDocumentCode(supportingDocumentCsiCodes)
				|| invoice.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes))
				|| invoice.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes))
				|| invoice.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCodes),

			CusEntryInstruction entryInstruction =>
				entryInstruction.HasSupportingDocumentCode(supportingDocumentCsiCodes)
				|| entryInstruction.Invoices.Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes))
				|| entryInstruction.InvoiceLines.Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes))
				|| entryInstruction.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCodes),

			JobDeclaration declaration =>
				declaration.HasSupportingDocumentCode(supportingDocumentCsiCodes)
				|| declaration.CustomsEntryInstructions.Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes))
				|| declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes))
				|| declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCodes)),

			_ => false
		};

	public static bool HasSupportingDocumentCode(this CusSupportingInfo parentInfo, ZString supportingDocumentCsiCode)
		=> parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine =>
				invoiceLine.HasSupportingDocumentCode(supportingDocumentCsiCode)
				|| invoiceLine.EntryInstruction.HasSupportingDocumentCode(supportingDocumentCsiCode)
				|| invoiceLine.InvoiceHeader.HasSupportingDocumentCode(supportingDocumentCsiCode)
				|| invoiceLine.Declaration.HasSupportingDocumentCode(supportingDocumentCsiCode),

			JobComInvoiceHeader invoice =>
				invoice.HasSupportingDocumentCode(supportingDocumentCsiCode)
				|| invoice.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode))
				|| invoice.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode))
				|| invoice.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCode),

			CusEntryInstruction entryInstruction =>
				entryInstruction.HasSupportingDocumentCode(supportingDocumentCsiCode)
				|| entryInstruction.Invoices.Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode))
				|| entryInstruction.InvoiceLines.Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode))
				|| entryInstruction.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCode),

			JobDeclaration declaration =>
				declaration.HasSupportingDocumentCode(supportingDocumentCsiCode)
				|| declaration.CustomsEntryInstructions.Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode))
				|| declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode))
				|| declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCsiCode)),

			_ => false
		};

	public static bool HasSupportingDocumentCodeWithReferenceNumber(this CusSupportingInfo parentInfo, ZString supportingDocumentCsiCode)
		=> parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine =>
				invoiceLine.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)
				|| invoiceLine.EntryInstruction.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)
				|| invoiceLine.InvoiceHeader.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)
				|| invoiceLine.Declaration.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode),

			JobComInvoiceHeader invoice =>
				invoice.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)
				|| invoice.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode))
				|| invoice.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode))
				|| invoice.JobDeclaration.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode),

			CusEntryInstruction entryInstruction =>
				entryInstruction.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)
				|| entryInstruction.Invoices.Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode))
				|| entryInstruction.InvoiceLines.Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode))
				|| entryInstruction.JobDeclaration.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode),

			JobDeclaration declaration =>
				declaration.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)
				|| declaration.CustomsEntryInstructions.Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode))
				|| declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode))
				|| declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasSupportingDocumentCodeWithReferenceNumber(supportingDocumentCsiCode)),

			_ => false
		};

	static bool HasPreviousProcedureCode(JobComInvoiceLine invoiceLine, IReadOnlyCollection<ZString> previousProcedureCodes) =>
		invoiceLine is not null
		&& previousProcedureCodes.Contains(invoiceLine.PreviousProcedureCode);

	internal static bool HasPreviousProcedureCode(this CusSupportingInfo parentInfo, IReadOnlyCollection<ZString> previousProcedureCodes) =>
		parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine => HasPreviousProcedureCode(invoiceLine, previousProcedureCodes),
			JobComInvoiceHeader header => header.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => HasPreviousProcedureCode(invoiceLine, previousProcedureCodes)),
			CusEntryInstruction entryInstruction => entryInstruction.InvoiceLines.Any(invoiceLine => HasPreviousProcedureCode(invoiceLine, previousProcedureCodes)),
			JobDeclaration declaration => declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => HasPreviousProcedureCode(invoiceLine, previousProcedureCodes)),
			_ => false
		};

	internal static bool HasEntryInstructionProcedureCode(this CusEntryInstruction entryInstruction, ZString procedureCode) =>
		entryInstruction is not null && entryInstruction.CEI_Procedure == procedureCode;

	internal static bool HasEntryInstructionProcedureCode(this CusSupportingInfo parentInfo, ZString procedureCode) =>
		parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine => HasEntryInstructionProcedureCode(invoiceLine.EntryInstruction, procedureCode),
			JobComInvoiceHeader header => header.CusEntryInstructions.Cast<CusEntryInstruction>().Any(invoiceHeaderInstruction => HasEntryInstructionProcedureCode(invoiceHeaderInstruction, procedureCode)),
			CusEntryInstruction entryInstruction => HasEntryInstructionProcedureCode(entryInstruction, procedureCode),
			JobDeclaration declaration => declaration.CustomsEntryInstructions.Any(declarationInstruction => HasEntryInstructionProcedureCode(declarationInstruction, procedureCode)),
			_ => false
		};

	internal static bool HasAdditionalInfoCode(this IAdditionalInfosProvider provider, ZString additionalInfoCode) =>
		provider?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == additionalInfoCode) ?? false;

	internal static bool HasAdditionalInfoCode(this IAdditionalInfosProvider provider, ZString documentType, ZString documentCode) =>
		provider?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == documentCode && x.CSI_SubType == documentType) ?? false;

	internal static bool HasAdditionalInfoCodeWithSameDescription(this IAdditionalInfosProvider provider, ZString documentType, ZString documentCode, ZString description) =>
		provider?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == documentCode && x.CSI_SubType == documentType && x.CSI_Description == description) ?? false;

	public static bool HasAdditionalDocumentCode(this CusSupportingInfo parentInfo, ZString documentType, ZString documentCode)
		=> parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine =>
				invoiceLine.HasAdditionalInfoCode(documentType, documentCode)
				|| invoiceLine.EntryInstruction.HasAdditionalInfoCode(documentType, documentCode)
				|| invoiceLine.InvoiceHeader.HasAdditionalInfoCode(documentType, documentCode)
				|| invoiceLine.Declaration.HasAdditionalInfoCode(documentType, documentCode),

			JobComInvoiceHeader invoice =>
				invoice.HasAdditionalInfoCode(documentType, documentCode)
				|| invoice.CusEntryInstructions.Cast<CusEntryInstruction>()
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode))
				|| invoice.InvoiceLines.Cast<JobComInvoiceLine>()
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode))
				|| invoice.JobDeclaration.HasAdditionalInfoCode(documentType, documentCode),

			CusEntryInstruction entryInstruction =>
				entryInstruction.HasAdditionalInfoCode(documentType, documentCode)
				|| entryInstruction.Invoices
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode))
				|| entryInstruction.InvoiceLines
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode))
				|| entryInstruction.JobDeclaration.HasAdditionalInfoCode(documentType, documentCode),

			JobDeclaration declaration =>
				declaration.HasAdditionalInfoCode(documentType, documentCode)
				|| declaration.CustomsEntryInstructions
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode))
				|| declaration.Invoices.Cast<JobComInvoiceHeader>()
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode))
				|| declaration.InvoiceLines.Cast<JobComInvoiceLine>()
					.Any(x => x.HasAdditionalInfoCode(documentType, documentCode)),

			_ => false
		};

	public static bool HasAdditionalDocumentCode(this CusSupportingInfo parentInfo, ZString documentCode) =>
		parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine =>
				invoiceLine.HasAdditionalInfoCode(documentCode)
				|| invoiceLine.EntryInstruction.HasAdditionalInfoCode(documentCode)
				|| invoiceLine.InvoiceHeader.HasAdditionalInfoCode(documentCode)
				|| invoiceLine.Declaration.HasAdditionalInfoCode(documentCode),

			JobComInvoiceHeader invoice =>
				invoice.HasAdditionalInfoCode(documentCode)
				|| invoice.CusEntryInstructions.Cast<CusEntryInstruction>()
					.Any(x => x.HasAdditionalInfoCode(documentCode))
				|| invoice.InvoiceLines.Cast<JobComInvoiceLine>()
					.Any(x => x.HasAdditionalInfoCode(documentCode))
				|| invoice.JobDeclaration.HasAdditionalInfoCode(documentCode),

			CusEntryInstruction entryInstruction =>
				entryInstruction.HasAdditionalInfoCode(documentCode)
				|| entryInstruction.Invoices.Any(x => x.HasAdditionalInfoCode(documentCode))
				|| entryInstruction.InvoiceLines.Any(x => x.HasAdditionalInfoCode(documentCode))
				|| entryInstruction.JobDeclaration.HasAdditionalInfoCode(documentCode),

			JobDeclaration declaration =>
				declaration.HasAdditionalInfoCode(documentCode)
				|| declaration.CustomsEntryInstructions
					.Any(x => x.HasAdditionalInfoCode(documentCode))
				|| declaration.Invoices.Cast<JobComInvoiceHeader>()
					.Any(x => x.HasAdditionalInfoCode(documentCode))
				|| declaration.InvoiceLines.Cast<JobComInvoiceLine>()
					.Any(x => x.HasAdditionalInfoCode(documentCode)),
			_ => false
		};

	internal static bool HasEntryInstructionProcedureCode(this CusEntryInstruction entryInstruction, IReadOnlyCollection<ZString> procedureCodes) =>
		entryInstruction != null && procedureCodes.Contains(entryInstruction.CEI_Procedure);

	internal static bool HasEntryInstructionProcedureCode(this CusSupportingInfo parentInfo, IReadOnlyCollection<ZString> procedureCodes) =>
		parentInfo?.Parent switch
		{
			JobComInvoiceLine invoiceLine => HasEntryInstructionProcedureCode(invoiceLine.EntryInstruction, procedureCodes),
			JobComInvoiceHeader header => header.CusEntryInstructions.Cast<CusEntryInstruction>().Any(invoiceHeaderInstruction => HasEntryInstructionProcedureCode(invoiceHeaderInstruction, procedureCodes)),
			CusEntryInstruction entryInstruction => HasEntryInstructionProcedureCode(entryInstruction, procedureCodes),
			JobDeclaration declaration => declaration.CustomsEntryInstructions.Any(declarationInstruction => HasEntryInstructionProcedureCode(declarationInstruction, procedureCodes)),
			_ => false
		};

	internal static void RuleR0091EForSCOPurpose(JobDeclaration declaration, BusinessObject rowBO)
	{
		var additionalDocuments = declaration?.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()) ?? Array.Empty<AdditionalInfo>();
		if (additionalDocuments.Any(x => x.CSI_Code == Constants.AdditionalInfoCodes._00100 && x.CSI_SubType == Constants.AdditionalInfoTypes.INF))
		{
			var customsOffices = declaration?.CustomsOfficeCollection;
			if (!customsOffices.Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice))
			{
				rowBO.AddRowMessageError(Res.GetString("F250496D-C804-4AFD-A740-65233B13C630", "[R0091E] Supervising Customs Office ’SCO’ is required with Additional Information code '00100'."));
			}
			else if (customsOffices.Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice && x.CY_Data != declaration.JE_CustomsOffice))
			{
				rowBO.AddRowMessageError(Res.GetString("08E6C368-5C6D-482A-B927-A0AFA24E8BA4", "[R0091E] Supervising Customs Office ’SCO’ must be equal Customs Office of Export with Additional Information code '00100'."));
			}
		}
	}
}
