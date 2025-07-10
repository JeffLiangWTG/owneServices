using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class JobVoyageReferences : IReferencesParent
	{
		public JobVoyageReferences(IXmlEventValueObjectContextValueList context)
		{
			Argument.NotNull(context, "context");
			Hydrate(context);
		}

		public JobVoyageReferences(Shipment shipmentDataObject)
		{
			Argument.NotNull(shipmentDataObject, nameof(shipmentDataObject));
			Hydrate(shipmentDataObject);
		}

		public JobVoyageReferences(JobVoyage voyage)
		{
			Argument.NotNull(voyage, "voyage");
			Hydrate(voyage);
		}

		public ZString TransportMode { get; set; }
		public ZString VesselName { get; set; }
		public ZString VoyageFlight { get; set; }
		public ZGuid CarrierPK { get; set; }
		public ZDateTime FlightDate { get; set; }
		public ZBool IsCharter { get; set; }
		public ZString LloydsNumber { get; set; }
		public ZString VesselCallSign { get; set; }
		public ZString CarrierSCACCode { get; set; }
		public ZString CarrierOrgCode { get; set; }

		void Hydrate(IXmlEventValueObjectContextValueList context)
		{
			TransportMode = context.TransportMode;
			VesselName = context.VesselName.GetValueOrDefault();
			if (TransportMode == Core.Constants.TransportModes.Air)
			{
				VoyageFlight = context.FlightNumber.GetValueOrDefault();
			}
			else
			{
				VoyageFlight = context.VoyageNumber.GetValueOrDefault();
			}
			CarrierPK = ZGuid.Empty;
			CarrierOrgCode = ZString.Empty;
			FlightDate = context.FlightDate;
			IsCharter = context.IsCharter;
			if (TransportMode == Core.Constants.TransportModes.Sea)
			{
				LloydsNumber = context.LloydsNumber.GetValueOrDefault();
				VesselCallSign = context.VesselCallSign;
				CarrierSCACCode = context.CarrierCode.GetValueOrDefault();
			}
		}

		void Hydrate(Shipment dataObject)
		{
			TransportMode = dataObject.TransportMode?.Code.GetValueOrDefault() ?? ZString.Empty;
			VesselName = dataObject.VesselName.GetValueOrDefault();
			VoyageFlight = dataObject.VoyageFlightNo.GetValueOrDefault();

			if (TransportMode == Core.Constants.TransportModes.Sea)
			{
				LloydsNumber = dataObject.LloydsIMO.GetValueOrDefault();
			}

			CarrierPK = ZGuid.Empty;
			CarrierOrgCode = GetCarrierOrgCode(dataObject);
			CarrierSCACCode = GetCarrierSCACCode(dataObject);

			if (dataObject.TransportLegCollection != null && dataObject.TransportLegCollection.Any())
			{
				var firstLeg = dataObject.TransportLegCollection
					.Cast<TransportLeg>()
					.Where(t => t.EstimatedDeparture.HasValue)
					.OrderBy(t => t.EstimatedDeparture)
					.FirstOrDefault();
				if (firstLeg != null)
				{
					FlightDate = firstLeg.EstimatedDeparture.Value;
				}
			}
		}

		ZString GetCarrierOrgCode(Shipment dataObject)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var carrier = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Carrier));
				if (carrier != null && carrier.OrganizationCode.HasValue)
				{
					return carrier.OrganizationCode.Value;
				}

				if (dataObject.TransportLegCollection != null)
				{
					var leg = dataObject.TransportLegCollection.FirstOrDefault(t =>
						t.Carrier != null
						&& t.Carrier.OrganizationCode.HasValue);
					if (leg != null)
					{
						return leg.Carrier.OrganizationCode.Value;
					}
				}
			}

			return ZString.Empty;
		}

		ZString GetCarrierSCACCode(Shipment dataObject)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var carrier = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Carrier));
				if (carrier != null)
				{
					var scacCode = GetSCACCode(carrier.RegistrationNumberCollection);
					if (!scacCode.IsEmpty)
					{
						return scacCode;
					}
				}
			}

			if (dataObject.TransportLegCollection != null)
			{
				var leg = dataObject.TransportLegCollection.FirstOrDefault(t =>
					t.Carrier != null
					&& !GetSCACCode(t.Carrier.RegistrationNumberCollection).IsEmpty);
				if (leg != null)
				{
					return GetSCACCode(leg.Carrier.RegistrationNumberCollection);
				}
			}

			return ZString.Empty;
		}

		ZString GetSCACCode(List<RegistrationNumber> registrationNumbers)
		{
			if (registrationNumbers != null)
			{
				var scacRegistrationNumber = registrationNumbers.FirstOrDefault(x =>
						x.Type != null
						&& x.Type.Code.HasValue
						&& x.Type.Code.Value == OrgCusCode.CodeTypes.CarrierCode
						&& x.CountryOfIssue != null
						&& x.CountryOfIssue.Code.HasValue
						&& x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.UnitedStates
						&& x.Value.HasValue);

				if (scacRegistrationNumber != null)
				{
					return scacRegistrationNumber.Value.Value;
				}
			}

			return ZString.Empty;
		}

		void Hydrate(JobVoyage voyage)
		{
			TransportMode = voyage.JV_AirSeaRoad;
			VesselName = voyage.JV_RV_NKVessel;
			VoyageFlight = voyage.JV_VoyageFlight;
			CarrierPK = voyage.JV_OH_Line;
			FlightDate = voyage.JV_FlightDate;
			IsCharter = voyage.JV_IsChartered;
			if (TransportMode == Core.Constants.TransportModes.Sea)
			{
				if (voyage.Vessel != null)
				{
					LloydsNumber = voyage.Vessel.RV_LloydsNumber;
					VesselCallSign = voyage.Vessel.RV_RadioCallSign;
				}

				if (voyage.Line != null)
				{
					CarrierSCACCode = voyage.Line.SCACCode;
				}
			}
		}
	}
}
