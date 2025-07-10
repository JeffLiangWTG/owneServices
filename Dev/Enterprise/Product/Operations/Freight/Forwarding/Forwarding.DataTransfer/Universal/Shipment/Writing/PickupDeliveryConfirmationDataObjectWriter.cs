using Enterprise.Freight.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class PickupDeliveryConfirmationDataObjectWriter : DataObjectWriter<CommonPickupDeliveryConfirm, Confirmation>
	{
		public PickupDeliveryConfirmationDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override Confirmation PopulateDataObject(CommonPickupDeliveryConfirm confirmBO)
		{
			var confirmation = new Confirmation();

			if (confirmBO.EU_PickupDeliveryType == Core.Constants.PickupDeliveryConfirmTypes.OriginPickup)
			{
				confirmation.DateDescription = ConfirmationTypes.Codes.PickUp;
			}
			else if (confirmBO.EU_PickupDeliveryType == Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
			{
				confirmation.DateDescription = ConfirmationTypes.Codes.Delivery;
			}

			confirmation.EstimatedDate = confirmBO.EU_PlannedPickupDeliveryTime;
			confirmation.RequiredToDate = confirmBO.EU_RequestedPickupDeliveryTime;
			confirmation.ActualDate = confirmBO.EU_PickupDeliveryTime;
			confirmation.ReceivedBy = confirmBO.EU_GoodsSignForBy;
			confirmation.Distance = confirmBO.EU_Distance;
			confirmation.DistanceUnit = ListHelper.GetWithDescription<UnitOfLength>(confirmBO.EU_DistanceUnit, new CodeDescriptionPairList(OLookUpEditType.Length));

			return confirmation;
		}
	}
}
