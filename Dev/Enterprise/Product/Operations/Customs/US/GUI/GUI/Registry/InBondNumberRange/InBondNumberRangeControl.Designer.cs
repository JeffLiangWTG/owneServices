namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class InBondNumberRangeControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InBondNumberRangeControl));
			this.StartNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LastNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WarningLimitNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.InBondNumberRange);
			// 
			// StartNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.StartNumberTextBox, "StartNumberForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DataRegistry.Business.InBondNumberRange)(null)).StartNumber)));
			this.StartNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InBondNumberRangeControl|43d1b317-fe10-4993-9047-619bdc58c0fb", "Start Number");
			this.StartNumberTextBox.DecimalPlaces = 0;
			this.StartNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 12, true);
			this.StartNumberTextBox.Name = "StartNumberTextBox";
			this.StartNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.StartNumberTextBox.TabIndex = 2;
			this.StartNumberTextBox.Text = "0";
			this.StartNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.StartNumberTextBox.TrackDisposedAccess = true;
			// 
			// LastNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LastNumberTextBox, "LastNumberForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DataRegistry.Business.InBondNumberRange)(null)).LastNumber)));
			this.LastNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InBondNumberRangeControl|5bcc9d2c-a143-4ec1-95dd-47e3a4d05c96", "Last Number");
			this.LastNumberTextBox.DecimalPlaces = 0;
			this.LastNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 42, true);
			this.LastNumberTextBox.Name = "LastNumberTextBox";
			this.LastNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LastNumberTextBox.TabIndex = 3;
			this.LastNumberTextBox.Text = "0";
			this.LastNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LastNumberTextBox.TrackDisposedAccess = true;
			// 
			// WarningLimitNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WarningLimitNumberCalcEdit, "RunOutWarningLimitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DataRegistry.Business.InBondNumberRange)(null)).RunOutWarningLimitNumber)));
			this.WarningLimitNumberCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InBondNumberRangeControl|6be9cd8a-9943-43bf-b7c2-823348321a4f", "Run Out Limit", "Run Out Warning Limit", "Reached Run Out Warning Limit");
			this.WarningLimitNumberCalcEdit.DecimalPlaces = 0;
			this.WarningLimitNumberCalcEdit.Decimals = 0;
			this.WarningLimitNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 101, true);
			this.WarningLimitNumberCalcEdit.Name = "WarningLimitNumberCalcEdit";
			this.WarningLimitNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.WarningLimitNumberCalcEdit.TabIndex = 4;
			this.WarningLimitNumberCalcEdit.Text = "0";
			this.WarningLimitNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WarningLimitNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// InBondNumberRangeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WarningLimitNumberCalcEdit);
			this.Controls.Add(this.LastNumberTextBox);
			this.Controls.Add(this.StartNumberTextBox);
			this.Name = "InBondNumberRangeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 141, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox StartNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox LastNumberTextBox;
		private ZArchitecture.ZCalcEdit WarningLimitNumberCalcEdit;
	}
}
