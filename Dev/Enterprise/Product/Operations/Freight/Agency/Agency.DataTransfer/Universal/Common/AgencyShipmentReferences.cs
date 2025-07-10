using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyShipmentReferences : IReferencesParent
	{
		public AgencyShipmentReferences(IXmlEventValueObjectContextValueList context)
		{
			Argument.NotNull(context, "context");
			Hydrate(context);
		}

		public AgencyShipmentReferences(UniversalShipment dataObject, ZGuid bookingPartyPk, ZString bookingPartyName)
		{
			Argument.NotNull(dataObject, "dataObject");
			Hydrate(dataObject, bookingPartyPk, bookingPartyName);
		}

		public AgencyShipmentReferences(AgencyShipment agencyShipment)
		{
			Argument.NotNull(agencyShipment, "agencyShipment");
			Hydrate(agencyShipment);
		}

		public ZString OceanBillNumber { get; private set; }

		public ZString CarriersBookingReference { get; private set; }

		public ZString AgentsReference { get; private set; }

		public ZGuid BookingPartyPK { get; private set; }

		public ZString BookingPartyName { get; private set; }

		public ZBool IsTargettedToBothAgentModules { get; private set; }

		public ZBool IsCarrierVGM { get; private set; }

		public ZBool IsVGM { get; private set; }

		public ZString PurposeCode { get; private set; }

		public ZString OriginUNLOCO { get; set; }

		public ZString DestinationUNLOCO { get; set; }

		public ZString ShipmentID { get; set; }

		public ZString ContainerNumber { get; set; }

		#region SailingReferences

		public IEnumerable<SailingReference> SailingReferences
		{
			get { return sailingReferences; }
		}

		readonly List<SailingReference> sailingReferences = new List<SailingReference>();

		#endregion

		#region Implementation

		void Hydrate(IXmlEventValueObjectContextValueList context)
		{
			OceanBillNumber = context.MBOLNumber.GetValueOrDefault();
			CarriersBookingReference = context.CarriersBookingReference;
			AgentsReference = context.AgentsReference;

			PurposeCode = ZString.Empty;

			sailingReferences.Add(new SailingReference
			{
				LloydsNumber = context.LloydsNumber.GetValueOrDefault(),
				VesselName = context.VesselName.GetValueOrDefault(),
				VoyageNumber = context.VoyageNumber.GetValueOrDefault(),
				PortOfLoading = context.LegOriginUNLOCO,
				PortOfDischarge = context.LegDestinationUNLOCO
			});
		}

		void Hydrate(UniversalShipment dataObject, ZGuid bookingPartyPk, ZString bookingPartyName)
		{
			OceanBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			CarriersBookingReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
			AgentsReference = dataObject.AgentsReference.GetValueOrDefault();

			BookingPartyPK = bookingPartyPk;
			BookingPartyName = bookingPartyName;

			ContainerNumber = dataObject.ContainerCollection?.FirstOrDefault()?.ContainerNumber.GetValueOrDefault() ?? ZString.Empty;

			IsCarrierVGM = dataObject.ContainsRecipientRoleAndServiceCode(RecipientRoleType.CAR, ServiceCodeType.VGM);
			IsVGM = dataObject.ContainsServiceCode(ServiceCodeType.VGM);

			IsTargettedToBothAgentModules = dataObject.ContainsDataTargetType(DataContextType.BillOfLading)
											&& dataObject.ContainsDataTargetType(DataContextType.AgencyBooking);

			PurposeCode = dataObject.DataContext?.DocumentaryOverride?.Purpose?.Code ?? ZString.Empty;

			if (dataObject.TransportLegCollection != null && dataObject.TransportLegCollection.Count > 0)
			{
				sailingReferences.AddRange(
					dataObject.TransportLegCollection
					.Where(transportLeg => transportLeg.TransportMode.HasValue && transportLeg.TransportMode.Value == TransportMode.Sea)
					.Select(GetSailingReference)
					.Distinct());
			}
		}

		void Hydrate(AgencyShipment agencyShipment)
		{
			OceanBillNumber = agencyShipment.JS_HouseBill;
			CarriersBookingReference = agencyShipment.JS_CFSReference;
			AgentsReference = agencyShipment.JS_BookingReference;

			BookingPartyPK = agencyShipment.BookingPartyDocumentaryAddress.HasRealOrganisation ? agencyShipment.BookingParty.PK : ZGuid.Empty;
			BookingPartyName = agencyShipment.BookingPartyDocumentaryAddress.E2_AddressOverride ? agencyShipment.BookingPartyDocumentaryAddress.CompanyName.ToUpper() : ZString.Empty;

			PurposeCode = ZString.Empty;

			if (agencyShipment.Sailing != null)
			{
				var sailingReference = new SailingReference
				{
					LloydsNumber = (agencyShipment.Sailing.Vessel != null) ? agencyShipment.Sailing.Vessel.RV_LloydsNumber : ZString.Empty,
					VesselName = agencyShipment.Sailing.JX_JV_NKVessel,
					VoyageNumber = agencyShipment.Sailing.JX_JV_VoyageFlight,
					PortOfLoading = agencyShipment.Sailing.JX_JA_RL_NKPortOfLoading,
					PortOfDischarge = agencyShipment.Sailing.JX_JB_RL_NKPortOfDischarge
				};

				sailingReferences.Add(sailingReference);
			}
		}

		SailingReference GetSailingReference(TransportLeg transportLeg)
		{
			return new SailingReference
			{
				VesselName = transportLeg.GetVesselName(),
				VoyageNumber = transportLeg.VoyageFlightNo.GetValueOrDefault(),
				PortOfLoading = transportLeg.PortOfLoading != null ? transportLeg.PortOfLoading.Code.GetValueOrDefault() : ZString.Empty,
				PortOfDischarge = transportLeg.PortOfDischarge != null ? transportLeg.PortOfDischarge.Code.GetValueOrDefault() : ZString.Empty
			};
		}

		public virtual List<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			contextValues.AddIfNotEmpty(Event.ContextTypes.MBOLNumber, OceanBillNumber);
			contextValues.AddIfNotEmpty(Event.ContextTypes.CarriersBookingReference, CarriersBookingReference);
			contextValues.AddIfNotEmpty(Event.ContextTypes.AgentsReference, AgentsReference);

			if (SailingReferences.Any())
			{
				contextValues.AddIfNotEmpty(Event.ContextTypes.LloydsNumber, SailingReferences.First().LloydsNumber);
				contextValues.AddIfNotEmpty(Event.ContextTypes.VesselName, SailingReferences.First().VesselName);
				contextValues.AddIfNotEmpty(Event.ContextTypes.VoyageNumber, SailingReferences.First().VoyageNumber);
				contextValues.AddIfNotEmpty(Event.ContextTypes.LegOriginUNLOCO, SailingReferences.First().PortOfLoading);
				contextValues.AddIfNotEmpty(Event.ContextTypes.LegDestinationUNLOCO, SailingReferences.First().PortOfDischarge);
			}

			return contextValues;
		}

		#endregion
	}
}


