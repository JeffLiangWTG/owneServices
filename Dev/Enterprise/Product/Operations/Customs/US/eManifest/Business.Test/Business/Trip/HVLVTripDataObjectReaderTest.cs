using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class HVLVTripDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestStringValueCharacterCaseIsUpper()
		{
			var characterCasingProperty = typeof(HVLVTripDataObjectReader).GetProperty(
				"StringValueCharacterCase",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);

			var reader = new HVLVTripDataObjectReader(new UniversalXml.Shipment(), new UniversalXml.Shipment(),
				logger, Factory);
			var characterCasingValue = characterCasingProperty.GetValue(reader);

			AssertEquals(CharacterCase.Upper, characterCasingValue);
		}

		public void TestPopulateTripDetails()
		{
			CombineAssertions("Trip details", () =>
			{
				AssertEquals("Carrier code", "KARY", trip.BH_CarrierSCAC);
				AssertEquals("Transit Direction", TransitDirectionCodes.Codes.Importation, trip.BH_TransitDirection);
				AssertEquals("ETA", new ZDateTime(2021, 2, 23, 12, 23, 34), trip.BH_ETA);
				AssertEquals("Port of discharge", "USCHI", trip.BH_RL_NKPortUnlading);
				AssertEquals("Schedule D", "3901", trip.BH_PortUnladingDCode);
			});
		}

		public void TestOrganizations()
		{
			var importerAddress = trip.Importer;
			var importer = importerAddress.Header;

			var carrier = trip.Carrier;

			CombineAssertions("Org details", () =>
			{
				AssertEquals("Client organization code", "SHIPPER", trip.Importer.Header.OH_Code);
				AssertEquals("Carrier organization code", "CARRIER", trip.Carrier.OH_Code);
			});
		}

		public void TestPopulateParentInfo()
		{
			CombineAssertions("Parent Info", () =>
			{
				AssertEquals("ParentTableCode", JobShipmentSchema.Constants.Prefix, trip.BH_ParentTableCode);
				AssertEquals("ParentID", forwardingShipment.PK, trip.BH_ParentID);
			});
		}

		public void TestMatchAndSyncTrip()
		{
			AssertEquals("Precondition: Port of Discharge current value before sync is USCHI", "USCHI", trip.BH_RL_NKPortUnlading);

			var forwardingShipmentLevelDataObject = helper.ForwardingShipmentLevelDataObject;
			forwardingShipmentLevelDataObject.PortOfDischarge = new UNLOCO() { Code = "USLAX" };

			var tripBO = new HVLVTripDataObjectReader(helper.ForwardingConsolLevelDataObject, forwardingShipmentLevelDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("The same Trip has been matched", tripBO.PK, trip.PK);
				AssertEquals("Port of discharge has been updated", "USLAX", tripBO.BH_RL_NKPortUnlading);
			});
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_EmptyMasterBillNumberWhenSyncing()
		{
			Logger.ClearLogs();
			trip.Shipments.ForEach(x => x.B0_MasterBillNumber = ZString.Empty);
			Factory.SaveForTesting();

			AssertContainsExactElementsInAnyOrder("Precondition: All Shipments have empty master bill number", new[] { "", "", "" }, trip.Shipments.Select(x => x.B0_MasterBillNumber));

			var tripBO = new HVLVTripDataObjectReader(helper.ForwardingConsolLevelDataObject, helper.ForwardingShipmentLevelDataObject, Logger, Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				Assert(Logger.HasErrors);
				AssertContains("Cannot sync eManifest as there are Shipment(s) that have an empty House Bill Number. Please fix before reattempting to sync.", Logger.GetErrors());
			});
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_DuplicateMasterBillNumberWhenSyncing()
		{
			Logger.ClearLogs();
			trip.Shipments.ForEach(x => x.B0_MasterBillNumber = "LTTSTORE01");
			Factory.SaveForTesting();

			AssertContainsExactElementsInAnyOrder("Precondition: All Shipments have the same master bill number", new[] { "LTTSTORE01", "LTTSTORE01", "LTTSTORE01" }, trip.Shipments.Select(x => x.B0_MasterBillNumber));

			var tripBO = new HVLVTripDataObjectReader(helper.ForwardingConsolLevelDataObject, helper.ForwardingShipmentLevelDataObject, Logger, Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				Assert(Logger.HasErrors);
				AssertContains("Cannot sync eManifest as the following House Bill Numbers are duplicated on the Shipments: LTTSTORE01. Please fix before reattempting to sync.", Logger.GetErrors());
			});
		}

		HVLVeManifestDataTransferTestHelper helper;
		Trip trip;
		IForwardingShipment forwardingShipment;

		protected override void SetUp()
		{
			base.SetUp();

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			newFactory.Save();

			helper = new HVLVeManifestDataTransferTestHelper(Factory);
			trip = helper.Trip;
			forwardingShipment = helper.forwardingShipment;
		}
	}

	sealed class HVLVeManifestDataTransferTestHelper : Assertion
	{
		public HVLVeManifestDataTransferTestHelper(UniversalObjectFactory factory)
		{
			this.factory = factory;
		}

		public OrgAddress ConsignmentConsigneeOrgAddress;
		public OrgAddress ConsignmentShipperOrgAddress;

		readonly UniversalObjectFactory factory;
		UniversalXml.Shipment dataObject;
		public IForwardingShipment forwardingShipment;
		BusinessObject consignmentHeader;

		public Trip Trip
		{
			get
			{
				var trip = new HVLVTripDataObjectReader(ForwardingConsolLevelDataObject, ForwardingShipmentLevelDataObject, new DummyLogger(), factory).ReadIntoBusinessObject();

				if (consignmentHeader != null)
				{
					var query = GetGenPivotQuery(trip);
					if (!factory.BOFactory.Exists(typeof(GenPivot), query))
					{
						CreateGenPivot(trip);
					}
				}

				return trip;
			}
		}

		ZQuery GetGenPivotQuery(Trip trip)
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, consignmentHeader.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, consignmentHeader.TablePrefix);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, trip.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, trip.TablePrefix);
			query.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
			return query;
		}

		void CreateGenPivot(Trip trip)
		{
			var genPivot = factory.New<GenPivot>();
			genPivot.XX_Relation1ID = consignmentHeader.PK;
			genPivot.XX_Relation1TableCode = consignmentHeader.TablePrefix;
			genPivot.XX_Relation2ID = trip.PK;
			genPivot.XX_Relation2TableCode = trip.TablePrefix;
			genPivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			factory.SaveForTesting();
		}

		public UniversalXml.Shipment ForwardingShipmentLevelDataObject
		{
			get
			{
				InitialiseConsolAndShipmentIfNeeded();
				return dataObject?.SubShipmentCollection?.Single();
			}
		}

		public UniversalXml.Shipment ForwardingConsolLevelDataObject
		{
			get
			{
				InitialiseConsolAndShipmentIfNeeded();
				return dataObject;
			}
		}

		void InitialiseConsolAndShipmentIfNeeded()
		{
			if (dataObject == null)
			{
				var consol = factory.BOFactory.New<IForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "CATOR";
				consol.JK_RL_NKDischargePort = "USCHI";
				consol.JK_MasterBillNum = "JHJW849020";

				var transport = consol.Transports_Get(0);
				var transportBO = transport as BusinessObject;
				transportBO[JobConsolTransportSchema.JW_ATD.Name] = new ZDateTime(2021, 2, 22);
				transport.JW_ETA = new ZDateTime(2021, 2, 23, 12, 23, 34);
				transport.JW_VoyageFlight = "TRUCK01";

				var sendingAgentAddress = factory.NewWithValidTestData<OrgAddress>();
				sendingAgentAddress.OA_Address1 = "1 Forward Ave";
				sendingAgentAddress.OA_City = "OTTAWA";
				sendingAgentAddress.OA_PostCode = "66666";
				sendingAgentAddress.OA_State = "ON";
				sendingAgentAddress.OA_RN_NKCountryCode = CountryCodes.Canada;
				sendingAgentAddress.Header.OH_IsForwarder = true;
				var sendingAgentCCCCode = sendingAgentAddress.Header.CustomsCodes.AddNew();
				sendingAgentCCCCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				sendingAgentCCCCode.OK_RN_NKCodeCountry = CountryCodes.Canada;
				sendingAgentCCCCode.OK_CustomsRegNo = "SEND";
				consol.JK_OA_SendingForwarderAddress = sendingAgentAddress.PK;
				var carrierAddress = factory.NewWithValidTestData<OrgAddress>();
				carrierAddress.OA_Address1 = "2 Tranport Rd";
				carrierAddress.OA_City = "Chicago";
				carrierAddress.OA_PostCode = "60000";
				carrierAddress.OA_State = "IL";
				carrierAddress.OA_RN_NKCountryCode = CountryCodes.UnitedStates;
				carrierAddress.Header.OH_IsForwarder = true;
				carrierAddress.Header.OH_Code = "CARRIER";
				var carrierCCCCode = carrierAddress.Header.CustomsCodes.AddNew();
				carrierCCCCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				carrierCCCCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
				carrierCCCCode.OK_CustomsRegNo = "KARY";
				consol.JK_OA_ShippingLineAddress = carrierAddress.PK;

				var shipment = factory.BOFactory.New<IForwardingShipment>();
				shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
				shipment.JS_HouseBill = "HBL937278923";

				var consignorAddress = factory.NewWithValidTestData<OrgAddress>();
				consignorAddress.OA_Address1 = "1 Shipper St";
				consignorAddress.OA_City = "CLYDE";
				consignorAddress.OA_State = "OH";
				consignorAddress.OA_PostCode = "43410";
				consignorAddress.Header.OH_Code = "SHIPPER";
				var consignorDocAddress = factory.New<JobDocAddress>();
				consignorDocAddress.E2_OA_Address = consignorAddress.PK;
				consignorDocAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
				consignorDocAddress.E2_ParentID = shipment.PK;
				consignorDocAddress.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				var consigneeAddress = factory.New<JobDocAddress>();
				consigneeAddress.E2_AddressOverride = true;
				consigneeAddress.E2_Address1 = "PO Box 123";
				consigneeAddress.E2_City = "Chicago";
				consigneeAddress.E2_State = "IL";
				consigneeAddress.E2_Postcode = "62626";
				consigneeAddress.E2_CompanyName = "Receiver Co.";
				consigneeAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
				consigneeAddress.E2_ParentID = shipment.PK;
				consigneeAddress.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				consignmentHeader = factory.BOFactory.LoadTop1(ObjectFactory.GetType("IHVLVConsignmentHeader"), new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK));

				var consignmentType = ObjectFactory.GetType("IHVLVConsignment");
				var consignment1 = factory.New(consignmentType);
				var consignment2 = factory.New(consignmentType);
				var consignment3 = factory.New(consignmentType);

				consignment1[HVLVConsignmentSchema.HVC_HCH_Header] = consignmentHeader.PK;

				consignment1[HVLVConsignmentSchema.HVC_WaybillNumber] = "Test Waybill";
				consignment1[HVLVConsignmentSchema.HVC_ShipperReference] = "Test Shipper";
				consignment1[HVLVConsignmentSchema.HVC_ShipperName.Name] = "AMAZON CA";
				consignment1[HVLVConsignmentSchema.HVC_ShipperAddress1.Name] = "1 Online Shopping Ave";
				consignment1[HVLVConsignmentSchema.HVC_ShipperAddress2.Name] = "Mail Box";
				consignment1[HVLVConsignmentSchema.HVC_ShipperCity.Name] = "Toronto";
				consignment1[HVLVConsignmentSchema.HVC_ShipperState.Name] = "ON";
				consignment1[HVLVConsignmentSchema.HVC_ShipperPostcode.Name] = "50000";
				consignment1[HVLVConsignmentSchema.HVC_ShipperContact.Name] = "John";
				consignment1[HVLVConsignmentSchema.HVC_ShipperEmail.Name] = "John@amazon.ca";
				consignment1[HVLVConsignmentSchema.HVC_ShipperFax.Name] = "78923789234";
				consignment1[HVLVConsignmentSchema.HVC_ShipperMobile.Name] = "673467234";
				consignment1[HVLVConsignmentSchema.HVC_ShipperPhone.Name] = "923893478";
				consignment1[HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode.Name] = CountryCodes.Canada;
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeName.Name] = "Bob";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeAddress1.Name] = "123 Spender St";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeAddress2.Name] = "Backyard";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeCity.Name] = "Chicago";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeState.Name] = "IL";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneePostcode.Name] = "60000";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeContact.Name] = "Mike";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeEmail.Name] = "Mike@purchase.com";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeFax.Name] = "783478234";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneeMobile.Name] = "7812367234";
				consignment1[HVLVConsignmentSchema.HVC_ConsigneePhone.Name] = "567823478";
				consignment1[HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode.Name] = CountryCodes.UnitedStates;
				consignment1[HVLVConsignmentSchema.HVC_GoodsDescription.Name] = "medicine";
				consignment1[HVLVConsignmentSchema.HVC_GoodsValue.Name] = 100;
				consignment1[HVLVConsignmentSchema.HVC_RX_NKGoodsValueCurrency.Name] = CurrencyCodes.UnitedStates;
				consignment1[HVLVConsignmentSchema.HVC_WeightUQ.Name] = Weight.Kilograms;
				consignment1[HVLVConsignmentSchema.HVC_VolumeUQ.Name] = Volume.Litre;

				consignment1[HVLVConsignmentSchema.HVC_OA_ConsigneeAddress.Name] = ConsignmentConsigneeOrgAddress?.PK;
				consignment1[HVLVConsignmentSchema.HVC_OA_ShipperAddress.Name] = ConsignmentShipperOrgAddress?.PK;

				consignment2[HVLVConsignmentSchema.HVC_HCH_Header] = consignmentHeader.PK;

				consignment2[HVLVConsignmentSchema.HVC_WaybillNumber] = "Consignment2 Waybill";
				consignment2[HVLVConsignmentSchema.HVC_ShipperReference] = "Consignment2 Shipper Ref";
				consignment2[HVLVConsignmentSchema.HVC_ShipperName.Name] = "AMAZON CA";
				consignment2[HVLVConsignmentSchema.HVC_ShipperAddress1.Name] = "1 Online Shopping Ave";
				consignment2[HVLVConsignmentSchema.HVC_ShipperCity.Name] = "Toronto";
				consignment2[HVLVConsignmentSchema.HVC_ShipperState.Name] = "ON";
				consignment2[HVLVConsignmentSchema.HVC_ShipperPostcode.Name] = "50000";
				consignment2[HVLVConsignmentSchema.HVC_ShipperContact.Name] = "John";
				consignment2[HVLVConsignmentSchema.HVC_ShipperMobile.Name] = "673467234";
				consignment2[HVLVConsignmentSchema.HVC_ShipperPhone.Name] = "923893478";
				consignment2[HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode.Name] = CountryCodes.Canada;
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeName.Name] = "Bob";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeAddress1.Name] = "123 Spender St";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeAddress2.Name] = "Backyard";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeCity.Name] = "Chicago";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeState.Name] = "IL";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneePostcode.Name] = "60000";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeContact.Name] = "Mike";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneeMobile.Name] = "7812367234";
				consignment2[HVLVConsignmentSchema.HVC_ConsigneePhone.Name] = "567823478";
				consignment2[HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode.Name] = CountryCodes.UnitedStates;
				consignment2[HVLVConsignmentSchema.HVC_GoodsDescription.Name] = "medicine";
				consignment2[HVLVConsignmentSchema.HVC_GoodsValue.Name] = 100;
				consignment2[HVLVConsignmentSchema.HVC_RX_NKGoodsValueCurrency.Name] = CurrencyCodes.UnitedStates;
				consignment2[HVLVConsignmentSchema.HVC_WeightUQ.Name] = Weight.Kilograms;
				consignment2[HVLVConsignmentSchema.HVC_VolumeUQ.Name] = Volume.Litre;

				consignment2[HVLVConsignmentSchema.HVC_OA_ConsigneeAddress.Name] = ConsignmentConsigneeOrgAddress?.PK;
				consignment2[HVLVConsignmentSchema.HVC_OA_ShipperAddress.Name] = ConsignmentShipperOrgAddress?.PK;

				consignment3[HVLVConsignmentSchema.HVC_HCH_Header] = consignmentHeader.PK;
				consignment3[HVLVConsignmentSchema.HVC_WaybillNumber] = "Consignment3 Waybill";
				consignment3[HVLVConsignmentSchema.HVC_ShipperReference] = "Consignment3 Shipper Ref";
				consignment3[HVLVConsignmentSchema.HVC_ShipperName.Name] = "AMAZON CA";
				consignment3[HVLVConsignmentSchema.HVC_ShipperAddress1.Name] = "1 Online Shopping Ave";
				consignment3[HVLVConsignmentSchema.HVC_ShipperCity.Name] = "Toronto";
				consignment3[HVLVConsignmentSchema.HVC_ShipperState.Name] = "ON";
				consignment3[HVLVConsignmentSchema.HVC_ShipperPostcode.Name] = "50000";
				consignment3[HVLVConsignmentSchema.HVC_ShipperContact.Name] = "John";
				consignment3[HVLVConsignmentSchema.HVC_ShipperMobile.Name] = "673467234";
				consignment3[HVLVConsignmentSchema.HVC_ShipperPhone.Name] = "923893478";
				consignment3[HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode.Name] = CountryCodes.Canada;
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeName.Name] = "Bob";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeAddress1.Name] = "123 Spender St";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeAddress2.Name] = "Backyard";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeCity.Name] = "Chicago";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeState.Name] = "IL";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneePostcode.Name] = "60000";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeContact.Name] = "Mike";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneeMobile.Name] = "7812367234";
				consignment3[HVLVConsignmentSchema.HVC_ConsigneePhone.Name] = "567823478";
				consignment3[HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode.Name] = CountryCodes.UnitedStates;
				consignment3[HVLVConsignmentSchema.HVC_GoodsDescription.Name] = "medicine";
				consignment3[HVLVConsignmentSchema.HVC_GoodsValue.Name] = 100;
				consignment3[HVLVConsignmentSchema.HVC_RX_NKGoodsValueCurrency.Name] = CurrencyCodes.UnitedStates;
				consignment3[HVLVConsignmentSchema.HVC_WeightUQ.Name] = Weight.Kilograms;
				consignment3[HVLVConsignmentSchema.HVC_VolumeUQ.Name] = Volume.Litre;

				var itemType = ObjectFactory.GetType("IHVLVItem");
				var item1 = factory.New(itemType);
				var item2 = factory.New(itemType);
				var item3 = factory.New(itemType);

				item1[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment1.PK;
				item1[HVLVItemSchema.HVI_GoodsDescription.Name] = "covid-19 vaccine";
				item1[HVLVItemSchema.HVI_ActualVolume.Name] = 0.01;
				item1[HVLVItemSchema.HVI_ActualWeight.Name] = 0.002;
				item1[HVLVItemSchema.HVI_CurrentBarcode.Name] = "3478978237893489";
				item1[HVLVItemSchema.HVI_F3_NKPackType.Name] = "BAG";
				item1[HVLVItemSchema.HVI_JS_LoadedOnShipment.Name] = shipment.PK;

				item2[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment2.PK;
				item2[HVLVItemSchema.HVI_GoodsDescription.Name] = "Ymir spinal fluid";
				item2[HVLVItemSchema.HVI_ActualVolume.Name] = 0.01;
				item2[HVLVItemSchema.HVI_ActualWeight.Name] = 0.002;
				item2[HVLVItemSchema.HVI_CurrentBarcode.Name] = "1234567890123456";
				item2[HVLVItemSchema.HVI_F3_NKPackType.Name] = "BAG";
				item2[HVLVItemSchema.HVI_JS_LoadedOnShipment.Name] = shipment.PK;

				item3[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment3.PK;
				item3[HVLVItemSchema.HVI_GoodsDescription.Name] = "Sand";
				item3[HVLVItemSchema.HVI_ActualVolume.Name] = 0.01;
				item3[HVLVItemSchema.HVI_ActualWeight.Name] = 0.002;
				item3[HVLVItemSchema.HVI_CurrentBarcode.Name] = "0123456789012345";
				item3[HVLVItemSchema.HVI_F3_NKPackType.Name] = "BAG";
				item3[HVLVItemSchema.HVI_JS_LoadedOnShipment.Name] = shipment.PK;

				var itemLineType = ObjectFactory.GetType("IHVLVItemLine");
				var itemLine1 = factory.New(itemLineType);
				var itemLine2 = factory.New(itemLineType);
				var itemLine3 = factory.New(itemLineType);
				var itemLine4 = factory.New(itemLineType);

				itemLine1[HVLVItemLineSchema.HVS_HVI_HVLVItem.Name] = item1.PK;
				itemLine1[HVLVItemLineSchema.HVS_CustomsValue.Name] = 120;
				itemLine1[HVLVItemLineSchema.HVS_GrossWeight.Name] = 0.12;
				itemLine1[HVLVItemLineSchema.HVS_WeightUnit.Name] = Weight.Kilograms;
				itemLine1[HVLVItemLineSchema.HVS_GoodsDescription.Name] = "Meds";
				itemLine1[HVLVItemLineSchema.HVS_DestinationTariff.Name] = "83823893";
				itemLine1[HVLVItemLineSchema.HVS_OriginTariff.Name] = "58982348023";
				itemLine1[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode.Name] = CountryCodes.Belgium;
				itemLine1[HVLVItemLineSchema.HVS_Quantity.Name] = (ZShort)5;

				itemLine2[HVLVItemLineSchema.HVS_HVI_HVLVItem.Name] = item2.PK;
				itemLine2[HVLVItemLineSchema.HVS_CustomsValue.Name] = 1780;
				itemLine2[HVLVItemLineSchema.HVS_GrossWeight.Name] = 0.15;
				itemLine2[HVLVItemLineSchema.HVS_WeightUnit.Name] = Weight.Kilograms;
				itemLine2[HVLVItemLineSchema.HVS_GoodsDescription.Name] = "Organic Material";
				itemLine2[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode.Name] = ZString.Empty;
				itemLine2[HVLVItemLineSchema.HVS_Quantity.Name] = (ZShort)9;

				itemLine3[HVLVItemLineSchema.HVS_HVI_HVLVItem.Name] = item3.PK;
				itemLine3[HVLVItemLineSchema.HVS_CustomsValue.Name] = 100;
				itemLine3[HVLVItemLineSchema.HVS_GrossWeight.Name] = 0.10;
				itemLine3[HVLVItemLineSchema.HVS_WeightUnit.Name] = Weight.Kilograms;
				itemLine3[HVLVItemLineSchema.HVS_GoodsDescription.Name] = "Clay";
				itemLine3[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode.Name] = ZString.Empty;
				itemLine3[HVLVItemLineSchema.HVS_Quantity.Name] = (ZShort)3;

				itemLine4[HVLVItemLineSchema.HVS_HVI_HVLVItem.Name] = item3.PK;
				itemLine4[HVLVItemLineSchema.HVS_CustomsValue.Name] = 150;
				itemLine4[HVLVItemLineSchema.HVS_GrossWeight.Name] = 0.15;
				itemLine4[HVLVItemLineSchema.HVS_WeightUnit.Name] = Weight.Kilograms;
				itemLine4[HVLVItemLineSchema.HVS_GoodsDescription.Name] = "Gold";
				itemLine4[HVLVItemLineSchema.HVS_DestinationTariff.Name] = "57393736";
				itemLine4[HVLVItemLineSchema.HVS_OriginTariff.Name] = "15183947478";
				itemLine4[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode.Name] = CountryCodes.Canada;
				itemLine4[HVLVItemLineSchema.HVS_Quantity.Name] = (ZShort)7;

				consol.AddShipment(shipment);

				factory.SaveForTesting();

				var shipmentBO = shipment as BusinessObject;
				var contextManager = DataContextType.ForwardingShipment.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = contextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipmentBO)));
				dataObject = writer.GetDataObject(shipmentBO) as UniversalXml.Shipment;
				forwardingShipment = shipment;
			}
		}
	}
}
