using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module.ReferenceFiles.TransitTimeServiceLevelCombination;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	partial class TransitTimeServiceLevelCombinationFilterControl : ZFilterStripControl
	{
		public TransitTimeServiceLevelCombinationFilterControl(IBusinessObjectCollection gridCollection, TransitTimeServiceLevelCombinationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new TransitTimeServiceLevelCombinationFilterStrip();

		void SetupServiceLevelColumns()
		{
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.ColumnName = "TSC_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2.ColumnName = "ServiceLevelDescription";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("8c1c4f1b-7713-4467-b897-654074f743bc", "Description");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		}

		public override Type DataSourceType => typeof(Business.TransitTimeServiceLevelCombinationView);
	}
}
