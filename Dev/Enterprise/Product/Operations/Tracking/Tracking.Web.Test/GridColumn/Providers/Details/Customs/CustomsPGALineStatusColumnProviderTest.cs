using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsPGALineStatusColumnProvider))]
	sealed class CustomsPGALineStatusColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("PGA", OGADispositionData.Schema.US_OGAIdentifier) { ColumnKey = WebTracker.Grids.CustomsPGALine.PGA });
			AddDefaultsColumn(new ZDateTimeColumn("Date", OGADispositionData.Schema.US_DispositionDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsPGALine.Date });
			AddDefaultsColumn(new ZTextEditColumn("Entry Status Code", OGADispositionData.Schema.US_OGADispositionStatusCode) { ColumnKey = WebTracker.Grids.CustomsPGALine.Status });
			AddDefaultsColumn(new ZTextEditColumn("Entry Status Message", OGADispositionData.Schema.US_OGADispositionStatusMessage) { ColumnKey = WebTracker.Grids.CustomsPGALine.StatusMessage });
			AddDefaultsColumn(new ZTextEditColumn("PGA Line Status Code", OGADispositionData.Schema.US_Code) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionCode });
			AddDefaultsColumn(new ZTextEditColumn("PGA Line Status", OGADispositionData.Schema.DispositionCodeDesc) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionCodeDescription });
			AddDefaultsColumn(new ZTextEditColumn("Beg. CBP Line", OGADispositionData.Schema.US_OGADispositionBeginningCBPLine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionBeginningCBPLine });
			AddDefaultsColumn(new ZTextEditColumn("Beg. PGA Line", OGADispositionData.Schema.US_OGADispositionBeginningOGALine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionBeginningPGALine });
			AddDefaultsColumn(new ZTextEditColumn("Range", OGADispositionData.Schema.US_OGADispositionRangeIndicator) { ColumnKey = WebTracker.Grids.CustomsPGALine.Range });
			AddDefaultsColumn(new ZTextEditColumn("End PGA Line", OGADispositionData.Schema.US_OGADispositionEndOGALine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionEndPGALine });
			AddDefaultsColumn(new ZTextEditColumn("End CBP Line", OGADispositionData.Schema.US_OGADispositionEndCBPLine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionEndCBPLine });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsPGALineStatusColumnProvider();
		}
	}
}
