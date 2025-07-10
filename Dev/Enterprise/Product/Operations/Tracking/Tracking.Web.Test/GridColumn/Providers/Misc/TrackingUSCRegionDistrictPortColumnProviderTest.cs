using System.Web.UI.WebControls;
using Enterprise.Customs.Universal;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingUSCRegionDistrictPortColumnProvider))]
	sealed class TrackingUSCRegionDistrictPortColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingUSCRegionDistrictPorts.Name],
				TestProvider[WebTracker.Grids.TrackingUSCRegionDistrictPorts.Code]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZButtonColumn("Code", ZZRefCusCodeListCombined.Schema.ZZD_Code)
			{
				ColumnKey = WebTracker.Grids.TrackingUSCRegionDistrictPorts.Code,
				CommandName = "Select"
			});
			AddDefaultsColumn(new ZTextEditColumn("Name", ZZRefCusCodeListCombined.Schema.ZZD_Description) { ColumnKey = WebTracker.Grids.TrackingUSCRegionDistrictPorts.Name });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingUSCRegionDistrictPortColumnProvider();
		}

		#endregion
	}
}
