using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class SailingLocator : ISailingManaged
	{
		public SailingLocator(BusinessObjectFactory factory, bool isImportingData = false)
		{
			this.Factory = factory;
			this.isImportingData = isImportingData;
		}

		readonly ZBool isImportingData;
		readonly BusinessObjectFactory Factory;

		#region FindSailingFromSailingManager

		public BaseSailingManager FindSailingFromSailingManager(SailingDetails details, bool ignoreActiveFilter = false)
		{
			return FindSailingFromSailingManager(details.TransportMode, details.LoadPort, details.DischargePort, details.Vessel, details.Voyage, details.ShippingLine, details.EtdValue, details.EtaValue, details.StdValue, details.StaValue, ignoreActiveFilter);
		}

		public BaseSailingManager FindSailingFromSailingManager(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vessel, ZString voyage, ZGuid shippingLine, ZDateTime etdValue, ZDateTime etaValue, bool ignoreActiveFilter = false)
		{
			return FindSailingFromSailingManager(transportMode, loadPort, dischargePort, vessel, voyage, shippingLine, etdValue, etaValue, ZDateTime.Empty, ZDateTime.Empty, ignoreActiveFilter);
		}

		public BaseSailingManager FindSailingFromSailingManager(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vessel, ZString voyage, ZGuid shippingLine, ZDateTime etdValue, ZDateTime etaValue, ZDateTime stdValue, ZDateTime staValue, bool ignoreActiveFilter = false)
		{
			TransportMode = transportMode;
			Load = loadPort;
			Discharge = dischargePort;
			Vessel = vessel;
			Voyage = voyage;
			ShippingLine = shippingLine;
			ETD = etdValue;
			ETA = etaValue;
			STD = stdValue;
			STA = staValue;

			var sailingManager = BaseSailingManager.New(this);
			sailingManager.FindExistingSailing(ignoreActiveFilter);
			return sailingManager;
		}

		#endregion

		#region FindOrCreateSailingFromSailingManager

		public BaseSailingManager FindOrCreateSailingFromSailingManager(SailingDetails details)
		{
			return FindOrCreateSailingFromSailingManager(details.TransportMode, details.LoadPort, details.DischargePort, details.Vessel, details.Voyage, details.ShippingLine, details.EtdValue, details.EtaValue, details.StdValue, details.StaValue);
		}

		public BaseSailingManager FindOrCreateSailingFromSailingManager(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vessel, ZString voyage, ZGuid shippingLine, ZDateTime etdValue, ZDateTime etaValue)
		{
			return FindOrCreateSailingFromSailingManager(transportMode, loadPort, dischargePort, vessel, voyage, shippingLine, etdValue, etaValue, ZDateTime.Empty, ZDateTime.Empty);
		}

		public BaseSailingManager FindOrCreateSailingFromSailingManager(ZString transportMode, ZString loadPort, ZString dischargePort, ZString vessel, ZString voyage, ZGuid shippingLine, ZDateTime etdValue, ZDateTime etaValue, ZDateTime stdValue, ZDateTime staValue)
		{
			loadPort = loadPort.SubstringSafe(0, JobVoyOriginSchema.JA_RL_NKPortOfLoading.MaxLength);
			dischargePort = dischargePort.SubstringSafe(0, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge.MaxLength);
			vessel = vessel.SubstringSafe(0, JobVoyageSchema.JV_RV_NKVessel.MaxLength);
			voyage = voyage.SubstringSafe(0, JobVoyageSchema.JV_VoyageFlight.MaxLength);

			TransportMode = transportMode;
			Load = loadPort;
			Discharge = dischargePort;
			Vessel = vessel;
			Voyage = voyage;
			ShippingLine = shippingLine;
			ETD = etdValue;
			ETA = etaValue;
			STD = stdValue;
			STA = staValue;

			var sailingManager = BaseSailingManager.New(this);
			sailingManager.NotifyRead();

			if (sailingManager.Sailing == null)
			{
				sailingManager.ResetSailing();
			}

			return sailingManager;
		}

		#endregion

		#region ISailingManaged Members

		BusinessObjectFactory ISailingManaged.Factory
		{
			get { return this.Factory; }
		}

		public bool AllowScheduleCreation
		{
			get { return canCreateSchedules; }
			set { canCreateSchedules = value; }
		}

		bool canCreateSchedules = true;

		bool ISailingManaged.AllowScheduleDatesChanging
		{
			get { return true; }
		}

		ZString ISailingManaged.Load
		{
			get { return Load; }
			set { Load = value; }
		}
		ZString Load;

		ZString ISailingManaged.Discharge
		{
			get { return Discharge; }
			set { Discharge = value; }
		}
		ZString Discharge;

		ZString ISailingManaged.Vessel
		{
			get { return Vessel; }
			set { Vessel = value; }
		}
		ZString Vessel;

		ZString ISailingManaged.Voyage
		{
			get { return Voyage; }
			set { Voyage = value; }
		}
		ZString Voyage;

		ZDateTime ISailingManaged.ATD
		{
			get { return ATD; }
			set { ATD = value; }
		}
		ZDateTime ATD;

		ZDateTime ISailingManaged.ATA
		{
			get { return ATA; }
			set { ATA = value; }
		}
		ZDateTime ATA;

		ZDateTime ISailingManaged.ETD
		{
			get { return ETD; }
			set { ETD = value; }
		}
		ZDateTime ETD;

		ZDateTime ISailingManaged.ETA
		{
			get { return ETA; }
			set { ETA = value; }
		}
		ZDateTime ETA;

		ZDateTime ISailingManaged.STD
		{
			get { return STD; }
			set { STD = value; }
		}
		ZDateTime STD;

		ZDateTime ISailingManaged.STA
		{
			get { return STA; }
			set { STA = value; }
		}
		ZDateTime STA;

		ZGuid ISailingManaged.ShippingLine
		{
			get { return ShippingLine; }
			set { ShippingLine = value; }
		}
		ZGuid ShippingLine;

		ZString ISailingManaged.TransportMode
		{
			get { return TransportMode; }
		}
		ZString TransportMode;

		ZGuid ISailingManaged.SailingPK
		{
			get { return SailingPK; }
			set { SailingPK = value; }
		}
		ZGuid SailingPK;

		ZBool ISailingManaged.IsCharter
		{
			get { return false; }
			set { }
		}

		ZString ISailingManaged.AircraftType
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString ISailingManaged.OnlineScheduleStatus
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString ISailingManaged.RegistrationNo
		{
			get { return ""; }
			set { }
		}

		ZBool ISailingManaged.IsCargoOnly
		{
			get { return false; }
			set { }
		}

		ZBool ISailingManaged.IsImportingData
		{
			get { return isImportingData; }
		}

		bool ISailingManaged.HasChanges
		{
			get { return false; }
			set { }
		}

		JobSailingValidation ISailingManaged.SailingAdditionalValidation => null;

		JobVoyOriginValidation ISailingManaged.VoyOriginAdditionalValidation => null;

		JobVoyDestinationValidation ISailingManaged.VoyDestinationAdditionalValidation => null;

		void ISailingManaged.SuspendValidation()
		{
		}

		void ISailingManaged.ResumeValidation()
		{
		}

		void ISailingManaged.SetFCLReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetLCLReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetFCLCutOff(ZDateTime value) { }

		void ISailingManaged.SetLCLCutOff(ZDateTime value) { }

		void ISailingManaged.SetDocsCutOff(ZDateTime value) { }

		void ISailingManaged.SetVGMCutOff(ZDateTime value) { }

		void ISailingManaged.SetAvailabilityDate(ZDateTime value) { }

		void ISailingManaged.SetLCLAvailabilityDate(ZDateTime value) { }

		void ISailingManaged.SetStorageDate(ZDateTime value) { }

		void ISailingManaged.SetLCLStorageDate(ZDateTime value) { }

		void ISailingManaged.SetLoadETA(ZDateTime value) { }

		void ISailingManaged.SetLoadATA(ZDateTime value) { }

		void ISailingManaged.SetOA_DepartureLocation(ZGuid value) { }

		void ISailingManaged.SetOA_ArrivalLocation(ZGuid value) { }

		void ISailingManaged.SetEmptyCutOff(ZDateTime value) { }

		void ISailingManaged.SetEmptyReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetDGCutOff(ZDateTime value) { }

		void ISailingManaged.SetDGReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetReeferCutOff(ZDateTime value) { }

		void ISailingManaged.SetReeferReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetServiceString(ZString value) { }

		void ISailingManaged.RegisterEditableChildObject(IBusiness bizO)
		{
		}

		void ISailingManaged.UnRegisterEditableChildObject(IBusiness bizO)
		{
		}

		void ISailingManaged.SetArrivalPortRouteId(ZString value) { }

		void ISailingManaged.SetDeparturePortRouteId(ZString value) { }

		void ISailingManaged.LogDateEvents(object sender) { }

		#endregion
	}
}
