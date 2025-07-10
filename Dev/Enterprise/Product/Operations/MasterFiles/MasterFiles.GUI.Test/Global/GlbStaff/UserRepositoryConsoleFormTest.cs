using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UserRepositoryConsoleFormTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestShowUserDefinedTypeDefinition()
		{
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var info = "";
			var userRepository = new UserRepository(new BusinessObjectFactory(), sqlPassword);
			var factory = userRepository.Factory;
			var testUser = CreateTestUser(factory);

			try
			{
				using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					userRepository.Execute("CREATE TYPE TestType AS TABLE (Col1 INT);");

					using (var form = new UserRepositoryConsoleForm(userRepository))
					{
						form.Show();

						var userRepositoryObjectListView = (CargoWise.Windows.UI.KListView)form.Controls.Find("UserRepositoryObjectListView", true)[0];
						userRepositoryObjectListView.Focus();
						userRepositoryObjectListView.Items[0].Selected = true;

						var showUseRepositoryObjectDefinitionButton = (ZArchitecture.GUI.ZButton)form.Controls.Find("ShowUseRepositoryObjectDefinitionButton", true)[0];
						showUseRepositoryObjectDefinitionButton.PerformClick();

						info = UnitTestUserNotification.Instance.LastMessage.Text;
					}

					var list = userRepository.GetUserRepositoryDbObjects().Cast<UserRepository.IUserRepositoryDatabaseObject>().ToList();
					list[0].Drop();

					AssertEquals(UserRepositoryConsoleForm.ShowUserDefinedTypeDefinitionInformationMessage, info);
				}
			}
			finally
			{
				RemoveTestUser(testUser, factory);
			}
		}

		GlbStaff CreateTestUser(BusinessObjectFactory factory)
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "TestUser";
			staff.GS_Code = "TSU";
			staff.IsDatabaseDeveloper = true;
			staff.StaffPlainTextPassword = "1234";
			new DbUserManager().SetPasswordForStaff(staff, sqlPassword);
			factory.Save();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: true);
				sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);
			}

			return staff;
		}

		const string sqlPassword = "sql1234";

		void RemoveTestUser(GlbStaff staff, BusinessObjectFactory factory)
		{
			staff.Delete();
			factory.Save();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: true);
				sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);
			}
		}
	}
}
