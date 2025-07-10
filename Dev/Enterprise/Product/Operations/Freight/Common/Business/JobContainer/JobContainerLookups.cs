//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobContainerLookups
//
//    This class should be used for overriding collections in AutoJobContainerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class JobContainerLookups : AutoJobContainerLookups
	{
		public JobContainerLookups(AutoJobContainer parent) : base(parent)
		{
		}

		public FCLEquipmentNeededList DropModeList
		{
			get { return Factory.GetCachedValue<FCLEquipmentNeededList>(); }
		}

		public ContainerYardCollection ContainerYardList
		{
			get { return BindingLists.OrgContainerYard_List; }
		}

		public ICodeDescriptionPairList ContainerQualities
		{
			get { return FreightDataRegistry.Instance.ContainerQualityList.Value; }
		}

		public ICodeDescriptionPairList ContainerStatuses
		{
			get { return FreightDataRegistry.Instance.ContainerStatusList.Value; }
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}
	}
}
