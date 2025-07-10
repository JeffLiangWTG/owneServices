using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsDispositionColumnProvider))]
	sealed class CustomsDispositionColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Code ID", ErrorsRecord.Schema.ErrorMessageIdentifier) { ColumnKey = WebTracker.Grids.CustomsDisposition.Code });
			AddDefaultsColumn(new ZTextEditColumn("Narrative", ErrorsRecord.Schema.NarrativeMessage) { ColumnKey = WebTracker.Grids.CustomsDisposition.Narrative });
			AddDefaultsColumn(new ZDateTimeColumn("Date", ErrorsRecord.Schema.StatusDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsDisposition.Date, SortExpression = ErrorsRecord.Schema.StatusDate });
			AddDefaultsColumn(new ZDateTimeColumn("Release Date", ErrorsRecord.Schema.ReleaseDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.CustomsDisposition.ReleaseDate });
			AddDefaultsColumn(new ZTextEditColumn("Release Origin", ErrorsRecord.Schema.ReleaseOrigin) { ColumnKey = WebTracker.Grids.CustomsDisposition.ReleaseOrigin });
			AddDefaultsColumn(new ZTextEditColumn("Release Origin Description", ErrorsRecord.Schema.ReleaseOriginDescription) { ColumnKey = WebTracker.Grids.CustomsDisposition.ReleaseOriginDescription });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsDispositionColumnProvider();
		}
	}
}
