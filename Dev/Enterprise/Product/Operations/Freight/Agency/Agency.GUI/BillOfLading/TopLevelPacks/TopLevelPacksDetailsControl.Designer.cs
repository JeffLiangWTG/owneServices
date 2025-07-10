namespace Enterprise.Freight.Agency.GUI
{
	partial class TopLevelPacksDetailsControl
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
			this.GoodsValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.CommodityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DimensionsUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StowagePositionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tariffTextBoxPlaceholder = new CargoWise.Windows.UI.KPanel();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.harmonisedCodeFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipmentContainersView);
			// 
			// GoodsValueCalcFindBox
			// 
			this.GoodsValueCalcFindBox.AllowDrop = true;
			this.GoodsValueCalcFindBox.BindToAmount = "JC_GoodsValue";
			this.GoodsValueCalcFindBox.BindToUnit = "JC_RX_NKGoodsCurrency";
			this.GoodsValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 143, true);
			this.GoodsValueCalcFindBox.Name = "GoodsValueCalcFindBox";
			this.GoodsValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.GoodsValueCalcFindBox.TabIndex = 5;
			// 
			// CommodityCodeFindBox
			// 
			this.CommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeFindBox, "JC_RH_NKContainerCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_RH_NKContainerCommodityCode)));
			this.CommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 33, true);
			this.CommodityCodeFindBox.Name = "CommodityCodeFindBox";
			this.CommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.CommodityCodeFindBox.TabIndex = 2;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "JC_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_Description)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 89, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 48, true);
			this.DescriptionTextBox.TabIndex = 4;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_GrossWeightUQ)));
			this.WeightCalcDropEdit.BindToAmount = "JC_GrossWeight";
			this.WeightCalcDropEdit.BindToUnit = "JC_GrossWeightUQ";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 169, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.WeightCalcDropEdit.TabIndex = 6;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_GrossVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_GrossVolumeUQ)));
			this.VolumeCalcDropEdit.BindToAmount = "JC_GrossVolume";
			this.VolumeCalcDropEdit.BindToUnit = "JC_GrossVolumeUQ";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 169, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 7;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// LengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LengthCalcEdit, "JC_TotalLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_TotalLength)));
			this.LengthCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("77b1c65d-462b-41d1-83fd-c7bc7e586f36", "Length");
			this.LengthCalcEdit.DecimalPlaces = 2;
			this.LengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 195, true);
			this.LengthCalcEdit.Name = "LengthCalcEdit";
			this.LengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.LengthCalcEdit.TabIndex = 8;
			this.LengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WidthCalcEdit, "JC_TotalWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_TotalWidth)));
			this.WidthCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("a86c0fac-2206-411f-91f1-fcd280412a23", "Width");
			this.WidthCalcEdit.DecimalPlaces = 2;
			this.WidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 195, true);
			this.WidthCalcEdit.Name = "WidthCalcEdit";
			this.WidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.WidthCalcEdit.TabIndex = 9;
			this.WidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// HeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.HeightCalcEdit, "JC_TotalHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_TotalHeight)));
			this.HeightCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("6d5a99d8-fe08-4448-a5f0-08cb8bcb6f2a", "Height");
			this.HeightCalcEdit.DecimalPlaces = 2;
			this.HeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 195, true);
			this.HeightCalcEdit.Name = "HeightCalcEdit";
			this.HeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.HeightCalcEdit.TabIndex = 10;
			this.HeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DimensionsUnitDropEdit
			// 
			this.DimensionsUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DimensionsUnitDropEdit, "JC_TotalUnitOfMeasure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_TotalUnitOfMeasure)));
			this.DimensionsUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 221, true);
			this.DimensionsUnitDropEdit.Name = "DimensionsUnitDropEdit";
			this.DimensionsUnitDropEdit.PreBoundMaxLength = 2;
			this.DimensionsUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DimensionsUnitDropEdit.TabIndex = 11;
			// 
			// StowagePositionTextBox
			// 
			this.BindingSource.SetBindingMember(this.StowagePositionTextBox, "JC_StowagePosition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_StowagePosition)));
			this.StowagePositionTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("57e29e4b-212f-443c-ab81-eb15cc26bd5c", "Stowage Position", "Stowage Position format should be DDBBBRRTT where DD is Deck, BBB is Bay, RR is Row and TT is Tier.");
			this.StowagePositionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 7, true);
			this.StowagePositionTextBox.Name = "StowagePositionTextBox";
			this.StowagePositionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.StowagePositionTextBox.TabIndex = 1;
			// 
			// tariffTextBoxPlaceholder
			// 
			this.tariffTextBoxPlaceholder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 59, true);
			this.tariffTextBoxPlaceholder.Name = "tariffTextBoxPlaceholder";
			this.tariffTextBoxPlaceholder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 24, true);
			this.tariffTextBoxPlaceholder.TabIndex = 3;
			this.tariffTextBoxPlaceholder.Controls.Add(harmonisedCodeFindBox);
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "JC_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_ContainerNum)));
			this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("66645116-5a33-4614-bf64-fddcc837d3db", "Reference Number");
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 7, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 0;
			// 
			// harmonisedCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.harmonisedCodeFindBox, "JC_HarmonisedCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(null)).JC_HarmonisedCode)));
			this.harmonisedCodeFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("b0606c23-9ec8-43f7-ae98-af7439a01bd7", "Harmonized Code");
			this.harmonisedCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 2, true);
			this.harmonisedCodeFindBox.Name = "HarmonisedCodeFindBox";
			this.harmonisedCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(tariffTextBoxPlaceholder.Size.Width), 20);
			this.harmonisedCodeFindBox.TabIndex = 3;

			// 
			// TopLevelPacksDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.tariffTextBoxPlaceholder);
			this.Controls.Add(this.StowagePositionTextBox);
			this.Controls.Add(this.DimensionsUnitDropEdit);
			this.Controls.Add(this.HeightCalcEdit);
			this.Controls.Add(this.WidthCalcEdit);
			this.Controls.Add(this.LengthCalcEdit);
			this.Controls.Add(this.VolumeCalcDropEdit);
			this.Controls.Add(this.WeightCalcDropEdit);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.CommodityCodeFindBox);
			this.Controls.Add(this.GoodsValueCalcFindBox);
			this.Name = "TopLevelPacksDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 255, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCalcFindBox GoodsValueCalcFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CommodityCodeFindBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		private ZArchitecture.ZCalcEdit LengthCalcEdit;
		private ZArchitecture.ZCalcEdit WidthCalcEdit;
		private ZArchitecture.ZCalcEdit HeightCalcEdit;
		private ZArchitecture.GUI.ZDropEdit DimensionsUnitDropEdit;
		private ZArchitecture.ZTextBox StowagePositionTextBox;
		private CargoWise.Windows.UI.KPanel tariffTextBoxPlaceholder;
		private ZArchitecture.ZTextBox ReferenceNumberTextBox;
		Enterprise.Customs.Universal.GUI.TariffFindBox harmonisedCodeFindBox;
	}
}
