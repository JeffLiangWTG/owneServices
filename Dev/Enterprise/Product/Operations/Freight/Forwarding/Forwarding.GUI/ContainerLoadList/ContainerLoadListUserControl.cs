using System;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ContainerLoadListUserControl : ZUserControl, INotifications
	{
		public ContainerLoadListUserControl()
		{
			InitializeComponent();

			this.LoadListContainerDetailLinkLabel.LinkClicked += LoadListContainergDetailLinkLabel_LinkClicked;
		}

		void LoadListContainergDetailLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (this.DataSource is CYContainerLoadList header && header.CLH_LoadMode == ContainerLoadListHeaderLoadMode.ContainerYard)
			{
				GlowLinksHelper.OpenEnityInGlow(this, "Goto/ContainerLoadList", header);
			}
			else
			{
				throw new ArgumentException(FormattableString.Invariant($"LoadMode not supported: {((CommonContainerLoadList)this.DataSource).CLH_LoadMode}"), "CLH_LoadMode");
			}
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion
	}
}
