using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
{
	public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override CusSupportingInfoValidation GetNewPhase5Validation()
		=> new NctsAdditionalInfoPhase5Validation(this);

	protected override int CSI_ReferenceNumberMaxLength =>
		ParentAsGoodsItem is NctsArrivalCargoDesc && IsPhase5 ? 70 : base.CSI_ReferenceNumberMaxLength;

	protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsAdditionalInfoPhase5Lookups(this);

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
			}
		}
	}

	[ReadOnlyMember(nameof(IsDescriptionReadOnly))]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set => base.CSI_Description = value;
	}

	bool TryGetPreConfiguredDescription(out ZString value)
	{
		if (IsAnAdditionalInformation
			&& (CSI_Code == Constants.AdditionalInfoCodes._PCS01 || CSI_Code == Constants.AdditionalInfoCodes._POW01)
			&& !string.IsNullOrWhiteSpace(PLCustomsDataRegistry.Instance.PCSEmailChannel.Value))
		{
			value = new(PLCustomsDataRegistry.Instance.PCSEmailChannel.Value);
			return true;
		}

		value = ZString.Empty;
		return false;
	}

	public bool IsDescriptionReadOnly => IsNotAdditionalInformation || TryGetPreConfiguredDescription(out _);

	ZBool IsNotAdditionalInformation => !IsAnAdditionalInformation;
}
