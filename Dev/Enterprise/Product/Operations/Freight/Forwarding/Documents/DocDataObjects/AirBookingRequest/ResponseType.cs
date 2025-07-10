namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	enum ResponseType
	{
		Undefined,
		Unsuccessful,
		Empty,
		Invalid,
		InterchangeSent,
		InterchangeRejected,
		InterchangeAccepted,
		BookingConfirmed,
		BookingPending,
		BookingRejected,
		BookingCancelled,
		Rates
	}
}
