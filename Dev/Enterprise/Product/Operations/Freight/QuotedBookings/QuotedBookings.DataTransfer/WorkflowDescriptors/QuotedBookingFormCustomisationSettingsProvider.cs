using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.Freight.QuotedBookings.DataTransfer.ResString;
using static Enterprise.GlobalCommercialInvoice.Integration.Constants;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		public QuotedBookingFormCustomisationSettingsProvider(QuotedBookingWorkflowDescriptor parentWorkflowDescriptor)
		{
			Argument.NotNull(parentWorkflowDescriptor, "parentWorkflowDescriptor");

			ParentWorkflowDescriptor = parentWorkflowDescriptor;
		}

		public QuotedBookingWorkflowDescriptor ParentWorkflowDescriptor { get; private set; }

		class TabNames
		{
			public const string DetailsTabPage = "MainTabPage";
			public const string AdditionalDetailsTabPage = "AdditionalDetailsTabPage";
			public const string CustomFieldsTabPage = "CustomFieldsTabPage";
		}

		class GroupNames
		{
			public const string General1 = "General1";
			public const string General2 = "General2";
			public const string General3 = "General3";
			public const string GoodsDetails1 = "GoodsDetails1";
			public const string GoodsDetails2 = "GoodsDetails2";
			public const string MonetaryValues = "MonetaryValues";
			public const string AdditionalDetails1 = "AdditionalDetails1";
			public const string FreightRate = "FreightRate";
			public const string AdditionalContacts = "AdditionalContacts";
		}

		ZString GetCurrentTemplateType()
		{
			ZString result = string.Empty;
			ProcessTaskTemplate template = ParentWorkflowDescriptor.LastProcessTaskTemplate;

			if (template != null)
			{
				switch (template.P0_SubType2)
				{
					case QuotedBooking.BookingWithQuoteCode:
						result = QuotedBooking.BookingWithQuoteCode;
						break;
					case QuotedBooking.QuickBookingCode:
						result = QuotedBooking.QuickBookingCode;
						break;
					case QuotedBooking.SpotQuoteCode:
						result = QuotedBooking.SpotQuoteCode;
						break;
					default:
						result = string.Empty;
						break;
				}
			}

			return result;
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();

			tabs.SuspendValidation();

			tabs.Add(ResString.GetMultilingualString("f39b83bd-7a28-4a36-88b3-f4d8a9478bdd", "Details"), TabNames.DetailsTabPage, true);

			ZString currentTemplateType = GetCurrentTemplateType();

			FormCustomisableElement additionalDetailsTab = tabs.Add(ResString.GetMultilingualString("632bd1e3-9524-478b-97b5-e3c322b184d6", "Additional Details"), TabNames.AdditionalDetailsTabPage, true);
			additionalDetailsTab.IsAvailableFunction = () => currentTemplateType == QuotedBooking.BookingWithQuoteCode || currentTemplateType == QuotedBooking.QuickBookingCode;

			tabs.Add(ResString.GetMultilingualString("bd262f36-77ff-4d5f-a13b-acd729fd3f60", "Custom Fields"), TabNames.CustomFieldsTabPage, true);

			var addressesTab = tabs.Add(ResString.GetMultilingualString("c11bf8f3-bd5b-dd85-4086-bcd5a3b5120b", "Addresses"), "AddressesTabPage");
			addressesTab.IsAvailableFunction = () => currentTemplateType == QuotedBooking.BookingWithQuoteCode || currentTemplateType == QuotedBooking.QuickBookingCode;

			var complianceRiskTab = tabs.Add(ResString.GetMultilingualString("EAB37F3A-5BCE-42E0-BB4A-92338E967485", "Compliance Risk"), ComplianceRisk.Integration.ComplianceWiseConstants.ComplianceRiskTabPageName);
			complianceRiskTab.IsAvailableFunction = () => currentTemplateType == QuotedBooking.BookingWithQuoteCode || currentTemplateType == QuotedBooking.QuickBookingCode;

			var commercialInvoiceTab = tabs.Add(ResString.GetMultilingualString("0318EA88-1709-48C0-8B60-4C3B63D5F126", PluginName), PluginTabPageName);
			commercialInvoiceTab.IsAvailableFunction = () => currentTemplateType == QuotedBooking.BookingWithQuoteCode || currentTemplateType == QuotedBooking.QuickBookingCode;

			tabs.ResumeValidation();

			return tabs;
		}

		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			ZString currentTemplateType = GetCurrentTemplateType();

			FormCustomisableElementCollection fields = new FormCustomisableElementCollection();

			fields.SuspendValidation();

			fields.Add(ResString.GetMultilingualString("0eb09c5e-93b8-00b1-4f70-ad668519b161", "Payment/Incoterm"), "PaymentTermDropEdit", false, (NoResString)GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 2);
			fields.Add(ResString.GetMultilingualString("f2401fb4-b183-4451-afc5-b65cb9fca2f1", "Additional Terms"), "AdditionalTermsTextBox", false, (NoResString)GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 3);
			fields.Add(ResString.GetMultilingualString("71d76d7e-8253-4ff6-9547-0a95032055c2", "Service Level"), "ServiceLevelFindBox", false, (NoResString)GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 4);
			fields.Add(ResString.GetMultilingualString("18e95949-4c80-44bd-a34e-925e92c67aea", "Goods Value"), "ValueOfGoodsCalcEdit", false, (NoResString)GroupNames.MonetaryValues, TabNames.DetailsTabPage, TabPlacement.Placements.BottomRight, 0);
			fields.Add(ResString.GetMultilingualString("28d1c432-d9ad-46fd-89e9-c672081159af", "Insurance Value"), "ValueOfInsuranceCalcEdit", false, (NoResString)GroupNames.MonetaryValues, TabNames.DetailsTabPage, TabPlacement.Placements.BottomRight, 1);

			fields.Add(ResString.GetMultilingualString("9ba578cb-3c16-4c7f-9249-1f544a14a980", "Pickup Drop Mode"), "PickupEquipmentDropEdit", false, (NoResString)GroupNames.GoodsDetails2, TabNames.DetailsTabPage, TabPlacement.Placements.BottomMiddle, 0);
			fields.Add(ResString.GetMultilingualString("e82ae2ed-761d-438b-ab15-b8004de57b84", "Delivery Drop Mode"), "DeliveryEquipmentDropEdit", false, (NoResString)GroupNames.GoodsDetails2, TabNames.DetailsTabPage, TabPlacement.Placements.BottomMiddle, 1);
			fields.Add(ResString.GetMultilingualString("78f445c8-8516-407e-9eae-dbf26fa96eb7", "Commodity"), "CommodityFindBox", false, (NoResString)GroupNames.GoodsDetails2, TabNames.DetailsTabPage, TabPlacement.Placements.BottomMiddle, 2);
			fields.Add(ResString.GetMultilingualString("6d8e0d84-777d-46e2-b3c4-9c78563c3a06", "Rating Local Code"), "RateLocalCode", false, (NoResString)GroupNames.GoodsDetails2, TabNames.DetailsTabPage, TabPlacement.Placements.BottomMiddle, 3);
			fields.Add(ResString.GetMultilingualString("54fda755-0dc9-4807-b490-ed5182663adc", "FMC Tariff ID"), "FMCTariffID", false, (NoResString)GroupNames.GoodsDetails2, TabNames.DetailsTabPage, TabPlacement.Placements.BottomMiddle, 4);
			fields.Add(ResString.GetMultilingualString("077ff60f-d296-49fb-b9f9-a39b6017a66d", "Transport Mode"), "TransportModeDropEdit", false, (NoResString)GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 0);
			fields.Add(ResString.GetMultilingualString("65d4eba5-2cef-4483-beb1-5e7ae68cad3d", "Container Mode"), "ContainerModeDropEdit", false, (NoResString)GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 1);
			fields.Add(ResString.GetMultilingualString("54fda755-0dc9-4807-b490-ed5182663add", "Creditor"), "CreditorGuidFindBox", false, (NoResString)GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.BottomMiddle, 10);

			AddGoodsDetails1Field(fields, currentTemplateType, ResString.GetMultilingualString("9c839d80-11c7-4633-8af3-744e648ae7ac", "Weight"), "ActualWeightDropEdit", GroupNames.GoodsDetails1, TabNames.DetailsTabPage, TabPlacement.Placements.BottomLeft, 1);
			AddGoodsDetails1Field(fields, currentTemplateType, ResString.GetMultilingualString("6922e837-d1eb-45b4-a1d9-dc9f69d1acb1", "Volume"), "VolumeCalcDropEdit", GroupNames.GoodsDetails1, TabNames.DetailsTabPage, TabPlacement.Placements.BottomLeft, 2);
			AddGoodsDetails1Field(fields, currentTemplateType, ResString.GetMultilingualString("37dfd663-5c7f-47d2-a8c3-47d5be5b7273", "Chargeable"), "ChargeableDropEdit", GroupNames.GoodsDetails1, TabNames.DetailsTabPage, TabPlacement.Placements.BottomLeft, 3);

			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("7463a4a9-14b8-41e8-b45a-2b06927d08bc", "Shipper's Ref"), "JS_ShippingReferenceTextEdit", GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 5); // JS_ShippingReferenceTextEdit is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("16f94e47-1f60-41b3-b4f5-977bb3cbb387", "Goods Description"), "JS_GoodsDescriptionBoundTextBox", GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 6); // JS_GoodsDescriptionBoundTextBox is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("e2a8c556-7b68-44b8-9cd8-c2b07172ec5b", "Marks and Numbers"), "MarksAndNumbersNotePopupEdit", GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 7);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("57dfeb1d-d338-4e68-acde-a0c2bf75c249", "Estimated Pickup"), "PickupReadyDateEdit", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 1);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("2197169a-4a5d-4289-837e-d653df19e181", "Estimated Delivery"), "DeliveryOpenDateEdit", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 2);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("643933c4-ec5a-414c-b0bb-46ad7d381412", "On Board"), "OnBoardDropEdit", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 5);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("7df7d7b7-e45d-42ea-8282-12cb462f7c69", "Packs"), "PackagesCalcDropEdit", GroupNames.GoodsDetails1, TabNames.DetailsTabPage, TabPlacement.Placements.BottomLeft, 0);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("4d56ef18-ddd2-4fbd-b16b-a107d0816486", "Insurance Required"), "JP_InsuranceRequiredCheckBox", GroupNames.MonetaryValues, TabNames.DetailsTabPage, TabPlacement.Placements.BottomRight, 2); // JP_InsuranceRequiredCheckBox is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("9477DD7C-6F15-412F-B88E-8BCE309A1FD2", "Order Management Links"), "QuotedBookingOrderManagementControl", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 0);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("1e5810d9-c091-4d0b-a4f9-6b7eb37c8081", "Customs Entry"), "CustomsEntryNumberTypeBoundDropEdit", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 5);

			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("e4f1d502-b740-46d5-a4ce-ce692eab3bb3", "Pickup CFS"), "ReceivalPointGroupBox", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 0);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("3323065f-032d-4e97-b702-c07ba4351220", "Delivery CFS"), "DeliveryPointGroupBox", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 1);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("4dfd6ee6-3da7-4042-a91b-eccd7b65dc6d", "Pickup Agent"), "PickupAgentBoundOrganisationControl", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 2);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("77f1cd3a-81ac-485d-955a-25a2f19175e8", "Delivery Agent"), "DeliveryAgentBoundOrganisationControl", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 3);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("7d108ca7-4bf4-442d-bbea-ac86bce47b45", "Export Broker"), "ExportBrokerCoOrganisationControl", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 4);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("02a63656-3cb7-48d4-a12b-bc73e7855480", "Import Broker"), "ImportBrokerCoOrganisationControl", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 5);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("147a296b-bb1a-4919-8674-bbd038705e6d", "Port Transport"), "PortTransportGroupBox", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 6);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("7eee9467-90ed-41c8-8c44-fdc3a490a23a", "Controlling Customer"), "ControllingCustomerAddressControl", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 7);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("ce865e08-eca3-40ac-be42-da78aaea16d8", "Controlling Agent"), "ControllingAgentAddressControl", GroupNames.AdditionalContacts, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomRight, 8);

			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("b06395f4-ec66-4364-b4fd-5cb9f1830b22", "Booked Date"), "JS_A_BKDBoundReadOnlyDateEdit", GroupNames.AdditionalDetails1, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopRight, 0); // JS_A_BKDBoundReadOnlyDateEdit is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("6f33ffe7-ffa3-40e1-b549-cf0f45c9da8c", "Client Requested ETA"), "JS_ClientRequestedETADateEdit", GroupNames.AdditionalDetails1, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopRight, 1); // JS_ClientRequestedETADateEdit is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("806809DF-A247-41CE-916B-498FD3EAB46E", "Warehouse Rec."), "JS_A_RCVBoundDateEdit", GroupNames.AdditionalDetails1, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopRight, 2); // JS_A_RCVBoundDateEdit is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("42a2ca80-abf9-4ba1-b1f3-e8d107329d89", "Interim Receipt"), "JS_InterimReceiptBoundTextEdit", GroupNames.AdditionalDetails1, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopRight, 3); // JS_InterimReceiptBoundTextEdit is the name of a control, not a column
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("7f18608c-dd47-47e1-a1dc-cd1c62d49be4", "CFS Reference"), "CFSReferenceTextBox", GroupNames.AdditionalDetails1, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopRight, 4);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("58bb9d20-eb8b-4f9b-9ca2-e804cf9331d6", "Web Booking Party"), "BookingPartyDocAddressControl", GroupNames.AdditionalDetails1, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopRight, 5);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("6ACE8C9A-0988-4499-95C9-A180D866ABB4", "Pickup Address"), "PickupDocAddressControl", string.Empty, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomLeft, 0);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("CB9E57C7-6B58-4AE7-88E4-E70574916A98", "Delivery Address"), "DeliveryDocAddressControl", string.Empty, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopMiddle, 0);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("fc765179-3d76-4063-b179-935278b22977", "Gateway Sell"), "FreightGatewaySellRate", GroupNames.FreightRate, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomMiddle, 1);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("26e4a61c-2b08-4a17-a310-655c78ea521f", "Negotiated Cost"), "FreightCostRate", GroupNames.FreightRate, TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.BottomMiddle, 2);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("096a017b-8660-4499-93be-13cc85ac0cca", "Release Type"), "ReleaseTypeDropEdit", GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 8);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("70fcd72b-b1a6-4880-b4c4-d5a4c28eecf2", "Charges Apply"), "ChargesApplyDropEdit", GroupNames.General1, TabNames.DetailsTabPage, TabPlacement.Placements.TopLeft, 9);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("dd2f7881-8aec-4cb2-9ef7-685eddcffde0", "Shipper COD Amount"), "ShipperCODAmountCalcEdit", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 1);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("2750c435-8e22-48d7-acce-aee2e992e1b0", "Shipper COD Payment Method"), "ShipperCODTypeDropEdit", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 2);
			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("24c0f32b-90cd-4d0b-b913-50571dcb7fd1", "Spot Rate"), "FreightSpotRate", GroupNames.MonetaryValues, TabNames.DetailsTabPage, TabPlacement.Placements.BottomRight, 3);

			AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("7bcaa169-2955-4005-a2a3-facfb97a2633", "HBL Delivery Mode"), "ContainerModeOverrideDropEdit", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 8, false);

			AddQuoteOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("a68f3b5c-bd36-4e52-a35e-9bf2e0f88704", "Via"), "ViaCodeFindBox", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 0);
			AddQuoteOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("6e15c186-6b80-462b-a49a-864cdb31f7b5", "Transit Time"), "EntryRateTransitTimeDropEdit", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 3);

			AddQuoteOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("37eb2dcd-a1e6-4da7-9ffe-95769ed62a34", "Number of Entries"), "EntriesCalcEdit", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 3);
			AddQuoteOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("71c378d0-6cc9-4850-90bb-e7870639972d", "Number of Customs Entry/Invoice Lines"), "EntryInvoiceLinesCalcEdit", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 4);
			AddSpotQuoteOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("5df83ac4-3001-4fe7-ac61-317d2613eca6", "Potential Carriers"), "PotentialCarriersGroupBox", GroupNames.General3, TabNames.DetailsTabPage, TabPlacement.Placements.TopRight, 2);

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				AddBookingOnlyField(fields, currentTemplateType, ResString.GetMultilingualString("70bca386-fb47-440b-92f2-c2764ab9c8263", "Delivery Due Date"), "DeliveryDueDateDateEdit", GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 4);
			}

			var totalCO2e = fields.Add(ResString.GetMultilingualString("b701e33c-202f-4d5b-986d-386827ae4758", "Total CO2e"), "TotalCO2eTextBox", false, (NoResString)GroupNames.General2, TabNames.DetailsTabPage, TabPlacement.Placements.TopMiddle, 8);
			totalCO2e.IsAvailableFunction = () => ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled;

			fields.ResumeValidation();

			return fields;
		}

		void AddBookingOnlyField(FormCustomisableElementCollection collection, ZString currentTemplateType, MultilingualString elementDescription, string elementName, string groupName, string defaultTabPage, string defaultPlacement, int rowNumber, bool visible = true)
		{
			var element = collection.Add(elementDescription, elementName, false, (NoResString)groupName, defaultTabPage, defaultPlacement, rowNumber, visible);
			element.IsAvailableFunction = () => currentTemplateType == QuotedBooking.BookingWithQuoteCode || currentTemplateType == QuotedBooking.QuickBookingCode;
		}

		void AddQuoteOnlyField(FormCustomisableElementCollection collection, ZString currentTemplateType, MultilingualString elementDescription, string elementName, string groupName, string defaultTabPage, string defaultPlacement, int rowNumber)
		{
			FormCustomisableElement element = collection.Add(elementDescription, elementName, false, (NoResString)groupName, defaultTabPage, defaultPlacement, rowNumber);
			element.IsAvailableFunction = () => currentTemplateType == QuotedBooking.BookingWithQuoteCode || currentTemplateType == QuotedBooking.SpotQuoteCode;
		}

		void AddSpotQuoteOnlyField(FormCustomisableElementCollection collection, ZString currentTemplateType, MultilingualString elementDescription, string elementName, string groupName, string defaultTabPage, string defaultPlacement, int rowNumber)
		{
			FormCustomisableElement element = collection.Add(elementDescription, elementName, false, (NoResString)groupName, defaultTabPage, defaultPlacement, rowNumber);
			element.IsAvailableFunction = () => currentTemplateType == QuotedBooking.SpotQuoteCode;
		}

		void AddGoodsDetails1Field(FormCustomisableElementCollection collection, ZString currentTemplateType, MultilingualString elementDescription, string elementName, string groupName, string defaultTabPage, string defaultPlacement, int rowNumber)
		{
			var element = collection.Add(elementDescription, elementName, false, (NoResString)groupName, defaultTabPage, defaultPlacement, rowNumber);
			if (currentTemplateType == QuotedBooking.SpotQuoteCode)
			{
				var template = ParentWorkflowDescriptor.LastProcessTaskTemplate;
				element.IsAvailableFunction = () => template == null || template.P0_SubType1 != Core.Constants.ContainerModes.FCL;
			}
		}

		public override TabPlacementProhibition[] TabPlacementProhibitions
		{
			get { return new[] { new TabPlacementProhibition(TabNames.AdditionalDetailsTabPage, TabPlacement.Placements.TopLeft) }; }
		}

		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[] { QuotedBooking.Schema.Mode, QuotedBooking.Schema.Origin, QuotedBooking.Schema.Destination };
		}

		protected override string[] GetPropertiesThatAffectWorkflowTemplate()
		{
			return new[] { ProcessTaskTemplate.Schema.P0_SubType2 };
		}
	}
}
