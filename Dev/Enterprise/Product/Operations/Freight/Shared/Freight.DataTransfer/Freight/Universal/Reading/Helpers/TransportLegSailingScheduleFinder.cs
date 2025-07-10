using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegSailingScheduleFinder
	{
		public TransportLegSailingScheduleFinder(UniversalObjectFactory universalObjectFactory,
			TransportLeg transportLeg,
			IXmlImportLogger logger,
			Transport transport = null,
			ITransportParent transportParent = null)
		{
			UniversalObjectFactory = universalObjectFactory;
			Factory = universalObjectFactory.BOFactory;
			TransportLeg = transportLeg;
			Logger = logger;
			Transport = transport;
			TransportParent = transportParent;
		}

		protected BusinessObjectFactory Factory { get; }

		UniversalObjectFactory UniversalObjectFactory { get; }

		TransportLeg TransportLeg { get; }

		IXmlImportLogger Logger { get; }

		Transport Transport { get; }

		ITransportParent TransportParent { get; }

		public BaseSailingManager GetTransportSailingFromSailingManager()
		{
			var details = GetSailingDetails();
			return GetTransportSailingFromSailingManager(details);
		}

		protected virtual ZGuid GetCarrierFromTransportParent()
		{
			if (TransportParent != null)
			{
				return Factory.Load<OrgHeader>(TransportParent.TransportSupporter.ShippingLine)?.PK ?? ZGuid.Empty;
			}

			return ZGuid.Empty;
		}

		#region Implementation

		SailingDetails GetSailingDetails()
		{
			var carrier = ZGuid.Empty;

			var transportMode = new TransportModeConverter().FromEnumValue(TransportLeg.TransportMode);
			if (transportMode == Core.Constants.TransportModes.Sea)
			{
				if (!TryGetValidCarrier(out carrier))
				{
					return SailingDetails.Empty;
				}
			}

			return ConstructSailingDetails(carrier);
		}

		BaseSailingManager GetTransportSailingFromSailingManager(SailingDetails details)
		{
			RefVessel refVessel = null;
			if (details.TransportMode == Core.Constants.TransportModes.Sea)
			{
				refVessel = GetValidExistingVessel();
			}

			var locator = new SailingLocator(Factory, true)
			{
				AllowScheduleCreation = details.TransportMode != Core.Constants.TransportModes.Sea || !details.ShippingLine.IsEmpty
			};

			var createSailingIfNotExists = SailingCreationAllowed(details, refVessel);
			if (!createSailingIfNotExists)
			{
				var log = refVessel == null && details.TransportMode == Core.Constants.TransportModes.Sea
					? (NoResString)"An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule..."
					: (NoResString)"A Schedule will not be created. Attempting to find an existing Schedule...";

				Logger.Log(LogType.Information, log);
			}

			return FindSailingFromSailingManager(locator, details, createSailingIfNotExists);
		}

		bool SailingCreationAllowed(SailingDetails details, RefVessel refVessel)
		{
			return SailingCreationAllowedCore(details, refVessel);
		}

		bool TryGetValidCarrier(out ZGuid carrierPK)
		{
			var refVessel = GetValidExistingVessel();

			if (TransportLeg.Carrier != null)
			{
				var carrierAddress = new TransportLegCarrierDataObjectReader(TransportLeg.Carrier, Logger, UniversalObjectFactory).GetMatched();
				carrierPK = carrierAddress?.OA_OH ?? ZGuid.Empty;
				return carrierAddress != null;
			}

			if (refVessel?.RV_OH is ZGuid pk
				&& !pk.IsEmpty)
			{
				carrierPK = pk;
				return true;
			}

			carrierPK = GetCarrierFromTransportParent();
			return true;
		}

		RefVessel GetValidExistingVessel()
		{
			var vesselName = TrimAndTruncateStringToColumnLength(TransportLeg.GetVesselName(Factory), JobConsolTransportSchema.JW_Vessel);
			if (!vesselName.IsEmpty)
			{
				return RefVessel.LookupVesselByName(vesselName, Factory).FirstOrDefault();
			}

			return null;
		}

		protected virtual bool SailingCreationAllowedCore(SailingDetails details, RefVessel refVessel)
		{
			return (Transport?.TransportSupporter?.CreateSailingIfNotExistsForUniversalShipment ?? false)
				&& (details.TransportMode != Core.Constants.TransportModes.Sea || refVessel != null);
		}

		BaseSailingManager FindSailingFromSailingManager(SailingLocator locator, SailingDetails details, bool createSailingIfNotExists)
		{
			return createSailingIfNotExists
				? FindOrCreateSailingFromSailingManager(locator, details)
				: locator.FindSailingFromSailingManager(details);
		}

		BaseSailingManager FindOrCreateSailingFromSailingManager(SailingLocator locator, SailingDetails details)
		{
			var sailingManager = FindSailingFromSailingManagerWithInactiveFallback(locator, details);
			return IsArchivedSailingSchedule(sailingManager.Sailing)
				? sailingManager
				: locator.FindOrCreateSailingFromSailingManager(details);
		}

		BaseSailingManager FindSailingFromSailingManagerWithInactiveFallback(SailingLocator locator, SailingDetails details)
		{
			var sailingManager = locator.FindSailingFromSailingManager(details);
			return sailingManager.Sailing == null
				? locator.FindSailingFromSailingManager(details, true)
				: sailingManager;
		}

		bool IsArchivedSailingSchedule(JobSailing sailing)
		{
			return sailing?.Voyage != null && sailing.Voyage.IsArchived;
		}

		SailingDetails ConstructSailingDetails(ZGuid carrierPK)
		{
			var transportMode = new TransportModeConverter().FromEnumValue(TransportLeg.TransportMode);

			var vesselName = TrimAndTruncateStringToColumnLength(TransportLeg.GetVesselName(Factory), JobConsolTransportSchema.JW_Vessel);
			var voyageFlight = TrimAndTruncateStringToColumnLength(TransportLeg.VoyageFlightNo ?? ZString.Empty, JobConsolTransportSchema.JW_VoyageFlight);

			var portOfLoading = TransportLeg.PortOfLoading?.GetUNLOCOAsUpperCase(Factory) ?? ZString.Empty;
			var portOfDischarge = TransportLeg.PortOfDischarge?.GetUNLOCOAsUpperCase(Factory) ?? ZString.Empty;

			var eta = TransportLeg.EstimatedArrival ?? ZDateTime.Empty;
			var etd = TransportLeg.EstimatedDeparture ?? ZDateTime.Empty;
			var std = TransportLeg.ScheduledDeparture ?? ZDateTime.Empty;
			var sta = TransportLeg.ScheduledArrival ?? ZDateTime.Empty;

			return new SailingDetails(transportMode, portOfLoading, portOfDischarge, vesselName, voyageFlight, carrierPK, etd, eta, std, sta);
		}

		ZString TrimAndTruncateStringToColumnLength(ZString stringToTrimAndTruncate, SchemaStringColumn column)
		{
			return stringToTrimAndTruncate.Length > column.MaxLength
				? stringToTrimAndTruncate.Trim().Substring(0, column.MaxLength)
				: stringToTrimAndTruncate;
		}

		#endregion
	}
}
