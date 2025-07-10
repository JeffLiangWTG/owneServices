using System;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WhsFacilityUserControl : OrganisationSecurityContainerControl
	{
		public WhsFacilityUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
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
