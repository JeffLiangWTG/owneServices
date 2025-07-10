using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsDepartureMovementHeaderPhase5Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation
{
	public NctsDepartureMovementHeaderPhase5Validation(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	protected override void CheckBM_ActiveBorderIdentificationType()
	{
		base.CheckBM_ActiveBorderIdentificationType();

		var parent = (NctsDepartureMovementHeader)Parent;
		var value = parent.BM_ActiveBorderIdentificationType;
		var targetInfo = parent.BM_ActiveBorderIdentificationTypeInfo;
		if (value.IsEmpty)
		{
			if (CheckRuleR2101())
			{
				targetInfo.AddMessageError(Res.GetString("F66AE42B-5A87-44FF-9C05-F51FFFB4C9B4", "[B2101] Type of ID can't be empty."));
			}
		}
	}

	protected override void CheckBM_CustomsOfficeAtBorder()
	{
		base.CheckBM_CustomsOfficeAtBorder();

		var parent = Parent;
		var value = parent.BM_CustomsOfficeAtBorder;
		var targetInfo = parent.BM_CustomsOfficeAtBorderInfo;
		if (value.IsEmpty && CheckRuleR2101())
		{
			targetInfo.AddMessageError(Res.GetString("78941B46-2272-446B-A310-53A87C2B6315", "[B2101] Customs Office for Transport Border must be declared."));
		}
	}

	protected override void CheckBM_RN_NKTOLCarrierNationality()
	{
		base.CheckBM_RN_NKTOLCarrierNationality();

		var parent = Parent;
		var value = parent.BM_RN_NKTOLCarrierNationality;
		var targetInfo = parent.BM_RN_NKTOLCarrierNationalityInfo;
		if (value.IsEmpty)
		{
			if (CheckRuleR2101())
			{
				targetInfo.AddMessageError(Res.GetString("1824C955-D0BD-4538-94C5-29FFDA2183D5", "[B2101] Nationality for Transport ID is empty. It must be declared."));
			}
			if (CheckRuleR1850())
			{
				targetInfo.AddMessageError(Res.GetString("384F96B9-6078-4E20-A8FA-9D6B48446944", "[B1850] Nationality for Transport ID is empty. It must be declared."));
			}
		}

		bool CheckRuleR1850() => IsInPhase5TransitionPeriod
			&& BorderMethodOfTransportDetailsIsVisible
			&& parent.BM_ExportTransportMode != ModeOfTransportList.Codes._2_RailTransport;
	}

	protected override void CheckBM_TOLCarrierID()
	{
		base.CheckBM_TOLCarrierID();

		var parent = (NctsDepartureMovementHeader)Parent;
		var value = parent.BM_TOLCarrierID;
		var targetInfo = parent.BM_TOLCarrierIDInfo;
		if (value.IsEmpty)
		{
			if (CheckRuleR2101())
			{
				targetInfo.AddMessageError(Res.GetString("A0CEBF44-65B8-47E3-835F-408B8EA12F81", "[B2101] Transport ID can't be empty."));
			}
		}
	}

	protected override void CheckBM_ExportDate()
	{
		base.CheckBM_ExportDate();

		if (Parent.BM_ExportDate.IsEmpty)
		{
			CheckRuleRP54();
		}
		else
		{
			CheckRuleRP56();
		}
	}

	void CheckRuleRP54()
	{
		var parent = Parent;
		if (parent.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D
			&& parent.IsSimplifiedNctsProcedure)
		{
			parent.BM_ExportDateInfo.AddMessageError(Res.GetString("3917C872-154E-4FBE-883D-AC9C045CB0EB", "[RP54] You have not entered Date Limit."));
		}
	}

	void CheckRuleRP56()
	{
		var parent = Parent;
		if (parent.IsSimplifiedNctsProcedure
			&& parent.BM_ExportDate < ZDateTime.Today)
		{
			parent.BM_ExportDateInfo.AddMessageError(Res.GetString("CC2104EC-E9C8-40B7-A713-89B7A4DAA22F", "[RP56] The entered Date Limit is earlier to today. It should be a current/future date."));
		}
	}

	bool CheckRuleR2101() => !IsInPhase5TransitionPeriod && BorderMethodOfTransportDetailsIsVisible;

	bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => Parent.IsInPhase5TransitionPeriod);
	CachedValue<bool> isInPhase5TransitionPeriod;
}
