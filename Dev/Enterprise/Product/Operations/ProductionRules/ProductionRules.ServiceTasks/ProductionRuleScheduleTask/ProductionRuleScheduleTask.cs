using System.Data;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProductionRules.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProductionRules.ServiceTasks
{
	public class ProductionRuleScheduleTask : StmScheduleTask, IProductionRuleScheduleTask
	{
		public ProductionRuleScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ParentTableCode = ProductionRuleSchema.Constants.Prefix;
		}

		public ProductionRule Rule => Factory.Load<ProductionRule>(S5_ParentID);

		public override ZString DescriptionForLog
		{
			get
			{
				var rule = Rule;
				var ruleSet = rule?.RuleSet;
				return $"Queue Scheduled Rule, Context: {ruleSet?.PRS_Context}, RuleSet: {ruleSet?.PRS_Name}, Rule: {rule?.PRL_Name}";
			}
		}

		protected override int PriorityCore
		{
			get
			{
				var priority = Rule?.PRL_Priority ?? 0;
				return priority == 0 ? 0 : int.MaxValue - priority;
			}
		}

		IWhsWarehouse Warehouse => Rule?.RuleSet?.Warehouse;

		public override ZGuid S5_GB
		{
			get => Warehouse?.WW_GB_RelatedCompanyBranch ?? base.S5_GB;
			set => base.S5_GB = value;
		}

		protected override void RunCore(INotifications notifications, CancellationToken token)
		{
			var rule = Rule;
			var ruleSet = rule?.RuleSet;

			if (rule is null)
			{
				notifications.AddWarning(Res.GetString("10c0e406-0aae-42c7-8f33-d11b3599f49c", "Production Rule not found. Rule PK = {0}", S5_ParentID.ToString()));
			}
			else if (ruleSet is null)
			{
				notifications.AddWarning(Res.GetString("99780ea7-a9a0-4809-ad9d-aa3e5c590bf0", "Production Ruleset not found. Ruleset PK = {0}", rule.PRL_PRS_RuleSet));
			}
			else if (!ruleSet.PRS_IsLive)
			{
				notifications.AddWarning(Res.GetString("079af85d-e48c-4cd7-b079-15eff4da583d", "Production Ruleset is not live: {0}", DescriptionForLog));
			}
			else
			{
				var factoryToCreateQueue = new BusinessObjectFactory { NameForDebugging = nameof(ProductionRuleScheduleTask) };

				var queue = factoryToCreateQueue.New<ProductionRuleScheduleQueue>();
				queue.PRQ_PRL_Rule = rule.PK;

				try
				{
					factoryToCreateQueue.Save();
				}
				catch (ZSaveException ex) when (ex.IndexNameIfUniqueIndexViolation == ProductionRuleScheduleQueueSchema.Constants.Indexes.FK_UX__PRQ_PRL_Rule)
				{
					notifications.AddWarning(Res.GetString("fec63e22-6cc3-4fab-bc83-cbb15c0af5b4", "Skipping Production Rule as it has already been queued for processing: {0}", DescriptionForLog));
				}
			}
		}
	}
}
