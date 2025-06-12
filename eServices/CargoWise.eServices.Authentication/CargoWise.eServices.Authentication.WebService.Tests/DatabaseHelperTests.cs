using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WebService.Tests
{
	[TestFixture]
	public class AuthenticationDatabaseHelperTests
	{
		private Mock<IDbCommand> _command;
		private Mock<IDbConnection> _connection;
		private Mock<DatabaseHelper> _databaseHelper;
		private Mock<IDataParameterCollection> _parameters;

		[SetUp]
		public void SetUp()
		{
			_command = new Mock<IDbCommand>();
			_connection = new Mock<IDbConnection>();
			_databaseHelper = new Mock<DatabaseHelper>();
			_parameters = new Mock<IDataParameterCollection>();

			_command.Setup(_ => _.Dispose());
			_command.Setup(_ => _.Parameters).Returns(_parameters.Object);
			_databaseHelper.Setup(_ => _.DbConnection).Returns(_connection.Object);
		}

		private void SetParameterVerify(IList<SqlParameter> parameters)
		{
			foreach (var para in parameters)
			{
				_parameters.Setup(_ => _.Add(It.Is<SqlParameter>(p => 
						para.ParameterName == p.ParameterName && 
						para.SqlDbType == p.SqlDbType && 
						para.Size == p.Size && 
						para.Value == p.Value
					))).Verifiable();
			}
		}

		[Test]
		public void TestGetSystemLastEditUTC()
		{
			var utcNow = DateTime.UtcNow;
			_databaseHelper.Setup(_ => _.GetCommand("SELECT AT_SystemLastModifiedTimeUTC FROM Authentication WHERE AT_SystemID = @SystemID", _connection.Object)).Returns(_command.Object);

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = "ABC" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(null);
			Assert.AreEqual(null, _databaseHelper.Object.GetSystemLastEditUTC("ABC"));

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = "BCD" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(utcNow);
			Assert.AreEqual(utcNow, _databaseHelper.Object.GetSystemLastEditUTC("BCD"));

			_databaseHelper.VerifyAll();
		}

		[Test]
		public void TestCheckSystemIDExistenceDB()
		{
			var utcNow = DateTime.UtcNow;
			_databaseHelper.Setup(_ => _.GetCommand("SELECT AT_SystemLastModifiedTimeUTC FROM Authentication WHERE AT_SystemID = @SystemID", _connection.Object)).Returns(_command.Object);

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = "ABC" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(null);
			Assert.AreEqual(false, _databaseHelper.Object.CheckSystemIDExistence("ABC"));

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = "BCD" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(utcNow);
			Assert.AreEqual(true, _databaseHelper.Object.CheckSystemIDExistence("BCD"));

			_databaseHelper.VerifyAll();
		}

		[Test]
		public void TestCheckCodeExistenceDB()
		{
			_databaseHelper.Setup(_ => _.GetCommand("SELECT COUNT(*) FROM Authentication WHERE AT_EnterpriseCode = @EnterpriseCode AND AT_ServerCode = @ServerCode", _connection.Object)).Returns(_command.Object);

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = "ABC" },
				new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = "BCD" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(0);
			Assert.AreEqual(false, _databaseHelper.Object.CheckCodeExistence("ABC", "BCD"));
			_parameters.VerifyAll();

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = "123" },
				new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = "234" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(1);
			Assert.AreEqual(true, _databaseHelper.Object.CheckCodeExistence("123", "234"));

			_databaseHelper.VerifyAll();
		}

		[Test]
		public void TestValidateSystemIDAndPasswordDB()
		{
			_databaseHelper.Setup(_ => _.GetCommand(It.IsAny<string>(), _connection.Object)).Returns(_command.Object);

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = "ABC" },
				new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = "BCD" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(0);
			Assert.AreEqual(false, _databaseHelper.Object.ValidateSystemIDAndPassword("ABC", "BCD"));

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = "123" },
				new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = "234" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(1);
			Assert.AreEqual(true, _databaseHelper.Object.ValidateSystemIDAndPassword("123", "234"));

			_databaseHelper.VerifyAll();
		}

		[Test]
		public void TestValidateCodeAndPasswordDB()
		{
			_databaseHelper.Setup(_ => _.GetCommand(It.IsAny<string>(), _connection.Object)).Returns(_command.Object);

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = "ABC" },
				new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = "BCD" },
				new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = "password" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(0);
			Assert.AreEqual(false, _databaseHelper.Object.ValidateCodeAndPassword("ABC", "BCD", "password"));

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = "123" },
				new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = "234" },
				new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = "password" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Returns(1);
			Assert.AreEqual(true, _databaseHelper.Object.ValidateCodeAndPassword("123", "234", "password"));

			_databaseHelper.VerifyAll();
		}

		[Test]
		public void TestUnhandledExceptions()
		{
			_databaseHelper.Setup(_ => _.GetCommand(It.IsAny<string>(), It.Is<IDbConnection>(con => con == _connection.Object))).Returns(_command.Object);

			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = "ABC" },
				new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = "BCD" },
				new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = "password" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Throws(new ArgumentException("Unknown Exception"));
			try
			{
				_databaseHelper.Object.ValidateCodeAndPassword("ABC", "BCD", "password");
				Assert.Fail("An exception was thrown");
			}
			catch(ArgumentException ex)
			{
				Assert.AreEqual("Unknown Exception", ex.Message);
			}

			_databaseHelper.VerifyAll();
		}

		[Test]
		public void TestHandledExceptions()
		{
			SetParameterVerify(new List<SqlParameter>
			{
				new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = "ABC" },
				new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = "BCD" },
				new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = "password" },
			});
			_command.Setup(_ => _.ExecuteScalar()).Throws(new InvalidOperationException("Invalid Operation Exception"));
			_databaseHelper.Setup(_ => _.GetCommand(It.IsAny<string>(), _connection.Object)).Returns(_command.Object);
			Assert.AreEqual(false, _databaseHelper.Object.ValidateCodeAndPassword("ABC", "BCD", "password"));
			_databaseHelper.VerifyAll();

			_command.Setup(_ => _.ExecuteScalar()).Returns(1);
			_databaseHelper.Setup(_ => _.DbConnection).Returns(new SqlConnection(@"Data Source=.;Database=THIS_IS_NOT_AN_EXISTING_DATABASE;Connection Timeout=1"));
			Assert.AreEqual(false, _databaseHelper.Object.ValidateCodeAndPassword("123", "234", "password"));
		}

		[Test]
		public void GetCommand_GivenNonSqlConnection_ThenThrowException()
		{
			var connection = new OleDbConnection();
			var databaseHelper = new Mock<DatabaseHelper> { CallBase = true };
			var exception = Assert.Throws<ArgumentException>(() => databaseHelper.Object.GetCommand(string.Empty, connection));
			Assert.That(exception.Message, Is.EqualTo("Expecting connection type: SqlConnection"));
		}

		[Test]
		public void TestIsDatabaseAlive()
		{
			var databaseHelper = new Mock<DatabaseHelper> { CallBase = true };
			var connection = new Mock<IDbConnection>();
			databaseHelper.Setup(_ => _.DbConnection).Returns(connection.Object);

			Assert.AreEqual(true, databaseHelper.Object.IsDatabaseAlive());

			connection.Setup(_ => _.Open()).Throws(new Exception());
			Assert.AreEqual(false, databaseHelper.Object.IsDatabaseAlive());
		}
	}
}
