using System;
using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class SailingValueObjectDataAdapter : FreightValueObjectDataAdapter<JobSailing, Xsd.SailingBase>
	{
		public SailingValueObjectDataAdapter(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
		{
			this.TransportMode = transportMode;
			this.LoadPort = loadPort;
			this.DischargePort = dischargePort;
			this.VesselName = vesselName.SubstringSafe(0, JobVoyageSchema.JV_RV_NKVessel.MaxLength);
			this.VoyageNo = voyageNo.SubstringSafe(0, JobVoyageSchema.JV_VoyageFlight.MaxLength);
			this.CarrierPK = carrierPK;

			AllowUpdates = true;
		}

		public readonly ZString TransportMode;
		public readonly ZString LoadPort;
		public readonly ZString DischargePort;
		public readonly ZString VesselName;
		public readonly ZString VoyageNo;
		public readonly ZGuid CarrierPK;

		JobVoyage Voyage;

		public bool AllowUpdates { get; set; }

		#region Schema Overrides

		public override string RootCollectionElementName
		{
			get { throw new NotSupportedException(); }
		}

		public override string RootElementName
		{
			get
			{
				return (TransportMode == Core.Constants.TransportModes.Sea || TransportMode == Core.Constants.TransportModes.Rail) ?
					(NoResString)"Sailing" : "RoadRailFlight";
			}
		}

		public override XmlSchema Schema
		{
			get
			{
				return (TransportMode == Core.Constants.TransportModes.Sea || TransportMode == Core.Constants.TransportModes.Rail) ?
					FreightXmlSchemaDefinitions.Instance.SailingSchema : FreightXmlSchemaDefinitions.Instance.RoadRailFlightSchema;
			}
		}

		public override XmlSchema CollectionSchema
		{
			get { throw new NotSupportedException(); }
		}

		protected override JobSailing FindBusinessObject(Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			throw new NotImplementedException();
		}

		protected override JobSailing NewBusinessObject(Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Import

		public override JobSailing CreateOrUpdateFromValueObject(Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			return CreateOrUpdateFromValueObjectCore(sailingValue, context, false);
		}

		public JobSailing CreateOrUpdateFromValueObjectWithSchedule(Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			return CreateOrUpdateFromValueObjectCore(sailingValue, context, true);
		}

		JobSailing CreateOrUpdateFromValueObjectCore(Xsd.SailingBase sailingValue, IValueObjectImportContext context, bool createVoyageIfNotFound)
		{
			if (LoadPort.IsEmpty && DischargePort.IsEmpty)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("c87bc772-2751-4db9-bbbf-bbaa830c6ff2", "Both Load and Discharge Ports are missing.")));
				return null;
			}
			else if (LoadPort == DischargePort)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("28a2719f-07f2-4556-8a81-973f80e63c69", "Cannot import circular route (Load = '{0}' -> Discharge = '{1}').", LoadPort, DischargePort)));
				return null;
			}
			else if ((LoadPort.IsEmpty || DischargePort.IsEmpty) &&
					(sailingValue.ETD.IsEmpty || !sailingValue.ETD.IsValid) &&
					(TransportMode == Core.Constants.TransportModes.Air || TransportMode == Core.Constants.TransportModes.Road))
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("56e17071-4f38-43dc-b5cc-bc243d3660eb", "For Air or Road sailings departure date is required when Load or Discharge Port is missing.")));
				return null;
			}

			Voyage = new JobVoyage.Loader(context.Factory).Load(TransportMode, VesselName, VoyageNo, CarrierPK, sailingValue.ETD);
			if (Voyage == null && createVoyageIfNotFound)
			{
				var voyageAdapter = new ScheduleValueObjectDataAdapter();
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.Road:
						if (!sailingValue.ETD.IsEmpty && sailingValue.ETD.IsValid)
						{
							Voyage = voyageAdapter.FindOrCeateRoadAirVoyage(context.Factory, TransportMode, VoyageNo, sailingValue.ETD.ToDateTime());
						}
						break;
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.Rail:
						Voyage = voyageAdapter.FindOrCeateSeaRailVoyage(context.Factory, TransportMode, VesselName, VoyageNo, CarrierPK);
						break;
				}
			}
			if (Voyage == null)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("5ef1c847-731c-46cd-b58e-e0fb4013ed03", "Could not locate parent schedule.")));
				return null;
			}

			var origin = EnsureOriginExists();
			var destination = EnsureDestinationExists();
			if (HasInvalidDateCombination(origin, destination, sailingValue, context))
			{
				return null;
			}

			if (origin is null)
			{
				UpdateDestinationDatesOnly(destination, sailingValue, context);
				context.Notify(new BusinessObjectCreatedOrUpdatedNotification(destination));
				return null;
			}
			if (destination is null)
			{
				UpdateOriginDatesOnly(origin, sailingValue, context);
				context.Notify(new BusinessObjectCreatedOrUpdatedNotification(origin));
				return null;
			}

			var sailing = Voyage.Sailings.Cast<JobSailing>().FirstOrDefault(s => s.JX_JA == origin.PK && s.JX_JB == destination.PK);
			if (sailing != null)
			{
				ImportFromValueObjectCore(sailing, sailingValue, context);
				context.Notify(new BusinessObjectCreatedOrUpdatedNotification(sailing));
			}
			return sailing;
		}

		bool HasInvalidDateCombination(VoyageOrigin origin, VoyageDestination destination, Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			var transportMode = JobVoyageSailingsHelper.ParseToTransportMode(TransportMode);
			var etdUTC = sailingValue.ETD.IsValid ? Env.Time.GetUtcFromUnlocoTime(LoadPort, sailingValue.ETD.ToDateTime()) : ZDateTime.Empty;
			var etaUTC = sailingValue.ETA.IsValid ? Env.Time.GetUtcFromUnlocoTime(DischargePort, sailingValue.ETA.ToDateTime()) : ZDateTime.Empty;
			if (origin is null)
			{
				foreach (var sailing in Voyage.Sailings.Cast<JobSailing>())
				{
					if (sailing.JX_JB == destination.PK
						&& JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(sailing.Origin.JA_E_DEP, sailingValue.ETA, sailing.Origin.JA_E_DEP_UTC, etaUTC, transportMode))
					{
						context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
							Res.GetString("a17aea9f-41c6-4557-85c2-b9a0c7dbf7a7", "Invalid date combination from Origin ({0} {1}) to Destination ({2} {3}).",
							sailing.Origin.JA_RL_NKPortOfLoading, sailing.Origin.JA_E_DEP, DischargePort, sailingValue.ETA)));
						return true;
					}
				}
			}
			else if (destination is null)
			{
				foreach (var sailing in Voyage.Sailings.Cast<JobSailing>())
				{
					if (sailing.JX_JA == origin.PK
						&& JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(sailingValue.ETD, sailing.Destination.JB_E_ARV, etdUTC, sailing.Destination.JB_E_ARV_UTC, transportMode))
					{
						context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
							Res.GetString("a17aea9f-41c6-4557-85c2-b9a0c7dbf7a7", "Invalid date combination from Origin ({0} {1}) to Destination ({2} {3}).",
							LoadPort, sailingValue.ETD, sailing.Destination.JB_RL_NKPortOfDischarge, sailing.Destination.JB_E_ARV)));
						return true;
					}
				}
			}
			else if (JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(sailingValue.ETD, sailingValue.ETA, etdUTC, etaUTC, transportMode))
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError,
					Res.GetString("a17aea9f-41c6-4557-85c2-b9a0c7dbf7a7", "Invalid date combination from Origin ({0} {1}) to Destination ({2} {3}).",
					LoadPort, sailingValue.ETD, DischargePort, sailingValue.ETA)));
				return true;
			}

			return false;
		}

		VoyageOrigin EnsureOriginExists()
		{
			if (LoadPort.IsEmpty)
			{
				return null;
			}

			var origin = (from o in Voyage.Origins.Cast<VoyageOrigin>()
						  where o.JA_RL_NKPortOfLoading == LoadPort
						  select o).FirstOrDefault();

			if (origin == null)
			{
				origin = Voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = LoadPort;
			}

			return origin;
		}

		VoyageDestination EnsureDestinationExists()
		{
			if (DischargePort.IsEmpty)
			{
				return null;
			}

			var destination = (from d in Voyage.Destinations.Cast<VoyageDestination>()
							   where d.JB_RL_NKPortOfDischarge == DischargePort
							   select d).FirstOrDefault();

			if (destination == null)
			{
				destination = Voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = DischargePort;
			}

			return destination;
		}

		void UpdateDestinationDatesOnly(VoyageDestination destination, Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			if (!AllowUpdates)
			{
				return;
			}

			if (sailingValue.ATA.IsValid)
			{
				context.SetPropertyInfoValue(destination.JB_A_ARVInfo, sailingValue.ATA.ToDateTime());
			}

			if (sailingValue.ETA.IsValid)
			{
				context.SetPropertyInfoValue(destination.JB_E_ARVInfo, sailingValue.ETA.ToDateTime());
			}

			if (sailingValue is Xsd.SailingWithLoadDischargePorts sailingWithPorts)
			{
				ImportDestinationFCLDates(sailingWithPorts, destination, context);
				ImportSailingLCLDates(sailingWithPorts, destination, context);
			}

			ImportDestinationDetails(sailingValue, destination, context);
		}

		void ImportDestinationFCLDates(Xsd.SailingWithLoadDischargePorts sailingValue, VoyageDestination destination, IValueObjectImportContext context)
		{
			if (sailingValue.FCLDates.AvailableDate.IsValid)
			{
				context.SetPropertyInfoValue(destination.JB_AvailabilityDateInfo, sailingValue.FCLDates.AvailableDate.ToDateTime());
			}
			if (sailingValue.FCLDates.StorageDate.IsValid)
			{
				context.SetPropertyInfoValue(destination.JB_StorageDateInfo, sailingValue.FCLDates.StorageDate.ToDateTime());
			}
		}

		void ImportSailingLCLDates(Xsd.SailingWithLoadDischargePorts sailingValue, VoyageDestination destination, IValueObjectImportContext context)
		{
			var destinationSailings = context.Factory.Load<JobSailing>(new ZQuery(JobSailingSchema.JX_JB, destination.PK));

			foreach (var sailing in destinationSailings)
			{
				context.Notify(new InfoNotification(Res.GetString("925772ea-514e-44fa-b58a-8a62ba590aa6", "Updating Sailing Port Pair (Load='{0}' Discharge='{1}')", sailing.JX_JA_RL_NKPortOfLoading, sailing.JX_JB_RL_NKPortOfDischarge)));

				if (sailingValue.LCLDates.AvailableDate.IsValid)
				{
					context.SetPropertyInfoValue(sailing.JX_DepotAvailabilityDateInfo, sailingValue.LCLDates.AvailableDate.ToDateTime());
				}
				if (sailingValue.LCLDates.StorageDate.IsValid)
				{
					context.SetPropertyInfoValue(sailing.JX_DepotStorageDateInfo, sailingValue.LCLDates.StorageDate.ToDateTime());
				}
			}
		}

		void UpdateOriginDatesOnly(VoyageOrigin origin, Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			if (!AllowUpdates)
			{
				return;
			}

			if (sailingValue.ATD.IsValid)
			{
				context.SetPropertyInfoValue(origin.JA_A_DEPInfo, sailingValue.ATD.ToDateTime());
			}
			if (sailingValue.ETD.IsValid)
			{
				context.SetPropertyInfoValue(origin.JA_E_DEPInfo, sailingValue.ETD.ToDateTime());
			}
			if (sailingValue is Xsd.SailingWithLoadDischargePorts sailingWithPorts)
			{
				ImportOriginFCLDates(sailingWithPorts, origin, context);
				ImportSailingLCLDates(sailingWithPorts, origin, context);
			}

			ImportOriginDetails(sailingValue, origin, context);
		}

		void ImportOriginFCLDates(Xsd.SailingWithLoadDischargePorts sailingValue, VoyageOrigin origin, IValueObjectImportContext context)
		{
			if (sailingValue.FCLDates.CutOffDate.IsValid)
			{
				context.SetPropertyInfoValue(origin.JA_CutOffInfo, sailingValue.FCLDates.CutOffDate.ToDateTime());
			}
			if (sailingValue.FCLDates.ReceivalCommencesDate.IsValid)
			{
				context.SetPropertyInfoValue(origin.JA_ReceivalCommencesInfo, sailingValue.FCLDates.ReceivalCommencesDate.ToDateTime());
			}
		}

		void ImportSailingLCLDates(Xsd.SailingWithLoadDischargePorts sailingValue, VoyageOrigin origin, IValueObjectImportContext context)
		{
			var originSailings = context.Factory.Load<JobSailing>(new ZQuery(JobSailingSchema.JX_JA, origin.PK));

			foreach (var sailing in originSailings)
			{
				context.Notify(new InfoNotification(Res.GetString("925772ea-514e-44fa-b58a-8a62ba590aa6", "Updating Sailing Port Pair (Load='{0}' Discharge='{1}')", sailing.JX_JA_RL_NKPortOfLoading, sailing.JX_JB_RL_NKPortOfDischarge)));

				if (sailingValue.LCLDates.CutOffDate.IsValid)
				{
					context.SetPropertyInfoValue(sailing.JX_DepotCutOffInfo, sailingValue.LCLDates.CutOffDate.ToDateTime());
				}
				if (sailingValue.LCLDates.ReceivalCommencesDate.IsValid)
				{
					context.SetPropertyInfoValue(sailing.JX_DepotReceivalCommencesInfo, sailingValue.LCLDates.ReceivalCommencesDate.ToDateTime());
				}
			}
		}

		internal bool ConfirmUpdateOfExistingJobSailing(JobSailing bizObj, INotifications notifications)
		{
			return ConfirmUpdateOfExistingBusinessObject(bizObj, notifications);
		}

		protected override void ImportFromValueObjectCore(JobSailing sailing, Xsd.SailingBase sailingValue, IValueObjectImportContext context)
		{
			if (AllowUpdates || !sailing.Origin.IsInDatabase)
			{
				ImportOriginDetails(sailingValue, sailing.Origin, context);
			}

			if (AllowUpdates || !sailing.Destination.IsInDatabase)
			{
				ImportDestinationDetails(sailingValue, sailing.Destination, context);
			}

			if (AllowUpdates || !sailing.IsInDatabase)
			{
				ImportSailingDetails(sailingValue, sailing, context);
			}
		}

		void ImportSailingDetails(Xsd.SailingBase sailingValue, JobSailing sailing, IValueObjectImportContext context)
		{
			if (sailingValue.IsPublishedSpecified && !sailing.IsInDatabase)
			{
				sailing.JX_IsPublished = sailingValue.IsPublished;
			}
			if (sailingValue is Xsd.Sailing)
			{
				Xsd.Sailing sailingDetails = (Xsd.Sailing)sailingValue;
				XsdSailingDates.FromLCLValueObject(sailing, sailingDetails.LCLDates);
				XsdSailingDates.FromFCLValueObject(sailing, sailingDetails.FCLDates);
			}
			else if (sailingValue is Xsd.Flight)
			{
				Xsd.Flight flightValue = (Xsd.Flight)sailingValue;
				XsdSailingDates.FromLCLValueObject(sailing, flightValue.Dates);
			}
		}

		void ImportOriginDetails(Xsd.SailingBase sailingValue, VoyageOrigin origin, IValueObjectImportContext context)
		{
			if (sailingValue.ETD.IsValid)
			{
				origin.JA_E_DEP = sailingValue.ETD;
			}

			if (sailingValue.ATD.IsValid)
			{
				origin.JA_A_DEP = sailingValue.ATD;
			}

			if (sailingValue.LoadPortETA.IsValid)
			{
				origin.JA_E_ARV = sailingValue.LoadPortETA;
			}

			if (sailingValue.LoadPortATA.IsValid)
			{
				origin.JA_A_ARV = sailingValue.LoadPortATA;
			}

			context.SetPropertyInfoValue(origin.JA_RL_NKPortOfLoadingInfo, this.LoadPort, true, Res.GetString("590afb9b-ceb2-4d73-87c9-9cd1a0207dd8", "Origin"));

			if (origin.JA_Berth.IsEmpty)
			{
				context.SetPropertyInfoValue(origin.JA_BerthInfo, sailingValue.DepartureBerth, sailingValue.DepartureBerthSpecified);
			}

			if (!origin.JA_OA_DepartureCTOAddress.IsValid)
			{
				origin.JA_OA_DepartureCTOAddress = new AddressValueObjectHelper(Res.GetString("8342e93f-f990-4fb7-9982-839a5260ba77", "Departure CTO")).FromAddressReferenceGetAddressPK(sailingValue.DepartureCTO, context);
			}

			if (sailingValue.DocCutOffDate.IsValid && !origin.JA_DocumentaryCutoff.IsValid)
			{
				context.SetPropertyInfoValue(origin.JA_DocumentaryCutoffInfo, sailingValue.DocCutOffDate.ToDateTime());
			}

			context.SetPropertyInfoValue(origin.JA_DepartReferenceInfo, sailingValue.DepartureReference);
		}

		void ImportDestinationDetails(Xsd.SailingBase sailingValue, VoyageDestination destination, IValueObjectImportContext context)
		{
			if (sailingValue.ETA.IsValid)
			{
				destination.JB_E_ARV = sailingValue.ETA;
			}

			if (sailingValue.ATA.IsValid)
			{
				destination.JB_A_ARV = sailingValue.ATA;
			}

			context.SetPropertyInfoValue(destination.JB_RL_NKPortOfDischargeInfo, this.DischargePort, true, Res.GetString("1ac803a7-e124-4342-8d29-212e1907ddc0", "Destination"));

			if (!sailingValue.ArrivalBerth.IsEmpty)
			{
				context.SetPropertyInfoValue(destination.JB_BerthInfo, sailingValue.ArrivalBerth, sailingValue.ArrivalBerthSpecified);
			}

			if (sailingValue.ArrivalCTO.IsSpecified)
			{
				destination.JB_OA_ArrivalCTOAddress = new AddressValueObjectHelper(Res.GetString("941c785c-2edd-4caa-8bae-aa1405432941", "Arrival CTO")).FromAddressReferenceGetAddressPK(sailingValue.ArrivalCTO, context);
			}

			if (sailingValue.IsTranshipmentSpecified)
			{
				destination.JB_IsTranshipment = sailingValue.IsTranshipment;
			}
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return SystemRegistry.UpdateSailingSchedulesDuringAutomaticImport.Value; }
		}

		#endregion

		#region Export

		/// <summary>
		/// Create new adapter with empty parameters and run ExportToValueObject()
		/// </summary>
		public static void RunExport(JobSailing sailing, Xsd.SailingBase result, IValueObjectExportContext context)
		{
			var adapter = new SailingValueObjectDataAdapter(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZGuid.Empty);
			adapter.ExportToValueObject(sailing, result, context);
		}

		protected override void ExportToValueObjectCore(JobSailing sailing, Xsd.SailingBase result, IValueObjectExportContext context)
		{
			if (result is Xsd.Sailing)
			{
				Xsd.Sailing sailingValue = (Xsd.Sailing)result;
				sailingValue.LCLDates = XsdSailingDates.ToLCLValueObject(
					sailing,
					sailing.JX_DepotAvailabilityDate,
					sailing.JX_DepotCutOff,
					sailing.JX_DepotReceivalCommences,
					sailing.JX_DepotStorageDate);

				sailingValue.FCLDates = XsdSailingDates.ToFCLValueObject(
					sailing,
					sailing.JX_JB_CTOAvailabilityDate,
					sailing.JX_JA_CTOCutOff,
					sailing.JX_JA_CTOReceivalCommences,
					sailing.JX_JB_CTOStorageDate);
			}
			else if (result is Xsd.Flight)
			{
				Xsd.Flight flightValue = (Xsd.Flight)result;
				flightValue.Dates = XsdSailingDates.ToLCLValueObject(
					sailing,
					sailing.JX_DepotAvailabilityDate,
					sailing.JX_DepotCutOff,
					sailing.JX_DepotReceivalCommences,
					sailing.JX_DepotStorageDate);

				flightValue.DocCutOffDate = sailing.JX_JA_DocumentaryCutoff;
			}

			result.DocCutOffDate = sailing.JX_JA_DocumentaryCutoff;
			ExportOriginDetails(sailing, result, context);
			ExportDestinationDetails(sailing, result, context);

			result.IsTranshipment = (bool)(sailing.Destination?.JB_IsTranshipment ?? false);
			result.IsTranshipmentSpecified = result.IsTranshipment;
		}

		void ExportOriginDetails(JobSailing sailing, Xsd.SailingBase sailingValue, IValueObjectExportContext context)
		{
			if (sailing.Origin != null)
			{
				sailingValue.ETD = sailing.Origin.JA_E_DEP;
				sailingValue.ATD = sailing.Origin.JA_A_DEP;
				sailingValue.LoadPortETA = sailing.Origin.JA_E_ARV;
				sailingValue.LoadPortATA = sailing.Origin.JA_A_ARV;
				sailingValue.DepartureBerth = sailing.Origin.JA_Berth;
				sailingValue.DepartureCTO = new AddressValueObjectHelper(Res.GetString("d28e3920-cf00-4396-a26d-e46347868694", "Departure CTO")).ToAddressReference(sailing.Origin.DepartureCTOAddress, context);
			}
		}

		void ExportDestinationDetails(JobSailing sailing, Xsd.SailingBase sailingValue, IValueObjectExportContext context)
		{
			if (sailing.Destination != null)
			{
				sailingValue.ETA = sailing.Destination.JB_E_ARV;
				sailingValue.ATA = sailing.Destination.JB_A_ARV;
				sailingValue.ArrivalBerth = sailing.Destination.JB_Berth;
				sailingValue.ArrivalCTO = new AddressValueObjectHelper(Res.GetString("299dd654-ed8c-4aac-a897-799cd5975480", "Arrival CTO")).ToAddressReference(sailing.Destination.ArrivalCTOAddress, context);
			}
		}

		#endregion
	}
}
