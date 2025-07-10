using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ILogger = CargoWise.RefDbRepo.Client.Common.ILogger;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public abstract class SharedDataSetUpdaterWrapper : Customs.Shared.ISharedDataSetUpdaterWrapper
	{
		public void SetLogger(Enterprise.Integration.ILogger logger)
		{
			Argument.NotNull(logger, nameof(logger));
			this.logger = new ServiceLoggerWrapper(logger);
		}

		public abstract IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetAllDataSetUpdater();

		public abstract bool Update(string tableName, string dataSet);

		public abstract bool UpdateAll();

		public virtual void LogSystemNotRegisteredInfo()
		{
			logger.WriteLine((NoResString)"Terminate because the system does not have a registration.");
			logger.WriteLine((NoResString)"Please register your system in order to use Reference Service.");
		}

		public virtual bool DbInitializationCheck()
		{
			return CheckDbConnection() && InitializeDb();
		}

		public virtual void Dispose()
		{
			logger = null;
		}

		public virtual int SavedDataSetCount => savedDataSetCount;

		#region Private/Protected Fields

		protected int savedDataSetCount;
		protected ILogger logger;

		protected Action<RefVersionControlManager> onInitializeDb;

		protected virtual string InitialDatabase => Db.DatabaseName;

		#endregion

		#region Helper Methods

		static bool CheckDbConnection()
		{
			return Db.Connection.State != ConnectionState.Broken && Db.Connection.State != ConnectionState.Closed;
		}

		bool InitializeDb()
		{
			using (var connection = Db.NewAdminConnection(InitialDatabase))
			{
				if (GetSqlConnection(connection, out var adoConnection))
				{
					var dbHelper = new DBHelper(adoConnection);
					var versionControl = new RefVersionControlManager(dbHelper);
					versionControl.IsMainDb = (InitialDatabase == Db.DatabaseName);
					onInitializeDb?.Invoke(versionControl);
					return true;
				}
				return false;
			}
		}

		bool GetSqlConnection(IDbConnectionInternals connection, out IDbConnection adoConnection)
		{
			Argument.NotNull(connection, nameof(connection));

			var connectedSuccessfully = false;
			adoConnection = null;
			try
			{
				adoConnection = connection.InternalDbConnection;
				connectedSuccessfully = true;
			}
			catch (SqlException ex)
			{
				logger.WriteError(ex.Message, ex);
			}
			return connectedSuccessfully;
		}

		#endregion
	}
}
