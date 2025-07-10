using System.Collections.Generic;
using System.Data;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SRDbDataSetUpdater : CommonDataSetUpdater<JsonObject>, ISRDbDataSetUpdater
	{
		public SRDbDataSetUpdater(DataSetScripts dataSetScripts, string dataContractVersion, IDBHelper dBHelper, IServerProxy proxy, IRefVersionControlManager versionControlManager, ILogger logger)
			: base(dBHelper, proxy, versionControlManager)
		{
			this.dataContractVersion = dataContractVersion;
			this.dBHelper = dBHelper;
			this.dataSetScripts = dataSetScripts;
			Logger = logger;
		}

		readonly IDBHelper dBHelper;
		readonly DataSetScripts dataSetScripts;
		readonly string dataContractVersion;

		public override string UpdaterName => dataSetScripts.DataSetType;

		public string[] Prerequisites => dataSetScripts.Prerequisites;

		protected override void PrepareTemporaryTables(string dataSetName)
		{
			foreach (var script in dataSetScripts.PrepareTemporaryTablesScripts)
			{
				dBHelper.ExecuteNonQuery(script.Value);
			}
		}

		protected sealed override void CleanupTemporaryTables(string dataSetName) { }

		protected override IEnumerable<DataTable> GetDataTables()
		{
			foreach (var tmpTableScript in dataSetScripts.PrepareTemporaryTablesScripts)
			{
				yield return SRDbDataSetHelper.CreateDataTable(tmpTableScript.Key, dBHelper);
			}
		}

		protected override void AddServerData(IEnumerable<DataTable> serverDataTables, JsonObject dataSet)
		{
			SRDbDataSetHelper.AddServerData(serverDataTables, dataSetScripts.Mapping, dataSet);
		}

		protected sealed override async Task MergeData(IDbTransaction transaction, string dataSetName, DataTable[] dataTables, int? mergeTimeout)
		{
			if (dataTables[0].Rows.Count > 0)
			{
				foreach (var dataTable in dataTables)
				{
					await dBHelper.BulkInsertAsync(dataSetName, dataTable, transaction, dataTable.TableName);
				}
				dBHelper.ExecuteNonQuery(dataSetScripts.MergeScript, transaction, mergeTimeout ?? 600);
			}
		}

		protected override string DataContractVersion => dataContractVersion;
		protected override string DataSetType => dataSetScripts.DataSetType;
		public override int UpdaterVersion => dataSetScripts.UpdaterVersion;
		protected override string GetCheckPoint(JsonObject data)
		{
			return (string)data["Checkpoint"];
		}
	}
}
