using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing.Global.Staff.GlbStaff.DBUserManager;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static System.FormattableString;
using ADTestAdmin = CargoWise.ActiveDirectory.TestFramework.TestConstants.ADTestAdminAccount;
using ADTests = CargoWise.ActiveDirectory.TestFramework.TestConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	[RequiresSoftware(RequiredSoftware.IsVM)]
	class DbUserManagerTests : DbUserManagerBaseTest
	{
		[DeveloperOnlyTest]
		public void TestSynchroniseAllStaffAndDbLogins()
		{
			var testStaffLoginNames = Enumerable.Range(1, 1).Select(x => Invariant($"DbUserManagerTestStaff_{x}")).ToArray();

			var domainCredentials = ObjectFactory.New<IDomainCredentials>();
			domainCredentials.DomainName = ADTests.Domain;
			domainCredentials.DomainUserName = ADTestAdmin.NameWithDomain;
			domainCredentials.DomainUserPassword = ADTestAdmin.Password;
			domainCredentials.IsDefaultDomain = true;
			domainCredentials.UserOrganisationalUnit = ADTests.ValidOU;
			domainCredentials.GroupOrganisationalUnit = ADTests.ValidOU;
			domainCredentials.DefaultPassword = "Chang!me1234";

			var adRegistryMock = new Mock<IADRegistry>();
			adRegistryMock.Setup(x => x.DefaultDomainCredentials).Returns(domainCredentials);
			adRegistryMock.Setup(x => x.IsIntegrationEnabled).Returns(true);
			EnvProxy.SetHostedLocationForTest("www.YourPlace.com");

			using (ObjectFactory.Substitute(adRegistryMock.Object))
			using (var adTestUser = new DisposableAdTestUser())
			{
				Assert(EnvProxy.IsHostedWithCargowise);
				Assert(ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProviderMock = new Mock<IADEntityProvider>();
				adEntityProviderMock.Setup(x => x
					.GetADUser(It.IsAny<GlbStaff>()))
					.Returns((IGlbStaff glbStaff) =>
					{
						if (!(glbStaff is GlbStaff staff))
						{
							return null;
						}

						var adUserMock = new Mock<IADUser>();
						adUserMock.Setup(x => x.HasExistingDirectoryEntry()).Returns(true);
						adUserMock.Setup(x => x.PasswordMustChangeAtNextLogon).Returns(false);
						adUserMock.Setup(x => x.LockedOut).Returns(false);
						adUserMock.Setup(x => x.DomainCredentials).Returns(domainCredentials);
						adUserMock.Setup(x => x.IsIdentityInConflict()).Returns(false);
						adUserMock.Setup(x => x.DomainNetBiosName).Returns(ADTests.DomainPreWin2000);
						adUserMock.Setup(x => x.LoginName).Returns(staff.GS_LoginName);
						adUserMock.Setup(x => x.SAMAccountName).Returns(adTestUser.SAMAccountName);

						return adUserMock.Object;
					});

				var factory = new BusinessObjectFactory(TestConnection);

				using (ObjectFactory.Substitute(adEntityProviderMock.Object))
				using (BiServers.TemporarilySetAuditServerToNull())
				using (BiServers.TemporarilySetDataWarehouseServerToNull())
				{
					// Pre-requisites
					var staffs =
						CreateGlbStaffUsersHavingDatabaseDeveloperRole(testStaffLoginNames)
							.Concat(CreateGlbStaffUsersHavingDatabaseDeveloperRole(
								new[] { adTestUser.UserName },
								staff =>
								{
									staff.GS_ActiveDirectoryObjectGuid = new ZGuid(adTestUser.ObjectGuid);
								})).ToArray();
					factory.Save();

					DropStaffDbUserAndLoginsIfExist(staffs);
					AssertStaffLoginExists("Staff logins have not been created yet", assertExists: false, staffs);

					try
					{
						// Arrange
						var dbUserManager = new DbUserManager();
						dbUserManager.SynchroniseAllStaffAndDbLogins(TestConnection);
						AssertStaffLoginExists("Staff logins have just been created", assertExists: true, staffs);

						adTestUser.ChangeSamAccountName();
						AssertAdUserExists();
						adEntityProviderMock.Invocations.Clear();

						// Act
						dbUserManager.SynchroniseAllStaffAndDbLogins(TestConnection);

						// Assert
						AssertStaffLoginExists("Staff logins should have been synchronized to all databases", assertExists: true, staffs);
					}
					finally
					{
						DropStaffDbUserAndLoginsIfExist(staffs);
					}
				}

				void AssertAdUserExists()
				{
					var maxWaitTimeSpan = TimeSpan.FromSeconds(30);
					var cancellationTokenSource = new CancellationTokenSource(maxWaitTimeSpan);
					while (!cancellationTokenSource.IsCancellationRequested)
					{
						var sid = TestConnection.ExecuteScalar($"SELECT SUSER_SID('{adTestUser.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}')");
						if (sid != null && sid != DBNull.Value)
						{
							break;
						}

						Thread.Sleep(100);
					}
				}

				void AssertStaffLoginExists(string assertionMessage, bool assertExists, IEnumerable<GlbStaff> staffs)
				{
					CombineAssertions(assertionMessage, () =>
					{
						var dbUserManager = new DbUserManager();

						foreach (var staff in staffs)
						{
							var staffLoginName = dbUserManager.GetFullUserLoginName_Exposed(
								staff.GS_LoginName,
								staff.GetDownLevelLogonName,
								staff.IsADLinked ? DbUserManager.DatabaseAuthenticationMode.Windows : DbUserManager.DatabaseAuthenticationMode.Sql);
							AssertEquals($"Staff login: '{staffLoginName}' expected exist: {assertExists}", assertExists, CheckLoginExistsWithRealLoginName(staffLoginName));
						}
					});
				}

				IEnumerable<GlbStaff> CreateGlbStaffUsersHavingDatabaseDeveloperRole(IEnumerable<string> loginNames, Action<GlbStaff> staffAction = null)
				{
					foreach (var loginName in loginNames)
					{
						var staff = GetNewStaff(factory, loginName);
						staff.IsDatabaseDeveloper = true;
						staffAction?.Invoke(staff);

						yield return staff;
					}
				}
			}
		}
	}
}
