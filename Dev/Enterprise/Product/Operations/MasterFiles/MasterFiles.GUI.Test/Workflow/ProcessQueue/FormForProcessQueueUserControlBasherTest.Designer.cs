namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed partial class FormForProcessQueueUserControlBasherTest
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			Controls.Add(new ProcessQueueUserControl());
			this.CaptionRenderingEnabled = true;
		}
	}
}
