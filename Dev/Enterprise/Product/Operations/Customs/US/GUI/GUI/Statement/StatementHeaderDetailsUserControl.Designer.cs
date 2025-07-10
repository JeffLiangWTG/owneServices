namespace Enterprise.Customs.US.GUI
{
	partial class StatementHeaderDetailsUserControl
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
			this.FinalTotalAmountDueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalAmountDueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FinalTotalAmountDueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAmountDueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.BranchDesignationZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchDesignationZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImporterCustomsIDZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.PrintDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ProcessPortZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessPortZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EntryFilerCodeZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryFilerCodeZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrintDateZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StatementNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementNumberZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaymentStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaymentStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentTypeZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StatementStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatementStatusZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PayerUnitNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentPartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PayerUnitNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CheckNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CheckNoLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusStatementHeader);
			// 
			// FinalTotalAmountDueLabel
			// 
			this.FinalTotalAmountDueLabel.AutoSize = true;
			this.FinalTotalAmountDueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 131, true);
			this.FinalTotalAmountDueLabel.Name = "FinalTotalAmountDueLabel";
			this.FinalTotalAmountDueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.FinalTotalAmountDueLabel.TabIndex = 15;
			this.FinalTotalAmountDueLabel.Text = "Final Total Amount:";
			// 
			// TotalAmountDueLabel
			// 
			this.TotalAmountDueLabel.AutoSize = true;
			this.TotalAmountDueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 131, true);
			this.TotalAmountDueLabel.Name = "TotalAmountDueLabel";
			this.TotalAmountDueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.TotalAmountDueLabel.TabIndex = 28;
			this.TotalAmountDueLabel.Text = "Total Amount:";
			// 
			// FinalTotalAmountDueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FinalTotalAmountDueCalcEdit, "FinalTotalAmountDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).FinalTotalAmountDue)));
			this.FinalTotalAmountDueCalcEdit.DecimalPlaces = 2;
			this.FinalTotalAmountDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 127, true);
			this.FinalTotalAmountDueCalcEdit.Name = "FinalTotalAmountDueCalcEdit";
			this.FinalTotalAmountDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.FinalTotalAmountDueCalcEdit.TabIndex = 16;
			this.FinalTotalAmountDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAmountDueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalAmountDueCalcEdit, "TotalAmountDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).TotalAmountDue)));
			this.TotalAmountDueCalcEdit.DecimalPlaces = 2;
			this.TotalAmountDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 127, true);
			this.TotalAmountDueCalcEdit.Name = "TotalAmountDueCalcEdit";
			this.TotalAmountDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.TotalAmountDueCalcEdit.TabIndex = 14;
			this.TotalAmountDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "B2_PaymentAuthorizationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_PaymentAuthorizationDate)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 103, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 10;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 107, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 13, true);
			this.zLabel1.TabIndex = 10;
			this.zLabel1.Text = "Payment Date:";
			// 
			// BranchDesignationZLabel
			// 
			this.BranchDesignationZLabel.AutoSize = true;
			this.BranchDesignationZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 6, true);
			this.BranchDesignationZLabel.Name = "BranchDesignationZLabel";
			this.BranchDesignationZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.BranchDesignationZLabel.TabIndex = 16;
			this.BranchDesignationZLabel.Text = "Branch Designation:";
			// 
			// BranchDesignationZTextBox
			// 
			this.BindingSource.SetBindingMember(this.BranchDesignationZTextBox, "B2_BranchDesignation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_BranchDesignation)));
			this.BranchDesignationZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(808, 2, true);
			this.BranchDesignationZTextBox.Name = "BranchDesignationZTextBox";
			this.BranchDesignationZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
			this.BranchDesignationZTextBox.TabIndex = 3;
			// 
			// ImporterFindBox
			// 
			this.ImporterFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterFindBox, "B2_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_OH_Importer)));
			this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 28, true);
			this.ImporterFindBox.Name = "ImporterFindBox";
			this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
			this.ImporterFindBox.TabIndex = 5;
			// 
			// ImporterCustomsIDZTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterCustomsIDZTextBox, "ImporterCustomsIDForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).ImporterCustomsIDForDisplay)));
			this.ImporterCustomsIDZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 28, true);
			this.ImporterCustomsIDZTextBox.Name = "ImporterCustomsIDZTextBox";
			this.ImporterCustomsIDZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.ImporterCustomsIDZTextBox.TabIndex = 4;
			// 
			// zLabel11
			// 
			this.zLabel11.AutoSize = true;
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 13, true);
			this.zLabel11.TabIndex = 2;
			this.zLabel11.Text = "Importer Customs ID:";
			// 
			// PrintDateDateEdit
			// 
			this.PrintDateDateEdit.AllowDrop = true;
			this.PrintDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PrintDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PrintDateDateEdit, "B2_PrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_PrintDate)));
			this.PrintDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 78, true);
			this.PrintDateDateEdit.Name = "PrintDateDateEdit";
			this.PrintDateDateEdit.TabIndex = 8;
			// 
			// ProcessPortZTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessPortZTextBox, "B2_ProcessPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_ProcessPort)));
			this.ProcessPortZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 2, true);
			this.ProcessPortZTextBox.Name = "ProcessPortZTextBox";
			this.ProcessPortZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.ProcessPortZTextBox.TabIndex = 2;
			this.ProcessPortZTextBox.Tag = "";
			// 
			// ProcessPortZLabel
			// 
			this.ProcessPortZLabel.AutoSize = true;
			this.ProcessPortZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(543, 6, true);
			this.ProcessPortZLabel.Name = "ProcessPortZLabel";
			this.ProcessPortZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.ProcessPortZLabel.TabIndex = 14;
			this.ProcessPortZLabel.Text = "Distric. Port:";
			// 
			// EntryFilerCodeZTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryFilerCodeZTextBox, "B2_EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_EntryFilerCode)));
			this.EntryFilerCodeZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 3, true);
			this.EntryFilerCodeZTextBox.Name = "EntryFilerCodeZTextBox";
			this.EntryFilerCodeZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.EntryFilerCodeZTextBox.TabIndex = 1;
			// 
			// EntryFilerCodeZLabel
			// 
			this.EntryFilerCodeZLabel.AutoSize = true;
			this.EntryFilerCodeZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 7, true);
			this.EntryFilerCodeZLabel.Name = "EntryFilerCodeZLabel";
			this.EntryFilerCodeZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.EntryFilerCodeZLabel.TabIndex = 12;
			this.EntryFilerCodeZLabel.Text = "Entry Filer Code:";
			// 
			// PrintDateZLabel
			// 
			this.PrintDateZLabel.AutoSize = true;
			this.PrintDateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.PrintDateZLabel.Name = "PrintDateZLabel";
			this.PrintDateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.PrintDateZLabel.TabIndex = 8;
			this.PrintDateZLabel.Text = "Print Date:";
			// 
			// StatementNumberZTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatementNumberZTextBox, "B2_StatementNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_StatementNumber)));
			this.StatementNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 3, true);
			this.StatementNumberZTextBox.Name = "StatementNumberZTextBox";
			this.StatementNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.StatementNumberZTextBox.TabIndex = 0;
			// 
			// StatementNumberZLabel
			// 
			this.StatementNumberZLabel.AutoSize = true;
			this.StatementNumberZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.StatementNumberZLabel.Name = "StatementNumberZLabel";
			this.StatementNumberZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.StatementNumberZLabel.TabIndex = 0;
			this.StatementNumberZLabel.Text = "Statement Number:";
			// 
			// ImporterZLabel
			// 
			this.ImporterZLabel.AutoSize = true;
			this.ImporterZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 32, true);
			this.ImporterZLabel.Name = "ImporterZLabel";
			this.ImporterZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 13, true);
			this.ImporterZLabel.TabIndex = 18;
			this.ImporterZLabel.Text = "Importer:";
			// 
			// PaymentStatusLabel
			// 
			this.PaymentStatusLabel.AutoSize = true;
			this.PaymentStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 82, true);
			this.PaymentStatusLabel.Name = "PaymentStatusLabel";
			this.PaymentStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.PaymentStatusLabel.TabIndex = 22;
			this.PaymentStatusLabel.Text = "Payment Status:";
			// 
			// PaymentStatusDropEdit
			// 
			this.PaymentStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentStatusDropEdit, "B2_PaymentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_PaymentStatus)));
			this.PaymentStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 78, true);
			this.PaymentStatusDropEdit.Name = "PaymentStatusDropEdit";
			this.PaymentStatusDropEdit.PreBoundMaxLength = 2;
			this.PaymentStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
			this.PaymentStatusDropEdit.TabIndex = 9;
			// 
			// PaymentTypeZDropEdit
			// 
			this.PaymentTypeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeZDropEdit, "B2_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_PaymentType)));
			this.PaymentTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 52, true);
			this.PaymentTypeZDropEdit.Name = "PaymentTypeZDropEdit";
			this.PaymentTypeZDropEdit.PreBoundMaxLength = 1;
			this.PaymentTypeZDropEdit.ShowDescriptionBox = false;
			this.PaymentTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PaymentTypeZDropEdit.TabIndex = 6;
			// 
			// PaymentTypeZLabel
			// 
			this.PaymentTypeZLabel.AutoSize = true;
			this.PaymentTypeZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.PaymentTypeZLabel.Name = "PaymentTypeZLabel";
			this.PaymentTypeZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.PaymentTypeZLabel.TabIndex = 4;
			this.PaymentTypeZLabel.Text = "Payment Type:";
			// 
			// StatementStatusDropEdit
			// 
			this.StatementStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementStatusDropEdit, "B2_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_Status)));
			this.StatementStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 52, true);
			this.StatementStatusDropEdit.Name = "StatementStatusDropEdit";
			this.StatementStatusDropEdit.PreBoundMaxLength = 2;
			this.StatementStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 20, true);
			this.StatementStatusDropEdit.TabIndex = 7;
			// 
			// StatementStatusZLabel
			// 
			this.StatementStatusZLabel.AutoSize = true;
			this.StatementStatusZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 56, true);
			this.StatementStatusZLabel.Name = "StatementStatusZLabel";
			this.StatementStatusZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.StatementStatusZLabel.TabIndex = 20;
			this.StatementStatusZLabel.Text = "Statement Status:";
			// 
			// PayerUnitNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PayerUnitNoTextBox, "B2_AccountNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_AccountNo)));
			this.PayerUnitNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 103, true);
			this.PayerUnitNoTextBox.Name = "PayerUnitNoTextBox";
			this.PayerUnitNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.PayerUnitNoTextBox.TabIndex = 11;
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "B2_PaymentParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_PaymentParty)));
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 103, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.PreBoundMaxLength = 2;
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 12;
			// 
			// PaymentPartyLabel
			// 
			this.PaymentPartyLabel.AutoSize = true;
			this.PaymentPartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 107, true);
			this.PaymentPartyLabel.Name = "PaymentPartyLabel";
			this.PaymentPartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.PaymentPartyLabel.TabIndex = 26;
			this.PaymentPartyLabel.Text = "Payment Party:";
			// 
			// PayerUnitNoLabel
			// 
			this.PayerUnitNoLabel.AutoSize = true;
			this.PayerUnitNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 107, true);
			this.PayerUnitNoLabel.Name = "PayerUnitNoLabel";
			this.PayerUnitNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.PayerUnitNoLabel.TabIndex = 24;
			this.PayerUnitNoLabel.Text = "Payer Unit No:";
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.CheckNoTextBox, "B2_CheckNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).B2_CheckNo)));
			this.CheckNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 127, true);
			this.CheckNoTextBox.Name = "CheckNoTextBox";
			this.CheckNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.CheckNoTextBox.TabIndex = 13;
			// 
			// CheckNoLabel
			// 
			this.CheckNoLabel.AutoSize = true;
			this.CheckNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 130, true);
			this.CheckNoLabel.Name = "CheckNoLabel";
			this.CheckNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.CheckNoLabel.TabIndex = 32;
			this.CheckNoLabel.Text = "Importer's Check No:";
			// 
			// StatementHeaderDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CheckNoTextBox);
			this.Controls.Add(this.CheckNoLabel);
			this.Controls.Add(this.PayerUnitNoTextBox);
			this.Controls.Add(this.PaymentPartyDropEdit);
			this.Controls.Add(this.PaymentPartyLabel);
			this.Controls.Add(this.PayerUnitNoLabel);
			this.Controls.Add(this.PaymentTypeZDropEdit);
			this.Controls.Add(this.PaymentTypeZLabel);
			this.Controls.Add(this.StatementStatusDropEdit);
			this.Controls.Add(this.StatementStatusZLabel);
			this.Controls.Add(this.PaymentStatusDropEdit);
			this.Controls.Add(this.PaymentStatusLabel);
			this.Controls.Add(this.FinalTotalAmountDueLabel);
			this.Controls.Add(this.TotalAmountDueLabel);
			this.Controls.Add(this.FinalTotalAmountDueCalcEdit);
			this.Controls.Add(this.TotalAmountDueCalcEdit);
			this.Controls.Add(this.zDateEdit1);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.BranchDesignationZLabel);
			this.Controls.Add(this.BranchDesignationZTextBox);
			this.Controls.Add(this.ImporterFindBox);
			this.Controls.Add(this.ImporterCustomsIDZTextBox);
			this.Controls.Add(this.zLabel11);
			this.Controls.Add(this.PrintDateDateEdit);
			this.Controls.Add(this.ProcessPortZTextBox);
			this.Controls.Add(this.ProcessPortZLabel);
			this.Controls.Add(this.EntryFilerCodeZTextBox);
			this.Controls.Add(this.EntryFilerCodeZLabel);
			this.Controls.Add(this.PrintDateZLabel);
			this.Controls.Add(this.StatementNumberZTextBox);
			this.Controls.Add(this.StatementNumberZLabel);
			this.Controls.Add(this.ImporterZLabel);
			this.Name = "StatementHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 155, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel FinalTotalAmountDueLabel;
		private Enterprise.ZArchitecture.ZLabel TotalAmountDueLabel;
		private Enterprise.ZArchitecture.ZCalcEdit FinalTotalAmountDueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalAmountDueCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel BranchDesignationZLabel;
		private Enterprise.ZArchitecture.ZTextBox BranchDesignationZTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterFindBox;
		private Enterprise.ZArchitecture.ZTextBox ImporterCustomsIDZTextBox;
		private Enterprise.ZArchitecture.ZLabel zLabel11;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PrintDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox ProcessPortZTextBox;
		private Enterprise.ZArchitecture.ZLabel ProcessPortZLabel;
		private Enterprise.ZArchitecture.ZTextBox EntryFilerCodeZTextBox;
		private Enterprise.ZArchitecture.ZLabel EntryFilerCodeZLabel;
		private Enterprise.ZArchitecture.ZLabel PrintDateZLabel;
		private Enterprise.ZArchitecture.ZTextBox StatementNumberZTextBox;
		private Enterprise.ZArchitecture.ZLabel StatementNumberZLabel;
		private Enterprise.ZArchitecture.ZLabel ImporterZLabel;
		private Enterprise.ZArchitecture.ZLabel PaymentStatusLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentStatusDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentTypeZDropEdit;
		private Enterprise.ZArchitecture.ZLabel PaymentTypeZLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatementStatusDropEdit;
		private Enterprise.ZArchitecture.ZLabel StatementStatusZLabel;
		private Enterprise.ZArchitecture.ZTextBox PayerUnitNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		private Enterprise.ZArchitecture.ZLabel PaymentPartyLabel;
		private Enterprise.ZArchitecture.ZLabel PayerUnitNoLabel;
		private ZArchitecture.ZTextBox CheckNoTextBox;
		private ZArchitecture.ZLabel CheckNoLabel;
	}
}
