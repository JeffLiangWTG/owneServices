using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module.Organisation.OrgImport;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrganisationModule))]
	public class OrganisationModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestSendImporterBondQueryMenuItem_NoPermissionToSendSSN()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			var menuItemName = "&Send Importer Bond Query";
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				var orgHeaderFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};

				var orgHeader1 = orgHeaderFactory.NewWithValidTestData<OrgHeader>();
				orgHeader1.OH_Code = "ORGAAA";
				orgHeader1.OH_IsConsignee = false;
				var orgHeader2 = orgHeaderFactory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_Code = "ORGBBB";
				orgHeader2.OH_IsConsignee = true;
				var cusCode1 = orgHeader2.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
				if (cusCode1 == null)
				{
					orgHeader2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123121234");
				}
				var orgHeader3 = orgHeaderFactory.NewWithValidTestData<OrgHeader>();
				orgHeader3.OH_Code = "ORGCCC";
				orgHeader3.OH_IsConsignee = true;
				var cusCode2 = orgHeader3.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.SocialSecurityNumber, Core.Constants.CountryCodes.UnitedStates);
				if (cusCode2 == null)
				{
					orgHeader3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123-12-1234");
				}

				var orgHeaderCollection = new OrgHeaderCollection(orgHeaderFactory) { orgHeader1, orgHeader2, orgHeader3 };
				orgHeaderFactory.Save();

				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
				moduleForTest.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");
				moduleForTest.Grid_Exposed.DataSource = orgHeaderCollection;
				var menuItem = moduleForTest.FormActionMenu.FindByText(menuItemName, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(@"0 message(s) sent.
There is no Importer Bond Query message sent for following organizations because they are not marked as Consignee. 
  ORGAAA.
There is no Importer Bond Query message sent for following organizations because they don't have a valid EIN or CBP Assigned Number or Social Security number. 
  ORGBBB.
There is no Importer Bond Query message sent for following organizations because you do not have the security right to view personal information. 
  ORGCCC.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendImporterBondQueryMenuItem()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var menuItemName = "&Send Importer Bond Query";
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				Assert("Send Importer Bond Query", CheckMenuItemIsAdded(moduleForTest, menuItemName));
			}
		}

		public void TestCheckCopySelectedRowsAllowed()
		{
			using (TestOrganisationModule module = new TestOrganisationModule())
			{
				bool prev = module.ExportSecurityCheckpoint_Exposed.IsAllowed;
				try
				{
					module.ExportSecurityCheckpoint_Exposed.IsAllowed = true;
					Assert(module.CheckCopySelectedRowsAllowed_Exposed());

					module.ExportSecurityCheckpoint_Exposed.IsAllowed = false;
					Assert(!module.CheckCopySelectedRowsAllowed_Exposed());
				}
				finally
				{
					module.ExportSecurityCheckpoint_Exposed.IsAllowed = prev;
				}
			}
		}

		public void TestDeniedPartyScreeningMenuAdded()
		{
			using (var module = new OrganisationModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Screen", true));
			}
		}

		#region ExportPatternMatchOverride

		public void TestPatternOverrideMenuAdded()
		{
			string exportNativeXMLText = "Export Native XML";
			string exportPatternOverrideMenuText = "Export Pattern Match Overrides as Native XML";

			using (var module = new OrganisationModule())
			{
				var dataTransferSubMenu = module.FormActionMenu.FindByText("D&ata Transfer", true);
				Assert("Pre-condition", dataTransferSubMenu != null);

				dataTransferSubMenu.ShowPopupMenu();
				var exportXmlMenu = dataTransferSubMenu.MenuItems.FindByText(exportNativeXMLText, true);
				Assert("Pre-condition", exportXmlMenu != null);

				var exportOverrideXmlMenu = dataTransferSubMenu.MenuItems.FindByText(exportPatternOverrideMenuText, true);
				AssertNotNull($"'{exportPatternOverrideMenuText}' Menu Item", exportOverrideXmlMenu);
				AssertEquals($"'{exportPatternOverrideMenuText}' is after '{exportNativeXMLText}' Menu Item", exportXmlMenu.Index + 1, exportOverrideXmlMenu.Index);

				dataTransferSubMenu.ShowPopupMenu();

				MenuItem duplicateMenu = null;
				if (dataTransferSubMenu.MenuItems.Count > exportOverrideXmlMenu.Index + 1)
				{
					duplicateMenu = dataTransferSubMenu.MenuItems[exportOverrideXmlMenu.Index + 1];
				}

				Assert("Menu must not be added twice", duplicateMenu == null || duplicateMenu.Text != exportPatternOverrideMenuText);
			}
		}

		[RequiresSTA]
		public void TestPatternOverrideUtilHasExpectedOrgs()
		{
			var orgs = new List<OrgHeader>() { Factory.NewWithValidTestData<OrgHeader>(), Factory.NewWithValidTestData<OrgHeader>() };
			Factory.Save();

			using (var orgModule = new TestOrganisationModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(orgModule.EmbeddedControl);
				form.Show();

				foreach (var org in orgs)
				{
					var filter = orgModule.FilterBusinessObject.AddTextFilterStrip("Code", org.OH_Code);
					filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					filter.OrCategory = FilterOrCategory.AliceBlue;
				}

				orgModule.PerformSearch();
				orgModule.Grid_Exposed.SelectAllElements();

				AssertNotNull("Util has GetOrgHeaders delegate set", orgModule.ExportPatternMatchOverridesUtil_Exposed.GetOrgHeadersExposedForTesting);
				AssertContainsExactElementsInAnyOrder("Util orgs matched grids selected orgs", orgs.Select(x => x.PK), orgModule.ExportPatternMatchOverridesUtil_Exposed?.GetOrgHeadersExposedForTesting().Select(x => x.PK));
			}
		}

		#endregion ExportPatternMatchOverride

		public void TestBusinessContexts()
		{
			using (OrganisationModule module = new OrganisationModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Organisation;
		}

		public void TestMergeOrganisationSecurity()
		{
			bool previousMergeOrgSecurity = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;

			try
			{
				using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
				{
					Env.Security.OrgDuplicateDetectionMerge.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					moduleForTest.MergeOrganisation_Exposed();
					Assert("Message shown about no security", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

					Env.Security.OrgDuplicateDetectionMerge.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					moduleForTest.MergeOrganisation_Exposed();
					AssertEquals("No Message shown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousMergeOrgSecurity;
			}
		}

		[RequiresSTA]
		public void TestMergeSelectedOrganisationSecurity()
		{
			bool previousMergeOrgSecurity = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;

			try
			{
				using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
				{
					var orgHeaderFactory = new BusinessObjectFactory()
					{
						RefreshEnabled = false
					};

					var orgHeaderCollection = new OrgHeaderCollection(orgHeaderFactory)
					{
						orgHeaderFactory.NewWithValidTestData<OrgHeader>()
					};
					orgHeaderFactory.Save();

					moduleForTest.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");
					moduleForTest.Grid_Exposed.Select(0);

					Env.Security.OrgDuplicateDetectionMerge.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					moduleForTest.FindAndMergeSimilarOrgs_Exposed();
					Assert("Message shown about no security", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

					Env.Security.OrgDuplicateDetectionMerge.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					moduleForTest.FindAndMergeSimilarOrgs_Exposed();
					Assert("No Message shown", !UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousMergeOrgSecurity;
			}
		}

		[RequiresSTA]
		public void TestMergeSelectedOrganisationMergeOnlySelected()
		{
			bool previousMergeOrgSecurity = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;

			try
			{
				using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
				{
					var orgHeaderFactory = new BusinessObjectFactory()
					{
						RefreshEnabled = false
					};

					var orgHeaderCollection = new OrgHeaderCollection(orgHeaderFactory)
					{
						orgHeaderFactory.NewWithValidTestData<OrgHeader>(),
						orgHeaderFactory.NewWithValidTestData<OrgHeader>(),
						orgHeaderFactory.NewWithValidTestData<OrgHeader>()
					};
					orgHeaderFactory.Save();

					moduleForTest.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");
					moduleForTest.Grid_Exposed.Select(0);

					AssertEquals(moduleForTest.Grid_Exposed.ListManager.Count, 3);
					Env.Security.OrgDuplicateDetectionMerge.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					moduleForTest.FindAndMergeSimilarOrgs_Exposed();
					Assert("Only 1 OrgHeader merged", UnitTestUserNotification.Instance.LastMessage.Contains("Number of Organizations processed is 1."));
				}
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousMergeOrgSecurity;
			}
		}

		[RequiresSTA]
		public void TestMergeSelectedOrganisationMessageWhenNoItemSelected()
		{
			bool previousMergeOrgSecurity = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;

			try
			{
				using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
				{
					Env.Security.OrgDuplicateDetectionMerge.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					moduleForTest.FindAndMergeSimilarOrgs_Exposed();
					AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Please select a record in the grid.");
				}
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousMergeOrgSecurity;
			}
		}

		public void TestActivateDeactivateOrganisationSecurity()
		{
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				AssertExceptionThrown("this shows GridSelectedElements gets hit", typeof(Exception), moduleForTest.OnActivate_Exposed);
				AssertExceptionThrown("this shows GridSelectedElements gets hit", typeof(Exception), moduleForTest.OnDeActivate_Exposed);
			}
		}

		public void TestCountrySpecificMenu()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				MenuItem item = GetMenuItem(moduleForTest, "&UEN Reference Update");
				Assert(item.Enabled);
				Assert(item.Visible);
				AssertEquals("&UEN Reference Update", item.Text);
			}
		}

		#region TestGetNewActionMenuItems_ReDefaultARAPTaxSettingsMenuItem

		public void TestGetNewActionMenuItems_ReDefaultARAPTaxSettingsMenuItem()
		{
			using (var module = new TestOrganisationModule())
			{
				var actionsMenu = module.FormActionMenu.FindByText("Actions");
				AssertNotNull("&Re-Default AR/AP Tax Settings should exist", actionsMenu.MenuItems.FindByText("&Re-Default AR/AP Tax Settings"));
			}
		}

		#endregion

		#region TestGetNewActionMenuItems_ActiveSystemDataMergeMenuItem

		public void TestGetNewActionMenuItems_ActiveSystemDataMergeMenuItem()
		{
			string menuItemName = "System Data Merge (CargoWise to CargoWise)";

			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Assert("System Data Merge menu item should be inactive if ActivateSystemMergeDataInterface is false", !CheckMenuItemIsAdded(moduleForTest, menuItemName));
			}

			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Assert("System Data Merge menu item should be active if ActivateSystemMergeDataInterface is true", CheckMenuItemIsAdded(moduleForTest, menuItemName));
			}
		}

		#endregion

		#region TestGetNewActionMenuItems_WarehouseInventory

		public void TestGetNewActionMenuItems_WarehouseInventory()
		{
			SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var module = new TestOrganisationModule())
			{
				var actionsMenu = module.FormActionMenu.FindByText("Actions");
				var mergeMenu = actionsMenu.MenuItems.FindByText("System Data Merge (CargoWise to CargoWise)");
				AssertNotNull("Warehouse inventory import should be available to user.", mergeMenu.MenuItems.FindByText("Import Warehouse Inventory"));
				AssertNotNull("Warehouse inventory export should be available to user.", mergeMenu.MenuItems.FindByText("Export Warehouse Inventory"));
			}
		}

		#endregion

		#region TestGetNewActionMenuItems_EPaymentConfigurationsMenuItem

		public void TestGetNewActionMenuItems_EPaymentConfigurationsMenuItem()
		{
			AssertGetNewActionMenuItems_EPaymentConfigurationsMenuItem(true);
			AssertGetNewActionMenuItems_EPaymentConfigurationsMenuItem(false);
		}

		void AssertGetNewActionMenuItems_EPaymentConfigurationsMenuItem(bool isOFXEPaymentEnabled)
		{
			string menuItemName = "E-Payment Configurations";
			string submenuItemName = "&Match Recipients for E-Payment";
			var configurationCollection = new EPaymentConfigurationCollection();
			var config = configurationCollection.AddNew();
			config.CountryCode = GlbCompany.CurrentCompany.Country.RN_Code;
			config.CountryDescription = GlbCompany.CurrentCompany.Country.RN_Desc;
			config.OFXEPaymentEnabled = isOFXEPaymentEnabled;
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), isOFXEPaymentEnabled))
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				AssertEquals(isOFXEPaymentEnabled, CheckMenuItemIsAdded(moduleForTest, menuItemName));

				if (isOFXEPaymentEnabled)
				{
					var menu = moduleForTest.FormActionMenu.FindByText(menuItemName, true);
					menu.ShowPopupMenu();
					var subMenu = menu.MenuItems.FindByText(submenuItemName, true);
					AssertNotNull(subMenu);
				}
			}
		}

		public void TestEPaymentConfigurationsMenuItemSecurityRight()
		{
			string menuItemName = "E-Payment Configurations";
			string submenuItemName = "&Match Recipients for E-Payment";
			var configurationCollection = new EPaymentConfigurationCollection();
			var config = configurationCollection.AddNew();
			config.CountryCode = GlbCompany.CurrentCompany.Country.RN_Code;
			config.CountryDescription = GlbCompany.CurrentCompany.Country.RN_Desc;
			config.OFXEPaymentEnabled = true;
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			using (var moduleForTest = new TestOrganisationModule())
			{
				var menu = moduleForTest.FormActionMenu.FindByText(menuItemName, true);
				menu.ShowPopupMenu();
				var subMenu = menu.MenuItems.FindByText(submenuItemName, true);
				Env.Security.OrganisationMatchRecipientsForEPayment.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessages();
				subMenu.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> E-Payment Configurations -> Match Recipients for E-Payment", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrganisationMatchRecipientsForEPayment.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessages();
				subMenu.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestGetNewSystemMergeDataTransferExporters

		public void TestGetNewSystemMergeDataTransferExporters()
		{
			using (var module = new TestOrganisationModule())
			{
				AssertNotNull("SystemMergeOrgDataTransferExporter", module.GetNewSysMergeExporter("Organization"));
				AssertNotNull("SystemMergeWarehousesDataTransferExporter", module.GetNewSysMergeExporter("Warehouses"));
				AssertNotNull("SystemMergeRatingDataTransferExporter", module.GetNewSysMergeExporter("Rating"));
				AssertNotNull("SystemMergeProductDataTransferExporter", module.GetNewSysMergeExporter("Product"));
				AssertNotNull("SystemMergeEdocsDataTransferExporter", module.GetNewSysMergeExporter("EDocs"));
				AssertNotNull("SystemMergeCusClassificationsDataTransferExporter", module.GetNewSysMergeExporter("Classifications"));
				AssertNotNull("SystemMergeWarehouseInventoryDataTransferExporter", module.GetNewSysMergeExporter("WarehouseInventory"));
			}
		}

		#endregion

		#region TestGetNewSystemMergeDataTransferImporters

		public void TestGetNewSystemMergeDataTransferImporters()
		{
			using (var module = new TestOrganisationModule())
			{
				AssertNotNull("SystemMergeOrgDataTransferImporter", module.GetNewSysMergeImporter("Organization"));
				AssertNotNull("SystemMergeWarehousesDataTransferImporter", module.GetNewSysMergeImporter("Warehouses"));
				AssertNotNull("SystemMergeRatingDataTransferImporter", module.GetNewSysMergeImporter("Rating"));
				AssertNotNull("SystemMergeProductDataTransferImporter", module.GetNewSysMergeImporter("Product"));
				AssertNotNull("SystemMergeEdocsDataTransferImporter", module.GetNewSysMergeImporter("EDocs"));
				AssertNotNull("SystemMergeCusClassificationsDataTransferImporter", module.GetNewSysMergeImporter("Classifications"));
				AssertNotNull("SystemMergeWarehouseInventoryDataTransferImporter", module.GetNewSysMergeImporter("WarehouseInventory"));
			}
		}

		#endregion

		#region TestReDefaultARAPTaxSettings

		public void TestReDefaultARAPTaxSettings()
		{
			using (var module = new TestOrganisationModule())
			{
				var reDefaultARAPTaxSettingsMenuItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("&Re-Default AR/AP Tax Settings");

				Env.Security.OrganisationAllowReDefaultARAPTaxSettings.IsAllowed = false;
				reDefaultARAPTaxSettingsMenuItem.PerformClick();

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Allow Re-Default AR/AP Tax Settings", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Env.Security.OrganisationAllowReDefaultARAPTaxSettings.IsAllowed = true;
				reDefaultARAPTaxSettingsMenuItem.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.Any(msg => msg.Text == @"This action will re-default the Tax (GST/VAT) settings on ALL Organizations flagged as either a Receivables or Payables Organization Type.
Are you sure you want to re-default your Tax settings?"));
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.Any(msg => msg.Caption == "Re-Default AR/AP Tax Settings"));
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.Any(msg => msg.Text == @"NOTE: Changing the Tax settings will only affect NEW transactions. Transactions ALREADY POSTED for each Creditor / Debtor will NOT change."));
				AssertEquals("Tax Settings Re-Defaulted", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestGenerateClientNumber

		[RequiresSTA]
		public void TestGenerateClientNumber()
		{
			using (TestOrganisationModule module = new TestOrganisationModule())
			{
				var actionsMenu = module.FormActionMenu.FindByText("Actions");
				var generateMenu = actionsMenu.MenuItems.FindByText("Generate Client Number");
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					Env.Security.OrganisationGenerateClientNumber.IsAllowed = false;
					generateMenu.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Generate Client Number", UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.OrganisationGenerateClientNumber.IsAllowed = true;

					var orgHeaderCollection = new OrgHeaderCollection(Factory);
					module.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");

					UnitTestUserNotification.Instance.ClearMessages();
					generateMenu.PerformClick();
					AssertEquals("You have not selected any Organizations.", UnitTestUserNotification.Instance.LastMessage.Text);

					var query = new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, true);
					query.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					var companyData = Factory.LoadTop1<OrgCompanyData>(query);
					orgHeaderCollection.Add(companyData.Organisation);

					AssertNullOrEmpty(companyData.OB_ARClientNumber);

					UnitTestUserNotification.Instance.ClearMessages();
					module.Grid_Exposed.Select(0);
					generateMenu.PerformClick();
					AssertEquals("Client Numbers are generated.", UnitTestUserNotification.Instance.LastMessage.Text);

					var newCompanyData = new BusinessObjectFactory().Load<OrgCompanyData>(companyData.PK);
					AssertNotNullOrEmpty(newCompanyData.OB_ARClientNumber);

					UnitTestUserNotification.Instance.ClearMessages();
					module.Grid_Exposed.Select(0);
					generateMenu.PerformClick();
					AssertContains("Client Number has already been generated for the following organizations:", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region Test Organisation Rate Update File
		public void TestMenuOrganisationRateUpdateFile_Country_NotImplement_GetCountryFactoryReturnNull()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(() => null);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (var moduleForTest = new TestOrganisationModule())
			{
				AssertEquals(false, CheckMenuItemIsAdded(moduleForTest, menuItemName));
				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			}
		}

		public void TestMenuOrganisationRateUpdateFile_Country_GetCountryFactoryCurrentCompany()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			AssertCheckMenuItemIsAdded(false);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Argentina);
			AssertCheckMenuItemIsAdded(true);

			void AssertCheckMenuItemIsAdded(bool expectedExistMenu)
			{
				using (var moduleForTest = new TestOrganisationModule())
				{
					AssertEquals(expectedExistMenu, CheckMenuItemIsAdded(moduleForTest, menuItemName));
				}
			}
		}

		public void TestMenuOrganisationRateUpdateFile_Country_NotImplement_IOrgTaxRateImportFileFormatProvider()
		{
			string menuItemName = "Tax Configuration Organization Rate Update File Import";

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (var moduleForTest = new TestOrganisationModule())
			{
				AssertEquals(false, CheckMenuItemIsAdded(moduleForTest, menuItemName));
				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			}
		}

		public void TestMenuOrganisationRateUpdateFile_Country_Implement_IOrgTaxRateImportFileFormatProvider()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			var (mockIGlobalAccountingCountryFactory, mockIAccountingCountryFactory) = SetupCountryImplementIOrgTaxRateImportFileFormatProviderMocks();
			var (factoryMock, _) = SetupHelperMocks();

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (ObjectFactory.Substitute(factoryMock.Object))
			using (var moduleForTest = new TestOrganisationModule())
			{
				var menuOptionOrganisationRateUpdateFile = GetMenuItem(moduleForTest, menuItemName);

				Assert(menuOptionOrganisationRateUpdateFile.Visible);
				AssertEquals(menuItemName, menuOptionOrganisationRateUpdateFile.Text);

				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
				mockIAccountingCountryFactory.As<IInstanceProvider<IOrgTaxRateImportFileFormatProvider>>().Verify(x => x.Get(), Times.Once);
			}
		}

		public void TestMenuOrganisationRateUpdateFile_Country_Implement_HasAnyTaxConfigurationThatSupportsOrganisationRatesWithParameterCurrentCompany()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			var (mockIGlobalAccountingCountryFactory, _) = SetupCountryImplementIOrgTaxRateImportFileFormatProviderMocks();
			var (factoryMock, helperMock) = SetupHelperMocks();

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (ObjectFactory.Substitute(factoryMock.Object))
			using (var moduleForTest = new TestOrganisationModule())
			{
				var menuOptionOrganisationRateUpdateFile = GetMenuItem(moduleForTest, menuItemName);

				helperMock.Verify(x => x.HasAnyTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), GlbCompany.CurrentCompany), Times.Once);
			}
		}

		public void TestOrganisationRateUpdateFile_IsEnabledMenu_WhenCountryHaveTaxConfigurationNotSetCorrectly()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			var (mockIGlobalAccountingCountryFactory, _) = SetupCountryImplementIOrgTaxRateImportFileFormatProviderMocks();

			AssertIfMenuMustEnabled("The 'Import Organisation Rate File' menu item should be enabled if there are correctly configured tax settings, and the country is set to the default value.", expectedReturnFromHelper: false, expectedEnabledMenu: false);
			AssertIfMenuMustEnabled("The 'Import Organisation Rate File' menu item should not be enabled if the tax settings are not correctly configured and the country is is set to the default value..", expectedReturnFromHelper: true, expectedEnabledMenu: true);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Argentina);
			AssertIfMenuMustEnabled("The 'Import Organisation Rate File' menu item should be enabled if there are correctly configured tax settings, and the country is Argentina.", expectedReturnFromHelper: false, expectedEnabledMenu: false);
			AssertIfMenuMustEnabled("The 'Import Organisation Rate File' menu item should not be enabled if the tax settings are not correctly configured and the country is Argentina.", expectedReturnFromHelper: true, expectedEnabledMenu: true);

			void AssertIfMenuMustEnabled(string assertMessage, bool expectedReturnFromHelper = false, bool expectedEnabledMenu = false, string countryCode = Enterprise.Core.Constants.CountryCodes.Argentina)
			{
				var (factoryMock, helperMock) = SetupHelperMocks();
				helperMock.Setup(x => x.HasAnyTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(() => expectedReturnFromHelper);

				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				using (ObjectFactory.Substitute(factoryMock.Object))
				using (var moduleForTest = new TestOrganisationModule())
				{
					var menuOptionOrganisationRateUpdateFile = GetMenuItem(moduleForTest, menuItemName);
					AssertEquals(expectedEnabledMenu, menuOptionOrganisationRateUpdateFile.Enabled);
				}
			}
		}

		[RequiresSTA]
		public void TestOrganisationRateUpdateFile_FormOpenWhenClickOnMenuOption()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			var (mockIGlobalAccountingCountryFactory, _) = SetupCountryImplementIOrgTaxRateImportFileFormatProviderMocks();
			var (factoryMock, _) = SetupHelperMocks();

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (ObjectFactory.Substitute(factoryMock.Object))
			using (var moduleForTest = new TestOrganisationModule())
			{
				var forms = ZApplication.GetOpenForms().OfType<OrganisationTaxRateFileImportForm>().ToArray();
				Assert("Precondition - No opened OrganisationTaxRateFileImportForm form", forms.IsNullOrEmpty());

				var menuOptionOrganisationRateUpdateFile = GetMenuItem(moduleForTest, menuItemName);
				menuOptionOrganisationRateUpdateFile.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				forms = ZApplication.GetOpenForms().OfType<OrganisationTaxRateFileImportForm>().ToArray();
				AssertEquals("Open one OrganisationTaxRateFileImportForm form", 1, forms.Length);

				forms.ForEach(form => form.Dispose());
			}
		}

		public void TestOrganisationRateUpdateFile_SecurityRights()
		{
			var menuItemName = "Tax Configuration Organization Rate Update File Import";

			var (mockIGlobalAccountingCountryFactory, _) = SetupCountryImplementIOrgTaxRateImportFileFormatProviderMocks();
			var (factoryMock, _) = SetupHelperMocks();

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (ObjectFactory.Substitute(factoryMock.Object))
			using (var moduleForTest = new TestOrganisationModule())
			{
				Env.Security.OrganisationTaxRateFileImport.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessages();

				var menuOptionOrganisationRateUpdateFile = GetMenuItem(moduleForTest, menuItemName);
				menuOptionOrganisationRateUpdateFile.PerformClick();

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Allow Organization Tax Rate File Import", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrganisationTaxRateFileImport.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessages();
				menuOptionOrganisationRateUpdateFile.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				ZApplication.GetOpenForms().OfType<OrganisationTaxRateFileImportForm>().ToArray().ForEach(f => f.Dispose());
			}
		}

		#region Implementation

		static (Mock<IGlobalAccountingCountryFactory> mockIGlobalAccountingCountryFactory, Mock<IAccountingCountryFactory> mockIAccountingCountryFactory) SetupCountryImplementIOrgTaxRateImportFileFormatProviderMocks()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIOrgTaxRateImportFileFormatProvider = new Mock<IOrgTaxRateImportFileFormatProvider>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IOrgTaxRateImportFileFormatProvider>>().Setup(x => x.Get()).Returns(mockIOrgTaxRateImportFileFormatProvider.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			return (mockIGlobalAccountingCountryFactory, mockIAccountingCountryFactory);
		}

		(Mock<IAccountingMasterFilesDependencyFactory> factoryMock, Mock<ITaxFrameworkConfigurationHelper> helperMock) SetupHelperMocks()
		{
			var factoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
			var helperMock = new Mock<ITaxFrameworkConfigurationHelper>();

			helperMock.Setup(x => x.HasAnyTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(true);
			helperMock.Setup(x => x.GetCompanyTaxConfigurations(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZQuery>())).Returns(new AccTaxConfigurationCollection(Factory, ZQuery.NoResultQuery));
			factoryMock.Setup(x => x.GetTaxFrameworkConfigurationHelper()).Returns(helperMock.Object);

			return (factoryMock, helperMock);
		}

		#endregion

		#endregion

		public void TestDefaultMessageWhenCreatingANewBizObjFromFindBox()
		{
			using (TestOrganisationModule module = new TestOrganisationModule())
			using (ZCodeFindBox findBox = new ZCodeFindBox())
			{
				OrganisationsFindBoxCollection orgList = new OrganisationsFindBoxCollection(Factory);

				findBox.List = orgList;
				findBox.ModuleID = module.ID;
				findBox.CodeBox.Text = OrgHeader.UnmatchedOrganisationCode;

				AssertEquals("Precondition: orgList doesn't have any conditional orgfieldDefaults", 0, ((IOrganisationDefaultProvider)orgList).ConditionalDefaults.Count);
				ZString expectedMesgReturned = $"The code '{OrgHeader.UnmatchedOrganisationCode}' does not exist. Would you like to create a new {module.Description}?";
				AssertEquals("message returned", expectedMesgReturned, module.DefaultMessageWhenCreatingANewBizObjFromFindBox(findBox));

				orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgAddress.Schema.OA_Address1, Value = new ZString("aaa") });
				expectedMesgReturned = $"Would you like to create a new organization with details defaulted from '{PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description}' Note?";
				AssertEquals("message returned", expectedMesgReturned, module.DefaultMessageWhenCreatingANewBizObjFromFindBox(findBox));
			}
		}

		public void TestAllMenuItemsHaveProperOrgSpelling()
		{
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				foreach (MenuItem item in moduleForTest.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
				{
					Assert("Organization should be written via Z", !item.Text.ToLower().Contains("organisation"));
				}
			}
		}

		public void TestFindAndMergeSimilarAction_HasCorrectWording()
		{
			var expectedText = "&Find and Merge Similar Organizations...";
			using (TestOrganisationModule moduleForTest = new TestOrganisationModule())
			{
				var actionMenuItems = moduleForTest.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.OfType<MenuItem>();
				AssertNoExceptionThrown(() => actionMenuItems.Single(item => item.Text == expectedText));
			}
		}

		public void TestSupportsWorkflow()
		{
			using (TestOrganisationModule module = new TestOrganisationModule())
			{
				Assert("Should support Worflow", module.SupportsWorkflow);
			}
		}

		void OutputHeaderLine(StreamWriter sw)
		{
			OutputHeaderLine(sw, true);
		}

		void OutputHeaderLine(StreamWriter sw, bool withOptionalCols)
		{
			string optionalCols = withOptionalCols ? ",BankCurrency,Warehouse" : "";
			sw.WriteLine(string.Format("Code,Name,Address1,Address2,City,State,PostCode,UNLOCO,Country,PortCity,Phone,Fax,Email,Web,RegNo,CorpCode,Debtor,Creditor,Consignee,Consignor,Forwarder,Broker,Carrier,ShipLine,Airline,LocalTransport,SalesLead,Services,Competitor,Contact,Title,Email,Phone,Mobile,Fax,DebtorCode,DebtorGroup,DebtorSettleGroup,Currency,CreditLimit,CreditRating,GST,INV_TERMS_STANDARD,INV_DAYS_STANDARD,INV_TERMS_DISBURSEMENT,INV_DAYS_DISBURSEMENT,CreditorGroup,CustomsAgent,PostAddress1,PostAddress2,PostCity,PostState,PostPostCode,DeliverAddress1,DeliverAddress2,DeliverCity,DeliverState,DeliverPostCode,Bank,AccountName,AccountNo,BSB,CCD,CSC,SCC,CCP,CCC,CMP,WorkNotes,HandlingNotes,DeliveryNotes,ARNotes,ARCreditNotes,APNotes,ContactSourceType,ContactDateDetailsVerified,ContactSalutation,Language,Mainaddresslanguage,Postaladdresslanguage,Deliveryaddresslanguage" + optionalCols));
		}

		OrgHeader LoadOrganisationByCode(ZString code)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_Code, code);

			var foundOrg = Factory.LoadTop1<OrgHeader>(query);

			AssertNotNull(foundOrg);

			return foundOrg;
		}

		string GetNoteByDescription(string desc, BusinessObjectCollection notes)
		{
			string noteText = "";
			foreach (StmNote note in notes)
			{
				if (note.ST_Description == desc)
				{
					noteText = note.ST_NoteDataAsText;
					break;
				}
			}

			return noteText;
		}

		[RequiresSTA]
		public void TestImportDataWizardMenuItem()
		{
			AssertImportByDataWizardRequiresImportToSystemPrivilege(Env.Security.Organisation, Env.Security.OrganisationNew);
		}

		public void TestProcessingImport()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgFlattenedCollection orgCollection = new OrgFlattenedCollection(Factory);
			OrgFlattened org = orgCollection.AddNew();
			org.OH_Code = "ORG1";
			org.OH_FullName = "Organisation One";
			org.OA_Address1 = "31 Blah St";
			org.OA_Address2 = "Bardon";
			org.OA_City = "Brisbane";
			org.Country = "AU";
			org.OA_State = "QLD";
			org.OA_PostCode = "4000";
			org.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org.OH_RL_NKClosestPort = "AUSYD"; // pick a location in another state, because this property will change OA_State
			org.OA_Phone = "85555555";
			org.OA_Fax = "85555556";
			org.OA_Email = "orga@orgone.com";

			org.PU_URL = "http://www.orgone.com";
			org.OH_Language = Core.SharedConstants.Languages.Greek;

			org.BusRegNo = "23 112 936 991";
			org.BusRegACN = "22222222";
			org.OB_IsDebtor = true;
			org.OB_IsCreditor = true;
			org.OH_IsConsignee = false;
			org.OH_IsConsignor = false;
			org.OH_IsForwarder = true;
			org.OH_IsBroker = true;
			org.OH_IsShippingProvider = false;
			org.OH_IsShippingLine = false;
			org.OH_IsAirLine = true;
			org.OH_IsLocalTransport = true;
			org.OH_IsSalesLead = false;
			org.OH_IsMiscFreightServices = false;
			org.OH_IsCompetitor = true;

			org.DebtorGroup = "DG1";
			org.CurrencyCode = "USD";
			org.CreditLimit = 20000.00;
			org.OM_ARCreditRating = "CR3";

			org.GSTApplicable = true;
			org.PY_InvoiceTerm = "INV";
			org.PY_InvoiceDays = 7;
			org.InvoiceTermDisbursement = "COD";
			org.InvoiceDaysDisbursement = 14;
			org.CreditorGroup = "CG1";

			org.A1_BankName = "Org One Bank";
			org.A1_AccountName = "";
			org.A1_BankAccount = "100119914";
			org.A1_BankBsb = "332085";

			org.CustomsCode = "1111111";
			org.SupplierCode = "2222222";
			org.CMRSupplierCode = "3333333";
			org.PremiseID = "44444444";
			org.CarrierCode = "55555555";
			org.ManifestProviderCode = "66666666";

			org.WorkNotes = "work note";
			org.ForwardingNotes = "forwarding note";
			org.DeliveryNotes = "delivery note";
			org.ARNotes = "AR note";
			org.InvoiceNotes = "Invoice note";
			org.APNotes = "AP note";

			org.Postal_OA_Address1 = "32 Postal St";
			org.Postal_OA_Address2 = "Postal";
			org.Postal_OA_City = "Brisbane";
			org.Postal_OA_State = "QLD";
			org.Postal_OA_PostCode = "4000";
			org.Postal_OA_Language = Core.SharedConstants.Languages.Greek;

			org.Delivery_OA_Address1 = "33 Delivery St";
			org.Delivery_OA_Address2 = "Delivery";
			org.Delivery_OA_City = "Brisbane";
			org.Delivery_OA_State = "QLD";
			org.Delivery_OA_PostCode = "4000";
			org.Delivery_OA_Language = Core.SharedConstants.Languages.Greek;

			org.OH_Code = org.OH_Code;
			org.OC_ContactName = "John Doe";
			org.OC_Title = "Mr";
			org.OC_Email = "";
			org.OC_Phone = "85553234";
			org.OC_Mobile = "0402043234";
			org.OC_Fax = "85553235";
			org.OC_ContactSource = "Advertisement";
			org.OC_DetailsVerified = ZDateTime.Today;
			org.OC_Salutation = "John";

			// do import
			using (TestOrganisationModule module = new TestOrganisationModule())
			{
				module.ProcessImport(orgCollection);
			}

			// check import
			OrgHeader org1 = LoadOrganisationByCode("ORGONESYD");
			AssertEquals("ORGONESYD", org1.OH_Code);
			AssertEquals(org.OH_FullName, org1.OH_FullName);

			AssertEquals(org.OA_Address1, org1.MainAddress.OA_Address1);
			AssertEquals(org.OA_Address2, org1.MainAddress.OA_Address2);
			AssertEquals(org.OA_City, org1.MainAddress.OA_City);
			AssertEquals(org.OA_State, org1.MainAddress.OA_State);
			AssertEquals(org.OA_PostCode, org1.MainAddress.OA_PostCode);

			AssertEquals(org.OH_RL_NKClosestPort, org1.OH_RL_NKClosestPort);

			AssertEquals(org.OA_Phone, org1.MainAddress.OA_Phone);
			AssertEquals(org.OA_Fax, org1.MainAddress.OA_Fax);
			AssertEquals(org.OA_Email, org1.MainAddress.OA_Email);
			AssertEquals(org.OA_Language, org1.MainAddress.OA_Language);

			AssertEquals(org.PU_URL, org1.MainWebURL.PU_URL);
			AssertEquals(org.OH_Language, org1.OH_Language);

			AssertEquals(org.BusRegNo, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber));
			AssertEquals(org.BusRegACN, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CorporationCode));
			AssertEquals(org.OB_IsDebtor, org1.CompanyData.OB_IsDebtor);
			AssertEquals(org.OB_IsCreditor, org1.CompanyData.OB_IsCreditor);
			AssertEquals(org.OH_IsConsignee, org1.OH_IsConsignee);
			AssertEquals(org.OH_IsConsignor, org1.OH_IsConsignor);
			AssertEquals(org.OH_IsForwarder, org1.OH_IsForwarder);
			AssertEquals(org.OH_IsBroker, org1.OH_IsBroker);
			AssertEquals(org.OH_IsShippingProvider, org1.OH_IsShippingProvider);
			AssertEquals(org.OH_IsShippingLine, org1.OH_IsShippingLine);
			AssertEquals(org.OH_IsAirLine, org1.OH_IsAirLine);
			AssertEquals(org.OH_IsLocalTransport, org1.OH_IsLocalTransport);
			AssertEquals(org.OH_IsSalesLead, org1.OH_IsSalesLead);
			AssertEquals(org.OH_IsMiscFreightServices, org1.OH_IsMiscFreightServices);
			AssertEquals(org.OH_IsCompetitor, org1.OH_IsCompetitor);

			AssertNotEquals(Guid.Empty, org1.MiscServ.OM_OJ_ARDebtorGroup);
			AssertEquals(org.DebtorGroup, (Factory.Load<OrgDebtorGroup>(org1.MiscServ.OM_OJ_ARDebtorGroup)).OJ_Code);

			AssertEquals(org.CurrencyCode, org1.CompanyData.OB_RX_NKARDDefltCurrency);
			AssertEquals(org.CreditLimit, org1.MiscServ.OM_ARCreditLimit);
			AssertEquals(org.OM_ARCreditRating, org1.MiscServ.OM_ARCreditRating);

			AssertEquals(org.GSTApplicable, org1.CompanyData.IsARTaxApplicable);
			OrgARTerms termsAR = org1.CompanyData.CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.All.Code);
			AssertNotNull(termsAR);
			AssertEquals(org.PY_InvoiceTerm, termsAR.PY_InvoiceTerm);
			AssertEquals(org.PY_InvoiceDays, termsAR.PY_InvoiceDays);
			termsAR = org1.CompanyData.CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.DSB.Code);
			AssertNotNull(termsAR);
			AssertEquals(org.InvoiceTermDisbursement, termsAR.PY_InvoiceTerm);
			AssertEquals(org.InvoiceDaysDisbursement, termsAR.PY_InvoiceDays);

			AssertNotEquals(Guid.Empty, org1.MiscServ.OM_OG_APCreditorGroup);
			AssertEquals(org.CreditorGroup, (Factory.Load<OrgCreditorGroup>(org1.MiscServ.OM_OG_APCreditorGroup)).OG_Code);

			AssertEquals(org.A1_BankName, org1.MiscServ.OM_ARPreviousChequeDrawerBank);
			AssertEquals(org.A1_BankName, org1.CompanyData.AccountDetailsCollection[0].A1_BankName);
			AssertEquals(org.A1_AccountName, org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch);
			AssertEquals(org.A1_AccountName, org1.CompanyData.AccountDetailsCollection[0].A1_AccountName);
			AssertEquals(org.A1_BankAccount, org1.CompanyData.AccountDetailsCollection[0].A1_BankAccount);
			AssertEquals(org.A1_BankBsb, org1.CompanyData.AccountDetailsCollection[0].A1_BankBsb);

			AssertEquals(org.CustomsCode, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode));
			AssertEquals(org.SupplierCode, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.SupplierCode));
			AssertEquals(org.CMRSupplierCode, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientID));
			AssertEquals(org.PremiseID, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID));
			AssertEquals(org.CarrierCode, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode));
			AssertEquals(org.ManifestProviderCode, org1.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ManifestProviderID));

			AssertEquals(org.WorkNotes, org1.Notes.FindByDescription(PredefinedNoteTypes.Instance.InternalWorkNotes.Description)[0].ST_NoteDataAsText);
			AssertEquals(org.ForwardingNotes, org1.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)[0].ST_NoteDataAsText);
			AssertEquals(org.DeliveryNotes, org1.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description)[0].ST_NoteDataAsText);
			AssertEquals(org.ARNotes, org1.Notes.FindByDescription(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description)[0].ST_NoteDataAsText);
			AssertEquals(org.InvoiceNotes, org1.Notes.FindByDescription(PredefinedNoteTypes.Instance.AccountsReceivableCreditManagementNote.Description)[0].ST_NoteDataAsText);
			AssertEquals(org.APNotes, org1.Notes.FindByDescription(PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description)[0].ST_NoteDataAsText);

			// Address
			AssertEquals("The number of addresses on the organisation is incorrect", 3, org1.Addresses.Count);
			OrgAddress postalAddress = org1.Addresses[1];
			AssertEquals(OrgAddressType.Postal.Code, postalAddress.AddressCapability.GetListOfCodes());
			AssertEquals(org.Postal_OA_Address1, postalAddress.OA_Address1);
			AssertEquals(org.Postal_OA_Address2, postalAddress.OA_Address2);
			AssertEquals(org.Postal_OA_City, postalAddress.OA_City);
			AssertEquals(org.Postal_OA_State, postalAddress.OA_State);
			AssertEquals(org.Postal_OA_PostCode, postalAddress.OA_PostCode);
			AssertEquals(org.Postal_OA_Language, postalAddress.OA_Language);

			OrgAddress deliveryAddress = org1.Addresses[2];
			AssertEquals(OrgAddressType.Delivery.Code, deliveryAddress.AddressCapability.GetListOfCodes());
			AssertEquals(org.Delivery_OA_Address1, deliveryAddress.OA_Address1);
			AssertEquals(org.Delivery_OA_Address2, deliveryAddress.OA_Address2);
			AssertEquals(org.Delivery_OA_City, deliveryAddress.OA_City);
			AssertEquals(org.Delivery_OA_State, deliveryAddress.OA_State);
			AssertEquals(org.Delivery_OA_PostCode, deliveryAddress.OA_PostCode);
			AssertEquals(org.Delivery_OA_Language, deliveryAddress.OA_Language);

			// Contact
			AssertEquals("The number of contacts on the organisation is incorrect", 1, org1.Contacts.Count);
			OrgContact contact = org1.Contacts[0];
			AssertEquals(org.OC_ContactName, contact.OC_ContactName);
			AssertEquals(org.OC_Title, contact.OC_Title);
			AssertEquals(org.OC_Email, contact.OC_Email);
			AssertEquals(org.OC_Phone, contact.OC_Phone);
			AssertEquals(org.OC_Mobile, contact.OC_Mobile);
			AssertEquals(org.OC_Fax, contact.OC_Fax);
			AssertEquals(org.OC_ContactSource, contact.OC_ContactSource);
			AssertEquals(org.OC_DetailsVerified, contact.OC_DetailsVerified);
			AssertEquals(org.OC_Salutation, contact.OC_Salutation);
		}

		public void TestImportOrganisations()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					OutputHeaderLine(sw);
					sw.WriteLine("Z321Test2,Z321-Test Co Name,Test Address1,Test Address2, ,NSW,2000,AUSYD,AU,,99994444,99995555,test@email.au,www.test.com.au,999988885599,,Y,N,Y,Y,N,Y,N,N,N,N,N,N,N,,,,,,,,,,AUD,5000,CR2,Y,PER,14,INV,7,,,,,,,,,,,,,ANZ,,787458,012-045,,,,,,,Test notes,,,,,,25-12-2005,Monseiur,,,,,,N");
					sw.WriteLine("Z321Test1,Z321-Test Name,Address1,Address2,City,QLD,Post Code,AUBNE,Country/Region,Port City,Phone,Fax,Email,Web,Business Reg No,Government Corporation Code,Y,Y,Y,Y,Y,Y,Y,N,Y,Y,Y,Y,Y,Contact Name,Contact Job Title,Contact Email,Contact Work Phone,Contact Mobile,Contact Fax,Z321Test2,Debtor Acc Group,Z321Test2,HKD,1500,CR3,Y,,0,,0,CG1,Z321Test2,Postal Address1,Postal Address2,Postal City,Postal State,Postal Post Code,Deliver Address1,Deliver Address2,Deliver City,Deliver State,Deliver Post Code,Bank Name,Account Name,Account Number,BSB Number,Customs Client Code,Customs Supplier Code,Customs Supplier Code (CMR),Customs Controlled Premises Code,CCC1,Customs Manifest Provider Code,Bad Work Notes,Nice Goods Handling Instructions,Great Import Delivery Instructions,Fantastic A/R Account Management Notes,Interesting A/R Credit Management Note,Useless A/P Account Management Notes,050908_EXI_PRS,20051225,Senor,,,,,,Y");
					sw.WriteLine("Z321Test3,Z321-TestOrg,Addr1,Addr2,City,,2020,AUSYD,AU,,98250011,,,,,,N,Y,N,N,N,N,N,N,N,N,N,N,N,John Smith,Manager,,98250011,01481123456,98250013,,,,AUD,1500,,Y,,0,,0,,,,,,,,,,,,,ANZ,,457983723,012-344,,,,,,,,,,,,,,");
					sw.Flush();
				}

				using (TestOrganisationModule module = new TestOrganisationModule())
				{
					ImportWizard wizard = new ImportWizard((module as IImportCollectionInfoProvider).ImportCollectionInfo, null,
																								 new FileMapperForTest());
					wizard.FileName = testFileName.Filename;
					wizard.StartingRow = 2;

					ImportWizardMappingCollection orgLineCollection = wizard.Mapping;

					#region setColumnMapping

					SetColumnMapping(orgLineCollection, "Code", 0);
					SetColumnMapping(orgLineCollection, "Name", 1);
					SetColumnMapping(orgLineCollection, "Address 1", 2);
					SetColumnMapping(orgLineCollection, "Address 2", 3);
					SetColumnMapping(orgLineCollection, "City", 4);
					SetColumnMapping(orgLineCollection, "State", 5);
					SetColumnMapping(orgLineCollection, "Postcode", 6);
					SetColumnMapping(orgLineCollection, "UNLOCO", 7);
					SetColumnMapping(orgLineCollection, "Country/Region", 8);
					SetColumnMapping(orgLineCollection, "Port City", 9);
					SetColumnMapping(orgLineCollection, "Phone", 10);
					SetColumnMapping(orgLineCollection, "Fax", 11);
					SetColumnMapping(orgLineCollection, "Email", 12);
					SetColumnMapping(orgLineCollection, "Web", 13);
					SetColumnMapping(orgLineCollection, "Business Registration Number", 14);
					SetColumnMapping(orgLineCollection, "Government Corporation Code", 15);
					SetColumnMapping(orgLineCollection, "Debtor", 16);
					SetColumnMapping(orgLineCollection, "Creditor", 17);
					SetColumnMapping(orgLineCollection, "Consignee", 18);
					SetColumnMapping(orgLineCollection, "Consignor", 19);
					SetColumnMapping(orgLineCollection, "Forwarder", 20);
					SetColumnMapping(orgLineCollection, "Broker", 21);
					SetColumnMapping(orgLineCollection, "Carrier", 22);
					SetColumnMapping(orgLineCollection, "Ship Line", 23);
					SetColumnMapping(orgLineCollection, "Airline", 24);
					SetColumnMapping(orgLineCollection, "Port Transport", 25);
					SetColumnMapping(orgLineCollection, "Sales Lead", 26);
					SetColumnMapping(orgLineCollection, "Services", 27);
					SetColumnMapping(orgLineCollection, "Competitor", 28);
					SetColumnMapping(orgLineCollection, "Warehouse", 82);

					SetColumnMapping(orgLineCollection, "Contact Name", 29);
					SetColumnMapping(orgLineCollection, "Contact Job Title", 30);
					SetColumnMapping(orgLineCollection, "Contact Email", 31);
					SetColumnMapping(orgLineCollection, "Contact Phone", 32);
					SetColumnMapping(orgLineCollection, "Contact Mobile", 33);
					SetColumnMapping(orgLineCollection, "Contact Fax", 34);
					SetColumnMapping(orgLineCollection, "Contact Source Type", 74);
					SetColumnMapping(orgLineCollection, "Contact Date Details Verified", 75);
					SetColumnMapping(orgLineCollection, "Contact Salutation", 76);

					SetColumnMapping(orgLineCollection, "Debtor Code", 35);
					SetColumnMapping(orgLineCollection, "Debtor Group", 36);
					SetColumnMapping(orgLineCollection, "Debtor Settlement Group", 37);
					SetColumnMapping(orgLineCollection, "Currency", 38);
					SetColumnMapping(orgLineCollection, "Credit Limit", 39);
					SetColumnMapping(orgLineCollection, "Credit Rating", 40);
					SetColumnMapping(orgLineCollection, "GST", 41);
					SetColumnMapping(orgLineCollection, "Inv. Terms Standard", 42);
					SetColumnMapping(orgLineCollection, "Inv. Days Standard", 43);
					SetColumnMapping(orgLineCollection, "Inv. Terms Disbursement", 44);
					SetColumnMapping(orgLineCollection, "Inv. Days Disbursement", 45);
					SetColumnMapping(orgLineCollection, "Creditor Group", 46);
					SetColumnMapping(orgLineCollection, "Customs Agent", 47);

					SetColumnMapping(orgLineCollection, "Postal Address 1", 48);
					SetColumnMapping(orgLineCollection, "Postal Address 2", 49);
					SetColumnMapping(orgLineCollection, "Postal City", 50);
					SetColumnMapping(orgLineCollection, "Postal State", 51);
					SetColumnMapping(orgLineCollection, "Postal Postcode", 52);

					SetColumnMapping(orgLineCollection, "Delivery Address 1", 53);
					SetColumnMapping(orgLineCollection, "Delivery Address 2", 54);
					SetColumnMapping(orgLineCollection, "Delivery City", 55);
					SetColumnMapping(orgLineCollection, "Delivery State", 56);
					SetColumnMapping(orgLineCollection, "Delivery Postcode", 57);

					SetColumnMapping(orgLineCollection, "Bank", 58);
					SetColumnMapping(orgLineCollection, "Account Name", 59);
					SetColumnMapping(orgLineCollection, "Account Number", 60);
					SetColumnMapping(orgLineCollection, "BSB", 61);
					SetColumnMapping(orgLineCollection, "CCD", 62);
					SetColumnMapping(orgLineCollection, "CSC", 63);
					SetColumnMapping(orgLineCollection, "SCC", 64);
					SetColumnMapping(orgLineCollection, "CPP", 65);
					SetColumnMapping(orgLineCollection, "CCC", 66);
					SetColumnMapping(orgLineCollection, "CMP", 67);

					SetColumnMapping(orgLineCollection, "Work Notes", 68);
					SetColumnMapping(orgLineCollection, "Handling Notes", 69);
					SetColumnMapping(orgLineCollection, "Delivery Notes", 70);
					SetColumnMapping(orgLineCollection, "AR Notes", 71);
					SetColumnMapping(orgLineCollection, "AR Credit Notes", 72);
					SetColumnMapping(orgLineCollection, "AP Notes", 73);

					SetColumnMapping(orgLineCollection, "Contact Source Type", 74);
					SetColumnMapping(orgLineCollection, "Contact Date Details Verified", 75);
					SetColumnMapping(orgLineCollection, "Contact Salutation", 76);

					SetColumnMapping(orgLineCollection, "Language", 77);
					SetColumnMapping(orgLineCollection, "Main Address Language", 78);
					SetColumnMapping(orgLineCollection, "Postal Address Language", 79);
					SetColumnMapping(orgLineCollection, "Delivery Address Language", 80);

					#endregion

					IBusinessObjectCollection collection = wizard.CollectionInfo.Collection;
					AssertNoExceptionThrown(() => wizard.ImportIntoCollection(collection));
					module.ProcessImport();
				}

				AssertTestImportOrganisations();
			}
		}

		static void SetColumnMapping(ImportWizardMappingCollection lineCollection, string propName, int fileColumnIndex)
		{
			ImportWizardMapping orgColMapping = lineCollection.Cast<ImportWizardMapping>().FirstOrDefault(m => m.Text == propName);

			if (orgColMapping != null)
			{
				orgColMapping.AddFileColumnIndex(fileColumnIndex);
			}
			else
			{
				throw new ArgumentException("The specified property does not exist", nameof(propName));
			}
		}
		[RequiresSTA]
		public void TestModuleColumn()
		{
			string originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				using (var module = new TestOrganisationModule())
				{
					using (var form = new ZChildForm(module.GridCollection))
					{
						var control = (OrganisationFilterControl)module.EmbeddedControl;
						form.Controls.Add(control);
						form.Show();
						Assert(control.FilteredGrid.Columns.Contains("CAAccountSecurityNumber"));
					}
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		void AssertTestImportOrganisations()
		{
			ZQuery checkFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Z321-Test");
			checkFilter.OrderBy = OrgHeaderSchema.OH_FullName.Name;
			BusinessObject[] createdOrgs = Factory.Load(typeof(OrgHeader), checkFilter);
			AssertEquals("There should have been 3 organisations created", 3, createdOrgs.Length);

			for (int counter = 0; counter < createdOrgs.Length; counter++)
			{
				Assert(createdOrgs[counter] is OrgHeader);
			}

			OrgHeader org1 = (OrgHeader)createdOrgs[1];
			AssertEquals("Z321-Test Name", org1.OH_FullName);
			AssertEquals("Address1", org1.MainAddress.OA_Address1);
			AssertEquals("Address2", org1.MainAddress.OA_Address2);
			AssertEquals("City", org1.MainAddress.OA_City);
			AssertEquals("QLD", org1.MainAddress.OA_State);
			AssertEquals("POST CODE", org1.MainAddress.OA_PostCode);
			AssertEquals("AUBNE", org1.OH_RL_NKClosestPort);
			AssertEquals("PHONE", org1.MainAddress.OA_Phone);
			AssertEquals("FAX", org1.MainAddress.OA_Fax);
			AssertEquals("Email", org1.MainAddress.OA_Email);
			AssertEquals("Web", org1.MainWebURL.PU_URL);
			Assert("Organisation should be active", org1.OH_IsActive);
			Assert("Organisation should be Debtor", org1.OH_IsDebtor);
			Assert("Organisation should be Consignee", org1.OH_IsConsignee);
			Assert("Organisation should be Consignor", org1.OH_IsConsignor);
			Assert("Organisation should be Shipping Provider", org1.OH_IsShippingProvider);
			Assert("Organisation should not be Shipping Line", !org1.OH_IsShippingLine);
			Assert("Organisation should be Airline", org1.OH_IsAirLine);
			Assert("Organisation should be Local Transport", org1.OH_IsLocalTransport);
			Assert("Organisation should be Forwarder", org1.OH_IsForwarder);
			Assert("Organisation should be Broker", org1.OH_IsBroker);
			Assert("Organisation should be Services", org1.OH_IsMiscFreightServices);
			Assert("Organisation should be Competitor", org1.OH_IsCompetitor);
			Assert("Organisation should be Sales Lead", org1.OH_IsSalesLead);
			ZString defaultStringCurrency = "HKD";
			AssertEquals("Default A/R Currency should be set", defaultStringCurrency, org1.CompanyData.OB_RX_NKARDDefltCurrency);
			AssertEquals("Default A/P Currency should be set", defaultStringCurrency, org1.CompanyData.OB_RX_NKAPDefltCurrency);
			AssertEquals("Default Forwarding Currency should be set", defaultStringCurrency, org1.MiscServ.OM_RX_NKFWDefCurrency);
			AssertEquals("Should have been 3 address records created for this organisation", 3, org1.Addresses.Count);
			AssertEquals("Should be 1 Element in the AccountDetailsCollection", 1, org1.CompanyData.AccountDetailsCollection.Count);
			AccAPAccountDetails accountDetails = org1.CompanyData.AccountDetailsCollection[0];
			AssertEquals("Bank Name", accountDetails.A1_BankName);
			AssertEquals("Account Name", accountDetails.A1_AccountName);
			AssertEquals("Account Number", accountDetails.A1_BankAccount);
			AssertEquals("BSB Number", accountDetails.A1_BankBsb);

			Assert("A/P GST should be applicable", org1.CompanyData.IsAPTaxApplicable);
			Assert("A/R GST should be applicable", org1.CompanyData.IsARTaxApplicable);
			AssertEquals("Credit Limit", 1500M, org1.MiscServ.OM_ARCreditLimit);
			AssertEquals("Credit Rating", "CR3", org1.MiscServ.OM_ARCreditRating);
			AssertEquals("ARTerms.Count", 1, org1.CompanyData.ARTerms.Count);
			AssertEquals("Default Inv Type", "ALL", org1.CompanyData.ARTerms[0].PY_InvoiceClass);
			AssertEquals("Default Inv Terms", "COD", org1.CompanyData.ARTerms[0].PY_InvoiceTerm);
			AssertEquals("Default Inv Term Days", (short)0, org1.CompanyData.ARTerms[0].PY_InvoiceDays);
			AssertEquals(1, org1.Contacts.Count);
			OrgContact orgContact1 = org1.Contacts[0];
			AssertEquals(org1.PK, orgContact1.OC_OH);
			AssertEquals("Contact Email", orgContact1.OC_Email);
			AssertEquals("EML", orgContact1.OC_NotifyMode);
			AssertEquals("Contact Name", orgContact1.OC_ContactName);
			AssertEquals("Contact Job Title", orgContact1.OC_Title);
			AssertEquals("CONTACT WORK PHONE", orgContact1.OC_Phone);
			AssertEquals("CONTACT MOBILE", orgContact1.OC_Mobile);
			AssertEquals("CONTACT FAX", orgContact1.OC_Fax);
			AssertEquals("Contact Source", "050908_EXI_PRS", orgContact1.OC_ContactSource);
			AssertEquals("Date Details Verified", new ZDateTime(2005, 12, 25), orgContact1.OC_DetailsVerified);
			AssertEquals("Contact Salutation", "Senor", orgContact1.OC_Salutation);
			Assert("Organisation should be Warehouse", org1.OH_IsWarehouseClient);
			AssertEquals("Config details should have been created", 9, org1.CustomsCodes.Count);
			Assert("Notes should have been added", org1.Notes.HasNotes);
			OrgCreditorGroup creditorGroup = Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, "CG1");
			AssertNotNull(creditorGroup);
			AssertEquals("Crs Account Group", creditorGroup.PK.ToGuid(), org1.MiscServ.OM_OG_APCreditorGroup);

			foreach (StmNote note in org1.Notes.GetAllNotes())
			{
				Assert("Invalid note description: " + note.ST_Description,
							 PredefinedNoteTypes.Instance.NoteTypeByDescription(note.ST_Description) != null);
			}

			AssertEquals("Invalid Internal Work Notes", "Bad Work Notes",
									 GetNoteByDescription("Internal Work Notes", org1.Notes.GetAllNotes()));
			AssertEquals("Invalid Goods Handling Instructions", "Nice Goods Handling Instructions",
									 GetNoteByDescription("Goods Handling Instructions", org1.Notes.GetAllNotes()));
			AssertEquals("Invalid Import Delivery Instructions", "Great Import Delivery Instructions",
									 GetNoteByDescription("Import Delivery Instructions", org1.Notes.GetAllNotes()));
			AssertEquals("Invalid A/R Account Management Notes", "Fantastic A/R Account Management Notes",
									 GetNoteByDescription("A/R Account Management Notes", org1.Notes.GetAllNotes()));
			AssertEquals("Invalid A/R Credit Management Note", "Interesting A/R Credit Management Note",
									 GetNoteByDescription("A/R Credit Management Note", org1.Notes.GetAllNotes()));
			AssertEquals("Invalid A/P Account Management Notes", "Useless A/P Account Management Notes",
									 GetNoteByDescription("A/P Account Management Notes", org1.Notes.GetAllNotes()));

			OrgHeader org2 = (OrgHeader)createdOrgs[0];
			AssertEquals("Z321-Test Co Name", org2.OH_FullName);
			AssertEquals("Test Address1", org2.MainAddress.OA_Address1);
			AssertEquals("Test Address2", org2.MainAddress.OA_Address2);
			AssertEquals("NSW", org2.MainAddress.OA_State);
			AssertEquals("AUSYD", org2.OH_RL_NKClosestPort);
			AssertEquals("2000", org2.MainAddress.OA_PostCode);
			AssertEquals("99994444", org2.MainAddress.OA_Phone);
			AssertEquals("99995555", org2.MainAddress.OA_Fax);
			AssertEquals("test@email.au", org2.MainAddress.OA_Email);
			AssertEquals("www.test.com.au", org2.MainWebURL.PU_URL);
			AssertEquals(true, org2.OH_IsActive);
			Assert("Organisation should be Debtor", org2.OH_IsDebtor);
			AssertEquals("ARTerms.Count", 2, org2.CompanyData.ARTerms.Count);
			AssertEquals("Standard Inv Type", "ALL", org2.CompanyData.ARTerms[0].PY_InvoiceClass);
			AssertEquals("Standard Inv Terms", "PER", org2.CompanyData.ARTerms[0].PY_InvoiceTerm);
			AssertEquals("Standard Inv Term Days", (short)14, org2.CompanyData.ARTerms[0].PY_InvoiceDays);
			AssertEquals("Disbursment Inv Type", "DSB", org2.CompanyData.ARTerms[1].PY_InvoiceClass);
			AssertEquals("Disbursment Inv Terms", "INV", org2.CompanyData.ARTerms[1].PY_InvoiceTerm);
			AssertEquals("Disbursment Inv Term Days", (short)7, org2.CompanyData.ARTerms[1].PY_InvoiceDays);
			AssertEquals("Organisation should NOT be Warehouse", false, org2.OH_IsWarehouseClient);
			AssertEquals(0, org2.Contacts.Count);
			AssertEquals("9 related parties", 9, org1.AllRelatedParties.Count);
			AssertEquals(org2.PK, org1.DeliveryFreightBillTo.PK);
			AssertEquals(org2.PK, org1.DeliveryCustomsBillTo.PK);
			AssertEquals(org2.PK, org1.PickupFreightBillTo.PK);
			AssertEquals(org2.PK, org1.PickupCustomsBillTo.PK);
			AssertEquals(org2.PK, org1.DeliverySeaCustomsBroker.PK);
			AssertEquals(org2.PK, org1.DeliveryAirCustomsBroker.PK);
			AssertEquals(org2.PK, org1.PickupSeaCustomsBroker.PK);
			AssertEquals(org2.PK, org1.PickupAirCustomsBroker.PK);
			AssertEquals("linked settlement group organisation", org2.PK, org1.ARSettlementGroupPK);

			OrgHeader org3 = (OrgHeader)createdOrgs[2];
			AssertEquals(1, org3.Contacts.Count);
			OrgContact contactWithNoEmail = org3.Contacts[0];
			AssertEquals(org3.PK, contactWithNoEmail.OC_OH);
			AssertEquals("", contactWithNoEmail.OC_Email);
			AssertEquals("PRN", contactWithNoEmail.OC_NotifyMode);
			AssertEquals("John Smith", contactWithNoEmail.OC_ContactName);
			AssertEquals("Manager", contactWithNoEmail.OC_Title);
			AssertEquals("98250011", contactWithNoEmail.OC_Phone);
			AssertEquals("01481123456", contactWithNoEmail.OC_Mobile);
			AssertEquals("98250013", contactWithNoEmail.OC_Fax);
			AssertEquals("Organisation should NOT be Warehouse", false, org2.OH_IsWarehouseClient);
		}

		[RequiresSTA]
		public void TestMergeNullOrganisation()
		{
			var orgHeaderFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};

			var orgHeaderCollection = new OrgHeaderCollection(orgHeaderFactory);
			var orgHeader = orgHeaderFactory.NewWithValidTestData<OrgHeader>();
			orgHeaderCollection.Add(orgHeader);
			orgHeaderFactory.Save();

			var factory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};

			factory.Load<OrgHeader>(orgHeader.PK).Delete();
			factory.Save();

			using (var module = new TestOrganisationModule())
			{
				module.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");
				module.Grid_Exposed.Select(0);

				module.MergeIntoOrganisation_Exposed();
			}

			AssertEquals("Message should have been displayed to user", "The organization has been deleted, please reload the grid. If the error persists, please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOSMGMenu()
		{
			using (var moduleForTest = new TestOrganisationModule())
			{
				var item = GetMenuItem(moduleForTest, "&Set Organization Security Access Group");
				Assert(item.Enabled);
				Assert(item.Visible);
				AssertEquals("&Set Organization Security Access Group", item.Text);
			}
		}

		public void TestSetOrgSecurity()
		{
			using (var moduleForTest = new TestOrganisationModule())
			{
				var item = GetMenuItem(moduleForTest, "&Apply Web Security Profile");
				Assert(item.Enabled);
				Assert(item.Visible);
				AssertEquals("&Apply Web Security Profile", item.Text);
			}
		}

		[RequiresSTA]
		public void TestImportFromLegacyCSVSecurityRight()
		{
			using (var moduleForTest = new TestOrganisationModule())
			{
				using (var form = new ZChildForm(moduleForTest.GridCollection))
				{
					var control = (OrganisationFilterControl)moduleForTest.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();

					Env.Security.OrganisationImportFromLegacyCSV.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					var importFromLegacyCSVMenuItem = moduleForTest.DataTransferMenuItem.MenuItems.FindByText("Import From Legacy CSV", false);
					importFromLegacyCSVMenuItem.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Import from Legacy CSV", UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.OrganisationImportFromLegacyCSV.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					importFromLegacyCSVMenuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					ZApplication.GetOpenForms().OfType<ImportOrganisationsFromCSVForm>().ToArray().ForEach(f => f.Dispose());
				}
			}
		}

		[RequiresSTA]
		public void TestDataTransferMenuItemsContainImportFromLegacyCSV()
		{
			using (var moduleForTest = new TestOrganisationModule())
			{
				using (var form = new ZChildForm(moduleForTest.GridCollection))
				{
					var control = (OrganisationFilterControl)moduleForTest.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();

					var flag = false;
					var dataTransferMenuItemUnderRightClick = moduleForTest.DataTransferMenuItem;
					foreach (MenuItem row in dataTransferMenuItemUnderRightClick.MenuItems)
					{
						if (row.Text.Contains("Legacy CSV"))
						{
							flag = true;
							break;
						}
					}
					Assert("Import from Legacy CSV option should be in right click menu.", flag);

					flag = false;
					var dataTransferMenuItemUnderActionButton = GetMenuItem(moduleForTest, "D&ata Transfer");
					foreach (MenuItem row in dataTransferMenuItemUnderActionButton.MenuItems)
					{
						if (row.Text.Contains("Legacy CSV"))
						{
							flag = true;
							break;
						}
					}
					Assert("Import from Legacy CSV option should under tool bar Action button.", flag);
				}
			}
		}

		#region Implementation

		MenuItem GetMenuItem(TestOrganisationModule module, string menuName)
		{
			MenuItem result = null;

			foreach (MenuItem button in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
			{
				if (button.Text == menuName)
				{
					result = button;
					break;
				}
			}

			AssertNotNull("'" + menuName + "' should exist.", result);
			return result;
		}

		bool CheckMenuItemIsAdded(TestOrganisationModule module, string menuName)
		{
			bool result = false;

			foreach (MenuItem button in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
			{
				if (button.Text == menuName)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			// The test db will be full of OrgHeaders, so an additional identifier is needed to filter by in order to avoid test problems.
			var org = (OrgHeader)factory.NewWithValidTestData(businessObjectType);
			org.OH_FullName = filterStripHelperTestOrgName;

			return org;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (ModuleTextFilter)filterBusinessObject["Name"];
			filter.IsActive = true;
			filter.Property = filterStripHelperTestOrgName;
		}

		const string filterStripHelperTestOrgName = "MODULE BASHER TEST";

		protected override void SetUp()
		{
			base.SetUp();

			Env.Registry.SetOrgAllowMixedCase(true);
		}

		#endregion
	}
}
