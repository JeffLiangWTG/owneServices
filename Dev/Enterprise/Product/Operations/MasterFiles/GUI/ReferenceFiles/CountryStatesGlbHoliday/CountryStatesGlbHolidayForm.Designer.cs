using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class CountryStatesGlbHolidayForm : ZForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CountryCodeBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IsRecurringCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsWorkingDayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.IsStateSpecific = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSelectAll = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryCodeBoundCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StatesGrid)).BeginInit();
			this.StatesGrid.SuspendLayout();
			this.DateEdit.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo);
			//
			// CountryCodeBoundCodeFindBox
			//
			this.CountryCodeBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeBoundCodeFindBox, "GHC_CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_CountryCode)));
			this.CountryCodeBoundCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|Country", "Country");
			this.CountryCodeBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 19, true);
			this.CountryCodeBoundCodeFindBox.Name = "CountryCodeBoundCodeFindBox";
			this.CountryCodeBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeBoundCodeFindBox.ParentType = null;
			this.CountryCodeBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.CountryCodeBoundCodeFindBox.TabIndex = 0;
			//
			// StatesGrid
			//
			this.StatesGrid.AllowNavigation = false;
			this.StatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));

			this.BindingSource.SetBindingMember(this.StatesGrid, "GHC_CountryStates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_CountryStates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_CountryStates)).SyncRoot)).IsChecked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_CountryStates)).SyncRoot)).StateName)));
			this.StatesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|StateSelection|IsChecked", "Apply to");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsChecked";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|StateSelection|State", "State");
			zTextBoxColumnStyleInfo1.ColumnName = "StateName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.StatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.StatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StatesGrid.GridId = "e04ab8cc-7cd1-4e7b-b0c8-889191191e26";
			this.StatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StatesGrid.LayoutKey = "StatesGrid";
			this.StatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 45, true);
			this.StatesGrid.Name = "StatesGrid";
			this.StatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 235, true);
			this.StatesGrid.TabIndex = 7;
			this.StatesGrid.SizeChanged += StatesGrid_SizeChanged;
			// 
			// DescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "GHC_HolidayName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_HolidayName)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|Description", "Description");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 45, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			//
			// DateEdit
			//
			this.DateEdit.AllowDrop = true;
			this.DateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateEdit, "GHC_Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_Date)));
			this.DateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|Date", "Date");
			this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 71, true);
			this.DateEdit.Name = "DateEdit";
			this.DateEdit.TabIndex = 2;
			//
			// IsRecurringCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsRecurringCheckBox, "GHC_Recurring");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_Recurring)));
			this.IsRecurringCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|IsRecurring", "Recurring");
			this.IsRecurringCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 97, true);
			this.IsRecurringCheckBox.Name = "IsRecurringCheckBox";
			this.IsRecurringCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.IsRecurringCheckBox.TabIndex = 4;
			this.IsRecurringCheckBox.UseVisualStyleBackColor = false;
			//
			// IsWorkingDayCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsWorkingDayCheckBox, "GHC_IsWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_IsWorkingDay)));
			this.IsWorkingDayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|IsWorkingDay", "Working Day");
			this.IsWorkingDayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 73, true);
			this.IsWorkingDayCheckBox.Name = "IsWorkingDayCheckBox";
			this.IsWorkingDayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.IsWorkingDayCheckBox.TabIndex = 3;
			this.IsWorkingDayCheckBox.UseVisualStyleBackColor = false;
			//
			// PostingButtonsUserControl
			//
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 348, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 25, true);
			this.PostingButtonsUserControl.TabIndex = 8;
			//
			// IsStateSpecific
			//
			this.BindingSource.SetBindingMember(this.IsStateSpecific, "GHC_IsStateSpecific");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).GHC_IsStateSpecific)));
			this.IsStateSpecific.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|IsStateSpecific", "State Specific");
			this.IsStateSpecific.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 20, true);
			this.IsStateSpecific.Name = "IsStateSpecific";
			this.IsStateSpecific.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 18, true);
			this.IsStateSpecific.TabIndex = 5;
			this.IsStateSpecific.UseVisualStyleBackColor = false;
			//
			// IsSelectAll
			//
			this.BindingSource.SetBindingMember(this.IsSelectAll, "CountryStatesGlbHolidayEntrySelectAll");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo)(null)).CountryStatesGlbHolidayEntrySelectAll)));
			this.IsSelectAll.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|IsSelectAll", "Select All");
			this.IsSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 20, true);
			this.IsSelectAll.Name = "IsSelectAll";
			this.IsSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 18, true);
			this.IsSelectAll.TabIndex = 6;
			this.IsSelectAll.UseVisualStyleBackColor = false;
			//
			// CountryStatesGlbHolidayForm
			//
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CountryStatesGlbHolidayForm|FormName", "Holiday");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 400, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 400, true);
			this.Controls.Add(this.IsSelectAll);
			this.Controls.Add(this.IsStateSpecific);
			this.Controls.Add(this.IsWorkingDayCheckBox);
			this.Controls.Add(this.IsRecurringCheckBox);
			this.Controls.Add(this.DateEdit);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.StatesGrid);
			this.Controls.Add(this.CountryCodeBoundCodeFindBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.CountryStatesGlbHolidayBizo);
			this.Name = "CountryStatesGlbHolidayForm";
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CountryCodeBoundCodeFindBox, 0);
			this.Controls.SetChildIndex(this.StatesGrid, 0);
			this.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.DateEdit, 0);
			this.Controls.SetChildIndex(this.IsRecurringCheckBox, 0);
			this.Controls.SetChildIndex(this.IsWorkingDayCheckBox, 0);
			this.Controls.SetChildIndex(this.IsStateSpecific, 0);
			this.Controls.SetChildIndex(this.IsSelectAll, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryCodeBoundCodeFindBox.ResumeLayout(true);
			this.CountryCodeBoundCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StatesGrid)).EndInit();
			this.StatesGrid.ResumeLayout(false);
			this.StatesGrid.PerformLayout();
			this.DateEdit.ResumeLayout(true);
			this.DateEdit.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void StatesGrid_SizeChanged(object sender, System.EventArgs e)
		{
			this.Refresh();
		}

		#endregion

		protected ZCodeFindBox CountryCodeBoundCodeFindBox;
		protected ZGrid StatesGrid;
		protected ZTextBox DescriptionTextBox;
		protected ZDateEdit DateEdit;
		protected ZCheckBox IsRecurringCheckBox;
		protected ZCheckBox IsWorkingDayCheckBox;
		protected ZPostingButtonsUserControl PostingButtonsUserControl;
		protected ZCheckBox IsStateSpecific;
		protected ZCheckBox IsSelectAll;
	}
}
