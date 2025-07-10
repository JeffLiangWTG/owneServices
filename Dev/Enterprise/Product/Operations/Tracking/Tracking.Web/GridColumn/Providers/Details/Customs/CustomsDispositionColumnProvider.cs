using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsDispositionColumnProvider : GridColumnProvider
	{
		public CustomsDispositionColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dddf9760-028b-4c73-9603-2a7a02935e74", "Code ID"), ErrorsRecord.Schema.ErrorMessageIdentifier) { ColumnKey = WebTracker.Grids.CustomsDisposition.Code });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c4928511-d843-4597-9799-aab7764f7a79", "Narrative"), ErrorsRecord.Schema.NarrativeMessage) { ColumnKey = WebTracker.Grids.CustomsDisposition.Narrative });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("CBB1143E-E0BD-4E0C-8445-6AB2052AAB2E", "Date"), ErrorsRecord.Schema.StatusDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsDisposition.Date });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("9e96cc29-fbd2-4785-a68f-0ff4d26d6c76", "Release Date"), ErrorsRecord.Schema.ReleaseDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.CustomsDisposition.ReleaseDate });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("cd229ea2-6038-4b01-a050-2e879ea79f19", "Release Origin"), ErrorsRecord.Schema.ReleaseOrigin) { ColumnKey = WebTracker.Grids.CustomsDisposition.ReleaseOrigin });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("20d60b06-62f5-4ac0-8376-a2c2ca89b553", "Release Origin Description"), ErrorsRecord.Schema.ReleaseOriginDescription) { ColumnKey = WebTracker.Grids.CustomsDisposition.ReleaseOriginDescription });
		}
	}
}
