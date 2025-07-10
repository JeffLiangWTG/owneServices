using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.ServiceTask.Deduplication
{
	class DeduplicationQueueProcessBatcher<T> : ManagedBatchProcessor<AppLockedItem<T>> where T : BusinessObject, IDeduplicatable
	{
		internal DeduplicationQueueProcessBatcher(IDuplicateDetectorProvider duplicateDetectorProvider, Func<T, IEnumerable<DuplicateDetectorProviderOutput>> detectDuplicates, SchemaPKColumn schemaPKColumn, string parentTableCode) : base()
		{
			this.duplicateDetectorProvider = duplicateDetectorProvider;
			this.detectDuplicates = detectDuplicates;
			this.schemaPKColumn = schemaPKColumn;
			this.parentTableCode = parentTableCode;
		}

		readonly IDuplicateDetectorProvider duplicateDetectorProvider;
		readonly Func<T, IEnumerable<DuplicateDetectorProviderOutput>> detectDuplicates;
		readonly SchemaPKColumn schemaPKColumn;
		readonly string parentTableCode;

		protected override ZQuery GetQuery()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "{0} IN (SELECT TOP {1} PMT_MasterPK FROM dbo.PatternMatchingResult WHERE PMT_Status = @Status AND PMT_MasterTableCode = @MasterTableCode)", schemaPKColumn.Name, BatchSize);
			var collection = new ZSqlParameterCollection()
			{
				ZSqlParameter.New("@Status", PatternMatchingResult.StatusCodes.QueuedForProcessing, PatternMatchingResultSchema.PMT_Status),
				ZSqlParameter.New("@MasterTableCode", parentTableCode, PatternMatchingResultSchema.PMT_MasterTableCode)
			};

			var query = new ZDBOnlyQuery(typeof(T));
			query.IgnoreBlobFieldsCheck = false;
			query.AddFilterAndZSQLParameterCollection(sqlText, collection);

			return query;
		}

		protected override ZQuery GetSingularQuery(AppLockedItem<T> row)
		{
			return new ZQuery(schemaPKColumn, row.Item.PK);
		}

		void DeleteItems(T row, BusinessObjectFactory patternMatchingResultFactory)
		{
			var itemsToDeleteQuery = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, row.PK);
			itemsToDeleteQuery.AddToFilter(JoinCondition.Or, PatternMatchingResultSchema.PMT_TargetPK, row.PK);
			var itemsToDelete = patternMatchingResultFactory.Load<PatternMatchingResult>(itemsToDeleteQuery);

			foreach (var item in itemsToDelete)
			{
				item.Delete();
			}
		}

		protected override void MarkRowAsBadCore(INotifications notifications, AppLockedItem<T> lockedRow, BusinessObjectFactory patternMatchingResultFactory)
		{
			var row = lockedRow.Item;
			notifications.AddWarning(FormattableString.Invariant($"De-duplication error caused with {row.TablePrefix}: {row.PK}"));
			DeleteItems(row, patternMatchingResultFactory);
		}

		protected override void ProcessRowCore(INotifications notifications, CancellationToken token, AppLockedItem<T> lockedRow, BusinessObjectFactory patternMatchingResultFactory, (int, int) position)
		{
			var row = lockedRow.Item;
			row.ShouldRunDeduplication = true;

			DeleteItems(row, patternMatchingResultFactory);

			var detectorOutputs = detectDuplicates(row);

			if (duplicateDetectorProvider.LastRunStatus != DuplicationStatus.OK)
			{
				PatternMatchingResult.CreatePatternMatchingResult(row.TablePrefix, row.PK, patternMatchingResultFactory, PatternMatchingResult.StatusCodes.Error);

				if (duplicateDetectorProvider.LastRunStatus is DuplicationStatus.Timeout)
				{
					notifications.AddWarning(FormattableString.Invariant($"Duplicate detection for {row.TablePrefix}: {row.PK} took longer than expected"));
				}
				else
				{
					notifications.AddWarning(FormattableString.Invariant($"An error occurred while duplicate detection for {row.TablePrefix}: {row.PK}"));
				}
			}
			else if (detectorOutputs.IsNullOrEmpty())
			{
				PatternMatchingResult.CreatePatternMatchingResult(row.TablePrefix, row.PK, patternMatchingResultFactory, PatternMatchingResult.StatusCodes.NoDuplicates);
				notifications.AddInfo(FormattableString.Invariant($"'No Duplicates' status has been created for {row.TablePrefix}: {row.PK}"));
			}
			else
			{
				foreach (var output in detectorOutputs)
				{
					if (!patternMatchingResultFactory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, row.PK)
						.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, output.TargetPK)))
					{
						var inconsistentRecordForTarget = patternMatchingResultFactory.LoadTop1<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, output.TargetPK)
							.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, DBNull.Value));
						if (inconsistentRecordForTarget != null)
						{
							inconsistentRecordForTarget.Delete();
						}

						DuplicateDetectorProvider.AddBothDuplicateRecords(output, patternMatchingResultFactory);
						notifications.AddInfo(FormattableString.Invariant($"'Potential Duplicate' status has been created for master {row.TablePrefix}: {row.PK} and target {row.TablePrefix}: {output.TargetPK}"));
					}
				}
			}
		}

		protected override IList<AppLockedItem<T>> LoadBatchCore(BusinessObjectFactory patternMatchingResultFactory, ZQuery query)
		{
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(T));
			dbOnlyQuery.AddToFilter(query);
			return new ReadOnlyBusinessObjectFactory { NameForDebugging = string.Format(CultureInfo.InvariantCulture, "DQP Master Loader") }.LoadWithApplocks<T>(PatternMatchingConstants.AppLockKey, dbOnlyQuery, BatchSize).ItemsWithLocks;
		}

		protected override void OnAfterBatch(IList<AppLockedItem<T>> batch)
		{
			foreach (var row in batch)
			{
				row.Dispose();
			}
		}

		public override int BatchSize => OrganisationsDataRegistry.Instance.QueueProcessingBatchSize.Value;
	}
}
