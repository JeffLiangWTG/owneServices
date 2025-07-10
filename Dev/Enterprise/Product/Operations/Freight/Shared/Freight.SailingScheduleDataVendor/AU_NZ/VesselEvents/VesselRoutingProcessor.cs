using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.Integration;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(VesselRoutingProcessor))]

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselRoutingProcessor : OneStopProcessorBase
	{
		#region Process Vessel Routing

		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^Enterprise Vessel Routing$")]
		public bool ProcessVesselRouting(ZGuid mailItemPK, ILogger logger)
		{
			if (ProcessMailItem(mailItemPK, logger))
			{
				RemoveOutdatedRecords();
				return true;
			}

			return false;
		}

		protected override void ProcessRecords(List<string[]> records)
		{
			LoadOneStopVesselRoutingsIntoFactory();
			BuildRelatedVesselScheduleList();

			var validRecords = records.Select(x => new VesselRoutingLineRecord(x))
				.Where(x => x.IsValid() && HasRelatedVesselSchedule(x));
			foreach (var vesselRoutingLine in validRecords)
			{
				var vesselRoute = GetExistingJobVesselRouting(vesselRoutingLine);
				if (vesselRoute == null)
				{
					IncreaseAddedRecords(JobVesselRoutingSchema.Constants.TableName);
					InsertRecord(vesselRoutingLine);
				}
				else
				{
					UpdateVesselRoute(vesselRoute, vesselRoutingLine);
				}
			}

			FactoryProvider.SaveCurrentAndCreateNew();
		}

		#endregion

		#region RemoveOutdatedRecords

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RemoveOutdatedRecords()
		{
			var sql = @"
DELETE dbo.JobVesselRouting
FROM dbo.JobVesselRouting
LEFT JOIN dbo.JobVesselSchedule ON EV_IMOLloydsNumber = E1_LloydsID
	AND (EV_ShipOperatorVoyageIn = E1_VoyageNumber OR EV_ShipOperatorVoyageOut = E1_VoyageNumber)
	AND EV_DataProvider = @dataProvider
WHERE EV_PK IS NULL
AND E1_DataProviderReference = ''";

			var command = Db.Connection.Command(sql); // delete obsolete JobVesselRouting
			command.AddParameterBasedOnDbColumn("@dataProvider", DataProvider, JobVesselScheduleSchema.EV_DataProvider);
			command.ExecuteNonQuery();
		}

		#endregion

		#region Implementation

		void LoadOneStopVesselRoutingsIntoFactory()
		{
			// Empty VesselSchedule and DataProviderReference indicates One-Stop record
			var query = new ZQuery(JobVesselRoutingSchema.E1_EV, null);
			query.AddToFilter(JobVesselRoutingSchema.E1_DataProviderReference, string.Empty);
			Factory.Load<JobVesselRouting>(query);
		}

		#region Related Vessel Schedules

		void BuildRelatedVesselScheduleList()
		{
			var sql = @"
				SELECT DISTINCT
					EV_IMOLloydsNumber AS LloydsID,
					EV_ShipOperatorVoyageIn AS VoyageIn,
					EV_ShipOperatorVoyageOut AS VoyageOut
				FROM dbo.JobVesselSchedule
				WHERE EV_DataProvider = @OneStopDataProvider";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@OneStopDataProvider", FreightConstants.VesselDataProviders.OneStop, JobVesselScheduleSchema.EV_DataProvider);

			var dynamicBizObjs = new DynamicBusinessObjectCollection(Factory);
			dynamicBizObjs.Load(sql, parameters);

			relatedVesselSchedules = new HashSet<string>();
			dynamicBizObjs.ForEach(x => relatedVesselSchedules.Add(FormattableString.Invariant($"{x["LloydsID"]}|{x["VoyageIn"]}")));
			dynamicBizObjs.ForEach(x => relatedVesselSchedules.Add(FormattableString.Invariant($"{x["LloydsID"]}|{x["VoyageOut"]}")));
		}

		HashSet<string> relatedVesselSchedules;

		bool HasRelatedVesselSchedule(VesselRoutingLineRecord vesselRoutingLine)
		{
			return relatedVesselSchedules.Contains(FormattableString.Invariant($"{vesselRoutingLine.LloydsID}|{vesselRoutingLine.VoyageNumber}"));
		}

		#endregion

		JobVesselRouting GetExistingJobVesselRouting(VesselRoutingLineRecord vesselRoutingLine)
		{
			var query = new ZQuery();
			query.AddToFilter(JobVesselRoutingSchema.E1_VoyageNumber, vesselRoutingLine.VoyageNumber);
			query.AddToFilter(JobVesselRoutingSchema.E1_LloydsID, vesselRoutingLine.LloydsID);
			query.AddToFilter(JobVesselRoutingSchema.E1_RL_NKDischargePortCode, vesselRoutingLine.DischargePortCode);
			query.AddToFilter(JobVesselRoutingSchema.E1_TerminalCode, vesselRoutingLine.TerminalCode);
			query.AddToFilter(JobVesselRoutingSchema.E1_EV, null);

			query.FetchOnlyFromLocalCache = true;

			return Factory.LoadTop1<JobVesselRouting>(query);
		}

		void InsertRecord(VesselRoutingLineRecord vesselRoutingLine)
		{
			var vesselRoute = Factory.New<JobVesselRouting>();

			vesselRoute.E1_TerminalCode = vesselRoutingLine.TerminalCode;
			vesselRoute.E1_LloydsID = vesselRoutingLine.LloydsID;
			vesselRoute.E1_VoyageNumber = vesselRoutingLine.VoyageNumber;
			vesselRoute.E1_RL_NKDischargePortCode = vesselRoutingLine.DischargePortCode;

			UpdateVesselRouteObject(vesselRoute, vesselRoutingLine);
		}

		void UpdateVesselRoute(JobVesselRouting vesselRoute, VesselRoutingLineRecord vesselRoutingLine)
		{
			if (RecordAndObjectDataDiffer(vesselRoute, vesselRoutingLine))
			{
				IncreaseUpdatedRecords(JobVesselRoutingSchema.Constants.TableName);
				UpdateVesselRouteObject(vesselRoute, vesselRoutingLine);
			}
		}

		bool RecordAndObjectDataDiffer(JobVesselRouting vesselRoute, VesselRoutingLineRecord vesselRoutingLine)
		{
			return vesselRoute.E1_TerminalName != TrimToMaxLength(JobVesselRoutingSchema.E1_TerminalName, vesselRoutingLine.TerminalName)
				|| vesselRoute.E1_ShipName != TrimToMaxLength(JobVesselRoutingSchema.E1_ShipName, vesselRoutingLine.ShipName)
				|| vesselRoute.E1_DischargeCountry != TrimToMaxLength(JobVesselRoutingSchema.E1_DischargeCountry, vesselRoutingLine.DischargeCountry)
				|| vesselRoute.E1_DischargePortName != TrimToMaxLength(JobVesselRoutingSchema.E1_DischargePortName, vesselRoutingLine.DischargePortName)
				|| vesselRoute.E1_DischargePortState != TrimToMaxLength(JobVesselRoutingSchema.E1_DischargePortState, vesselRoutingLine.DischargePortState);
		}

		void UpdateVesselRouteObject(JobVesselRouting vesselRoute, VesselRoutingLineRecord vesselRoutingLine)
		{
			vesselRoute.E1_TerminalName = TrimToMaxLength(JobVesselRoutingSchema.E1_TerminalName, vesselRoutingLine.TerminalName);
			vesselRoute.E1_ShipName = TrimToMaxLength(JobVesselRoutingSchema.E1_ShipName, vesselRoutingLine.ShipName);
			vesselRoute.E1_DischargeCountry = TrimToMaxLength(JobVesselRoutingSchema.E1_DischargeCountry, vesselRoutingLine.DischargeCountry);
			vesselRoute.E1_DischargePortName = TrimToMaxLength(JobVesselRoutingSchema.E1_DischargePortName, vesselRoutingLine.DischargePortName);
			vesselRoute.E1_DischargePortState = TrimToMaxLength(JobVesselRoutingSchema.E1_DischargePortState, vesselRoutingLine.DischargePortState);
			vesselRoute.E1_EV = ZGuid.Empty;
		}

		#endregion
	}
}
