using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgLandedCostingPrefsValidation : AutoOrgLandedCostingPrefsValidation
	{
		public OrgLandedCostingPrefsValidation(AutoOrgLandedCostingPrefs parent) : base(parent)
		{
			this.Prefs = (OrgLandedCostingPrefs)parent;
		}

		readonly OrgLandedCostingPrefs Prefs;

		#region Cost Distribution Mechanism

		protected override void CheckO9_DistributeCostBy()
		{
			base.CheckO9_DistributeCostBy();
			MandatoryValidation.CheckEntered(Prefs.O9_DistributeCostByInfo);
			ListValidation.ErrorIfInvalidCode(Prefs.O9_DistributeCostByInfo);
		}

		#endregion

		#region Group ID

		protected override void CheckO9_LandedCostGroup()
		{
			base.CheckO9_LandedCostGroup();
			MandatoryValidation.CheckEntered(Prefs.O9_LandedCostGroupInfo);

			foreach (OrgLandedCostingPrefs pref in Prefs.Header.LandedCostingPreferences)
			{
				if (pref.PK != Prefs.PK && pref.O9_LandedCostGroup == Prefs.O9_LandedCostGroup)
				{
					Prefs.O9_LandedCostGroupInfo.AddError(Res.GetString("29cc8663-f435-488a-ab28-ef7d28e2c6d6", "You cannot have more than one landed cost group with the same ID."));
					break;
				}
			}
		}

		#endregion

		#region Group Name

		protected override void CheckO9_LandedCostGroupName()
		{
			base.CheckO9_LandedCostGroupName();
			MandatoryValidation.CheckEntered(Prefs.O9_LandedCostGroupNameInfo);
		}

		#endregion
	}
}
