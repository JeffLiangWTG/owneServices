using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Integration;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(VesselScheduleProcessor))]

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselScheduleProcessor : OneStopProcessorBase
	{
		#region Process

		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^Enterprise Vessel Schedule$")]
		public bool ProcessVesselSchedule(ZGuid mailItemPK, ILogger logger)
		{
			PopulateObsoleteOneStopRecordsList();

			using (Env.Instance.SuspendBranchAccessError())
			{
				if (ProcessMailItem(mailItemPK, logger))
				{
					DeleteObsoleteOneStopRecords();
					RemoveOutdatedRecords();
					return true;
				}
			}

			return false;
		}

		protected override void ProcessRecords(List<string[]> records)
		{
			LoadOneStopVesselSchedulesIntoFactory();

			var validRecords = records.Select(x => new VesselScheduleLineRecord(x))
				.Where(x => x.IsValid() && DateTimeIsWithinRange(x));
			var processedResults = new VesselScheduleProcessedResults();
			foreach (var vesselScheduleLine in validRecords)
			{
				var vesselSchedule = GetExistingJobVesselSchedule(vesselScheduleLine);
				if (vesselSchedule == null)
				{
					IncreaseAddedRecords(JobVesselScheduleSchema.Constants.TableName);
					vesselSchedule = InsertRecord(vesselScheduleLine);
					vesselScheduleLine.IsNewRecord = true;
				}
				else
				{
					UpdateVesselSchedule(vesselSchedule, vesselScheduleLine);
					ObsoleteOneStopRecords.Remove(vesselSchedule.PK);
				}
				processedResults.AddOrUpdate(vesselSchedule, vesselScheduleLine);
			}

			RejectInvalidVesselSchedules(processedResults);

			ZExceptionReporting.ProcessWithSaveExceptionHandling(FactoryProvider.SaveCurrentAndCreateNew, null, true);
		}

		#endregion

		#region Obsolete and Outdated records

		void PopulateObsoleteOneStopRecordsList()
		{
			var sql = @"
				SELECT EV_PK AS PK
				FROM dbo.JobVesselSchedule
				WHERE EV_DataProvider = @OneStopDataProvider
				AND (EV_ETD >= @ETDFuture
					OR EV_ETA >= @ETAFuture)";

			var parameters = new ZSqlParameterCollection();
			var futureDate = ZDateTime.Today.AddDays(1);

			parameters.Add("@OneStopDataProvider", FreightConstants.VesselDataProviders.OneStop, JobVesselScheduleSchema.EV_DataProvider);
			parameters.Add("@ETDFuture", futureDate, JobVesselScheduleSchema.EV_ETD);
			parameters.Add("@ETAFuture", futureDate, JobVesselScheduleSchema.EV_ETA);

			var dynamicBizObjs = new DynamicBusinessObjectCollection(Factory);
			dynamicBizObjs.Load(sql, parameters);

			ObsoleteOneStopRecords = dynamicBizObjs.Cast<DynamicBusinessObject>().Select(record => new ZGuid(record["PK"])).ToList();
		}

		List<ZGuid> ObsoleteOneStopRecords = new List<ZGuid>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteObsoleteOneStopRecords()
		{
			const int batchSize = 50;

			string paramName;
			var paramNamesStringBuilder = new ZStringBuilder();
			var paramsList = new List<ZSqlParameter>();
			var totalRecords = ObsoleteOneStopRecords.Count;

			var i = 0;
			foreach (var pk in ObsoleteOneStopRecords)
			{
				paramName = FormattableString.Invariant($"@pk{i % batchSize}");
				paramNamesStringBuilder.Append(paramName);
				paramsList.Add(ZSqlParameter.New(paramName, pk, JobVesselScheduleSchema.PK));

				i++;

				if (i % batchSize == 0 || i == totalRecords)
				{
					var sql = FormattableString.Invariant($"DELETE FROM dbo.JobVesselSchedule WHERE EV_PK IN ({paramNamesStringBuilder.ToStringWithDelimiterBetweenAppends(",")})");
					var command = Db.Connection.Command(sql); // Deleting potentially large number of obsolete records
					command.AddParameters(paramsList.ToArray());
					command.ExecuteNonQuery();

					paramsList.Clear();
					paramNamesStringBuilder.Clear();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RemoveOutdatedRecords()
		{
			var sql = $@"
DELETE FROM dbo.JobVesselSchedule
WHERE EV_DataProvider = @dataProvider
AND
(
	ISNULL(EV_ActualArrival, EV_ETA) < @oldestArrivalDate
	OR
	ISNULL(EV_ActualArrival, EV_ETA) IS NULL AND ISNULL(EV_ActualDeparture, EV_ETD) < @oldestDepartureDate
)";

			var command = Db.Connection.Command(sql);
			command.AddParameterBasedOnDbColumn("@dataProvider", DataProvider, JobVesselScheduleSchema.EV_DataProvider);
			command.AddParameter("@oldestArrivalDate", SqlDbType.DateTime, oldestArrivalDate.ToDateTime());
			command.AddParameter("@oldestDepartureDate", SqlDbType.DateTime, oldestDepartureDate.ToDateTime());
			command.ExecuteNonQuery();
		}

		#endregion

		#region Implementation

		void LoadOneStopVesselSchedulesIntoFactory()
		{
			Factory.Load<JobVesselSchedule>(new ZQuery(JobVesselScheduleSchema.EV_DataProvider, DataProvider));
		}

		bool DateTimeIsWithinRange(VesselScheduleLineRecord vesselScheduleLine)
		{
			var arrivalDate = vesselScheduleLine.ActualArrival.IsValid ? vesselScheduleLine.ActualArrival : vesselScheduleLine.ETA;
			if (arrivalDate.IsValid && arrivalDate >= oldestArrivalDate)
			{
				return true;
			}

			if (!arrivalDate.IsValid)
			{
				var departureDate = vesselScheduleLine.ActualDepart.IsValid ? vesselScheduleLine.ActualDepart : vesselScheduleLine.ETD;
				return departureDate.IsValid && departureDate >= oldestDepartureDate;
			}

			return false;
		}

		JobVesselSchedule GetExistingJobVesselSchedule(VesselScheduleLineRecord vesselScheduleLine)
		{
			var query = new ZQuery();
			query.AddToFilter(JobVesselScheduleSchema.EV_ShipOperatorVoyageIn, vesselScheduleLine.ShipOperatorVoyageIn);
			query.AddToFilter(JobVesselScheduleSchema.EV_ShipOperatorVoyageOut, vesselScheduleLine.ShipOperatorVoyageOut);
			query.AddToFilter(JobVesselScheduleSchema.EV_IMOLloydsNumber, vesselScheduleLine.LloydsID);
			query.AddToFilter(JobVesselScheduleSchema.EV_LineOperator, vesselScheduleLine.LineOperator);
			query.AddToFilter(JobVesselScheduleSchema.EV_TerminalID, vesselScheduleLine.TerminalID);
			query.AddToFilter(JobVesselScheduleSchema.EV_DataProvider, DataProvider);

			query.FetchOnlyFromLocalCache = true;

			return Factory.LoadTop1<JobVesselSchedule>(query);
		}

		JobVesselSchedule InsertRecord(VesselScheduleLineRecord vesselScheduleLine)
		{
			var vesselSchedule = Factory.New<JobVesselSchedule>();

			vesselSchedule.EV_TerminalID = vesselScheduleLine.TerminalID;
			vesselSchedule.EV_IMOLloydsNumber = vesselScheduleLine.LloydsID;
			vesselSchedule.EV_ShipOperatorVoyageIn = vesselScheduleLine.ShipOperatorVoyageIn;
			vesselSchedule.EV_ShipOperatorVoyageOut = vesselScheduleLine.ShipOperatorVoyageOut;
			vesselSchedule.EV_LineOperator = vesselScheduleLine.LineOperator;
			vesselSchedule.EV_DataProvider = DataProvider;

			UpdateVesselScheduleObject(vesselSchedule, vesselScheduleLine);
			return vesselSchedule;
		}

		void UpdateVesselSchedule(JobVesselSchedule vesselSchedule, VesselScheduleLineRecord vesselScheduleLine)
		{
			if (RecordAndObjectDataDiffer(vesselSchedule, vesselScheduleLine))
			{
				IncreaseUpdatedRecords(JobVesselScheduleSchema.Constants.TableName);
				UpdateVesselScheduleObject(vesselSchedule, vesselScheduleLine);
			}
		}

		bool RecordAndObjectDataDiffer(JobVesselSchedule vesselSchedule, VesselScheduleLineRecord vesselScheduleLine)
		{
			return vesselSchedule.EV_RL_NKPortCode != TrimToMaxLength(JobVesselScheduleSchema.EV_RL_NKPortCode, vesselScheduleLine.UNLOCO)
				|| vesselSchedule.EV_ETA != vesselScheduleLine.ETA
				|| vesselSchedule.EV_ETD != vesselScheduleLine.ETD
				|| vesselSchedule.EV_ShipName != TrimToMaxLength(JobVesselScheduleSchema.EV_ShipName, vesselScheduleLine.ShipName)
				|| vesselSchedule.EV_CargoCuttOff != vesselScheduleLine.CargoCutoff
				|| vesselSchedule.EV_ReeferCutOff != vesselScheduleLine.ReeferCutoff
				|| vesselSchedule.EV_OperatorsDescription != TrimToMaxLength(JobVesselScheduleSchema.EV_OperatorsDescription, vesselScheduleLine.OperatorDescription)
				|| vesselSchedule.EV_ShipOperatorsCode != TrimToMaxLength(JobVesselScheduleSchema.EV_ShipOperatorsCode, vesselScheduleLine.ShipOperatorCode)
				|| vesselSchedule.EV_ExportReceivalCommencementDate != vesselScheduleLine.ExportReceivalCommencement
				|| vesselSchedule.EV_ImportAvailability != vesselScheduleLine.ImportAvailability
				|| vesselSchedule.EV_ImportStorageCommences != vesselScheduleLine.ImportStorage
				|| vesselSchedule.EV_ContainerVessel != TrimToMaxLength(JobVesselScheduleSchema.EV_ContainerVessel, vesselScheduleLine.ContainerVessel)
				|| vesselSchedule.EV_ActualArrival != vesselScheduleLine.ActualArrival
				|| vesselSchedule.EV_ActualDeparture != vesselScheduleLine.ActualDepart
				|| vesselSchedule.EV_VesselCode != TrimToMaxLength(JobVesselScheduleSchema.EV_VesselCode, vesselScheduleLine.VesselCode);
		}

		void UpdateVesselScheduleObject(JobVesselSchedule vesselSchedule, VesselScheduleLineRecord vesselScheduleLine)
		{
			vesselSchedule.EV_ETA = vesselScheduleLine.ETA;
			vesselSchedule.EV_ETD = vesselScheduleLine.ETD;
			vesselSchedule.EV_CargoCuttOff = vesselScheduleLine.CargoCutoff;
			vesselSchedule.EV_ReeferCutOff = vesselScheduleLine.ReeferCutoff;
			vesselSchedule.EV_ExportReceivalCommencementDate = vesselScheduleLine.ExportReceivalCommencement;
			vesselSchedule.EV_ImportAvailability = vesselScheduleLine.ImportAvailability;
			vesselSchedule.EV_ImportStorageCommences = vesselScheduleLine.ImportStorage;
			vesselSchedule.EV_ActualArrival = vesselScheduleLine.ActualArrival;
			vesselSchedule.EV_ActualDeparture = vesselScheduleLine.ActualDepart;

			vesselSchedule.EV_RL_NKPortCode = TrimToMaxLength(JobVesselScheduleSchema.EV_RL_NKPortCode, vesselScheduleLine.UNLOCO);
			vesselSchedule.EV_ShipName = TrimToMaxLength(JobVesselScheduleSchema.EV_ShipName, vesselScheduleLine.ShipName);
			vesselSchedule.EV_OperatorsDescription = TrimToMaxLength(JobVesselScheduleSchema.EV_OperatorsDescription, vesselScheduleLine.OperatorDescription);
			vesselSchedule.EV_ShipOperatorsCode = TrimToMaxLength(JobVesselScheduleSchema.EV_ShipOperatorsCode, vesselScheduleLine.ShipOperatorCode);
			vesselSchedule.EV_ContainerVessel = TrimToMaxLength(JobVesselScheduleSchema.EV_ContainerVessel, vesselScheduleLine.ContainerVessel);
			vesselSchedule.EV_VesselCode = TrimToMaxLength(JobVesselScheduleSchema.EV_VesselCode, vesselScheduleLine.VesselCode);
		}

		void RejectInvalidVesselSchedules(VesselScheduleProcessedResults processedResults)
		{
			var voyageValidationCache = new Dictionary<ZGuid, bool>();
			foreach (var vesselSchedule in processedResults.VesselSchedules)
			{
				if (IsETDValidForUpdatingOrigin(vesselSchedule.EV_ETD, vesselSchedule.VoyageOrigin, voyageValidationCache)
					&& IsETAValidForUpdatingDestination(vesselSchedule.EV_ETA, vesselSchedule.VoyageDestination, voyageValidationCache))
				{
					continue;
				}
				vesselSchedule.CancelChanges();
				foreach (var line in processedResults.LineRecords(vesselSchedule.PK))
				{
					IncreaseRejectedRecords(JobVesselScheduleSchema.Constants.TableName);
					if (line.IsNewRecord)
					{
						DecreaseAddedRecords(JobVesselScheduleSchema.Constants.TableName);
					}
					else
					{
						DecreaseUpdatedRecords(JobVesselScheduleSchema.Constants.TableName);
					}
				}
			}
		}

		bool IsETDValidForUpdatingOrigin(ZDateTime etd, VoyageOrigin origin, Dictionary<ZGuid, bool> voyageValidationCache)
		{
			if (origin == null)
			{
				return true;
			}
			var voyage = origin.Voyage;
			if (voyageValidationCache.TryGetValue(voyage.PK, out var cachedResult))
			{
				return cachedResult;
			}
			var etdUTC = etd.IsValid ? Env.Time.GetUtcFromUnlocoTime(origin.JA_RL_NKPortOfLoading, etd.ToDateTime()) : ZDateTime.Empty;
			var result = voyage.Destinations.Cast<VoyageDestination>().Any(destination =>
				JobVoyageSailingsHelper.PortPairShouldCreateSailing(origin, destination, voyage.JV_AirSeaRoad, null, null, etd, etdUTC));
			voyageValidationCache.Add(voyage.PK, result);
			return result;
		}

		bool IsETAValidForUpdatingDestination(ZDateTime eta, VoyageDestination destination, Dictionary<ZGuid, bool> voyageValidationCache)
		{
			if (destination == null)
			{
				return true;
			}
			var voyage = destination.Voyage;
			if (voyageValidationCache.TryGetValue(voyage.PK, out var cachedResult))
			{
				return cachedResult;
			}
			var etaUTC = eta.IsValid ? Env.Time.GetUtcFromUnlocoTime(destination.JB_RL_NKPortOfDischarge, eta.ToDateTime()) : ZDateTime.Empty;
			var result = voyage.Origins.Cast<VoyageOrigin>().Any(origin =>
				JobVoyageSailingsHelper.PortPairShouldCreateSailing(origin, destination, voyage.JV_AirSeaRoad, eta, etaUTC, null, null));
			voyageValidationCache.Add(voyage.PK, result);
			return result;
		}

		readonly struct VesselScheduleProcessedResults
		{
			public VesselScheduleProcessedResults()
			{
				innerDict = [];
				VesselSchedules = [];
			}

			readonly Dictionary<ZGuid, List<VesselScheduleLineRecord>> innerDict;
			public HashSet<JobVesselSchedule> VesselSchedules { get; }

			public void AddOrUpdate(JobVesselSchedule vesselSchedule, VesselScheduleLineRecord lineRecord)
			{
				if (innerDict.TryGetValue(vesselSchedule.PK, out var lineList))
				{
					lineList.Add(lineRecord);
				}
				else
				{
					innerDict.Add(vesselSchedule.PK, [lineRecord]);
				}
				VesselSchedules.Add(vesselSchedule);
			}

			public List<VesselScheduleLineRecord> LineRecords(ZGuid pk)
			{
				if (innerDict.TryGetValue(pk, out var lineList))
				{
					return lineList;
				}
				return [];
			}
		}

		#endregion
	}
}
