using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Declaration;
namespace Enterprise.Customs.PL.NCTS.Business;
public class NctsBillAdditionalDocumentPhase5Validation : NctsBillAdditionalDocumentValidation
{
	public NctsBillAdditionalDocumentPhase5Validation(NctsBillAdditionalDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		CheckCSI_Code_HouseConsignmentLevel();
	}

	void CheckCSI_Code_HouseConsignmentLevel()
	{
		NctsBillAdditionalDocument parent = (NctsBillAdditionalDocument)Parent;

			if (parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
			&& (parent.CSI_Code == Constants.AdditionalInfoCodes._POW01 || parent.CSI_Code == Constants.AdditionalInfoCodes._PCS01)
			)
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("E654A208-CFC3-4C2C-BA9D-D38DABCB1FA4",
					"[RP51] The POW01/PCS01 code should be declared only at the Consignment level."));
			}
	}
}

