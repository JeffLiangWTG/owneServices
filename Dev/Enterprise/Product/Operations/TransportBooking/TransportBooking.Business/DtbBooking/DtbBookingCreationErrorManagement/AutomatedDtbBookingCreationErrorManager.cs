using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business
{
	class AutomatedDtbBookingCreationErrorManager : IAutomatedDtbBookingCreationErrorManager
	{
		public bool Retry => errorList.Any() && errorList.All(e => e.Retry);

		public bool SendToErrorReporter => errorList.Any(e => e.SendToErrorReporter);

		public string MessageToLog => string.Join(System.Environment.NewLine, errorList.Select(e => e.Message));

		public string MessageToSendToErrorReporter => string.Join(System.Environment.NewLine, errorList.Where(e => e.SendToErrorReporter).Select(e => e.Message));

		public Exception LastException => errorList.Select(e => e.Exception).LastOrDefault(e => e != null);

		public IReadOnlyList<DtbBookingCreationError> ErrorList => errorList.AsReadOnly();

		public void AddError(DtbBookingCreationErrorType errorType, string message)
		{
			AddError(errorType, message, null);
		}

		public void AddError(DtbBookingCreationErrorType errorType, Exception exception)
		{
			AddError(errorType, exception.Message, exception);
		}

		void AddError(DtbBookingCreationErrorType errorType, string message, Exception exception)
		{
			bool errorRetry = false;
			bool errorSendToErrorReporter = false;
			switch (errorType)
			{
				case DtbBookingCreationErrorType.ZSaveConcurrencyError:
				case DtbBookingCreationErrorType.ZCannotSaveError:
				case DtbBookingCreationErrorType.DuplicateBookingConsolidationError:
					errorRetry = true;
					break;

				case DtbBookingCreationErrorType.UnknownError:
					errorRetry = true;
					errorSendToErrorReporter = true;
					break;

				default:
					break;
			}

			errorList.Add(new DtbBookingCreationError(errorType, message, errorRetry, errorSendToErrorReporter, exception));
		}

		public void ClearErrors()
		{
			errorList.Clear();
		}

		readonly List<DtbBookingCreationError> errorList = new List<DtbBookingCreationError>();
	}
}
