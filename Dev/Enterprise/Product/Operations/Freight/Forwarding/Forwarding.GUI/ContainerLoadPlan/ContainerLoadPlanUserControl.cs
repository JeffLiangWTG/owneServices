using System;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ContainerLoadPlanUserControl : ZUserControl, INotifications
	{
		public ContainerLoadPlanUserControl()
		{
			InitializeComponent();

			this.LoadPlanContainerDetailLinkLabel.LinkClicked += LoadPlanContainergDetailLinkLabel_LinkClicked;
		}

		void LoadPlanContainergDetailLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (this.DataSource is CFSContainerLoadList header && header.CLH_LoadMode == ContainerLoadListHeaderLoadMode.ContainerFreightStation)
			{
				GlowLinksHelper.OpenEnityInGlow(this, "Goto/ContainerLoadPlan", header);
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
