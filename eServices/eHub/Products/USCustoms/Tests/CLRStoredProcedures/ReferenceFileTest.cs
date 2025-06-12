using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CargoWise.eHub.Integration;
using CargoWise.eServices.USCustoms.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass()]
	public class ReferenceFileTest : TestBase
	{
		SqlConnection mockSqlConnection;
		SqlTransaction mockSqlTransaction;
		SqlCommand mockSqlCommand;
		SqlParameter mockSqlParameter;
		SqlParameterCollection mockSqlParameters;
		SqlDataReader mockSqlReader;
		IMessageQueuer mockMessageQueuer;
		bool flagConnOpen;
		bool flagTranBegun;
		bool flagCmdTranSet;
		Action assertConnectionOpen;
		Action assertConnectionClosed;
		Action assertTranBegun;
		Action assertTranNotBegun;
		Action assertCmdTranSet;

		[TestMethod]
		public void ReferenceFile_SendReferenceFileRequest_InvalidQuery()
		{
			InitialiseTest();

			mockSqlCommand.Expect(x => x.ExecuteReader(CommandBehavior.SingleRow))
				.Callback(new Func<CommandBehavior, bool>((b) => mockSqlCommand.CommandText == "SelectReferenceFileQuery"))
				.Do(new Func<CommandBehavior, SqlDataReader>((b) =>
				{ assertConnectionOpen(); assertTranBegun(); assertCmdTranSet(); return mockSqlReader; }));
			mockSqlReader.Stub(x => x.Read()).Return(false);

			AssertException<ArgumentException>(() =>
				ReferenceFile.SendReferenceFileRequest(mockSqlConnection, "INVALID", mockMessageQueuer),
				x => x.Message == "Reference file query not found for ID 'INVALID'");

			mockMessageQueuer.VerifyAllExpectations();
			mockSqlCommand.VerifyAllExpectations();
			mockSqlTransaction.AssertWasCalled(x => x.Rollback());
			Assert.IsFalse(flagTranBegun);
			Assert.IsFalse(flagConnOpen);
		}

		[TestMethod]
		public void ReferenceFile_SendReferenceFileRequest_SqlException()
		{
			InitialiseTest();

			var sqlException = new Exception("SQL EXCEPTION");
			mockSqlCommand.Expect(x => x.ExecuteReader(CommandBehavior.SingleRow))
				.Callback(new Func<CommandBehavior, bool>((b) => mockSqlCommand.CommandText == "SelectReferenceFileQuery"))
				.Throw(sqlException);

			AssertException<Exception>(() =>
				ReferenceFile.SendReferenceFileRequest(mockSqlConnection, "INVALID", mockMessageQueuer),
				x => x.Equals(sqlException));

			mockMessageQueuer.VerifyAllExpectations();
			mockSqlCommand.VerifyAllExpectations();
			mockSqlTransaction.AssertWasCalled(x => x.Rollback());
			Assert.IsFalse(flagTranBegun);
			Assert.IsFalse(flagConnOpen);
		}

		[TestMethod]
		public void ReferenceFile_SendReferenceFileRequest_QueuerException()
		{
			InitialiseTest();

			var queuerException = new Exception("QUEUER EXCEPTION");
			mockSqlReader.Stub(x => x.Read()).Return(true);
			mockSqlCommand.Expect(x => x.ExecuteReader(CommandBehavior.SingleRow)).Return(mockSqlReader);
			mockSqlCommand.Expect(x => x.ExecuteNonQuery()).Return(1);
			mockMessageQueuer.Expect(x => x.EnqueueMessage(Arg.Is(mockSqlTransaction), Arg<string>.Is.Anything, Arg<Guid>.Is.Anything,
				Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Anything)).Throw(queuerException);

			AssertException<Exception>(() =>
				ReferenceFile.SendReferenceFileRequest(mockSqlConnection, "INVALID", mockMessageQueuer),
				x => x.Equals(queuerException));

			mockMessageQueuer.VerifyAllExpectations();
			mockSqlCommand.VerifyAllExpectations();
			mockSqlTransaction.AssertWasCalled(x => x.Rollback());
			Assert.IsFalse(flagTranBegun);
			Assert.IsFalse(flagConnOpen);
		}

		void InitialiseTest()
		{
			mockSqlConnection = MockRepository.GenerateMock<SqlConnection>();
			mockSqlTransaction = MockRepository.GenerateMock<SqlTransaction>();
			mockSqlCommand = MockRepository.GenerateStrictMock<SqlCommand>();
			mockSqlParameter = MockRepository.GenerateMock<SqlParameter>();
			mockSqlParameters = MockRepository.GenerateMock<SqlParameterCollection>();
			mockSqlReader = MockRepository.GenerateMock<SqlDataReader>();
			mockMessageQueuer = MockRepository.GenerateMock<IMessageQueuer>();
			flagConnOpen = false;
			flagTranBegun = false;
			flagCmdTranSet = false;
			assertConnectionOpen = new Action(() => Assert.IsTrue(flagConnOpen, "Connection is not open."));
			assertConnectionClosed = new Action(() => Assert.IsFalse(flagConnOpen, "Connection already open."));
			assertTranBegun = new Action(() => Assert.IsTrue(flagTranBegun, "Transaction not begun."));
			assertTranNotBegun = new Action(() => Assert.IsFalse(flagTranBegun, "Transaction already begun."));
			assertCmdTranSet = new Action(() => Assert.IsTrue(flagCmdTranSet, "Command does not have transaction set."));
			mockSqlConnection.Stub(x => x.Open()).Do(new Action(() => { assertConnectionClosed(); flagConnOpen = true; }));
			mockSqlConnection.Stub(x => x.Close()).Do(new Action(() => { assertConnectionOpen(); flagConnOpen = false; }));
			mockSqlConnection.Stub(x => x.BeginTransaction(IsolationLevel.ReadCommitted)).Do(new Func<IsolationLevel, SqlTransaction>((i) => { assertTranNotBegun(); flagTranBegun = true; return mockSqlTransaction; }));
			mockSqlTransaction.Stub(x => x.Commit()).Do(new Action(() => { assertTranBegun(); flagTranBegun = false; }));
			mockSqlTransaction.Stub(x => x.Rollback()).Do(new Action(() => { assertTranBegun(); flagTranBegun = false; }));
			mockSqlConnection.Stub(x => x.CreateCommand()).Return(mockSqlCommand);
			mockSqlParameters.Stub(x => x.Add(null, 0, 0)).IgnoreArguments().Return(mockSqlParameter);
			mockSqlCommand.Stub<IDisposable>(x => x.Dispose()).Do(new Action(() => { mockSqlCommand.CommandText = null; flagCmdTranSet = false; }));
			mockSqlCommand.Stub(x => x.Parameters).Return(mockSqlParameters);
			mockSqlCommand.Stub(x => x.CommandText).PropertyBehavior();
			mockSqlCommand.Stub(x => x.CommandType).PropertyBehavior();
			mockSqlCommand.Stub(x => x.Transaction = mockSqlTransaction).Do(new Action<SqlTransaction>((t) => flagCmdTranSet = true));
		}
	}
}
