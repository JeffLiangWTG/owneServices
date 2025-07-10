using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class OrgAddressImportHelperTest : TransitUniversalTestCase
	{
		#region TestTestPopulateTransportOrg_FromRelatedPartyMainAddress

		public void TestTestPopulateTransportOrg_FromRelatedPartyMainAddress()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });

			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Road };

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKP";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_Code = "RP";

			var anotherAddress = Factory.NewWithValidTestData<OrgAddress>();
			anotherAddress.OA_Address1 = "another address";
			anotherAddress.OA_OH = relatedParty.PK;
			relatedParty.Addresses.Add(anotherAddress);

			relatedParty.MainAddress.OA_Address1 = "related party address";

			bookingParty.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Road, ContainerModes.All, null);

			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			Factory.SaveForTesting();

			OrgAddressImportHelper.PopulateTransportOrg(Data.ShipmentDataObject, null, bookingParty, rtu, Logger, Factory);
			Factory.SaveForTesting();

			AssertTransportCompanyAddress(relatedParty.MainAddress.PK, rtu.PK, true);
		}

		#endregion

		#region TestTestPopulateTransportOrg_ByTransportCompany

		public void TestPopulateTransportOrg_ByTransportCompany_ArrivalWarehouse() => TestPopulateTransportOrg_ByTransportCompany(RecipientRoleType.ATW);

		public void TestPopulateTransportOrg_ByTransportCompany_DepartureWarehouse() => TestPopulateTransportOrg_ByTransportCompany(RecipientRoleType.DTW);

		void TestPopulateTransportOrg_ByTransportCompany(RecipientRoleType recipientRoleType)
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var helper = new TestHelperForUniversal(Factory.BOFactory);
			var transportAddressForDepartureCFS = helper.CreateOrganizationAddress("DTRA", DocAddressType.DepartureCFSLocalTransportAddress);
			var orgHeaderForDepartureCFS = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderForDepartureCFS.OH_Code = "DTRA";
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(transportAddressForDepartureCFS);

			var transportAddressForArrivalCFS = helper.CreateOrganizationAddress("ATRA", DocAddressType.ArrivalCFSLocalTransportAddress);
			var orgHeaderForArrivalCFS = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderForArrivalCFS.OH_Code = "ATRA";
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(transportAddressForArrivalCFS);

			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Road };

			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			Factory.SaveForTesting();

			OrgAddressImportHelper.PopulateTransportOrg(Data.ShipmentDataObject, null, null, rtu, Logger, Factory);
			Factory.SaveForTesting();

			AssertTransportCompanyAddress(recipientRoleType == RecipientRoleType.DTW ? orgHeaderForDepartureCFS.MainAddress.PK : orgHeaderForArrivalCFS.MainAddress.PK, rtu.PK);
		}

		#endregion

		#region TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_LTT_DLV_ArrivalWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, RecipientRoleType.ATW, true);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_LTT_PIC_ArrivalWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, RecipientRoleType.ATW, false);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_LTT_PAD_ArrivalWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, RecipientRoleType.ATW, true);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_OTH_PAD_ArrivalWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.PickupAndDelivery, RecipientRoleType.ATW, false);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_LTT_DLV_DepartureWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, RecipientRoleType.DTW, false);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_LTT_PIC_DepartureWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, RecipientRoleType.DTW, true);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_LTT_PAD_DepartureWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, RecipientRoleType.DTW, true);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection_OTH_PAD_DepartureWarehouse() => TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.PickupAndDelivery, RecipientRoleType.DTW, false);

		void TestPopulateTransportOrg_ByRelatedParties_MatchByPartyTypeAndDirection(string partyType, string direction, RecipientRoleType recipientRoleType, bool matched)
		{
			Data.SetupForForwardingImport();

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_Code = "AU";

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKP";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_Code = "RP";
			relatedParty.MainAddress.OA_Address1 = "related party address";

			bookingParty.AddRelatedParty(relatedParty.PK, partyType, direction, TransportModes.Sea, ContainerModes.FCL, null);

			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Sea };
			Data.ShipmentDataObject.ContainerMode = new ContainerMode() { Code = ContainerModes.FCL };

			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			Factory.SaveForTesting();

			OrgAddressImportHelper.PopulateTransportOrg(Data.ShipmentDataObject, null, bookingParty, rtu, Logger, Factory);
			Factory.SaveForTesting();

			AssertTransportCompanyAddress(relatedParty.MainAddress.PK, rtu.PK, matched);
		}

		#endregion

		#region TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode_Sea_FCL() => TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(TransportModes.Sea, ContainerModes.FCL);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode_Sea_LCL() => TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(TransportModes.Sea, ContainerModes.LCL);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode_Sea_Other() => TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(TransportModes.Sea, ContainerModes.Other, isMatched: false);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode_Road() => TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(TransportModes.Road);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode_Air() => TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(TransportModes.Air);

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode_DifferentTransportMode() => TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(TransportModes.Air, consolTransportMode: TransportModes.Road, isMatched: false);

		void TestPopulateTransportOrg_ByRelatedParties_MatchByTransportModeAndContainerMode(string transportMode, string containerMode = ContainerModes.All, bool isMatched = true, string consolTransportMode = null)
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });

			consolTransportMode = consolTransportMode.IsNullOrEmpty() ? transportMode : consolTransportMode;
			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = consolTransportMode };
			Data.ShipmentDataObject.ContainerMode = new ContainerMode() { Code = containerMode };

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKP";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_Code = "RP";
			relatedParty.MainAddress.OA_Address1 = "related party address";

			bookingParty.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, transportMode, containerMode, null);

			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			Factory.SaveForTesting();

			OrgAddressImportHelper.PopulateTransportOrg(Data.ShipmentDataObject, null, bookingParty, rtu, Logger, Factory);
			Factory.SaveForTesting();

			AssertTransportCompanyAddress(relatedParty.MainAddress.PK, rtu.PK, isMatched);
		}

		#endregion

		#region TestPopulateTransportOrg_ByRelatedParties_MatchByLocation

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByLocation_UNLOCO() => TestPopulateTransportOrg_ByRelatedParties_MatchByLocation("UNLOCO");

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByLocation_Country() => TestPopulateTransportOrg_ByRelatedParties_MatchByLocation("Country");

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByLocation_Empty() => TestPopulateTransportOrg_ByRelatedParties_MatchByLocation("Empty");

		public void TestPopulateTransportOrg_ByRelatedParties_MatchByLocation_NoMatched() => TestPopulateTransportOrg_ByRelatedParties_MatchByLocation("NoMatched");

		void TestPopulateTransportOrg_ByRelatedParties_MatchByLocation(string matchCase)
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });
			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Sea };
			Data.ShipmentDataObject.ContainerMode = new ContainerMode() { Code = ContainerModes.FCL };

			var warehouseUnloco = Data.Warehouse.WarehouseAddress.Header.UNLOCO;

			var anotherUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			anotherUNLOCO.RL_RN_NKCountryCode = "XX";
			anotherUNLOCO.RL_Code = "XX111";

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKP";

			var relatedPartyUNLOCOMatched = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyUNLOCOMatched.OH_Code = "RP1";
			relatedPartyUNLOCOMatched.MainAddress.OA_Address1 = "related party address 1";

			var relatedPartyCountryMatched = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyCountryMatched.OH_Code = "RP2";
			relatedPartyCountryMatched.MainAddress.OA_Address1 = "related party address 2";

			var relatedPartyUNLOCOAndCountryNotMatched = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyUNLOCOAndCountryNotMatched.OH_Code = "RP3";
			relatedPartyUNLOCOAndCountryNotMatched.MainAddress.OA_Address1 = "related party address 3";

			var relatedPartyLocationIsEmpty = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyLocationIsEmpty.OH_Code = "RP4";
			relatedPartyLocationIsEmpty.MainAddress.OA_Address1 = "related party address 4";

			var addressPK = bookingParty.PK;
			if (matchCase == "UNLOCO")
			{
				bookingParty.SetRelatedParty(relatedPartyUNLOCOMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, warehouseUnloco.Code);
				bookingParty.SetRelatedParty(relatedPartyCountryMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, warehouseUnloco.RL_RN_NKCountryCode);
				bookingParty.SetRelatedParty(relatedPartyUNLOCOAndCountryNotMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, anotherUNLOCO.Code);
				bookingParty.SetRelatedParty(relatedPartyLocationIsEmpty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, string.Empty);
				addressPK = relatedPartyUNLOCOMatched.MainAddress.PK;
			}
			else if (matchCase == "Country")
			{
				bookingParty.SetRelatedParty(relatedPartyCountryMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, warehouseUnloco.RL_RN_NKCountryCode);
				bookingParty.SetRelatedParty(relatedPartyUNLOCOAndCountryNotMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, anotherUNLOCO.Code);
				bookingParty.SetRelatedParty(relatedPartyLocationIsEmpty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, string.Empty);
				addressPK = relatedPartyCountryMatched.MainAddress.PK;
			}
			else if (matchCase == "Empty")
			{
				bookingParty.SetRelatedParty(relatedPartyUNLOCOAndCountryNotMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, anotherUNLOCO.Code);
				bookingParty.SetRelatedParty(relatedPartyLocationIsEmpty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, string.Empty);
				addressPK = relatedPartyLocationIsEmpty.MainAddress.PK;
			}
			else
			{
				bookingParty.SetRelatedParty(relatedPartyUNLOCOAndCountryNotMatched.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, anotherUNLOCO.Code);
			}

			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			Factory.SaveForTesting();

			OrgAddressImportHelper.PopulateTransportOrg(Data.ShipmentDataObject, null, bookingParty, rtu, Logger, Factory);
			Factory.SaveForTesting();

			AssertTransportCompanyAddress(addressPK, rtu.PK, addressPK != bookingParty.PK);
		}

		#endregion

		#region TestPopulateTransportOrg_ByRelatedParties_FallbackToWarehouseRelatedParties

		public void TestPopulateTransportOrg_ByRelatedParties_FallbackToWarehouseRelatedParties_Yes() => TestPopulateTransportOrg_ByRelatedParties_FallbackToWarehouseRelatedParties(true);

		public void TestPopulateTransportOrg_ByRelatedParties_FallbackToWarehouseRelatedParties_No() => TestPopulateTransportOrg_ByRelatedParties_FallbackToWarehouseRelatedParties(false);

		void TestPopulateTransportOrg_ByRelatedParties_FallbackToWarehouseRelatedParties(bool shouldFallback)
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });
			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Air };

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKP";

			var bookingPartyRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingPartyRelatedParty.OH_Code = "BPRP";
			bookingPartyRelatedParty.MainAddress.OA_Address1 = "related party address 1";

			var warehouseRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			warehouseRelatedParty.OH_Code = "WRP";
			warehouseRelatedParty.MainAddress.OA_Address1 = "related party address 2";

			var addressPK = warehouseRelatedParty.MainAddress.PK;
			if (shouldFallback)
			{
				bookingParty.SetRelatedParty(bookingPartyRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL);
			}
			else
			{
				bookingParty.SetRelatedParty(bookingPartyRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Air, ContainerModes.All);
				addressPK = bookingPartyRelatedParty.MainAddress.PK;
			}
			Data.Warehouse.WarehouseAddress.Header.SetRelatedParty(warehouseRelatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Air, ContainerModes.All);

			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			Factory.SaveForTesting();

			OrgAddressImportHelper.PopulateTransportOrg(Data.ShipmentDataObject, null, bookingParty, rtu, Logger, Factory);
			Factory.SaveForTesting();
			AssertTransportCompanyAddress(addressPK, rtu.PK, true);
		}

		#endregion

		void AssertTransportCompanyAddress(ZGuid addressPK, ZGuid parentPK, bool isMatched = true)
		{
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, parentPK);
			query = query.AddToFilter(new ZQuery(JobDocAddressSchema.E2_AddressType, "TRA"));

			var transportCompanies = Factory.Load<JobDocAddress>(query);
			if (isMatched)
			{
				AssertEquals(1, transportCompanies.Length);

				var transportCompany = transportCompanies[0];
				AssertNotNull(transportCompany);
				AssertEquals(transportCompany.E2_OA_Address, addressPK);
			}
			else
			{
				AssertEquals(0, transportCompanies.Length);
			}
		}
	}
}
