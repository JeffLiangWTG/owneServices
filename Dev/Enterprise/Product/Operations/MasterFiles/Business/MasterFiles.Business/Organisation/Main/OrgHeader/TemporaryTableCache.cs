using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	public class TemporaryTableCache : Disposable
	{
		protected readonly DbConnection connection;
		readonly string temporaryTableName;
		readonly string createTableQuery;
		readonly string dropTableQuery;
		readonly int createCacheTimeoutMinutes;
		readonly ZSqlParameterCollection tableContentsQueryParameters;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public TemporaryTableCache(DbConnection connection, string tableContentsQuery, string temporaryTableName, int createCacheTimeoutMinutes = 5)
			: this(connection, temporaryTableName, GetCreateTableQuery(tableContentsQuery, temporaryTableName), string.Empty, createCacheTimeoutMinutes)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public TemporaryTableCache(DbConnection connection, string tableContentsQuery, ZSqlParameterCollection tableContentsQueryParameters, string temporaryTableName, int createCacheTimeoutMinutes = 5)
			: this(connection, temporaryTableName, GetCreateTableQuery(tableContentsQuery, temporaryTableName), string.Empty, createCacheTimeoutMinutes)
		{
			this.tableContentsQueryParameters = tableContentsQueryParameters;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public TemporaryTableCache(DbConnection connection, string temporaryTableName, string createTableQuery, string dropTableQuery, int createCacheTimeoutMinutes = 5)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(createTableQuery, nameof(createTableQuery));
			if (!IsValidTableName(temporaryTableName))
			{
				throw new ArgumentException("Invalid argument.", nameof(temporaryTableName));
			}

			this.connection = connection;
			this.temporaryTableName = temporaryTableName;
			this.createTableQuery = createTableQuery;
			this.dropTableQuery = String.IsNullOrWhiteSpace(dropTableQuery) ? GetDropTableQuery(temporaryTableName) : dropTableQuery;
			this.createCacheTimeoutMinutes = createCacheTimeoutMinutes;
		}

		public static bool IsValidTableName(string tableName)
		{
			return Regex.IsMatch(tableName, "^#\\w+$");
		}

		public void EnsureCacheTableCreated()
		{
			CreateTableIfNeeded();
		}

		public int RowCount
		{
			get
			{
				CreateTableIfNeeded();

				using (var command = connection.Command("SELECT COUNT(*) FROM " + temporaryTableName))
				{
					return (int)command.ExecuteScalar();
				}
			}
		}

		bool tableHasBeenCreated;
		void CreateTableIfNeeded()
		{
			if (!tableHasBeenCreated)
			{
				CreateTableIfNeededCore();
				tableHasBeenCreated = true;
			}
		}

		protected virtual void CreateTableIfNeededCore()
		{
			using (var command = connection.Command(createTableQuery, createCacheTimeoutMinutes * 60))
			{
				if (tableContentsQueryParameters != null)
				{
					command.AddParameters(tableContentsQueryParameters.ToArray());
				}
				command.ExecuteNonQuery();
			}
		}

		public DynamicBusinessObjectCollection Load(string query, ZSqlParameterCollection parameters = null)
		{
			CreateTableIfNeeded();

			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory(connection));
			collection.Load(query.Replace(TableNamePlaceholder, temporaryTableName), parameters ?? new ZSqlParameterCollection());

			return collection;
		}

		#region SQL Query Strings
		#region SuppressResourceStringsCheckRegion

		const string TableNamePlaceholder = "<tablename>";

		static string GetCreateTableQuery(string tableContentsQuery, string temporaryTableName)
		{
			return String.Format(CultureInfo.InvariantCulture, "SELECT * INTO {0} FROM ({1}) subQueryName;", temporaryTableName, tableContentsQuery);
		}

		static string GetDropTableQuery(string temporaryTableName)
		{
			return String.Format(CultureInfo.InvariantCulture, "DROP TABLE {0};", temporaryTableName);
		}

		#endregion
		#endregion

		protected override void Dispose(bool isDisposing)
		{
			if (connection.State != System.Data.ConnectionState.Open)
			{
				return;
			}

			try
			{
				using (var command = connection.Command(dropTableQuery))
				{
					command.ExecuteNonQuery();
				}
			}
			catch (SqlException)
			{
				//An exception likely means the connection is dead, and/or the table no longer exists
			}
		}
	}
}
