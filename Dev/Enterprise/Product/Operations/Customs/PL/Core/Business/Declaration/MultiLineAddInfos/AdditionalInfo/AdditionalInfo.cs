using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
{
	public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	internal JobDeclaration JobDeclaration
		=> Parent switch
		{
			JobComInvoiceLine invoiceLine => invoiceLine.Declaration,
			JobComInvoiceHeader header => header.JobDeclaration,
			CusEntryInstruction instruction => instruction.JobDeclaration,
			_ => Parent as JobDeclaration
		};

	[ResourceStringData("PLAdditionalInfo|CSI_SubType", Caption = "Kind")]
	[MaxLength(nameof(CSI_SubTypeMaxLength))]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set
		{
			var oldValue = CSI_SubType;
			base.CSI_SubType = value;
			if (value != oldValue && !IsCopying)
			{
				if (TryGetPreConfiguredDescription(out var description))
				{
					CSI_Description = description;
				}
				else if (IsAnAdditionalInformation)
				{
					CSI_ReferenceNumber = ZString.Empty;
				}
				else
				{
					CSI_Description = ZString.Empty;
				}
			}
		}
	}

	[ReadOnlyMember(nameof(IsDescriptionReadOnly))]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set => base.CSI_Description = value;
	}

	public bool IsDescriptionReadOnly => IsNotAdditionalInformation || TryGetPreConfiguredDescription(out _);

	bool TryGetPreConfiguredDescription(out ZString value)
	{
		if (IsAnAdditionalInformation && CSI_Code == Constants.AdditionalInfoCodes._PCS01 && !string.IsNullOrWhiteSpace(PLCustomsDataRegistry.Instance.PCSEmailChannel.Value))
		{
			value = new(PLCustomsDataRegistry.Instance.PCSEmailChannel.Value);
			return true;
		}

		value = ZString.Empty;
		return false;
	}

	[MaxLength(70)]
	[ReadOnlyMember(nameof(IsAnAdditionalInformation))]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ResourceStringData("PLAdditionalInfo|CSI_Code", Caption = "Type")]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (value != oldValue && !IsCopying)
			{
				if (TryGetPreConfiguredDescription(out var description))
				{
					CSI_Description = description;
				}

				JobDeclaration?.MarkAsNeedingValidation();
			}
		}
	}

	protected new EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation Validation => (EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation()
	{
		return IsImport() ? new ImportAdditionalInfoValidation(this) : new ExportAdditionalInfoValidation(this);
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Status = ZString.Empty;
	}

	protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInfoLookups(this);

	public new AdditionalInfoLookups Lookups => (AdditionalInfoLookups)base.Lookups;

	public ZBool IsImport() => (Parent as EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport)?.IsImport ?? false;

	ZBool IsNotAdditionalInformation => !IsAnAdditionalInformation && !IsImport();

	protected override bool IsLineCore => true;

	int CSI_SubTypeMaxLength => IsImport() ? CusSupportingInfo.Schema.CSI_SubTypeMaxLength : 3;
}
