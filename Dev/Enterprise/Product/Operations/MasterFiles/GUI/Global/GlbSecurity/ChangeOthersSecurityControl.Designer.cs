using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class ChangeOthersSecurityControl
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
			this.addGroupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.changeOthersSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.changeOthersSecurityGrid = new Enterprise.ZArchitecture.ZGrid();
			this.addStaffButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.deleteStaffOrGroupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.changeOthersSecurityGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity);
			// 
			// addGroupButton
			// 
			this.addGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addGroupButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|616781c5-cd3e-4e8b-b31b-d0f545d74dba", "Add Group");
			this.addGroupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 516, true);
			this.addGroupButton.Name = "addGroupButton";
			this.addGroupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.addGroupButton.TabIndex = 2;
			this.addGroupButton.UseVisualStyleBackColor = true;
			this.addGroupButton.Click += new System.EventHandler(this.AddGroupButton_Click);
			// 
			// changeOthersSecurityLabel
			// 
			this.changeOthersSecurityLabel.AutoSize = true;
			this.changeOthersSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|afa72d8a-c236-4a76-9f1c-4e47f4a340d4", "Modify security rights for the following staff and groups");
			this.changeOthersSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.changeOthersSecurityLabel.Name = "changeOthersSecurityLabel";
			this.changeOthersSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 13, true);
			this.changeOthersSecurityLabel.TabIndex = 0;
			// 
			// changeOthersSecurityGrid
			// 
			this.changeOthersSecurityGrid.AllowNavigation = false;
			this.changeOthersSecurityGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.changeOthersSecurityGrid, "SecurityChangeOthersView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(null)).SecurityChangeOthersView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(null)).SecurityChangeOthersView)).SyncRoot)).ItemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(null)).SecurityChangeOthersView)).SyncRoot)).ItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(null)).SecurityChangeOthersView)).SyncRoot)).ItemType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(null)).SecurityChangeOthersView)).SyncRoot)).ParentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(null)).SecurityChangeOthersView)).SyncRoot)).ParentType)));
			this.changeOthersSecurityGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|f6e7799e-20e1-46c2-a8a9-d83e78896ec5", "Item Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ItemCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|4baaee84-5cf1-499f-a273-99f9629173ff", "Item Name");
			zTextBoxColumnStyleInfo2.ColumnName = "ItemName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|00ad6e72-cf06-4717-9de5-193488d134c5", "Item Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ItemType";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|5d6e2c8c-c2d5-4cbd-88b1-b8bdb61a887e", "Owner Code");
			zTextBoxColumnStyleInfo4.ColumnName = "ParentCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|96170605-bb12-499d-a618-fe6b68fdb971", "Owner Description");
			zTextBoxColumnStyleInfo5.ColumnName = "ParentDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|2ecd2b5d-dc83-43e0-a3da-381607b4aa21", "Parent Record");
			zTextBoxColumnStyleInfo6.ColumnName = "ParentType";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.changeOthersSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.changeOthersSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.changeOthersSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.changeOthersSecurityGrid.GridId = "d62a0d64-d42a-460a-b84e-8e3d087d1654";
			this.changeOthersSecurityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.changeOthersSecurityGrid.LayoutKey = "ChangeOthersSecurityGrid";
			this.changeOthersSecurityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.changeOthersSecurityGrid.Name = "changeOthersSecurityGrid";
			this.changeOthersSecurityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 486, true);
			this.changeOthersSecurityGrid.TabIndex = 1;
			this.changeOthersSecurityGrid.RemoveAction = RemoveAction.RemoveAndDelete;
			this.changeOthersSecurityGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.changeOthersSecurityGrid.DoubleClick += new System.EventHandler(this.ChangeOthersSecurityGrid_DoubleClick);
			// 
			// addStaffButton
			// 
			this.addStaffButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addStaffButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|31f852f2-e6de-45df-aecf-e987191cb076", "Add Staff");
			this.addStaffButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 516, true);
			this.addStaffButton.Name = "addStaffButton";
			this.addStaffButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.addStaffButton.TabIndex = 3;
			this.addStaffButton.UseVisualStyleBackColor = true;
			this.addStaffButton.Click += new System.EventHandler(this.AddStaffButton_Click);
			// 
			// deleteStaffOrGroupButton
			// 
			this.deleteStaffOrGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteStaffOrGroupButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|845A68F5-4B2D-4760-8927-07A96FDF0993", "Delete");
			this.deleteStaffOrGroupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 516, true);
			this.deleteStaffOrGroupButton.Name = "deleteStaffOrGroupButton";
			this.deleteStaffOrGroupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.deleteStaffOrGroupButton.TabIndex = 3;
			this.deleteStaffOrGroupButton.UseVisualStyleBackColor = true;
			this.deleteStaffOrGroupButton.Click += new System.EventHandler(this.DeleteStaffOrGroupButton_Click);
			// 
			// ChangeOthersSecurityControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.addStaffButton);
			this.Controls.Add(this.addGroupButton);
			this.Controls.Add(this.deleteStaffOrGroupButton);
			this.Controls.Add(this.changeOthersSecurityLabel);
			this.Controls.Add(this.changeOthersSecurityGrid);
			this.Name = "ChangeOthersSecurityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 539, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.changeOthersSecurityGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton addGroupButton;
		private Enterprise.ZArchitecture.ZLabel changeOthersSecurityLabel;
		internal Enterprise.ZArchitecture.ZGrid changeOthersSecurityGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton addStaffButton;
		internal Enterprise.ZArchitecture.GUI.ZButton deleteStaffOrGroupButton;

		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
	}
}
