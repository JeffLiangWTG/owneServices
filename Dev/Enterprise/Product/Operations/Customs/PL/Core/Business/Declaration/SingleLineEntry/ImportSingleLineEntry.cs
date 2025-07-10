using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportSingleLineEntry : SingleLineEntry
{
	public ImportSingleLineEntry(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
	}

	public JobDeclaration Declaration => (JobDeclaration)declaration;

	public new ImportSingleLineEntryLookups Lookups => (ImportSingleLineEntryLookups)base.Lookups;

	[List(nameof(Lookups) + "." + nameof(ImportSingleLineEntryLookups.CountryOfOrigins))]
	[ResourceStringData("PLImportSingleLineEntry|GoodsOrigin", Caption = "[34] Goods Origin")]
	[MaxLength(2)]
	public ZString GoodsOrigin
	{
		get => goodsOrigin;
		set
		{
			SetNonPersistentPropertyValue(GoodsOriginInfo, ref goodsOrigin, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateGoodsOrigin();
			}
		}
	}

	public ZPropertyInfo GoodsOriginInfo => GetZPropertyInfo(nameof(GoodsOrigin));

	ZString goodsOrigin;

	[List(nameof(Lookups) + "." + nameof(ImportSingleLineEntryLookups.PreviousDocumentCodes))]
	[ResourceStringData("PLImportSingleLineEntry|PreviousDocument", Caption = "Previous Document")]
	[MaxLength(6)]
	public ZString PreviousDocument
	{
		get => previousDocument;
		set
		{
			SetNonPersistentPropertyValue(PreviousDocumentInfo, ref previousDocument, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidatePreviousDocument();
			}
		}
	}

	public ZPropertyInfo PreviousDocumentInfo => GetZPropertyInfo(nameof(PreviousDocument));

	ZString previousDocument;

	[ResourceStringData("PLImportSingleLineEntry|PreviousDocumentReferenceNumber", Caption = "Previous Document Reference Number", MediumCaption = "Reference Number", ShortCaption = "Ref. Num.")]
	[MaxLength(35)]
	public ZString PreviousDocumentNumber
	{
		get => previousDocumentNumber;
		set => SetNonPersistentPropertyValue(PreviousDocumentNumberInfo, ref previousDocumentNumber, value);
	}

	public ZPropertyInfo PreviousDocumentNumberInfo => GetZPropertyInfo(nameof(PreviousDocumentNumber));

	ZString previousDocumentNumber;

	public new ImportSingleLineEntryValidation Validation => (ImportSingleLineEntryValidation)base.Validation;

	protected override SingleLineEntryValidation GetNewValidation() => new ImportSingleLineEntryValidation(this);

	protected override SingleLineEntryLookups GetNewLookups() => new ImportSingleLineEntryLookups(this);
}
