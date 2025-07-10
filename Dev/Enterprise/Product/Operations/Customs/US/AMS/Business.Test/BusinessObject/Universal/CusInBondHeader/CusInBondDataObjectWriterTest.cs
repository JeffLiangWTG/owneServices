using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectWriterTest : DataTransfer.Universal.Testing.InBondDataObjectWriterTest
	{
		public void TestInBondMappings()
		{
			var foreignShipper = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var broker = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var header = SetupCusInBondHeader();
			header.PortArrivalDetails.AddNewIfNotExist("1001", new ZDateTime(2018, 04, 01));
			SetupDisposition(header.DispositionCodes.AddNew(), "AAA", new ZDateTime(2018, 2, 1, 11, 0, 0), 1);
			SetupDisposition(header.DispositionCodes.AddNew(), "BBB", new ZDateTime(2018, 2, 11, 1, 0, 0), 2);
			var bill1 = header.Bills.AddNew();
			SetupCusInBondBill(bill1, "APLU", "HB24");
			bill1.ForeignShipper.E2_OA_Address = foreignShipper.MainAddress.PK;
			bill1.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			bill1.NotifyParty1.E2_AddressOverride = ZBool.True;
			bill1.NotifyParty1.E2_CompanyName = "BOB THE BUILDER";
			bill1.NotifyParty1.E2_Address1 = "ADDRESS 1";
			bill1.NotifyParty1.E2_Address2 = "ADDRESS 2";
			bill1.NotifyParty1.E2_City = "CITY BOB";
			bill1.NotifyParty1.E2_State = "STATE BOB";
			bill1.NotifyParty1.E2_Postcode = "39234";
			bill1.NotifyParty1.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
			var ref1 = bill1.ShipmentReferenceDetails.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.IN;
			ref1.BR_ReferenceNum = "I986";
			var ref2 = bill1.ShipmentReferenceDetails.AddNew();
			ref2.BR_Qualifier = ReferenceQualifierList.Codes.IN;
			ref2.BR_ReferenceNum = "I498";
			var ref3 = bill1.ShipmentReferenceDetails.AddNew();
			ref3.BR_Qualifier = ReferenceQualifierList.Codes.CG;
			ref3.BR_ReferenceNum = "C3234";
			bill1.SecondaryNotifyParties.AddNewIfNotExist("OTT1");
			bill1.SecondaryNotifyParties.AddNewIfNotExist("APLU");
			var bill2 = header.Bills.AddNew();
			SetupCusInBondBill(bill2, "A3", "HB89");
			bill2.CustomsBroker.E2_OA_Address = broker.MainAddress.PK;
			var container1 = bill2.MovementDetail.Containers.AddNew();
			SetupCusInBondContainer(container1, "CONT1");
			SetupUNDGForContainer(container1, Substance.PK, 100.10m);
			var commodity1_Container1 = container1.Commodities.AddNew();
			SetupCommodity(commodity1_Container1, "10203010", 100m, null);
			var commodity2_Container1 = container1.Commodities.AddNew();
			SetupCommodity(commodity2_Container1, "10203020", 200m, "detailedDescription2");
			var container2 = bill2.MovementDetail.Containers.AddNew();
			SetupCusInBondContainer(container2, "CONT2");
			SetupVehicleDetail(container2, "VIN02123415450001");
			var bill3 = header.Bills.AddNew();
			SetupCusInBondBill(bill3, "A4", "HB43");
			var container3 = bill3.MovementDetail.Containers.AddNew();
			SetupCusInBondContainer(container3, "CONT3");
			SetupUNDGForContainer(container3, Substance2.PK, 200.20m);
			SetupVehicleDetail(container3, "VIN02123415450003");
			var commodity1_Container3 = container3.Commodities.AddNew();
			SetupCommodity(commodity1_Container3, "10203030", 300m, "detailedDescription3");
			bill3.B0_ManifestQty = 100;
			SetupCusInBondMoveHeaderForAMS(header.MovementHeader, "001234");
			var pttMoveHeader = header.PTTMovements.AddNew();
			var pttMoveDetail = pttMoveHeader.MovementDetails.AddNew(bill2.PK);
			SetupCusInBondMoveHeaderForPTT(pttMoveHeader, "PTTINB1", AMSBillMessageStatusList.Codes.ClearDeparture);
			SetupCusInBondMoveDetailForPTT(pttMoveDetail, 100, "CUS", "MES");
			var moveHeader1 = header.InBondMovementHeaders.AddNew();
			SetupCusInBondMoveHeader(moveHeader1, "INB323423");
			var moveHeader1Detail1 = moveHeader1.MovementDetails.AddNew(bill1.PK);
			SetupCusInBondMoveDetail(moveHeader1Detail1, 100, 100m, "3901", new ZDateTime(2018, 3, 20, 13, 30, 0), "IAN SHIP", "PRIT323222", "", "");
			var moveHeader1Detail2 = moveHeader1.MovementDetails.AddNew(bill3.PK);
			SetupCusInBondMoveDetail(moveHeader1Detail2, 200, 200m, "3901", new ZDateTime(2018, 3, 21, 3, 30, 0), "IAN SHIP", "PRIT323223", "", "");
			var moveHeader2 = header.InBondMovementHeaders.AddNew();
			SetupCusInBondMoveHeader(moveHeader2, "INB323424", true);
			var moveHeader2Detail1 = moveHeader2.MovementDetails.AddNew(bill1.PK);
			SetupCusInBondMoveDetail(moveHeader2Detail1, 100, 100m, "3901", new ZDateTime(2018, 3, 21, 3, 30, 0), "IAN SHIP", "PRIT323224", "", "");
			Factory.SaveForTesting();
			var writer = new CusInBondHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertInBondHeaderContents(headerData);
			var dispositionDataCollection = headerData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDisposition).ToArray();
			AssertEquals("dispositionDataCollection.Length", 2, dispositionDataCollection.Length);
			AssertInBondDisposition(dispositionDataCollection[0], "AAA", new ZDateTime(2018, 2, 1, 11, 0, 0), 1);
			AssertInBondDisposition(dispositionDataCollection[1], "BBB", new ZDateTime(2018, 2, 11, 1, 0, 0), 2);
			var portArrivalDateCollection = headerData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.PortArrivalDate.GroupTypeCode).ToArray();
			AssertEquals("portArrivalDateCollection.Length", 1, portArrivalDateCollection.Length);
			AssertPortArrival(portArrivalDateCollection[0], "1001", new ZDateTime(2018, 04, 01));
			AssertNotNull("headerData.ContainerCollection", headerData.ContainerCollection);
			AssertEquals("headerData.ContainerCollection.Count", 3, headerData.ContainerCollection.Count);
			var containerData = headerData.ContainerCollection[0];
			AssertInBondContainerContents(containerData, 1, "CONT1");
			AssertEquals("containerData.UNDGCollection.Count", 1, containerData.UNDGCollection.Count);
			AssertContainUNDG(containerData.UNDGCollection, Substance.DG_Code, "100.10");
			var vehicleNumbers = containerData.CustomsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.VehicleReference.Type).ToList();
			AssertEquals("vehicleNumbers.Count", 0, vehicleNumbers.Count);
			containerData = headerData.ContainerCollection[1];
			AssertInBondContainerContents(containerData, 2, "CONT2");
			AssertNull("containerData.UNDGCollection", containerData.UNDGCollection);
			vehicleNumbers = containerData.CustomsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.VehicleReference.Type).ToList();
			AssertEquals("vehicleNumbers.Count", 1, vehicleNumbers.Count);
			AssertContainVehicleNumber(vehicleNumbers, "VIN02123415450001");
			containerData = headerData.ContainerCollection[2];
			AssertInBondContainerContents(containerData, 3, "CONT3");
			AssertEquals("containerData.UNDGCollection.Count", 1, containerData.UNDGCollection.Count);
			AssertContainUNDG(containerData.UNDGCollection, Substance2.DG_Code, "200.20");
			vehicleNumbers = containerData.CustomsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.VehicleReference.Type).ToList();
			AssertEquals("vehicleNumbers.Count", 1, vehicleNumbers.Count);
			AssertContainVehicleNumber(vehicleNumbers, "VIN02123415450003");
			AssertEquals("headerData.PackingLineCollection.Count", 3, headerData.PackingLineCollection.Count);
			AssertInBondCommodityContents(headerData.PackingLineCollection[0], 1, "10203010", 100m, ZString.Empty);
			AssertInBondCommodityContents(headerData.PackingLineCollection[1], 1, "10203020", 200m, "detailedDescription2");
			AssertInBondCommodityContents(headerData.PackingLineCollection[2], 3, "10203030", 300m, "detailedDescription3");
			AssertEquals("headerData.AdditionalBillCollection.Count", 3, headerData.AdditionalBillCollection.Count);
			var billData = headerData.AdditionalBillCollection[0];
			AssertInBondBillContents(billData, "APLU", "HB24");
			var organizatonAddressCollection = billData.OrganizationAddressCollection;
			AssertEquals("billData.OrganizationAddressCollection.Count", 3, organizatonAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ForeignShipper", organizatonAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ForeignShipperDocumentaryAddress)), nameof(DocAddressType.ForeignShipperDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", organizatonAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertAddress("NotifyParty", organizatonAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty), null, "BOB THE BUILDER", ZBool.True, "ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas, "", "", "", "", "");
			AssertEquals("billData.CustomsReferenceCollection.Count", 5, billData.CustomsReferenceCollection.Count);
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[0], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.CG, "C3234");
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[1], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.IN, "I498");
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[2], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.IN, "I986");
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[3], Constants.SecondaryNotifyParty.Type, "OTT1", "1");
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[4], Constants.SecondaryNotifyParty.Type, "APLU", "2");
			billData = headerData.AdditionalBillCollection[1];
			AssertInBondBillContents(billData, "A3", "HB89");
			organizatonAddressCollection = billData.OrganizationAddressCollection;
			AssertEquals("billData.OrganizationAddressCollection.Count", 1, organizatonAddressCollection.Count);
			AssertOrganizationBO_INTHEMSYD("ImportBroker", organizatonAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ImportBroker)), nameof(DocAddressType.ImportBroker));
			AssertEquals("billData.CustomsReferenceCollection.Count", 0, billData.CustomsReferenceCollection.Count);
			billData = headerData.AdditionalBillCollection[2];
			AssertInBondBillContents(billData, "A4", "HB43");
			AssertNull("billData.OrganizationAddressCollection", billData.OrganizationAddressCollection);
			AssertEquals("billData.CustomsReferenceCollection.Count", 0, billData.CustomsReferenceCollection.Count);
			AssertNotNull("headerData.InBondMoveHeaderCollection", headerData.InBondMoveHeaderCollection);
			AssertEquals("headerData.InBondMoveHeaderCollection.Count", 4, headerData.InBondMoveHeaderCollection.Count);
			var inBondMoveHeaderDataForAMS = headerData.InBondMoveHeaderCollection.FirstOrDefault(x => x.MessagingApplicationCode.GetCodeAsUpperCase() == SubApplicationCodeList.Codes.AMS);
			AssertInBondMoveHeaderContentsForAMS(inBondMoveHeaderDataForAMS, "001234");
			AssertNotNull("inBondMoveHeaderDataForAMS.InBondMoveDetailCollection", inBondMoveHeaderDataForAMS.InBondMoveDetailCollection);
			AssertEquals("inBondMoveHeaderDataForAMS.InBondMoveDetailCollection.Count", 3, inBondMoveHeaderDataForAMS.InBondMoveDetailCollection.Count);
			var inBondMoveDetailData = inBondMoveHeaderDataForAMS.InBondMoveDetailCollection[0];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 1, 0, 0, "", ZDateTime.Empty, "", "", "", "", "", "", "", "");
			var containerLinkCollection = inBondMoveDetailData.ContainerLinkCollection;
			AssertEquals("containerLinkCollection.Count", 0, containerLinkCollection.Count);
			inBondMoveDetailData = inBondMoveHeaderDataForAMS.InBondMoveDetailCollection[1];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 2, 0, 0, "", ZDateTime.Empty, "", "", "", "", "", "", "", "");
			containerLinkCollection = inBondMoveDetailData.ContainerLinkCollection;
			AssertEquals("containerLinkCollection.Count", 2, containerLinkCollection.Count);
			var containerLinkData = containerLinkCollection[0];
			AssertEquals("containerLinkData.Link", 1, containerLinkData.Link);
			AssertEquals("containerLinkData.ContainerNumber", "CONT1", containerLinkData.ContainerNumber);
			containerLinkData = containerLinkCollection[1];
			AssertEquals("containerLinkData.Link", 2, containerLinkData.Link);
			AssertEquals("containerLinkData.ContainerNumber", "CONT2", containerLinkData.ContainerNumber);
			inBondMoveDetailData = inBondMoveHeaderDataForAMS.InBondMoveDetailCollection[2];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 3, 0, 0, "", ZDateTime.Empty, "", "", "", "", "", "", "", "");
			containerLinkCollection = inBondMoveDetailData.ContainerLinkCollection;
			AssertEquals("containerLinkCollection.Count", 1, containerLinkCollection.Count);
			containerLinkData = containerLinkCollection[0];
			AssertEquals("containerLinkData.Link", 3, containerLinkData.Link);
			AssertEquals("containerLinkData.ContainerNumber", "CONT3", containerLinkData.ContainerNumber);
			var inBondMoveHeaderDataForPTT = headerData.InBondMoveHeaderCollection.FirstOrDefault(x => x.MessagingApplicationCode.GetCodeAsUpperCase() == SubApplicationCodeList.Codes.PermitToTransfer);
			AssertInBondMoveHeaderContentsForPTT(inBondMoveHeaderDataForPTT, "PTTINB1", "CUS");
			AssertEquals("inBondMoveHeaderDataForPTT.InBondMoveDetailCollection.Count", 1, inBondMoveHeaderDataForPTT.InBondMoveDetailCollection.Count);
			inBondMoveDetailData = inBondMoveHeaderDataForPTT.InBondMoveDetailCollection[0];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 2, 100, 0m, "", ZDateTime.Empty, "", "", "CUS", "MES", "", "", "", "");
			var inBondMoveHeaderDataForINB = headerData.InBondMoveHeaderCollection.FirstOrDefault(x => x.MessagingApplicationCode.GetCodeAsUpperCase() == SubApplicationCodeList.Codes.MasterInBond);
			AssertNotNull("inBondMoveHeaderDataForINB", inBondMoveHeaderDataForINB);
			AssertInBondMoveHeaderContents(inBondMoveHeaderDataForINB, "INB323423");
			AssertEquals("inBondMoveHeaderDataForINB.InBondMoveDetailCollection.Count", 2, inBondMoveHeaderDataForINB.InBondMoveDetailCollection.Count);
			inBondMoveDetailData = inBondMoveHeaderDataForINB.InBondMoveDetailCollection[0];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 1, 100, 100m, "3901", new ZDateTime(2018, 3, 20, 13, 30, 0), "IAN SHIP", "PRIT323222", "", "", "", "", "", "");
			inBondMoveDetailData = inBondMoveHeaderDataForINB.InBondMoveDetailCollection[1];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 3, 200, 200m, "3901", new ZDateTime(2018, 3, 21, 3, 30, 0), "IAN SHIP", "PRIT323223", "", "", "", "", "", "");
			var inBondMoveHeaderDataForSIB = headerData.InBondMoveHeaderCollection.FirstOrDefault(x => x.MessagingApplicationCode.GetCodeAsUpperCase() == SubApplicationCodeList.Codes.SubsequentInBond);
			AssertNotNull("inBondMoveHeaderDataForSIB", inBondMoveHeaderDataForSIB);
			AssertInBondMoveHeaderContents(inBondMoveHeaderDataForSIB, "INB323424");
			AssertEquals("inBondMoveHeaderDataForSIB.InBondMoveDetailCollection.Count", 1, inBondMoveHeaderDataForSIB.InBondMoveDetailCollection.Count);
			inBondMoveDetailData = inBondMoveHeaderDataForSIB.InBondMoveDetailCollection[0];
			AssertInBondMoveDetailContents(inBondMoveDetailData, 1, 100, 100m, "3901", new ZDateTime(2018, 3, 21, 3, 30, 0), "IAN SHIP", "PRIT323224", "", "", "", "", "", "");
		}

		protected override BusinessObject CreateBizObjWithParent(out Customs.Business.CusInBondHeader header)
		{
			header = Factory.New<CusInBondHeader>();
			header.BH_ImportConveyanceName = "BOB VESSEL";
			return null;
		}

		protected override DataContextType GetDataContextType
		{
			get
			{
				return DataContextType.USAMS;
			}
		}

		CusInBondHeader SetupCusInBondHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			header.BH_GB = InBondBranch.PK;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = CarrierCode1;
			header.BH_IsPaperlessMIBParticipant = true;
			header.BH_IsOutboundCargo = true;
			header.BH_RL_NKImportLoadPort = "AUSYD";
			header.BH_FirstExportDate = new ZDateTime(2018, 03, 20);
			header.BH_RL_NKPortUnlading = "USPHL";
			header.BH_PortUnladingDCode = "1101";
			header.BH_ETA = new ZDateTime(2018, 03, 31);
			header.BH_FIRMS = "F320";
			header.BH_ImportConveyanceName = APLVessel.RV_Code;
			header.BH_VoyageNumber = "V324";
			header.BH_LloydsNumber = APLVessel.RV_LloydsNumber;
			header.BH_ImportConveyanceCountry = APLVessel.RV_RN_NKCountryOfReg;
			header.BH_UniqueVoyageIdentifier = "123151321";
			header.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			header.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			return header;
		}

		void AssertInBondHeaderContents(Shipment headerData)
		{
			AssertNotNull("Precondition: headerData", headerData);
			CombineAssertions(() =>
			{
				AssertEquals("headerData.MessageType", DirectionTypeList.Codes.MVOCC, headerData.MessageType.Code);
				AssertEquals("headerData.Branch", InBondBranch.GB_Code, headerData.Branch.Code);
				AssertEquals("headerData.TransportMode", US.Business.TransportTypeList.Codes.Sea, headerData.TransportMode.Code);
				AssertEquals("headerData.CustomsContainerMode", US.Business.ContainerModeList.Codes.Containerized, headerData.CustomsContainerMode.Code);
				AssertEquals("headerData.PortOfLoading", "AUSYD", headerData.PortOfLoading.Code);
				AssertEquals("headerData.PortOfDischarge", "USPHL", headerData.PortOfDischarge.Code);
				AssertEquals("headerData.VesselName", APLVessel.RV_Code, headerData.VesselName.GetValueOrDefault());
				AssertEquals("headerData.VoyageFlightNo", "V324", headerData.VoyageFlightNo.GetValueOrDefault());
				AssertEquals("headerData.LloydsIMO", APLVessel.RV_LloydsNumber, headerData.LloydsIMO.GetValueOrDefault());
				AssertEquals("headerData.VesselCountryOfRegistration", APLVessel.RV_RN_NKCountryOfReg, headerData.VesselCountryOfRegistration.Code);
				AssertNotNull(headerData.AddInfoCollection);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.CarrierSCAC, CarrierCode1);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.IsPaperlessMIBParticipant, YesNoDefaultList.Codes.Yes);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.IsOutboundCargo, YesNoDefaultList.Codes.Yes);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.PortOfDischargeScheduleD, "1101");
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.FIRMS, "F320");
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.UniqueVoyageIdentifier, "123151321");
				AssertNotNull(headerData.DateCollection);
				AssertContainDate(headerData.DateCollection, DateType.Departure, true, new ZDateTime(2018, 03, 20));
				AssertContainDate(headerData.DateCollection, DateType.Arrival, true, new ZDateTime(2018, 03, 31));
				AssertContainDate(headerData.DateCollection, DateType.Arrival, false, new ZDateTime(2018, 04, 01));
				AssertNotNull(headerData.NoteCollection);
				AssertContainNote(headerData.NoteCollection, ZBool.True, "DUMMY NOTE", "HELLO WORLD");
				AssertContainNote(headerData.NoteCollection, ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			});
		}

		void AssertPortArrival(UniversalCustoms.AddInfoGroup portArrivalData, ZString? portCode, ZDateTime? arrivalDate)
		{
			AssertEquals("portArrivalData.PortCode", portCode, portArrivalData.AddInfoCollection.GetZStringValue(Constants.PortArrivalDate.AddInfo.PortCode));
			AssertEquals("portArrivalData.ArrivalDate", arrivalDate, portArrivalData.AddInfoCollection.GetZDateTimeValue(Constants.PortArrivalDate.AddInfo.ArrivalDate));
		}
	}
}
