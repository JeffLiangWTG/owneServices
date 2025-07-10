using System;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	static class FinaliseJobFormHelper
	{
		#region FinaliseDocket

		public static void FinaliseDocket<T>(T docket, ZForm form, INotifications notify, Action<T> finaliseAction = null)
			where T : WhsDocket
		{
			using (new DocketNotificationSubscriberWithWaitCursor(docket, notify))
			{
				Finalise(docket, finaliseAction ?? (d => d.FinaliseDocket()), form);
			}
		}

		public static void Finalise<T>(T entity, Action<T> finaliseAction, ZForm form)
			where T : EnterpriseBusinessObject
		{
			using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(form))
			{
				finaliseAction(entity);
			}
		}

		#endregion
	}
}
