using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Common.Business
{
	public interface ISailingManaged
	{
		BusinessObjectFactory Factory { get; }

		ZString TransportMode { get; }
		ZString Load { get; set; }
		ZString Discharge { get; set; }
		ZString Vessel { get; set; }
		ZString Voyage { get; set; }
		ZDateTime ATD { get; set; }
		ZDateTime ATA { get; set; }
		ZDateTime ETD { get; set; }
		ZDateTime ETA { get; set; }
		ZDateTime STD { get; set; }
		ZDateTime STA { get; set; }
		bool HasChanges { get; set; }
		ZGuid ShippingLine { get; set; }
		ZGuid SailingPK { get; set; }
		ZBool IsCharter { get; set; }
		ZString RegistrationNo { get; set; }
		ZBool IsCargoOnly { get; set; }
		ZBool IsImportingData { get; }
		ZString AircraftType { get; set; }
		ZString OnlineScheduleStatus { get; set; }

		bool AllowScheduleCreation { get; }
		bool AllowScheduleDatesChanging { get; }

		JobSailingValidation SailingAdditionalValidation { get; }
		JobVoyOriginValidation VoyOriginAdditionalValidation { get; }
		JobVoyDestinationValidation VoyDestinationAdditionalValidation { get; }

		void SuspendValidation();
		void ResumeValidation();
		void RegisterEditableChildObject(IBusiness bizO);
		void UnRegisterEditableChildObject(IBusiness bizO);

		void SetFCLReceivalCommences(ZDateTime value);
		void SetLCLReceivalCommences(ZDateTime value);
		void SetFCLCutOff(ZDateTime value);
		void SetLCLCutOff(ZDateTime value);
		void SetDocsCutOff(ZDateTime value);
		void SetVGMCutOff(ZDateTime value);
		void SetAvailabilityDate(ZDateTime value);
		void SetLCLAvailabilityDate(ZDateTime value);
		void SetStorageDate(ZDateTime value);
		void SetLCLStorageDate(ZDateTime value);
		void SetLoadETA(ZDateTime value);
		void SetLoadATA(ZDateTime value);

		void SetOA_DepartureLocation(ZGuid value);
		void SetOA_ArrivalLocation(ZGuid value);

		void SetEmptyCutOff(ZDateTime value);
		void SetEmptyReceivalCommences(ZDateTime value);
		void SetDGCutOff(ZDateTime value);
		void SetDGReceivalCommences(ZDateTime value);
		void SetReeferCutOff(ZDateTime value);
		void SetReeferReceivalCommences(ZDateTime value);

		void SetServiceString(ZString value);

		void SetArrivalPortRouteId(ZString value);
		void SetDeparturePortRouteId(ZString value);

		void LogDateEvents(object sender);
	}
}
