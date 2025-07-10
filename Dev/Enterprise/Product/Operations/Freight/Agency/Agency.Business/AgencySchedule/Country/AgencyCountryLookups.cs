using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyCountryLookups : ZLookups
	{
		public AgencyCountryLookups(AgencyCountry parent)
			: base(parent) { }

		public AllocationMethodList AllocationMethods
		{
			get { return allocationMethods ?? (allocationMethods = Factory.GetCachedValue<AllocationMethodList>()); }
		}
		AllocationMethodList allocationMethods;

		public ShipsAgencyPrincipalCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		#region Implementation

		protected new AgencyCountry Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyCountry)base.Parent; }
		}

		#endregion
	}
}
