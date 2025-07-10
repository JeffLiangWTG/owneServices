using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

[CodeProperty(Schema.CEI_SubStyle), DescriptionProperty(Schema.DescriptionForDisplay)]
public class CusEntryInstruction : AutoCusEntryInstruction
	, Integration.Customs.PL.ICusEntryInstruction
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public IEnumerable<SupportingDocument> AllInstructionSupportingDocuments => JobDeclaration.SupportingDocuments.Cast<SupportingDocument>()
		.Concat(Invoices.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()))
		.Concat(InvoiceLines.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()));

	public IEnumerable<AdditionalInfo> AllInstructionAdditionalInfos => JobDeclaration.AdditionalInfos.Cast<AdditionalInfo>()
		.Concat(Invoices.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()))
		.Concat(InvoiceLines.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()));

	public new IEnumerable<JobComInvoiceLine> InvoiceLines => base.InvoiceLines.Cast<JobComInvoiceLine>();

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new class Schema : EU.Business.Declaration.CusEntryInstruction.Schema
	{
		public const string DescriptionForDisplay = nameof(CusEntryInstruction.DescriptionForDisplay);
	}

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> CusAuthorizationUsages => (EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>)base.CusAuthorizationUsages;

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
	{
		if (JobDeclaration?.IsExport ?? false)
		{
			return new ExportCusEntryInstructionValidation(this);
		}
		else if (JobDeclaration?.IsImport ?? false)
		{
			return new ImportCusEntryInstructionValidation(this);
		}

		return new CusEntryInstructionValidation(this);
	}

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

	protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(this);

	protected new AddInfoCusEntryInstruction AddInfo => (AddInfoCusEntryInstruction)base.AddInfo;

	public new AddInfoCusEntryInstructionValidation AddInfoValidation => AddInfo.Validation;

	public new AddInfoCusEntryInstructionLookups AddInfoLookups => AddInfo.Lookups;

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	public new GuaranteeBondDetailCollection Guarantees => (GuaranteeBondDetailCollection)base.Guarantees;

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override GuaranteeForEntryInstructionCollection GetNewGuaranteeForEntryInstructionCollection() => new GuaranteeBondDetailCollection(this);

	public new ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences => (ICusFiscalReferenceCollection<CusFiscalReference>)base.FiscalReferences;

	protected override ICusFiscalReferenceCollection<EU.Business.Declaration.CusFiscalReference> GetNewFiscalReferenceCollection() => new CusFiscalReferenceCollection<CusFiscalReference>(this);
	protected override Type FiscalReferenceType => typeof(CusFiscalReference);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		ZG_PostExportTransit = false;
		ZG_ExportManifest = false;
		ZG_EADPrintOut = EadPrintOutList.Codes._0;
		CEI_DateForDuty = ZDateTime.Today;
	}

	public ZString DescriptionForDisplay => $"CPC {CEI_Procedure} - {CEI_Description}";

	[ResourceStringData("F1EC686D-D414-4F51-85BD-8B5300C233B6", Caption = "Declaration Date")]
	public override ZDateTime CEI_DateForDuty
	{
		get => base.CEI_DateForDuty;
		set
		{
			var oldValue = CEI_DateForDuty;
			base.CEI_DateForDuty = value;
			if (!IsCopying && oldValue != CEI_DateForDuty)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.MarkAsNeedingValidation());
			}
		}
	}

	[ResourceStringData("0FDE3DCF-44F8-4854-8E39-02A2B0A8CF2C", Caption = "CPC")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CPCList))]
	[MaxLength(2)]
	public override ZString CEI_Procedure
	{
		get => base.CEI_Procedure;
		set
		{
			var oldValue = CEI_Procedure;
			base.CEI_Procedure = value;
			if (!IsCopying && oldValue != CEI_Procedure)
			{
				JobDeclaration?.MarkAsNeedingValidation();
				InvoiceLines.ForEach(line => line.MarkAsNeedingValidation());
			}
		}
	}

	public bool IsExitSummary => JobDeclaration?.IsExitSummary ?? false;

	#region AddInfo

	#region ZG_EADPrintOut

	[ResourceStringData("C8C55419-3DC2-4AFA-95F5-AB08D44C6914", Caption = "EAD Print Out")]
	public override ZString ZG_EADPrintOut { get => AddInfo.ZG_EADPrintOut; set => AddInfo.ZG_EADPrintOut = value; }

	#endregion

	#region Temporary Location

	[ResourceStringData("C21B6435-9F27-447E-A01D-6DD3A1D544AD", Caption = "Temporary location")]
	public override ZString ZG_TemporaryLocationCodeType { get => AddInfo.ZG_TemporaryLocationCodeType; set => AddInfo.ZG_TemporaryLocationCodeType = value; }

	public override ZString ZG_TemporaryLocation { get => AddInfo.ZG_TemporaryLocation; set => AddInfo.ZG_TemporaryLocation = value; }
	#endregion

	#region ZG_OfficeOfExitArrivalTimeLimit

	[ResourceStringData("192FBFA3-E6B1-4E5B-B53E-DC3C23B22681", Caption = "Office Of Exit Arrival Time Limit")]
	public override ZDateTime ZG_OfficeOfExitArrivalTimeLimit { get => AddInfo.ZG_OfficeOfExitArrivalTimeLimit; set => AddInfo.ZG_OfficeOfExitArrivalTimeLimit = value; }

	#endregion

	#region ZG_PostExportTransit

	[ResourceStringData("6E7F34E4-546D-4871-A1A3-042B06E590F9", Caption = "Post Export Transit")]
	public override ZBool ZG_PostExportTransit { get => AddInfo.ZG_PostExportTransit; set => AddInfo.ZG_PostExportTransit = value; }

	#endregion

	#region ZG_ExportManifest

	[ResourceStringData("20CA7F66-B49A-461D-8933-4A557846089D", Caption = "Export Manifest")]
	public override ZBool ZG_ExportManifest { get => AddInfo.ZG_ExportManifest; set => AddInfo.ZG_ExportManifest = value; }

	#endregion

	#region ZG_SealsCount

	[ResourceStringData("547681C2-85FD-4EA3-B5CB-B11EE5BA261F", Caption = "Total Seals Amount")]
	public override ZInt ZG_SealsCount { get => AddInfo.ZG_SealsCount; set => AddInfo.ZG_SealsCount = value; }

	#endregion

	#endregion

	public bool HasInvoiceLineStartWithProcedureCodeValue(ZString value)
	{
		return !value.IsEmpty && InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.JI_Procedure.StartsWith(value));
	}

	public bool HasGuarantees()
	{
		return Guarantees.Any();
	}

	public bool HasOnlyOneGuarantee()
	{
		return Guarantees.Count == 1;
	}

	public void SetGuaranteeAmountIfNeeded()
	{
		if (HasOnlyOneGuarantee())
		{
			var totalAmountPayable = EntryHeader?.TotalAmountPayable ?? 0;
			foreach (GuaranteeBondDetail bondData in Guarantees)
			{
				bondData.PW_BondAmount = totalAmountPayable;
			}
		}
	}

	public ZBool IsExportManifest => (JobDeclaration?.IsExportGoodsLocatedAtOfficeOfExit ?? false) && ZG_ExportManifest;

	bool EntryInstructionAlreadyMergedOrSent
	{
		get
		{
			var entryHeader = EntryHeader;
			return entryHeader != null && (entryHeader.HasBeenLodgedAtCustoms || entryHeader.IsWaitingForResponse);
		}
	}

	public bool DateForDutyIsObsolete => !EntryInstructionAlreadyMergedOrSent && ((CEI_DateForDuty < ZDateTime.Today) || CEI_DateForDuty.IsEmpty);

	public bool IsIE515BMessage => CEI_SubStyle == EntrySubStyleList.Codes.IncompleteDeclaration || CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;

	public bool UniqueCurrencyUsed => Factory.GetValue(ref uniqueCurrencyUsed, () => Invoices.Select(inv => inv.JZ_RX_NKInvoice_Currency).Distinct().Count() == 1);
	CachedProperty<bool> uniqueCurrencyUsed;

	internal bool AnyInvoiceLineValuationDateIsNotEmpty => Factory.GetValue(ref anyInvoiceLineValuationDateIsNotEmpty, () => InvoiceLines.Any(x => !x.JI_ValuationDateOverride.IsEmpty));
	CachedProperty<bool> anyInvoiceLineValuationDateIsNotEmpty;

	public bool IsCentralizedCustomsDeclaration => Factory.GetValue(ref isCentralizedCustomsDeclaration,
		() => CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.CustomsCode == CusAuthorizationUsageType.C513));
	CachedProperty<bool> isCentralizedCustomsDeclaration;

	internal bool HasAuthorisationUsageCode(ZString authorisationAgcCode) => CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.CustomsCode == authorisationAgcCode);

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var result = base.GetCusSupportingInfoTypesCore();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override bool IsSimplifiedEntryInstructionCore => (JobDeclaration?.IsExport ?? false)
		&& (string)CEI_SubStyle
		is SubStyleCodes.B
		or SubStyleCodes.E
		or SubStyleCodes.C
		or SubStyleCodes.F;

	public bool IsSupplementary => (string)CEI_SubStyle
		is SubStyleCodes.X
		or SubStyleCodes.Y;
}
