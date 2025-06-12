using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using CargoWise.Billing.CollectorService.Plugin;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.WindowsService.Common
{

	/// <summary>
	/// Provides the implementation of billing plugin that can retrieve billing information from a database using a single query.
	/// </summary>
	public abstract class SqlBillingTransactionsPlugin : AbstractPlugin
	{
		/// <summary>
		/// Returns a sequence of billing transactions with service time between <paramref name="start"/> and <paramref name="end"/>
		/// </summary>
		/// <param name="start">Minimal service time of transactions to return, exclusive.</param>
		/// <param name="end">Maximum service time of transactions to return, inclusive.</param>
		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
		{
			using (var connection = OpenConnection())
			using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
			using (var command = CreateCommand(connection, transaction, start, end))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					List<TimeStampedTransaction> recordTransactions;
					try
					{
						recordTransactions = CreateTransactions(reader).ToList();
					}
					catch (Exception e) when (logTransactionExceptions)
					{
						ErrorReportingClient?.ReportToIssueManager($"Could not create billing transactions from the data record [{DataRecordToString(reader)}]", e, Logger);
						continue;
					}

					foreach (var recordTransaction in recordTransactions)
					{
						yield return recordTransaction;
					}
				}
			}
		}

		/// <summary>
		/// Updates a sql connection string that is used to access database.
		/// </summary>
		/// <param name="settings">Generic plugin settings that contain ConnectionString parameter.</param>
		public override void UpdateSettings(PluginSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}
			ConnectionString = settings.Parameters.Single(p => p.Name == "ConnectionString").Value;

			var logTransactionSetting = settings.Parameters.SingleOrDefault(p => p.Name == "LogTransactionExceptions");
			if (logTransactionSetting != null)
			{
				logTransactionExceptions = ((IConvertible)logTransactionSetting.Value).ToBoolean(null);
			}
		}

		/// <summary>
		/// The full name of a manifest resource containing billing transactions query.
		/// </summary>
		protected abstract string QueryName { get; }

		/// <summary>
		/// Transforms a data record into a sequence of billing transactions.
		/// </summary>
		/// <param name="record">Data record selected from database using <see cref="Query"/></param>
		protected abstract IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record);

		/// <summary>
		/// Gets the string value of the specified field or null if it contains NULL.
		/// </summary>
		/// <param name="record">The record containing <paramref name="name"/> field.</param>
		/// <param name="name">The name of the field to find.</param>
		/// <param name="expectNull">Indicates whether NULL value is expected for this field.</param>
		protected string GetString(IDataRecord record, string name, bool expectNull = false)
		{
			var i = record.GetOrdinal(name);
			if (!record.IsDBNull(i)) return record.GetString(i);
			if (!expectNull)
			{
				ReportUnexpectedNull(record, name);
			}
			return null;
		}

		/// <summary>
		/// Gets the string value of the specified field or null if it contains NULL.
		/// </summary>
		/// <param name="record">The record containing <paramref name="name"/> field.</param>
		/// <param name="name">The name of the field to find.</param>
		/// <param name="expectNull">Indicates whether NULL value is expected for this field.</param>
		protected string GetValueAsString(IDataRecord record, string name, bool expectNull = false)
		{
			var i = record.GetOrdinal(name);
			if (!record.IsDBNull(i)) return decimal.Parse(record[name].ToString()).ToString();
			if (!expectNull)
			{
				ReportUnexpectedNull(record, name);
			}
			return null;
		}

		/// <summary>
		/// Gets the integer value of the specified field or 0 if it contains NULL.
		/// </summary>
		/// <param name="record">The record containing <paramref name="name"/> field.</param>
		/// <param name="name">The name of the field to find.</param>
		/// <param name="expectNull">Indicates whether NULL value is expected for this field.</param>
		protected int GetInt(IDataRecord record, string name, bool expectNull = false)
		{
			var i = record.GetOrdinal(name);
			if (!record.IsDBNull(i)) return record.GetInt32(i);
			if (!expectNull)
			{
				ReportUnexpectedNull(record, name);
			}
			return 0;
		}

		/// <summary>
		/// Gets the date and time data value of the specified field or empty <see cref="DateTime"/> if it contains NULL.
		/// </summary>
		/// <param name="record">The record containing <paramref name="name"/> field.</param>
		/// <param name="name">The name of the field to find.</param>
		/// <param name="expectNull">Indicates whether NULL value is expected for this field.</param>
		protected DateTime GetDateTime(IDataRecord record, string name, bool expectNull = false)
		{
			var i = record.GetOrdinal(name);
			if (!record.IsDBNull(i)) return record.GetDateTime(i);
			if (!expectNull)
			{
				ReportUnexpectedNull(record, name);
			}
			return default(DateTime);
		}

		protected string DecodeAndDecompress(string rawMessage)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawMessage)))
			{
				return stream.DecodeAndDecompress().ReadToEnd();
			}
		}

		protected XElement GetMessageElement(IDataRecord record, string columnName)
		{
			var messageContent = DecodeAndDecompress(GetString(record, columnName));
			return XElement.Parse(messageContent) ;
		}
		/// <summary>
		/// Gets the <see cref="Guid"/> data value of the specified field or empty <see cref="Guid"/> if it contains NULL.
		/// </summary>
		/// <param name="record">The record containing <paramref name="name"/> field.</param>
		/// <param name="name">The name of the field to find.</param>
		/// <param name="expectNull">Indicates whether NULL value is expected for this field.</param>
		protected Guid GetGuid(IDataRecord record, string name, bool expectNull = false)
		{
			var i = record.GetOrdinal(name);
			if (!record.IsDBNull(i)) return record.GetGuid(i);
			if (!expectNull)
			{
				ReportUnexpectedNull(record, name);
			}
			return default(Guid);
		}

		/// <summary>
		/// Gets the boolean value of the specified field or false if it contains NULL.
		/// </summary>
		/// <param name="record">The record containing <paramref name="name"/> field.</param>
		/// <param name="name">The name of the field to find.</param>
		/// <param name="expectNull">Indicates whether NULL value is expected for this field.</param>
		protected bool GetBool(IDataRecord record, string name, bool expectNull = false)
		{
			var i = record.GetOrdinal(name);
			if (!record.IsDBNull(i)) return record.GetBoolean(i);
			if (!expectNull)
			{
				ReportUnexpectedNull(record, name);
			}
			return false;
		}
	
		static void AddParameter(IDbCommand command, string name, DateTime value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = DbType.DateTime;
			parameter.Value = value;
			command.Parameters.Add(parameter);
		}

		protected  IDbConnection OpenConnection()
		{
			var connection = CreateConnection(ConnectionString);
			connection.Open();
			return connection;
		}

		protected IDbCommand CreateCommand(IDbConnection connection, IDbTransaction transaction, DateTime startUTC, DateTime endUTC)
		{
			var command = connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = Query;
			command.CommandTimeout = 0;
			AddParameter(command, "@startUTC", startUTC);
			AddParameter(command, "@endUTC", endUTC);
			return command;
		}

		string Query
		{
			get { return query ?? (query = GetQuery()); }
		}

		string GetQuery()
		{
			Stream stream = null;
			try
			{
				stream = QueryAssembly.GetManifestResourceStream(QueryName);
				using (var reader = new StreamReader(stream))
				{
					stream = null;
					return ModifyQuery(reader.ReadToEnd());
				}
			}
			finally
			{
				if (stream != null) stream.Dispose();
			}
		}

		void ReportUnexpectedNull(IDataRecord record, string name)
		{
			var error = "Unexpected NULL value ";
			ErrorReportingClient?.ReportToIssueManager(error + $"while running plugin [{PluginNamespace}]", new ArgumentException(error + $"in the field {name}: [{DataRecordToString(record)}]"), Logger, true);
		}

		 protected static string DataRecordToString(IDataRecord record)
		{
			return string.Join(", ", Enumerable.Range(0, record.FieldCount).Select(i => record.GetName(i) + ": " + record.GetValue(i)));
		}

		protected virtual string ModifyQuery(string input)
		{
			return input;
		}

		internal virtual IDbConnection CreateConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		internal virtual Assembly QueryAssembly
		{
			get { return GetType().Assembly; }
		}

		internal virtual string PluginNamespace => GetType().Namespace;

		string ConnectionString { get; set; }
		bool logTransactionExceptions = true;
		string query;
	}
}
