using System.Globalization;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVItemLookups : AutoHVLVItemLookups
	{
		public HVLVItemLookups(AutoHVLVItem parent)
			: base(parent)
		{ }

		public CodeDescriptionPairList HVI_CarrierBookingStatus_List => Factory.GetCachedValue("ETail|HVI_CarrierBookingStatus_List", () => GetAllHVLVItemCarrierBookingStatuses());

		public CodeDescriptionPairList HVI_Status_List => Factory.GetCachedValue("ETail|HVI_Status_List", () => GetAllHVLVItemStatus());

		public ShipmentCollection HVI_JS_LoadedOnShipment_List
		{
			get
			{
				var filter = new ZQuery();
				filter.AddToFilter(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValue);
				filter.AddToFilter(JobShipmentSchema.JS_IsBooking, ZBool.False);
				return new ShipmentCollection(Factory, filter);
			}
		}

		public HVLVOriginLoadListCollection HVI_HVL_LoadList_List
		{
			get
			{
				var filter = new ZQuery(HVLVOriginLoadListSchema.HVL_Status, HVLVOriginLoadListStatus.Codes.Lodged);
				var shipment = ((HVLVItem)Parent).Shipment;
				if (shipment == null)
				{
					filter.AddToFilter(new ZQuery(HVLVOriginLoadListSchema.HVL_Status, HVLVOriginLoadListStatus.Codes.Open), JoinCondition.Or);
				}

				return new HVLVOriginLoadListCollection(Factory, filter);
			}
		}

		public HVLVOuterPackageCollection HVI_HVO_OuterPackage_List
		{
			get
			{
				return new HVLVOuterPackageCollection(Factory);
			}
		}

		public CodeDescriptionPairList HVI_UnitOfDimensionList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		public CodeDescriptionPairList ContainerNumber_List
		{
			get
			{
				CodeDescriptionPairList result = null;
				var shipment = ((HVLVItem)Parent).Shipment;
				if (shipment != null)
				{
					var cacheKey = string.Format(CultureInfo.InvariantCulture, (NoResString)"Shipment.ContainerNumber_List|{0}", shipment.PK); // Cache key for factory only
					result = Factory.GetCachedValue(cacheKey, () =>
					{
						var list = new CodeDescriptionPairList();
						shipment.Consols.AllContainers.ForEach(c => list.Add(new CodeDescriptionPair(c.JC_ContainerNum.ToString(), c.Consol.JK_UniqueConsignRef)));
						return list;
					});
				}

				return result;
			}
		}

		public static CodeDescriptionPairList GetAllHVLVItemCarrierBookingStatuses()
		{
			var result = new CodeDescriptionPairList
			{
				new CodeDescriptionPair(HVLVItemCarrierBookingStatus.Codes.NotBooked, ResString.GetMultilingualString("94298156-1066-448e-a135-4eec7062bafa", HVLVItemCarrierBookingStatus.Descriptions.NotBooked)),
				new CodeDescriptionPair(HVLVItemCarrierBookingStatus.Codes.BookingRequested, ResString.GetMultilingualString("a7913cc6-8170-43a1-8ed1-9bec6cc04584", HVLVItemCarrierBookingStatus.Descriptions.BookingRequested)),
				new CodeDescriptionPair(HVLVItemCarrierBookingStatus.Codes.BookingConfirmed, ResString.GetMultilingualString("e1a3afe3-e5f5-4bca-920c-72ed51e239d2", HVLVItemCarrierBookingStatus.Descriptions.BookingConfirmed)),
				new CodeDescriptionPair(HVLVItemCarrierBookingStatus.Codes.BookingRejected, ResString.GetMultilingualString("aa577793-cb3b-4427-a59c-469daa212381", HVLVItemCarrierBookingStatus.Descriptions.BookingRejected)),
				new CodeDescriptionPair(HVLVItemCarrierBookingStatus.Codes.BookingCancelled, ResString.GetMultilingualString("a4e8e49e-f794-40e2-b312-f66fd19c62b5", HVLVItemCarrierBookingStatus.Descriptions.BookingCancelled))
			};
			return result;
		}

		public static CodeDescriptionPairList GetAllHVLVItemStatus()
		{
			var result = new CodeDescriptionPairList
			{
				new CodeDescriptionPair(HVLVItemStatus.Codes.ManifestedByETailer, ResString.GetMultilingualString("59e9a324-7a6e-40c9-8a9d-32cba793ad7c", HVLVItemStatus.Descriptions.ManifestedByETailer)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ReceivedAtOriginWithException, ResString.GetMultilingualString("6ce31aae-7be0-442a-a260-bef999276650", HVLVItemStatus.Descriptions.ReceivedAtOriginWithException)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.LoadListAllocated, ResString.GetMultilingualString("6c6068aa-d8d2-44cc-93a6-0e0ffadba958", HVLVItemStatus.Descriptions.LoadListAllocated)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.LoadListLodged, ResString.GetMultilingualString("85a606ed-4b29-4bbc-8563-70d27d2c5a5f", HVLVItemStatus.Descriptions.LoadListLodged)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ShipmentAllocated, ResString.GetMultilingualString("e23afbb6-defe-4a1e-92f0-35a02c981724", HVLVItemStatus.Descriptions.ShipmentAllocated)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ShipmentDeparted, ResString.GetMultilingualString("718569c9-4d36-45b2-8d1b-e3805fc7b13e", HVLVItemStatus.Descriptions.ShipmentDeparted)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ShipmentArrived, ResString.GetMultilingualString("0d071eab-63f2-4624-a872-7d2cc7dff23a", HVLVItemStatus.Descriptions.ShipmentArrived)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ShortShippedAtDestinationDepot, ResString.GetMultilingualString("c79aaaa5-c4ac-4f06-ae4c-ff7fd73fe3ca", HVLVItemStatus.Descriptions.ShortShippedAtDestinationDepot)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.SurplusAtDestinationDepot, ResString.GetMultilingualString("38462d6d-6d58-436f-bc54-70227bbbde51", HVLVItemStatus.Descriptions.SurplusAtDestinationDepot)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, ResString.GetMultilingualString("bea06d55-34c9-40b7-bc2e-1ed9b13f85ff", HVLVItemStatus.Descriptions.PendingClearanceAtDestinationDepot)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.DiscardedAtDestinationDepot, ResString.GetMultilingualString("8cc980b4-2ed5-4c6b-ace4-bf05aaa2e5f4", HVLVItemStatus.Descriptions.DiscardedAtDestinationDepot)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException, ResString.GetMultilingualString("1e798bdd-9c6f-4e59-87d9-e40bc9f43027", HVLVItemStatus.Descriptions.ReceivedAtDestinationDepotWithException)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ReadyForLastMileDelivery, ResString.GetMultilingualString("b9448fcd-60cf-41e3-9b35-52863d83318c", HVLVItemStatus.Descriptions.ReadyForLastMileDelivery)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.SeizedByCustoms, ResString.GetMultilingualString("c82b5493-731d-4819-b5dc-a2e01f1d1299", HVLVItemStatus.Descriptions.SeizedByCustoms)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.DirectedToHeightenedSecurity, ResString.GetMultilingualString("e6fb4154-ac4d-4da3-9a06-31e8753c20f3", HVLVItemStatus.Descriptions.DirectedToHeightenedSecurity)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.DispatchedToLastMileCarrier, ResString.GetMultilingualString("413a9493-1077-4dc4-b790-59a3461916b7", HVLVItemStatus.Descriptions.DispatchedToLastMileCarrier)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.PickedUpByLastMileCarrier, ResString.GetMultilingualString("63338c6a-008b-41cc-9467-c3eafd5901e6", HVLVItemStatus.Descriptions.PickedUpByLastMileCarrier)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ProcessedAtFacility, ResString.GetMultilingualString("2c0a6901-6814-4fc6-8f76-8965c4dc261d", HVLVItemStatus.Descriptions.ProcessedAtFacility)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.OnboardForDelivery, ResString.GetMultilingualString("d8f64313-e376-43e7-9759-9dfdcbc56295", HVLVItemStatus.Descriptions.OnboardForDelivery)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.Delivered, ResString.GetMultilingualString("bb4f03f8-92bb-4ded-96ef-48390fc30b21", HVLVItemStatus.Descriptions.Delivered)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ReturnToSender, ResString.GetMultilingualString("f56c3e8a-d826-42c0-b8fe-a2b40f8e43c2", HVLVItemStatus.Descriptions.ReturnToSender)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.RedeliveryPending, ResString.GetMultilingualString("f5a5eba2-8273-41e7-bd0c-18a1794a4e49", HVLVItemStatus.Descriptions.RedeliveryPending)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.SuspendedByLastMileCarrier, ResString.GetMultilingualString("2e057ed9-a454-4057-861e-2d547b63be3d", HVLVItemStatus.Descriptions.SuspendedByLastMileCarrier)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.DamagedLostStolenByLastMileCarrier, ResString.GetMultilingualString("5f5fbf73-6c84-4c63-9b64-8f4371e4e791", HVLVItemStatus.Descriptions.DamagedLostStolenByLastMileCarrier)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.TrackingCancelledOrDeleted, ResString.GetMultilingualString("a0484d25-64b4-40bf-9a29-4f8a8fad4a30", HVLVItemStatus.Descriptions.TrackingCancelledOrDeleted)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.AllUnknownEvents, ResString.GetMultilingualString("ecce3ad1-b7d8-4784-b147-cfa28f8c9fd1", HVLVItemStatus.Descriptions.AllUnknownEvents)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ProofOfDeliveryReceived, ResString.GetMultilingualString("473cec40-88c2-4f82-b900-d0d0705c6907", HVLVItemStatus.Descriptions.ProofOfDeliveryReceived)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.PickUpFromShipper, ResString.GetMultilingualString("37bd8889-4b3b-45dc-a4f7-569727a0eea3", HVLVItemStatus.Descriptions.PickUpFromShipper)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.ReceivedAtOrigin, ResString.GetMultilingualString("31338787-910a-45b0-8fc4-6072f2a9509d", HVLVItemStatus.Descriptions.ReceivedAtOrigin)),
				new CodeDescriptionPair(HVLVItemStatus.Codes.DeliveryFailure, ResString.GetMultilingualString("ba9da62a-319e-43b4-bd52-75103d8db056", HVLVItemStatus.Descriptions.DeliveryFailure))
			};
			return result;
		}

		public DtbBookingCollection LastMileTransportBooking_List => new DtbBookingCollection(Factory);
	}
}
