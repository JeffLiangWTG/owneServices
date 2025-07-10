using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DbSecurityLockDownWithAdIntegrationsTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestStaffDbUserCreatedFromWindows_HavingShortenedADSamAccountName_AreNotDropped()
		{
			#region Test Setup

			const string domainCredentials = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfDomainCredentials xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<DomainCredentials>
		<DomainName>sand.wtg.zone</DomainName>
		<DomainUserName>sand\ADTest_Admin</DomainUserName>
		<DomainUserPassword>Cqe+Qog2s8WjLEzx6xX/vfC+zMVVF+PxJB0lFuy0eKz/+67g3784MZPxBM5IXYin</DomainUserPassword>
		<IsDefaultDomain>Y</IsDefaultDomain>
		<UserOrganisationalUnit>root/Accounts/ADUnitTesting</UserOrganisationalUnit>
		<GroupOrganisationalUnit />
		<DefaultPassword>Changeme1234</DefaultPassword>
	</DomainCredentials>
</ArrayOfDomainCredentials>";

			var insertStmDataDomainCredentials = Invariant($@"
UPDATE dbo.StmData SET
	SD_BinaryValue = @domainCredentials
WHERE 1=1
	AND SD_Name = 'DomainCredentialsCollection'
	AND SD_Owner is NULL
	AND SD_DepartmentGuid is NULL

IF (@@rowcount = 0)
BEGIN
	INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue)
		VALUES (NEWID(), 'DomainCredentialsCollection', NULL, NULL, @domainCredentials)
END
");

			var adUsers = new List<(string name, string samAccountName, string adObjectGuid)>
			{
				(ADTestUserLongNameB.Name, ADTestUserLongNameB.NameWithDomainPreWindows2000, ADTestUserLongNameB.Guid),
				(ADTestUserLongNameC.Name, ADTestUserLongNameC.NameWithDomainPreWindows2000, ADTestUserLongNameC.Guid),
			};

			var createStaffLoginAndDbUserScriptBuilder = new StringBuilder();
			var dropStaffAndDbUsersScriptBuilder = new StringBuilder();

			var n = 0;
			var staffDbUsers = new SortedSet<string>();
			foreach (var (name, sqlLoginCreatedFromWindows, adObjectGuid) in adUsers)
			{
				var staffUserCode = Invariant($"#{++n:D2}");
				var staffDbUserName = sqlLoginCreatedFromWindows;

				staffDbUsers.Add(staffDbUserName);

				dropStaffAndDbUsersScriptBuilder.AppendLine(Invariant($@"
DELETE FROM dbo.GlbStaff WHERE GS_Code = '{staffUserCode}';
DELETE FROM dbo.GlbStaff WHERE GS_ActiveDirectoryObjectGuid = '{adObjectGuid}';
DROP USER IF EXISTS {staffDbUserName.QuoteName()};
IF EXISTS(SELECT NULL FROM sys.server_principals WHERE name = N'{sqlLoginCreatedFromWindows.QuoteEscapedName('\'')}') DROP LOGIN {sqlLoginCreatedFromWindows.QuoteName()};
"));

				var staffPK = Guid.NewGuid();

				createStaffLoginAndDbUserScriptBuilder.AppendLine(Invariant($@"
INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_DomainName, GS_ActiveDirectoryObjectGuid, GS_IsActive, GS_SystemCreateTimeUtc, Gs_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{staffPK}', '{staffUserCode}', '{name}', '{TestDomainName}', '{adObjectGuid}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbDeveloperGroupPK}', '{staffPK}');
CREATE LOGIN {sqlLoginCreatedFromWindows.QuoteName()} FROM WINDOWS;
CREATE USER {staffDbUserName.QuoteName()} FOR LOGIN {sqlLoginCreatedFromWindows.QuoteName()};
"));
			}

			try
			{
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				{
					connection.ExecuteNonQuery(Invariant($"EXEC [{Db.DatabaseName}]..sp_executesql N'{dropStaffAndDbUsersScriptBuilder.ToString().QuoteEscapedName('\'')}'")); // This class access systems tables and procedures not supported by ZArchitecture
					connection.ExecuteNonQuery(Invariant($"EXEC [{Db.DatabaseName}]..sp_executesql N'{createStaffLoginAndDbUserScriptBuilder.ToString().QuoteEscapedName('\'')}'")); // This class access systems tables and procedures not supported by ZArchitecture

					connection.ExecuteNonQuery(insertStmDataDomainCredentials, command =>
					{
						command.AddParameterBasedOnDbColumn("@domainCredentials", Encoding.Unicode.GetBytes(domainCredentials), StmDataSchema.SD_BinaryValue);
					});
				}
			}
			catch (Exception exception)
			{
				Fail(exception.Message);
			}

			var getStaffDbUsersScript =
				Invariant($"select name from [{Db.DatabaseName}].sys.database_principals where name in ({string.Join(",", staffDbUsers.Select(x => x.QuoteName('\'')))})");

			#endregion

			var staffDbUsersBeforeLockDown = string.Empty;
			var staffDbUsersAfterLockDown = string.Empty;

			var adRegistryMock = new Mock<IADRegistry>();
			adRegistryMock.Setup(x => x.IsIntegrationEnabled).Returns(true);

			using (ObjectFactory.Substitute(adRegistryMock.Object))
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					staffDbUsersBeforeLockDown = string.Join(",", LoadGlbStaffDbUsers(getStaffDbUsersScript, connection));

					new DbSecurityLockDown().LockdownDatabaseLevelSecurity(connection);

					staffDbUsersAfterLockDown = string.Join(",", LoadGlbStaffDbUsers(getStaffDbUsersScript, connection));
				}
				catch (Exception exception)
				{
					Fail($"{exception}");
				}
				finally
				{
					connection.ExecuteNonQuery(Invariant($"EXEC [{Db.DatabaseName}]..sp_executesql N'{dropStaffAndDbUsersScriptBuilder.ToString().QuoteEscapedName('\'')}'")); // This class access systems tables and procedures not supported by ZArchitecture
				}
			}

			AssertEquals(
				Invariant($@"Staff db users should not have been cleaned up by DSA service task!
Before LockDown: {staffDbUsersBeforeLockDown},
After LockDown: {staffDbUsersAfterLockDown}"),
				staffDbUsersBeforeLockDown,
				staffDbUsersAfterLockDown);
		}

		static SortedSet<string> LoadGlbStaffDbUsers(string script, DbConnection connection)
		{
			var dbUsers = new SortedSet<string>();
			using (var cmd = connection.Command(script))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					dbUsers.Add((string)reader[0]);
				}
			}

			return dbUsers;
		}

		const string TestDomainName = "sand.wtg.zone";

		static class ADTestUserLongNameB
		{
			public const string Guid = "cf37d78c-4d08-44a4-a0df-1bc92be23d99";
			public const string Name = "12345678901234567890B";
			public const string NameWithDomain = "12345678901234567890B@sand.wtg.zone";
			public const string NameWithDomainPreWindows2000 = "sand\\12345678901234567802";
			public const string Password = "Changeme1234";
		}

		static class ADTestUserLongNameC
		{
			public const string Guid = "60003917-fe84-4961-b196-65bd8764276b";
			public const string Name = "12345678901234567890C";
			public const string NameWithDomain = "12345678901234567890C@sand.wtg.zone";
			public const string NameWithDomainPreWindows2000 = "sand\\12345678901234567803";
			public const string Password = "Changeme1234";
		}
	}
}
