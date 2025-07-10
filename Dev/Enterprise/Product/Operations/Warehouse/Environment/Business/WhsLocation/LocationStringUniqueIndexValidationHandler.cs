using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	class LocationStringUniqueIndexValidationHandler(string locationStringError) : IUniqueIndexFailureHandler
	{
		const string UniqueLocationStringIndexName = "NR_UC__WhsLocationView_DoNotUse";

		public IEnumerable<string> HandledUniqueIndexNames => [UniqueLocationStringIndexName];

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			if (indexName == UniqueLocationStringIndexName)
			{
				notifier.ReportInformation(
					locationStringError,
					Res.GetString("fb877216-a20b-42eb-b5ec-acb9a5f1a26b", "Duplicate Location Barcode Detected"));
			}
		}
	}
}
