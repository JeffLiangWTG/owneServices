using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class FreightJobMawbLink
	{
		readonly MAWBStockManagementStrategy mAWBStockManagementStrategy;

		public FreightJobMawbLink(BusinessObjectFactory factory)
		{
			Factory = factory;
			mAWBStockManagementStrategy = new MAWBStockManagementStrategy(factory);
		}
		readonly BusinessObjectFactory Factory;

		#region Allocate/Deallocate MAWBs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Error code string")]
		public JobMawb AllocateUnusedMAWB(ZString airlinePrefix, ZString mawbBookingReference, ZString serviceLevel,
			ZGuid parentID, ZString parentPrefix, JobMawb exceptMawb = null)
		{
			ZGuid allocatedMAWBId = ZGuid.Empty;
			if (!mawbBookingReference.IsEmpty)
			{
				mawbBookingReference = mawbBookingReference.ExcludeChars(" -").SubstringSafe(3, 8);
			}

			using (var command = Db.Connection.Command("TryAllocateMAWB"))  // Calling the stored procedure
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@CompanyID", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@BranchID", SqlDbType.UniqueIdentifier, GlbBranch.CurrentBranch.PK.ToGuid());
				command.AddParameter("@UseOtherBranchMawbRegistryValue", SqlDbType.Bit, 1, FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.Value);
				command.AddParameter("@AirlinePrefix", SqlDbType.VarChar, 3, airlinePrefix.ToString());
				command.AddParameter("@ServiceLevel", SqlDbType.VarChar, 3, serviceLevel.ToString());
				command.AddParameter("@BookingReference", SqlDbType.VarChar, JobMawbSchema.JM_MAWB.MaxLength, mawbBookingReference.ToString());
				command.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, parentID.ToGuid());
				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, 3, parentPrefix.ToString());
				command.AddParameter("@ExceptMawbID", SqlDbType.UniqueIdentifier, exceptMawb == null ? Guid.Empty : exceptMawb.PK.ToGuid());
				command.AddParameter("@UserCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
				command.AddOutputParameter("@JobMAWBId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				try
				{
					command.ExecuteNonQuery();
					allocatedMAWBId = (Guid)command.GetParameterValue("@JobMAWBId");
					if (!allocatedMAWBId.IsEmpty)
					{
						var allocatedMAWB = LoadOrRefreshMAWB(allocatedMAWBId);
						if (allocatedMAWB != null)
						{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							allocatedMAWB.Logs.AddNew(Events.EditedARecord, ZString.Format("Attached to job {0}", allocatedMAWB.JM_Calc_ParentJobNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						}
						return allocatedMAWB;
					}
					else
					{
						return null;
					}
				}
				catch (SqlException ex)
				{
					var errorHandler = new DbErrorHandler(ex, Db.Connection);

					if (ex.Message == "[=NO_MAWBS_AVAILABLE=]")
					{
						throw new MAWBAllocationException(NoMasterBillNumbersInStockMessage);
					}
					else if (errorHandler.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
					{
						return LoadFromParentPK(parentPrefix, parentID);
					}
					else
					{
						throw;
					}
				}
			}
		}

		public static string NoMasterBillNumbersInStockMessage => Res.GetString("79d965ce-eef2-4792-a70e-2ed5d9db1bd0", "No Master Bill Numbers in stock to allocate to this job. MAWB stock can be added via Maintain > Reference Files > MAWB Stock.");

		public void DoDeallocateMAWB(ZGuid jobMAWBId, ZGuid parentId, IStmNoteParent notesParent)
		{
			var mawbToUnallocate = Factory.Load<JobMawb>(jobMAWBId);
			if (mawbToUnallocate != null && parentId != ZGuid.Empty)
			{
				var parentJobNumber = mawbToUnallocate.JM_Calc_ParentJobNumber;
				var isPrintedMawb = mawbToUnallocate.JM_IsPrinted;

				UnallocateJobMAWB(mawbToUnallocate, parentId);
				mawbToUnallocate = LoadOrRefreshMAWB(jobMAWBId);

				if (isPrintedMawb)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					mawbToUnallocate.Logs.AddNew(Events.EditedARecord, ZString.Format("Detached printed MAWB from job {0}", parentJobNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					CreateOrUpdatePrintedMawbUnallocationNote(mawbToUnallocate, notesParent);
				}
				else
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					mawbToUnallocate.Logs.AddNew(Events.EditedARecord, ZString.Format("Detached from job {0}", parentJobNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		void UnallocateJobMAWB(JobMawb mawb, ZGuid parentId)
		{
			using (var command = Db.Connection.Command("UnallocateMAWB"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@JobMAWBId", SqlDbType.UniqueIdentifier, mawb.PK.ToGuid());
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentId.ToGuid());
				command.AddParameter("@UserCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Error code string")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Calling the stored procedure, Error code string")]
		public JobMawb TryForceAllocateJobMAWB(ZGuid jobMAWBId, ZGuid parentID, ZString parentPrefix)
		{
			using (var command = Db.Connection.Command("ForceAllocateMAWB"))
			{
				try
				{
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameter("@JobMAWBId", SqlDbType.UniqueIdentifier, jobMAWBId.ToGuid());
					command.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, parentID.ToGuid());
					command.AddParameter("@ParentTableCode", SqlDbType.VarChar, 3, parentPrefix.ToString());
					command.AddParameter("@UserCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
					command.ExecuteNonQuery();
					var allocatedMAWB = LoadOrRefreshMAWB(jobMAWBId);
					if (allocatedMAWB != null)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						allocatedMAWB.Logs.AddNew(Events.EditedARecord, ZString.Format("Attached to job {0}", allocatedMAWB.JM_Calc_ParentJobNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					return allocatedMAWB;
				}
				catch (SqlException ex)
				{
					if (ex.Message == "[=UNABLE TO ALLOCATE MAWB=]")
					{
						throw new MAWBAllocationException(Res.GetString("1aa923d0-efa8-447a-84e6-e131e7954ec6", "Unable to allocate the specified MAWB."));
					}
					else
					{
						throw;
					}
				}
			}
		}

		#endregion

		#region Load

		public JobMawb LoadExistingByMAWB(ZString airlinePrefix, ZString mawbNo)
		{
			JobMawb result = null;

			if (!airlinePrefix.IsEmpty && !mawbNo.IsEmpty)
			{
				ZQuery query = GetJobMawbsFilterByAirlineAndMAWBNo(airlinePrefix, mawbNo);

				result = Factory.Load<JobMawb>(query).FirstOrDefault();
			}

			return result;
		}

		public JobMawb LoadByAirlineAndMawbNo(ZString airlinePrefix, ZString mawbNo, ZDateTime cutOffDate)
		{
			return LoadByAirlineAndMawbNo(airlinePrefix, mawbNo, cutOffDate, true);
		}

		public JobMawb LoadByAirlineAndMawbNo(ZString airlinePrefix, ZString mawbNo, ZDateTime cutOffDate, bool withinBranch)
		{
			JobMawb mawb = null;

			if (!airlinePrefix.IsEmpty && !mawbNo.IsEmpty)
			{
				ZQuery query = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, airlinePrefix);
				query.AddToFilter(JobMawbSchema.JM_MAWB, mawbNo);
				if (cutOffDate.IsValidSmallDateTime)
				{
					query.AddToFilter(JobMawbSchema.JM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, cutOffDate);
				}

				mawb = Factory.Load<JobMawb>(query).FirstOrDefault();
			}

			return mawb;
		}

		public JobMawb LoadFromParentPK(ZString parentTableCode, ZGuid parentID)
		{
			ZQuery query = new ZQuery(JobMawbSchema.JM_ParentTableCode, parentTableCode);
			query.AddToFilter(JoinCondition.And, JobMawbSchema.JM_ParentID, SQLComparisonOperator.Equal, parentID);

			JobMawb mawb = Factory.Load<JobMawb>(query).FirstOrDefault();
			if (mawb != null)
			{
				return LoadOrRefreshMAWB(mawb.PK);
			}
			else
			{
				return TryLoadFromParentPKFromDB(parentTableCode, parentID);
			}
		}

		public JobMawb LoadBOInTheDBFromParentPK(ZString parentTableCode, ZGuid parentID)
		{
			ZQuery query = new ZQuery(JobMawbSchema.JM_ParentTableCode, parentTableCode);
			query.AddToFilter(JoinCondition.And, JobMawbSchema.JM_ParentID, SQLComparisonOperator.Equal, parentID);

			JobMawb mawb = Factory.Load<JobMawb>(query).Where(x => x.IsInDatabase).FirstOrDefault();
			if (mawb != null)
			{
				return mawb;
			}
			else
			{
				return TryLoadFromParentPKFromDB(parentTableCode, parentID);
			}
		}

		JobMawb TryLoadFromParentPKFromDB(ZString parentTableCode, ZGuid parentID)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobMawb));
			query.AddToFilter(JobMawbSchema.JM_ParentTableCode, parentTableCode);
			query.AddToFilter(JoinCondition.And, JobMawbSchema.JM_ParentID, SQLComparisonOperator.Equal, parentID);
			query.FetchOnlyFromLocalCache = false;
			return Factory.Load<JobMawb>(query).FirstOrDefault();
		}

		public JobMawb LoadUnallocatedPrintedMawb(IStmNoteParent notesParent)
		{
			JobMawb mawb = null;

			StmNote note = notesParent != null ? GetUnallocatedPrintedMawbNote(notesParent) : null;

			if (note != null)
			{
				ZQuery query = new ZQuery(JobMawbSchema.PK, new ZGuid(note.ST_NoteData.ToAscii()));
				query.AddToFilter(JobMawbSchema.JM_ParentID, null);
				query.AddToFilter(JobMawbSchema.JM_ParentTableCode, ZString.Empty);
				query.AddToFilter(JobMawbSchema.JM_OH_AllocatedTo, null);
				query.AddToFilter(JobMawbSchema.JM_IsPaper, false);

				mawb = Factory.Load<JobMawb>(query).FirstOrDefault();
			}

			return mawb;
		}

		public JobMawb LoadOrRefreshMAWB(ZGuid mawbPK)
		{
			ZQuery cacheOnlyFilter = new ZQuery(JobMawbSchema.PK, mawbPK);
			cacheOnlyFilter.FetchOnlyFromLocalCache = true;
			JobMawb mawb = Factory.LoadTop1<JobMawb>(cacheOnlyFilter);

			if (mawb == null)
			{
				mawb = Factory.Load<JobMawb>(mawbPK);
			}
			else if (mawb.IsInDatabase)
			{
				mawb.Reload();
			}

			return mawb;
		}

		public void CreateOrUpdatePrintedMawbUnallocationNote(JobMawb mawb, IStmNoteParent notesParent)
		{
			if (mawb != null && notesParent != null)
			{
				var note = GetUnallocatedPrintedMawbNote(notesParent) ?? NewUnallocatedPrintedMawbNote(notesParent);
				note.ST_NoteData = ZBlob.FromAscii(mawb.PK.ToString());
				note.ST_NoteText = string.Format((NoResString)"MAWB {0}-{1} has been unallocated by {2} on {3}", // Audit info for support
					mawb.JM_Airline3DigitPrefix, mawb.JM_MAWB, Environment.Env.CurrentUser.FullName, ZDateTime.Now);
			}
		}

		StmNote GetUnallocatedPrintedMawbNote(IStmNoteParent notesParent)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, notesParent.NotesParentPK);
			query.AddToFilter(StmNoteSchema.ST_Description, PrintedMawbUnallocationInfo);

			var noteParentBizObj = notesParent as BusinessObject;
			if (noteParentBizObj != null)
			{
				query.FetchOnlyFromLocalCache = !noteParentBizObj.IsInDatabase;
			}

			return Factory.Load<HiddenStmNote>(query).FirstOrDefault();
		}

		StmNote NewUnallocatedPrintedMawbNote(IStmNoteParent notesParent)
		{
			var note = Factory.New<HiddenStmNote>();
			note.ST_ParentID = notesParent.NotesParentPK;
			note.ST_Table = notesParent.NotesParentTableName;
			note.ST_Description = PrintedMawbUnallocationInfo;
			note.ST_IsCustomDescription = true;

			return note;
		}

		public const string PrintedMawbUnallocationInfo = "PrintedMawbUnallocationInfo";

		#endregion

		#region Count

		public int NoJobMawbsAvailable(ZString airlineCode, ZString[] serviceLevels)
		{
			return NoJobMawbsAvailable(airlineCode, ZString.Empty, serviceLevels);
		}

		public int NoJobMawbsAvailable(ZString airlineCode, ZString mawbToExclude, ZString[] serviceLevels)
		{
			ZQuery query = GetUnusedJobMawbsFilterByAirlineAndServices(airlineCode, serviceLevels);

			if (mawbToExclude != ZString.Empty)
			{
				query.AddToFilter(JobMawbSchema.JM_MAWB, SQLComparisonOperator.NotEqual, mawbToExclude);
			}

			return Factory.GetDatabaseCount(typeof(JobMawb), query);
		}

		#endregion

		#region Query

		ZQuery GetJobMawbsFilterByAirlineAndMAWBNo(ZString airlinePrefix, ZString mawbNo)
		{
			ZQuery query = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, airlinePrefix);
			query.AddToFilter(JobMawbSchema.JM_MAWB, mawbNo);
			query.AddToFilter(GetCompanyAndBranchFilter(airlinePrefix));

			return query;
		}

		ZQuery GetUnusedJobMawbsFilterByAirlineAndServices(ZString airlinePrefix, params ZString[] serviceLevels)
		{
			ZQuery query = new ZDBOnlyQuery(typeof(JobMawb));
			query.ReLoadExistingRows = true;
			query.AddToFilter(JobMawbSchema.JM_Airline3DigitPrefix, airlinePrefix);
			query.AddToFilter(JobMawbSchema.JM_OH_AllocatedTo, null);
			query.AddToFilter(JobMawbSchema.JM_ParentTableCode, ZString.Empty);
			query.AddToFilter(JobMawbSchema.JM_ParentID, null);
			query.AddToFilter(JobMawbSchema.JM_IsPaper, false);
			query.AddToFilter(JobMawbSchema.JM_IsPrinted, false);
			query.AddToFilter(GetCompanyAndBranchFilter(airlinePrefix));

			if (serviceLevels != null)
			{
				query.AddToFilter(new ZQuery(JobMawbSchema.JM_ServiceLevel, SQLComparisonOperator.Equal, serviceLevels));
			}

			query.OrderBy = JobMawb.Schema.JM_MAWB;

			return query;
		}

		public ZQuery GetCompanyAndBranchFilter(ZString airlinePrefix)
		{
			var mawbStockSetting = mAWBStockManagementStrategy.GetBranchStrategy(GlbBranch.CurrentBranch, airlinePrefix);

			var query = new ZQuery();
			if (mawbStockSetting.UseBranchStock)
			{
				query.AddToFilter(new ZQuery(JobMawbSchema.JM_GB, GlbBranch.CurrentBranch.PK), JoinCondition.Or);
			}

			if (mawbStockSetting.UseCompanyStock)
			{
				var companyQuery = new ZQuery(JobMawbSchema.JM_GC_Company, GlbCompany.CurrentCompany.PK);
				companyQuery.AddToFilter(JobMawbSchema.JM_GB, null);
				query.AddToFilter(companyQuery, JoinCondition.Or);
			}

			if (mawbStockSetting.UseGlobalStock)
			{
				var globalQuery = new ZQuery(JobMawbSchema.JM_GC_Company, null);
				globalQuery.AddToFilter(JobMawbSchema.JM_GB, null);
				query.AddToFilter(globalQuery, JoinCondition.Or);
			}

			if (mawbStockSetting.UseOtherBranchStock)
			{
				var otherBranchQuery = new ZQuery(JobMawbSchema.JM_GB, GlbCompany.CurrentCompany.Branches.Select(branch => branch.PK).Except(GlbBranch.CurrentBranch.PK));
				query.AddToFilter(otherBranchQuery, JoinCondition.Or);
			}

			return query;
		}
		#endregion
	}

	[Serializable]
	public class MAWBAllocationException : ZCannotSaveException
	{
		public MAWBAllocationException(string reasonCantAllocate)
			: base(reasonCantAllocate, Res.GetString("195bab32-368a-4c63-85f1-02b212986ea2", "Error allocating Neutral MAWB"), ExceptionType.BusinessFailure)
		{
		}

#if NETFRAMEWORK
		protected MAWBAllocationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
