using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class MAWBWithMessagesUserControl : ZUserControl
	{
		public MAWBWithMessagesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			SecurityDeclarationTabPage.TabVisible = SupplyChainSecurityConfiguration.New().UseConsignmentSecurityDeclaration;
		}
	}
}
