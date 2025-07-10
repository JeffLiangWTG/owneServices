using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CustomsControlLockManagerTest : TestCaseWithFactory
	{
		public void TestControlLock_Event()
		{
			AssertControlLock((declaration, form) =>
			{
				var expectedMessage = @"These tab pages have been locked for edit.

Declaration

You can click the Brokerage - Unlock Customs Declaration to unlock them.
You can change the lock config in the System Registry under Customs -> Declaration Lock For Edit.";
				var customsFileParent = (ICustomsFileParent)declaration;
				customsFileParent.LockFile(string.Empty);
				var tabPage1 = form.Controls.Find("TabPage1", true).First();
				var tabPage2 = form.Controls.Find("TabPage2", true).First();
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, false, new[] { controlForIgnore });
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
				customsFileParent.UnlockFile(string.Empty);
				expectedMessage = @"All tab pages are unlocked.";
				actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, true);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
			});
		}

		public void TestControlLock_DeclarationType()
		{
			AssertControlLock((declaration, form) =>
			{
				var expectedMessage = @"These tab pages have been locked for edit.

Declaration

You can click the Brokerage - Unlock Customs Declaration to unlock them.
You can change the lock config in the System Registry under Customs -> Declaration Lock For Edit.";
				var configs = CustomsDataRegistry.Instance.DeclarationLockForEdit.Value;
				var config = configs.AddNew();
				config.DeclarationType = "BBB";
				var tabInfo = config.TabInfos.AddNew();
				tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Containers;
				var customsFileParent = (ICustomsFileParent)declaration;
				customsFileParent.LockFile(string.Empty);
				var log = customsFileParent.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
				AssertEquals(string.Empty, log.SL_Reference);
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				var tabPage1 = form.Controls.Find("TabPage1", true).First();
				var tabPage2 = form.Controls.Find("TabPage2", true).First();
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, false, new[] { controlForIgnore });
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
				customsFileParent.DeclarationTypeInfo.SetValueFromString("BBB");
				log = customsFileParent.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
				AssertEquals("Lock From Declaration Type - BBB", log.SL_Reference);
				expectedMessage = @"These tab pages have been locked for edit.

Containers

You can click the Brokerage - Unlock Customs Declaration to unlock them.
You can change the lock config in the System Registry under Customs -> Declaration Lock For Edit.";
				actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, true);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, false);
			});
		}

		public void TestControlLock_ReadOnlyParent()
		{
			AssertControlLock((declaration, form) =>
			{
				declaration.ReadOnly = true;
				var customsFileParent = (ICustomsFileParent)declaration;
				customsFileParent.LockFile(string.Empty);
				var expectedMessage = "The Declaration is Read-only, the function of lock is not available.";
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				var tabPage1 = form.Controls.Find("TabPage1", true).First();
				var tabPage2 = form.Controls.Find("TabPage2", true).First();
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, true);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
		void AssertControlLock(Action<BaseJobDeclaration, ZForm> assertAction)
		{
			var configs = new DeclarationLockConfigCollection(null, Factory);
			var config = configs.AddNew();
			config.DeclarationType = "AAA";
			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var customsFileParent = (ICustomsFileParent)declaration;
			customsFileParent.DeclarationTypeInfo.SetValueFromString("AAA");
			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configs))
			using (var frm = new ZForm(declaration))
			{
				var tabPage1 = new TabPage { Name = "TabPage1" };
				var tabPage2 = new TabPage { Name = "TabPage2" };
				var textBox1 = new TextBox { Name = controlForLock };
				var textBox2 = new TextBox { Name = controlForIgnore };
				tabPage1.Controls.Add(textBox1);
				tabPage1.Controls.Add(textBox2);
				var textBox3 = new TextBox { Name = "TextBox3" };
				tabPage2.Controls.Add(textBox3);
				var tabControl = new TabControl { Name = "TabControl" };
				tabControl.Controls.Add(tabPage1);
				tabControl.Controls.Add(tabPage2);
				frm.Controls.Add(tabControl);
				using (var lockManager = new CustomsControlLockManager(declaration, tabControl))
				{
					lockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Declaration, tabPage1, new[] { controlForIgnore });
					lockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.Containers, tabPage2);
					frm.Show();
					Application.DoEvents();
					assertAction(declaration, frm);
				}
			}
		}

		readonly string controlForLock = "ControlForLock";
		readonly string controlForIgnore = "ControlForIgnore";
	}

	public static class CustomsControlLockManagerExtensions
	{
		public static void AssertIsRegisteredForLock(this BaseCustomsControlLockManager lockManager, Control control, string key, bool expectLocked = true, params string[] ignoreControlNames)
		{
			var controlLockInfo = lockManager.ControlLockInfos.SingleOrDefault(cli => cli.Control.Equals(control) && cli.Key == key);
			Assertion.Assert($"Control {control.Name} is expected to be {(expectLocked ? string.Empty : "not ")}registered for lock", (controlLockInfo != null) == expectLocked);

			if (controlLockInfo != null && ignoreControlNames.Any())
			{
				Assertion.AssertNotNull($"IgnoreControlNames expected o be configured for locked {control.Name}", controlLockInfo.IgnoreControlNames);
				var ignoreControlNamesToCheck = controlLockInfo.IgnoreControlNames.ToList();
				foreach (var ignoreControlName in ignoreControlNames)
				{
					Assertion.Assert($"Control {ignoreControlName} should be ignored for lock", ignoreControlNamesToCheck.Remove(ignoreControlName));
				}

				Assertion.AssertEquals($"Controls {string.Join(", ", ignoreControlNamesToCheck)} are not expected to be ignored for lock", 0, ignoreControlNamesToCheck.Count);
			}
		}
	}
}
