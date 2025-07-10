using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsEntrySummaryStatusColumnProvider : GridColumnProvider
	{
		public CustomsEntrySummaryStatusColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("fabb1f01-904e-4106-86b7-53c060ede38c", "Error ID"), ErrorsRecord.Schema.ErrorMessageIdentifier) { ColumnKey = WebTracker.Grids.CustomsEntrySummaryStatus.ErrorID });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("98b50c35-335d-4083-ae65-ddabf7e47755", "Narrative Message"), ErrorsRecord.Schema.NarrativeMessage) { ColumnKey = WebTracker.Grids.CustomsEntrySummaryStatus.NarrativeMessage });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("11f8dbc9-b2eb-434d-8d0c-e53ab9f4cd20", "Status Date"), ErrorsRecord.Schema.StatusDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsEntrySummaryStatus.StatusDate });
		}
	}
}
