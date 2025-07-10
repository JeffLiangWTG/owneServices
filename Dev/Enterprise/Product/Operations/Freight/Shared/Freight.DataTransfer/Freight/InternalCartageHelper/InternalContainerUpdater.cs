using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer
{
	public static class InternalContainerUpdater
	{
		public static void UpdateContainerFromXsd(bool isPickup, CartageJob xsdCartage, CommonContainer container, List<CartageLeg> containerLegs, IValueObjectImportContext context)
		{
			var refInfo = isPickup ? container.JC_DepartureCartageRefInfo : container.JC_ArrivalCartageRefInfo;
			refInfo.SetValueFromString(xsdCartage.JobNumber);

			var demurrageTotal = new TimeSpan();
			foreach (var cartageLeg in containerLegs)
			{
				SetContainerFieldsFromCartageLegXsd(isPickup, container, cartageLeg, context);
				demurrageTotal += GetDemurrage(cartageLeg.CartageLegDates);
			}

			if (demurrageTotal.TotalMinutes > 0)
			{
				var demInfo = isPickup ? container.DepartureTruckWaitTimeInfo : container.ArrivalTruckWaitTimeInfo;
				SetDate(demInfo, ZeroTime + demurrageTotal, context);
			}
		}

		#region SetContainerFieldsFromCartageLegXsd

		static void SetContainerFieldsFromCartageLegXsd(bool isPickup, CommonContainer container, CartageLeg cartageLeg, IValueObjectImportContext context)
		{
			var containerLeg = cartageLeg.Item as CartageLegContainer;
			var dates = cartageLeg.CartageLegDates;
			var info = containerLeg.ContainerAdditionalInfo;

			var isDeliveringToCTO = cartageLeg.Delivery.DocAddress.AddressType == DocAddressAddressType.LCT;
			var isDeliveringToConsignee = cartageLeg.Delivery.DocAddress.AddressType == DocAddressAddressType.LCI;
			var isDeliveringToYard = cartageLeg.Delivery.DocAddress.AddressType == DocAddressAddressType.LCY;

			var isPickingUpFromCTO = cartageLeg.Pickup.DocAddress.AddressType == DocAddressAddressType.LCT;
			var isPickingUpFromConsignor = cartageLeg.Pickup.DocAddress.AddressType == DocAddressAddressType.LCE;
			var isPickingUpFromYard = cartageLeg.Pickup.DocAddress.AddressType == DocAddressAddressType.LCY;

			if (isPickup && isPickingUpFromConsignor)
			{
				var completeDate = (!dates.PickupTimeInDate.IsEmpty) ? dates.PickupTimeInDate : dates.PickupTimeOutDate;
				SetDate(container.JC_DepartureCartageCompleteInfo, completeDate, context);
			}

			if (!isPickup && isDeliveringToConsignee)
			{
				var completeDate = (!dates.DeliverTimeInDate.IsEmpty) ? dates.DeliverTimeInDate : dates.DeliverTimeOutDate;
				SetDate(container.JC_ArrivalCartageCompleteInfo, completeDate, context);
			}

			if (isDeliveringToCTO)
			{
				SetDate(container.JC_FCLWharfGateInInfo, dates.DeliverTimeInDate, context);

				container.JC_DepartureSlotReferenceInfo.SetValueFromString(info.DepartureSlotRef);
				SetDate(container.JC_DepartureSlotDateTimeInfo, info.DepartureSlotDate, context);
			}

			if (isPickingUpFromCTO)
			{
				SetDate(container.JC_FCLWharfGateOutInfo, dates.PickupTimeOutDate, context);

				container.JC_ArrivalSlotReferenceInfo.SetValueFromString(info.ArrivalSlotRef);
				SetDate(container.JC_ArrivalSlotDateTimeInfo, info.ArrivalSlotDate, context);
			}

			if (isPickingUpFromYard)
			{
				SetAddress(container.JC_OA_DepartureContainerYardAddressInfo, cartageLeg.Pickup, context);
				var completeDate = (!dates.PickupTimeOutDate.IsEmpty) ? dates.PickupTimeOutDate : dates.DeliverTimeInDate;
				SetDate(container.JC_ContainerYardEmptyPickupGateOutInfo, completeDate, context);
			}

			if (isDeliveringToYard)
			{
				SetAddress(container.JC_OA_ArrivalContainerYardAddressInfo, cartageLeg.Delivery, context);

				var completeDate = (!dates.DeliverTimeInDate.IsEmpty) ? dates.DeliverTimeInDate : dates.DeliverTimeOutDate;
				SetDate(container.JC_ContainerYardEmptyReturnGateInInfo, completeDate, context);
			}
		}

		#endregion

		#region SetAddress

		static void SetAddress(ZPropertyInfo info, CartageLegDocAddress address, IValueObjectImportContext context)
		{
			var org = context.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, address.DocAddress.AddressReference.Organisation.EDICode);
			if (org != null)
			{
				info.Value = org.MainAddress.PK;
			}
		}

		#endregion

		#region SetDate

		static void SetDate(ZPropertyInfo info, ZDateTime date, IValueObjectImportContext context)
		{
			if (date.IsValid && info != null)
			{
				context.SetPropertyInfoValue(info, date.ToDateTime());
			}
		}

		#endregion

		#region GetDemurrage

		static TimeSpan GetDemurrage(CartageLegDates dates)
		{
			var pickupDemurrage = dates.PickupDemurrage;
			var deliveryDemurrage = dates.DeliveryDemurrage;

			var pickupTimeSpan = !pickupDemurrage.IsEmpty && pickupDemurrage.IsValid ? new TimeSpan(pickupDemurrage.DayOfYear - 1, pickupDemurrage.Hour, pickupDemurrage.Minute, 0) : new TimeSpan();
			var demurrageTimeSpan = !deliveryDemurrage.IsEmpty && deliveryDemurrage.IsValid ? new TimeSpan(deliveryDemurrage.DayOfYear - 1, deliveryDemurrage.Hour, deliveryDemurrage.Minute, 0) : new TimeSpan();

			return pickupTimeSpan + demurrageTimeSpan;
		}

		#endregion

		#region ZeroTime

		static ZDateTime ZeroTime
		{
			get { return new ZDateTime(ZDateTime.Today.Year, 1, 1); }
		}

		#endregion
	}
}
