using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class HTICusUSClassificationValidation : CusUSClassificationValidation
	{
		public HTICusUSClassificationValidation(CusUSClassification parent)
			: base(parent)
		{
		}

		protected override void CheckCD_DDTCIndicator()
		{
			base.CheckCD_DDTCIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_DDTCIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.DDTC, ParentPivot.OGAAgencyRequirements);
		}

		protected override void CheckCD_OMCIndicator()
		{
			base.CheckCD_OMCIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.CD_OMCIndicatorInfo, Parent.Lookups.US_OGAIndicatorList);
			var supTariff = ParentPivot.ImportSupTariff;
			var tariff = ParentPivot.ImportTariff;

			var hasRequirement = supTariff != null && supTariff.HasOMCRequirement || tariff != null && tariff.HasOMCRequirement;

			AgencyRequirementsValidator.ValidatePGA(Parent.CD_OMCIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.OMC, ZZCustomsFunctionality.IsOMCEffective, hasRequirement);
			AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_OMCIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.OMC, ParentPivot.OMCHeaders.Cast<IPGADataCorrection>());
			AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_OMCIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireOMC);

			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.OMC, ParentPivot.OGAAgencyRequirements);
			ValidateCD_OMCDisclaimReason();
		}

		protected override void CheckCD_AMMVPerUnit()
		{
			base.CheckCD_AMMVPerUnit();
			if (!Parent.CD_AMMVPercentage.IsEmpty && !Parent.CD_AMMVPerUnit.IsEmpty)
			{
				Parent.CD_AMMVPerUnitInfo.AddMessageError(Res.GetString("5D9B331F-4984-42C2-B1F3-2F9CFDCD1182", "You can enter either Assist/AMMV Unit or Assist/AMMV Percent, not both."));
			}
		}

		protected override void CheckCD_AMMVPercentage()
		{
			base.CheckCD_AMMVPercentage();
			if (!Parent.CD_AMMVPercentage.IsEmpty && !Parent.CD_AMMVPerUnit.IsEmpty)
			{
				Parent.CD_AMMVPercentageInfo.AddMessageError(Res.GetString("5BE41FE6-B78E-4CFD-8AFF-E51C95F3EDBC", "You can enter either Assist/AMMV Unit or Assist/AMMV Percent, not both."));
			}
		}

		protected override void CheckCD_HFCIndicator()
		{
			base.CheckCD_HFCIndicator();

			AgencyRequirementsValidator.ValidatePGA(Parent.CD_HFCIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.HFC, true, ParentPivot.PGARequirementIndicator.RequireHFC);
			AgencyRequirementsValidator.ValidatePGAIndicatorAndData(Parent.CD_HFCIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.HFC, ParentPivot.HFCHeaders.Cast<IPGADataCorrection>());
			AgencyRequirementsValidator.ValidatePGADisclaimedIndicator(Parent.CD_HFCIndicatorInfo, ParentPivot.PGARequirementIndicator.RequireHFC);

			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.HFC, ParentPivot.OGAAgencyRequirements);
			ValidateCD_HFCDisclaimReason();
		}
	}
}
