using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	// tested where used, for example PkgPackageItemDivotTest.TestUniqueIndexFailureHandler
	public class PackingUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public PackingUniqueIndexFailureHandler(IEnumerable<string> uniqueIndexesToHandle)
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
			var errorMsg = Res.GetString("8DADF98F-EA16-4E9D-9E2E-D86ADA2653E5", "Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.");
			var caption = Res.GetString("63BAB0C7-CBB1-426F-BDD2-A21BEA7F59E9", "Error");
			notifier.ReportError(errorMsg, caption);
		}
	}
}
