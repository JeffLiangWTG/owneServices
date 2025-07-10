using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using JobMessageTypeList = Enterprise.Customs.NZ.Business.JobMessageTypeList;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class NZEDIMenuTest : TestCaseWithFactory
	{
		public void TestNotifyUserIfDeclarationIsNull()
		{
			NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest();
			menu.Declaration = null;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menu.SubmitJobMenuItem.PerformClick();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", NZEDIMenu.ErrorMessageDeclarationIsRequiredForThisJob, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menu.CancelJobMenuItem.PerformClick();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", NZEDIMenu.ErrorMessageDeclarationIsRequiredForThisJob, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menu.ResetToOriginalMenuItem.PerformClick();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", NZEDIMenu.ErrorMessageDeclarationIsRequiredForThisJob, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCustomsWebsiteItems()
		{
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				AssertNotNull("menu.CustomsWebSiteMenuGroup", menu.CustomsWebSiteMenuGroup);
				AssertEquals("menu.CustomsWebSiteMenuGroup.Text", NZEDIMenu.DescriptionCustomsWebSite, menu.CustomsWebSiteMenuGroup.Text);
				AssertEquals("menu.MenuItems.Contains(menu.CustomsWebSiteMenuGroup)", true, menu.MenuItems.Contains(menu.CustomsWebSiteMenuGroup));
				AssertEquals("menu.CustomsWebSiteMenuGroup.MenuItems.Count", 2, menu.CustomsWebSiteMenuGroup.MenuItems.Count);

				AssertEquals("menu.CustomsWebSiteMenuGroup.MenuItems.Contains(menu.CustomsHomePageMenuItem)", true, menu.CustomsWebSiteMenuGroup.MenuItems.Contains(menu.CustomsHomePageMenuItem));
				AssertEquals("menu.CustomsHomePageMenuItem.Text", NZEDIMenu.DescriptionCustomsHomePage, menu.CustomsHomePageMenuItem.Text);

				AssertEquals("menu.CustomsWebSiteMenuGroup.MenuItems.Contains(menu.CustomsFindVesselOrFlightMenuItem)", true, menu.CustomsWebSiteMenuGroup.MenuItems.Contains(menu.CustomsFindVesselOrFlightMenuItem));
				AssertEquals("menu.CustomsFindVesselOrFlightMenuItem.Text", NZEDIMenu.DescriptionCustomsFindVesselOrFlight, menu.CustomsFindVesselOrFlightMenuItem.Text);
			}
		}

		public void TestBiosecurityWebsiteItems()
		{
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				AssertNotNull("menu.BiosecurityWebSiteMenuGroup", menu.BiosecurityWebSiteMenuGroup);
				AssertEquals("menu.BiosecurityWebSiteMenuGroup.Text", NZEDIMenu.DescriptionBiosecurityWebSite, menu.BiosecurityWebSiteMenuGroup.Text);
				AssertEquals("menu.MenuItems.Contains(menu.BiosecurityWebSiteMenuGroup)", true, menu.MenuItems.Contains(menu.BiosecurityWebSiteMenuGroup));
				AssertEquals("menu.BiosecurityWebSiteMenuGroup.MenuItems.Count", 2, menu.BiosecurityWebSiteMenuGroup.MenuItems.Count);

				AssertEquals("menu.BiosecurityWebSiteMenuGroup.MenuItems.Contains(menu.BiosecurityHomePageMenuItem)", true, menu.BiosecurityWebSiteMenuGroup.MenuItems.Contains(menu.BiosecurityHomePageMenuItem));
				AssertEquals("menu.BiosecurityHomePageMenuItem.Text", NZEDIMenu.DescriptionBiosecurityHomePage, menu.BiosecurityHomePageMenuItem.Text);

				AssertEquals("menu.BiosecurityWebSiteMenuGroup.MenuItems.Contains(menu.BiosecurityComtainerRequirementsMenuItem)", true, menu.BiosecurityWebSiteMenuGroup.MenuItems.Contains(menu.BiosecurityComtainerRequirementsMenuItem));
				AssertEquals("menu.BiosecurityComtainerRequirementsMenuItem.Text", NZEDIMenu.DescriptionBiosecurityContainerRequirements, menu.BiosecurityComtainerRequirementsMenuItem.Text);
			}
		}

		public void TestMAFWebsiteItems()
		{
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				AssertNotNull("menu.MAFWebSiteMenuGroup", menu.MAFWebSiteMenuGroup);
				AssertEquals("menu.MAFWebSiteMenuGroup.Text", NZEDIMenu.DescriptionMAFWebSite, menu.MAFWebSiteMenuGroup.Text);
				AssertEquals("menu.MenuItems.Contains(menu.MAFWebSiteMenuGroup)", true, menu.MenuItems.Contains(menu.MAFWebSiteMenuGroup));
				AssertEquals("menu.MAFWebSiteMenuGroup.MenuItems.Count", 2, menu.MAFWebSiteMenuGroup.MenuItems.Count);

				AssertEquals("menu.MAFWebSiteMenuGroup.MenuItems.Contains(menu.MAFHomePageMenuItem)", true, menu.MAFWebSiteMenuGroup.MenuItems.Contains(menu.MAFHomePageMenuItem));
				AssertEquals("menu.MAFHomePageMenuItem.Text", NZEDIMenu.DescriptionMAFHomePage, menu.MAFHomePageMenuItem.Text);

				AssertEquals("menu.MAFWebSiteMenuGroup.MenuItems.Contains(menu.MAFQuarrantineCargoInfoMenuItem)", true, menu.MAFWebSiteMenuGroup.MenuItems.Contains(menu.MAFQuarrantineCargoInfoMenuItem));
				AssertEquals("menu.MAFQuarrantineCargoInfoMenuItem.Text", NZEDIMenu.DescriptionMAFQuarrantineCargoInfo, menu.MAFQuarrantineCargoInfoMenuItem.Text);
			}
		}

		public void TestDataTransferImplReturnsNZConcrete()
		{
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				Assert("Menu.DataTransferImpl is DataTransferImplementation", menu.DataTransferImpl is Business.Data.FlatFileImporter.DataTransferImplementation);
			}
		}

		public void TestMergeMenuItem()
		{
			using (NZEDIMenu menu = new NZEDIMenu())
			{
				TestFormalEntryCreator decCreator = new TestFormalEntryCreator(Declaration);
				decCreator.SetupTestConsignmentDetails();
				decCreator.SetupTestForAir();
				decCreator.SetupTestForImportFromAU();
				decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
				decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
				decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
				decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
				Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				menu.Declaration = Declaration;
				AssertEquals("Precondition: Declaration.CusEntryHeader.MergedLines.Count", 0, Declaration.CusEntryHeader.MergedLines.Count);
				menu.GenerateEntriesMenuItem.PerformClick();
				AssertEquals("Declaration.CusEntryHeader.MergedLines.Count", 1, Declaration.CusEntryHeader.MergedLines.Count);
			}
		}

		public void TestMenuItemVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Submit options should be visible for all", true, menu.SubmitJobMenuItem.Visible);
				AssertEquals("TSW CRE declaration", true, menu.TSWSubmitWithDetails.Visible);
				AssertEquals("TSW CRE manifest submission", true, menu.TSWQueueForManifesting.Visible);
				AssertEquals("TSW CRE declaration", true, menu.TSWSeparator.Visible);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Submit options should be visible for all", true, menu.SubmitJobMenuItem.Visible);
				AssertEquals("Legacy write-off declaration", false, menu.TSWSubmitWithDetails.Visible);
				AssertEquals("Legacy write-off manifest submission", false, menu.TSWQueueForManifesting.Visible);
				AssertEquals("Legacy write-off declaration", false, menu.TSWSeparator.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Submit options should be visible for all", true, menu.SubmitJobMenuItem.Visible);
				AssertEquals("TSW Import Normal declaration", false, menu.TSWSubmitWithDetails.Visible);
				AssertEquals("TSW Import Normal declaration does not have queue for manifest option", false, menu.TSWQueueForManifesting.Visible);
				AssertEquals("TSW Import Normal declaration", true, menu.TSWSeparator.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Submit options should be visible for all", true, menu.SubmitJobMenuItem.Visible);
				AssertEquals("TSW Export Normal declaration", false, menu.TSWSubmitWithDetails.Visible);
				AssertEquals("TSW Export Normal declaration does not have queue for manifest option", false, menu.TSWQueueForManifesting.Visible);
				AssertEquals("TSW Export Normal declaration", true, menu.TSWSeparator.Visible);
			}
		}

		public void TestMenuItemsNotVisibleWhenInterface()
		{
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				using (var menu = new NZEDIMenu_ForTest())
				{
					menu.Declaration = declaration;
					menu.RefreshMenu();
					AssertEquals("Seperator", false, menu.Separator1.Visible);
					AssertEquals("TSW declaration options", false, menu.TSWSubmitWithDetails.Visible);
					AssertEquals("TSW manifest submission", false, menu.TSWQueueForManifesting.Visible);
					AssertEquals("TSW declaration seperator", false, menu.TSWSeparator.Visible);
					AssertEquals("TSW Submit options should be invisible for interface jobs", false, menu.SubmitJobMenuItem.Visible);
					AssertEquals("TSW Cancel options should be invisible for interface jobs", false, menu.CancelJobMenuItem.Visible);
					AssertEquals("TSW Reset options should be invisible for interface jobs", false, menu.ResetToOriginalMenuItem.Visible);
				}
			}
		}

		public void TestMenuItemIsDisabled()
		{
			var cancelledTSWStatus = new string[] { LowValueConsignmentStatusList.Codes.ConsignmentCancelled, TSWEntryStatusList.Codes.CAN, TSWEntryStatusList.Codes.DCC, TSWEntryStatusList.Codes.DCP, };
			foreach (var status in cancelledTSWStatus)
			{
				TestMenuItemIsDisabledByTSWCancellation(status);
			}
		}

		public void TestMenuIsActiveIfDeclarationNotCancelled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCA;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu should be active for dec with status DCA", true, menu.SubmitJobMenuItem.Enabled);
				AssertEquals("Menu should be active for dec with this status", true, menu.CancelJobMenuItem.Enabled);
				AssertEquals("Menu should be active for dec with this status", true, menu.ResetToOriginalMenuItem.Enabled);
			}

			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Menu should be active for dec with status DCI", true, menu.SubmitJobMenuItem.Enabled);
				AssertEquals("Menu should be active for dec with this status", true, menu.CancelJobMenuItem.Enabled);
				AssertEquals("Menu should be active for dec with this status", true, menu.ResetToOriginalMenuItem.Enabled);
			}
		}

		void TestMenuItemIsDisabledByTSWCancellation(ZString tswCombinedStatus)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TSWCombinedStatus = tswCombinedStatus;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Disable Submit Job Menu for TSW Cancelled job", false, menu.SubmitJobMenuItem.Enabled);
				AssertEquals("Disable Cancel Job Menu for TSW Cancelled job", false, menu.CancelJobMenuItem.Enabled);
				AssertEquals("Disable Reset Menu for TSW Cancelled job", false, menu.ResetToOriginalMenuItem.Enabled);
			}
		}

		public void TestManifestingMenuNotVisibleForSeaFreight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("TSW CRE manifest submission", true, menu.TSWQueueForManifesting.Visible);
			}

			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			using (NZEDIMenu_ForTest menu = new NZEDIMenu_ForTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("TSW CRE manifest submission is not available for SeaFreight job", false, menu.TSWQueueForManifesting.Visible);
			}
		}

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}

				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;
		#endregion
	}

	class NZEDIMenu_ForTest : NZEDIMenu
	{
		public new DataTransferImpl DataTransferImpl => base.DataTransferImpl;

		public MenuItem CustomsWebSiteMenuGroup => base.customsWebSiteMenuGroup;

		public MenuItem CustomsHomePageMenuItem => base.customsHomePageMenuItem;

		public MenuItem CustomsFindVesselOrFlightMenuItem => base.customsFindVesselOrFlightMenuItem;

		public MenuItem BiosecurityWebSiteMenuGroup => base.biosecurityWebSiteMenuGroup;

		public MenuItem BiosecurityHomePageMenuItem => base.biosecurityHomePageMenuItem;

		public MenuItem BiosecurityComtainerRequirementsMenuItem => base.biosecurityComtainerRequirementsMenuItem;

		public MenuItem MAFWebSiteMenuGroup => base.mAFWebSiteMenuGroup;

		public MenuItem MAFHomePageMenuItem => base.mAFHomePageMenuItem;

		public MenuItem MAFQuarrantineCargoInfoMenuItem => base.mAFQuarrantineCargoInfoMenuItem;

		public MenuItem SubmitJobMenuItem => base.submitJobMenuItem;

		public MenuItem CancelJobMenuItem => base.cancelJobMenuItem;

		public MenuItem ResetToOriginalMenuItem => base.resetToOriginalMenuItem;

		public MenuItem TSWSubmitWithDetails => base.tSWSubmitWithDetails;

		public MenuItem TSWQueueForManifesting => base.tSWQueueForManifesting;

		public MenuItem Separator1 => base.separator1;

		public MenuItem TSWSeparator => base.tSWSeparator;
	}
}
