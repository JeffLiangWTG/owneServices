using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	static class CresaTestHelper
	{
		#region Forwarding

		public static ForwardingShipment CreateShipment(BusinessObjectFactory factory)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_UnitOfVolume = "D3";

			var deliveryOrderReceiptNote = shipment.Notes.AddNew();
			deliveryOrderReceiptNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description;
			deliveryOrderReceiptNote.ST_NoteText = "Delivery Order Receipt Note";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_ExportRefNumber = "AMRUT57%";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";
			packline1.JL_PackLineId = "FRCRESA0000001";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_ExportRefNumber = "AMRUT43%";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";
			packline2.JL_PackLineId = "FRCRESA0000002";

			// No Detailed Description
			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 300;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 225;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 5;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-50";
			packline3.JL_ExportRefNumber = "AMRUT50%";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers3";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 50%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";
			packline3.JL_PackLineId = "FRCRESA0000003";

			// No Detailed Description, No Description, No Marks And Numbers
			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 2;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 200;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 150;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_Length = 10;
			packline4.JL_Width = 5;
			packline4.JL_Height = 3;
			packline4.JL_UnitOfDimension = "M";
			packline4.JL_HarmonisedCode = "WHISKY";
			packline4.JL_RefNumber = "AMR-64";
			packline4.JL_ExportRefNumber = "AMRUT64%";
			packline4.JL_LastKnownTransitWarehouseStatus = "RCV";
			packline4.JL_PackLineId = "FRCRESA0000004";

			var contact = factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			PopulateShipmentAddresses(factory, shipment);

			factory.Save();
			return shipment;
		}

		public static ForwardingShipment PopulateRequiredOrgCodes(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			AddOrgCode(GlbBranch.CurrentBranch.OrgProxy.MainAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");

			if (shipment.ExportReceivingDepot is OrgAddress exportReceivingDepotAddress)
			{
				AddOrgCode(exportReceivingDepotAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
				AddOrgCode(exportReceivingDepotAddress, OrgCusCode.CodeTypes.PortSystemNumber, "001\\ZZZ");
				AddOrgCode(exportReceivingDepotAddress, OrgCusCode.CodeTypes.PortServiceReference, "002");
			}

			if (shipment.DocsAndCartage.PickupCartageCoAddr is OrgAddress pickupAddress)
			{
				AddOrgCode(pickupAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			}

			return shipment;
		}

		static void PopulateShipmentAddresses(BusinessObjectFactory factory, ForwardingShipment shipment)
		{
			var cfs = factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRMAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, cfs.OH_RL_NKClosestPort);

			var unloco = factory.LoadTop1<RefUNLOCO>(query);
			unloco.RefLocoMaps.DeleteAll();

			var mapFRSoget = unloco.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = "MGI";

			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var consignor = factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "3023";
			consignee.MainAddress.State = "VIC";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var consignorPickupAddress = factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "consignor Pickup org";
			consignorPickupAddress.OH_RL_NKClosestPort = "CNBZN";
			consignorPickupAddress.MainAddress.Address1 = "Unit 645";
			consignorPickupAddress.MainAddress.Address2 = "234 Drive";
			consignorPickupAddress.MainAddress.City = "unknown city";
			consignorPickupAddress.MainAddress.Postcode = "3243";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "consignee delivery org";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGJUR";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 563";
			consigneeDeliveryAddress.MainAddress.Address2 = "435 Drive";
			consigneeDeliveryAddress.MainAddress.City = "unknown city";
			consigneeDeliveryAddress.MainAddress.Postcode = "4356";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var pickupAgent = factory.New<OrgHeader>();
			pickupAgent.OH_FullName = "pickup agent org";
			pickupAgent.OH_RL_NKClosestPort = "FRNCE";
			pickupAgent.MainAddress.Address1 = "Unit 283";
			pickupAgent.MainAddress.Address2 = "283 Drive";
			pickupAgent.MainAddress.City = "unknown city";
			pickupAgent.MainAddress.Postcode = "2836";
			pickupAgent.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.PickupAgentDocumentaryAddress.E2_OA_Address = pickupAgent.MainAddress.PK;

			var docsAndCartage = shipment.DocsAndCartage;
			docsAndCartage.JP_OA_PickupCartageCoAddr = pickupAgent.MainAddress.PK;
		}

		#endregion

		#region TransitWarehouse

		public static WhsItemReceiveConsignment CreateReceiveConsignment(BusinessObjectFactory factory)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var helper = new WhsTransitTestHelper(factory);

			var warehouse = helper.CreateTRWWarehouse("TWH");

			var consignment = helper.CreateReceiveConsignment("RC1", warehouse.PK);
			consignment.WRC_RS_NKServiceLevel = "STD";
			consignment.WRC_RL_NKNextDischargePort = "NLAMS";

			var consignee = CreateOrganisation(factory, "CNE Org", "FRPRS", "52 Florence", "St Clair CT", "Paris", "12121", string.Empty, "FR");
			var consignor = CreateOrganisation(factory, "CNR Org", "DKAAL", "Unit 13", "4 Lost Lane", "Aalborg", "2000", string.Empty, "DK");
			var bookingParty = CreateOrganisation(factory, "BKP Org", "AUSYD", "ABC Forwarder", "Unit 399", "Sydney", "2050", "NSW", "AU");
			var warehouseOrg = CreateOrganisation(factory, "TW Org", "AUSYD", "ABC TW Org", "Unit 888", "Carlingford", "3002", "VIC", "AU");

			consignment.Warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
			consignment.BookingPartyDocAddress.OrganisationPK = bookingParty.PK;
			consignment.ConsignorDocAddress.OrganisationPK = consignor.PK;
			consignment.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			var rcu1 = helper.CreateReceiveTransportationUnit("0001", warehouse.PK, warehouse.DefaultLocation.PK);
			rcu1.WRH_GateInTime = ZDateTimeOffset.Today;
			var cargoReceipt = helper.CreatePackageState(consignment, 1, "PLT", "PKG1", "ARV", rcu1, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			cargoReceipt.Package.KP_GoodsDescription = "PKG1 - Description";

			var rcu2 = helper.CreateReceiveTransportationUnit("0002", warehouse.PK, warehouse.DefaultLocation.PK);
			var goodsInDateTime = helper.CreatePackageState(consignment, 1, "PLT", "PKG2", "ARV", rcu2, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			goodsInDateTime.WPS_UnloadedTime = ZDateTimeOffset.Now;
			goodsInDateTime.Package.KP_GoodsDescription = "PKG2 - Description";

			factory.Save();

			return consignment;
		}

		public static WhsItemDispatchConsignment CreateDispatchConsignment(BusinessObjectFactory factory)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var helper = new WhsTransitTestHelper(factory);

			var warehouse = helper.CreateTRWWarehouse("TWH");

			var rcn = helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = helper.CreateDispatchConsignment("DC1", warehouse.PK);
			dcn.WDC_RS_NKServiceLevel = "STD";
			dcn.WDC_RL_NKDestination = "NLAMS";

			var consignee = CreateOrganisation(factory, "CNE Org", "FRPRS", "52 Florence", "St Clair CT", "Paris", "12121", string.Empty, "FR");
			var consignor = CreateOrganisation(factory, "CNR Org", "DKAAL", "Unit 13", "4 Lost Lane", "Aalborg", "2000", string.Empty, "DK");
			var bookingParty = CreateOrganisation(factory, "BKP Org", "AUSYD", "ABC Forwarder", "Unit 399", "Sydney", "2050", "NSW", "AU");
			var warehouseOrg = CreateOrganisation(factory, "TW Org", "AUSYD", "ABC TW Org", "Unit 888", "Carlingford", "3002", "VIC", "AU");

			dcn.Warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
			dcn.BookingPartyDocAddress.OrganisationPK = bookingParty.PK;
			dcn.ConsignorDocAddress.OrganisationPK = consignor.PK;
			dcn.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			var rtu1 = helper.CreateReceiveTransportationUnit("0001", warehouse.PK, warehouse.DefaultLocation.PK);
			rtu1.WRH_GateInTime = ZDateTimeOffset.Today;
			var cargoReceipt = helper.CreatePackageState(rcn, 1, "PLT", "PKG1", "ARV", rtu1, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG", dispatchConsignment: dcn);
			cargoReceipt.Package.KP_GoodsDescription = "PKG1 - Description";

			var rtu2 = helper.CreateReceiveTransportationUnit("0002", warehouse.PK, warehouse.DefaultLocation.PK);
			var goodsInDateTime = helper.CreatePackageState(rcn, 1, "PLT", "PKG2", "ARV", rtu2, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG", dispatchConsignment: dcn);
			goodsInDateTime.WPS_UnloadedTime = ZDateTimeOffset.Now;
			goodsInDateTime.Package.KP_GoodsDescription = "PKG2 - Description";

			factory.Save();

			return dcn;
		}

		public static WhsItemReceiveConsignment PopulateRequiredOrgCodes(this WhsItemReceiveConsignment consignment)
		{
			if (consignment == null)
			{
				return null;
			}

			AddOrgCode(GlbBranch.CurrentBranch.OrgProxy.MainAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");

			if (consignment.Warehouse?.WarehouseAddress is OrgAddress warehouseAddress)
			{
				AddOrgCode(warehouseAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
				AddOrgCode(warehouseAddress, OrgCusCode.FranceCodeTypes.SOW, "sow");
				AddOrgCode(consignment.BookingPartyDocAddress.Address, OrgCusCode.CodeTypes.PortSystemNumber, "001\\ZZZ");
				AddOrgCode(consignment.BookingPartyDocAddress.Address, OrgCusCode.CodeTypes.PortServiceReference, "002");
			}

			if (consignment.BookingPartyDocAddress?.Address is OrgAddress agentAddress)
			{
				AddOrgCode(agentAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			}

			return consignment;
		}

		public static WhsItemDispatchConsignment PopulateRequiredOrgCodes(this WhsItemDispatchConsignment consignment)
		{
			if (consignment == null)
			{
				return null;
			}

			AddOrgCode(GlbBranch.CurrentBranch.OrgProxy.MainAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");

			if (consignment.Warehouse?.WarehouseAddress is OrgAddress warehouseAddress)
			{
				AddOrgCode(warehouseAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
				AddOrgCode(warehouseAddress, OrgCusCode.FranceCodeTypes.SOW, "sow");
				AddOrgCode(consignment.BookingPartyDocAddress.Address, OrgCusCode.CodeTypes.PortSystemNumber, "001\\ZZZ");
				AddOrgCode(consignment.BookingPartyDocAddress.Address, OrgCusCode.CodeTypes.PortServiceReference, "002");
			}

			if (consignment.BookingPartyDocAddress?.Address is OrgAddress agentAddress)
			{
				AddOrgCode(agentAddress, OrgCusCode.FranceCodeTypes.CI5, "ci5");
			}

			return consignment;
		}

		public static Transport CreateTransportRouting(BusinessObjectFactory factory, Type parentType, string parentTypePrefix, ZGuid pk, string voyage, string discPort, string loadPort)
		{
			var transport = factory.New<Transport>();
			transport.ParentType = typeof(WhsItemReceiveConsignment);
			transport.JW_ParentType = parentTypePrefix;
			transport.JW_ParentGUID = pk;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_VoyageFlight = voyage;

			return transport;
		}

		#endregion

		#region Shared

		static OrgCusCode AddOrgCode(OrgAddress address, string type, string code)
		{
			var orgCusCode = address.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = type;
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			orgCusCode.OK_CustomsRegNo = code;

			return orgCusCode;
		}

		static OrgHeader CreateOrganisation(BusinessObjectFactory factory, string fullName, string closestPort, string address1, string address2, string city, string postCode, string state, string countryCode)
		{
			var org = factory.New<OrgHeader>();
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.Address1 = address1;
			org.MainAddress.Address2 = address2;
			org.MainAddress.City = city;
			org.MainAddress.Postcode = postCode;
			org.MainAddress.State = state;
			org.MainAddress.OA_RN_NKCountryCode = countryCode;
			return org;
		}

		#endregion
	}
}
