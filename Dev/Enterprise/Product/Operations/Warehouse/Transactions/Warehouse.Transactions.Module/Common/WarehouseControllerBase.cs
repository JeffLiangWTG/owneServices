using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WhsControllerBase : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		INotificationSubscriberQueryUser WhsNotificationSubscriberGuiHelper => whsNotificationSubscriberGuiHelper ?? (whsNotificationSubscriberGuiHelper = new NotificationSubscriberGuiHelper());

		INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper;

		protected WhsControllerBase()
		{
		}

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			return GetForm(businessEntity, WhsNotificationSubscriberGuiHelper);
		}

		protected abstract IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser notificationSubscriber);

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			var userDeleteCondition = sourceEntity as ICanDelete;

			if ((userDeleteCondition == null) || userDeleteCondition.CanDelete)
			{
				result = base.ShowDeleteForm(sourceEntity);
			}
			else
			{
				Globals.Message.Show(userDeleteCondition.ReasonForNotAbleToDelete);
			}

			return result;
		}
	}
}
