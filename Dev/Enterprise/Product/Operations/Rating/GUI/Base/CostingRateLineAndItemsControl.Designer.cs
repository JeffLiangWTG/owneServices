using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CostingRateLineAndItemsControl
	{
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.zPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RateLinesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			// 
			// RateLinesGrid
			// 
			this.BindingSource.SetBindingMember(this.RateLinesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).TL_AC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).ChargeCodeDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).RateType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Mode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Supplier);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Carrier);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Origin);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Destination);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Via);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).ServiceLevel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).CarrierServiceLevel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).CommodityCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).ContainerCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).TransitTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).Frequency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).FrequencyUnit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).StartDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).EndDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).TL_ContainerOwnership);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).TL_Condition);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).TL_ConditionalExpression);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).ContractNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).ConversionFactorForBinding.ConversionFactorString);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RelatedRateLine)(null)).FMCTariffID);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TL_AC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|c33a64ab-f1f7-4521-a1ae-6974f532b495", "Charge Code Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|e1924c94-a2cd-4de6-ad45-498aee9cd4d0", "Type");
			zTextBoxColumnStyleInfo2.ColumnName = "RateType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|2b1887a1-c935-42b5-951d-917818a7ddc7", "Mode");
			zTextBoxColumnStyleInfo3.ColumnName = "Mode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|7f9864fd-a35f-4d9d-8421-a53ef3f7ac57", "Svc. Prov.", "Service Provider", "");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Supplier";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|0575215d-1216-49c0-803b-af441cbd06c4", "Carrier", "Carrier", "Carrier", "");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Carrier";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|3f83101e-94c2-4857-96ee-9c3f01f23d7b", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Origin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|a09f570b-cfb4-4bbc-ab30-fcbce0067a7f", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Destination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|4407ae16-370b-40ab-a5d3-530423559f56", "Via");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "Via";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|651e47b1-df7c-4e17-b8df-646d47d91420", "Srv. Lvl.", "Service Level", "");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "ServiceLevel";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|67e58398-265f-458f-a7fc-32ba63f6c9c4", "Car Srv. Lvl.", "Carrier Service Level", "");
			zDropEditColumnStyleInfo1.ColumnName = "CarrierServiceLevel";
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|bf7e12b7-a1c0-46bc-925d-b57659c970c8", "Comm.", "Commodity", "");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "CommodityCode";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|2630d49a-41ec-4948-9fd8-cd3d01cdb9e3", "Cont.", "Container", "");
			zTextBoxColumnStyleInfo4.ColumnName = "ContainerCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|a9d77f70-2834-48af-9208-95c4b10388f2", "T/Time", "Transit Time", "");
			zTextBoxColumnStyleInfo5.ColumnName = "TransitTime";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|8688cdde-ee35-435b-9bcd-57b5f15dfbad", "Freq.", "Frequency", "");
			zCalcEditColumnStyleInfo1.ColumnName = "Frequency";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|f9ecd7f6-53c1-4454-ab02-e803c859d5e6", "Freq. Unit", "Frequency Unit", "");
			zTextBoxColumnStyleInfo6.ColumnName = "FrequencyUnit";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|75f3ca12-2d70-4cf5-acc2-eb4812d6a7ca", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|6729a867-9137-4dcc-b26a-da82936d22f8", "Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "TL_ContainerOwnership";
			zTextBoxColumnStyleInfo8.ColumnName = "TL_Condition";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BaseRateLinesAndItemsControl|c6d0f4bf-8511-4d68-a7b2-3a8ac3de2f2a", "Expression");
			zTextBoxColumnStyleInfo9.ColumnName = "TL_ConditionalExpression";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|e1c6f813-2d7b-4690-b27c-46da84bae504", "Contract No.", "Carrier Contract Number", "");
			zTextBoxColumnStyleInfo10.ColumnName = "ContractNumber";
			zTextBoxColumnStyleInfo11.ColumnName = "ConversionFactorForBinding.ConversionFactorString";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostingRateLineAndItemsControl|e665349b-b98c-4747-a2ee-96036ebb0f69", "FMC TID", "FMC Tariff ID", "");
			zTextBoxColumnStyleInfo12.ColumnName = "FMCTariffID";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.RateLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RateLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.RateLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RateLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RateLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.RateLinesGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(this.RateLinesGrid_ColourDeciding);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RelatedRateLine);
			// 
			// CostingRateLineAndItemsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "CostingRateLineAndItemsControl";
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RateLinesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
