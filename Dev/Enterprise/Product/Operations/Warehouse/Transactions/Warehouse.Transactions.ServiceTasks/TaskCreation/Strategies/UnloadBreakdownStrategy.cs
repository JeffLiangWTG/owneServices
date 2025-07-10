using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class UnloadBreakdownStrategy : TaskCreationJobWithBreakdownStrategy
	{
		public override TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job)
		{
			var receive = LoadReceive(job);
			var warehouse = receive.Warehouse;
			return new TaskCreationWorkflowInfo(receive.HumanReadableName, warehouse.WW_GB_RelatedCompanyBranch, warehouse.PK, warehouse.WW_GG_ReleaseGroup, receive);
		}

		public override void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(planningStatus, nameof(planningStatus));

			var receive = LoadReceive(job, factory);
			receive.WD_TaskPlanningStatus = planningStatus;
		}

		protected override RulesContextSubType BreakdownSubType => RulesContextSubType.ProductWarehouseUnloadLine;

		protected override ZString FormflowType => WarehouseTaskFormFlowTypes.UnloadJob;

		protected override ZString TaskAndWorkflowName => Res.GetString("a721e9d9-9133-4c75-b7c4-b490c3960784", "Unload");

		protected override short RawNudge => 100;

		protected override IEnumerable<IInputFact> GetFacts(WhsReadyForPlanningJobsView job, CancellationToken token)
		{
			var receive = LoadReceive(job);
			var facts = new List<IInputFact>
			{
				new TaskManagementContextFact(0)
			};

			var organisations = new Dictionary<ZGuid, OrganisationFact>();
			var products = new Dictionary<ZGuid, ProductFact>();
			var pallets = new Dictionary<string, TaskManagementGroupingFact>(StringComparer.OrdinalIgnoreCase);

			var client = receive.Client;
			var clientFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, receive.WD_OH_Client, () => client);
			var supplierFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, receive.SupplierPK, () => receive.Supplier);

			foreach (WhsReceiveLine receiveLine in receive.Lines)
			{
				var consigneeFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, receiveLine.ConsigneePK, () => receiveLine.Consignee);

				var product = receiveLine.SupplierPart;
				var whsProduct = WhsProduct.GetWhsProduct(product);
				var relation = product.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);

				if (!products.TryGetValue(relation.PK, out var productFact))
				{
					products[relation.PK] = productFact = new ProductFact(product, relation);
				}

				TaskManagementGroupingFact groupingFact = null;
				if (receiveLine.WE_PalletID.IsEmpty)
				{
					groupingFact = new TaskManagementGroupingFact(Guid.NewGuid()) { NumberOfLines = 1 };
				}
				else
				{
					if (!pallets.TryGetValue(receiveLine.WE_PalletID, out groupingFact))
					{
						pallets[receiveLine.WE_PalletID] = groupingFact = new TaskManagementGroupingFact(Guid.NewGuid()) { NumberOfPacks = 1 };
					}

					groupingFact.NumberOfLines++;
				}

				facts.Add(new TaskManagementUnloadLineFact(groupingFact, receive, receiveLine, clientFact, supplierFact, consigneeFact, productFact));
			}

			return facts;
		}

		public override void LinkTasks(
			WhsReadyForPlanningJobsView job,
			IReadOnlyDictionary<ZGuid, ProcessTask> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
		{
			// No linking required
		}

		WhsReceive LoadReceive(WhsReadyForPlanningJobsView job, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(job, nameof(job));
			return Argument.NotNull((factory ?? job.Factory).Load<WhsReceive>(job.PK), "loaded job");
		}
	}
}
