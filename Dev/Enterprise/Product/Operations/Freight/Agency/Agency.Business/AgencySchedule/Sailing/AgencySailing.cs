using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencySailing : AgencyAllocationItem<JobSailing>, IObsoleteValidation
	{
		public AgencySailing(JobSailing sailing, ZGuid principalPK)
			: base(sailing, principalPK)
		{
		}

		#region Related BusinessObjects

		public JobSailing Sailing
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
			return Principal.UsageProvider.GetSailingUsage(Sailing);
		}

		protected override bool ShouldValidateUsage
		{
			get { return Sailing.Origin.VoyageCountry.J0_AllocationMethod == AllocationMethodList.Codes.Sailing; }
		}

		protected override SlotAllocationDependentCollection SlotAllocations
		{
			get { return Sailing == null ? null : Sailing.SlotAllocations; }
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
