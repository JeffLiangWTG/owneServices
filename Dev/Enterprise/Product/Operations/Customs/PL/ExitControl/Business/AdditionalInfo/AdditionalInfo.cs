using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class AdditionalInfo(BusinessObjectFactory factory, DataRow row)
	: EU.ExitControl.Business.AdditionalInfo(factory, row)
{
	[ResourceStringData("31EBE2EE-8FB2-4E1B-B4E0-A83B67D48E19", Caption = "Kind")]
	[ReadOnly(false)]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set => base.CSI_SubType = value;
	}

	[ResourceStringData("848895D6-965E-452E-B038-8DDD60CDDE0D", Caption = "Status", FullDescription = "Additional Document Status")]
	[ReadOnlyMember(nameof(CSI_Status_ReadOnly))]
	public override ZString CSI_Status
	{
		get => base.CSI_Status;
		set => base.CSI_Status = value;
	}

	protected bool CSI_Status_ReadOnly => CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;

	protected override CusSupportingInfoLookups GetNewLookups() => IsUCC6
		? new AdditionalInfoUcc6Lookups(this)
		: new AdditionalInfoLookups(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CSI_SubType = ZString.Empty;
		CSI_Status = ZString.Empty;
	}
	protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);
}
