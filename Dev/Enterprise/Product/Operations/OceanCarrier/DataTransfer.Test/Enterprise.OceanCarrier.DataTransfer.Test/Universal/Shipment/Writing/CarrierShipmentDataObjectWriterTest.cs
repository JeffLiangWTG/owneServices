using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.OceanCarrier.Business;
using Enterprise.OceanCarrier.DataTransfer.Universal.Shipment;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using AutoDocAddressTypes = Enterprise.MasterFiles.Integration.AutoDocAddressTypes.Codes;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Testing
{
	sealed class CarrierShipmentDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Test Cases
		public void TestGetEdiMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ((ITopLevelDataObjectWriter)new CarrierShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		public void TestGetTopLevelDataContextType()
		{
			AssertEquals(DataContextType.CarrierShipment, ((ITopLevelDataObjectWriter)new CarrierShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		public void TestPopulateHeader()
		{
			var shipment = CreateCarrierShipmentHeader("CSH001");

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);

			AssertNotNull("shipmentDataObject", shipmentDataObject);

			#region Check Contents of shipmentData object
			CombineAssertions(() =>
			{
				AssertEquals("shipmentDataObject.BookingConfirmationReference", "CSH001", shipmentDataObject.BookingConfirmationReference);
				AssertEquals("shipmentDataObject.GoodsValue", (ZDecimal)150000, shipmentDataObject.GoodsValue);
				AssertEquals("shipmentDataObject.GoodsValueCurrency.Code", "EUR", shipmentDataObject.GoodsValueCurrency.Code);
				AssertEquals("shipmentDataObject.GoodsValueCurrency.Description", "Euro", shipmentDataObject.GoodsValueCurrency.Description);
				AssertEquals("shipmentDataObject.PlaceOfDelivery.Code", "EGDAM", shipmentDataObject.PlaceOfDelivery.Code);
				AssertEquals("shipmentDataObject.PlaceOfDelivery.Name", "Dumyat (Damietta)", shipmentDataObject.PlaceOfDelivery.Name);
				AssertEquals("shipmentDataObject.PlaceOfReceipt.Code", "AUSYD", shipmentDataObject.PlaceOfReceipt.Code);
				AssertEquals("shipmentDataObject.PlaceOfReceipt.Name", "Sydney", shipmentDataObject.PlaceOfReceipt.Name);
				AssertEquals("shipmentDataObject.PortOfDischarge.Code", "USORF", shipmentDataObject.PortOfDischarge.Code);
				AssertEquals("shipmentDataObject.PortOfDischarge.Name", "Norfolk", shipmentDataObject.PortOfDischarge.Name);
				AssertEquals("shipmentDataObject.PortOfLoading.Code", "DEBRV", shipmentDataObject.PortOfLoading.Code);
				AssertEquals("shipmentDataObject.PortOfLoading.Name", "Bremerhaven", shipmentDataObject.PortOfLoading.Name);
				AssertEquals("shipmentDataObject.PortOfOrigin.Code", "DEBRE", shipmentDataObject.PortOfOrigin.Code);
				AssertEquals("shipmentDataObject.PortOfOrigin.Name", "Bremen", shipmentDataObject.PortOfOrigin.Name);
				AssertEquals("shipmentDataObject.PortOfDestination.Code", "USNYC", shipmentDataObject.PortOfDestination.Code);
				AssertEquals("shipmentDataObject.PortOfDestination.Name", "New York", shipmentDataObject.PortOfDestination.Name);
				AssertEquals("shipmentDataObject.TransportMode.Name", TransportModes.Sea, shipmentDataObject.TransportMode.Code);
				AssertEquals("shipmentDataObject.TransportMode.Description", TransportModeDescriptions.Sea, shipmentDataObject.TransportMode.Description);
				AssertEquals("shipmentDataObject.WayBillNumber", "WB001", shipmentDataObject.WayBillNumber);
				AssertEquals("shipmentDataObject.WayBillType.Code", "SWB", shipmentDataObject.WayBillType.Code);
				AssertEquals("shipmentDataObject.WayBillType.Description", ListHelper.GetDescription("SWB", new WayBillTypeList()), shipmentDataObject.WayBillType.Description);
			});
			#endregion
		}

		public void TestPopulateDataContext()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";
			Factory.SaveForTesting();

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);
			AssertEquals(nameof(DataContextType.CarrierShipment), shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Type);
			AssertEquals("CSH001", shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Key);
		}

		public void TestPopulateParties_WithOrganization()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";

			var bookingPartyOrgHeader = CreateOrgHeader(BookingParty.CompanyCode);
			var bookingPartyOrgAddress = CreateOrgAddress(bookingPartyOrgHeader, BookingParty);
			CreateOrgContact(bookingPartyOrgHeader, BookingParty);
			CreateJobDocAddress(shipment, BookingParty, ZBool.False, bookingPartyOrgAddress.PK);

			var notifyOrgHeader = CreateOrgHeader(NotifyParty.CompanyCode);
			var notifyOrgAddress = CreateOrgAddress(notifyOrgHeader, NotifyParty);
			CreateOrgContact(notifyOrgHeader, NotifyParty);
			CreateJobDocAddress(shipment, NotifyParty, ZBool.False, notifyOrgAddress.PK);

			var consignorOrgHeader = CreateOrgHeader(ConsignorParty.CompanyCode);
			var consignorOrgAddress = CreateOrgAddress(consignorOrgHeader, ConsignorParty);
			CreateOrgContact(consignorOrgHeader, ConsignorParty);
			CreateJobDocAddress(shipment, ConsignorParty, ZBool.False, consignorOrgAddress.PK);

			var consigneeOrgHeader = CreateOrgHeader(ConsigneeParty.CompanyCode);
			var consigneeOrgAddress = CreateOrgAddress(consigneeOrgHeader, ConsigneeParty);
			CreateOrgContact(consigneeOrgHeader, ConsigneeParty);
			CreateJobDocAddress(shipment, ConsigneeParty, ZBool.False, consigneeOrgAddress.PK);
			Factory.SaveForTesting();

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);

			AssertNotNull("shipmentDataObject", shipmentDataObject);
			AssertEquals(nameof(DataContextType.CarrierShipment), shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Type);
			AssertEquals("CSH001", shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Key);
			AssertEquals("CSH001", shipmentDataObject.BookingConfirmationReference);
			AssertParties(shipmentDataObject, BookingParty, ZBool.False);
			AssertParties(shipmentDataObject, ConsigneeParty, ZBool.False);
			AssertParties(shipmentDataObject, ConsignorParty, ZBool.False);
			AssertParties(shipmentDataObject, NotifyParty, ZBool.False);
		}

		public void TestPopulateParties_AddressOverride()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";

			CreateJobDocAddress(shipment, BookingParty, ZBool.True, null);
			CreateJobDocAddress(shipment, NotifyParty, ZBool.True, null);
			CreateJobDocAddress(shipment, ConsignorParty, ZBool.True, null);
			CreateJobDocAddress(shipment, ConsigneeParty, ZBool.True, null);
			Factory.SaveForTesting();

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);

			AssertNotNull("shipmentDataObject", shipmentDataObject);
			AssertEquals(nameof(DataContextType.CarrierShipment), shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Type);
			AssertEquals("CSH001", shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Key);
			AssertEquals("CSH001", shipmentDataObject.BookingConfirmationReference);
			AssertParties(shipmentDataObject, BookingParty, ZBool.True);
			AssertParties(shipmentDataObject, ConsigneeParty, ZBool.True);
			AssertParties(shipmentDataObject, ConsignorParty, ZBool.True);
			AssertParties(shipmentDataObject, NotifyParty, ZBool.True);
		}

		public void TestPopulateContainers()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";

			CreateCommodity(Container1.Commodity, Container1.CommodityDescription);
			var container1 = CreateCarrierShipmentCargoContainer(shipment, Container1);

			CreateCommodity(Container2.Commodity, Container2.CommodityDescription);
			var container2 = CreateCarrierShipmentCargoContainer(shipment, Container2);
			Factory.SaveForTesting();

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);

			AssertNotNull("shipmentDataObject", shipmentDataObject);
			AssertEquals(nameof(DataContextType.CarrierShipment), shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Type);
			AssertEquals("CSH001", shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Key);
			AssertEquals("CSH001", shipmentDataObject.BookingConfirmationReference);

			AssertContainer(shipmentDataObject, container1);
			AssertContainer(shipmentDataObject, container2);
		}

		public void TestPopulatePackingLines_BreakBulk()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";

			CreatePackType(Breakbulk1.PackType, Breakbulk1.PackTypeDescription);
			CreateCommodity(Breakbulk1.Commodity, Breakbulk1.CommodityDescription);
			var breakBulk1 = CreateCarrierShipmentCargoBreakBulk(shipment, Breakbulk1);
			Factory.SaveForTesting();

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);

			AssertNotNull("shipmentDataObject", shipmentDataObject);
			AssertEquals(nameof(DataContextType.CarrierShipment), shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Type);
			AssertEquals("CSH001", shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Key);
			AssertEquals("CSH001", shipmentDataObject.BookingConfirmationReference);

			var packingLineXml = shipmentDataObject.PackingLineCollection.FirstOrDefault(packingLine => packingLine.ReferenceNumber.Equals(breakBulk1.CSC_IdentificationReference));
			AssertNotNull($"Packing line is not found in XML: Reference Number: {breakBulk1.CSC_IdentificationReference}", packingLineXml);
			AssertPackingLine(shipmentDataObject, breakBulk1, packingLineXml);
		}

		public void TestPopulateNestedCargo_ContainerAsParent_BreakBulkAsChild()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";

			CreateCommodity(Container1.Commodity, Container1.CommodityDescription);
			var containerParent = CreateCarrierShipmentCargoContainer(shipment, Container1);

			CreateCommodity(Container2.Commodity, Container2.CommodityDescription);
			var container2 = CreateCarrierShipmentCargoContainer(shipment, Container2);

			CreatePackType(Breakbulk1.PackType, Breakbulk1.PackTypeDescription);
			CreateCommodity(Breakbulk1.Commodity, Breakbulk1.CommodityDescription);
			var breakBulkChild = CreateCarrierShipmentCargoBreakBulk(shipment, Breakbulk1);

			CreateCarrierShipmentCargoLink(containerParent, breakBulkChild);
			Factory.SaveForTesting();

			var writer = new CarrierShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)));
			var shipmentDataObject = writer.GetDataObject(shipment);

			AssertNotNull("shipmentDataObject", shipmentDataObject);
			AssertEquals(nameof(DataContextType.CarrierShipment), shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Type);
			AssertEquals("CSH001", shipmentDataObject.DataContext.DataSourceCollection.FirstOrDefault()?.Key);
			AssertEquals("CSH001", shipmentDataObject.BookingConfirmationReference);

			AssertContainer(shipmentDataObject, containerParent);
			AssertContainer(shipmentDataObject, container2);

			var packingLineXml = shipmentDataObject.PackingLineCollection.FirstOrDefault(packingLine => packingLine.ReferenceNumber.Equals(breakBulkChild.CSC_IdentificationReference));
			AssertNotNull($"Packing line is not found in XML: Reference Number: {breakBulkChild.CSC_IdentificationReference}", packingLineXml);
			AssertPackingLine(shipmentDataObject, breakBulkChild, packingLineXml);
			AssertContainerLink(shipmentDataObject, containerParent, packingLineXml);
		}
		#endregion

		#region Implementation
		CarrierShipmentHeader CreateCarrierShipmentHeader(ZString carrierShipmentReference)
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = carrierShipmentReference;
			shipment.CSH_DeclaredValue = 150000;
			shipment.CSH_RX_NKDeclaredValueCurrency = "EUR";
			shipment.CSH_RL_NKPlaceOfDeliveryCode = "EGDAM";
			shipment.CSH_RL_NKPlaceOfReceiptCode = "AUSYD";
			shipment.CSH_RL_NKPortOfDischargeCode = "USORF";
			shipment.CSH_RL_NKPortOfLoadingCode = "DEBRV";
			shipment.CSH_RL_NKPortOfOriginCode = "DEBRE";
			shipment.CSH_RL_NKPortOfDestinationCode = "USNYC";
			shipment.CSH_RequestedTransportDocumentReference = "WB001";
			shipment.CSH_RequestedTransportDocumentType = "SWB";
			return shipment;
		}

		CarrierShipmentCargo CreateCarrierShipmentCargoContainer(CarrierShipmentHeader carrierShipmentHeader, ContainerData container)
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;
			cargo.CSC_CargoType = ContainerModes.Containerised;
			cargo.CSC_IsTopLevel = ZBool.True;
			cargo.CSC_CargoMovementTypeOrigin = "FCL";
			cargo.CSC_CargoMovementTypeDestination = "FCL";
			cargo.CSC_DeliveryDrayage = "ANY";
			cargo.CSC_ReceiptDrayage = "ANY";
			cargo.CSC_RC_ChargeableEquipmentType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container.EquipmentType).PK;
			cargo.CSC_PieceCount = container.PieceCount;
			cargo.CSC_EquipmentNo = container.EquipmentNumber;
			cargo.CSC_RH_NKCommodityCode = container.Commodity;
			cargo.CSC_DescriptionOfGoods = container.GoodsDescription;
			cargo.CSC_IsEmpty = container.IsEmptyContainer;
			cargo.CSC_IsShipperOwned = container.IsShipperOwned;
			cargo.CSC_ReeferNonOperated = container.IsNonOperating;
			cargo.CSC_CargoWeight = container.GoodsWeight;
			cargo.CSC_UnitOfWeight = container.WeightUnit;
			cargo.CSC_DunnageWeight = container.DunnageWeight;
			cargo.CSC_EquipmentTareWeight = container.TareWeight;

			cargo.CSC_OOGDoor = container.OverhangBack ?? cargo.CSC_OOGDoor;
			cargo.CSC_OOGFront = container.OverhangFront ?? cargo.CSC_OOGFront;
			cargo.CSC_OOGTop = container.OverhangHeight ?? cargo.CSC_OOGTop;
			cargo.CSC_OOGLeft = container.OverhangLeft ?? cargo.CSC_OOGLeft;
			cargo.CSC_OOGRight = container.OverhangRight ?? cargo.CSC_OOGRight;
			cargo.CSC_OOGUnit = container.LengthUnit ?? cargo.CSC_OOGUnit;

			cargo.CSC_SealNumber1 = container.SealNumber1;
			cargo.CSC_SealNumber2 = container.SealNumber2;
			cargo.CSC_SealNumber3 = container.SealNumber3;
			cargo.CSC_SealNumber4 = container.SealNumber4;
			cargo.CSC_SealNumber5 = container.SealNumber5;
			cargo.CSC_SealNumber6 = container.SealNumber6;
			cargo.CSC_SealParty1 = container.SealParty1;
			cargo.CSC_SealParty2 = container.SealParty2;
			cargo.CSC_SealParty3 = container.SealParty3;
			cargo.CSC_VGMWeighingDateTime = container.GrossWeightVerificationDateTime.ToDateTime();
			cargo.CSC_VGMWeighingMethod = container.GrossWeightVerificationType;
			return cargo;
		}

		CarrierShipmentCargo CreateCarrierShipmentCargoBreakBulk(CarrierShipmentHeader carrierShipmentHeader, PackingLineData packingLineData)
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;
			cargo.CSC_CargoType = packingLineData.CargoType;
			cargo.CSC_DeliveryDrayage = "";
			cargo.CSC_ReceiptDrayage = "";
			cargo.CSC_IsTopLevel = ZBool.True;
			cargo.CSC_CargoMovementTypeOrigin = packingLineData.CargoMovementTypeOrigin;
			cargo.CSC_CargoMovementTypeDestination = packingLineData.CargoMovementTypeDestination;
			cargo.CSC_PieceCount = packingLineData.PackQty;
			cargo.CSC_F3_NKPackType = packingLineData.PackType;
			cargo.CSC_RH_NKCommodityCode = packingLineData.Commodity;
			cargo.CSC_DescriptionOfGoods = packingLineData.DescriptionOfGoods;
			cargo.CSC_IdentificationReference = packingLineData.IdentificationReference;
			cargo.CSC_IsStackable = packingLineData.IsStackable;
			cargo.CSC_ChargeableGrossWeight = packingLineData.Weight;
			cargo.CSC_ChargeableGrossWeightUnit = packingLineData.WeightUnit;
			cargo.CSC_ChargeableLength = packingLineData.Length;
			cargo.CSC_ChargeableUnitOfDimension = packingLineData.LengthUnit;
			cargo.CSC_ChargeableWidth = packingLineData.Width;
			cargo.CSC_ChargeableHeight = packingLineData.Height;
			return cargo;
		}

		CarrierShipmentCargoLink CreateCarrierShipmentCargoLink(CarrierShipmentCargo parent, CarrierShipmentCargo child)
		{
			var link = Factory.New<CarrierShipmentCargoLink>();
			link.CCK_CSC_Parent = parent.PK;
			link.CCK_CSC_Child = child.PK;
			child.CSC_IsTopLevel = ZBool.False;
			return link;
		}

		RefCommodityCode CreateCommodity(string code, string description)
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = code;
			commodity.RH_Description = description;
			return commodity;
		}

		RefPackType CreatePackType(string code, string description)
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = code;
			packType.F3_Description = description;
			return packType;
		}

		OrgHeader CreateOrgHeader(string orgHeaderCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = orgHeaderCode;

			return orgHeader;
		}

		OrgAddress CreateOrgAddress(OrgHeader orgHeader, AddressData addressData)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.AddressCode = addressData.CompanyCode;
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_CompanyNameOverride = addressData.CompanyName;
			orgAddress.OA_Address1 = addressData.Address1;
			orgAddress.OA_Address2 = addressData.Address2;
			orgAddress.OA_City = addressData.City;
			orgAddress.OA_PostCode = addressData.PostCode;
			orgAddress.OA_State = addressData.State;
			orgAddress.OA_RN_NKCountryCode = addressData.Country;
			orgAddress.OA_Email = addressData.Email;
			orgAddress.OA_Phone = addressData.Phone;
			return orgAddress;
		}

		OrgContact CreateOrgContact(OrgHeader orgHeader, AddressData addressData)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = orgHeader.PK;
			contact.OC_ContactName = addressData.Contact;
			contact.OC_Email = addressData.Email;
			contact.OC_Phone = addressData.Phone;
			contact.OC_Mobile = addressData.Mobile;
			return contact;
		}

		JobDocAddress CreateJobDocAddress(CarrierShipmentHeader shipment, AddressData addressData, ZBool addressOverride, ZGuid? orgAddressPk)
		{
			var addressBO = Factory.New<JobDocAddress>();

			addressBO.E2_ParentID = shipment.PK;
			addressBO.E2_ParentTableCode = CarrierShipmentHeaderSchema.Constants.Prefix;
			addressBO.E2_AddressType = addressData.AddressType;
			addressBO.E2_AddressOverride = addressOverride;
			addressBO.E2_Contact = addressData.Contact;
			addressBO.E2_Email = addressData.Email;
			addressBO.E2_Phone = addressData.Phone;
			addressBO.E2_Mobile = addressData.Mobile;

			if (addressOverride)
			{
				addressBO.E2_CompanyName = addressData.CompanyName;
				addressBO.E2_Address1 = addressData.Address1;
				addressBO.E2_Address2 = addressData.Address2;
				addressBO.E2_City = addressData.City;
				addressBO.E2_State = addressData.State;
				addressBO.E2_Postcode = addressData.PostCode;
				addressBO.E2_RN_NKCountryCode = addressData.Country;
			}
			else
			{
				if (orgAddressPk.HasValue)
				{
					addressBO.E2_OA_Address = orgAddressPk.Value;
				}
			}

			return addressBO;
		}

		void AssertParties(UniversalShipment shipmentDataObject, AddressData address, ZBool overrideAddress)
		{
			var addressXml = shipmentDataObject.OrganizationAddressCollection.FirstOrDefault(orgAddress => orgAddress.AddressType.Equals(address.AddressTypeDescription));
			AssertNotNull($"{address.AddressTypeDescription} Organization Address not found", addressXml);

			CombineAssertions(() =>
			{
				AssertEquals(address.AddressTypeDescription + ".AddressType", address.AddressTypeDescription,
					addressXml.AddressType);
				AssertEquals(address.AddressTypeDescription + ".AddressOverride", overrideAddress,
					addressXml.AddressOverride);
				AssertEquals(address.AddressTypeDescription + ".CompanyName", address.CompanyName,
					addressXml.CompanyName);
				AssertEquals(address.AddressTypeDescription + ".Address1", address.Address1, addressXml.Address1);
				AssertEquals(address.AddressTypeDescription + ".Address2", address.Address2, addressXml.Address2);
				AssertEquals(address.AddressTypeDescription + ".City", address.City, addressXml.City);
				AssertEquals(address.AddressTypeDescription + ".Postcode", address.PostCode, addressXml.Postcode);
				AssertEquals(address.AddressTypeDescription + ".State.Code", address.State, addressXml.State.Code);
				AssertEquals(address.AddressTypeDescription + ".State.Description", address.StateDescription,
					addressXml.State.Description);
				AssertEquals(address.AddressTypeDescription + ".Country.Code", address.Country,
					addressXml.Country.Code);
				AssertEquals(address.AddressTypeDescription + ".Country.Name", address.CountryName,
					addressXml.Country.Name);
				AssertEquals(address.AddressTypeDescription + ".Contact", address.Contact, addressXml.Contact);
				AssertEquals(address.AddressTypeDescription + ".Email", address.Email, addressXml.Email);
				AssertEquals(address.AddressTypeDescription + ".Phone", address.Phone, addressXml.Phone);
				AssertEquals(address.AddressTypeDescription + ".Mobile", address.Mobile, addressXml.Mobile);

				if (overrideAddress)
				{
					AssertNull(address.AddressTypeDescription + ".OrganizationCode", addressXml.OrganizationCode);
				}
				else
				{
					AssertEquals(address.AddressTypeDescription + ".OrganizationCode", address.CompanyCode,
						addressXml.OrganizationCode);
				}
			});
		}

		void AssertContainer(UniversalShipment shipmentDataObject, CarrierShipmentCargo container)
		{
			var containerXml = shipmentDataObject.ContainerCollection.FirstOrDefault(containerRecord => containerRecord.ContainerNumber.Equals(container.CSC_EquipmentNo));
			AssertNotNull($"Container {container.CSC_EquipmentNo} not found in XML.", containerXml);

			CombineAssertions(() =>
			{
				AssertEquals("Container.PieceCount", container.CSC_PieceCount, containerXml.ContainerCount);
				AssertEquals("Container.EquipmentType.Code", container.ChargeableEquipmentType.RC_Code, containerXml.ContainerType.Code);
				AssertEquals("Container.EquipmentType.Description", container.ChargeableEquipmentType?.RC_Description, containerXml.ContainerType.Description);
				AssertEquals("Container.EquipmentType.ISOCode", container.ChargeableEquipmentType?.RC_ISOType, containerXml.ContainerType.ISOCode);
				AssertEquals("Container.ContainerNumber", container.CSC_EquipmentNo, containerXml.ContainerNumber);
				AssertEquals("Container.Commodity.Code", container.CSC_RH_NKCommodityCode, containerXml.Commodity.Code);
				AssertEquals("Container.Commodity.Description", container.CommodityCode?.RH_Description, containerXml.Commodity.Description);
				AssertEquals("Container.GoodsDescription", container.CSC_DescriptionOfGoods, containerXml.GoodsDescription);
				AssertEquals("Container.IsEmptyContainer", container.CSC_IsEmpty, containerXml.IsEmptyContainer);
				AssertEquals("Container.IsShipperOwned", container.CSC_IsShipperOwned, containerXml.IsShipperOwned);
				AssertEquals("Container.IsNonOperating", container.CSC_ReeferNonOperated, containerXml.IsNonOperating);
				AssertEquals("Container.GoodsWeight", container.CSC_CargoWeight, containerXml.GoodsWeight);
				AssertEquals("Container.WeightUnit.Code", container.CSC_UnitOfWeight, containerXml.WeightUnit.Code);
				AssertEquals("Container.WeightUnit.Description", ListHelper.GetDescription(container.CSC_UnitOfWeight, container.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)), containerXml.WeightUnit.Description);
				AssertEquals("Container.DunnageWeight", container.CSC_DunnageWeight, containerXml.DunnageWeight);
				AssertEquals("Container.TareWeight", container.CSC_EquipmentTareWeight, containerXml.TareWeight);
				AssertEquals("Container.GrossWeight", container.GetTotalCargoGrossWeightMeasure(), containerXml.GrossWeight);
				AssertEquals("Container.OverhangBack", container.CSC_OOGDoor, containerXml.OverhangBack ?? 0);
				AssertEquals("Container.OverhangFront", container.CSC_OOGFront, containerXml.OverhangFront ?? 0);
				AssertEquals("Container.OverhangHeight", container.CSC_OOGTop, containerXml.OverhangHeight ?? 0);
				AssertEquals("Container.OverhangLeft", container.CSC_OOGLeft, containerXml.OverhangLeft ?? 0);
				AssertEquals("Container.OverhangRight", container.CSC_OOGRight, containerXml.OverhangRight ?? 0);
				AssertEquals("Container.LengthUnit.Code", container.CSC_OOGUnit, containerXml.LengthUnit?.Code ?? "");
				AssertEquals("Container.LengthUnit.Description", ListHelper.GetDescription(container.CSC_OOGUnit, container.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length)), containerXml.LengthUnit?.Description);
				AssertEquals("Container.Seal", container.CSC_SealNumber1, containerXml.Seal);
				AssertEquals("Container.SecondSeal", container.CSC_SealNumber2, containerXml.SecondSeal);
				AssertEquals("Container.ThirdSeal", container.CSC_SealNumber3, containerXml.ThirdSeal);
				AssertEquals("Container.SealPartyType.Code", container.CSC_SealParty1, containerXml.SealPartyType.Code);
				AssertEquals("Container.SealPartyType.Description", ListHelper.GetDescription(container.CSC_SealParty1, container.Lookups.SealParty_List), containerXml.SealPartyType.Description);
				AssertEquals("Container.SecondSealPartyType.Code", container.CSC_SealParty2, containerXml.SecondSealPartyType.Code);
				AssertEquals("Container.SecondSealPartyType.Description", ListHelper.GetDescription(container.CSC_SealParty2, container.Lookups.SealParty_List), containerXml.SecondSealPartyType.Description);
				AssertEquals("Container.ThirdSealPartyType.Code", container.CSC_SealParty3, containerXml.ThirdSealPartyType.Code);
				AssertEquals("Container.ThirdSealPartyType.Description", ListHelper.GetDescription(container.CSC_SealParty3, container.Lookups.SealParty_List), containerXml.ThirdSealPartyType.Description);
				AssertEquals("Container.GrossWeightVerificationDateTime", container.CSC_VGMWeighingDateTime.ToDateTime(), containerXml.GrossWeightVerificationDateTime);
				AssertEquals("Container.GrossWeightVerificationType.Code", container.CSC_VGMWeighingMethod, containerXml.GrossWeightVerificationType.Code);
				AssertEquals("Container.GrossWeightVerificationType.Description", ListHelper.GetDescription(container.CSC_VGMWeighingMethod, container.Lookups.GrossWeightVerificationTypeList), containerXml.GrossWeightVerificationType.Description);
				AssertEquals("Container.Link " + container.CSC_EquipmentNo, GetGeneratedContainerLinkFromXmlFile(shipmentDataObject, container.CSC_EquipmentNo), containerXml.Link);
				AssertEquals("Container.SealNumber4", container.CSC_SealNumber4, containerXml.AdditionalSealNumberCollection[0].Number);
				AssertEquals("Container.SealNumber5", container.CSC_SealNumber5, containerXml.AdditionalSealNumberCollection[1].Number);
				AssertEquals("Container.SealNumber6", container.CSC_SealNumber6, containerXml.AdditionalSealNumberCollection[2].Number);
			});
		}

		void AssertPackingLine(UniversalShipment shipmentDataObject, CarrierShipmentCargo carrierShipmentCargo, PackingLine packingLineXml)
		{
			CombineAssertions(() =>
			{
				AssertEquals("PackingLine.PackQty", ZLong.Parse(carrierShipmentCargo.CSC_PieceCount.ToString()), packingLineXml.PackQty);
				AssertEquals("PackingLine.PackType.Code", carrierShipmentCargo.CSC_F3_NKPackType, packingLineXml.PackType.Code);
				AssertEquals("PackingLine.PackType.Description", carrierShipmentCargo.PackType?.F3_Description, packingLineXml.PackType.Description);
				AssertEquals("PackingLine.Commodity.Code", carrierShipmentCargo.CSC_RH_NKCommodityCode, packingLineXml.Commodity.Code);
				AssertEquals("PackingLine.Commodity.Description", carrierShipmentCargo.CommodityCode?.RH_Description, packingLineXml.Commodity.Description);
				AssertEquals("PackingLine.DetailedDescription", carrierShipmentCargo.CSC_DescriptionOfGoods, packingLineXml.DetailedDescription);
				AssertEquals("PackingLine.ReferenceNumber", carrierShipmentCargo.CSC_IdentificationReference, packingLineXml.ReferenceNumber);
				AssertEquals("PackingLine.NonStackable", !carrierShipmentCargo.CSC_IsStackable, packingLineXml.NonStackable);
				AssertEquals("PackingLine.Weight", carrierShipmentCargo.CSC_ChargeableGrossWeight, packingLineXml.Weight);
				AssertEquals("PackingLine.WeightUnit.Code", carrierShipmentCargo.CSC_ChargeableGrossWeightUnit, packingLineXml.WeightUnit.Code);
				AssertEquals("PackingLine.WeightUnit.Description", ListHelper.GetDescription(carrierShipmentCargo.CSC_ChargeableGrossWeightUnit, carrierShipmentCargo.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)), packingLineXml.WeightUnit.Description);
				AssertEquals("PackingLine.Length", carrierShipmentCargo.CSC_ChargeableLength, packingLineXml.Length);
				AssertEquals("PackingLine.LengthUnit.Code", carrierShipmentCargo.CSC_ChargeableUnitOfDimension, packingLineXml.LengthUnit.Code);
				AssertEquals("PackingLine.LengthUnit.Description", ListHelper.GetDescription(carrierShipmentCargo.CSC_ChargeableUnitOfDimension, carrierShipmentCargo.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length)), packingLineXml.LengthUnit.Description);
				AssertEquals("PackingLine.Width", carrierShipmentCargo.CSC_ChargeableWidth, packingLineXml.Width);
				AssertEquals("PackingLine.Height", carrierShipmentCargo.CSC_ChargeableHeight, packingLineXml.Height);
			});
		}

		void AssertContainerLink(UniversalShipment shipmentDataObject, CarrierShipmentCargo containerParent, PackingLine packingLineXml)
		{
			CombineAssertions(() =>
			{
				var containerLink = GetGeneratedContainerLinkFromXmlFile(shipmentDataObject, containerParent.CSC_EquipmentNo);
				AssertEquals("PackingLine.ContainerLink", containerLink, packingLineXml.ContainerLink);
				AssertEquals("PackingLine.ContainerNumber", containerParent.CSC_EquipmentNo, packingLineXml.ContainerNumber);
			});
		}

		ZInt? GetGeneratedContainerLinkFromXmlFile(UniversalShipment shipmentDataObject, ZString equipmentNumber)
		{
			var containerXml = shipmentDataObject.ContainerCollection.FirstOrDefault(containerRecord => containerRecord.ContainerNumber.Equals(equipmentNumber));
			return containerXml?.Link;
		}
		#endregion

		#region Test Data
		class AddressData
		{
			public ZString AddressType { get; set; }
			public ZString AddressTypeDescription { get; set; }
			public ZString CompanyCode { get; set; }
			public ZString CompanyName { get; set; }
			public ZString Address1 { get; set; }
			public ZString Address2 { get; set; }
			public ZString City { get; set; }
			public ZString PostCode { get; set; }
			public ZString State { get; set; }
			public ZString StateDescription { get; set; }
			public ZString Country { get; set; }
			public ZString CountryName { get; set; }
			public ZString Contact { get; set; }
			public ZString Email { get; set; }
			public ZString Phone { get; set; }
			public ZString Mobile { get; set; }
		}

		class ContainerData
		{
			public ZString EquipmentNumber { get; set; }
			public ZString EquipmentType { get; set; }
			public ZInt PieceCount { get; set; }
			public ZString Commodity { get; set; }
			public ZString CommodityDescription { get; set; }
			public ZString GoodsDescription { get; set; }
			public ZBool IsEmptyContainer { get; set; }
			public ZBool IsShipperOwned { get; set; }
			public ZBool IsNonOperating { get; set; }
			public ZDecimal GoodsWeight { get; set; }
			public ZString WeightUnit { get; set; }
			public ZDecimal DunnageWeight { get; set; }
			public ZDecimal GrossWeight { get; set; }
			public ZDecimal TareWeight { get; set; }
			public ZDecimal? OverhangBack { get; set; }
			public ZDecimal? OverhangFront { get; set; }
			public ZDecimal? OverhangHeight { get; set; }
			public ZDecimal? OverhangLeft { get; set; }
			public ZDecimal? OverhangRight { get; set; }
			public ZString? LengthUnit { get; set; }
			public ZString SealNumber1 { get; set; }
			public ZString SealNumber2 { get; set; }
			public ZString SealNumber3 { get; set; }
			public ZString SealNumber4 { get; set; }
			public ZString SealNumber5 { get; set; }
			public ZString SealNumber6 { get; set; }
			public ZString SealParty1 { get; set; }
			public ZString SealParty2 { get; set; }
			public ZString SealParty3 { get; set; }
			public ZDateTime GrossWeightVerificationDateTime { get; set; }
			public ZString GrossWeightVerificationType { get; set; }
		}

		class PackingLineData
		{
			public ZString CargoType { get; set; }
			public ZString CargoMovementTypeOrigin { get; set; }
			public ZString CargoMovementTypeDestination { get; set; }
			public ZInt PackQty { get; set; }
			public ZString PackType { get; set; }
			public ZString PackTypeDescription { get; set; }
			public ZString Commodity { get; set; }
			public ZString CommodityDescription { get; set; }
			public ZString DescriptionOfGoods { get; set; }
			public ZString IdentificationReference { get; set; }
			public ZBool IsStackable { get; set; }
			public ZDecimal Weight { get; set; }
			public ZString WeightUnit { get; set; }
			public ZDecimal Length { get; set; }
			public ZString LengthUnit { get; set; }
			public ZDecimal Width { get; set; }
			public ZDecimal Height { get; set; }
		}

		AddressData NotifyParty { get; } = new AddressData()
		{
			AddressType = AutoDocAddressTypes.NotifyParty,
			AddressTypeDescription = nameof(DocAddressType.NotifyParty),
			CompanyCode = "N1",
			CompanyName = "Notify Party Company",
			Address1 = "Notify Party Address 1",
			Address2 = "Notify Party Address 2",
			City = "Sydney",
			PostCode = "2025",
			State = "NSW",
			StateDescription = "New South Wales",
			Country = "AU",
			CountryName = "Australia",
			Contact = "Notify Party Contact",
			Email = "notify@email.com",
			Phone = "12345678",
			Mobile = "11112222",
		};

		AddressData ConsignorParty { get; } = new AddressData()
		{
			AddressType = AutoDocAddressTypes.ConsignorAddress,
			AddressTypeDescription = nameof(DocAddressType.ConsignorAddress),
			CompanyCode = "CNR",
			CompanyName = "Consignor Company",
			Address1 = "Consignor Address 1",
			Address2 = "Consignor Address 2",
			City = "Hamburg",
			PostCode = "1045",
			State = "HH",
			StateDescription = "Hamburg (Hansestadt)",
			Country = "DE",
			CountryName = "Germany",
			Contact = "Consignor Contact",
			Email = "consignor@email.com",
			Phone = "33334444",
			Mobile = "55556666"
		};

		AddressData ConsigneeParty { get; } = new AddressData()
		{
			AddressType = AutoDocAddressTypes.ConsigneeAddress,
			AddressTypeDescription = nameof(DocAddressType.ConsigneeAddress),
			CompanyName = "Consignee Company",
			CompanyCode = "CNE",
			Address1 = "Consignee Address 1",
			Address2 = "Consignee Address 2",
			City = "Aukland",
			PostCode = "0209",
			State = "AUK",
			StateDescription = "Auckland",
			Country = "NZ",
			CountryName = "New Zealand",
			Contact = "Consignee Contact",
			Email = "consignee@test.com",
			Phone = "77778888",
			Mobile = "99990000"
		};

		AddressData BookingParty { get; } = new AddressData()
		{
			AddressType = AutoDocAddressTypes.BookingPartyDocumentaryAddress,
			AddressTypeDescription = nameof(DocAddressType.BookingPartyDocumentaryAddress),
			CompanyCode = "BP",
			CompanyName = "Booking Party Company",
			Address1 = "Booking Party Address 1",
			Address2 = "Booking Party Address 2",
			City = "Los Angeles",
			PostCode = "0002",
			State = "CA",
			StateDescription = "California",
			Country = "US",
			CountryName = "United States",
			Contact = "Booking Party Contact",
			Email = "BP@email.com",
			Phone = "11223344",
			Mobile = "55667788"
		};

		ContainerData Container1 { get; } = new ContainerData()
		{
			EquipmentNumber = "ABCD1234560",
			PieceCount = 1,
			EquipmentType = "20FR",
			Commodity = "COM1",
			CommodityDescription = "Commodity Test 1",
			GoodsDescription = "Goods Description Test 1",
			IsEmptyContainer = ZBool.False,
			IsShipperOwned = ZBool.True,
			IsNonOperating = ZBool.False,
			GoodsWeight = 2013.18,
			WeightUnit = Weight.Kilograms,
			DunnageWeight = 1000,
			TareWeight = 2000,
			GrossWeight = 5013.18,
			OverhangBack = 30,
			OverhangFront = 12,
			OverhangHeight = 5,
			OverhangLeft = 19,
			OverhangRight = 13,
			LengthUnit = Length.Centimetres,
			SealNumber1 = "SEALNO1",
			SealNumber2 = "SEALNO2",
			SealNumber3 = "SEALNO3",
			SealNumber4 = "SEALNO4",
			SealNumber5 = "SEALNO5",
			SealNumber6 = "SEALNO6",
			SealParty1 = "CAR",
			SealParty2 = "CRD",
			SealParty3 = "CUS",
			GrossWeightVerificationDateTime = new ZDateTime(2024, 02, 14, 10, 05, 30),
			GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
		};

		ContainerData Container2 { get; } = new ContainerData()
		{
			EquipmentNumber = "TEST9876540",
			PieceCount = 1,
			EquipmentType = "20GP",
			Commodity = "COM2",
			CommodityDescription = "Commodity Test2",
			GoodsDescription = "Goods Description Test 2",
			IsEmptyContainer = ZBool.False,
			IsShipperOwned = ZBool.False,
			IsNonOperating = ZBool.False,
			GoodsWeight = 1720,
			WeightUnit = Weight.Kilograms,
			DunnageWeight = 520,
			TareWeight = 2102,
			GrossWeight = 4342,
			SealNumber1 = "S01",
			SealNumber2 = "S02",
			SealNumber3 = "S03",
			SealParty1 = "QRT",
			SealParty2 = "CTO",
			SealParty3 = "CUS",
			GrossWeightVerificationDateTime = new ZDateTime(2025, 01, 22, 19, 35, 00),
			GrossWeightVerificationType = ContainerGrossWeightVerificationTypes.Codes.Method2Packages,
		};

		PackingLineData Breakbulk1 { get; } = new PackingLineData()
		{
			CargoType = Constants.ContainerModes.BreakBulk,
			CargoMovementTypeOrigin = "BB",
			CargoMovementTypeDestination = "BB",
			PackQty = 6,
			PackType = "F3T",
			PackTypeDescription = "PackType Test 1",
			Commodity = "COM3",
			CommodityDescription = "Commodity Test 3",
			DescriptionOfGoods = "Breakbulk Test",
			IdentificationReference = "BBK001",
			IsStackable = ZBool.False,
			Weight = 1939,
			WeightUnit = Weight.Kilograms,
			Length = 1941,
			LengthUnit = Length.Centimetres,
			Width = 420,
			Height = 613
		};
		#endregion
	}
}
