using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class EntrySummaryStatusNotificationsUserControlTest : TestCaseWithFactory
	{
		public void TestStatusNotificationGridContextMenu_Declaration()
		{
			var declaration = GetDeclarationWithStatusNotification();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Factory.Save();
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var userControl = (StatusUserControl)statusTabPage.Controls[0];
				userControl.ImportStatusTabControl.SelectedTab = userControl.StatusNotificationsTabPage;
				var ensControl = (EntrySummaryStatusNotificationsUserControl)userControl.ImportStatusTabControl.SelectedTab.Controls[0];
				ensControl.StatusNotificationsGrid.Select();
				ensControl.StatusNotificationsGrid.Focus();
				Application.DoEvents();
				ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				AssertNotNull(ensControl.StatusNotificationsGrid.ListManager.GetCurrent());
				AssertNotNull("Context menu is added", ensControl.StatusNotificationsGrid.ContextMenu.MenuItems.FindByText(EntrySummaryStatusNotificationsUserControl.StatusActionMenuItemCaption));

				ensControl.RefreshContextMenu();
				var menu = ensControl.StatusNotificationsGrid.ContextMenu.MenuItems.FindByText(EntrySummaryStatusNotificationsUserControl.StatusActionMenuItemCaption);
				AssertNotNull(menu);
				Assert(menu.Enabled);

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
				var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
				AssertEquals(1, message.Logs.Find(x => x.SL_Reference == "Reviewed and sent" && x.SL_SE_NKEvent == Events.Authorised.Code && !x.SL_IsCancelled).Count());

				ensControl.RefreshContextMenu();
				menu = ensControl.StatusNotificationsGrid.ContextMenu.MenuItems.FindByText(EntrySummaryStatusNotificationsUserControl.StatusActionMenuItemCaption);
				Assert(!menu.Enabled);

				Factory.Save();
				AssertContains("ENSAction=Complete", declaration.JE_AddInfo);
			}
		}

		public void TestStatusNotificationGridContextMenu_Recon()
		{
			var recon = GetReconWithStatusNotification(messageTextWithQuotas);
			using (var form = new ReconDeclarationForm(recon))
			{
				form.Show();
				Factory.Save();
				var statusTabPage = form.StatusTabPage;
				form.MainTabControlExposed.SelectedTab = statusTabPage;
				var statusTabControl = (ZTabControl)statusTabPage.Controls[0];
				statusTabControl.SelectedTab = (ZTabPage)statusTabControl.Controls[2];
				var ensControl = (EntrySummaryStatusNotificationsUserControl)statusTabControl.SelectedTab.Controls[0];
				ensControl.StatusNotificationsGrid.Select();
				ensControl.StatusNotificationsGrid.Focus();
				Application.DoEvents();
				ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				AssertNotNull(ensControl.StatusNotificationsGrid.ListManager.GetCurrent());
				AssertNotNull("Context menu is added", ensControl.StatusNotificationsGrid.ContextMenu.MenuItems.FindByText(EntrySummaryStatusNotificationsUserControl.StatusActionMenuItemCaption));

				ensControl.RefreshContextMenu();
				var menu = ensControl.StatusNotificationsGrid.ContextMenu.MenuItems.FindByText(EntrySummaryStatusNotificationsUserControl.StatusActionMenuItemCaption);
				AssertNotNull(menu);
				Assert(menu.Enabled);

				ZFormModaliser.SetDelegateToCallOnFormShown(x =>
				{
					if (x is ZForm form)
					{
						var reference = form.FindSingle<ZTextBox>("ReferenceTextBox");
						reference.Text = "Reviewed and sent (recon)";
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
				var message = (MQEDIMessage)recon.ReconEntry.Messages[0];
				AssertEquals(1, message.Logs.Find(x => x.SL_Reference == "Reviewed and sent (recon)" && x.SL_SE_NKEvent == Events.Authorised.Code && !x.SL_IsCancelled).Count());

				ensControl.RefreshContextMenu();
				menu = ensControl.StatusNotificationsGrid.ContextMenu.MenuItems.FindByText(EntrySummaryStatusNotificationsUserControl.StatusActionMenuItemCaption);
				Assert(!menu.Enabled);

				Factory.Save();
				AssertContains("ENSAction=Complete", recon.ReconWrappedJobDeclaration.JE_AddInfo);
			}
		}

		public void TestStatusNotificationGridContextMenuException_Declaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Factory.Save();
				ZTabPage statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				StatusUserControl userControl = (StatusUserControl)statusTabPage.Controls[0];
				userControl.ImportStatusTabControl.SelectedTab = userControl.StatusNotificationsTabPage;
				EntrySummaryStatusNotificationsUserControl ensControl = (EntrySummaryStatusNotificationsUserControl)userControl.ImportStatusTabControl.SelectedTab.Controls[0];
				AssertNoExceptionThrown(delegate
				{
					ensControl.StatusNotificationsGrid.Select();
					ensControl.StatusNotificationsGrid.Focus();
					Application.DoEvents();
					ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
					Application.DoEvents();
					ensControl.RefreshContextMenu();
				});
			}
		}

		public void TestStatusNotificationGridContextMenuException_Recon()
		{
			var recon = GetReconWithStatusNotification(messageTextWithQuotas);
			using (var form = new ReconDeclarationForm(recon))
			{
				form.Show();
				Factory.Save();
				var statusTabPage = form.StatusTabPage;
				form.MainTabControlExposed.SelectedTab = statusTabPage;
				var statusTabControl = (ZTabControl)statusTabPage.Controls[0];
				statusTabControl.SelectedTab = (ZTabPage)statusTabControl.Controls[2];
				EntrySummaryStatusNotificationsUserControl ensControl = (EntrySummaryStatusNotificationsUserControl)statusTabControl.SelectedTab.Controls[0];
				AssertNoExceptionThrown(delegate
				{
					ensControl.StatusNotificationsGrid.Select();
					ensControl.StatusNotificationsGrid.Focus();
					Application.DoEvents();
					ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
					Application.DoEvents();
					ensControl.RefreshContextMenu();
				});
			}
		}

		public void TestQuotaInformationsGridVisible_Declaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E171333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"E4TA3Q04                                  18          MC 877777.23  TL          " +
				"Y  8888XJ5UC00003";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var userControl = (StatusUserControl)statusTabPage.Controls[0];
				userControl.ImportStatusTabControl.SelectedTab = userControl.StatusNotificationsTabPage;
				EntrySummaryStatusNotificationsUserControl ensControl = (EntrySummaryStatusNotificationsUserControl)userControl.ImportStatusTabControl.SelectedTab.Controls[0];
				ensControl.StatusNotificationsGrid.Select();
				ensControl.StatusNotificationsGrid.Focus();
				Application.DoEvents();
				ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				Assert(ensControl.QuotaInformationsGrid.Visible);
				Assert(ensControl.QuotaInformationsGrid.Size.Height > 200);
				Assert(ensControl.StatusNotificationsGrid.Size.Height > 200);
				Assert((ensControl.QuotaInformationsGrid.Size.Height + ensControl.StatusNotificationsGrid.Size.Height) < userControl.StatusNotificationsTabPage.Size.Height);
			}
		}

		public void TestQuotaInformationsGridVisible_Recon()
		{
			var recon = GetReconWithStatusNotification(messageTextWithQuotas);
			using (var form = new ReconDeclarationForm(recon))
			{
				form.Show();
				var statusTabPage = form.StatusTabPage;
				form.MainTabControlExposed.SelectedTab = statusTabPage;
				var statusTabControl = (ZTabControl)statusTabPage.Controls[0];
				statusTabControl.SelectedTab = (ZTabPage)statusTabControl.Controls[2];
				EntrySummaryStatusNotificationsUserControl ensControl = (EntrySummaryStatusNotificationsUserControl)statusTabControl.SelectedTab.Controls[0];
				ensControl.StatusNotificationsGrid.Select();
				ensControl.StatusNotificationsGrid.Focus();
				Application.DoEvents();
				ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				Assert(ensControl.QuotaInformationsGrid.Visible);
				Assert(ensControl.QuotaInformationsGrid.Size.Height > 200);
				Assert(ensControl.StatusNotificationsGrid.Size.Height > 200);
				Assert((ensControl.QuotaInformationsGrid.Size.Height + ensControl.StatusNotificationsGrid.Size.Height) < statusTabControl.Size.Height);
			}
		}

		public void TestQuotaInformationsGridHide_Declaration()
		{
			var declaration = GetDeclarationWithStatusNotification();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var statusTabPage = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).StatusTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = statusTabPage;
				var userControl = (StatusUserControl)statusTabPage.Controls[0];
				userControl.ImportStatusTabControl.SelectedTab = userControl.StatusNotificationsTabPage;
				EntrySummaryStatusNotificationsUserControl ensControl = (EntrySummaryStatusNotificationsUserControl)userControl.ImportStatusTabControl.SelectedTab.Controls[0];
				ensControl.StatusNotificationsGrid.Select();
				ensControl.StatusNotificationsGrid.Focus();
				Application.DoEvents();
				ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				Assert(!ensControl.QuotaInformationsGrid.Visible);
			}
		}

		public void TestQuotaInformationsGridHide_Recon()
		{
			var recon = GetReconWithStatusNotification(messageTextWithoutQuotas);
			using (var form = new ReconDeclarationForm(recon))
			{
				form.Show();
				var statusTabPage = form.StatusTabPage;
				form.MainTabControlExposed.SelectedTab = statusTabPage;
				var statusTabControl = (ZTabControl)statusTabPage.Controls[0];
				statusTabControl.SelectedTab = (ZTabPage)statusTabControl.Controls[2];
				EntrySummaryStatusNotificationsUserControl ensControl = (EntrySummaryStatusNotificationsUserControl)statusTabControl.SelectedTab.Controls[0];
				ensControl.StatusNotificationsGrid.Select();
				ensControl.StatusNotificationsGrid.Focus();
				Application.DoEvents();
				ensControl.StatusNotificationsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				Assert(!ensControl.QuotaInformationsGrid.Visible);
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
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E171333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"Y  8888XJ5UC00003";
			return declaration;
		}

		ReconDeclaration GetReconWithStatusNotification(string messageText)
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.ActiveEntryHeaders[0].EntryNumber = "00123456";
			reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
			invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries[0].CH_PK;
			reconDeclaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var message = reconDeclaration.ReconEntry.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageSubType = "ENS";
			message.EM_MessageText = messageText;
			Factory.Save();
			return reconDeclaration;
		}

		readonly string messageTextWithQuotas =
			"B018888XJ5UC                                               34                   " +
			"E171333   090110                                  XJ5  00000063                 " +
			"E2  CHRIS SMITH                     5555555555  123456789012                    " +
			"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
			"E4TA3Q04                                  18          MC 877777.23  TL          " +
			"Y  8888XJ5UC00003";
		readonly string messageTextWithoutQuotas =
			"B018888XJ5UC                                               34                   " +
			"E171333   090110                                  XJ5  00000063                 " +
			"E2  CHRIS SMITH                     5555555555  123456789012                    " +
			"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
			"Y  8888XJ5UC00003";

		protected override void SetUp()
		{
			base.SetUp();
			ZFormModaliser.ShowDialogsInTest = true;
		}
	}
}
