using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest : DataTransfer.Universal.Testing.InBondDataObjectReaderTest<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public void TestImportingInBondHeaderDataDateFallback()
		{
			var headerDataObject = SetupInBondHeader();
			headerDataObject.SetDateCollection(() => new List<Date>());
			var loadingActual = Date.New(DateType.LoadingDate, ZBool.True, new ZDateTime(2010, 1, 5));
			headerDataObject.DateCollection.Add(loadingActual);
			var loadingEstimate = Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2010, 1, 2));
			headerDataObject.DateCollection.Add(loadingEstimate);
			var departureActual = Date.New(DateType.Departure, ZBool.True, new ZDateTime(2010, 1, 4));
			headerDataObject.DateCollection.Add(departureActual);
			var departureEstimate = Date.New(DateType.Departure, ZBool.False, new ZDateTime(2010, 1, 3));
			headerDataObject.DateCollection.Add(departureEstimate);
			var dischargeActual = Date.New(DateType.DischargeDate, ZBool.True, new ZDateTime(2010, 1, 6));
			headerDataObject.DateCollection.Add(dischargeActual);
			var dischargeEstimate = Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2010, 1, 9));
			headerDataObject.DateCollection.Add(dischargeEstimate);
			var arrivalActual = Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2010, 1, 7));
			headerDataObject.DateCollection.Add(arrivalActual);
			var arrivalEstimate = Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2010, 1, 8));
			headerDataObject.DateCollection.Add(arrivalEstimate);
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.BH_ETD", new ZDateTime(2010, 1, 3), headerBO.BH_SailingDate);
			AssertEquals("headerBO.BH_ETA", new ZDateTime(2010, 1, 8), headerBO.BH_ETA);
			headerDataObject.DateCollection.Remove(departureEstimate);
			headerDataObject.DateCollection.Remove(arrivalEstimate);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.BH_ETD", new ZDateTime(2010, 1, 4), headerBO.BH_SailingDate);
			AssertEquals("headerBO.BH_ETA", new ZDateTime(2010, 1, 7), headerBO.BH_ETA);
			headerDataObject.DateCollection.Remove(departureActual);
			headerDataObject.DateCollection.Remove(arrivalActual);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.BH_ETD", new ZDateTime(2010, 1, 2), headerBO.BH_SailingDate);
			AssertEquals("headerBO.BH_ETA", new ZDateTime(2010, 1, 9), headerBO.BH_ETA);
			headerDataObject.DateCollection.Remove(loadingEstimate);
			headerDataObject.DateCollection.Remove(dischargeEstimate);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals("headerBO.BH_ETD", new ZDateTime(2010, 1, 5), headerBO.BH_SailingDate);
			AssertEquals("headerBO.BH_ETA", new ZDateTime(2010, 1, 6), headerBO.BH_ETA);
		}

		public void TestImportingInBondData()
		{
			var headerDataObject = SetupInBondHeader();
			headerDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			headerDataObject.NoteCollection.Add(SetupNote());
			headerDataObject.NoteCollection.Add(SetupNote2());
			var bill1DataObject = SetupInBondBill("HB8953", WayBillTypeList.Codes.Master, ZString.Empty, 1);
			bill1DataObject.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[] { SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.IN, "IN324"), SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.CG, "CG986"), SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.BL, "BL362") });
			var bill2DataObject = SetupInBondBill("HB2343", WayBillTypeList.Codes.Master, ZString.Empty, 2);
			bill2DataObject.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[] { SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.ED, "ED356"), });
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
			inBondMoveHeader1DataObject.InBondMoveDetailCollection = new List<UniversalCustoms.InBondMoveDetail>(new[] { inBondMoveDetail1DataObject, inBondMoveDetail2DataObject });
			var inBondMoveHeader2DataObject = SetupInBondMoveHeader("IT896588");
			var inBondMoveDetail3DataObject = SetupInBondMoveDetail(2, "0001");
			inBondMoveDetail3DataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)4, ContainerNumber = "CONT2" } });
			var inBondMoveDetail4DataObject = SetupInBondMoveDetail(1, "0002");
			inBondMoveDetail4DataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)5, ContainerNumber = "CONT1" }, new ContainerLink()
			{ Link = (ZInt)6, ContainerNumber = "CONT2" } });
			inBondMoveHeader2DataObject.InBondMoveDetailCollection = new List<UniversalCustoms.InBondMoveDetail>(new[] { inBondMoveDetail3DataObject, inBondMoveDetail4DataObject });
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<UniversalCustoms.InBondMoveHeader>(new[] { inBondMoveHeader1DataObject, inBondMoveHeader2DataObject }));
			Factory.SaveForTesting();
			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			CombineAssertions(delegate
			{
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
				AssertEquals("headerBO.MovementHeaders.Count", 2, headerBO.MovementHeaders.Count);
				var moveHeader1BO = headerBO.MovementHeaders.OfType<CusInBondMoveHeader>().First(x => x.InBondNumber == "IT302322");
				AssertCusInBondMoveHeaderContents(moveHeader1BO);
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
				var moveHeader2BO = headerBO.MovementHeaders.OfType<CusInBondMoveHeader>().First(x => x.InBondNumber == "IT896588");
				AssertCusInBondMoveHeaderContents(moveHeader2BO);
				AssertEquals("moveHeader2BO.MovementDetails.Count", 2, moveHeader2BO.MovementDetails.Count);
				var moveHeader2MoveDetail1BO = moveHeader2BO.MovementDetails.First(x => x.B9_B0 == bill2BO.PK);
				AssertCusInBondMoveDetailContents(moveHeader2MoveDetail1BO, "0002");
				AssertEquals("moveHeader2MoveDetail1BO.Containers.Count", 2, moveHeader2MoveDetail1BO.Containers.Count);
				var moveHeader2MoveDetail1Container1BO = moveHeader2MoveDetail1BO.Containers.First(x => x.BC_ContainerNum == "CONT1");
				AssertCusInBondContainerContents(moveHeader2MoveDetail1Container1BO, "CONT1");
				AssertEquals("moveHeader2MoveDetail1Container1BO.Commodities.Count", 2, moveHeader2MoveDetail1Container1BO.Commodities.Count);
				AssertCusInBondCargoDescContents(moveHeader2MoveDetail1Container1BO.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_HarmonisedTariff == "501010"), "501010");
				AssertCusInBondCargoDescContents(moveHeader2MoveDetail1Container1BO.Commodities.OfType<CusInBondCargoDesc>().First(x => x.BY_HarmonisedTariff == "502020"), "502020");
				var moveHeader2MoveDetail1Container2BO = moveHeader2MoveDetail1BO.Containers.First(x => x.BC_ContainerNum == "CONT2");
				AssertCusInBondContainerContents(moveHeader2MoveDetail1Container2BO, "CONT2");
				AssertEquals("moveHeader2MoveDetail1Container2BO.Commodities.Count", 1, moveHeader2MoveDetail1Container2BO.Commodities.Count);
				AssertCusInBondCargoDescContents(moveHeader2MoveDetail1Container2BO.Commodities[0], "202020");
				var moveHeader2MoveDetail2BO = moveHeader2BO.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill1BO.PK);
				AssertCusInBondMoveDetailContents(moveHeader2MoveDetail2BO, "0001");
				AssertEquals("moveHeader2MoveDetail2BO.Containers.Count", 1, moveHeader2MoveDetail2BO.Containers.Count);
				var moveHeader2MoveDetail2ContainerBO = moveHeader2MoveDetail2BO.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2");
				AssertCusInBondContainerContents(moveHeader2MoveDetail2ContainerBO, "CONT2");
				AssertEquals("moveHeader2MoveDetail2ContainerBO.Commodities.Count", 1, moveHeader2MoveDetail2ContainerBO.Commodities.Count);
				AssertCusInBondCargoDescContents(moveHeader2MoveDetail2ContainerBO.Commodities[0], "401010");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusInBondHeader found, creating new CusInBondHeader.
