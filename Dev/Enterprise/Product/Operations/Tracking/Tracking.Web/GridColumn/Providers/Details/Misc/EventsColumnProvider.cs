using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class EventsColumnProvider : GridColumnProvider
	{
		public EventsColumnProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((StmALog)null).SL_SE_NKEvent);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c480c76e-ff68-4346-9785-295c63f52a3b", "Event Code"), StmALogSchema.SL_SE_NKEvent.Name) { ColumnKey = WebTracker.Grids.Event.Code });

			ZBindToChecker.CheckBindTo((ZDateTime)((StmALog)null).SL_EventTime);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("f9d98add-74ce-4588-9947-1841b4a1b740", "Event Time"), StmALogSchema.SL_EventTime.Name) { ColumnKey = WebTracker.Grids.Event.Time });

			ZBindToChecker.CheckBindTo((StmEvent)((StmALog)null).Event);
			ZBindToChecker.CheckBindTo((ZString)((StmEvent)null).SE_Desc);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f2c8cf35-2661-4337-8768-6b69daabb765", "Description"), "Event.SE_Desc") { ColumnKey = WebTracker.Grids.Event.Description });

			ZBindToChecker.CheckBindTo((ZString)((StmALog)null).DisplayEventReference);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("F52B98AE-F269-43BB-98CC-9FB3E52294A3", "Event Details"), StmALog.Schema.DisplayEventReference) { ColumnKey = WebTracker.Grids.Event.EventDetails });

			ZBindToChecker.CheckBindTo((ZString)((StmALog)null).SL_TableFriendlyName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4ab6293c-b5f8-4e9b-9aaf-6dc6cb559dce", "Event Source"), StmALog.Schema.SL_TableFriendlyName) { ColumnKey = WebTracker.Grids.Event.Source });

			ZBindToChecker.CheckBindTo((ZBool)((StmALog)null).SL_IsEstimate);
			AddToDictionary(new ZCheckBoxColumn(Res.GetString("ed705e40-1d13-46ce-9a8d-e847e4070699", "Estimate"), StmALogSchema.SL_IsEstimate.Name) { ColumnKey = WebTracker.Grids.Event.IsEstimate });
		}
	}
}
