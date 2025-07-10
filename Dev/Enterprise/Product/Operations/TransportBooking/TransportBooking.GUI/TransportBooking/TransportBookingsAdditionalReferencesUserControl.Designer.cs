using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingsAdditionalReferencesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zBookingOnlyCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.numbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.numberDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.issueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.entryLineReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.entryNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.entryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numbersGrid)).BeginInit();
			this.numbersGrid.SuspendLayout();
			this.numberDetailsPanel.SuspendLayout();
			this.issueDateEdit.SuspendLayout();
			this.entryTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.TransportBookingAdditionalReferenceCollection);
			// 
			// ReferenceNumbersGroupBox
			// 
			this.ReferenceNumbersGroupBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingsAdditionalReferencesUserControl|53f988bb-fa84-48ae-afb1-91d4b965255d", "Reference Numbers");
			this.ReferenceNumbersGroupBox.Controls.Add(this.numbersGrid);
			this.ReferenceNumbersGroupBox.Controls.Add(this.numberDetailsPanel);
			this.ReferenceNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbersGroupBox.Name = "ReferenceNumbersGroupBox";
			this.ReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 184, true);
			this.ReferenceNumbersGroupBox.TabIndex = 0;
			this.ReferenceNumbersGroupBox.TabStop = false;
			// 
			// numbersGrid
			// 
			this.numbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.numbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).EntryLineReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).AdditionalReferenceNumberTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).BookingOnly)));
			this.numbersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EntryType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "EntryNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "EntryLineReference";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "IssueDate";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("e6ddb29c-2010-4b2f-b270-1dc0ac18db9d", "Type Description");
			zTextBoxColumnStyleInfo3.ColumnName = "AdditionalReferenceNumberTypeDescription";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zBookingOnlyCheckBoxColumnStyleInfo1.ColumnName = "BookingOnly";
			zBookingOnlyCheckBoxColumnStyleInfo1.IsVisible = true;
			zBookingOnlyCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.numbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.numbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.numbersGrid.ColumnStyles.Add(zBookingOnlyCheckBoxColumnStyleInfo1);
			this.numbersGrid.CopySelectedRowsAllowed = true;
			this.numbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.numbersGrid.GridId = "d7f2d6aa-d6bd-48d2-bbf0-05b3281d39e0";
			this.numbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.numbersGrid.LayoutKey = "NumbersGrid";
			this.numbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.numbersGrid.Name = "numbersGrid";
			this.numbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 63, true);
			this.numbersGrid.TabIndex = 0;
			// 
			// numberDetailsPanel
			// 
			this.numberDetailsPanel.Controls.Add(this.issueDateEdit);
			this.numberDetailsPanel.Controls.Add(this.entryLineReferenceTextBox);
			this.numberDetailsPanel.Controls.Add(this.entryNumTextBox);
			this.numberDetailsPanel.Controls.Add(this.entryTypeDropEdit);
			this.numberDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.numberDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 78, true);
			this.numberDetailsPanel.Name = "numberDetailsPanel";
			this.numberDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 104, true);
			this.numberDetailsPanel.TabIndex = 5;
			// 
			// issueDateEdit
			// 
			this.issueDateEdit.AllowDrop = true;
			this.issueDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.issueDateEdit.AutoCompleteMonthThreshold = 1;
			this.issueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.issueDateEdit, "IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).IssueDate)));
			this.issueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 80, true);
			this.issueDateEdit.Name = "issueDateEdit";
			this.issueDateEdit.TabIndex = 4;
			// 
			// entryLineReferenceTextBox
			// 
			this.entryLineReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.entryLineReferenceTextBox, "EntryLineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).EntryLineReference)));
			this.entryLineReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 54, true);
			this.entryLineReferenceTextBox.Name = "entryLineReferenceTextBox";
			this.entryLineReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.entryLineReferenceTextBox.TabIndex = 3;
			// 
			// entryNumTextBox
			// 
			this.entryNumTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.entryNumTextBox, "EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).EntryNum)));
			this.entryNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 28, true);
			this.entryNumTextBox.Name = "entryNumTextBox";
			this.entryNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.entryNumTextBox.TabIndex = 2;
			// 
			// entryTypeDropEdit
			// 
			this.entryTypeDropEdit.AllowDrop = true;
			this.entryTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.entryTypeDropEdit, "EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReference)(null)).EntryType)));
			this.entryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 4, true);
			this.entryTypeDropEdit.Name = "entryTypeDropEdit";
			this.entryTypeDropEdit.PreBoundMaxLength = 3;
			this.entryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.entryTypeDropEdit.TabIndex = 1;
			// 
			// TransportBookingsAdditionalReferencesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumbersGroupBox);
			this.Name = "TransportBookingsAdditionalReferencesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 184, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumbersGroupBox.ResumeLayout(false);
			this.ReferenceNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numbersGrid)).EndInit();
			this.numbersGrid.ResumeLayout(false);
			this.numbersGrid.PerformLayout();
			this.numberDetailsPanel.ResumeLayout(false);
			this.numberDetailsPanel.PerformLayout();
			this.issueDateEdit.ResumeLayout(true);
			this.issueDateEdit.PerformLayout();
			this.entryTypeDropEdit.ResumeLayout(true);
			this.entryTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ReferenceNumbersGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit issueDateEdit;
		private Enterprise.ZArchitecture.ZTextBox entryLineReferenceTextBox;
		private Enterprise.ZArchitecture.ZTextBox entryNumTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit entryTypeDropEdit;
		private Enterprise.ZArchitecture.ZGrid numbersGrid;
		private ZArchitecture.GUI.ZPanel numberDetailsPanel;
	}
}
