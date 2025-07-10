namespace Enterprise.Freight.Forwarding.Business
{
	public static class CertifiedPickupConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class DocumentNames
		{
			public const string AcceptDecline = "Certified Pickup - Accept/Decline";
			public const string Transfer = "Certified Pickup - Transfer";
			public const string Revoke = "Certified Pickup - Revoke";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class ParameterTypes
		{
			public const string ContainerRelease = "Container Release";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class ParameterMessageTypes
		{
			public const string AcceptDecline = DocumentNames.AcceptDecline;
			public const string Transfer = DocumentNames.Transfer;
			public const string Revoke = DocumentNames.Revoke;
			public const string ReleaseRight = "Certified Pickup - ReleaseRight";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class EventStatus
		{
			public const string Transferred = "Transferred";
			public const string RevokedByPreviousParty = "RevokedByPreviousParty";
			public const string DeclinedByNextParty = "DeclinedByNextParty";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class Status
		{
			public const string Assigned = "Assigned";
			public const string TransferSentAwaitingResponse = "TransferSentAwaitingResponse";
			public const string TransferSent = "TransferSent";
			public const string Accepted = "Accepted";
			public const string Revoked = "Revoked";
			public const string DeclinedByNextPartyForAcceptDecline = "DeclinedByNextPartyForAcceptDecline";
			public const string DeclinedByNextPartyForTransferRevoke = "DeclinedByNextPartyForTransferRevoke";
			public const string DeclinedByOtherReason = "DeclinedByOtherReason";
			public const string NotApplicable = "NotApplicable";
		}

		public static class ContainerEventParameter
		{
			public const string EventCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageSubType;

			public static class Values
			{
				public const string TransferSentAwaitingResponse = Status.TransferSentAwaitingResponse;
			}
		}
	}
}
