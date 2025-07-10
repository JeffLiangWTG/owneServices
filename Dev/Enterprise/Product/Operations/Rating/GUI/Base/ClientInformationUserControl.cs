using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class ClientInformationUserControl : ZUserControl
	{
		public ClientInformationUserControl()
			: base()
		{
			InitializeComponent();
		}

		public void SetOrgHeaderLabelToSupplier()
		{
			this.ClientOrganisationControl.Text = Res.GetString("98d70f08-c80c-455e-ad16-c6f046903784", "Service Provider");
		}

		#region IDisposable Members

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
