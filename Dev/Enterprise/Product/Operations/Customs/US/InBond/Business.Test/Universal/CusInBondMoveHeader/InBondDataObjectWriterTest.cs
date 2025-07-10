using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		public void TestBM_MoveToFTZ()
		{
			var helper = new WhsDataTestHelper(Factory.BOFactory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@", WarehouseTypes.Codes.FreeTradeZone);
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_MoveToFTZ = YesNoDefaultList.Codes.Yes;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "EN31";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			Factory.SaveForTesting();
			var writer = new WarehouseInBondDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(moveHeader);
			AssertNotNull("headerData.InBondMoveHeaderCollection", headerData.InBondMoveHeaderCollection);
			AssertEquals("headerData.InBondMoveHeaderCollection.Count", 1, headerData.InBondMoveHeaderCollection.Count);
			var moveHeaderData = headerData.InBondMoveHeaderCollection[0];
			AssertEquals("moveHeaderData.MoveToFTZ.Code", YesNoDefaultList.Codes.Yes, moveHeaderData.MoveToFTZ.Code);
			AssertEquals("moveHeaderData.MoveToFTZ.Description", YesNoDefaultList.Descriptions.Yes, moveHeaderData.MoveToFTZ.Description);
		}

		void AssertInBondMoveHeaderContents(UniversalCustoms.InBondMoveHeader moveHeaderData, ZString? inBondNumber)
		{
			AssertInBondMoveHeaderContents(moveHeaderData, "INB3", CodeDescriptionPairForTesting.New(InbondCommonTypeList.Codes._2TransportandExport, InbondCommonTypeList.Descriptions._2TransportandExport),
				"INB32523", inBondNumber, YesNoDefaultList.Codes.Yes, 15203m, new ZDateTime(2014, 2, 5), CodeDescriptionPairForTesting.New(SeaLocalPort2ScheduleD.ZZD_Code, SeaLocalPort2ScheduleD.ZZD_Description),
				CodeDescriptionPairForTesting.New(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), CodeDescriptionPairForTesting.New(SeaForeignPort2ScheduleK.ZZD_Code, SeaForeignPort2ScheduleK.ZZD_Description),
				CodeDescriptionPairForTesting.New(ImportMessageStatusList.Codes.AwaitingArrival, ImportMessageStatusList.Descriptions.AwaitingArrival), new ZDateTime(2014, 2, 10), "BOB'S VESSEL",
				CodeDescriptionPairForTesting.New(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea), CodeDescriptionPairForTesting.New(ContainerModeList.Codes.Containerized, ContainerModeList.Descriptions.Containerized),
				"TOL3", "TOL29833", "TOL CITY", CodeDescriptionPairForTesting.New(USStatesList.Codes.Alabama, USStatesList.Descriptions.Alabama), new ZDateTime(2014, 2, 6), new ZDateTime(2014, 2, 1),
				CodeDescriptionPairForTesting.New(CusAgent.GS_Code, CusAgent.GS_FullName), CodeDescriptionPairForTesting.New(SeaForeignPort1ScheduleK.ZZD_Code, SeaForeignPort1ScheduleK.ZZD_Description), "EXTRA TEXT", "SEAL1, SEAL2",
				CodeDescriptionPairForTesting.New(SeaLocalPort1ScheduleD.ZZD_Code, SeaLocalPort1ScheduleD.ZZD_Description), "GO23", "PD3234");
			var organizationAddressCollection = moveHeaderData.OrganizationAddressCollection;
			AssertNotNull("moveHeaderData.OrganizationAddressCollection", organizationAddressCollection);
			AssertEquals("billData.OrganizationAddressCollection.Count", 2, organizationAddressCollection.Count);
			AssertOrganizationBO_CRAHOLSYD("InBondCarrier", organizationAddressCollection[0], Enterprise.Customs.US.DataTransfer.Universal.Constants.CusInBond.AddressTypes.InBondCarrier);
			AssertOrganizationBO_WUFSHIJNB("TransferOfLiabilityCarrier", organizationAddressCollection[1], Enterprise.Customs.US.DataTransfer.Universal.Constants.CusInBond.AddressTypes.TransferOfLiabilityCarrier);
		}

		void AssertInBondMoveHeaderContents(UniversalCustoms.InBondMoveHeader moveHeaderData, ZString? inBondCarrierSCAC, ICodeDescription inBondEntryType, ZString? inBondCarrierID, ZString? inBondNumber,
			ZString? btaIndicator, ZDecimal? monetaryValue, ZDateTime? arrivalDate, ICodeDescription destinationPortDCode, ICodeDescription foreignDestPort, ICodeDescription foreignDestPortKCode,
			ICodeDescription customsStatus, ZDateTime? exportDate, ZString? exportLadenOn, ICodeDescription exportTransportMode, ICodeDescription exportContainerMode, ZString? tolCarrierCode, ZString? tolCarrierID,
			ZString? tolCityName, ICodeDescription tolStateCode, ZDateTime? tolDate, ZDateTime? entryDate, ICodeDescription cusAgent, ICodeDescription lastForeignPortScheduleK, ZString? additionalText,
			ZString? seals, ICodeDescription portOfPresentationCode, ZString? goNumber, ZString? pedimentoNumber)
		{
			AssertNotNull("Precondition: moveHeaderData", moveHeaderData);
			CombineAssertions(delegate
			{
				AssertEquals("moveHeaderData.InBondCarrierSCAC", inBondCarrierSCAC, moveHeaderData.InBondCarrierSCAC);
				if (inBondEntryType == null)
				{
					AssertNull("moveHeaderData.EntryType", moveHeaderData.EntryType);
				}
				else
				{
					AssertNotNull("moveHeaderData.EntryType", moveHeaderData.EntryType);
					AssertEquals("moveHeaderData.EntryType.Code", inBondEntryType.Code, moveHeaderData.EntryType.Code);
					AssertEquals("moveHeaderData.EntryType.Description", inBondEntryType.Description, moveHeaderData.EntryType.Description);
				}

				AssertEquals("moveHeaderData.InBondCarrierID", inBondCarrierID, moveHeaderData.InBondCarrierID);
				AssertEquals("moveHeaderData.BioterrorismActIndicator", btaIndicator, moveHeaderData.BioterrorismActIndicator);
				AssertEquals("moveHeaderData.MonetaryValue", monetaryValue, moveHeaderData.MonetaryValue);
				if (destinationPortDCode == null)
				{
					AssertNull("moveHeaderData.DestinationPortScheduleD", moveHeaderData.DestinationPortScheduleD);
				}
				else
				{
					AssertNotNull("moveHeaderData.DestinationPortScheduleD", moveHeaderData.DestinationPortScheduleD);
					AssertEquals("moveHeaderData.DestinationPortScheduleD.Code", destinationPortDCode.Code, moveHeaderData.DestinationPortScheduleD.Code);
					AssertEquals("moveHeaderData.DestinationPortScheduleD.Description", destinationPortDCode.Description, moveHeaderData.DestinationPortScheduleD.Description);
				}

				if (foreignDestPort == null)
				{
					AssertNull("moveHeaderData.ForeignDestinationPortUNLOCO", moveHeaderData.ForeignDestinationPortUNLOCO);
				}
				else
				{
					AssertNotNull("moveHeaderData.ForeignDestinationPortUNLOCO", moveHeaderData.ForeignDestinationPortUNLOCO);
					AssertEquals("moveHeaderData.ForeignDestinationPortUNLOCO.Code", foreignDestPort.Code, moveHeaderData.ForeignDestinationPortUNLOCO.Code);
					AssertEquals("moveHeaderData.ForeignDestinationPortUNLOCO.Name", foreignDestPort.Description, moveHeaderData.ForeignDestinationPortUNLOCO.Name);
				}

				if (foreignDestPortKCode == null)
				{
					AssertNull("moveHeaderData.ForeignDestinationPortScheduleK", moveHeaderData.ForeignDestinationPortScheduleK);
				}
				else
				{
					AssertNotNull("moveHeaderData.ForeignDestinationPortScheduleK", moveHeaderData.ForeignDestinationPortScheduleK);
					AssertEquals("moveHeaderData.ForeignDestinationPortScheduleK.Code", foreignDestPortKCode.Code, moveHeaderData.ForeignDestinationPortScheduleK.Code);
					AssertEquals("moveHeaderData.ForeignDestinationPortScheduleK.Description", foreignDestPortKCode.Description, moveHeaderData.ForeignDestinationPortScheduleK.Description);
				}

				if (customsStatus == null)
				{
					AssertNull("moveHeaderData.CustomsStatus", moveHeaderData.CustomsStatus);
				}
				else
				{
					AssertNotNull("moveHeaderData.CustomsStatus", moveHeaderData.CustomsStatus);
					AssertEquals("moveHeaderData.CustomsStatus.Code", customsStatus.Code, moveHeaderData.CustomsStatus.Code);
					AssertEquals("moveHeaderData.CustomsStatus.Description", customsStatus.Description, moveHeaderData.CustomsStatus.Description);
				}

				AssertEquals("moveHeaderData.ExportVesselName", exportLadenOn, moveHeaderData.ExportVesselName);
				if (exportTransportMode == null)
				{
					AssertNull("moveHeaderData.ExportTransportMode", moveHeaderData.ExportTransportMode);
				}
				else
				{
					AssertNotNull("moveHeaderData.ExportTransportMode", moveHeaderData.ExportTransportMode);
					AssertEquals("moveHeaderData.ExportTransportMode.Code", exportTransportMode.Code, moveHeaderData.ExportTransportMode.Code);
					AssertEquals("moveHeaderData.ExportTransportMode.Description", exportTransportMode.Description, moveHeaderData.ExportTransportMode.Description);
				}

				if (exportContainerMode == null)
				{
					AssertNull("moveHeaderData.ExportContainerMode", moveHeaderData.ExportContainerMode);
				}
				else
				{
					AssertNotNull("moveHeaderData.ExportContainerMode", moveHeaderData.ExportContainerMode);
					AssertEquals("moveHeaderData.ExportContainerMode.Code", exportContainerMode.Code, moveHeaderData.ExportContainerMode.Code);
					AssertEquals("moveHeaderData.ExportContainerMode.Description", exportContainerMode.Description, moveHeaderData.ExportContainerMode.Description);
				}

				AssertEquals("moveHeaderData.TransferOfLiabilityCarrierCode", tolCarrierCode, moveHeaderData.TransferOfLiabilityCarrierCode);
				AssertEquals("moveHeaderData.TransferOfLiabilityCarrierID", tolCarrierID, moveHeaderData.TransferOfLiabilityCarrierID);
				AssertEquals("moveHeaderData.TransferOfLiabilityCityName", tolCityName, moveHeaderData.TransferOfLiabilityCityName);
				if (tolStateCode == null)
				{
					AssertNull("moveHeaderData.TransferOfLiabilityStateCode", moveHeaderData.TransferOfLiabilityStateCode);
				}
				else
				{
					AssertNotNull("moveHeaderData.TransferOfLiabilityStateCode", moveHeaderData.TransferOfLiabilityStateCode);
					AssertEquals("moveHeaderData.TransferOfLiabilityStateCode.Code", tolStateCode.Code, moveHeaderData.TransferOfLiabilityStateCode.Code);
					AssertEquals("moveHeaderData.TransferOfLiabilityStateCode.Description", tolStateCode.Description, moveHeaderData.TransferOfLiabilityStateCode.Description);
				}

				if (cusAgent == null)
				{
					AssertNull("moveHeaderData.CustomsAgent", moveHeaderData.CustomsAgent);
				}
				else
				{
					AssertNotNull("moveHeaderData.CustomsAgent", moveHeaderData.CustomsAgent);
					AssertEquals("moveHeaderData.CustomsAgent.Code", cusAgent.Code, moveHeaderData.CustomsAgent.Code);
					AssertEquals("moveHeaderData.CustomsAgent.Name", cusAgent.Description, moveHeaderData.CustomsAgent.Name);
				}

				if (lastForeignPortScheduleK == null)
				{
					AssertNull("moveHeaderData.LastForeignPortScheduleK", moveHeaderData.LastForeignPortScheduleK);
				}
				else
				{
					AssertNotNull("moveHeaderData.LastForeignPortScheduleK", moveHeaderData.LastForeignPortScheduleK);
					AssertEquals("moveHeaderData.LastForeignPortScheduleK.Code", lastForeignPortScheduleK.Code, moveHeaderData.LastForeignPortScheduleK.Code);
					AssertEquals("moveHeaderData.LastForeignPortScheduleK.Description", lastForeignPortScheduleK.Description, moveHeaderData.LastForeignPortScheduleK.Description);
				}

				AssertEquals("moveHeaderData.AdditionalText", additionalText, moveHeaderData.AdditionalText);
				AssertEquals("moveHeaderData.Seals", seals, moveHeaderData.Seals);
				if (portOfPresentationCode == null)
				{
					AssertNull("moveHeaderData.PortOfPresentationScheduleD", moveHeaderData.PortOfPresentationScheduleD);
				}
				else
				{
					AssertNotNull("moveHeaderData.PortOfPresentationScheduleD", moveHeaderData.PortOfPresentationScheduleD);
					AssertEquals("moveHeaderData.PortOfPresentationScheduleD.Code", portOfPresentationCode.Code, moveHeaderData.PortOfPresentationScheduleD.Code);
					AssertEquals("moveHeaderData.PortOfPresentationScheduleD.Description", portOfPresentationCode.Description, moveHeaderData.PortOfPresentationScheduleD.Description);
				}

				AssertNotNull("moveHeaderData.EntryNumberCollection", moveHeaderData.EntryNumberCollection);
				AssertContainEntryNumber(moveHeaderData.EntryNumberCollection, CodeDescriptionPairForTesting.New(Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond, Constants.MovementHeader.NumberTypes.InBondNumberDescription), inBondNumber);
				AssertNotNull("moveHeaderData.AdditionalReferenceCollection", moveHeaderData.AdditionalReferenceCollection);
				AssertContainAdditionalReference(moveHeaderData.AdditionalReferenceCollection, CodeDescriptionPairForTesting.New(Constants.MovementHeader.AdditionalReferences.GONumber, Constants.MovementHeader.AdditionalReferences.GONumberDescription), goNumber);
				AssertContainAdditionalReference(moveHeaderData.AdditionalReferenceCollection, CodeDescriptionPairForTesting.New(Constants.MovementHeader.AdditionalReferences.PedimentoNumber, Constants.MovementHeader.AdditionalReferences.PedimentoNumberDescription), pedimentoNumber);
				AssertNotNull("moveHeaderData.DateCollection", moveHeaderData.DateCollection);
				AssertContainDate(moveHeaderData.DateCollection, DateType.EntryDate, ZBool.True, entryDate);
				AssertContainDate(moveHeaderData.DateCollection, DateType.Arrival, ZBool.True, arrivalDate);
				AssertContainDate(moveHeaderData.DateCollection, DateType.Departure, ZBool.True, exportDate);
				AssertContainDate(moveHeaderData.DateCollection, DateType.TransferOfLiability, ZBool.True, tolDate);
			});
		}

		void AssertContainEntryNumber(List<EntryNumber> entryNumberCollection, ICodeDescription entryType, ZString? number)
		{
			var entryNumberData = entryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == entryType.Code);
			AssertNotNull(string.Format("Precondition: entryNumberCollection should contain EntryNumber.Type({0})", entryType), entryNumberData);
			if (entryType == null)
			{
				AssertNull("entryNumberData.Type", entryNumberData.Type);
			}
			else
			{
				AssertNotNull("entryNumberData.Type", entryNumberData.Type);
				AssertEquals("entryNumberData.Type.Code", entryType.Code, entryNumberData.Type.Code);
				AssertEquals("entryNumberData.Type.Description", entryType.Description, entryNumberData.Type.Description);
			}

			AssertEquals("entryNumberData.Number", number, entryNumberData.Number);
		}

		void AssertContainAdditionalReference(DataObjectList<AdditionalReference> additionalReferenceCollection, ICodeDescription entryType, ZString? number)
		{
			var additionalReferenceData = additionalReferenceCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == entryType.Code);
			AssertNotNull(string.Format("Precondition: additionalReferenceCollection should contain AdditionalReference.Type({0})", entryType), additionalReferenceData);
			if (entryType == null)
			{
				AssertNull("additionalReferenceData.Type", additionalReferenceData.Type);
			}
			else
			{
				AssertNotNull("additionalReferenceData.Type", additionalReferenceData.Type);
				AssertEquals("additionalReferenceData.Type.Code", entryType.Code, additionalReferenceData.Type.Code);
				AssertEquals("additionalReferenceData.Type.Description", entryType.Description, additionalReferenceData.Type.Description);
			}

			AssertEquals("additionalReferenceData.ReferenceNumber", number, additionalReferenceData.ReferenceNumber);
		}

		CusInBondMoveHeader SetupCusInBondMoveHeader(CusInBondMoveHeader moveHeader, ZString inBondNumber)
		{
			moveHeader.BM_OA_InBondCarrier = GetOrganizationBO_CRAHOLSYD(moveHeader.Factory).MainAddress.PK;
			moveHeader.BM_OA_TOLCarrier = GetOrganizationBO_WUFSHIJNB(moveHeader.Factory).MainAddress.PK;
			SetupCusInBondMoveHeader(moveHeader, "INB3", InbondCommonTypeList.Codes._2TransportandExport, "INB32523", inBondNumber, YesNoDefaultList.Codes.Yes, 15203m, new ZDateTime(2014, 2, 5),
				SeaLocalPort2ScheduleD.ZZD_Code, SeaForeignPort2.RL_Code, SeaForeignPort2ScheduleK.ZZD_Code, ImportMessageStatusList.Codes.AwaitingArrival, new ZDateTime(2014, 2, 10), "BOB'S VESSEL",
				TransportModeCodes.Codes.VesselContainer, "TOL3", "TOL29833", "TOL CITY", USStatesList.Codes.Alabama, new ZDateTime(2014, 2, 6), new ZDateTime(2014, 2, 1), CusAgent.GS_Code,
				SeaForeignPort1ScheduleK.ZZD_Code, "EXTRA TEXT", "SEAL1, SEAL2", SeaLocalPort1ScheduleD.ZZD_Code, "GO23", "PD3234");
			return moveHeader;
		}

		CusInBondMoveHeader SetupCusInBondMoveHeader(CusInBondMoveHeader moveHeader, ZString inBondCarrierSCAC, ZString inBondEntryType, ZString inBondCarrierID, ZString inBondNumber, ZString btaIndicator,
			ZDecimal monetaryValue, ZDateTime arrivalDate, ZString destinationPortDCode, ZString foreignDestPort, ZString foreignDestPortKCode, ZString customsStatus, ZDateTime exportDate, ZString exportLadenOn,
			ZString exportTransportMode, ZString tolCarrierCode, ZString tolCarrierID, ZString tolCityName, ZString tolStateCode, ZDateTime tolDate, ZDateTime entryDate, ZString cusAgent, ZString lastForeignPortScheduleK,
			ZString additionalText, ZString seals, ZString portOfPresentationCode, ZString goNumber, ZString pedimentoNumber)
		{
			moveHeader.BM_InBondCarrierSCAC = inBondCarrierSCAC;
			moveHeader.BM_InBondEntryType = inBondEntryType;
			moveHeader.BM_InBondCarrierID = inBondCarrierID;
			moveHeader.InBondNumber = inBondNumber;
			moveHeader.BM_BTAIndicator = btaIndicator;
			moveHeader.BM_MonetaryValue = monetaryValue;
			moveHeader.BM_ArrivalDate = arrivalDate;
			moveHeader.BM_DestinationPortCode = destinationPortDCode;
			moveHeader.BM_ForeignDestPortKCode = foreignDestPortKCode;
			moveHeader.BM_RL_NKForeignDestPort = foreignDestPort;
			moveHeader.BM_CustomsStatus = customsStatus;
			moveHeader.BM_ExportDate = exportDate;
			moveHeader.BM_ExportLadenOn = exportLadenOn;
			moveHeader.BM_ExportTransportMode = exportTransportMode;
			moveHeader.BM_TOLCarrierCode = tolCarrierCode;
			moveHeader.BM_TOLCarrierID = tolCarrierID;
			moveHeader.BM_TOLCityName = tolCityName;
			moveHeader.BM_TOLStateCode = tolStateCode;
			moveHeader.BM_TOLDate = tolDate;
			moveHeader.BM_EntryDate = entryDate;
			moveHeader.BM_GS_NKCusAgent = cusAgent;
			moveHeader.BM_Via = lastForeignPortScheduleK;
			moveHeader.BM_AdditionalText = additionalText;
			moveHeader.BM_Seals = seals;
			moveHeader.BM_PortOfPresentationCode = portOfPresentationCode;
			moveHeader.BM_GONumber = goNumber;
			moveHeader.BM_PedimentoNumber = pedimentoNumber;
			return moveHeader;
		}
	}
}
