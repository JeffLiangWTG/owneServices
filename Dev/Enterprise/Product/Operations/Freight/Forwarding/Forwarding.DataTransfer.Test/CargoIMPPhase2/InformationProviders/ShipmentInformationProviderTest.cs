using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ShipmentInformationProviderTest : InformationProviderTest
	{
		public void TestForwarder()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZString.Empty, provider.Forwarder.Value);
			AssertEquals(string.Format("'{0}' registry item", ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.Caption), provider.Forwarder.Name);
			AssertEquals(null, provider.Forwarder.BusinessEntity);

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FID");
			AssertEquals("FID", provider.Forwarder.Value);
		}

		public void TestHouseBillId()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZString.Empty, provider.HouseBillId.Value);
			AssertEquals(shipment.JS_HouseBillInfo.HumanReadableName, provider.HouseBillId.Name);
			AssertEquals(shipment, provider.HouseBillId.BusinessEntity);

			shipment.JS_HouseBill = "1111";
			AssertEquals("1111", provider.HouseBillId.Value);
		}

		public void TestHouseBillDate()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_E_DEP = ZDateTime.Now.AddDays(1);
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(shipment.JS_SystemCreateTimeUtc, provider.HouseBillDate.Value);
			AssertEquals(shipment.JS_SystemCreateTimeUtcInfo.HumanReadableName, provider.HouseBillDate.Name);
			AssertEquals(shipment, provider.HouseBillDate.BusinessEntity);

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2UseETDForHouseBillDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(shipment.JS_E_DEP, provider.HouseBillDate.Value);
			AssertEquals(shipment.JS_E_DEPInfo.HumanReadableName, provider.HouseBillDate.Name);
			AssertEquals(shipment, provider.HouseBillDate.BusinessEntity);
		}

		public void TestHouseBillOrigin()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("SYD", provider.HouseBillOrigin.Value);
			AssertEquals(shipment.JS_RL_NKOriginInfo.HumanReadableName, provider.HouseBillOrigin.Name);
			AssertEquals(shipment, provider.HouseBillOrigin.BusinessEntity);

			shipment.JS_RL_NKOrigin = "UACHE";
			AssertEquals("UACHE", provider.HouseBillOrigin.Value);
		}

		public void TestHouseBillDestination()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("SYD", provider.HouseBillDestination.Value);
			AssertEquals(shipment.JS_RL_NKDestinationInfo.HumanReadableName, provider.HouseBillDestination.Name);
			AssertEquals(shipment, provider.HouseBillDestination.BusinessEntity);

			shipment.JS_RL_NKDestination = "UACHE";
			AssertEquals("UACHE", provider.HouseBillDestination.Value);
		}

		public void TestPieces()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_OuterPacks = 10;
			AssertEquals(10, provider.Pieces.Value);
			AssertEquals(shipment.JS_OuterPacksInfo.HumanReadableName, provider.Pieces.Name);
			AssertEquals(shipment, provider.Pieces.BusinessEntity);

			shipment.JS_OuterPacks = 20;
			AssertEquals(20, provider.Pieces.Value);
		}

		public void TestWeight()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_ActualWeight = 10.5M;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(10.5M, provider.Weight.Value);
			AssertEquals(shipment.JS_ActualWeightInfo.HumanReadableName, provider.Weight.Name);
			AssertEquals(shipment, provider.Weight.BusinessEntity);

			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			AssertEquals(10.5M, provider.Weight.Value);

			shipment.JS_ActualWeight = 10500M;
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			AssertEquals(10.5M, provider.Weight.Value);
		}

		public void TestWeightUnit()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AssertEquals(WeightUnits.Kilograms, provider.WeightUnit.Value);
			AssertEquals(shipment.JS_UnitOfWeightInfo.HumanReadableName, provider.WeightUnit.Name);
			AssertEquals(shipment, provider.WeightUnit.BusinessEntity);

			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			AssertEquals(WeightUnits.Pounds, provider.WeightUnit.Value);

			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			AssertEquals(WeightUnits.Kilograms, provider.WeightUnit.Value);
		}

		public void TestVolume()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_ActualVolume = 10.5M;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals(10.5M, provider.Volume.Value);
			AssertEquals(shipment.JS_ActualVolumeInfo.HumanReadableName, provider.Volume.Name);
			AssertEquals(shipment, provider.Volume.BusinessEntity);

			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			AssertEquals(10.5M, provider.Volume.Value);

			shipment.JS_ActualVolume = 10500M;
			shipment.JS_UnitOfVolume = Constants.Volume.Litre;
			AssertEquals(10.5M, provider.Volume.Value);
		}

		public void TestVolumeUnit()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			AssertEquals(VolumeUnits.CubicMetres, provider.VolumeUnit.Value);
			AssertEquals(shipment.JS_UnitOfVolumeInfo.HumanReadableName, provider.VolumeUnit.Name);
			AssertEquals(shipment, provider.VolumeUnit.BusinessEntity);

			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			AssertEquals(VolumeUnits.CubicFeet, provider.VolumeUnit.Value);

			shipment.JS_UnitOfVolume = Constants.Volume.Litre;
			AssertEquals(VolumeUnits.CubicMetres, provider.VolumeUnit.Value);
		}

		public void TestProductCode()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZString.Empty, provider.ProductCode.Value);
			AssertEquals("", provider.ProductCode.Name);
			AssertEquals(null, provider.ProductCode.BusinessEntity);
		}

		public void TestServiceCode()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZString.Empty, provider.ServiceCode.Value);
			AssertEquals("", provider.ServiceCode.Name);
			AssertEquals(null, provider.ServiceCode.BusinessEntity);
		}

		public void TestHandlingCode1()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZString.Empty, provider.HandlingCode1.Value);
			AssertEquals("", provider.HandlingCode1.Name);
			AssertEquals(null, provider.HandlingCode1.BusinessEntity);
		}

		public void TestHandlingCode2()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZString.Empty, provider.HandlingCode2.Value);
			AssertEquals("", provider.HandlingCode2.Name);
			AssertEquals(null, provider.HandlingCode2.BusinessEntity);
		}

		public void TestTransports()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			var provider = new ShipmentInformationProvider(shipment);
			var transports = provider.Transports;
			AssertEquals(0, transports.Count);

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			consol1.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "NZAKL";
			shipment.Consols.Add(consol1);
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "NZAKL";
			consol2.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "HKHKG";
			shipment.Consols.Add(consol2);

			transports = provider.Transports;
			AssertEquals(2, transports.Count);
			AssertEquals("SYD", transports[0].DepartureLocation.Value);
			AssertEquals("AKL", transports[1].DepartureLocation.Value);
		}

		public void TestReferences()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			var references = provider.References;
			AssertEquals(1, references.Count);
			AssertEquals(ShipmentReferenceTypes.JobNumber, references[0].ReferenceType.Value);
			AssertEquals("Reference Number Type", references[0].ReferenceType.Name);
			AssertEquals(shipment.JS_UniqueConsignRef, references[0].Reference.Value);
			AssertEquals(shipment.JS_UniqueConsignRefInfo.HumanReadableName, references[0].Reference.Name);
			AssertEquals(shipment, references[0].Reference.BusinessEntity);
		}

		public void TestInterestedParties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			var parties = provider.InterestedParties;
			AssertEquals(0, parties.Count);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "CODE3";

			shipment.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org3.PK;

			parties = provider.InterestedParties;
			AssertEquals(3, parties.Count);
			AssertEquals("CODE1", parties[0].Id.Value);
			AssertEquals("Consignor", parties[0].Id.Name);
			AssertEquals(shipment, parties[0].Id.BusinessEntity);
			AssertEquals(InterestedPartyTypes.Consignor, parties[0].Type.Value);
			AssertEquals("Party Type", parties[0].Type.Name);

			AssertEquals("CODE2", parties[1].Id.Value);
			AssertEquals("Consignee", parties[1].Id.Name);
			AssertEquals(shipment, parties[1].Id.BusinessEntity);
			AssertEquals(InterestedPartyTypes.Consignee, parties[1].Type.Value);
			AssertEquals("Party Type", parties[1].Type.Name);

			AssertEquals("CODE3", parties[2].Id.Value);
			AssertEquals("Notify Party", parties[2].Id.Name);
			AssertEquals(shipment, parties[2].Id.BusinessEntity);
			AssertEquals(InterestedPartyTypes.NotifyParty, parties[2].Type.Value);
			AssertEquals("Party Type", parties[2].Type.Name);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			parties = provider.InterestedParties;
			AssertEquals(0, parties.Count);
		}

		public void TestConsignor()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.Consignor.Value);
			AssertEquals("Consignor", provider.Consignor.Name);
			AssertEquals(shipment, provider.Consignor.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "Wxyz_\\.90^/-";
			shipment.ConsignorPK = org.PK;
			AssertEquals("WXYZ.90/-", provider.Consignor.Value);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("", provider.Consignor.Value);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
			shipment.ConsignorPK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("", provider.Consignor.Value);
		}

		public void TestConsignee()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.Consignee.Value);
			AssertEquals("Consignee", provider.Consignee.Name);
			AssertEquals(shipment, provider.Consignee.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.ConsigneePK = org.PK;
			AssertEquals("OCODE", provider.Consignee.Value);

			shipment.ConsigneePK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("", provider.Consignee.Value);
		}

		public void TestNotifyParty()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.NotifyParty.Value);
			AssertEquals("Notify Party", provider.NotifyParty.Name);
			AssertEquals(shipment, provider.NotifyParty.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("OCODE", provider.NotifyParty.Value);

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.NotifyParty.Value);
		}

		public void TestPickupFrom()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.PickupFrom.Value);
			AssertEquals("Consignor Pickup Address", provider.PickupFrom.Name);
			AssertEquals(shipment, provider.PickupFrom.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			org.OH_RL_NKClosestPort = "AUSYD";
			shipment.ConsignorPickupAddress.OrganisationPK = org.PK;
			AssertEquals("SYD", provider.PickupFrom.Value);

			shipment.ConsignorPickupAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("", provider.PickupFrom.Value);
		}

		public void TestEstimatedPickupDate()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZDateTime.Empty, provider.EstimatedPickupDate.Value);
			AssertEquals(shipment.DocsAndCartage.JP_EstimatedPickupInfo.HumanReadableName, provider.EstimatedPickupDate.Name);
			AssertEquals(shipment, provider.EstimatedPickupDate.BusinessEntity);

			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Now;
			AssertEquals(shipment.DocsAndCartage.JP_EstimatedPickup, provider.EstimatedPickupDate.Value);
		}

		public void TestPickupCustomer()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.PickupCustomer.Value);
			AssertEquals("Pickup From", provider.PickupCustomer.Name);
			AssertEquals(null, provider.PickupCustomer.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			AssertEquals("OCODE", provider.PickupCustomer.Value);
			AssertEquals("Consignor", provider.PickupCustomer.Name);
			AssertEquals(shipment, provider.PickupCustomer.BusinessEntity);
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsignorPickupAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("OCODE", provider.PickupCustomer.Value);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			shipment.ConsignorPickupAddress.OrganisationPK = org2.PK;
			AssertEquals("OCODE1", provider.PickupCustomer.Value);
			AssertEquals("Consignor Pickup Address", provider.PickupCustomer.Name);
			AssertEquals(shipment, provider.PickupCustomer.BusinessEntity);
		}

		public void TestPickupParty()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.PickupParty.Value);
			AssertEquals(shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo.HumanReadableName, provider.PickupParty.Name);
			AssertEquals(shipment, provider.PickupParty.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK;
			AssertEquals("OCODE", provider.PickupParty.Value);

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.PickupParty.Value);
		}

		public void TestExportWarehouse()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals("MEL", provider.ExportWarehouse.Value);
			AssertEquals(shipment.JS_RL_NKOriginInfo.HumanReadableName, provider.ExportWarehouse.Name);
			AssertEquals(shipment, provider.ExportWarehouse.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			org.OH_RL_NKClosestPort = "AUSYD";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_PackDepotAddress = org.MainAddress.PK;
			shipment.Consols.Add(consol);
			AssertEquals("SYD", provider.ExportWarehouse.Value);
			AssertEquals(consol.JK_OA_PackDepotAddressInfo.HumanReadableName, provider.ExportWarehouse.Name);
			AssertEquals(consol, provider.ExportWarehouse.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			org2.OH_RL_NKClosestPort = "UACHE";
			shipment.JS_OA_ExportReceivingDepot = org2.MainAddress.PK;
			AssertEquals("UACHE", provider.ExportWarehouse.Value);
			AssertEquals(shipment.JS_OA_ExportReceivingDepotInfo.HumanReadableName, provider.ExportWarehouse.Name);
			AssertEquals(shipment, provider.ExportWarehouse.BusinessEntity);

			shipment.JS_OA_ExportReceivingDepot = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("SYD", provider.ExportWarehouse.Value);

			consol.JK_OA_PackDepotAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("MEL", provider.ExportWarehouse.Value);
		}

		public void TestEstimatedExportWarehouseDeliveryDate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZDateTime.Empty, provider.EstimatedExportWarehouseDeliveryDate.Value);
			AssertEquals("Planned Date and Time Of Receipt", provider.EstimatedExportWarehouseDeliveryDate.Name);
			AssertEquals(null, provider.EstimatedExportWarehouseDeliveryDate.BusinessEntity);
		}

		public void TestExportWarehouseCustomer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ExportWarehouseCustomer.Value);
			AssertEquals("Export Warehouse Customer", provider.ExportWarehouseCustomer.Name);
			AssertEquals(null, provider.ExportWarehouseCustomer.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_PackDepotAddress = org.MainAddress.PK;
			shipment.Consols.Add(consol);
			AssertEquals("OCODE", provider.ExportWarehouseCustomer.Value);
			AssertEquals(consol.JK_OA_PackDepotAddressInfo.HumanReadableName, provider.ExportWarehouseCustomer.Name);
			AssertEquals(consol, provider.ExportWarehouseCustomer.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			shipment.JS_OA_ExportReceivingDepot = org2.MainAddress.PK;
			AssertEquals("OCODE1", provider.ExportWarehouseCustomer.Value);
			AssertEquals(shipment.JS_OA_ExportReceivingDepotInfo.HumanReadableName, provider.ExportWarehouseCustomer.Name);
			AssertEquals(shipment, provider.ExportWarehouseCustomer.BusinessEntity);

			shipment.JS_OA_ExportReceivingDepot = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("OCODE", provider.ExportWarehouseCustomer.Value);

			consol.JK_OA_PackDepotAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.ExportWarehouseCustomer.Value);
		}

		public void TestExportWarehouseDeliveryParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ExportWarehouseDeliveryParty.Value);
			AssertEquals(shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo.HumanReadableName, provider.ExportWarehouseDeliveryParty.Name);
			AssertEquals(shipment, provider.ExportWarehouseDeliveryParty.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK;
			AssertEquals("OCODE", provider.ExportWarehouseDeliveryParty.Value);

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.ExportWarehouseDeliveryParty.Value);
		}

		public void TestExportWarehousePickupParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ExportWarehousePickupParty.Value);
			AssertEquals("Export Warehouse Pickup Party", provider.ExportWarehousePickupParty.Name);
			AssertEquals(null, provider.ExportWarehousePickupParty.BusinessEntity);
		}

		public void TestExportingCarriersTerminal()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ExportingCarriersTerminal.Value);
			AssertEquals("Exporting Carrier's Terminal Customer (Departure Consol CTO Address or MAWB Origin Code)", provider.ExportingCarriersTerminal.Name);
			AssertEquals(null, provider.ExportingCarriersTerminal.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols.Add(consol);
			AssertEquals("SYD", provider.ExportingCarriersTerminal.Value);
			AssertEquals(consol.AWBHeader.EH_AWBOriginCodeInfo.HumanReadableName, provider.ExportingCarriersTerminal.Name);
			AssertEquals(consol.AWBHeader, provider.ExportingCarriersTerminal.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			org2.OH_RL_NKClosestPort = "AUMEL";
			consol.JK_OA_DepartureCTOAddress = org2.MainAddress.PK;
			AssertEquals("MEL", provider.ExportingCarriersTerminal.Value);
			AssertEquals(consol.JK_OA_DepartureCTOAddressInfo.HumanReadableName, provider.ExportingCarriersTerminal.Name);
			AssertEquals(consol, provider.ExportingCarriersTerminal.BusinessEntity);

			consol.JK_OA_DepartureCTOAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("SYD", provider.ExportingCarriersTerminal.Value);
		}

		public void TestExportingCarriersTerminalCustomer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ExportingCarriersTerminalCustomer.Value);
			AssertEquals("Consol Departure CTO Address", provider.ExportingCarriersTerminalCustomer.Name);
			AssertEquals(null, provider.ExportingCarriersTerminalCustomer.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			var consol = Factory.New<ForwardingConsol>();
			shipment.Consols.Add(consol);
			consol.JK_OA_DepartureCTOAddress = org.MainAddress.PK;
			AssertEquals("OCODE", provider.ExportingCarriersTerminalCustomer.Value);
			AssertEquals(consol.JK_OA_DepartureCTOAddressInfo.HumanReadableName, provider.ExportingCarriersTerminalCustomer.Name);
			AssertEquals(consol, provider.ExportingCarriersTerminalCustomer.BusinessEntity);

			consol.JK_OA_DepartureCTOAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.ExportingCarriersTerminalCustomer.Value);
		}

		public void TestExportingCarriersTerminalDeliveryParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ExportingCarriersTerminalDeliveryParty.Value);
			AssertEquals("Exporting Carrier's Terminal Delivery Party", provider.ExportingCarriersTerminalDeliveryParty.Name);
			AssertEquals(null, provider.ExportingCarriersTerminalDeliveryParty.BusinessEntity);
		}

		public void TestImportingCarriersTerminal()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ImportingCarriersTerminal.Value);
			AssertEquals("Importing Carrier's Terminal (Arrival Consol CTO Address or MAWB Destination Code)", provider.ImportingCarriersTerminal.Name);
			AssertEquals(null, provider.ImportingCarriersTerminal.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.Consols.Add(consol);
			provider = new ShipmentInformationProvider(shipment);
			AssertEquals("SYD", provider.ImportingCarriersTerminal.Value);
			AssertEquals(consol.AWBHeader.EH_AirportOfDestinationCodeInfo.HumanReadableName, provider.ImportingCarriersTerminal.Name);
			AssertEquals(consol.AWBHeader, provider.ImportingCarriersTerminal.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			org2.OH_RL_NKClosestPort = "AUMEL";
			consol.JK_OA_ArrivalCTOAddress = org2.MainAddress.PK;
			provider = new ShipmentInformationProvider(shipment);
			AssertEquals("MEL", provider.ImportingCarriersTerminal.Value);
			AssertEquals(consol.JK_OA_ArrivalCTOAddressInfo.HumanReadableName, provider.ImportingCarriersTerminal.Name);
			AssertEquals(consol, provider.ImportingCarriersTerminal.BusinessEntity);

			consol.JK_OA_ArrivalCTOAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("SYD", provider.ImportingCarriersTerminal.Value);
		}

		public void TestImportWarehouse()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("MEL", provider.ImportWarehouse.Value);
			AssertEquals(shipment.JS_RL_NKDestinationInfo.HumanReadableName, provider.ImportWarehouse.Name);
			AssertEquals(shipment, provider.ImportWarehouse.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			org.OH_RL_NKClosestPort = "AUSYD";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;
			shipment.Consols.Add(consol);
			AssertEquals("SYD", provider.ImportWarehouse.Value);
			AssertEquals(consol.JK_OA_UnpackDepotAddressInfo.HumanReadableName, provider.ImportWarehouse.Name);
			AssertEquals(consol, provider.ImportWarehouse.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			org2.OH_RL_NKClosestPort = "UACHE";
			shipment.JS_OA_ImportReleaseDepot = org2.MainAddress.PK;
			AssertEquals("UACHE", provider.ImportWarehouse.Value);
			AssertEquals(shipment.JS_OA_ImportReleaseDepotInfo.HumanReadableName, provider.ImportWarehouse.Name);
			AssertEquals(shipment, provider.ImportWarehouse.BusinessEntity);

			org2.MainAddress.OA_RL_NKRelatedPortCode = "UAIEV";
			AssertEquals("IEV", provider.ImportWarehouse.Value);

			shipment.JS_OA_ImportReleaseDepot = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("SYD", provider.ImportWarehouse.Value);
		}

		public void TestEstimatedImportWarehousePickupDate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZDateTime.Empty, provider.EstimatedImportWarehousePickupDate.Value);
			AssertEquals("Estimated Import Warehouse Pickup Date", provider.EstimatedImportWarehousePickupDate.Name);
			AssertEquals(null, provider.EstimatedImportWarehousePickupDate.BusinessEntity);
		}

		public void TestImportWarehouseCustomer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ImportWarehouseCustomer.Value);
			AssertEquals("Import Warehouse Customer", provider.ImportWarehouseCustomer.Name);
			AssertEquals(null, provider.ImportWarehouseCustomer.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;
			shipment.Consols.Add(consol);
			AssertEquals("OCODE", provider.ImportWarehouseCustomer.Value);
			AssertEquals(consol.JK_OA_UnpackDepotAddressInfo.HumanReadableName, provider.ImportWarehouseCustomer.Name);
			AssertEquals(consol, provider.ImportWarehouseCustomer.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			shipment.JS_OA_ImportReleaseDepot = org2.MainAddress.PK;
			AssertEquals("OCODE1", provider.ImportWarehouseCustomer.Value);
			AssertEquals(shipment.JS_OA_ImportReleaseDepotInfo.HumanReadableName, provider.ImportWarehouseCustomer.Name);
			AssertEquals(shipment, provider.ImportWarehouseCustomer.BusinessEntity);

			shipment.JS_OA_ImportReleaseDepot = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("OCODE", provider.ImportWarehouseCustomer.Value);

			consol.JK_OA_UnpackDepotAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.ImportWarehouseCustomer.Value);
		}

		public void TestImportWarehouseDeliveryParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ImportWarehouseDeliveryParty.Value);
			AssertEquals("Import Warehouse Delivery Party", provider.ImportWarehouseDeliveryParty.Name);
			AssertEquals(null, provider.ImportWarehouseDeliveryParty.BusinessEntity);
		}

		public void TestImportWarehousePickupParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.ImportWarehousePickupParty.Value);
			AssertEquals(shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.HumanReadableName, provider.ImportWarehousePickupParty.Name);
			AssertEquals(shipment, provider.ImportWarehousePickupParty.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK;
			AssertEquals("OCODE", provider.ImportWarehousePickupParty.Value);

			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.ImportWarehousePickupParty.Value);
		}

		public void TestDeliverTo()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.DeliverTo.Value);
			AssertEquals("Consignee Delivery Address", provider.DeliverTo.Name);
			AssertEquals(shipment, provider.DeliverTo.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			org.OH_RL_NKClosestPort = "AUSYD";

			shipment.ConsigneeDeliveryAddress.OrganisationPK = org.PK;
			AssertEquals("SYD", provider.DeliverTo.Value);

			shipment.ConsigneeDeliveryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("", provider.DeliverTo.Value);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("Destination", provider.DeliverTo.Name);
			AssertEquals("AKL", provider.DeliverTo.Value);
		}

		public void TestEstimatedDeliveryDate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals(ZDateTime.Empty, provider.EstimatedDeliveryDate.Value);
			AssertEquals(shipment.DocsAndCartage.JP_EstimatedDeliveryInfo.HumanReadableName, provider.EstimatedDeliveryDate.Name);
			AssertEquals(shipment, provider.EstimatedDeliveryDate.BusinessEntity);

			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;
			AssertEquals(shipment.DocsAndCartage.JP_EstimatedDelivery, provider.EstimatedDeliveryDate.Value);
		}

		public void TestDeliverToCustomer()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentInformationProvider provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.DeliverToCustomer.Value);
			AssertEquals("Deliver To", provider.DeliverToCustomer.Name);
			AssertEquals(null, provider.DeliverToCustomer.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			AssertEquals("OCODE", provider.DeliverToCustomer.Value);
			AssertEquals("Consignee", provider.DeliverToCustomer.Name);
			AssertEquals(shipment, provider.DeliverToCustomer.BusinessEntity);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OCODE1";
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org2.PK;
			AssertEquals("OCODE1", provider.DeliverToCustomer.Value);
			AssertEquals("Consignee Delivery Address", provider.DeliverToCustomer.Name);
			AssertEquals(shipment, provider.DeliverToCustomer.BusinessEntity);

			shipment.ConsigneeDeliveryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("OCODE", provider.DeliverToCustomer.Value);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertEquals("", provider.DeliverToCustomer.Value);
		}

		public void TestDeliverToDeliveryParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = new ShipmentInformationProvider(shipment);
			AssertEquals("", provider.DeliverToDeliveryParty.Value);
			AssertEquals(shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.HumanReadableName, provider.DeliverToDeliveryParty.Name);
			AssertEquals(shipment, provider.DeliverToDeliveryParty.BusinessEntity);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "OCODE";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK;
			AssertEquals("OCODE", provider.DeliverToDeliveryParty.Value);

			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("", provider.DeliverToDeliveryParty.Value);
		}
	}
}
