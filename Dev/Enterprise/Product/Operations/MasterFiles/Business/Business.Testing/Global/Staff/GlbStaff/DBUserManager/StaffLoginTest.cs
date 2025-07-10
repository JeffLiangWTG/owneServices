using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	[RequiresSoftware(RequiredSoftware.IsVM)]
	class StaffLoginTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestStaffSqlLoginPasswordIsSynchronisedToReplicasFromPrimary()
		{
			// now that password hash will be saved in the database,
			// there is no need to either same it immediately to logins or propagate to secondary servers
			// Once UseModernSqlSecuritySystem flag is removed this test should be removed
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			const string secondaryReplica = "SYDSP-SSQL-6.sand.wtg.zone\\INSTANCE1";
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			// Test setup
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";
			var dbUserManager = new DbUserManager();

			// setup replicas
			var primaryReplica = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(Db.Connection.ServerNameReportedByDatabase);
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = secondaryReplica, AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplica, AvailabilityMode = 1 },
			};
			var replicas = new List<string> { primaryReplica, secondaryReplica };

			using (TestSetupDisposable())
			{
				// create a new staff user
				var staff = CreateStaffUserWithDbAccess();

				// set staff login password
				dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);
				staff.Factory.Save();

				// Arrange
				ChangeStaffSqlLoginPasswordOnSecondaryReplica();

				// Act
				using (var adminConnection = Db.NewAdminConnection())
				{
					dbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(adminConnection);
				}

				// Assert
				AssertThatStaffSqlLoginCanOpenConnectionToAllReplicas();
			}

			[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
			void AssertThatStaffSqlLoginCanOpenConnectionToAllReplicas()
			{
				var connectionStates = new Dictionary<string, bool>();
				var stringBuilder = new StringBuilder();
				replicas.ForEach(replica =>
				{
					// staff login should be able to open a connection to the sql instance with the provided staffSqlLoginPassword
					using (var dbConnection = new SqlConnection(GetConnectionString(replica, Db.SqlMasterDb, staffSqlLoginName, staffSqlLoginPassword)))
					{
						try
						{
							dbConnection.Open();
							connectionStates[replica] = dbConnection.State == System.Data.ConnectionState.Open;
						}
						catch (Exception ex)
						{
							connectionStates[replica] = false;
							stringBuilder.AppendLine($"Failed to connect '{replica}' with error: {ex}");
						}
					}
				});

				Assert(stringBuilder.ToString(), connectionStates.All(x => x.Value));
			}

			void ChangeStaffSqlLoginPasswordOnSecondaryReplica()
			{
				var changePasswordScript = $@"
ALTER LOGIN [{staffSqlLoginName}] WITH PASSWORD = '{Guid.NewGuid():N}', CHECK_POLICY = OFF, DEFAULT_DATABASE = [{Db.DatabaseName}], DEFAULT_LANGUAGE = us_english;
";
				// change the login password on the secondary replica, same as the issue reported from the incident
				// so that to test if dbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases can fix it
				ExecuteNonQuery(secondaryReplica, changePasswordScript);
			}

			GlbStaff CreateStaffUserWithDbAccess()
			{
				var factory = new BusinessObjectFactory();
				var staff = factory.New<GlbStaff>();
				staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
				staff.GS_LoginName = staffLoginName;
				staff.GS_Code = staffCode;
				staff.IsReadOnlyDBUser = true; // db access role

				var group = factory.New<GlbGroup>();
				var readerRole = group.Roles.AddNew();
				readerRole.GGR_RoleName = DbRoleTypes.CwRestrictedReaderRole; // CW1 reader role

				var link = factory.New<GlbGroupLink>();
				link.GK_GS = staff.PK;
				link.GK_GG = group.PK;

				factory.Save();

				return staff;
			}

			IDisposable TestSetupDisposable()
			{
				// clean up [GlbStaff] table for the test staff
				ExecuteNonQuery(primaryReplica, "DELETE [GlbStaff] WHERE GS_Code = @staffCode", Db.DatabaseName, command =>
				{
					command.AddParameter("@staffCode", System.Data.SqlDbType.VarChar, 3, staffCode);
				});

				var weHaveCreatedTestDbOnSecondaryReplica = false;
				using (var adminConnectionToSecondaryReplica = Db.NewAdminConnection(secondaryReplica, Db.SqlMasterDb))
				{
					if (!adminConnectionToSecondaryReplica.DatabaseExists(Db.DatabaseName))
					{
						// create a db on secondary if not exist yet
						adminConnectionToSecondaryReplica.ExecuteNonQuery($"IF NOT EXISTS (SELECT NULL FROM sys.databases WHERE name = '{Db.DatabaseName}') CREATE DATABASE [{Db.DatabaseName}]");
						weHaveCreatedTestDbOnSecondaryReplica = true;
					}
				}

				return new DisposableAction(() =>
				{
					var dropLoginIfExists = $@"-- StaffLoginTest Drop Logins
DECLARE @stmt NVARCHAR(max) = ''

SELECT @stmt = @stmt + CHAR(13) + CHAR(10) + 'drop login [' + name + ']'
FROM sys.sql_logins 
WHERE 1=1
	AND name LIKE '{DbUserRepository.StaffDbLoginPrefix}[_]{Db.DatabaseName}[_]%'

EXEC sp_executesql @stmt
";

					var dropDatabaseIfExists = $@"-- StaffLoginTest Drop Database
IF EXISTS (SELECT NULL FROM sys.databases WHERE name = '{Db.DatabaseName}') DROP DATABASE [{Db.DatabaseName}]
";
					// drop the test sql login on primary
					ExecuteNonQuery(primaryReplica, dropLoginIfExists);

					// clean-up artifacts we created for the test
					if (weHaveCreatedTestDbOnSecondaryReplica)
					{
						// drop the test sql login on secondary
						ExecuteNonQuery(secondaryReplica, dropLoginIfExists);

						// drop the test db on secondary if we had created for the test
						ExecuteNonQuery(secondaryReplica, dropDatabaseIfExists);
					}
				});
			}

			int ExecuteNonQuery(string sqlInstance, string script, string databaseName = Db.SqlMasterDb, Action<DbCommand> action = null)
			{
				using (var connection = Db.NewAdminConnection(sqlInstance, databaseName))
				{
					return action == null
						? connection.ExecuteNonQuery(script)
						: connection.ExecuteNonQuery(script, action);
				}
			}
		}

		static string GetConnectionString(string serverName, string databaseName, string userId, string password)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder()
			{
				DataSource = serverName,
				InitialCatalog = databaseName,
				UserID = userId,
				Password = password,
				ApplicationName = "Enterprise.MasterFiles.Business.Testing",
				PersistSecurityInfo = false,
				TrustServerCertificate = true,
				Encrypt = false,
				Pooling = false,
			};

			return connectionStringBuilder.ToString();
		}
	}
}
