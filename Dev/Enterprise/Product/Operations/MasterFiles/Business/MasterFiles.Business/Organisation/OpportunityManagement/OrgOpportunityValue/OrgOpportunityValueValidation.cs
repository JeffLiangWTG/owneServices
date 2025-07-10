using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityValueValidation : AutoOrgOpportunityValueValidation
	{
		public OrgOpportunityValueValidation(AutoOrgOpportunityValue parent) : base(parent)
		{
		}

		#region PV_RevenueType

		protected override void CheckPV_RevenueType()
		{
			base.CheckPV_RevenueType();
			MandatoryValidation.CheckEntered(Parent.PV_RevenueTypeInfo);
			if (!Parent.IsInDatabase || Parent.PV_RevenueTypeInfo.HasChanges || !Parent.Lookups.ValueTypes.ContainsCode(Parent.PV_RevenueType))
			{
				ListValidation.ErrorIfInvalidCode(Parent.PV_RevenueTypeInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.PV_RevenueTypeInfo, Parent.Lookups.ActiveValueTypes, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion
	}
}
