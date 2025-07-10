using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest
	{
		public void TestImportingInBondDataForWarehouse()
		{
			var headerDataObject = SetupInBondHeader(DataContextType.WarehouseInBond);
			SetupWarehouseData(headerDataObject);
			headerDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			headerDataObject.NoteCollection.Add(SetupNote());
			headerDataObject.NoteCollection.Add(SetupNote2());
			var bill1DataObject = SetupInBondBill("HB8953", WayBillTypeList.Codes.Master, ZString.Empty, 1);
			bill1DataObject.CustomsReferenceCollection = new List<CustomsReference>(new[] { SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.IN, "IN324"), SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.CG, "CG986"), SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.BL, "BL362") });
			var bill2DataObject = SetupInBondBill("HB2343", WayBillTypeList.Codes.Master, ZString.Empty, 2);
			bill2DataObject.CustomsReferenceCollection = new List<CustomsReference>(new[] { SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.ED, "ED356"), });
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { bill1DataObject, bill2DataObject }));
			var container1DataObject = SetupContainer("CONT1", 1);
			var container2DataObject = SetupContainer("CONT2", 2);
			var container3DataObject = SetupContainer("CONT2", 3);
			var container4DataObject = SetupContainer("CONT2", 4);
			var container5DataObject = SetupContainer("CONT1", 5);
			var container6DataObject = SetupContainer("CONT2", 6);
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { container1DataObject, container2DataObject, container3DataObject, container4DataObject, container5DataObject, container6DataObject }));
			var packingLine1DataObject = SetupPackingLine("101010", 1);
			var packingLine2DataObject = SetupPackingLine("202020", 1);
			var packingLine3DataObject = SetupPackingLine("201010", 2);
			var packingLine4DataObject = SetupPackingLine("302020", 3);
			var packingLine5DataObject = SetupPackingLine("401010", 4);
			var packingLine6DataObject = SetupPackingLine("502020", 5);
			var packingLine7DataObject = SetupPackingLine("501010", 5);
			var packingLine8DataObject = SetupPackingLine("202020", 6);
			headerDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine1DataObject, packingLine2DataObject, packingLine3DataObject, packingLine4DataObject, packingLine5DataObject, packingLine6DataObject, packingLine7DataObject, packingLine8DataObject }));
			headerDataObject.PackingLineCollection.Content = CollectionContent.Complete;
			var inBondMoveHeader1DataObject = SetupInBondMoveHeader("IT302322");
			var inBondMoveDetail1DataObject = SetupInBondMoveDetail(2, "0001");
			inBondMoveDetail1DataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)2, ContainerNumber = "CONT2" } });
			var inBondMoveDetail2DataObject = SetupInBondMoveDetail(1, "0002");
			inBondMoveDetail2DataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)1, ContainerNumber = "CONT1" }, new ContainerLink()
			{ Link = (ZInt)3, ContainerNumber = "CONT2" } });
			inBondMoveHeader1DataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>(new[] { inBondMoveDetail1DataObject, inBondMoveDetail2DataObject });
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>(new[] { inBondMoveHeader1DataObject }));
			Factory.SaveForTesting();
			var reader = new WarehouseInBondDataObjectReader(headerDataObject, logger, Factory);
			var moveHeaderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(moveHeaderBO);
			CombineAssertions(delegate
			{
				var headerBO = moveHeaderBO.Header;
				AssertContents(headerBO);
				AssertEquals("headerBO.Bills.Count", 2, headerBO.Bills.Count);
				var bill1BO = headerBO.Bills.OfType<CusInBondBill>().First(x => x.B0_MasterBillNumber == "HB2343");
				var bill2BO = headerBO.Bills.OfType<CusInBondBill>().First(x => x.B0_MasterBillNumber == "HB8953");
				AssertEquals("bill1BO.AdditionalReferences.Count", 1, bill1BO.AdditionalReferences.Count);
				AssertContains(ReferenceQualifierList.Codes.ED, "ED356", bill1BO.AdditionalReferences);
				AssertEquals("bill2BO.AdditionalReferences.Count", 3, bill2BO.AdditionalReferences.Count);
				AssertContains(ReferenceQualifierList.Codes.IN, "IN324", bill2BO.AdditionalReferences);
				AssertContains(ReferenceQualifierList.Codes.CG, "CG986", bill2BO.AdditionalReferences);
				AssertContains(ReferenceQualifierList.Codes.BL, "BL362", bill2BO.AdditionalReferences);
				AssertEquals("headerBO.MovementHeaders.Count", 1, headerBO.MovementHeaders.Count);
				var moveHeader1BO = headerBO.MovementHeaders[0];
				AssertEquals(moveHeaderBO, moveHeader1BO);
				AssertCusInBondMoveHeaderContents(moveHeader1BO);
				AssertWarehouseContents(moveHeader1BO);
				AssertEquals("moveHeader1BO.MovementDetails.Count", 2, moveHeader1BO.MovementDetails.Count);
				var moveHeader1MoveDetail1BO = moveHeader1BO.MovementDetails.First(x => x.B9_B0 == bill2BO.PK);
				AssertCusInBondMoveDetailContents(moveHeader1MoveDetail1BO, "0002");
				AssertEquals("moveHeader1MoveDetail1BO.Containers.Count", 2, moveHeader1MoveDetail1BO.Containers.Count);
				var moveHeader1MoveDetail1Container1BO = moveHeader1MoveDetail1BO.Containers.First(x => x.BC_ContainerNum == "CONT1");
				AssertCusInBondContainerContents(moveHeader1MoveDetail1Container1BO, "CONT1");
				AssertEquals("moveHeader1MoveDetail1Container1BO.Commodities.Count", 2, moveHeader1MoveDetail1Container1BO.Commodities.Count);
				AssertCusInBondCargoDescContents(moveHeader1MoveDetail1Container1BO.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_HarmonisedTariff == "101010"), "101010");
				AssertCusInBondCargoDescContents(moveHeader1MoveDetail1Container1BO.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_HarmonisedTariff == "202020"), "202020");
				var moveHeader1MoveDetail1Container2BO = moveHeader1MoveDetail1BO.Containers.First(x => x.BC_ContainerNum == "CONT2");
				AssertCusInBondContainerContents(moveHeader1MoveDetail1Container2BO, "CONT2");
				AssertEquals("moveHeader1MoveDetail1Container2BO.Commodities.Count", 1, moveHeader1MoveDetail1Container2BO.Commodities.Count);
				AssertCusInBondCargoDescContents(moveHeader1MoveDetail1Container2BO.Commodities[0], "302020");
				var moveHeader1MoveDetail2BO = moveHeader1BO.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill1BO.PK);
				AssertCusInBondMoveDetailContents(moveHeader1MoveDetail2BO, "0001");
				AssertEquals("moveHeader1MoveDetail2BO.Containers.Count", 1, moveHeader1MoveDetail2BO.Containers.Count);
				var moveHeader1MoveDetail2ContainerBO = moveHeader1MoveDetail2BO.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2");
				AssertCusInBondContainerContents(moveHeader1MoveDetail2ContainerBO, "CONT2");
				AssertEquals("moveHeader1MoveDetail2ContainerBO.Commodities.Count", 1, moveHeader1MoveDetail2ContainerBO.Commodities.Count);
				AssertCusInBondCargoDescContents(moveHeader1MoveDetail2ContainerBO.Commodities[0], "201010");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusInBondMoveHeader found, creating new CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: WORM EATER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Matching 'ImporterDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - No matching CusInBondBill found, creating new CusInBondBill.
