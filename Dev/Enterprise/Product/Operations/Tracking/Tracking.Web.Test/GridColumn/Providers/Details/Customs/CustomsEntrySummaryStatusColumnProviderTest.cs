using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsEntrySummaryStatusColumnProvider))]
	sealed class CustomsEntrySummaryStatusColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Error ID", ErrorsRecord.Schema.ErrorMessageIdentifier) { ColumnKey = WebTracker.Grids.CustomsEntrySummaryStatus.ErrorID });
			AddDefaultsColumn(new ZTextEditColumn("Narrative Message", ErrorsRecord.Schema.NarrativeMessage) { ColumnKey = WebTracker.Grids.CustomsEntrySummaryStatus.NarrativeMessage });
			AddDefaultsColumn(new ZDateTimeColumn("Status Date", ErrorsRecord.Schema.StatusDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsEntrySummaryStatus.StatusDate });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsEntrySummaryStatusColumnProvider();
		}
	}
}
