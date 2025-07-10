namespace Enterprise.MasterFiles.GUI
{
	public partial class CoordinateEntryControl
	{
		void InitializeComponent()
		{
			this.LatitudeTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LongitudeTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LatitudeTextBox
			// 
			this.LatitudeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CoordinateEntryControl|8cc7d119-3d38-4b77-86ba-8b6c13ea5d55", "Latitude");
			this.LatitudeTextBox.DecimalPlaces = 5;
			this.LatitudeTextBox.MaxLength = 9;
			this.LatitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 11, true);
			this.LatitudeTextBox.Name = "LatitudeTextBox";
			this.LatitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LatitudeTextBox.TabIndex = 0;
			// 
			// LongitudeTextBox
			// 
			this.LongitudeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CoordinateEntryControl|d8db3c29-5bde-48af-9580-765220cb4abc", "Longitude");
			this.LongitudeTextBox.DecimalPlaces = 5;
			this.LongitudeTextBox.MaxLength = 10;
			this.LongitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 44, true);
			this.LongitudeTextBox.Name = "LongitudeTextBox";
			this.LongitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LongitudeTextBox.TabIndex = 2;
			// 
			// CoordinateEntryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LongitudeTextBox);
			this.Controls.Add(this.LatitudeTextBox);
			this.Name = "CoordinateEntryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 75, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.ZCalcEdit LongitudeTextBox;
		Enterprise.ZArchitecture.ZCalcEdit LatitudeTextBox;
	}
}
