using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.ServiceTasks
{
	public partial class DispatchTransportationUnitFinalisedTimeProcessingManager
	{
		public DispatchTransportationUnitFinalisedTimeProcessingManager(ILogger logger)
		{
			Logger = logger;
		}

		readonly ILogger Logger;

		public void SetFinalizedIfRequired()
		{
			var newFactory = new BusinessObjectFactory();
			var informationLogs = new ZStringBuilder();
			var errorLogs = new ZStringBuilder();
			informationLogs.Append(Res.GetString("0ae5ebc1-aa51-4c8f-b272-70291884b1f1", "Finalize Dispatch Transportation Units."));

			var query = new ZQuery(WhsItemDispatchTransportationUnitSchema.WDH_GateOutTime, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_FinalisedTime, SQLComparisonOperator.Equal, null);
			query.MaximumRows = 1000;

			var dtus = newFactory.Load<WhsItemDispatchTransportationUnit>(query).GroupBy(d => d.Warehouse);
			var whsBranchMappedToDtuAndPeriods = new Dictionary<Guid, List<(WhsItemDispatchTransportationUnit, int)>>();
			var hasUnprocessedDTUs = false;

			if (dtus.Any())
			{
				var nowTime = DateTimeOffset.Now;

				foreach (var item in dtus)
				{
					var whsBranch = item.Key.WW_GB_RelatedCompanyBranch.ToGuid();
					var period = WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.GetFallBackValueAtAllLevels(Guid.Empty, whsBranch, Guid.Empty);
					var registryDataType = WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.DataType as IntRegistryDataType;
					if (period < registryDataType.LowerBound)
					{
						hasUnprocessedDTUs = true;
						errorLogs.Append(Res.GetString("6b5aac48-3365-4949-9dec-00be69b8ab25", "Cannot process Dispatch Transport Units from {0} because the value of Registry [Departed Package Auto Finalization Delay] {1} is less than the minimum valid value {2}", item.Key.WW_WarehouseNameMultilingual, period, (int)registryDataType.LowerBound));
					}
					else if (period > registryDataType.UpperBound)
					{
						hasUnprocessedDTUs = true;
						errorLogs.Append(Res.GetString("e8041a2c-fca4-45d7-9859-4e06e745eca1", "Cannot process Dispatch Transport Units from {0} because the value of Registry [Departed Package Auto Finalization Delay] {1} is greater than the maximum valid value {2}", item.Key.WW_WarehouseNameMultilingual, period, (int)registryDataType.LowerBound));
					}
					else
					{
						var gateOutDtus = item.ToList();

						foreach (var dtu in gateOutDtus)
						{
							if (dtu.WDH_GateOutTime.AddHours(period) < nowTime)
							{
								if (!whsBranchMappedToDtuAndPeriods.ContainsKey(whsBranch))
								{
									whsBranchMappedToDtuAndPeriods.Add(whsBranch, new List<(WhsItemDispatchTransportationUnit, int)>());
								}
								whsBranchMappedToDtuAndPeriods[whsBranch].Add((dtu, period));
							}
						}
					}
				}

				if (whsBranchMappedToDtuAndPeriods.Any())
				{
					informationLogs.Append(Res.GetString("1efc5ead-abb0-4323-abd6-f1e3aca22c68", "Found {0} Dispatch Transport Units to be Finalized:", whsBranchMappedToDtuAndPeriods.Values.Sum(v => v.Count)));

					foreach (var item in whsBranchMappedToDtuAndPeriods)
					{
						var whsBranch = item.Key;
						var dtuAndPeriods = item.Value;

						using (DisposableEnvironment.ForBranch(whsBranch))
						{
							foreach (var (dtu, period) in dtuAndPeriods)
							{
								dtu.Finalise(dtu.WDH_GateOutTime.AddHours(period), string.Format(CultureInfo.InvariantCulture, (NoResString)"Auto Finalised after {0} {1}", period, period > 1 ? (NoResString)"hours" : (NoResString)"hour")); // Log Reference.

								informationLogs.Append(Res.GetString("8a72e460-2de1-4e16-9c9a-2bbd42e1c21f", "DTU {0} ({1}) gated out at {2}.", dtu.WDH_ReferenceNumber, dtu.WDH_VehicleReference, dtu.WDH_GateOutTime.ToZDateTime().ToLongTimeString()));

								newFactory.Save();
							}
						}
					}
				}
			}

			if (!whsBranchMappedToDtuAndPeriods.Any() && !hasUnprocessedDTUs)
			{
				informationLogs.Append(Res.GetString("3e260877-fa71-4e4c-a99a-1939088639c1", "No Dispatch Transportation Units found."));
			}

			Logger.Information(informationLogs.ToStringWithNewLineBetweenAppends());
			if (!errorLogs.IsEmpty)
			{
				Logger.Error(errorLogs.ToStringWithNewLineBetweenAppends());
			}
		}
	}
}
