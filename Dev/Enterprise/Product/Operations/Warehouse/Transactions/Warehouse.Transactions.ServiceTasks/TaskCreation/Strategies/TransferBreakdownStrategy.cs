using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class TransferBreakdownStrategy : TaskCreationJobWithBreakdownStrategy
	{
		public override TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job)
		{
			var transfer = LoadTransfer(job);
			var warehouse = transfer.Warehouse;
			return new TaskCreationWorkflowInfo(transfer.HumanReadableName, warehouse.WW_GB_RelatedCompanyBranch, warehouse.PK, warehouse.WW_GG_ReleaseGroup, transfer);
		}

		public override void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(planningStatus, nameof(planningStatus));

			var transfer = LoadTransfer(job, factory);
			transfer.WD_TaskPlanningStatus = planningStatus;
		}

		protected override RulesContextSubType BreakdownSubType => RulesContextSubType.ProductWarehouseTransferLine;

		protected override ZString FormflowType => WarehouseTaskFormFlowTypes.TransferJob;

		protected override ZString TaskAndWorkflowName => Res.GetString("4651043d-5abe-4f3b-a4d6-910065bdd581", "Transfer");

		protected override short RawNudge => 100;

		protected override IEnumerable<IInputFact> GetFacts(WhsReadyForPlanningJobsView job, CancellationToken token)
		{
			var transfer = LoadTransfer(job);
			var facts = new List<IInputFact>
			{
				new TaskManagementContextFact(0)
			};

			var organisations = new Dictionary<ZGuid, OrganisationFact>();
			var products = new Dictionary<ZGuid, ProductFact>();
			var locations = new Dictionary<ZGuid, TaskManagementLocationFact>();
			var pallets = new Dictionary<string, TaskManagementGroupingFact>(StringComparer.OrdinalIgnoreCase);

			var client = transfer.Client;
			var clientFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, transfer.WD_OH_Client, () => client);

			foreach (WhsTransferLine transferLine in transfer.Lines)
			{
				var product = transferLine.SupplierPart;
				var whsProduct = WhsProduct.GetWhsProduct(product);
				var relation = product.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);

				if (!products.TryGetValue(relation.PK, out var productFact))
				{
					products[relation.PK] = productFact = new ProductFact(product, relation);
				}

				var fromLocation = GetLocationFact(transferLine.WE_WL_TransferFrom, () => transferLine.TransferFromLocation);
				var toLocation = GetLocationFact(transferLine.WE_WL, () => transferLine.Location);

				TaskManagementGroupingFact groupingFact = null;
				if (!transferLine.WE_TransferFromPalletId.IsEmpty)
				{
					var palletIdKey = GetPalletIDsKey(transferLine);
					if (!pallets.TryGetValue(palletIdKey, out groupingFact))
					{
						pallets[palletIdKey] = groupingFact = new TaskManagementGroupingFact(Guid.NewGuid()) { NumberOfPacks = 1 };
					}

					groupingFact.NumberOfLines++;
				}
				else
				{
					groupingFact = new TaskManagementGroupingFact(Guid.NewGuid()) { NumberOfLines = 1 };
				}

				facts.Add(new TaskManagementTransferLineFact(groupingFact, transfer, transferLine, clientFact, productFact, fromLocation, toLocation, GetHasAwaitingPicks(transfer, transferLine)));
			}

			return facts;

			TaskManagementLocationFact GetLocationFact(ZGuid locationPK, Func<IWhsLocation> getLocation)
			{
				if (!locations.TryGetValue(locationPK, out var locationFact) && locationPK.IsValid)
				{
					locations[locationPK] = locationFact = new TaskManagementLocationFact(getLocation());
				}

				return locationFact;
			}

			static string GetPalletIDsKey(WhsTransferLine transferLine) => $"{transferLine.WE_TransferFromPalletId}→{transferLine.WE_PalletID}";
		}

		protected virtual bool GetHasAwaitingPicks(WhsTransfer transfer, WhsTransferLine transferLine) => false;

		public override void LinkTasks(
			WhsReadyForPlanningJobsView job,
			IReadOnlyDictionary<ZGuid, ProcessTask> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
		{
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(tasks, nameof(tasks));
			Argument.NotNull(lines, nameof(lines));

			var transfer = LoadTransfer(job);
			var transferLines = transfer.Lines.ToDictionary(l => l.PK);

			foreach (var taskLink in lines
				.Select(l => new { l.Grouping.Fact.AssignedTask, LinePK = l.PK })
				.Where(tl => tl.AssignedTask != Guid.Empty))
			{
				transferLines[taskLink.LinePK].WE_P9_Task = tasks[taskLink.AssignedTask].PK;
			}
		}

		WhsTransfer LoadTransfer(WhsReadyForPlanningJobsView job, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(job, nameof(job));
			return Argument.NotNull((factory ?? job.Factory).Load<WhsTransfer>(job.PK), "loaded job");
		}
	}
}
