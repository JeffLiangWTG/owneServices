using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC043C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC043CMessageProcessor : NCTSResponseMessageProcessor<ICC043CDataProvider>
{
	public CC043CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewPhase => false;

	protected override ZString GetNewCustomsStatus(NctsCommonMovementHeader movementHeader) => (string)movementHeader.BM_CustomsStatus switch
	{
		"" => !isContinueUnloading ? NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted : ZString.Empty,
		NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted => isContinueUnloading ? NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks : ZString.Empty,
		_ => ZString.Empty,
	};

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = ((NctsHeader)message.EM_LinkedObject).ArrivalMovementHeader;
		var dataProvider = GetMessageDataProvider(message);
		isContinueUnloading = dataProvider.CTLControlContinueUnloading > ZInt.Zero;
		interchangeNumber = message.EM_InterchangeNumber;
		arrivalStatus = moveHeader.BM_CustomsStatus;
		return arrivalStatus switch
		{
			NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted or NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks or "" => (ZBool)true,
			_ => (ZBool)false,
		};
	}

	protected override ZString LogMessageWhenDiscarded => $"The message is discarded because its 'Arrival Status' has the status {arrivalStatus}.";

	protected override ZString NoteMessageWhenDiscarded => $"The message with interchange {interchangeNumber} is discarded because its 'Arrival Status' has the status {arrivalStatus}.";

	protected override void ProcessMessageCore(NLEDIMessage message)
	{
		var nctsHeader = (NctsHeader)message.EM_LinkedObject;
		var moveHeader = nctsHeader.ArrivalMovementHeader;
		var dataProvider = GetMessageDataProvider(message);
		moveHeader.BM_InBondEntryType = dataProvider.DeclarationType;
		moveHeader.BM_EntryDate = dataProvider.DeclarationAcceptanceDate;
		moveHeader.BM_TypeOfSecurity = GetTypeOfSecurity(dataProvider.Security);
		moveHeader.BM_ReducedDatasetIndicator = dataProvider.ReducedDatasetIndicator;
		moveHeader.BM_RL_NKDestinationPort = dataProvider.CountryOfDestination?.Length > 2 ? dataProvider.CountryOfDestination.Substring(0, 2) : dataProvider.CountryOfDestination;
		moveHeader.BM_InlandTransportMode = dataProvider.InlandModeOfTransport;
		moveHeader.BM_GrossWeight = dataProvider.GrossMass ?? default;
		if (dataProvider.ContainerIndicator != null)
		{
			CreateContainerForConsignment(nctsHeader, dataProvider.TransportEquipments, dataProvider.ContainerIndicator.Value);
			CreateDepartureTransportMeans(moveHeader.ArrivalTransportInfos, dataProvider.DepartureTransportMeans);
			CreateDocument(nctsHeader.PreviousDocuments, dataProvider.PreviousDocuments, CusSupportingInfoTypeList.Codes.PreviousDocument);
			CreateDocument(moveHeader.SupportingDocuments, dataProvider.SupportingDocuments, CusSupportingInfoTypeList.Codes.SupportingDocument);
			CreateDocument(moveHeader.AdditionalDocuments, dataProvider.TransportDocuments, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument);
			CreateDocument(moveHeader.AdditionalDocuments, dataProvider.AdditionalReferences, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalReference);
			CreateDocument(moveHeader.AdditionalDocuments, dataProvider.AdditionalInformation, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			CreateIncident(nctsHeader, dataProvider.Incidents);
		}

		foreach (var consignment in dataProvider.HouseConsignments)
		{
			ProcessHouseConsignment(consignment, nctsHeader, moveHeader);
		}

		LinkEquipmentAndGoodItems(nctsHeader, dataProvider.TransportEquipments);
	}

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByMRN(message, NctsMoveHeaderType.Codes.Arrival);

	protected override IMessageInterpreter<ICC043CDataProvider> Interpreter => new CC043CMessageInterpreter();

	protected override ICC043CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc043CType, CC043CDataProvider>();

	string interchangeNumber;
	string arrivalStatus;
	bool isContinueUnloading;

	string GetTypeOfSecurity(string security) => security switch
	{
		"0" => NctsTypeOfSecurityList.Codes.NON,
		"1" => NctsTypeOfSecurityList.Codes.ENT,
		"2" => NctsTypeOfSecurityList.Codes.EXI,
		"3" => NctsTypeOfSecurityList.Codes.BTH,
		_ => string.Empty,
	};

	void CreateContainerForConsignment(NctsHeader header, IReadOnlyCollection<INCTSTransportEquipmentProvider> transportEquipments, bool containerIndicator)
	{
		header.ArrivalHeaderContainers.RemoveAndDeleteAll();
		header.ArrivalMovementHeader.BM_SealQty = 0;
		foreach (var equipment in transportEquipments)
		{
			if (!equipment.Id.IsNullOrEmpty())
			{
				var container = header.ArrivalHeaderContainers.AddNew();
				container.BC_SequenceNumber = (ZShort)equipment.SequenceNumeric;
				container.BC_ContainerNum = equipment.Id;
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				container.BC_Mode = containerIndicator ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
				header.ArrivalMovementHeader.BM_SealQty += (ZShort)equipment.NumberOfSeals;
				CreateSeal(container, equipment.Seals);
			}
		}
	}

	void CreateSeal(NctsArrivalHeaderContainer container, IReadOnlyCollection<INCTSSealProvider> seals)
	{
		foreach (var s in seals)
		{
			var seal = container.Seals.AddNew();
			seal.BK_SequenceNumber = (ZShort)s.SequenceNumeric;
			seal.BK_SealNumber = s.Id;
			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		}
	}

	void CreateContainerForIncident(EnRouteIncident incident, IReadOnlyCollection<INCTSTransportEquipmentProvider> transportEquipments, bool containerIndicator)
	{
		foreach (var equipment in transportEquipments)
		{
			var container = incident.IncidentContainers.AddNew();
			container.BC_SequenceNumber = (ZShort)equipment.SequenceNumeric;
			container.BC_ContainerNum = equipment.Id;
			container.BC_Mode = containerIndicator ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
			CreateSeal(container, equipment.Seals);
			CreateGoodReferences(container, equipment.GoodsReferences);
		}
	}

	void CreateSeal(NctsContainer container, IReadOnlyCollection<INCTSSealProvider> seals)
	{
		foreach (var s in seals)
		{
			var seal = container.Seals.AddNew();
			seal.BK_SequenceNumber = (ZShort)s.SequenceNumeric;
			seal.BK_SealNumber = s.Id;
		}
	}

	void CreateGoodReferences(NctsContainer container, IReadOnlyCollection<INCTSGoodsReferenceProvider> goodReferences)
	{
		foreach (var goodReference in goodReferences)
		{
			var itemNumber = container.ItemNumbers.AddNew();
			itemNumber.CY_Code = "GDS";
			itemNumber.CY_Type = "ITM";
			itemNumber.CY_Order = (ZShort)goodReference.SequenceNumeric;
			itemNumber.CY_Data = goodReference.GoodsItemNumber.ToString();
		}
	}

	static void CreateDepartureTransportMeans(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> transportInfos, IReadOnlyCollection<INCTSDepartureTransportMeansProvider> transportMeans)
	{
		transportInfos.RemoveAndDeleteAll();
		foreach (var transportMean in transportMeans)
		{
			var transportInfo = transportInfos.AddNew();
			transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			transportInfo.TPM_SequenceNumber = transportMean.SequenceNumeric.Equals(0) ? ZShort.Zero : (ZShort)transportMean.SequenceNumeric;
			transportInfo.TPM_TypeOfIdentification = transportMean.TypeOfIdentification;
			transportInfo.TPM_IdentificationNumber = transportMean.Id;
			transportInfo.TPM_RN_NKTransportNationality = transportMean.Nationality;
		}
	}

	static void CreateDocument<T>(ICusSupportingInfoCollection<T> supportingInfoCollection, IReadOnlyCollection<INCTSReferenceDocumentProvider> documents, string type, string subType = "") where T : CusSupportingInfo
	{
		foreach (var document in documents)
		{
			var supportingInfo = supportingInfoCollection.AddNew();
			supportingInfo.CSI_Type = type;
			supportingInfo.CSI_SubType = subType;
			supportingInfo.CSI_LineNo = document.SequenceNumeric;
			supportingInfo.CSI_Code = document.Type;
			supportingInfo.CSI_ReferenceNumber = document.ReferenceNumber;
			supportingInfo.CSI_ReferenceNumber2 = document.ComplementOfInformation;
			supportingInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}
	}

	static void CreateDocument<T>(ICusSupportingInfoCollection<T> supportingInfoCollection, IReadOnlyCollection<INCTSAdditionalInformationProvider> documents, string type, string subType = "") where T : CusSupportingInfo
	{
		foreach (var document in documents)
		{
			var supportingInfo = supportingInfoCollection.AddNew();
			supportingInfo.CSI_Type = type;
			supportingInfo.CSI_SubType = subType;
			supportingInfo.CSI_LineNo = document.SequenceNumeric;
			supportingInfo.CSI_Code = document.Code;
			supportingInfo.CSI_Description = document.Text;
			supportingInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}
	}

	void CreateIncident(NctsHeader header, IReadOnlyCollection<INCTSIncidentProvider> incidents)
	{
		header.BH_ExportFlag = incidents.Count > 0 ? EventFlagList.Codes.Yes : EventFlagList.Codes.No;
		foreach (var incident in incidents)
		{
			var enRouteIncident = header.EnRouteIncidents.AddNew();
			enRouteIncident.BN_IncidentCode = incident.Code;
			enRouteIncident.BN_Information = incident.Text;
			enRouteIncident.BN_EndorsementDate = incident.Endorsement?.Date ?? default;
			enRouteIncident.BN_EndorsementAuthority = incident.Endorsement?.Authority;
			enRouteIncident.BN_EndorsementPlace = incident.Endorsement?.Place;
			enRouteIncident.BN_EndorsementCountryCode = incident.Endorsement?.Country;
			enRouteIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.CUS;
			enRouteIncident.BN_LocationQualifier = incident.Location.QualifierOfIdentification;
			enRouteIncident.BN_EventPlace = incident.Location.UnLocode;
			enRouteIncident.BN_EventCountryCode = incident.Location.Country;
			if (!incident.Location.GNSSLongitude.IsNullOrEmpty() && !incident.Location.GNSSLatitute.IsNullOrEmpty())
			{
				enRouteIncident.BN_GeoLocation = ZGeography.CreatePoint(double.Parse(incident.Location.GNSSLongitude), double.Parse(incident.Location.GNSSLatitute));
			}
			if (incident.Location.Address is INCTSAddressProvider address)
			{
				enRouteIncident.GoodsLocation.Address.E2_AddressType = AutoDocAddressTypes.Codes.Location;
				enRouteIncident.GoodsLocation.Address.E2_Address1 = address.StreetAndNumber;
				enRouteIncident.GoodsLocation.Address.E2_Postcode = address.Postcode;
				enRouteIncident.GoodsLocation.Address.E2_City = address.City;
			}
			CreateContainerForIncident(enRouteIncident, incident.TransportEquipments, incident.Transhipment?.ContainerIndicator ?? false);
			enRouteIncident.BN_TransportAtDepartureType = incident.Transhipment?.TypeOfIdentification;
			enRouteIncident.BN_TransportAtDepartureID = incident.Transhipment?.Id;
			enRouteIncident.BN_RN_NKTransportAtDepartureIDNationality = incident.Transhipment?.Nationality;
		}
	}

	static void CreateParty(JobDocAddress party, INCTSPartyWithAddressProvider providerParty)
	{
		if (providerParty != null)
		{
			var identification = providerParty.Id;
			if (!identification.IsNullOrEmpty())
			{
				party.E2_GovRegNum = identification;
				party.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			}
			party.E2_CompanyName = providerParty.Name;

			if (providerParty.Address != null)
			{
				var address = providerParty.Address;
				party.E2_Address1 = address.StreetAndNumber;
				party.E2_Postcode = address.Postcode;
				party.E2_City = address.City;
				party.E2_RN_NKCountryCode = address.Country;
				party.E2_AddressOverride = true;
			}
		}
	}

	static void CreateGoodItems(NctsBill bill, IReadOnlyCollection<INCTSConsignmentItemProvider> consignmentItems)
	{
		foreach (var consignmentItem in consignmentItems)
		{
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem.BY_LineNo = (ZShort)consignmentItem.GoodsItemNumber;
			goodsItem.BY_DeclarationGoodsItemNumber = consignmentItem.DeclarationGoodsItemNumber;
			goodsItem.BY_Type = consignmentItem.DeclarationType;
			goodsItem.BY_RN_NKCountryOfDestination = consignmentItem.CountryOfDestination;

			CreateCommodity(goodsItem, consignmentItem.Commodity);
			CreatePackaging(goodsItem, consignmentItem.Packagings);
			CreateDocument(goodsItem.PreviousDocuments, consignmentItem.PreviousDocuments, CusSupportingInfoTypeList.Codes.PreviousDocument);
			CreateDocument(goodsItem.SupportingDocuments, consignmentItem.SupportingDocuments, CusSupportingInfoTypeList.Codes.SupportingDocument);
			CreateDocument(goodsItem.AdditionalInfos, consignmentItem.TransportDocuments, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument);
			CreateDocument(goodsItem.AdditionalInfos, consignmentItem.AdditionalReferences, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalReference);
			CreateDocument(goodsItem.AdditionalInfos, consignmentItem.AdditionalInformation, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalInformation);
		}
	}

	static void CreateCommodity(NctsArrivalCargoDesc goodsItem, INCTSCommodityProvider consignmentItemCommodity)
	{
		goodsItem.BY_Description = consignmentItemCommodity.DescriptionOfGoods;
		goodsItem.BY_CusC4Number = consignmentItemCommodity.CusCode;
		goodsItem.BY_HarmonisedTariff = consignmentItemCommodity.HarmonizedSystemSubHeadingCode + consignmentItemCommodity.CombinedNomenclatureCode;
		goodsItem.BY_GrossWeight = consignmentItemCommodity.GrossMass ?? ZDecimal.Zero;
		goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		goodsItem.BY_NetWeight = consignmentItemCommodity.NetMass ?? ZDecimal.Zero;
		goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
		foreach (var dangerousGood in consignmentItemCommodity.DangerousGoods)
		{
			var undg = goodsItem.UNDGs.AddNew();
			undg.DI_DG_NKSubs = dangerousGood.UNNumber;
		}
	}

	static void CreatePackaging(NctsArrivalCargoDesc goodsItem, IReadOnlyCollection<INCTSPackagingProvider> packagings)
	{
		foreach (var packaging in packagings)
		{
			var package = goodsItem.Packages.AddNew();
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			package.B5_SequenceNumber = (ZShort)packaging.SequenceNumeric;
			package.B5_UnitType = packaging.TypeOfPackages;
			package.B5_UnitCount = (ZLong)packaging.NumberOfPackages;
			package.B5_MarksAndNumbers = packaging.ShippingMarks;
		}
	}

	static void ProcessHouseConsignment(INCTSHouseConsignmentProvider houseConsignment, NctsHeader nctsHeader, NctsArrivalMovementHeader moveHeader)
	{
		var bill = nctsHeader.Bills.AddNew();
		var moveDetail = bill.MovementDetail;

		moveDetail.B9_BM = moveHeader.PK;
		moveDetail.B9_B9_InBondMoveDetail = ZGuid.Empty;
		moveDetail.B9_SeqNo = houseConsignment.SequenceNumeric.ToString();
		moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		ZDecimal grossMass = houseConsignment.GrossMass;
		ZString grossMassUnit = Core.Constants.Weight.Kilograms;
		if (grossMass.DecimalPlaces > 3)
		{
			grossMass *= 1000;
			grossMassUnit = Core.Constants.Weight.Grams;
		}
		bill.B0_Weight = grossMass;
		bill.B0_WeightUQ = grossMassUnit;
		bill.B0_SecurityIndicatorFromExport = houseConsignment.SecurityIndicatorFromExportDeclaration.Equals("1");

		if (houseConsignment.Consignor is INCTSPartyWithAddressProvider consignor)
		{
			CreateParty(moveDetail.ConsignorDocAddress, consignor);
		}

		if (houseConsignment.Consignee is INCTSPartyWithAddressProvider consignee)
		{
			CreateParty(moveDetail.ConsigneeDocAddress, consignee);
		}

		CreateDepartureTransportMeans(bill.ArrivalTransportInfos, houseConsignment.DepartureTransportMeans);
		CreateDocument(bill.PreviousDocuments, houseConsignment.PreviousDocuments, CusSupportingInfoTypeList.Codes.PreviousDocument);
		CreateDocument(bill.SupportingDocuments, houseConsignment.SupportingDocuments, CusSupportingInfoTypeList.Codes.SupportingDocument);
		CreateDocument(bill.AdditionalDocuments, houseConsignment.TransportDocuments, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument);
		CreateDocument(bill.AdditionalDocuments, houseConsignment.AdditionalReferences, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalReference);
		CreateDocument(bill.AdditionalDocuments, houseConsignment.AdditionalInformation, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalInformation);

		CreateGoodItems(bill, houseConsignment.ConsignmentItems);
	}

	static void LinkEquipmentAndGoodItems(NctsHeader nctsHeader, IReadOnlyCollection<INCTSTransportEquipmentProvider> equipments)
	{
		foreach (var equipment in equipments)
		{
			var containerNumber = equipment.Id;
			var dbEquipments = nctsHeader.ArrivalHeaderContainers.Where(x => x.BC_ContainerNum == containerNumber);
			var dbEquipment = dbEquipments.SingleOrDefault();
			if (dbEquipment != null)
			{
				var goodsReferences = equipment.GoodsReferences.Select(x => x.GoodsItemNumber).ToHashSet();
				var packages = nctsHeader.Bills
					.SelectMany(x => x.ArrivalGoodsItems)
					.Where(x => goodsReferences
					.Contains(x.BY_DeclarationGoodsItemNumber))
					.SelectMany(x => x.Packages)
					.Where(x => !x.IsInDatabase);

				foreach (var package in packages)
				{
					var genPivot = nctsHeader.Factory.New<NctsCusInBondContainerPackageGenPivot>();
					genPivot.XX_Relation1ID = package.PK;
					genPivot.XX_Relation2ID = dbEquipment.PK;
				}
			}
		}
	}
}
