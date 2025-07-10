namespace Enterprise.Customs.US.Module
{
	partial class FTZAdmissionNumberControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.controlNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.yearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.FTZAdmissionNumberFilter);
			// 
			// controlNoTextBox
			// 
			this.controlNoTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.controlNoTextBox, "ControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.FTZAdmissionNumberFilter)(null)).ControlNumber)));
			this.controlNoTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("1b0c16ce-5672-46b2-8fd5-bd8c7c127650", "Control No.");
			this.controlNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 1, true);
			this.controlNoTextBox.Name = "controlNoTextBox";
			this.controlNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.controlNoTextBox.TabIndex = 2;
			// 
			// zoneTextBox
			// 
			this.zoneTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zoneTextBox, "ZoneID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.FTZAdmissionNumberFilter)(null)).ZoneID)));
			this.zoneTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("93d29f02-6f45-4504-90db-900a36140f39", "Zone ID");
			this.zoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 1, true);
			this.zoneTextBox.Name = "zoneTextBox";
			this.zoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.zoneTextBox.TabIndex = 0;
			// 
			// yearTextBox
			// 
			this.yearTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.yearTextBox, "Year");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.FTZAdmissionNumberFilter)(null)).Year)));
			this.yearTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("e68be012-887e-40af-a768-58129a203e8c", "Year");
			this.yearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 1, true);
			this.yearTextBox.Name = "yearTextBox";
			this.yearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.yearTextBox.TabIndex = 1;
			// 
			// FTZAdmissionNumberControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.yearTextBox);
			this.Controls.Add(this.zoneTextBox);
			this.Controls.Add(this.controlNoTextBox);
			this.Name = "FTZAdmissionNumberControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox controlNoTextBox;
		private ZArchitecture.ZTextBox zoneTextBox;
		private ZArchitecture.ZTextBox yearTextBox;
	}
}
