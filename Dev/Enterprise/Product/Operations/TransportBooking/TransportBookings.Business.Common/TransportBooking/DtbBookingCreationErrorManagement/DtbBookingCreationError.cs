using System;

namespace Enterprise.TransportBookings.Shared
{
	public struct DtbBookingCreationError
	{
		public DtbBookingCreationError(DtbBookingCreationErrorType errorType, string message, bool retry, bool sendToErrorReporter, Exception exception)
		{
			ErrorType = errorType;
			Message = message;
			Retry = retry;
			SendToErrorReporter = sendToErrorReporter;
			Exception = exception;
		}

		public DtbBookingCreationErrorType ErrorType { get; }
		public string Message { get; }
		public bool Retry { get; }
		public bool SendToErrorReporter { get; }
		public Exception Exception { get; }
	}
}
