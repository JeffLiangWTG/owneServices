using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest
	{
		public void TestImportingInBondMovementData()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB2";
			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { SetupContainer2("CONT1", 1), SetupContainer("CONT2", 2) }));
			var helper = new InBondDataObjectReaderHelper(Factory);
			helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(shipmentDataObject);
			helper.CollectBillLink(bill1, SetupInBondBill("MB1", WayBillTypeList.Codes.Master, ZString.Empty, 1));
			helper.CollectBillLink(bill2, SetupInBondBill("MB2", WayBillTypeList.Codes.Master, ZString.Empty, 2));
			var inBondMoveHeaderDataObject = SetupInBondMoveHeader("IT302322");
			var inBondMoveDetail1DataObject = SetupInBondMoveDetail(2, "0001");
			inBondMoveDetail1DataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)2, ContainerNumber = "CONT2" } });
			var inBondMoveDetail2DataObject = SetupInBondMoveDetail(1, "0002");
			inBondMoveDetail2DataObject.ContainerLinkCollection = new List<ContainerLink>(new[] { new ContainerLink()
			{ Link = (ZInt)1, ContainerNumber = "CONT1" }, new ContainerLink()
			{ Link = (ZInt)2, ContainerNumber = "CONT2" } });
			inBondMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>(new[] { inBondMoveDetail1DataObject, inBondMoveDetail2DataObject });

			var moveHeader = header.MovementHeader;
			var inbEntryNumber = Factory.New<CusEntryNumber>();
			inbEntryNumber.CE_ParentID = moveHeader.PK;
			inbEntryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inbEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			inbEntryNumber.CE_EntryNum = "IT302322";
			inbEntryNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.SaveForTesting();

			var reader = new CusInBondMoveHeaderDataObjectReader(inBondMoveHeaderDataObject, logger, helper, header);
			var moveHeaderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(moveHeaderBO);
			AssertEquals(1, header.MovementHeaders.Count);
			AssertEquals("IT302322", moveHeaderBO.InBondNumber);

			inbEntryNumber.CE_EntryNum = "IT302344";
			inbEntryNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.SaveForTesting();
			moveHeaderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(moveHeaderBO);
			AssertEquals(2, header.MovementHeaders.Count);
			AssertNotNull(header.MovementHeaders.OfType<CusInBondMoveHeader>().First(x => x.InBondNumber == "IT302322"));
			AssertNotNull(header.MovementHeaders.OfType<CusInBondMoveHeader>().First(x => x.InBondNumber == "IT302344"));

			CombineAssertions(delegate
			{
				AssertCusInBondMoveHeaderContents(moveHeaderBO);
				AssertEquals("moveHeaderBO.BM_BH", header.PK, moveHeaderBO.BM_BH);
				AssertEquals("moveHeaderBO.MovementDetails.Count", 2, moveHeaderBO.MovementDetails.Count);
				var moveDetailBO1 = moveHeaderBO.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill1.PK);
				AssertCusInBondMoveDetailContents(moveDetailBO1, "0002");
				AssertEquals("moveDetailBO1.Containers.Count", 2, moveDetailBO1.Containers.Count);
				AssertNotNull(moveDetailBO1.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1"));
				AssertNotNull(moveDetailBO1.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2"));
				var moveDetailBO2 = moveHeaderBO.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill2.PK);
				AssertCusInBondMoveDetailContents(moveDetailBO2, "0001");
				AssertEquals("moveDetailBO2.Containers.Count", 1, moveDetailBO2.Containers.Count);
				AssertNotNull(moveDetailBO2.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT2"));
				AssertMultilineASCIIEquals("logger.Logs", @" 
Information - Successfully loaded matching CusInBondMoveHeader.
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
Information - No matching CusInBondMoveDetail found, creating new CusInBondMoveDetail.
Information - Populating CusInBondMoveDetail...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
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
Information - No matching CusInBondMoveDetail found, creating new CusInBondMoveDetail.
Information - Populating CusInBondMoveDetail...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...".Trim(), logger.Logs);
			});
		}

		protected override void AssertCusInBondMoveHeaderContents(US.Business.CusInBondMoveHeader moveHeaderBO)
		{
			AssertCusInBondMoveHeaderContents((CusInBondMoveHeader)moveHeaderBO, InbondCommonTypeList.Codes._2TransportandExport, YesNoDefaultList.Codes.Yes,
				GetOrganizationBO_CRAHOLSYD(moveHeaderBO.Factory).MainAddress.PK, "13-150279800", "INB3", SeaLocalPort2ScheduleD.ZZD_Code, SeaForeignPort2ScheduleK.ZZD_Code, SeaForeignPort2.RL_Code,
				15203m, new ZDateTime(2014, 2, 5), new ZDateTime(2014, 2, 10), "BOB'S VESSEL", TransportModeCodes.Codes.VesselNonContainer, GetOrganizationBO_WUFSHIJNB(moveHeaderBO.Factory).MainAddress.PK,
				"TOL29833", "TOL3", new ZDateTime(2014, 2, 6), "TOL CITY", USStatesList.Codes.Alabama, new ZDateTime(2014, 2, 1), SeaLocalPort1ScheduleD.ZZD_Code, SeaForeignPort1ScheduleK.ZZD_Code, "EXTRA TEXT",
				"SEAL1, SEAL2", CusAgent.GS_Code, "PD3234", "GO23");
		}

		protected override InBondMoveHeader SetupInBondMoveHeader(ZString inBondNumber, bool isDefaultingEnabled = false)
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-150279800", Core.Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "INB3", Core.Constants.CountryCodes.UnitedStates);
			return SetupInBondMoveHeader(inBondNumber, InbondCommonTypeList.Codes._2TransportandExport, YesNoDefaultList.Codes.Yes, org.MainAddress, isDefaultingEnabled ? "" : "13-150279800",
				isDefaultingEnabled ? "" : "INB3", SeaLocalPort2ScheduleD.ZZD_Code, SeaForeignPort2ScheduleK.ZZD_Code, SeaForeignPort2.RL_Code, 15203m, new ZDateTime(2014, 2, 5), new ZDateTime(2014, 2, 10),
				"BOB'S VESSEL", TransportTypeList.Codes.Sea, ContainerModeList.Codes.NonContainerized, GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress, "TOL29833", "TOL3", new ZDateTime(2014, 2, 6),
				"TOL CITY", USStatesList.Codes.Alabama, new ZDateTime(2014, 2, 1), SeaLocalPort1ScheduleD.ZZD_Code, SeaForeignPort1ScheduleK.ZZD_Code, "EXTRA TEXT", "SEAL1, SEAL2", CusAgent.GS_Code, "PD3234", "GO23");
		}

		InBondMoveHeader SetupInBondMoveHeader(ZString inBondNumber, ZString entryType, ZString bTAIndicator, OrgAddress inBondCarrierAddress, ZString inBondCarrierID, ZString inBondCarrierSCAC,
			ZString destinationPortDCode, ZString foreignDestPortKCode, ZString foreignDestPort, ZDecimal monetaryValue, ZDateTime arrivalDate, ZDateTime exportDate, ZString exportLadenOn, ZString exportTransportMode,
			ZString exportContainerMode, OrgAddress tOLCarrierAddress, ZString tOLCarrierID, ZString tOLCarrierCode, ZDateTime tOLDate, ZString tOLCityName, ZString tOLStateCode, ZDateTime entryDate,
			ZString portOfPresentationCode, ZString via, ZString additionalText, ZString seals, ZString cusAgent, ZString pedimentoNumber, ZString gONumber)
		{
			var result = SetupInBondMoveHeader(inBondNumber, entryType, bTAIndicator, inBondCarrierID, inBondCarrierSCAC, destinationPortDCode, foreignDestPortKCode, exportLadenOn, cusAgent, tOLCarrierID, tOLCarrierCode, tOLDate, tOLCityName, tOLStateCode, entryDate, arrivalDate, exportDate, inBondCarrierAddress, tOLCarrierAddress);
			result.MonetaryValue = monetaryValue;
			result.ForeignDestinationPortUNLOCO = new UNLOCO()
			{ Code = foreignDestPort };
			result.ExportTransportMode = new CodeDescriptionPair()
			{ Code = exportTransportMode };
			result.ExportContainerMode = new ContainerMode()
			{ Code = exportContainerMode };
			result.PortOfPresentationScheduleD = new CodeDescriptionPair4Char()
			{ Code = portOfPresentationCode };
			result.LastForeignPortScheduleK = new CodeDescriptionPair5Char()
			{ Code = via };
			result.AdditionalText = additionalText;
			result.Seals = seals;
			result.AdditionalReferenceCollection = new DataObjectList<AdditionalReference>(new[] { new AdditionalReference()
			{ Type = new EntryType()
			{ Code = Constants.MovementHeader.AdditionalReferences.GONumber }, ReferenceNumber = gONumber }, new AdditionalReference()
			{ Type = new EntryType()
			{ Code = Constants.MovementHeader.AdditionalReferences.PedimentoNumber }, ReferenceNumber = pedimentoNumber } });
			return result;
		}

		void AssertCusInBondMoveHeaderContents(CusInBondMoveHeader moveHeaderBO, ZString entryType, ZString bTAIndicator, ZGuid inBondCarrierPK, ZString inBondCarrierID, ZString inBondCarrierSCAC,
			ZString destinationPortDCode, ZString foreignDestPortKCode, ZString foreignDestPort, ZDecimal monetaryValue, ZDateTime arrivalDate, ZDateTime exportDate, ZString exportLadenOn, ZString exportTransportMode,
			ZGuid tOLCarrierPK, ZString tOLCarrierID, ZString tOLCarrierCode, ZDateTime tOLDate, ZString tOLCityName, ZString tOLStateCode, ZDateTime entryDate, ZString portOfPresentationCode, ZString via,
			ZString additionalText, ZString seals, ZString cusAgent, ZString pedimentoNumber, ZString gONumber)
		{
			AssertEquals("moveHeaderBO.BM_InBondEntryType", entryType, moveHeaderBO.BM_InBondEntryType);
			AssertEquals("moveHeaderBO.BM_BTAIndicator", bTAIndicator, moveHeaderBO.BM_BTAIndicator);
			AssertEquals("moveHeaderBO.BM_OA_InBondCarrier", inBondCarrierPK, moveHeaderBO.BM_OA_InBondCarrier);
			AssertEquals("moveHeaderBO.BM_InBondCarrierID", inBondCarrierID, moveHeaderBO.BM_InBondCarrierID);
			AssertEquals("moveHeaderBO.BM_InBondCarrierSCAC", inBondCarrierSCAC, moveHeaderBO.BM_InBondCarrierSCAC);
			AssertEquals("moveHeaderBO.BM_DestinationPortCode", destinationPortDCode, moveHeaderBO.BM_DestinationPortCode);
			AssertEquals("moveHeaderBO.BM_ForeignDestPortKCode", foreignDestPortKCode, moveHeaderBO.BM_ForeignDestPortKCode);
			AssertEquals("moveHeaderBO.BM_RL_NKForeignDestPort", foreignDestPort, moveHeaderBO.BM_RL_NKForeignDestPort);
			AssertEquals("moveHeaderBO.BM_MonetaryValue", monetaryValue, moveHeaderBO.BM_MonetaryValue);
			AssertEquals("moveHeaderBO.BM_ArrivalDate", arrivalDate, moveHeaderBO.BM_ArrivalDate);
			AssertEquals("moveHeaderBO.BM_ExportDate", exportDate, moveHeaderBO.BM_ExportDate);
			AssertEquals("moveHeaderBO.BM_ExportLadenOn", exportLadenOn, moveHeaderBO.BM_ExportLadenOn);
			AssertEquals("moveHeaderBO.BM_ExportTransportMode", exportTransportMode, moveHeaderBO.BM_ExportTransportMode);
			AssertEquals("moveHeaderBO.B0_MasterBillNumber", tOLCarrierPK, moveHeaderBO.BM_OA_TOLCarrier);
			AssertEquals("moveHeaderBO.BM_TOLCarrierID", tOLCarrierID, moveHeaderBO.BM_TOLCarrierID);
			AssertEquals("moveHeaderBO.BM_TOLCarrierCode", tOLCarrierCode, moveHeaderBO.BM_TOLCarrierCode);
			AssertEquals("moveHeaderBO.BM_TOLDate", tOLDate, moveHeaderBO.BM_TOLDate);
			AssertEquals("moveHeaderBO.BM_TOLCityName", tOLCityName, moveHeaderBO.BM_TOLCityName);
			AssertEquals("moveHeaderBO.BM_TOLStateCode", tOLStateCode, moveHeaderBO.BM_TOLStateCode);
			AssertEquals("moveHeaderBO.BM_EntryDate", entryDate, moveHeaderBO.BM_EntryDate);
			AssertEquals("moveHeaderBO.BM_PortOfPresentationCode", portOfPresentationCode, moveHeaderBO.BM_PortOfPresentationCode);
			AssertEquals("moveHeaderBO.BM_Via", via, moveHeaderBO.BM_Via);
			AssertEquals("moveHeaderBO.BM_AdditionalText", additionalText, moveHeaderBO.BM_AdditionalText);
			AssertEquals("moveHeaderBO.BM_Seals", seals, moveHeaderBO.BM_Seals);
			AssertEquals("moveHeaderBO.BM_GS_NKCusAgent", cusAgent, moveHeaderBO.BM_GS_NKCusAgent);
			AssertEquals("moveHeaderBO.BM_PedimentoNumber", pedimentoNumber, moveHeaderBO.BM_PedimentoNumber);
			AssertEquals("moveHeaderBO.BM_GONumber", gONumber, moveHeaderBO.BM_GONumber);
		}
	}
}
