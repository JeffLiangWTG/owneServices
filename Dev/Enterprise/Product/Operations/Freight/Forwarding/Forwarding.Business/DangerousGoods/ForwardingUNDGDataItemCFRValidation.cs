using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods
{
	public class ForwardingUNDGDataItemCFRValidation : ForwardingUNDGDataItemValidation
	{
		public ForwardingUNDGDataItemCFRValidation(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		#region CheckDI_DG

		protected override void CheckDI_DG()
		{
			base.CheckDI_DG();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance is null || shipment is null)
			{
				return;
			}

			CheckProhibitions(shipment, substance);
		}

		void CheckProhibitions(ForwardingShipment shipment, UNDGSubstance substance)
		{
			if (!(substance.StandardSubstance is UNDGSubstanceCFR cfrSubstance))
			{
				return;
			}

			var transports = shipment
				.TransportsIncludingRelated
				.Cast<Transport>()
				.ToList();

			switch (cfrSubstance.CFR_Prefix.ToUpper())
			{
				case UNDGSubstanceCFR.Prefixes.NA:
					ValidateForNACFRSubstance(transports);
					break;

				case UNDGSubstanceCFR.Prefixes.ID:
					ValidateForIDCFRSubstance(transports);
					break;

				default:
					ValidateForUNCFRSubstance(transports, shipment);
					break;
			}

			bool isCargoAirRailLimitForbidden = cfrSubstance.CFR_CargoAirRailLimitType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			bool isPassengerAirRailLimitForbidden = cfrSubstance.CFR_PAXAirRailLimitType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			if ((isCargoAirRailLimitForbidden || isPassengerAirRailLimitForbidden)
				&& DoesShipmentHaveRelatedAirLegs(shipment))
			{
				Parent.DI_DGInfo.AddError(Res.GetString("8EE267F1-BF19-4797-B160-DFDD2A1FE956",
					"This substance is forbidden for uplift by Air."));
			}
		}

		void ValidateForNACFRSubstance(IReadOnlyCollection<Transport> transports)
		{
			if (!transports.Any(IsTransportLegSuitableForNACFRSubstances))
			{
				Parent.DI_DGInfo.AddError(Res.GetString("3005CBA3-3BFB-4087-A156-989EDF4F0896",
					"DG substances with NA codes are only allowed for domestic movements within the United States or movements between the United States and Canada."));
			}
		}
		void ValidateForIDCFRSubstance(IReadOnlyCollection<Transport> transportsWithRelated)
		{
			if (!transportsWithRelated.Any(IsTransportLegLoadOrDiscInUSOrUSTerritory))
			{
				Parent.DI_DGInfo.AddError(Res.GetString("24B9162A-AB31-4E0B-BC06-6E5A577C53CB",
					"49 CFR DG Substances with ID codes are only allowed for domestic and international movements from/to the United States."));
			}
		}

		void ValidateForUNCFRSubstance(IReadOnlyCollection<Transport> transportsWithRelated, ForwardingShipment shipment)
		{
			if (!transportsWithRelated.Any(IsTransportLegLoadOrDiscInUSOrUSTerritory)
				&& (shipment.Transports.Count > 0 || !IsShipmentInUSOrUSTerritory(shipment)))
			{
				Parent.DI_DGInfo.AddError(Res.GetString("2D81FB1D-131F-4D62-9935-6E3A08AAB347",
					"49 CFR DG Substances with UN codes are only allowed for domestic and international movements from/to the United States."));
			}
		}

		bool IsTransportLegSuitableForNACFRSubstances(Transport transport)
		{
			var isLoadUSOrUSTerritory = IsPortInUSOrUSTerritory(transport.JW_RL_NKLoadPort);
			var isDiscUSOrUSTerritory = IsPortInUSOrUSTerritory(transport.JW_RL_NKDiscPort);
			var isDomesticUSOrUSTerritory = isLoadUSOrUSTerritory && isDiscUSOrUSTerritory;

			var isBetweenUSAndCanada = (isLoadUSOrUSTerritory || isDiscUSOrUSTerritory)
									&& (transport.JW_RL_NKLoadPort.StartsWith(Constants.CountryCodes.Canada)
									|| transport.JW_RL_NKDiscPort.StartsWith(Constants.CountryCodes.Canada));

			return isDomesticUSOrUSTerritory || isBetweenUSAndCanada;
		}

		bool IsTransportLegLoadOrDiscInUSOrUSTerritory(Transport transport)
		{
			return IsPortInUSOrUSTerritory(transport.JW_RL_NKLoadPort)
				|| IsPortInUSOrUSTerritory(transport.JW_RL_NKDiscPort);
		}

		bool IsShipmentInUSOrUSTerritory(ForwardingShipment shipment)
		{
			return IsPortInUSOrUSTerritory(shipment.JS_RL_NKOrigin)
				|| IsPortInUSOrUSTerritory(shipment.JS_RL_NKDestination);
		}

		bool IsPortInUSOrUSTerritory(string portCode)
		{
			return Constants.CountryCodes.UsaAndTerritoriesList.Any(country => portCode.StartsWith(country));
		}

		#endregion

		#region CheckDI_DGWeight

		protected override void CheckDI_DGWeight()
		{
			base.CheckDI_DGWeight();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance is null || shipment is null)
			{
				return;
			}

			CheckWeight(substance, shipment);
		}

		void CheckWeight(UNDGSubstance substance, ForwardingShipment shipment)
		{
			const string uraniumHexafluorideUnno = "3507";

			if (Parent.DI_IsSalvagePackaging && Parent.DI_DGWeight.IsEmpty && Parent.DI_DGVolume.IsEmpty)
			{
				Parent.DI_DGWeightInfo.AddWarning(Res.GetString("7d26763e-92e2-40e8-8abd-2fd555080588",
					"Either a weight or volume estimate is required for Salvage Packaging."));
			}

			var undgsWithNoAirRailLimit = new ZString[] { "2800", "3164", "3166", "3506", "8000" };
			if (undgsWithNoAirRailLimit.Contains(substance.DG_UNNO))
			{
				Parent.DI_DGWeightInfo.AddWarning(Res.GetString("6F87368F-E9EF-4A6F-902C-5433D97F13B1",
					"When shipping this substance, the gross weight is to be entered."));
			}

			var convertedTemperature =
				Constants.Weight.ConvertSafe(0.1m, Constants.Weight.Kilograms, Parent.DI_UnitOfWeight);
			if (substance.DG_UNNO != uraniumHexafluorideUnno
				|| Parent.DI_UnitOfWeight.IsEmpty
				|| Parent.DI_DGWeight <= convertedTemperature)
			{
				return;
			}

			if (DoesShipmentHaveRelatedPassengerAirLegs(shipment))
			{
				Parent.DI_DGWeightInfo.AddError(Res.GetString("ed9b5b68-da6c-44e6-a803-0a2702fe1f09",
					"This quantity exceeds the maximum allowed for a Passenger Aircraft."));
			}
			else if (DoesShipmentHaveRelatedAirLegs(shipment))
			{
				Parent.DI_DGWeightInfo.AddError(Res.GetString("d4f8ac2a-e848-48d5-af32-c46e4a6b34f0",
					"This quantity exceeds the maximum allowed for a Cargo Aircraft."));
			}
		}

		bool DoesShipmentHaveRelatedAirLegs(ForwardingShipment shipment)
		{
			if (shipment is null)
			{
				return false;
			}

			return shipment.TransportsIncludingRelated.OfType<Transport>()
				.Any(transportLeg => transportLeg.JW_TransportMode == Constants.TransportModes.Air);
		}

		bool DoesShipmentHaveRelatedPassengerAirLegs(ForwardingShipment shipment)
		{
			if (shipment is null)
			{
				return false;
			}

			return shipment.TransportsIncludingRelated.OfType<Transport>()
				.Any(transportLeg => transportLeg.JW_TransportMode == Constants.TransportModes.Air
					&& !transportLeg.JW_IsCargoOnly);
		}

		#endregion

		#region CheckDI_DGVolume

		protected override void CheckDI_DGVolume()
		{
			base.CheckDI_DGVolume();

			CheckVolume();
		}

		void CheckVolume()
		{
			if (Parent.DI_IsSalvagePackaging && Parent.DI_DGWeight.IsEmpty && Parent.DI_DGVolume.IsEmpty)
			{
				Parent.DI_DGVolumeInfo.AddWarning(Res.GetString("ba57f8ab-ed5f-440d-ad45-949d22dbc3f7",
					"Either a weight or volume estimate is required for Salvage Packaging."));
			}
		}

		#endregion

		#region CheckDI_RadioactiveLabelCategory

		protected override void CheckDI_RadioactiveLabelCategory()
		{
			if (Parent.DI_IsHighwayRouteControlledQuantity && Parent.DI_RadioactiveLabelCategory != RadioactiveLabelCategoryList.Codes.YellowIII)
			{
				var resourceString = Res.GetString("a743c66d-70fe-40b9-a2af-665d215734e3",
					"49 CFR Section 172.403 (C) (1) advises any package containing a Highway Route Controlled Quantity must be labeled as RADIOACTIVE YELLOW-III.");
				Parent.DI_RadioactiveLabelCategoryInfo.AddWarning(resourceString);
			}
		}

		#endregion

		#region CheckDI_TechnicalName

		protected override void CheckDI_TechnicalName()
		{
			base.CheckDI_TechnicalName();

			if (!Parent.DI_TechnicalName.IsEmpty)
			{
				return;
			}

			var substance = Parent.Substance?.StandardSubstance as UNDGSubstanceCFR;
			if (substance is null)
			{
				return;
			}

			if (substance.CFR_SpecialProvisions.Split(Array.Empty<char>()).Contains("441"))
			{
				Parent.DI_TechnicalNameInfo.AddMessageError(Res.GetString("D72A506E-600B-4A7F-86CD-6CFF335C2258",
					"Technical Name is required for the Ocean Booking for substances of the IMO Standard with Special Provision 274 and/or 318, and substances of the CFR Standard with Special Provision 441."));
			}
		}

		#endregion
	}
}
