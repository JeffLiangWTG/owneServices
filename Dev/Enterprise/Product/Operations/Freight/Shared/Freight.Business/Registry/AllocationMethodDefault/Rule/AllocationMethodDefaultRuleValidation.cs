using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class AllocationMethodDefaultRuleValidation : AutoAllocationMethodDefaultRuleValidation
	{
		public AllocationMethodDefaultRuleValidation(AutoAllocationMethodDefaultRule parent)
			: base(parent) { }

		protected override void CheckAllocationMethod()
		{
			base.CheckAllocationMethod();
			MandatoryValidation.CheckEntered(Parent.AllocationMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AllocationMethodInfo, Parent.Lookups.AllocationMethods);
		}

		protected override void CheckCountryCode()
		{
			base.CheckCountryCode();
			MandatoryValidation.CheckEntered(Parent.CountryCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CountryCodeInfo, Parent.Lookups.Countries);
		}

		#region Implementation

		public new AllocationMethodDefaultRule Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AllocationMethodDefaultRule)base.Parent; }
		}

		#endregion
	}
}
