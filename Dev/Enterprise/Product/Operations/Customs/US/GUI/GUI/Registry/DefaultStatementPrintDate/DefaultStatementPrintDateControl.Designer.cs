namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class DefaultStatementPrintDateControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DoDataDefaultZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NumberOfDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.DefaultStatementPrintDate);
			// 
			// DoDataDefaultZCheckBox
			// 
			this.DoDataDefaultZCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DoDataDefaultZCheckBox, "DoDefaultPrelimStatementPrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.DataRegistry.Business.DefaultStatementPrintDate)(null)).DoDefaultPrelimStatementPrintDate)));
			this.DoDataDefaultZCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DefaultStatementPrintDateControl|2ccca4e0-cd1b-4f16-bb7b-a92151b543b4", "Default Statement Print Date/Month?");
			this.DoDataDefaultZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 12, true);
			this.DoDataDefaultZCheckBox.Name = "DoDataDefaultZCheckBox";
			this.DoDataDefaultZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.DoDataDefaultZCheckBox.TabIndex = 2;
			// 
			// NumberOfDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfDaysCalcEdit, "NumberOfDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DataRegistry.Business.DefaultStatementPrintDate)(null)).NumberOfDays)));
			this.NumberOfDaysCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DefaultStatementPrintDateControl|2166e0c6-6fca-42c6-bc50-681934dad5cc", "Number Of Days");
			this.NumberOfDaysCalcEdit.DecimalPlaces = 0;
			this.NumberOfDaysCalcEdit.Decimals = 0;
			this.NumberOfDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 50, true);
			this.NumberOfDaysCalcEdit.Name = "NumberOfDaysCalcEdit";
			this.NumberOfDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.NumberOfDaysCalcEdit.TabIndex = 3;
			this.NumberOfDaysCalcEdit.Text = "0";
			this.NumberOfDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NumberOfDaysCalcEdit.TrackDisposedAccess = true;
			// 
			// DefaultStatementPrintDateControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberOfDaysCalcEdit);
			this.Controls.Add(this.DoDataDefaultZCheckBox);
			this.Name = "DefaultStatementPrintDateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox DoDataDefaultZCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit NumberOfDaysCalcEdit;
	}
}
