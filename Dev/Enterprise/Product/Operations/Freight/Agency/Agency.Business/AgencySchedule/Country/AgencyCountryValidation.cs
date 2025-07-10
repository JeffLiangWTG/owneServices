using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyCountryValidation : AutoAgencyCountryValidation
	{
		public AgencyCountryValidation(AutoAgencyCountry parent)
			: base(parent) { }

		protected override void CheckJ0_AllocationsByPrincipal()
		{
			base.CheckJ0_AllocationsByPrincipal();
			if (Parent.J0_AllocationsByPrincipal)
			{
				switch (Parent.Principals.Count)
				{
					case 0:
						Parent.J0_AllocationsByPrincipalInfo.AddError(Res.GetString("494e5d08-8d87-4092-b6c6-a1de2d71c842", "You have enabled per-principal allocations but have not added any principals to the principals list."));
						break;

					case 1:
						Parent.J0_AllocationsByPrincipalInfo.AddWarning(Res.GetString("59e46458-5ecd-4b8f-8016-b43de6680fc7", "You have enabled per-principal allocations but only added one principal."));
						break;
				}
			}
			else if (Parent.Principals.Count > 0)
			{
				Parent.J0_AllocationsByPrincipalInfo.AddError(Res.GetString("ead3fcdf-b2c0-4594-9135-e58400e30fad", "There are principals in the principals list but per-principal allocations has not been enabled."));
			}
		}

		protected override void CheckJ0_AllocationMethod()
		{
			base.CheckJ0_AllocationMethod();
			MandatoryValidation.CheckEntered(Parent.J0_AllocationMethodInfo, Res.GetString("2f999598-78aa-484e-a3bc-e387dbcc91bb", "Allocation Method"));
			ListValidation.ErrorIfInvalidCode(Parent.J0_AllocationMethodInfo, Parent.Lookups.AllocationMethods);

			if (!Parent.J0_AllocationMethodInfo.HasErrors())
			{
				if (Parent.J0_AllocationMethod == AllocationMethodList.Codes.NotSet)
				{
					if (!Parent.GenericPrincipal.IsUsageEmpty)
					{
						Parent.J0_AllocationMethodInfo.AddError(Res.GetString("117e7245-e2da-47c0-aee1-af30dc98a635", "You cannot change allocations to \"Not Set\" because there is cargo booked against this sailing. Either revert to the original allocation method or choose \"Ignore\" to disregard and ignore allocations for this sailing."));
					}
				}
			}
		}

		#region Implementation

		public new AgencyCountry Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyCountry)base.Parent; }
		}

		#endregion
	}
}
