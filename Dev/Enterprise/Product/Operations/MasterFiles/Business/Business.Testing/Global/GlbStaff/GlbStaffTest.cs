using System;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	partial class GlbStaffTest : EnterpriseBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public class TestDSAIsNotNudgedOnFactorySaveWhenModerSecurityIsOff : TestCase
		{
			[ExpectNoExceptions]
			public void TestDSAIsNotNugedIfDatabaseAccessRoleIsRemoved()
			{
				// Arrange
				var staffReader = Factory.New<GlbStaff>();
				staffReader.GS_LoginName = "BobReader";
				staffReader.GS_FullName = "Bob Reader";
				staffReader.IsDatabaseDeveloper = false;
				staffReader.IsReadOnlyDBUser = true;
				staffReader.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();

					staffReader.IsReadOnlyDBUser = false;

					// Act
					Factory.Save();

					// Assert
					try
					{
						serviceTaskNudgerMock.Verify(
							nudger => nudger.NudgeServiceTask("DSA", null),
							Times.Never(),
							"Should not nudge DSA if modern security is off.");
					}
					finally
					{
						staffReader.Delete();
						Factory.Save();
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNugedIfDatabaseAccessRoleIsAdded()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = false;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();

					staffDeveloper.IsDatabaseDeveloper = true;

					// Act
					Factory.Save();

					// Assert
					try
					{
						serviceTaskNudgerMock.Verify(
							nudger => nudger.NudgeServiceTask("DSA", null),
							Times.Never(),
							"Should not nudge DSA if modern security is off.");
					}
					finally
					{
						staffDeveloper.Delete();
						Factory.Save();
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNugedIfNewStaffWithDatabaseAccessRoleIsCreated()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = true;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				// Act
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
				}

				// Assert
				try
				{
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Never(),
						"Should not nudge DSA if modern security is off.");
				}
				finally
				{
					staffDeveloper.Delete();
					Factory.Save();
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNugedIfStaffWithDatabaseAccessRoleIsDeleted()
			{
				// Arrange
				var staffDeveloperAndReader = Factory.New<GlbStaff>();
				staffDeveloperAndReader.GS_LoginName = "BobDeveloperAndReader";
				staffDeveloperAndReader.GS_FullName = "Bob DeveloperAndReader";
				staffDeveloperAndReader.IsDatabaseDeveloper = true;
				staffDeveloperAndReader.IsReadOnlyDBUser = true;
				staffDeveloperAndReader.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					staffDeveloperAndReader.Delete();

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
					nudger => nudger.NudgeServiceTask("DSA", null),
					Times.Never(),
					"Should not nudge DSA if modern security is off.");
				}
			}

			AdminConnection adminConnection;
			BusinessObjectFactory factory;
			IDisposable dropUserRepository;

			BusinessObjectFactory Factory
			{
				get
				{
					if (factory == null)
					{
						factory = new BusinessObjectFactory(AdminConnection);
					}

					return factory;
				}
			}

			AdminConnection AdminConnection
			{
				get
				{
					if (adminConnection == null)
					{
						adminConnection = Db.NewAdminConnection();
					}

					return adminConnection;
				}
			}

			protected override void SetUp()
			{
				base.SetUp();
				using (var connection = Db.NewAdminConnection())
				{
					// make sure we have all databases created and that we may need for testing BEFORE transaction scope open
					var userRepositoryDb = $"{Db.DatabaseName}{DbUserRepository.RepositoryDbSuffix}";
					if (!connection.DatabaseExists(userRepositoryDb))
					{
						dropUserRepository = new DisposableAction(() =>
						{
							using (var adminConnection = Db.NewAdminConnection())
							{
								AdoTestUtils.DropDbIfExists(adminConnection, userRepositoryDb);
							}
						});

						(new DbUserRepository()).CreateRepositoryDatabase();
					}
				}

				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			}

			protected override void TearDown()
			{
				adminConnection?.Dispose();
				adminConnection = null;
				dropUserRepository?.Dispose();

				base.TearDown();
			}
		}

		[UseSnapshotProtection]
		public class TestDSAIsNudgedOnFactorySaveWhenRequiredIfModerSecurityIsOn : TestCase
		{
			[ExpectNoExceptions]
			public void TestDSAIsNudgedIfLoginNameForStaffRecordWithDatabaseAccessIsChanged()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = true;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.GS_LoginName = "BobDeveloper1";

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Once(),
						"Should nudge DSA if login name for GlbStaff with database access is changed.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNudgedAfterLoadingStaffInFactoryAndChangingLoginNameForStaffRecordWithDatabaseAccess()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = true;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					var factory = new BusinessObjectFactory();

					staffDeveloper = factory.Load<GlbStaff>(staffDeveloper.PK);

					staffDeveloper.GS_LoginName = "BobDeveloper1";
					serviceTaskNudgerMock.Invocations.Clear();

					// Act
					factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Once(),
						"Should nudge DSA if login name for GlbStaff with database access is changed.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNudgedIfLoginNameForStaffRecordWithoutDatabaseAccessIsChanged()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = false;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.GS_LoginName = "BobDeveloper1";

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Never(),
						"Should not nudge DSA if login name is changed GlbStaff without database access.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNudgedIfDomainNameForADIntegratedStaffRecordWithDatabaseAccessIsChanged()
			{
				// Arrange
				var adRegistryMock = new Mock<IADRegistry>();
				adRegistryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				using (ObjectFactory.Substitute(adRegistryMock.Object))
				using (ObjectFactory.Substitute(GetADEntityProviderMockWithADTestUserAccount()))
				{
					AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

					var staffDeveloper = Factory.New<GlbStaff>();
					staffDeveloper.GS_LoginName = TestConstants.ADTestUserAccount.Name;
					staffDeveloper.GS_DomainName = TestConstants.Domain;
					staffDeveloper.GS_ActiveDirectoryObjectGuid = Guid.Parse(TestConstants.ADTestUserAccount.Guid);
					staffDeveloper.GS_FullName = "Bob Developer";
					staffDeveloper.IsDatabaseDeveloper = true;
					staffDeveloper.IsReadOnlyDBUser = false;
					staffDeveloper.IsBackupOperator = false;

					var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

					using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
					{
						Factory.Save();
						serviceTaskNudgerMock.Invocations.Clear();

						staffDeveloper.DomainName = "Sand1";

						// Act
						Factory.Save();

						// Assert
						serviceTaskNudgerMock.Verify(
							nudger => nudger.NudgeServiceTask("DSA", null),
							Times.Once(),
							"Should nudge DSA if domain name for GlbStaff with database access is changed.");
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNudgedIfDomainNameForADIntegratedStaffRecordWithoutDatabaseAccessIsChanged()
			{
				// Arrange
				var adRegistryMock = new Mock<IADRegistry>();
				adRegistryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				using (ObjectFactory.Substitute(adRegistryMock.Object))
				using (ObjectFactory.Substitute(GetADEntityProviderMockWithADTestUserAccount()))
				{
					AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

					var staffDeveloper = Factory.New<GlbStaff>();
					staffDeveloper.GS_LoginName = TestConstants.ADTestUserAccount.Name;
					staffDeveloper.GS_DomainName = TestConstants.Domain;
					staffDeveloper.GS_ActiveDirectoryObjectGuid = Guid.Parse(TestConstants.ADTestUserAccount.Guid);
					staffDeveloper.GS_FullName = "Bob Developer";
					staffDeveloper.IsDatabaseDeveloper = false;
					staffDeveloper.IsReadOnlyDBUser = false;
					staffDeveloper.IsBackupOperator = false;

					var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

					using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
					{
						Factory.Save();
						serviceTaskNudgerMock.Invocations.Clear();

						staffDeveloper.DomainName = "Sand1";

						// Act
						Factory.Save();

						// Assert
						serviceTaskNudgerMock.Verify(
							nudger => nudger.NudgeServiceTask("DSA", null),
							Times.Never(),
							"Should not nudge DSA if domain name is changed for GlbStaff without database accesess.");
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNudgedIfActiveDirectoryObjectGuidForADIntegratedStaffRecordWithDatabaseAccessIsChanged()
			{
				// Arrange
				var adRegistryMock = new Mock<IADRegistry>();
				adRegistryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				using (ObjectFactory.Substitute(adRegistryMock.Object))
				using (ObjectFactory.Substitute(GetADEntityProviderMockWithADTestUserAccount()))
				{
					AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

					var staffDeveloper = Factory.New<GlbStaff>();
					staffDeveloper.GS_LoginName = TestConstants.ADTestUserAccount.Name;
					staffDeveloper.GS_DomainName = TestConstants.Domain;
					staffDeveloper.GS_ActiveDirectoryObjectGuid = Guid.Parse(TestConstants.ADTestUserAccount.Guid);
					staffDeveloper.GS_FullName = "Bob Developer";
					staffDeveloper.IsDatabaseDeveloper = true;
					staffDeveloper.IsReadOnlyDBUser = false;
					staffDeveloper.IsBackupOperator = false;

					var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

					using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
					{
						Factory.Save();
						serviceTaskNudgerMock.Invocations.Clear();

						staffDeveloper.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

						// Act
						Factory.Save();

						// Assert
						serviceTaskNudgerMock.Verify(
							nudger => nudger.NudgeServiceTask("DSA", null),
							Times.Once(),
							"Should nudge DSA if domain name for GlbStaff with database access is changed.");
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNudgedIfActiveDirectoryObjectGuidForADIntegratedStaffRecordWithoutDatabaseAccessIsChanged()
			{
				// Arrange
				var adRegistryMock = new Mock<IADRegistry>();
				adRegistryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				using (ObjectFactory.Substitute(adRegistryMock.Object))
				using (ObjectFactory.Substitute(GetADEntityProviderMockWithADTestUserAccount()))
				{
					AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

					var staffDeveloper = Factory.New<GlbStaff>();
					staffDeveloper.GS_LoginName = TestConstants.ADTestUserAccount.Name;
					staffDeveloper.GS_DomainName = TestConstants.Domain;
					staffDeveloper.GS_ActiveDirectoryObjectGuid = Guid.Parse(TestConstants.ADTestUserAccount.Guid);
					staffDeveloper.GS_FullName = "Bob Developer";
					staffDeveloper.IsDatabaseDeveloper = false;
					staffDeveloper.IsReadOnlyDBUser = false;
					staffDeveloper.IsBackupOperator = false;

					var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

					using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
					{
						Factory.Save();
						serviceTaskNudgerMock.Invocations.Clear();

						staffDeveloper.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

						// Act
						Factory.Save();

						// Assert
						serviceTaskNudgerMock.Verify(
							nudger => nudger.NudgeServiceTask("DSA", null),
							Times.Never(),
							"Should not nudge DSA if domain name is changed for GlbStaff without database accesess.");
					}
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNudgedIfStaffRecordWithDatabaseAccessIsDeactivated()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = true;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;
				staffDeveloper.GS_IsActive = true;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.GS_IsActive = false;

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Once(),
						"Should nudge DSA if GlbStaff with database access is deactivated.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNudgedIfStaffRecordWithoutDatabaseAccessIsDeactivated()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = false;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;
				staffDeveloper.GS_IsActive = true;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.GS_IsActive = false;

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Never(),
						"Should not nudge DSA if GlbStaff without database access is deactivated.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNudgedIfStaffRecordWithoutDatabaseAccessIsActivated()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = false;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;
				staffDeveloper.GS_IsActive = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.GS_IsActive = true;

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Never(),
						"Should not nudge DSA if GlbStaff without databse access is activated.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNudgedIfStaffRecordIsAlteredButNoDatabaseAccessRoleLoginNamePasswordOrDomainNameIsChanged()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = true;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.GS_IsDriver = true;

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Never(),
						"Should not nudge DSA if GlbStaff with database access is altered but no Login name, Domain or AD Guid is changed.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNugedIfDatabaseAccessRoleIsRemoved()
			{
				// Arrange
				var staffReader = Factory.New<GlbStaff>();
				staffReader.GS_LoginName = "BobReader";
				staffReader.GS_FullName = "Bob Reader";
				staffReader.IsDatabaseDeveloper = false;
				staffReader.IsReadOnlyDBUser = true;
				staffReader.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffReader.IsReadOnlyDBUser = false;

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						"Should nudge DSA if database access role is removed from GlbStaff.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNugedIfDatabaseAccessRoleIsAdded()
			{
				// Arrange
				var staffDeveloper = Factory.New<GlbStaff>();
				staffDeveloper.GS_LoginName = "BobDeveloper";
				staffDeveloper.GS_FullName = "Bob Developer";
				staffDeveloper.IsDatabaseDeveloper = false;
				staffDeveloper.IsReadOnlyDBUser = false;
				staffDeveloper.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					staffDeveloper.IsDatabaseDeveloper = true;

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						"Should nudge DSA if database access role is added to GlbStaff.");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNugedIfNewStaffWithDatabaseAccessRoleIsCreated()
			{
				// Arrange
				var staffBackupOperator = Factory.New<GlbStaff>();
				staffBackupOperator.GS_LoginName = "BobBackupOperator";
				staffBackupOperator.GS_FullName = "Bob BackupOperator";
				staffBackupOperator.IsDatabaseDeveloper = false;
				staffBackupOperator.IsReadOnlyDBUser = false;
				staffBackupOperator.IsBackupOperator = true;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				// Act
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
				}

				// Assert
				serviceTaskNudgerMock.Verify(
					nudger => nudger.NudgeServiceTask("DSA", null),
					"Should nudge DSA if GlbStaff with database access role added.");
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNugedIfNewStaffWithoutDatabaseAccessRoleIsCreated()
			{
				// Arrange
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "Bob";
				staff.GS_FullName = "Bob";
				staff.IsDatabaseDeveloper = false;
				staff.IsReadOnlyDBUser = false;
				staff.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				// Act
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
				}

				// Assert
				serviceTaskNudgerMock.Verify(
					nudger => nudger.NudgeServiceTask("DSA", null),
					Times.Never(),
					"Should not nudge DSA if new GlbStaff is added without database access.");
			}

			[ExpectNoExceptions]
			public void TestDSAIsNugedIfStaffWithDatabaseAccessRoleIsDeleted()
			{
				// Arrange
				var staffDeveloperAndReader = Factory.New<GlbStaff>();
				staffDeveloperAndReader.GS_LoginName = "BobDeveloperAndReader";
				staffDeveloperAndReader.GS_FullName = "Bob DeveloperAndReader";
				staffDeveloperAndReader.IsDatabaseDeveloper = true;
				staffDeveloperAndReader.IsReadOnlyDBUser = true;
				staffDeveloperAndReader.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();
					staffDeveloperAndReader.Delete();

					// Act
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						"Should nudge DSA if GlbStaff with database access is deleted");
				}
			}

			[ExpectNoExceptions]
			public void TestDSAIsNotNugedIfStaffWithoutDatabaseAccessRoleIsDeleted()
			{
				// Arrange
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "Bob";
				staff.GS_FullName = "Bob";
				staff.IsDatabaseDeveloper = false;
				staff.IsReadOnlyDBUser = false;
				staff.IsBackupOperator = false;

				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					Factory.Save();
					serviceTaskNudgerMock.Invocations.Clear();

					// Act
					staff.Delete();
					Factory.Save();

					// Assert
					serviceTaskNudgerMock.Verify(
						nudger => nudger.NudgeServiceTask("DSA", null),
						Times.Never(),
						"Should not nudge DSA if staff without database access is deleted.");
				}
			}

			AdminConnection adminConnection;
			BusinessObjectFactory factory;

			BusinessObjectFactory Factory
			{
				get
				{
					if (factory == null)
					{
						factory = new BusinessObjectFactory(AdminConnection);
					}

					return factory;
				}
			}

			AdminConnection AdminConnection
			{
				get
				{
					if (adminConnection == null)
					{
						adminConnection = Db.NewAdminConnection();
					}

					return adminConnection;
				}
			}

			protected override void SetUp()
			{
				base.SetUp();
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			}

			protected override void TearDown()
			{
				adminConnection?.Dispose();
				adminConnection = null;

				base.TearDown();
			}
		}

		public void TestICertificatesValidationProvider_GetValidation_NotExistingCountry()
		{
			var glbStaff = Factory.New<GlbStaff>();
			var provider = (ICertificatesValidationProvider)glbStaff;
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = "AA";
			AssertNull(provider.GetValidation(certificate));
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidate_GlbGroupEditSUPHostedAlreadySUP() //shouldn't be allowed to set it to sup if it's already sup - should be forced to change it
		{
			var location = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");

			var factory = new BusinessObjectFactory();

			var glbStaff = factory.New<GlbStaff>();
			glbStaff.FillWithValidTestData();
			glbStaff.GS_LoginName = "Tester";

			var glbGroup = factory.New<GlbGroup>();
			glbGroup.FillWithValidTestData();
			glbGroup.GG_Code = "SUP";
			var supPk = glbGroup.PK.ToGuid();

			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext("Tester", Environment.Env.CurrentBranchPK, Environment.Env.CurrentDepartmentPK))
			{
				OptionalDataType.Validate(
				new GuidRegistryItem(
					"Test",
					(MultilingualString)null,
					null,
					null,
					new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					supPk),
				supPk, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			EnvProxy.SetHostedLocationForTest(location);
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestValidate_GlbGroupEditSUPHosted()
		{
			var location = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");

			var factory = new BusinessObjectFactory();

			var glbStaff = factory.New<GlbStaff>();
			glbStaff.FillWithValidTestData();
			glbStaff.GS_LoginName = "Tester";

			var glbGroup = factory.New<GlbGroup>();
			glbGroup.FillWithValidTestData();
			glbGroup.GG_Code = "SUP";

			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext("Tester", Environment.Env.CurrentBranchPK, Environment.Env.CurrentDepartmentPK))
			{
				OptionalDataType.Validate(
				new GuidRegistryItem(
					"Test",
					(MultilingualString)null,
					null,
					null,
					new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					Guid.Empty),
				glbGroup.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}
			EnvProxy.SetHostedLocationForTest(location);
		}

		[ExpectNoExceptions]
		public void TestValidate_GlbGroupEditSUPHostedAsSupport()
		{
			var location = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");

			var factory = new BusinessObjectFactory();
			var glbGroup = factory.New<GlbGroup>();
			glbGroup.FillWithValidTestData();
			glbGroup.GG_Code = "SUP";
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext("CWSupport", Environment.Env.CurrentBranchPK, Environment.Env.CurrentDepartmentPK))
			{
				OptionalDataType.Validate(
					new GuidRegistryItem(
						"Test",
						(MultilingualString)null,
						null,
						null,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Guid.Empty),
					glbGroup.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}
			EnvProxy.SetHostedLocationForTest(location);
		}

		[ExpectNoExceptions]
		public void TestValidate_GlbGroupEditSUPNonHosted()
		{
			var location = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("NCW");

			var factory = new BusinessObjectFactory();
			var glbGroup = factory.New<GlbGroup>();
			glbGroup.FillWithValidTestData();
			glbGroup.GG_Code = "SUP";
			factory.Save();

			OptionalDataType.Validate(
				new GuidRegistryItem(
					"Test",
					(MultilingualString)null,
					null,
					null,
					new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					Guid.Empty),
				glbGroup.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			EnvProxy.SetHostedLocationForTest(location);
		}

		public void TestStaffExcelPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Tester";
			staff.GS_Code = "SSS";
			staff.ExcelPasswordForOpening = "OpeningPassword";
			staff.ExcelPasswordForModifying = "ModifyingPassword";
			Factory.Save();

			var staff1 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbStaff>(staff.PK);

			AssertEquals(true, staff1.ExcelPasswordForOpening_ReadOnly);
			AssertEquals(true, staff1.ExcelPasswordForModifying_ReadOnly);
			AssertEquals(staff1.ViewDeniedMessage, staff1.ExcelPasswordForOpening);
			AssertEquals(staff1.ViewDeniedMessage, staff1.ExcelPasswordForModifying);

			using (EnvProxy.Instance.SetTemporaryUserContext("Tester", Environment.Env.CurrentBranchPK, Environment.Env.CurrentDepartmentPK))
			{
				AssertEquals(false, staff1.ExcelPasswordForOpening_ReadOnly);
				AssertEquals(false, staff1.ExcelPasswordForOpening_ReadOnly);
				AssertEquals("OpeningPassword", staff1.ExcelPasswordForOpening);
				AssertEquals("ModifyingPassword", staff1.ExcelPasswordForModifying);
			}
		}

		#region Implementation

		GuidRegistryDataType OptionalDataType
		{
			get
			{
				if (optionalDataType == null)
				{
					optionalDataType = (GuidRegistryDataType)new GuidRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueOptional).DataType;
				}
				return optionalDataType;
			}
		}

		GuidRegistryDataType optionalDataType;

		public static IDomainCredentials TestDomainCredentials
		{
			get
			{
				var testDomainsCredentialsMock = new Mock<IDomainCredentials>();
				testDomainsCredentialsMock.CallBase = true;

				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainName).Returns(TestConstants.Domain);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainUserName).Returns(TestConstants.ADTestAdminAccount.NameWithDomain);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.DomainUserPassword).Returns(TestConstants.ADTestAdminAccount.Password);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.IsDefaultDomain).Returns(true);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.UserOrganisationalUnit).Returns(TestConstants.ValidOU);
				testDomainsCredentialsMock.SetupGet(domainCredentials => domainCredentials.GroupOrganisationalUnit).Returns(TestConstants.ValidOU);

				return testDomainsCredentialsMock.Object;
			}
		}

		public static IADEntityProvider GetADEntityProviderMockWithADTestUserAccount()
		{
			var adEntityProviderMock = new Mock<IADEntityProvider>();

			var adUserMock = new Mock<IADUser>();
			adUserMock.SetupGet(adUser => adUser.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
			adUserMock.SetupGet(adUser => adUser.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
			adUserMock.Setup(adUser => adUser.HasExistingDirectoryEntry()).Returns(true);

			adEntityProviderMock.Setup(entityProvider => entityProvider.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adUserMock.Object);
			adUserMock.Setup(adUser => adUser.DomainCredentials).Returns(TestDomainCredentials);

			return adEntityProviderMock.Object;
		}

		#endregion Implementation
	}
}
