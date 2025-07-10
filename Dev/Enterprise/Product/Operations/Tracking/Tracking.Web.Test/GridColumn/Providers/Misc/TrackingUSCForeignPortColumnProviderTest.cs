using System.Web.UI.WebControls;
using Enterprise.Customs.Universal;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingUSCForeignPortColumnProvider))]
	sealed class TrackingUSCForeignPortColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingUSCForeignPorts.Name],
				TestProvider[WebTracker.Grids.TrackingUSCForeignPorts.Code]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZButtonColumn("Code", ZZRefCusCodeListCombined.Schema.ZZD_Code)
			{
				ColumnKey = WebTracker.Grids.TrackingUSCForeignPorts.Code,
				CommandName = "Select" // Query related
			});
			AddDefaultsColumn(new ZTextEditColumn("Name", ZZRefCusCodeListCombined.Schema.ZZD_Description) { ColumnKey = WebTracker.Grids.TrackingUSCForeignPorts.Name });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingUSCForeignPortColumnProvider();
		}

		#endregion
	}
}
