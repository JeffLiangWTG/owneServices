using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class PickBreakdownStrategy : TaskCreationJobWithBreakdownStrategy
	{
		public override TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job)
		{
			var pick = LoadWhsPick(job);
			var warehouse = pick.Warehouse;
			return new TaskCreationWorkflowInfo(pick.HumanReadableName, warehouse.WW_GB_RelatedCompanyBranch, warehouse.PK, warehouse.WW_GG_ReleaseGroup, pick);
		}

		public override void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(planningStatus, nameof(planningStatus));

			var pick = LoadWhsPick(job, factory);
			pick.WP_TaskPlanningStatus = planningStatus;
		}

		protected override RulesContextSubType BreakdownSubType => RulesContextSubType.ProductWarehousePickLine;

		protected override ZString FormflowType => WarehouseTaskFormFlowTypes.PickJob;

		protected override ZString TaskAndWorkflowName => Res.GetString("e7b11128-6a8f-4b82-a0a2-504fda9e71dc", "Pick Lines");

		protected override short RawNudge => 200;

		protected override IEnumerable<IInputFact> GetFacts(WhsReadyForPlanningJobsView job, CancellationToken token)
		{
			var pick = LoadWhsPick(job);
			var facts = new List<IInputFact>
			{
				new TaskManagementContextFact(0)
			};

			var availableInventorySplits = new Dictionary<Guid, WhsPickAvailableInventorySplitBase>();
			var organisations = new Dictionary<ZGuid, OrganisationFact>();
			var products = new Dictionary<ZGuid, ProductFact>();
			var locations = new Dictionary<ZGuid, TaskManagementLocationFact>();

			var isPickByUOMEnabled = pick.IsPickByUOMEnabled;

			foreach (var orderedInventory in pick.OrderedInventories.Cast<WhsPickOrderedInventory>())
			{
				var client = new Lazy<OrgHeader>(() => orderedInventory.Client);
				var product = new Lazy<OrgSupplierPart>(() => orderedInventory.SupplierPart);
				var whsProduct = new Lazy<WhsProduct>(() => WhsProduct.GetWhsProduct(product.Value));
				var relation = new Lazy<OrgPartRelation>(() => product.Value.RelatedOrganisations.FindByOrganisationAndRelationship(client.Value, OrgPartRelation.RelationshipTypes.Owner));

				var clientFact = new Lazy<IOrganisationFact>(() => WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, client.Value.PK, () => client.Value));
				var productFact = new Lazy<IProductFact>(() => GetOrCreateProductFact(products, relation.Value, product.Value));

				foreach (var availableInventory in
							orderedInventory.AvailableInventories
							.Cast<WhsPickAvailableInventory>()
							.Where(ai => ai.PickLines.Any(pl => pl.WZ_Units > 0)))
				{
					var locationFact = GetOrCreateLocationFact(locations, availableInventory.LocationPK, () => availableInventory.Location);

					foreach (var availableInventorySplit in availableInventory.AvailableInventoriesSplit.Where(ai => ai.PickedDate.IsEmpty))
					{
						var splitByUOM = availableInventorySplit as WhsPickAvailableInventorySplitByUOM;

						if (splitByUOM == null || ShouldPlanTaskBasedOnUOMType(splitByUOM.UOMType))
						{
							var availableInventorySplitPK = availableInventorySplit.PK.ToGuid();
							availableInventorySplits.Add(availableInventorySplitPK, availableInventorySplit);

							var packQty = (int)Math.Ceiling(splitByUOM?.PackQuantity ?? availableInventorySplit.StockUnitQuantity);
							var packUq = splitByUOM?.PackQuantityUQ ?? availableInventorySplit.StockKeepingUnit;
							var isPackTypeSKU = availableInventorySplit.StockKeepingUnit.EqualsIgnoringCase(packUq);
							var conversion = !isPackTypeSKU ? product.Value.PartUnits.Cast<OrgPartUnit>().FirstOrDefault(x => x.OF_ParentPackType.EqualsIgnoringCase(packUq) && (x.OF_Cubic > 0 || x.OF_Weight > 0)) : null;

							facts.Add(new TaskManagementPickLineFact(
								availableInventorySplitPK,
								availableInventorySplitPK,
								pick,
								availableInventory,
								clientFact.Value,
								productFact.Value,
								locationFact,
								packUq,
								splitByUOM?.UOMType ?? string.Empty,
								availableInventorySplit.StockUnitQuantity,
								(int)Math.Ceiling(availableInventorySplit.StockUnitQuantity),
								packQty,
								conversion != null ? packQty * conversion.OF_Weight : availableInventorySplit.StockUnitQuantity * product.Value.OP_Weight,
								product.Value.OP_WeightUQ,
								conversion != null ? packQty * conversion.OF_Cubic : availableInventorySplit.StockUnitQuantity * product.Value.OP_Cubic,
								product.Value.OP_CubicUQ));
						}
					}
				}
			}

			job.Factory.GetCachedValue<IDictionary<Guid, WhsPickAvailableInventorySplitBase>>(CacheKey, () => availableInventorySplits);

			return facts;

			static TaskManagementLocationFact GetOrCreateLocationFact(
				Dictionary<ZGuid, TaskManagementLocationFact> locationFacts,
				ZGuid locationPk,
				Func<IWhsLocation> getLocation)
			{
				if (!locationFacts.TryGetValue(locationPk, out var locationFact))
				{
					var location = getLocation();
					locationFacts[locationPk] = locationFact = new TaskManagementLocationFact(location);
				}

				return locationFact;
			}

			static ProductFact GetOrCreateProductFact(
				Dictionary<ZGuid, ProductFact> productFacts,
				OrgPartRelation relation,
				OrgSupplierPart part)
			{
				if (!productFacts.TryGetValue(relation.PK, out var productFact))
				{
					productFacts[relation.PK] = productFact = new ProductFact(part, relation);
				}

				return productFact;
			}

			bool ShouldPlanTaskBasedOnUOMType(ZString uomType)
				=> (!pick.WP_PickPalletsByLabel || !uomType.EqualsIgnoringCase(UOMPackTypesList.Codes.Pallet)) &&
					(!pick.WP_PickCasesByLabel || !uomType.EqualsIgnoringCase(UOMPackTypesList.Codes.Case)) &&
					(!pick.WP_CartoniseSplitCases || !uomType.EqualsIgnoringCase(UOMPackTypesList.Codes.SplitCase));
		}

		public override void LinkTasks(
			WhsReadyForPlanningJobsView job,
			IReadOnlyDictionary<ZGuid, ProcessTask> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
		{
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(tasks, nameof(tasks));
			Argument.NotNull(lines, nameof(lines));

			if (job.Factory.TryGetValueFromCacheOnly<IDictionary<Guid, WhsPickAvailableInventorySplitBase>>(CacheKey, out var availableInventorySplits))
			{
				foreach (var line in lines.Cast<ITaskManagementPickLineFact>().Where(l => l.AssignedTask != Guid.Empty))
				{
					var assignedTask = tasks[line.AssignedTask];
					var availableInventorySplit = availableInventorySplits[line.AvailableInventorySplitPK];

					availableInventorySplit.LinkPickLinesToTask(line.Quantity, assignedTask.PK);
				}
			}
		}

		WhsPick LoadWhsPick(WhsReadyForPlanningJobsView job, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(job, nameof(job));
			return Argument.NotNull((factory ?? job.Factory).Load<WhsPick>(job.PK), "loaded job");
		}

		const string CacheKey = $"{nameof(PickBreakdownStrategy)}_AvailableInventorySplits";
	}
}
