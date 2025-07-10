using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class CommercialInvoiceEDIMenuTest : TestCaseWithFactory
	{
		public void TestCreatePackingListMenuItemShouldBeAvailable()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				AssertNotNull(testMenu.MenuItems.FindByText("Create Packing List"));
			}

			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				AssertNull("Should be hidden.", testMenu.MenuItems.FindByText("Create Packing List"));
			}
		}

		public void TestCreatePackingListOnlyAfterInvoiceSaved()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			var query = new ZQuery(CusPackingListSchema.CUL_JZ, invoice.PK);
			var packingList = Factory.LoadTop1<CusPackingList>(query);
			AssertNull("Pre condition", packingList);
			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				testMenu.Declaration = declaration;
				var createPackingListItem = testMenu.MenuItems.FindByText("Create Packing List");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				createPackingListItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(packingList);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createPackingListItem.PerformClick();
				var openedForm = (testMenu.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				entity.PackageJob.Packages.First().KP_MarksAndNumbers = "marks and numbers";
				openedForm.FireSaveButton();
				packingList = Factory.LoadTop1<CusPackingList>(query);
				AssertNotNull(packingList);
				(testMenu.LastController.LastShownForm as ZForm).Close();
			}
		}

		public void TestCreatePackingListAddsDefaultPackage()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			Factory.Save();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoice).HeaderData;

			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				testMenu.Declaration = declaration;
				var createPackingListItem = testMenu.MenuItems.FindByText("Create Packing List");
				createPackingListItem.PerformClick();
				var openedForm = (testMenu.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				AssertEquals(1, entity.PackageJob.Packages.Count);
				(testMenu.LastController.LastShownForm as ZForm).Close();
			}
		}

		public void TestPackingListMenuItemCaption()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertNotNull(testMenu.MenuItems.FindByText("Create Packing List"));
				AssertNull(testMenu.MenuItems.FindByText("Edit Packing List"));
			}

			invoice.CreateCusPackingList(Factory);
			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertNotNull(testMenu.MenuItems.FindByText("Create Packing List"));
				AssertNull(testMenu.MenuItems.FindByText("Edit Packing List"));
			}

			Factory.Save();
			using (var testMenu = new CommercialInvoiceEDIMenu())
			{
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				AssertNull(testMenu.MenuItems.FindByText("Create Packing List"));
				AssertNotNull(testMenu.MenuItems.FindByText("Edit Packing List"));
			}
		}

		public void TestPackingListDocMenuRefreshsAfterCreatingPackingListAndSave()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			Factory.Save();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				var documentMenu = form.Menu.MenuItems.FindByText("&Documents");
				documentMenu.PerformClick();
				var packingListDocItem = documentMenu.MenuItems.FindByText("Packing List");
				AssertNull(packingListDocItem);
				var customsMenu = form.Menu.MenuItems.FindByText("&Brokerage") as CommercialInvoiceEDIMenu;
				var createPackingListItem = customsMenu.MenuItems.FindByText("Create Packing List");
				createPackingListItem.PerformClick();
				var openedForm = (customsMenu.LastController.LastShownForm as ZForm);
				var entity = openedForm.BusinessEntity as CusPackingList;
				entity.PackageJob.Packages.First().KP_MarksAndNumbers = "123";
				entity.Factory.Save();
				documentMenu.PerformClick();
				packingListDocItem = documentMenu.MenuItems.FindByText("Packing List");
				AssertNotNull(packingListDocItem);
				form.Close();
			}
		}
	}
}
