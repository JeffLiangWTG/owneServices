using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class XsdPlannedLegObjectHelper
	{
		#region Import

		public static void ImportPlannedLegs(TransportCollection transports, Xsd.PlannedLegCollection plannedLegs, IValueObjectImportContext context, string errorContext)
		{
			Array.ForEach(plannedLegs.Cast<Xsd.PlannedLeg>().Where(leg => leg.Item is Xsd.SailingWithVesselVoyage).ToArray(),
			new Action<Xsd.PlannedLeg>((Xsd.PlannedLeg leg) =>
			{
				((Xsd.SailingWithVesselVoyage)leg.Item).VesselName = VesselNameImportHelper.MatchVesselAndGetVesselName((Xsd.SailingWithVesselVoyage)leg.Item, context.Factory);
			}));

			Array.ForEach(plannedLegs.Cast<Xsd.PlannedLeg>().Where(leg => leg.Item is Xsd.SailingForPlannedLegs).ToArray(),
			new Action<Xsd.PlannedLeg>((Xsd.PlannedLeg leg) =>
			{
				((Xsd.SailingForPlannedLegs)leg.Item).VesselName = VesselNameImportHelper.MatchVesselAndGetVesselName((Xsd.SailingForPlannedLegs)leg.Item, context.Factory);
			}));

			List<Transport> legs = new List<Transport>(transports.Count);
			List<Xsd.PlannedLeg> emptyLegs = new List<Xsd.PlannedLeg>();

			foreach (Transport transport in transports)
			{
				legs.Add(transport);
			}

			if (context.ImportingJob == null
				|| (context.ImportingJob != null && !(context.ImportingJob is IQuotedBooking)))
			{
				foreach (Xsd.PlannedLeg plannedLeg in plannedLegs)
				{
					ZString load = GetPortCode(plannedLeg.PortOfLoading, context);
					ZString disc = GetPortCode(plannedLeg.PortOfDischarge, context);

					if (load.IsEmpty && disc.IsEmpty)
					{
						emptyLegs.Add(plannedLeg);
					}
					else
					{
						Transport matchingTransport = FindLeg(legs, load, disc);

						if (matchingTransport != null)
						{
							legs.Remove(matchingTransport);
						}
						else
						{
							matchingTransport = transports.AddNew();
							matchingTransport.JW_IsLinked = true;
						}

						ImportPlannedLeg(matchingTransport, plannedLeg, context, errorContext);
					}
				}
			}

			foreach (Xsd.PlannedLeg emptyLeg in emptyLegs)
			{
				Transport matchingTransport;

				if (legs.Count > 0)
				{
					matchingTransport = legs[legs.Count - 1];
					legs.RemoveAt(legs.Count - 1);
				}
				else
				{
					matchingTransport = transports.AddNew();
					matchingTransport.JW_IsLinked = true;
				}

				ImportPlannedLeg(matchingTransport, emptyLeg, context, errorContext);
			}

			foreach (Transport transport in legs)
			{
				transport.Delete();
			}

			RecalculateLegOrderIfNeeded(transports);
		}

		static void ImportPlannedLeg(Transport transport, Xsd.PlannedLeg plannedLegValue, IValueObjectImportContext context, string errorContext)
		{
			if (plannedLegValue.LegOrderNumberSpecified)
			{
				transport.JW_LegOrder = plannedLegValue.LegOrderNumber;
			}

			if (transport.JW_IsLinked)
			{
				ImportLinkedPlannedLeg(transport, plannedLegValue, context, errorContext);
			}
			else
			{
				ImportUnlinkedPlannedLeg(transport, plannedLegValue, context, errorContext);
			}
		}

		static void ImportLinkedPlannedLeg(Transport transport, Xsd.PlannedLeg plannedLegValue, IValueObjectImportContext context, string errorContext)
		{
			ZString transportMode = TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(plannedLegValue.TransportMode.ToString(), errorContext, context);
			ZString vesselName = ZString.Empty;
			ZString voyageFlight = ZString.Empty;
			ZString portOfLoading;
			ZString portOfDischarge;
			ZDateTime eTD = ZDateTime.Empty;
			ZDateTime eTA = ZDateTime.Empty;
			ZGuid carrierPK = ZGuid.Empty;

			if (plannedLegValue.Item is Xsd.SailingForPlannedLegs)
			{
				Xsd.SailingForPlannedLegs sailingValue = (Xsd.SailingForPlannedLegs)plannedLegValue.Item;
				vesselName = sailingValue.VesselName;
				voyageFlight = sailingValue.VoyageNo;

				if (sailingValue.Carrier.IsSpecified)
				{
					carrierPK = context.FindOrCreateTempOrganisationPK(sailingValue.Carrier, transport, OrganisationTypes.Carrier);
				}
			}
			else if (plannedLegValue.Item is Xsd.FlightWithFlightNumber)
			{
				Xsd.FlightWithFlightNumber flightValue = (Xsd.FlightWithFlightNumber)plannedLegValue.Item;
				vesselName = "";
				voyageFlight = flightValue.FlightNoJourneyNoTruckRegNo;
			}
			else if (plannedLegValue.Item is Xsd.SailingWithVesselVoyage)
			{
				Xsd.SailingWithVesselVoyage sailingValue = (Xsd.SailingWithVesselVoyage)plannedLegValue.Item;
				vesselName = sailingValue.VesselName;
				voyageFlight = sailingValue.VoyageNo;

				if (sailingValue.Carrier.IsSpecified)
				{
					carrierPK = context.FindOrCreateTempOrganisationPK(sailingValue.Carrier, transport, OrganisationTypes.Carrier);
				}
			}

			if (vesselName.Length > JobConsolTransportSchema.JW_Vessel.MaxLength)
			{
				vesselName = vesselName.Trim().Substring(0, JobConsolTransportSchema.JW_Vessel.MaxLength);
			}
			if (voyageFlight.Length > JobConsolTransportSchema.JW_VoyageFlight.MaxLength)
			{
				voyageFlight = voyageFlight.Trim().Substring(0, JobConsolTransportSchema.JW_VoyageFlight.MaxLength);
			}

			if (plannedLegValue.PortOfLoading != null && plannedLegValue.PortOfLoading.IsSpecified)
			{
				portOfLoading = GetPortCode(plannedLegValue.PortOfLoading, context);
				eTD = plannedLegValue.PortOfLoading.EstimatedDateTime;
			}
			else
			{
				portOfLoading = transport.JW_RL_NKLoadPort;
				eTD = transport.JW_ETD;
			}

			if (plannedLegValue.PortOfDischarge != null && plannedLegValue.PortOfDischarge.IsSpecified)
			{
				portOfDischarge = GetPortCode(plannedLegValue.PortOfDischarge, context);
				eTA = plannedLegValue.PortOfDischarge.EstimatedDateTime;
			}
			else
			{
				portOfDischarge = transport.JW_RL_NKDiscPort;
				eTA = transport.JW_ETA;
			}

			var locator = new SailingLocator(transport.Factory, true);
			var sailing = locator.FindOrCreateSailingFromSailingManager(
				transportMode,
				portOfLoading,
				portOfDischarge,
				vesselName,
				voyageFlight,
				carrierPK,
				eTD,
				eTA)
				.Sailing;

			if (transportMode == Constants.TransportModes.Sea
				&& sailing != null
				&& sailing.Voyage != null
				&& !sailing.Voyage.IsInDatabase
				&& sailing.Voyage.JV_OH_Line.IsEmpty)
			{
				// Should not create and save SEA voyages without carrier during dataimport
				sailing.Voyage.Delete();
				sailing = null;
			}

			if (sailing == null)
			{
				transport.JW_IsLinked = false;
				ImportUnlinkedPlannedLeg(transport, plannedLegValue, context, errorContext);
			}
			else
			{
				transport.JW_IsLinked = true;
				transport.JW_TransportMode = transportMode;

				if (plannedLegValue.TransportTypeSpecified)
				{
					context.SetPropertyInfoValue(transport.JW_TransportTypeInfo, TransportPlanningTypeXmlCodeMappings.Instance.GetEnterpriseCode(plannedLegValue.TransportType.ToString(), errorContext, context), plannedLegValue.TransportTypeSpecified);
				}

				transport.JW_JX = sailing.PK;

				SailingValueObjectDataAdapter sailingAdapter = new SailingValueObjectDataAdapter(
					transportMode,
					portOfLoading,
					portOfDischarge,
					vesselName,
					voyageFlight,
					carrierPK);

				sailingAdapter.AllowUpdates = Env.CurrentUser.IsBatchProcessor ?
					SystemDataRegistry.Instance.UpdateSailingSchedulesDuringAutomaticImport.Value :
					sailingAdapter.ConfirmUpdateOfExistingJobSailing(sailing, context);

				sailingAdapter.ImportFromValueObject(sailing, plannedLegValue.Item, context);

				if (sailingAdapter.AllowUpdates || !sailing.Origin.IsInDatabase)
				{
					XsdMovement.ToPortEstimatedActualDates(
						plannedLegValue.PortOfLoading,
						transport.JW_RL_NKLoadPortInfo,
						transport.JW_ETDInfo,
						transport.JW_ATDInfo,
						Res.GetString("5a2b201f-66f2-4b09-b7bd-0556a45a8b84", "{0}; Port of loading", errorContext),
						context
						);
				}

				if (sailingAdapter.AllowUpdates || !sailing.Destination.IsInDatabase)
				{
					XsdMovement.ToPortEstimatedActualDates(
						plannedLegValue.PortOfDischarge,
						transport.JW_RL_NKDiscPortInfo,
						transport.JW_ETAInfo,
						transport.JW_ATAInfo,
						Res.GetString("a866a586-4c64-4201-a588-d8cf6ea883bd", "{0}; Port of discharge", errorContext),
						context
						);
				}

				if (transport.IsSea)
				{
					transport.JW_IsLinked = RefVessel.LookupVesselByFK(transport, JobConsolTransportSchema.JW_Vessel) != null;
				}
			}
		}

		static void ImportUnlinkedPlannedLeg(Transport transport, Xsd.PlannedLeg plannedLegValue, IValueObjectImportContext context, string errorContext)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(transport.JW_TransportModeInfo, TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(plannedLegValue.TransportMode.ToString(), errorContext, context));
			if (plannedLegValue.TransportTypeSpecified)
			{
				context.SetPropertyInfoValue(transport.JW_TransportTypeInfo, TransportPlanningTypeXmlCodeMappings.Instance.GetEnterpriseCode(plannedLegValue.TransportType.ToString(), errorContext, context), plannedLegValue.TransportTypeSpecified);
			}
			XsdMovement.ToPortEstimatedActualDates(plannedLegValue.PortOfLoading, transport.JW_RL_NKLoadPortInfo, transport.JW_ETDInfo, transport.JW_ATDInfo, Res.GetString("a5485238-73ad-4abc-ac84-a85afd4a77f7", "Port of loading"), context);
			XsdMovement.ToPortEstimatedActualDates(plannedLegValue.PortOfDischarge, transport.JW_RL_NKDiscPortInfo, transport.JW_ETAInfo, transport.JW_ATAInfo, Res.GetString("b37441e4-3e53-4ee0-a510-215cd59c238b", "Port of discharge"), context);

			Xsd.SailingForPlannedLegs plannedVessel = plannedLegValue.Item as Xsd.SailingForPlannedLegs;
			Xsd.SailingWithVesselVoyage sailingVessel = plannedLegValue.Item as Xsd.SailingWithVesselVoyage;
			Xsd.FlightWithFlightNumber plannedRoadRailFlight = plannedLegValue.Item as Xsd.FlightWithFlightNumber;
			if (plannedVessel != null)
			{
				context.SetPropertyInfoValue(transport.JW_VoyageFlightInfo, plannedVessel.VoyageNo, plannedVessel.VoyageNoSpecified);
				context.SetPropertyInfoValue(transport.JW_VesselInfo, plannedVessel.VesselName, plannedVessel.VesselNameSpecified);
			}
			if (sailingVessel != null)
			{
				context.SetPropertyInfoValue(transport.JW_VoyageFlightInfo, sailingVessel.VoyageNo, sailingVessel.VoyageNoSpecified);
				context.SetPropertyInfoValue(transport.JW_VesselInfo, sailingVessel.VesselName, sailingVessel.VesselNameSpecified);
			}
			else if (plannedRoadRailFlight != null)
			{
				context.SetPropertyInfoValue(transport.JW_VoyageFlightInfo, plannedRoadRailFlight.FlightNoJourneyNoTruckRegNo, plannedRoadRailFlight.FlightNoJourneyNoTruckRegNoSpecified);
			}
		}

		#endregion

		#region Export

		public static void ExportPlannedLeg(Xsd.PlannedLeg plannedLegValue, Transport transport, IValueObjectExportContext context, string errorContext)
		{
			plannedLegValue.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(transport.JW_TransportMode, errorContext, context);
			if (!transport.JW_TransportType.IsEmpty)
			{
				plannedLegValue.TransportType = TransportPlanningTypeXmlCodeMappings.Instance.GetExternalCode(transport.JW_TransportType, errorContext, context);
				plannedLegValue.TransportTypeSpecified = true;
			}
			else
			{
				plannedLegValue.TransportTypeSpecified = false;
			}

			plannedLegValue.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(transport.Factory, transport.JW_RL_NKLoadPort, transport.JW_ETD, transport.JW_ATD);
			plannedLegValue.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(transport.Factory, transport.JW_RL_NKDiscPort, transport.JW_ETA, transport.JW_ATA);
			plannedLegValue.Item = ExportPlannedLegSailingDetail(transport, context);

			if (!transport.JW_LegOrder.IsEmpty && !transport.JW_LegOrder.IsDefault && transport.JW_LegOrder.IsValid)
			{
				plannedLegValue.LegOrderNumber = transport.JW_LegOrder;
			}
		}

		static Xsd.SailingBase ExportPlannedLegSailingDetail(Transport transport, IValueObjectExportContext context)
		{
			JobSailing sailing = (transport.JW_IsLinked ? transport.Factory.Load<JobSailing>(transport.JW_JX) : null);
			Xsd.SailingBase result;

			if (sailing == null)
			{
				result = ExportUnlinkedPlannedLegSailingDetail(transport);
			}
			else
			{
				result = ExportLinkedPlannedLegSailingDetail(sailing, context);
			}

			return result;
		}

		static Xsd.SailingBase ExportLinkedPlannedLegSailingDetail(JobSailing sailing, IValueObjectExportContext context)
		{
			Xsd.SailingBase sailingValue = null;

			if (sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea)
			{
				Xsd.SailingForPlannedLegs vesselDetails = new Xsd.SailingForPlannedLegs();
				sailingValue = vesselDetails;

				vesselDetails.LCLDates.IsSpecified = false;
				vesselDetails.FCLDates.IsSpecified = false;
				vesselDetails.VesselName = (sailing.Voyage.JV_RV_NKVessel.IsEmpty) ? null : (string)sailing.Voyage.JV_RV_NKVessel;
				vesselDetails.LloydsNo = (sailing.Vessel == null) ? null : sailing.Vessel.RV_LloydsNumber;
				vesselDetails.CargoCarrierCode = (sailing.Vessel == null) ? ZString.Empty : sailing.Vessel.RV_CarrierCode;
				vesselDetails.VoyageNo = (sailing.JX_JV_VoyageFlight.IsEmpty) ? null : (string)sailing.JX_JV_VoyageFlight;

				if (sailing.Voyage != null && sailing.Voyage.Line != null)
				{
					vesselDetails.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(sailing.Voyage.Line, context);
				}
			}
			else
			{
				Xsd.FlightWithFlightNumber flightDetails = new Xsd.FlightWithFlightNumber();
				sailingValue = flightDetails;

				flightDetails.Dates = null;
				flightDetails.FlightNoJourneyNoTruckRegNo = (sailing.JX_JV_VoyageFlight.IsEmpty) ? null : (string)sailing.JX_JV_VoyageFlight;
			}

			SailingValueObjectDataAdapter.RunExport(sailing, sailingValue, context);

			return sailingValue;
		}

		static Xsd.SailingBase ExportUnlinkedPlannedLegSailingDetail(Transport transport)
		{
			Xsd.SailingBase result;

			switch (transport.JW_TransportMode)
			{
				case Core.Constants.TransportModes.Sea:
					Xsd.SailingForPlannedLegs vessel = new Xsd.SailingForPlannedLegs();
					vessel.VesselName = transport.JW_Vessel;
					vessel.LloydsNo = (transport.Vessel == null) ? "" : (string)transport.Vessel.RV_LloydsNumber;
					vessel.CargoCarrierCode = (transport.Vessel == null) ? "" : (string)transport.Vessel.RV_CarrierCode;
					vessel.VoyageNo = transport.JW_VoyageFlight;

					result = vessel;
					break;

				case Core.Constants.TransportModes.Storage:
					result = new Xsd.Storage();
					break;

				default:
					Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
					flight.FlightNoJourneyNoTruckRegNo = transport.JW_VoyageFlight;
					result = flight;
					break;
			}

			result.ETD = transport.JW_ETD;
			result.ETA = transport.JW_ETA;
			result.ATD = transport.JW_ATD;
			result.ATA = transport.JW_ATA;

			return result;
		}

		#endregion

		public static ZString GetPortCode(Xsd.Movement movement, IValueObjectImportContext context)
		{
			ZString result = "";

			if (movement != null && movement.Port != null)
			{
				result = context.ConvertRawStringToZTypeValue<ZString>(movement.Port.Value, ForeignKeyType.PortNK);
			}

			return result.SubstringSafe(0, 5);
		}

		static Transport FindLeg(IEnumerable<Transport> legs, ZString load, ZString discharge)
		{
			Transport loadLeg = null;
			Transport dischargeLeg = null;

			foreach (Transport leg in legs)
			{
				if (StrongMatch(load, leg.JW_RL_NKLoadPort) && StrongMatch(discharge, leg.JW_RL_NKDiscPort))
				{
					return leg;
				}
				else if (StrongMatch(load, leg.JW_RL_NKLoadPort) && WeakMatch(discharge, leg.JW_RL_NKDiscPort))
				{
					loadLeg = leg;
				}
				else if (WeakMatch(load, leg.JW_RL_NKLoadPort) && StrongMatch(discharge, leg.JW_RL_NKDiscPort))
				{
					dischargeLeg = leg;
				}
			}

			return loadLeg ?? dischargeLeg;
		}

		static bool StrongMatch(ZString value1, ZString value2)
		{
			return !value1.IsEmpty && value1 == value2;
		}
		static bool WeakMatch(ZString value1, ZString value2)
		{
			return value1.IsEmpty || value2.IsEmpty || value1 == value2;
		}

		static void RecalculateLegOrderIfNeeded(TransportCollection transports)
		{
			bool[] legOrderFlags = new bool[transports.Count];

			foreach (Transport transport in transports)
			{
				if (transport.JW_LegOrder.IsEmpty
					|| transport.JW_LegOrder > legOrderFlags.Length
					|| legOrderFlags[transport.JW_LegOrder - 1])
				{
					RecalculateLegOrder(transports);
					break;
				}

				legOrderFlags[transport.JW_LegOrder - 1] = true;
			}
		}

		static void RecalculateLegOrder(TransportCollection transports)
		{
			Transport[] list = (Transport[])transports.ToArray(typeof(Transport));
			MovementLegComparer.SortMovementLegsByPorts(list);

			byte legNumber = 1;
			foreach (Transport transport in list)
			{
				transport.JW_LegOrder = legNumber++;
			}
		}
	}
}
