using System;

namespace Enterprise.Customs.NL.GUI
{
	partial class EntrySelectionForm
	{
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntrySelectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntrySelectionGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntrySelectionGrid)).BeginInit();
			this.EntrySelectionGrid.SuspendLayout();
			this.EntrySelectionGridGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 289, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("0AA1BB83-7E35-4950-84AB-53D192AE59CE", "Select");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 263, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 3;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("153C617A-BE4A-4FF9-8478-BA8F7A23BFBC", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 263, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// EntrySelectionGrid
			// 
			this.EntrySelectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntrySelectionGrid, "EntrySelections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).EntrySelections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NL.Business.EntrySelection)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).EntrySelections)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.EntrySelection)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).EntrySelections)).SyncRoot)).EntryNumber)));
			this.EntrySelectionGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.EntrySelectionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntrySelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntrySelectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntrySelectionGrid.GridId = "680DF1ED-F49C-4F58-B764-FABF56EDB462";
			this.EntrySelectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntrySelectionGrid.LayoutKey = "EntrySelectionGridControl";
			this.EntrySelectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.EntrySelectionGrid.Name = "EntrySelectionGrid";
			this.EntrySelectionGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EntrySelectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 243, true);
			this.EntrySelectionGrid.TabIndex = 1;
			// 
			// EntrySelectionGridGroupBox
			// 
			this.EntrySelectionGridGroupBox.Controls.Add(this.EntrySelectionGrid);
			this.EntrySelectionGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntrySelectionGridGroupBox.Name = "EntrySelectionGridGroupBox";
			this.EntrySelectionGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 259, true);
			this.EntrySelectionGridGroupBox.TabIndex = 2;
			this.EntrySelectionGridGroupBox.TabStop = false;
			// 
			// EntrySelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 313, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.EntrySelectionGridGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 350, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 350, true);
			this.Name = "EntrySelectionForm";
			this.Controls.SetChildIndex(this.EntrySelectionGridGroupBox, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntrySelectionGrid)).EndInit();
			this.EntrySelectionGrid.ResumeLayout(false);
			this.EntrySelectionGrid.PerformLayout();
			this.EntrySelectionGridGroupBox.ResumeLayout(false);
			this.EntrySelectionGridGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZButton OkButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.ZGrid EntrySelectionGrid;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox EntrySelectionGridGroupBox;
	}
}
