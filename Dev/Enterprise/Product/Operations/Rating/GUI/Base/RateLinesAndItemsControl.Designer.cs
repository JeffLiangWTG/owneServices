using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateLinesAndItemsControl
	{
		void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo startDateInfo = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo endDateInfo = new ZDateEditColumnStyleInfo();

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
			// zPanel1
			// 
			this.zPanel1.TabIndex = 0;
			// 
			// RateLinesGrid
			// 
			this.BindingSource.SetBindingMember(this.RateLinesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_AC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_RateDesc);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_RX_NKCurrency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_WeightVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_Rounding);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).RoundingFactor);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).RoundingFactor);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_ActualPercentage);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_RateStartDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_RateEndDate);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TL_AC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "TL_RateDesc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo2.ColumnName = "TL_RateDescLocal";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TL_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.ColumnName = "TL_WeightVolume";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RoundingFactor";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.GroupName = Res.GetData("e8ea60c2-bff0-4b8a-824e-fef84827e583", "Rounding");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("b984f789-25ec-4d14-a9a1-9805c0d85f17", "Rnd. Fact.", "Rounding Factor", "Steps of rounding");
			zDropEditColumnStyleInfo3.ColumnName = "TL_Rounding";
			zDropEditColumnStyleInfo3.GroupName = Res.GetData("e8ea60c2-bff0-4b8a-824e-fef84827e583", "Rounding");
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TL_ActualPercentage";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			startDateInfo.ColumnName = "TL_RateStartDate";
			startDateInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			startDateInfo.CharacterCasing = CharacterCasing.Upper;
			startDateInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			startDateInfo.IsVisible = false;
			endDateInfo.ColumnName = "TL_RateEndDate";
			endDateInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			endDateInfo.CharacterCasing = CharacterCasing.Upper;
			endDateInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			endDateInfo.IsVisible = false;
			this.RateLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);

			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			this.RateLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RateLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RateLinesGrid.ColumnStyles.Add(startDateInfo);
			this.RateLinesGrid.ColumnStyles.Add(endDateInfo);
			this.RateLinesGrid.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RateLine);
			// 
			// RateLinesAndItemsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RateLinesAndItemsControl";
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
