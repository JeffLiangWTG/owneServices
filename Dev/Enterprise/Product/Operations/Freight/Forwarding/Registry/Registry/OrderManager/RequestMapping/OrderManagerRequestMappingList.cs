using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class OrderManagerRequestMappingList : CodeDescriptionPairList
	{
		public static class Descriptions
		{
			public static MultilingualString CargoDateVsShipmentWindow { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|CargoDateVsShipmentWindow", "Cargo Available Date vs Shipment Window"); } }
			public static MultilingualString CargoDateVsExWorksDate { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|CargoDateVsExWorksDate", "Cargo Available Date vs Ex-Works Date"); } }
			public static MultilingualString CargoDateVsRequiredInStoreDate { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|CargoDateVsRequiredInStoreDate", "Cargo Available Date vs Required In-Store Date"); } }
			public static MultilingualString BookedQuantity { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|BookedQuantity", "Booked Quantity vs Order Quantity"); } }
			public static MultilingualString TransportMode { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|TransportMode", "SBK Transport Mode vs Order Transport Mode"); } }
			public static MultilingualString PortOfLoading { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|PortOfLoading", "SBK Port of Loading vs Order Port of Loading"); } }
			public static MultilingualString Overweight { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|Overweight", "Equipment Overweight"); } }
			public static MultilingualString MinimumVolume { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|MinimumVolume", "Equipment Minimum Volume"); } }
			public static MultilingualString MaximumVolume { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|MaximumVolume", "Equipment Maximum Volume"); } }
			public static MultilingualString PartialReceived { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|PartialReceived", "Receiving Complete while one supplier booking line is not fully received at least"); } }
			public static MultilingualString FinalizeBookingWithOutstandingLines { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|FinalizeBookingWithOutstandingLines", "Supplier booking was finalized while there were outstanding booking lines to be packed."); } }
			public static MultilingualString DispatchShortageForLooseCargo { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|DispatchShortageForLooseCargo", "Not fully picked up or dispatched for Loose Cargo Booking."); } }
			public static MultilingualString PackedQtyExceedsBooked { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|PackedQtyExceedsBooked", "Packed quantity at least one load list line is different from the booked quantity while submitting CY container load list."); } }
			public static MultilingualString BookedQuantityAllocatedPartially { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|BookedQuantityAllocatedPartially", "Booked quantity on one or more supplier booking lines is different from the total packed quantity while submitting CY container load list."); } }
			public static MultilingualString UnusedContainersAllocatedToBooking { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|UnusedContainersAllocatedToBooking", "Unused containers allocated to the supplier booking."); } }
			public static MultilingualString MinimumRequirementOfSeal { get { return ResString.GetMultilingualString("OrderManagerRequestMappingList|MinimumRequirementOfSeal", "Minimum requirement of Seal Number and Sealed By was not met."); } }
		}

		public OrderManagerRequestMappingList()
		{
			AddPair(RequestMappingCodes.CargoDateVsShipmentWindow, Descriptions.CargoDateVsShipmentWindow);
			AddPair(RequestMappingCodes.CargoDateVsExWorksDate, Descriptions.CargoDateVsExWorksDate);
			AddPair(RequestMappingCodes.CargoDateVsRequiredInStoreDate, Descriptions.CargoDateVsRequiredInStoreDate);
			AddPair(RequestMappingCodes.BookedQuantity, Descriptions.BookedQuantity);
			AddPair(RequestMappingCodes.TransportMode, Descriptions.TransportMode);
			AddPair(RequestMappingCodes.PortOfLoading, Descriptions.PortOfLoading);
			AddPair(RequestMappingCodes.Overweight, Descriptions.Overweight);
			AddPair(RequestMappingCodes.MinimumVolume, Descriptions.MinimumVolume);
			AddPair(RequestMappingCodes.MaximumVolume, Descriptions.MaximumVolume);
			AddPair(RequestMappingCodes.PartialReceived, Descriptions.PartialReceived);
			AddPair(RequestMappingCodes.FinalizeBookingWithOutstandingLines, Descriptions.FinalizeBookingWithOutstandingLines);
			AddPair(RequestMappingCodes.DispatchShortageForLooseCargo, Descriptions.DispatchShortageForLooseCargo);
			AddPair(RequestMappingCodes.PackedQtyExceedsBooked, Descriptions.PackedQtyExceedsBooked);
			AddPair(RequestMappingCodes.BookedQuantityAllocatedPartially, Descriptions.BookedQuantityAllocatedPartially);
			AddPair(RequestMappingCodes.UnusedContainersAllocatedToBooking, Descriptions.UnusedContainersAllocatedToBooking);
			AddPair(RequestMappingCodes.MinimumRequirementOfSeal, Descriptions.MinimumRequirementOfSeal);
		}
	}
}
