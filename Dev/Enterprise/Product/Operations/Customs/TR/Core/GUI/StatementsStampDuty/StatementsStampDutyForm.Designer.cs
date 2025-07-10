namespace Enterprise.Customs.TR.GUI
{
	partial class StatementsStampDutyForm
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
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.StatementLinesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.StatementLinesChargesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.EntriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.HeaderDetailsUserControl = new Enterprise.Customs.TR.GUI.StatementHeaderUserControl();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StatementLinesGrid)).BeginInit();
            this.StatementLinesGrid.SuspendLayout();
            this.ChargesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StatementLinesChargesGrid)).BeginInit();
            this.StatementLinesChargesGrid.SuspendLayout();
            this.EntriesGroupBox.SuspendLayout();
            this.HeaderDetailsUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 464, true);
			// 
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.EntriesGroupBox);
            this.MainTabPage.Controls.Add(this.ChargesGroupBox);
			this.MainTabPage.Controls.Add(this.HeaderDetailsUserControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
            this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 441, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 441, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 441, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 464, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.CusStatementHeader);
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EntriesGroupBox.Controls.Add(this.StatementLinesGrid);
			this.EntriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 121, true);
			this.EntriesGroupBox.Name = "EntriesGroupBox";
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 180, true);
			this.EntriesGroupBox.TabIndex = 7;
			this.EntriesGroupBox.TabStop = false;
			this.EntriesGroupBox.Text = Enterprise.Customs.TR.GUI.Res.GetString("B430259A-6FE2-4E22-9CF8-FE8603C296A5", "Entries");
			// 
			// StatementLinesGrid
			// 
			this.StatementLinesGrid.AllowNavigation = false;
            this.StatementLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.StatementLinesGrid, "StatementLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_BrokerReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_EntryDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_AssociatedEntry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_Status)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).B3_CustomsFeesTotal)));
            this.StatementLinesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "B3_BrokerReference";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zDropEditColumnStyleInfo1.ColumnName = "B3_EntryType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
            zTextBoxColumnStyleInfo2.ColumnName = "B3_EntryNum";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            zDateEditColumnStyleInfo1.ColumnName = "B3_EntryDate";
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
            zTextBoxColumnStyleInfo3.ColumnName = "B3_AssociatedEntry";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDropEditColumnStyleInfo2.ColumnName = "B3_Status";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "B3_CustomsFeesTotal";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
            this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.StatementLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.StatementLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.StatementLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.StatementLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.StatementLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.StatementLinesGrid.GridId = "3bc0e5b8-c9fe-474c-8422-3195998583f6";
            this.StatementLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.StatementLinesGrid.LayoutKey = "StatementLinesGrid";
            this.StatementLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
            this.StatementLinesGrid.Name = "StatementLinesGrid";
            this.StatementLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 150, true);
			this.StatementLinesGrid.TabIndex = 8;
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChargesGroupBox.Controls.Add(this.StatementLinesChargesGrid);
            this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 310, true);
            this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 120, true);
			this.ChargesGroupBox.TabIndex = 9;
            this.ChargesGroupBox.TabStop = false;
            this.ChargesGroupBox.Text = Enterprise.Customs.TR.GUI.Res.GetString("C275268A-C9F6-4DCE-99A1-6DE0F550F191", "Entries Charges");
			// 
			// StatementLinesChargesGrid
			// 
			this.StatementLinesChargesGrid.AllowNavigation = false;
            this.StatementLinesChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.StatementLinesChargesGrid, "StatementLines.Charges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).B4_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).B4_ChargeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.CusStatementLineCharge)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).StatementLines)).SyncRoot)).Charges)).SyncRoot)).B4_ChargeAmount)));
            this.StatementLinesChargesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo4.ColumnName = "B4_ReferenceNumber";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo3.ColumnName = "B4_ChargeType";
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "B4_ChargeAmount";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.StatementLinesChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.StatementLinesChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.StatementLinesChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.StatementLinesChargesGrid.GridId = "c77f880b-c403-4e07-b50b-73727c11631d";
            this.StatementLinesChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.StatementLinesChargesGrid.LayoutKey = "StatementLinesChargesGrid";
            this.StatementLinesChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
            this.StatementLinesChargesGrid.Name = "StatementLinesChargesGrid";
			this.StatementLinesChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 90, true);
			this.StatementLinesChargesGrid.TabIndex = 10;
            // 
            // HeaderDetailsUserControl
            // 
            this.HeaderDetailsUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.HeaderDetailsUserControl, ".");
            this.HeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.HeaderDetailsUserControl.Name = "HeaderDetailsUserControl";
            this.HeaderDetailsUserControl.TabIndex = 1;
			// 
			// StatementsStampDutyForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 520, true);
            this.DataSourceType = typeof(Enterprise.Customs.TR.Business.CusStatementHeader);
            this.Name = "StatementsStampDutyForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "StatementsStampDutyForm";
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
            ((System.ComponentModel.ISupportInitialize)(this.StatementLinesGrid)).EndInit();
            this.StatementLinesGrid.ResumeLayout(false);
            this.StatementLinesGrid.PerformLayout();
            this.ChargesGroupBox.ResumeLayout(false);
            this.ChargesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StatementLinesChargesGrid)).EndInit();
            this.StatementLinesChargesGrid.ResumeLayout(false);
            this.StatementLinesChargesGrid.PerformLayout();
            this.EntriesGroupBox.ResumeLayout(false);
            this.EntriesGroupBox.PerformLayout();
            this.HeaderDetailsUserControl.ResumeLayout(true);
            this.HeaderDetailsUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private StatementHeaderUserControl HeaderDetailsUserControl;
		private ZArchitecture.ZGrid StatementLinesGrid;
		private ZArchitecture.GUI.ZGroupBox ChargesGroupBox;
		private ZArchitecture.GUI.ZGroupBox EntriesGroupBox;
		private ZArchitecture.ZGrid StatementLinesChargesGrid;
	}
}
