using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	static class OrderManagerExtensions
	{
		public static ForwardingShipment ClonePlanningShipment(this ForwardingShipment source, ForwardingShipment target)
		{
			target.JS_TransportMode = source.JS_TransportMode;
			target.JS_PackingMode = source.JS_PackingMode;
			target.JS_ShipmentType = source.JS_ShipmentType;
			target.JS_INCO = source.JS_INCO;
			target.JS_GoodsDescription = source.JS_GoodsDescription;
			target.JS_E_DEP = source.JS_E_DEP;
			target.JS_E_ARV = source.JS_E_ARV;
			target.DetailedGoodsDescriptionNoteText = source.DetailedGoodsDescriptionNoteText;
			target.JS_MarksAndNumbers = source.JS_MarksAndNumbers;
			target.JS_RL_NKOrigin = source.JS_RL_NKOrigin;
			target.JS_RL_NKDestination = source.JS_RL_NKDestination;

			target.ConsignorDocumentaryAddress.CopyDocAddressFrom(source.ConsignorDocumentaryAddress);
			target.ControllingCustomerAddress.CopyDocAddressFrom(source.ControllingCustomerAddress);
			target.NotifyPartyDocumentaryAddress.CopyDocAddressFrom(source.NotifyPartyDocumentaryAddress);
			target.NotifyParty2DocumentaryAddress.CopyDocAddressFrom(source.NotifyParty2DocumentaryAddress);
			target.NotifyParty3DocumentaryAddress.CopyDocAddressFrom(source.NotifyParty3DocumentaryAddress);
			target.ConsigneeDocumentaryAddress.CopyDocAddressFrom(source.ConsigneeDocumentaryAddress);
			target.ManufacturerDocAddress.CopyDocAddressFrom(source.ManufacturerDocAddress);
			target.ConsignorPickupAddress.CopyDocAddressFrom(source.ConsignorPickupAddress);
			target.ConsigneeDeliveryAddress.CopyDocAddressFrom(source.ConsigneeDeliveryAddress);

			return target;
		}

		public static ZDecimal CalculatePackLinePrice(this ForwardingShipment shipment, RefCurrency orderCurrency, ZDecimal itemPrice, ZDecimal quantity)
		{
			return (shipment?.GoodsValueCurr != null && orderCurrency != null)
				? (orderCurrency.ConvertUsingSellRate(ZDateTime.Now, itemPrice, shipment.GoodsValueCurr) * quantity)
				: (itemPrice * quantity);
		}

		public static void CopyDocAddressFrom(this JobDocAddress toDocAddress, JobDocAddress fromDocAddress)
		{
			if (fromDocAddress != null && toDocAddress != null)
			{
				if (fromDocAddress.E2_AddressOverride)
				{
					toDocAddress.CopyPersistentValuesFrom(fromDocAddress);
				}
				else
				{
					toDocAddress.OrganisationPK = fromDocAddress.OrganisationPK;
					toDocAddress.ContactPK = fromDocAddress.ContactPK;
					toDocAddress.E2_OA_Address = fromDocAddress.E2_OA_Address;
				}
			}
		}

		static readonly Dictionary<string, string> NonLSEBookingContainerModeMapping = new Dictionary<string, string>()
		{
			{ TransportModes.Sea, ContainerModes.FCL },
			{ TransportModes.Air, ContainerModes.ULD },
			{ TransportModes.Road, ContainerModes.FTL },
			{ TransportModes.Rail, ContainerModes.FCL },
		};

		static readonly Dictionary<string, string> LSEBookingContainerModeMapping = new Dictionary<string, string>()
		{
			{ TransportModes.Sea, ContainerModes.LCL },
			{ TransportModes.Air, ContainerModes.Loose },
			{ TransportModes.Road, ContainerModes.LTL },
			{ TransportModes.Rail, ContainerModes.LCL },
		};

		public static string GetConvertedPackingMode(this JobSupplierBooking booking)
		{
			var convertedPackingMode = "";
			if (booking.JSB_LoadMode == SupplierBookingLoadMode.LooseCargo)
			{
				return LSEBookingContainerModeMapping.TryGetValue(booking.JSB_TransportMode, out convertedPackingMode) ? convertedPackingMode : "";
			}

			return NonLSEBookingContainerModeMapping.TryGetValue(booking.JSB_TransportMode, out convertedPackingMode) ? convertedPackingMode : "";
		}

		public static void SetMarksAndNumbers(this ForwardingShipment shipment, ZString newValue)
		{
			shipment.JS_MarksAndNumbers = newValue.Truncate(shipment.JS_MarksAndNumbersInfo.MaxLength);
		}

		public static void UpdateShipmentMeasuresFromPackLines(this ForwardingShipment shipment)
		{
			var packLines = shipment.OuterPackLines.OfType<ForwardingPackLine>().ToList();
			if (packLines.Any())
			{
				shipment.JS_UnitOfVolume = GetUnitOfLinesOrDefault(packLines, line => line.JL_ActualVolumeUQ, Volume.CubicMetres);
				shipment.JS_UnitOfWeight = GetUnitOfLinesOrDefault(packLines, line => line.JL_ActualWeightUQ, Weight.Kilograms);
				shipment.JS_F3_NKPackType = GetUnitOfLinesOrDefault(packLines, line => line.JL_F3_NKPackType, PkgUnit.Package);
			}
		}

		public static void UpdateOverflowingContainerWeightMeasureUnit(this ForwardingShipment shipment)
		{
			var overflowingContainers = shipment.Containers.Where(c => !c.JC_GrossWeight.IsWithinSqlPrecisionAndScale(9, 3));
			foreach (var container in overflowingContainers)
			{
				var magnitude = container.JC_GrossWeight;
				var unit = container.JC_GrossWeightUQ;

				new WeightConversionStrategy().ReScale(ref magnitude, ref unit, 9, 3);
				container.JC_GrossWeightUQ = unit;
				container.JC_GrossWeight = magnitude;
			}
		}

		public static void UpdateShipmentMeasuresFromBookingLines(this ForwardingShipment shipment, JobSupplierBookingLineCollection supplierBookingLines)
		{
			using (GetDisposableActionWithoutUpdatePackLines(shipment))
			{
				UpdateShipmentMeasureUnit(supplierBookingLines, shipment);

				if (supplierBookingLines.Any())
				{
					shipment.JS_OuterPacks = (ZInt)supplierBookingLines.Sum(supplierBookingLine => supplierBookingLine.JSL_BookedPackages);
					shipment.JS_ActualWeight = supplierBookingLines.Sum(supplierBookingLine => Weight.Convert(supplierBookingLine.JSL_GrossWeight, supplierBookingLine.JSL_GrossWeightUnit, shipment.JS_UnitOfWeight, false));
					shipment.JS_ActualVolume = supplierBookingLines.Sum(supplierBookingLine => Volume.Convert(supplierBookingLine.JSL_Volume, supplierBookingLine.JSL_VolumeUnit, shipment.JS_UnitOfVolume, false));
					shipment.JS_GoodsValue = supplierBookingLines.Sum(supplierBookingLine => supplierBookingLine.OrderLine.JO_ItemPrice * supplierBookingLine.JSL_BookedQuantity);
				}
			}
		}

		public static void UpdateShipmentFromContainerLoadListHeader(this ForwardingShipment shipment, CommonContainerLoadList containerLoadListHeader)
		{
			using (GetDisposableActionWithoutUpdatePackLines(shipment))
			{
				shipment.SetMarksAndNumbers(containerLoadListHeader.CLH_MarksAndNumbers);
				shipment.JS_GoodsDescription = containerLoadListHeader.CLH_GoodsDescription;
				shipment.DetailedGoodsDescriptionNoteText = containerLoadListHeader.CLH_DetailedGoodsDescription;
			}
		}

		public static void UpdateShipmentFromSupplierBooking(this ForwardingShipment shipment, JobSupplierBooking supplierBooking)
		{
			using (GetDisposableActionWithoutUpdatePackLines(shipment))
			{
				shipment.SetMarksAndNumbers(supplierBooking.JSB_MarksAndNumbers);
				shipment.JS_GoodsDescription = supplierBooking.JSB_GoodsDescription;
				shipment.DetailedGoodsDescriptionNoteText = supplierBooking.JSB_DetailedGoodsDescription;
			}
		}

		static void UpdateShipmentMeasureUnit(JobSupplierBookingLineCollection supplierBookingLines, ForwardingShipment shipment)
		{
			shipment.JS_UnitOfVolume = GetUnitOfLinesOrDefault(supplierBookingLines, supplierBookingLine => supplierBookingLine.JSL_VolumeUnit, Volume.CubicMetres);
			shipment.JS_UnitOfWeight = GetUnitOfLinesOrDefault(supplierBookingLines, supplierBookingLine => supplierBookingLine.JSL_GrossWeightUnit, Weight.Kilograms);
			shipment.JS_F3_NKPackType = GetUnitOfLinesOrDefault(supplierBookingLines, supplierBookingLine => supplierBookingLine.JSL_F3_NKBookedPackagesUnit, PkgUnit.Package);
		}

		static ZString GetUnitOfLinesOrDefault<T>(IList<T> lines, Func<T, ZString> getUnitFunc, ZString defaultValue)
		{
			if (lines != null && lines.Any() && lines.All(line => getUnitFunc(line) == getUnitFunc(lines[0])))
			{
				var value = getUnitFunc(lines[0]);
				return value.IsEmpty ? defaultValue : value;
			}

			return defaultValue;
		}

		static DisposableAction GetDisposableActionWithoutUpdatePackLines(ForwardingShipment shipment)
		{
			var originalSuppressPackLinesUpdate = shipment.SuppressPackLinesUpdate;
			return new DisposableAction(
				() => shipment.SuppressPackLinesUpdate = true,
				() => shipment.SuppressPackLinesUpdate = originalSuppressPackLinesUpdate);
		}

		public static bool MatchReferenceParameter(this IQueuedLog log, ZString parameterType, ZString parameterValue)
		{
			return log != null
				&& !string.IsNullOrWhiteSpace(log.Reference)
				&& StmALog.GetParametersFromReference(log.Reference).TryGetValue(parameterType, out var parameter)
				&& parameter == parameterValue;
		}
	}
}
