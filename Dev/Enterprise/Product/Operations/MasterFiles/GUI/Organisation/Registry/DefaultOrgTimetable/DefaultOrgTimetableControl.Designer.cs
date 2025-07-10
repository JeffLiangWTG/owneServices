using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class DefaultOrgTimetableControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
#if DEBUG
		public
#endif
		CargoWise.Windows.UI.KSplitContainer splitContainer;
		ZGroupBox TimetableGroupBox;
		ZGrid TimetableGrid;
		internal ZGrid CountryGrid;

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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CountryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TimetableGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TimetableGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TimetableGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryGrid)).BeginInit();
			this.TimetableGroupBox.SuspendLayout();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultOrgTimetableSettingsCollection);
			// 
			// CountryGrid
			// 
			this.CountryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).CountryName)));
			this.CountryGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|8ce8f8e0-0d71-48f7-ac0f-05c8052a8224", "Country/Region");
			zDropEditColumnStyleInfo3.ColumnName = "CountryCode";
			zDropEditColumnStyleInfo3.ToolTip = "Please select a country code to configure delivery timetable.";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|f98685c0-fa7f-431d-8c2e-3bb080200419", "Country/Region Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CountryName";
			this.CountryGrid.AddColumn(zDropEditColumnStyleInfo3);
			this.CountryGrid.AddColumn(zTextBoxColumnStyleInfo1);
			this.CountryGrid.GridId = "c1352883-3082-4ba9-9153-4628b54a7725";
			this.CountryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryGrid.LayoutKey = "CountryGrid";
			this.CountryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CountryGrid.Name = "CountryGrid";
			this.CountryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 161, true);
			this.CountryGrid.TabIndex = 0;
			// 
			// TimetableGroupBox
			// 
			this.TimetableGroupBox.AutoSize = true;
			this.TimetableGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("060ae4a7-df2e-4d31-b742-ca0d65cc75ae", "Timetables");
			this.TimetableGroupBox.Controls.Add(this.TimetableGrid);
			this.TimetableGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TimetableGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TimetableGroupBox.Name = "TimetableGroupBox";
			this.TimetableGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 420, true);
			this.TimetableGroupBox.TabIndex = 1;
			this.TimetableGroupBox.TabStop = false;
			// 
			// TimetableGrid
			// 
			//this.TimetableGrid.AutoSize = true;
			this.TimetableGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TimetableGrid, "Timetables");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultOrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.DefaultOrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)).SyncRoot)).From)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.DefaultOrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)).SyncRoot)).To)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultOrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)).SyncRoot)).Day)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.DefaultOrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)).SyncRoot)).CutOffTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.DefaultOrgTimetable)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultOrgTimetableSettings)(null)).Timetables)).SyncRoot)).ProcessingTimeInHours)));
			this.TimetableGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|bc07e100-7e10-49d6-a39d-4be437bc3be0", "Direction");
			zDropEditColumnStyleInfo1.ColumnName = "Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.IsSortable = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|5ec13363-907f-453e-b03e-89169285aa6b", "From");
			zDateEditColumnStyleInfo1.ColumnName = "From";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsSortable = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|5d0433ee-d8db-4920-9642-46dbc4da4428", "To");
			zDateEditColumnStyleInfo2.ColumnName = "To";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.IsSortable = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|ba250bed-c8d3-4414-8ded-b4a53c83a0ed", "Day");
			zDropEditColumnStyleInfo2.ColumnName = "Day";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|75c2f1fe-fcba-435e-b79d-7e496320e4e4", "Cut-Off Time");
			zDateEditColumnStyleInfo3.ColumnName = "CutOffTime";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo3.IsMandatory = false;
			zDateEditColumnStyleInfo3.IsSortable = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo1.ColumnName = "ProcessingTimeInHours";
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|d517b1d2-ed4f-44f2-a826-d3943732cd9f", "Processing Time (Hours)");
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.IsSortable = false;
			zTimeEditExColumnStyleInfo1.IsMandatory = false;
			this.TimetableGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TimetableGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TimetableGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TimetableGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TimetableGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TimetableGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.TimetableGrid.GridId = "6FEC89B1-A75B-4D7C-87A0-F0E42A600F2A";
			this.TimetableGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TimetableGrid.LayoutKey = "TimetableGrid";
			this.TimetableGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TimetableGrid.Name = "TimetableGrid";
			this.TimetableGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 400, true);
			this.TimetableGrid.TabIndex = 0;
			this.TimetableGrid.Dock = DockStyle.Fill;
			//
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.CountryGrid);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.TimetableGroupBox);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 425, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(181);
			this.splitContainer.TabIndex = 1;

			// 
			// DefaultOrgTimetableControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "DefaultOrgTimetableControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 368, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryGrid)).EndInit();
			this.CountryGrid.ResumeLayout(false);
			this.CountryGrid.PerformLayout();
			this.TimetableGroupBox.ResumeLayout(false);
			this.TimetableGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TimetableGrid)).EndInit();
			this.TimetableGrid.ResumeLayout(false);
			this.TimetableGrid.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
