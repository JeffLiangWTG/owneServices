using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	class DtbBookingInstructionPkgDivotUniqueIndexHandler : IUniqueIndexFailureHandler
	{
		public DtbBookingInstructionPkgDivotUniqueIndexHandler(IEnumerable<string> uniqueIndexesToHandle)
		{
			UniqueIndexesToHandle = Argument.NotNull(uniqueIndexesToHandle, nameof(uniqueIndexesToHandle));
		}

		readonly IEnumerable<string> UniqueIndexesToHandle;

		IEnumerable<string> IUniqueIndexFailureHandler.HandledUniqueIndexNames
		{
			get { return UniqueIndexesToHandle; }
		}

		void IUniqueIndexFailureHandler.NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var errorMsg = Res.GetString("B12AFD9E-62B0-4827-AAE3-6D10444D1D6E", "Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.");
			var caption = Res.GetString("F3676943-B7A9-41B5-9981-01456F696C97", "Error");
			notifier.ReportError(errorMsg, caption);
		}
	}
}
