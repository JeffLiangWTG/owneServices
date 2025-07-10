
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NL.GUI
{
	partial class MessageUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CustomsRemarksGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsRemarksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewEntryDetailsTabPage.SuspendLayout();
			this.EntryDetailsUserControl.SuspendLayout();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
			this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
			this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
			this.MainHorizontalSplitContainer.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.ExtendedInfoGroupBox.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.Panel2.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsRemarksGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsRemarksGrid)).BeginInit();
			this.CustomsRemarksGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// NewEntryDetailsTabPage
			// 
			this.NewEntryDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.NewEntryDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 462, true);
			// 
			// EntryDetailsUserControl
			// 
			this.EntryDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 462, true);
			// 
			// MainHorizontalSplitContainer
			// 
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 462, true);
			// 
			// EntryLineGrid
			// 
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 339, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 342, true);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 117, true);
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 462, true);
			// 
			// TopVerticalSplitContainer
			// 
			// 
			// TopVerticalSplitContainer.Panel2
			// 
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.CustomsRemarksGroupBox);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 457, true);
			// 
			// CustomsRemarksGroupBox
			// 
			this.CustomsRemarksGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("CE9915BC-3609-4710-B941-E1B291980BAE", "Additional statements sent");
			this.CustomsRemarksGroupBox.Controls.Add(this.CustomsRemarksGrid);
			this.CustomsRemarksGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsRemarksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsRemarksGroupBox.Name = "CustomsRemarksGroupBox";
			this.CustomsRemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 65, true);
			this.CustomsRemarksGroupBox.TabIndex = 0;
			this.CustomsRemarksGroupBox.TabStop = false;
			// 
			// CustomsRemarksGrid
			// 
			this.CustomsRemarksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsRemarksGrid, "CustomsEntryHeaders.CustomsMessageRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsMessageRemarks)));
			this.CustomsRemarksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "StatementType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "StatementDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.CustomsRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CustomsRemarksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsRemarksGrid.GridId = "094CC42C-2E02-4585-B597-85CFB70C2C45";
			this.CustomsRemarksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsRemarksGrid.LayoutKey = "CustomsRemarksGrid";
			this.CustomsRemarksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CustomsRemarksGrid.Name = "CustomsRemarksGrid";
			this.CustomsRemarksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 49, true);
			this.CustomsRemarksGrid.TabIndex = 0;
			// 
			// MessageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "MessageUserControl";
			this.NewEntryDetailsTabPage.ResumeLayout(false);
			this.NewEntryDetailsTabPage.PerformLayout();
			this.EntryDetailsUserControl.ResumeLayout(true);
			this.EntryDetailsUserControl.PerformLayout();
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
			this.MainHorizontalSplitContainer.ResumeLayout(false);
			this.MainHorizontalSplitContainer.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsRemarksGroupBox.ResumeLayout(false);
			this.CustomsRemarksGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsRemarksGrid)).EndInit();
			this.CustomsRemarksGrid.ResumeLayout(false);
			this.CustomsRemarksGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.ZGrid CustomsRemarksGrid;
		ZGroupBox CustomsRemarksGroupBox;
	}
}
