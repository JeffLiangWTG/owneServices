namespace Enterprise.MasterFiles.GUI.Testing
{
	public partial class DummyForm
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			TabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			TabControl.Height = this.Height - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			TabControl.Width = this.Width - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.Controls.Add(TabControl);
			this.CaptionRenderingEnabled = true;
		}

		Enterprise.ZArchitecture.GUI.ZTemplateTabControl TabControl;
	}
}
