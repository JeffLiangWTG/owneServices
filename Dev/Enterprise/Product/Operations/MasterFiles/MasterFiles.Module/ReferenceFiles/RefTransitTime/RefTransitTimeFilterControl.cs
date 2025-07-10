using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	partial class RefTransitTimeFilterControl : ZFilterStripControl
	{
		public RefTransitTimeFilterControl(IBusinessObjectCollection gridCollection, RefTransitTimeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new RefTransitTimeFilterStrip();

		public override Type DataSourceType => typeof(Business.RefTransitTime);

		void SetupServiceLevelColumns()
		{
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.ColumnName = "RTT_RS_NKServiceLevel";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		}
	}
}
