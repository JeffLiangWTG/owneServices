using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalSchedule = Enterprise.UniversalDataBuss.DataObjects.Universal.Schedule;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.Freight.SailingScheduleDataVendor
{
	class UniversalScheduleImporter : IUniversalScheduleImporter
	{
		public UniversalScheduleImporter(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.Factory = factory;
			this.Logger = logger;
		}

		readonly BusinessObjectFactory Factory;
		readonly IXmlImportLogger Logger;

		public MessageStatus ImportUniversalSchedule(ITopLevelDataObject dataObject)
		{
			var result = MessageStatus.Discarded;

			var schedule = dataObject as UniversalSchedule;
			if (schedule != null)
			{
				if (IsValidSchedule(schedule))
				{
					var (vesselSchedulesWithETAChanges, vesselSchedulesWithETDChanges) = ProcessSchedule(schedule);
					if (vesselSchedulesWithETAChanges.Count > 0 || vesselSchedulesWithETDChanges.Count > 0)
					{
						ValidateVoyageSailings(vesselSchedulesWithETAChanges, vesselSchedulesWithETDChanges);
					}
					result = MessageStatus.Processed;
				}
			}

			return result;
		}

		bool IsValidDataProvider(ZString? dataProvider)
		{
			if (dataProvider.HasValue)
			{
				switch (dataProvider.Value)
				{
					case FreightConstants.VesselDataProviders.DAKOSY:
						return true;
					case FreightConstants.VesselDataProviders.DBH:
						return true;
					case FreightConstants.VesselDataProviders.OneStop:
						return true;
				}
			}

			return false;
		}

		bool IsValidSchedule(UniversalSchedule schedule)
		{
			var dataProviderIsValid = IsValidDataProvider(schedule.DataProvider);
			var voyageNumberIsValid = !string.IsNullOrEmpty(schedule.Transport.Sea.VoyageNumber);
			var vesselDetailsAreValid = !(string.IsNullOrEmpty(schedule.Transport.Sea.Vessel.VesselName) && string.IsNullOrEmpty(schedule.Transport.Sea.Vessel.LloydsNumber));

			if (!dataProviderIsValid)
			{
				Logger.LogBoth(LogType.Error, Res.GetString("1a1dfaca-3f02-40e3-9431-1efa3b3b6c63", "Data Provider is invalid. Should be {0}, {1} or {2}",
							FreightConstants.VesselDataProviders.OneStop,
							FreightConstants.VesselDataProviders.DBH,
							FreightConstants.VesselDataProviders.DAKOSY));
			}

			if (!voyageNumberIsValid)
			{
				throw new DataObjectReadFailureException(ResString.GetMultilingualString("c92d5e27-35e3-4d37-8593-22e77c2b8fdd", "Voyage Number is empty."));
			}

			if (!vesselDetailsAreValid)
			{
				throw new DataObjectReadFailureException(ResString.GetMultilingualString("6ab25080-744f-4976-b17c-1e5e9da47631", "Vessel Name and Lloyds Number are empty."));
			}

			return dataProviderIsValid && voyageNumberIsValid && vesselDetailsAreValid;
		}

		(List<JobVesselSchedule>, List<JobVesselSchedule>) ProcessSchedule(UniversalSchedule schedule)
		{
			bool hasDischargeCollection = schedule.DischargeCollection != null;
			bool hasLoadingCollection = schedule.LoadingCollection != null;

			if (schedule.IsCancellation.GetValueOrDefault())
			{
				if (hasDischargeCollection)
				{
					CancelSchedule(schedule, isLoading: false);
				}
				if (hasLoadingCollection)
				{
					CancelSchedule(schedule, isLoading: true);
				}

				return (new List<JobVesselSchedule>(), new List<JobVesselSchedule>());
			}

			DeleteNonIncludedPorts(schedule);

			var vesselSchedulesWithETAChanges = new List<JobVesselSchedule>();
			var vesselSchedulesWithETDChanges = new List<JobVesselSchedule>();
			if (hasDischargeCollection)
			{
				foreach (var dischargePort in schedule.DischargeCollection)
				{
					vesselSchedulesWithETAChanges.Add(MapVesselScheduleFromDischarge(dischargePort, schedule));
				}
			}
			if (hasLoadingCollection)
			{
				foreach (var loadingPort in schedule.LoadingCollection)
				{
					vesselSchedulesWithETDChanges.Add(MapVesselScheduleFromLoading(loadingPort, schedule));
				}
			}
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;

			return (vesselSchedulesWithETAChanges, vesselSchedulesWithETDChanges);
		}

		static void ValidateVoyageSailings(List<JobVesselSchedule> vesselSchedulesWithETAChanges, List<JobVesselSchedule> vesselSchedulesWithETDChanges)
		{
			var voyagesToValidate = new List<JobVoyage>();
			var updatedETAs = new Dictionary<ZGuid, ZDateTime>();
			var updatedETDs = new Dictionary<ZGuid, ZDateTime>();

			foreach (var vesselScheduleWithETAChange in vesselSchedulesWithETAChanges)
			{
				var voyageDestination = vesselScheduleWithETAChange?.VoyageDestination;
				if (voyageDestination != null)
				{
					updatedETAs[voyageDestination.PK] = vesselScheduleWithETAChange.EV_ETA;

					if (voyageDestination.Voyage != null)
					{
						voyagesToValidate.Add(voyageDestination.Voyage);
					}
				}
			}

			foreach (var vesselScheduleWithETDChange in vesselSchedulesWithETDChanges)
			{
				var voyageOrigin = vesselScheduleWithETDChange?.VoyageOrigin;
				if (voyageOrigin != null)
				{
					updatedETDs[voyageOrigin.PK] = vesselScheduleWithETDChange.EV_ETD;

					if (voyageOrigin.Voyage != null)
					{
						voyagesToValidate.Add(voyageOrigin.Voyage);
					}
				}
			}

			foreach (var voyage in voyagesToValidate.Distinct())
			{
				var sailingsFoundForVoyage = false;
				foreach (VoyageOrigin origin in voyage.Origins)
				{
					foreach (VoyageDestination dest in voyage.Destinations)
					{
						ZDateTime? eta = updatedETAs.ContainsKey(dest.PK) ? updatedETAs[dest.PK] : null;
						ZDateTime? etd = updatedETDs.ContainsKey(origin.PK) ? updatedETDs[origin.PK] : null;
						ZDateTime? etaUtc = eta == null ? null : eta.Value.IsValid ? Env.Time.GetUtcFromUnlocoTime(dest.JB_RL_NKPortOfDischarge.ToString(), eta.Value.ToDateTime()) : ZDateTime.Empty;
						ZDateTime? etdUtc = etd == null ? null : etd.Value.IsValid ? Env.Time.GetUtcFromUnlocoTime(origin.JA_RL_NKPortOfLoading.ToString(), etd.Value.ToDateTime()) : ZDateTime.Empty;
						if (JobVoyageSailingsHelper.PortPairShouldCreateSailing(origin, dest, voyage.JV_AirSeaRoad, eta, etaUtc, etd, etdUtc))
						{
							sailingsFoundForVoyage = true;
						}
					}
				}

				if (!sailingsFoundForVoyage)
				{
					throw new DataObjectReadFailureException(ResString.GetMultilingualString(
						"aaf00261-abdb-4e3d-9ee9-2a0aa31fe541",
						"No sailings for voyage {0}. Please check {1} in {2} and {3} in {4}.",
						voyage.PK, (NoResString)"EstimatedArrival", (NoResString)"DischargeCollection", (NoResString)"EstimatedDeparture", (NoResString)"LoadingCollection"
						));
				}
			}
		}

		#region Cancel Schedule

		void CancelSchedule(UniversalSchedule schedule, bool isLoading)
		{
			var query = GetQueryForMatching(schedule, isLoading);
			var schedules = Factory.Load<JobVesselSchedule>(query);

			var collection = isLoading
				? schedule.LoadingCollection.ToList<IDataObject>()
				: schedule.DischargeCollection.ToList<IDataObject>();

			var schedulesToBeCancelled = new List<JobVesselSchedule>();

			foreach (var port in collection)
			{
				var portCode = isLoading ? ((Loading)port).Port.Code : ((Discharge)port).Port.Code;

				var matchedSchedules = schedules.Where(s => s.EV_RL_NKPortCode.Equals(portCode)).ToList();
				schedulesToBeCancelled.AddRange(matchedSchedules);
			}

			schedulesToBeCancelled.ForEach(s => s.Delete());
		}

		#endregion

		#region Map Schedule

		JobVesselSchedule MapVesselScheduleFromDischarge(Discharge discharge, UniversalSchedule schedule)
		{
			var vesselSchedule = CreateOrLoadVesselSchedule(schedule, discharge.Port.Code, false);
			var vesselScheduleColumnIndexer = (IColumnIndexer)((INeedRow)vesselSchedule).Row;
			var changed = false;
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_RL_NKPortCode, discharge.Port.Code);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ETA, discharge.EstimatedArrival);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ActualArrival, discharge.ActualArrival);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ShipOperatorVoyageIn, schedule.Transport.Sea.VoyageNumber);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ImportAvailability, discharge.FCLAvailability);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ImportStorageCommences, discharge.FCLStorage);

			changed |= MapScheduleVesselData(vesselScheduleColumnIndexer, schedule);
			changed |= MapCarrier(vesselScheduleColumnIndexer, schedule.Carrier);

			if (changed)
			{
				vesselSchedule.HasChanges = true;
			}

			return vesselSchedule;
		}

		JobVesselSchedule MapVesselScheduleFromLoading(Loading loading, UniversalSchedule schedule)
		{
			var vesselSchedule = CreateOrLoadVesselSchedule(schedule, loading.Port.Code, true);
			var vesselScheduleColumnIndexer = (IColumnIndexer)((INeedRow)vesselSchedule).Row;
			var changed = false;
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_RL_NKPortCode, loading.Port.Code);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_CargoCuttOff, loading.FCLCutOff);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ETD, loading.EstimatedDeparture);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ActualDeparture, loading.ActualDeparture);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ShipOperatorVoyageOut, schedule.Transport.Sea.VoyageNumber);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_DataProviderReference, loading.DepartureReference);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_TerminalID, loading.TerminalCode);
			changed |= SetValue(vesselScheduleColumnIndexer, JobVesselScheduleSchema.EV_ExportReceivalCommencementDate, loading.FCLReceivalCommences);

			changed |= MapScheduleVesselData(vesselScheduleColumnIndexer, schedule);
			changed |= MapCarrier(vesselScheduleColumnIndexer, schedule.Carrier);

			if (changed)
			{
				vesselSchedule.HasChanges = true;
			}

			return vesselSchedule;
		}

		bool MapScheduleVesselData(IColumnIndexer vesselSchedule, UniversalSchedule schedule)
		{
			var changed = false;
			changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_DataProvider, schedule.DataProvider);
			changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_ShipName, schedule.Transport.Sea.Vessel.VesselName);
			changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_VesselCode, schedule.Transport.Sea.Vessel.VesselName);
			changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_RadioCallSign, schedule.Transport.Sea.Vessel.CallSign);
			changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_IMOLloydsNumber, schedule.Transport.Sea.Vessel.LloydsNumber);
			return changed;
		}

		bool MapCarrier(IColumnIndexer vesselSchedule, OrganizationAddress carrier)
		{
			var changed = false;
			if (carrier != null)
			{
				changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_OperatorsDescription, carrier.CompanyName);
				changed |= SetValue(vesselSchedule, JobVesselScheduleSchema.EV_LineOperator, carrier.OrganizationCode);
			}
			return changed;
		}

		JobVesselSchedule CreateOrLoadVesselSchedule(UniversalSchedule schedule, string portCode, bool isLoading)
		{
			var query = GetQueryForMatching(schedule, isLoading);
			query.AddToFilter(JobVesselScheduleSchema.EV_RL_NKPortCode, portCode);
			return Factory.LoadTop1<JobVesselSchedule>(query) ?? Factory.New<JobVesselSchedule>();
		}

		#endregion

		#region Implementation

		void DeleteNonIncludedPorts(UniversalSchedule schedule)
		{
			var portlist = GetSQLFormattedListOfPorts(schedule);
			if (portlist.Count > 0)
			{
				var voyageNumberInQuery = GetQueryForMatching(schedule, isLoading: false, filterByLloydsNumber: false);
				voyageNumberInQuery.AddToFilter(JoinCondition.And, JobVesselScheduleSchema.EV_ShipOperatorVoyageOut, ZString.Empty);

				var voyageNumberOutQuery = GetQueryForMatching(schedule, isLoading: true, filterByLloydsNumber: false);
				voyageNumberOutQuery.AddToFilter(JoinCondition.And, JobVesselScheduleSchema.EV_ShipOperatorVoyageIn, ZString.Empty);

				var query = new ZQuery();
				query.AddToFilter(voyageNumberInQuery);
				query.AddToFilter(voyageNumberOutQuery, JoinCondition.Or);

				var vesselSchedules = Factory.Load<JobVesselSchedule>(query);
				var vesselSchedulesToDelete = vesselSchedules.Where(s => !portlist.Contains(s.EV_RL_NKPortCode)).ToArray();
				Array.ForEach(vesselSchedulesToDelete, v => v.Delete());
			}
		}

		static List<string> GetSQLFormattedListOfPorts(UniversalSchedule schedule)
		{
			var ports = new List<string>();
			if (!schedule.DischargeCollection.IsNullOrEmpty())
			{
				ports.AddRange(schedule.DischargeCollection.Select(d => d.Port.Code.GetValueOrDefault().ToString()));
			}
			if (!schedule.LoadingCollection.IsNullOrEmpty())
			{
				ports.AddRange(schedule.LoadingCollection.Select(l => l.Port.Code.GetValueOrDefault().ToString()));
			}
			return ports;
		}

		static bool SetValue(IColumnIndexer targetBO, SchemaStringColumn column, ZString? valueSource)
		{
			if (valueSource.HasValue)
			{
				var newValue = GetValidMaxLengthValue(valueSource.Value, column);
				if (!newValue.Equals(targetBO[column.Name]))
				{
					targetBO[column.Name] = newValue;
					return true;
				}
			}
			return false;
		}

		static bool SetValue(IColumnIndexer targetBO, SchemaDateTimeColumn column, ZDateTime? valueSource)
		{
			if (valueSource.HasValue)
			{
				var value = (!valueSource.Value.IsEmpty && valueSource.Value.IsValidSmallDateTime) ? valueSource.Value.ToSmallDateTime() : valueSource.Value;

				if (value.IsEmpty || value.IsValidSmallDateTime)
				{
					if (!value.Equals(targetBO[column.Name]))
					{
						targetBO[column.Name] = value;
						return true;
					}
				}
			}
			return false;
		}

		static ZString GetValidMaxLengthValue(ZString value, SchemaStringColumn column)
		{
			var valueLength = value.Length;
			int maxLength = column.MaxLength;
			if (valueLength > maxLength)
			{
				value = value.Substring(0, maxLength);
			}
			return value;
		}

		ZQuery GetQueryForMatching(UniversalSchedule schedule, bool isLoading, bool filterByLloydsNumber = true)
		{
			var voyageColumn = isLoading ? JobVesselScheduleSchema.EV_ShipOperatorVoyageOut : JobVesselScheduleSchema.EV_ShipOperatorVoyageIn;
			var query = new ZQuery(voyageColumn, schedule.Transport.Sea.VoyageNumber);
			query.AddToFilter(JobVesselScheduleSchema.EV_DataProvider, schedule.DataProvider);

			if (filterByLloydsNumber && schedule.Transport.Sea.Vessel.LloydsNumber.HasValue && schedule.Transport.Sea.Vessel.LloydsNumber.Value != ZString.Empty)
			{
				query.AddToFilter(JobVesselScheduleSchema.EV_IMOLloydsNumber, schedule.Transport.Sea.Vessel.LloydsNumber);
			}
			else
			{
				query.AddToFilter(JobVesselScheduleSchema.EV_ShipName, schedule.Transport.Sea.Vessel.VesselName);
			}

			if (schedule.Carrier != null && schedule.Carrier.OrganizationCode.HasValue)
			{
				query.AddToFilter(JobVesselScheduleSchema.EV_LineOperator, (ZString)schedule.Carrier.OrganizationCode);
			}

			return query;
		}

		#endregion
	}
}
