namespace Enterprise.Customs.PL.GUI
{
	partial class CarInfoUserControl
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
			this.CarInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarMakeModelLookUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CarInfoMarkModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarInfoVinTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarInfoEngineNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarInfoCapacityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CarInfoCapacityCm3Label = new Enterprise.ZArchitecture.ZLabel();
			this.CarInfoFuelTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarInfoProductionYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarInfoGroupBox.SuspendLayout();
			this.CarInfoFuelTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine);
			// 
			// CarInfoGroupBox
			// 
			this.CarInfoGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("86d027f4-c7a3-463a-a20f-403c0f1d3aa7", "Car Information");
			this.CarInfoGroupBox.Controls.Add(this.CarMakeModelLookUpButton);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoMarkModelTextBox);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoVinTextBox);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoEngineNoTextBox);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoCapacityCalcEdit);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoCapacityCm3Label);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoFuelTypeDropEdit);
			this.CarInfoGroupBox.Controls.Add(this.CarInfoProductionYearTextBox);
			this.CarInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CarInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CarInfoGroupBox.Name = "CarInfoGroupBox";
			this.CarInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 406, true);
			this.CarInfoGroupBox.TabIndex = 1;
			this.CarInfoGroupBox.TabStop = false;
			// 
			// CarMakeModelLookUpButton
			// 
			this.CarMakeModelLookUpButton.IsCaptionOverridden = true;
			this.CarMakeModelLookUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 19, true);
			this.CarMakeModelLookUpButton.Name = "CarMakeModelLookUpButton";
			this.CarMakeModelLookUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CarMakeModelLookUpButton.TabIndex = 1;
			this.CarMakeModelLookUpButton.Text = "...";
			this.CarMakeModelLookUpButton.ToolTipCaption = null;
			this.CarMakeModelLookUpButton.Click += new System.EventHandler(this.CarMakeModelLookUpButton_Click);
			// 
			// CarInfoMarkModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarInfoMarkModelTextBox, "JI_MarkModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).JI_MarkModel)));
			this.CarInfoMarkModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 19, true);
			this.CarInfoMarkModelTextBox.Name = "CarInfoMarkModelTextBox";
			this.CarInfoMarkModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CarInfoMarkModelTextBox.TabIndex = 0;
			// 
			// CarInfoVinTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarInfoVinTextBox, "FirstVehicle+CVH_VehicleIdentificationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).FirstVehicle.CVH_VehicleIdentificationNumber)));
			this.CarInfoVinTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CarInfoVinTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 41, true);
			this.CarInfoVinTextBox.Name = "CarInfoVinTextBox";
			this.CarInfoVinTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CarInfoVinTextBox.TabIndex = 2;
			// 
			// CarInfoEngineNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarInfoEngineNoTextBox, "FirstVehicle+Engine+CEG_EngineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).FirstVehicle.Engine.CEG_EngineNumber)));
			this.CarInfoEngineNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CarInfoEngineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 63, true);
			this.CarInfoEngineNoTextBox.Name = "CarInfoEngineNoTextBox";
			this.CarInfoEngineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CarInfoEngineNoTextBox.TabIndex = 3;
			// 
			// CarInfoCapacityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CarInfoCapacityCalcEdit, "FirstVehicle+Engine+CEG_CapacityCC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).FirstVehicle.Engine.CEG_CapacityCC)));
			this.CarInfoCapacityCalcEdit.DecimalPlaces = 0;
			this.CarInfoCapacityCalcEdit.Decimals = 0;
			this.CarInfoCapacityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 85, true);
			this.CarInfoCapacityCalcEdit.MaxValue = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.CarInfoCapacityCalcEdit.Name = "CarInfoCapacityCalcEdit";
			this.CarInfoCapacityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CarInfoCapacityCalcEdit.TabIndex = 4;
			this.CarInfoCapacityCalcEdit.Text = "0";
			this.CarInfoCapacityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CarInfoCapacityCalcEdit.TrackDisposedAccess = true;
			// 
			// CarInfoCapacityCm3Label
			// 
			this.CarInfoCapacityCm3Label.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("709c5995-10d8-44e1-bc82-3a3ac0d54b82", "cm 3");
			this.CarInfoCapacityCm3Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CarInfoCapacityCm3Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 85, true);
			this.CarInfoCapacityCm3Label.Name = "CarInfoCapacityCm3Label";
			this.CarInfoCapacityCm3Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.CarInfoCapacityCm3Label.TabIndex = 4;
			this.CarInfoCapacityCm3Label.UseMnemonic = false;
			// 
			// CarInfoFuelTypeDropEdit
			// 
			this.CarInfoFuelTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarInfoFuelTypeDropEdit, "FirstVehicle+Engine+CEG_EngineType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).FirstVehicle.Engine.CEG_EngineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).FirstVehicle.Engine.Lookups.FuelTypeList)));
			this.CarInfoFuelTypeDropEdit.BindToList = "FirstVehicle.Engine.Lookups+FuelTypeList";
			this.CarInfoFuelTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 107, true);
			this.CarInfoFuelTypeDropEdit.Name = "CarInfoFuelTypeDropEdit";
			this.CarInfoFuelTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CarInfoFuelTypeDropEdit.TabIndex = 5;
			// 
			// CarInfoProductionYearTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarInfoProductionYearTextBox, "FirstVehicle+CVH_ModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).FirstVehicle.CVH_ModelYear)));
			this.CarInfoProductionYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 130, true);
			this.CarInfoProductionYearTextBox.Name = "CarInfoProductionYearTextBox";
			this.CarInfoProductionYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 18, true);
			this.CarInfoProductionYearTextBox.TabIndex = 6;
			// 
			// CarInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CarInfoGroupBox);
			this.Name = "CarInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 406, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarInfoGroupBox.ResumeLayout(false);
			this.CarInfoGroupBox.PerformLayout();
			this.CarInfoFuelTypeDropEdit.ResumeLayout(true);
			this.CarInfoFuelTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CarInfoGroupBox;
		internal ZArchitecture.GUI.ZButton CarMakeModelLookUpButton;
		internal ZArchitecture.ZTextBox CarInfoMarkModelTextBox;
		internal ZArchitecture.ZTextBox CarInfoVinTextBox;
		internal ZArchitecture.ZTextBox CarInfoEngineNoTextBox;
		internal ZArchitecture.ZCalcEdit CarInfoCapacityCalcEdit;
		internal ZArchitecture.ZLabel CarInfoCapacityCm3Label;
		internal ZArchitecture.GUI.ZDropEdit CarInfoFuelTypeDropEdit;
		internal ZArchitecture.ZTextBox CarInfoProductionYearTextBox;
	}
}