Information - Populating CusInBondBill...
Information - No matching CusInBondBill found, creating new CusInBondBill.
Information - Populating CusInBondBill...
Information - Matching 'CustomsWarehouseAddress':- Matched to 'INTHEMSYD' by code, address 'THEMOMENT' (only address).
Information - Matching 'InBondCarrier':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Information - Matching 'TransferOfLiabilityCarrier':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - No matching CusInBondMoveDetail found, creating new CusInBondMoveDetail.
Information - Populating CusInBondMoveDetail...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...
Information - No matching CusInBondMoveDetail found, creating new CusInBondMoveDetail.
Information - Populating CusInBondMoveDetail...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...
Information - Added In-Bond Movement Header IT302322 from UniversalShipment.".Trim(), logger.Logs);
			});
		}

		public void TestMatchingToExistingInBondHeaderForWarehouse()
		{
			var newFactory = new BusinessObjectFactory();
			var headerWithoutInBondNumber = newFactory.New<CusInBondHeader>();
			var existingHeader1 = newFactory.New<CusInBondHeader>();
			var existingHeader1MoveHeader = existingHeader1.MovementHeaders.AddNew();
			existingHeader1MoveHeader.InBondNumber = "IB2343";
			var existingHeader2 = newFactory.New<CusInBondHeader>();
			var existingHeader2MoveHeader = existingHeader2.MovementHeaders.AddNew();
			existingHeader2MoveHeader.InBondNumber = "IB2343";
			newFactory.Save();
			headerWithoutInBondNumber.BH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);
			existingHeader1.BH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			existingHeader2.BH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			newFactory.Save();
			var existingHeader1Bill1 = existingHeader1.Bills.AddNew();
			existingHeader1Bill1.B0_MasterBillNumber = "HB2343";
			var existingHeader1Bill2 = existingHeader1.Bills.AddNew();
			existingHeader1Bill2.B0_MasterBillNumber = "HB8953";
			var existingHeader1Bill3 = existingHeader1.Bills.AddNew();
			existingHeader1Bill3.B0_MasterBillNumber = "HB1TOBEDELETED";
			var existingHeader2Bill1 = existingHeader2.Bills.AddNew();
			existingHeader2Bill1.B0_MasterBillNumber = "HB2343";
			var existingHeader2Bill2 = existingHeader2.Bills.AddNew();
			existingHeader2Bill2.B0_MasterBillNumber = "HB8953";
			var existingHeader2Bill3 = existingHeader2.Bills.AddNew();
			existingHeader2Bill3.B0_MasterBillNumber = "HB1TOBEDELETED";
			var headerWithoutInBondNumberBill1 = headerWithoutInBondNumber.Bills.AddNew();
			headerWithoutInBondNumberBill1.B0_MasterBillNumber = "HB2343";
			var headerWithoutInBondNumberBill2 = headerWithoutInBondNumber.Bills.AddNew();
			headerWithoutInBondNumberBill2.B0_MasterBillNumber = "HB8953";
			var headerWithoutInBondNumberBill3 = headerWithoutInBondNumber.Bills.AddNew();
			headerWithoutInBondNumberBill3.B0_MasterBillNumber = "HB1TOBEDELETED";
			var existingHeader2Bill1Ref1 = existingHeader2Bill1.AdditionalReferences.AddNew();
			existingHeader2Bill1Ref1.BR_Qualifier = ReferenceQualifierList.Codes.BL;
			existingHeader2Bill1Ref1.BR_ReferenceNum = "REF1";
			var existingHeader2Bill1Ref2 = existingHeader2Bill1.AdditionalReferences.AddNew();
			existingHeader2Bill1Ref2.BR_Qualifier = ReferenceQualifierList.Codes.BL;
			existingHeader2Bill1Ref2.BR_ReferenceNum = "REF2";
			newFactory.Save();
			var headerDataObject = SetupInBondHeader();
			SetupWarehouseData(headerDataObject);
			var billDataObject = SetupInBondBill("HB2343", WayBillTypeList.Codes.Master, ZString.Empty, 1);
			billDataObject.CustomsReferenceCollection = new List<CustomsReference>(new[]
			{
				SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.IN, "IN324"),
				SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.CG, "CG986"),
				SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.BL, "REF2")
			});
			var billDataObject2 = SetupInBondBill("HB6935", WayBillTypeList.Codes.Master, ZString.Empty, 2);
			var billDataObject3 = SetupInBondBill("HB8953", WayBillTypeList.Codes.Master, ZString.Empty, 3);
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { billDataObject, billDataObject2, billDataObject3 }));
			var inBondMoveHeaderDataObject1 = SetupInBondMoveHeader("IB693584");
			var inBondMoveHeaderDataObject2 = SetupInBondMoveHeader("IB2343");
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>(new[] { inBondMoveHeaderDataObject1, inBondMoveHeaderDataObject2 }));
			var reader = new WarehouseInBondDataObjectReader(headerDataObject, logger, Factory);
			var moveHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertNull("Invalid data for WarehouseInBond - Multiple movement", moveHeaderBO);
			AssertMultilineASCIIEquals("logger.Logs", @"
Error - Cannot populate CusInBondMoveHeader because:
Warehouse InBond must have one element in InBondMoveHeaderCollection".Trim(), logger.Logs);
			logger.ClearLogs();
			headerDataObject.InBondMoveHeaderCollection.Remove(inBondMoveHeaderDataObject1);
			Factory.BOFactory.ClearQueryCache();
			reader = new WarehouseInBondDataObjectReader(headerDataObject, logger, Factory);
			moveHeaderBO = reader.ReadIntoBusinessObject();
			AssertNull("Multiple job matched", moveHeaderBO);
			AssertMultilineASCIIEquals("logger.Logs", @"
Error - Cannot populate CusInBondMoveHeader because:
Multiple jobs were matched with InBond Number (IB2343)".Trim(), logger.Logs);
			headerDataObject.InBondMoveHeaderCollection[0].EntryNumberCollection[0].Type.Code = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			logger.ClearLogs();
			headerDataObject.InBondMoveHeaderCollection.Remove(inBondMoveHeaderDataObject1);
			Factory.BOFactory.ClearQueryCache();
			reader = new WarehouseInBondDataObjectReader(headerDataObject, logger, Factory);
			moveHeaderBO = reader.ReadIntoBusinessObject();
			AssertNull("No InBond Number", moveHeaderBO);
			AssertMultilineASCIIEquals("logger.Logs", @"
Error - Cannot populate CusInBondMoveHeader because:
Warehouse InBond must have an InBond Number specified".Trim(), logger.Logs);
			headerDataObject.InBondMoveHeaderCollection[0].EntryNumberCollection[0].Type.Code = CusEntryHeaderMessageTypeList.Codes.InBond;
			existingHeader1MoveHeader.InBondNumber = "IB23294";
			newFactory.Save();
			Factory.BOFactory.ClearQueryCache();
			logger.ClearLogs();
			reader = new WarehouseInBondDataObjectReader(headerDataObject, logger, Factory);
			moveHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);
			AssertNotNull(moveHeaderBO);
			CombineAssertions(delegate
			{
				var headerBO = moveHeaderBO.Header;
				AssertEquals("Should have been matched to existingHeader2", existingHeader2.PK, headerBO.PK);
				AssertContents(headerBO);
				AssertEquals("headerBO.BH_OverrideFreightDefaults", false, headerBO.BH_OverrideFreightDefaults);
				AssertEquals("headerBO.Bills.Count", 3, headerBO.Bills.Count);
				existingHeader2Bill1 = Factory.Load<CusInBondBill>(existingHeader2Bill1.PK);
				AssertNotNull("existingHeader2Bill1", existingHeader2Bill1);
				existingHeader2Bill2 = Factory.Load<CusInBondBill>(existingHeader2Bill2.PK);
				AssertNotNull("existingHeader2Bill2", existingHeader2Bill2);
				AssertNull("existingHeader2Bill3.IsDeleted", Factory.Load<CusInBondBill>(existingHeader2Bill3.PK));
				var billBO1 = headerBO.Bills.OfType<CusInBondBill>().FirstOrDefault(x => x.B0_MasterBillNumber == "HB2343");
				var billBO2 = headerBO.Bills.OfType<CusInBondBill>().FirstOrDefault(x => x.B0_MasterBillNumber == "HB6935");
				var billBO3 = headerBO.Bills.OfType<CusInBondBill>().FirstOrDefault(x => x.B0_MasterBillNumber == "HB8953");
				AssertEquals("billBO1 should have been matched to existingHeader2Bill1", existingHeader2Bill1.PK, billBO1.PK);
				AssertCusInBondBillContents(billBO1, "HB2343", ZString.Empty);
				AssertEquals("billBO3 should have been matched to existingHeader2Bill2", existingHeader2Bill2.PK, billBO3.PK);
				AssertCusInBondBillContents(billBO3, "HB8953", ZString.Empty);
				AssertCusInBondBillContents(billBO2, "HB6935", ZString.Empty);
				existingHeader2Bill1Ref1 = Factory.Load<CusInbondBillAddRef>(existingHeader2Bill1Ref1.PK);
				AssertNull("existingHeader2Bill1Ref1 should have been deleted", existingHeader2Bill1Ref1);
				existingHeader2Bill1Ref2 = Factory.Load<CusInbondBillAddRef>(existingHeader2Bill1Ref2.PK);
				AssertNull("existingHeader2Bill1Ref2 should have been deleted", existingHeader2Bill1Ref2);
				AssertEquals("headerBO.MovementHeaders", 1, headerBO.MovementHeaders.Count);
				var inBondMoveHeaderBO1 = headerBO.MovementHeaders[0];
				AssertEquals("inBondMoveHeaderBO1 should have been matched to existingHeader2MoveHeader", existingHeader2MoveHeader.PK, inBondMoveHeaderBO1.PK);
				AssertEquals("inBondMoveHeaderBO1.InBondNumber", "IB2343", inBondMoveHeaderBO1.InBondNumber);
				AssertCusInBondMoveHeaderContents(inBondMoveHeaderBO1);
				AssertWarehouseContents(inBondMoveHeaderBO1);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
Information - Matching 'ImporterDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Successfully loaded matching CusInBondBill.
Information - Populating CusInBondBill...
Information - No matching CusInBondBill found, creating new CusInBondBill.
Information - Populating CusInBondBill...
Information - Successfully loaded matching CusInBondBill.
Information - Populating CusInBondBill...
Information - Deleted Bill Of Lading HB1TOBEDELETED from UniversalShipment.
Information - Matching 'CustomsWarehouseAddress':- Matched to 'INTHEMSYD' by code, address 'THEMOMENT' (only address).
Information - Matching 'InBondCarrier':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Information - Matching 'TransferOfLiabilityCarrier':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Updated In-Bond Movement Header IB2343 from UniversalShipment.
Information - Successfully saved In-Bond Movement Header IB2343 with 3 x CusInBondBill.
".Trim(), logger.Logs);
			});
		}

		protected override void AssertMatchingToExistingInBondHeaderLogs(ZString actualLogs)
		{
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusInBondHeader.
Information - Populating CusInBondHeader...
Information - Matching 'ImporterDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Successfully loaded matching CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
Information - Matching 'InBondCarrier':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Information - Matching 'TransferOfLiabilityCarrier':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Updated In-Bond  from UniversalShipment.", actualLogs);
		}

		void SetupWarehouseData(Shipment shipmentData)
		{
			SetupAddressData(shipmentData, Warehouse, DocAddressType.CustomsWarehouseAddress);
			shipmentData.SetAddInfoCollection(() =>
			{
				var addInfoCollection = shipmentData.AddInfoCollection ?? new List<AddInfo>();
				addInfoCollection.Add(new AddInfo()
				{ Key = JobDeclaration.Schema.US_WHSEntryFilerCode.Substring(3), Value = "XJ5" });
				addInfoCollection.Add(new AddInfo()
				{ Key = JobDeclaration.Schema.US_WHSEntryNumber.Substring(3), Value = "ENT221121" });
				return addInfoCollection;
			});
			shipmentData.CommercialInfo = new CommercialInfo()
			{
				Name = "WAREHOUSE",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { new CommercialInvoiceHeader()
			{//InvoiceNumber = "WAREHOUSE",
			//CommercialInvoiceLineCollection = new List<CommercialInvoiceLine>(new[] { SetupProductLine(), SetupProductLine2( )})
			} })
			};
		}

		void AssertWarehouseContents(CusInBondMoveHeader moveHeaderBO)
		{
			AssertEquals("moveHeaderBO.BM_OA_WarehouseAddress", Warehouse.MainAddress.PK, moveHeaderBO.BM_OA_WarehouseAddress);
			//moveHeaderBO.ProductLines.Load();
			//AssertEquals("moveHeaderBO.ProductLines.Count", 2, moveHeaderBO.ProductLines.Count);
			//var productLineBO1 = moveHeaderBO.ProductLines[0];
			//var productLineBO2 = moveHeaderBO.ProductLines[1];
			//if (productLineBO2.US_PartNo == Part.OP_PartNum)
			//{
			//	productLineBO1 = moveHeaderBO.ProductLines[1];
			//	productLineBO2 = moveHeaderBO.ProductLines[0];
			//}
			//AssertCusInBondProductLineContents(productLineBO1);
			//AssertCusInBondProductLineContents2(productLineBO2);
		}
	}
}
