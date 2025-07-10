using System;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class AllocateSelectionJobMawbForm : ZForm
	{
		private ZGroupBox BorrowedOutGroupBox;
		private ZCheckBox IsCompletedzCheckBox;
		private ZGuidFindBox AllocatedToBoundFindBox;
		private ZCheckBox IsBorrowedzCheckBox;
		private Core.Forms.ZPostingButtonsUserControl PostButtons;
		private ZDateEdit AllocatedUntilBoundDateEdit;

		protected new void InitializeComponent()
		{
			this.BorrowedOutGroupBox = new ZGroupBox();
			this.AllocatedUntilBoundDateEdit = new ZDateEdit();
			this.IsCompletedzCheckBox = new ZCheckBox();
			this.AllocatedToBoundFindBox = new ZGuidFindBox();
			this.IsBorrowedzCheckBox = new ZCheckBox();
			this.PostButtons = new Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BorrowedOutGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AllocateSelectionJobMawb);
			// 
			// BorrowedOutGroupBox
			// 
			this.BorrowedOutGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocateSelectionJobMawbForm|d375ef36-0c86-427d-b07b-17f6aecbb832", "Borrow \'out\' Waybills");
			this.BorrowedOutGroupBox.Controls.Add(this.AllocatedUntilBoundDateEdit);
			this.BorrowedOutGroupBox.Controls.Add(this.IsCompletedzCheckBox);
			this.BorrowedOutGroupBox.Controls.Add(this.AllocatedToBoundFindBox);
			this.BorrowedOutGroupBox.Controls.Add(this.IsBorrowedzCheckBox);
			this.BorrowedOutGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.BorrowedOutGroupBox.Name = "BorrowedOutGroupBox";
			this.BorrowedOutGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 104, true);
			this.BorrowedOutGroupBox.TabIndex = 9;
			this.BorrowedOutGroupBox.TabStop = false;
			// 
			// AllocatedUntilBoundDateEdit
			// 
			this.AllocatedUntilBoundDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.AllocatedUntilBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AllocatedUntilBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AllocatedUntilBoundDateEdit, "ReservedUntil");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AllocateSelectionJobMawb)(null)).ReservedUntil)));
			this.AllocatedUntilBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocateSelectionJobMawbForm|e4bc34df-36b4-41fc-a74e-af0f3602a122", "Borrow \'out\' Until");
			this.AllocatedUntilBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 42, true);
			this.AllocatedUntilBoundDateEdit.Name = "AllocatedUntilBoundDateEdit";
			this.AllocatedUntilBoundDateEdit.TabIndex = 2;
			// 
			// IsCompletedzCheckBox
			// 
			this.IsCompletedzCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsCompletedzCheckBox, "IsCompletedAWBReturned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((AllocateSelectionJobMawb)(null)).IsCompletedAWBReturned)));
			this.IsCompletedzCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsCompletedzCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocateSelectionJobMawbForm|19ebdf6c-b0f3-4b34-b05c-f7dcff822343", "Completed AWB Returned");
			this.IsCompletedzCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCompletedzCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 75, true);
			this.IsCompletedzCheckBox.Name = "IsCompletedzCheckBox";
			this.IsCompletedzCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsCompletedzCheckBox.TabIndex = 3;
			// 
			// AllocatedToBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AllocatedToBoundFindBox, "AllocatedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AllocateSelectionJobMawb)(null)).AllocatedTo)));
			this.AllocatedToBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocateSelectionJobMawbForm|c11089a7-9d3e-4643-a04b-0704d6a160aa", "Borrow \'out\' Waybill to");
			this.AllocatedToBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 22, true);
			this.AllocatedToBoundFindBox.Name = "AllocatedToBoundFindBox";
			this.AllocatedToBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.AllocatedToBoundFindBox.TabIndex = 1;
			// 
			// IsBorrowedzCheckBox
			// 
			this.IsBorrowedzCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsBorrowedzCheckBox, "IsBorrowedAWBInvoiced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((AllocateSelectionJobMawb)(null)).IsBorrowedAWBInvoiced)));
			this.IsBorrowedzCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsBorrowedzCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocateSelectionJobMawbForm|28b1b536-5952-4e96-9b46-88eaf0d5c12d", "Borrowed AWB Invoiced");
			this.IsBorrowedzCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsBorrowedzCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 75, true);
			this.IsBorrowedzCheckBox.Name = "IsBorrowedzCheckBox";
			this.IsBorrowedzCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsBorrowedzCheckBox.TabIndex = 4;
			// 
			// PostButtons
			// 
			this.PostButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButtons.AutoSize = true;
			this.PostButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 117, true);
			this.PostButtons.Name = "PostButtons";
			this.PostButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.PostButtons.TabIndex = 5;
			// 
			// AllocateSelectionJobMawbForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 175, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocateSelectionJobMawbForm|1df41a33-76b5-4127-89cc-af61762a15bf", "Borrow \'out\' selected Waybills");
			this.Controls.Add(this.PostButtons);
			this.Controls.Add(this.BorrowedOutGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(AllocateSelectionJobMawb);
			this.DataSourceTypeName = "AllocateSelectionJobMawb";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "AllocateSelectionJobMawbForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BorrowedOutGroupBox, 0);
			this.Controls.SetChildIndex(this.PostButtons, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BorrowedOutGroupBox.ResumeLayout(false);
			this.BorrowedOutGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
