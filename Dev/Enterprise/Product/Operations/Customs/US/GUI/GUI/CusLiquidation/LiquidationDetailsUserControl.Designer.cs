namespace Enterprise.Customs.US.GUI
{
	partial class LiquidationDetailsUserControl
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
			this.ExtensionSuspensionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImporterNoZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LiquidationDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ChangeLiqReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LiquidationTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NoOfSuspensionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomsDocumentFilingLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LiquidatedTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InterestAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExtensionSuspensionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TotalLiquidatedFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPaidFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LiqDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChangeLiqReasonCodeDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChangeLiqReasonCodeDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChangeLiqReasonCodeDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExstGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExtensionSuspensionCodeDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExtensionSuspensionCodeDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExtensionSuspensionCodeDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmountsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DutyPaidCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutyLiquidatedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ADDCVDGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExtensionSuspensionDateDateEdit.SuspendLayout();
			this.LiquidationDateZDateEdit.SuspendLayout();
			this.ChangeLiqReasonCodeDropEdit.SuspendLayout();
			this.LiquidationTypeZDropEdit.SuspendLayout();
			this.EntryDateDateEdit.SuspendLayout();
			this.ExtensionSuspensionCodeDropEdit.SuspendLayout();
			this.EntryDetailsGroupBox.SuspendLayout();
			this.LiqDetailsGroupBox.SuspendLayout();
			this.ChangeLiqReasonCodeDropEdit4.SuspendLayout();
			this.ChangeLiqReasonCodeDropEdit3.SuspendLayout();
			this.ChangeLiqReasonCodeDropEdit2.SuspendLayout();
			this.ExstGroupBox.SuspendLayout();
			this.ExtensionSuspensionCodeDropEdit4.SuspendLayout();
			this.ExtensionSuspensionCodeDropEdit3.SuspendLayout();
			this.ExtensionSuspensionCodeDropEdit2.SuspendLayout();
			this.AmountsGroupBox.SuspendLayout();
			this.ADDCVDGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusLiquidation);
			// 
			// ExtensionSuspensionDateDateEdit
			// 
			this.ExtensionSuspensionDateDateEdit.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.ExtensionSuspensionDateDateEdit.AllowDrop = true;
			this.ExtensionSuspensionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExtensionSuspensionDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExtensionSuspensionDateDateEdit, "B8_ExtensionSuspensionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionDate)));
			this.ExtensionSuspensionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 15, true);
			this.ExtensionSuspensionDateDateEdit.Name = "ExtensionSuspensionDateDateEdit";
			this.ExtensionSuspensionDateDateEdit.TabIndex = 0;
			// 
			// ImporterNoZTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterNoZTextBox, "ImporterOfRecordNumberForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).ImporterOfRecordNumberForDisplay)));
			this.ImporterNoZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 45, true);
			this.ImporterNoZTextBox.Name = "ImporterNoZTextBox";
			this.ImporterNoZTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EEAA669A-4B08-4741-A1D8-C09D3FCF0D81", "Importer Of Record No");
			this.ImporterNoZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.ImporterNoZTextBox.TabIndex = 1;
			// 
			// LiquidationDateZDateEdit
			// 
			this.LiquidationDateZDateEdit.AllowDrop = true;
			this.LiquidationDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.LiquidationDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LiquidationDateZDateEdit, "B8_LiquidationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidationDate)));
			this.LiquidationDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 44, true);
			this.LiquidationDateZDateEdit.Name = "LiquidationDateZDateEdit";
			this.LiquidationDateZDateEdit.TabIndex = 2;
			// 
			// ChangeLiqReasonCodeDropEdit
			// 
			this.ChangeLiqReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChangeLiqReasonCodeDropEdit, "B8_ChangeLiquidationReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ChangeLiquidationReasonCode)));
			this.ChangeLiqReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 70, true);
			this.ChangeLiqReasonCodeDropEdit.Name = "ChangeLiqReasonCodeDropEdit";
			this.ChangeLiqReasonCodeDropEdit.PreBoundMaxLength = 3;
			this.ChangeLiqReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.ChangeLiqReasonCodeDropEdit.TabIndex = 0;
			// 
			// LiquidationTypeZDropEdit
			// 
			this.LiquidationTypeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LiquidationTypeZDropEdit, "B8_LiquidationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidationType)));
			this.LiquidationTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 18, true);
			this.LiquidationTypeZDropEdit.Name = "LiquidationTypeZDropEdit";
			this.LiquidationTypeZDropEdit.PreBoundMaxLength = 1;
			this.LiquidationTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.LiquidationTypeZDropEdit.TabIndex = 1;
			// 
			// NoOfSuspensionsTextBox
			// 
			this.NoOfSuspensionsTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
			this.BindingSource.SetBindingMember(this.NoOfSuspensionsTextBox, "B8_NoOfSuspensions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_NoOfSuspensions)));
			this.NoOfSuspensionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(641, 15, true);
			this.NoOfSuspensionsTextBox.Name = "NoOfSuspensionsTextBox";
			this.NoOfSuspensionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.NoOfSuspensionsTextBox.TabIndex = 1;
			// 
			// EntryDateDateEdit
			// 
			this.EntryDateDateEdit.AllowDrop = true;
			this.EntryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EntryDateDateEdit.AutoCompleteYear = false;
			this.BindingSource.SetBindingMember(this.EntryDateDateEdit, "B8_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_EntryDate)));
			this.EntryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 19, true);
			this.EntryDateDateEdit.Name = "EntryDateDateEdit";
			this.EntryDateDateEdit.TabIndex = 0;
			// 
			// CustomsDocumentFilingLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsDocumentFilingLocationTextBox, "B8_CustomsDocumentFilingLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_CustomsDocumentFilingLocation)));
			this.CustomsDocumentFilingLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 72, true);
			this.CustomsDocumentFilingLocationTextBox.Name = "CustomsDocumentFilingLocationTextBox";
			this.CustomsDocumentFilingLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.CustomsDocumentFilingLocationTextBox.TabIndex = 2;
			this.CustomsDocumentFilingLocationTextBox.Tag = "";
			// 
			// LiquidatedTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LiquidatedTaxCalcEdit, "B8_LiquidatedTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidatedTax)));
			this.LiquidatedTaxCalcEdit.DecimalPlaces = 2;
			this.LiquidatedTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 45, true);
			this.LiquidatedTaxCalcEdit.Name = "LiquidatedTaxCalcEdit";
			this.LiquidatedTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.LiquidatedTaxCalcEdit.TabIndex = 1;
			this.LiquidatedTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InterestAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InterestAmountCalcEdit, "B8_InterestAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_InterestAmount)));
			this.InterestAmountCalcEdit.DecimalPlaces = 2;
			this.InterestAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 19, true);
			this.InterestAmountCalcEdit.Name = "InterestAmountCalcEdit";
			this.InterestAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.InterestAmountCalcEdit.TabIndex = 0;
			this.InterestAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExtensionSuspensionCodeDropEdit
			// 
			this.ExtensionSuspensionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtensionSuspensionCodeDropEdit, "B8_ExtensionSuspensionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionCode)));
			this.ExtensionSuspensionCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ba5ed965-dc65-4cb3-93ab-56daae5e9cdd", "Extension Suspension Code");
			this.ExtensionSuspensionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 41, true);
			this.ExtensionSuspensionCodeDropEdit.Name = "ExtensionSuspensionCodeDropEdit";
			this.ExtensionSuspensionCodeDropEdit.PreBoundMaxLength = 1;
			this.ExtensionSuspensionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.ExtensionSuspensionCodeDropEdit.TabIndex = 2;
			// 
			// TotalLiquidatedFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalLiquidatedFeesCalcEdit, "B8_TotalLiquidatedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalLiquidatedFees)));
			this.TotalLiquidatedFeesCalcEdit.DecimalPlaces = 2;
			this.TotalLiquidatedFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 98, true);
			this.TotalLiquidatedFeesCalcEdit.Name = "TotalLiquidatedFeesCalcEdit";
			this.TotalLiquidatedFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.TotalLiquidatedFeesCalcEdit.TabIndex = 3;
			this.TotalLiquidatedFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPaidFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPaidFeesCalcEdit, "B8_TotalPaidFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalPaidFees)));
			this.TotalPaidFeesCalcEdit.DecimalPlaces = 2;
			this.TotalPaidFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 71, true);
			this.TotalPaidFeesCalcEdit.Name = "TotalPaidFeesCalcEdit";
			this.TotalPaidFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.TotalPaidFeesCalcEdit.TabIndex = 2;
			this.TotalPaidFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EntryDetailsGroupBox
			// 
			this.EntryDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d650605d-1fd5-4119-a6c8-7f8f2907c13e", "Common Details");
			this.EntryDetailsGroupBox.Controls.Add(this.EntryDateDateEdit);
			this.EntryDetailsGroupBox.Controls.Add(this.ImporterNoZTextBox);
			this.EntryDetailsGroupBox.Controls.Add(this.CustomsDocumentFilingLocationTextBox);
			this.EntryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 1, true);
			this.EntryDetailsGroupBox.Name = "EntryDetailsGroupBox";
			this.EntryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 181, true);
			this.EntryDetailsGroupBox.TabIndex = 0;
			this.EntryDetailsGroupBox.TabStop = false;
			// 
			// LiqDetailsGroupBox
			// 
			this.LiqDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2214ac6f-ce2c-4126-9694-210051acfb00", "Liquidation Details");
			this.LiqDetailsGroupBox.Controls.Add(this.ChangeLiqReasonCodeDropEdit4);
			this.LiqDetailsGroupBox.Controls.Add(this.ChangeLiqReasonCodeDropEdit3);
			this.LiqDetailsGroupBox.Controls.Add(this.ChangeLiqReasonCodeDropEdit2);
			this.LiqDetailsGroupBox.Controls.Add(this.ChangeLiqReasonCodeDropEdit);
			this.LiqDetailsGroupBox.Controls.Add(this.LiquidationTypeZDropEdit);
			this.LiqDetailsGroupBox.Controls.Add(this.LiquidationDateZDateEdit);
			this.LiqDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 2, true);
			this.LiqDetailsGroupBox.Name = "LiqDetailsGroupBox";
			this.LiqDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 180, true);
			this.LiqDetailsGroupBox.TabIndex = 1;
			this.LiqDetailsGroupBox.TabStop = false;
			// 
			// ChangeLiqReasonCodeDropEdit4
			// 
			this.ChangeLiqReasonCodeDropEdit4.AllowDrop = true;
			this.ChangeLiqReasonCodeDropEdit4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.BindingSource.SetBindingMember(this.ChangeLiqReasonCodeDropEdit4, "B8_ChangeLiquidationReasonCode4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ChangeLiquidationReasonCode4)));
			this.ChangeLiqReasonCodeDropEdit4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5dcfba51-5acb-473f-9d81-400b76095db5", "Change Liquidation Reason Code 4");
			this.ChangeLiqReasonCodeDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 148, true);
			this.ChangeLiqReasonCodeDropEdit4.Name = "ChangeLiqReasonCodeDropEdit4";
			this.ChangeLiqReasonCodeDropEdit4.PreBoundMaxLength = 3;
			this.ChangeLiqReasonCodeDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.ChangeLiqReasonCodeDropEdit4.TabIndex = 5;
			// 
			// ChangeLiqReasonCodeDropEdit3
			// 
			this.ChangeLiqReasonCodeDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChangeLiqReasonCodeDropEdit3, "B8_ChangeLiquidationReasonCode3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ChangeLiquidationReasonCode3)));
			this.ChangeLiqReasonCodeDropEdit3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2a1d0a43-d79b-47c6-a345-d4034869a596", "Change Liquidation Reason Code 3");
			this.ChangeLiqReasonCodeDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 122, true);
			this.ChangeLiqReasonCodeDropEdit3.Name = "ChangeLiqReasonCodeDropEdit3";
			this.ChangeLiqReasonCodeDropEdit3.PreBoundMaxLength = 3;
			this.ChangeLiqReasonCodeDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.ChangeLiqReasonCodeDropEdit3.TabIndex = 4;
			// 
			// ChangeLiqReasonCodeDropEdit2
			// 
			this.ChangeLiqReasonCodeDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChangeLiqReasonCodeDropEdit2, "B8_ChangeLiquidationReasonCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ChangeLiquidationReasonCode2)));
			this.ChangeLiqReasonCodeDropEdit2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6b4f9bba-ad20-4fbe-9d13-564b1c718a83", "Change Liquidation Reason Code 2");
			this.ChangeLiqReasonCodeDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 96, true);
			this.ChangeLiqReasonCodeDropEdit2.Name = "ChangeLiqReasonCodeDropEdit2";
			this.ChangeLiqReasonCodeDropEdit2.PreBoundMaxLength = 3;
			this.ChangeLiqReasonCodeDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.ChangeLiqReasonCodeDropEdit2.TabIndex = 3;
			// 
			// ExstGroupBox
			// 
			this.ExstGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0c6d930e-bcd4-4a2d-93c0-7894ab26645c", "Extension Suspension Details");
			this.ExstGroupBox.Controls.Add(this.ExtensionSuspensionCodeDropEdit4);
			this.ExstGroupBox.Controls.Add(this.ExtensionSuspensionCodeDropEdit3);
			this.ExstGroupBox.Controls.Add(this.ExtensionSuspensionCodeDropEdit2);
			this.ExstGroupBox.Controls.Add(this.ExtensionSuspensionDateDateEdit);
			this.ExstGroupBox.Controls.Add(this.ExtensionSuspensionCodeDropEdit);
			this.ExstGroupBox.Controls.Add(this.NoOfSuspensionsTextBox);
			this.ExstGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 183, true);
			this.ExstGroupBox.Name = "ExstGroupBox";
			this.ExstGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 97, true);
			this.ExstGroupBox.TabIndex = 2;
			this.ExstGroupBox.TabStop = false;
			// 
			// ExtensionSuspensionCodeDropEdit4
			// 
			this.ExtensionSuspensionCodeDropEdit4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtensionSuspensionCodeDropEdit4, "B8_ExtensionSuspensionCode4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionCode4)));
			this.ExtensionSuspensionCodeDropEdit4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bf5f326c-8ca7-46a0-bf1d-e6c038245d6c", "Extension Suspension Code 4");
			this.ExtensionSuspensionCodeDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(641, 67, true);
			this.ExtensionSuspensionCodeDropEdit4.Name = "ExtensionSuspensionCodeDropEdit4";
			this.ExtensionSuspensionCodeDropEdit4.PreBoundMaxLength = 1;
			this.ExtensionSuspensionCodeDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.ExtensionSuspensionCodeDropEdit4.TabIndex = 5;
			// 
			// ExtensionSuspensionCodeDropEdit3
			// 
			this.ExtensionSuspensionCodeDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtensionSuspensionCodeDropEdit3, "B8_ExtensionSuspensionCode3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionCode3)));
			this.ExtensionSuspensionCodeDropEdit3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9e2b4da2-cd52-40d3-94a6-bb62001d7702", "Extension Suspension Code 3");
			this.ExtensionSuspensionCodeDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 67, true);
			this.ExtensionSuspensionCodeDropEdit3.Name = "ExtensionSuspensionCodeDropEdit3";
			this.ExtensionSuspensionCodeDropEdit3.PreBoundMaxLength = 1;
			this.ExtensionSuspensionCodeDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.ExtensionSuspensionCodeDropEdit3.TabIndex = 4;
			// 
			// ExtensionSuspensionCodeDropEdit2
			// 
			this.ExtensionSuspensionCodeDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtensionSuspensionCodeDropEdit2, "B8_ExtensionSuspensionCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionCode2)));
			this.ExtensionSuspensionCodeDropEdit2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5c74930c-2b4d-40ed-bcd4-3b3e060c7f70", "Extension Suspension Code 2");
			this.ExtensionSuspensionCodeDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(641, 41, true);
			this.ExtensionSuspensionCodeDropEdit2.Name = "ExtensionSuspensionCodeDropEdit2";
			this.ExtensionSuspensionCodeDropEdit2.PreBoundMaxLength = 1;
			this.ExtensionSuspensionCodeDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.ExtensionSuspensionCodeDropEdit2.TabIndex = 3;
			// 
			// AmountsGroupBox
			// 
			this.AmountsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f153c145-a408-47d5-8451-4af88157887e", "Liquidation Amounts");
			this.AmountsGroupBox.Controls.Add(this.DutyPaidCalcEdit);
			this.AmountsGroupBox.Controls.Add(this.DutyLiquidatedCalcEdit);
			this.AmountsGroupBox.Controls.Add(this.InterestAmountCalcEdit);
			this.AmountsGroupBox.Controls.Add(this.LiquidatedTaxCalcEdit);
			this.AmountsGroupBox.Controls.Add(this.TotalLiquidatedFeesCalcEdit);
			this.AmountsGroupBox.Controls.Add(this.TotalPaidFeesCalcEdit);
			this.AmountsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 282, true);
			this.AmountsGroupBox.Name = "AmountsGroupBox";
			this.AmountsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 125, true);
			this.AmountsGroupBox.TabIndex = 3;
			this.AmountsGroupBox.TabStop = false;
			// 
			// DutyPaidCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyPaidCalcEdit, "B8_DutyPaid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_DutyPaid)));
			this.DutyPaidCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("613a1190-dffa-49a9-9720-de553c42059d", "Duty Paid");
			this.DutyPaidCalcEdit.DecimalPlaces = 2;
			this.DutyPaidCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 19, true);
			this.DutyPaidCalcEdit.Name = "DutyPaidCalcEdit";
			this.DutyPaidCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.DutyPaidCalcEdit.TabIndex = 4;
			this.DutyPaidCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutyLiquidatedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyLiquidatedCalcEdit, "B8_LiquidatedDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidatedDuty)));
			this.DutyLiquidatedCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7a5d60dc-9d6c-4982-83f8-87f38385c153", "Duty Liquidated");
			this.DutyLiquidatedCalcEdit.DecimalPlaces = 2;
			this.DutyLiquidatedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 45, true);
			this.DutyLiquidatedCalcEdit.Name = "DutyLiquidatedCalcEdit";
			this.DutyLiquidatedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.DutyLiquidatedCalcEdit.TabIndex = 5;
			this.DutyLiquidatedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ADDCVDGroupBox
			// 
			this.ADDCVDGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("663da9df-23f4-4925-b93b-ed24bbf5d30f", "ADD/CVD Amounts");
			this.ADDCVDGroupBox.Controls.Add(this.zCalcEdit1);
			this.ADDCVDGroupBox.Controls.Add(this.zCalcEdit2);
			this.ADDCVDGroupBox.Controls.Add(this.zCalcEdit3);
			this.ADDCVDGroupBox.Controls.Add(this.zCalcEdit4);
			this.ADDCVDGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 282, true);
			this.ADDCVDGroupBox.Name = "ADDCVDGroupBox";
			this.ADDCVDGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 125, true);
			this.ADDCVDGroupBox.TabIndex = 4;
			this.ADDCVDGroupBox.TabStop = false;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "B8_TotalPaidCounterVailingDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalPaidCounterVailingDuty)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 19, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.zCalcEdit1.TabIndex = 0;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "B8_TotalPaidAntiDumpingDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalPaidAntiDumpingDuty)));
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 71, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.zCalcEdit2.TabIndex = 2;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "B8_TotalLiquidatedCounterVailingDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalLiquidatedCounterVailingDuty)));
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 45, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.zCalcEdit3.TabIndex = 1;
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "B8_TotalLiquidatedAntiDumpingDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalLiquidatedAntiDumpingDuty)));
			this.zCalcEdit4.DecimalPlaces = 2;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 98, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.zCalcEdit4.TabIndex = 3;
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LiquidationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ADDCVDGroupBox);
			this.Controls.Add(this.AmountsGroupBox);
			this.Controls.Add(this.ExstGroupBox);
			this.Controls.Add(this.LiqDetailsGroupBox);
			this.Controls.Add(this.EntryDetailsGroupBox);
			this.Name = "LiquidationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 414, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExtensionSuspensionDateDateEdit.ResumeLayout(true);
			this.ExtensionSuspensionDateDateEdit.PerformLayout();
			this.LiquidationDateZDateEdit.ResumeLayout(true);
			this.LiquidationDateZDateEdit.PerformLayout();
			this.ChangeLiqReasonCodeDropEdit.ResumeLayout(true);
			this.ChangeLiqReasonCodeDropEdit.PerformLayout();
			this.LiquidationTypeZDropEdit.ResumeLayout(true);
			this.LiquidationTypeZDropEdit.PerformLayout();
			this.EntryDateDateEdit.ResumeLayout(true);
			this.EntryDateDateEdit.PerformLayout();
			this.ExtensionSuspensionCodeDropEdit.ResumeLayout(true);
			this.ExtensionSuspensionCodeDropEdit.PerformLayout();
			this.EntryDetailsGroupBox.ResumeLayout(false);
			this.EntryDetailsGroupBox.PerformLayout();
			this.LiqDetailsGroupBox.ResumeLayout(false);
			this.LiqDetailsGroupBox.PerformLayout();
			this.ChangeLiqReasonCodeDropEdit4.ResumeLayout(true);
			this.ChangeLiqReasonCodeDropEdit4.PerformLayout();
			this.ChangeLiqReasonCodeDropEdit3.ResumeLayout(true);
			this.ChangeLiqReasonCodeDropEdit3.PerformLayout();
			this.ChangeLiqReasonCodeDropEdit2.ResumeLayout(true);
			this.ChangeLiqReasonCodeDropEdit2.PerformLayout();
			this.ExstGroupBox.ResumeLayout(false);
			this.ExstGroupBox.PerformLayout();
			this.ExtensionSuspensionCodeDropEdit4.ResumeLayout(true);
			this.ExtensionSuspensionCodeDropEdit4.PerformLayout();
			this.ExtensionSuspensionCodeDropEdit3.ResumeLayout(true);
			this.ExtensionSuspensionCodeDropEdit3.PerformLayout();
			this.ExtensionSuspensionCodeDropEdit2.ResumeLayout(true);
			this.ExtensionSuspensionCodeDropEdit2.PerformLayout();
			this.AmountsGroupBox.ResumeLayout(false);
			this.AmountsGroupBox.PerformLayout();
			this.ADDCVDGroupBox.ResumeLayout(false);
			this.ADDCVDGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDateEdit ExtensionSuspensionDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox ImporterNoZTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit LiquidationDateZDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChangeLiqReasonCodeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit LiquidationTypeZDropEdit;
		private Enterprise.ZArchitecture.ZTextBox NoOfSuspensionsTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EntryDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox CustomsDocumentFilingLocationTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit LiquidatedTaxCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit InterestAmountCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ExtensionSuspensionCodeDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalLiquidatedFeesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalPaidFeesCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EntryDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LiqDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ExstGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AmountsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ADDCVDGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit2;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit3;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit4;
		private ZArchitecture.ZCalcEdit DutyPaidCalcEdit;
		private ZArchitecture.ZCalcEdit DutyLiquidatedCalcEdit;
		private ZArchitecture.GUI.ZDropEdit ExtensionSuspensionCodeDropEdit2;
		private ZArchitecture.GUI.ZDropEdit ChangeLiqReasonCodeDropEdit4;
		private ZArchitecture.GUI.ZDropEdit ChangeLiqReasonCodeDropEdit3;
		private ZArchitecture.GUI.ZDropEdit ChangeLiqReasonCodeDropEdit2;
		private ZArchitecture.GUI.ZDropEdit ExtensionSuspensionCodeDropEdit4;
		private ZArchitecture.GUI.ZDropEdit ExtensionSuspensionCodeDropEdit3;
	}
}
