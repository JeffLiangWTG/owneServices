using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Reflection;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.DataModel.Tests.Accessors
{
	[TestClass]
	public class eHubTransactionsAccessorTests
	{
		[TestMethod]
		public void Accessors_eHubTransactionsAccessor_GetCodeMappedValue()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;

			mockeHubTransactionsContext.Expect(x => x.SqlQuery<string>(@"EXEC [dbo].[GetRecipientCode] 
@senderClientCode = @p0, 
@recipientClientCode = @p1, 
@transformationName = @p2, 
@codeSetName = @p3, 
@resultField = @p4, 
@key1Value = @p5, 
@key2Value = @p6, 
@key3Value = @p7, 
@key4Value = @p8, 
@key5Value = @p9", "SENDER", "RECIPIENT", "TRANSFORM", "CODESET", "RESULT", "KEY1", null, null, null, null)).Return(new[] { "RESULT" });
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var result = eHubTransactionsAccessor.GetCodeMappedValue_WithRetries("SENDER", "RECIPIENT", "TRANSFORM", "CODESET", "RESULT", "KEY1", null, null, null, null, new NoOpLogger());

			mockeHubTransactionsContext.VerifyAllExpectations();
			Assert.AreEqual("RESULT", result);
		}

		[TestMethod]
		public void Accessors_eHubTransactionsAccessor_GetCounterValue()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;

			mockeHubTransactionsContext.Expect(x => x.SqlQuery<string>("DECLARE @value varchar(20); EXEC [dbo].[GetCounterValue] @name = @p0, @padlength = @p1, @value = @value OUTPUT; SELECT @value;", "COUNTER", 4)).Return(new[] { "0001" });
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var result = eHubTransactionsAccessor.GetCounterValue_WithRetries("COUNTER", 4, new NoOpLogger());

			mockeHubTransactionsContext.VerifyAllExpectations();
			Assert.AreEqual("0001", result);
		}

		[TestMethod]
		public void Accessors_eHubTransactionsAccessor_GetCounterInterfaceValue()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;

			mockeHubTransactionsContext.Expect(x => x.SqlQuery<string>(@"DECLARE @startValue bigint; 
EXEC [dbo].[GetCounterInterfaceValue] 
@TransformatonSetName = @p0, 
@Name = @p1, 
@MaxValue = @p2, 
@IncrementValue = @p3, 
@StartValue = @startValue OUTPUT; 
SELECT CAST(@startValue as varchar(max));", "Import Notification", "COUNTER", Convert.ToInt64(9999), 1)).Return(new[] { "1" });
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var result = eHubTransactionsAccessor.GetCounterInterfaceValue_WithRetries("Import Notification", "COUNTER", 9999, 1, new NoOpLogger());

			mockeHubTransactionsContext.VerifyAllExpectations();
			Assert.AreEqual("1", result);
		}

		[TestMethod]
		public void Accessors_eHubTransactionsAccessor_IsProductionClient()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;

			mockeHubTransactionsContext.Expect(x => x.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", "AAA", "CCC")).Return(new[] { "PRD" });
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());

			var result = eHubTransactionsAccessor.IsProductionClient_WithRetries("AAABBBCCC", new NoOpLogger());

			mockeHubTransactionsContext.VerifyAllExpectations();
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void Accessors_eHubTransactionsAccessor_UpdateMessageDistributionStatus()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;

			mockeHubTransactionsContext.Expect(x => x.BeginTransaction()).Return(mockDBTransaction);
			mockeHubTransactionsContext.Expect(x => x.ExecuteSqlCommand("EXEC [dbo].[UpdateMessageDistributionStatus] @MessageTrackingID = @p0, @SenderID = @p1, @RecipientID = @p2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "SENDER", "RECIPIENT")).Return(1);
			mockDBTransaction.Expect(x => x.Commit());
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());
			((IDisposable)mockDBTransaction).Expect(x => x.Dispose());

			eHubTransactionsAccessor.UpdateMessageDistributionStatus_WithRetries(new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"), "SENDER", "RECIPIENT", new NoOpLogger());

			mockeHubTransactionsContext.VerifyAllExpectations();
			mockDBTransaction.VerifyAllExpectations();
		}

		[TestMethod]
		public void Accessors_eHubTransactionsAccessor_InsertError()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;
			eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 1, 1);
			eHubTransactionsAccessor.GetNewGuid = () => Guid.Empty;

			var eHubInboxMessages = new TestDbSet<eHubInboxMessage> { new eHubInboxMessage { EI_PK = new Guid("11111111-1111-1111-1111-111111111111"), EI_MessageTrackingID = "MESSAGEID" } };
			var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage> { new eHubOutboxMessage { OI_PK = new Guid("22222222-2222-2222-2222-222222222222"), OI_MessageTrackingID = "MESSAGEID" } };
			mockeHubTransactionsContext.Stub(x => x.eHubInboxMessages).Return(eHubInboxMessages).Repeat.Any();
			mockeHubTransactionsContext.Stub(x => x.eHubOutboxMessages).Return(eHubOutboxMessages).Repeat.Any();

			mockeHubTransactionsContext.Expect(x => x.BeginTransaction()).Return(mockDBTransaction);
			mockeHubTransactionsContext.Expect(x => x.ExecuteSqlCommand("EXEC [dbo].[InsertError] @ErrorPK = @p0, @Source = 'BIZ', @ErrorType = 'Fai', @Description = @p1, @InboxPK = @p2, @OutboxPK = @p3, @CurrentDateTimeUTC = @p4",
						Guid.Empty, "DESCRIPTION", new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), "2016-01-01T00:00:00")).Return(1);
			mockDBTransaction.Expect(x => x.Commit());
			((IDisposable)mockeHubTransactionsContext).Expect(x => x.Dispose());
			((IDisposable)mockDBTransaction).Expect(x => x.Dispose());

			eHubTransactionsAccessor.InsertError_WithRetries("MESSAGEID", "DESCRIPTION", new NoOpLogger());

			mockeHubTransactionsContext.VerifyAllExpectations();
			mockDBTransaction.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_ShouldNotRetry()
		{
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(new InsufficientMemoryException("Exception should be passed to caller")).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (InsufficientMemoryException ex)
			{
				Assert.AreEqual("Exception should be passed to caller", ex.Message);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(1));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(new InvalidOperationException("Not related to Timeout")).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("Not related to Timeout", ex.Message);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(new DataException("Not Deadlock", new UpdateException("DuplicateKeyViolation", CreateSqlException(2601)))).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing, new NoOpLogger());
				Assert.Fail("Should throw");
			}
			catch (DataException ex)
			{
				Assert.AreEqual("Not Deadlock", ex.Message);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(3));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(new FatalMessageProcessingException("No certificate registered for South African Customs messaging.")).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (FatalMessageProcessingException ex)
			{
				Assert.AreEqual("No certificate registered for South African Customs messaging.", ex.Message);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(4));
			}
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_ShouldNotRetry_SqlException()
		{
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(CreateSqlException(2601)).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 2601);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(1));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(CreateSqlException(2627)).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 2627);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(CreateSqlException(8152)).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 8152);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(3));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(CreateSqlException(50000)).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 50000);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(4));
			}

			mockProcessing.Stub(x => x.Invoke()).Throw(CreateSqlException(547)).Repeat.Once();

			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw");
			}
			catch (SqlException ex)
			{
				Assert.IsTrue(ex.Number == 547);
				mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(5));
			}
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_ShouldRetry_SqlException()
		{
			var sqlException = CreateSqlException(1205);
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(sqlException).Repeat.Once();

			DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing, new NoOpLogger());

			mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_ShouldRetry_Timeout()
		{
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(new InvalidOperationException("Timeout expired.")).Repeat.Once();

			DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing, new NoOpLogger());
			mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_ShouldRetry_Timeout_InnerException()
		{
			var dataException = new DataException("Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", CreateSqlException(-2));
			var outterException = new DbUpdateException("An error occurred while updating the entries. See the inner exception for details.", new UpdateException("An error occurred while updating the entries. See the inner exception for details.", dataException));
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(outterException).Repeat.Once();

			DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing, new NoOpLogger());
			mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_ShouldRetry_Deadlock()
		{
			var dataException = new DataException("deadlock", CreateSqlException(1205));
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(dataException).Repeat.Once();

			DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing, new NoOpLogger());

			mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
		}

		[TestMethod]
		public void TestDatabaseAccessHelpers_AccessDatabaseWithRetries_RetryTimeLimitIsNotReached_ShouldKeepRetrying()
		{
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(new InvalidOperationException("Timeout expired.")).Repeat.Times(2);

			var oldValue = DatabaseAccessHelpers.RetryTimeLimit;
			DatabaseAccessHelpers.RetryTimeLimit = 1200;
			DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);

			mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(3));
			DatabaseAccessHelpers.RetryTimeLimit = oldValue;
		}

		[TestMethod]
		public void TestDatabaseHelper_AccessDatabaseWithRetries_RetryTimeLimitIsReached_ShouldStopRetrying()
		{
			var mockProcessing = MockRepository.GenerateStub<Action>();
			mockProcessing.Stub(x => x.Invoke()).Throw(new InvalidOperationException("Timeout expired.")).Repeat.Times(2);

			var oldValue = DatabaseAccessHelpers.RetryTimeLimit;
			DatabaseAccessHelpers.RetryTimeLimit = 1000;
			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(mockProcessing);
				Assert.Fail("Should throw TimeoutException");
			}
			catch (TimeoutException ex)
			{
				Assert.AreEqual("Time limit '00:00:01' exceeded while accessing database. See inner exception for the actual error.", ex.Message);
				Assert.IsTrue(ex.InnerException is InvalidOperationException);
				Assert.AreEqual("Timeout expired.", ex.InnerException.Message);
			}

			mockProcessing.AssertWasCalled(x => x.Invoke(), x => x.Repeat.Times(2));
			DatabaseAccessHelpers.RetryTimeLimit = oldValue;
		}

		[TestMethod]
		public void Accesors_GetAsyncPollingConfig()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;
			eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 1, 1);
			eHubTransactionsAccessor.GetNewGuid = () => Guid.Empty;
			var eHubAsyncPollingRegistrations = new TestDbSet<eHubAsyncPollingRegistration>
			{
				new eHubAsyncPollingRegistration
				{
					PR_PK = new Guid("11111111-1111-1111-1111-111111111111"),
					PR_CC = new Guid("22222222-2222-2222-2222-222222222222"),
					PR_RT = new Guid("33333333-3333-3333-3333-333333333333"),
					PR_EH = new Guid("44444444-4444-4444-4444-444444444444"),
					PR_Text = "NPN",
					PR_XML = "<ABC>DDD</ABC>"
				}
			};
			mockeHubTransactionsContext.Stub(x => x.eHubAsyncPollingRegistrations).Return(eHubAsyncPollingRegistrations).Repeat.Any();
			mockeHubTransactionsContext.Expect(x => x.Dispose()).Repeat.Any();
			var prXml = eHubTransactionsAccessor.GetAsyncPollingConfig_WithRetries("11111111-1111-1111-1111-111111111111");

			Assert.AreEqual("<ABC>DDD</ABC>", prXml);

			prXml = eHubTransactionsAccessor.GetAsyncPollingConfig_WithRetries("22222222-2222-2222-2222-222222222222");
			Assert.IsNull(prXml);
			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod]
		public void Accesors_GetRequiredUniqueCertificate()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;
			eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 1, 1);
			eHubTransactionsAccessor.GetNewGuid = () => Guid.Empty;

			var eHubCertificates = new TestDbSet<eHubCertificate>()
			{
				new eHubCertificate
				{
					CE_PK = new Guid("11111111-1111-1111-1111-111111111111"),
					CE_Category = "ACAS_BR",
					CE_ID = "TST001",
					eHubClient = new eHubClient{ CC_ID = "WTLEDITST" },
					eHubClientSystem = new eHubClientSystem { EH_ID = "WTLTST" },
					CE_ValidFromUTC = new DateTime(2015, 1, 1),
					CE_ValidToUTC = new DateTime(2021, 1, 1),
					CE_BinaryContainer = new byte[]{ 1 }
				}
			};
			mockeHubTransactionsContext.Stub(x => x.eHubCertificates).Return(eHubCertificates).Repeat.Any();
			mockeHubTransactionsContext.Expect(x => x.Dispose()).Repeat.Any();

			var certificate = eHubTransactionsAccessor.GetRequiredUniqueCertificate_WithRetries("WTLTST", "TST001", "ACAS_BR");

			Assert.AreEqual("11111111-1111-1111-1111-111111111111", certificate.CE_PK.ToString());
			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod]
		public void Accesors_GetRequiredUniqueCertificate_Exceptions()
		{
			var mockeHubTransactionsContext = MockRepository.GenerateStrictMock<eHubTransactionsContext>();
			var mockDBTransaction = MockRepository.GenerateStrictMock<IDbTransaction>();
			eHubTransactionsAccessor.GetDbContext = () => mockeHubTransactionsContext;
			eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 1, 1);
			eHubTransactionsAccessor.GetNewGuid = () => Guid.Empty;
			var cert1 = new eHubCertificate
			{
				CE_PK = new Guid("11111111-1111-1111-1111-111111111111"),
				CE_Category = "ACAS_BR",
				CE_ID = "TST001",
				eHubClient = new eHubClient { CC_ID = "WTLEDITST" },
				eHubClientSystem = new eHubClientSystem { EH_ID = "WTLTST" },
				CE_ValidFromUTC = new DateTime(2015, 1, 1),
				CE_ValidToUTC = new DateTime(2021, 1, 1),
				CE_BinaryContainer = new byte[] { }
			};

			var cert2 = new eHubCertificate
			{
				CE_PK = new Guid("22222222-2222-2222-2222-222222222222"),
				CE_Category = "ACAS_BR",
				CE_ID = "TST001",
				eHubClient = new eHubClient { CC_ID = "WTLEDITST" },
				eHubClientSystem = new eHubClientSystem { EH_ID = "WTLTST" },
				CE_ValidFromUTC = new DateTime(2015, 1, 1),
				CE_ValidToUTC = new DateTime(2021, 1, 1),
				CE_BinaryContainer = new byte[] { }
			};

			var eHubCertificates = new TestDbSet<eHubCertificate>();
			eHubCertificates.Add(cert1);
			eHubCertificates.Add(cert2);
			mockeHubTransactionsContext.Stub(x => x.eHubCertificates).Return(eHubCertificates).Repeat.Any();
			mockeHubTransactionsContext.Expect(x => x.Dispose()).Repeat.Any();

			try
			{
				var certificate = eHubTransactionsAccessor.GetRequiredUniqueCertificate_WithRetries("WTLTST", "TST002", "ACAS_BR");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
				Assert.IsTrue(ex.Message.Contains("Expected 1 but found 0 certificate(s) matching systemID=WTLTST, id=TST002, category=ACAS_BR"));
			}

			try
			{
				var certificate = eHubTransactionsAccessor.GetRequiredUniqueCertificate_WithRetries("WTLTST", "TST001", "ACAS_BR");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
				Assert.IsTrue(ex.Message.Contains("Expected 1 but found 2 certificate(s) matching systemID=WTLTST, id=TST001, category=ACAS_BR"));
			}

			eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2011, 1, 1);
			cert2.CE_ID = "TST002";

			try
			{
				var certificate = eHubTransactionsAccessor.GetRequiredUniqueCertificate_WithRetries("WTLTST", "TST002", "ACAS_BR");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
				Assert.IsTrue(ex.Message.Contains("Certificate is expired. From 2015-01-01T00:00:00 to 2021-01-01T00:00:00. SystemID=WTLTST, id=TST002, category=ACAS_BR"));
			}

			eHubTransactionsAccessor.GetDateTimeUtcNow = () => new DateTime(2016, 1, 1);
			cert2.CE_BinaryContainer = null;

			try
			{
				var certificate = eHubTransactionsAccessor.GetRequiredUniqueCertificate_WithRetries("WTLTST", "TST002", "ACAS_BR");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
				Assert.IsTrue(ex.Message.Contains("Found empty certificate matching systemID=WTLTST, id=TST002, category=ACAS_BR"));
			}

			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		static SqlException CreateSqlException(int errorNumber = 0, byte errorState = 0, byte errorClass = 0, string server = "", string errorMessage = "", string procedure = "", int lineNumber = 0)
		{
			var collection = typeof(SqlErrorCollection)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(new object[] { }) as SqlErrorCollection;

			var error = typeof(SqlError)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
				new[]
				{
					typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int)
				},
				null).Invoke(new object[] { errorNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber }) as SqlError;

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });

			var ex = typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(SqlErrorCollection), typeof(string) }, null)
				.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;

			return ex;
		}
	}
}
