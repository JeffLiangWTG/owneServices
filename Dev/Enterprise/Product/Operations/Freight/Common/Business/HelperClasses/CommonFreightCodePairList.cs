
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public static class CommonFreightCodePairLists
	{
		public static CodeDescriptionPairList CartageJobBookingLookupList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddRange(CartageJobFWDBookingActionList());
			result.AddRange(CartageJobBookingActionList());
			result.AddRange(CartageJobBookingStatusList());
			return result;
		}

		public static CodeDescriptionPairList CartageJobFWDBookingActionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.PreBookingAdvice, CommonFreightConstants.LocalCartageBookingStatus.Description.PreBookingAdvice);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest, CommonFreightConstants.LocalCartageBookingStatus.Description.FirmBookingRequest);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.BookingModificationRequest, CommonFreightConstants.LocalCartageBookingStatus.Description.BookingModificationRequest);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.BookingCancellationRequest, CommonFreightConstants.LocalCartageBookingStatus.Description.BookingCancellationRequest);
			return result;
		}

		public static CodeDescriptionPairList CartageJobBookingActionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.BookingAccepted, CommonFreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.BookingRejected, CommonFreightConstants.LocalCartageBookingStatus.Description.BookingRejected);
			return result;
		}

		public static CodeDescriptionPairList CartageJobBookingStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.WorkCommenced, CommonFreightConstants.LocalCartageBookingStatus.Description.WorkCommenced);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.GoodsReceivedOnBoard, CommonFreightConstants.LocalCartageBookingStatus.Description.GoodsReceivedOnBoard);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.GoodsUnloadedDelivered, CommonFreightConstants.LocalCartageBookingStatus.Description.GoodsUnloadedDelivered);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.WorkCompleted, CommonFreightConstants.LocalCartageBookingStatus.Description.WorkCompleted);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.DemurrageEventAtWharf, CommonFreightConstants.LocalCartageBookingStatus.Description.DemurrageEventAtWharf);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.DemurrageEventAtCFSDepot, CommonFreightConstants.LocalCartageBookingStatus.Description.DemurrageEventAtCFSDepot);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.DemurrageEventAtPickupPoint, CommonFreightConstants.LocalCartageBookingStatus.Description.DemurrageEventAtPickupPoint);
			result.AddPair(CommonFreightConstants.LocalCartageBookingStatus.Codes.SlotDateBooked, CommonFreightConstants.LocalCartageBookingStatus.Description.SlotDateBooked);
			return result;
		}
	}
}
