using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Reflection;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.Tests
{
	[TestFixture]
	public class ConfigurationHelperTests
	{
		[Test]
		public void TestFindRecipient()
		{
			var pa1 = new SqlParameter();
			var pa2 = new SqlParameter();
			var mockParamCollection = MockRepository.GenerateMock<IDataParameterCollection>();
			mockParamCollection.Expect(_ => _.Add(pa1)).Return(1);
			mockParamCollection.Expect(_ => _.Add(pa2)).Return(1);

			var command = MockRepository.GenerateMock<IDbCommand>();
			command.Expect(_ => _.CreateParameter()).Return(pa1);
			command.Expect(_ => _.CreateParameter()).Return(pa2);
			command.Stub(_ => _.Parameters).Return(mockParamCollection);
			
			var connection = MockRepository.GenerateMock<IDbConnection>();
			command.Expect(_ => _.ExecuteScalar()).Return("PI2PUKTRN").Repeat.Once();
			connection.Stub(c => c.CreateCommand()).Return(command);

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();

			var testMessageTypes = new TestDbSet<eHubMessageType>
			{
				new eHubMessageType {DT_PK = new Guid("1FFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), DT_Code = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration"}
			};

			mockContext.Stub(_ => _.eHubMessageTypes).Return(testMessageTypes);

			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockLogger);
			mockConfigurationHelper.ContextFactory = () => mockContext;
			mockConfigurationHelper.Stub(_ => _.eHubTransactionsSecondary).Return(connection);
			
			var expectedRecipient = mockConfigurationHelper.FindRecipient("PI2TRN.GB333557108000.CDS");

			Assert.AreEqual("PI2PUKTRN", expectedRecipient);
		}

		[TestCase("HYECMT.GB333557108000.CDS", "HYEDUKCMT")]
		[TestCase("HYECM2.GB333557108000.CDS", "HYEDUKCM2")]
		[TestCase("WTLML5.GB333557108000.CDS", "WTLPUKML5")]
		[TestCase("EDIUAT.GB333557108000.CDS", "EDIEDIUAT")]
		[TestCase("WUTE8A.GB333557108000.CDS", "WUTDTOE8A")]
		public void TestFindRecipient_ShouldReturnInternalSystems(string naturalKey, string ehubClientID)
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockLogger);

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockConfigurationHelper.ContextFactory = () => mockContext;
			var testClients = new TestDbSet<eHubClient> {
				new eHubClient {CC_PK = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"), CC_ID = ehubClientID, CC_PermitInboxRecipient = true},
				new eHubClient {CC_PK = new Guid("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"), CC_ID = "PI2AAATRN", CC_PermitInboxRecipient = true},
				new eHubClient {CC_PK = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), CC_ID = "eHub"}
			};

			mockContext.Stub(_ => _.eHubClients).Return(testClients);
			
			var expectedRecipient = mockConfigurationHelper.FindRecipient(naturalKey);

			Assert.AreEqual(ehubClientID, expectedRecipient);
		}

		[Test]
		public void TestFindRecipient_SearchCacheFroRecipient()
		{
			var pa1 = new SqlParameter();
			var pa2 = new SqlParameter();
			var mockParamCollection = MockRepository.GenerateMock<IDataParameterCollection>();
			mockParamCollection.Expect(_ => _.Add(pa1)).Return(1);
			mockParamCollection.Expect(_ => _.Add(pa2)).Return(1);

			var command = MockRepository.GenerateMock<IDbCommand>();
			command.Expect(_ => _.CreateParameter()).Return(pa1);
			command.Expect(_ => _.CreateParameter()).Return(pa2);
			command.Stub(_ => _.Parameters).Return(mockParamCollection);

			var connection = MockRepository.GenerateMock<IDbConnection>();
			command.Expect(_ => _.ExecuteScalar()).Return("PI2PUKTRN").Repeat.Once();
			connection.Stub(c => c.CreateCommand()).Return(command);

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();

			var testMessageTypes = new TestDbSet<eHubMessageType>
			{
				new eHubMessageType {DT_PK = new Guid("1FFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), DT_Code = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration"}
			};

			mockContext.Stub(_ => _.eHubMessageTypes).Return(testMessageTypes);

			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockLogger);
			mockConfigurationHelper.ContextFactory = () => mockContext;
			mockConfigurationHelper.Stub(_ => _.eHubTransactionsSecondary).Return(connection);

			var expectedRecipient = mockConfigurationHelper.FindRecipient("PI2TRN.GB333557108000.CDS");
			var expectedCacheRecipient = mockConfigurationHelper.FindRecipient("PI2TRN.GB333557108000.CDS");

			Assert.AreEqual("PI2PUKTRN", expectedRecipient);
			Assert.AreEqual("PI2PUKTRN", MemoryCache.Default.Get("PI2TRN"));
			Assert.AreEqual(1, MemoryCache.Default.GetCount());
			Assert.AreEqual("PI2PUKTRN", expectedCacheRecipient);
		}

		[Test]
		public void TestFindRecipient_ThrowsException_WhenNoRecipientFound()
		{
			var parameters = MockRepository.GenerateMock<IDataParameterCollection>();

			var command = MockRepository.GenerateMock<IDbCommand>();
			command.Expect(_ => _.Parameters).Return(parameters).Repeat.Any();

			var connection = MockRepository.GenerateMock<IDbConnection>();
			command.Expect(_ => _.ExecuteScalar()).Return(null).Repeat.Once();
			connection.Stub(c => c.CreateCommand()).Return(command);

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();

			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockLogger);
			mockConfigurationHelper.ContextFactory = () => mockContext;
			mockConfigurationHelper.Stub(_ => _.eHubTransactionsSecondary).Return(connection);

			Assert.Throws<ArgumentException>(() => mockConfigurationHelper.FindRecipient("PI2TRN.GB333557108000.CDS"), "A recipient for the client system 'PI2TRN' cannot be found.");

			command.VerifyAllExpectations();
		}

		[Test]
		public void TestFindRecipientAppWideToken()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockLogger);

			var returnedRecipient = mockConfigurationHelper.FindRecipient("GBCTST.AppWide.CDS");
			Assert.AreEqual("", returnedRecipient);

			returnedRecipient = mockConfigurationHelper.FindRecipient("GBCPRD.AppWide.CDS");
			Assert.AreEqual("", returnedRecipient);

		}

		[Test]
		public void TestIsLicenceProductionAppWideToken()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockLogger);

			var returnedLicense = mockConfigurationHelper.IsLicenceProduction("GBCTST.AppWide.CDS");
			Assert.AreEqual(false, returnedLicense);

			returnedLicense = mockConfigurationHelper.IsLicenceProduction("GBCPRD.AppWide.CDS");
			Assert.AreEqual(true, returnedLicense);

		}

		static SqlException CreateSqlException(int infoNumber = 0, byte errorState = 0, byte errorClass = 0, string server = "", string errorMessage = "", string procedure = "", int lineNumber = 0)
		{
			var collection = typeof(SqlErrorCollection)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(new object[] { }) as SqlErrorCollection;

			var error = typeof(SqlError)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
				new[]
						{
							typeof (int), typeof (byte), typeof (byte), typeof (string), typeof(string), typeof (string),
							typeof (int)
						},
				null).Invoke(new object[] { infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber }) as SqlError;

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });


			var e = typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(SqlErrorCollection), typeof(string) }, null)
				.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;

			return e;
		}

		[TearDown]
		public void TearDown()
		{
			MemoryCache.Default.Trim(100);
		}
	}
}
