using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business.Testing
{
	class MockISailing : ISailingManaged
	{
		public MockISailing(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			this.TransportMode = Core.Constants.TransportModes.Sea;
			this.IsPublished = true;
			this.AllowScheduleCreation = true;
			IsImportingData = true;
		}

		#region ISailingManaged Members

		public bool AllowScheduleCreation { get; set; }
		public bool AllowScheduleDatesChanging { get; set; }

		public BusinessObjectFactory Factory { get; set; }
		public ZString Load { get; set; }
		public ZString Discharge { get; set; }
		public ZString Vessel { get; set; }
		public ZString Voyage { get; set; }

		public ZDateTime ATD { get; set; }
		public ZDateTime ATA { get; set; }
		public ZDateTime ETD { get; set; }
		public ZDateTime ETA { get; set; }
		public ZDateTime STD { get; set; }
		public ZDateTime STA { get; set; }
		public ZGuid ShippingLine { get; set; }
		public ZString TransportMode { get; set; }
		public ZGuid SailingPK { get; set; }
		public ZBool IsCharter { get; set; }
		public ZString RegistrationNo { get; set; }
		public ZBool IsPublished { get; set; }
		public ZBool IsCargoOnly { get; set; }
		public ZBool IsImportingData { get; set; }
		public ZString AircraftType { get; set; }
		public ZString OnlineScheduleStatus { get; set; }

		public JobSailingValidation SailingAdditionalValidation { get; set; }

		public JobVoyOriginValidation VoyOriginAdditionalValidation { get; set; }

		public JobVoyDestinationValidation VoyDestinationAdditionalValidation { get; set; }

		public bool HasChanges
		{
			get { return false; }
			set { }
		}

		public void SuspendValidation()
		{
		}

		public void ResumeValidation()
		{
		}

		public void SetFCLReceivalCommences(ZDateTime value) { }

		public void SetLCLReceivalCommences(ZDateTime value) { }

		public void SetFCLCutOff(ZDateTime value) { }

		public void SetLCLCutOff(ZDateTime value) { }

		public void SetDocsCutOff(ZDateTime value) { }

		public void SetVGMCutOff(ZDateTime value) { }

		public void SetAvailabilityDate(ZDateTime value) { }

		public void SetLCLAvailabilityDate(ZDateTime value) { }

		public void SetStorageDate(ZDateTime value) { }

		public void SetLCLStorageDate(ZDateTime value) { }

		public void SetLoadETA(ZDateTime value) { }

		public void SetLoadATA(ZDateTime value) { }

		public void SetOA_DepartureLocation(ZGuid value) { }

		public void SetOA_ArrivalLocation(ZGuid value) { }

		public void SetEmptyCutOff(ZDateTime value) { }

		public void SetEmptyReceivalCommences(ZDateTime value) { }

		public void SetDGCutOff(ZDateTime value) { }

		public void SetDGReceivalCommences(ZDateTime value) { }

		public void SetReeferCutOff(ZDateTime value) { }

		public void SetReeferReceivalCommences(ZDateTime value) { }

		public void SetServiceString(ZString value) { }

		public void SetCO2ePerTonneInKg(ZDecimal value) { }

		public void SetCO2eStatus(ZString value) { }

		public void SetCO2eDistanceInKM(ZDecimal value) { }

		void ISailingManaged.LogDateEvents(object sender) { }

		void ISailingManaged.RegisterEditableChildObject(IBusiness bizO)
		{
		}

		void ISailingManaged.UnRegisterEditableChildObject(IBusiness bizO)
		{
		}

		void ISailingManaged.SetArrivalPortRouteId(ZString value) { }

		void ISailingManaged.SetDeparturePortRouteId(ZString value) { }

		#endregion
	}
}
