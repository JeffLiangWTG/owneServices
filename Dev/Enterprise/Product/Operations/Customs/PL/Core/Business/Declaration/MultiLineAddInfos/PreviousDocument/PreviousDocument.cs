using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.PL.Business.Declaration;

[SystemDefinedValues]
public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
	{
		public new const int CSI_ReferenceNumber2MaxLength = 5;
	}

	public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

	protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() =>
		IsExport
			? new ExportPreviousDocumentValidation(this)
			: new PreviousDocumentValidation(this);

	public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;
	protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

	public ZBool IsExport => (Parent as ICanBeImportOrExport)?.IsExport ?? false;

	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var cSI_Code = CSI_Code;
			base.CSI_Code = value;
			if (cSI_Code != CSI_Code && !IsCopying && !IsValidationSuspended)
			{
				Validation.ValidateCSI_LineNo();
			}
		}
	}

	[ResourceStringData("PLPreviousDocument|CSI_Quantity", Caption = "Goods Quantity")]
	public override ZDecimal CSI_Quantity
	{
		get => base.CSI_Quantity;
		set => base.CSI_Quantity = value;
	}

	[ResourceStringData("PLPreviousDocument|CSI_PackQty", Caption = "Packs Quantity")]
	public override ZInt CSI_PackQty
	{
		get => base.CSI_PackQty;
		set => base.CSI_PackQty = value;
	}

	[ResourceStringData("PLPreviousDocument|CSI_PackType", Caption = "Pack Type Code")]
	public override ZString CSI_PackType
	{
		get => base.CSI_PackType;
		set => base.CSI_PackType = value;
	}

	[MaxLength(nameof(MaxLengthReferenceNumber))]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}
	int MaxLengthReferenceNumber => (Declaration?.IsImport ?? false) || UniversalValidationHelper.IsInAESTransitionPeriod ? 35 : 70;

	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	[ResourceStringData("PLPreviousDocument|CSI_ReferenceNumber2", Caption = "Goods Shipment Number")]
	public override ZString CSI_ReferenceNumber2
	{
		get => base.CSI_ReferenceNumber2;
		set => base.CSI_ReferenceNumber2 = value;
	}

	protected bool CSI_ReferenceNumber2_ReadOnly => !(ParentIsJobComInvoiceLineOrHeader
													&& Constants.ValidationLists.SpecialProcedureCodesForPreviousDocuments(Parent.Factory).Contains(CSI_Code));

	public bool ParentIsJobComInvoiceLineOrHeader => Parent is JobComInvoiceLine or JobComInvoiceHeader;
}
