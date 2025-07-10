using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ServiceTasks;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	IdpUserMigrationServiceTask.Code,
	IdpUserMigrationServiceTask.Description,
	"ACC",
	typeof(IdpUserMigrationServiceTask),
	IsMandatory = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour",
	CanRunInAnyBranch = true,
	ActiveByDefault = true)
]

[assembly: HostedServiceQueueProvider(
	IdpUserMigrationServiceTask.Code,
	IdpUserMigrationServiceTask.Description,
	typeof(IdpUserMigrationServiceTask.QueueProvider))
]

namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	public class IdpUserMigrationServiceTask : ServiceProviderImpl
	{
		public const string Code = "IDP";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "IDP Import Service Task";

		public override void RunTask(CancellationToken cancellationToken)
		{
			if (!SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.Value)
			{
				ServiceLogger.Information($"Please enable registry {SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.GetLocation()} to run this task.");
				return;
			}

			Run(cancellationToken);
			ServiceLogger.Log(LogType.Information, $"{Description} finished.");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Migrated message")]
		void Run(CancellationToken cancellationToken)
		{
			const string migratedMessage = "Staff records migrated: {0}";
			var total = 0;
			var totalFailed = 0;
			var reader = GetNewReader();
			var batch = GetNextBatch(reader);

			if (!batch.Any())
			{
				ResetHighWatermarkToMinValue();
				reader = GetNewReader();
				batch = GetNextBatch(reader);
			}

			LogMigrationStart();

			while (batch.Any())
			{
				if (cancellationToken.IsCancellationRequested)
				{
					ServiceLogger.Log(LogType.Information, "Cancellation requested.");
					break;
				}

				var (filteredStaff, duplicateEmails) = FilterDuplicates(batch);

				if (duplicateEmails.Any())
				{
					ServiceLogger.Log(LogType.Warning,
						$"Duplicate emails ignored: {string.Join(", ", duplicateEmails)}");
				}

				var results = MigrateUsers(filteredStaff);

				if (!results.Any())
				{
					HandleNullResults(reader);
				}
				else
				{
					CheckMissingRecords(filteredStaff, results);

					var successUsers = ProcessResults(filteredStaff, results);
					var successCount = successUsers.Count();
					try
					{
						reader.FactoryProvider.SaveCurrentAndCreateNew();
						total += successCount;
						totalFailed += batch.Length - successCount;

						ServiceLogger.Log(LogType.Information, string.Format(migratedMessage, string.Join(", ", successUsers)));
					}
					catch (ZSaveException ex) when (!ex.IsCriticalException())
					{
						ServiceLogger.Log(LogType.Warning, "Unable to save a batch, saving one by one.");
						total += InsertManually(batch, successUsers, migratedMessage, ex);

						reader.FactoryProvider.CreateNewWithoutSave();
					}
				}

				var lastInBatch = batch[batch.Length - 1];
				SystemDataRegistry.Instance.IdpUserImportHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastInBatch.GS_SystemCreateTimeUtc.ToDateTime());

				batch = GetNextBatch(reader, lastInBatch);
			}

			ServiceLogger.Log(LogType.Information, $"Finishing migrating Staff records into Idp. Total records migrated: [{total}]. Total failed: [{totalFailed}].");
			ResetHighWatermarkToMinValue();
		}

		void ResetHighWatermarkToMinValue()
		{
			SystemDataRegistry.Instance.IdpUserImportHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Migrated message")]
		void LogMigrationStart()
		{
			var watermarkDate = new ZDateTime(SystemDataRegistry.Instance.IdpUserImportHighWaterMark.Value);
			var watermarkDateStr = watermarkDate.IsValid ? watermarkDate.ToBestReadableDateString() : "Min";
			ServiceLogger.Log(LogType.Information, $"Starting migration. Watermark: {watermarkDateStr}");
		}

		void HandleNullResults(FilteredBusinessObjectReader reader)
		{
			ServiceLogger.Log(LogType.Warning, "No results received");
			reader.FactoryProvider.CreateNewWithoutSave();
		}

		IEnumerable<string> ProcessResults(GlbStaff[] filteredStaff, IEnumerable<CreatedUserInfo> results)
		{
			var successUserEmails = new List<string>();

			var staffDict = filteredStaff.ToDictionary(s => s.GS_EmailAddress);

			foreach (var result in results)
			{
				if (result == null)
				{
					ServiceLogger.Log(LogType.Warning, "Received a null User");
					continue;
				}

				var email = result.Email ?? "null";

				if (!staffDict.TryGetValue(result.Email, out var staff))
				{
					ServiceLogger.Log(LogType.Warning, $"Unexpected user in Response: [{email}]");
					continue;
				}

				if (!string.IsNullOrEmpty(result.ErrorMessage))
				{
					ServiceLogger.Log(LogType.Warning, $"Error importing user [{email}]: {result.ErrorMessage}");
					staff.Logs.AddNew(AutoEvents.IDPImportFailure);
				}
				else
				{
					staff.Logs.AddNew(AutoEvents.IDPImport);
					successUserEmails.Add(staff.GS_EmailAddress);
				}
			}

			return successUserEmails;
		}

		(GlbStaff[] FilteredStaff, IEnumerable<string> DuplicateEmails) FilterDuplicates(GlbStaff[] staffList)
		{
			var groups = staffList
				.GroupBy(s => s.GS_EmailAddress)
				.ToList();

			var duplicates = groups
				.Where(g => g.Count() > 1)
				.Select(g => g.Key.ToString())
				.ToList();

			var filtered = groups
				.Where(g => g.Count() == 1)
				.SelectMany(g => g)
				.ToArray();

			return (filtered, duplicates);
		}

		void CheckMissingRecords(GlbStaff[] batch, IEnumerable<CreatedUserInfo> results)
		{
			var missing = batch.Cast<GlbStaff>().Where(s => !results.Any(r => r.Email == s.GS_EmailAddress));

			foreach (var staff in missing)
			{
				ServiceLogger.Log(LogType.Warning, $"User not found in Response: [{staff.GS_EmailAddress}], [{staff.PK}]");
				staff.Logs.AddNew(AutoEvents.IDPImportFailure);
			}
		}

		static GlbStaff[] GetNextBatch(FilteredBusinessObjectReader reader, BusinessObject? lastBizO = null)
		{
			return reader.LoadNextBatchInANewFactory(lastBizO).Cast<GlbStaff>().ToArray();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Should save regardless of validations")]
		int InsertManually(BusinessObject[] batch, IEnumerable<string> successUsers, string migratedMessage, ZSaveException ex)
		{
			var inserted = 0;
			var savedOneByOne = new List<string>();

			foreach (GlbStaff staff in batch)
			{
				try
				{
					InsertIdpLog(staff, successUsers.Contains(staff.GS_EmailAddress.ToString()));
				}
				catch (Exception logEx) when (!logEx.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Warning, $"Unable to insert a log: [{staff.GS_EmailAddress}], Error: {ex.Message}");
				}
			}

			ServiceLogger.Log(LogType.Information, string.Format(migratedMessage, string.Join(", ", savedOneByOne.ToArray())));
			return inserted;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Should save regardless of validations")]
		static void InsertIdpLog(GlbStaff staff, bool success)
		{
			using (var logCommand = Db.Connection.Command(@$"
insert into StmALog (SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
values
(newid(), '{GlbStaffSchema.Constants.TableName}', @PK, getdate(), @Event, @User) 
"))
			{
				logCommand.AddParameter("@PK", SqlDbType.UniqueIdentifier, staff.PK.ToGuid());
				logCommand.AddParameter("@Event", SqlDbType.VarChar, success ? AutoEvents.IDPImportCode : AutoEvents.IDPImportFailureCode);
				logCommand.AddParameter("@User", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code);
			}
		}

		FilteredBusinessObjectReader GetNewReader()
		{
			var reader = new FilteredBusinessObjectReader(GetStaffQuery(), typeof(GlbStaff));
			reader.BatchSize = 10;
			return reader;
		}

		protected virtual ZQuery GetStaffQuery()
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			query.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			query.AddToFilter(GlbStaffSchema.GS_IsRobot, false);
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			query.AddToFilter(GlbStaffSchema.GS_IsDevice, false);
			query.AddToFilter(GlbStaffSchema.GS_EmailAddress, SQLComparisonOperator.IsNotBlank, string.Empty);

			var watermark = SystemDataRegistry.Instance.IdpUserImportHighWaterMark.Value;
			if (watermark != DateTime.MinValue)
			{
				query.AddToFilter(GlbStaffSchema.GS_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, watermark);
			}

			var personSub = new ZDBOnlySubQuery(typeof(GlbPerson), GlbStaffSchema.GS_PER);
			personSub.AddToFilter(GlbPersonSchema.PER_IDPUserId, DBNull.Value);
			query.AddSubQuery(personSub, JoinCondition.And);

			var pendingLogSub = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);
			pendingLogSub.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.IDPImportCode);
			pendingLogSub.AddToFilter(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, GlbStaffSchema.PK);
			query.AddSubQuery(pendingLogSub, JoinCondition.And);

			query.OrderBy = GlbStaffSchema.Constants.GS_SystemCreateTimeUtc + " " + OrderByClause.Ascending;

			return query;
		}

		protected virtual IEnumerable<CreatedUserInfo> MigrateUsers(IEnumerable<GlbStaff> batch)
		{
			var idpUsers = GetIdpUsers(batch);

			try
			{
				return IdpApiClient.CreateUsersBulk(idpUsers);
			}
			catch (IdpCreateUsersException ex)
			{
				ServiceLogger.Log(LogType.Warning, ex.GetInnermostException().Message);
			}

			return Enumerable.Empty<CreatedUserInfo>();
		}

		protected IEnumerable<MigratedUser> GetIdpUsers(IEnumerable<GlbStaff> batch)
		{
			var idpUsers = new List<MigratedUser>();
			foreach (var staff in batch)
			{
				if (staff.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IDPImport.Code))
				{
					continue;
				}

				idpUsers.Add(new MigratedUser()
				{
					FullName = staff.GS_FullName,
					WorkEmail = staff.GS_EmailAddress,
					LoginName = staff.GS_LoginName,
					Active = staff.GS_IsActive
				});
			}

			return idpUsers;
		}

		public class QueueProvider : IHostedServiceQueueProvider
		{
			QueueResult IHostedServiceQueueProvider.QueueResult
			{
				get
				{
					if (!SystemDataRegistry.Instance.IdpUserSynchronisationEnabled.Value)
					{
						return QueueResult.Zero;
					}

					var sqlText = @"
SELECT
	COUNT(*),
	ISNULL(MAX(DATEDIFF(second, PER_SystemCreateTimeUtc, GetDate())), 0)
FROM GlbPerson
	join GlbStaff on GS_PER = PER_PK
where
	PER_IDPUserId is null
	and GS_EmailAddress <> ''
	and GS_IsSystemAccount = 0
	and GS_IsRobot = 0
	and GS_IsResource = 0
	and GS_IsDevice = 0
	and GS_PK not in
	(
		SELECT SL_Parent from StmALog where SL_Parent = GS_PK and SL_Table = 'GlbStaff' and SL_SE_NKEvent = 'IDI'
	)
";
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
					using (var cmd = Db.Connection.Command(sqlText))
					{
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								var count = reader.GetInt32(0);
								var age = TimeSpan.FromSeconds(reader.GetInt32(1));
								return new QueueResult(count, age);
							}
						}
					}
					return QueueResult.Error;
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
				}
			}
		}
	}
}
