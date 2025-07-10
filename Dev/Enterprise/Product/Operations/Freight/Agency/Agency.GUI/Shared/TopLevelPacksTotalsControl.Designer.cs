namespace Enterprise.Freight.Agency.GUI
{
	partial class TopLevelPacksTotalsControl
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
			this.TotalsPanel = new CargoWise.Windows.UI.KPanel();
			this.TopLevelPacksTotalPacks = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentTotalPacks = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TopLevelPacksTotalWeight = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentTotalWeight = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TopLevelPacksTotalVolume = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentTotalVolume = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TotalsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipment);
			// 
			// TotalsPanel
			// 
			this.TotalsPanel.Controls.Add(this.TopLevelPacksTotalPacks);
			this.TotalsPanel.Controls.Add(this.ShipmentTotalPacks);
			this.TotalsPanel.Controls.Add(this.TopLevelPacksTotalWeight);
			this.TotalsPanel.Controls.Add(this.ShipmentTotalWeight);
			this.TotalsPanel.Controls.Add(this.TopLevelPacksTotalVolume);
			this.TotalsPanel.Controls.Add(this.ShipmentTotalVolume);
			this.TotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TotalsPanel.Name = "TotalsPanel";
			this.TotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 56, true);
			this.TotalsPanel.TabIndex = 2;
			// 
			// TopLevelPacksTotalPacks
			// 
			this.TopLevelPacksTotalPacks.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TopLevelPacksTotalPacks, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TopLevelPacksTotalPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacksUnit)));
			this.TopLevelPacksTotalPacks.BindToAmount = "TopLevelPacksTotalPacks";
			this.TopLevelPacksTotalPacks.BindToUnit = "TotalOuterPacksUnit";
			this.TopLevelPacksTotalPacks.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("8e1a4b43-55cd-4a53-9c13-9ebdccb19a52", "Packs", "Packline Total Packs", "");
			this.TopLevelPacksTotalPacks.Decimals = 2;
			this.TopLevelPacksTotalPacks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 8, true);
			this.TopLevelPacksTotalPacks.Name = "TopLevelPacksTotalPacks";
			this.TopLevelPacksTotalPacks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TopLevelPacksTotalPacks.TabIndex = 0;
			this.TopLevelPacksTotalPacks.UnitPreBoundMaxLength = 3;
			// 
			// ShipmentTotalPacks
			// 
			this.ShipmentTotalPacks.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalPacks, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_OuterPacksReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacksUnit)));
			this.ShipmentTotalPacks.BindToAmount = "JS_OuterPacksReadOnly";
			this.ShipmentTotalPacks.BindToUnit = "TotalOuterPacksUnit";
			this.ShipmentTotalPacks.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ffd37b10-e20f-4665-916f-e65d7430abc0", "Packs", "Shipment Total Packs", "");
			this.ShipmentTotalPacks.Decimals = 2;
			this.ShipmentTotalPacks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 32, true);
			this.ShipmentTotalPacks.Name = "ShipmentTotalPacks";
			this.ShipmentTotalPacks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentTotalPacks.TabIndex = 1;
			this.ShipmentTotalPacks.UnitPreBoundMaxLength = 3;
			// 
			// TopLevelPacksTotalWeight
			// 
			this.TopLevelPacksTotalWeight.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TopLevelPacksTotalWeight, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TopLevelPacksTotalWeightInShipmentWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineWeightUnit)));
			this.TopLevelPacksTotalWeight.BindToAmount = "TopLevelPacksTotalWeightInShipmentWeightUnit";
			this.TopLevelPacksTotalWeight.BindToUnit = "TotalPackLineWeightUnit";
			this.TopLevelPacksTotalWeight.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("9ec5aff5-e9b5-4744-bce7-625a0b198c5b", "Weight");
			this.TopLevelPacksTotalWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 8, true);
			this.TopLevelPacksTotalWeight.Name = "TopLevelPacksTotalWeight";
			this.TopLevelPacksTotalWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TopLevelPacksTotalWeight.TabIndex = 2;
			this.TopLevelPacksTotalWeight.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentTotalWeight
			// 
			this.ShipmentTotalWeight.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalWeight, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_ActualWeightReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineWeightUnit)));
			this.ShipmentTotalWeight.BindToAmount = "JS_ActualWeightReadOnly";
			this.ShipmentTotalWeight.BindToUnit = "TotalPackLineWeightUnit";
			this.ShipmentTotalWeight.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("679aa0d3-a244-4399-9e02-8d9ea981b7d3", "Weight");
			this.ShipmentTotalWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 32, true);
			this.ShipmentTotalWeight.Name = "ShipmentTotalWeight";
			this.ShipmentTotalWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmentTotalWeight.TabIndex = 3;
			this.ShipmentTotalWeight.UnitPreBoundMaxLength = 2;
			// 
			// TopLevelPacksTotalVolume
			// 
			this.TopLevelPacksTotalVolume.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TopLevelPacksTotalVolume, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TopLevelPacksTotalVolumeInShipmentVolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineVolumeUnit)));
			this.TopLevelPacksTotalVolume.BindToAmount = "TopLevelPacksTotalVolumeInShipmentVolumeUnit";
			this.TopLevelPacksTotalVolume.BindToUnit = "TotalPackLineVolumeUnit";
			this.TopLevelPacksTotalVolume.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("45aee643-60e0-42b2-aa72-16b7c27f3541", "Volume");
			this.TopLevelPacksTotalVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 8, true);
			this.TopLevelPacksTotalVolume.Name = "TopLevelPacksTotalVolume";
			this.TopLevelPacksTotalVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TopLevelPacksTotalVolume.TabIndex = 4;
			this.TopLevelPacksTotalVolume.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentTotalVolume
			// 
			this.ShipmentTotalVolume.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalVolume, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_ActualVolumeReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineVolumeUnit)));
			this.ShipmentTotalVolume.BindToAmount = "JS_ActualVolumeReadOnly";
			this.ShipmentTotalVolume.BindToUnit = "TotalPackLineVolumeUnit";
			this.ShipmentTotalVolume.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("713c31e3-9134-4bcd-8d98-6a22118d41fd", "Volume");
			this.ShipmentTotalVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 32, true);
			this.ShipmentTotalVolume.Name = "ShipmentTotalVolume";
			this.ShipmentTotalVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmentTotalVolume.TabIndex = 5;
			this.ShipmentTotalVolume.UnitPreBoundMaxLength = 2;
			// 
			// TopLevelPacksTotalsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TotalsPanel);
			this.Name = "TopLevelPacksTotalsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TotalsPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KPanel TotalsPanel;
		private ZArchitecture.GUI.ZCalcDropEdit TopLevelPacksTotalPacks;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentTotalPacks;
		private ZArchitecture.GUI.ZCalcDropEdit TopLevelPacksTotalWeight;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentTotalWeight;
		private ZArchitecture.GUI.ZCalcDropEdit TopLevelPacksTotalVolume;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentTotalVolume;
	}
}
