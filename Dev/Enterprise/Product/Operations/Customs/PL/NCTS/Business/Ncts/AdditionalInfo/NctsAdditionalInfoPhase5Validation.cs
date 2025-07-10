using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsAdditionalInfoPhase5Validation : EU.NCTS.Business.NctsAdditionalInfoPhase5Validation
{
	public NctsAdditionalInfoPhase5Validation(EU.NCTS.Business.NctsAdditionalInfo parent) : base(parent)
	{
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		CheckCSI_Code_GoodsLevel();
		CheckDuplicatePOW01_PCS01_AtConsignmentLevel();
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		var parent = Parent;
		if (parent.ImportExportParent is NctsHeader header)
		{
			if (IsInfSubTypeAndCodeEqualsPOW01orPCS01(parent) && parent.CSI_Description.IsEmpty)
			{
				parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("A9CB9438-4314-42B4-8B65-5D9CCFCCC73B",
					"[RP51] Description is required for Additional Information Type 'POW01' or 'PCS01' and must contain IDSISC or email address"));
			}
		}
	}

	void CheckCSI_Code_GoodsLevel()
	{
		var parent = Parent;
		if (parent.ImportExportParent is NctsCommonCargoDesc)
		{
			if (IsInfSubTypeAndCodeEqualsPOW01orPCS01(parent))
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("E654A208-CFC3-4C2C-BA9D-D38DABCB1FA4",
					"[RP51] The POW01/PCS01 code should be declared only at the Consignment level."));
			}
		}
	}

	void CheckDuplicatePOW01_PCS01_AtConsignmentLevel()
	{
		var parent = Parent;

		if (parent.ImportExportParent is NctsHeader header && IsInfSubTypeAndCodeEqualsPOW01orPCS01(parent))
		{
			bool otherSpecialCodeExists = header.AdditionalDocuments
				.Any(info =>
					info != parent &&
					IsInfSubTypeAndCodeEqualsPOW01orPCS01(info));

			if (otherSpecialCodeExists)
			{
				parent.CSI_CodeInfo.AddMessageError(
					Res.GetString("0F34A48A-B8D2-4058-B48D-C75E08C625A4",
					"[RP51] The declaration may contain only one occurrence of either 'POW01' or 'PCS01' code."));
			}
		}
	}

	bool IsInfSubTypeAndCodeEqualsPOW01orPCS01(EU.NCTS.Business.NctsAdditionalInfo parent)
	{
		return parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
			&& (parent.CSI_Code == Constants.AdditionalInfoCodes._POW01
			|| parent.CSI_Code == Constants.AdditionalInfoCodes._PCS01);
	}
}


