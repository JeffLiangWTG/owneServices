using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using GlowIndexQueryService.Business;
using Constants = GlowIndexQueryService.Business.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLuceneTaskSearch : IWhsLuceneTaskSearch
	{
		public WhsLuceneTaskSearch(IGlowIndexQueryEngine glowIndexQueryEngine)
		{
			GlowIndexQueryEngine = Argument.NotNull(glowIndexQueryEngine, nameof(glowIndexQueryEngine));
		}
		IGlowIndexQueryEngine GlowIndexQueryEngine { get; }

		public GlowIndexQueryResultCollection QueryLuceneForTasks(
			string reference,
			IGlbStaff staff,
			WhsRFRegistry userRegistry,
			WhsWarehouse warehouse,
			string[] formFlowTypesToConsider,
			Guid[] tasksToIgnore)
		{
			var query = new GlowIndexQueryParam(
				new List<IGlowQuery> { BuildQueries(reference, staff, userRegistry, warehouse, formFlowTypesToConsider, tasksToIgnore) },
				WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType,
				Constants.MAXIMUM_QUERY_RESULTS_RETURNED,
				onlyIncludePk: false);

			return GlowIndexQueryEngine.Query(query);
		}

		IGlowQuery BuildQueries(
			string reference,
			IGlbStaff staff,
			WhsRFRegistry userRegistry,
			WhsWarehouse warehouse,
			string[] formFlowTypesToConsider,
			Guid[] tasksToIgnore)
		{
			if (formFlowTypesToConsider.Length == 0)
			{
				throw new ArgumentException("At least one form flow type must be specified.", nameof(formFlowTypesToConsider));
			}

			var formFlowTypesToInclude = new HashSet<string>();
			foreach (var formFlowType in formFlowTypesToConsider)
			{
				if (!LuceneSupportedTaskTypes.Contains(formFlowType))
				{
					throw new ArgumentException($"Specified Form Flow Type: '{formFlowType}' is not supported by Lucene.", nameof(formFlowTypesToConsider));
				}
				formFlowTypesToInclude.Add(formFlowType);
			}

			var clientCode = userRegistry.WRR_OH_Client.IsValid ? userRegistry.Client.OH_Code : null;

#pragma warning disable EDI007 // Customizable Data Translation Rule
			var pickingArea = userRegistry.WRR_WA_PickingArea.IsValid ? userRegistry.PickingArea.WA_Name.ToString() : null;
			var putawayArea = userRegistry.WRR_WA_PutawayArea.IsValid ? userRegistry.PutawayArea.WA_Name.ToString() : null;
#pragma warning restore EDI007 // Customizable Data Translation Rule

			var areaNames = new[] { pickingArea, putawayArea }.Where(a => !string.IsNullOrEmpty(a));
			var areaNameQuery = PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.AreaName, areaNames.ToArray());

			var queries = new List<IGlowQuery>();
			var standardQueryFilters = new BooleanQuery(BooleanOperator.And, CreateDefaultTaskQueries(warehouse.WW_WarehouseCode, reference, staff, tasksToIgnore));

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.UnloadJob))
			{
				AddQueryIfValid(queries, CreateUnloadTaskQuery(clientCode, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.PutawayJob))
			{
				AddQueryIfValid(queries, CreatePutawayTaskQuery(userRegistry, clientCode, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.ReplenishmentJob))
			{
				AddQueryIfValid(queries, CreateReplenishmentTaskQuery(userRegistry, clientCode, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.TransferJob))
			{
				AddQueryIfValid(queries, CreateTransferTaskQuery(userRegistry, clientCode, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.PickJob))
			{
				AddQueryIfValid(queries, CreatePickingTaskQuery(userRegistry, clientCode, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.DirectedPackingJob))
			{
				AddQueryIfValid(queries, CreateDirectedPackingTaskQuery(clientCode, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.CycleCountJob))
			{
				AddQueryIfValid(queries, CreateCycleCountTaskQuery(userRegistry, areaNameQuery));
			}

			if (formFlowTypesToInclude.Contains(WarehouseTaskFormFlowTypes.LoadJob))
			{
				AddQueryIfValid(queries, CreateLoadTaskQuery(userRegistry, clientCode, areaNameQuery));
			}

			var specificFormFlowQueries = new BooleanQuery(BooleanOperator.Or, queries.ToArray());
			return new BooleanQuery(BooleanOperator.And, standardQueryFilters, specificFormFlowQueries);
		}

		#region CreateDefaultTaskQueries

		IGlowQuery[] CreateDefaultTaskQueries(string warehouseCode, string reference, IGlbStaff staff, Guid[] tasksToIgnore)
		{
			var queries = new List<IGlowQuery>();

			queries.Add(PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.Warehouse, warehouseCode));
			queries.Add(PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.Reference, reference));

			var capabilities = staff.Capabilities.Cast<GlbCapability>().Select(c => c.G4_Code.ToString());
			queries.Add(PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.Capability, capabilities.Append(string.Empty).ToArray()));

			var isInBufferTerm = new Term(WhsLuceneProcessTaskDefinitions.IsInABuffer, $"true");
			queries.Add(new EqualQuery(isInBufferTerm, useQuotes: false));

			var staffQuery = PrepareStaffQuery(staff);
			queries.Add(staffQuery);

			var tasksToIgnoreQuery = PrepareNotEqualsQuery(WhsLuceneProcessTaskDefinitions.PK, useQuotes: false, tasksToIgnore.Select(pk => pk.ToString()).ToArray());
			queries.Add(tasksToIgnoreQuery);

			return queries.ToArray();
		}

		IGlowQuery PrepareStaffQuery(IGlbStaff staff)
		{
			var assignedQuery = PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.Staff, staff.GS_Code);
			var assignedStatusQueries = PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.Status, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Working, ProcessTaskStatusCodeList.Codes.Suspended);
			var isAssignedQuery = new BooleanQuery(BooleanOperator.And, assignedQuery, assignedStatusQueries);

			var unassignedQuery = PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.Staff, String.Empty);
			var unassignedStatusQueries = PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.Status, ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned);
			var isUnassignedQuery = new BooleanQuery(BooleanOperator.And, unassignedQuery, unassignedStatusQueries);

			return new BooleanQuery(BooleanOperator.Or, isAssignedQuery, isUnassignedQuery);
		}

		#endregion

		#region CreateTaskQueries

		IGlowQuery CreateUnloadTaskQuery(string clientCode, IGlowQuery areaQuery)
		{
			var unloadQueries = new List<IGlowQuery>();
			AddQueryIfValid(unloadQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.UnloadJob));
			AddQueryIfValid(unloadQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.Client, clientCode));
			AddQueryIfValid(unloadQueries, areaQuery);
			return new BooleanQuery(BooleanOperator.And, unloadQueries.ToArray());
		}

		IGlowQuery CreatePutawayTaskQuery(WhsRFRegistry registry, string clientCode, IGlowQuery areaQuery)
			=> CreateTransferTaskQuery(WarehouseTaskFormFlowTypes.PutawayJob, registry, clientCode, areaQuery);

		IGlowQuery CreateReplenishmentTaskQuery(WhsRFRegistry registry, string clientCode, IGlowQuery areaQuery)
			=> CreateTransferTaskQuery(WarehouseTaskFormFlowTypes.ReplenishmentJob, registry, clientCode, areaQuery);

		IGlowQuery CreateTransferTaskQuery(WhsRFRegistry registry, string clientCode, IGlowQuery areaQuery)
			=> CreateTransferTaskQuery(WarehouseTaskFormFlowTypes.TransferJob, registry, clientCode, areaQuery);

		IGlowQuery CreateTransferTaskQuery(string formFlowType, WhsRFRegistry registry, string clientCode, IGlowQuery areaQuery)
		{
			var transferQueries = new List<IGlowQuery>();
			AddQueryIfValid(transferQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.FormFlowType, formFlowType));
			AddQueryIfValid(transferQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.Client, clientCode));
			AddQueryIfValid(transferQueries, areaQuery);

			if (!WhsAreaAndPickMethodHelper.IsAnyCode(registry.WRR_PickMethodCode))
			{
				AddQueryIfValid(transferQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.PickMethod, registry.WRR_PickMethodCode));
			}

			if (registry.WRR_UOMPackType != WhsRFRegistry.DefaultUOMPackType)
			{
				AddQueryIfValid(transferQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.UOMType, registry.WRR_UOMPackType));
			}

			return new BooleanQuery(BooleanOperator.And, transferQueries.ToArray());
		}

		IGlowQuery CreatePickingTaskQuery(WhsRFRegistry registry, string clientCode, IGlowQuery areaQuery)
		{
			var pickQueries = new List<IGlowQuery>();
			AddQueryIfValid(pickQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.PickJob));
			AddQueryIfValid(pickQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.Client, clientCode));
			AddQueryIfValid(pickQueries, areaQuery);

			if (!WhsAreaAndPickMethodHelper.IsAnyCode(registry.WRR_PickMethodCode))
			{
				AddQueryIfValid(pickQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.PickMethod, registry.WRR_PickMethodCode));
			}

			if (registry.WRR_UOMPackType != WhsRFRegistry.DefaultUOMPackType)
			{
				AddQueryIfValid(pickQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.UOMType, registry.WRR_UOMPackType));
			}

			if (registry.WRR_PickGroupSequence != 0)
			{
				AddQueryIfValid(pickQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.PickGroup, registry.WRR_PickGroupSequence.ToString()));
			}

			return new BooleanQuery(BooleanOperator.And, pickQueries.ToArray());
		}

		IGlowQuery CreateDirectedPackingTaskQuery(string clientCode, IGlowQuery areaQuery)
		{
			var directedPackingQueries = new List<IGlowQuery>();
			AddQueryIfValid(directedPackingQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.DirectedPackingJob));
			AddQueryIfValid(directedPackingQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.Client, clientCode));
			AddQueryIfValid(directedPackingQueries, areaQuery);

			return new BooleanQuery(BooleanOperator.And, directedPackingQueries.ToArray());
		}

		IGlowQuery CreateCycleCountTaskQuery(WhsRFRegistry registry, IGlowQuery areaQuery)
		{
			var cycleCountQueries = new List<IGlowQuery>();
			AddQueryIfValid(cycleCountQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.CycleCountJob));
			AddQueryIfValid(cycleCountQueries, areaQuery);

			if (!WhsAreaAndPickMethodHelper.IsAnyCode(registry.WRR_PickMethodCode))
			{
				AddQueryIfValid(cycleCountQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.PickMethod, registry.WRR_PickMethodCode));
			}

			return new BooleanQuery(BooleanOperator.And, cycleCountQueries.ToArray());
		}

		IGlowQuery CreateLoadTaskQuery(WhsRFRegistry registry, string clientCode, IGlowQuery areaQuery)
		{
			var loadQueries = new List<IGlowQuery>();
			AddQueryIfValid(loadQueries, PrepareEqualsQuery(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.LoadJob));
			AddQueryIfValid(loadQueries, PrepareEqualsIfValidQuery(WhsLuceneProcessTaskDefinitions.Client, clientCode));
			AddQueryIfValid(loadQueries, areaQuery);
			return new BooleanQuery(BooleanOperator.And, loadQueries.ToArray());
		}

		#endregion

		#region LuceneDefinitions

		public static readonly HashSet<string> LuceneSupportedTaskTypes = new HashSet<string>
		{
			WarehouseTaskFormFlowTypes.UnloadJob,
			WarehouseTaskFormFlowTypes.PutawayJob,
			WarehouseTaskFormFlowTypes.PickJob,
			WarehouseTaskFormFlowTypes.DirectedPackingJob,
			WarehouseTaskFormFlowTypes.TransferJob,
			WarehouseTaskFormFlowTypes.ReplenishmentJob,
			WarehouseTaskFormFlowTypes.LoadJob,
			WarehouseTaskFormFlowTypes.CycleCountJob,
		};

		#endregion

		#region Implementation

		IGlowQuery PrepareEqualsQuery(string searchField, params string[] keyWords)
		{
			var equalityQueries = new List<IGlowQuery>(keyWords.Length);
			foreach (var keyWord in keyWords)
			{
				var term = new Term(searchField, keyWord);
				IGlowQuery query = string.IsNullOrEmpty(keyWord)
					? new IsBlankQuery(term)
					: new EqualQuery(term);
				equalityQueries.Add(query);
			}
			return new BooleanQuery(BooleanOperator.Or, equalityQueries.ToArray());
		}

		IGlowQuery PrepareEqualsIfValidQuery(string searchField, string keyWord)
		=> !string.IsNullOrEmpty(keyWord)
			? PrepareEqualsQuery(searchField, keyWord)
			: null;

		IGlowQuery PrepareNotEqualsQuery(string searchField, bool useQuotes, params string[] keyWords)
		{
			var inequalityQueries = new List<IGlowQuery>(keyWords.Length);
			foreach (var keyWord in keyWords)
			{
				var term = new Term(searchField, keyWord);
				IGlowQuery query = string.IsNullOrEmpty(keyWord)
					? new IsNotBlankQuery(term)
					: new NotEqualQuery(term, useQuotes);
				inequalityQueries.Add(query);
			}
			return new BooleanQuery(BooleanOperator.And, inequalityQueries.ToArray());
		}

		void AddQueryIfValid(List<IGlowQuery> queries, IGlowQuery query)
		{
			if (query != null)
			{
				queries.Add(query);
			}
		}

		#endregion
	}
}
