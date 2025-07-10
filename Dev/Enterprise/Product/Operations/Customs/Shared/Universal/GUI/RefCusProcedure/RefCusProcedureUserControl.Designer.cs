namespace Enterprise.Customs.Universal.GUI
{
	partial class RefCusProcedureUserControl
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
			this.ZZ6_CategoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreviousProcedureCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryOrGroupingCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZZ6_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZZ6_ProcedureCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConcessionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DataGroupingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CalculateDutyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LandedCostCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntoWarehouseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutOfWarehouseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IntoVATWarehouseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutOfVATWarehouseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZZ6_StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ZZ6_EndDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ZZ6_StartDateDateEdit.SuspendLayout();
			this.ZZ6_EndDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.RefCusProcedure);
			// 
			// ZZ6_CategoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZ6_CategoryTextBox, "ZZ6_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_Category)));
			this.ZZ6_CategoryTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("AC9FE80D-2193-481F-A714-CD3DDFAE313D", "Category");
			this.ZZ6_CategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 306, true);
			this.ZZ6_CategoryTextBox.Name = "ZZ6_CategoryTextBox";
			this.ZZ6_CategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.ZZ6_CategoryTextBox.TabIndex = 8;
			// 
			// PreviousProcedureCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousProcedureCodeTextBox, "ZZ6_PreviousProcedureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_PreviousProcedureCode)));
			this.PreviousProcedureCodeTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("03284ae1-9451-4748-acc4-97ad41f9c3a8", "Previous Procedure Code");
			this.PreviousProcedureCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 46, true);
			this.PreviousProcedureCodeTextBox.Name = "PreviousProcedureCodeTextBox";
			this.PreviousProcedureCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.PreviousProcedureCodeTextBox.TabIndex = 2;
			// 
			// CountryOrGroupingCodeTextBox
			// 
			this.CountryOrGroupingCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOrGroupingCodeTextBox, "ZZ6_Group");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_Group)));
			this.CountryOrGroupingCodeTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("64FE128B-32C0-4FEC-9E67-824525168D3D", "Group");
			this.CountryOrGroupingCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 284, true);
			this.CountryOrGroupingCodeTextBox.Name = "CountryOrGroupingCodeTextBox";
			this.CountryOrGroupingCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.CountryOrGroupingCodeTextBox.TabIndex = 7;
			// 
			// ZZ6_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZ6_DescriptionTextBox, "ZZ6_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_Description)));
			this.ZZ6_DescriptionTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("94EDF31E-AD6F-4E3A-BB8D-6CCFCC641EEE", "Description");
			this.ZZ6_DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ZZ6_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 90, true);
			this.ZZ6_DescriptionTextBox.Multiline = true;
			this.ZZ6_DescriptionTextBox.Name = "ZZ6_DescriptionTextBox";
			this.ZZ6_DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ZZ6_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 145, true);
			this.ZZ6_DescriptionTextBox.TabIndex = 4;
			// 
			// ZZ6_ProcedureCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZ6_ProcedureCodeTextBox, "ZZ6_ProcedureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_ProcedureCode)));
			this.ZZ6_ProcedureCodeTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("87C459A5-F10D-428C-8E04-CCFD6CB82305", "Procedure Code");
			this.ZZ6_ProcedureCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 24, true);
			this.ZZ6_ProcedureCodeTextBox.Name = "ZZ6_ProcedureCodeTextBox";
			this.ZZ6_ProcedureCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.ZZ6_ProcedureCodeTextBox.TabIndex = 1;
			// 
			// ConcessionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConcessionTextBox, "ZZ6_Concession");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_Concession)));
			this.ConcessionTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("94400e32-7fdf-499e-b2e0-326c108f4a49", "Concession");
			this.ConcessionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 68, true);
			this.ConcessionTextBox.Name = "ConcessionTextBox";
			this.ConcessionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.ConcessionTextBox.TabIndex = 3;
			// 
			// DataGroupingTextBox
			// 
			this.BindingSource.SetBindingMember(this.DataGroupingTextBox, "ZZ6_ZZZ_NKDataGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_ZZZ_NKDataGrouping)));
			this.DataGroupingTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("b298e4d6-9371-44ea-85fa-d8fdd92506d0", "Data Grouping");
			this.DataGroupingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 239, true);
			this.DataGroupingTextBox.Name = "DataGroupingTextBox";
			this.DataGroupingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.DataGroupingTextBox.TabIndex = 5;
			// 
			// ShipmentTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTypeTextBox, "ZZ6_ShipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_ShipmentType)));
			this.ShipmentTypeTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("de9e4cab-1e05-4a7d-8469-ae1549c0236d", "Shipment Type");
			this.ShipmentTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 262, true);
			this.ShipmentTypeTextBox.Name = "ShipmentTypeTextBox";
			this.ShipmentTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.ShipmentTypeTextBox.TabIndex = 6;
			// 
			// CalculateDutyCheckBox
			// 
			this.CalculateDutyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CalculateDutyCheckBox, "ZZ6_CalculateDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_CalculateDuty)));
			this.CalculateDutyCheckBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("5af4d8ef-ddaf-45e4-98c4-b072a97dbf9a", "Calculate Duty");
			this.CalculateDutyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CalculateDutyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CalculateDutyCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.CalculateDutyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 330, true);
			this.CalculateDutyCheckBox.Name = "CalculateDutyCheckBox";
			this.CalculateDutyCheckBox.ReadOnly = true;
			this.CalculateDutyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CalculateDutyCheckBox.TabIndex = 9;
			// 
			// LandedCostCheckBox
			// 
			this.LandedCostCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LandedCostCheckBox, "ZZ6_LandedCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_LandedCost)));
			this.LandedCostCheckBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("99106e50-813c-4f36-acc4-7327df8d79ed", "Landed Cost");
			this.LandedCostCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LandedCostCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LandedCostCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.LandedCostCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 330, true);
			this.LandedCostCheckBox.Name = "LandedCostCheckBox";
			this.LandedCostCheckBox.ReadOnly = true;
			this.LandedCostCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.LandedCostCheckBox.TabIndex = 10;
			// 
			// IntoWarehouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.IntoWarehouseTextBox, "ZZ6_IntoWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_IntoWarehouse)));
			this.IntoWarehouseTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("196a16fe-93c6-4597-903d-a3547a9d913d", "Into Warehouse");
			this.IntoWarehouseTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IntoWarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 327, true);
			this.IntoWarehouseTextBox.Name = "IntoWarehouseTextBox";
			this.IntoWarehouseTextBox.ReadOnly = true;
			this.IntoWarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.IntoWarehouseTextBox.TabIndex = 11;
			// 
			// OutOfWarehouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutOfWarehouseTextBox, "ZZ6_OutOfWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_OutOfWarehouse)));
			this.OutOfWarehouseTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("d87ccd2f-2da0-4e96-96b4-69f4dee2a604", "Out of Warehouse");
			this.OutOfWarehouseTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.OutOfWarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 327, true);
			this.OutOfWarehouseTextBox.Name = "OutOfWarehouseTextBox";
			this.OutOfWarehouseTextBox.ReadOnly = true;
			this.OutOfWarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.OutOfWarehouseTextBox.TabIndex = 12;
			// 
			// ZZ6_StartDateDateEdit
			// 
			this.ZZ6_StartDateDateEdit.AllowDrop = true;
			this.ZZ6_StartDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ZZ6_StartDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ZZ6_StartDateDateEdit, "ZZ6_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_StartDate)));
			this.ZZ6_StartDateDateEdit.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("A9043ABB-9265-4031-991F-8F8180DF8EC6", "From");
			this.ZZ6_StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 347, true);
			this.ZZ6_StartDateDateEdit.Name = "ZZ6_StartDateDateEdit";
			this.ZZ6_StartDateDateEdit.TabIndex = 13;
			// 
			// ZZ6_EndDateDateEdit
			// 
			this.ZZ6_EndDateDateEdit.AllowDrop = true;
			this.ZZ6_EndDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ZZ6_EndDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ZZ6_EndDateDateEdit, "ZZ6_EndDate_ForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_EndDate_ForDisplay)));
			this.ZZ6_EndDateDateEdit.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("1CD69B44-5899-43A0-A4B9-8CAE98133FED", "To");
			this.ZZ6_EndDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 347, true);
			this.ZZ6_EndDateDateEdit.Name = "ZZ6_EndDateDateEdit";
			this.ZZ6_EndDateDateEdit.TabIndex = 14;
			// 
			// IntoVATWarehouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.IntoVATWarehouseTextBox, "ZZ6_IntoVATWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_IntoVATWarehouse)));
			this.IntoVATWarehouseTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("B400E8CF-CD06-42B8-AFBF-AE3F256C0819", "Into VAT Warehouse");
			this.IntoVATWarehouseTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IntoVATWarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 347, true);
			this.IntoVATWarehouseTextBox.Name = "IntoVATWarehouseTextBox";
			this.IntoVATWarehouseTextBox.ReadOnly = true;
			this.IntoVATWarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.IntoVATWarehouseTextBox.TabIndex = 15;
			// 
			// OutOfVATWarehouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutOfVATWarehouseTextBox, "ZZ6_OutOfVATWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefCusProcedure)(null)).ZZ6_OutOfVATWarehouse)));
			this.OutOfVATWarehouseTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("5221081B-6403-4102-8DA5-629219348244", "Out of VAT Warehouse");
			this.OutOfVATWarehouseTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.OutOfVATWarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 347, true);
			this.OutOfVATWarehouseTextBox.Name = "OutOfVATWarehouseTextBox";
			this.OutOfVATWarehouseTextBox.ReadOnly = true;
			this.OutOfVATWarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.OutOfVATWarehouseTextBox.TabIndex = 16;
			// 
			// RefCusProcedureUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OutOfVATWarehouseTextBox);
			this.Controls.Add(this.IntoVATWarehouseTextBox);
			this.Controls.Add(this.OutOfWarehouseTextBox);
			this.Controls.Add(this.IntoWarehouseTextBox);
			this.Controls.Add(this.LandedCostCheckBox);
			this.Controls.Add(this.CalculateDutyCheckBox);
			this.Controls.Add(this.ZZ6_EndDateDateEdit);
			this.Controls.Add(this.ZZ6_StartDateDateEdit);
			this.Controls.Add(this.ShipmentTypeTextBox);
			this.Controls.Add(this.DataGroupingTextBox);
			this.Controls.Add(this.ConcessionTextBox);
			this.Controls.Add(this.PreviousProcedureCodeTextBox);
			this.Controls.Add(this.CountryOrGroupingCodeTextBox);
			this.Controls.Add(this.ZZ6_DescriptionTextBox);
			this.Controls.Add(this.ZZ6_ProcedureCodeTextBox);
			this.Controls.Add(this.ZZ6_CategoryTextBox);
			this.Name = "RefCusProcedureUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ZZ6_StartDateDateEdit.ResumeLayout(true);
			this.ZZ6_StartDateDateEdit.PerformLayout();
			this.ZZ6_EndDateDateEdit.ResumeLayout(true);
			this.ZZ6_EndDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox ZZ6_CategoryTextBox;
		private ZArchitecture.ZTextBox PreviousProcedureCodeTextBox;
		private ZArchitecture.ZTextBox CountryOrGroupingCodeTextBox;
		private ZArchitecture.ZTextBox ZZ6_DescriptionTextBox;
		private ZArchitecture.ZTextBox ZZ6_ProcedureCodeTextBox;
		private ZArchitecture.ZTextBox ConcessionTextBox;
		private ZArchitecture.ZTextBox DataGroupingTextBox;
		private ZArchitecture.ZTextBox ShipmentTypeTextBox;
		private ZArchitecture.ZTextBox IntoWarehouseTextBox;
		private ZArchitecture.ZTextBox OutOfWarehouseTextBox;
		private ZArchitecture.ZTextBox IntoVATWarehouseTextBox;
		private ZArchitecture.ZTextBox OutOfVATWarehouseTextBox;
		private ZArchitecture.GUI.ZDateEdit ZZ6_EndDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit ZZ6_StartDateDateEdit;
		private ZArchitecture.GUI.ZCheckBox CalculateDutyCheckBox;
		private ZArchitecture.GUI.ZCheckBox LandedCostCheckBox;
	}
}
