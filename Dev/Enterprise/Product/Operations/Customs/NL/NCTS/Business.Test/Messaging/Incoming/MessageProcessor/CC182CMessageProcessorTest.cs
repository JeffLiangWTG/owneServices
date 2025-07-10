using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC182CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC182CMessageProcessor, ICC182CDataProvider>
{
	public void TestIncidentsCreated()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) => SetupNctsHeaderForCC182C(nctsHeader, includeIncident: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;
		var dataProvider = GetMessageDataProviderMock();

		AssertEquals("Incidents created", 2, enRouteIncidents.Find(e => dataProvider.Incidents.Any(i => e.BN_IncidentCode.EqualsIgnoringCase(i.Code) && i.Endorsement.Date == e.BN_EndorsementDate &&
																										e.BN_EndorsementAuthority.EqualsIgnoringCase(i.Endorsement.Authority) && i.Endorsement.Place.Equals(e.BN_EndorsementPlace) &&
																										e.BN_EndorsementCountryCode.EqualsIgnoringCase(i.Endorsement.Country))).Count());
		AssertEquals("Incident GoodsLocation address created", 2, enRouteIncidents.Sum(x => x.GoodsLocation.Address.IsNull ? 0 : 1));
	}

	public void TestNoExtraIncidentsCreated()
	{
		var dataProviderMock = GetMessageDataProviderMock(includeContainerID: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;

		AssertEquals("No extra incidents created", 2, nctsHeader.EnRouteIncidents.Find(e => dataProviderMock.Incidents.Any(i => e.BN_IncidentCode.EqualsIgnoringCase(i.Code) && i.Endorsement.Date == e.BN_EndorsementDate &&
																															e.BN_EndorsementAuthority.EqualsIgnoringCase(i.Endorsement.Authority) && e.BN_EndorsementPlace.EqualsIgnoringCase(i.Endorsement.Place) &&
																															e.BN_EndorsementCountryCode.EqualsIgnoringCase(i.Endorsement.Country))).Count());
	}

	public void TestContainersCreated()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;

		AssertEquals("Containers created", 4, enRouteIncidents.Sum(x => x.IncidentContainers.Count));
		AssertEquals("CusCodeData created", 8, enRouteIncidents.Sum(x => x.IncidentContainers.Sum(c => c.GetCusCodeDataChildren().Length)));
	}

	public void TestNoContainersCreated()
	{
		var dataProviderMock = GetMessageDataProviderMock(includeContainerID: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;

		AssertEquals("Containers created", 0, enRouteIncidents.Sum(x => x.IncidentContainers.Count));
		AssertEquals("CusCodeData created", 0, enRouteIncidents.Sum(x => x.IncidentContainers.Sum(c => c.GetCusCodeDataChildren().Length)));
	}

	public void TestSealsCreatedOnContainer()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;

		AssertEquals("Seals created on incident (BN)", 0, enRouteIncidents.Sum(x => x.Seals.Count));
		AssertEquals("Seals created on container (BC)", 8, enRouteIncidents.Sum(x => x.IncidentContainers.Sum(c => c.TotalSealCount)));
	}

	public void TestSealsCreatedOIncident()
	{
		var dataProviderMock = GetMessageDataProviderMock(includeContainerID: false);
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, dataProviderMock, SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;

		AssertEquals("Seals created on incident (BN)", 8, enRouteIncidents.Sum(x => x.Seals.Count));
		AssertEquals("Seals created on container (BC)", 0, enRouteIncidents.Sum(x => x.IncidentContainers.Sum(c => c.TotalSealCount)));
	}

	public static ICC182CDataProvider GetMessageDataProviderMock(bool includeContainerID)
	{
		return Mock.Of<ICC182CDataProvider>(p =>
			p.MRN == "22NL000000000012J3" &&
			p.CustomsOfficeOfIncidentRegistrationReferenceNumber == "REF" &&
			p.IncidentNotificationDateAndTime == new DateTime(2024, 03, 28, 12, 25, 45) &&
			p.Incidents == new Collection<INCTSIncidentProvider>()
			{
				GetIncidentProviderMock(1, "1", "Z", includeContainerID, 0),
				GetIncidentProviderMock(2, "2", "W", includeContainerID, 2),
			}
		);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool SetNewCustomsStatusExpected => false;

	protected override bool SetNewMessageStatusExpected => false;

	protected override bool SetNewPhaseExpected => false;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => SetupNctsHeaderForCC182C(nctsHeader, includeIncident: true);

	protected override ICC182CDataProvider GetMessageDataProviderMock() => GetMessageDataProviderMock(includeContainerID: true);

	static INCTSIncidentProvider GetIncidentProviderMock(int sequence, string code, string qualifierOfIdentification, bool includeContainerID, int sealCount)
	{
		return Mock.Of<INCTSIncidentProvider>(i =>
			i.SequenceNumeric == sequence &&
			i.Code == code &&
			i.Text == "TX" &&
			i.Endorsement.Place == "PL" + code &&
			i.Endorsement.Country == "EC" &&
			i.Endorsement.Date == new DateTime(2024, 03, 28, 12, 25, 45) &&
			i.Endorsement.Authority == "AU" &&
			i.Location.Country == "LC" &&
			i.Location.GNSSLatitute == "GN" &&
			i.Location.QualifierOfIdentification == qualifierOfIdentification &&
			i.Location.Address.Country == "AC" &&
			i.Location.Address.City == "AI" &&
			i.Location.Address.Postcode == "PC" &&
			i.Location.Address.StreetAndNumber == "SN" &&
			i.TransportEquipments == new List<INCTSTransportEquipmentProvider>()
			{
				GetTransportEquipmentProviderMock(1, includeContainerID, sealCount),
				GetTransportEquipmentProviderMock(2, includeContainerID, sealCount + 2),
			});
	}

	static INCTSTransportEquipmentProvider GetTransportEquipmentProviderMock(int sequence, bool includeContainerID, int sealCount)
	{
		return Mock.Of<INCTSTransportEquipmentProvider>(t =>
			t.SequenceNumeric == sequence &&
			t.Id == (includeContainerID ? $"ID{sequence}" : string.Empty) &&
			t.NumberOfSeals == 2 &&
			t.GoodsReferences == (includeContainerID
				? new List<INCTSGoodsReferenceProvider>()
				{
					GetGoodsReferenceProvider(1),
					GetGoodsReferenceProvider(2),
				}
				: null
				) &&
			t.Seals == new List<INCTSSealProvider>()
			{
				GetSealProvider(1 + sealCount),
				GetSealProvider(2 + sealCount),
			});
	}

	static INCTSGoodsReferenceProvider GetGoodsReferenceProvider(int sequence)
	{
		return Mock.Of<INCTSGoodsReferenceProvider>(g =>
			g.SequenceNumeric == sequence &&
			g.GoodsItemNumber == 100 + sequence);
	}

	static INCTSSealProvider GetSealProvider(int sequence)
	{
		return Mock.Of<INCTSSealProvider>(s =>
			s.SequenceNumeric == sequence &&
			s.Id == $"ID{sequence}");
	}

	void SetupNctsHeaderForCC182C(NctsHeader nctsHeader, bool includeIncident)
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J3";

		if (includeIncident)
		{
			SetupNctsHeaderIncident(nctsHeader, "PL1");
			SetupNctsHeaderIncident(nctsHeader, "PL2");
		}
	}

	void SetupNctsHeaderIncident(NctsHeader nctsHeader, string place)
	{
		var incident = nctsHeader.EnRouteIncidents.AddNew();
		incident.BN_EndorsementDate = new DateTime(2024, 03, 28, 12, 25, 45);
		incident.BN_EndorsementAuthority = "AU";
		incident.BN_EndorsementPlace = place;
		incident.BN_EndorsementCountryCode = "EC";
	}
}
