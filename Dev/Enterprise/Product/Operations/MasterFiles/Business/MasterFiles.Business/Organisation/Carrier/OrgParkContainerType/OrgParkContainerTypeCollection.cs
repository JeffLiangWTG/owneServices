using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgParkContainerTypeCollection : ActiveBusinessObjectCollection<OrgParkContainerType>
	{
		public OrgParkContainerTypeCollection(OrgCarrierAppointedAgentPorts parentCarrierAgentType)
			: base(parentCarrierAgentType)
		{
			this.parentCarrierAgentType = parentCarrierAgentType;
		}
		readonly OrgCarrierAppointedAgentPorts parentCarrierAgentType;

		protected override void OnAdded(OrgParkContainerType orgParkContainerType)
		{
			base.OnAdded(orgParkContainerType);
			orgParkContainerType.ParentCarrierAgentType = parentCarrierAgentType;
		}

		public bool ContainsContainerClass(ZString containerClass)
		{
			foreach (var element in Find(new ZQuery(OrgParkContainerTypeSchema.PT_ContainerStorageClass, containerClass)))
			{
				return true;
			}
			return false;
		}
	}
}
