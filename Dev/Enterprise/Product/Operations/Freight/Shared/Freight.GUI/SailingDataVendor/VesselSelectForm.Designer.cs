using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Resources;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	partial class VesselSelectForm
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

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.VesselsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.VesselNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LloydsNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VesselsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 313, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(296);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(297);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber);
			// 
			// VesselsGrid
			// 
			this.VesselsGrid.AllowNavigation = false;
			this.VesselsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.VesselsGrid, "AvailableVessels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)).SyncRoot)).RV_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)).SyncRoot)).RV_LloydsNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)).SyncRoot)).Header.OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)).SyncRoot)).RV_VesselType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefVessel)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)).SyncRoot)).RV_NetRegisterTon)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefVessel)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).AvailableVessels)).SyncRoot)).RV_RadioCallSign)));
			this.VesselsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|bc3609ee-6358-44c5-b5e9-c96f5e4edb95", "Vessel Name");
			zTextBoxColumnStyleInfo1.ColumnName = "RV_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|932bd97b-8668-4986-a59b-77dc3c587ce8", "Lloyds #");
			zTextBoxColumnStyleInfo2.ColumnName = "RV_LloydsNumber";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|114f19e5-5dca-44c1-a5f3-3cc747c1f24f", "Shipping Provider");
			zTextBoxColumnStyleInfo3.ColumnName = "Header+OH_Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "RV_VesselType";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RV_NetRegisterTon";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|2b7abec0-55b5-4861-931f-5ef3cc14bc9a", "Call Sign");
			zTextBoxColumnStyleInfo5.ColumnName = "RV_RadioCallSign";
			this.VesselsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.VesselsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.VesselsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.VesselsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VesselsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.VesselsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VesselsGrid.GridId = "43037c8d-22d6-4c6c-aed2-be2cc663bf7d";
			this.VesselsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VesselsGrid.IsWholeRowSelectedOnClick = true;
			this.VesselsGrid.LayoutKey = "VesselsGrid";
			this.VesselsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 102, true);
			this.VesselsGrid.Name = "VesselsGrid";
			this.VesselsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 176, true);
			this.VesselsGrid.TabIndex = 2;
			this.VesselsGrid.DoubleClick += new System.EventHandler(this.OnVesselsGrid_DoubleClick);
			// 
			// TopLabel
			// 
			this.TopLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|8a4982bc-9e06-433c-a970-c2603642d24e", "Select the correct match for the vessel with the given Vessel Name / Lloyds number. The Vessel Name and Lloyds number of the vessel you select will be overwritten with the following details.");
			this.TopLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.TopLabel.Name = "TopLabel";
			this.TopLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 39, true);
			this.TopLabel.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|a1dad67a-06fe-48a6-85fc-fed0b8338d99", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 283, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OnOK_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|7e2d519a-a733-4c36-87dd-30591c32d72d", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 283, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// VesselNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.VesselNameBoundTextBox, "VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).VesselName)));
			this.VesselNameBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|71dd2caa-8f10-4298-a86b-310da1fa8ed3", "Vessel Name");
			this.VesselNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 51, true);
			this.VesselNameBoundTextBox.Name = "VesselNameBoundTextBox";
			this.VesselNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VesselNameBoundTextBox.TabIndex = 0;
			// 
			// LloydsNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.LloydsNumberBoundTextBox, "LloydsNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber)(null)).LloydsNumber)));
			this.LloydsNumberBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|282cb39e-fc31-4adf-afee-d73add19a633", "Lloyds Number");
			this.LloydsNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 73, true);
			this.LloydsNumberBoundTextBox.Name = "LloydsNumberBoundTextBox";
			this.LloydsNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LloydsNumberBoundTextBox.TabIndex = 1;
			// 
			// VesselSelectForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 337, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselSelectForm|9c2f2826-b162-4e8e-b2dc-3c13a06dd6a2", "Ambiguous match for Vessel");
			this.Controls.Add(this.LloydsNumberBoundTextBox);
			this.Controls.Add(this.VesselNameBoundTextBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.VesselsGrid);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.TopLabel);
			this.DataSourceType = typeof(Enterprise.Freight.SailingDataVendor.Business.QueryUserSelectVesselFromLloydsNumber);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 371, true);
			this.Name = "VesselSelectForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopLabel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.VesselsGrid, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.VesselNameBoundTextBox, 0);
			this.Controls.SetChildIndex(this.LloydsNumberBoundTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VesselsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZGrid VesselsGrid;
		private Enterprise.ZArchitecture.ZLabel TopLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.ZTextBox VesselNameBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox LloydsNumberBoundTextBox;
	}
}
