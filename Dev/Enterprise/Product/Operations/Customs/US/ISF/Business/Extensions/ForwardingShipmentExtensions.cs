using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public static class ForwardingShipmentExtensions
	{
		public static ZString GetUpperCaseAMSBill(this ForwardingShipment shipment)
		{
			var ams = shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
			return ams?.CE_EntryNum.ToUpper() ?? ZString.Empty;
		}
	}
}
