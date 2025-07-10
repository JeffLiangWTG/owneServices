using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BulkDeactivationForm : ZChildForm
	{
		private ZButton CancelBtn;
		private ZButton ContinueBtn;
		private ZLabel TextLabel;
		private ZCalcEdit EstimatedProductCountCalcEdit;
		private ZRadioButton ARadioButton;
		private ZRadioButton DRadioButton;
		private ZLabel ProdSelectedLabel;

		protected override void InitializeComponent()
		{
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContinueBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EstimatedProductCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ARadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.DRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ProdSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 219, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 10, true);
			this.MainStatusBar.TabIndex = 7;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBulkDeactivator);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|423c116d-359c-430d-bf87-3d1b780a491d", "Cancel");
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 183, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBtn.TabIndex = 6;
			this.CancelBtn.UseVisualStyleBackColor = true;
			// 
			// ContinueBtn
			// 
			this.ContinueBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueBtn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|5ba1669f-156b-4763-a5b5-1331b6f34f5c", "Continue");
			this.ContinueBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 183, true);
			this.ContinueBtn.Name = "ContinueBtn";
			this.ContinueBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ContinueBtn.TabIndex = 5;
			this.ContinueBtn.UseVisualStyleBackColor = true;
			this.ContinueBtn.Click += new System.EventHandler(this.ContinueBtn_Click);
			// 
			// TextLabel
			// 
			this.TextLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|31b63983-e30e-43aa-92c8-01355aa03b39", "", "This form allows you to do a \'Bulk\' Deactivation/Activation of all products currently selected by the Grid Filter selections that you have made. If some products have Stock on hand quantities - they cannot be deactivated. Number of Deactivated/Activated products will be provided when process will be completed.");
			this.TextLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.TextLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, true);
			this.TextLabel.Name = "TextLabel";
			this.TextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 45, true);
			this.TextLabel.TabIndex = 0;
			this.TextLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// EstimatedProductCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EstimatedProductCountCalcEdit, "EstimatedProductCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBulkDeactivator)(null)).EstimatedProductCount)));
			this.EstimatedProductCountCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.EstimatedProductCountCalcEdit.DecimalPlaces = 0;
			this.EstimatedProductCountCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EstimatedProductCountCalcEdit, false);
			this.EstimatedProductCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 119, true);
			this.EstimatedProductCountCalcEdit.Name = "EstimatedProductCountCalcEdit";
			this.EstimatedProductCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.EstimatedProductCountCalcEdit.TabIndex = 3;
			this.EstimatedProductCountCalcEdit.Text = "0";
			this.EstimatedProductCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ARadioButton
			// 
			this.ARadioButton.AutoCheck = false;
			this.ARadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ARadioButton, "ShouldActivate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierBulkDeactivator)(null)).ShouldActivate)));
			this.ARadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|cb757a1b-91a8-4bb2-8484-b19877173c7c", "Activate");
			this.ARadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 81, true);
			this.ARadioButton.Name = "ARadioButton";
			this.ARadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.ARadioButton.TabIndex = 2;
			this.ARadioButton.UseVisualStyleBackColor = true;
			// 
			// DRadioButton
			// 
			this.DRadioButton.AutoCheck = false;
			this.DRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DRadioButton, "ShouldDeactivate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierBulkDeactivator)(null)).ShouldDeactivate)));
			this.DRadioButton.Checked = true;
			this.DRadioButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|3e735b1f-5805-4763-b12f-bfda9327a6a5", "Deactivate");
			this.DRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 81, true);
			this.DRadioButton.Name = "DRadioButton";
			this.DRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.DRadioButton.TabIndex = 1;
			this.DRadioButton.TabStop = true;
			this.DRadioButton.UseVisualStyleBackColor = true;
			// 
			// ProdSelectedLabel
			// 
			this.ProdSelectedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|762cbe77-3a56-480b-ac2b-4e51ed2af96e", "", "Products selected for Deactivation/Activation. Number of products here may be different from number of products in the module grid, because only products with appropriate status will be affected.");
			this.ProdSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 119, true);
			this.ProdSelectedLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, true);
			this.ProdSelectedLabel.Name = "ProdSelectedLabel";
			this.ProdSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 55, true);
			this.ProdSelectedLabel.TabIndex = 8;
			this.ProdSelectedLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// BulkDeactivationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 242, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkDeactivationForm|47c2372a-4405-46cf-81fc-18d3cda9a6c9", "Bulk Deactivation");
			this.Controls.Add(this.ProdSelectedLabel);
			this.Controls.Add(this.ARadioButton);
			this.Controls.Add(this.DRadioButton);
			this.Controls.Add(this.TextLabel);
			this.Controls.Add(this.ContinueBtn);
			this.Controls.Add(this.EstimatedProductCountCalcEdit);
			this.Controls.Add(this.CancelBtn);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBulkDeactivator);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.OrgSupplierBulkDeactivator";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 300, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 224, true);
			this.Name = "BulkDeactivationForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, true);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.EstimatedProductCountCalcEdit, 0);
			this.Controls.SetChildIndex(this.ContinueBtn, 0);
			this.Controls.SetChildIndex(this.TextLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DRadioButton, 0);
			this.Controls.SetChildIndex(this.ARadioButton, 0);
			this.Controls.SetChildIndex(this.ProdSelectedLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
