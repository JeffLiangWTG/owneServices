using CargoWise.Common;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class DeliveryOrderDocDataSendingObject
	{
		public DeliveryOrderDocDataSendingObject(DeliveryOrderDocDataObject deliveryOrderDocDataObject, bool isCancelActionTypeCode)
		{
			DeliveryOrderDocDataObject = Argument.NotNull(deliveryOrderDocDataObject, nameof(deliveryOrderDocDataObject));
			IsCancelActionTypeCode = isCancelActionTypeCode;
		}

		public readonly DeliveryOrderDocDataObject DeliveryOrderDocDataObject;

		public bool IsCancelActionTypeCode { get; }
	}
}
