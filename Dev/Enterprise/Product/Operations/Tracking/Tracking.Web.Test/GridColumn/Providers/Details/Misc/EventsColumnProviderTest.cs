using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(EventsColumnProvider))]
	sealed class EventsColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Event Code", StmALogSchema.SL_SE_NKEvent.Name) { ColumnKey = WebTracker.Grids.Event.Code });
			AddDefaultsColumn(new ZDateTimeColumn("Event Time", StmALogSchema.SL_EventTime.Name) { ColumnKey = WebTracker.Grids.Event.Time });
			AddDefaultsColumn(new ZTextEditColumn("Description", "Event.SE_Desc") { ColumnKey = WebTracker.Grids.Event.Description });
			AddDefaultsColumn(new ZTextEditColumn("Event Details", StmALog.Schema.DisplayEventReference) { ColumnKey = WebTracker.Grids.Event.EventDetails });
			AddColumn(new ZTextEditColumn("Event Source", "SL_TableFriendlyName") { ColumnKey = WebTracker.Grids.Event.Source });
			AddColumn(new ZCheckBoxColumn("Estimate", StmALogSchema.SL_IsEstimate.Name) { ColumnKey = WebTracker.Grids.Event.IsEstimate });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new EventsColumnProvider();
		}
	}
}
