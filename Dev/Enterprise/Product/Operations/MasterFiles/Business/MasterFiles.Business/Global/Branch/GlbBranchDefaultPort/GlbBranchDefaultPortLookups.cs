//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbBranchDefaultPortLookups
//
//    This class should be used for overriding collections in AutoGlbBranchDefaultPortLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchDefaultPortLookups : AutoGlbBranchDefaultPortLookups
	{
		public GlbBranchDefaultPortLookups(AutoGlbBranchDefaultPort parent) : base(parent)
		{
		}

		public GlbBranchDefaultPort DefaultPort
		{
			get { return Parent as GlbBranchDefaultPort; }
		}

		bool IsDefaultToShipment => DefaultPort.GBP_DefaultTo == GlbBranchDefaultToList.Codes.ShipmentOrigin || DefaultPort.GBP_DefaultTo == GlbBranchDefaultToList.Codes.ShipmentDestination;

		#region DefaultToList

		public CodeDescriptionPairList DefaultToList
		{
			get { return Factory.GetCachedValue<GlbBranchDefaultToList>(); }
		}

		#endregion

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				return Factory.GetCachedValue("DefaultPortCodePairLists.TransportModeList_" + (IsDefaultToShipment ? (NoResString)"Shipment" : (NoResString)"Consol"),
					() => DefaultPortCodePairLists.TransportModeList(IsDefaultToShipment));
			}
		}

		#endregion

		#region ContainerModeList

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				if (IsDefaultToShipment)
				{
					return Factory.GetCachedValue("DefaultPortCodePairLists.ShipmentContainerModeList" + DefaultPort.GBP_TransportMode,
						() => DefaultPortCodePairLists.ShipmentContainerModeList(DefaultPort.GBP_TransportMode));
				}

				return Factory.GetCachedValue("DefaultPortCodePairLists.ConsolContainerModeList_" + DefaultPort.GBP_TransportMode,
					() => DefaultPortCodePairLists.ConsolContainerModeList(DefaultPort.GBP_TransportMode));
			}
		}

		#endregion
	}
}
