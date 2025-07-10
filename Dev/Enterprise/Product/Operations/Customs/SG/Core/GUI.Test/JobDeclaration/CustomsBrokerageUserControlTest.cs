using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestJobDeclarationUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var jobDeclarationUserControl = customsBrokerageUserControl.GetDeclarationUserControl())
			{
				Assert(jobDeclarationUserControl is SGJobDeclarationUserControl);
			}
		}

		public void TestMiscOptionsUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var miscOptionsUserControl = customsBrokerageUserControl.GetMiscOptionsUserControl())
			{
				Assert(miscOptionsUserControl is MiscOptionsUserControl);
			}
		}

		public void TestSupplierUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var supplierUserControl = customsBrokerageUserControl.GetSupplierHeaderUserControl())
			{
				Assert(supplierUserControl is SGSupplierHeaderUserControl);
			}
		}

		public void TestInvoiceLineUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var invoiceLineUserControl = customsBrokerageUserControl.GetInvoiceLinesUserControl())
			{
				Assert(invoiceLineUserControl is SGInvoiceLineUserControl);
			}
		}

		public void TestGroupInvoiceUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var groupInvoiceUserControl = customsBrokerageUserControl.GetInvoiceGroupingUserControl())
			{
				Assert(groupInvoiceUserControl is GroupInvoiceUserControl);
			}
		}

		public void TestContainerUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var containerUserControl = customsBrokerageUserControl.GetContainerUserControl())
			{
				Assert(containerUserControl is ContainerUserControl);
			}
		}

		public void TestMessagesUserControl()
		{
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			using (var messagesUserControl = customsBrokerageUserControl.GetMessageUserControl())
			{
				Assert(messagesUserControl is Customs.GUI.ImportMessageUserControl);
			}
		}

		public void TestPackingTabIsVisible()
		{
			using (var form = new ZForm(Declaration))
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				form.Controls.Add(customsBrokerageUserControl);
				customsBrokerageUserControl.JobDeclaration = Declaration;
				form.Show();
				foreach (CodeDescriptionPair pair in Declaration.Lookups.MessageTypeList)
				{
					Declaration.JE_MessageType = pair.Code;
					AssertEquals("PackingTabPage.TabVisible", false, customsBrokerageUserControl.PackingTabPage.TabVisible);
				}
			}
		}

		[TestDate(2011, 11, 01)]
		public void TestSGDetailsTab()
		{
			using (var form = new ZForm())
			using (var customsBrokerageUserControl = new CustomsBrokerageUserControlTestClass())
			{
				form.Controls.Add(customsBrokerageUserControl);
				customsBrokerageUserControl.JobDeclaration = Declaration;
				var tabPage = customsBrokerageUserControl.MainTabControl.TabPages[1] as Customs.GUI.BaseDeclarationTabPage;
				AssertNotNull(tabPage);
				AssertEquals("Details", tabPage.Text);
				AssertEquals("SGDetailsTabPage", tabPage.Name);
				AssertEquals(true, tabPage.CheckForNotifications);
				customsBrokerageUserControl.Show();
				customsBrokerageUserControl.MainTabControl.SelectedIndex = 1;
				tabPage.SetDataBinding(Declaration, "");
				var userControl = tabPage.Controls[0];
				AssertNotNull(userControl);
				AssertEquals(DockStyle.Fill, userControl.Dock);
			}
		}

		public void TestSGDetailsTabIsOnlyAddedOnce()
		{
			using (var form = new JobDeclarationForm(Declaration))
			{
				form.Show();
				var countofTabPages = form.CustomsBrokerageUserControl.MainTabControl.TabPages.Count;
				AssertEquals(17, countofTabPages);
				AssertNotEquals("Details", form.CustomsBrokerageUserControl.MainTabControl.TabPages[3]);
			}
		}

		public void TestSGDeclarationUserControlOnApplicationChange()
		{
			using (var form = new ZForm(Declaration))
			using (var userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = Declaration;
				form.Controls.Add(userControl);
				form.Show();
				var tabPage = userControl.MainTabControl.TabPages[1] as Customs.GUI.BaseDeclarationTabPage;
				Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
				userControl.MainTabControl.SelectedIndex = 1;
				var sgTabPage = tabPage.Controls[0];
				AssertEquals("TN Version 4 form is expected", typeof(SGDeclarationUserControl), sgTabPage.GetType());
				userControl.MainTabControl.SelectedTab = userControl.DeclarationTabPage;
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
				userControl.MainTabControl.SelectedIndex = 1;
				sgTabPage = tabPage.Controls[0];
				AssertEquals("TN Versions 4.1 form is expected", typeof(SGTN41DeclarationUserControl), sgTabPage.GetType());
				tabPage.Dispose();
				sgTabPage.Dispose();
			}
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		sealed class CustomsBrokerageUserControlTestClass : CustomsBrokerageUserControl
		{
			internal new Customs.GUI.BaseCustomsEntryUserControl GetDeclarationUserControl() => base.GetDeclarationUserControl();

			internal new Customs.GUI.BaseMiscOptionsUserControl GetMiscOptionsUserControl() => base.GetMiscOptionsUserControl();

			internal new Customs.GUI.BaseCustomsCusContainersUserControl GetContainerUserControl() => base.GetContainerUserControl();

			internal new Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl() => base.GetSupplierHeaderUserControl();

			internal new Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl() => base.GetInvoiceLinesUserControl();

			internal new Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl() => base.GetMessageUserControl();

			internal new Customs.GUI.BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl() => base.GetInvoiceGroupingUserControl();
		}
	}
}
