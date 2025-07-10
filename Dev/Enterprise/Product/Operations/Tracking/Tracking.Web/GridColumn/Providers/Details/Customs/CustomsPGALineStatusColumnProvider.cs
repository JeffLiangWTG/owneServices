using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsPGALineStatusColumnProvider : GridColumnProvider
	{
		public CustomsPGALineStatusColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a4f16f25-4afb-45de-811c-ecb0c6c0895b", "PGA"), OGADispositionData.Schema.US_OGAIdentifier) { ColumnKey = WebTracker.Grids.CustomsPGALine.PGA });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("ec57211c-8586-4146-ad2e-4e1a36dbf1b9", "Date"), OGADispositionData.Schema.US_DispositionDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsPGALine.Date });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("15801432-212f-47ed-ab40-e8d52754a03e", "Entry Status Code"), OGADispositionData.Schema.US_OGADispositionStatusCode) { ColumnKey = WebTracker.Grids.CustomsPGALine.Status });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("34f3f449-bb15-45d8-8907-926d54fc3d87", "Entry Status Message"), OGADispositionData.Schema.US_OGADispositionStatusMessage) { ColumnKey = WebTracker.Grids.CustomsPGALine.StatusMessage });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dfdb74cf-8e0c-4fda-b5b1-c95ac2ae36e0", "PGA Line Status Code"), OGADispositionData.Schema.US_Code) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionCode });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("599963c6-6c34-4038-8acc-bbe91de83851", "PGA Line Status"), OGADispositionData.Schema.DispositionCodeDesc) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionCodeDescription });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9a535a59-041d-46d7-b4ae-cdd4b39e9abf", "Beg. CBP Line"), OGADispositionData.Schema.US_OGADispositionBeginningCBPLine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionBeginningCBPLine });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2410ba26-5873-4e3f-8089-5039e0eb30e5", "Beg. PGA Line"), OGADispositionData.Schema.US_OGADispositionBeginningOGALine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionBeginningPGALine });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("70a082c3-4829-4162-8051-fd816c3a8a13", "Range"), OGADispositionData.Schema.US_OGADispositionRangeIndicator) { ColumnKey = WebTracker.Grids.CustomsPGALine.Range });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f301309d-327e-4fa1-a4ff-95b1fe6766bf", "End PGA Line"), OGADispositionData.Schema.US_OGADispositionEndOGALine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionEndPGALine });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("28ded740-87f5-4183-ab59-a79900ca7671", "End CBP Line"), OGADispositionData.Schema.US_OGADispositionEndCBPLine) { ColumnKey = WebTracker.Grids.CustomsPGALine.DispositionEndCBPLine });
		}
	}
}
