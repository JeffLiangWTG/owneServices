namespace Enterprise.Customs.NZ.GUI.Declaration
{
	partial class CusEntryLineSyncrhoniserForm
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            EffectedLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            EffectedInvoiceLinesGrid = new Enterprise.ZArchitecture.ZGrid();
            NewLineInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            EntryLineCodesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            PermitCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            PermitCodesGrid = new Enterprise.ZArchitecture.ZGrid();
            ProhibitedCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            ProhibittedCodesGrid = new Enterprise.ZArchitecture.ZGrid();
            OtherInfosTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            OtherInfosGrid = new Enterprise.ZArchitecture.ZGrid();
            ConcessionCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.EffectedLinesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(EffectedInvoiceLinesGrid)).BeginInit();
            this.EffectedInvoiceLinesGrid.SuspendLayout();
            this.NewLineInformationGroupBox.SuspendLayout();
            this.EntryLineCodesTabControl.SuspendLayout();
            this.PermitCodesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(PermitCodesGrid)).BeginInit();
            this.PermitCodesGrid.SuspendLayout();
            this.ProhibitedCodesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(ProhibittedCodesGrid)).BeginInit();
            this.ProhibittedCodesGrid.SuspendLayout();
            this.OtherInfosTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(OtherInfosGrid)).BeginInit();
            this.OtherInfosGrid.SuspendLayout();
            this.ConcessionCodeCodeFindBox.SuspendLayout();
            this.zPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 406, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 24, true);
            this.MainStatusBar.SizingGrip = false;
            this.MainStatusBar.TabIndex = 3;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser);
            // 
            // EffectedLinesGroupBox
            // 
            EffectedLinesGroupBox.Controls.Add(EffectedInvoiceLinesGrid);
            EffectedLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            EffectedLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            EffectedLinesGroupBox.Name = "EffectedLinesGroupBox";
            EffectedLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 112, true);
            EffectedLinesGroupBox.TabIndex = 0;
            EffectedLinesGroupBox.TabStop = false;
            EffectedLinesGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8ba04fbb-3585-4b50-8aa1-a2fa5c87d1a2", "Effected Invoice Lines:");
            // 
            // EffectedInvoiceLinesGrid
            // 
            EffectedInvoiceLinesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(EffectedInvoiceLinesGrid, "EffectedLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).EffectedLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).EffectedLines)).SyncRoot)).JI_LineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).EffectedLines)).SyncRoot)).JI_Tariff)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).EffectedLines)).SyncRoot)).JI_Description)));
            EffectedInvoiceLinesGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4420f8d7-df14-4888-9a39-c8d34c10a39e", "Line No");
            zCalcEditColumnStyleInfo1.ColumnName = "JI_LineNo";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.IsReadOnly = true;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("06752a39-6ce5-438f-8e09-11f15d24ecfc", "Tariff");
            zTextBoxColumnStyleInfo1.ColumnName = "JI_Tariff";
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("5fc8b6af-c0d5-486f-8e19-0f88018bfeef", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "JI_Description";
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            EffectedInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            EffectedInvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            EffectedInvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            EffectedInvoiceLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            EffectedInvoiceLinesGrid.GridId = "e4cd3a38-9cb1-42b5-baa5-e966d0acd582";
            EffectedInvoiceLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            EffectedInvoiceLinesGrid.LayoutKey = "zGrid1";
            EffectedInvoiceLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            EffectedInvoiceLinesGrid.Name = "EffectedInvoiceLinesGrid";
            EffectedInvoiceLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 93, true);
            EffectedInvoiceLinesGrid.TabIndex = 0;
            EffectedInvoiceLinesGrid.TabStop = false;
            // 
            // NewLineInformationGroupBox
            // 
            NewLineInformationGroupBox.Controls.Add(EntryLineCodesTabControl);
            NewLineInformationGroupBox.Controls.Add(ConcessionCodeCodeFindBox);
            NewLineInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            NewLineInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
            NewLineInformationGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
            NewLineInformationGroupBox.Name = "NewLineInformationGroupBox";
            NewLineInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 262, true);
            NewLineInformationGroupBox.TabIndex = 0;
            NewLineInformationGroupBox.TabStop = false;
            NewLineInformationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("19756a18-eb9f-431f-ba34-3672d74273c5", "New Line Information");
            // 
            // EntryLineCodesTabControl
            // 
            EntryLineCodesTabControl.Controls.Add(PermitCodesTabPage);
            EntryLineCodesTabControl.Controls.Add(ProhibitedCodesTabPage);
            EntryLineCodesTabControl.Controls.Add(OtherInfosTabPage);
            EntryLineCodesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 45, true);
            EntryLineCodesTabControl.Name = "EntryLineCodesTabControl";
            EntryLineCodesTabControl.SelectedIndex = 0;
            EntryLineCodesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 211, true);
            EntryLineCodesTabControl.TabIndex = 2;
            // 
            // PermitCodesTabPage
            // 
            PermitCodesTabPage.Controls.Add(PermitCodesGrid);
            PermitCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            PermitCodesTabPage.Name = "PermitCodesTabPage";
            PermitCodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            PermitCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 184, true);
            PermitCodesTabPage.TabIndex = 0;
            PermitCodesTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("c58e640a-b111-4c07-96a6-4410a80801f9", "Permit Codes");
            // 
            // PermitCodesGrid
            // 
            PermitCodesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(PermitCodesGrid, "InvoiceLine+PermitCodes");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.PermitCodes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.PermitCodes)).SyncRoot)).ZO_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.PermitCodes)).SyncRoot)).ZO_CodeList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.PermitCodes)).SyncRoot)).ZO_Data)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.PermitCodes)).SyncRoot)).ZO_Description)));
            PermitCodesGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.BindToList = "ZO_CodeList";
            zDropEditColumnStyleInfo1.ColumnName = "ZO_Code";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo3.ColumnName = "ZO_Data";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "ZO_Description";
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            PermitCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            PermitCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            PermitCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            PermitCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            PermitCodesGrid.GridId = "ac88b0b4-bd93-4fa0-afd8-1d38dc242166";
            PermitCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            PermitCodesGrid.LayoutKey = "zGrid2";
            PermitCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            PermitCodesGrid.Name = "PermitCodesGrid";
            PermitCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 178, true);
            PermitCodesGrid.TabIndex = 0;
            // 
            // ProhibitedCodesTabPage
            // 
            ProhibitedCodesTabPage.Controls.Add(ProhibittedCodesGrid);
            ProhibitedCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            ProhibitedCodesTabPage.Name = "ProhibitedCodesTabPage";
            ProhibitedCodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            ProhibitedCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 184, true);
            ProhibitedCodesTabPage.TabIndex = 1;
            ProhibitedCodesTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7da16173-50a4-4617-a716-b2decfa1ffc9", "Prohibited Codes");
            // 
            // ProhibittedCodesGrid
            // 
            ProhibittedCodesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(ProhibittedCodesGrid, "InvoiceLine+ProhibitedCodes");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.ProhibitedCodes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ProhibitedCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.ProhibitedCodes)).SyncRoot)).ZO_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.ProhibitedCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.ProhibitedCodes)).SyncRoot)).ZO_CodeList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ProhibitedCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.ProhibitedCodes)).SyncRoot)).ZO_Description)));
            ProhibittedCodesGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo2.BindToList = "ZO_CodeList";
            zDropEditColumnStyleInfo2.ColumnName = "ZO_Code";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo5.ColumnName = "ZO_Description";
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            ProhibittedCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            ProhibittedCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            ProhibittedCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            ProhibittedCodesGrid.GridId = "533d7d96-efe2-4e63-a0e3-c76dbb270271";
            ProhibittedCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            ProhibittedCodesGrid.LayoutKey = "zGrid2";
            ProhibittedCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            ProhibittedCodesGrid.Name = "ProhibittedCodesGrid";
            ProhibittedCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 178, true);
            ProhibittedCodesGrid.TabIndex = 2;
            // 
            // OtherInfosTabPage
            // 
            OtherInfosTabPage.Controls.Add(OtherInfosGrid);
            OtherInfosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            OtherInfosTabPage.Name = "OtherInfosTabPage";
            OtherInfosTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            OtherInfosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 184, true);
            OtherInfosTabPage.TabIndex = 2;
            OtherInfosTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2f6015d7-125d-4491-9ccd-12f8a2d1836f", "Other Infos");
            // 
            // OtherInfosGrid
            // 
            OtherInfosGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(OtherInfosGrid, "InvoiceLine+OtherInfos");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.OtherInfos)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.OtherInfos)).SyncRoot)).ZO_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.OtherInfos)).SyncRoot)).ZO_CodeList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.OtherInfos)).SyncRoot)).ZO_Data)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.OtherInfos)).SyncRoot)).ZO_Description)));
            OtherInfosGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo3.BindToList = "ZO_CodeList";
            zDropEditColumnStyleInfo3.ColumnName = "ZO_Code";
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "ZO_Data";
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo7.ColumnName = "ZO_Description";
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            OtherInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            OtherInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            OtherInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            OtherInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            OtherInfosGrid.GridId = "29b98c60-f2fd-40e5-9207-684bdfdd1762";
            OtherInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            OtherInfosGrid.LayoutKey = "zGrid2";
            OtherInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            OtherInfosGrid.Name = "OtherInfosGrid";
            OtherInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 178, true);
            OtherInfosGrid.TabIndex = 2;
            // 
            // ConcessionCodeCodeFindBox
            // 
            ConcessionCodeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(ConcessionCodeCodeFindBox, "InvoiceLine+JI_ConcessionCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.JI_ConcessionCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser)(null)).InvoiceLine.Lookups.ConcessionList)));
            ConcessionCodeCodeFindBox.BindToList = "InvoiceLine+Lookups+ConcessionList";
            ConcessionCodeCodeFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("60d86c5f-d11a-46b3-ab7b-d58dc457430b", "Concession Code");
            ConcessionCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
            ConcessionCodeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.NZ.Concession;
            ConcessionCodeCodeFindBox.Name = "ConcessionCodeCodeFindBox";
            ConcessionCodeCodeFindBox.ShouldResize = true;
            ConcessionCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            ConcessionCodeCodeFindBox.TabIndex = 1;
            // 
            // zPanel1
            // 
            zPanel1.Controls.Add(CloseButton);
            zPanel1.Controls.Add(OKButton);
            zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 374, true);
            zPanel1.Name = "zPanel1";
            zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 32, true);
            zPanel1.TabIndex = 2;
            // 
            // CloseButton
            // 
            CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 6, true);
            CloseButton.Name = "CloseButton";
            CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            CloseButton.TabIndex = 5;
            CloseButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3b46f583-d916-4bce-88a6-0d479c12ae2f", "&Cancel");
            CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            CloseButton.ToolTipCaption = null;
            CloseButton.UseVisualStyleBackColor = true;
            CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // OKButton
            // 
            OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 6, true);
            OKButton.Name = "OKButton";
            OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            OKButton.TabIndex = 4;
            OKButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("27cae026-5a81-4b03-a3d8-4770d2f8e396", "&OK");
            OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            OKButton.ToolTipCaption = null;
            OKButton.UseVisualStyleBackColor = true;
            OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CusEntryLineSyncrhoniserForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 430, true);
            this.Controls.Add(NewLineInformationGroupBox);
            this.Controls.Add(zPanel1);
            this.Controls.Add(EffectedLinesGroupBox);
            this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
            this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser);
            this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.CusEntryLineSyncroniser";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 324, true);
            this.Name = "CusEntryLineSyncrhoniserForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Controls.SetChildIndex(EffectedLinesGroupBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(zPanel1, 0);
            this.Controls.SetChildIndex(NewLineInformationGroupBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.EffectedLinesGroupBox.ResumeLayout(false);
            this.EffectedLinesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(EffectedInvoiceLinesGrid)).EndInit();
            this.EffectedInvoiceLinesGrid.ResumeLayout(false);
            this.EffectedInvoiceLinesGrid.PerformLayout();
            this.NewLineInformationGroupBox.ResumeLayout(false);
            this.NewLineInformationGroupBox.PerformLayout();
            this.EntryLineCodesTabControl.ResumeLayout(false);
            this.EntryLineCodesTabControl.PerformLayout();
            this.PermitCodesTabPage.ResumeLayout(false);
            this.PermitCodesTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(PermitCodesGrid)).EndInit();
            this.PermitCodesGrid.ResumeLayout(false);
            this.PermitCodesGrid.PerformLayout();
            this.ProhibitedCodesTabPage.ResumeLayout(false);
            this.ProhibitedCodesTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(ProhibittedCodesGrid)).EndInit();
            this.ProhibittedCodesGrid.ResumeLayout(false);
            this.ProhibittedCodesGrid.PerformLayout();
            this.OtherInfosTabPage.ResumeLayout(false);
            this.OtherInfosTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(OtherInfosGrid)).EndInit();
            this.OtherInfosGrid.ResumeLayout(false);
            this.OtherInfosGrid.PerformLayout();
            this.ConcessionCodeCodeFindBox.ResumeLayout(true);
            this.ConcessionCodeCodeFindBox.PerformLayout();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox EffectedLinesGroupBox;
		private Enterprise.ZArchitecture.ZGrid EffectedInvoiceLinesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NewLineInformationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZTabControl EntryLineCodesTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage PermitCodesTabPage;
		private Enterprise.ZArchitecture.ZGrid PermitCodesGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage ProhibitedCodesTabPage;
		private Enterprise.ZArchitecture.ZGrid ProhibittedCodesGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage OtherInfosTabPage;
		private Enterprise.ZArchitecture.ZGrid OtherInfosGrid;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ConcessionCodeCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}
