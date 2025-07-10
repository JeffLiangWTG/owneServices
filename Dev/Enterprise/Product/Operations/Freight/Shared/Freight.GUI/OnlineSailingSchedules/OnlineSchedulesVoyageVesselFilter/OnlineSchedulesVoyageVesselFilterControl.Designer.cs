namespace Enterprise.Freight.GUI
{
	partial class OnlineSchedulesVoyageVesselFilterControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.CaptionRenderingEnabled = true;
			this.VoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.GUI.OnlineSchedulesVoyageVesselFilter);
			this.TabIndex = 0;
			// 
			// VoyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageFlightTextBox, "VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.GUI.OnlineSchedulesVoyageVesselFilter)(null)).VoyageFlightNo)));
			this.VoyageFlightTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("6b044f10-284f-4967-8209-410ddc683962", "Voyage #");
			this.VoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 1, true);
			this.VoyageFlightTextBox.Name = "VoyageFlightTextBox";
			this.VoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.VoyageFlightTextBox.TabIndex = 1;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselFindBox, "Vessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.GUI.OnlineSchedulesVoyageVesselFilter)(null)).Vessel)));
			this.VesselFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("f100fe05-24b1-4e7a-ac49-1fafc3737cf7", "Vessel");
			this.VesselFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 1, true);
			this.VesselFindBox.Name = "VesselFindBox";
			this.VesselFindBox.ShowDescriptionBox = false;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.VesselFindBox.TabIndex = 2;
			// 
			// OnlineSchedulesVoyageVesselFilterControl
			// 
			this.Controls.Add(this.VesselFindBox);
			this.Controls.Add(this.VoyageFlightTextBox);
			this.Name = "OnlineSchedulesVoyageVesselFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		}

		#endregion

		private ZArchitecture.ZTextBox VoyageFlightTextBox;
		private ZArchitecture.GUI.ZCodeFindBox VesselFindBox;
	}
}
