using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingShipmentCustomsEntriesDataColumnProvider : GridColumnProvider
	{
		public ForwardingShipmentCustomsEntriesDataColumnProvider(TrackingShipment shipment)
		{
			this.shipment = shipment;
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("7212bf9f-444c-4667-8796-cc4a37c1debb", "Reference #"), Customs.Business.CusEntryHeader.Schema.CH_BGMReference) { ColumnKey = WebTracker.Grids.CustomsEntriesData.ReferenceNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("45388818-8233-43c9-8690-4dc6309bfde0", "Entry #"), Customs.Business.CusEntryHeader.Schema.EntryNumber) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryNumber });
			if (Shipment != null && Shipment.LastDeclaration != null && Shipment.LastDeclaration is Customs.AU.Declaration.Business.JobDeclaration)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9bb943fc-6b94-4b13-92bc-b58d8a1e2148", "Message Status"), Customs.AU.Declaration.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d7e08d89-430f-4e2a-bac4-a14307accfc7", "Entry advice"), Customs.AU.Declaration.Business.CusEntryHeader.Schema.ImportEntryAdvice) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
			else
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9bb943fc-6b94-4b13-92bc-b58d8a1e2148", "Message Status"), Customs.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c23bde14-d019-47fd-b6d7-0d77a3dc6b93", "Entry Advice"), Customs.Business.CusEntryHeader.Schema.CH_EntryStatus) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
		}

		public TrackingShipment Shipment
		{
			get { return shipment; }
		}

		readonly TrackingShipment shipment;
	}
}
