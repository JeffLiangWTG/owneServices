using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsEntryPGAStatusColumnProvider : GridColumnProvider
	{
		public CustomsEntryPGAStatusColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9261987B-4CD5-43DF-B177-553F9493BB87", "Agency Code"), CusDisposition.Schema.CDI_StatusKey) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.AgencyCode });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("785943E8-005E-4AB8-A975-AA2F5EAB2C7F", "Status Code"), CusDisposition.Schema.CDI_Status) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.StatusCode });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("7CB99345-7A52-40C5-BC07-E428EBE03F77", "Status Description"), CusDisposition.Schema.StatusDescription) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.StatusDescription });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("DB46051E-329B-48D6-97D9-BC7D7D5B8FCE", "Status Date"), CusDisposition.Schema.CDI_StatusDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.StatusDate });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("921D8CC3-041A-477F-B61F-50AC40698264", "Notes"), CusDisposition.Schema.CDI_Notes) { ColumnKey = WebTracker.Grids.CustomsPGAStatus.Notes });
		}
	}
}
