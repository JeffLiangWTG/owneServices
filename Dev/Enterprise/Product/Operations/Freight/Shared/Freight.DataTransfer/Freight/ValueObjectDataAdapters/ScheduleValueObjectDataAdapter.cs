using System;
using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class ScheduleValueObjectDataAdapter : ValueObjectDataAdapter<JobVoyage, Xsd.Schedule>
	{
		#region Overrides

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Schedules"; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Schedule"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleScheduleSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SchedulesSchema; }
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(JobVoyage bizObj, Xsd.Schedule valueObj, IValueObjectImportContext importContext)
		{
			throw new NotSupportedException("Schedules should be updated sailing by sailing");
		}

		public override JobVoyage CreateOrUpdateFromValueObject(Xsd.Schedule scheduleValue, IValueObjectImportContext context)
		{
			JobVoyage voyage = null;

			string transportMode = TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(scheduleValue.TransportMode, string.Empty, context);

			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
				case Core.Constants.TransportModes.Road:
					voyage = ImportRoadAirSchedule(transportMode, scheduleValue, context);
					break;

				case Core.Constants.TransportModes.Sea:
				case Core.Constants.TransportModes.Rail:
					voyage = ImportSeaRailSchedule(transportMode, scheduleValue, context);
					break;

				default:
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("380772e6-873a-4057-8218-5433010198d6", "{0} schedule import is not supported.", scheduleValue.TransportMode)));
					return null;
			}

			if (voyage != null && scheduleValue.Carrier.IsSpecified)
			{
				voyage.JV_OH_Line = context.FindOrCreateTempOrganisationPK(scheduleValue.Carrier, voyage, OrganisationTypes.Carrier);
			}

			return voyage;
		}

		#region Transport import methods

		JobVoyage ImportRoadAirSchedule(string transportMode, Xsd.Schedule scheduleValue, IValueObjectImportContext context)
		{
			Xsd.ScheduleRoadRailFlight roadAirSchedule = scheduleValue.Item as Xsd.ScheduleRoadRailFlight;

			if (roadAirSchedule == null)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("df9099fe-8d00-48e8-bdb2-cee16d882762", "Schedule data is not in valid format.")));
				return null;
			}

			string voyageNo = GetValue(roadAirSchedule.FlightNoJourneyNoTruckRegNo, JobVoyageSchema.JV_VoyageFlight, context);

			DateTime? etd = null;

			foreach (Xsd.FlightWithLoadDischargePortsAndConsols flight in roadAirSchedule.Flights)
			{
				if (flight.ETD.IsValid && !flight.ETD.IsEmpty && (!etd.HasValue || flight.ETD < etd.Value))
				{
					etd = flight.ETD.ToDateTime();
				}
			}

			if (!etd.HasValue)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("65ad1778-5cc3-4f1d-aadc-0f28c8cd5dbb", "Departure date is required for Road and Air schedules.")));
				return null;
			}

			var voyage = FindOrCeateRoadAirVoyage(context.Factory, transportMode, voyageNo, etd.Value);

			ImportSailings(scheduleValue, context, transportMode, string.Empty, voyageNo, ZGuid.Empty);

			if (IsIncompleteVoyage(voyage))
			{
				LogIncompleteVoyageImportFailure(context, voyage);
				return null;
			}

			return voyage;
		}

		JobVoyage ImportSeaRailSchedule(string transportMode, Xsd.Schedule scheduleValue, IValueObjectImportContext context)
		{
			Xsd.ScheduleSailing seaRailSchedule = scheduleValue.Item as Xsd.ScheduleSailing;

			if (seaRailSchedule == null)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("df9099fe-8d00-48e8-bdb2-cee16d882762", "Schedule data is not in valid format.")));
				return null;
			}

			string vesselName = GetValue(seaRailSchedule.VesselName, JobVoyageSchema.JV_RV_NKVessel, context);

			string voyageNo = GetValue(seaRailSchedule.VoyageNo, JobVoyageSchema.JV_VoyageFlight, context);

			ZGuid carrierPK = ZGuid.Empty;
			if (scheduleValue.Carrier.IsSpecified)
			{
				carrierPK = context.FindOrCreateTempOrganisationPK(scheduleValue.Carrier, null, OrganisationTypes.Carrier);
			}

			var voyage = FindOrCeateSeaRailVoyage(context.Factory, transportMode, vesselName, voyageNo, carrierPK);

			ImportSailings(scheduleValue, context, transportMode, vesselName, voyageNo, carrierPK);

			if (IsIncompleteVoyage(voyage))
			{
				LogIncompleteVoyageImportFailure(context, voyage);
				return null;
			}

			return voyage;
		}

		void LogIncompleteVoyageImportFailure(IValueObjectImportContext context, JobVoyage voyage)
		{
			string message = Res.GetString("0bb22c37-20a7-4c96-82d1-fecd3621f550", @"Could not locate an existing matching schedule - {0}.
Cannot create a schedule with an incomplete port pair. Schedule must have both Load port and Destination port data specified.", voyage.HumanReadableName);

			context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, message));
		}

		public JobVoyage FindOrCeateSeaRailVoyage(BusinessObjectFactory factory, string transportMode, string vesselName, string voyageNo, ZGuid carrierPK)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("Valid factory is required");
			}
			else if (transportMode != Core.Constants.TransportModes.Sea && transportMode != Core.Constants.TransportModes.Rail ||
				String.IsNullOrEmpty(vesselName) || String.IsNullOrEmpty(voyageNo))
			{
				return null;
			}

			var voyage = new JobVoyage.Loader(factory).Load(transportMode, vesselName, voyageNo, carrierPK);

			if (voyage == null)
			{
				voyage = factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = transportMode;
				voyage.JV_RV_NKVessel = vesselName;
				voyage.JV_VoyageFlight = voyageNo;
				voyage.JV_OH_Line = carrierPK;
			}

			return voyage;
		}

		public JobVoyage FindOrCeateRoadAirVoyage(BusinessObjectFactory factory, string transportMode, string voyageNo, DateTime? etd)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("Valid factory is required");
			}
			else if (transportMode != Core.Constants.TransportModes.Road && transportMode != Core.Constants.TransportModes.Air ||
				String.IsNullOrEmpty(voyageNo) || !etd.HasValue)
			{
				return null;
			}

			var voyage = new JobVoyage.Loader(factory).Load(transportMode, string.Empty, voyageNo, etd.Value);

			if (voyage == null)
			{
				voyage = factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = transportMode;
				voyage.JV_VoyageFlight = voyageNo;
				voyage.JV_FlightDate = etd.Value;
			}

			return voyage;
		}

		ZString GetValue(ZString data, SchemaColumn schemaColumn, IValueObjectImportContext context)
		{
			if (schemaColumn.HasMaxLength)
			{
				int maxLength = schemaColumn.MaxLength;

				if (data.Length > maxLength)
				{
					context.Notify(new WarningNotification(Res.GetString("dfbdf011-a61b-4b54-9338-1165f15e7bec", "{0} accepts max {1} characters but {2} were entered", schemaColumn.Name, maxLength, data.Length)));
					return data.SubstringSafe(0, maxLength);
				}
			}
			return data;
		}

		void ImportSailings(Xsd.Schedule scheduleValue, IValueObjectImportContext context, string transportMode, string vesselName, string voyageNo, ZGuid carrierPK)
		{
			Xsd.SailingBase[] sailingValues = XsdSchedule.GetSailings(scheduleValue);

			foreach (Xsd.SailingBase sailingValue in sailingValues)
			{
				SailingValueObjectDataAdapter sailingAdapter = NewSailingDataAdapter(
					transportMode,
					XsdSailingBase.GetLoadPort(sailingValue),
					XsdSailingBase.GetDischargePort(sailingValue),
					vesselName,
					voyageNo,
					carrierPK);

				sailingAdapter.CreateOrUpdateFromValueObject(sailingValue, context);
			}
		}

		public static bool IsIncompleteVoyage(JobVoyage voyage)
		{
			return voyage != null && (voyage.Origins == null || voyage.Destinations == null || !voyage.Origins.Any() || !voyage.Destinations.Any());
		}

		#endregion

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(JobVoyage schedule, Xsd.Schedule scheduleValue, IValueObjectExportContext context)
		{
			// Rail schedule requires VesselName which is MIA in Xsd.ScheduleRoadRailFlight so we need to use Xsd.ScheduleSailing
			if (schedule.IsRoad || schedule.IsAir)
			{
				Xsd.ScheduleRoadRailFlight roadAirSchedule = new Xsd.ScheduleRoadRailFlight();
				roadAirSchedule.FlightNoJourneyNoTruckRegNo = schedule.JV_VoyageFlight.IsEmpty ? null : (string)schedule.JV_VoyageFlight;
				scheduleValue.Item = roadAirSchedule;
			}
			else
			{
				Xsd.ScheduleSailing railSeaSchedule = new Xsd.ScheduleSailing();
				railSeaSchedule.VesselName = schedule.JV_RV_NKVessel.IsEmpty ? null : (string)schedule.JV_RV_NKVessel;
				railSeaSchedule.VoyageNo = schedule.JV_VoyageFlight.IsEmpty ? null : (string)schedule.JV_VoyageFlight;
				scheduleValue.Item = railSeaSchedule;
			}

			scheduleValue.TransportMode = TransportModeToXmlCodeMappings.Instance.GetExternalCode(schedule.JV_AirSeaRoad, string.Empty, context);
			scheduleValue.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(schedule.Line, context);

			ExportSailings(schedule, scheduleValue, context);
		}

		void ExportSailings(JobVoyage schedule, Xsd.Schedule scheduleValue, IValueObjectExportContext context)
		{
			foreach (JobSailing sailing in schedule.Sailings)
			{
				SailingValueObjectDataAdapter sailingAdapter = NewSailingDataAdapter(
					ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					ZGuid.Empty);

				if (scheduleValue.Item is Xsd.ScheduleRoadRailFlight)
				{
					ExportAirRoadRailSailing(sailingAdapter, sailing, scheduleValue, context);
				}
				else if (scheduleValue.Item is Xsd.ScheduleSailing)
				{
					ExportSeaSailing(sailingAdapter, sailing, scheduleValue, context);
				}
			}
		}

		void ExportAirRoadRailSailing(SailingValueObjectDataAdapter sailingAdapter, JobSailing sailing, Xsd.Schedule scheduleValue, IValueObjectExportContext context)
		{
			Xsd.ScheduleRoadRailFlight flightDetailsValue = (Xsd.ScheduleRoadRailFlight)scheduleValue.Item;
			if (flightDetailsValue.Flights == null)
			{
				flightDetailsValue.Flights = new Xsd.FlightWithLoadDischargePortsAndConsolsCollection();
			}
			Xsd.FlightWithLoadDischargePortsAndConsols flightValue = flightDetailsValue.Flights.AddNew();
			sailingAdapter.ExportToValueObject(sailing, flightValue, context);
			flightValue.LoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			flightValue.DischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
		}

		void ExportSeaSailing(SailingValueObjectDataAdapter sailingAdapter, JobSailing sailing, Xsd.Schedule scheduleValue, IValueObjectExportContext context)
		{
			Xsd.ScheduleSailing sailingDetailsValue = (Xsd.ScheduleSailing)scheduleValue.Item;
			if (sailingDetailsValue.Sailings == null)
			{
				sailingDetailsValue.Sailings = new Xsd.SailingWithLoadDischargePortsAndConsolsCollection();
			}
			Xsd.SailingWithLoadDischargePortsAndConsols sailingValue = sailingDetailsValue.Sailings.AddNew();
			sailingAdapter.ExportToValueObject(sailing, sailingValue, context);
			sailingValue.LoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			sailingValue.DischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
		}

		protected virtual SailingValueObjectDataAdapter NewSailingDataAdapter(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vesselName, ZString voyageNo, ZGuid carrierPK)
		{
			return new SailingValueObjectDataAdapter(transportMode, loadPort, dischargePort, vesselName, voyageNo, carrierPK);
		}

		#endregion
	}
}
