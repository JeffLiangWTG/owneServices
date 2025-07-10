
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.GUI.Forms
{
	partial class TelPreDriveChecklistForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.driverTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.completionTimeBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.notesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.completionTimeBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid)).BeginInit();
			this.zGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Telematics.Business.TelPreDriveChecklistHeader);
			// 
			// driverTextBox
			// 
			this.BindingSource.SetBindingMember(this.driverTextBox, "TPH_GS_NKDriver");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).TPH_GS_NKDriver)));
			this.driverTextBox.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("27082CDA-0357-4048-8FEC-FA37217C7EE3", "Driver", "Driver", "");
			this.driverTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 12, true);
			this.driverTextBox.Name = "driverTextBox";
			this.driverTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 15, true);
			this.driverTextBox.TabIndex = 1;
			// 
			// completionTimeBox
			// 
			this.completionTimeBox.AllowDrop = true;
			this.completionTimeBox.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.completionTimeBox, "CompletionTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).CompletionTimeLocal)));
			this.completionTimeBox.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("81EAD2F4-02EF-43DA-985F-78D33EA05BCF", "Completion Time", "Completion Time", "");
			this.completionTimeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 38, true);
			this.completionTimeBox.Name = "completionTimeBox";
			this.completionTimeBox.TabIndex = 2;
			// 
			// notesTextBox
			// 
			this.BindingSource.SetBindingMember(this.notesTextBox, "TPH_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).TPH_Notes)));
			this.notesTextBox.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("FA2E0258-08DA-4265-9070-3F80AAF650EF", "Notes", "Notes", "");
			this.notesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 64, true);
			this.notesTextBox.Name = "notesTextBox";
			this.notesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 15, true);
			this.notesTextBox.TabIndex = 3;
			// 
			// zGrid
			// 
			this.zGrid.AllowNavigation = false;
			this.zGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid, "Entries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).Entries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Telematics.Business.TelPreDriveChecklistEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).Entries)).SyncRoot)).TPE_Index)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).Entries)).SyncRoot)).TPE_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).Entries)).SyncRoot)).TPE_IsAgreed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).Entries)).SyncRoot)).TPE_FaultDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Telematics.Business.TelPreDriveChecklistEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistHeader)(null)).Entries)).SyncRoot)).TPE_FaultPriority)));
			this.zGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("21B3C690-952A-4336-8800-9714E7A19399", "Index");
			zTextBoxColumnStyleInfo1.ColumnName = "TPE_Index";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("348B0110-CE21-440A-BDA5-0E2E7352A8EE", "Checkbox Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TPE_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("607D49D5-08D9-4315-B4CB-93375CF594F7", "Is Agreed");
			zTextBoxColumnStyleInfo3.ColumnName = "TPE_IsAgreed";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("ddc520d1-af69-4e00-8dcf-cf941e5f0e70", "Fault Description");
			zTextBoxColumnStyleInfo4.ColumnName = "TPE_FaultDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("0c2afb04-471f-43c8-9b8b-f87c9a782dd3", "Fault Priority");
			zCalcEditColumnStyleInfo1.ColumnName = "TPE_FaultPriority";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid.GridId = "dd0e24c0-474f-40c1-9613-268d0b3fee20";
			this.zGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid.LayoutKey = "zGrid";
			this.zGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 90, true);
			this.zGrid.Name = "zGrid";
			this.zGrid.ReadOnly = true;
			this.zGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 382, true);
			this.zGrid.TabIndex = 4;
			// 
			// TelPreDriveChecklistForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 511, true);
			this.Controls.Add(this.notesTextBox);
			this.Controls.Add(this.zGrid);
			this.Controls.Add(this.completionTimeBox);
			this.Controls.Add(this.driverTextBox);
			this.DataSourceType = typeof(Enterprise.Telematics.Business.TelPreDriveChecklistHeader);
			this.Name = "TelPreDriveChecklistForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.driverTextBox, 0);
			this.Controls.SetChildIndex(this.completionTimeBox, 0);
			this.Controls.SetChildIndex(this.zGrid, 0);
			this.Controls.SetChildIndex(this.notesTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.completionTimeBox.ResumeLayout(true);
			this.completionTimeBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid)).EndInit();
			this.zGrid.ResumeLayout(false);
			this.zGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox driverTextBox;
		private ZArchitecture.GUI.ZDateEdit completionTimeBox;
		private ZArchitecture.ZGrid zGrid;
		private ZArchitecture.ZTextBox notesTextBox;
	}
}
