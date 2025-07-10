using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyOrigin : AgencyAllocationItem<VoyageOrigin>, IObsoleteValidation
	{
		public AgencyOrigin(VoyageOrigin origin, ZGuid principalPK)
			: base(origin, principalPK) { }

		#region Related BusinessObjects

		public VoyageOrigin Origin
		{
			get { return BizObj; }
		}

		public AgencyPrincipal Principal
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return principal; }
			[System.Diagnostics.DebuggerStepThrough]
			set { principal = value; }
		}
		AgencyPrincipal principal;

		#endregion

		#region Implementation

		protected override AllocationUsage GetUsage()
		{
			return Principal.UsageProvider.GetOriginUsage(Origin);
		}

		protected override bool ShouldValidateUsage
		{
			get { return Origin.VoyageCountry.J0_AllocationMethod == AllocationMethodList.Codes.Origin; }
		}

		protected override SlotAllocationDependentCollection SlotAllocations
		{
			get { return Origin == null ? null : Origin.SlotAllocations; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		#endregion
	}
}
