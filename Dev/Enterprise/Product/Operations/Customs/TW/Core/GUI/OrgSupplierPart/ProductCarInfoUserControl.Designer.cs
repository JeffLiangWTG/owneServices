namespace Enterprise.Customs.TW.GUI
{
	partial class ProductCarInfoUserControl
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
			this.CarTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransmissionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EngineTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LeftSideSteeringDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CatalyticConverterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StandardEquipmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModelYearCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DisplacementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NumberOfDoorsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NumberOfSeatsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NumberOfCylindersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NumberOfGearCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarTypeDropEdit.SuspendLayout();
			this.TransmissionDropEdit.SuspendLayout();
			this.EngineTypeDropEdit.SuspendLayout();
			this.LeftSideSteeringDropEdit.SuspendLayout();
			this.CatalyticConverterDropEdit.SuspendLayout();
			this.StandardEquipmentDropEdit.SuspendLayout();
			this.ConditionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusClassPartPivot);
			// 
			// CarTypeDropEdit
			// 
			this.CarTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarTypeDropEdit, "CI_CarType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_CarType)));
			this.CarTypeDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("db05a46b-ca2b-4331-9a81-81d13678ea47", "Car Type");
			this.CarTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 55, true);
			this.CarTypeDropEdit.Name = "CarTypeDropEdit";
			this.CarTypeDropEdit.PreBoundMaxLength = 3;
			this.CarTypeDropEdit.ShouldResizeByMaxLength = false;
			this.CarTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.CarTypeDropEdit.TabIndex = 2;
			// 
			// TransmissionDropEdit
			// 
			this.TransmissionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransmissionDropEdit, "CI_Transmission");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_Transmission)));
			this.TransmissionDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("85a063c3-3826-4be9-aef7-ce22ec64fe4b", "Transmission");
			this.TransmissionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 81, true);
			this.TransmissionDropEdit.Name = "TransmissionDropEdit";
			this.TransmissionDropEdit.PreBoundMaxLength = 3;
			this.TransmissionDropEdit.ShouldResizeByMaxLength = false;
			this.TransmissionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.TransmissionDropEdit.TabIndex = 3;
			// 
			// EngineTypeDropEdit
			// 
			this.EngineTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EngineTypeDropEdit, "CI_EngineType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_EngineType)));
			this.EngineTypeDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("046bb2e9-2639-4998-9938-98a9629c24da", "Engine Type");
			this.EngineTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 107, true);
			this.EngineTypeDropEdit.Name = "EngineTypeDropEdit";
			this.EngineTypeDropEdit.PreBoundMaxLength = 3;
			this.EngineTypeDropEdit.ShouldResizeByMaxLength = false;
			this.EngineTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.EngineTypeDropEdit.TabIndex = 4;
			// 
			// LeftSideSteeringDropEdit
			// 
			this.LeftSideSteeringDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LeftSideSteeringDropEdit, "CI_LHD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_LHD)));
			this.LeftSideSteeringDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("7cfbb791-8ca5-4ba0-a1a2-5f8c80353992", "Left Side Steering");
			this.LeftSideSteeringDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 133, true);
			this.LeftSideSteeringDropEdit.Name = "LeftSideSteeringDropEdit";
			this.LeftSideSteeringDropEdit.PreBoundMaxLength = 3;
			this.LeftSideSteeringDropEdit.ShouldResizeByMaxLength = false;
			this.LeftSideSteeringDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.LeftSideSteeringDropEdit.TabIndex = 5;
			// 
			// CatalyticConverterDropEdit
			// 
			this.CatalyticConverterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CatalyticConverterDropEdit, "CI_HasCatalystConverter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_HasCatalystConverter)));
			this.CatalyticConverterDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("33e34c3f-8f8a-4eea-9543-68f6be5ef00d", "Catalytic Converter?");
			this.CatalyticConverterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 3, true);
			this.CatalyticConverterDropEdit.Name = "CatalyticConverterDropEdit";
			this.CatalyticConverterDropEdit.PreBoundMaxLength = 3;
			this.CatalyticConverterDropEdit.ShouldResizeByMaxLength = false;
			this.CatalyticConverterDropEdit.ShowDescriptionBox = false;
			this.CatalyticConverterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CatalyticConverterDropEdit.TabIndex = 0;
			// 
			// StandardEquipmentDropEdit
			// 
			this.StandardEquipmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StandardEquipmentDropEdit, "CI_EquipmentPrintMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_EquipmentPrintMode)));
			this.StandardEquipmentDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8a8bd3b0-1267-4875-8d73-2c372c4aa69f", "Standard Equipment");
			this.StandardEquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 29, true);
			this.StandardEquipmentDropEdit.Name = "StandardEquipmentDropEdit";
			this.StandardEquipmentDropEdit.PreBoundMaxLength = 3;
			this.StandardEquipmentDropEdit.ShouldResizeByMaxLength = false;
			this.StandardEquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.StandardEquipmentDropEdit.TabIndex = 1;
			// 
			// ConditionDropEdit
			// 
			this.ConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConditionDropEdit, "CI_CarCondition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_CarCondition)));
			this.ConditionDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("ed29de50-6baf-4f49-aca4-15be06d3d918", "Condition");
			this.ConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 159, true);
			this.ConditionDropEdit.Name = "ConditionDropEdit";
			this.ConditionDropEdit.PreBoundMaxLength = 3;
			this.ConditionDropEdit.ShouldResizeByMaxLength = false;
			this.ConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.ConditionDropEdit.TabIndex = 6;
			// 
			// ModelYearCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ModelYearCalcEdit, "CI_ModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_ModelYear)));
			this.ModelYearCalcEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("668a4b2f-3940-42cc-80b7-cdb69ea636bd", "Model Year");
			this.ModelYearCalcEdit.DecimalPlaces = 0;
			this.ModelYearCalcEdit.Decimals = 0;
			this.ModelYearCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 3, true);
			this.ModelYearCalcEdit.MaxValue = new decimal(new int[] {
			9999,
			0,
			0,
			0});
			this.ModelYearCalcEdit.Name = "ModelYearCalcEdit";
			this.ModelYearCalcEdit.ShowGroupSeparators = false;
			this.ModelYearCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.ModelYearCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.ModelYearCalcEdit.TabIndex = 7;
			this.ModelYearCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisplacementTextBox
			// 
			this.DisplacementTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisplacementTextBox, "CI_Displacement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_Displacement)));
			this.DisplacementTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("9de58e31-1946-4d59-963b-5494c7c21812", "Displacement (cc)");
			this.DisplacementTextBox.DecimalPlaces = 0;
			this.DisplacementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 29, true);
			this.DisplacementTextBox.Name = "DisplacementTextBox";
			this.DisplacementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.DisplacementTextBox.TabIndex = 8;
			this.DisplacementTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberOfDoorsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfDoorsCalcEdit, "CI_NumberOfDoor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_NumberOfDoor)));
			this.NumberOfDoorsCalcEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("132984f2-4a4c-4f4c-a719-a97d26a08923", "Number of Door(s)");
			this.NumberOfDoorsCalcEdit.DecimalPlaces = 0;
			this.NumberOfDoorsCalcEdit.Decimals = 0;
			this.NumberOfDoorsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 55, true);
			this.NumberOfDoorsCalcEdit.MaxValue = new decimal(new int[] {
			9,
			0,
			0,
			0});
			this.NumberOfDoorsCalcEdit.Name = "NumberOfDoorsCalcEdit";
			this.NumberOfDoorsCalcEdit.ShowGroupSeparators = false;
			this.NumberOfDoorsCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.NumberOfDoorsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.NumberOfDoorsCalcEdit.TabIndex = 9;
			this.NumberOfDoorsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberOfSeatsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfSeatsCalcEdit, "CI_Seats");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_Seats)));
			this.NumberOfSeatsCalcEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d34a6bba-f238-4bed-87b3-863f8741db39", "Number of Seat(s)");
			this.NumberOfSeatsCalcEdit.DecimalPlaces = 0;
			this.NumberOfSeatsCalcEdit.Decimals = 0;
			this.NumberOfSeatsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 81, true);
			this.NumberOfSeatsCalcEdit.MaxValue = new decimal(new int[] {
			99,
			0,
			0,
			0});
			this.NumberOfSeatsCalcEdit.Name = "NumberOfSeatsCalcEdit";
			this.NumberOfSeatsCalcEdit.ShowGroupSeparators = false;
			this.NumberOfSeatsCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.NumberOfSeatsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.NumberOfSeatsCalcEdit.TabIndex = 10;
			this.NumberOfSeatsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberOfCylindersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfCylindersCalcEdit, "CI_Cylinders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_Cylinders)));
			this.NumberOfCylindersCalcEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e37e18df-4ee3-40a2-8455-968ae9db053e", "Number of Cylinder(s)");
			this.NumberOfCylindersCalcEdit.DecimalPlaces = 0;
			this.NumberOfCylindersCalcEdit.Decimals = 0;
			this.NumberOfCylindersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 107, true);
			this.NumberOfCylindersCalcEdit.MaxValue = new decimal(new int[] {
			99,
			0,
			0,
			0});
			this.NumberOfCylindersCalcEdit.Name = "NumberOfCylindersCalcEdit";
			this.NumberOfCylindersCalcEdit.ShowGroupSeparators = false;
			this.NumberOfCylindersCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.NumberOfCylindersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.NumberOfCylindersCalcEdit.TabIndex = 11;
			this.NumberOfCylindersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberOfGearCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfGearCalcEdit, "CI_Gears");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.TW.Business.CusClassPartPivot)(null)).CI_Gears)));
			this.NumberOfGearCalcEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("a121909d-ff2b-4721-acd3-aabc01aa555c", "Number of Gear(s)");
			this.NumberOfGearCalcEdit.DecimalPlaces = 0;
			this.NumberOfGearCalcEdit.Decimals = 0;
			this.NumberOfGearCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 133, true);
			this.NumberOfGearCalcEdit.MaxValue = new decimal(new int[] {
			99,
			0,
			0,
			0});
			this.NumberOfGearCalcEdit.Name = "NumberOfGearCalcEdit";
			this.NumberOfGearCalcEdit.ShowGroupSeparators = false;
			this.NumberOfGearCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.NumberOfGearCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.NumberOfGearCalcEdit.TabIndex = 12;
			this.NumberOfGearCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProductCarInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.NumberOfGearCalcEdit);
			this.Controls.Add(this.NumberOfCylindersCalcEdit);
			this.Controls.Add(this.NumberOfSeatsCalcEdit);
			this.Controls.Add(this.NumberOfDoorsCalcEdit);
			this.Controls.Add(this.DisplacementTextBox);
			this.Controls.Add(this.ConditionDropEdit);
			this.Controls.Add(this.ModelYearCalcEdit);
			this.Controls.Add(this.StandardEquipmentDropEdit);
			this.Controls.Add(this.CatalyticConverterDropEdit);
			this.Controls.Add(this.LeftSideSteeringDropEdit);
			this.Controls.Add(this.EngineTypeDropEdit);
			this.Controls.Add(this.TransmissionDropEdit);
			this.Controls.Add(this.CarTypeDropEdit);
			this.Name = "ProductCarInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarTypeDropEdit.ResumeLayout(true);
			this.CarTypeDropEdit.PerformLayout();
			this.TransmissionDropEdit.ResumeLayout(true);
			this.TransmissionDropEdit.PerformLayout();
			this.EngineTypeDropEdit.ResumeLayout(true);
			this.EngineTypeDropEdit.PerformLayout();
			this.LeftSideSteeringDropEdit.ResumeLayout(true);
			this.LeftSideSteeringDropEdit.PerformLayout();
			this.CatalyticConverterDropEdit.ResumeLayout(true);
			this.CatalyticConverterDropEdit.PerformLayout();
			this.StandardEquipmentDropEdit.ResumeLayout(true);
			this.StandardEquipmentDropEdit.PerformLayout();
			this.ConditionDropEdit.ResumeLayout(true);
			this.ConditionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit CarTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit TransmissionDropEdit;
		private ZArchitecture.GUI.ZDropEdit EngineTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit LeftSideSteeringDropEdit;
		private ZArchitecture.GUI.ZDropEdit CatalyticConverterDropEdit;
		private ZArchitecture.GUI.ZDropEdit StandardEquipmentDropEdit;
		private ZArchitecture.GUI.ZDropEdit ConditionDropEdit;
		private ZArchitecture.ZCalcEdit ModelYearCalcEdit;
		private ZArchitecture.ZTextBox DisplacementTextBox;
		private ZArchitecture.ZCalcEdit NumberOfDoorsCalcEdit;
		private ZArchitecture.ZCalcEdit NumberOfSeatsCalcEdit;
		private ZArchitecture.ZCalcEdit NumberOfCylindersCalcEdit;
		private ZArchitecture.ZCalcEdit NumberOfGearCalcEdit;
	}
}
