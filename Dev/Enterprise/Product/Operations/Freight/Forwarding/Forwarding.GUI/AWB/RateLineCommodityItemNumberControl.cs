using System;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class RateLineCommodityItemNumberControl : ZUserControl
	{
		public RateLineCommodityItemNumberControl()
		{
			InitializeComponent();
		}

		protected ExportAWBRateLine RateLine
		{
			get { return (ExportAWBRateLine)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (RateLine != null && !RateLine.IsDeleted)
			{
				RateLine.ER_RateClassInfo.ValueChanged -= ER_RateClassValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (RateLine != null && !RateLine.IsDeleted)
			{
				RateLine.ER_RateClassInfo.ValueChanged += ER_RateClassValueChanged;
				SetControlsVisibility();
			}
		}

		void ER_RateClassValueChanged(object sender, EventArgs e)
		{
			SetControlsVisibility();
		}

		void SetControlsVisibility()
		{
			var consolRateLine = RateLine as ConsolExportAWBRateLine;

			CommodityItemNumberFindBox.Visible = consolRateLine != null && (consolRateLine.ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceBasicCharge || consolRateLine.ER_RateClass == Core.Constants.AWB.RateClass.SpecificCommodityRate);
			CommodityItemNumberDropEdit.Visible = consolRateLine != null && consolRateLine.ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation;
			CommodityNumberItemTextBox.Visible = !CommodityItemNumberDropEdit.Visible && !CommodityItemNumberFindBox.Visible;
		}
	}
}
