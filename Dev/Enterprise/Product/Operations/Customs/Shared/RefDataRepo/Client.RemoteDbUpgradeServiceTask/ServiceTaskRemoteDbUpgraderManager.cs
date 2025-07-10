using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using Enterprise.DbUpgrader.ReferenceDatabases;

namespace CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask
{
	public class ServiceTaskRemoteDbUpgraderManager
	{
		readonly IServerProxyHelper _serverProxyHelper;
		readonly IErrorReportingClientWrapper _errorReportingWrapper;
		readonly IEnumerable<string> serverList;
		protected ILogger logger;
#if DEBUG
		public virtual
#endif
		string refDatabaseName => RefDbTableNameResolver.SingleRefDatabaseName;

		public ServiceTaskRemoteDbUpgraderManager(Enterprise.Integration.ILogger logger, IErrorReportingClientWrapper errorReportingWrapper, IEnumerable<string> serverList)
		{
			this.logger = new ServiceLoggerWrapper(logger);
			_serverProxyHelper = new ServerProxyHelper(this.logger, RemoteDatabaseRegistry.Instance);
			_errorReportingWrapper = errorReportingWrapper;
			this.serverList = serverList;
		}

		public
#if DEBUG
		virtual
#endif
		bool Update()
		{
			try
			{
				var result = true;
				if (!_serverProxyHelper.CanInitiliseServerProxy())
				{
					return false;
				}

				result = UpdateAllServers();
				return result;
			}
			catch (Exception ex)
			{
				ReportError(ex);
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error logging")]
		void ReportError(Exception ex)
		{
			logger.WriteError(ex.ToString());
			if (_errorReportingWrapper.PostCrashReport(ex))
			{
				logger.WriteLine("Error report sent");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error logging")]
		bool UpdateAllServers()
		{
			var result = true;
			string primaryServerName = serverList.FirstOrDefault();
			try
			{
#if DEBUG
				logger.WriteLine($@"A more efficient way to update reference data is to go to Help -> Database Administration -> Reference Data.
Use this form can get reference data updated per dataset instead of download all data.");
#endif

				foreach (var server in serverList)
				{
					logger.WriteLine($"Start upgrading {refDatabaseName} schemas on server {server}.");

					logger.WriteLine("Machine Name: " + Environment.MachineName);

					using (var connection = Db.NewAdminConnection(server, Db.SqlMasterDb))
					{
#if DEBUG
						if (DataUtils.LoadDbExtendedProperty(connection, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName) == "Y")
						{
							logger.WriteError("SRDb is set to use DAT Snapshot, RDU won't run, please go to Testing -> Reset Single Reference Database -> Use Reference Service menu to switch");
							return false;
						}
#endif
						RefDatabaseInitialiser.CreateRefDbIfNotExists(connection, refDatabaseName);
						((ICurrentDbControl)connection).UseDatabase(refDatabaseName);
						var sRDbUpgrader = SetupUpgrader(((IDbConnectionInternals)connection).ADOConnection);

						var upgradeResult = false;
						try
						{
							upgradeResult = sRDbUpgrader.DoUpgrade(((IDbConnectionInternals)connection).ADOTransaction);
						}
						catch (Exception ex)
						{
							sRDbUpgrader.GetServerResponse();
							ReportError(ex);
#if DEBUG
							logger.WriteError("Please use Help -> Database Administraion -> Reference Data option and try again");
#endif
							logger.WriteError(string.Format(CultureInfo.InvariantCulture, "Can not upgrade SRDb because " + ex.ToString()));
						}

						result &= upgradeResult;
						if (upgradeResult)
						{
							logger.WriteLine($"Upgraded {refDatabaseName} on server {server} successfully.");
						}
						else
						{
							logger.WriteLine($"Upgraded {refDatabaseName} on server {server} failed.");
						}

						if (VerifyServerNeedRecreateSynonym(server, primaryServerName))
						{
							using (var mainConnection = Db.NewAdminConnection(server, refDatabaseName))
							{
								logger.WriteLine($"Begin synchronizing synonyms for {refDatabaseName}");
								var creator = new RefDatabaseSynonymRecreator(mainConnection);
								creator.RecreateSynonym();
								logger.WriteLine($"Finish synchronizing synonyms for {refDatabaseName}");
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				result = false;
				ReportError(exception);
			}
			return result;
		}

		#region Helper Methods

#if DEBUG
		protected virtual
#endif
		IRefDataBaseUpgrader SetupUpgrader(IDbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new RefDataBaseUpgrader(logger, _serverProxyHelper.GetServerProxy(), new DBUpgradeHelper(connection));
		}

		bool VerifyServerNeedRecreateSynonym(string server, string primaryServerName)
		{
			return (server == primaryServerName)
				|| (server != primaryServerName && !string.IsNullOrEmpty(server) && server == DbRegistry.BiDataWarehouseServer.LoadValue(Db.NewAdminConnection()));
		}
		#endregion
	}
}
