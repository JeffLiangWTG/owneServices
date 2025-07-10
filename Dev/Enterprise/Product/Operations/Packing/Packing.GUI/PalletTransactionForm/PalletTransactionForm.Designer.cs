using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Packing.GUI
{
	partial class PalletTransactionForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.numbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PalletTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransactionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PapaerDocketIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.ActionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PalletTransactionIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDocAddressControl2 = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.zDocAddressControl1 = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.TransferFromTradingAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransferToTradingAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.numbersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.PalletTypeDropEdit.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.TransactionTypeDropEdit.SuspendLayout();
			this.zDropEdit3.SuspendLayout();
			this.ActionTypeDropEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zDocAddressControl2.SuspendLayout();
			this.zDocAddressControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.numbersTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 495, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.numbersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.TransferToTradingAccountNumberTextBox);
			this.MainTabPage.Controls.Add(this.TransferFromTradingAccountNumberTextBox);
			this.MainTabPage.Controls.Add(this.zDocAddressControl1);
			this.MainTabPage.Controls.Add(this.zDocAddressControl2);
			this.MainTabPage.Controls.Add(this.zDropEdit1);
			this.MainTabPage.Controls.Add(this.PalletTransactionIDTextBox);
			this.MainTabPage.Controls.Add(this.ActionTypeDropEdit);
			this.MainTabPage.Controls.Add(this.zTextBox4);
			this.MainTabPage.Controls.Add(this.zTextBox3);
			this.MainTabPage.Controls.Add(this.PapaerDocketIDTextBox);
			this.MainTabPage.Controls.Add(this.zDropEdit3);
			this.MainTabPage.Controls.Add(this.TransactionTypeDropEdit);
			this.MainTabPage.Controls.Add(this.zDateEdit1);
			this.MainTabPage.Controls.Add(this.PalletTypeDropEdit);
			this.MainTabPage.Controls.Add(this.zCalcEdit1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 472, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 472, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 472, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 495, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Packing.Business.PkgPalletTransaction);
			// 
			// numbersTabPage
			// 
			this.numbersTabPage.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("eb67e522-b5cb-45d0-819c-2b9951ba3f77", "Additional Numbers");
			this.numbersTabPage.Controls.Add(this.zGrid1);
			this.numbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.numbersTabPage.Name = "numbersTabPage";
			this.numbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 472, true);
			this.numbersTabPage.TabIndex = 3;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "AdditionalReferenceNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).AdditionalReferenceNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(((System.Collections.IList)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(((System.Collections.IList)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).AdditionalReferenceNumbers)).SyncRoot)).AdditionalReferenceNumberTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(((System.Collections.IList)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(((System.Collections.IList)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryLineReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Integration.Customs.ICusEntryNumber)(((System.Collections.IList)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_IssueDate)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("d0e3559e-26ed-466e-a756-1f888fac72fa", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "AdditionalReferenceNumberTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo2.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "CE_EntryLineReference";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo1.ColumnName = "CE_IssueDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "0257f50d-bb38-4dc6-83dc-af2add7f47f6";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 472, true);
			this.zGrid1.TabIndex = 0;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "KTR_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_Quantity)));
			this.zCalcEdit1.DecimalPlaces = 0;
			this.zCalcEdit1.Decimals = 0;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 38, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 19, true);
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.Text = "0";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PalletTypeDropEdit
			// 
			this.PalletTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PalletTypeDropEdit, "KTR_PalletType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_PalletType)));
			this.PalletTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 104, true);
			this.PalletTypeDropEdit.Name = "PalletTypeDropEdit";
			this.PalletTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 19, true);
			this.PalletTypeDropEdit.TabIndex = 7;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "KTR_EffectiveDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_EffectiveDateTime)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 154, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 9;
			// 
			// TransactionTypeDropEdit
			// 
			this.TransactionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionTypeDropEdit, "KTR_TransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_TransactionType)));
			this.TransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 38, true);
			this.TransactionTypeDropEdit.Name = "TransactionTypeDropEdit";
			this.TransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.TransactionTypeDropEdit.TabIndex = 2;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "KTR_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_Status)));
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 62, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.zDropEdit3.TabIndex = 6;
			// 
			// PapaerDocketIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.PapaerDocketIDTextBox, "KTR_PaperDocketID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_PaperDocketID)));
			this.PapaerDocketIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 178, true);
			this.PapaerDocketIDTextBox.Name = "PapaerDocketIDTextBox";
			this.PapaerDocketIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.PapaerDocketIDTextBox.TabIndex = 10;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "KTR_EquipmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_EquipmentCode)));
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 130, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.zTextBox3.TabIndex = 8;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "RelatedJobCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).RelatedJobCode)));
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 212, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.ReadOnly = true;
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.zTextBox4.TabIndex = 11;
			// 
			// ActionTypeDropEdit
			// 
			this.ActionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActionTypeDropEdit, "ActionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).ActionType)));
			this.ActionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 14, true);
			this.ActionTypeDropEdit.Name = "ActionTypeDropEdit";
			this.ActionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.ActionTypeDropEdit.TabIndex = 1;
			// 
			// PalletTransactionIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.PalletTransactionIDTextBox, "KTR_TransactionID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_TransactionID)));
			this.PalletTransactionIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(779, 14, true);
			this.PalletTransactionIDTextBox.Name = "PalletTransactionIDTextBox";
			this.PalletTransactionIDTextBox.ReadOnly = true;
			this.PalletTransactionIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 19, true);
			this.PalletTransactionIDTextBox.TabIndex = 4;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "KTR_TransferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_TransferType)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 38, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 19, true);
			this.zDropEdit1.TabIndex = 3;
			// 
			// zDocAddressControl2
			// 
			this.zDocAddressControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDocAddressControl2, "TransferTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).TransferTo)));
			this.zDocAddressControl2.BindToOrganisations = "Lookups.Parties";
			this.zDocAddressControl2.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("1708d3d8-e8ff-48c2-a0d3-fa7cd58c5af2", "To");
			this.zDocAddressControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 248, true);
			this.zDocAddressControl2.Name = "zDocAddressControl2";
			this.zDocAddressControl2.ReadOnly = false;
			this.zDocAddressControl2.SingleLineNoGroupBoxPanelWidth = 296;
			this.zDocAddressControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.zDocAddressControl2.TabIndex = 13;
			this.zDocAddressControl2.ValidationJustForced = false;
			// 
			// zDocAddressControl1
			// 
			this.zDocAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDocAddressControl1, "TransferFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).TransferFrom)));
			this.zDocAddressControl1.BindToOrganisations = "Lookups.Parties";
			this.zDocAddressControl1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("bc1f26db-aacf-46b0-8327-27cd92210450", "From");
			this.zDocAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 248, true);
			this.zDocAddressControl1.Name = "zDocAddressControl1";
			this.zDocAddressControl1.ReadOnly = false;
			this.zDocAddressControl1.SingleLineNoGroupBoxPanelWidth = 296;
			this.zDocAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.zDocAddressControl1.TabIndex = 12;
			this.zDocAddressControl1.ValidationJustForced = false;
			// 
			// TransferFromTradingAccountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransferFromTradingAccountNumberTextBox, "KTR_TransferFromAccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_TransferFromAccountNumber)));
			this.TransferFromTradingAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 442, true);
			this.TransferFromTradingAccountNumberTextBox.Name = "TransferFromTradingAccountNumberTextBox";
			this.TransferFromTradingAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 19, true);
			this.TransferFromTradingAccountNumberTextBox.TabIndex = 14;
			// 
			// TransferToTradingAccountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransferToTradingAccountNumberTextBox, "KTR_TransferToAccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPalletTransaction)(null)).KTR_TransferToAccountNumber)));
			this.TransferToTradingAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 442, true);
			this.TransferToTradingAccountNumberTextBox.Name = "TransferToTradingAccountNumberTextBox";
			this.TransferToTradingAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 19, true);
			this.TransferToTradingAccountNumberTextBox.TabIndex = 15;
			// 
			// PalletTransactionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 551, true);
			this.DataSourceType = typeof(Enterprise.Packing.Business.PkgPalletTransaction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 393, true);
			this.Name = "PalletTransactionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.numbersTabPage.ResumeLayout(false);
			this.numbersTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.PalletTypeDropEdit.ResumeLayout(true);
			this.PalletTypeDropEdit.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.TransactionTypeDropEdit.ResumeLayout(true);
			this.TransactionTypeDropEdit.PerformLayout();
			this.zDropEdit3.ResumeLayout(true);
			this.zDropEdit3.PerformLayout();
			this.ActionTypeDropEdit.ResumeLayout(true);
			this.ActionTypeDropEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zDocAddressControl2.ResumeLayout(true);
			this.zDocAddressControl2.PerformLayout();
			this.zDocAddressControl1.ResumeLayout(true);
			this.zDocAddressControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private ZArchitecture.GUI.ZDropEdit PalletTypeDropEdit;
		private ZArchitecture.ZCalcEdit zCalcEdit1;
		private ZArchitecture.ZTextBox PapaerDocketIDTextBox;
		private ZArchitecture.GUI.ZDropEdit zDropEdit3;
		private ZArchitecture.GUI.ZDropEdit TransactionTypeDropEdit;
		private ZArchitecture.ZTextBox zTextBox3;
		private ZArchitecture.ZTextBox zTextBox4;
		private ZTabPage numbersTabPage;
		private ZArchitecture.ZGrid zGrid1;
		private ZDropEdit ActionTypeDropEdit;
		private ZArchitecture.ZTextBox PalletTransactionIDTextBox;
		private ZDropEdit zDropEdit1;
		private MasterFiles.GUI.ZDocAddressControl zDocAddressControl2;
		private MasterFiles.GUI.ZDocAddressControl zDocAddressControl1;
		private ZArchitecture.ZTextBox TransferToTradingAccountNumberTextBox;
		private ZArchitecture.ZTextBox TransferFromTradingAccountNumberTextBox;
	}
}
