using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.DbBackup.Engine;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	partial class SqlAlwaysOnAutomation : ISqlAlwaysOnAutomation
	{
		public SqlAlwaysOnAutomation(object logger)
			: this()
		{
			Logger = (ILogger)logger;
		}

		public SqlAlwaysOnAutomation()
		{
			InitialiseAttributes(out this.availabilityGroup, out this.secondaryServers);
		}

		#region Backup Database

		/// <summary>
		/// 1. Backup Database
		/// </summary>
		public void BackupNewDatabase(string newDbName)
		{
			try
			{
				BackupNewDatabaseUnsafe(newDbName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger?.Warning(Res.GetString("97bd9ef7-21f8-47cc-93ed-163135e73eac",
					"Backup of a newly created database [{0}] failed\r\n{1}"
					, newDbName, ex.Message));

				Globals.Message.ShowDeveloperErrorAlways(ex.Message, Res.GetString("0f3d17c1-d42d-4842-b3af-7410741c0c25", "Backup of a newly created database {0} failed", newDbName));
			}
		}

		void BackupNewDatabaseUnsafe(string newDbName)
		{
			string errorMessage = null;

			if (string.IsNullOrWhiteSpace(dbBackupDirectory))
			{
				errorMessage = Res.GetString("60c1c143-f4fe-4e9e-aa85-01e8b8d8cd7e", "New database created {0}. Please include it in your database maintenance plan, unless backups are performed by {1} Service Tasks.", newDbName, BrandingFactory.Instance.ProductName);
			}
			else
			{
				var backupLogger = new DbManagerLogger();

				using (var connection = Db.NewAdminConnection())
				{
					new DbMaintenanceController(dbBackupDirectory, new EmailNotificationSender(), backupLogger).PerformFullBackupForSingleDatabase(connection, newDbName);

					if (RefDbTableNameResolver.IsSharedDatabase(newDbName))
					{
						BackupLog(connection, newDbName, backupLogger);
					}
				}

				if (backupLogger.Errors.Count > 0)
				{
					errorMessage = string.Join("\r\n", backupLogger.Errors.ToArray());
				}
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Logger?.Warning(errorMessage);
				SendEmailNotification(Res.GetString("5e236031-aff5-4761-9c73-34f033046ba8", "Backup report {0}", newDbName), errorMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void BackupLog(AdminConnection connection, string dbName, ILogger logger)
		{
			try
			{
				var utcNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
				var backupFileName = $"{dbName}_{utcNow}.trn";

				connection.ExecuteNonQuery("ep_BackupDb"
					, (cmd) =>
					{
						cmd.CommandType = System.Data.CommandType.StoredProcedure;
						cmd.AddParameter("@DbName", System.Data.SqlDbType.VarChar, 128, dbName);
						cmd.AddParameter("@FolderPath", System.Data.SqlDbType.VarChar, 800, dbBackupDirectory);
						cmd.AddParameter("@FileName", System.Data.SqlDbType.VarChar, 200, backupFileName);
						cmd.AddParameter("@BkpType", System.Data.SqlDbType.VarChar, 20, "LOG");
						cmd.AddParameter("@IsCompressed", System.Data.SqlDbType.Bit, true);
					});
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Warning(Res.GetString("9a97224f-88e5-4730-b554-570757341519",
					"Failed to backup log for database [{0}] to backup folder [{1}]\r\n{2}"
					, dbName, dbBackupDirectory, ex.Message));
			}
		}

		readonly string dbBackupDirectory = Env.Registry.BackupDirectoryPath;

		#region DbManagerLogger

		class DbManagerLogger : ILogger
		{
			List<string> errors;
			public List<string> Errors => errors ?? (errors = new List<string>());

			public void Log(LogType type, string message, Exception ex)
			{
				if (type == LogType.Error)
				{
					Errors.Add(type.ToString() + ": " + message + (ex != null ? (NoResString)"\r\n\r\nException:\r\n" + ex.Message : ""));
				}
			}

			public void Log(LogType type, string message)
			{
				Log(type, message, null);
			}
		}

		#endregion // DbManagerLogger

		#endregion // Backup Database

		#region Add Database to Existing Always On Availability Group

		public void AddDatabaseToAlwaysOnGroup(string newDbName)
		{
			if (!string.IsNullOrWhiteSpace(availabilityGroup))
			{
				if (dbBackupDirectory.StartsWith(@"\\"))
				{
					AddDbToPrimaryAndSecondaryServerGroups(newDbName);
				}
				else
				{
					var message = Res.GetString("481d91f8-4145-4df5-8151-eb18d00a34ca",
						"Backup path must be a UNC so it can be consistently referenced by the primary and all secondary servers. Current backup path is\r\n{0}."
						, dbBackupDirectory);

					Logger?.Warning(message);
					SendEmailNotification(
						AlwaysOnNotificationSubject + " - " + newDbName,
						message);
				}
			}
		}

		void AddDbToPrimaryAndSecondaryServerGroups(string newDbName)
		{
			try
			{
				AddDbToPrimaryServerGroup(newDbName);
				if (AlwaysOn.IsDbPartOfAlwaysOn(Db.Connection, newDbName))
				{
					AddDatabaseToSecondaryServers(newDbName);
				}
			}
			catch (SqlException ex)
			{
				if (new DbErrorMatch(ex).ExceptionType != DbErrorType.DatabaseAlreadyJoinedToAvailabilityGroup)
				{
					Logger?.Warning(Res.GetString("97a4d8e6-553f-4778-848c-ff3b303b496a",
						"Failed to add database [{0}] to Availability Group [{1}]\r\n{2}"
						, newDbName, availabilityGroup, ex.Message));

					SendFailedToAddDatabaseToAGEmailNotification(ex, newDbName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger?.Warning(Res.GetString("87a007f1-3275-4e60-964c-1e814a41c4c0",
					"Failed to add database [{0}] to availability group [{1}]\r\n{2}"
					, newDbName, availabilityGroup, ex.Message));

				SendFailedToAddDatabaseToAGEmailNotification(ex, newDbName);
			}
		}

		void SendFailedToAddDatabaseToAGEmailNotification(Exception ex, string dbName)
		{
			SendEmailNotification(
				AlwaysOnNotificationSubject + " - " + dbName,
				Res.GetString("aebd6793-aa63-46ec-b438-13e7900e87aa",
					"Failed to add new database to availability group [{0}].\r\n\r\n{1}\r\n\r\nStack trace:\r\n{2}",
					availabilityGroup, ex.ToString(), System.Environment.StackTrace)
			);
		}

		/// <summary>
		/// 2. Add Database to availability group in the PRIMARY server
		/// </summary>
		protected virtual void AddDbToPrimaryServerGroup(string newDbName)
		{
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				// Add to availability group on primary
				string sqlText = $"ALTER AVAILABILITY GROUP [{availabilityGroup}] ADD DATABASE [{newDbName}]";
				adminConnection.ExecuteNonQuery(sqlText);
			}
		}

		#region Secondary Server

		/// <summary>
		/// 3. After adding a database to an availability group, you need to
		/// configure the corresponding secondary database on each server instance that hosts a secondary replica.
		///   - initialise database on the secondary server by restoring its latest full backup
		///   - and add it to the same availability group on the secondary server
		/// </summary>
		protected virtual void AddDatabaseToSecondaryServers(string newDbName)
		{
			var secServerWorker = new SqlAlwaysOnSecondaryServerWorker(availabilityGroup, newDbName);

			Parallel.ForEach(secondaryServers, secondaryServerName =>
			{
				secServerWorker.AddDbToGivenSecondaryServer(secondaryServerName);
			});

			foreach (var secServerException in secServerWorker.SecondaryServerExceptions)
			{
				var message = Res.GetString("7795c0aa-6e3d-4ab2-bee0-5884514cfe8d",
					"Failed to add new database to secondary server [{0}].\r\n{1}",
					secServerException.Key, secServerException.Value.Message);

				Logger?.Warning(message);
				SendEmailNotification(
					AlwaysOnNotificationSubject + " - " + newDbName,
					message
				);
			}
		}

		#endregion

		internal void InitialiseAttributes(out string primaryAvailabilityGroup, out string[] secondaryReplicaServers)
		{
			primaryAvailabilityGroup = null;
			var secondaryServerList = new List<string>();

			if (Db.Connection.ServerEdition == DbConnection.SqlServerEdition.EnterpriseDeveloper)
			{
				string sqlText = string.Format(@"
					SELECT
						ag.name AS GroupName,
						secReplica.replica_server_name AS SecondaryServer
					FROM
						sys.availability_groups ag
						INNER JOIN sys.dm_hadr_availability_replica_states priReplica
							ON priReplica.group_id = ag.group_id AND priReplica.role = 1 AND priReplica.is_local = 1
						INNER JOIN sys.databases priDb ON priDb.replica_id = priReplica.replica_id
						LEFT JOIN
						(
							sys.availability_replicas secReplica
							INNER JOIN sys.dm_hadr_availability_replica_states secReplicaState ON secReplicaState.replica_id = secReplica.replica_id
						) ON secReplica.group_id = ag.group_id AND secReplicaState.role = 2
					WHERE
						priDb.name = '{0}'",
					Db.DatabaseName);

				using (var adminConnection = Db.NewAdminConnection())
				using (var cmd = adminConnection.Command(sqlText))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						if (primaryAvailabilityGroup == null)
						{
							primaryAvailabilityGroup = reader[0].ToString();
						}

						if (reader[1] != DBNull.Value)
						{
							secondaryServerList.Add(DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(reader[1].ToString()));
						}
					}
				}
			}

			secondaryReplicaServers = secondaryServerList.ToArray();
		}

		protected string availabilityGroup;
		readonly string[] secondaryServers;

		#endregion // Add Database to Existing Always On Availability Group

		internal void SendEmailNotification(string emailSubject, string messageBody)
		{
#if DEBUG
			if (Globals.IsTest && SkipSendEmailNotification_ForTest)
			{
				return;
			}
#endif

			try
			{
				StringCollection groupEmails = new EmailGroupUtility().SendNotificationToDatabaseAdministrator(NotificationDataRegistry.Instance.DatabaseBackupAndMaintenanceNotificationGroup.Value, false);

				if (groupEmails.Count > 0)
				{
					EmailDef notificationEmail = new EmailDef();
					notificationEmail.AddRecipientForUserCommunication(groupEmails);
					notificationEmail.Body = messageBody;
					notificationEmail.Subject = emailSubject;

					Env.OutgoingMailManager.CreateAndSave(notificationEmail);
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSaveToPostmasterGroup(new EmailDef() { Body = messageBody, Subject = emailSubject });
				}
			}
			catch (EmailSendFailedException)
			{
				// It's the client's resposibility to configure email.
			}
		}

		string AlwaysOnNotificationSubject => Res.GetString("2dd75266-457b-4d2f-ab4f-4d58b1e08689", "AlwaysOn configuration");

		ILogger Logger { get; }
	}
}

#region Test
#if DEBUG

#region Partial class

namespace Enterprise.MasterFiles.Business
{
	partial class SqlAlwaysOnAutomation
	{
		internal bool SkipSendEmailNotification_ForTest { get; set; }
	}
}

#endregion // Partial class

#endif
#endregion
