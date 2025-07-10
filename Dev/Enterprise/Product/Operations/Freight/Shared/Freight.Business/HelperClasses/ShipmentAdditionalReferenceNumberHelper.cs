using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class ShipmentAdditionalReferenceNumberHelper
	{
		public static void PopulateCarrierMessageReferenceNumber(CommonShipment shipment, ZString referenceNumber)
		{
			var cmrReferenceNumber = FindCMRAdditionalReferenceNumber(shipment) ?? CreateCMRAdditionalReferenceNumber(shipment);
			cmrReferenceNumber.CE_EntryNum = referenceNumber;
		}

		static CusEntryNumber FindCMRAdditionalReferenceNumber(CommonShipment shipment)
		{
			return shipment.Numbers.Find(num => num.CE_EntryType == ShipmentNonCustomsAdditionalReferenceCodesCodeList.Codes.CMR
				&& num.CE_EntryIsSystemGenerated).FirstOrDefault();
		}

		static CusEntryNumber CreateCMRAdditionalReferenceNumber(CommonShipment shipment)
		{
			var result = shipment.Numbers.AddNew();
			result.CE_EntryType = ShipmentNonCustomsAdditionalReferenceCodesCodeList.Codes.CMR;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty;
			result.CE_EntryIsSystemGenerated = true;

			result.SetReadOnlyIncludingChildren(true);

			return result;
		}
	}
}
