using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class HAWBUserControl : AWBUserControl
	{
		protected ZArchitecture.ZGrid OtherChargesGrid;
		protected ZArchitecture.ZLabel EH_ManifestDescriptionOfGoodsLabel;
		protected ZArchitecture.ZTextBox EH_ManifestDescriptionOfGoodsTextBox;
		ZArchitecture.ZTextBox EH_HouseDeclaredValueCurrencyTextBox;
		ZArchitecture.ZTextBox EH_HouseCustomsValueCurrencyTextBox;
		ZArchitecture.ZTextBox EH_HouseInsuranceValueCurrencyTextBox;

		private void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.OtherChargesGrid = new ZArchitecture.ZGrid();
			this.EH_ManifestDescriptionOfGoodsLabel = new ZArchitecture.ZLabel();
			this.EH_HouseDeclaredValueCurrencyTextBox = new ZArchitecture.ZTextBox();
			this.EH_HouseCustomsValueCurrencyTextBox = new ZArchitecture.ZTextBox();
			this.EH_HouseInsuranceValueCurrencyTextBox = new ZArchitecture.ZTextBox();
			this.EH_ManifestDescriptionOfGoodsTextBox = new ZArchitecture.ZTextBox();
			this.MainPanel.SuspendLayout();
			this.AWBBottomPanel.SuspendLayout();
			this.AWBPanel.SuspendLayout();
			this.AWBTopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OtherChargesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// AWBBottomPanel
			// 
			this.AWBBottomPanel.Controls.Add(this.OtherChargesGrid);
			this.AWBBottomPanel.Controls.SetChildIndex(this.AWBPanel, 0);
			this.AWBBottomPanel.Controls.SetChildIndex(this.OtherChargesGrid, 0);
			// 
			// AWBPanel
			// 
			this.AWBPanel.Controls.Add(this.EH_HouseInsuranceValueCurrencyTextBox);
			this.AWBPanel.Controls.Add(this.EH_HouseCustomsValueCurrencyTextBox);
			this.AWBPanel.Controls.Add(this.EH_HouseDeclaredValueCurrencyTextBox);
			this.AWBPanel.Controls.Add(this.EH_ManifestDescriptionOfGoodsTextBox);
			this.AWBPanel.Controls.Add(this.EH_ManifestDescriptionOfGoodsLabel);
			this.AWBPanel.Controls.SetChildIndex(this.EH_HouseDeclaredValueCurrencyTextBox, 0);
			this.AWBPanel.Controls.SetChildIndex(this.EH_HouseCustomsValueCurrencyTextBox, 0);
			this.AWBPanel.Controls.SetChildIndex(this.EH_HouseInsuranceValueCurrencyTextBox, 0);
			this.AWBPanel.Controls.SetChildIndex(this.EH_ManifestDescriptionOfGoodsTextBox, 0);
			this.AWBPanel.Controls.SetChildIndex(this.EH_ManifestDescriptionOfGoodsLabel, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ForwardingShipment);
			// 
			// OtherChargesGrid
			// 
			this.OtherChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherChargesGrid, "AWBHeaderManager.AWBOtherCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.OtherChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EO_ChargeCode";
			zDropEditColumnStyleInfo2.ColumnName = "EO_EntitlementCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "EO_PPDCLT";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "EO_ChargeDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo2.ColumnName = "Currency";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "EO_Amount";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OtherChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OtherChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OtherChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OtherChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OtherChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OtherChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OtherChargesGrid.GridId = "9362d71e-929b-40df-8a17-1a37d4e44008";
			this.OtherChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherChargesGrid.LayoutKey = "OtherChargesGrid";
			this.OtherChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 1008, true);
			this.OtherChargesGrid.Name = "OtherChargesGrid";
			this.OtherChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 108, true);
			this.OtherChargesGrid.TabIndex = 1006;
			// 
			// EH_ManifestDescriptionOfGoodsLabel
			// 
			this.EH_ManifestDescriptionOfGoodsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|811d94ad-09e4-4dfe-b909-5c067908ff93", "Short Description Override (FHL)");
			this.EH_ManifestDescriptionOfGoodsLabel.IsFontBold = true;
			this.EH_ManifestDescriptionOfGoodsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 956, true);
			this.EH_ManifestDescriptionOfGoodsLabel.Name = "EH_ManifestDescriptionOfGoodsLabel";
			this.EH_ManifestDescriptionOfGoodsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 30, true);
			this.EH_ManifestDescriptionOfGoodsLabel.TabIndex = 195;
			// 
			// EH_HouseDeclaredValueCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_HouseDeclaredValueCurrencyTextBox, "AWBHeaderManager.EH_HouseDeclaredValueCurrency");
			this.EH_HouseDeclaredValueCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 488, true);
			this.EH_HouseDeclaredValueCurrencyTextBox.Name = "EH_HouseDeclaredValueCurrencyTextBox";
			this.EH_HouseDeclaredValueCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.EH_HouseDeclaredValueCurrencyTextBox.TabIndex = 58;
			// 
			// EH_HouseCustomsValueCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_HouseCustomsValueCurrencyTextBox, "AWBHeaderManager.EH_HouseCustomsValueCurrency");
			this.EH_HouseCustomsValueCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(918, 488, true);
			this.EH_HouseCustomsValueCurrencyTextBox.Name = "EH_HouseCustomsValueCurrencyTextBox";
			this.EH_HouseCustomsValueCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.EH_HouseCustomsValueCurrencyTextBox.TabIndex = 60;
			// 
			// EH_HouseInsuranceValueCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_HouseInsuranceValueCurrencyTextBox, "AWBHeaderManager.EH_HouseInsuranceValueCurrency");
			this.EH_HouseInsuranceValueCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 532, true);
			this.EH_HouseInsuranceValueCurrencyTextBox.Name = "EH_HouseInsuranceValueCurrencyTextBox";
			this.EH_HouseInsuranceValueCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.EH_HouseInsuranceValueCurrencyTextBox.TabIndex = 74;
			// 
			// EH_ManifestDescriptionOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ManifestDescriptionOfGoodsTextBox, "AWBHeaderManager.EH_ManifestDescriptionOfGoods");
			this.EH_ManifestDescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(866, 960, true);
			this.EH_ManifestDescriptionOfGoodsTextBox.Name = "EH_ManifestDescriptionOfGoodsTextBox";
			this.EH_ManifestDescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.EH_ManifestDescriptionOfGoodsTextBox.TabIndex = 197;
			// 
			// HAWBUserControl
			// 
			this.Name = "HAWBUserControl";
			this.MainPanel.ResumeLayout(false);
			this.AWBBottomPanel.ResumeLayout(false);
			this.AWBPanel.ResumeLayout(false);
			this.AWBPanel.PerformLayout();
			this.AWBTopPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OtherChargesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
