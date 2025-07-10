using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class AllocationMethodDefaultRule : AutoAllocationMethodDefaultRule
	{
		public AllocationMethodDefaultRule(BusinessObjectFactory factory)
			: base(factory) { }

		[List("Lookups.AllocationMethods")]
		public override ZString AllocationMethod
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.AllocationMethod; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.AllocationMethod = value; }
		}

		[List("Lookups.Countries")]
		public override ZString CountryCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CountryCode; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.CountryCode = value; }
		}

		public AllocationMethodDefaultRuleLookups Lookups
		{
			get { return lookups ?? (lookups = new AllocationMethodDefaultRuleLookups(this)); }
		}
		AllocationMethodDefaultRuleLookups lookups;
	}
}
