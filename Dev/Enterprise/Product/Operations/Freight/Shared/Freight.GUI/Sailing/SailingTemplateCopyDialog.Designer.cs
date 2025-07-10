using Enterprise.ZArchitecture;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.GUI
{
	partial class SailingTemplateCopyDialog : ZChildForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

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
		new void InitializeComponent()
		{
			this.BottomPanel = new CargoWise.Windows.UI.KPanel();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReferencePortBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferencePortDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AdjustedDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(246);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.SailingTemplateCopyCriteria);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CancelZButton);
			this.BottomPanel.Controls.Add(this.CopyZButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 23, true);
			this.BottomPanel.TabIndex = 6;
			// 
			// CancelZButton
			// 
			this.CancelZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelZButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|c8011b3a-e62e-4090-acad-1029d2da0e8b", "Cancel");
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 0, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelZButton.TabIndex = 1;
			this.CancelZButton.UseVisualStyleBackColor = true;
			this.CancelZButton.Click += new System.EventHandler(this.CancelZButton_Click);
			// 
			// CopyZButton
			// 
			this.CopyZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyZButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|875b1936-b2d8-4200-bfd1-b82fce6ebafd", "Copy");
			this.CopyZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 0, true);
			this.CopyZButton.Name = "CopyZButton";
			this.CopyZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CopyZButton.TabIndex = 0;
			this.CopyZButton.UseVisualStyleBackColor = true;
			this.CopyZButton.Click += new System.EventHandler(this.CopyZButton_Click);
			// 
			// ReferencePortBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferencePortBoundTextBox, "ReferencePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.SailingTemplateCopyCriteria)(null)).ReferencePort)));
			this.ReferencePortBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|17ec570d-e496-4f16-acc0-59129bc8420f", "Reference");
			this.ReferencePortBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.ReferencePortBoundTextBox.Name = "ReferencePortBoundTextBox";
			this.ReferencePortBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReferencePortBoundTextBox.TabIndex = 2;
			// 
			// ReferencePortDateBoundDateEdit
			// 
			this.ReferencePortDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReferencePortDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReferencePortDateBoundDateEdit, "ReferencePortDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.SailingTemplateCopyCriteria)(null)).ReferencePortDate)));
			this.ReferencePortDateBoundDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|8a0e0a56-fe1d-4a4b-beae-c2255dc62839", "From Date");
			this.ReferencePortDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.ReferencePortDateBoundDateEdit.Name = "ReferencePortDateBoundDateEdit";
			this.ReferencePortDateBoundDateEdit.TabIndex = 3;
			// 
			// AdjustedDateBoundDateEdit
			// 
			this.AdjustedDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AdjustedDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AdjustedDateBoundDateEdit, "AdjustedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.SailingTemplateCopyCriteria)(null)).AdjustedDate)));
			this.AdjustedDateBoundDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|88b58d62-7507-4bac-b006-269ff1806f9c", "To Date");
			this.AdjustedDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 56, true);
			this.AdjustedDateBoundDateEdit.Name = "AdjustedDateBoundDateEdit";
			this.AdjustedDateBoundDateEdit.TabIndex = 4;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox1, "RetainVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.SailingTemplateCopyCriteria)(null)).RetainVessel)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|3c38ce80-646e-411f-8061-bc506cb78430", "Retain Vessel Information");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 80, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.zCheckBox1.TabIndex = 5;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// SailingTemplateCopyDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelZButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 156, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("SailingTemplateCopyDialog|8b3fb42e-45fb-49e1-b4c9-bb1482673755", "Copy Sailing Schedule");
			this.Controls.Add(this.zCheckBox1);
			this.Controls.Add(this.AdjustedDateBoundDateEdit);
			this.Controls.Add(this.ReferencePortDateBoundDateEdit);
			this.Controls.Add(this.ReferencePortBoundTextBox);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Business.SailingTemplateCopyCriteria);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SailingTemplateCopyDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ReferencePortBoundTextBox, 0);
			this.Controls.SetChildIndex(this.ReferencePortDateBoundDateEdit, 0);
			this.Controls.SetChildIndex(this.AdjustedDateBoundDateEdit, 0);
			this.Controls.SetChildIndex(this.zCheckBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KPanel BottomPanel;
		private ZButton CancelZButton;
		private ZButton CopyZButton;
		private ZTextBox ReferencePortBoundTextBox;
		private ZDateEdit ReferencePortDateBoundDateEdit;
		private ZDateEdit AdjustedDateBoundDateEdit;
		private ZCheckBox zCheckBox1;
	}
}
