using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC182C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC182CMessageProcessor : NCTSResponseMessageProcessor<ICC182CDataProvider>
{
	public CC182CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString NewMessageStatus => null;

	protected override bool SetNewCustomsStatus => false;

	protected override bool SetNewMessageStatus => false;

	protected override bool SetNewPhase => false;

	protected override ZBool IsMessageOkForProcessing(EDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		return moveHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
	}

	protected override ZString LogMessageWhenDiscarded => Res.GetString("CE0E8943-32D3-4FF4-838A-CF1846C6A60B", "The message with interchange ... was discarded, because its 'Status at Customs' has already the status WRO.");

	protected override ZString NoteMessageWhenDiscarded => Res.GetString("BDF445D4-EE1E-46F2-96A8-F7280826DD5E", "The message with interchange ... was discarded, because its 'Status at Customs' has already the status WRO.");

	protected override IMessageInterpreter<ICC182CDataProvider> Interpreter => new CC182CMessageInterpreter();

	protected override ICC182CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc182CType, CC182CDataProvider>();

	protected override void ProcessMessageCore(NL.Business.NLEDIMessage message)
	{
		var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
		var nctsHeader = moveHeader.Header;
		var enRouteIncidents = nctsHeader.EnRouteIncidents;
		var dataProvider = GetMessageDataProvider(message);
		foreach (var incident in dataProvider.Incidents)
		{
			if (!enRouteIncidents.Any(e => e.BN_IncidentCode.EqualsIgnoringCase(incident.Code) && e.BN_EndorsementDate == incident.Endorsement.Date && e.BN_EndorsementPlace.EqualsIgnoringCase(incident.Endorsement.Place) && e.BN_EndorsementCountryCode.EqualsIgnoringCase(incident.Endorsement.Country)))
			{
				CreateIncident(incident, enRouteIncidents);
			}
		}

		void CreateIncident(INCTSIncidentProvider incident, EnRouteIncidentCollection enRouteIncidents)
		{
			var enRouteIncident = enRouteIncidents.AddNew();
			enRouteIncident.BN_IncidentCode = incident.Code;
			enRouteIncident.BN_Information = incident.Text;

			if (incident.Endorsement is INCTSEndorsementProvider endorsement)
			{
				enRouteIncident.BN_EndorsementDate = incident.Endorsement.Date;
				enRouteIncident.BN_EndorsementAuthority = incident.Endorsement.Authority;
				enRouteIncident.BN_EndorsementPlace = incident.Endorsement.Place;
				enRouteIncident.BN_EndorsementCountryCode = incident.Endorsement.Country;
			}

			if (incident.Location is INCTSLocationProvider location)
			{
				enRouteIncident.BN_LocationQualifier = incident.Location.QualifierOfIdentification;
				enRouteIncident.BN_EventPlace = incident.Location.UnLocode;
				enRouteIncident.BN_EventCountryCode = incident.Location.Country;

				if (location.Address is INCTSAddressProvider address)
				{
					enRouteIncident.GoodsLocation.Address.E2_Address1 = address.StreetAndNumber;
					enRouteIncident.GoodsLocation.Address.E2_Postcode = address.Postcode;
					enRouteIncident.GoodsLocation.Address.E2_City = address.City;
				}

				if (!location.GNSSLongitude.IsEmpty() && !location.GNSSLatitute.IsEmpty())
				{
					enRouteIncident.BN_GeoLocation = ZGeography.CreatePoint(double.Parse(location.GNSSLongitude), double.Parse(location.GNSSLatitute));
				}
			}

			if (incident.Transhipment is INCTSTranshipmentProvider transhipment)
			{
				enRouteIncident.BN_TransportAtDepartureType = transhipment.TypeOfIdentification;
				enRouteIncident.BN_TransportAtDepartureID = transhipment.Id;
				enRouteIncident.BN_RN_NKTransportAtDepartureIDNationality = transhipment.Nationality;
			}

			foreach (var equipment in incident.TransportEquipments)
			{
				CreateContainer(equipment, enRouteIncident);
			}
		}

		void CreateContainer(INCTSTransportEquipmentProvider equipment, EnRouteIncident enRouteIncident)
		{
			NctsContainer container = null;
			if (!equipment.Id.IsNullOrEmpty())
			{
				container = enRouteIncident.IncidentContainers.AddNew();
				container.BC_SequenceNumber = (ZShort)equipment.SequenceNumeric;
				container.BC_ContainerNum = equipment.Id;

				foreach (var item in equipment.GoodsReferences)
				{
					CreateGoodsReference(item, container);
				}
			}

			foreach (var xmlSeal in equipment.Seals)
			{
				CreateSeal(xmlSeal, equipment.Id.IsNullOrEmpty() ? null : container, enRouteIncident);
			}
		}

		void CreateSeal(INCTSSealProvider xmlSeal, NctsContainer container, EnRouteIncident enRouteIncident)
		{
			if (container != null)
			{
				if (container.BC_Seal1.IsEmpty)
				{
					container.BC_Seal1 = xmlSeal.Id;
					return;
				}
				if (container.BC_Seal2.IsEmpty)
				{
					container.BC_Seal2 = xmlSeal.Id;
					return;
				}
			}

			var seal = container != null ? container.Seals.AddNew() : enRouteIncident.Seals.AddNew();
			seal.BK_SequenceNumber = (ZShort)xmlSeal.SequenceNumeric;
			seal.BK_SealNumber = xmlSeal.Id;
		}

		void CreateGoodsReference(INCTSGoodsReferenceProvider goodsItem, NctsContainer container)
		{
			var itemNumber = container.ItemNumbers.AddNew();
			itemNumber.CY_Order = (ZShort)goodsItem.SequenceNumeric;
			itemNumber.CY_Data = goodsItem.GoodsItemNumber.ToString();
		}
	}

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => Res.GetString("54649BDD-33DF-4DA6-8327-754FD2A6432F", "The processing of the message with interchange ... failed, because the message could not be linked to a NCTS declaration.");
}
