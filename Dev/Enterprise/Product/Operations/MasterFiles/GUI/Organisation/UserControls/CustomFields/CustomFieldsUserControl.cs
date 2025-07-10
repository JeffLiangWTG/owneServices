namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomFieldsUserControl : OrganisationSecurityContainerControl
	{
		public CustomFieldsUserControl()
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
