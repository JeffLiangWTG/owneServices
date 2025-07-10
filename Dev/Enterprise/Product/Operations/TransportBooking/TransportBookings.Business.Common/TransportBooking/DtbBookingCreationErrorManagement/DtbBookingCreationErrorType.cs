namespace Enterprise.TransportBookings.Shared
{
	public enum DtbBookingCreationErrorType
	{
		UnknownError,
		ZSaveConcurrencyError,
		ZCannotSaveError,
		ServiceHasCommencedError,
		ConsolidationCannotBeOverriddenAgainError,
		CancelledParentError,
		InvalidParentError,
		UniversalEventDeliveryFailure,
		UniversalResultError,
		DuplicateBookingConsolidationError,
		ExistingBookingIsSubError,
	}
}
