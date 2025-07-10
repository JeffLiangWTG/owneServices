namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomLabelsUserControl
	{

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.NoFieldsAvailableLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// NoFieldsAvailableLabel
			// 
			this.NoFieldsAvailableLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.NoFieldsAvailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 48, true);
			this.NoFieldsAvailableLabel.Name = "NoFieldsAvailableLabel";
			this.NoFieldsAvailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 23, true);
			this.NoFieldsAvailableLabel.TabIndex = 0;
			// 
			// CustomLabelsUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.NoFieldsAvailableLabel);
			this.Name = "CustomLabelsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 136, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		Enterprise.ZArchitecture.ZLabel NoFieldsAvailableLabel;
	}
}
