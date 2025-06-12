using CargoWise.eServices.Billing.DataAccess;
using CargoWise.eServices.TestHelpers.Database.Common;
using Common.Logging;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CargoWise.eServices.Billing.Tests.Common
{
    public static class BillingDataTestHelper
	{
		public static SqlConnection GetNewOpenConnection()
		{
			return SqlServerHelper.OpenAdminSqlConnection("BillingContext");
		}

		public static DataTable LoadTable(SqlConnection con, string query)
		{
			DataTable table = new DataTable();
			using (var adapter = new SqlDataAdapter(query, con))
			{
				adapter.Fill(table);
			}
			return table;
		}

		public static DataTable LoadAllStagingTransactionsMostRecentFirst(SqlConnection con)
		{
			return LoadTable(con, "select * from Staging order by TX_SystemCreateUTC desc");
		}

		public static DataTable LoadAllStagingTransactionsMostOccurrenceFirst(SqlConnection con)
		{
			return LoadTable(con, "select * from Staging order by TX_ServiceOccuredUTC desc");
		}

		public static DataTable LoadAllChargeableMostRecentFirst(SqlConnection con)
		{
			DataTable table = new DataTable();
			using (var adapter = new SqlDataAdapter("select * from edi.Chargeable order by CH_SystemCreateUTC desc", con))
			{
				adapter.Fill(table);
			}
			return table;
		}

		public static DataTable LoadAllChargeableOfCategory(SqlConnection con, string category)
		{
			DataTable table = new DataTable();
			using (var adapter = new SqlDataAdapter($"select * from edi.Chargeable WHERE CH_Category = '{category}' order by CH_SystemCreateUTC desc", con))
			{
				adapter.Fill(table);
			}
			return table;
		}

		public static DataTable LoadTableByName(SqlConnection con, string tableName)
		{
			return LoadTable(con, "select * from " + tableName);
		}

		public static void TruncateStagingTable(SqlConnection con)
		{
			ExecuteNonQuery(con, "truncate table dbo.Staging");
			ExecuteNonQuery(con, "truncate table edi.StagingBatch");
			ExecuteNonQuery(con, "truncate table edi.StagingUnknownSystems");
		}

		public static void TruncateTable(SqlConnection con, string tableName)
		{
			ExecuteNonQuery(con, "truncate table " + tableName);
		}

		public static void TruncateTableIfExists(SqlConnection con, string tableName)
		{
			ExecuteNonQuery(con, $@"IF OBJECT_ID('{tableName}') IS NOT NULL
begin
	truncate table {tableName}
end");
		}

		public static void AddTransaction(CargoWise.Billing.API.BillingTransaction transaction)
        {
            GetBillingRepository().Add(transaction);
        }

		public static void AddELKTransaction(string jsonData)
		{
			GetBillingRepository().InsertELKResubmitTransaction(jsonData);
		}

		public static void DeleteELKTransaction(string pk)
		{
			GetBillingRepository().DeleteELKTransaction(new Guid(pk));
		}

		public static DataTable SelectELKTransactions(int maxTransactions = 1000)
		{
			return GetBillingRepository().SelectELKTransaction(maxTransactions);
		}

		public static void AddTransactionRange(IEnumerable<CargoWise.Billing.API.BillingTransaction> transactions)
        {
            GetBillingRepository().AddRange(transactions);
        }

		public static int CountStaging()
		{
			return GetBillingRepository().CountStaging();
		}

		public static DateTime? OldestSystemCreateUTCInStaging()
		{
			return GetBillingRepository().OldestSystemCreateUTCInStaging();
		}

		public static BillingRepository GetBillingRepository(string connectionStringName = "BillingContext", ILog logger = null, bool logErrorsAsInfoMsgEvent = false, CancellationToken token = default)
		{
            return new BillingRepository(
                () => SqlServerHelper.GetAdminSqlConnection(connectionStringName), logger, logErrorsAsInfoMsgEvent, token);
		}

		public static void AssertUsagePropertiesAreEqual(object expectedOriginalProperties, int expectedDatabaseNumber, int expectedCompanyNumber, DataRow actual)
		{
			AssertPropertiesAreEqual(expectedOriginalProperties, actual, "US_");
			Assert.That(actual["US_DatabaseNumber"], Is.EqualTo(expectedDatabaseNumber));
			Assert.That(actual["US_CompanyNumber"], Is.EqualTo(expectedCompanyNumber));
			var serviceOccuredUTC = (DateTime)actual["US_ServiceOccuredUTC"];
			var expectedPeriod = GetChargeablePeriod(serviceOccuredUTC);
			Assert.That(actual["US_Period"], Is.EqualTo(expectedPeriod));
		}

		public static void AssertChargeablePropertiesAreEqual(object expectedOriginalProperties, int expectedDatabaseNumber, int expectedCompanyNumber, DataRow actual)
		{
			AssertPropertiesAreEqual(expectedOriginalProperties, actual, "CH_");
			Assert.That(actual["CH_DatabaseNumber"], Is.EqualTo(expectedDatabaseNumber));
			Assert.That(actual["CH_CompanyNumber"], Is.EqualTo(expectedCompanyNumber));
			var serviceOccuredUTC = (DateTime)actual["CH_ServiceOccuredUTC"];
			var expectedPeriod = GetChargeablePeriod(serviceOccuredUTC);
			Assert.That(actual["CH_Period"], Is.EqualTo(expectedPeriod));
		}

		public static void AssertPropertiesAreEqual(object expected, DataRow actual, string columnPrefix = "TX_", params string[] excludedColumnNameWithoutPrefixs)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var actualType = actual.GetType();
			foreach (var expectedProperty in expected.GetType().GetProperties(flags))
			{
				var categoryAttr = expectedProperty.GetCustomAttribute<System.ComponentModel.CategoryAttribute>();
				if (categoryAttr == null || categoryAttr.Category != "Non-billed")
				{
					var expectedPropertyName = expectedProperty.Name;
					if (excludedColumnNameWithoutPrefixs.Contains(expectedPropertyName)) continue;
					var columnName = columnPrefix + expectedPropertyName;
					var columnValue = actual[columnName];

                    var actualPropertyValue = columnValue == DBNull.Value ? null : columnValue;
					var expectedPropertyValue = expectedProperty.GetValue(expected);
					Assert.That(actualPropertyValue, Is.EqualTo(expectedPropertyValue), expectedPropertyName);
				}
			}
		}

		public static int ExecuteNonQuery(SqlConnection con, string sql)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();
			}
		}

		public static int ExecuteStoredProcedure(SqlConnection con, string sql)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = sql;
				return cmd.ExecuteNonQuery();
			}
		}

		public static void ClearDatabaseAndCompanyList(SqlConnection con)
		{
			BillingDataTestHelper.ExecuteNonQuery(con,
				@"delete from edi.ClientCompanyCodeHistory;
				delete from edi.ClientCompany;
				delete from edi.LicenceDatabaseCodeHistory;
				delete from edi.InternalLicenceDatabaseCodeHistory;");
		}

		public static void AddDatabaseAndCompany(SqlConnection con, int databaseNumber, int companyNumber, string companyCode, string countryCode = "AU", string serverCode = "SRV", string hostedLocation = "SYD", string enterpriseCode = "ENT", bool isActive = true, bool isTeardownInProgress = false, string licenceType = "PRD", string product = "")
		{
			BillingDataTestHelper.ExecuteNonQuery(con,
				@"INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, IsActive, IsTeardownInProgress, LicenceType, Product) " +
				@"	VALUES('" + Base27Encoding.Encode(databaseNumber) + @"', " + databaseNumber + @", '" + enterpriseCode + @"', '" + serverCode + @"', '2000-01-01', '" + hostedLocation + "', " + (isActive ? "1" : "0") + ", " + (isTeardownInProgress ? "1" : "0") + ", '" + licenceType + "', '" + product + "')" +
				@"INSERT INTO edi.ClientCompany(DatabaseNumber, CompanyNumber, LCC_PK, CountryCode, ValidFromUtc) " +
				@"	VALUES(" + databaseNumber + @", " + companyNumber + @", NEWID(), '" + countryCode + @"', '2000-01-01')" +
				@"INSERT INTO edi.ClientCompanyCodeHistory(DatabaseNumber, CompanyNumber, CompanyCode, ValidFromUtc) " +
				@"	VALUES(" + databaseNumber + @", " + companyNumber + @", '" + companyCode + @"', '2000-01-01')");
		}

		public static void AddOldDatabaseLocation(SqlConnection con, int databaseNumber, string oldHostedLocation, string enterpriseCode = "ENT", string serverCode = "SRV")
		{
			BillingDataTestHelper.ExecuteNonQuery(con,
				@"INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, ValidToUtc) " +
				@"	VALUES('" + Base27Encoding.Encode(databaseNumber) + @"', " + databaseNumber + @", 'ENT', '" + serverCode + @"', '1995-01-01', '" + oldHostedLocation + "', '2000-01-01')");
		}

		public static void AddDatabase(SqlConnection con, int databaseNumber, string serverCode = "SRV", string product = "CW1", string tenantId = "", string category = "")
		{
			BillingDataTestHelper.ExecuteNonQuery(con,
				@"INSERT INTO edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, Product, TenantId, Category) " +
				@"	VALUES('" + Base27Encoding.Encode(databaseNumber) + @"', " + databaseNumber + @", 'ENT', '" + serverCode + @"', '2000-01-01','" + product + "','" + tenantId + "','" + category + "')");
		}

		public static void AddCompany(SqlConnection con, int databaseNumber, int companyNumber, string companyCode, string countryCode = "AU", int validFromDayNumber = 1)
		{
			BillingDataTestHelper.ExecuteNonQuery(con,
				@"INSERT INTO edi.ClientCompany(DatabaseNumber, CompanyNumber, LCC_PK, CountryCode, ValidFromUtc) " +
				@"	VALUES(" + databaseNumber + @", " + companyNumber + @", NEWID(), '" + countryCode + @"', '2000-01-" + validFromDayNumber + "')" +
				@"INSERT INTO edi.ClientCompanyCodeHistory(DatabaseNumber, CompanyNumber, CompanyCode, ValidFromUtc) " +
				@"	VALUES(" + databaseNumber + @", " + companyNumber + @", '" + companyCode + @"', '2000-01-" + validFromDayNumber + "')");
		}

		public static void ExecuteUpdateChargeableAtTime(SqlConnection con, DateTime utcNow, int firstPeriodOfNewCollection = 202110, bool performMonthlyAggregation = true, bool isTestServer = false)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "edi.UpdateChargeable";
				cmd.Parameters.Add("@utcNow", SqlDbType.DateTime).Value = utcNow;
				cmd.Parameters.Add("@firstPeriodOfNewCollection", SqlDbType.Int).Value = firstPeriodOfNewCollection;
				cmd.Parameters.Add("@performMonthlyAggregation", SqlDbType.Bit).Value = performMonthlyAggregation ? 1 : 0;
				cmd.Parameters.Add("@isTestServer", SqlDbType.Bit).Value = isTestServer ? 1 : 0;
				cmd.Parameters.Add("@UpdateBillingCube", SqlDbType.Bit).Value = false;
				cmd.ExecuteNonQuery();
			}
		}

		public static void UpdateJobStateDateTime(SqlConnection con, string jobId, string stateName, DateTime createdAt)
		{
			ExecuteNonQuery(con,
				$@"UPDATE [Hangfire].[State]
					  SET CreatedAt = '{createdAt:yyyy-MM-dd HH:mm:ss}'
					  WHERE JobId = '{jobId}' AND [Name] = '{stateName}'");
		}

		public static void DeleteJobState(SqlConnection con, string jobId, string stateName = null)
		{
			ExecuteNonQuery(con, $@"DELETE FROM [Hangfire].[State] WHERE JobId = '{jobId}' {(string.IsNullOrEmpty(stateName) ? string.Empty : $"AND [Name] = '{stateName}'")}");
			if (string.IsNullOrEmpty(stateName))
			{
				ExecuteNonQuery(con, $@"DELETE FROM [Hangfire].[Job] WHERE [Id] = '{jobId}'");
			}
		}

		public static void ExecuteUpdateChargeableAfterPeriod(SqlConnection con, int chargeablePeriod, int daysAfter, bool performMonthlyAggregation = true, bool isTestServer = false)
		{
			ExecuteUpdateChargeableAtTime(con, GetChargeablePeriodAsDate(chargeablePeriod, daysAfter, DateTimeKind.Local).AddMonths(1), performMonthlyAggregation: performMonthlyAggregation, isTestServer: isTestServer);
		}

		public static void AddUserTransactions(SqlConnection con, int users, int chargeablePeriod, int databaseNumber, int companyNumber, string clientId)
		{
			if (users <= 0)
			{
				return;
			}

			var firstDayOfMonth = $"'{chargeablePeriod / 100}-{chargeablePeriod % 100}-01'";
			var scriptBuilder = new StringBuilder();
			scriptBuilder.AppendLine("INSERT INTO edi.Chargeable (CH_ID, CH_Period, CH_Category, CH_PriceItemCode, CH_BillableCount, CH_ReportingSource, CH_Reference1, CH_ServiceOccuredUtc, CH_ClientID, CH_DatabaseNumber, CH_CompanyNumber, CH_CapturedUtc) VALUES");
			for (int i = 0; i < users; ++i)
			{
				scriptBuilder.Append($"({i}, {chargeablePeriod}, 'STL', 'USR', 1, 'ENT', {i}, {firstDayOfMonth}, '{clientId}', {databaseNumber}, {companyNumber}, {firstDayOfMonth})");
				if (i < users - 1)
				{
					scriptBuilder.AppendLine(",");
				}
			}
			scriptBuilder.Append(";");

			ExecuteNonQuery(con, scriptBuilder.ToString());
		}

		public static int GetChargeablePeriod(DateTime utcTime)
		{
			var localTime = utcTime.ToLocalTime();
			return (localTime.Year * 100) + localTime.Month;
		}

		public static DateTime GetChargeablePeriodAsDate(int chargeablePeriod, int day, DateTimeKind dateTimeKind)
		{
			return new DateTime(chargeablePeriod / 100, chargeablePeriod % 100, day, 0, 0, 0, dateTimeKind);
		}

		public static CargoWise.Billing.API.BillingTransaction CreateAndAddTransaction(string priceItemCode, DateTime serviceOccuredUtc, string clientId = "ENT???SRV", string category = "STL", string reference1 = null, string reference2 = null, string reference3 = null, string reference4 = null, string reference5 = null)
		{
			var tran = CreateTransaction(priceItemCode, serviceOccuredUtc, clientId, category, reference1, reference2, reference3, reference4, reference5);
			AddTransaction(tran);
			return tran;
		}

		public static CargoWise.Billing.API.BillingTransaction CreateTransaction(string priceItemCode, DateTime serviceOccuredUtc, string clientId = "ENT???SRV", string category = "STL", string reference1 = null, string reference2 = null, string reference3 = null, string reference4 = null, string reference5 = null)
		{
			return new CargoWise.Billing.API.BillingTransaction
			{
				BillableCount = 1,
				ClientID = clientId,
				Category = category,
				PriceItemCode = priceItemCode,
				Reference1 = reference1,
				Reference2 = reference2,
				Reference3 = reference3,
				Reference4 = reference4,
				Reference5 = reference5,
				ReportingSource = "ENT",
				ServiceOccuredUTC = serviceOccuredUtc,
				Version = 1,
			};
		}

		public static void SetUpBillingRules(SqlConnection con)
		{
			ExecuteNonQuery(con, @"
INSERT edi.BillingRules(BR_SPName, BR_Fields, BR_Value1, BR_Value2, BR_Value3)
SELECT *
FROM (VALUES
	('ProcessUsageData', 'Category-PriceItemCode', 'WGR', 'WGR', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'STL', 'WTP', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'STL', 'TX1', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'STL', 'TX2', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'TW1', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'IN1', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'IT1', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'HU1', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'WS1', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'TW2', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'IN2', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'IT2', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'HU2', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'ACC', 'WS2', ''),
	('ProcessUsageData', 'Category-PriceItemCode', 'RIM', 'RIM', ''),
	('ProcessUsageData', 'Category-PriceItemCode-Reference3', 'RIM', 'RIC', 'CLU'),
	('ProcessUsageData', 'Category-PriceItemCode-Reference4', 'STL', 'AM2', 'HVL'),
	('ProcessUsageData', 'Category-PriceItemCode-Reference4', 'STL', 'IS2', 'HVL'),
	('ProcessUsageData', 'Category-PriceItemCode-ClientID', 'TST', 'TST', ''),
	('ProcessUsageData', 'Category-PriceItemCode-ClientID', '', '', 'INZ%'),
	('ProcessUsageData', 'Category-PriceItemCode-ClientID', '', '', 'CEW%'),
	('ProcessUsageData', 'Category-PriceItemCode-ClientID', '', '', 'WPC%'),
	('ProcessUsageData', 'Category-PriceItemCode-ClientID', 'WGR', 'WGR', 'XXX???YYY'),
	('ProcessUsageData', 'Category-PriceItemCode-ClientID', 'WGR', 'WGR', 'ead???ers'),
	('ProcessUsageData', 'Category-Reference4', 'DOS', 'Fail', ''),
	('ProcessUsageData', 'ClientID', 'PROES_EAD', '', ''),
	('ProcessUsageData', 'ClientID', 'ENVAS_E1D', '', ''),
	('ProcessUsageData', 'ClientID', 'TSTTSTTST', '', ''),
	('ProcessUsageData', 'ClientID', 'UATUATUAT', '', ''),
	('ProcessUsageData', 'ClientID', 'PLKES_EAD', '', ''),
	('ProcessUsageData', 'ClientID', 'B92ME_EAD', '', ''),
	('ProcessUsageData', 'ClientID', '?????????', '', ''),
	('ProcessCMP', 'Category-PriceItemCode', 'CMP', 'CMP', ''),
	('ProcessCMP', 'Category-PriceItemCode', 'CMP', 'SCO', ''),
	('ProcessCMP', 'Category-PriceItemCode', 'CMP', 'SCS', ''),
	('ProcessSelfHostedUsageData', 'Category-PriceItemCode', 'STL', 'PRS', ''),
	('ProcessSelfHostedUsageData', 'Category-PriceItemCode', 'STL', 'PRT', ''),
	('ProcessSelfHostedUsageData', 'Category-PriceItemCode', 'STL', 'STS', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Category-PriceItemCode', 'STL', 'TX1', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'HU', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'IT', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'IN', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'WS', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'VN', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'TW', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'FJ', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'EG', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'CN', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'SA', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'RO', '', ''),
	('ProduceAccountsReceivableMonthlyChargeables', 'Reference3', 'AR', '', ''),
	('ProduceAccountsPayableMonthlyChargeables', 'Category-PriceItemCode', 'STL', 'TX2', ''),
	('ProduceAccountsPayableMonthlyChargeables', 'Reference3', 'ES', '', ''),
	('ProduceAccountsPayableMonthlyChargeables', 'Reference3', 'TR', '', '')
)AS Rules(SPName, Fields, Value1, Value2, Value3)
WHERE NOT EXISTS (SELECT 1 FROM edi.BillingRules b
	WHERE Rules.SPName = b.BR_SPName
		AND Rules.Fields = b.BR_Fields
		AND (Rules.Value1 = b.BR_Value1 OR Rules.Value1 IS NULL AND b.BR_Value1 IS NULL)
		AND (Rules.Value2 = b.BR_Value2 OR Rules.Value2 IS NULL AND b.BR_Value2 IS NULL)
		AND (Rules.Value3 = b.BR_Value3 OR Rules.Value3 IS NULL AND b.BR_Value3 IS NULL))
");
		}
	}

	/// <summary>
	/// Encode a binary number to letters and digit characters.
	/// Not case sensitive.
	/// Omits vowels to avoid actual words.
	/// Omits digits 0, 1 for similarity to vowels o, i.
	/// Omits digit 8 for similarity to letter B.
	/// Omits letter L for similarity to letter i.
	/// </summary>
	public static class Base27Encoding
	{
		const string EncodingCharSet = "BCDFGHJKMNPQRSTVWXYZ2345679";
		const int N = 27;
		const int MaxEncodedLength = 7; // maximum integer (2^31 - 1) encodes to 7 characters

		public static string Encode(int number)
		{
			if (number < 0)
			{
				throw new ArgumentOutOfRangeException("number", "cannot be negative");
			}

			var buffer = new char[MaxEncodedLength];
			int startIndex = MaxEncodedLength;
			do
			{
				buffer[--startIndex] = EncodingCharSet[number % N];
				number /= N;
			} while (number > 0 && startIndex > 0);

			return new string(buffer, startIndex, MaxEncodedLength - startIndex);
		}

		public static bool TryDecode(string text, out int number)
		{
			number = 0;
			bool result = true;
			if (text != null && text.Length > 0 && text.Length <= MaxEncodedLength)
			{
				int working = 0;
				for (int i = 0; i < text.Length; ++i)
				{
					int digit = DecodeLookupTable.Lookup(text[i]);
					if (digit == -1)
					{
						working = 0;
						result = false;
						break;
					}

					working = working * N + digit;
				}
				number = working;
			}
			else
			{
				result = false;
			}

			return result;
		}

		public static int Decode(string text)
		{
			int result;
			if (!TryDecode(text, out result))
			{
				throw new ArgumentException("invalid text: {" + text + "}");
			}

			return result;
		}

		static class DecodeLookupTable
		{
			static sbyte[] table = InitTable();
			const char FirstChar = '0';
			const char LastChar = 'z';

			static sbyte[] InitTable()
			{
				var result = new sbyte[LastChar - FirstChar + 1];
				for (int i = 0; i < result.Length; ++i)
				{
					result[i] = -1;
				}

				for (sbyte i = 0; i < EncodingCharSet.Length; ++i)
				{
					char c = EncodingCharSet[i];
					result[c - FirstChar] = i;
					if (c >= 'A')
					{
						// lower case
						result[c - FirstChar + 'a' - 'A'] = i;
					}
				}

				// 8 -> B
				result['8' - FirstChar] = result['B' - FirstChar];
				return result;
			}

			public static int Lookup(char c)
			{
				if (c >= FirstChar && c - FirstChar < table.Length)
				{
					return table[c - FirstChar];
				}
				return -1;
			}
		}
	}
}
