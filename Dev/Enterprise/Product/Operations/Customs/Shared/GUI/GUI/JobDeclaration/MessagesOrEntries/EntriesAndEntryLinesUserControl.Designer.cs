using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CargoWise.Windows.UI;

namespace Enterprise.Customs.GUI
{
	partial class EntriesAndEntryLinesUserControl
	{
		public ZGroupBox EntriesGroupBox;
		public ZGrid EntriesBoundGrid;
		protected KSplitter HorizontalSplitter;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		protected Enterprise.ZArchitecture.GUI.ZTabControl EntryLinesMessagesTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage EntryLinesTabPage;
		public ZGrid EntryLineGrid;
		protected KSplitter VerticalSplitter;

		private System.ComponentModel.IContainer components = null;


		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();

			this.EntriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntriesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HorizontalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntryLinesMessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.EntryLinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.VerticalSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Controls.Add(this.EntriesBoundGrid);
			this.EntriesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntriesGroupBox.Name = "EntriesGroupBox";
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 138, true);
			this.EntriesGroupBox.TabIndex = 0;
			this.EntriesGroupBox.TabStop = false;
			this.EntriesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4F3DFF3B-877B-4518-B720-10A8EB80910E", "Entries");
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntriesBoundGrid, "CustomsEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryHeaderStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MovementReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).DeclarationUCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Duty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).VAT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalDutyAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).GSTAmount)));
			this.EntriesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9C91992F-83FD-427E-A651-D0B73342E660", "Ref No.");
			zTextBoxColumnStyleInfo1.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4EA18F2A-8839-42D2-A264-A076CD063869", "Message Type");
			zTextBoxColumnStyleInfo2.ColumnName = "CH_MessageType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("504F6C89-D8B1-4B83-8787-89B2EB3128C0", "Message Type Desc.");
			zTextBoxColumnStyleInfo3.ColumnName = "CH_MessageTypeDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1BABAFCF-DA6D-4D85-9E31-DB20292C927A", "Entry Status");
			zTextBoxColumnStyleInfo4.ColumnName = "CH_EntryStatus";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B17F9A57-C91E-45BB-92F0-DD750E10AE38", "Entry Status Desc.");
			zTextBoxColumnStyleInfo5.ColumnName = "EntryHeaderStatusDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9B38859E-8EAE-465F-9683-4AC043F9FE0D", "MRN");
			zTextBoxColumnStyleInfo6.ColumnName = "MovementReferenceNumber";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("50656027-9F14-40A4-8D11-1B41270A8F26", "Declaration UCR");
			zTextBoxColumnStyleInfo7.ColumnName = "DeclarationUCR";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3C8970FD-E445-4DF9-B5F9-A5D08EA7751B", "Entry Submitted Date");
			zDateEditColumnStyleInfo1.ColumnName = "CH_EntrySubmittedDate";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "CH_EntryReleaseDate";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1E5D7090-B6E5-4D1F-A676-DAFB6E7EA1C9", "Duty");
			zCalcEditColumnStyleInfo1.ColumnName = "Duty";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3D71F005-EAFE-4BBE-A4F1-CD0330C29C38", "VAT");
			zCalcEditColumnStyleInfo2.ColumnName = "VAT";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("A8B502FC-8EF8-4555-96C1-4D2F206557DC", "Duty");
			zCalcEditColumnStyleInfo7.ColumnName = "TotalDutyAmount";
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("36B875BE-088A-457E-A464-6CA1CB6DAE23", "VAT");
			zCalcEditColumnStyleInfo8.ColumnName = "GSTAmount";
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EntriesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntriesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.EntriesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesBoundGrid.GridId = "52a32545-ab95-4cce-b905-3eebb51483c0";
			this.EntriesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesBoundGrid.LayoutKey = "EntriesBoundGrid";
			this.EntriesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntriesBoundGrid.Name = "EntriesBoundGrid";
			this.EntriesBoundGrid.Size = this.EntriesBoundGrid.Size;
			this.EntriesBoundGrid.TabIndex = 1;
			// 
			// HorizontalSplitter
			// 
			this.HorizontalSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.HorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.HorizontalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
			this.HorizontalSplitter.Name = "HorizontalSplitter";
			this.HorizontalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 3, true);
			this.HorizontalSplitter.TabIndex = 1;
			this.HorizontalSplitter.TabStop = false;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.EntryLinesMessagesTabControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 141, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 0, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 410, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EntryLinesMessagesTabControl.Controls.Add(this.EntryLinesTabPage);
			this.EntryLinesMessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesMessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLinesMessagesTabControl.Name = "EntryLinesMessagesTabControl";
			this.EntryLinesMessagesTabControl.SelectedIndex = 0;
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 410, true);
			this.EntryLinesMessagesTabControl.TabIndex = 0;
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Controls.Add(this.EntryLineGrid);
			this.EntryLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryLinesTabPage.Name = "EntryLinesTabPage";
			this.EntryLinesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 383, true);
			this.EntryLinesTabPage.TabIndex = 1;
			this.EntryLinesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("{EC0F0228-C7AE-4CC9-8465-C6EDBA6EDF5A}", "Entry Lines");
			// 
			// EntryLineGrid
			// 
			this.EntryLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineGrid, "CustomsEntryHeaders.AllEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).LineSubmissionStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).DutyAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).GSTVATAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_CustomsValue)));
			this.EntryLineGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("{DA943EA2-F6D4-4FEE-B4DA-403E13C2A051}", "Entry Line No.");
			zCalcEditColumnStyleInfo3.ColumnName = "CL_LineNumber";
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F4FE799D-63B2-435B-92C7-6436DD94A15B", "Line Status");
			zTextBoxColumnStyleInfo8.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C7117815-450E-4E3E-A01C-EA976089C414", "Tariff");
			zTextBoxColumnStyleInfo9.ColumnName = "FormattedTariff";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("FDE7FE35-EB6F-4216-8838-A27ACE19D5E8", "Description");
			zTextBoxColumnStyleInfo10.ColumnName = "EffectiveDescription";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("76809F7C-799C-4BB5-A449-4D2453972068", "Total Duty Tax");
			zCalcEditColumnStyleInfo4.ColumnName = "DutyAmount";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9BFBD2E1-202D-4FA1-B0C6-BEB5C91AF27B", "VAT Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "GSTVATAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2478C898-13B8-4F2A-9A45-39BE119AA3A0", "Customs Value");
			zCalcEditColumnStyleInfo6.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.EntryLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineGrid.GridId = "099c68b3-05cb-4214-a3f5-ab3f2a9a8831";
			this.EntryLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineGrid.LayoutKey = "zGrid1";
			this.EntryLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineGrid.Name = "EntryLineGrid";
			this.EntryLineGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 377, true);
			this.EntryLineGrid.TabIndex = 0;
			// 
			// VerticalSplitter
			// 
			this.VerticalSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.VerticalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 3, true);
			this.VerticalSplitter.Name = "VerticalSplitter";
			this.VerticalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 431, true);
			this.VerticalSplitter.TabIndex = 1;
			this.VerticalSplitter.TabStop = false;
			// 
			// EntriesAndEntryLinesUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.HorizontalSplitter);
			this.Controls.Add(this.EntriesGroupBox);
			this.Name = "EntriesAndEntryLinesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 551, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
