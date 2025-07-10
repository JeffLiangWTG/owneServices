using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsEntriesDataColumnProvider : GridColumnProvider
	{
		public CustomsEntriesDataColumnProvider(TrackingDeclaration declaration)
		{
			this.declaration = declaration;
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("69a2d735-ca3d-4c99-8792-7da19aac9e4a", "Reference #"), Customs.Business.CusEntryHeader.Schema.CH_BGMReference) { ColumnKey = WebTracker.Grids.CustomsEntriesData.ReferenceNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f2d19734-2cf7-49d7-b175-b474574a4b5d", "Entry #"), Customs.Business.CusEntryHeader.Schema.EntryNumber) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryNumber });
			if (Declaration != null && Declaration.Declaration is Customs.AU.Declaration.Business.JobDeclaration)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9fb95ea7-e32a-4b54-8e05-f876b2602cb7", "Message Status"), Customs.AU.Declaration.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("39534917-1813-4988-bb49-87d12b2c0d7a", "Entry Advice"), Customs.AU.Declaration.Business.CusEntryHeader.Schema.ImportEntryAdvice) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
			else
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9fb95ea7-e32a-4b54-8e05-f876b2602cb7", "Message Status"), Customs.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("39534917-1813-4988-bb49-87d12b2c0d7a", "Entry Advice"), Customs.Business.CusEntryHeader.Schema.CH_EntryStatus) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
		}

		public TrackingDeclaration Declaration
		{
			get { return declaration; }
		}

		readonly TrackingDeclaration declaration;
	}
}
