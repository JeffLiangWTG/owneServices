using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class AllocationMethodDefaultRuleLookups : ZLookups
	{
		public AllocationMethodDefaultRuleLookups(AllocationMethodDefaultRule parent)
			: base(parent) { }

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Parent.Factory); }
		}

		public AllocationMethodList AllocationMethods
		{
			get { return allocationMethods ?? (allocationMethods = Factory.GetCachedValue<AllocationMethodList>()); }
		}
		AllocationMethodList allocationMethods;

		#region Implementation

		protected new AllocationMethodDefaultRule Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AllocationMethodDefaultRule)base.Parent; }
		}

		#endregion
	}
}
