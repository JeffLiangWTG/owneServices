namespace Enterprise.MasterFiles.GUI
{
	public partial class ContainerYardConfigurationUserControl : OrganisationContainerControl
	{
		public ContainerYardConfigurationUserControl()
		{
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
