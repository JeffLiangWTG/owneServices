using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class AllocationMethodDefaultHeaderLookups : ZLookups
	{
		public AllocationMethodDefaultHeaderLookups(AllocationMethodDefaultHeader parent)
			: base(parent) { }

		public AllocationMethodList AllocationMethods
		{
			get { return allocationMethods ?? (allocationMethods = Parent.CurrentFactory.GetCachedValue<AllocationMethodList>()); }
		}
		AllocationMethodList allocationMethods;

		#region Implementation

		protected new AllocationMethodDefaultHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AllocationMethodDefaultHeader)base.Parent; }
		}

		#endregion
	}
}
