using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.CollectorService.Plugin.Tests;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests
{
	[TestFixture]
	public abstract class SqlBillingTransactionsPluginTest<TPlugin> : AbstractPluginTest<TPlugin>
		where TPlugin : SqlBillingTransactionsPlugin, new()
	{
		[Test]
		public void TestGetTransactions()
		{
			var reader = MockIDataReader(QueryColumns, QueryResult);
			mockCommand.Setup(_ => _.ExecuteReader()).Returns(reader).Verifiable();

			var transactions = mockPlugin.Object.GetTransactions(start, end).ToArray();
			AssertTransactions(transactions);
			if (SendUsageTransaction)
			{
				AssertUsageTransactions(transactions);
			}
			AssertQuery(mockCommand.Object.CommandText);
			AssertLogs();

			mockParameters.Verify();
			mockCommand.Verify();
			mockConnection.Verify();
			mockPlugin.Verify(_ => _.CreateConnection("Test Connection String"), Times.AtLeastOnce);
			mockPlugin.Verify(_ => _.QueryAssembly, Times.AtLeastOnce);
		}

		protected abstract string[] QueryColumns { get; }

		protected virtual string Log4NetFilename => "SqlBillingTransactionsPluginTest.log4net.config";

		protected abstract IReadOnlyList<object[]> QueryResult { get; }

		protected abstract void AssertTransactions(TimeStampedTransaction[] transactions);

		protected virtual void AssertUsageTransactions(TimeStampedTransaction[] transactions) => throw new NotImplementedException();

		protected virtual void AssertLogs()
		{
		}

		protected void AssertTransaction(TimeStampedTransaction transaction,
			DateTime timeStamp,
			int billableCount,
			string clientID,
			string clientNumber,
			string clientStaffCode,
			string category,
			string priceItemCode,
			string reference1,
			string reference2,
			string reference3,
			string reference4,
			string reference5,
			string reportingSource,
			DateTime serviceOccuredUTC,
			string MessageTrackingID)
		{
			Assert.That(transaction.BillingTransaction.BillableCount, Is.EqualTo(billableCount));
			Assert.That(transaction.BillingTransaction.ClientID, Is.EqualTo(clientID));
			Assert.That(transaction.BillingTransaction.ClientNumber, Is.EqualTo(clientNumber));
			Assert.That(transaction.BillingTransaction.ClientStaffCode, Is.EqualTo(clientStaffCode));
			Assert.That(transaction.BillingTransaction.Category, Is.EqualTo(category));
			Assert.That(transaction.BillingTransaction.PriceItemCode, Is.EqualTo(priceItemCode));
			Assert.That(transaction.BillingTransaction.Reference1, Is.EqualTo(reference1));
			Assert.That(transaction.BillingTransaction.Reference2, Is.EqualTo(reference2));
			Assert.That(transaction.BillingTransaction.Reference3, Is.EqualTo(reference3));
			Assert.That(transaction.BillingTransaction.Reference4, Is.EqualTo(reference4));
			Assert.That(transaction.BillingTransaction.Reference5, Is.EqualTo(reference5));
			Assert.That(transaction.BillingTransaction.ReportingSource, Is.EqualTo(reportingSource));
			Assert.That(transaction.BillingTransaction.ServiceOccuredUTC, Is.EqualTo(serviceOccuredUTC));
			Assert.That(transaction.BillingTransaction.MessageTrackingID, Is.EqualTo(MessageTrackingID));
			Assert.That(transaction.TimeStamp, Is.EqualTo(timeStamp));
		}

		protected void AssertUsageTransaction(TimeStampedTransaction transaction,
			DateTime timeStamp,
			int usageCount,
			string usageCode,
			string enterpriseCode,
			string companyCode,
			string serverCode,
			string companyName,
			string branchCode,
			string environment,
			string additionalRefs,
			DateTime serviceOccuredUTC)
		{
			Assert.That(transaction.UsageTransaction.UsageCode, Is.EqualTo(usageCode));
			Assert.That(transaction.UsageTransaction.EnterpriseCode, Is.EqualTo(enterpriseCode));
			Assert.That(transaction.UsageTransaction.CompanyCode, Is.EqualTo(companyCode));
			Assert.That(transaction.UsageTransaction.ServerCode, Is.EqualTo(serverCode));
			Assert.That(transaction.UsageTransaction.CompanyName, Is.EqualTo(companyName));
			Assert.That(transaction.UsageTransaction.BranchCode, Is.EqualTo(branchCode));
			Assert.That(transaction.UsageTransaction.UsageCount, Is.EqualTo(usageCount));
			Assert.That(transaction.UsageTransaction.AdditionalRefs, Is.EqualTo(additionalRefs));
			Assert.That(transaction.UsageTransaction.ServiceOccuredUTC, Is.EqualTo(serviceOccuredUTC));
			Assert.That(transaction.UsageTransaction.Environment, Is.EqualTo(environment));
			Assert.That(transaction.TimeStamp, Is.EqualTo(timeStamp));
		}

		protected IDataReader MockIDataReader(string[] columns, IReadOnlyList<object[]> data)
		{
			int count = -1;
			var mockDataReader = new Mock<IDataReader>();
			mockDataReader
				.Setup(_ => _.Read())
				.Returns(() => count < data.Count - 1)
				.Callback(() => count++);

			mockDataReader
					.Setup(x => x[It.IsAny<string>()])
					.Returns((string columnName) => {
						var index = Array.FindIndex(columns, column => column == columnName);
						return data[count][index];
					});

			mockDataReader
					.Setup(_ => _.GetOrdinal(It.IsAny<string>()))
					.Returns((string name) => Array.FindIndex(columns, column => column == name));

			mockDataReader
				.Setup(_ => _.IsDBNull(It.IsAny<int>()))
				.Returns((int i) => data[count][i] == DBNull.Value);
			mockDataReader
				.Setup(_ => _.GetString(It.IsAny<int>()))
				.Returns((int i) => (string)data[count][i]);
			mockDataReader
				.Setup(_ => _.GetDateTime(It.IsAny<int>()))
				.Returns((int i) => (DateTime)data[count][i]);
			mockDataReader
				.Setup(_ => _.GetGuid(It.IsAny<int>()))
				.Returns((int i) => (Guid)data[count][i]);
			mockDataReader
				.Setup(_ => _.GetInt32(It.IsAny<int>()))
				.Returns((int i) => (int)data[count][i]);
			mockDataReader
				.Setup(_ => _.GetBoolean(It.IsAny<int>()))
				.Returns((int i) => (bool)data[count][i]);
			mockDataReader
				.Setup(_ => _.FieldCount)
				.Returns(columns.Length);
			mockDataReader
				.Setup(_ => _.GetName(It.IsAny<int>()))
				.Returns((int i) => columns[i]);
			mockDataReader
				.Setup(_ => _.GetValue(It.IsAny<int>()))
				.Returns((int i) => data[count][i]);

			return mockDataReader.Object;
		}

		protected virtual void AssertQuery(string query)
		{
			Assert.That(query.IndexOf("@startUTC", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
			Assert.That(query.IndexOf("@endUTC", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
		}

		[SetUp]
		public void Setup() => SetUpInternal();

		internal protected virtual void SetUpInternal()
		{
			mockLogger = new Mock<ILogger>();
			start = new DateTime(2014, 9, 30, 23, 9, 16);
			end = new DateTime(2014, 9, 8, 13, 37, 10);

			var mockTransaction = new Mock<IDbTransaction>();
			mockParameters = new Mock<IDataParameterCollection>();
			mockParameters.Setup(_ => _.Add(It.Is<IDataParameter>(p => p.ParameterName == "@startUTC" && p.DbType == DbType.DateTime && start.Equals(p.Value)))).Verifiable();
			mockParameters.Setup(_ => _.Add(It.Is<IDataParameter>(p => p.ParameterName == "@endUTC" && p.DbType == DbType.DateTime && end.Equals(p.Value)))).Verifiable();
			mockCommand = new Mock<IDbCommand>(MockBehavior.Strict) { DefaultValue = DefaultValue.Mock };
			mockCommand.SetupProperty(_ => _.CommandText);
			mockCommand.SetupSet(_ => _.Transaction = mockTransaction.Object);
			mockCommand.SetupSet(_ => _.CommandTimeout = 0);
			mockCommand.Setup(_ => _.Parameters).Returns(mockParameters.Object);
			mockCommand.Setup(_ => _.CreateParameter()).Returns(() => new SqlParameter());
			mockCommand.Setup(_ => _.Dispose());
			mockConnection = new Mock<IDbConnection>();
			mockConnection.Setup(_ => _.CreateCommand()).Returns(mockCommand.Object).Verifiable();
			mockConnection.Setup(_ => _.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(mockTransaction.Object).Verifiable();
			mockPlugin = new Mock<TPlugin> { CallBase = true };
			mockPlugin.Setup(_ => _.CreateConnection("Test Connection String")).Returns(mockConnection.Object);
			mockPlugin.Setup(_ => _.QueryAssembly).Returns(typeof(TPlugin).Assembly);
			errorReportingClientMock = new Mock<IErrorReportingClient>();
			errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
			loggerFactory = loggerFactory ?? new LoggerFactory().AddLog4Net(Log4NetFilename);
			mockPlugin.Object.SetLoggerFactory(loggerFactory);

			errorReportingClientMock = new Mock<IErrorReportingClient>();
			errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("http://abc.com"));
			errorReportingClientMock.Setup(_ => _.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
							.Callback((IOpaqueErrorReport errBuilder, CancellationToken cancellationToken) =>
							{
								var reportBuilder = errBuilder as EnterpriseErrorReportBuilder;
								using (var memStream = new MemoryStream())
								{
									var task = reportBuilder.WriteToAsync(memStream);
									task.ConfigureAwait(false);
									task.Wait();
									memStream.Seek(0, SeekOrigin.Begin);
									using (var str = new StreamReader(memStream))
									{
										var t2 = str.ReadToEndAsync();
										t2.ConfigureAwait(false);
										t2.Wait();
										issueDescriptions.Add(t2.Result);
									}
								}
							}).Returns(Task.CompletedTask);

			mockPlugin.Object.SetErrorReportingClient(errorReportingClientMock.Object);

			mockPlugin.Object.UpdateSettings(new PluginSettings { Parameters = new[] { new PluginParameter("ConnectionString", "Test Connection String") } });
		}

		[TearDown]
		public void TearDown() => TearDownInternal();

		internal protected virtual void TearDownInternal()
		{
			issueDescriptions.Clear();
		}

		protected DateTime start;
		protected DateTime end;
		protected Mock<IDataParameterCollection> mockParameters;
		protected Mock<IDbCommand> mockCommand;
		protected Mock<IDbConnection> mockConnection;
		protected Mock<TPlugin> mockPlugin;
		protected Mock<ILogger> mockLogger;
		protected List<string> issueDescriptions = new List<string>();
		protected virtual bool SendUsageTransaction => false;
		protected ILoggerFactory loggerFactory;
		protected Mock<IErrorReportingClient> errorReportingClientMock;
	}
}
