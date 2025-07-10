using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoodsExtensions;
using static Enterprise.Freight.Forwarding.Business.DangerousGoodsExtensions.ForwardingDangerousGoodsExtensions;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class DGStandardCalculator
	{
		/// <summary>
		/// Returns the UNDG Standard most suitable for the shipment.
		/// Checks UNDG Standards that do not have a direct one-to-one mapping onto transport modes.
		/// </summary>
		/// <param name="shipment"></param>
		/// <returns>3 character ZString code representing the UNDG Standard</returns>
		public static ZString GetCorrespondingStandardForShipment(ForwardingShipment shipment)
		{
			if (shipment.IsValidForCFRStandard())
			{
				return UNDGSubstanceStandardTypes.CFR;
			}
			else if (shipment.IsValidForJTTStandard())
			{
				return UNDGSubstanceStandardTypes.JTT;
			}

			return GetCorrespondingStandardForShipmentMode(shipment);
		}

		/// <summary>
		/// Returns the corresponding UNDG Substance Standard for the shipment's transport mode.
		/// </summary>
		/// <param name="shipment"></param>
		/// <returns>3 character ZString code representing the UNDG Standard</returns>
		public static ZString GetCorrespondingStandardForShipmentMode(ForwardingShipment shipment)
		{
			var shipmentTransportMode = shipment?.JS_TransportMode ?? ZString.Empty;

			return GetCorrespondingStandard(shipmentTransportMode);
		}

		internal static ZString GetCorrespondingStandard(ZString transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return UNDGSubstanceStandardTypes.IATA;

				case Core.Constants.TransportModes.Road:
					return UNDGSubstanceStandardTypes.ADR;

				case Core.Constants.TransportModes.Rail:
					return UNDGSubstanceStandardTypes.RID;

				default:
					return UNDGSubstanceStandardTypes.IMO;
			}
		}

		public static IEnumerable<ZString> GetAllValidStandardsForTransport(Transport transport)
		{
			// todo: consider JTT, CFR, ADN here. Needs further clarification from product.
			yield return GetCorrespondingStandard(transport.JW_TransportMode);
		}

		/// <summary>
		/// Contains the UNDG Standards that have direct one-to-one mappings onto transport modes.
		/// </summary>
		public static IReadOnlyCollection<ZString> StandardsWithCorrespondingModes => standardsWithCorrespondingModes
			?? (standardsWithCorrespondingModes = new ZString[]
		{
			UNDGSubstanceStandardTypes.IMO,
			UNDGSubstanceStandardTypes.IATA,
			UNDGSubstanceStandardTypes.ADR,
			UNDGSubstanceStandardTypes.RID
		});

		[ThreadStatic]
		static IReadOnlyCollection<ZString> standardsWithCorrespondingModes;
	}
}
