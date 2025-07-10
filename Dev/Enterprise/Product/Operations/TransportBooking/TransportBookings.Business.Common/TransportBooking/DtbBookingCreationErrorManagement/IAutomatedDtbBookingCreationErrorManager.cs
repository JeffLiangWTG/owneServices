using System;
using System.Collections.Generic;

namespace Enterprise.TransportBookings.Shared
{
	public interface IAutomatedDtbBookingCreationErrorManager
	{
		void AddError(DtbBookingCreationErrorType errorType, string message = null);
		void AddError(DtbBookingCreationErrorType errorType, Exception exception);
		void ClearErrors();
		IReadOnlyList<DtbBookingCreationError> ErrorList { get; }
		bool Retry { get; }
		bool SendToErrorReporter { get; }
		string MessageToLog { get; }
		string MessageToSendToErrorReporter { get; }
		Exception LastException { get; }
	}
}
