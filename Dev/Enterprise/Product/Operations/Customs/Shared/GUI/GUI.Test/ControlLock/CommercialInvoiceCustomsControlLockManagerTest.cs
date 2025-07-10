using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommercialInvoiceCustomsControlLockManagerTest : TestCaseWithFactory
	{
		public void TestControlLock_Event()
		{
			AssertControlLock((invoice, form) =>
			{
				var expectedMessage = @"These tab pages have been locked for edit.

Lines

You can click the Brokerage - Unlock Commercial Invoice to unlock them.";
				var customsFileParent = (ICustomsFileParent)invoice;
				customsFileParent.LockFile(string.Empty);
				var tabPage1 = form.Controls.Find("TabPage1", true).First();
				var tabPage2 = form.Controls.Find("TabPage2", true).First();
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, false);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
				customsFileParent.UnlockFile(string.Empty);
				expectedMessage = @"All tab pages are unlocked.";
				actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, true);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
			});
		}

		public void TestControlLock_ReadOnlyParent()
		{
			AssertControlLock((invoice, form) =>
			{
				invoice.ReadOnly = true;
				var customsFileParent = (ICustomsFileParent)invoice;
				customsFileParent.LockFile(string.Empty);
				var expectedMessage = "The Commercial Invoice is Read-only, the function of lock is not available.";
				var actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, actualMessage);
				var tabPage1 = form.Controls.Find("TabPage1", true).First();
				var tabPage2 = form.Controls.Find("TabPage2", true).First();
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage1, true);
				ZControlExtensionsTest.AssertEditableIncludingChildren(tabPage2, true);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
		void AssertControlLock(Action<BaseJobComInvoiceHeader, ZForm> assertAction)
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var customsFileParent = (ICustomsFileParent)invoice;
			using (var frm = new ZForm(invoice))
			{
				var tabPage1 = new TabPage { Name = "TabPage1" };
				var tabPage2 = new TabPage { Name = "TabPage2" };
				var textBox1 = new TextBox { Name = "TextBox1" };
				var textBox2 = new TextBox { Name = "TextBox2" };
				tabPage1.Controls.Add(textBox1);
				tabPage1.Controls.Add(textBox2);
				var textBox3 = new TextBox { Name = "TextBox3" };
				tabPage2.Controls.Add(textBox3);
				var tabControl = new TabControl { Name = "TabControl" };
				tabControl.Controls.Add(tabPage1);
				tabControl.Controls.Add(tabPage2);
				frm.Controls.Add(tabControl);
				using (var lockManager = new CommercialInvoiceCustomsControlLockManager(invoice, tabControl))
				{
					lockManager.Register(CommercialInvoiceCustomsControlLockManager.TabPages.Codes.Lines, tabPage1);
					lockManager.Register("TTT", tabPage2);
					frm.Show();
					Application.DoEvents();
					assertAction(invoice, frm);
				}
			}
		}
	}
}
