namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentPackingDetailForm
	{
		#region Dispose

		protected override void Dispose(bool IsNotFinalizing)
		{
			if (IsNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(IsNotFinalizing);
		}

		#endregion

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		protected new void InitializeComponent()
		{
			this.packageDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.shipmentTotalsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.outerPackTotalsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.packLinesContol = new Enterprise.Freight.Forwarding.GUI.PackLinesContol();
			this.shipmentWeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.totalWeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.shipmentVolumeUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.totalVolumeUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.actualVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.actualWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.totalPackageCountBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.totalPackLineVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.totalPackLineWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.totalPackLinesPackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.matchShipmentTotalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.contentContainerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.packageDetailsGroupBox.SuspendLayout();
			this.contentContainerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 296, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// packageDetailsGroupBox
			// 
			this.packageDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|f693831a-3ca4-4d29-8a1e-19cb24a57f44", "Packaging Details");
			this.packageDetailsGroupBox.Controls.Add(this.shipmentTotalsLabel);
			this.packageDetailsGroupBox.Controls.Add(this.outerPackTotalsLabel);
			this.packageDetailsGroupBox.Controls.Add(this.packLinesContol);
			this.packageDetailsGroupBox.Controls.Add(this.shipmentWeightUnitLabel);
			this.packageDetailsGroupBox.Controls.Add(this.totalWeightUnitLabel);
			this.packageDetailsGroupBox.Controls.Add(this.shipmentVolumeUnitLabel);
			this.packageDetailsGroupBox.Controls.Add(this.totalVolumeUnitLabel);
			this.packageDetailsGroupBox.Controls.Add(this.actualVolumeCalcEdit);
			this.packageDetailsGroupBox.Controls.Add(this.actualWeightCalcEdit);
			this.packageDetailsGroupBox.Controls.Add(this.totalPackageCountBoundCalcEdit);
			this.packageDetailsGroupBox.Controls.Add(this.totalPackLineVolumeCalcEdit);
			this.packageDetailsGroupBox.Controls.Add(this.totalPackLineWeightCalcEdit);
			this.packageDetailsGroupBox.Controls.Add(this.totalPackLinesPackagesCalcEdit);
			this.packageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 12, true);
			this.packageDetailsGroupBox.Name = "packageDetailsGroupBox";
			this.packageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 245, true);
			this.packageDetailsGroupBox.TabIndex = 1;
			this.packageDetailsGroupBox.TabStop = false;
			// 
			// shipmentTotalsLabel
			// 
			this.shipmentTotalsLabel.AutoSize = true;
			this.shipmentTotalsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|420e5f98-f6e7-4fa7-b75c-4b07a6052d0f", "Shipment Totals");
			this.shipmentTotalsLabel.IsFontBold = true;
			this.shipmentTotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 218, true);
			this.shipmentTotalsLabel.Name = "shipmentTotalsLabel";
			this.shipmentTotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.shipmentTotalsLabel.TabIndex = 31;
			// 
			// outerPackTotalsLabel
			// 
			this.outerPackTotalsLabel.AutoSize = true;
			this.outerPackTotalsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|f61e57ea-1724-4470-bba5-82412b2bb991", "Outer Pack Totals");
			this.outerPackTotalsLabel.IsFontBold = true;
			this.outerPackTotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 196, true);
			this.outerPackTotalsLabel.Name = "outerPackTotalsLabel";
			this.outerPackTotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.outerPackTotalsLabel.TabIndex = 30;
			// 
			// packLinesContol
			// 
			this.packLinesContol.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packLinesContol, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonShipment)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)))));
			this.packLinesContol.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.packLinesContol.Name = "packLinesContol";
			this.packLinesContol.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 168, true);
			this.packLinesContol.TabIndex = 2;
			// 
			// shipmentWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.shipmentWeightUnitLabel, "TotalPackLineWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalPackLineWeightUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.shipmentWeightUnitLabel, false);
			this.shipmentWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 218, true);
			this.shipmentWeightUnitLabel.Name = "shipmentWeightUnitLabel";
			this.shipmentWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.shipmentWeightUnitLabel.TabIndex = 28;
			// 
			// totalWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.totalWeightUnitLabel, "TotalPackLineWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalPackLineWeightUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.totalWeightUnitLabel, false);
			this.totalWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 196, true);
			this.totalWeightUnitLabel.Name = "totalWeightUnitLabel";
			this.totalWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.totalWeightUnitLabel.TabIndex = 27;
			// 
			// shipmentVolumeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.shipmentVolumeUnitLabel, "TotalPackLineVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalPackLineVolumeUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.shipmentVolumeUnitLabel, false);
			this.shipmentVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(718, 218, true);
			this.shipmentVolumeUnitLabel.Name = "shipmentVolumeUnitLabel";
			this.shipmentVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.shipmentVolumeUnitLabel.TabIndex = 26;
			// 
			// totalVolumeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.totalVolumeUnitLabel, "TotalPackLineVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalPackLineVolumeUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.totalVolumeUnitLabel, false);
			this.totalVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(718, 196, true);
			this.totalVolumeUnitLabel.Name = "totalVolumeUnitLabel";
			this.totalVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.totalVolumeUnitLabel.TabIndex = 25;
			// 
			// actualVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.actualVolumeCalcEdit, "JS_ActualVolumeReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ActualVolumeReadOnly)));
			this.actualVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|205c13f8-7626-4d07-8d1a-4dd99672e28c", "Volume", "The total volume of outer packages across this shipment.");
			this.actualVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 215, true);
			this.actualVolumeCalcEdit.Name = "actualVolumeCalcEdit";
			this.actualVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.actualVolumeCalcEdit.TabIndex = 7;
			this.actualVolumeCalcEdit.TabStop = false;
			this.actualVolumeCalcEdit.Text = "0.000";
			this.actualVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// actualWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.actualWeightCalcEdit, "JS_ActualWeightReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_ActualWeightReadOnly)));
			this.actualWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|5048ac5c-8c91-4837-a2f1-151bc46647a9", "Weight", "The total weight of outer packages across this shipment.");
			this.actualWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 215, true);
			this.actualWeightCalcEdit.Name = "actualWeightCalcEdit";
			this.actualWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.actualWeightCalcEdit.TabIndex = 5;
			this.actualWeightCalcEdit.TabStop = false;
			this.actualWeightCalcEdit.Text = "0.000";
			this.actualWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalPackageCountBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalPackageCountBoundCalcEdit, "JS_OuterPacksReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_OuterPacksReadOnly)));
			this.totalPackageCountBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|e9a2627d-da62-46b8-a705-b27aef4fe2d1", "Outer Packages", "The total number of outer packages across this shipment.");
			this.totalPackageCountBoundCalcEdit.DecimalPlaces = 0;
			this.totalPackageCountBoundCalcEdit.Decimals = 0;
			this.totalPackageCountBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 215, true);
			this.totalPackageCountBoundCalcEdit.Name = "totalPackageCountBoundCalcEdit";
			this.totalPackageCountBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.totalPackageCountBoundCalcEdit.TabIndex = 2;
			this.totalPackageCountBoundCalcEdit.TabStop = false;
			this.totalPackageCountBoundCalcEdit.Text = "0";
			this.totalPackageCountBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalPackLineVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalPackLineVolumeCalcEdit, "TotalOuterPacksVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalOuterPacksVolume)));
			this.totalPackLineVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|aa000988-0595-429c-8f68-c7bd7db3b3d9", "Volume", "The total volume of outer packages.");
			this.totalPackLineVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 193, true);
			this.totalPackLineVolumeCalcEdit.Name = "totalPackLineVolumeCalcEdit";
			this.totalPackLineVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.totalPackLineVolumeCalcEdit.TabIndex = 6;
			this.totalPackLineVolumeCalcEdit.TabStop = false;
			this.totalPackLineVolumeCalcEdit.Text = "0.000";
			this.totalPackLineVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalPackLineWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalPackLineWeightCalcEdit, "TotalOuterPacksWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalOuterPacksWeight)));
			this.totalPackLineWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|6186d5f2-6e76-4069-bc7d-92b34db4b84b", "Weight", "The total weight of outer packages.");
			this.totalPackLineWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 193, true);
			this.totalPackLineWeightCalcEdit.Name = "totalPackLineWeightCalcEdit";
			this.totalPackLineWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.totalPackLineWeightCalcEdit.TabIndex = 3;
			this.totalPackLineWeightCalcEdit.TabStop = false;
			this.totalPackLineWeightCalcEdit.Text = "0.000";
			this.totalPackLineWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalPackLinesPackagesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalPackLinesPackagesCalcEdit, "TotalOuterPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).TotalOuterPacks)));
			this.totalPackLinesPackagesCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|08eef136-bdbc-4cc3-9e1c-a7020c4a2dc9", "Outer Packages", "The total number of outer packages.");
			this.totalPackLinesPackagesCalcEdit.DecimalPlaces = 0;
			this.totalPackLinesPackagesCalcEdit.Decimals = 0;
			this.totalPackLinesPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 193, true);
			this.totalPackLinesPackagesCalcEdit.Name = "totalPackLinesPackagesCalcEdit";
			this.totalPackLinesPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.totalPackLinesPackagesCalcEdit.TabIndex = 1;
			this.totalPackLinesPackagesCalcEdit.TabStop = false;
			this.totalPackLinesPackagesCalcEdit.Text = "0";
			this.totalPackLinesPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|cd1cdcef-cfa8-4a74-81c3-e98ce1af2a53", "OK");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(695, 266, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 0;
			// 
			// matchShipmentTotalCheckBox
			// 
			this.matchShipmentTotalCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.matchShipmentTotalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.matchShipmentTotalCheckBox, "MatchShipmentTotalsOnPackLinesDetailForm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).MatchShipmentTotalsOnPackLinesDetailForm)));
			this.matchShipmentTotalCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPackingDetailForm|386f3728-891e-4cfc-97fb-8db99eee96d1", "Match shipment with pack total");
			this.matchShipmentTotalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.matchShipmentTotalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 275, true);
			this.matchShipmentTotalCheckBox.Name = "matchShipmentTotalCheckBox";
			this.matchShipmentTotalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.matchShipmentTotalCheckBox.TabIndex = 8;
			this.matchShipmentTotalCheckBox.UseVisualStyleBackColor = true;
			// 
			// contentContainerPanel
			// 
			this.contentContainerPanel.Controls.Add(this.matchShipmentTotalCheckBox);
			this.contentContainerPanel.Controls.Add(this.packageDetailsGroupBox);
			this.contentContainerPanel.Controls.Add(this.okButton);
			this.contentContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.contentContainerPanel.Name = "contentContainerPanel";
			this.contentContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 296, true);
			this.contentContainerPanel.TabIndex = 14;
			// 
			// ShipmentPackingDetailForm
			// 
			this.AcceptButton = this.okButton;
			this.CancelButton = this.okButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 320, true);
			this.Controls.Add(this.contentContainerPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 348, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 348, true);
			this.Name = "ShipmentPackingDetailForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.contentContainerPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.packageDetailsGroupBox.ResumeLayout(false);
			this.packageDetailsGroupBox.PerformLayout();
			this.contentContainerPanel.ResumeLayout(false);
			this.contentContainerPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox packageDetailsGroupBox;
		private Enterprise.ZArchitecture.ZLabel shipmentWeightUnitLabel;
		private Enterprise.ZArchitecture.ZLabel totalWeightUnitLabel;
		private Enterprise.ZArchitecture.ZLabel shipmentVolumeUnitLabel;
		private Enterprise.ZArchitecture.ZLabel totalVolumeUnitLabel;
		private Enterprise.ZArchitecture.ZCalcEdit actualVolumeCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit actualWeightCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit totalPackageCountBoundCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit totalPackLineVolumeCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit totalPackLineWeightCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit totalPackLinesPackagesCalcEdit;
		protected Enterprise.Freight.Forwarding.GUI.PackLinesContol packLinesContol;
		private Enterprise.ZArchitecture.ZLabel shipmentTotalsLabel;
		private Enterprise.ZArchitecture.ZLabel outerPackTotalsLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel contentContainerPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton okButton;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox matchShipmentTotalCheckBox;
	}
}
