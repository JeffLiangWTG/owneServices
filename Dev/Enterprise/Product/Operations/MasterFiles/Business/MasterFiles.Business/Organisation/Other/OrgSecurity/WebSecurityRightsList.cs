using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Registry.Business;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Defines a list of Organisation Web Security rights.
	/// If any item's description is changed, a transformation MUST be created to update all items in the OrgSecurity table.
	/// </summary>
	[ImmutableObject(true)]
	public class WebSecurityRightsList : WebSecurityRightsProvider
	{
		#region SuppressResourceStringsCheckRegion

		public static readonly WebSecurityRight WebQuotes = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("547e5c21-efaa-4465-90f9-7e0ce9efbcaa", "Web Quoting"), WebSecurityRightSharesList.WebQuotes);
		public static readonly WebSecurityRight WebInvoicingAndStatements = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("a8a04f93-6141-4cd6-b0fa-78a94b84df63", "Web Invoicing and Statements"), WebSecurityRightSharesList.WebInvoicingAndStatements);
		public static readonly WebSecurityRight WebBookingsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("94b1360a-324e-40f5-9d3c-66939553e6ee", "Web Booking (View)"), WebSecurityRightSharesList.WebBookingsView);
		public static readonly WebSecurityRight WebBookingsAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("82b94f36-eaf1-4ef5-ae60-1c2274fb05ed", "Web Booking (Add/Edit)"), WebSecurityRightSharesList.WebBookingsAddEdit);
		public static readonly WebSecurityRight WebBookingsSelectSchedules = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("06c8a527-e9e0-4c48-a535-81fa6bbb4939", "Web Booking - Select Schedules"), WebSecurityRightSharesList.WebBookingsSelectSchedules);
		public static readonly WebSecurityRight WebDeclarationView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("86584d41-993c-41d7-95ec-77fc1d4c09c6", "Web Declaration (View)"), WebSecurityRightSharesList.WebDeclarationView);
		public static readonly WebSecurityRight WebShipmentsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("b7f981a9-d3a3-4cc1-878c-7a4f78a88e62", "Web Shipments (View)"), WebSecurityRightSharesList.WebShipmentsView);

		public static readonly WebSecurityRight WebCFSShipmentView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("05ED290E-188D-49FB-9D05-EF096CD102D1", "Web CFS Shipment (View)"), WebSecurityRightSharesList.WebCFSShipmentView);

		public static readonly WebSecurityRight WebISFView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("416c39d8-3360-4f68-9d32-07bf414ae9c5", "Web ISF (View)"), WebSecurityRightSharesList.WebISFView);
		public static readonly WebSecurityRight WebISFAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("5e75da11-2643-489d-8393-fc08cc5917fc", "Web ISF (Add/Edit)"), WebSecurityRightSharesList.WebISFAddEdit);
		public static readonly WebSecurityRight WebISFSend = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("61cbfc96-a460-449a-9f26-9993730488c7", "Web ISF (Send)"), WebSecurityRightSharesList.WebISFSend);
		public static readonly WebSecurityRight WebISFDelete = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("76ab58b8-f3aa-4b2a-a771-1c1676ac0771", "Web ISF (Delete)"), WebSecurityRightSharesList.WebISFDelete);

		public static readonly WebSecurityRight WebOrdersView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("b992f6e0-cf57-466e-ba3e-3b0cf52eb47c", "Web Orders (View)"), WebSecurityRightSharesList.WebOrdersView);
		public static readonly WebSecurityRight WebOrdersAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("74a018fc-ada8-4246-9076-4054996e543f", "Web Orders (Add/Edit)"), WebSecurityRightSharesList.WebOrdersAddEdit);
		public static readonly WebSecurityRight WebOrdersAllowSplit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("97422D64-DFD6-4021-8689-186C234759FF", "Web Orders (Allow Split)"), WebSecurityRightSharesList.WebOrdersAllowSplit);

		public static readonly WebSecurityRight WebInventoryView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("45bd3dfb-da95-4baf-86fc-2ed14ba89a3b", "Web Warehouse Inventory (View)"), WebSecurityRightSharesList.WebInventoryView);
		public static readonly WebSecurityRight WebWarehouseOrdersView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("ca9ae666-42e8-446e-80e8-b5f7da365ccf", "Web Warehouse Orders (View)"), WebSecurityRightSharesList.WebWarehouseOrdersView);
		public static readonly WebSecurityRight WebWarehouseOrdersAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("bbbd8093-4b4b-4833-b8ab-be7268bbbe5a", "Web Warehouse Orders (Add/Edit)"), WebSecurityRightSharesList.WebWarehouseOrdersAddEdit);
		public static readonly WebSecurityRight WebWarehouseProductsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("a57dcc8e-c332-4237-88de-da5866770ad8", "Web Warehouse Products (View)"), WebSecurityRightSharesList.WebWarehouseProductsView);
		public static readonly WebSecurityRight WebWarehouseReceiptsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("ff15c20f-5017-4556-bcd6-7ba57ce99230", "Web Warehouse Receipts (View)"), WebSecurityRightSharesList.WebWarehouseReceiptsView);
		public static readonly WebSecurityRight WebWarehouseReceiptsAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("c16c8cd8-41d1-494c-940c-789a1a4c7cff", "Web Warehouse Receipts (Add/Edit)"), WebSecurityRightSharesList.WebWarehouseReceiptsAddEdit);

		public static readonly WebSecurityRight WebCartageView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("725325dc-fcdf-46a7-b3b6-e7caf57d1e60", "Web Transport Job (View)"), WebSecurityRightSharesList.WebCartageView);

		public static readonly WebSecurityRight WebContainers = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("3f4a1da9-4706-44b6-9256-043fb1a4bb23", "Web Containers (View)"), WebSecurityRightSharesList.WebContainers);
		public static readonly WebSecurityRight WebContainersEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("c7f57c98-5931-463f-9778-9be0af176fa9", "Web Containers (Edit)"), WebSecurityRightSharesList.WebContainersEdit);
		public static readonly WebSecurityRight WebContainersAddEditNumber = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("7ad5bce9-feef-4c4e-8cf3-608a31dbd46a", "Web Containers (Add/Edit) Number"), WebSecurityRightSharesList.WebContainersAddEditNumber);
		public static readonly WebSecurityRight WebContainersEditClientRef = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("1e07af0c-4037-4164-828b-9eee9ef18d40", "Container (Edit) Client Reference"), WebSecurityRightSharesList.WebContainersEditClientRef);
		public static readonly WebSecurityRight WebContainersReqDeliveryDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("0379bd8c-c669-4d84-a3de-402b0e8277b5", "Container Required Delivery (Edit)"), WebSecurityRightSharesList.WebContainersReqDeliveryDate);
		public static readonly WebSecurityRight WebContainersConDeliveryDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("74559f22-6536-4538-a548-7297831944bd", "Container Confirmed Delivery (Edit)"), WebSecurityRightSharesList.WebContainersConDeliveryDate);
		public static readonly WebSecurityRight WebContainersActDeliveryDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("d4bc1846-4455-49fe-8129-87f427bb0bce", "Container Actual Delivery (Edit)"), WebSecurityRightSharesList.WebContainersActDeliveryDate);
		public static readonly WebSecurityRight WebContainersEstDehireDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("1e4bfbfd-02f6-4bdd-a19d-dc51e5f5def3", "Container Estimated De-hire (Edit)"), WebSecurityRightSharesList.WebContainersEstDehireDate);
		public static readonly WebSecurityRight WebContainersPickup = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("5e838cd7-6785-42ee-ab95-14b2199351ae", "Container Pickup (Edit)"), WebSecurityRightSharesList.WebContainersPickup);
		public static readonly WebSecurityRight WebContainersActualDehire = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("ccafdb10-7974-4515-b627-402a10208725", "Container Actual De-hire (Edit)"), WebSecurityRightSharesList.WebContainersActualDehire);
		public static readonly WebSecurityRight WebContainersSequence = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("f4a44900-266d-4515-9fa2-426fcfad16bd", "Container Sequence (Edit)"), WebSecurityRightSharesList.WebContainersSequence);

		public static readonly WebSecurityRight WebLinerAndAgencyContainers = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("940E0F6E-3A3D-4F33-987D-943C431EB100", "Web Liner & Agency Containers (View)"), WebSecurityRightSharesList.WebLinerAndAgencyContainers);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("80C74021-FB9D-43CD-B03A-159C359BF3AD", "Web Liner & Agency Containers (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyContainersEdit);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersAddEditNumber = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("9297C44E-A349-4DDE-A0D7-363CFF2F3B88", "Web Liner & Agency Containers (Add/Edit) Number"), WebSecurityRightSharesList.WebLinerAndAgencyContainersAddEditNumber);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersEstimatedFullDeliveryDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("90A4DE0A-E1ED-4C7F-AA68-8227554509B8", "Web Liner & Agency Container Estimated Full Delivery (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyContainersEstimatedFullDeliveryDate);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersActDeliveryDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("EC334EB9-0E06-4215-9BBF-BEB4A70D18DB", "Web Liner & Agency Container Actual Delivery (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyContainersActDeliveryDate);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersEmptyReadyToReturnDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("1757C412-8AC6-498E-924F-112402AFBD79", "Web Liner & Agency Container Empty Ready to Return (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyContainersEmptyReadyToReturnDate);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersEmptyReturnReqByDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("46BC77E7-0842-4ADD-92AE-4CD80F9E833E", "Web Liner & Agency Container Empty Return Req. By (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyContainersEmptyReturnReqByDate);
		public static readonly WebSecurityRight WebLinerAndAgencyContainersEmptyReadyOnDate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("61E9CEFE-B7EC-478B-8AAD-712EF70416B4", "Web Liner & Agency Container Empty Ready On (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyContainersEmptyReadyOnDate);

		public static readonly WebSecurityRight WebReports = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("9310342a-32fb-48f2-a7db-9b20687c21db", "Web Reports"), WebSecurityRightSharesList.WebReports);
		public static readonly WebSecurityRight WebPublishLayouts = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("4f0b3015-643b-4c10-916a-03e1d1b1170c", "Web Publish Layouts"), WebSecurityRightSharesList.WebPublishLayouts);
		public static readonly WebSecurityRight WebAddNewOrganisations = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("655aea1c-46ec-41e6-874a-3da8b8740d4a", "Web Organizations (Add)"), WebSecurityRightSharesList.WebAddNewOrganisations);

		public static readonly WebSecurityRight WebLinerAndAgencyBookingsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("a728f703-5e17-4260-a47e-4263498749bd", "Web Liner & Agency Bookings (View)"), WebSecurityRightSharesList.WebLinerAndAgencyBookingsView);
		public static readonly WebSecurityRight WebLinerAndAgencyBookingsAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("ebde373b-e234-485b-b13d-5c32192bd57f", "Web Liner & Agency Bookings (Add/Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyBookingsAddEdit);
		public static readonly WebSecurityRight WebLinerAndAgencyBillsOfLadingView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("c9935b12-36bf-4a7e-b282-542f69c4e322", "Web Liner & Agency Bills of Lading (View)"), WebSecurityRightSharesList.WebLinerAndAgencyBillsOfLadingView);
		public static readonly WebSecurityRight WebLinerAndAgencyFwdInstructionsEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("3647b73f-f489-4b4d-bf78-5eba25b15615", "Web Liner & Agency Fwd Instruction (Edit)"), WebSecurityRightSharesList.WebLinerAndAgencyFwdInstructionsEdit);

		public static readonly WebSecurityRight WebShipmentDeliveryAdd = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("55fa3453-1a8e-4cab-b155-dc662a58a136", "Web Shipment Delivery (Add)"), WebSecurityRightSharesList.WebShipmentDeliveryAdd);
		public static readonly WebSecurityRight WebShipmentDeliveryEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("329b968e-2b07-4c1a-a518-04aa0fb84928", "Web Shipment Delivery (Edit)"), WebSecurityRightSharesList.WebShipmentDeliveryEdit);

		public static readonly WebSecurityRight WebDocumentsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("c587c0d5-88cf-43d8-8ca4-4da35b677724", "Web Documents (View)"), WebSecurityRightSharesList.WebDocumentsView);
		public static readonly WebSecurityRight WebDocumentsAdd = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("f2f7206a-dac2-4f93-8e5f-5b6cd069e82e", "Web Documents (Add)"), WebSecurityRightSharesList.WebDocumentsAdd);

		public static readonly WebSecurityRight WebMAWBView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("c083f96d-13f5-4336-938f-dd0146c2bd42", "Web MAWB (View)"), WebSecurityRightSharesList.WebMAWBView);
		public static readonly WebSecurityRight WebMAWBEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("7c04916f-1d45-4a4b-9041-39f3c58fb01e", "Web MAWB (Edit)"), WebSecurityRightSharesList.WebMAWBEdit);
		public static readonly WebSecurityRight WebMAWBSend = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("627a8c35-af48-4ba7-ae1f-70a7a6aee369", "Web MAWB (Send)"), WebSecurityRightSharesList.WebMAWBSend);
		public static readonly WebSecurityRight WebMAWBAdmin = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("8e182f5e-2361-44c5-ad54-917e184458ad", "Web MAWB (Admin)"), WebSecurityRightSharesList.WebMAWBAdmin);
		public static readonly WebSecurityRight WebHAWBView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("59f4be98-f7c7-4040-a021-dcc3f28a4de6", "Web HAWB (View)"), WebSecurityRightSharesList.WebHAWBView);
		public static readonly WebSecurityRight WebHAWBEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("b579c272-7953-48b0-a00c-5f2edb6294b7", "Web HAWB (Edit)"), WebSecurityRightSharesList.WebHAWBEdit);
		public static readonly WebSecurityRight WebHAWBSend = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("2e239a93-f5f8-4346-9c63-034bbadf2c1c", "Web HAWB (Send)"), WebSecurityRightSharesList.WebHAWBSend);
		public static readonly WebSecurityRight WebHAWBAdmin = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("435706ef-6fbf-4e6d-8402-56933078e156", "Web HAWB (Admin)"), WebSecurityRightSharesList.WebHAWBAdmin);

		public static readonly WebSecurityRight WebEstimatedMilestonesUpdate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("d574a3e6-bfa9-4db2-b8cf-bdcfd5d65fa0", "Estimated Milestones (Update)"), WebSecurityRightSharesList.WebEstimatedMilestonesUpdate);
		public static readonly WebSecurityRight WebActualMilestonesUpdate = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("921e1927-6b32-464d-9637-c581df71785d", "Actual Milestones (Update)"), WebSecurityRightSharesList.WebActualMilestonesUpdate);
		public static readonly WebSecurityRight WebEventsView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("8f888a9d-cba4-44c6-a394-daef93f2d6f0", "Events (View)"), WebSecurityRightSharesList.WebEventsView);

		public static readonly WebSecurityRight WebTranslationFeedback = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("7d5140f0-c951-4e3f-b04e-9bd633212761", "Web Translation Feedback"), WebSecurityRightSharesList.WebTranslationFeedback);

		public static readonly WebSecurityRight eCommerceShipperPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("4c93b6b7-d812-4e26-a538-49aa5c641101", "e-Commerce - Shipper Portal"), WebSecurityRightSharesList.eCommerceShipperPortal);
		public static readonly WebSecurityRight eCommerceOriginDepotPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("122dfc76-8d98-46a9-9d3c-1d9e95194c3f", "e-Commerce - Origin Depot Portal"), WebSecurityRightSharesList.eCommerceOriginDepotPortal);
		public static readonly WebSecurityRight eCommerceDestinationDepotPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("7fcbacfe-0b06-4257-ae02-346dfe53dcff", "e-Commerce - Destination Depot Portal"), WebSecurityRightSharesList.eCommerceDestinationDepotPortal);
		public static readonly WebSecurityRight eCommerceViewCarriersAndDepots = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("fcde53eb-c156-42a8-adf0-348e211708d5", "e-Commerce - Carriers & Depots (View)"), WebSecurityRightSharesList.eCommerceViewCarriersAndDepots);
		public static readonly WebSecurityRight eCommerceConfirmHVLVBookingHeader = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("0b8adc9f-9f4f-48ff-bbb9-c94f15e0e8c9", "e-Commerce - Confirm HVLV Booking Header"), WebSecurityRightSharesList.eCommerceConfirmHVLVBookingHeader);
		public static readonly WebSecurityRight eCommerceReceiveHVLVBookingHeader = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("5e652625-875e-450c-93f3-0090113bfb97", "e-Commerce - Receive HVLV Booking Header"), WebSecurityRightSharesList.eCommerceReceiveHVLVBookingHeader);
		public static readonly WebSecurityRight eCommerceLodgeHVLVOriginLoadList = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("2dded4b1-afff-457e-a994-3f720bac62fe", "e-Commerce - Lodge HVLV Origin Load List"), WebSecurityRightSharesList.eCommerceLodgeHVLVOriginLoadList);
		public static readonly WebSecurityRight eCommerceCalculateDepotAndLastMileCarrierDetails = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("AB77FFDF-5284-41C8-B6A6-43D3A408DE31", "e-Commerce - Calculate Depot and Last Mile Carrier Details"), WebSecurityRightSharesList.eCommerceCalculateDepotAndLastMileCarrierDetails);
		public static readonly WebSecurityRight eCommerceLastMileCarrierBooking = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("3FD9E31C-8A10-4442-978E-D6D2E355271A", "e-Commerce - Last Mile Carrier Booking"), WebSecurityRightSharesList.eCommerceLastMileCarrierBooking);
		public static readonly WebSecurityRight eCommerceAutomatedBookingHeaderCreation = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("8E9F41D8-90AA-4F23-8849-6E6FDC06E1B1", "e-Commerce - Automated Booking Header Creation"), WebSecurityRightSharesList.eCommerceAutomatedBookingHeaderCreation);

		public static readonly WebSecurityRight TransportCustomerPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("b5ccd30b-96d6-40dd-a4a7-c2f34dd400a4", "Land Transport Customer Portal"), WebSecurityRightSharesList.TransportCustomerPortal);
		public static readonly WebSecurityRight TransportSubContractorPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("caaf2001-e670-44fe-b544-97fcd680f8ac", "Land Transport Sub-Contractor Portal"), WebSecurityRightSharesList.TransportSubContractorPortal);
		public static readonly WebSecurityRight MapConsignmentViaDataWizard = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("fa59d42e-1489-40d1-b539-fcb945d44d13", "Land Transport - Map Consignment via Data Wizard"), WebSecurityRightSharesList.MapConsignmentViaDataWizard);

		public static readonly WebSecurityRight NettingParticipantPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("22bbf57e-9dcf-4399-8c1f-7da74e07eacd", "Netting Participant Portal"), WebSecurityRightSharesList.NettingParticipantPortal);
		public static readonly WebSecurityRight USAMSPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("9AB504AB-E91D-46D8-A51E-01BCAC4B8409", "US AMS Portal"), WebSecurityRightSharesList.USAMSPortal);
		public static readonly WebSecurityRight USAMSView = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("4cab9585-0b52-4bd8-814d-0f580b2301b7", "US AMS (View)"), WebSecurityRightSharesList.USAMSView);
		public static readonly WebSecurityRight USAMSAddEdit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("5ca678f6-c43b-47c2-b221-5e1597fd3242", "US AMS (Add/Edit)"), WebSecurityRightSharesList.USAMSAddEdit);
		public static readonly WebSecurityRight USAMSSend = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("dbe91517-2268-43cc-a2c7-c73233fd44b3", "US AMS (Send)"), WebSecurityRightSharesList.USAMSSend);
		public static readonly WebSecurityRight USAMSDelete = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("ea47841d-91fc-4db3-a68e-3a68d92c8900", "US AMS (Delete)"), WebSecurityRightSharesList.USAMSDelete);

		public static readonly WebSecurityRight MapEntitiesViaDataWizard = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("72d31b94-b80e-42eb-9b93-cd259ef8aaea", "Generic Data Import - Create Mappings"), WebSecurityRightSharesList.MapEntitiesViaDataWizard);

		public static readonly WebSecurityRight eRequestPortalViewAll = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("e4c50ce5-2adf-4132-88a5-84b5430ea2af", "eRequest Portal (View All)"), WebSecurityRightSharesList.eRequestPortalViewAll);
		public static readonly WebSecurityRight eRequestPortalViewOwn = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("fd44140e-73ce-4501-8d3b-d2e943454ab5", "eRequest Portal (View Own)"), WebSecurityRightSharesList.eRequestPortalViewOwn);
		public static readonly WebSecurityRight eRequestPortalSubmit = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("3647281f-ca42-4677-9634-03e9d0ac677d", "eRequest Portal Submit"), WebSecurityRightSharesList.eRequestPortalSubmit);

		public static readonly WebSecurityRight TrackingPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("bc8cb2a6-2635-4419-ac14-7ef6d2849bf3", "Tracking Portal"), WebSecurityRightSharesList.TrackingPortal);

		public static readonly WebSecurityRight SlotManagementClientPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("3987946b-e4bc-42e1-b602-f2c1c6836af0", "Slot Management Client Portal"), WebSecurityRightSharesList.SlotManagementClientPortal);
		public static readonly WebSecurityRight SlotManagementStaffPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("6188a026-bd3c-42ab-b272-6174acf6f15c", "Slot Management Staff Portal"), WebSecurityRightSharesList.SlotManagementStaffPortal);

		public static readonly WebSecurityRight ContainerYardShippingLinePortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("ffe605b5-cbf0-4c7c-be99-e25ad459a94c", "Container Yard Shipping Line Portal"), WebSecurityRightSharesList.ContainerYardShippingLinePortal);

		public static readonly WebSecurityRight WebAccreditationsViewAll = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("1206e6bb-5f1c-4a6a-8280-9d6a854b86cd", "Web Accreditations (View All)"), WebSecurityRightSharesList.WebAccreditationsViewAll);
		public static readonly WebSecurityRight WebAccreditationsViewOwn = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("0e55c7f8-4ecf-4e90-963f-c11782241bbf", "Web Accreditations (View Own)"), WebSecurityRightSharesList.WebAccreditationsViewOwn);

		public static readonly WebSecurityRight TransitWarehouseClientPortal = WebSecurityRight.FromWebSecurityRightShare(ResString.GetMultilingualString("acf909b7-b6ab-4519-b0d4-aff6ed738d4f", "Transit Warehouse Client Portal"), WebSecurityRightSharesList.TransitWarehouseClientPortal);

		#endregion

		#region List Population using Reflection

		public static WebSecurityRightsList New()
		{
			WebSecurityRightsList result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = new WebSecurityRightsList();
			}
			return result;
		}

		protected delegate WebSecurityRightsList NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This is the factory pattern. Called methods do not rely on state")]
		protected WebSecurityRightsList()
		{
			var enableSecurityGroupsForContactsInGLOW = GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value;
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
			{
				var pair = field.GetValue(this) as WebSecurityRight;
				if (pair != null && ShouldAddReflectedRight(pair) && !(enableSecurityGroupsForContactsInGLOW && WebSecurityApplication.GlowWeb == pair.WebApplication))
				{
					Add(pair);
				}
			}
		}

		protected virtual bool ShouldAddReflectedRight(WebSecurityRight right)
		{
			return true;
		}

		#endregion
	}
}
