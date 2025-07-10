namespace Enterprise.MasterFiles.GUI
{
	public partial class ForwarderUserControl : OrganisationSecurityContainerControl
	{
		public ForwarderUserControl()
		{
			InitializeComponent();
		}

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
