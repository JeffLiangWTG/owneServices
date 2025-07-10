using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsigneeUserControl : OrganisationSecurityContainerControl
	{
		public ConsigneeUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ConsigneeTabControl.PlugIns.Add(ControllerIDs.Customs.AU.CMRLodgementQuestion);

			RelationshipsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				RelationshipsControl.RelationshipDetailsUserControl.SetupPlugIn(RelationshipsControl.OrgSupplierLinkBoundGrid);
			});
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