Information - Populating CusInBondHeader...
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
Information - No matching CusInBondMoveHeader found, creating new CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
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
Information - No matching CusInBondMoveHeader found, creating new CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
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
Information - Added  from UniversalShipment.", logger.Logs);
			});
		}

		protected override DataContextType ContextType => DataContextType.InBond;

		protected new InBondDataObjectReaderHelper Helper => (InBondDataObjectReaderHelper)base.Helper;

		protected override DataTransfer.Universal.InBondDataObjectReaderHelper CreateHelper(UniversalObjectFactory factory) => new InBondDataObjectReaderHelper(factory);

		protected override DataTransfer.Universal.CusInBondHeaderDataObjectReader<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> InBondHeaderReader(Shipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
		{
			return new CusInBondHeaderDataObjectReader(headerDataObject, logger, factory, parentBO);
		}

		protected override Shipment SetupInBondHeader()
		{
			return SetupInBondHeader(InBondBranch, TransportTypeList.Codes.Sea, ContainerModeList.Codes.Containerized, APLVessel.RV_Code, Core.Constants.CountryCodes.Jamaica,
				APLVessel.RV_LloydsNumber, "V324", SeaForeignPort1.RL_Code, SeaLocalPort1.RL_Code, new ZDateTime(2014, 2, 10), new ZDateTime(2014, 1, 19), ZBool.True, CarrierCode1,
				SeaLocalPort1ScheduleD.ZZD_Code, "F320", InBondHeaderTypeList.Codes.FullData, SeaForeignPort1ScheduleK.ZZD_Code, Importer.MainAddress, Core.Constants.CountryCodes.Australia,
				new ZDateTime(2014, 1, 20));
		}

		protected override void AssertContents(Customs.Business.CusInBondHeader headerBO)
		{
			AssertContents((CusInBondHeader)headerBO, InBondBranch.PK, InBondTransportModeCodes.Codes.VesselContainer, APLVessel.RV_Code, Core.Constants.CountryCodes.Jamaica,
				APLVessel.RV_LloydsNumber, "V324", new ZDateTime(2014, 2, 10), new ZDateTime(2014, 1, 19), ZBool.True, CarrierCode1, SeaLocalPort1ScheduleD.ZZD_Code, "F320",
				InBondHeaderTypeList.Codes.FullData, SeaForeignPort1ScheduleK.ZZD_Code, Importer.MainAddress.PK, Core.Constants.CountryCodes.Australia, new ZDateTime(2014, 1, 20));
		}

		Shipment SetupInBondHeader(DataContextType? dataContextType = null)
		{
			var result = SetupInBondHeader(InBondBranch, TransportTypeList.Codes.Sea, ContainerModeList.Codes.Containerized, APLVessel.RV_Code, Core.Constants.CountryCodes.Jamaica,
				APLVessel.RV_LloydsNumber, "V324", SeaForeignPort1.RL_Code, SeaLocalPort1.RL_Code, new ZDateTime(2014, 2, 10), new ZDateTime(2014, 1, 19), ZBool.True, CarrierCode1,
				SeaLocalPort1ScheduleD.ZZD_Code, "F320", InBondHeaderTypeList.Codes.FullData, SeaForeignPort1ScheduleK.ZZD_Code, Importer.MainAddress, Core.Constants.CountryCodes.Australia,
				new ZDateTime(2014, 1, 20));
			result.DataContext.AddDataSource(dataContextType ?? DataContextType.InBond, null);
			return result;
		}

		Shipment SetupInBondHeader(GlbBranch branch, ZString transportMode, ZString containerMode, ZString vessel, ZString vesselCountryOfRegistration, ZString lloyds, ZString voyage,
			ZString portOfLoading, ZString portOfDischarge, ZDateTime eta, ZDateTime sailingDate, ZBool ftzMove, ZString carrierSCAC, ZString portUnladingDCode, ZString fIRMS, ZString headerType,
			ZString importLoadPortKCode, OrgAddress importerAddress, ZString firstExportCountry, ZDateTime firstExportDate)
		{
			var result = SetupInBondHeader(branch, lloyds, voyage, transportMode, containerMode, vessel, vesselCountryOfRegistration, portOfLoading, portOfDischarge);
			result.SetAddInfoCollection(() => new List<AddInfo>()
			{ new AddInfo()
			{ Key = Constants.Header.AddInfo.FTZMove, Value = ftzMove ? Constants.AddInfo.True : Constants.AddInfo.False }, new AddInfo()
			{ Key = Constants.Header.AddInfo.UI_NKCarrierSCAC, Value = carrierSCAC }, new AddInfo()
			{ Key = Constants.Header.AddInfo.SchDArrival, Value = portUnladingDCode }, new AddInfo()
			{ Key = Constants.Header.AddInfo.US_NKLocationOfGoods, Value = fIRMS }, new AddInfo()
			{ Key = Constants.Header.AddInfo.InBondMode, Value = headerType }, new AddInfo()
			{ Key = Constants.Header.AddInfo.SchDLoading, Value = importLoadPortKCode } });
			result.SetDateCollection(() => new List<Date>()
			{ new Date()
			{ Type = DateType.Arrival, IsEstimate = ZBool.False, Value = eta }, new Date()
			{ Type = DateType.Departure, IsEstimate = ZBool.True, Value = sailingDate } });
			result.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			result.TransportLegCollection.Add(new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LegOrder = 1,
				ActualDeparture = firstExportDate,
				PortOfLoading = new UNLOCO()
				{ Code = firstExportCountry }
			});
			result.TransportLegCollection.Add(new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{ LegOrder = 2 });
			SetupAddressData(result, importerAddress, DocAddressType.ImporterDocumentaryAddress);
			return result;
		}

		void AssertContents(CusInBondHeader headerBO, ZGuid branchPK, ZString transportMode, ZString vessel, ZString vesselCountryOfRegistration, ZString lloyds, ZString voyage, ZDateTime eta,
			ZDateTime sailingDate, ZBool ftzMove, ZString carrierSCAC, ZString portUnladingDCode, ZString fIRMS, ZString headerType, ZString importLoadPortKCode, ZGuid importerPK, ZString
			firstExportCountry, ZDateTime firstExportDate)
		{
			AssertEquals("headerBO.BH_GB", branchPK, headerBO.BH_GB);
			AssertEquals("headerBO.BH_ImportTransportMode", transportMode, headerBO.BH_ImportTransportMode);
			AssertEquals("headerBO.BH_ImportConveyanceName", vessel, headerBO.BH_ImportConveyanceName);
			AssertEquals("headerBO.BH_ImportConveyanceCountry", vesselCountryOfRegistration, headerBO.BH_ImportConveyanceCountry);
			AssertEquals("headerBO.BH_LloydsNumber", lloyds, headerBO.BH_LloydsNumber);
			AssertEquals("headerBO.BH_VoyageNumber", voyage, headerBO.BH_VoyageNumber);
			AssertEquals("headerBO.BH_ETA", eta, headerBO.BH_ETA);
			AssertEquals("headerBO.BH_SailingDate", sailingDate, headerBO.BH_SailingDate);
			AssertEquals("headerBO.BH_FTZMove", ftzMove, headerBO.BH_FTZMove);
			AssertEquals("headerBO.BH_CarrierSCAC", carrierSCAC, headerBO.BH_CarrierSCAC);
			AssertEquals("headerBO.BH_PortUnladingDCode", portUnladingDCode, headerBO.BH_PortUnladingDCode);
			AssertEquals("headerBO.BH_FIRMS", fIRMS, headerBO.BH_FIRMS);
			AssertEquals("headerBO.BH_HeaderType", headerType, headerBO.BH_HeaderType);
			AssertEquals("headerBO.BH_ImportLoadPortKCode", importLoadPortKCode, headerBO.BH_ImportLoadPortKCode);
			AssertEquals("headerBO.BH_OA_Importer", importerPK, headerBO.BH_OA_Importer);
			AssertEquals("headerBO.BH_RN_NKFirstExportCountry", firstExportCountry, headerBO.BH_RN_NKFirstExportCountry);
			AssertEquals("headerBO.BH_FirstExportDate", firstExportDate, headerBO.BH_FirstExportDate);
		}
	}
}
