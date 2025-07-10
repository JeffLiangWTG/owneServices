using System;
using System.Data;
using System.Reflection;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Data;
using CargoWise.Data.Providers.Common;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

public class DatabaseAccessorTests : TestWithDatabase
{
	[Test]
	public void EnabledMultiSubnetFailover()
	{
		// Arrange
		var currentServer = databaseAccessor.ServerName;
		using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { currentServer }))
		{
			// Act
			var connectionString = databaseAccessor.GetConnectionString();

			// Assert
			Assert.That(new SqlConnectionStringBuilder(connectionString).MultiSubnetFailover, Is.EqualTo(true));
		}
	}

	[Test]
	public void DisabledMultiSubnetFailover()
	{
		// Arrange
		using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { "notarealserver" }))
		{
			// Act
			var connectionString = databaseAccessor.GetConnectionString();

			// Assert
			Assert.That(new SqlConnectionStringBuilder(connectionString).MultiSubnetFailover, Is.EqualTo(false));
		}
	}

	[Test]
	public void ExecuteDbCommandReceiveResult()
	{
		const int result = 0;
		var commandResult = databaseAccessor.ExecuteDbCommand(command =>
		{
			command.CommandText = "SELECT @result";
			command.CommandType = CommandType.Text;
			command.Parameters.AddWithValue("@result", result);
			return (int)command.ExecuteScalar();
		});

		Assert.That(commandResult, Is.EqualTo(result));
	}

	[Test]
	public void ExecuteDbCommandWhenDatabaseIsUpgrading()
	{
		using (var adminConn = NewAdminConnection())
		{
			try
			{
				LockDb(adminConn);
				var ex = Assert.Throws<DatabaseAccessException>(() => databaseAccessor.ExecuteDbCommand(_ => 0));
				Assert.That(ex.Message, Is.EqualTo("A system upgrade is in progress. Please try again later."));
			}
			finally
			{
				UnlockDb(adminConn);
			}
		}

		void LockDb(IDbConnection conn)
		{
			var command = conn.CreateCommand();
			command.CommandText = "EXEC sys.sp_addextendedproperty 'DbIsLockedOutFor', 'For Test'";
			command.CommandType = System.Data.CommandType.Text;
			command.ExecuteNonQuery();
		}

		void UnlockDb(IDbConnection conn)
		{
			var command = conn.CreateCommand();
			command.CommandText = "EXEC sys.sp_dropextendedproperty 'DbIsLockedOutFor'";
			command.CommandType = System.Data.CommandType.Text;
			command.ExecuteNonQuery();
		}
	}

	[TestCase(0)]
	[TestCase(1222)]
	[TestCase(5245)]
	[TestCase(3617)]
	public void ExecuteDbCommandOccurSqlException(int errorNumber)
	{
		var sqlException = SqlExceptionHelper.GenerateSqlException(errorNumber);
		var dbError = new DbErrorMatch(sqlException);
		var ex = Assert.Throws<DatabaseAccessException>(() => databaseAccessor.ExecuteDbCommand<int>(_ => throw sqlException));
		Assert.That(ex.Message, Is.EqualTo(dbError.GetUserFriendlyMessage(null)));
	}

	class SqlExceptionHelper
	{
		public static SqlException GenerateSqlException(int errorNumber)
		{
			var collection = Construct<SqlErrorCollection>();
			var error = Construct<SqlError>(errorNumber, (byte)2, (byte)3, "server name", "error message", "proc", 100, (uint)1, null);

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });

			var e = typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.ExplicitThis, new[] { typeof(SqlErrorCollection), typeof(string) }, Array.Empty<ParameterModifier>())
				.Invoke(null, new object[] { collection, "11.0.0" }) as SqlException;

			return e;
		}

		static T Construct<T>(params object[] p)
		{
			return (T)typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0].Invoke(p);
		}
	}
}
