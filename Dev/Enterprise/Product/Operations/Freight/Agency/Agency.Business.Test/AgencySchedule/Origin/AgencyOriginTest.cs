using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyOriginTest : AgencyAllocationItemTest<VoyageOrigin>
	{
		#region Implementation
		protected override BusinessObject GetAllocationParentFromVoyage(JobVoyage voyage)
		{
			return voyage.Origins[0];
		}

		protected override AgencyAllocationItem<VoyageOrigin> WrapAllocationParent(AgencyPrincipal principal, BusinessObject allocationParent)
		{
			return principal.Origins.GetWrapperForTesting((VoyageOrigin)allocationParent);
		}

		protected override ZString AllocationMethod
		{
			get
			{
				return AllocationMethodList.Codes.Origin;
			}
		}

		protected override SlotAllocationDependentCollection GetSlotCollection(BusinessObject allocationParent)
		{
			return ((VoyageOrigin)allocationParent).SlotAllocations;
		}
		#endregion
	}
}
