using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	class DbUserManagerSetPasswordForStaffTest : TestCase
	{
		public void TestSqlPasswordHashIsSetToCorrectValue()
		{
			// Arrange
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";
			var dbUserManager = new DbUserManager();

			var staff = CreateStaffUserWithDbAccess(staffCode, staffLoginName);

			// Act
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);
			staff.Factory.Save();

			// Assert
			AssertNotEquals(
				DBNull.Value,
				Db.Connection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));

			var factory = new BusinessObjectFactory();
			var staffLoaded = factory.Load<GlbStaff>(staff.PK);

			Assert(Db.Connection.ExecuteScalar<bool>(
				"SELECT CAST(PWDCOMPARE(@password, GS_SqlLoginPasswordHash) AS bit) FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd =>
				{
					cmd.AddParameter("@password", System.Data.SqlDbType.NVarChar, 128, staffSqlLoginPassword);
					cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName);
				}));
		}

		public void TestSqlPasswordHashIsSetToSqlLogin_ModernSecurityIsOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";

			var dbUserManager = new DbUserManager();

			var staff = CreateStaffUserWithDbAccess(staffCode, staffLoginName);

			// Act
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);
			staff.Factory.Save();

			// Assert
			using (var adminConnection = Db.NewAdminConnection())
			{
				Assert(
					adminConnection.ExecuteScalar<bool>(
						@"
SELECT CAST(IIF(staff.GS_SqlLoginPasswordHash = login.password_hash, 1, 0) AS bit)
FROM dbo.GlbStaff AS staff
	JOIN sys.sql_logins AS login ON login.name = @sqlLoginName
WHERE staff.GS_LoginName = @loginName
",
						cmd =>
						{
							cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, staffLoginName);
							cmd.AddParameter("@sqlLoginName", System.Data.SqlDbType.NVarChar, 128, staffSqlLoginName);
						}));
			}
		}

		[ExpectNoExceptions]
		public void TestDSAIsNudgedAfterSettingSqlPasssword_ModernSecurityIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var dbUserManager = new DbUserManager();

			var staff = CreateStaffUserWithDbAccess(staffCode, staffLoginName);
			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

			// Act				
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);
				staff.Factory.Save();

				// Assert
				serviceTaskNudgerMock.Verify(
					nudger => nudger.NudgeServiceTask("DSA", null),
					Times.Once(),
					"Should nudge DSA after setting sql password.");
			}
		}

		[ExpectNoExceptions]
		public void TestDSAIsNudgedAfterLoadingStaffInFactoryAndSettingSqlPasssword_ModernSecurityIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var dbUserManager = new DbUserManager();

			var staff = CreateStaffUserWithDbAccess(staffCode, staffLoginName);
			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();

			var factory = new BusinessObjectFactory();

			staff = factory.Load<GlbStaff>(staff.PK);

			// Act				
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);
				staff.Factory.Save();

				// Assert
				serviceTaskNudgerMock.Verify(
					nudger => nudger.NudgeServiceTask("DSA", null),
					Times.Once(),
					"Should nudge DSA after setting sql password.");
			}
		}

		public void TestCanLoginWhenSqlPasswordIsSet_ModernSecurityIsOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";

			var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";

			var dbUserManager = new DbUserManager();

			var staff = CreateStaffUserWithDbAccess(staffCode, staffLoginName);

			// Act
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);
			staff.Factory.Save();

			// Assert
			using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, staffSqlLoginName, staffSqlLoginPassword))
			{
				AssertNoExceptionThrown(() => connection.EnsureIsOpen());
			}
		}

		#region Implementation

		const string staffLoginName = "Tester_67f37fa5-0091-4af6-91c5-1b794ad1f115";

		static GlbStaff CreateStaffUserWithDbAccess(string staffCode, string staffLoginName)
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var factory = new BusinessObjectFactory();
				var staff = factory.New<GlbStaff>();
				staff.GS_FullName = staffLoginName;
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
		}

		void CleanUpLogin()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";
				adminConnection.ExecuteNonQuery(@$"
IF EXISTS (SELECT null FROM sys.sql_logins WHERE name = N'{staffSqlLoginName.QuoteEscapedName('\'')}')
	DROP LOGIN {staffSqlLoginName.QuoteName()};
");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CleanUpLogin();
		}

		protected override void TearDown()
		{
			CleanUpLogin();
			base.TearDown();
		}

		#endregion Implementation
	}
}
