using System;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsignorUserControl : OrganisationSecurityContainerControl
	{
		public ConsignorUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RelationshipsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				consignorRelationshipsControl1.RelationshipDetailsUserControl.SetupPlugIn(consignorRelationshipsControl1.OrgBuyerLinkBoundGrid);
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
