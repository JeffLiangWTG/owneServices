using CargoWise.ComponentModel;
using Enterprise.ProductionRules.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class WhsReceiveUserControl : ZUserControl, INotifications
	{
		public WhsReceiveUserControl()
		{
			InitializeComponent();
		}

		void ProductionRulesEngineLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProductionRulesEngineLinkManager.HandleProductionRulesEngineLinkClick(this, ProductionRuleSetAliases.ProductWarehousePutaway);
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		void Notify(INotification notification)
		{
			if (notification.Type.EnumValueName == NotificationType.Information.EnumValueName)
			{
				Globals.Message.ShowInformation(notification.Message);
			}
			else if (notification.Type.EnumValueName == NotificationType.Warning.EnumValueName)
			{
				Globals.Message.ShowWarning(notification.Message);
			}
			else
			{
				Globals.Message.ShowError(notification.Message);
			}
		}

		#endregion
	}
}
