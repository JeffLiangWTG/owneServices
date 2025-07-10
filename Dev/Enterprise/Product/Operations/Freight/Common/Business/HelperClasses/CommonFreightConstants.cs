namespace Enterprise.Freight.Common.Business
{
	public static class CommonFreightConstants
	{
		#region LocalCartageBookingStatus

		public static class LocalCartageBookingStatus
		{
			public static class Codes
			{
				public const string PreBookingAdvice = "PBA";
				public const string FirmBookingRequest = "FBR";
				public const string BookingModificationRequest = "BMR";
				public const string BookingCancellationRequest = "BCR";

				public const string BookingAccepted = "ACC";
				public const string BookingRejected = "REJ";

				public const string WorkCommenced = "WKC";
				public const string GoodsReceivedOnBoard = "GOB";
				public const string GoodsUnloadedDelivered = "GOD";
				public const string WorkCompleted = "WKD";
				public const string DemurrageEventAtWharf = "DAW";
				public const string DemurrageEventAtCFSDepot = "DAC";
				public const string DemurrageEventAtPickupPoint = "DAP";
				public const string SlotDateBooked = "SDB";
			}

			public static class Description
			{
				public static string PreBookingAdvice
				{
					get { return Res.GetString("5e1005ac-5e0d-4231-98fb-298281cfc856", "Pre Booking Advice"); }
				}
				public static string FirmBookingRequest
				{
					get { return Res.GetString("5e48c8e9-b22f-435e-ac72-483ed1478cca", "Firm Booking Request"); }
				}
				public static string BookingModificationRequest
				{
					get { return Res.GetString("13a2aa2b-7e1b-4e9f-bb7a-d8f9fbc9dca0", "Booking Modification Request"); }
				}
				public static string BookingCancellationRequest
				{
					get { return Res.GetString("0a5879ff-4ebc-46b5-a5ad-c4a575748722", "Booking Cancellation Request"); }
				}

				public static string BookingAccepted
				{
					get { return Res.GetString("39f83986-4154-4c75-8b75-32b9059ca3d9", "Booking Request Accepted"); }
				}
				public static string BookingRejected
				{
					get { return Res.GetString("d03a9784-1ec1-433e-afd4-4c5e3c133662", "Booking Request Rejected"); }
				}

				public static string WorkCommenced
				{
					get { return Res.GetString("6f762ed0-61d4-4d8b-a37e-727eb7a5da48", "Work commenced"); }
				}
				public static string GoodsReceivedOnBoard
				{
					get { return Res.GetString("c5c9992a-bb21-4825-b479-4926f17a16dc", "Goods received on board"); }
				}
				public static string GoodsUnloadedDelivered
				{
					get { return Res.GetString("228240f9-22f4-4b26-ac80-b3bea5751a04", "Goods unloaded/delivered"); }
				}
				public static string WorkCompleted
				{
					get { return Res.GetString("0e2df062-24e1-4f32-ae66-3e8bb9f2c0d6", "Work Completed/Container De-hired"); }
				}
				public static string DemurrageEventAtWharf
				{
					get { return Res.GetString("33ea1f40-ab42-4470-a768-d1e93764db39", "Demurrage Event at Wharf"); }
				}
				public static string DemurrageEventAtCFSDepot
				{
					get { return Res.GetString("8947dc77-69de-4f7c-9495-2cceb2bdcef1", "Demurrage Event at CFS/depot"); }
				}
				public static string DemurrageEventAtPickupPoint
				{
					get { return Res.GetString("87305487-d0c7-493c-ae63-98789cddcc9d", "Demurrage Event at Pickup point"); }
				}
				public static string SlotDateBooked
				{
					get { return Res.GetString("2cf893b5-fefa-43b5-8add-de8c78fcfdb7", "Slot Date Booked"); }
				}
			}
		}

		#endregion
	}
}
