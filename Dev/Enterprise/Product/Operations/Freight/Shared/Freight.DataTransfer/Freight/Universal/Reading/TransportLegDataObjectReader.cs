using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegDataObjectReader : DataObjectReader<TransportLeg, Transport>
	{
		public TransportLegDataObjectReader(TransportLeg transportLegDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ITransportParentCommon transportParent, Func<TransportLeg, Transport> transportLegBizObjProvider = null, bool allowUpdateSailing = true)
			: base(transportLegDataObject, logger, factory)
		{
			this.TransportParent = Argument.NotNull(transportParent, "transportParent");
			this.TransportLegBizObjProvider = transportLegBizObjProvider;
			this.allowUpdateSailing = allowUpdateSailing;
		}

		readonly ITransportParentCommon TransportParent;
		readonly Func<TransportLeg, Transport> TransportLegBizObjProvider;
		readonly bool allowUpdateSailing;

		TransportCollection Transports
		{
			get
			{
				TransportCollection transports = null;
				var parent = TransportParent as ITransportParent;
				if (parent != null)
				{
					transports = parent.Transports;
				}
				else
				{
					transports = new TransportCollection(TransportParent);
					transports.Load();
				}
				return transports;
			}
		}

		bool HasSufficientInformationToFindSailingSchedule
		{
			get
			{
				switch (dataObject.TransportMode)
				{
					case TransportMode.Sea:
					case TransportMode.Rail:
						{
							if (dataObject.PortOfLoading != null &&
								dataObject.PortOfLoading.Code.GetValueOrDefault() != ZString.Empty &&
								dataObject.PortOfDischarge != null &&
								dataObject.PortOfDischarge.Code.GetValueOrDefault() != ZString.Empty &&
								dataObject.VesselName.GetValueOrDefault() != ZString.Empty &&
								dataObject.VoyageFlightNo.GetValueOrDefault() != ZString.Empty)
							{
								return true;
							}

							break;
						}

					case TransportMode.Air:
					case TransportMode.Road:
						{
							if (dataObject.PortOfLoading != null &&
								dataObject.PortOfLoading.Code.GetValueOrDefault() != ZString.Empty &&
								dataObject.PortOfDischarge != null &&
								dataObject.PortOfDischarge.Code.GetValueOrDefault() != ZString.Empty &&
								(dataObject.EstimatedArrival.HasValue || dataObject.EstimatedDeparture.HasValue))
							{
								return true;
							}

							break;
						}
				}

				return false;
			}
		}

		bool HasValidDateCombination
		{
			get
			{
				var estimatedDepartureUTC = dataObject.EstimatedDeparture.GetValueOrDefault().IsValid && dataObject.PortOfLoading != null && !string.IsNullOrEmpty(dataObject.PortOfLoading.Code)
					? Env.Time.GetUtcFromUnlocoTime(dataObject.PortOfLoading.Code, dataObject.EstimatedDeparture.GetValueOrDefault().ToDateTime()) : ZDateTime.Empty;
				var estimatedArrivalUTC = dataObject.EstimatedArrival.GetValueOrDefault().IsValid && dataObject.PortOfDischarge != null && !string.IsNullOrEmpty(dataObject.PortOfDischarge.Code)
					? Env.Time.GetUtcFromUnlocoTime(dataObject.PortOfDischarge.Code, dataObject.EstimatedArrival.GetValueOrDefault().ToDateTime()) : ZDateTime.Empty;
				return !JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(dataObject.EstimatedDeparture.GetValueOrDefault(), dataObject.EstimatedArrival.GetValueOrDefault(), estimatedDepartureUTC, estimatedArrivalUTC, dataObject.TransportMode);
			}
		}

		#region Implementation

		protected override Transport GetExistingBusinessObject()
		{
			return TransportLegBizObjProvider != null ? TransportLegBizObjProvider(dataObject) : GetExistingBusinessObjectFromParent(dataObject);
		}

		Transport GetExistingBusinessObjectFromParent(TransportLeg transportLegDataObject)
		{
			var finder = new TransportLegBusinessObjectFinder(transportLegDataObject, TransportParent);
			return finder.Find(Transports.Cast<Transport>());
		}

		protected override void PopulateBusinessObject(Transport transportBO)
		{
			logger.Log(LogType.Information, FormattableString.Invariant($@"Transport Leg: Origin: {dataObject.PortOfLoading?.Code} Destination: {dataObject.PortOfDischarge?.Code}"));

			var vesselName = transportBO.JW_Vessel;

			transportBO.ParentType = TransportParent.GetType();
			SetValue(transportBO, JobConsolTransportSchema.JW_ParentGUID, TransportParent.PK);
			SetValue(transportBO, JobConsolTransportSchema.JW_LegOrder, dataObject.LegOrder);

			var shouldUpdateTransportLeg = true;
			if (!transportBO.IsInDatabase || transportBO.JW_IsLinked)
			{
				logger.Log(LogType.Information, "Attempting to get Schedule for the Transport Leg");

				if (dataObject.TransportMode == TransportMode.InlandWaterway)
				{
					logger.Log(LogType.Information, "No schedule for Inland Waterway Transport. The Transport Leg will not be linked.");
					transportBO.JW_IsLinked = false;
				}
				else if (!HasValidDateCombination)
				{
					logger.Log(LogType.Information, "Invalid date combination. The Transport Leg will not be linked.");
					transportBO.JW_IsLinked = false;
				}
				else if (!HasSufficientInformationToFindSailingSchedule)
				{
					logger.Log(LogType.Information, "Unable to link to schedule because the Transport Leg has insufficient information.");
					transportBO.JW_IsLinked = false;
				}
				else
				{
					var scheduleFinder = new TransportLegSailingScheduleFinder(factory, dataObject, logger, transportBO, TransportParent as ITransportParent);
					var sailingManager = scheduleFinder.GetTransportSailingFromSailingManager();
					var sailing = sailingManager.Sailing;

					if (sailing == null)
					{
						logger.Log(LogType.Information, "An existing Schedule could not be found. The Transport Leg will not be linked.");
						sailingManager.LogReasonForSailingNotGenerated(logger);
						transportBO.JW_IsLinked = false;
					}
					else if (IsArchivedSailingSchedule(sailing))
					{
						logger.Log(LogType.Information, "The Schedule found is archived. Discarding.");
						if (SailingDataHasChanged(sailing))
						{
							transportBO.JW_IsLinked = false;
						}
						else
						{
							shouldUpdateTransportLeg = false;
						}
					}
					else
					{
						logger.Log(LogType.Information, "A Schedule has been found and linked to the Transport Leg.");
						transportBO.JW_IsLinked = true;
						transportBO.JW_JX = sailing.PK;
					}
				}
			}

			if (shouldUpdateTransportLeg)
			{
				ImportOriginDetails(transportBO, AllowUpdateSailing);
				ImportDestinationDetails(transportBO, AllowUpdateSailing);

				SetValue(transportBO, JobConsolTransportSchema.JW_TransportMode, new TransportModeConverter().FromEnumValue(dataObject.TransportMode));

				if (dataObject.LegType.HasValue)
				{
					SetValue(transportBO, JobConsolTransportSchema.JW_TransportType, new LegTypeConverter().FromEnumValue(dataObject.LegType));
				}

				SetValue(transportBO, JobConsolTransportSchema.JW_LegNotes, dataObject.LegNotes);

				if (transportBO.JW_TransportMode != Core.Constants.TransportModes.Air)
				{
					var vesselNameFromDataObject = dataObject.GetVesselName(transportBO.Factory);
					if (vesselNameFromDataObject.IsEmpty)
					{
						SetValue(transportBO, JobConsolTransportSchema.JW_Vessel, vesselName);
					}
					else
					{
						transportBO.JW_Vessel = vesselNameFromDataObject;
					}
				}

				SetValue(transportBO, JobConsolTransportSchema.JW_VoyageFlight, dataObject.VoyageFlightNo);
				SetValue(transportBO, JobConsolTransportSchema.JW_AircraftType, dataObject.AircraftType);
				SetValue(transportBO, JobConsolTransportSchema.JW_CarrierBookingReference, dataObject.CarrierBookingReference);
				SetValue(transportBO, JobConsolTransportSchema.JW_PL_NKCarrierServiceLevel, dataObject.CarrierServiceLevel);

				if (dataObject.IsCargoOnly.HasValue)
				{
					transportBO.JW_IsCargoOnly = dataObject.IsCargoOnly.Value;
				}

				if (!(transportBO.IsSea && transportBO.JW_IsLinked))
				{
					if (dataObject.Carrier != null)
					{
						var carrierOrgAddress = new TransportLegCarrierDataObjectReader(dataObject.Carrier, logger, factory).GetMatched();
						if (carrierOrgAddress != null)
						{
							transportBO[JobConsolTransportSchema.JW_OA_CarrierAddress] = carrierOrgAddress.PK;
						}
					}
				}
				SetValue(transportBO, JobConsolTransportSchema.JW_OA_CreditorAddress, dataObject.Creditor);

				SetValue(transportBO, JobConsolTransportSchema.JW_Status, dataObject.BookingStatus);

				PopulateAddtionalTransportModes(transportBO);

				logger.Log(LogType.Information, "Transport Leg updated.");
			}
		}

		void PopulateAddtionalTransportModes(Transport transportBO)
		{
			var additionalTransportModes = dataObject.AdditionalTransportModeCollection;
			if (additionalTransportModes != null && additionalTransportModes.Count > 0)
			{
				if (additionalTransportModes.Count > 1)
				{
					throw new DataObjectReadFailureException(Res.GetString("6abed9a1-6194-4bea-9850-f4fb9148da5d", "Multiple additional transport modes are found."));
				}

				var additionalTransportMode = new TransportModeConverter().FromEnumValue(additionalTransportModes[0].TransportMode);
				if (additionalTransportModes[0].TransportMode != TransportMode.Road
					&& additionalTransportModes[0].TransportMode != TransportMode.Rail)
				{
					logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Invalid additional transport mode {0} is found.", additionalTransportMode));
					return;
				}

				if ((dataObject.TransportMode == TransportMode.Rail && additionalTransportModes[0].TransportMode != TransportMode.Road)
					|| (dataObject.TransportMode != TransportMode.Rail
						&& dataObject.TransportMode != TransportMode.InlandWaterway))
				{
					logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Additional transport mode {0} is not applicable for {1} transport mode.", additionalTransportMode, transportBO.JW_TransportMode));
					return;
				}

				SetValue(transportBO, JobConsolTransportSchema.JW_AdditionalTransportMode, additionalTransportMode);
			}
		}

		void ImportOriginDetails(Transport transportBO, bool allowUpdates)
		{
			if (!transportBO.JW_IsLinked
				|| (transportBO.JW_IsLinked && allowUpdates)
				|| (transportBO.Sailing != null && !transportBO.Sailing.IsInDatabase))
			{
				SetValue(transportBO, JobConsolTransportSchema.JW_RL_NKLoadPort, dataObject.PortOfLoading);
				SetValue(transportBO, JobConsolTransportSchema.JW_ETD, dataObject.EstimatedDeparture);
				SetValue(transportBO, JobConsolTransportSchema.JW_ATD, dataObject.ActualDeparture);

				SetValue(transportBO, JobConsolTransportSchema.JW_DocumentaryCutOff, dataObject.DocumentCutOff);
				SetValue(transportBO, JobConsolTransportSchema.JW_TerminalCutOff, dataObject.FCLCutOff);
				SetValue(transportBO, JobConsolTransportSchema.JW_VGMCutOff, dataObject.VGMCutOff);
				SetValue(transportBO, JobConsolTransportSchema.JW_TerminalReceivalCommences, dataObject.FCLReceivalCommences);

				SetValue(transportBO, JobConsolTransportSchema.JW_DepotReceivalCommences, dataObject.LCLReceivalCommences);
				SetValue(transportBO, JobConsolTransportSchema.JW_DepotCutOff, dataObject.LCLCutOff);
				SetValue(transportBO, JobConsolTransportSchema.JW_STD, dataObject.ScheduledDeparture);

				SetValue(transportBO, JobConsolTransportSchema.JW_EmptyReceivalCommences, dataObject.EmptyReceivalCommences);
				SetValue(transportBO, JobConsolTransportSchema.JW_EmptyCutOff, dataObject.EmptyCutOff);
				SetValue(transportBO, JobConsolTransportSchema.JW_ReeferReceivalCommences, dataObject.ReeferReceivalCommences);
				SetValue(transportBO, JobConsolTransportSchema.JW_ReeferCutOff, dataObject.ReeferCutOff);
				SetValue(transportBO, JobConsolTransportSchema.JW_DGReceivalCommences, dataObject.HazzardReceivalCommences);
				SetValue(transportBO, JobConsolTransportSchema.JW_DGCutOff, dataObject.HazzardCutOffDate);

				if (dataObject.DepartureFrom != null)
				{
					var orgDataReader = new OrganisationDataObjectReader(dataObject.DepartureFrom, logger, factory);
					var address = orgDataReader.GetMatched();
					if (address != null)
					{
						transportBO.JW_OA_DepartureLocation = address.PK;
					}
				}

				if (transportBO.Sailing != null && transportBO.Sailing.Origin != null)
				{
					ImportSailingOriginDetails(transportBO.Sailing.Origin);
				}
			}
		}

		void ImportDestinationDetails(Transport transportBO, bool allowUpdates)
		{
			if (!transportBO.JW_IsLinked
				|| (transportBO.JW_IsLinked && allowUpdates)
				|| (transportBO.Sailing != null && !transportBO.Sailing.IsInDatabase))
			{
				SetValue(transportBO, JobConsolTransportSchema.JW_RL_NKDiscPort, dataObject.PortOfDischarge);
				SetValue(transportBO, JobConsolTransportSchema.JW_ETA, dataObject.EstimatedArrival);
				SetValue(transportBO, JobConsolTransportSchema.JW_ATA, dataObject.ActualArrival);
				SetValue(transportBO, JobConsolTransportSchema.JW_TerminalAvailabilityDate, dataObject.FCLAvailability);
				SetValue(transportBO, JobConsolTransportSchema.JW_TerminalStorageDate, dataObject.FCLStorage);
				SetValue(transportBO, JobConsolTransportSchema.JW_DepotAvailabilityDate, dataObject.LCLAvailability);
				SetValue(transportBO, JobConsolTransportSchema.JW_DepotStorageDate, dataObject.LCLStorageDate);
				SetValue(transportBO, JobConsolTransportSchema.JW_STA, dataObject.ScheduledArrival);

				if (dataObject.ArrivalAt != null)
				{
					var orgDataReader = new OrganisationDataObjectReader(dataObject.ArrivalAt, logger, factory);
					var address = orgDataReader.GetMatched();
					if (address != null)
					{
						transportBO.JW_OA_ArrivalLocation = address.PK;
					}
				}

				if (transportBO.Sailing?.Destination != null)
				{
					ImportSailingDestinationDetails(transportBO.Sailing.Destination);
				}
			}
		}

		void ImportSailingOriginDetails(VoyageOrigin origin)
		{
			SetValue(origin, JobVoyOriginSchema.JA_E_ARV, dataObject.EstimatedArrivalInPortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_A_ARV, dataObject.ActualArrivalInPortOfLoading);
			SetValue(origin, JobVoyOriginSchema.JA_Berth, dataObject.DepartureBerth);
			SetValue(origin, JobVoyOriginSchema.JA_DepartReference, dataObject.DepartureReference);

			if (dataObject.DepartureFrom == null && dataObject.DepartureCTO != null)
			{
				var orgDataReader = new OrganisationDataObjectReader(dataObject.DepartureCTO, logger, factory);
				var address = orgDataReader.GetMatched();
				if (address != null)
				{
					origin.JA_OA_DepartureCTOAddress = address.PK;
				}
			}
		}

		void ImportSailingDestinationDetails(VoyageDestination destination)
		{
			SetValue(destination, JobVoyDestinationSchema.JB_Berth, dataObject.ArrivalBerth);
			SetValue(destination, JobVoyDestinationSchema.JB_ArrivalReference, dataObject.ArrivalReference);

			if (dataObject.ArrivalAt == null && dataObject.ArrivalCTO != null)
			{
				var orgDataReader = new OrganisationDataObjectReader(dataObject.ArrivalCTO, logger, factory);
				var address = orgDataReader.GetMatched();
				if (address != null)
				{
					destination.JB_OA_ArrivalCTOAddress = address.PK;
				}
			}
		}

		public virtual bool IsChangeTransportType() => true;

		#region GetSailingSchedule

		bool IsArchivedSailingSchedule(JobSailing sailing)
		{
			return sailing?.Voyage != null && sailing.Voyage.IsArchived;
		}

		bool SailingDataHasChanged(JobSailing sailing)
		{
			var departureAddressPK = sailing.Origin != null ? sailing.Origin.JA_OA_DepartureCTOAddress : ZGuid.Empty;
			var arrivalAddressPK = sailing.Destination != null ? sailing.Destination.JB_OA_ArrivalCTOAddress : ZGuid.Empty;

			return sailing.JX_DepotAvailabilityDate != dataObject.LCLAvailability.GetValueOrDefault()
				|| sailing.JX_DepotCutOff != dataObject.LCLCutOff.GetValueOrDefault()
				|| sailing.JX_DepotReceivalCommences != dataObject.LCLReceivalCommences.GetValueOrDefault()
				|| sailing.JX_DepotStorageDate != dataObject.LCLStorageDate.GetValueOrDefault()
				|| sailing.JX_JA_E_DEP != dataObject.EstimatedDeparture.GetValueOrDefault()
				|| sailing.JX_JA_A_DEP != dataObject.ActualDeparture.GetValueOrDefault()
				|| sailing.JX_JB_E_ARV != dataObject.EstimatedArrival.GetValueOrDefault()
				|| sailing.JX_JB_A_ARV != dataObject.ActualArrival.GetValueOrDefault()
				|| departureAddressPK != GetOrgLocationPK(dataObject.DepartureFrom)
				|| arrivalAddressPK != GetOrgLocationPK(dataObject.ArrivalAt);
		}

		ZGuid GetOrgLocationPK(OrganizationAddress orgAddress)
		{
			var location = ZGuid.Empty;
			if (orgAddress != null)
			{
				var orgDataReader = new OrganisationDataObjectReader(orgAddress, logger, factory);
				var address = orgDataReader.GetMatched();
				if (address != null)
				{
					location = address.PK;
				}
			}

			return location;
		}

		#endregion

		protected override bool ShouldUpdateBO
		{
			get
			{
				if (logger.IsUpdatingConsol)
				{
					return eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.Value;
				}

				return base.ShouldUpdateBO;
			}
		}

		void SetValue(BusinessObject transportBO, SchemaColumn column, OrganizationAddress organizationAddress)
		{
			if (organizationAddress != null)
			{
				var orgAddress = new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched();
				if (orgAddress == null)
				{
					return;
				}

				transportBO[column] = orgAddress.PK;
			}
		}

		bool AllowUpdateSailing
		{
			get
			{
				return SystemDataRegistry.Instance.UpdateSailingSchedulesDuringAutomaticImport.Value
					&& allowUpdateSailing;
			}
		}

		#endregion
	}
}
