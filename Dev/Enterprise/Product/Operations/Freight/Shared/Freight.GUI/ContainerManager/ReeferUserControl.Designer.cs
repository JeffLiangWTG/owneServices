namespace Enterprise.Freight.GUI
{
	partial class ReeferUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.exportIsControlledAtmosphereCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JY_AirVentFlowRateCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.exportIsFrozenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.exportIsChillerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JC_HumidityPercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReeferGeneratorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JY_SetPointTempCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TempRecorderSerialNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.exportIsNonOperatingReeferCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JY_AirVentFlowRateCalcDropEdit.SuspendLayout();
			this.JY_SetPointTempCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonContainer);
			// 
			// exportIsControlledAtmosphereCheckBox
			// 
			this.exportIsControlledAtmosphereCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.exportIsControlledAtmosphereCheckBox, "JobContainer+JC_IsControlledAtmosphere");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_IsControlledAtmosphere)));
			this.exportIsControlledAtmosphereCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.exportIsControlledAtmosphereCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 48, true);
			this.exportIsControlledAtmosphereCheckBox.Name = "exportIsControlledAtmosphereCheckBox";
			this.exportIsControlledAtmosphereCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.exportIsControlledAtmosphereCheckBox.TabIndex = 1;
			this.exportIsControlledAtmosphereCheckBox.UseVisualStyleBackColor = false;
			// 
			// JY_AirVentFlowRateCalcDropEdit
			// 
			this.JY_AirVentFlowRateCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JY_AirVentFlowRateCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_AirVentFlow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_AirVentFlowRateUnit)));
			this.JY_AirVentFlowRateCalcDropEdit.BindToAmount = "JobContainer+JC_AirVentFlow";
			this.JY_AirVentFlowRateCalcDropEdit.BindToUnit = "JobContainer+JC_AirVentFlowRateUnit";
			this.JY_AirVentFlowRateCalcDropEdit.Decimals = 0;
			this.JY_AirVentFlowRateCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 181, true);
			this.JY_AirVentFlowRateCalcDropEdit.Name = "JY_AirVentFlowRateCalcDropEdit";
			this.JY_AirVentFlowRateCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.JY_AirVentFlowRateCalcDropEdit.TabIndex = 10;
			this.JY_AirVentFlowRateCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// exportIsFrozenCheckBox
			// 
			this.exportIsFrozenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.exportIsFrozenCheckBox, "JobContainer+IsFreezer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.IsFreezer)));
			this.exportIsFrozenCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.exportIsFrozenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 90, true);
			this.exportIsFrozenCheckBox.Name = "exportIsFrozenCheckBox";
			this.exportIsFrozenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.exportIsFrozenCheckBox.TabIndex = 2;
			// 
			// exportIsChillerCheckBox
			// 
			this.exportIsChillerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.exportIsChillerCheckBox, "JobContainer+IsChiller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.IsChiller)));
			this.exportIsChillerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.exportIsChillerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 69, true);
			this.exportIsChillerCheckBox.Name = "exportIsChillerCheckBox";
			this.exportIsChillerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.exportIsChillerCheckBox.TabIndex = 1;
			// 
			// JC_HumidityPercentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JC_HumidityPercentCalcEdit, "JobContainer+JC_HumidityPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_HumidityPercent)));
			this.JC_HumidityPercentCalcEdit.CaptionResourceString = null;
			this.JC_HumidityPercentCalcEdit.DecimalPlaces = 0;
			this.JC_HumidityPercentCalcEdit.Decimals = 0;
			this.JC_HumidityPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 137, true);
			this.JC_HumidityPercentCalcEdit.Name = "JC_HumidityPercentCalcEdit";
			this.JC_HumidityPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.JC_HumidityPercentCalcEdit.TabIndex = 6;
			this.JC_HumidityPercentCalcEdit.Text = "0";
			this.JC_HumidityPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReeferGeneratorTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReeferGeneratorTextBox, "JobContainer+JC_RefrigGeneratorID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_RefrigGeneratorID)));
			this.ReeferGeneratorTextBox.CaptionResourceString = null;
			this.ReeferGeneratorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 203, true);
			this.ReeferGeneratorTextBox.Name = "ReeferGeneratorTextBox";
			this.ReeferGeneratorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.ReeferGeneratorTextBox.TabIndex = 12;
			// 
			// JY_SetPointTempCalcDropEdit
			// 
			this.JY_SetPointTempCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JY_SetPointTempCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_SetPointTempUnit)));
			this.JY_SetPointTempCalcDropEdit.BindToAmount = "JobContainer+JC_SetPointTemp";
			this.JY_SetPointTempCalcDropEdit.BindToUnit = "JobContainer+JC_SetPointTempUnit";
			this.JY_SetPointTempCalcDropEdit.Decimals = 1;
			this.JY_SetPointTempCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 115, true);
			this.JY_SetPointTempCalcDropEdit.Name = "JY_SetPointTempCalcDropEdit";
			this.JY_SetPointTempCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.JY_SetPointTempCalcDropEdit.TabIndex = 4;
			this.JY_SetPointTempCalcDropEdit.UnitPreBoundMaxLength = 1;
			// 
			// TempRecorderSerialNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.TempRecorderSerialNoTextBox, "JobContainer+JC_TempRecorderSerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_TempRecorderSerialNo)));
			this.TempRecorderSerialNoTextBox.CaptionResourceString = null;
			this.TempRecorderSerialNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 159, true);
			this.TempRecorderSerialNoTextBox.Name = "TempRecorderSerialNoTextBox";
			this.TempRecorderSerialNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.TempRecorderSerialNoTextBox.TabIndex = 8;
			// 
			// exportIsNonOperatingReeferCheckBox
			// 
			this.exportIsNonOperatingReeferCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.exportIsNonOperatingReeferCheckBox, "JobContainer+JC_IsNonOperativeReefer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_IsNonOperativeReefer)));
			this.exportIsNonOperatingReeferCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.exportIsNonOperatingReeferCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 23, true);
			this.exportIsNonOperatingReeferCheckBox.Name = "exportIsNonOperatingReeferCheckBox";
			this.exportIsNonOperatingReeferCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.exportIsNonOperatingReeferCheckBox.TabIndex = 0;
			this.exportIsNonOperatingReeferCheckBox.UseVisualStyleBackColor = false;
			// 
			// ReeferUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.exportIsNonOperatingReeferCheckBox);
			this.Controls.Add(this.TempRecorderSerialNoTextBox);
			this.Controls.Add(this.exportIsControlledAtmosphereCheckBox);
			this.Controls.Add(this.JY_SetPointTempCalcDropEdit);
			this.Controls.Add(this.JY_AirVentFlowRateCalcDropEdit);
			this.Controls.Add(this.ReeferGeneratorTextBox);
			this.Controls.Add(this.exportIsFrozenCheckBox);
			this.Controls.Add(this.JC_HumidityPercentCalcEdit);
			this.Controls.Add(this.exportIsChillerCheckBox);
			this.Name = "ReeferUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 253, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JY_AirVentFlowRateCalcDropEdit.ResumeLayout(true);
			this.JY_AirVentFlowRateCalcDropEdit.PerformLayout();
			this.JY_SetPointTempCalcDropEdit.ResumeLayout(true);
			this.JY_SetPointTempCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox exportIsNonOperatingReeferCheckBox;
		private ZArchitecture.GUI.ZCheckBox exportIsControlledAtmosphereCheckBox;
		private ZArchitecture.GUI.ZCalcDropEdit JY_AirVentFlowRateCalcDropEdit;
		private ZArchitecture.GUI.ZCheckBox exportIsFrozenCheckBox;
		private ZArchitecture.GUI.ZCheckBox exportIsChillerCheckBox;
		private ZArchitecture.ZCalcEdit JC_HumidityPercentCalcEdit;
		private ZArchitecture.ZTextBox ReeferGeneratorTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit JY_SetPointTempCalcDropEdit;
		private ZArchitecture.ZTextBox TempRecorderSerialNoTextBox;
	}
}
