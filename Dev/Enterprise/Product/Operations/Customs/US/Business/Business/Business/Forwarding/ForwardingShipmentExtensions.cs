using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class ForwardingShipmentExtensions
	{
		public static List<KeyValuePair<ZString, ZInt>> GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(this ForwardingShipment shipment, ForwardingConsol consol)
		{
			var result = new List<KeyValuePair<ZString, ZInt>>();
			foreach (ForwardingContainer container in shipment.GetUniqueContainersFromConsol(consol))
			{
				if (!container.HasNewMBOL)
				{
					var containerIT = container.GetEffectiveITNumber(shipment, consol);
					if (!containerIT.IsEmpty)
					{
						var itNo = result.Find(x => x.Key.EqualsIgnoringCase(containerIT));
						if (itNo.Equals(default(KeyValuePair<ZString, ZInt>)))
						{
							itNo = new KeyValuePair<ZString, ZInt>(containerIT, container.GetNoOfPacksOnSpecificShipment(shipment));
						}
						result.Add(itNo);
					}
				}
			}
			if (result.Count == 0)
			{
				result.Add(new KeyValuePair<ZString, ZInt>(shipment.GetEffectiveITNumber(consol), shipment.JS_OuterPacks));
			}
			return result;
		}

		public static ZString GetEffectiveITNumber(this ForwardingShipment shipment, ForwardingConsol consol)
		{
			var result = shipment.ITNumber;
			if (result.IsEmpty)
			{
				var consolITNumber = consol != null ? consol.Numbers.GetFirstReferenceNumberByType(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT) : null;
				result = consolITNumber != null ? consolITNumber.CE_EntryNum : ZString.Empty;
			}
			return result;
		}

		public static IEnumerable<ForwardingContainer> GetUniqueContainersFromConsol(this ForwardingShipment shipment, ForwardingConsol consol)
		{
			var result = new List<ForwardingContainer>();
			if (consol != null)
			{
				var consolShipments = shipment.GetShipmentsForBill();
				foreach (var consolShipment in consolShipments)
				{
					foreach (PackLine packLine in consolShipment.OuterPackLines)
					{
						var container = packLine.GetContainer(consol) as ForwardingContainer;
						if (container != null && !result.Contains(container))
						{
							result.Add(container);
						}
					}
				}
			}
			return result;
		}

		public static IEnumerable<ZString> GetValidHouseBillIssuerCodes(this ForwardingShipment shipment)
		{
			var validIssuerCodes = new List<ZString>();
			if (shipment is IBillDetails billDetails)
			{
				foreach (var issuer in billDetails.SCACIssuers.Distinct())
				{
					var carrierCode = issuer.USLocalCustomsCarrierCode(shipment.IsRoad);
					if (!carrierCode.IsEmpty)
					{
						validIssuerCodes.Add(carrierCode);
					}
				}
			}

			return validIssuerCodes;
		}
	}
}
