using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	/// <summary>
	/// Extracted code out of many Whs forms and consolidated here.
	/// Use an instance of this class when implementing INotifications.
	/// Tested by consumers (forms).
	/// </summary>
	public class WhsNotificationsHelper
	{
		public WhsNotificationsHelper(BusinessObject dataSource)
		{
			DataSource = dataSource;
		}

		readonly BusinessObject DataSource;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sniffer dazed and confused, Hard-coded constant")]
		public void Add(INotification notification)
		{
			ZErrorMessageBox msgBox = null;
			try
			{
				INotificationSubscriberType type = notification.Type as INotificationSubscriberType;
				if (type != null && type.Name == "ZErrorMessageBox")
				{
					if (type.Message == "Pick")
					{
						msgBox = new ZErrorMessageBox(DataSource, DataSource.HumanReadableName.Trim(), type.Message, "Picked");
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					}
					else if (type.Message == "Finalise" || type.Message == "Finalize")
					{
						msgBox = new ZErrorMessageBox(DataSource, DataSource.HumanReadableName.Trim(), Res.GetString("829a69da-2bfc-4a1b-b4b6-134207ed727a", "Finalize"), Res.GetString("449d3852-69f5-4189-aa35-2cd5c8dd278d", "Finalized"));
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					}
					else if (type.Message == "Putaway")
					{
						msgBox = new ZErrorMessageBox(DataSource, (NoResString)"Receipt", Res.GetString("7083edc2-e655-440d-bdcc-ef8cf5e5f17b", "Putaway"), Res.GetString("7083edc2-e655-440d-bdcc-ef8cf5e5f17b", "Putaway"));
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					}
					else
					{
						msgBox = new ZErrorMessageBox(DataSource);
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					}
				}
				else if (notification.Type is ErrorType)
				{
					Globals.Message.ShowError(notification.Message);
				}
				else
				{
					Globals.Message.Show(notification.Message);
				}
			}
			finally
			{
				if (msgBox != null)
				{
					msgBox.Dispose();
				}
			}
		}
	}
}
