
namespace CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging
{
	interface IOceanCarrierMessagingType
	{
		string PrefixName { get; }
		string Name { get; }
		string ID { get; } // unique

		string[] JqGridColumns();
		string GetColumnName(string column);
		object ValuesEdit();
		object Save();
		object GetClients(bool includeNonProd);
		object GetServiceProviders();
		object GetDocumentNames();
		object GetPartiesToCopy();
		object GetSplitByOptions();
		object GetShipmentTypes();
		object Values();
		void ClearCache();
		object MoveRow();
		void ReformatSessionRowID();
		byte[] GetValuesExportCsv();
		string GetExportFileName();
		object ImportCsv();
	}
}
