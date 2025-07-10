namespace Enterprise.MasterFiles.GUI.Testing
{
	public partial class FormForCurrentQueueUserControlBasherTest
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			CurrentQueueUserControl userControl = new CurrentQueueUserControl();
			userControl.BindToPrefix = BindToPrefix;
			Controls.Add(userControl);
			this.CaptionRenderingEnabled = true;
		}
	}
}
