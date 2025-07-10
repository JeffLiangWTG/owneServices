using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class ShipmentExtensions
	{
		public static bool IsDoorPickup(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return false;
			}

			return shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_CFS
				|| shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_CY
				|| shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_PORT
				|| shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
		}

		public static bool IsDoorDelivery(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return false;
			}

			return shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CFS_DOOR
				|| shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.CY_DOOR
				|| shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.PORT_DOOR
				|| shipment.JS_HBLContainerPackModeOverride == Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
		}

		public static ZString GetCarrierBookingReference(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return ZString.Empty;
			}

			var contractNumbers = shipment.Numbers.Cast<CusEntryNumber>()
				.Where(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG).ToArray();

			return string.Join(", ", contractNumbers.Select(o => o.CE_EntryNum));
		}

		public static ZString GetCarrierContractNumber(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return ZString.Empty;
			}

			var result = ZString.Empty;
			var contractNumbers = shipment.Numbers.Cast<CusEntryNumber>()
				.Where(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON).ToArray();

			return string.Join(", ", contractNumbers.Select(o => o.CE_EntryNum));
		}

		public static ZString GetGoodsHandlingInstructions(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return ZString.Empty;
			}

			var notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			var result = new StringBuilder();

			foreach (var note in notes)
			{
				result.AppendLine(note.ST_NoteDataAsText);
			}

			return result.ToString();
		}

		public static ZString GetGoodsDescriptionWithFallback(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return ZString.Empty;
			}

			return shipment.DetailedGoodsDescriptionNoteText.IsEmpty
					? shipment.JS_GoodsDescription
					: shipment.DetailedGoodsDescriptionNoteText;
		}

		public static ZString GetCarrierCodeWithFallback(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return ZString.Empty;
			}

			return shipment.GetCarrierWithFallback()?.GetUSCCCOrFallbackToC1C() ?? ZString.Empty;
		}

		public static OrgHeader GetCarrierWithFallback(this ForwardingShipment shipment)
		{
			var consol = shipment.Consols?.Cast<ForwardingConsol>().FirstOrDefault(c => c.CreditorIsNVOCC);
			return consol?.Creditor ?? shipment.BookedShippingLine;
		}

		static ZString GetUSCCCOrFallbackToC1C(this OrgHeader carrier)
		{
			var customsCodes = carrier.CustomsCodes;
			var usCCC = customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Constants.CountryCodes.UnitedStates)?.OK_CustomsRegNo ?? ZString.Empty;
			if (!usCCC.IsEmpty)
			{
				return usCCC;
			}

			var c1c = customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, ZString.Empty)?.OK_CustomsRegNo ?? ZString.Empty;
			return c1c;
		}
	}
}
