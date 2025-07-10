using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema
	{
		public const string CSI_CodeDescription = nameof(SupportingDocument.CSI_CodeDescription);
	}

	protected override CusSupportingInfoValidation GetNewValidation()
	{
		var declaration = Declaration;
		return declaration?.IsExport ?? false
			? new ExportSupportingDocumentValidation(this)
			: declaration?.IsImport ?? false
				? new ImportSupportingDocumentValidation(this)
				: new SupportingDocumentValidation(this);
	}

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

	protected override bool IsLineOnlyCore => true;

	protected override bool IsLineCore => true;

	[MaxLength(nameof(MaxLengthReferenceNumber))]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }
	int MaxLengthReferenceNumber => (Declaration?.IsImport ?? false) || UniversalValidationHelper.IsInAESTransitionPeriod ? 35 : 70;

	[MaxLength(nameof(MaxLengthReferenceNumber2))]
	[ResourceStringData("PLSupportingDocument|CSI_ReferenceNumber2", Caption = "Add. Identifier")]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }
	int MaxLengthReferenceNumber2 => (Declaration?.IsImport ?? false) ? 35 : 70;

	[MaxLength(35)]
	[ResourceStringData("PLSupportingDocument|CSI_Description", Caption = "Comments")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.UnitOfQuantityList))]
	public override ZString CSI_UnitOfQuantity
	{
		get => base.CSI_UnitOfQuantity;
		set => base.CSI_UnitOfQuantity = value;
	}

	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && oldValue != CSI_Code)
			{
				Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	[MaxLength(5)]
	[ResourceStringData("PLSupportingDocument|CSI_ItemNumber", Caption = "Line No.")]
	public override ZInt CSI_ItemNumber
	{
		get => base.CSI_ItemNumber;
		set => base.CSI_ItemNumber = value;
	}

	[MaxLength(70)]
	[ResourceStringData("PLSupportingDocument|CSI_AdditionalDescription", Caption = "Issuing Authority Name")]
	public override ZString CSI_AdditionalDescription
	{
		get => base.CSI_AdditionalDescription;
		set => base.CSI_AdditionalDescription = value;
	}

	[ResourceStringData("PLSupportingDocument|CSI_DateOfExpiry", Caption = "Validity date")]
	public override ZDateTime CSI_DateOfExpiry
	{
		get => base.CSI_DateOfExpiry;
		set => base.CSI_DateOfExpiry = value;
	}

	[DecimalPlaces(2), DecimalPrecision(16)]
	public override ZDecimal CSI_Value
	{
		get => base.CSI_Value;
		set => base.CSI_Value = value;
	}

	public ZBool IsParentInvoiceLine => Parent is JobComInvoiceLine;

	public override ZString KeyToDeterimeUniqueness => base.KeyToDeterimeUniqueness + MessageProviderHelper.ReturnNullIfEmpty(CSI_ItemNumber) + CSI_UnitOfQuantity + CSI_RX_NKCurrency + CSI_AdditionalDescription;
}
