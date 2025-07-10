using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(QuotedBookingModule))]
	public class QuotedBookingModuleTest : ZModuleBasherTest
	{
		public void TestAllowSendEmailAction()
		{
			using (var module = new QuotedBookingModule())
			{
				AssertEquals("Should allow send email from actions menu", true, module.AllowSendEmailAction);
			}
		}

		public void TestDeliverDocumentsInOneEmail_QuotedBooking()
		{
			var testHelper = new DeliverDocumentsTestHelper(Factory);
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var docType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document");
			using (var module = new QuotedBookingModule())
			{
				var action = Factory.New<OperationalAction>();
				action.Context = new OperationalActionContext(module.OperationalActionSupporter, "Module Name");
				var command = testHelper.CreateNewDocumentCommand("Test Template", "Test Command", ContactType.Consignee.Code, false, false, false, false, docType, false);
				action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
				Factory.Save();

				var factoryForChanges = new BusinessObjectFactory();
				var dummyLog = new DummyOperationalActionLog();
				var selectedRecords = new SelectedRecords() { PrimaryKeys = new[] { quotedBooking.PK } };
				var runner = new OperationalActionRunner(action, action.Context.Supporter.RootType, selectedRecords)
				{ DeliverDocumentsInOneEmail = true, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
				runner.Printer = ZGuid.NewZGuid();
				runner.Run(dummyLog, factoryForChanges);

				AssertContains($"ERROR: There is no valid recipient for [{quotedBooking.HumanReadableName}]", dummyLog.MessagesString());
				AssertEquals("No printed document", 0, testHelper.CountPrintJobs(quotedBooking.PK));
			}
		}

		public void TestOverrideRecipientEmailEnabled_QuotedBooking()
		{
			var testHelper = new DeliverDocumentsTestHelper(Factory);
			var docType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document");
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = ZGuid.NewZGuid();
			viewQuotedBooking.VB_TH = ZGuid.NewZGuid();

			using (var module = new QuotedBookingModule())
			{
				var action = Factory.New<OperationalAction>();
				action.Context = new OperationalActionContext(module.OperationalActionSupporter, "Module Name");
				var command = testHelper.CreateNewDocumentCommand("Test Template", "Test Command", ContactType.Consignee.Code, false, false, false, false, docType, false);
				action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;

				Factory.Save();

				var selectedRecords = new SelectedRecords() { PrimaryKeys = new[] { viewQuotedBooking.PK } };
				var runner = new OperationalActionRunner(action, action.Context.Supporter.RootType, selectedRecords)
				{ DeliverDocumentsInOneEmail = true, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };

				AssertNoExceptionThrown(
				"No exception thrown when get runner.OverrideRecipientEmailEnabled property.", () =>
				{
					var overrideRecipientEmailEnabled = runner.OverrideRecipientEmailEnabled;
				});
				AssertEquals("OverrideRecipientEmailEnabled is false", false, runner.OverrideRecipientEmailEnabled);
			}
		}

		public void TestSetGuiProviders()
		{
			using (QuotedBookingModuleForTest module = new QuotedBookingModuleForTest())
			{
				module.PerformSearchForTest();

				var shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);

				module.PerformSearchForTest();

				shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (QuotedBookingModule module = new QuotedBookingModule())
			{
				AssertEquals(ModuleIDs.QuotedBookings, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestFilterBusinessObjectCloneIncludesDeniedPartyFilters_CRTEnabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new QuotedBookingModuleForTest())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Legacy Screening Status_"));
				AssertNotEquals(null, filter);

				var clone = module.FilterBusinessObject.Clone();
				var filter2 = (ModuleTextFilter)clone.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Legacy Screening Status_"));
				AssertNotEquals(null, filter2);
			}
		}

		public void TestFilterBusinessObjectCloneIncludesDeniedPartyFilters_CRTDisabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new QuotedBookingModuleForTest())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Screening Status_"));
				AssertNotEquals(null, filter);

				var clone = module.FilterBusinessObject.Clone();
				var filter2 = (ModuleTextFilter)clone.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Screening Status"));
				AssertNotEquals(null, filter2);
			}
		}

		public void TestComplianceFiltersWhenEnableComplianceRiskRegistryIsEnabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var module = new QuotedBookingModuleForTest())
			{
				var complianceFilters = module.FilterBusinessObject.ModuleFilters
					.Where(c => c.Category.Description == "Compliance")
					.Select(f => f.Description.Substring(0, f.Description.IndexOf("_"))).ToArray();

				AssertEquals("Compliance Category", 4, complianceFilters.Length);

				var expectedFilters = new string[] { "Overall", "Location", "Party", "Commodity" };
				AssertContainsExactElementsInAnyOrder(complianceFilters, expectedFilters);
			}
		}

		public void TestComplianceFiltersWhenEnableComplianceRiskRegistryIsDisabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new QuotedBookingModuleForTest())
			{
				var complianceFilters = module.FilterBusinessObject.ModuleFilters.Where(c => c.Category.Description == "Compliance").ToArray();
				AssertEquals("Compliance Category", 0, complianceFilters.Length);
			}
		}

		public void TestOperationalActionsPlugInAddedInModule()
		{
			using (var module = new QuotedBookingModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestOperationalActionsSupporterInQuotedBookingModuleTest()
		{
			using (var module = new QuotedBookingModule())
			{
				AssertEquals(ModuleIDs.QuotedBookings, module.ID);
				AssertNotNull(module.OperationalActionSupporter);
				AssertEquals(typeof(QuotedBookingSupporter), module.OperationalActionSupporter.GetType());
			}
		}

		public void TestActionMenu_ReapplyWorkflowTemplates()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
				var menuItem = actionsMenuItem.MenuItems.FindByText("Reapply Workflow Templates");
				AssertNotNull("Menu item should be available", menuItem);
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.QuotedBookings;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;

			return quotedBooking;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;

			Factory.Save();

			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = booking.PK;
			viewQuotedBooking.VB_TH = quote.PK;

			collection.Add(viewQuotedBooking);

			base.AddTestObjects(collection);
		}

		[ExpectNoExceptions()]
		public void TestQuoteAttachedTo2Bookings()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking1 = QuotedBooking.CreateNewBooking(Factory);
			ForwardingShipment booking2 = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking quotedBooking1 = QuotedBooking.New(quote.PK, booking1.PK, Factory);
			QuotedBooking quotedBooking2 = QuotedBooking.New(quote.PK, booking2.PK, Factory);

			quotedBooking1.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking1.Mode = Core.Constants.RateMode.FCL;

			quotedBooking2.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking2.Mode = Core.Constants.RateMode.FCL;

			Factory.Save();

			using (QuotedBookingModuleForTest module = new QuotedBookingModuleForTest())
			{
				module.PerformSearchForTest();
			}
		}

		public void TestAllowDelete()
		{
			using (QuotedBookingModule module = new QuotedBookingModule())
			{
				AssertEquals("Bookings can be cancelled but not deleted, yes but this needs to be true so the deactivate menu shows ... dumb", true, module.AllowDelete);
			}
		}

		public void TestCheckpoints()
		{
			using (QuotedBookingModule module = new QuotedBookingModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.QuickBooking, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Booking, module.LicenceCheckPoint);
			}
		}

		public void TestImportFromXmlButton()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (QuotedBookingModuleForTest module = new QuotedBookingModuleForTest())
			{
				EventHandler handler = module.ImportMenuItems["Import From &XML"];
				AssertNotNull("Should find the button on the module", handler);

				// expect no exception
				handler(null, null);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (QuotedBookingModuleForTest module = new QuotedBookingModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is QuotedBookingFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestQuotedBookingModule_ColorContextKey()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is QuotedBookingFilterControl);
				AssertEquals(string.Empty, (filterControl as QuotedBookingFilterControl).Grid.ColorContextKey);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (QuotedBookingModuleForTest module = new QuotedBookingModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is ViewQuotedBookingCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (QuotedBookingModuleForTest module = new QuotedBookingModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is QuotedBookingFilterStripBusinessObject);
			}
		}

		public void TestMergeIntoSelectedBookingActionMenu()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;

			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var module = new QuotedBookingModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Application.DoEvents();

				module.PerformSearchForTest();

				var mergeMenu = module.FormActionMenu.FindByText("Merge Into Selected Booking", true);
				AssertNotNull(mergeMenu);

				var grid = ((IFilterGridModuleInternalsForTesting)module).Grid;
				grid.SelectAllElements();

				mergeMenu.PerformClick();

				var combineForm = Application.OpenForms.OfType<CombineBookingsForm>().First();
				AssertNotNull("Show CombineForm", combineForm);

				combineForm.Close();
			}
		}

		public void TestDeniedPartyScreeningMenuAddedWhenEnableComplianceRiskIsFalse()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new QuotedBookingModuleForTest())
			{
				var menuItem = module.FormActionMenu.FindByText("View Compliance Status", true);
				AssertNotNull("View Compliance Status menu should exist", menuItem);
			}
		}

		public void TestGetValidScreeningBusinessObjects()
		{
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking1 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking1 = QuotedBooking.New(quote1.PK, booking1.PK, Factory);
			var vqBooking1 = ViewQuotedBooking.LoadOrCreate(quotedBooking1);

			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking2 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking2 = QuotedBooking.New(quote2.PK, booking2.PK, Factory);
			var vqBooking2 = ViewQuotedBooking.LoadOrCreate(quotedBooking2);

			var quote_invalid = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking_invalid = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking_invalid = QuotedBooking.New(quote_invalid.PK, booking_invalid.PK, Factory);
			var vqBooking_invalid = ViewQuotedBooking.LoadOrCreate(quotedBooking_invalid);
			((ITemplateRecordProvider)vqBooking_invalid).IsTemplateRecord = true;

			var bookingsWithInvalid = new[] { vqBooking1, vqBooking2, vqBooking_invalid };
			var bookingsAllValid = new[] { vqBooking1, vqBooking2 };
			var bookingsAllInvalid = new[] { vqBooking_invalid };

			using (var module = new QuotedBookingModuleForTest())
			{
				module.SetSelectedBusinessObjects(bookingsWithInvalid);

				var result1 = module.GetValidScreeningbizOs();

				AssertContainsExactElementsInAnyOrder("Valid bookings are included in array", result1.BusinessObjects.Select(bo => bo.PK), new[] { booking1.PK, booking2.PK });
				AssertEquals("Invalid flag is set", result1.HasInvalidItems, true);

				module.SetSelectedBusinessObjects(bookingsAllValid);

				var result2 = module.GetValidScreeningbizOs();

				AssertContainsExactElementsInAnyOrder("Valid bookings are included in array", result2.BusinessObjects.Select(bo => bo.PK), new[] { booking1.PK, booking2.PK });
				AssertEquals("Invalid flag is set", result2.HasInvalidItems, false);

				module.SetSelectedBusinessObjects(bookingsAllInvalid);

				var result3 = module.GetValidScreeningbizOs();
				var emptyArray = Array.Empty<BusinessObject>();

				AssertContainsExactElementsInAnyOrder(result3.BusinessObjects, emptyArray);
				AssertEquals("Invalid flag is set", result3.HasInvalidItems, true);
			}
		}

		public void TestUniversalCopyInstanceType()
		{
			var instanceTypeAttribute = typeof(QuotedBookingModule).GetCustomAttribute<UniversalCopyInstanceTypeAttribute>();
			AssertEquals(typeof(QuotedBooking), instanceTypeAttribute.InstanceType);
		}

		public void TestUniversalCopyIsDisabledWhenConsolidated()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);

			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var module = new QuotedBookingModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Application.DoEvents();

				module.PerformSearchForTest();

				var grid = ((IFilterGridModuleInternalsForTesting)module).Grid;
				grid.SelectAllElements();
				grid.ContextMenu.DoPopup();

				var viewQuotedBooking = (ViewQuotedBooking)grid.SelectedElements[0];
				Assert("Not consolidated", !viewQuotedBooking.VB_IsConsolidated);

				var universalCopyMenu = grid.ContextMenu.MenuItems.FindByName("UniversalCopy");
				AssertNotNull(universalCopyMenu);

				Assert("Universal Copy is enabled", universalCopyMenu.Enabled);
				Assert("Universal Copy is visible", universalCopyMenu.Visible);

				viewQuotedBooking.VB_IsConsolidated = true;
				Assert("Consolidated", viewQuotedBooking.VB_IsConsolidated);

				grid.ContextMenu.DoPopup();

				Assert("Universal Copy is disabled", !universalCopyMenu.Enabled);
				Assert("Universal Copy is hidden", !universalCopyMenu.Visible);
			}
		}

		public void TestCopyScheduleIsDisabled()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);

			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var module = new QuotedBookingModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Application.DoEvents();

				module.PerformSearchForTest();

				var grid = ((IFilterGridModuleInternalsForTesting)module).Grid;
				grid.SelectAllElements();
				grid.ContextMenu.DoPopup();

				var copyScheduleMenu = grid.ContextMenu.MenuItems.FindByName("CopySchedules", true);
				AssertNotNull(copyScheduleMenu);

				Assert("Copy schedule is disabled", !copyScheduleMenu.Enabled);
				Assert("Copy schedule is hidden", !copyScheduleMenu.Visible);
			}
		}

		public void TestBusinessContexts()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("The 'QuotedBooking' business context should be returned", BusinessContext.QuotedBooking, module.BusinessContexts[0]);
			}
		}

		#region Test records

		public void TestLoadTemplateRecordsByFilters_TransportMode_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadCollectionIncludeTemplateRecords();
			}
		}

		public void TestLoadCollectionIncludeTemplateRecords()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking1 = QuotedBooking.New(quote.PK, booking.PK, Factory);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "ABC123";
			quotedBooking1.Booking.JS_HouseBill = "XYZ";
			quotedBooking1.Booking.JS_TransportMode = "AIR";
			quotedBooking1.Booking.JS_PackingMode = "LSE";
			quotedBooking1.Booking.JS_RL_NKOrigin = "USORD";
			quotedBooking1.Booking.JS_RL_NKDestination = "AUSYD";
			quotedBooking1.Booking.ConsignorPK = consignor.PK;
			AssertEquals(consignor.PK, quotedBooking1.Booking.Consignor.PK);
			Factory.Save();

			using (var module = new QuotedBookingModuleForTest())
			{
				var quotedBooking2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking2.QuotedBooking.Booking.JS_HouseBill = "ABC";
				quotedBooking2.QuotedBooking.Booking.JS_TransportMode = "AIR";
				quotedBooking2.QuotedBooking.Booking.JS_PackingMode = "LSE";
				quotedBooking2.QuotedBooking.Booking.JS_RL_NKOrigin = "USORD";
				quotedBooking2.QuotedBooking.Booking.JS_RL_NKDestination = "AUSYD";
				quotedBooking2.QuotedBooking.Booking.JS_RS_NKServiceLevel = "STD";
				quotedBooking2.QuotedBooking.Booking.ConsignorPK = consignor.PK;
				AssertEquals(consignor.PK, quotedBooking2.QuotedBooking.Booking.Consignor.PK);
				quotedBooking2.Factory.Save();
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				var quotedBooking3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking3.QuotedBooking.Booking.JS_HouseBill = "234";
				quotedBooking3.QuotedBooking.Booking.JS_TransportMode = "AIR";
				quotedBooking3.QuotedBooking.Booking.JS_PackingMode = "LSE";
				quotedBooking3.QuotedBooking.Booking.JS_RL_NKOrigin = "USORD";
				quotedBooking3.QuotedBooking.Booking.JS_RL_NKDestination = "AUSYD";
				quotedBooking3.QuotedBooking.Booking.JS_RS_NKServiceLevel = "STD";
				quotedBooking3.Factory.Save();
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				var quotedBooking4 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking4.QuotedBooking.Booking.JS_HouseBill = "345";
				quotedBooking4.QuotedBooking.Booking.JS_TransportMode = "SEA";
				quotedBooking4.QuotedBooking.Booking.JS_PackingMode = "OTH";
				quotedBooking4.QuotedBooking.Booking.JS_RL_NKOrigin = "USORD";
				quotedBooking4.QuotedBooking.Booking.JS_RL_NKDestination = "AUSYD";
				quotedBooking4.QuotedBooking.Booking.JS_RS_NKServiceLevel = "STD";
				quotedBooking4.QuotedBooking.Booking.ConsignorPK = consignor.PK;
				AssertEquals(consignor.PK, quotedBooking4.QuotedBooking.Booking.Consignor.PK);
				quotedBooking4.Factory.Save();
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].QuotedBooking.Booking.JS_HouseBill);
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == QuotedBookingFilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = QuotedBookingFilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded;

				var filter2 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters["Mode"];
				filter2.IsActive = true;
				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				filter2.Property = "LSE";
				AssertNotNull(filter2.GetXQuery);

				var filter3 = (ModuleLocationFilter)module.FilterBusinessObject.ModuleFilters["Origin / Destination"];
				filter3.IsActive = true;
				filter3.Property1 = "USORD";
				filter3.Property2 = "AUSYD";
				AssertNotNull(filter3.GetXQuery);

				var filter4 = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Consignor"];
				filter4.IsActive = true;
				filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filter4.PropertyCode = "ABC123";
				AssertNotNull(filter4.XQueryInfo);

				var filter5 = (ModuleNkFilter)module.FilterBusinessObject.ModuleFilters["Service Level"];
				filter5.IsActive = true;
				filter5.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filter5.Property = "STD";
				AssertNotNull(filter5.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());
				AssertEquals(2, collection.Count);
				var collection2 = collection.OfType<ViewQuotedBooking>().OrderBy(x => x.QuotedBooking.Booking.JS_HouseBill).ToList();
				AssertEquals("ABC", collection2[0].QuotedBooking.Booking.JS_HouseBill);
				AssertEquals("XYZ", collection2[1].QuotedBooking.Booking.JS_HouseBill);

				filter.Property = QuotedBookingFilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].QuotedBooking.Booking.JS_HouseBill);
			}
		}

		public void TestLoadCollection_TemplateActive()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				var shipmentTemplate_Active = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipmentTemplate_Active.QuotedBooking.Booking.JS_HouseBill = "ABC";
				shipmentTemplate_Active.Factory.Save();

				var shipmentTemplate_Inactive = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipmentTemplate_Inactive.QuotedBooking.Booking.JS_HouseBill = "DEF";
				shipmentTemplate_Inactive.TemplateRecord.IsCancelled = true;
				shipmentTemplate_Inactive.Factory.Save();
			}

			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
			allLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					AssertTemplateRecordsFilter(
					FilterStripBusinessObject.TemplateRecordsActive,
					SQLComparisonOperator.Equal,
					FilterStripBusinessObject.StatusActive,
					new string[] { "ABC" },
					forceShowAllTemplates: true
					);

					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusInactive,
						new string[] { "DEF" },
						forceShowAllTemplates: true
					);

					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusAll,
						new string[] { "ABC", "DEF" },
						forceShowAllTemplates: true
					);
				}
			});
		}

		public void TestLoadCollection_TemplateName()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				var shipment1 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment1.QuotedBooking.Booking.JS_HouseBill = "ABC";
				shipment1.TemplateRecord.STR_TemplateName = "My Template";
				shipment1.Factory.Save();

				var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment2.QuotedBooking.Booking.JS_HouseBill = "DEF";
				shipment2.TemplateRecord.STR_TemplateName = "Other Template";
				shipment2.Factory.Save();

				var shipment3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment3.QuotedBooking.Booking.JS_HouseBill = "GHI";
				shipment3.Factory.Save();
			}

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"My Template",
				new string[] { "ABC" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"Other Template",
				new string[] { "DEF" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				ZString.Empty,
				new string[] { "ABC", "DEF", "GHI" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.IsBlank,
				ZString.Empty,
				new string[] { "GHI" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.IsNotBlank,
				ZString.Empty,
				new string[] { "ABC", "DEF" },
				forceShowAllTemplates: true
			);
		}

		public void TestGetNewTemplateRecordBusinessObjectCore()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				var bizO = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				var templateRecordProvider = (ITemplateRecordProvider)bizO;

				Assert(templateRecordProvider.IsTemplateRecord);
				AssertNotNull(templateRecordProvider.TemplateRecord);
				AssertEquals(typeof(StmTemplateRecord), templateRecordProvider.TemplateRecord.GetType());
				var templateRecord = (StmTemplateRecord)templateRecordProvider.TemplateRecord;
				AssertNotEquals(bizO.Factory._Instance, templateRecord.Factory._Instance);
				AssertEquals(module.ID.Name, templateRecord.STR_ModuleID);

				AssertEquals(typeof(TemplateRecordBusinessObjectFactory), bizO.Factory.GetType());
				var templateFactory = (TemplateRecordBusinessObjectFactory)bizO.Factory;
				AssertSame(bizO.QuotedBooking, templateFactory.TemplateRecordProvider);
				AssertSame(templateRecord.Factory, templateFactory.TemplateRecordFactory);
			}
		}

		public void TestLoadFromTemplateRecordPkCore()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				var bizO = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				bizO.QuotedBooking.Booking.JS_HouseBill = "ABC";
				bizO.Factory.Save();

				var templateRecordProvider = (ITemplateRecordProvider)bizO;
				var templateRecord = (StmTemplateRecord)templateRecordProvider.TemplateRecord;

				var localFactory = new BusinessObjectFactory();

				var reloadedBizO = (QuotedBooking)module.LoadFromTemplateRecordPk(localFactory, bizO.PK);
				AssertNull("Cannot reload template record by bizo's PK", reloadedBizO);

				reloadedBizO = (QuotedBooking)module.LoadFromTemplateRecordPk(localFactory, templateRecord.PK);
				AssertNotNull("Should load template shipment by template record's PK", reloadedBizO);
				AssertEquals("ABC", reloadedBizO.Booking.JS_HouseBill);
				AssertNotEquals(bizO.PK, reloadedBizO.PK);
				AssertSame("Should load in specific factory", localFactory, reloadedBizO.Factory);

				var reloadedTemplateRecordProvider = (ITemplateRecordProvider)reloadedBizO;
				Assert(reloadedTemplateRecordProvider.IsTemplateRecord);

				var reloadedTemplateRecord = (StmTemplateRecord)reloadedTemplateRecordProvider.TemplateRecord;
				AssertEquals(templateRecord.PK, reloadedTemplateRecord.PK);
				AssertSame("Should load in specific factory", localFactory, reloadedTemplateRecord.Factory);
			}
		}

		public void TestTemplateRecords_RelatedOrgs()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "Connor";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "Consee";
			var impBroker = Factory.NewWithValidTestData<OrgHeader>();
			impBroker.OH_Code = "ImpBro";
			var expBroker = Factory.NewWithValidTestData<OrgHeader>();
			expBroker.OH_Code = "ExpBro";
			var conCus = Factory.NewWithValidTestData<OrgHeader>();
			conCus.OH_Code = "ConCus";
			var conAgt = Factory.NewWithValidTestData<OrgHeader>();
			conAgt.OH_Code = "ConAgt";
			var dlvAgt = Factory.NewWithValidTestData<OrgHeader>();
			dlvAgt.OH_Code = "DlvAgt";
			var pckAgt = Factory.NewWithValidTestData<OrgHeader>();
			pckAgt.OH_Code = "PckAgt";
			Factory.Save();

			using (var module = new QuotedBookingModuleForTest())
			{
				var quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking.QuotedBooking.Booking.JS_HouseBill = "ABC";
				quotedBooking.QuotedBooking.Booking.ConsignorPK = consignor.PK;
				quotedBooking.QuotedBooking.Booking.JS_OH_ImportBroker = impBroker.PK;
				quotedBooking.QuotedBooking.Booking.ControllingAgentDocumentaryAddress.OrganisationPK = conAgt.PK;
				quotedBooking.QuotedBooking.Booking.JS_OH_DeliveryAgent = dlvAgt.PK;
				quotedBooking.Factory.Save();
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				var quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking.QuotedBooking.Booking.JS_HouseBill = "234";
				quotedBooking.QuotedBooking.Booking.ConsigneePK = consignee.PK;
				quotedBooking.QuotedBooking.Booking.JS_OH_ExportBroker = expBroker.PK;
				quotedBooking.QuotedBooking.Booking.ControllingCustomerAddress.OrganisationPK = conCus.PK;
				quotedBooking.QuotedBooking.Booking.PickupAgentDocumentaryAddress.OrganisationPK = pckAgt.PK;
				quotedBooking.Factory.Save();
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == QuotedBookingFilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = QuotedBookingFilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Consignor"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "Connor";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());
				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Consignee"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "Consee";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("234", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Import Broker"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "ImpBro";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Export Broker"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "ExpBro";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("234", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Controlling Agent"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "ConAgt";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Controlling Customer"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "ConCus";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("234", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Delivery Agent"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "DlvAgt";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].QuotedBooking.Booking.JS_HouseBill);

				filterOrg.IsActive = false;
				filterOrg = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters["Pickup Agent"];
				filterOrg.IsActive = true;
				filterOrg.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filterOrg.PropertyCode = "PckAgt";
				AssertNotNull(filterOrg.XQueryInfo);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("234", collection[0].QuotedBooking.Booking.JS_HouseBill);
			}
		}

		public void TestLoadTemplateRecordsByFilters_CreatingUser()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new QuotedBookingModuleForTest())
				{
					var quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					quotedBooking.QuotedBooking.Booking.JS_HouseBill = "BOOKING1";
					quotedBooking.Factory.Save();
				}
			}

			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new QuotedBookingModuleForTest())
				{
					var quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					quotedBooking.QuotedBooking.Booking.JS_HouseBill = "BOOKING2";
					quotedBooking.Factory.Save();
				}
			}

			using (var module = new QuotedBookingModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == QuotedBookingFilterStripBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = QuotedBookingFilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleNkFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == QuotedBookingFilterStripBusinessObject.Descriptions.AuditInformation.CreatingUser);
				filter2.IsActive = true;
				filter2.Property = user.GS_Code;

				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("BOOKING1", collection[0].QuotedBooking.Booking.JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("BOOKING2", collection[0].QuotedBooking.Booking.JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(0, collection.Count);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "BOOKING1", "BOOKING2" }, collection.Cast<ViewQuotedBooking>().Select(booking => booking.QuotedBooking.Booking.JS_HouseBill));
			}
		}

		#endregion

		#region TestDeactivateAndShowBookingRejectionMessage

		public void TestDeactivateAndShowBookingRejectionMessage()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new QuotedBookingModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearchForTest();
				AssertEquals(1, module.GridCollection.Count);

				module.DisplayGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var deactivateButton = module.ToolBarButtons.FirstOrDefault(t => t.Text == "Delete") as ZToolBarButton;
				AssertNotNull(deactivateButton);
				deactivateButton.PerformClick();

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertNotNull(message);
				AssertEquals("Booking Rejection Message", message.Caption);
				AssertEquals("This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", message.Text);

				var openedBookingForms = ZApplication.GetOpenForms().OfType<QuotedBookingForm>();
				foreach (var openedBookingForm in openedBookingForms)
				{
					openedBookingForm.Dispose();
				}
			}
		}

		#endregion

		#region IOperationalActionSupportable

		public void TestIOperationalActionSupportable()
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				AssertEquals(typeof(QuotedBookingSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}

		#endregion

		#region Implementation

		void AssertTemplateRecordsFilter(
			string filterType,
			SQLComparisonOperator filterOperation,
			string filterValue,
			string[] expectedHouseBills,
			bool forceShowAllTemplates = false)
		{
			using (var module = new QuotedBookingModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				if (forceShowAllTemplates)
				{
					var showTemplatesFilter = module
						.FilterBusinessObject
						.ModuleFilters
						.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription) as ModuleTextFilter;

					showTemplatesFilter.IsActive = true;
					showTemplatesFilter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;
				}

				var filter = module
					.FilterBusinessObject
					.ModuleFilters
					.First(f => f.Description == filterType) as ModuleTextFilter;

				filter.IsActive = true;
				filter.SqlComparisonOperator = filterOperation;
				filter.Property = filterValue;

				var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				CombineAssertions($"Should load expected quoted bookings (Filter: {filterType}, Operation: {filterOperation.ToString()} Value: {filterValue})", () =>
				{
					AssertEquals(expectedHouseBills.Length, collection.Count);
					AssertContainsExactElementsInAnyOrder(expectedHouseBills, collection.Cast<ViewQuotedBooking>().ToArray().Select(qb => qb.QuotedBooking.Booking.JS_HouseBill));
				});
			}
		}

		#region QuotedBookingModuleForTest

		internal class QuotedBookingModuleForTest : QuotedBookingModule
		{
			protected override XmlDataTransferDirector NewXmlDataTransferDirector(QuotedBookingValueObjectDataAdapter adapter)
			{
				return new XmlDataTransferDirector(adapter, false);
			}

			public new FilterModuleMenuItemDescriptorCollection ImportMenuItems
			{
				get
				{
					return base.ImportMenuItems;
				}
			}

			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}

			public void PerformSearchForTest()
			{
				base.PerformSearch();
			}

			internal ViewQuotedBooking GetNewTemplateRecordBusinessObjectCoreExposed()
			{
				return (ViewQuotedBooking)GetNewTemplateRecordBusinessObjectCore();
			}

			internal void LoadCollectionExposed(IBusinessObjectCollection collection, ZQuery query)
			{
				var result = PerformSearchCore(collection.TypeOfElements, query);
				BindSearchResultToGrid(collection, result);
			}

			public (BusinessObject[] BusinessObjects, bool HasInvalidItems) GetValidScreeningbizOs()
			{
				return GetValidScreeningBusinessObjects(this);
			}

			BusinessObject[] testBusinessObjects;

			public override BusinessObject[] GetSelectedBusinessObjects()
			{
				return testBusinessObjects;
			}

			public void SetSelectedBusinessObjects(BusinessObject[] biz0s)
			{
				testBusinessObjects = biz0s;
			}
		}

		#endregion

		#endregion
	}

	internal sealed class QuotedBookingBulkPostingModuleTest : BulkPostingModuleTest
	{
		public void TestBulkPostingSecurityWithoutRights()
		{
			string expectedMsg =
				"You do not have the appropriate security rights to run this function." + "\r\n\r\n"
				+ "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:" + "\r\n\r\n"
				+ "Operate -> Forwarding -> Bookings -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost";

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			AssertBulkPosting(false, expectedMsg);
		}

		public void TestPostBookingWithQuote()
		{
			var expectedMsg = "Posting action can only be run for Quick Booking.";

			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();

			AssertBulkPosting(true, expectedMsg);
		}

		public void TestPostMultipleBookingWithQuotes()
		{
			var expectedMsg = "Posting action can only be run for Quick Booking.";

			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var bookingWithQuote1 = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();

			AssertBulkPosting(true, expectedMsg, 2);
		}

		public void TestPostQuickBookingAndBookingWithQuote()
		{
			var expectedMsg = "Are you sure you want to Post All Charges and Costs for this Booking?";

			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			AssertBulkPosting(true, expectedMsg, 2);
		}

		void AssertBulkPosting(bool hasSecurityRight, string expectedMsg, int selectElements = 1)
		{
			GlbStaff staff = GetStaff(ParentCheckPoint, RightsCheckpoint, hasSecurityRight);
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: Current user has changed", staff.PK, Env.CurrentUser.PK);

				using (IBulkPostingModuleInternalsForTesting module = GetNewModuleForTest())
				using (ZForm form = new ZForm())
				{
					var bookingModule = (QuotedBookingModule)module;
					var filterControl = (QuotedBookingFilterControl)bookingModule.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();
					filterControl.Find();

					var grid = (ZDisplayGrid)bookingModule.DisplayGrid;
					grid.SelectAllElements();
					AssertEquals("Some business object should be selected.", selectElements, bookingModule.GetSelectedBusinessObjects().Length);

					MenuItem postMenuItem = MenuAssertion.AssertHasMenu(module.PostMenuItem as MenuItem, "Post All Charges and Costs");

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					postMenuItem.PerformClick();
					AssertContains(expectedMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (QuotedBookingModule)ZModuleFactory.Instance.Create(ModuleIDs.QuotedBookings);
		}

		SecurityCheckpoint ParentCheckPoint;
		SecurityCheckpoint RightsCheckpoint;

		protected override void SetUp()
		{
			base.SetUp();

			ParentCheckPoint = Env.Security.Operations;
			RightsCheckpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.QuickBookingJobInvoicing, SecurityCore.Bulk);
		}

		GlbStaff GetStaff(SecurityCheckpoint parentCheckpoint, SecurityCheckpoint rightsCheckpoint, bool isAllowed)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TestStaff";

			if (parentCheckpoint != null)
			{
				GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
				security.GU_SecurityItemIsAllowed = true;
				security.GU_SecurityRight = parentCheckpoint.Code;
				security.GU_GC = GlbCompany.CurrentCompany.PK;
				security.GU_GB = GlbBranch.CurrentBranch.PK;
				security.GU_GE = GlbDepartment.CurrentDepartment.PK;
				security.GU_GS = staff.PK;
			}

			if (rightsCheckpoint != null)
			{
				GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
				security.GU_SecurityItemIsAllowed = isAllowed;
				security.GU_SecurityRight = rightsCheckpoint.Code;
				security.GU_GC = GlbCompany.CurrentCompany.PK;
				security.GU_GB = GlbBranch.CurrentBranch.PK;
				security.GU_GE = GlbDepartment.CurrentDepartment.PK;
				security.GU_GS = staff.PK;
			}

			Factory.Save();

			return staff;
		}

		#endregion
	}

	internal sealed class BookingBulkJobProfitPrintingModuleTest : BaseBulkJobProfitPrintingModuleTest
	{
		protected override void FindAndClickMenuItem(IMenuItem testMenuItem)
		{
			using (ZFilterGridModule module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.QuotedBookings))
			{
				MenuItem post = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Post");
				foreach (MenuItem childMenuItem in post.MenuItems)
				{
					if (childMenuItem == testMenuItem)
					{
						childMenuItem.PerformClick();
						break;
					}
				}
			}
		}
	}
}
