using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	public partial class PortMessagingControl : ZUserControl
	{
		public PortMessagingControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(messageLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			portMessagingManager = (PortMessagingManager)dataSource;
		}

		PortMessagingManager portMessagingManager;

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			var tabPage = Parent as ZTabPage;

			if (tabPage != null)
			{
				tabPage.RunWhenTabInitialized((s, args) => Init());
			}
		}

		void Init()
		{
			Argument.NotNull(portMessagingManager, "portMessagingManager");

			portMessagingManager.DataChanged += (s, e) => ProcessContent();
			ProcessContent();
		}

		internal void ProcessContent()
		{
			if (portMessagingManager != null)
			{
				var portMessagingAvailability = portMessagingManager.CheckPortMessagingAvailability();
				if (!portMessagingAvailability.IsEmpty)
				{
					ShowMessage(portMessagingAvailability);
				}
				else
				{
					ShowContentControls();
				}

				var logs = portMessagingManager?.Data?.DakosyLogs;
				portMessagingEventsTabPage.TabVisible = logs != null;

				portOrderTabPage.TabVisible = !PortMessagingRegistry.Instance.EnablePortOrderFormBuilderForm.Value;
			}
		}

		void ShowContentControls()
		{
			consolDataControl.Visible = true;
			statusControl.Visible = true;

			if (portMessagingManager != null && portMessagingManager is ConsolPortMessagingManager)
			{
				consolDataControl.Find(c => c.Name == "shipperEORITextBox").FirstOrDefault().Visible = false;
				consolDataControl.Find(c => c.Name == "agentEORITextBox").FirstOrDefault().Visible = false;
			}

			if (portMessagingManager != null && portMessagingManager is ShipmentPortMessagingManager)
			{
				shipmentDataDynamicCreationControl.Visible = true;
				shipmentDataDynamicCreationControl.UserControlType = typeof(PortMessagingShipmentControl);
			}

			messageLabel.Visible = false;
			messageLabel.Text = null;
		}

		void ShowMessage(string message)
		{
			consolDataControl.Visible = false;
			statusControl.Visible = false;

			shipmentDataDynamicCreationControl.Visible = false;
			shipmentDataDynamicCreationControl.UserControlType = null;

			messageLabel.Visible = true;
			messageLabel.Text = message;
		}
	}
}
