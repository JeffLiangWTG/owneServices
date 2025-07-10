using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Customs.SG.Access.GUI
{
	public partial class ManifestBillFilterStripControl : ASYCUDA.Module.ASYCUDAManifestBillFilterStripControl
	{
		protected ManifestBillFilterStripControl() { }
		public ManifestBillFilterStripControl(IBusinessObjectCollection gridCollection, ManifestBillFilterStrip billFilterStripBusinessObject)
			: base(gridCollection, billFilterStripBusinessObject)
		{
			InitializeComponent();
			AddSGCustomizedColumns();
			RemoveGridColumns();
		}

		protected void RemoveGridColumns()
		{
			grid.SetAvailability(false, AsycudaBill.Schema.CountryCode);
		}

		void AddSGCustomizedColumns()
		{
			var billCycleDateDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			billCycleDateDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("C9B52C30-71A0-45F6-BD6A-A9F6826F3538", "Bill Cycle Date");
			billCycleDateDateEditColumnStyleInfo.ColumnName = "CycleDate";
			billCycleDateDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);

			var billCycleNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			billCycleNumberTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("0DCD7951-0DB6-4638-922A-8B11B4DC8578", "Bill Cycle Number");
			billCycleNumberTextBoxColumnStyleInfo.ColumnName = "CycleNumber";
			billCycleNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			var billBatchDateDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			billBatchDateDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("B011D489-0A95-42A9-8622-9C2AC98CB41B", "Bill Batch Date");
			billBatchDateDateEditColumnStyleInfo.ColumnName = "BatchDate";
			billBatchDateDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);

			var billBatchNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			billBatchNumberTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("AB187C28-EDD2-4E79-8C06-AD948C07D2D7", "Bill Batch Number");
			billBatchNumberTextBoxColumnStyleInfo.ColumnName = "BatchNumber";
			billBatchNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			var statusDescriptionTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			statusDescriptionTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("430CDF54-AE22-4D85-AD22-4ED6F3FC84A2", "Message Status Description");
			statusDescriptionTextBoxColumnStyleInfo.ColumnName = "StatusDescription";
			statusDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.grid.ColumnStyles.AddRange(new[]
			{
				billCycleDateDateEditColumnStyleInfo,
				billCycleNumberTextBoxColumnStyleInfo,
				billBatchDateDateEditColumnStyleInfo,
				billBatchNumberTextBoxColumnStyleInfo,
				statusDescriptionTextBoxColumnStyleInfo
			});
		}
	}
}
