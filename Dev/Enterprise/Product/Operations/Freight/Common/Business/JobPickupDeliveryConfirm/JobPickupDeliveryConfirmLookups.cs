using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Common.Business
{
	public class JobPickupDeliveryConfirmLookups : AutoJobPickupDeliveryConfirmLookups
	{
		public JobPickupDeliveryConfirmLookups(AutoJobPickupDeliveryConfirm parent) : base(parent)
		{
		}

		public LocalTransportCollection LocalTransportList
		{
			get
			{
				if (fLocalTransportList == null)
				{
					fLocalTransportList = new LocalTransportCollection(Factory);
				}

				return fLocalTransportList;
			}
		}
		LocalTransportCollection fLocalTransportList;
	}
}
