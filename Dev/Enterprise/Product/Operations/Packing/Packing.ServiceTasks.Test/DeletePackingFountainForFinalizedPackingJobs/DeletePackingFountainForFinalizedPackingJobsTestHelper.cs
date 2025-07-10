using System;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using PkgPackageDO = CargoWise.Database.TestFramework.ObjectModel.PkgPackage;
using PkgPackageHeaderDO = CargoWise.Database.TestFramework.ObjectModel.PkgPackageHeader;
using PkgPackageJobDO = CargoWise.Database.TestFramework.ObjectModel.PkgPackageJob;

namespace Enterprise.Packing.ServiceTasks.Testing
{
	public static class DeletePackingFountainForFinalizedPackingJobsTestHelper
	{
		public static WhsDocket SetupWhsOrderPackingParent(StringBuilder sql, OrgHeader client, WhsWarehouse whs, Guid productPK, Guid locationPK, string orderReference, string docketId, bool isFinalised = false, int index = 1, DateTime? finalizedDate = null)
		{
			var today = finalizedDate ?? DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{index}") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, productPK, 10m, locationPK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = today,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = isFinalised
				? new WhsPick(whs, $"P{index}", "FIN") { WP_FinalizedDateUtc = today, WP_SystemLastEditTimeUtc = today, WP_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql)
				: new WhsPick(whs, $"P{index}", "NEW").AppendInsertAndReturnObject(sql);

			var order = isFinalised
				? new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", docketId, orderReference) { WD_FinalisedDate = today, WD_WP = pickWithOrder.PK, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql)
				: new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", docketId, orderReference) { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);

			var orderLine = isFinalised
				? new WhsDocketLine(order, productPK, 10m) { WE_FinalisedDate = today, WE_DocketLineStatus = "DEP" }.AppendInsertAndReturnObject(sql)
				: new WhsDocketLine(order, productPK, 10m).AppendInsertAndReturnObject(sql);

			new WhsPickLine(receiveLine, orderLine, 10m)
			{
				WZ_PickedDateTime = today,
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = today,
				WZ_SystemLastEditTimeUtc = today,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			return order;
		}

		public static PkgPackageJobDO SetupPackageWithPackingParent(StringBuilder sql, Guid packingParent, string parentTableCode, string jobId, int index = 1)
		{
			var packageJob = new PkgPackageJobDO(packingParent) { KJ_ParentTableCode = parentTableCode, KJ_JobID = jobId }.AppendInsertAndReturnObject(sql);
			var packageHeader = new PkgPackageHeaderDO($"PKG{index}", DateTime.UtcNow, "~BP").AppendInsertAndReturnObject(sql);
			new PkgPackageDO(packageJob, "CNT", 1) { KP_KPH_PackageHeader = packageHeader }.AppendInsertAndReturnObject(sql);

			return packageJob;
		}

		public static void SetupNumberFountain(string name, Guid owner, DbConnection testConnection)
		{
			var sql = @"-- InsertStmNums
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_MinimumValue, SN_Value, SN_MaximumValue) VALUES
	(@Name, @Owner, 1, 1, 9220000000000000000);";

			using (var cmd = testConnection.Command(sql))
			{
				cmd.AddParameter("@Name", SqlDbType.VarChar, name);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, owner);
				cmd.ExecuteNonQuery();
			}
		}

		public static IDisposable SuspendTrigger(string triggerName, string tableName, DbConnection testConnection)
		{
			return new DisposableAction(
				() => testConnection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => testConnection.ExecuteNonQuery($"ENABLE TRIGGER {triggerName} ON {tableName}")
			);
		}

		public static int GetNumberFountain(Guid packageJobPK, string fountainName, DbConnection testConnection)
		{
			return (int)testConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.StmNums WHERE SN_Name = '{fountainName}' AND SN_Owner = '{packageJobPK}'");
		}
	}
}
