using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmLookups : JobPickupDeliveryConfirmLookups
	{
		public CommonPickupDeliveryConfirmLookups(CommonPickupDeliveryConfirm parent)
			: base(parent)
		{
		}

		#region DropModes

		public CodeDescriptionPairList DropModes
		{
			get { return BindToLists.DropModes(Parent.IsContainerised); }
		}

		#endregion

		#region BindToLists

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region ConsolidatedTransportBookings

		public CommonConsolidatedTransportBookingCollection ConsolidatedTransportBookings
		{
			get { return new CommonConsolidatedTransportBookingCollection(Factory); }
		}

		#endregion

		public new CommonPickupDeliveryConfirm Parent
		{
			get { return (CommonPickupDeliveryConfirm)base.Parent; }
		}
	}
}
