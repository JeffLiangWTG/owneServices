using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Confirmations.Module
{
	public class ConfirmationsPlugin : ZPlugIn, INotifications, INotificationSubscriberQueryUser, IConfirmationPlugin
	{
		public ConfirmationsPlugin(IConfirmationsHost host)
			: base((IBusiness)host)
		{
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}

		#endregion

		#region INotificationSubscriberQueryUser Members

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			QueryUserMsgBoxEventArgs msgBoxArgs = e as QueryUserMsgBoxEventArgs;
			if (msgBoxArgs != null)
			{
				msgBoxArgs.Response = Globals.Message.Show(msgBoxArgs.Message, msgBoxArgs.Caption, MessageBoxButtons.YesNo, (msgBoxArgs.Response ? DialogResult.Yes : DialogResult.No)) == DialogResult.Yes;
			}
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Business Entity

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Shipment;
		}

		#endregion

		#region User Control

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			switch (strategy)
			{
				case ConfirmationType.None:
					throw new InvalidOperationException("ConfirmationPlugin.SetStrategy has not been set. ");
				case ConfirmationType.OriginPickup:
					return new ShipmentPickupConfirmControl();
				case ConfirmationType.DestinationDelivery:
					return new ShipmentDeliveryConfirmControl();
				default:
					throw new NotSupportedException(string.Format("Strategy '{0}' not supported for ConfirmationPlugin.GetNewUserControl", strategy));
			}
		}

		#endregion

		CommonShipment Shipment
		{
			get { return (CommonShipment)HostBusinessEntity; }
		}

		public override string Name
		{
			get { return (NoResString)"Confirmations Plugin"; }
		}

		#region IConfirmationPlugin Members

		public void SetStrategy(ConfirmationType strategy, string tabPageCaption)
		{
			this.strategy = strategy;
			this.tabPageCaption = tabPageCaption;
		}
		ConfirmationType strategy = ConfirmationType.None;

		#endregion

		protected override string TextOverride
		{
			get { return string.IsNullOrEmpty(tabPageCaption) ? base.TextOverride : tabPageCaption; }
		}
		string tabPageCaption;
	}
}
