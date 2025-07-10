using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgLandedCostingPrefChargesValidation : AutoOrgLandedCostingPrefChargesValidation
	{
		public OrgLandedCostingPrefChargesValidation(AutoOrgLandedCostingPrefCharges parent) : base(parent)
		{
			this.Charges = (OrgLandedCostingPrefCharges)parent;
		}

		readonly OrgLandedCostingPrefCharges Charges;

		#region Charge Code

		protected override void CheckO0_AC_ChargeCode()
		{
			base.CheckO0_AC_ChargeCode();

			if (!Charges.O0_ChargeGroup.IsEmpty && Charges.ChargeCode != null)
			{
				if (Charges.O0_Excluded && Charges.ChargeCode.AC_ChargeGroup != Charges.O0_ChargeGroup)
				{
					Charges.O0_AC_ChargeCodeInfo.AddError(Res.GetString("b7b6dfe4-d33f-4076-afbe-d4afb1a8b007", "You cannot exclude this charge code as it is not part of the charge group specified."));
				}
				else if (!Charges.O0_Excluded && Charges.ChargeCode.AC_ChargeGroup == Charges.O0_ChargeGroup)
				{
					Charges.O0_AC_ChargeCodeInfo.AddError(Res.GetString("6fcfea99-432c-4886-8cae-22c11e1a54a2", "You cannot include this charge code as it is already part of the charge group specified, and therefore already included."));
				}
			}

			ValidateO0_Excluded();
			ValidateEachChargeExistsInOneLandedCostGroupOnly();
		}

		#endregion

		#region Charge Group

		protected override void CheckO0_ChargeGroup()
		{
			base.CheckO0_ChargeGroup();
			ListValidation.ErrorIfInvalidCode(Charges.O0_ChargeGroupInfo);

			ValidateO0_Excluded();
			ValidateEachChargeExistsInOneLandedCostGroupOnly();
		}

		#endregion

		#region Exclude Charge

		protected override void CheckO0_Excluded()
		{
			base.CheckO0_Excluded();

			if (Charges.O0_Excluded && (Charges.O0_ChargeGroup.IsEmpty || Charges.O0_AC_ChargeCode.IsEmpty))
			{
				Charges.O0_ExcludedInfo.AddError(Res.GetString("2f8dc1d8-8fe9-46b1-b190-33c5c2568971", "You cannot exclude a charge unless you specify a Charge Group and a Charge Code"));
			}

			ValidateO0_AC_ChargeCode();
		}

		#endregion

		#region Uniqueness Across Groups

		void ValidateEachChargeExistsInOneLandedCostGroupOnly()
		{
			Charges.ClearRowNotifications();

			OrgLandedCostingPrefs duplicatePref = null;

			if (!Charges.O0_ChargeGroup.IsEmpty)
			{
				duplicatePref = Charges.LandedCostingPrefs.Header.LandedCostingPreferences.GetLandedCostingGroupFromChargeGroup(Charges.O0_ChargeGroup, Charges.LandedCostingPrefs);
			}

			if (duplicatePref == null && Charges.ChargeCode != null && !Charges.O0_Excluded)
			{
				duplicatePref = Charges.LandedCostingPrefs.Header.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(Charges.ChargeCode, Charges.LandedCostingPrefs);
			}

			if (duplicatePref != null)
			{
				Charges.AddRowError(Res.GetString("58022601-35b1-4eff-bfba-ab197ebf4060", "This charge group / code is already included in Landed Cost Group {0} ({1}).", duplicatePref.O9_LandedCostGroup, duplicatePref.O9_LandedCostGroupName));
			}
		}

		#endregion
	}
}
