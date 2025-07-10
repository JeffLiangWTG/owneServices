using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class StatusUserControlTest : TestCaseWithFactory
	{
		public void TestTabAndColumnsVisibility()
		{
			var declaration = GetDeclarationWithStatusNotification();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var userControl = (StatusUserControl)statusTabPage.Controls[0];
				Assert(userControl.StatusNotificationsTabPage.TabVisible);
				var grid = userControl.BOLL7StatusGrid;
				AssertEquals(false, grid.GetColumnStyle("ActionIDNumber").IsUnavailable);
				AssertEquals(false, grid.GetColumnStyle("StatusDate").IsUnavailable);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				form.Show();
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				Assert(!userControl.StatusNotificationsTabPage.TabVisible);
				grid = userControl.BOLL7StatusGrid;
				AssertEquals(true, grid.GetColumnStyle("ActionIDNumber").IsUnavailable);
				AssertEquals(true, grid.GetColumnStyle("StatusDate").IsUnavailable);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				form.Show();
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				grid = userControl.BOLL7StatusGrid;
				AssertEquals(false, grid.GetColumnStyle("ActionIDNumber").IsUnavailable);
				AssertEquals(false, grid.GetColumnStyle("StatusDate").IsUnavailable);
				AssertEquals(StatusUserControl.SEBOLL7StatusGridCaption, userControl.BOLL7GroupBox.Text);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				form.Show();
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				var fTZFDA = userControl.FTZGroupBox;
				AssertEquals(true, fTZFDA.Visible);
				form.Show();
				var declarationTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).DeclarationTabPage;
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				AssertEquals("3461 Status", userControl.CRLTabPage.Text);
				AssertEquals("7501 Errors", userControl.ENSStatusTabPage.Text);
				AssertEquals("7501 Status Notifications", userControl.StatusNotificationsTabPage.Text);
				AssertEquals("7501 Status", userControl.ENSStatusGroupBox.Text);
				AssertEquals("3461 Sent Count", userControl.CRLTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("7501 Sent Count", userControl.ENSTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				form.Show();
				declarationTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).DeclarationTabPage;
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationTabPage;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				AssertEquals("Cargo Release Status", userControl.CRLTabPage.Text);
				AssertEquals("Entry Summary Errors", userControl.ENSStatusTabPage.Text);
				AssertEquals("Entry Summary Status Notifications", userControl.StatusNotificationsTabPage.Text);
				AssertEquals("Entry Summary Status", userControl.ENSStatusGroupBox.Text);
				AssertEquals("CRL Sent Count", userControl.CRLTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("ENS Sent Count", userControl.ENSTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				form.Show();
				declarationTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).DeclarationTabPage;
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationTabPage;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				AssertEquals("Cargo Release Status", userControl.CRLTabPage.Text);
				AssertEquals("Entry Summary Errors", userControl.ENSStatusTabPage.Text);
				AssertEquals("Entry Summary Status Notifications", userControl.StatusNotificationsTabPage.Text);
				AssertEquals("Entry Summary Status", userControl.ENSStatusGroupBox.Text);
				AssertEquals("CRL Sent Count", userControl.CRLTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("ENS Sent Count", userControl.ENSTransmitCountTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestPGATabeColumnsExistOnGrid()
		{
			var declaration = GetDeclarationWithStatusNotification();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var userControl = (StatusUserControl)statusTabPage.Controls[0];
				Assert(userControl.StatusNotificationsTabPage.TabVisible);
				var grid = userControl.OGAR6RecordsGrid;
				AssertEquals(true, grid.GetColumnStyle("US_ProgramCode").IsVisible);
			}
		}

		public void TestRefGridContextMenu()
		{
			var declaration = GetDeclarationWithStatusNotification();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Factory.Save();
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var userControl = (StatusUserControl)statusTabPage.Controls[0];
				userControl.ImportStatusTabControl.SelectedTab = userControl.CRLTabPage;
				userControl.RefGrid.Select();
				userControl.RefGrid.Focus();
				Application.DoEvents();
				userControl.RefGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				AssertNotNull(userControl.RefGrid.ListManager.GetCurrent());
				AssertNotNull("Context menu is added", userControl.RefGrid.ContextMenu.MenuItems.FindByText(StatusUserControl.AcknowledgeActionMenuItemCaption));

				userControl.RefreshContextMenuOnRefGrid();
				var menu = (WriteToLogMenuItem)userControl.RefGrid.ContextMenu.MenuItems.FindByText(StatusUserControl.AcknowledgeActionMenuItemCaption);
				AssertNotNull(menu);
				Assert("Menu Enabled", menu.Enabled);

				ZFormModaliser.SetDelegateToCallOnFormShown(x =>
				{
					if (x is ZForm form)
					{
						var reference = form.FindSingle<ZTextBox>("ReferenceTextBox");
						reference.Text = "Reviewed and sent";
#if WINZOR
						if (form.DataSource is Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger logger)
						{
							logger.Reference = reference.Text;
						}
#endif
						form.FindSingle<ZButton>("WriteToLogButton").PerformClick();
					}
				});
				menu.PerformClick();
				Factory.Save();

				var message = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
				AssertEquals(1, message.Logs.Find(x => x.SL_Reference == "Reviewed and sent" && x.SL_SE_NKEvent == Events.Authorised.Code && !x.SL_IsCancelled).Count());

				userControl.RefreshContextMenuOnRefGrid();
				menu = (WriteToLogMenuItem)userControl.RefGrid.ContextMenu.MenuItems.FindByText(StatusUserControl.AcknowledgeActionMenuItemCaption);
				Assert(!menu.Enabled);

				Factory.Save();
				AssertContains("CRLAction=Complete", declaration.JE_AddInfo);
			}
		}

		public void TestFTZControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				ZTabPage statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				StatusUserControl userControl = (StatusUserControl)statusTabPage.Controls[0];
				AssertNotEquals(1, userControl.ImportStatusTabControl.TabCount);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				userControl = (StatusUserControl)statusTabPage.Controls[0];
				AssertEquals(2, userControl.ImportStatusTabControl.TabCount);
				AssertNotNull(userControl.ImportStatusTabControl.AllTabPages.First(x => x.Name == "FTZSummaryTabPage"));
				AssertNotNull(userControl.ImportStatusTabControl.AllTabPages.First(x => x.Name == "BOLTabPage"));
			}
		}

		public void TestSimplifiedEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EnableCRL = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
				form.Show();
				ZTabPage statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var bollGrid = ((ZGrid)form.Controls.Find("BOLL7StatusGrid", true)[0]);
				AssertEquals(true, bollGrid.GetColumnStyle("ActionIDNumber").IsUnavailable);
				AssertEquals(true, bollGrid.GetColumnStyle("StatusDate").IsUnavailable);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				AssertEquals(CargoReleaseTypeList.Codes.SE, declaration.US_CargoReleaseType);
				statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				AssertEquals("Bill of Lading Status", form.Controls.Find("BOLL7GroupBox", true)[0].Text);
			}
		}

		public void TestSeparateInvoiceHeaderCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.FilteredInvoices.AddNew();
			invoice1.JZ_InvoiceNumber = "CCC";
			var invoice2 = declaration.FilteredInvoices.AddNew();
			invoice2.JZ_InvoiceNumber = "BBB";
			var invoice3 = declaration.FilteredInvoices.AddNew();
			invoice3.JZ_InvoiceNumber = "AAA";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceHeaderTabInvoicesGrid = ((USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl).JobComInvoiceHeadersBoundGrid;
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var statusUserControl = (StatusUserControl)(statusTabPage.Controls[0]);
				statusUserControl.ImportStatusTabControl.SelectedTab = statusUserControl.FindSingle<ZTabPage>("ElectronicInvoiceTabPage");
				var statusTabInvoicesGrid = statusUserControl.FindSingle<ZGrid>("InvoicesGrid");
				var discriptorForInvHeaderTab = invoiceHeaderTabInvoicesGrid.InnerGrid.ListManager.GetItemProperties()["JZ_InvoiceDisplaySequence"];
				invoiceHeaderTabInvoicesGrid.InnerGrid.List.ApplySort(discriptorForInvHeaderTab, ListSortDirection.Ascending);
				AssertEquals("CCC", invoiceHeaderTabInvoicesGrid.InnerGrid[0, 0]);
				var discriptorForStatusTab = statusTabInvoicesGrid.ListManager.GetItemProperties()["JZ_InvoiceNumber"];
				statusTabInvoicesGrid.List.ApplySort(discriptorForStatusTab, ListSortDirection.Ascending);
				AssertEquals("AAA", statusTabInvoicesGrid[0, 0]);
				AssertEquals("CCC", invoiceHeaderTabInvoicesGrid.InnerGrid[0, 0]);
			}
		}

		public void TestSeparateBillsCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var bill1 = declaration.FilteredBills.AddNew();
			bill1.CU_BillType = "MB";
			bill1.CU_BillNum = "300000";
			var bill2 = declaration.FilteredBills.AddNew();
			bill2.CU_BillType = "HB";
			bill2.CU_BillNum = "200000";
			var bill3 = declaration.FilteredBills.AddNew();
			bill3.CU_BillType = "SH";
			bill3.CU_BillNum = "100000";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.PackingTabPage;
				var packingTabHouseBillsGrid = ((ImportPackingUserControl)form.CustomsBrokerageUserControl.Packing).HouseBillsGrid;
				var descriptorForPackingTab = packingTabHouseBillsGrid.ListManager.GetItemProperties()["CU_BillType"];
				packingTabHouseBillsGrid.List.ApplySort(descriptorForPackingTab, ListSortDirection.Ascending);
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var statusUserControl = (StatusUserControl)(statusTabPage.Controls[0]);
				statusUserControl.ImportStatusTabControl.SelectedTab = statusUserControl.FindSingle<ZTabPage>("BOLTabPage");
				var statusTabBillsOfLadingGrid = statusUserControl.FindSingle<ZGrid>("BillsOfLadingGrid");
				var descriptorForStatusTab = statusTabBillsOfLadingGrid.ListManager.GetItemProperties()["CU_BillNum"];
				statusTabBillsOfLadingGrid.List.ApplySort(descriptorForStatusTab, ListSortDirection.Descending);
				AssertEquals("300000", statusTabBillsOfLadingGrid[0, 0]);
				AssertEquals("HB", packingTabHouseBillsGrid[0, 0]);
			}
		}

		JobDeclaration GetDeclarationWithStatusNotification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "00941598";
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.SimplifiedEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = ReferenceIdentifierQualifierCodeList.Codes.CMT;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_MessageText =
				"B003002907SO                                                                    " +
				"SO103002907  00941598 0113-611944100KKLUYM MATURITY         41E  040217         " +
				"SO20CMTTRANSFER FOR EXAM TO CES MERCER  PLEASE UPLOAD ENT                       " +
				"SO20CMTRY DOCS TO DIS                                                           " +
				"SO20CR B00101687                                                                " +
				"SO40RKKLUNB3706038                                         00001960     00001960" +
				"SO50040317105895BILL ARRIVED                                                    " +
				"SO60040317105822RELEASE DATE UPDATE                     04031701                " +
				"SO60040317105898RELEASED                                04031701                " +
				"SO60040317105801ONE USG                                                         " +
				"Y  3002907SO00000";
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZFormModaliser.ShowDialogsInTest = true;
		}
	}
}
