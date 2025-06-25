using Common.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using ValidationException = CargoWise.Billing.API.ValidationException;

namespace CargoWise.eServices.Billing.DataAccess
{
	public sealed class BillingRepository : IBillingRepository
	{
		readonly Func<SqlConnection> sqlConnectionFactory;

		bool logErrorsAsInfoMsgEvent;

		public BillingRepository(ILog logger = null, bool logErrorsAsInfoMsgEvent = false, CancellationToken cancellationToken = default)
			: this(null, logger, logErrorsAsInfoMsgEvent, cancellationToken)
		{
		}

		public BillingRepository(Func<SqlConnection> sqlConnectionFactory, ILog logger, bool logErrorsAsInfoMsgEvent, CancellationToken cancellationToken)
		{
			this.sqlConnectionFactory = sqlConnectionFactory ?? (() => new SqlConnection(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString));
			Logger = logger;
			this.logErrorsAsInfoMsgEvent = logErrorsAsInfoMsgEvent;
			this.cancellationToken = cancellationToken;
		}

		public void Add(CargoWise.Billing.API.BillingTransaction transaction)
		{
			CargoWise.Billing.API.BillingTransactionValidator.ValidateTransaction(transaction);

			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "BillingTransactionInsert";
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.Parameters.Add("@PriceItemCode", SqlDbType.VarChar, 3).Value = transaction.PriceItemCode;
					cmd.Parameters.Add("@BillableCount", SqlDbType.Int, 3).Value = transaction.BillableCount;
					cmd.Parameters.Add("@ReportingSource", SqlDbType.VarChar, 3).Value = transaction.ReportingSource;
					cmd.Parameters.Add("@ServiceOccuredUTC", SqlDbType.DateTime2).Value = transaction.ServiceOccuredUTC;
					cmd.Parameters.Add("@ClientID", SqlDbType.VarChar, 9).Value = transaction.ClientID;
					cmd.Parameters.Add("@ClientNumber", SqlDbType.VarChar, 50).Value = transaction.ClientNumber ?? (object)DBNull.Value;
					cmd.Parameters.Add("@ClientStaffCode", SqlDbType.VarChar, 3).Value = transaction.ClientStaffCode ?? (object)DBNull.Value;
					cmd.Parameters.Add("@Version", SqlDbType.Int).Value = transaction.Version;
					cmd.Parameters.Add("@Category", SqlDbType.VarChar, 3).Value = transaction.Category;
					cmd.Parameters.Add("@Branch", SqlDbType.VarChar, 3).Value = transaction.Branch ?? (object)DBNull.Value;
					cmd.Parameters.Add("@Reference1", SqlDbType.VarChar, 50).Value = transaction.Reference1 ?? "";
					cmd.Parameters.Add("@Reference2", SqlDbType.VarChar, 50).Value = transaction.Reference2 ?? (object)DBNull.Value;
					cmd.Parameters.Add("@Reference3", SqlDbType.VarChar, 50).Value = transaction.Reference3 ?? (object)DBNull.Value;
					cmd.Parameters.Add("@Reference4", SqlDbType.VarChar, 50).Value = transaction.Reference4 ?? (object)DBNull.Value;
					cmd.Parameters.Add("@Reference5", SqlDbType.VarChar, 50).Value = transaction.Reference5 ?? (object)DBNull.Value;
					cmd.Parameters.Add("@MessageTrackingID", SqlDbType.VarChar, 36).Value = transaction.MessageTrackingID ?? (object)DBNull.Value;

					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Add multiple transactions atomically.
		/// The records will end up with same creation time and will be processed together.
		/// Should be used when the transactions are extracted from a single message.
		/// </summary>
		public void AddRange(IEnumerable<CargoWise.Billing.API.BillingTransaction> transactions)
		{

			CargoWise.Billing.API.BillingTransactionValidator.ValidateTransactions(transactions);

			DataTable table = CreateBillingTransactionTableType();
			foreach (var transaction in transactions)
			{
				var row = table.NewRow();
				row["Category"] = transaction.Category;
				row["PriceItemCode"] = transaction.PriceItemCode;
				row["BillableCount"] = transaction.BillableCount;
				row["ReportingSource"] = transaction.ReportingSource;
				row["ServiceOccuredUTC"] = transaction.ServiceOccuredUTC;
				row["ClientID"] = transaction.ClientID;
				row["ClientNumber"] = transaction.ClientNumber ?? (object)DBNull.Value;
				row["ClientStaffCode"] = transaction.ClientStaffCode ?? (object)DBNull.Value;
				row["Reference1"] = transaction.Reference1;
				row["Reference2"] = transaction.Reference2 ?? (object)DBNull.Value;
				row["Reference3"] = transaction.Reference3 ?? (object)DBNull.Value;
				row["Reference4"] = transaction.Reference4 ?? (object)DBNull.Value;
				row["Reference5"] = transaction.Reference5 ?? (object)DBNull.Value;
				row["Version"] = transaction.Version;
				row["Branch"] = transaction.Branch ?? (object)DBNull.Value;
				row["MessageTrackingID"] = transaction.MessageTrackingID ?? (object)DBNull.Value;
				table.Rows.Add(row);
			}

			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "dbo.BillingTransactionInsertMany";
					cmd.CommandType = CommandType.StoredProcedure;
					SqlParameter parameter = cmd.Parameters.AddWithValue("@Transactions", table);
					parameter.SqlDbType = SqlDbType.Structured;
					parameter.TypeName = "dbo.TVP_BillingTransaction";
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void InsertELKResubmitTransactions(IEnumerable<string> transactions)
		{
			DataTable table = CreateUsageTransactionTableType();
			foreach (var transaction in transactions)
			{
				var row = table.NewRow();
				row["RT_PK"] = Guid.NewGuid();
				row["RT_JsonData"] = transaction;
				table.Rows.Add(row);
			}

			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "dbo.ELKResubmitTransactionInsertMany";
					cmd.CommandType = CommandType.StoredProcedure;
					SqlParameter parameter = cmd.Parameters.AddWithValue("@Transactions", table);
					parameter.SqlDbType = SqlDbType.Structured;
					parameter.TypeName = "dbo.TVP_UsageTransaction";
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void InsertELKResubmitTransaction(string transactionJson)
		{
			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "ELKResubmitTransactionInsert";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("@jsonData", SqlDbType.VarChar).Value = transactionJson;
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void DeleteELKTransaction(Guid transactionPk)
		{
			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "DELETE FROM ELKResubmitTransaction WHERE RT_PK = @pk";
					cmd.Parameters.Add("@pk", SqlDbType.UniqueIdentifier).Value = transactionPk;
					cmd.ExecuteNonQuery();
				}
			}
		}

		public DataTable SelectELKTransaction(int maxTransactions)
		{
			var table = new DataTable();
			using (var adapter = new SqlDataAdapter($"SELECT TOP {maxTransactions} RT_PK, RT_JsonData FROM ELKResubmitTransaction ORDER BY RT_SystemCreateTimeUtc ASC", sqlConnectionFactory.Invoke()))
			{
				adapter.Fill(table);
				return table;
			}
		}

                public bool DoesELKBacklogExceedThreshold(int threshold)
                {
                        using (var con = sqlConnectionFactory.Invoke())
                        {
                                con.Open();
                                using (var cmd = con.CreateCommand())
                                {
                                        cmd.CommandText = "SELECT CASE WHEN COUNT(RT_PK) > @threshold THEN 'TRUE' ELSE 'FALSE' END FROM ELKResubmitTransaction";
                                        cmd.Parameters.Add("@threshold", SqlDbType.Int).Value = threshold;
                                        return (bool.Parse(cmd.ExecuteScalar().ToString()));
                                }
                        }
                }

                public IEnumerable<CargoWise.Billing.API.LicenseInfo> GetLatestLicenses()
                {
                        var result = new List<CargoWise.Billing.API.LicenseInfo>();
                        using (var con = sqlConnectionFactory.Invoke())
                        {
                                con.Open();
                                using (var cmd = con.CreateCommand())
                                {
                                        cmd.CommandText = "SELECT EnterpriseCode, DatabaseNumber, ServerCode, HostedLocation, IsActive, IsTeardownInProgress FROM dbo.GetLatestLicense()";
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                                while (reader.Read())
                                                {
                                                        result.Add(new CargoWise.Billing.API.LicenseInfo
                                                        {
                                                                EnterpriseCode = reader["EnterpriseCode"].ToString(),
                                                                DatabaseNumber = Convert.ToInt32(reader["DatabaseNumber"]),
                                                                ServerCode = reader["ServerCode"].ToString(),
                                                                HostedLocation = reader["HostedLocation"].ToString(),
                                                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                                                IsTeardownInProgress = Convert.ToBoolean(reader["IsTeardownInProgress"])
                                                        });
                                                }
                                        }
                                }
                        }

                        return result;
                }

		static DataTable CreateBillingTransactionTableType()
		{
			DataTable table = new DataTable();
			table.Columns.Add("Category", typeof(string));
			table.Columns.Add("PriceItemCode", typeof(string));
			table.Columns.Add("BillableCount", typeof(int));
			table.Columns.Add("ReportingSource", typeof(string));
			table.Columns.Add("ServiceOccuredUTC", typeof(DateTime));
			table.Columns.Add("ClientID", typeof(string));
			table.Columns.Add("ClientNumber", typeof(string));
			table.Columns.Add("ClientStaffCode", typeof(string));
			table.Columns.Add("Reference1", typeof(string));
			table.Columns.Add("Reference2", typeof(string));
			table.Columns.Add("Reference3", typeof(string));
			table.Columns.Add("Reference4", typeof(string));
			table.Columns.Add("Reference5", typeof(string));
			table.Columns.Add("Version", typeof(int));
			table.Columns.Add("Branch", typeof(string));
			table.Columns.Add("MessageTrackingID", typeof(string));
			return table;
		}

		static DataTable CreateUsageTransactionTableType()
		{
			DataTable table = new DataTable();
			table.Columns.Add("RT_PK", typeof(Guid));
			table.Columns.Add("RT_JsonData", typeof(string));
			return table;
		}

		public void UpdateChargeable(DateTime? utcNow = null, int firstPeriodOfNewCollection = 202110, bool performMonthlyAggregation = true, bool updateBillingCube = false) => Execute((con) => UpdateChargeable(con, utcNow, firstPeriodOfNewCollection, performMonthlyAggregation, updateBillingCube));

		SqlCommand UpdateChargeable(SqlConnection con, DateTime? utcNow, int firstPeriodOfNewCollection, bool performMonthlyAggregation, bool updateBillingCube)
		{
			var cmd = con.CreateCommand();
			cmd.CommandTimeout = 0;
			cmd.Parameters.Add("@utcNow", SqlDbType.DateTime2).Value = utcNow ?? (object)DBNull.Value;
			cmd.Parameters.Add("@firstPeriodOfNewCollection", SqlDbType.Int, 6).Value = firstPeriodOfNewCollection;
			cmd.Parameters.Add("@performMonthlyAggregation", SqlDbType.Bit).Value = performMonthlyAggregation;
			cmd.Parameters.Add("@isTestServer", SqlDbType.Bit).Value = new SqlConnectionStringBuilder(con.ConnectionString).ApplicationName.Equals("TestBillingWebService");
			cmd.Parameters.Add("@SyncLicences", SqlDbType.Bit).Value = false;
			cmd.Parameters.Add("@UpdateBillingCube", SqlDbType.Bit).Value = updateBillingCube;
			cmd.CommandText = $"EXECUTE edi.UpdateChargeable {string.Join(", ", cmd.Parameters.Cast<SqlParameter>().Select(p => $"{p.ParameterName}={p.ParameterName}"))} WITH RECOMPILE";
			return cmd;
		}

		void Execute(Func<SqlConnection, SqlCommand> procedure)
		{
			List<SqlError> sqlErrors = new List<SqlError>();

			using (var con = sqlConnectionFactory.Invoke())
			{
				if (Logger != null)
				{
					con.FireInfoMessageEventOnUserErrors = logErrorsAsInfoMsgEvent;
					con.InfoMessage += (sender, e) =>
					{
						foreach (SqlError error in e.Errors)
						{
							if (!con.FireInfoMessageEventOnUserErrors || error.Class <= 10)
							{
								Logger.Info(error.Message);
							}
							else
							{
								sqlErrors.Add(error);
							}
						}
					};
				}

				con.Open();
				using (var cmd = procedure(con))
				{
					if (cancellationToken != default)
					{
						cancellationToken.Register(() =>
						{
							if (cmd != null && cmd.Connection.State == ConnectionState.Open)
							{
								cmd.Cancel();
							}
							Logger.Info("Cancellation requested by user");
						});
					}

					cmd.ExecuteNonQuery();
				}
			}

			if (sqlErrors != null && sqlErrors.Any())
			{
				throw new InvalidOperationException(string.Join(Environment.NewLine, sqlErrors));
			}
		}

		public void SPExecution(SPExecutionParam param) => Execute((con) => SPExecution(con, param));

		SqlCommand SPExecution(SqlConnection con, SPExecutionParam executionParam)
		{
			var cmd = con.CreateCommand();
			cmd.CommandTimeout = executionParam.TimeoutInSeconds;
			cmd.CommandText = executionParam.Name;
			cmd.CommandType = CommandType.StoredProcedure;
			foreach (var param in executionParam.SqlParams)
			{
				cmd.Parameters.Add(new SqlParameter(param.Name, param.DbType) { Value = param.Value });
			}

			return cmd;
		}

		public int CountStaging()
		{
			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "SELECT COUNT(*) FROM dbo.Staging";
					return Convert.ToInt32(cmd.ExecuteScalar());
				}
			}
		}

		public DateTime? OldestSystemCreateUTCInStaging()
		{
			using (var con = sqlConnectionFactory.Invoke())
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "SELECT MIN(TX_SystemCreateUtc) FROM dbo.Staging";
					var result = cmd.ExecuteScalar();

					if (result == DBNull.Value)
					{
						return null;
					}
					else
					{
						return Convert.ToDateTime(result);
					}
				}
			}
		}

		readonly ILog Logger;
		CancellationToken cancellationToken;
	}
}
