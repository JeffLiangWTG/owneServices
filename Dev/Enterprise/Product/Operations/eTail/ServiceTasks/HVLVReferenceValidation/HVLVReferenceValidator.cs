using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.eTail.ServiceTasks
{
	class HVLVReferenceValidator
	{
		public HVLVReferenceValidator(ILogger logger, IHVLVReferenceInfo referenceInfo)
		{
			Logger = Argument.NotNull(logger, nameof(logger));
			ReferenceInfo = Argument.NotNull(referenceInfo, nameof(referenceInfo));
		}

		ILogger Logger { get; }
		IHVLVReferenceInfo ReferenceInfo { get; }

		public void Run(CancellationToken cancellationToken)
		{
			new NumberFountainFixer(ReferenceInfo.FallbackNumberFountainInfo, ReferenceInfo).FixFountainToNextAvailableSpot();
			RegenerateNonUniqueIds(cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			ValidateUniqueIds();
		}

		#region Regenerate IDs

		void RegenerateNonUniqueIds(CancellationToken cancellationToken)
		{
			const int loadBatchSize = 10000;
			var repeat = true;

			while (repeat)
			{
				var idsProcessedInBatch = 0;
				var factory = new BusinessObjectFactory();

				foreach (var headerGroup in GetHVLVRecordInfos(loadBatchSize).GroupBy(x => x.ShipperPk))
				{
					var shipper = factory.Load<OrgAddress>(headerGroup.Key);
					var gs1Info = GS1Wrapper.GetGS1Info(shipper, factory);

					if (gs1Info?.SSCCNumberFountain != null)
					{
						var ssccFountainInfo = new NumberFountainInfo(gs1Info.SSCCNumberFountain, gs1Info.GS1Prefix, SSCCBarCodeChecker.GetFormatDigitsFromPrefix(gs1Info.GS1Prefix));
						new SSCCNumberFountainFixer(ssccFountainInfo, ReferenceInfo).FixFountainToNextAvailableSpot();
					}

					const int updateBatchSize = 500;

					foreach (var updateBatch in headerGroup.Batch(updateBatchSize))
					{
						var hvlvRecordInfos = updateBatch.ToArray();
						RegenerateNonUniqueIdsBatch(hvlvRecordInfos, gs1Info, cancellationToken);
						idsProcessedInBatch += hvlvRecordInfos.Length;
					}
				}

				repeat = idsProcessedInBatch == loadBatchSize;
			}
		}

		void RegenerateNonUniqueIdsBatch(HVLVRecordInfo[] hvlvRecordInfos, GS1Wrapper gs1Info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory();

			factory.Saving += fact =>
			{
				foreach (var hvlvRecordInfo in hvlvRecordInfos)
				{
					hvlvRecordInfo.NewId = gs1Info?.SSCCNumberFountain?.GetNextFormatted(factory) ?? ReferenceInfo.FallbackNumberFountainInfo.Fountain.GetNextFormatted(factory);
				}

				UpdateRecordsInDatabase(hvlvRecordInfos);
				AddCIDEvents(hvlvRecordInfos);
			};

			factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateRecordsInDatabase(HVLVRecordInfo[] hvlvRecordInfos)
		{
			const string newValueParam = "@NewValue";
			const string pkParam = "@Pk";
			var sql = Invariant($@"
UPDATE {ReferenceInfo.IdColumn.TableName}
SET {ReferenceInfo.IdColumn.Name} = {newValueParam}{{0}}
WHERE {ReferenceInfo.PkColumn.Name} = {pkParam}{{0}}"); // Direct SQL query

			var sqlFormatted = string.Join(System.Environment.NewLine, Enumerable.Range(0, hvlvRecordInfos.Length).Select(x => string.Format(CultureInfo.InvariantCulture, sql, x)));

			using (var cmd = Db.Connection.Command(sqlFormatted)) // Performance and memory overhead
			{
				foreach (var i in Enumerable.Range(0, hvlvRecordInfos.Length))
				{
					var hvlvRecordInfo = hvlvRecordInfos[i];
					cmd.AddParameter(Invariant($"{newValueParam}{i}"), SqlDbType.VarChar, hvlvRecordInfo.NewId); // SQL Parameter
					cmd.AddParameter(Invariant($"{pkParam}{i}"), SqlDbType.UniqueIdentifier, hvlvRecordInfo.Pk); // SQL Parameter
				}

				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddCIDEvents(HVLVRecordInfo[] hvlvRecordInfos)
		{
			const int logBatchSize = 500;
			const string parentPkParam = "@ParentPk";
			const string referenceParam = "@Reference";

			var sqlInsertHeader = Invariant($@"
INSERT INTO {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
({StmALogSchema.Constants.PK}, {StmALogSchema.Constants.SL_SE_NKEvent}, {StmALogSchema.Constants.SL_Table}, {StmALogSchema.Constants.SL_Parent}, {StmALogSchema.Constants.SL_Reference},
{StmALogSchema.Constants.SL_FireWorkflow}, {StmALogSchema.Constants.SL_EventTime}, {StmALogSchema.Constants.SL_GS_NKUser}, {StmALogSchema.Constants.SL_GB_NKBranch}, {StmALogSchema.Constants.SL_GE_NKDepartment})
VALUES");
			var sqlInsertRowDetails = Invariant($@"
(NEWID(), '{AutoEvents.ChangeOfIdentifierCode}', '{ReferenceInfo.IdColumn.TableName}', {parentPkParam}{{0}}, {referenceParam}{{0}},
1, '{ZDateTime.Now.SqlFormat}', '{GlbStaff.CurrentUser.GS_Code}', '{GlbBranch.CurrentBranch.GB_Code}', '{GlbDepartment.CurrentDepartment.GE_Code}')");

			foreach (var hvlvRecordInfoBatch in hvlvRecordInfos.Batch(logBatchSize))
			{
				var hvlvRecordInfoBatchArray = hvlvRecordInfoBatch.ToArray();
				var sqlFormatted = Invariant($@"{sqlInsertHeader}
{string.Join(",", Enumerable.Range(0, hvlvRecordInfoBatchArray.Length).Select(x => string.Format(CultureInfo.InvariantCulture, sqlInsertRowDetails, x)))}"); // Direct SQL query

				using (var cmd = Db.Connection.Command(sqlFormatted)) // Performance and memory overhead
				{
					foreach (var i in Enumerable.Range(0, hvlvRecordInfoBatchArray.Length))
					{
						var hvlvRecordInfo = hvlvRecordInfoBatchArray[i];
						var eventParameters = new[]
						{
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Old, hvlvRecordInfo.OriginalId),
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, hvlvRecordInfo.NewId)
						};
						var eventReference = StmALog.GenerateEventReference(string.Empty, eventParameters).ToString();

						cmd.AddParameter(Invariant($"{parentPkParam}{i}"), SqlDbType.UniqueIdentifier, hvlvRecordInfo.Pk); // SQL Parameter
						cmd.AddParameter(Invariant($"{referenceParam}{i}"), SqlDbType.VarChar, eventReference); // SQL Parameter
					}

					cmd.ExecuteNonQuery();
				}

				foreach (var hvlvRecordInfo in hvlvRecordInfoBatchArray)
				{
					Logger.Log(LogType.Debug, Invariant($"{ReferenceInfo.IdColumn.TableName} {hvlvRecordInfo.Pk} ID updated from '{hvlvRecordInfo.OriginalId}' to '{hvlvRecordInfo.NewId}'"));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<HVLVRecordInfo> GetHVLVRecordInfos(int batchSize)
		{
			const string batchSizeParam = "@BatchSize";

			var sql = Invariant($@"
WITH UnvalidatedRecordIds AS
(
	SELECT {ReferenceInfo.PkColumn.Name}, {ReferenceInfo.IdColumn.Name}, {ReferenceInfo.ClusterKeyColumn.Name}, ROW_NUMBER() OVER (PARTITION BY {ReferenceInfo.IdColumn.Name} ORDER BY {OrderRecordsBy}) RN
	FROM {ReferenceInfo.IdColumn.TableName}
	WHERE {ReferenceInfo.IsValidatedForUniquenessColumn.Name} = 0
),
UnvalidatedRecordIdsWithShipper AS
(
	SELECT {ReferenceInfo.PkColumn.Name}, {ReferenceInfo.IdColumn.Name}, {HVLVBookingHeaderSchema.Constants.HVH_OA_BillToParty}
	FROM UnvalidatedRecordIds
	LEFT JOIN {HVLVBookingHeaderSchema.Constants.SqlSchemaName}.{HVLVBookingHeaderSchema.Constants.TableName} ON {HVLVBookingHeaderSchema.Constants.HVH_ClusterKey} = {ReferenceInfo.ClusterKeyColumn.Name}
	WHERE RN != 1
	UNION
	SELECT {ReferenceInfo.PkColumn.Name}, {ReferenceInfo.IdColumn.Name}, {HVLVBookingHeaderSchema.Constants.HVH_OA_BillToParty}
	FROM UnvalidatedRecordIds
	LEFT JOIN {HVLVBookingHeaderSchema.Constants.SqlSchemaName}.{HVLVBookingHeaderSchema.Constants.TableName} ON {HVLVBookingHeaderSchema.Constants.HVH_ClusterKey} = {ReferenceInfo.ClusterKeyColumn.Name}
	WHERE {ReferenceInfo.IdColumn.Name} IN
	(
		SELECT InnerRecords.{ReferenceInfo.IdColumn.Name}
		FROM {ReferenceInfo.IdColumn.TableName} InnerRecords
		WHERE InnerRecords.{ReferenceInfo.IsValidatedForUniquenessColumn.Name} = 1
	)
)
SELECT TOP ({batchSizeParam}) *
FROM UnvalidatedRecordIdsWithShipper"); // Direct SQL query

			using (var cmd = Db.Connection.Command(sql)) // Performance and memory overhead
			{
				cmd.AddParameter(batchSizeParam, SqlDbType.Int, batchSize);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = reader.GetGuid(0);
						var id = reader.GetString(1);
						var shipperPk = reader.IsDBNull(2) ? Guid.Empty : reader.GetGuid(2);
						yield return new HVLVRecordInfo(pk, id, shipperPk);
					}
				}
			}
		}

		class HVLVRecordInfo
		{
			public HVLVRecordInfo(Guid pk, string originalId, Guid shipperPk)
			{
				Pk = pk;
				OriginalId = originalId;
				ShipperPk = shipperPk;
			}

			public Guid Pk { get; }
			public string OriginalId { get; }
			public Guid ShipperPk { get; }

			public string NewId { get; set; }
		}

		#endregion

		#region Validate IDs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL query, Performance and memory overhead")]
		void ValidateUniqueIds()
		{
			var sql = Invariant($@"
WITH UnusedRecordIds AS
(
	SELECT {ReferenceInfo.PkColumn.Name}, ROW_NUMBER() OVER (PARTITION BY {ReferenceInfo.IdColumn.Name} ORDER BY {OrderRecordsBy}) RN
	FROM {ReferenceInfo.IdColumn.TableName}
	WHERE {ReferenceInfo.IsValidatedForUniquenessColumn.Name} = 0
	AND {ReferenceInfo.IdColumn.Name} NOT IN
	(
		SELECT InnerRecords.{ReferenceInfo.IdColumn.Name}
		FROM {ReferenceInfo.IdColumn.TableName} InnerRecords
		WHERE InnerRecords.{ReferenceInfo.IsValidatedForUniquenessColumn.Name} = 1
	)
)
UPDATE {ReferenceInfo.IdColumn.TableName}
SET {ReferenceInfo.IsValidatedForUniquenessColumn.Name} = 1
FROM {ReferenceInfo.IdColumn.TableName}
JOIN UnusedRecordIds ON {ReferenceInfo.IdColumn.TableName}.{ReferenceInfo.PkColumn.Name} = UnusedRecordIds.{ReferenceInfo.PkColumn.Name}
WHERE RN = 1
SELECT @@ROWCOUNT");

			int rowsValidated;

			using (var cmd = Db.Connection.Command(sql))
			{
				rowsValidated = (int)cmd.ExecuteScalar();
			}

			if (rowsValidated > 0)
			{
				Logger.Log(LogType.Debug, Invariant($"{rowsValidated} {ReferenceInfo.IdColumn.TableName} {(rowsValidated == 1 ? "row" : "rows")} validated"));
			}
		}

		#endregion

		string OrderRecordsBy => ReferenceInfo.CreateTimeColumn?.Name ?? ReferenceInfo.IdColumn.Name;
	}
}
