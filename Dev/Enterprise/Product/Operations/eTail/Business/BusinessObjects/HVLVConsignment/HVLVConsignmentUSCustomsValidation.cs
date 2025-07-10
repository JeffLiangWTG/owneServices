using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentUSCustomsValidation : ValidationProvider
	{
		public static void MessageErrorIfWaybillTooLong(ZPropertyInfo waybillPropertyInfo)
		{
			var consignment = waybillPropertyInfo.BizObj as HVLVConsignment;
			if (consignment != null && consignment.ManifestedOnShipment is ForwardingShipment shipment)
			{
				var currentValue = (ZString)waybillPropertyInfo.Value;
				var waybillLength = currentValue.Length;
				var shipmentSCACCode = shipment.SCACCode(consignment.Factory);
				var startsWithSCACCode = !string.IsNullOrEmpty(shipmentSCACCode) && currentValue.StartsWith(shipmentSCACCode);

				if (startsWithSCACCode && waybillLength > USWaybillNumberMaxLength_WithSCACCode)
				{
					waybillPropertyInfo.AddMessageError(USWaybillNumberWithSCACCodeInvalidErrorMessage);
				}
				else if (!startsWithSCACCode && waybillLength > USWaybillNumberMaxLength_WithoutSCACCode)
				{
					waybillPropertyInfo.AddMessageError(USWaybillNumberInvalidErrorMessage);
				}
			}
		}

		public static MultilingualString USWaybillNumberInvalidErrorMessage => ResString.GetMultilingualString("74025f2f-7acc-4772-bd71-e71911928ec0", "Consignment Waybill is greater than {0} characters and does not start with a valid SCAC code.", USWaybillNumberMaxLength_WithoutSCACCode);

		public static MultilingualString USWaybillNumberWithSCACCodeInvalidErrorMessage => ResString.GetMultilingualString("65e8cf96-eab2-42e4-90a2-8c43e5a6a710", "Consignment Waybill that starts with valid SCAC code is greater than {0} characters.", USWaybillNumberMaxLength_WithSCACCode);

		const int USWaybillNumberMaxLength_WithoutSCACCode = 12;
		const int USWaybillNumberMaxLength_WithSCACCode = 16;
	}
}
