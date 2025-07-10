namespace Enterprise.Customs.US.LVS.GUI
{
	partial class BulkConvertToStandAloneDeclarationForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.panelBottom = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.checkBoxSelectAll = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.buttonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonSend = new Enterprise.ZArchitecture.GUI.ZButton();
			this.groupBoxHouseBills = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridHouseBills = new Enterprise.Customs.US.LVS.GUI.GridWithoutSettingHasChanges();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panelBottom.SuspendLayout();
			this.groupBoxHouseBills.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridHouseBills)).BeginInit();
			this.gridHouseBills.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearance);
			// 
			// panelBottom
			// 
			this.panelBottom.Controls.Add(this.checkBoxSelectAll);
			this.panelBottom.Controls.Add(this.buttonCancel);
			this.panelBottom.Controls.Add(this.buttonSend);
			this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 394, true);
			this.panelBottom.Name = "panelBottom";
			this.panelBottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 32, true);
			this.panelBottom.TabIndex = 1;
			// 
			// checkBoxSelectAll
			// 
			this.checkBoxSelectAll.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("4e581f58-a731-4ff8-9a07-8905c7d2a06d", "Select All");
			this.checkBoxSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.checkBoxSelectAll.Name = "checkBoxSelectAll";
			this.checkBoxSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 24, true);
			this.checkBoxSelectAll.TabIndex = 2;
			this.checkBoxSelectAll.Text = "Select All";
			this.checkBoxSelectAll.UseVisualStyleBackColor = true;
			this.checkBoxSelectAll.CheckedChanged += new System.EventHandler(this.CheckBoxSelectAll_CheckedChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("de95de19-4c4d-4ab8-9401-7859c8586a5b", "Cancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.IsCaptionOverridden = false;
			this.buttonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 5, true);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.buttonCancel.ToolTipCaption = null;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonSend
			// 
			this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSend.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("dc356510-c005-43b4-ad45-245cc5d1fb07", "Send");
			this.buttonSend.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonSend.IsCaptionOverridden = false;
			this.buttonSend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 5, true);
			this.buttonSend.Name = "buttonSend";
			this.buttonSend.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonSend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.buttonSend.TabIndex = 3;
			this.buttonSend.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.buttonSend.ToolTipCaption = null;
			this.buttonSend.UseVisualStyleBackColor = true;
			this.buttonSend.Enabled = false;
			// 
			// groupBoxHouseBills
			// 
			this.groupBoxHouseBills.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1ff6367d-110a-416a-b550-4231d310c5dc", "House Bills");
			this.groupBoxHouseBills.Controls.Add(this.gridHouseBills);
			this.groupBoxHouseBills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxHouseBills.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxHouseBills.Name = "groupBoxHouseBills";
			this.groupBoxHouseBills.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 394, true);
			this.groupBoxHouseBills.TabIndex = 2;
			this.groupBoxHouseBills.TabStop = false;
			// 
			// gridHouseBills
			// 
			this.gridHouseBills.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridHouseBills, "NonApplicableConsignments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).NonApplicableConsignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).NonApplicableConsignments)).SyncRoot)).ULB_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).NonApplicableConsignments)).SyncRoot)).ShouldConvertToStandaloneDeclaration)));
			this.gridHouseBills.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ULB_HouseBill";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldConvertToStandaloneDeclaration";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.gridHouseBills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridHouseBills.GridId = "1ad81c9f-5f35-49a4-9003-e37e9ca4f959";
			this.gridHouseBills.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridHouseBills.LayoutKey = "gridHouseBills";
			this.gridHouseBills.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridHouseBills.Name = "gridHouseBills";
			this.gridHouseBills.RemoveAction = ZArchitecture.RemoveAction.NoRemovePossible;
			this.gridHouseBills.ShouldSetErrorsOnTabPage = false;
			this.gridHouseBills.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 375, true);
			this.gridHouseBills.TabIndex = 0;
			// 
			// BulkConvertToStandAloneDeclarationForm
			// 
			this.AcceptButton = this.buttonSend;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.buttonCancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e056077c-4214-489b-86f2-2327ffc30761", "Convert to Stand Alone Declaration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 450, true);
			this.Controls.Add(this.groupBoxHouseBills);
			this.Controls.Add(this.panelBottom);
			this.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearance);
			this.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Browse;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "BulkConvertToStandAloneDeclarationForm";
			this.Text = "Convert to Stand Alone Declaration";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.panelBottom, 0);
			this.Controls.SetChildIndex(this.groupBoxHouseBills, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panelBottom.ResumeLayout(false);
			this.panelBottom.PerformLayout();
			this.groupBoxHouseBills.ResumeLayout(false);
			this.groupBoxHouseBills.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridHouseBills)).EndInit();
			this.gridHouseBills.ResumeLayout(false);
			this.gridHouseBills.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel panelBottom;
		private ZArchitecture.GUI.ZButton buttonCancel;
		private ZArchitecture.GUI.ZButton buttonSend;
		private ZArchitecture.GUI.ZGroupBox groupBoxHouseBills;
		private Enterprise.Customs.US.LVS.GUI.GridWithoutSettingHasChanges gridHouseBills;
		private ZArchitecture.GUI.ZCheckBox checkBoxSelectAll;
	}
}
