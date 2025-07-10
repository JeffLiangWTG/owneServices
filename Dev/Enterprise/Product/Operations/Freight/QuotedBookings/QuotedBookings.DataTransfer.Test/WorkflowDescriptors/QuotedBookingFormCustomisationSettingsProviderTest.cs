using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingFormCustomisationSettingsProvider))]
	public class QuotedBookingFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<QuotedBookingFormCustomisationSettingsProvider>
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new QuotedBookingFormCustomisationSettingsProvider(null));
			AssertNoExceptionThrown(() => new QuotedBookingFormCustomisationSettingsProvider(new QuotedBookingWorkflowDescriptor()));
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				QuotedBooking.Schema.Mode,
				QuotedBooking.Schema.Origin,
				QuotedBooking.Schema.Destination
			};

			QuotedBookingFormCustomisationSettingsProvider provider = GetNewProvider();

			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			QuotedBookingFormCustomisationSettingsProvider provider = GetNewProvider();
			FormCustomisableElementCollection tabs = provider.DisplayTabs;
			string[] expectedTabNames = new[] { "MainTabPage", "AdditionalDetailsTabPage", "CustomFieldsTabPage", "AddressesTabPage", "ComplianceRiskTabPage", "CommercialInvoiceTabPage" };
			AssertEquals("Expected 6 tabs", 6, tabs.Count);
			AssertContainsExactElementsInAnyOrder(expectedTabNames, tabs.Select(tab => tab.ElementName));
		}

		public void TestDisplayTabsWithWorkflowSubType()
		{
			AssertContainsExactElementsInAnyOrder(GetAvailableDisplayTabs(ZString.Empty), new[] { "MainTabPage", "CustomFieldsTabPage" });
			AssertContainsExactElementsInAnyOrder(GetAvailableDisplayTabs("XXX"), new[] { "MainTabPage", "CustomFieldsTabPage" });
			AssertContainsExactElementsInAnyOrder(GetAvailableDisplayTabs(QuotedBooking.QuickBookingCode), new[] { "MainTabPage", "AdditionalDetailsTabPage", "CustomFieldsTabPage", "AddressesTabPage", "ComplianceRiskTabPage", "CommercialInvoiceTabPage" });
			AssertContainsExactElementsInAnyOrder(GetAvailableDisplayTabs(QuotedBooking.BookingWithQuoteCode), new[] { "MainTabPage", "AdditionalDetailsTabPage", "CustomFieldsTabPage", "AddressesTabPage", "ComplianceRiskTabPage", "CommercialInvoiceTabPage" });
			AssertContainsExactElementsInAnyOrder(GetAvailableDisplayTabs(QuotedBooking.SpotQuoteCode), new[] { "MainTabPage", "CustomFieldsTabPage" });
		}

		public void TestDisplayFields()
		{
			QuotedBookingFormCustomisationSettingsProvider provider = GetNewProvider();
			FormCustomisableElement[] displayFields = provider.DisplayFields.Cast<FormCustomisableElement>().ToArray();
			AssertEquals("Display fields count", 58, displayFields.Length);

			FormCustomisableElement[] general1Fields = displayFields.Where(x => x.DisplayTabCode == "MainTabPage" && x.ElementGroup == "General1").ToArray();
			AssertEquals("Details group fields count", 10, general1Fields.Length);
			AssertEquals("Details group fields have distinct row numbers", 10, general1Fields.Select(x => x.RowNumber).Distinct().Count());

			FormCustomisableElement[] additionalContactsFields = displayFields.Where(x => x.DisplayTabCode == "AdditionalDetailsTabPage" && x.ElementGroup == "AdditionalContacts").ToArray();
			AssertEquals("Details group fields count", 9, additionalContactsFields.Length);
			AssertEquals("Details group fields have distinct row numbers", 9, additionalContactsFields.Select(x => x.RowNumber).Distinct().Count());
		}

		public void TestDisplayFieldsWithWorkflowSubType()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (CWNextFeatureTestHelper.EnableCWNext())
			{
				AssertContainsExactElementsInAnyOrder(GetAvailableDisplayFields(ZString.Empty), sharedControlNames);
				AssertContainsExactElementsInAnyOrder(GetAvailableDisplayFields("XXX"), sharedControlNames);
				AssertContainsExactElementsInAnyOrder(GetAvailableDisplayFields(QuotedBooking.QuickBookingCode), bookingOnlyControlNames.Concat(sharedControlNames));
				AssertContainsExactElementsInAnyOrder(GetAvailableDisplayFields(QuotedBooking.SpotQuoteCode), quoteCommonControlNames.Concat(sharedControlNames).Concat(quoteOnlyControlNames));
				AssertContainsExactElementsInAnyOrder(GetAvailableDisplayFields(QuotedBooking.BookingWithQuoteCode), bookingOnlyControlNames.Concat(quoteCommonControlNames.Concat(sharedControlNames)));
			}
		}

		public void TestBookingDisplayFieldsDoNotContainDeliveryDueDate_WhenCalculateDDDRegistryIsDisabled()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertEquals("Quick Booking", false, GetAvailableDisplayFields(QuotedBooking.QuickBookingCode).Contains("DeliveryDueDateDateEdit"));
				AssertEquals("Quick Booking", false, GetAvailableDisplayFields(QuotedBooking.BookingWithQuoteCode).Contains("DeliveryDueDateDateEdit"));
			}
		}

		public void TestBookingDisplayFieldsCO2()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				AssertEquals("One Off Quote", true, GetAvailableDisplayFields(QuotedBooking.SpotQuoteCode).Contains("TotalCO2eTextBox"));
				AssertEquals("Quoted Booking", true, GetAvailableDisplayFields(QuotedBooking.QuickBookingCode).Contains("TotalCO2eTextBox"));
				AssertEquals("Quick Booking", true, GetAvailableDisplayFields(QuotedBooking.BookingWithQuoteCode).Contains("TotalCO2eTextBox"));
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				AssertEquals("One Off Quote", false, GetAvailableDisplayFields(QuotedBooking.SpotQuoteCode).Contains("TotalCO2eTextBox"));
				AssertEquals("Quoted Booking", false, GetAvailableDisplayFields(QuotedBooking.QuickBookingCode).Contains("TotalCO2eTextBox"));
				AssertEquals("Quick Booking", false, GetAvailableDisplayFields(QuotedBooking.BookingWithQuoteCode).Contains("TotalCO2eTextBox"));
			}
		}

		public void TestGoodsDetails1InvisibleWhenFCL()
		{
			var provider = GetNewProvider();
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_SubType2 = QuotedBooking.BookingWithQuoteCode;
			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate = processTaskTemplate;
			processTaskTemplate.P0_SubType1 = Core.Constants.ContainerModes.LCL;
			var displayFields = provider.DisplayFields.Cast<FormCustomisableElement>().ToArray();
			Assert(displayFields.Where(x => x.ElementGroup == "GoodsDetails1").All(x => x.IsAvailable));

			provider = GetNewProvider();
			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate = processTaskTemplate;
			processTaskTemplate.P0_SubType1 = Core.Constants.ContainerModes.FCL;
			displayFields = provider.DisplayFields.Cast<FormCustomisableElement>().ToArray();
			Assert(displayFields.Where(x => x.ElementGroup == "GoodsDetails1").All(x => x.IsAvailable));

			provider = GetNewProvider();
			processTaskTemplate.P0_SubType2 = QuotedBooking.QuickBookingCode;
			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate = processTaskTemplate;
			processTaskTemplate.P0_SubType1 = Core.Constants.ContainerModes.FCL;
			displayFields = provider.DisplayFields.Cast<FormCustomisableElement>().ToArray();
			Assert(displayFields.Where(x => x.ElementGroup == "GoodsDetails1").All(x => x.IsAvailable));

			processTaskTemplate.P0_SubType2 = QuotedBooking.SpotQuoteCode;
			displayFields = provider.DisplayFields.Cast<FormCustomisableElement>().ToArray();
			Assert("The fields for Quote would be available", displayFields.Where(x => x.ElementGroup == "GoodsDetails1" && x.ElementName != "PackagesCalcDropEdit").All(x => !x.IsAvailable));
		}

		public override void TestTabPlacementProhibitions()
		{
			QuotedBookingFormCustomisationSettingsProvider provider = GetNewProvider();

			AssertEquals(1, provider.TabPlacementProhibitions.Length);
			AssertEquals("AdditionalDetailsTabPage", provider.TabPlacementProhibitions[0].TabName);
			AssertContainsExactElementsInAnyOrder(new string[1] { TabPlacement.Placements.TopLeft },
				provider.TabPlacementProhibitions[0].ProhibitedPlacements);
		}

		public void TestAddressesTabIsNotValidFieldTabPlacement()
		{
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "QBK");
			query.AddToFilter(ProcessTaskTemplateSchema.P0_SubType2, "QBN");
			var bookingWorkflowTemplate = Factory.LoadTop1<ProcessTaskTemplate>(query);
			var formCustomisationSettings = bookingWorkflowTemplate.FormCustomisationSettings;
			var element = formCustomisationSettings.DisplayFields.AddNew();
			AssertEquals(3, element.DisplayTabList.Count);
			AssertCollectionNotContains("Addresses is not in tabs list for display field placement", "Addresses", element.DisplayTabList.GetAllCodes());
			element.DisplayTab = "Addresses";
			AssertHasError(element.DisplayTabLocalizedInfo, "Enter a valid selection.");
		}

		public override QuotedBookingFormCustomisationSettingsProvider GetNewProvider()
		{
			return new QuotedBookingFormCustomisationSettingsProvider(new QuotedBookingWorkflowDescriptor());
		}

		#region Implementation

		readonly string[] bookingOnlyControlNames = new[]
		{
			"JS_ShippingReferenceTextEdit", "JS_GoodsDescriptionBoundTextBox", "MarksAndNumbersNotePopupEdit", "PickupReadyDateEdit",
			"DeliveryOpenDateEdit", "PackagesCalcDropEdit", "JP_InsuranceRequiredCheckBox", "QuotedBookingOrderManagementControl",
			"CustomsEntryNumberTypeBoundDropEdit", "JS_A_BKDBoundReadOnlyDateEdit", "JS_ClientRequestedETADateEdit", "JS_A_RCVBoundDateEdit", "JS_InterimReceiptBoundTextEdit",
			"CFSReferenceTextBox", "BookingPartyDocAddressControl", "PickupDocAddressControl", "DeliveryDocAddressControl",
			"ShipperCODAmountCalcEdit", "ShipperCODTypeDropEdit", "ReleaseTypeDropEdit", "ChargesApplyDropEdit", "OnBoardDropEdit",
			"FreightSpotRate", "FreightGatewaySellRate", "FreightCostRate", "ReceivalPointGroupBox", "DeliveryPointGroupBox", "PickupAgentBoundOrganisationControl",
			"DeliveryAgentBoundOrganisationControl", "ExportBrokerCoOrganisationControl", "ImportBrokerCoOrganisationControl", "PortTransportGroupBox",
			"ControllingCustomerAddressControl", "ControllingAgentAddressControl", "ContainerModeOverrideDropEdit", "DeliveryDueDateDateEdit"
		};

		readonly string[] quoteCommonControlNames = new[]
		{
			"ViaCodeFindBox", "EntriesCalcEdit", "EntryInvoiceLinesCalcEdit", "EntryRateTransitTimeDropEdit"
		};

		readonly string[] quoteOnlyControlNames = new[]
		{
			"PotentialCarriersGroupBox"
		};

		readonly string[] sharedControlNames = new[]
		{
			"TransportModeDropEdit", "ContainerModeDropEdit", "PaymentTermDropEdit", "AdditionalTermsTextBox", "ServiceLevelFindBox", "ActualWeightDropEdit", "VolumeCalcDropEdit",
			"ChargeableDropEdit", "PickupEquipmentDropEdit", "DeliveryEquipmentDropEdit", "ValueOfGoodsCalcEdit", "ValueOfInsuranceCalcEdit",
			"CommodityFindBox", "RateLocalCode", "FMCTariffID" , "CreditorGuidFindBox", "TotalCO2eTextBox"
		};

		string[] GetAvailableDisplayTabs(ZString workflowSubType)
		{
			QuotedBookingFormCustomisationSettingsProvider provider = GetNewProvider();
			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate.P0_SubType2 = workflowSubType;

			return provider.DisplayTabs.Cast<FormCustomisableElement>().Where(elem => elem.IsAvailable).Select(elem => (string)elem.ElementName).ToArray();
		}

		string[] GetAvailableDisplayFields(ZString workflowSubType)
		{
			QuotedBookingFormCustomisationSettingsProvider provider = GetNewProvider();
			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			provider.ParentWorkflowDescriptor.LastProcessTaskTemplate.P0_SubType2 = workflowSubType;

			return provider.DisplayFields.Cast<FormCustomisableElement>().Where(elem => elem.IsAvailable).Select(elem => (string)elem.ElementName).ToArray();
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		protected override void SetUp()
		{
			base.SetUp();
			FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() });
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() });
		}

		#endregion
	}
}
