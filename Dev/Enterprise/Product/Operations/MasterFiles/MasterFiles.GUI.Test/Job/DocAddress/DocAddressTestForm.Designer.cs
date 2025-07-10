namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed partial class DocAddressTestForm
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			Control.Dock = System.Windows.Forms.DockStyle.Fill;
			Controls.Add(Control);

			Size = new System.Drawing.Size(800, 600);
			this.CaptionRenderingEnabled = true;
		}
	}
}
