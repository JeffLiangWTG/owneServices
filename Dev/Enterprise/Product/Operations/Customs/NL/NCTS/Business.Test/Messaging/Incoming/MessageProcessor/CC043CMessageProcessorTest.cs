using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC043CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC043CMessageProcessor, ICC043CDataProvider>
{
	public void TestCustomsStatus_UAP_ContinueUnloading()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "TestMRN";
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		};
		var dataProviderMock = GetDataProviderMock("TestMRN", "0", containerIndicator: true, hasIncidents: true, incidentContainerIndicator: false, continueUnloading: 1);

		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		AssertEquals("CusInBondMoveHeader - BM_CustomsStatus should be set to 'ULR - UnloadRemarks'", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, moveHeader.BM_CustomsStatus);
	}

	public void TestCustomsStatus_NoStatus()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "TestMRN";
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = "";
		};

		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		AssertEquals("CusInBondMoveHeader - BM_CustomsStatus should be set to 'UAP - UnloadPermissionGranted'", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, moveHeader.BM_CustomsStatus);
	}

	public void TestMovementHeader()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("BM_InBondEntryType", "T1", moveHeader.BM_InBondEntryType);
			AssertEquals("BM_EntryDate", new ZDateTime(2024, 8, 29), moveHeader.BM_EntryDate);
			AssertEquals("BM_TypeOfSecurity", GetExpectedTypeOfSecurity("0"), moveHeader.BM_TypeOfSecurity);
			AssertEquals("BM_ReducedDatasetIndicator", true, moveHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_RL_NKDestinationPort", "NL", moveHeader.BM_RL_NKDestinationPort);
			AssertEquals("BM_InlandTransportMode", "3", moveHeader.BM_InlandTransportMode);
			AssertEquals("BM_GrossWeight", 1.2m, moveHeader.BM_GrossWeight);
			AssertEquals("Seal Count", (ZShort)4, moveHeader.BM_SealQty);
		});
	}

	public void TestPreviousDocuments()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocument Count", 2, nctsHeader.PreviousDocuments.Count);
			AssertPreviousDocuments(nctsHeader.PreviousDocuments[0], 1);
			AssertPreviousDocuments(nctsHeader.PreviousDocuments[1], 2);
		});
	}

	public void TestTransportEquipment_Containerised()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("TransportEquipments Count", 2, nctsHeader.ArrivalHeaderContainers.Count);
			AssertArrivalHeaderContainerAndSeals(nctsHeader.ArrivalHeaderContainers[0], 1, Core.Constants.ContainerModes.Containerised);
			AssertArrivalHeaderContainerAndSeals(nctsHeader.ArrivalHeaderContainers[1], 2, Core.Constants.ContainerModes.Containerised);
		});
	}

	public void TestTransportEquipment_NonContainerised()
	{
		var dataProviderMock = GetDataProviderMock("TestMRN", "0", containerIndicator: false, hasIncidents: true, incidentContainerIndicator: true);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("TransportEquipments Count", 2, nctsHeader.ArrivalHeaderContainers.Count);
			AssertArrivalHeaderContainerAndSeals(nctsHeader.ArrivalHeaderContainers[0], 1, Core.Constants.ContainerModes.NonContainerised);
			AssertArrivalHeaderContainerAndSeals(nctsHeader.ArrivalHeaderContainers[1], 2, Core.Constants.ContainerModes.NonContainerised);
		});
	}

	public void TestSupportingDocuments()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("SupportingDocument Count", 2, moveHeader.SupportingDocuments.Count);
			AssertSupportingDocuments(moveHeader.SupportingDocuments[0], 1);
			AssertSupportingDocuments(moveHeader.SupportingDocuments[1], 2);
		});
	}

	public void TestArrivalTransportInfo_Movement()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("DepartureTransportMeans Count", 2, moveHeader.ArrivalTransportInfos.Count);
			AssertArrivalTransportInfos(moveHeader.ArrivalTransportInfos[0], 1);
			AssertArrivalTransportInfos(moveHeader.ArrivalTransportInfos[1], 2);
		});
	}

	public void TestArrivalTransportInfo_Bill()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;
		var bill = nctsHeader.Bills[0];

		CombineAssertions(() =>
		{
			AssertEquals("DepartureTransportMeans Count", 2, bill.ArrivalTransportInfos.Count);
			AssertArrivalTransportInfos(bill.ArrivalTransportInfos[0], 1);
			AssertArrivalTransportInfos(bill.ArrivalTransportInfos[1], 2);
		});
	}

	public void TestAdditionalReferences()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			var additionalReferences = moveHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
			AssertEquals("AdditionalReference Count", 2, additionalReferences.Count());
			AssertOtherDocuments(additionalReferences.First(), 1, "REF");
			AssertOtherDocuments(additionalReferences.Last(), 2, "REF");
		});
	}

	public void TestAdditionalInformation()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			var additionalInformation = moveHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "INF");
			AssertEquals("AdditionalInformation Count", 2, additionalInformation.Count());
			AssertAdditionalInformation(additionalInformation.First(), 1);
			AssertAdditionalInformation(additionalInformation.Last(), 2);
		});
	}

	public void TestIncidents_Containerised()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("BH_Exportflag", EventFlagList.Codes.Yes, nctsHeader.BH_ExportFlag);
			AssertEquals("Incident Count", 2, nctsHeader.EnRouteIncidents.Count);
			AssertIndicdent(nctsHeader.EnRouteIncidents[0], 1, Core.Constants.ContainerModes.Containerised);
			AssertIndicdent(nctsHeader.EnRouteIncidents[1], 2, Core.Constants.ContainerModes.Containerised);
		});
	}

	public void TestIncidents_NonContainerised()
	{
		var dataProviderMock = GetDataProviderMock("TestMRN", "0", containerIndicator: true, hasIncidents: true, incidentContainerIndicator: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("BH_Exportflag", EventFlagList.Codes.Yes, nctsHeader.BH_ExportFlag);
			AssertEquals("Incident Count", 2, nctsHeader.EnRouteIncidents.Count);
			AssertIndicdent(nctsHeader.EnRouteIncidents[0], 1, Core.Constants.ContainerModes.NonContainerised);
			AssertIndicdent(nctsHeader.EnRouteIncidents[1], 2, Core.Constants.ContainerModes.NonContainerised);
		});
	}

	public void TestNoIncidents()
	{
		var dataProviderMock = GetDataProviderMock("TestMRN", "0", containerIndicator: true, hasIncidents: false, incidentContainerIndicator: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("BH_Exportflag", EventFlagList.Codes.No, nctsHeader.BH_ExportFlag);
			AssertEquals("Incident Count", 0, nctsHeader.EnRouteIncidents.Count);
		});
	}

	public void TestSecurity_0()
	{
		var dataProviderMock = GetDataProviderMock("TestMRN", "0", containerIndicator: false, hasIncidents: false, incidentContainerIndicator: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		AssertEquals("BM_TypeOfSecurity", GetExpectedTypeOfSecurity("0"), moveHeader.BM_TypeOfSecurity);
	}

	public void TestSecurity_1()
	{
		var dataProviderMock = GetDataProviderMock("TestMRN", "1", containerIndicator: false, hasIncidents: false, incidentContainerIndicator: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		AssertEquals("BM_TypeOfSecurity", GetExpectedTypeOfSecurity("1"), moveHeader.BM_TypeOfSecurity);
	}

	public void TestSecurity_2()
	{
		var dataProviderMock = GetDataProviderMock("TestMRN", "2", containerIndicator: false, hasIncidents: false, incidentContainerIndicator: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		AssertEquals("BM_TypeOfSecurity", GetExpectedTypeOfSecurity("2"), moveHeader.BM_TypeOfSecurity);
	}

	public void TestDiscardedMessage()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - Customs Status should not be changed", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, moveHeader.BM_CustomsStatus);
			AssertEquals("CusInBondMoveHeader - Phase should not be changed", NCTS5ArrivalPhaseList.Codes.Arrival, moveHeader.BM_Phase);
			AssertEquals("NctsHeader - Message Status should not be changed", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);

			var expectedMessage = "The message with interchange  is discarded because its 'Arrival Status' has the status CL1.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	public void TestHouseConsignment()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;

		CombineAssertions(() =>
		{
			AssertHouseConsignment(nctsHeader, 1);
		});
	}

	public void TestEventLogCount()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var nctsHeader = incomingMessage.EM_LinkedObject as NctsHeader;
		var moveHeader = nctsHeader.ArrivalMovementHeader;

		var eventLog = moveHeader.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == "CES" && (l.SL_Reference == ExpectedCustomsStatus));
		AssertEquals("only one event", 1, eventLog.Count());
	}

	protected override ICC043CDataProvider GetMessageDataProviderMock() => GetDataProviderMock("TestMRN", "0", containerIndicator: true, hasIncidents: true, incidentContainerIndicator: true);

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override string InitialCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

	protected override string InitialPhase => NCTS5ArrivalPhaseList.Codes.Arrival;

	protected override Action<NctsHeader> SetupNctsHeader => nctsHeader =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
	};

	protected override string ExpectedCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

	protected override string ExpectedPhase => NCTS5ArrivalPhaseList.Codes.Arrival;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	ICC043CDataProvider GetDataProviderMock(string mrn, string security, bool containerIndicator, bool hasIncidents, bool incidentContainerIndicator, int continueUnloading = 0)
	{
		return Mock.Of<ICC043CDataProvider>(provider =>
			provider.MRN == mrn &&
			provider.DeclarationType == "T1" &&
			provider.DeclarationAcceptanceDate == new DateTime(2024, 8, 29) &&
			provider.Security == security &&
			provider.ReducedDatasetIndicator &&
			provider.CountryOfDestination == "NL" &&
			provider.InlandModeOfTransport == "3" &&
			provider.GrossMass == 1.2m &&
			provider.ContainerIndicator == containerIndicator &&
			provider.CTLControlContinueUnloading == continueUnloading &&
			provider.TransportEquipments == new Collection<INCTSTransportEquipmentProvider>()
			{
				GetTransportEquipmentProviderMock(1, "containerId1"),
				GetTransportEquipmentProviderMock(2, "containerId2"),
				GetTransportEquipmentProviderMock(3, string.Empty),
			} &&
			provider.DepartureTransportMeans == new Collection<INCTSDepartureTransportMeansProvider>()
			{
				GetDepartureTransportMeansProviderMock(1),
				GetDepartureTransportMeansProviderMock(2),
			} &&
			provider.PreviousDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, "complementOfInformation1"),
				GetReferenceDocumentProviderMock(2, "complementOfInformation2"),
			} &&
			provider.SupportingDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, "complementOfInformation1"),
				GetReferenceDocumentProviderMock(2, "complementOfInformation2"),
			} &&
			provider.TransportDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, null),
				GetReferenceDocumentProviderMock(2, null),
			} &&
			provider.AdditionalReferences == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, null),
				GetReferenceDocumentProviderMock(2, null),
			} &&
			provider.AdditionalInformation == new Collection<INCTSAdditionalInformationProvider>()
			{
				GetAdditionalInformationProviderMock(1),
				GetAdditionalInformationProviderMock(2),
			} &&
			provider.Incidents == (hasIncidents
				? new Collection<INCTSIncidentProvider>()
				{
					GetIncidentProviderMock(1, incidentContainerIndicator),
					GetIncidentProviderMock(2, incidentContainerIndicator),
				}
				: new Collection<INCTSIncidentProvider>()) &&
			provider.HouseConsignments == new Collection<INCTSHouseConsignmentProvider>()
			{
				GetHouseConsignmentProviderMock(1)
			}
		);
	}

	INCTSTransportEquipmentProvider GetTransportEquipmentProviderMock(int sequenceNumber, string containerIdentificationNumber)
	{
		return Mock.Of<INCTSTransportEquipmentProvider>(transportEquipment =>
			transportEquipment.SequenceNumeric == sequenceNumber &&
			transportEquipment.Id == containerIdentificationNumber &&
			transportEquipment.NumberOfSeals == 2 &&
			transportEquipment.Seals == new Collection<INCTSSealProvider>()
			{
				GetSealProviderMock(1),
				GetSealProviderMock(2),
			} &&
			transportEquipment.GoodsReferences == new Collection<INCTSGoodsReferenceProvider>()
			{
				GetGoodsReferenceProviderMock(1),
				GetGoodsReferenceProviderMock(2),
			}
		);
	}

	INCTSSealProvider GetSealProviderMock(int sequenceNumber)
	{
		return Mock.Of<INCTSSealProvider>(seal =>
			seal.SequenceNumeric == sequenceNumber &&
			seal.Id == $"identifier{sequenceNumber}"
		);
	}

	INCTSGoodsReferenceProvider GetGoodsReferenceProviderMock(int sequenceNumber)
	{
		return Mock.Of<INCTSGoodsReferenceProvider>(reference =>
			reference.SequenceNumeric == sequenceNumber &&
			reference.GoodsItemNumber == sequenceNumber * 100
		);
	}

	INCTSDepartureTransportMeansProvider GetDepartureTransportMeansProviderMock(int sequenceNumber)
	{
		return Mock.Of<INCTSDepartureTransportMeansProvider>(depatureTransportMeans =>
			depatureTransportMeans.SequenceNumeric == sequenceNumber &&
			depatureTransportMeans.TypeOfIdentification == $"{sequenceNumber}0" &&
			depatureTransportMeans.Id == $"id{sequenceNumber}" &&
			depatureTransportMeans.Nationality == "NL"
		);
	}

	INCTSReferenceDocumentProvider GetReferenceDocumentProviderMock(int sequenceNumber, string complementOfInformation)
	{
		return Mock.Of<INCTSReferenceDocumentProvider>(referenceDocument =>
			referenceDocument.SequenceNumeric == sequenceNumber &&
			referenceDocument.Type == $"t{sequenceNumber}" &&
			referenceDocument.ReferenceNumber == $"referenceNumber{sequenceNumber}" &&
			referenceDocument.ComplementOfInformation == complementOfInformation
		);
	}

	INCTSAdditionalInformationProvider GetAdditionalInformationProviderMock(int sequenceNumber)
	{
		return Mock.Of<INCTSAdditionalInformationProvider>(additionalInformation =>
			additionalInformation.SequenceNumeric == sequenceNumber &&
			additionalInformation.Code == $"c{sequenceNumber}" &&
			additionalInformation.Text == $"text{sequenceNumber}"
		);
	}

	INCTSIncidentProvider GetIncidentProviderMock(int sequenceNumber, bool incidentContainerIndicator)
	{
		return Mock.Of<INCTSIncidentProvider>(incident =>
			incident.Code == $"{sequenceNumber}" &&
			incident.Text == $"text{sequenceNumber}" &&
			incident.Endorsement == Mock.Of<INCTSEndorsementProvider>(endorsement =>
				endorsement.Date == new DateTime(2024, 09, 10) &&
				endorsement.Authority == $"authority{sequenceNumber}" &&
				endorsement.Place == $"place{sequenceNumber}" &&
				endorsement.Country == "NL"
			) &&
			incident.Location == Mock.Of<INCTSLocationProvider>(location =>
				location.QualifierOfIdentification == "Z" &&
				location.UnLocode == $"unLocode{sequenceNumber}" &&
				location.Country == "BE" &&
				location.GNSSLongitude == "12" &&
				location.GNSSLatitute == "34" &&
				location.Address == Mock.Of<INCTSAddressProvider>(address =>
					address.StreetAndNumber == $"streetAndNumber{sequenceNumber}" &&
					address.Postcode == $"postcode{sequenceNumber}" &&
					address.City == $"city{sequenceNumber}"
				)
			) &&
			incident.TransportEquipments == new Collection<INCTSTransportEquipmentProvider>()
			{
				GetTransportEquipmentProviderMock(1, "containerId1"),
				GetTransportEquipmentProviderMock(2, "containerId2")
			} &&
			incident.Transhipment == Mock.Of<INCTSTranshipmentProvider>(transhipment =>
				transhipment.ContainerIndicator == incidentContainerIndicator &&
				transhipment.TypeOfIdentification == $"{sequenceNumber}0" &&
				transhipment.Id == $"id{sequenceNumber}" &&
				transhipment.Nationality == "BE"
			)
		);
	}

	INCTSPartyWithAddressProvider GetPartyProviderMock(string id)
	{
		return Mock.Of<INCTSPartyWithAddressProvider>(party =>
			party.Id == id &&
			party.Name == $"name{id}" &&
			party.Address == Mock.Of<INCTSAddressProvider>(address =>
				address.StreetAndNumber == $"streetAndNumber{id}" &&
				address.Postcode == $"postcode{id}" &&
				address.City == $"city{id}" &&
				address.Country == "NL"
			)
		);
	}

	INCTSDangerousGoodsProvider GetDangerousGoodsProviderMock(int sequence)
	{
		return Mock.Of<INCTSDangerousGoodsProvider>(dangerousGoods =>
			dangerousGoods.SequenceNumeric == sequence &&
			dangerousGoods.UNNumber == $"unNbr{sequence}"
		);
	}

	INCTSPackagingProvider GetPackagingProviderMock(int sequence)
	{
		return Mock.Of<INCTSPackagingProvider>(package =>
			package.SequenceNumeric == sequence &&
			package.TypeOfPackages == "UT" &&
			package.NumberOfPackages == 1 + sequence &&
			package.ShippingMarks == $"shippingMarks{sequence}"
		);
	}

	INCTSConsignmentItemProvider GetConsigmentItemProviderMock(int goodsItemNumber)
	{
		return Mock.Of<INCTSConsignmentItemProvider>(item =>
			item.GoodsItemNumber == goodsItemNumber &&
			item.DeclarationGoodsItemNumber == goodsItemNumber * 100 &&
			item.DeclarationType == $"type{goodsItemNumber}" &&
			item.CountryOfDestination == "NL" &&
			item.Commodity == Mock.Of<INCTSCommodityProvider>(comm =>
				comm.DescriptionOfGoods == $"descriptionOfGoods{goodsItemNumber}" &&
				comm.CusCode == $"cusCode{goodsItemNumber}" &&
				comm.HarmonizedSystemSubHeadingCode == "HSYSCD" &&
				comm.CombinedNomenclatureCode == $"N{goodsItemNumber}" &&
				comm.DangerousGoods == new Collection<INCTSDangerousGoodsProvider>()
				{
					GetDangerousGoodsProviderMock(1),
				} &&
				comm.GrossMass == 100 + goodsItemNumber &&
				comm.NetMass == 90 + goodsItemNumber
			) &&
			item.Packagings == new Collection<INCTSPackagingProvider>()
			{
				GetPackagingProviderMock(1),
			} &&
			item.PreviousDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, "complementOfInformation1"),
			} &&
			item.SupportingDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, "complementOfInformation1"),
			} &&
			item.TransportDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, null),
			} &&
			item.AdditionalReferences == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, null),
			} &&
			item.AdditionalInformation == new Collection<INCTSAdditionalInformationProvider>()
			{
				GetAdditionalInformationProviderMock(1),
			}
		);
	}

	INCTSHouseConsignmentProvider GetHouseConsignmentProviderMock(int sequenceNumber)
	{
		return Mock.Of<INCTSHouseConsignmentProvider>(houseConsignment =>
			houseConsignment.SequenceNumeric == sequenceNumber &&
			houseConsignment.GrossMass == sequenceNumber &&
			houseConsignment.SecurityIndicatorFromExportDeclaration == false &&
			houseConsignment.Consignor == GetPartyProviderMock(sequenceNumber.ToString()) &&
			houseConsignment.Consignee == GetPartyProviderMock(sequenceNumber.ToString()) &&
			houseConsignment.DepartureTransportMeans == new Collection<INCTSDepartureTransportMeansProvider>()
			{
				GetDepartureTransportMeansProviderMock(1),
				GetDepartureTransportMeansProviderMock(2)
			} &&
			houseConsignment.PreviousDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, "complementOfInformation1"),
			} &&
			houseConsignment.SupportingDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, "complementOfInformation1"),
			} &&
			houseConsignment.TransportDocuments == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, null),
			} &&
			houseConsignment.AdditionalReferences == new Collection<INCTSReferenceDocumentProvider>()
			{
				GetReferenceDocumentProviderMock(1, null),
			} &&
			houseConsignment.AdditionalInformation == new Collection<INCTSAdditionalInformationProvider>()
			{
				GetAdditionalInformationProviderMock(1),
			} &&
			houseConsignment.ConsignmentItems == new Collection<INCTSConsignmentItemProvider>()
			{
				GetConsigmentItemProviderMock(1),
			}
		);
	}

	string GetExpectedTypeOfSecurity(string security)
	{
		switch (security)
		{
			case "0":
				return "NON";
			case "1":
				return "ENT";
			case "2":
				return "EXI";
			case "3":
				return "BTH";
		}
		return string.Empty;
	}

	void AssertArrivalHeaderContainerAndSeals(NctsArrivalHeaderContainer container, int sequenceNumber, string mode)
	{
		AssertEquals("BC_sequenceNumber", sequenceNumber, container.BC_SequenceNumber);
		AssertEquals("BC_ContainerNum", $"containerId{sequenceNumber}", container.BC_ContainerNum);
		AssertEquals("BK_UnloadingState", NctsUnloadedStateList.Codes.DEC, container.BC_UnloadedState);
		AssertEquals("BC_Mode", mode, container.BC_Mode);
		for (int index = 0; index < 2; index++)
		{
			var seal = container.Seals[index];
			AssertEquals("Seal " + index + " BK_SequenceNumber", index + 1, seal.BK_SequenceNumber);
			AssertEquals("Seal " + index + " BK_SealNumber", $"identifier{index + 1}", seal.BK_SealNumber);
			AssertEquals("Seal " + index + " BK_UnloadingState", NctsUnloadedStateList.Codes.DEC, seal.BK_UnloadingState);
		}
	}

	void AssertNctsContainerAndSealsAndGoodsReference(NctsContainer container, int sequenceNumber, string mode)
	{
		AssertEquals("BC_sequenceNumber", sequenceNumber, container.BC_SequenceNumber);
		AssertEquals("BC_ContainerNum", $"containerId{sequenceNumber}", container.BC_ContainerNum);
		AssertEquals("BC_Mode", mode, container.BC_Mode);
		for (int index = 0; index < 2; index++)
		{
			var seal = container.Seals[index];
			AssertEquals("Seal " + index + " BK_SequenceNumber", index + 1, seal.BK_SequenceNumber);
			AssertEquals("Seal " + index + " BK_SealNumber", $"identifier{index + 1}", seal.BK_SealNumber);
		}
		for (int index = 0; index < 2; index++)
		{
			var goodReference = container.ItemNumbers[index];
			AssertEquals("Good Reference " + index + " CY_Code", "GDS", goodReference.CY_Code);
			AssertEquals("Good Reference " + index + " CY_Type", "ITM", goodReference.CY_Type);
			AssertEquals("Good Reference " + index + " CY_Order", (ZShort)index + 1, goodReference.CY_Order);
			AssertEquals("Good Reference " + index + " CY_Data", ((index + 1) * 100).ToString(), goodReference.CY_Data);
		}
	}

	void AssertArrivalTransportInfos(ArrivalCusTransportMeans transportMeans, int sequenceNumber)
	{
		AssertEquals("TPM_TransportState", NctsUnloadedStateList.Codes.DEC, transportMeans.TPM_TransportState);
		AssertEquals("TPM_SequenceNumber", sequenceNumber, transportMeans.TPM_SequenceNumber);
		AssertEquals("TPM_TypeOfIdentification", $"{sequenceNumber}0", transportMeans.TPM_TypeOfIdentification);
		AssertEquals("TPM_IdentificationNumber", $"id{sequenceNumber}", transportMeans.TPM_IdentificationNumber);
		AssertEquals("TPM_RN_NKTransportNationality", "NL", transportMeans.TPM_RN_NKTransportNationality);
	}

	void AssertPreviousDocuments(CommonPreviousDocument document, int sequenceNumber)
	{
		AssertEquals("CSI_Type", "PRE", document.CSI_Type);
		AssertEquals("CSI_LineNo", sequenceNumber, document.CSI_LineNo);
		AssertEquals("CSI_Code", $"t{sequenceNumber}", document.CSI_Code);
		AssertEquals("CSI_ReferenceNumber", $"referenceNumber{sequenceNumber}", document.CSI_ReferenceNumber);
		AssertEquals("CSI_ReferenceNumber2", $"complementOfInformation{sequenceNumber}", document.CSI_ReferenceNumber2);
		AssertEquals("CSI_Status", NctsUnloadedStateList.Codes.DEC, document.CSI_Status);
	}

	void AssertSupportingDocuments(NctsSupportingDocument document, int sequenceNumber)
	{
		AssertEquals("CSI_Type", "SUP", document.CSI_Type);
		AssertEquals("CSI_LineNo", sequenceNumber, document.CSI_LineNo);
		AssertEquals("CSI_Code", $"t{sequenceNumber}", document.CSI_Code);
		AssertEquals("CSI_ReferenceNumber", $"referenceNumber{sequenceNumber}", document.CSI_ReferenceNumber);
		AssertEquals("CSI_ReferenceNumber2", $"complementOfInformation{sequenceNumber}", document.CSI_ReferenceNumber2);
		AssertEquals("CSI_Status", NctsUnloadedStateList.Codes.DEC, document.CSI_Status);
	}

	void AssertOtherDocuments(NctsAdditionalInfo document, int sequenceNumber, string subType)
	{
		AssertEquals("CSI_Type", "OTH", document.CSI_Type);
		AssertEquals("CSI_SubType", subType, document.CSI_SubType);
		AssertEquals("CSI_LineNo", sequenceNumber, document.CSI_LineNo);
		AssertEquals("CSI_Code", $"t{sequenceNumber}", document.CSI_Code);
		AssertEquals("CSI_ReferenceNumber", $"referenceNumber{sequenceNumber}", document.CSI_ReferenceNumber);
		AssertEquals("CSI_Status", NctsUnloadedStateList.Codes.DEC, document.CSI_Status);
	}

	void AssertAdditionalInformation(NctsAdditionalInfo document, int sequenceNumber)
	{
		AssertEquals("CSI_Type", "OTH", document.CSI_Type);
		AssertEquals("CSI_SubType", "INF", document.CSI_SubType);
		AssertEquals("CSI_LineNo", sequenceNumber, document.CSI_LineNo);
		AssertEquals("CSI_Code", $"c{sequenceNumber}", document.CSI_Code);
		AssertEquals("CSI_Description", $"text{sequenceNumber}", document.CSI_Description);
		AssertEquals("CSI_Status", NctsUnloadedStateList.Codes.DEC, document.CSI_Status);
	}

	void AssertIndicdent(EnRouteIncident incident, int sequenceNumber, string mode)
	{
		AssertEquals("BN_IncidentCode", $"{sequenceNumber}", incident.BN_IncidentCode);
		AssertEquals("BN_Information", $"text{sequenceNumber}", incident.BN_Information);
		AssertEquals("BN_EndorsementDate", new ZDateTime(2024, 09, 10), incident.BN_EndorsementDate);
		AssertEquals("BN_EndorsementAuthority", $"authority{sequenceNumber}", incident.BN_EndorsementAuthority);
		AssertEquals("BN_EndorsementPlace", $"place{sequenceNumber}", incident.BN_EndorsementPlace);
		AssertEquals("BN_EndorsementCountryCode", "NL", incident.BN_EndorsementCountryCode);
		AssertEquals("BN_CustomsStatus", IncidentCustomsStatusList.Codes.CUS, incident.BN_CustomsStatus);
		AssertEquals("BN_LocationQualifier", "Z", incident.BN_LocationQualifier);
		AssertEquals("BN_EventPlace", $"unLocode{sequenceNumber}", incident.BN_EventPlace);
		AssertEquals("BN_EventCountryCode", "BE", incident.BN_EventCountryCode);
		AssertEquals("BN_Geolocation", "POINT (12 34)", incident.BN_GeoLocation.ToString());
		AssertEquals("E2_AddressType", AutoDocAddressTypes.Codes.Location, incident.GoodsLocation.Address.E2_AddressType);
		AssertEquals("E2_Address1", $"streetAndNumber{sequenceNumber}", incident.GoodsLocation.Address.E2_Address1);
		AssertEquals("E2_Postcode", $"postcode{sequenceNumber}", incident.GoodsLocation.Address.E2_Postcode);
		AssertEquals("E2_City", $"city{sequenceNumber}", incident.GoodsLocation.Address.E2_City);
		AssertEquals("TransportEquipment Count", 2, incident.IncidentContainers.Count);
		AssertNctsContainerAndSealsAndGoodsReference(incident.IncidentContainers[0], 1, mode);
		AssertNctsContainerAndSealsAndGoodsReference(incident.IncidentContainers[1], 2, mode);
		AssertEquals("BN_TransportAtDepartureType", $"{sequenceNumber}0", incident.BN_TransportAtDepartureType);
		AssertEquals("BN_TransportAtDepartureID", $"id{sequenceNumber}", incident.BN_TransportAtDepartureID);
		AssertEquals("BN_RN_NKTransportAtDepartureIDNationality", "BE", incident.BN_RN_NKTransportAtDepartureIDNationality);
	}

	void AssertHouseConsignment(NctsHeader nctsHeader, int sequenceNumber)
	{
		var bill = nctsHeader.Bills.Last();
		var moveDetail = bill.MovementDetail;

		AssertEquals("HouseConsignment Sequence Number", sequenceNumber.ToString(), moveDetail.B9_SeqNo);
		AssertEquals("HouseConsignment Unloaded State", NctsUnloadedStateList.Codes.DEC, moveDetail.B9_UnloadedState);
		AssertEquals("HouseConsignment Gross Mass", Convert.ToDecimal(sequenceNumber), bill.B0_Weight);
		AssertEquals("HouseConsignment Security Indicator From Export Declaration", false, bill.B0_SecurityIndicatorFromExport);

		var party = moveDetail.ConsignorDocAddress;
		AssertEquals("Consignor ID", sequenceNumber.ToString(), party.E2_GovRegNum);
		AssertEquals("Consignor ID Type", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, party.E2_GovRegNumType);
		AssertEquals("Consignor Name", $"name{sequenceNumber}", party.E2_CompanyName);
		AssertEquals("Consignor Street And Number", $"streetAndNumber{sequenceNumber}", party.E2_Address1);
		AssertEquals("Consignor Postcode", $"postcode{sequenceNumber}", party.E2_Postcode);
		AssertEquals("Consignor City", $"city{sequenceNumber}", party.E2_City);
		AssertEquals("Consignor Country", "NL", party.E2_RN_NKCountryCode);

		party = moveDetail.ConsigneeDocAddress;
		AssertEquals("Consignee ID", sequenceNumber.ToString(), party.E2_GovRegNum);
		AssertEquals("Consignee ID Type", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, party.E2_GovRegNumType);
		AssertEquals("Consignee Name", $"name{sequenceNumber}", party.E2_CompanyName);
		AssertEquals("Consignee Street And Number", $"streetAndNumber{sequenceNumber}", party.E2_Address1);
		AssertEquals("Consignee Postcode", $"postcode{sequenceNumber}", party.E2_Postcode);
		AssertEquals("Consignee City", $"city{sequenceNumber}", party.E2_City);
		AssertEquals("Consignee Country", "NL", party.E2_RN_NKCountryCode);

		var transportInfo = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.FirstOrDefault();
		AssertEquals("Departure Transport Means Sequence Number", sequenceNumber, transportInfo.TPM_SequenceNumber);
		AssertEquals("Departure Transport Means Type Of Identification", $"{sequenceNumber}0", transportInfo.TPM_TypeOfIdentification);
		AssertEquals("Departure Transport Means ID", $"id{sequenceNumber}", transportInfo.TPM_IdentificationNumber);
		AssertEquals("Departure Transport Means Transport State", NctsUnloadedStateList.Codes.DEC, transportInfo.TPM_TransportState);
		AssertEquals("Departure Transport Means Nationality", "NL", transportInfo.TPM_RN_NKTransportNationality);

		AssertDocuments(sequenceNumber, bill.PreviousDocuments, bill.SupportingDocuments, bill.AdditionalDocuments);

		var goodsItem = bill.ArrivalGoodsItems.FirstOrDefault();
		AssertConsignmentItem(goodsItem, sequenceNumber);
	}

	void AssertConsignmentItem(NctsArrivalCargoDesc goodsItem, int sequence)
	{
		AssertEquals("BY_UnloadedState", NctsUnloadedStateList.Codes.DEC, goodsItem.BY_UnloadedState);
		AssertEquals("BY_LineNo", sequence, goodsItem.BY_LineNo);
		AssertEquals("BY_DeclarationGoodsItemNumber", sequence * 100, goodsItem.BY_DeclarationGoodsItemNumber);
		AssertEquals("BY_Type", $"type{sequence}", goodsItem.BY_Type);
		AssertEquals("BY_RN_NKCountryOfDestination", "NL", goodsItem.BY_RN_NKCountryOfDestination);
		AssertEquals("BY_Description", $"descriptionOfGoods{sequence}", goodsItem.BY_Description);
		AssertEquals("BY_CusC4Number", $"cusCode{sequence}", goodsItem.BY_CusC4Number);
		AssertEquals("BY_HarmonisedTariff", $"HSYSCDN{sequence}", goodsItem.BY_HarmonisedTariff);

		var dangerousGoodsItem = goodsItem.UNDGs.FirstOrDefault();
		AssertEquals("DI_DG_NKSubs", $"unNbr{sequence}", dangerousGoodsItem.DI_DG_NKSubs);

		AssertEquals("BY_GrossWeight", 100m + sequence, goodsItem.BY_GrossWeight);
		AssertEquals("BY_GrossWeightUnit", "KG", goodsItem.BY_GrossWeightUnit);
		AssertEquals("BY_NetWeight", 90m + sequence, goodsItem.BY_NetWeight);
		AssertEquals("BY_NetWeightUnit", "KG", goodsItem.BY_NetWeightUnit);

		var package = goodsItem.Packages.FirstOrDefault();
		AssertEquals("B5_TypeOfDifference", NctsUnloadedStateList.Codes.DEC, package.B5_TypeOfDifference);
		AssertEquals("B5_SequenceNumber", sequence, package.B5_SequenceNumber);
		AssertEquals("B5_UnitType", "UT", package.B5_UnitType);
		AssertEquals("B5_UnitCount", 1 + sequence, package.B5_UnitCount);
		AssertEquals("B5_MarksAndNumbers", $"shippingMarks{sequence}", package.B5_MarksAndNumbers);

		AssertDocuments(sequence, goodsItem.PreviousDocuments, goodsItem.SupportingDocuments, goodsItem.AdditionalInfos);

		AssertArrayEqualsByElements("Link containers - Container Numbers", new ZString[] { "containerId1", "containerId2" }, package.ContainersSelected.ToArray());
	}

	void AssertDocuments(int sequenceNumber, ICusSupportingInfoCollection<CusSupportingInfo> previousDocuments, ICusSupportingInfoCollection<CusSupportingInfo> supportingDocuments, ICusSupportingInfoCollection<CusSupportingInfo> additionalDocuments)
	{
		var previousDocument = previousDocuments.FirstOrDefault();
		AssertEquals("Previous Document Sequence Number", sequenceNumber, previousDocument.CSI_LineNo);
		AssertEquals("Previous Document Code", $"t{sequenceNumber}", previousDocument.CSI_Code);
		AssertEquals("Previous Document Reference Number", $"referenceNumber{sequenceNumber}", previousDocument.CSI_ReferenceNumber);
		AssertEquals("Previous Document Complement Of Information", "complementOfInformation1", previousDocument.CSI_ReferenceNumber2);
		AssertEquals("Previous Document Status", NctsUnloadedStateList.Codes.DEC, previousDocument.CSI_Status);
		AssertEquals("Previous Document Type", CusSupportingInfoTypeList.Codes.PreviousDocument, previousDocument.CSI_Type);

		var supportingDocument = supportingDocuments.FirstOrDefault();
		AssertEquals("Supporting Document Sequence Number", sequenceNumber, supportingDocument.CSI_LineNo);
		AssertEquals("Supporting Document Code", $"t{sequenceNumber}", supportingDocument.CSI_Code);
		AssertEquals("Supporting Document Reference Number", $"referenceNumber{sequenceNumber}", supportingDocument.CSI_ReferenceNumber);
		AssertEquals("Supporting Document Complement Of Information", "complementOfInformation1", supportingDocument.CSI_ReferenceNumber2);
		AssertEquals("Supporting Document Status", NctsUnloadedStateList.Codes.DEC, supportingDocument.CSI_Status);
		AssertEquals("Supporting Document Type", CusSupportingInfoTypeList.Codes.SupportingDocument, supportingDocument.CSI_Type);

		var transportDocument = additionalDocuments.FirstOrDefault(x => x.CSI_SubType == "TRA");
		AssertEquals("Transport Document Sequence Number", sequenceNumber, transportDocument.CSI_LineNo);
		AssertEquals("Transport Document Code", $"t{sequenceNumber}", transportDocument.CSI_Code);
		AssertEquals("Transport Document Reference Number", $"referenceNumber{sequenceNumber}", transportDocument.CSI_ReferenceNumber);
		AssertEquals("Transport Document Status", NctsUnloadedStateList.Codes.DEC, transportDocument.CSI_Status);
		AssertEquals("Transport Document Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, transportDocument.CSI_Type);
		AssertEquals("Transport Document Sub Type", AdditionalInfoSubTypeList.Codes.TransportDocument, transportDocument.CSI_SubType);

		var additionalReference = additionalDocuments.FirstOrDefault(x => x.CSI_SubType == "REF");
		AssertEquals("Additional Reference Sequence Number", sequenceNumber, additionalReference.CSI_LineNo);
		AssertEquals("Additional Reference Code", $"t{sequenceNumber}", additionalReference.CSI_Code);
		AssertEquals("Additional Reference Reference Number", $"referenceNumber{sequenceNumber}", additionalReference.CSI_ReferenceNumber);
		AssertEquals("Additional Reference Status", NctsUnloadedStateList.Codes.DEC, additionalReference.CSI_Status);
		AssertEquals("Additional Reference Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalReference.CSI_Type);
		AssertEquals("Additional Reference Sub Type", AdditionalInfoSubTypeList.Codes.AdditionalReference, additionalReference.CSI_SubType);

		var additionalInformation = additionalDocuments.FirstOrDefault(x => x.CSI_SubType == "INF");
		AssertEquals("Additional Information Sequence Number", sequenceNumber, additionalInformation.CSI_LineNo);
		AssertEquals("Additional Information Code", $"c{sequenceNumber}", additionalInformation.CSI_Code);
		AssertEquals("Additional Information Description", $"text{sequenceNumber}", additionalInformation.CSI_Description);
		AssertEquals("Additional Information Status", NctsUnloadedStateList.Codes.DEC, additionalInformation.CSI_Status);
		AssertEquals("Additional Information Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalInformation.CSI_Type);
		AssertEquals("Additional Information Sub Type", AdditionalInfoSubTypeList.Codes.AdditionalInformation, additionalInformation.CSI_SubType);
	}
}
