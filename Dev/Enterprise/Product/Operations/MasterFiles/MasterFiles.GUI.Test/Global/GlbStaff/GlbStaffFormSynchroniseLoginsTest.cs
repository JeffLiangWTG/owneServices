using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[UseSnapshotProtection]
	sealed class GlbStaffFormSynchroniseLoginsTest : TestCase
	{
		const string testStaff1Name = "TestStaff1ForSyncLogins";
		const string testStaff2Name = "TestStaff2ForSyncLogins";

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		readonly DbConnection TestConnection = Db.Connection;

		public void TestSynchroniseLogins()
		{
			// LLZ: Since login synchronisation is done by DSA task when UseModerSqlSecuritySystem flag is on,
			// this test is irrelevant
			// Once flag UseModerSqlSecuritySystem is droped this test should be removed
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = testStaff1Name;
			staff1.GS_Code = "SZ1";
			staff1.GS_FullName = testStaff1Name;
			staff1.GS_UserAddress1 = "address";
			staff1.GS_City = "city";
			staff1.GS_IsActive = true;
			staff1.IsReadOnlyDBUser = staff1.IsDatabaseDeveloper = staff1.IsBackupOperator = false;
			staff1.GS_GB_HomeBranch = branch.PK;
			staff1.GS_GE_HomeDepartment = department.PK;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = testStaff2Name;
			staff2.GS_Code = "SZ2";
			staff2.GS_FullName = testStaff2Name;
			staff2.GS_UserAddress1 = "address";
			staff2.GS_City = "city";
			staff2.GS_IsActive = true;
			staff2.IsReadOnlyDBUser = staff2.IsDatabaseDeveloper = staff2.IsBackupOperator = false;
			staff2.GS_GB_HomeBranch = branch.PK;
			staff2.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals("Staff1 LoginExists", false, DbUserManagerForTesting.CheckLoginExists(staff1.GS_LoginName, adminConnection, readUncommitted: true));
				AssertEquals("Staff2 LoginExists", false, DbUserManagerForTesting.CheckLoginExists(staff2.GS_LoginName, adminConnection, readUncommitted: true));

				using (GlbStaffForm form = new GlbStaffForm(staff1))
				{
					form.Show();
					form.Close();
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("Form was cancelled, staff login should not be created", false, DbUserManagerForTesting.CheckLoginExists(staff1.GS_LoginName, adminConnection, readUncommitted: true));
				AssertEquals("Staff2 login should not be created", false, DbUserManagerForTesting.CheckLoginExists(staff2.GS_LoginName, adminConnection, readUncommitted: true));

				staff1.IsReadOnlyDBUser = true;

				using (GlbStaffForm form = new GlbStaffForm(staff1))
				{
					form.Show();
					staff1.GS_HomePhone_Formatted = "+61 2 8001 2200";
					form.FireSaveButton();
					form.Close();
					Assert("No error message Shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				AssertEquals("Staff1 LoginExists", true, DbUserManagerForTesting.CheckLoginExists(staff1.GS_LoginName, adminConnection, readUncommitted: true));
				AssertEquals("Staff1 UserRights", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(Db.DatabaseName, staff1.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, TestConnection));
				AssertEquals("Staff2 login should not be created", false, DbUserManagerForTesting.CheckLoginExists(staff2.GS_LoginName, adminConnection, readUncommitted: true));

				staff1.IsReadOnlyDBUser = false;

				using (GlbStaffForm form = new GlbStaffForm(staff1))
				{
					form.Show();
					form.Close();
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("Staff1 LoginExists", true, DbUserManagerForTesting.CheckLoginExists(staff1.GS_LoginName, adminConnection, readUncommitted: true));
				AssertEquals("Staff1 UserRights", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(Db.DatabaseName, staff1.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, TestConnection));
				AssertEquals("Staff2 LoginExists", false, DbUserManagerForTesting.CheckLoginExists(staff2.GS_LoginName, adminConnection, readUncommitted: true));

				staff1.IsReadOnlyDBUser = true;

				using (GlbStaffForm form = new GlbStaffForm(staff2))
				{
					form.Show();
					staff2.IsReadOnlyDBUser = true;
					form.FireSaveButton();
					form.Close();
					Assert("No error message Shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				AssertEquals("Staff1 LoginExists", true, DbUserManagerForTesting.CheckLoginExists(staff1.GS_LoginName, adminConnection, readUncommitted: true));
				AssertEquals("Staff1 UserRights", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(Db.DatabaseName, staff1.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, TestConnection));
				AssertEquals("Staff2 LoginExists", true, DbUserManagerForTesting.CheckLoginExists(staff2.GS_LoginName, adminConnection, readUncommitted: true));
				AssertEquals("Staff2 UserRights", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(Db.DatabaseName, staff2.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, TestConnection));

				DbUserManagerForTesting.CreateUserSchemaOnMainDb(staff1.GS_LoginName, adminConnection);
				DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(staff1.GS_LoginName, TestConnection);

				using (GlbStaffForm form = new GlbStaffForm(staff1))
				{
					form.Show();
					staff1.IsReadOnlyDBUser = false;
					staff1.GS_HomePhone_Formatted = "+61 2 8001 2201";
					form.FireSaveButton();
					Assert("error message shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Failed to synchronize logins, please contact your System Administrator and report the following error:"));
				}

				DbUserManagerForTesting.DropDummyTableForSchemaOnMainDb(staff1.GS_LoginName, TestConnection);
				DbUserManagerForTesting.DropUserSchemaOnMainDb(staff1.GS_LoginName, TestConnection);
				staff1.Delete();
				staff2.Delete();
				Factory.Save();
			}
		}

		public void TestChangeLoginName_WhenConflictWithAD_ShouldNotSave()
		{
			AssertWhenIdentityConflict_ShouldContinueWithSave(true, ContinueWithSave.No);
		}

		public void TestChangeLoginName_WhenNoConflictWithAD_ShouldSave()
		{
			AssertWhenIdentityConflict_ShouldContinueWithSave(false, ContinueWithSave.Yes);
		}

		void AssertWhenIdentityConflict_ShouldContinueWithSave(bool isIdentityInConflict, ContinueWithSave shouldContinueWithSave)
		{
			var adEntityProvider = new Mock<IADEntityProvider>(MockBehavior.Strict);
			var adUser = new Mock<IADUser>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Hobbiton";
			staff.GS_FullName = "Bilbo Baggins";
			staff.GS_UserAddress1 = "1 Bag End";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_GB_HomeBranch = Factory.NewWithValidTestData<GlbBranch>().PK;
			staff.GS_GE_HomeDepartment = Factory.NewWithValidTestData<GlbDepartment>().PK;
			Factory.Save();

			ObjectFactory.Substitute(adEntityProvider.Object);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			adEntityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
			adEntityProvider.Setup(m => m.GetADGroup(It.IsAny<GlbGroup>())).Returns((IADEntity)null);
			adUser.Setup(m => m.IsIdentityInConflict()).Returns(isIdentityInConflict);
			adUser.Setup(m => m.PasswordLastSet).Returns(DateTime.Now);
			using (var form = new GlbStaffForm(staff))
			{
				form.Show();
				staff.GS_LoginName = "Bilbo.Baggins";

				var message = string.Format("When {0}in identity conflict, form.FireSaveButton should return {1}", isIdentityInConflict ? "" : "NOT ", shouldContinueWithSave);
				AssertEquals(message, shouldContinueWithSave, form.FireSaveButton());
			}
		}

		public void TestEDocsSecurity()
		{
			using (var form = new GlbStaffForm(Factory.NewWithValidTestData<GlbStaff>()))
			{
				Assert("Should have ViewStaffAndResourceseDocs checkpoint",
					form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn).SecurityCheckpoint == Env.Security.ViewStaffeDocs);
			}
		}

		[ExpectNoExceptions]
		public void TestRemoveAllGroupsFromDataRefreshBus()
		{
			var staff = Factory.New<GlbStaff>();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var staff1 = factory.Load<GlbStaff>(staff.PK);
			using (var glbStaffForm = new GlbStaffForm(staff1))
			{
				glbStaffForm.Show();
				staff.IsCancelled = true;
				Factory.Save();
			}
		}

		public void TestAuditColumnsNotVisibleByDefault()
		{
			var columnsToTest = new[] { GlbStaffHolidaySchema.Constants.GA_SystemCreateTimeUtc, GlbStaffHolidaySchema.Constants.GA_SystemLastEditTimeUtc,
				GlbStaffHolidaySchema.Constants.GA_SystemCreateUser, GlbStaffHolidaySchema.Constants.GA_SystemLastEditUser };

			var staff = Factory.New<GlbStaff>();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var staff1 = factory.Load<GlbStaff>(staff.PK);
			using (var glbStaffForm = new GlbStaffForm(staff1))
			{
				var holidaysGrid = glbStaffForm.Controls.Find("HolidaysGrid", true).OfType<ZGrid>().Single();
				AssertContainsExactElementsInAnyOrder(holidaysGrid.ColumnStyles.OfType<ZGridColumnInfo>()
						.Where(x => !x.IsVisible && x.IsReadOnly && columnsToTest.Contains(x.ColumnName))
						.Select(x => x.ColumnName), columnsToTest);
			}
		}

		public void TestfindDedupDisabledForSecurityStaffNotAllowed()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Security.PersonIntelligenceDuplicateDetection.IsAllowed = false;
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				using (var sForm = new GlbStaffForm(staff))
				{
					sForm.Show();
					var actionsMenuItemCollection = sForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					var finddedupmenuItem = actionsMenuItemCollection.MenuItems.FindByText("Find Duplicates");
					AssertEquals(false, finddedupmenuItem.Enabled);
				}
			}
		}
	}
}
