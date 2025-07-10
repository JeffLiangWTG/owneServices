using System;
using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class DbLockout : IDbLockout
	{
		public DbLockout(IDbConnection connection, string[] applicationUsers)
		{
			Argument.Argument.NotNull(connection, nameof(connection));
			Argument.Argument.NotNull(applicationUsers, nameof(applicationUsers));

			this.connection = connection;
			this.applicationUsers = applicationUsers;
		}

		readonly IDbConnection connection;
		readonly string[] applicationUsers;

		public IDisposable Acquire()
		{
			RevokeUsersConnect();
			return new DbLockRealease(this);
		}

		void RevokeUsersConnect()
		{
			foreach (var applicationUser in applicationUsers)
			{
				Console.WriteLine($"Revoking user {applicationUser} connect access");
				connection.ExecuteNonQuery($@"
IF EXISTS( SELECT 1 FROM sys.database_principals WHERE name = '{applicationUser}')
REVOKE CONNECT FROM {applicationUser}");
			}
		}

		void GrantUsersConnect()
		{
			foreach (var applicationUser in applicationUsers)
			{
				Console.WriteLine($"Granting user {applicationUser} connect access");
				connection.ExecuteNonQuery($@"
IF EXISTS( SELECT 1 FROM sys.database_principals WHERE name = '{applicationUser}')
GRANT CONNECT TO {applicationUser}");
			}
		}

		sealed class DbLockRealease : IDisposable
		{
			public DbLockRealease(DbLockout lockout)
			{
				Argument.Argument.NotNull(lockout, nameof(lockout));
				this.lockout = lockout;
			}

			readonly DbLockout lockout;

			public void Dispose()
			{
				lockout.GrantUsersConnect();
			}
		}
	}
}
