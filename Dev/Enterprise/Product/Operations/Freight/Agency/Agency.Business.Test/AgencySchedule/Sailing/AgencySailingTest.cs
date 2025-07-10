using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencySailingTest : AgencyAllocationItemTest<JobSailing>
	{
		#region Implementation
		protected override BusinessObject GetAllocationParentFromVoyage(JobVoyage voyage)
		{
			return voyage.Sailings[0];
		}

		protected override AgencyAllocationItem<JobSailing> WrapAllocationParent(AgencyPrincipal principal, BusinessObject allocationParent)
		{
			return principal.Sailings.GetWrapperForTesting((JobSailing)allocationParent);
		}

		protected override ZString AllocationMethod
		{
			get
			{
				return AllocationMethodList.Codes.Sailing;
			}
		}

		protected override SlotAllocationDependentCollection GetSlotCollection(BusinessObject allocationParent)
		{
			return ((JobSailing)allocationParent).SlotAllocations;
		}
		#endregion
	}
}
