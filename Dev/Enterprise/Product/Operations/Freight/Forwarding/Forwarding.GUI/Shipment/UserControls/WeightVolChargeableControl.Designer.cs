using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class WeightVolChargeableControl : ZUserControl
	{
		ZGroupBox WeightVolume;
		ZPanel WeightVolumePanel;
		ZCalcEdit JS_DocumentedWeightBoundCalcEdit;
		ZCalcEdit JS_DocumentedVolumeBoundCalcEdit;
		ZCalcEdit JS_DocumentedChargeableCalcEdit;
		ZCalcEdit JS_ManifestedVolumeCalcEdit;
		ZLabel DocsChargeableUnitLabel;
		ZLabel DocsVolumeUnitLabel;
		ZLabel DocsWeightUnitLabel;
		ZLabel ManifestedValuesLabel;
		ZLabel DocumentedValuesLabel;
		ZCalcEdit JS_ManifestedChargeableCalcEdit;
		ZCalcEdit JS_ManifestedWeightCalcEdit;
		private ZPanel ShowBillPanel;
		internal ZButton ViewEditBillButton;
		private ZPanel ValuesGroupPanel;
		protected ZCalcEdit JS_ManifestedLoadingMetersCalcEdit;
		protected ZCalcEdit JS_DocumentedLoadingMetersCalcEdit;
		ZPanel UserConfigurableDetailsPanel;
		ZDynamicControlCreationUserControl DensityBox;

		void InitializeComponent()
		{
			this.WeightVolume = new ZGroupBox();
			this.ShowBillPanel = new ZPanel();
			this.ViewEditBillButton = new ZButton();
			this.WeightVolumePanel = new ZPanel();
			this.ValuesGroupPanel = new ZPanel();
			this.DocsWeightUnitLabel = new ZLabel();
			this.DocsVolumeUnitLabel = new ZLabel();
			this.JS_ManifestedLoadingMetersCalcEdit = new ZCalcEdit();
			this.JS_DocumentedWeightBoundCalcEdit = new ZCalcEdit();
			this.JS_ManifestedWeightCalcEdit = new ZCalcEdit();
			this.JS_DocumentedVolumeBoundCalcEdit = new ZCalcEdit();
			this.JS_ManifestedVolumeCalcEdit = new ZCalcEdit();
			this.JS_DocumentedLoadingMetersCalcEdit = new ZCalcEdit();
			this.JS_ManifestedChargeableCalcEdit = new ZCalcEdit();
			this.JS_DocumentedChargeableCalcEdit = new ZCalcEdit();
			this.DocsChargeableUnitLabel = new ZLabel();
			this.ManifestedValuesLabel = new ZLabel();
			this.DocumentedValuesLabel = new ZLabel();
			this.UserConfigurableDetailsPanel = new ZPanel();
			this.DensityBox = new ZDynamicControlCreationUserControl();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WeightVolume.SuspendLayout();
			this.ShowBillPanel.SuspendLayout();
			this.WeightVolumePanel.SuspendLayout();
			this.ValuesGroupPanel.SuspendLayout();
			this.DensityBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			//
			// WeightVolume
			//
			this.WeightVolume.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|8a05c169-0c55-4ab9-a829-b77254db815a", "Weight/Volume/Chargeable Details");
			this.WeightVolume.Controls.Add(this.ShowBillPanel);
			this.WeightVolume.Controls.Add(this.WeightVolumePanel);
			this.WeightVolume.Controls.Add(this.UserConfigurableDetailsPanel);
			this.WeightVolume.Dock = System.Windows.Forms.DockStyle.Left;
			this.WeightVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WeightVolume.Name = "WeightVolume";
			this.WeightVolume.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.WeightVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 156, true);
			this.WeightVolume.TabIndex = 0;
			this.WeightVolume.TabStop = false;
			//
			// ShowBillPanel
			//
			this.ShowBillPanel.Controls.Add(this.ViewEditBillButton);
			this.ShowBillPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShowBillPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 123, true);
			this.ShowBillPanel.Name = "ShowBillPanel";
			this.ShowBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 27, true);
			this.ShowBillPanel.TabIndex = 2;
			//
			// ViewEditBillButton
			//
			this.ViewEditBillButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|f8046a0d-d249-4ba0-97c2-9fcf36673d14", "View/Edit Bill");
			this.ViewEditBillButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 2, true);
			this.ViewEditBillButton.Name = "ViewEditBillButton";
			this.ViewEditBillButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
			this.ViewEditBillButton.TabIndex = 0;
			this.ViewEditBillButton.UseVisualStyleBackColor = true;
			this.ViewEditBillButton.Click += new EventHandler(this.ViewEditBillButton_Click);
			//
			// WeightVolumePanel
			//
			this.WeightVolumePanel.Controls.Add(this.ValuesGroupPanel);
			this.WeightVolumePanel.Controls.Add(this.ManifestedValuesLabel);
			this.WeightVolumePanel.Controls.Add(this.DocumentedValuesLabel);
			this.WeightVolumePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.WeightVolumePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 12, true);
			this.WeightVolumePanel.Name = "WeightVolumePanel";
			this.WeightVolumePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 110, true);
			this.WeightVolumePanel.TabIndex = 0;
			//
			// ValuesGroupPanel
			//
			this.ValuesGroupPanel.Controls.Add(this.DocsWeightUnitLabel);
			this.ValuesGroupPanel.Controls.Add(this.DocsVolumeUnitLabel);
			this.ValuesGroupPanel.Controls.Add(this.JS_ManifestedLoadingMetersCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_DocumentedWeightBoundCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_ManifestedWeightCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_DocumentedVolumeBoundCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_ManifestedVolumeCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_DocumentedLoadingMetersCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_ManifestedChargeableCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.JS_DocumentedChargeableCalcEdit);
			this.ValuesGroupPanel.Controls.Add(this.DocsChargeableUnitLabel);
			this.ValuesGroupPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 16, true);
			this.ValuesGroupPanel.Name = "ValuesGroupPanel";
			this.ValuesGroupPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 89, true);
			//
			// DocsWeightUnitLabel
			//
			this.DocsWeightUnitLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DocsWeightUnitLabel, "JS_UnitOfWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).JS_UnitOfWeight)));
			this.DocsWeightUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|a21e1db4-cdf9-4c87-8e19-6b5ae671622b", "KG");
			this.DocsWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 4, true);
			this.DocsWeightUnitLabel.Name = "DocsWeightUnitLabel";
			this.DocsWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.DocsWeightUnitLabel.TabIndex = 3;
			//
			// DocsVolumeUnitLabel
			//
			this.DocsVolumeUnitLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DocsVolumeUnitLabel, "JS_UnitOfVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).JS_UnitOfVolume)));
			this.DocsVolumeUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|08773df8-e15c-495b-b110-f3a17a851ebc", "M3");
			this.DocsVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 26, true);
			this.DocsVolumeUnitLabel.Name = "DocsVolumeUnitLabel";
			this.DocsVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.DocsVolumeUnitLabel.TabIndex = 6;
			//
			// JS_ManifestedLoadingMetersCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_ManifestedLoadingMetersCalcEdit, "JS_ManifestedLoadingMeters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ManifestedLoadingMeters)));
			this.JS_ManifestedLoadingMetersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 44, true);
			this.JS_ManifestedLoadingMetersCalcEdit.Name = "JS_ManifestedLoadingMetersCalcEdit";
			this.JS_ManifestedLoadingMetersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_ManifestedLoadingMetersCalcEdit.TabIndex = 8;
			this.JS_ManifestedLoadingMetersCalcEdit.Text = "0.000";
			this.JS_ManifestedLoadingMetersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_DocumentedWeightBoundCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_DocumentedWeightBoundCalcEdit, "JS_DocumentedWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_DocumentedWeight)));
			this.JS_DocumentedWeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JS_DocumentedWeightBoundCalcEdit.Name = "JS_DocumentedWeightBoundCalcEdit";
			this.JS_DocumentedWeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_DocumentedWeightBoundCalcEdit.TabIndex = 1;
			this.JS_DocumentedWeightBoundCalcEdit.Text = "0.000";
			this.JS_DocumentedWeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_ManifestedWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_ManifestedWeightCalcEdit, "JS_ManifestedWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ManifestedWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_ManifestedWeightCalcEdit, false);
			this.JS_ManifestedWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 0, true);
			this.JS_ManifestedWeightCalcEdit.Name = "JS_ManifestedWeightCalcEdit";
			this.JS_ManifestedWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_ManifestedWeightCalcEdit.TabIndex = 2;
			this.JS_ManifestedWeightCalcEdit.Text = "0.000";
			this.JS_ManifestedWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_DocumentedVolumeBoundCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_DocumentedVolumeBoundCalcEdit, "JS_DocumentedVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_DocumentedVolume)));
			this.JS_DocumentedVolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.JS_DocumentedVolumeBoundCalcEdit.Name = "JS_DocumentedVolumeBoundCalcEdit";
			this.JS_DocumentedVolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_DocumentedVolumeBoundCalcEdit.TabIndex = 4;
			this.JS_DocumentedVolumeBoundCalcEdit.Text = "0.000";
			this.JS_DocumentedVolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_ManifestedVolumeCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_ManifestedVolumeCalcEdit, "JS_ManifestedVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ManifestedVolume)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_ManifestedVolumeCalcEdit, false);
			this.JS_ManifestedVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 22, true);
			this.JS_ManifestedVolumeCalcEdit.Name = "JS_ManifestedVolumeCalcEdit";
			this.JS_ManifestedVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_ManifestedVolumeCalcEdit.TabIndex = 5;
			this.JS_ManifestedVolumeCalcEdit.Text = "0.000";
			this.JS_ManifestedVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_DocumentedLoadingMetersCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_DocumentedLoadingMetersCalcEdit, "JS_DocumentedLoadingMeters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_DocumentedLoadingMeters)));
			this.JS_DocumentedLoadingMetersCalcEdit.DecimalPlaces = 3;
			this.JS_DocumentedLoadingMetersCalcEdit.Decimals = 3;
			this.JS_DocumentedLoadingMetersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
			this.JS_DocumentedLoadingMetersCalcEdit.Name = "JS_DocumentedLoadingMetersCalcEdit";
			this.JS_DocumentedLoadingMetersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_DocumentedLoadingMetersCalcEdit.TabIndex = 7;
			this.JS_DocumentedLoadingMetersCalcEdit.Text = "0.000";
			this.JS_DocumentedLoadingMetersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_ManifestedChargeableCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_ManifestedChargeableCalcEdit, "JS_ManifestedChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ManifestedChargeable)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_ManifestedChargeableCalcEdit, false);
			this.JS_ManifestedChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 66, true);
			this.JS_ManifestedChargeableCalcEdit.Name = "JS_ManifestedChargeableCalcEdit";
			this.JS_ManifestedChargeableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_ManifestedChargeableCalcEdit.TabIndex = 10;
			this.JS_ManifestedChargeableCalcEdit.Text = "0.000";
			this.JS_ManifestedChargeableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_DocumentedChargeableCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JS_DocumentedChargeableCalcEdit, "JS_DocumentedChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_DocumentedChargeable)));
			this.JS_DocumentedChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 66, true);
			this.JS_DocumentedChargeableCalcEdit.Name = "JS_DocumentedChargeableCalcEdit";
			this.JS_DocumentedChargeableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.JS_DocumentedChargeableCalcEdit.TabIndex = 9;
			this.JS_DocumentedChargeableCalcEdit.Text = "0.000";
			this.JS_DocumentedChargeableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// DocsChargeableUnitLabel
			//
			this.DocsChargeableUnitLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DocsChargeableUnitLabel, "JS_ChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).JS_ChargeableUnit)));
			this.DocsChargeableUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|283b85a7-33a8-4818-94d5-724734862e07", "M3");
			this.DocsChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 70, true);
			this.DocsChargeableUnitLabel.Name = "DocsChargeableUnitLabel";
			this.DocsChargeableUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.DocsChargeableUnitLabel.TabIndex = 11;
			//
			// ManifestedValuesLabel
			//
			this.ManifestedValuesLabel.AutoSize = true;
			this.ManifestedValuesLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|1d54ee6e-b85e-4073-a7a6-e12b54a7764b", "Carrier");
			this.ManifestedValuesLabel.IsFontBold = true;
			this.ManifestedValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 3, true);
			this.ManifestedValuesLabel.Name = "ManifestedValuesLabel";
			this.ManifestedValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 14, true);
			this.ManifestedValuesLabel.TabIndex = 1;
			//
			// DocumentedValuesLabel
			//
			this.DocumentedValuesLabel.AutoSize = true;
			this.DocumentedValuesLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("WeightVolChargeableControl|b77651fb-6914-4bfd-bb7d-a0eb850eecf9", "Client");
			this.DocumentedValuesLabel.IsFontBold = true;
			this.DocumentedValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 3, true);
			this.DocumentedValuesLabel.Name = "DocumentedValuesLabel";
			this.DocumentedValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 14, true);
			this.DocumentedValuesLabel.TabIndex = 0;
			//
			// UserConfigurableDetailsPanel
			//
			this.UserConfigurableDetailsPanel.AutoSize = true;
			this.UserConfigurableDetailsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.UserConfigurableDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.UserConfigurableDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 12, true);
			this.UserConfigurableDetailsPanel.Name = "UserConfigurableDetailsPanel";
			this.UserConfigurableDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 0, true);
			this.UserConfigurableDetailsPanel.TabIndex = 1;
			//
			// DensityBox
			//
			this.DensityBox.AllowDrop = true;
			this.DensityBox.AutoSize = true;
			this.DensityBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DensityBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 0, true);
			this.DensityBox.Name = "DensityBox";
			this.DensityBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 156, true);
			this.DensityBox.TabIndex = 0;
			this.DensityBox.TabStop = false;
			this.DensityBox.UserControlType = typeof(DensityValuesControl);
			//
			// WeightVolChargeableControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DensityBox);
			this.Controls.Add(this.WeightVolume);
			this.Name = "WeightVolChargeableControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 156, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.WeightVolume.ResumeLayout(false);
			this.WeightVolume.PerformLayout();
			this.ShowBillPanel.ResumeLayout(false);
			this.ShowBillPanel.PerformLayout();
			this.WeightVolumePanel.ResumeLayout(false);
			this.WeightVolumePanel.PerformLayout();
			this.ValuesGroupPanel.ResumeLayout(false);
			this.ValuesGroupPanel.PerformLayout();
			this.DensityBox.ResumeLayout(true);
			this.DensityBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
