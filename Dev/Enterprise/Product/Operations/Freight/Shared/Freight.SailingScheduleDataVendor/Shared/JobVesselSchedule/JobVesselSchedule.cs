using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class JobVesselSchedule : JobVesselScheduleBase
	{
		public JobVesselSchedule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			UpdateSailingSchedule();

			VoyageOrigin?.Voyage?.GeneratePossibleInvalidOrMissingSailingsIfRequired();
			VoyageDestination?.Voyage?.GeneratePossibleInvalidOrMissingSailingsIfRequired();
		}

		public override void OnSaved(bool saveSucceed)
		{
			base.OnSaved(saveSucceed);

			if (saveSucceed)
			{
				var originVoyage = VoyageOrigin?.Voyage;
				var destinationVoyage = VoyageDestination?.Voyage;

				if (originVoyage != null && !originVoyage.IsDeleted && !originVoyage.Sailings.Any() && originVoyage.AnySailingIsMissing())
				{
					ErrorReporter.ReportOnce("Voyage exists without Sailings in JobVesselSchedule OnSaved", "Saving this JobVesselSchedule will cause Voyage exists without Sailings");
				}
				if (destinationVoyage != null && originVoyage != destinationVoyage && !destinationVoyage.IsDeleted && !destinationVoyage.Sailings.Any() && destinationVoyage.AnySailingIsMissing())
				{
					ErrorReporter.ReportOnce("Voyage exists without Sailings in JobVesselSchedule OnSaved", "Saving this JobVesselSchedule will cause Voyage exists without Sailings");
				}
			}
		}

		void UpdateSailingSchedule()
		{
			UpdateVoyageOrigin();
			UpdateVoyageDestination();
		}

		void UpdateVoyageOrigin()
		{
			var originScheduleChangeEmailSupporter = VoyageOrigin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				UpdateAndLogDateChange(VoyageOrigin.JA_E_DEPInfo, VoyageOrigin.Voyage, EV_ETD, originScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.ETD);
				UpdateAndLogDateChange(VoyageOrigin.JA_A_DEPInfo, VoyageOrigin.Voyage, EV_ActualDeparture, originScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.ATD);
				UpdateAndLog(VoyageOrigin.JA_DepartReferenceInfo, VoyageOrigin.Voyage, EV_DataProviderReference);

				if (DepartureSailing != null)
				{
					UpdateAndLogDateChange(DepartureSailing.Origin.JA_ReceivalCommencesInfo, DepartureSailing.Origin.Voyage, EV_ExportReceivalCommencementDate, originScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.FCLReceivalCommences);
					UpdateAndLogDateChange(DepartureSailing.Origin.JA_CutOffInfo, DepartureSailing.Origin.Voyage, EV_CargoCuttOff, originScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.FCLCutOff);
				}
			}
		}

		void UpdateVoyageDestination()
		{
			var destinationScheduleChangeEmailSupporter = VoyageDestination as IScheduleChangeEmailSupporter;
			if (destinationScheduleChangeEmailSupporter != null)
			{
				UpdateAndLogDateChange(VoyageDestination.JB_E_ARVInfo, VoyageDestination.Voyage, EV_ETA, destinationScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.ETA);
				UpdateAndLogDateChange(VoyageDestination.JB_A_ARVInfo, VoyageDestination.Voyage, EV_ActualArrival, destinationScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.ATA);

				if (ArrivalSailing != null)
				{
					UpdateAndLogDateChange(ArrivalSailing.Destination.JB_AvailabilityDateInfo, ArrivalSailing.Destination.Voyage, EV_ImportAvailability, destinationScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.FCLAvailable);
					UpdateAndLogDateChange(ArrivalSailing.Destination.JB_StorageDateInfo, ArrivalSailing.Destination.Voyage, EV_ImportStorageCommences, destinationScheduleChangeEmailSupporter, ScheduleDateTypes.Codes.FCLStorage);
				}
			}
		}

		void UpdateAndLogDateChange(ZPropertyInfo propertyInfo, EnterpriseBusinessObject parentBizObj, ZDateTime newValue, IScheduleChangeEmailSupporter scheduleChangeEmailSupporter, string dateType)
		{
			UpdateAndLog(propertyInfo, parentBizObj, newValue);
			scheduleChangeEmailSupporter.AddOrUpdateJobScheduleChange(dateType, EV_DataProvider);
		}

		void UpdateAndLog(ZPropertyInfo propertyInfo, EnterpriseBusinessObject parentBizObj, IZType newValue)
		{
			IZType previousValue = propertyInfo.Value;

			if (!newValue.IsEmpty && previousValue.CompareTo(newValue) != 0)
			{
				propertyInfo.Value = newValue;

				ZString logNewValue = newValue is ZDateTime ? ((ZDateTime)newValue).ToLongTimeString() : newValue.ToString();
				ZString logOldValue = previousValue is ZDateTime ? ((ZDateTime)previousValue).ToLongTimeString() : previousValue.ToString();
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				parentBizObj.Logs.AddNew(Events.EditedARecord, string.Format("Updated the {0} from '{1}' to '{2}' from the {3} feed.", propertyInfo.HumanReadableName, logOldValue, logNewValue, EV_DataProvider));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning restore IDE0058 // Expression value is never used
			}
		}

		#endregion

		#region Related Business Objects

		JobVoyage Voyage(ZString lloyds, ZString voyageNo, ZString lineOperator)
		{
			JobVoyage voyage = null;
			if (!lloyds.IsEmpty)
			{
				var vessels = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloyds)).Select((v) => v.RV_Name);

				var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageNo);
				query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Constants.TransportModes.Sea);
				query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, vessels);

				var voyages = Factory.Load<JobVoyage>(query);

				var lineOperatorPK = SailingScheduleHelper.GetLineOperatorPKFromExternalCode(lineOperator, EV_DataProvider, Factory);
				voyage = voyages.Where((v) => v.JV_OH_Line == lineOperatorPK)
					.OrderByDescending((v) => v.JV_OH_Line)
					.FirstOrDefault();
			}

			return voyage;
		}

		public VoyageOrigin VoyageOrigin
		{
			get
			{
				JobVoyage voyageOutOfOrigin;

				if (!voyageOriginPopulated && (voyageOutOfOrigin = Voyage(EV_IMOLloydsNumber, EV_ShipOperatorVoyageOut, EV_LineOperator)) != null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(JobVoyOriginSchema.JA_JV, voyageOutOfOrigin.PK);
					query.AddToFilter(JoinCondition.And, JobVoyOriginSchema.JA_RL_NKPortOfLoading, EV_RL_NKPortCode);
					voyageOrigin = Factory.LoadTop1<VoyageOrigin>(query);
					voyageOriginPopulated = true;
				}
				return voyageOrigin;
			}
		}
		VoyageOrigin voyageOrigin;
		bool voyageOriginPopulated;

		public VoyageDestination VoyageDestination
		{
			get
			{
				JobVoyage voyageIntoDestination;

				if (!voyageDestinationPopulated && (voyageIntoDestination = Voyage(EV_IMOLloydsNumber, EV_ShipOperatorVoyageIn, EV_LineOperator)) != null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(JobVoyDestinationSchema.JB_JV, voyageIntoDestination.PK);
					query.AddToFilter(JoinCondition.And, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, EV_RL_NKPortCode);
					voyageDestination = Factory.LoadTop1<VoyageDestination>(query);
					voyageDestinationPopulated = true;
				}
				return voyageDestination;
			}
		}
		VoyageDestination voyageDestination;
		bool voyageDestinationPopulated;

		JobSailing DepartureSailing
		{
			get { return (VoyageOrigin == null) ? null : Factory.LoadTop1<JobSailing>(new ZQuery(JobSailingSchema.JX_JA, VoyageOrigin.PK)); }
		}

		JobSailing ArrivalSailing
		{
			get { return (VoyageDestination == null) ? null : Factory.LoadTop1<JobSailing>(new ZQuery(JobSailingSchema.JX_JB, VoyageDestination.PK)); }
		}

		#endregion

		protected override bool SupportsCloneCore()
		{
			return true;
		}
	}
}
