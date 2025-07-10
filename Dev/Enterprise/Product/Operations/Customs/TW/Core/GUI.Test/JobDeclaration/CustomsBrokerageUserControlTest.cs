using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestPackingTabPageVisible()
		{
			AssertPackingTabPageVisible(true);
			AssertPackingTabPageVisible(false);
		}

		public void AssertPackingTabPageVisible(bool enableCustomsDeclarationPackingList)
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableCustomsDeclarationPackingList))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					Application.DoEvents();
					Assert("CustomsBrokerageUserControl should display the PackingTabPage.", form.CustomsBrokerageUserControl.PackingTabPage.TabVisible);

					declaration.JE_MessageType = "EXP";
					Assert("CustomsBrokerageUserControl should display the PackingTabPage.", form.CustomsBrokerageUserControl.PackingTabPage.TabVisible);
				}
			}
		}

		public void TestPackingTabPageCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("CustomsBrokerageUserControl PackingTabPage caption should be Bills.", "Bills", form.CustomsBrokerageUserControl.PackingTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestSupplierHeaderUserControl()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertControlType<ImportSupplierHeaderUserControl>(brokerageUserControl.GetSupplierHeaderUserControl());
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertControlType<ExportSupplierHeaderUserControl>(brokerageUserControl.GetSupplierHeaderUserControl());
		}

		public void TestInvoiceLinesUserControl()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertControlType<ImportInvoiceLineUserControl>(brokerageUserControl.GetInvoiceLinesUserControl());
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertControlType<ExportInvoiceLineUserControl>(brokerageUserControl.GetInvoiceLinesUserControl());
		}

		public void TestMiscOptionsUserControl()
		{
			AssertControlType<MiscOptionsUserControl>(brokerageUserControl.GetMiscOptionsUserControl());
		}

		public void TestPackingUserControl()
		{
			AssertControlType<CustomsBillsUserControl>(brokerageUserControl.GetPackingUserControl());
		}

		public void TestMessageUserControl()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertControlType<ImportCustomsEntriesAndEntryLinesUserControl>(brokerageUserControl.GetMessageUserControl());
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertControlType<ExportCustomsEntriesAndEntryLinesUserControl>(brokerageUserControl.GetMessageUserControl());
			declaration.JE_MessageType = ZString.Empty;
			AssertControlType<CustomsEntriesAndEntryLinesUserControl>(brokerageUserControl.GetMessageUserControl());
		}

		public void TestEntryInstructionUserControl()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertControlType<ControllingMessageUserControl>(brokerageUserControl.GetEntryInstructionUserControl());
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertControlType<ControllingMessageUserControl>(brokerageUserControl.GetEntryInstructionUserControl());
		}

		public void TestTWMessagesTabPageIndex()
		{
			var mainTabPages = brokerageUserControl.MainTabControl.TabPages;
			var messagesTabPageIndex = mainTabPages.IndexOf(brokerageUserControl.MessagesTabPage);
			var twMessagesTabPageIndex = mainTabPages.IndexOf(brokerageUserControl.TWMessagesTabPage);
			AssertEquals(messagesTabPageIndex + 1, twMessagesTabPageIndex);
		}

		public void TestEntryLineAdditionalDataUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				var messageUserControl = (CustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineAdditionalDataUserControl = messageUserControl.FindSingle<ZUserControl>("EntryLineAdditionalDataUserControl");
				AssertEquals(true, entryLineAdditionalDataUserControl.Visible);

				var extendedInfoGroupBox = messageUserControl.FindSingle<ZGroupBox>("ExtendedInfoGroupBox");
				AssertEquals(false, extendedInfoGroupBox.Visible);
			}
		}

		public void TestTabPageCaptionResourceString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				AssertEquals("Licensing", form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage.CaptionResourceString.Caption);
				AssertEquals("Entry", form.CustomsBrokerageUserControl.MessagesTabPage.CaptionResourceString.Caption);
				AssertEquals("Bills", form.CustomsBrokerageUserControl.PackingTabPage.CaptionResourceString.Caption);
			}
		}

		void AssertControlType<T>(IDisposable control)
		{
			AssertType<T>(control);
			control.Dispose();
		}

		protected override void SetUp()
		{
			base.SetUp();
			form = new ZForm();
			declaration = Factory.New<JobDeclaration>();
			brokerageUserControl = new CustomsBrokerageUserControlForTest();
			brokerageUserControl.JobDeclaration = declaration;
			form.Controls.Add(brokerageUserControl);
		}

		protected override void TearDown()
		{
			brokerageUserControl.Dispose();
			form.Dispose();
			base.TearDown();
		}

		ZForm form;
		JobDeclaration declaration;
		CustomsBrokerageUserControlForTest brokerageUserControl;
	}

	class CustomsBrokerageUserControlForTest : CustomsBrokerageUserControl
	{
		public new BaseCustomsEntryUserControl GetMessageUserControl() => base.GetMessageUserControl();
		public new BaseCustomsEntryUserControl GetEntryInstructionUserControl() => base.GetEntryInstructionUserControl();
		public new IBasePackingControl GetPackingUserControl() => base.GetPackingUserControl();
		public new BaseMiscOptionsUserControl GetMiscOptionsUserControl() => base.GetMiscOptionsUserControl();
		public new Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl() => base.GetInvoiceLinesUserControl();
		public new Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl() => base.GetSupplierHeaderUserControl();
		public new ZTabPage TWMessagesTabPage => base.TWMessagesTabPage;
	}
}
