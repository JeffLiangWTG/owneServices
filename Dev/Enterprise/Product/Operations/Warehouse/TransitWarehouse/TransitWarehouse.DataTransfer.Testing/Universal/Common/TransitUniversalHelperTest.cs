using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class TransitUniversalHelperTest : TransitUniversalTestCase
	{
		public void TestLogs_GetBookingParty_ArrivalCFS()
		{
			TestLogs_GetBookingPartyCore(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW);
			AssertContains("Should add a log when retrieving booking party.", "Information - Matching 'Booking Party':- Matched to 'OCEAND' by code from 'ReceivingForwarderAddress' for Arrival Transit Warehouse. Successfully loaded Booking Party.", Logger.Logs);
		}

		public void TestLogs_GetBookingParty_DepartureCFS()
		{
			TestLogs_GetBookingPartyCore(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW);
			AssertContains("Should add a log when retrieving booking party.", "Information - Matching 'Booking Party':- Matched to 'ISSEXP' by code from 'SendingForwarderAddress' for Departure Transit Warehouse. Successfully loaded Booking Party.", Logger.Logs);
		}

		public void TestLogs_GetBookingParty_NoRecipient_InternalUXML()
		{
			TestLogs_GetBookingPartyCore(DocAddressType.LocalCartageCFS);
			AssertContains("Should add a log when retrieving booking party.", "Information - Matching 'Booking Party':- Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.", Logger.Logs);
		} 

		void TestLogs_GetBookingPartyCore(DocAddressType docAddressType, RecipientRoleType? recipientRoleType = null)
		{
			Data.SetupForForwardingImport();

			var cfsAddress = GetNewAddressData_INTHEMSYD(docAddressType);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("ISSEXP", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("OCEAND", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			var bookingParty = TransitUniversalHelper.GetBookingParty(Factory, Logger);

			AssertNotNull("Booking party should be retrieved.", bookingParty);
		}

		public void TestLogs_GetBookingParty_NoRecipient_ExternalUXML()
		{
			Data.SetupForForwardingImport();

			var cfsAddress = GetNewAddressData_INTHEMSYD(DocAddressType.LocalCartageCFS);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DCT } } });

			var tempCompany = Factory.New<GlbCompany>();
			tempCompany.GC_Code = "";
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			var bookingParty = TransitUniversalHelper.GetBookingParty(Factory, Logger);

			AssertNull("Booking party should not be retrieved.", bookingParty);
			AssertContains("Should add an error for fetching booking party.", "Error - Could not find 'Booking Party':- Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse, and UXML does not originate from this system.", Logger.Logs);
		}

		public void TestLogs_GetBookingParty_ArrivalCFS_InvalidOrgCode_InternalUXML() => TestLogs_GetBookingParty_InvalidOrgCode_InternalUXML(RecipientRoleType.ATW, "Information - Matching 'Booking Party':- Organization Code 'NOTEX2' does not exist for this Arrival Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.");

		public void TestLogs_GetBookingParty_DepartureCFS_InvalidOrgCode_InternalUXML() => TestLogs_GetBookingParty_InvalidOrgCode_InternalUXML(RecipientRoleType.DTW, "Information - Matching 'Booking Party':- Organization Code 'NOTEX1' does not exist for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.");

		void TestLogs_GetBookingParty_InvalidOrgCode_InternalUXML(RecipientRoleType recipientRoleType, string orgCodeWarningMsg)
		{
			Data.SetupForForwardingImport();

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("NOTEX1", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("NOTEX2", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			var bookingParty = TransitUniversalHelper.GetBookingParty(Factory, Logger);

			AssertNotNull("Booking party should be retrieved.", bookingParty);
			AssertContains("Should add a warning for invalid org code.", orgCodeWarningMsg, Logger.Logs);
		}

		public void TestLogs_GetBookingParty_ArrivalCFS_InvalidOrgCode_ExternalUXML() => TestLogs_GetBookingParty_InvalidOrgCode_ExternalUXML(RecipientRoleType.ATW, "Error - Could not find 'Booking Party':- Organization Code 'NOTEX2' does not exist for this Arrival Transit Warehouse, and UXML does not originate from this system.");

		public void TestLogs_GetBookingParty_DepartureCFS_InvalidOrgCode_ExternalUXML() => TestLogs_GetBookingParty_InvalidOrgCode_ExternalUXML(RecipientRoleType.DTW, "Error - Could not find 'Booking Party':- Organization Code 'NOTEX1' does not exist for this Departure Transit Warehouse, and UXML does not originate from this system.");

		void TestLogs_GetBookingParty_InvalidOrgCode_ExternalUXML(RecipientRoleType recipientRoleType, string orgCodeErrorMsg)
		{
			Data.SetupForForwardingImport();

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("NOTEX1", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("NOTEX2", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			var tempCompany = Factory.New<GlbCompany>();
			tempCompany.GC_Code = "";
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			var bookingParty = TransitUniversalHelper.GetBookingParty(Factory, Logger);

			AssertNull("Booking party should not be retrieved.", bookingParty);
			AssertContains("Should add an error for fetching booking party.", orgCodeErrorMsg, Logger.Logs);
		}

		public void TestLogs_GetBookingParty_ArrivalCFS_EmptyOrgCode_InternalUXML() => TestLogs_GetBookingParty_EmptyOrgCode_InternalUXML(RecipientRoleType.ATW, "Information - Matching 'Booking Party':- Organization Code is empty in 'ReceivingForwarderAddress' for this Arrival Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.");

		public void TestLogs_GetBookingParty_DepartureCFS_EmptyOrgCode_InternalUXML() => TestLogs_GetBookingParty_EmptyOrgCode_InternalUXML(RecipientRoleType.DTW, "Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.");

		void TestLogs_GetBookingParty_EmptyOrgCode_InternalUXML(RecipientRoleType recipientRoleType, string orgCodeWarningMsg)
		{
			Data.SetupForForwardingImport();

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			var bookingParty = TransitUniversalHelper.GetBookingParty(Factory, Logger);

			AssertNotNull("Booking party should be retrieved.", bookingParty);
			AssertContains("Should add a warning for empty org code.", orgCodeWarningMsg, Logger.Logs);
		}

		public void TestLogs_GetBookingParty_ArrivalCFS_EmptyOrgCode_ExternalUXML() => TestLogs_GetBookingParty_EmptyOrgCode_ExternalUXML(RecipientRoleType.ATW, "Error - Could not find 'Booking Party':- Organization Code is empty in 'ReceivingForwarderAddress' for this Arrival Transit Warehouse, and UXML does not originate from this system.");

		public void TestLogs_GetBookingParty_DepartureCFS_EmptyOrgCode_ExternalUXML() => TestLogs_GetBookingParty_EmptyOrgCode_ExternalUXML(RecipientRoleType.DTW, "Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.");

		void TestLogs_GetBookingParty_EmptyOrgCode_ExternalUXML(RecipientRoleType recipientRoleType, string orgCodeWarningMsg)
		{
			Data.SetupForForwardingImport();

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			var tempCompany = Factory.New<GlbCompany>();
			tempCompany.GC_Code = "";
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			var bookingParty = TransitUniversalHelper.GetBookingParty(Factory, Logger);

			AssertNull("Booking party should not be retrieved.", bookingParty);
			AssertContains("Should add an error for fetching booking party.", orgCodeWarningMsg, Logger.Logs);
		}

		public void TestGetPackageStates_FromReceiveConsignment()
		{
			var consignmentID = "CONID123";
			var packageIDs = new[] { "PKG-1", "PKG-2" };
			var consignmentDataObject = Data.CreateShipmentWithPackages(consignmentID, packageIDs);
			var rcn = Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			// Set up Dispatch Consignment Data Object
			var packageIDsToUseInDispatch = packageIDs;
			var dispatchConsignmentID = "DISPATCH123";
			var dispatchConsignmentDataObject = Data.CreateShipmentWithPackages(dispatchConsignmentID, packageIDsToUseInDispatch);
			dispatchConsignmentDataObject.DataContext = consignmentDataObject.DataContext;
			var dcn = new WhsTransitDispatchConsignmentDataObjectReader(dispatchConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			var rcnPackages = TransitUniversalHelper.GetRelatedPackageStates(Factory, WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, new[] { rcn.PK });
			var dcnPackages = TransitUniversalHelper.GetRelatedPackageStates(Factory, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, new[] { dcn.PK });
			AssertEquals("Should be able to get package states from receive consignment", 2, rcnPackages.Length);
			AssertEquals("Should be able to get package states from dispatch consignment", 2, dcnPackages.Length);
		}

		#region Implementation

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory.BOFactory);

		#endregion
	}
}
