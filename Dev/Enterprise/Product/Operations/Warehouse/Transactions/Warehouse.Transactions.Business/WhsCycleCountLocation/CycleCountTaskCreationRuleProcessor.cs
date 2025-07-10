using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CycleCountTaskCreationRuleProcessor : IScheduledRuleProcessor
	{
		public CycleCountTaskCreationRuleProcessor(
			IWhsCycleCountLocationCreator cycleCountLocationCreator,
			ICycleCountLocationTaskCreationFactLoader factLoader)
		{
			CycleCountLocationCreator = Argument.NotNull(cycleCountLocationCreator, nameof(cycleCountLocationCreator));
			FactLoader = Argument.NotNull(factLoader, nameof(factLoader));
		}

		IWhsCycleCountLocationCreator CycleCountLocationCreator { get; }
		ICycleCountLocationTaskCreationFactLoader FactLoader { get; }

		public string InformationMessageForNothingProcessed => (NoResString)"No cycle count tasks were created in this run."; // Service Task Logging

		public GuidRegistryItem ErrorContactGroupRegistryItem => WarehouseDataRegistry.Instance.CycleCountingAutomationFailureNotificationGroup;

		public ZGuid GetBranchToRunRulesAgainst(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(ruleSet, nameof(ruleSet));

			var warehouse = factory.Load<WhsWarehouse>(ruleSet.PRS_WW_Warehouse);
			return warehouse.WW_GB_RelatedCompanyBranch;
		}

		public IEnumerable<IInputFact> LoadInputFacts(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet, CancellationToken cancellationToken)
			=> FactLoader.LoadInputFacts(Argument.NotNull(factory, nameof(factory)), Argument.NotNull(ruleSet, nameof(ruleSet)).PRS_WW_Warehouse, cancellationToken);

		public void ProcessResults(BusinessObjectFactory factory, ProductionRulesEngineResult result, INotifications notifications, CancellationToken cancellationToken)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(result, nameof(result));
			Argument.NotNull(notifications, nameof(notifications));

			var cycleCountTaskFacts = new List<CycleCountTaskFact>();
			foreach (var fact in result.Facts)
			{
				if (fact is CycleCountTaskFact cycleCountTaskFact)
				{
					cycleCountTaskFacts.Add(cycleCountTaskFact);
				}
			}

			if (cycleCountTaskFacts.Count > 0)
			{
				var cycleCountsByLocPK = cycleCountTaskFacts.ToDictionary(c => c.LocationPK);
				var cycleCountTasks =
					CycleCountLocationCreator.CreateCycleCountLocations(factory, cycleCountTaskFacts.Select(cy => new WhsCycleCountLocationInfo(cy.LocationPK, cy.Granularity, cy.Priority)));
				var createdForLocPKs = cycleCountTasks.Select(c => c.WCL_WL_Location).ToHashSet();

				foreach (var cycleCountTask in cycleCountTasks)
				{
					notifications.AddInformation($"Created Cycle Count Task for Location: {cycleCountsByLocPK[cycleCountTask.WCL_WL_Location.ToGuid()].LocationString}.");  // Service Task Logging
				}

				foreach (var cycleCount in cycleCountTaskFacts.Where(t => !createdForLocPKs.Contains(t.LocationPK)).ToArray())
				{
					notifications.AddWarning($"Skipped creating Cycle Count Task for Location: {cycleCountsByLocPK[cycleCount.LocationPK].LocationString} as one already exists.");  // Service Task Logging
				}
			}
			else
			{
				notifications.AddInformation(InformationMessageForNothingProcessed);
			}
		}
	}
}
