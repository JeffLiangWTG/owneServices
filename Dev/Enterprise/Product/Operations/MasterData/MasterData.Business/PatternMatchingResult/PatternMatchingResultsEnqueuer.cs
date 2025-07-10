using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public static class PatternMatchingResultsEnqueuer
	{
		public static bool QueueOrgHeaderForDeduplicationProcessing(OrgHeader master, BusinessObjectFactory factory)
		{
			var queued = false;

			using (var inputWithLock = new List<Guid> { master.PK.ToGuid() }.ApplyAppLocks(PatternMatchingConstants.AppLockKey))
			{
				if (inputWithLock.ItemsWithLocks.Any())
				{
					var queryResult = GetExistingQueuedRecord(master, factory);

					if (queryResult == null)
					{
						CreateQueuedRecordAndSave(master, PatternMatchingResult.StatusCodes.Queued);
						queued = true;
					}
					else if (queryResult.PMT_Status == PatternMatchingResult.StatusCodes.QueuedForProcessing)
					{
						queryResult.PMT_Status = PatternMatchingResult.StatusCodes.Queued;
						queued = true;
					}
				}
			}

			return queued;
		}

		public static bool QueueGlbPersonForDeduplicationProcessing(GlbPerson master, BusinessObjectFactory factory)
		{
			var queued = false;

			using (var inputWithLock = new List<Guid> { master.PK.ToGuid() }.ApplyAppLocks(PatternMatchingConstants.AppLockKey))
			{
				if (inputWithLock.ItemsWithLocks.Any())
				{
					var queryResult = GetExistingQueuedRecord(master, factory);

					if (queryResult == null)
					{
						CreateQueuedRecordAndSave(master, PatternMatchingResult.StatusCodes.QueuedForProcessing);
						queued = true;
					}
				}
			}

			return queued;
		}

		static PatternMatchingResult GetExistingQueuedRecord(BusinessObject master, BusinessObjectFactory factory)
		{
			factory.ClearQueryCache(AutoPatternMatchingResult.Schema.TableName);

			var query = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, master.PK)
			.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, null)
			.AddToFilter(PatternMatchingResultSchema.PMT_GS_NKExcludeBy, User.ServiceUserCode);

			return factory.LoadTop1<PatternMatchingResult>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		static void CreateQueuedRecordAndSave(BusinessObject master, string status)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO {0} ( {1}, {2}, {3}, {4}, {5}, {6}, {7} )
							VALUES
							(@PK, @MasterPK, @TablePrefix, @Status, @ScorePercent, @FoundTimeUtc, @ServiceUserCode)",
								AutoPatternMatchingResult.Schema.TableName,
								AutoPatternMatchingResult.Schema.PK,
								AutoPatternMatchingResult.Schema.PMT_MasterPK,
								AutoPatternMatchingResult.Schema.PMT_MasterTableCode,
								AutoPatternMatchingResult.Schema.PMT_Status,
								AutoPatternMatchingResult.Schema.PMT_ScorePercent,
								AutoPatternMatchingResult.Schema.PMT_FoundTimeUtc,
								AutoPatternMatchingResult.Schema.PMT_GS_NKExcludeBy);  // It is not valid to call Factory.Save in Log Walker News Transmitter.It is never valid or safe to call Factory.Save within a LogSubscriber of the LWK service task. 

			using (var cmd = Db.Connection.Command(sqlText)) // It is not valid to call Factory.Save in Log Walker News Transmitter. It is never valid or safe to call Factory.Save within a LogSubscriber of the LWK service task. 
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				cmd.AddParameter("@MasterPK", SqlDbType.UniqueIdentifier, master.PK.ToGuid());
				cmd.AddParameter("@TablePrefix", SqlDbType.VarChar, master.TablePrefix);
				cmd.AddParameter("@Status", SqlDbType.VarChar, status);
				cmd.AddParameter("@ScorePercent", SqlDbType.Int, 0);
				cmd.AddParameter("@FoundTimeUtc", SqlDbType.DateTime, DateTime.UtcNow);
				cmd.AddParameter("@ServiceUserCode", SqlDbType.VarChar, User.ServiceUserCode);

				cmd.ExecuteNonQuery();
			}
		}
	}
}
