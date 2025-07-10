using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ShipmentMeasurementsHelper
	{
		public static string GetMeasurements(params ForwardingShipment[] shipments)
		{
			Argument.NotNull(shipments, nameof(shipments));

			var builder = new StringBuilder();
			var isFirstShipment = true;

			foreach (var shipment in shipments)
			{
				if (shipment == null)
				{
					continue;
				}

				if (isFirstShipment)
				{
					isFirstShipment = false;
				}
				else
				{
					builder.Append("\n\n");
				}

				GetMeasurementsOfShipment(shipment, builder);
			}

			builder.Append("\n\n");
			builder.AppendFormat(CultureInfo.CurrentCulture, (NoResString)"Totals: {0}", GetTotals(false, false, shipments));

			return builder.Replace(" ", "\u2007").ToString();
		}

		public static string GetTotals(bool includeChargeable, bool includeLabels, params ForwardingShipment[] shipments)
		{
			var shipmentsInTotals = GetIncludedShipmentsInTotals(shipments);
			var packLines = shipmentsInTotals.SelectMany(s => s.OuterPackLines).Cast<ForwardingPackLine>().ToArray();

			var firstPack = packLines.FirstOrDefault();
			var firstPacksUnit = firstPack?.JL_F3_NKPackType ?? "PKG";
			var totalPacksUnit = packLines.All(packLine => packLine.JL_F3_NKPackType.Equals(firstPacksUnit))
				? firstPacksUnit.ToString()
				: "PKG";

			var totalPacks = packLines.Sum(packLine => packLine.JL_PackageCount);

			var firstWeightUnit = packLines.FirstOrDefault()?.JL_ActualWeightUQ ?? "KG";
			var totalWeightUnit = packLines.All(packLine => packLine.JL_ActualWeightUQ.Equals(firstWeightUnit))
				? firstWeightUnit.ToString()
				: "KG";
			var totalWeight = packLines.Sum(packLine => Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, totalWeightUnit, false));

			var firstVolumeUnit = packLines.FirstOrDefault()?.JL_ActualVolumeUQ ?? "M3";
			var totalVolumeUnit = packLines.All(packLine => packLine.JL_ActualVolumeUQ.Equals(firstVolumeUnit))
				? firstVolumeUnit.ToString()
				: "M3";
			var totalVolume = packLines.Sum(packLine => Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, totalVolumeUnit, false));

			var packs = string.Format(CultureInfo.CurrentCulture, "{0}{1} {2}", includeChargeable ? Res.GetString("da7b2655-08a6-4a89-8a48-95f9f1afe9e5", "Packs: ") : "", totalPacks, totalPacksUnit);
			var weight = string.Format(CultureInfo.CurrentCulture, "{0}{1:0.00} {2}", includeChargeable ? Res.GetString("1264ce38-0cbc-426a-a739-f2e3c0949093", "Weight: ") : "", totalWeight, totalWeightUnit);
			var volume = string.Format(CultureInfo.CurrentCulture, "{0}{1:0.000} {2}", includeChargeable ? Res.GetString("ee4c2161-0d9c-4edd-b22e-30b25a655b62", "Volume: ") : "", totalVolume, totalVolumeUnit);

			var chargeable = GetTotalChargeable(shipmentsInTotals, includeLabels);

			return includeChargeable
				? $"{packs,-15} {weight,25} {volume,25} {chargeable,25}"
				: $"{packs,-39} {weight,20} {volume,14}";
		}

		static string GetTotalChargeable(ForwardingShipment[] shipments, bool includeLabel)
		{
			var chargeLabel = includeLabel ? Res.GetString("d5819c60-e1f2-4442-b96a-2b480ec81bb2", "Chargeable: ") : string.Empty;

			if (shipments.Length == 0)
			{
				return chargeLabel + (NoResString)"0.000 KG";
			}

			var hasWeightChargeable = shipments.Any(s =>
				FreightDataRegistry.Instance.WeightChargableTransportModes.Contains((string)s.JS_TransportMode));

			var hasVolumeChargeable = shipments.Any(s =>
				FreightDataRegistry.Instance.VolumeChargableTransportModes.Contains((string)s.JS_TransportMode));

			var unit = hasWeightChargeable && !hasVolumeChargeable
				? Constants.Weight.Kilograms
				: !hasWeightChargeable && hasVolumeChargeable
					? Constants.Volume.CubicMetres
					: string.Empty;

			if (string.IsNullOrEmpty(unit))
			{
				return chargeLabel + "N/A";
			}

			var chargeable = shipments.Sum(s => hasWeightChargeable
				? Constants.Weight.Convert(s.JS_ActualChargeable, s.JS_ChargeableUnit, unit, false)
				: Constants.Volume.Convert(s.JS_ActualChargeable, s.JS_ChargeableUnit, unit, false));

			return string.Format(CultureInfo.CurrentCulture, "{0}{1:0.000} {2}", chargeLabel, chargeable, unit);
		}

		static ForwardingShipment[] GetIncludedShipmentsInTotals(ForwardingShipment[] shipments)
		{
			return shipments
				.Where(shipment => shipment != null && ExcludeAllLevelMasterShipments(shipments, (ForwardingShipment)shipment.CoLoadMasterShipment))
				.ToArray();
		}

		static bool ExcludeAllLevelMasterShipments(ForwardingShipment[] shipments, ForwardingShipment masterLeadShipment)
		{
			return masterLeadShipment == null
					|| IsLeadShipment(masterLeadShipment)
					|| (shipments.All(s => s.PK != masterLeadShipment.PK)
						&& ExcludeAllLevelMasterShipments(shipments, (ForwardingShipment)masterLeadShipment.CoLoadMasterShipment));
		}

		static bool IsMasterShipment(ForwardingShipment shipment)
		{
			return shipment.IsCoLoadMaster || shipment.IsBlindCoLoadMaster || shipment.IsAssemblyMaster;
		}

		static bool IsLeadShipment(ForwardingShipment shipment)
		{
			return shipment.IsBuyersConsolLead || shipment.IsShippersConsolLead;
		}

		static void GetMeasurementsOfShipment(ForwardingShipment shipment, StringBuilder builder)
		{
			builder.Append(shipment.JS_UniqueConsignRef);
			if (shipment.CoLoadMasterShipment != null)
			{
				var masterLeadShipment = (ForwardingShipment)shipment.CoLoadMasterShipment;
				builder.AppendFormat(CultureInfo.CurrentCulture,
(NoResString)" (This is a sub-shipment of {0} {1} {2}, Consol# {3})",
					masterLeadShipment.JS_ShipmentType,
					IsMasterShipment(masterLeadShipment) ? (NoResString)"Master" : (NoResString)"Lead",
					masterLeadShipment.JS_UniqueConsignRef,
					shipment.JS_JK_ConsolID);
			}
			else if (IsLeadShipment(shipment))
			{
				builder.AppendFormat(CultureInfo.CurrentCulture, (NoResString)" (This is a lead shipment of {0} type)", shipment.JS_ShipmentType);
			}
			builder.Append("\n");

			if (shipment.OuterPackLines.Count == 0)
			{
				builder.Append("-");
			}
			else
			{
				var firstPackline = true;

				foreach (var packline in shipment.OuterPackLines.Cast<ForwardingPackLine>())
				{
					if (firstPackline)
					{
						firstPackline = false;
					}
					else
					{
						builder.Append("\n");
					}

					GetMeasurementsOfPackline(packline, builder);
				}
			}
		}

		static void GetMeasurementsOfPackline(ForwardingPackLine packline, StringBuilder builder)
		{
			var packs = string.Format(CultureInfo.CurrentCulture, "{0} {1}", packline.JL_PackageCount, packline.JL_F3_NKPackType);
			var weight = string.Format(CultureInfo.CurrentCulture, "{0:0.00} {1}", packline.JL_ActualWeight, packline.JL_ActualWeightUQ);
			var volume = string.Format(CultureInfo.CurrentCulture, "{0:0.000} {1}", packline.JL_ActualVolume, packline.JL_ActualVolumeUQ);
			var dimensions = string.Format(CultureInfo.CurrentCulture, (NoResString)"{0:0.00} x {1:0.00} x {2:0.00} {3}", packline.JL_Length, packline.JL_Width, packline.JL_Height, packline.JL_UnitOfDimension); // Non translatable string
			builder.AppendFormat(CultureInfo.CurrentCulture, "{0,10} {1,42} {2,13} {3,14}", packs, dimensions, weight, volume);
		}
	}
}
