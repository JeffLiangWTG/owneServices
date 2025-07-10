using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public CommonPickupDeliveryConfirmUniqueIndexFailureHandler(CommonPickupDeliveryConfirm commonPickupDeliveryConfirm)
		{
			confirm = commonPickupDeliveryConfirm;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return JobPickupDeliveryConfirmSchema.Constants.Indexes.FK_UX__EU_JC_EU_PickupDeliveryType; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			confirm.Factory.ClearQueryCache();

			var query = new ZDBOnlyQuery(typeof(CommonPickupDeliveryConfirm));
			query.AddToFilter(JobPickupDeliveryConfirmSchema.EU_JC, confirm.EU_JC);
			query.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, confirm.EU_PickupDeliveryType);

			var reloadedConfirm = confirm.Factory.LoadTop1<CommonPickupDeliveryConfirm>(query);
			if (reloadedConfirm != null)
			{
				var container = confirm.Container;
				confirm.Delete();
				container.Confirms.RefreshFromDb();
			}

			var errorMessage = Res.GetString("1a688e58-de14-4959-bc8c-c33613a576b0", "Save Error");

			if (Globals.IsUserInteractive)
			{
				notifier.ReportError(ConflictMessage, errorMessage);
			}
			else
			{
				notifier.ReportInformation(ConflictMessageServiceTask, errorMessage);
			}
		}

		string ConflictMessage => Res.GetString("95fcebf1-ed23-4e69-b82e-f6e87ff87123", "Another user has made changes to the confirmation. Please review your changes and save again.") + "\r\n";
		string ConflictMessageServiceTask => Res.GetString("6d9202f2-2963-48a5-a41b-18bc37fad3c3", "Another user has made changes to the confirmation.") + "\r\n";

		readonly CommonPickupDeliveryConfirm confirm;
	}
}
