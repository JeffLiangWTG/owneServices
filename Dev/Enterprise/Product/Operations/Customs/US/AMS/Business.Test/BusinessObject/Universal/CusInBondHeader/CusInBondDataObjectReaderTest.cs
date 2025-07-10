using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectReaderTest : DataTransfer.Universal.Testing.InBondDataObjectReaderTest<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public void TestImportingInBondData()
		{
			var headerDataObject = SetupInBondHeader();
			headerDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			headerDataObject.NoteCollection.AddRange(new Note[] { SetupNote(), SetupNote2() });

			var billDataObject1 = SetupInBondBill("ABC", "2103201801", 100m, 1);
			billDataObject1.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.OB, "123456789"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT2", "1"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT1", "2")
			};
			var foreignShipperAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress;
			SetupAddressData(billDataObject1, foreignShipperAddress, DocAddressType.ForeignShipperDocumentaryAddress);
			var consigneeAddress = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress;
			SetupAddressData(billDataObject1, consigneeAddress, DocAddressType.ConsigneeAddress);
			var billDataObject2 = SetupInBondBill("BCD", "2103201802", 200m, 2);
			billDataObject2.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.CUB, "11111111"),
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.BN, "222222"),
			};
			var notifyPartyAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress;
			SetupAddressData(billDataObject2, notifyPartyAddress, DocAddressType.NotifyParty);
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject1, billDataObject2 });

			var amsMoveHeaderDataObject = SetupInBondMoveHeaderForAMS("021412");
			var amsMoveDetailDataObject1 = SetupInBondMoveDetail(1, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZInt.Zero);
			amsMoveDetailDataObject1.ContainerLinkCollection = new List<ContainerLink>()
			{
				new ContainerLink() { Link = 1, ContainerNumber = "CONT12354645" }
			};
			var amsMoveDetailDataObject2 = SetupInBondMoveDetail(2, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZInt.Zero);
			amsMoveDetailDataObject2.ContainerLinkCollection = new List<ContainerLink>()
			{
				new ContainerLink() { Link = 2, ContainerNumber = "CONT2345678" },
				new ContainerLink() { Link = 3, ContainerNumber = "CONT3456789" }
			};
			amsMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { amsMoveDetailDataObject1, amsMoveDetailDataObject2 };
			var pttMoveHeaderDataObject = SetupInBondMoveHeaderForPTT("12-3456789AB");
			pttMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { SetupInBondMoveDetailForPTT(1, 100), SetupInBondMoveDetailForPTT(2, 200) };
			var sibMoveHeaderDataObject = SetupInBondMoveHeader("006000072", "61", "Y", "12-3456789BC", "A2", "1101", "60200", "TEST VESSEL1", CusAgent.GS_Code, "TOL22031801", "TOL1", new ZDateTime(2018, 3, 20, 15, 0, 0),
				"TOL CITY 1", USStatesList.Codes.Alabama, new ZDateTime(2018, 03, 19), new ZDateTime(2018, 03, 25), new ZDateTime(2018, 03, 24), SubApplicationCodeList.Codes.SubsequentInBond, ZString.Empty, GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress, null);
			sibMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>()
			{
				SetupInBondMoveDetail(1, "1241421412", 50, 20m, "3901", new ZDateTime(2018, 03, 20, 16, 0, 0), "TOL VESSEL 1")
			};
			var inbMoveHeaderDataObject = SetupInBondMoveHeader("006000073", "62", "N", "12-3456789cd", "A3", "1102", "60201", "TEST VESSEL2", CusAgent.GS_Code, "TOL22031802", "TOL2", new ZDateTime(2018, 3, 21, 15, 0, 0),
				"TOL CITY 2", USStatesList.Codes.Alaska, new ZDateTime(2018, 03, 20), new ZDateTime(2018, 03, 26), new ZDateTime(2018, 03, 25), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty, null, GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress);
			inbMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>()
			{
				SetupInBondMoveDetail(1, "1241421413", 100, 50m, "1101", new ZDateTime(2018, 03, 21, 16, 0, 0), "TOL VESSEL 2"),
				SetupInBondMoveDetail(2, "1241421414", 200, 100m, "3901", new ZDateTime(2018, 03, 22, 16, 0, 0), "TOL VESSEL 3")
			};
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { amsMoveHeaderDataObject, pttMoveHeaderDataObject, sibMoveHeaderDataObject, inbMoveHeaderDataObject });

			var containerDataObject1 = SetupContainer("CONT12354645", ContainerType1, "SEAL1", "SEAL2", true, "1101", "60001", "CS", 1);
			containerDataObject1.SetUNDGCollection(() => new List<UNDG>() { SetupUNDG(Substance.DG_Code, "100.10") });
			SetupVehicle(containerDataObject1, "VIN02123415450001");
			var containerDataObject2 = SetupContainer("CONT2345678", ContainerType2, "SEAL3", "SEAL4", false, ZString.Empty, ZString.Empty, "CS", 2);
			SetupVehicle(containerDataObject2, "VIN02123415450002");
			var containerDataObject3 = SetupContainer("CONT3456789", ContainerType1, "SEAL5", "SEAL6", true, "1101", "60001", "CY", 3);
			containerDataObject3.SetUNDGCollection(() => new List<UNDG>() { SetupUNDG(Substance2.DG_Code, "200.20") });
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>() { containerDataObject1, containerDataObject2, containerDataObject3 });
			var commodityDataObject1 = SetupPackingLine(50, "BAG", "0101100010", 100.10m, "FIRST COMMODITY", 200m, "KG", "FIRST MARKS", "US", "FIRST C4", 1, "Detailed desc 1");
			var commodityDataObject2 = SetupPackingLine(100, "BAL", "1001100020", 200.20m, "SECOND COMMODITY", 300m, "KT", "SECOND MARKS", "CA", "SECOND C4", 2, "Detailed desc 2");
			var commodityDataObject3 = SetupPackingLine(150, "BBL", "2001100030", 300.30m, "THIRD COMMODITY", 400m, "T", "THIRD MARKS", "CN", "THIRD C4", 3, "Detailed desc 3");
			headerDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { commodityDataObject1, commodityDataObject2, commodityDataObject3 });
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);

			CombineAssertions(() =>
			{
				AssertContents(headerBO);
				AssertEquals("headerBO.Bills.Count", 2, headerBO.Bills.Count);
				var bill1BO = headerBO.Bills.OfType<CusInBondBill>().First(x => x.B0_MasterBillNumber == "2103201801");
				AssertCusInBondBillContents(bill1BO, "ABC", "2103201801", 100);
				AssertEquals("bill1BO.ShipmentReferenceDetails.Count", 1, bill1BO.ShipmentReferenceDetails.Count);
				AssertAdditionalReferenceContains(bill1BO.ShipmentReferenceDetails, BillReferenceList.Codes.OB, "123456789");
				AssertEquals("bill1BO.SecondaryNotifyParties.Count", 2, bill1BO.SecondaryNotifyParties.Count);
				AssertSecondaryNotifyPartyContains(bill1BO.SecondaryNotifyParties, "OTT2", 1);
				AssertSecondaryNotifyPartyContains(bill1BO.SecondaryNotifyParties, "OTT1", 2);
				AssertEquals("bill1BO.ForeignShipper.Address", foreignShipperAddress, bill1BO.ForeignShipper.Address);
				AssertEquals("bill1BO.Consignee.Address", consigneeAddress, bill1BO.Consignee.Address);
				var bill2BO = headerBO.Bills.OfType<CusInBondBill>().First(x => x.B0_MasterBillNumber == "2103201802");
				AssertCusInBondBillContents(bill2BO, "BCD", "2103201802", 200);
				AssertEquals("bill2BO.ShipmentReferenceDetails.Count", 2, bill2BO.ShipmentReferenceDetails.Count);
				AssertAdditionalReferenceContains(bill2BO.ShipmentReferenceDetails, BillReferenceList.Codes.CUB, "11111111");
				AssertAdditionalReferenceContains(bill2BO.ShipmentReferenceDetails, BillReferenceList.Codes.BN, "222222");
				AssertEquals("bill2BO.SecondaryNotifyParties.Count", 0, bill2BO.SecondaryNotifyParties.Count);
				AssertEquals("bill2BO.NotifyParty1.Address", notifyPartyAddress, bill2BO.NotifyParty1.Address);
				AssertEquals("headerBO.MovementHeaders.Count", 1, headerBO.MovementHeaders.Count);
				var moveHeaderBO = (CusInBondMoveHeader)headerBO.MovementHeaders[0];
				AssertCusInBondMoveHeaderForAMSContents(moveHeaderBO, "021412");
				AssertEquals("moveHeaderBO.MovementDetails.Count", 2, moveHeaderBO.MovementDetails.Count);
				var moveDetailBO = moveHeaderBO.MovementDetails[0];
				AssertCusInBondMoveDetailContents(moveDetailBO, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDateTime.Empty, ZString.Empty, bill1BO.PK);
				AssertEquals("moveDetailBO.Containers.Count", 1, moveDetailBO.Containers.Count);
				var containerBO = moveDetailBO.Containers[0];
				AssertCusInBondContainerContents(containerBO, "CONT12354645", ContainerTypeBO1.PK, "SEAL1", "SEAL2", true, "1101", "60001", "CS");
				AssertEquals("containerBO.UNDGs.Count", 1, containerBO.UNDGs.Count);
				var undgBO = containerBO.UNDGs[0];
				AssertUNDGContents(undgBO, Substance.DG_Code, 100.1m);
				AssertEquals("containerBO.Vehicles.Count", 1, containerBO.Vehicles.Count);
				var vehicleBO = containerBO.Vehicles[0];
				AssertEquals("vehicleBO.BV_VIN", "VIN02123415450001", vehicleBO.BV_VIN);
				AssertEquals("containerBO.Commodities.Count", 1, containerBO.Commodities.Count);
				var commodityBO = containerBO.Commodities[0];
				AssertCusInBondCargoDescContents(commodityBO, "0101100010", 100.1m, 200m, "KG", 50, "BAG", "FIRST COMMODITY", "FIRST MARKS", "US", "FIRST C4", "Detailed desc 1");
				moveDetailBO = moveHeaderBO.MovementDetails[1];
				AssertCusInBondMoveDetailContents(moveDetailBO, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDateTime.Empty, ZString.Empty, bill2BO.PK);
				AssertEquals("moveDetailBO.Containers.Count", 2, moveDetailBO.Containers.Count);
				containerBO = moveDetailBO.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2345678");
				AssertCusInBondContainerContents(containerBO, "CONT2345678", ContainerTypeBO2.PK, "SEAL3", "SEAL4", false, ZString.Empty, ZString.Empty, "CS");
				AssertEquals("containerBO.UNDGs.Count", 0, containerBO.UNDGs.Count);
				AssertEquals("containerBO.Vehicles.Count", 1, containerBO.Vehicles.Count);
				vehicleBO = containerBO.Vehicles[0];
				AssertEquals("vehicleBO.BV_VIN", "VIN02123415450002", vehicleBO.BV_VIN);
				containerBO = moveDetailBO.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT3456789");
				AssertCusInBondContainerContents(containerBO, "CONT3456789", ContainerTypeBO1.PK, "SEAL5", "SEAL6", true, "1101", "60001", "CY");
				AssertEquals("containerBO.UNDGs.Count", 1, containerBO.UNDGs.Count);
				undgBO = containerBO.UNDGs[0];
				AssertUNDGContents(undgBO, Substance2.DG_Code, 200.2m);
				AssertEquals("containerBO.Vehicles.Count", 0, containerBO.Vehicles.Count);
				AssertEquals("headerBO.PTTMovements.Count", 1, headerBO.PTTMovements.Count);
				moveHeaderBO = headerBO.PTTMovements[0];
				AssertCusInBondMoveHeaderForPTTContents(moveHeaderBO, "12-3456789AB");
				AssertEquals("moveHeaderBO.MovementDetails.Count", 2, moveHeaderBO.MovementDetails.Count);
				moveDetailBO = moveHeaderBO.MovementDetails[0];
				AssertCusInBondMoveDetailContents(moveDetailBO, ZString.Empty, 100, ZDecimal.Zero, ZString.Empty, ZDateTime.Empty, ZString.Empty, bill1BO.PK);
				moveDetailBO = moveHeaderBO.MovementDetails[1];
				AssertCusInBondMoveDetailContents(moveDetailBO, ZString.Empty, 200, ZDecimal.Zero, ZString.Empty, ZDateTime.Empty, ZString.Empty, bill2BO.PK);
				AssertEquals("headerBO.InBondMovementHeaders.Count", 2, headerBO.InBondMovementHeaders.Count);
				moveHeaderBO = (CusInBondMoveHeader)headerBO.InBondMovementHeaders.First(x => x.BM_SubApplicationCode == SubApplicationCodeList.Codes.MasterInBond);
				AssertNotNull("Master Inbond MoveHeader", moveHeaderBO);
				AssertCusInBondMoveHeaderContents(moveHeaderBO, "", "62", "N", "12-3456789cd", "A3", "1102", "60201", "TEST VESSEL2", CusAgent.GS_Code, "TOL22031802", "TOL2", new ZDateTime(2018, 3, 21, 15, 0, 0),
				"TOL CITY 2", USStatesList.Codes.Alaska, new ZDateTime(2018, 03, 20), new ZDateTime(2018, 03, 26), new ZDateTime(2018, 03, 25), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty);
				AssertEquals("moveHeaderBO.MovementDetails.Count", 2, moveHeaderBO.MovementDetails.Count);
				moveDetailBO = moveHeaderBO.MovementDetails[0];
				AssertCusInBondMoveDetailContents(moveDetailBO, "1241421413", 100, 50m, "1101", new ZDateTime(2018, 03, 21, 16, 0, 0), "TOL VESSEL 2", bill1BO.PK);
				moveDetailBO = moveHeaderBO.MovementDetails[1];
				AssertCusInBondMoveDetailContents(moveDetailBO, "1241421414", 200, 100m, "3901", new ZDateTime(2018, 03, 22, 16, 0, 0), "TOL VESSEL 3", bill2BO.PK);
				moveHeaderBO = (CusInBondMoveHeader)headerBO.InBondMovementHeaders.First(x => x.BM_SubApplicationCode == SubApplicationCodeList.Codes.SubsequentInBond);
				AssertNotNull("Subsequent InBond MoveHeader", moveHeaderBO);
				AssertCusInBondMoveHeaderContents(moveHeaderBO, "", "61", "Y", "12-3456789BC", "A2", "1101", "60200", "TEST VESSEL1", CusAgent.GS_Code, "TOL22031801", "TOL1", new ZDateTime(2018, 3, 20, 15, 0, 0),
				"TOL CITY 1", USStatesList.Codes.Alabama, new ZDateTime(2018, 03, 19), new ZDateTime(2018, 03, 25), new ZDateTime(2018, 03, 24), SubApplicationCodeList.Codes.SubsequentInBond, ZString.Empty);
				AssertEquals("moveHeaderBO.MovementDetails.Count", 1, moveHeaderBO.MovementDetails.Count);
				moveDetailBO = moveHeaderBO.MovementDetails[0];
				AssertCusInBondMoveDetailContents(moveDetailBO, "1241421412", 50, 20m, "3901", new ZDateTime(2018, 03, 20, 16, 0, 0), "TOL VESSEL 1", bill1BO.PK);
			});
		}

		public void TestImportingInBondDataWithoutMessageType()
		{
			var headerDataObject = SetupInBondHeader();
			headerDataObject.MessageType = new CodeDescriptionPair() { Code = "A" };
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNull(headerBO);
			AssertEquals(@"Error - Cannot populate CusInBondHeader because:
Message type is invalid for AMS job, should be either 'N' or 'M'.", logger.Logs);
		}

		public void TestImportingInBondDataWithDifferentMessageType()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			Factory.SaveForTesting();
			var headerDataObject = SetupInBondHeader();
			var dataTarget = headerDataObject.DataContext.DataTargetCollection.FirstOrDefault();
			dataTarget.Key = header.BH_JobReference;
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(header, headerBO);
			AssertEquals(@"Information - Successfully loaded matching CusInBondHeader.
Error - Cannot populate CusInBondHeader because:
Message type in UXML and in AMS job - AMS0000001 doesn't match.", logger.Logs);
		}

		public void TestImportingInBondData_DataExistAndMessageIsActive_UpdateAllowed()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.SaveForTesting();
			var headerDataObject = SetupInBondHeader();
			var dataTarget = headerDataObject.DataContext.DataTargetCollection.FirstOrDefault();
			dataTarget.Key = header.BH_JobReference;
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(header, headerBO);
			AssertStartsWith("Update allowed", @"Information - Successfully loaded matching CusInBondHeader.
Information - Populating CusInBondHeader...", logger.Logs);
		}

		public void TestImportingInBondDataRepeat_UpdateValueThatCustomsCanChange_DoNotReadIn()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.SaveForTesting();
			var headerDataObject = SetupInBondHeader();
			var dataTarget = headerDataObject.DataContext.DataTargetCollection.FirstOrDefault();
			dataTarget.Key = header.BH_JobReference;
			var billDataObject = SetupInBondBill("BCD", "2103201802", 200m, 1);
			billDataObject.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.CUB, "11111111")
			};
			var notifyPartyAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress;
			SetupAddressData(billDataObject, notifyPartyAddress, DocAddressType.NotifyParty);
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject });
			var portArrivalDateGroup = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.PortArrivalDate.GroupTypeCode, Description = Constants.PortArrivalDate.GroupTypeDescription },
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = Constants.PortArrivalDate.AddInfo.PortCode, Value = "1101" },
					new AddInfo() { Key = Constants.PortArrivalDate.AddInfo.ArrivalDate, Value = BaseAddInfo.GetStringRepresentation(new ZDateTime(2023, 08, 02)) }
				}
			};
			var dispositionGroup = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = "UDP", Description = "Disposition" },
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = "Code", Value = "AAD" },
					new AddInfo() { Key = "DispositionDate", Value = BaseAddInfo.GetStringRepresentation(new ZDateTime(2023, 08, 02)) },
					new AddInfo() { Key = "Order", Value = "1" }
				}
			};
			headerDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>() { portArrivalDateGroup, dispositionGroup });
			var inbMoveHeaderDataObject = SetupInBondMoveHeader("006000073", "62", "N", "12-3456789cd", "A3", "1102", "60201", "TEST VESSEL2", CusAgent.GS_Code, "TOL22031802", "TOL2", new ZDateTime(2018, 3, 21, 15, 0, 0),
				"TOL CITY 2", USStatesList.Codes.Alaska, new ZDateTime(2018, 03, 20), new ZDateTime(2018, 03, 26), new ZDateTime(2018, 03, 25), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty, null, GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress);
			inbMoveHeaderDataObject.CustomsStatus = new CodeDescriptionPair() { Code = CustomsStatusCodeList.Codes.Unknown, Description = "Unknown" };
			var inbMoveHeaderDetail = SetupInBondMoveDetail(1, "1241421413", 100, 50m, "1101", new ZDateTime(2018, 03, 21, 16, 0, 0), "TOL VESSEL 2");
			inbMoveHeaderDetail.CustomsStatus = new CodeDescriptionPair() { Code = "FIL", Description = "" };
			inbMoveHeaderDetail.MessageStatus = new CodeDescriptionPair() { Code = "ERR", Description = "Error" };
			inbMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { inbMoveHeaderDetail };
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { inbMoveHeaderDataObject });

			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("Read shipment success", header, headerBO);
			Assert("PortArrivalDetails should not read", header.PortArrivalDetails.Count == 0);
			Assert("DispositionCodes should not read", header.DispositionCodes.Count == 0);
			Assert("Should read right InBondMoveHeader length", header.InBondMovementHeaders.Count == 1);
			Assert("Should not read InBondMoveHeader CustomStatus", header.InBondMovementHeaders[0].BM_CustomsStatus.IsEmpty);
			Assert("Should not read CusInBondMoveDetail MessageStatus", header.InBondMovementHeaders[0].MovementDetails[0].B9_MessageStatus.IsEmpty);
			Assert("Should not read CusInBondMoveDetail CustomStatus", header.InBondMovementHeaders[0].MovementDetails[0].B9_CustomsStatus.IsEmpty);

			var arrivalDate = header.BH_ETA;
			Assert("Shoul have right bill length", header.Bills.Count == 1);
			var billPk = header.Bills[0].PK;
			var billDateOfDischarge = header.Bills[0].B0_DateOfDischarge;

			headerDataObject.DateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == DateType.Arrival).Value = new ZDateTime(2023, 08, 03);
			billDataObject.AddInfoCollection.First(f => f.Key.Equals(Constants.Bill.AddInfo.EstimatedUnloadDate)).Value = BaseAddInfo.GetStringRepresentation(new ZDateTime(2023, 08, 03));
			var headerBO2 = reader.ReadIntoBusinessObject();

			AssertNotEquals("BH_ETA can change by shipment", arrivalDate, headerBO2.BH_ETA);
			Assert("Bill exist should just update", headerBO2.Bills.Count == 1 && headerBO2.Bills[0].PK.Equals(billPk));
			AssertNotEquals("Bill B0_DateOfDischarge can change by shipment", billDateOfDischarge, headerBO2.Bills[0].B0_DateOfDischarge);
		}

		public void TestImportingInbodDataRepeat_UpdateCollectionData_UpdateItemExistInDbOrReBuildAll()
		{
			var headerDataObject = SetupInBondHeader();
			var billDataObject1 = SetupInBondBill("ABC", "2103201801", 100m, 1);
			billDataObject1.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.OB, "123456789"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT2", "1"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT1", "2")
			};
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject1 });
			var amsMoveHeaderDataObject = SetupInBondMoveHeaderForAMS("021412");
			var amsMoveDetailDataObject1 = SetupInBondMoveDetail(1, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZInt.Zero);
			amsMoveDetailDataObject1.ContainerLinkCollection = new List<ContainerLink>()
			{
				new ContainerLink() { Link = 1, ContainerNumber = "CONT12354645" }
			};
			amsMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { amsMoveDetailDataObject1 };
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { amsMoveHeaderDataObject });

			var containerDataObject1 = SetupContainer("CONT12354645", ContainerType1, "SEAL1", "SEAL2", true, "1101", "60001", "CS", 1);
			SetupVehicle(containerDataObject1, "VIN02123415450001");
			containerDataObject1.SetUNDGCollection(() => new List<UNDG>() { SetupUNDG(Substance.DG_Code, "100.10") });
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>() { containerDataObject1 });
			var commodityDataObject1 = SetupPackingLine(50, "BAG", "0101100010", 100.10m, "FIRST COMMODITY", 200m, "KG", "FIRST MARKS", "US", "FIRST C4", 1, "Detailed desc 1");
			headerDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { commodityDataObject1 });
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);

			AssertEquals("Bills.Count", 1, headerBO.Bills.Count);
			AssertEquals("ShipmentReferenceDetails.Count", 1, headerBO.Bills[0].ShipmentReferenceDetails.Count);
			AssertEquals("SecondaryNotifyParties.Count", 2, headerBO.Bills[0].SecondaryNotifyParties.Count);
			AssertEquals("MovementHeaders.Count", 1, headerBO.MovementHeaders.Count);
			var moveHeaderBO = (CusInBondMoveHeader)headerBO.MovementHeaders[0];
			AssertEquals("MovementDetails", 1, moveHeaderBO.MovementDetails.Count);
			AssertEquals("Containers.Count", 1, moveHeaderBO.MovementDetails[0].Containers.Count);
			AssertEquals("Vehicles.Count", 1, moveHeaderBO.MovementDetails[0].Containers[0].Vehicles.Count);
			AssertEquals("UNDGs.Count", 1, moveHeaderBO.MovementDetails[0].Containers[0].UNDGs.Count);
			AssertEquals("Commodities.Count", 1, moveHeaderBO.MovementDetails[0].Containers[0].Commodities.Count);

			var pkArray1 = SavePrimaryKeyTempory(headerBO, moveHeaderBO);

			billDataObject1.NoOfPacks = 300m;
			var billDataObject2 = SetupInBondBill("BCD", "2103201802", 200m, 2);
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject1, billDataObject2 });
			billDataObject1.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.OB, "qqq"),
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.OB, "eee"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT2", "www"),
			};

			amsMoveDetailDataObject1.ContainerLinkCollection = new List<ContainerLink>()
			{
				new ContainerLink() { Link = 1, ContainerNumber = "CONT12354645" },
				new ContainerLink() { Link = 2, ContainerNumber = "CONT12354666" }
			};
			var containerDataObject12 = SetupContainer("CONT12354666", ContainerType1, "SEAL1", "SEAL2", true, "1101", "60001", "CS", 2);
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>() { containerDataObject1, containerDataObject12 });

			var amsMoveDetailDataObject2 = SetupInBondMoveDetail(2, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZInt.Zero);
			amsMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { amsMoveDetailDataObject1, amsMoveDetailDataObject2 };

			var amsMoveHeaderDataObject2 = SetupInBondMoveHeaderForAMS("021413");
			var amsMoveDetailDataObject3 = SetupInBondMoveDetail(3, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZInt.Zero);
			amsMoveHeaderDataObject2.InBondMoveDetailCollection = new List<InBondMoveDetail>() { amsMoveDetailDataObject3 };
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { amsMoveHeaderDataObject, amsMoveHeaderDataObject2 });

			SetupVehicle(containerDataObject1, "VIN02123415450002");
			containerDataObject1.SetUNDGCollection(() => new List<UNDG>()
			{
				SetupUNDG(Substance.DG_Code, "100.10"),
				SetupUNDG(Substance.DG_Code, "200.10")
			});
			var commodityDataObject2 = SetupPackingLine(50, "BAG", "0101100010", 100.10m, "FIRST COMMODITY", 200m, "KG", "FIRST MARKS", "US", "FIRST C4", 1, "Detailed desc 2");
			headerDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { commodityDataObject1, commodityDataObject2 });
			Factory.SaveForTesting();
			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO2 = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO2);

			AssertEquals("Bills.Count change", 2, headerBO2.Bills.Count);
			AssertEquals("ShipmentReferenceDetails.Count change", 2, headerBO2.Bills[0].ShipmentReferenceDetails.Count);
			AssertEquals("SecondaryNotifyParties.Count change", 1, headerBO2.Bills[0].SecondaryNotifyParties.Count);
			AssertEquals("MovementHeaders.Count change", 2, headerBO2.MovementHeaders.Count);
			moveHeaderBO = (CusInBondMoveHeader)headerBO2.MovementHeaders[0];
			AssertEquals("MovementDetails change", 2, moveHeaderBO.MovementDetails.Count);
			AssertEquals("Containers.Count change", 2, moveHeaderBO.MovementDetails[0].Containers.Count);
			AssertEquals("Vehicles.Count change", 2, moveHeaderBO.MovementDetails[0].Containers[0].Vehicles.Count);
			AssertEquals("UNDGs.Count change", 2, moveHeaderBO.MovementDetails[0].Containers[0].UNDGs.Count);
			AssertEquals("Commodities.Count change", 2, moveHeaderBO.MovementDetails[0].Containers[0].Commodities.Count);

			var pkArray2 = SavePrimaryKeyTempory(headerBO2, moveHeaderBO);

			AssertEquals("Bill update if exist", pkArray1[0], pkArray2[0]);
			AssertEquals("ShipmentReferenceDetail update if exist", pkArray1[1], pkArray2[1]);
			AssertEquals("SecondaryNotifyPartie update if exist", pkArray1[2], pkArray2[2]);
			AssertEquals("MovementHeader update if exist", pkArray1[3], pkArray2[3]);
			AssertEquals("MovementDetail updaste if exist", pkArray1[4], pkArray2[4]);
			AssertEquals("Container update if exist", pkArray1[5], pkArray2[5]);
			AssertEquals("Vehicle update if exist", pkArray1[6], pkArray2[6]);
			AssertNotEquals("UNDG rebuild", pkArray1[7], pkArray2[7]);
			AssertNotEquals("Commoditie rebuild", pkArray1[8], pkArray2[8]);
		}

		public void TestImportingInbodDataRepeat_CollectionDataIsEmpty_DeleteItemsExistInDb()
		{
			var headerDataObject = SetupInBondHeader();
			var billDataObject1 = SetupInBondBill("ABC", "2103201801", 100m, 1);
			billDataObject1.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.OB, "123456789"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT2", "1"),
				SetupCustomsReferenceData(Constants.SecondaryNotifyParty.Type, "OTT1", "2")
			};
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject1 });
			var amsMoveHeaderDataObject = SetupInBondMoveHeaderForAMS("021412");
			var amsMoveDetailDataObject1 = SetupInBondMoveDetail(1, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZInt.Zero);
			amsMoveDetailDataObject1.ContainerLinkCollection = new List<ContainerLink>()
			{
				new ContainerLink() { Link = 1, ContainerNumber = "CONT12354645" }
			};
			amsMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { amsMoveDetailDataObject1 };
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { amsMoveHeaderDataObject });

			var containerDataObject1 = SetupContainer("CONT12354645", ContainerType1, "SEAL1", "SEAL2", true, "1101", "60001", "CS", 1);
			SetupVehicle(containerDataObject1, "VIN02123415450001");
			containerDataObject1.SetUNDGCollection(() => new List<UNDG>() { SetupUNDG(Substance.DG_Code, "100.10") });
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>() { containerDataObject1 });
			var commodityDataObject1 = SetupPackingLine(50, "BAG", "0101100010", 100.10m, "FIRST COMMODITY", 200m, "KG", "FIRST MARKS", "US", "FIRST C4", 1, "Detailed desc 1");
			headerDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { commodityDataObject1 });
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);

			containerDataObject1.CustomsReferenceCollection = new List<CustomsReference>();
			containerDataObject1.SetUNDGCollection(() => new List<UNDG>());
			headerDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			headerBO = reader.ReadIntoBusinessObject();
			var moveHeaderBO = (CusInBondMoveHeader)headerBO.MovementHeaders[0];
			AssertEquals("Vehicles removed", 0, moveHeaderBO.MovementDetails[0].Containers[0].Vehicles.Count);
			AssertEquals("UNDGs removed", 0, moveHeaderBO.MovementDetails[0].Containers[0].UNDGs.Count);
			AssertEquals("Commodities removed", 0, moveHeaderBO.MovementDetails[0].Containers[0].Commodities.Count);

			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			headerBO = reader.ReadIntoBusinessObject();
			moveHeaderBO = (CusInBondMoveHeader)headerBO.MovementHeaders[0];
			AssertEquals("Containers removed", 0, moveHeaderBO.MovementDetails[0].Containers.Count);

			amsMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>();
			headerBO = reader.ReadIntoBusinessObject();
			moveHeaderBO = (CusInBondMoveHeader)headerBO.MovementHeaders[0];
			AssertEquals("MovementDetails removed", 0, moveHeaderBO.MovementDetails.Count);

			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>());
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("MovementHeaders removed", 0, headerBO.MovementHeaders.Count);

			billDataObject1.CustomsReferenceCollection = new List<CustomsReference>();
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("ShipmentReferenceDetails removed", 0, headerBO.Bills[0].ShipmentReferenceDetails.Count);
			AssertEquals("SecondaryNotifyParties removed", 0, headerBO.Bills[0].SecondaryNotifyParties.Count);

			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("Bills removed", 0, headerBO.Bills.Count);
		}

		public void TestMatchForMVOCC()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			header.BH_ETA = new ZDateTime(2018, 3, 30);
			header.BH_ImportConveyanceName = "A SHIP";
			header.BH_VoyageNumber = "111";
			Factory.SaveForTesting();

			var headerDataObject = SetupInBondHeader();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("Matched AMS found by Vessel and Voyage", header, headerBO);

			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			header2.BH_ETA = new ZDateTime(2018, 4, 30);
			header2.BH_ImportConveyanceName = "A SHIP";
			header2.BH_VoyageNumber = "111";
			Factory.SaveForTesting();

			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNull("Multiple matched AMS found by Vessel and Voyage", headerBO);

			header2.BH_ETA = new ZDateTime(2019, 4, 30);
			Factory.SaveForTesting();
			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(header, headerBO);

			header.BH_VoyageNumber = "1112";
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			inbondMoveHeader.InBondNumber = "006000073";
			Factory.SaveForTesting();
			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNotEquals("No matched AMS job found, system will create a new AMS", header, headerBO);

			var inbMoveHeaderDataObject = SetupInBondMoveHeader("006000073", "62", "N", "12-3456789cd", "A3", "1102", "60201", "TEST VESSEL2", CusAgent.GS_Code, "TOL22031802", "TOL2", new ZDateTime(2018, 3, 21, 15, 0, 0),
	"TOL CITY 2", USStatesList.Codes.Alaska, new ZDateTime(2018, 03, 20), new ZDateTime(2018, 03, 26), new ZDateTime(2018, 03, 25), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty, null, GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress);
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { inbMoveHeaderDataObject });
			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("Match AMS job found by InBond Number", header, headerBO);
		}

		public void TestMatchForNVOCC()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.OceanBill.B0_IssuerCode = "APLU";
			header.OceanBill.B0_MasterBillNumber = "5358786781";
			Factory.SaveForTesting();

			var headerDataObject = SetupInBondHeader();
			headerDataObject.MessageType = new CodeDescriptionPair() { Code = DirectionTypeList.Codes.NVOCC };
			var billDataObject1 = SetupInBondBill("APLU", "5358786781", 0m, 1);
			billDataObject1.AddInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.ShipmentType, Value = CusInBondBill.OceanBillType });
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject1 });

			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("Matched AMS found by Ocean Bill details", header, headerBO);

			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header2.OceanBill.B0_IssuerCode = "APLU";
			header2.OceanBill.B0_MasterBillNumber = "5358786781";
			Factory.SaveForTesting();

			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNull("Multiple matched AMS found by Ocean Bill details", headerBO);

			header.OceanBill.B0_MasterBillNumber = "5358786782";
			header2.OceanBill.B0_MasterBillNumber = "5358786783";
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			inbondMoveHeader.InBondNumber = "006000073";
			Factory.SaveForTesting();
			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNotEquals("No matched AMS job found, system will create a new AMS", header, headerBO);

			var inbMoveHeaderDataObject = SetupInBondMoveHeader("006000073", "62", "N", "12-3456789cd", "A3", "1102", "60201", "TEST VESSEL2", CusAgent.GS_Code, "TOL22031802", "TOL2", new ZDateTime(2018, 3, 21, 15, 0, 0),
	"TOL CITY 2", USStatesList.Codes.Alaska, new ZDateTime(2018, 03, 20), new ZDateTime(2018, 03, 26), new ZDateTime(2018, 03, 25), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty, null, GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress);
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { inbMoveHeaderDataObject });
			reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("Match AMS job found by InBond Number", header, headerBO);
		}

		protected override DataContextType ContextType
		{
			get { return DataContextType.USAMS; }
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override DataTransfer.Universal.InBondDataObjectReaderHelper CreateHelper(UniversalObjectFactory factory)
		{
			return new InBondDataObjectReaderHelper(factory);
		}

		protected override DataTransfer.Universal.CusInBondHeaderDataObjectReader<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> InBondHeaderReader(Shipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
		{
			return new CusInBondHeaderDataObjectReader(headerDataObject, logger, factory, parentBO);
		}

		protected override void SetupInBondMoveHeader(CusInBondMoveHeader moveHeader)
		{
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
		}

		protected override Customs.Business.CusInBondMoveHeaderCollection GetInBondMoveHeaderCollection(CusInBondHeader header)
		{
			return header.InBondMovementHeaders;
		}

		protected override Shipment SetupInBondHeader()
		{
			return SetupInBondHeader(InBondBranch, Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea, Enterprise.Customs.US.Business.ContainerModeList.Codes.Containerized, "OTT1", YesNoList.Codes.Yes,
				YesNoList.Codes.Yes, new ZDateTime(2018, 03, 25), "AUSYD", "USPHL", "1234", new ZDateTime(2018, 04, 01), "A000", "A SHIP", "111", "9240212", "US", "12321421");
		}

		protected override void AssertContents(Customs.Business.CusInBondHeader headerBO)
		{
			AssertContents((CusInBondHeader)headerBO, InBondBranch.PK, InBondTransportModeCodes.Codes.VesselContainer, "OTT1", true, true, "AUSYD", "USPHL", new ZDateTime(2018, 03, 25), "1234", new ZDateTime(2018, 04, 01),
				"A000", "A SHIP", "111", "9240212", "US", "12321421");
		}

		protected override void AssertMatchingToExistingInBondHeaderLogs(ZString actualLogs)
		{
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusInBondHeader.
Information - Populating CusInBondHeader...
Information - Successfully loaded matching CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
Information - Matching 'InBondCarrier':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Information - Matching 'TransferOfLiabilityCarrier':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Deleted AMS Movement Header from UniversalShipment.
Information - Updated AMS C00001000 from UniversalShipment.", actualLogs);
		}

		CustomsReference SetupCustomsReferenceData(ZString type, ZString subType, ZString reference)
		{
			var customsReference = new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = type },
				SubType = new CodeDescriptionPair35Char() { Code = subType },
				Reference = reference
			};

			return customsReference;
		}

		Shipment SetupInBondHeader(GlbBranch branch, ZString transportMode, ZString containerMode, ZString carrierSCAC, ZString isPaperless, ZString isOutbound, ZDateTime firstExportDate,
			ZString portOfLoading, ZString portOfDischarge, ZString scheduleD, ZDateTime eta, ZString firms, ZString vessel, ZString voyage, ZString lloyds, ZString vesselCountry, ZString uniqueVoyageId)
		{
			var result = SetupInBondHeader(branch, lloyds, voyage, transportMode, containerMode, vessel, vesselCountry, portOfLoading, portOfDischarge);
			result.MessageType = new CodeDescriptionPair() { Code = DirectionTypeList.Codes.MVOCC };
			result.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = Constants.Header.AddInfo.CarrierSCAC, Value = carrierSCAC },
				new AddInfo() { Key = Constants.Header.AddInfo.IsPaperlessMIBParticipant, Value = isPaperless },
				new AddInfo() { Key = Constants.Header.AddInfo.IsOutboundCargo, Value = isOutbound },
				new AddInfo() { Key = Constants.Header.AddInfo.PortOfDischargeScheduleD, Value = scheduleD },
				new AddInfo() { Key = Constants.Header.AddInfo.FIRMS, Value = firms },
				new AddInfo() { Key = Constants.Header.AddInfo.UniqueVoyageIdentifier, Value = uniqueVoyageId }
			});
			result.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.Departure, IsEstimate = ZBool.True, Value = firstExportDate },
				new Date() { Type = DateType.Arrival, IsEstimate = ZBool.True, Value = eta },
			});
			return result;
		}

		void AssertContents(CusInBondHeader headerBO, ZGuid branchPK, ZString transportMode, ZString carrierSCAC, ZBool isPaperless, ZBool isOutbound, ZString portOfLoading, ZString portOfDischarge,
			ZDateTime firstExportDate, ZString scheduleD, ZDateTime eta, ZString firms, ZString vessel, ZString voyage, ZString lloyds, ZString vesselCountry, ZString uniqueVoyageId)
		{
			AssertEquals("headerBO.BH_TransitDirection", DirectionTypeList.Codes.MVOCC, headerBO.BH_TransitDirection);
			AssertEquals("headerBO.BH_GB", branchPK, headerBO.BH_GB);
			AssertEquals("headerBO.BH_ImportTransportMode", transportMode, headerBO.BH_ImportTransportMode);
			AssertEquals("headerBO.BH_CarrierSCAC", carrierSCAC, headerBO.BH_CarrierSCAC);
			AssertEquals("headerBO.BH_IsPaperlessMIBParticipant", isPaperless, headerBO.BH_IsPaperlessMIBParticipant);
			AssertEquals("headerBO.BH_IsOutboundCargo", isOutbound, headerBO.BH_IsOutboundCargo);
			AssertEquals("headerBO.BH_RL_NKImportLoadPort", portOfLoading, headerBO.BH_RL_NKImportLoadPort);
			AssertEquals("headerBO.BH_RL_NKPortUnlading", portOfDischarge, headerBO.BH_RL_NKPortUnlading);
			AssertEquals("headerBO.BH_FirstExportDate", firstExportDate, headerBO.BH_FirstExportDate);
			AssertEquals("headerBO.BH_PortUnladingDCode", scheduleD, headerBO.BH_PortUnladingDCode);
			AssertEquals("headerBO.BH_ETA", eta, headerBO.BH_ETA);
			AssertEquals("headerBO.BH_FIRMS", firms, headerBO.BH_FIRMS);
			AssertEquals("headerBO.BH_ImportConveyanceName", vessel, headerBO.BH_ImportConveyanceName);
			AssertEquals("headerBO.BH_VoyageNumber", voyage, headerBO.BH_VoyageNumber);
			AssertEquals("headerBO.BH_LloydsNumber", lloyds, headerBO.BH_LloydsNumber);
			AssertEquals("headerBO.BH_ImportConveyanceCountry", vesselCountry, headerBO.BH_ImportConveyanceCountry);
			AssertEquals("headerBO.BH_UniqueVoyageIdentifier", uniqueVoyageId, headerBO.BH_UniqueVoyageIdentifier);
		}

		ZGuid[] SavePrimaryKeyTempory(CusInBondHeader headerBO, CusInBondMoveHeader moveHeaderBO)
		{
			return new ZGuid[]
			{
				headerBO.Bills[0].PK,
				headerBO.Bills[0].ShipmentReferenceDetails[0].PK,
				headerBO.Bills[0].SecondaryNotifyParties[0].PK,
				headerBO.MovementHeaders[0].PK,
				headerBO.MovementHeaders[0].MovementDetails[0].PK,
				moveHeaderBO.MovementDetails[0].Containers[0].PK,
				moveHeaderBO.MovementDetails[0].Containers[0].Vehicles[0].PK,
				moveHeaderBO.MovementDetails[0].Containers[0].UNDGs[0].PK,
				moveHeaderBO.MovementDetails[0].Containers[0].Commodities[0].PK
			};
		}
	}
}
