using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ScheduleTransportLegDataObjectReader : DataObjectReader<TransportLeg, JobSailing>
	{
		public ScheduleTransportLegDataObjectReader(TransportLeg transportLegDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, JobVoyage voyage)
			: base(transportLegDataObject, logger, factory)
		{
			this.voyage = Argument.NotNull(voyage, nameof(voyage));
		}

		readonly JobVoyage voyage;

		#region Implementation

		protected override JobSailing GetExistingBusinessObject()
		{
			return new ScheduleTransportLegBusinessObjectFinder(dataObject, voyage).Find(voyage.Sailings.Cast<JobSailing>());
		}

		protected override void PopulateBusinessObject(JobSailing sailingBO)
		{
			if (dataObject.TransportMode.HasValue)
			{
				SetValue(voyage, JobVoyageSchema.JV_AirSeaRoad, new TransportModeConverter().FromEnumValue(dataObject.TransportMode));
			}

			if (dataObject.PortOfLoading == null || dataObject.PortOfDischarge == null)
			{
				var message = Res.GetString("343FA6A0-4DE5-4338-A6F1-B1F7A5663514", "Transport leg is missing Port Of Loading or Port Of Discharge.");
				throw new DataObjectReadFailureException(message);
			}

			if (dataObject.PortOfLoading.Code == dataObject.PortOfDischarge.Code && (voyage.IsAir || voyage.IsSea))
			{
				var message = Res.GetString("3419b264-45ae-4a50-89b5-d4a5a4a32f09", "Transport leg cannot have the same Loading & Discharge port when the mode is not ROA or RAI.");
				throw new DataObjectReadFailureException(message);
			}

			SetValue(voyage, JobVoyageSchema.JV_RV_NKVessel, dataObject.VesselName);
			SetValue(voyage, JobVoyageSchema.JV_VoyageFlight, dataObject.VoyageFlightNo);

			if (dataObject.VesselLloydsIMO.HasValue && voyage.Vessel != null)
			{
				SetValue(voyage.Vessel, RefVesselSchema.RV_LloydsNumber, dataObject.VesselLloydsIMO);
			}

			if (dataObject.Carrier != null)
			{
				var carrierAddress = new TransportLegCarrierDataObjectReader(dataObject.Carrier, logger, factory).GetMatched();
				if (carrierAddress != null)
				{
					SetValue(voyage, JobVoyageSchema.JV_OH_Line, carrierAddress.OA_OH);
				}
			}

			if (voyage.JV_AirSeaRoad == TransportModes.Air)
			{
				if (dataObject.IsCargoOnly.HasValue)
				{
					SetValue(voyage, JobVoyageSchema.JV_IsCargoOnly, dataObject.IsCargoOnly);
				}

				if (dataObject.AircraftType != null && dataObject.AircraftType.Code.HasValue)
				{
					SetValue(voyage, JobVoyageSchema.JV_AircraftType, dataObject.AircraftType);
				}
			}

			var origin = PopulateOrigin(sailingBO);
			var destination = PopulateDestination(sailingBO);
			if (JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, destination, voyage.JV_AirSeaRoad))
			{
				var message = Res.GetString("d067e7ce-76a2-4b28-b7ef-6c23605a8db4", "Invalid Estimated Departure {0} / {1} and Estimated Arrival {2} / {3} for Transport Mode {4}.", origin.JA_E_DEP, origin.JA_E_DEP_UTC, destination.JB_E_ARV, destination.JB_E_ARV_UTC, voyage.JV_AirSeaRoad);
				throw new DataObjectReadFailureException(message);
			}

			PopulateSailingLCLDates(sailingBO);
		}

		VoyageOrigin PopulateOrigin(JobSailing sailingBO)
		{
			if (sailingBO.Origin == null && dataObject.PortOfLoading != null && dataObject.PortOfLoading.Code.HasValue)
			{
				var voyageOrigin = voyage.Origins.Cast<VoyageOrigin>().FirstOrDefault(o => o.JA_RL_NKPortOfLoading == dataObject.PortOfLoading.Code.Value);
				if (voyageOrigin == null)
				{
					voyageOrigin = factory.New<VoyageOrigin>();
					using (voyage.Origins.SuppressSailingGeneration())
					{
						voyage.Origins.Add(voyageOrigin);
					}
				}

				sailingBO.JX_JA = voyageOrigin.PK;
			}

			var origin = sailingBO.Origin;

			if (origin == null)
			{
				return null;
			}

			SetValue(origin, JobVoyOriginSchema.JA_RL_NKPortOfLoading, dataObject.PortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_E_DEP, dataObject.EstimatedDeparture);
			SetValue(origin, JobVoyOriginSchema.JA_A_DEP, dataObject.ActualDeparture);
			SetValue(origin, JobVoyOriginSchema.JA_ReceivalCommences, dataObject.FCLReceivalCommences);
			SetValue(origin, JobVoyOriginSchema.JA_CutOff, dataObject.FCLCutOff);

			if (dataObject.DepartureCTO != null)
			{
				var ctoAddress = new OrganisationDataObjectReader(dataObject.DepartureCTO, logger, factory).GetMatched();
				if (ctoAddress != null)
				{
					SetValue(origin, JobVoyOriginSchema.JA_OA_DepartureCTOAddress, ctoAddress.PK);
				}
			}

			SetValue(origin, JobVoyOriginSchema.JA_Berth, dataObject.DepartureBerth);
			SetValue(origin, JobVoyOriginSchema.JA_DepartReference, dataObject.DepartureReference);
			SetValue(origin, JobVoyOriginSchema.JA_E_ARV, dataObject.EstimatedArrivalInPortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_A_ARV, dataObject.ActualArrivalInPortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_DocumentaryCutoff, dataObject.DocumentCutOff);
			SetValue(origin, JobVoyOriginSchema.JA_DGReceivalCommences, dataObject.HazzardReceivalCommences);
			SetValue(origin, JobVoyOriginSchema.JA_DGCutOff, dataObject.HazzardCutOffDate);
			SetValue(origin, JobVoyOriginSchema.JA_VGMCutOff, dataObject.VGMCutOff);

			SetValue(origin, JobVoyOriginSchema.JA_EmptyReceivalCommences, dataObject.EmptyReceivalCommences);
			SetValue(origin, JobVoyOriginSchema.JA_EmptyCutOff, dataObject.EmptyCutOff);
			SetValue(origin, JobVoyOriginSchema.JA_ReeferReceivalCommences, dataObject.ReeferReceivalCommences);
			SetValue(origin, JobVoyOriginSchema.JA_ReeferCutOff, dataObject.ReeferCutOff);
			SetValue(origin, JobVoyOriginSchema.JA_S_DEP, dataObject.ScheduledDeparture);
			SetValue(origin, JobVoyOriginSchema.JA_S_ARV, dataObject.ScheduledArrivalInPortOfLoading);

			return origin;
		}

		VoyageDestination PopulateDestination(JobSailing sailingBO)
		{
			if (sailingBO.Destination == null && dataObject.PortOfDischarge != null && dataObject.PortOfDischarge.Code.HasValue)
			{
				var voyageDestination = voyage.Destinations.Cast<VoyageDestination>().FirstOrDefault(d => d.JB_RL_NKPortOfDischarge == dataObject.PortOfDischarge.Code.Value);
				if (voyageDestination == null)
				{
					voyageDestination = factory.New<VoyageDestination>();
					using (voyage.Destinations.SuppressSailingGeneration())
					{
						voyage.Destinations.Add(voyageDestination);
					}
				}

				sailingBO.JX_JB = voyageDestination.PK;
			}

			var destination = sailingBO.Destination;

			if (destination == null)
			{
				return null;
			}

			SetValue(destination, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, dataObject.PortOfDischarge);
			SetValue(destination, JobVoyDestinationSchema.JB_E_ARV, dataObject.EstimatedArrival);
			SetValue(destination, JobVoyDestinationSchema.JB_A_ARV, dataObject.ActualArrival);
			SetValue(destination, JobVoyDestinationSchema.JB_AvailabilityDate, dataObject.FCLAvailability);
			SetValue(destination, JobVoyDestinationSchema.JB_StorageDate, dataObject.FCLStorage);

			if (dataObject.ArrivalCTO != null)
			{
				var ctoAddress = new OrganisationDataObjectReader(dataObject.ArrivalCTO, logger, factory).GetMatched();
				if (ctoAddress != null)
				{
					SetValue(destination, JobVoyDestinationSchema.JB_OA_ArrivalCTOAddress, ctoAddress.PK);
				}
			}

			SetValue(destination, JobVoyDestinationSchema.JB_Berth, dataObject.ArrivalBerth);
			SetValue(destination, JobVoyDestinationSchema.JB_ArrivalReference, dataObject.ArrivalReference);

			SetValue(destination, JobVoyDestinationSchema.JB_S_ARV, dataObject.ScheduledArrival);

			return destination;
		}

		void PopulateSailingLCLDates(JobSailing sailingBO)
		{
			SetValue(sailingBO, JobSailingSchema.JX_DepotReceivalCommences, dataObject.LCLReceivalCommences);
			SetValue(sailingBO, JobSailingSchema.JX_DepotCutOff, dataObject.LCLCutOff);
			SetValue(sailingBO, JobSailingSchema.JX_DepotAvailabilityDate, dataObject.LCLAvailability);
			SetValue(sailingBO, JobSailingSchema.JX_DepotStorageDate, dataObject.LCLStorageDate);
		}

		#endregion
	}
}
