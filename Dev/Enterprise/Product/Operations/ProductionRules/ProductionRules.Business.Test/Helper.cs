using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProductionRules.Business.Testing
{
	public class Helper
	{
		public Helper(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		BusinessObjectFactory Factory { get; }

		public ProductionRuleSet CreateRuleSet(string name, string description, bool isLive = true, bool isSystem = false, string context = "PWP", string contextSubType = "", ZGuid? warehousePK = null, ZGuid? companyPK = null, ZDateTime? lastEditTime = null)
		{
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = name;
			ruleSet.PRS_Description = description;
			ruleSet.PRS_Context = context;
			ruleSet.PRS_ContextSubType = contextSubType;
			ruleSet.PRS_WW_Warehouse = warehousePK ?? ZGuid.Empty;
			ruleSet.PRS_GC_Company = companyPK ?? ZGuid.Empty;
			ruleSet.PRS_IsLive = isLive;
			ruleSet.PRS_IsSystem = isSystem;
			ruleSet.PRS_SystemLastEditTimeUtc = lastEditTime ?? ZDateTime.UtcNow;
			return ruleSet;
		}

		public ProductionRule CreateRule(ProductionRuleSet ruleSet, string name, string description, short priority = 1)
		{
			var rule = Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = name;
			rule.PRL_Description = description;
			rule.PRL_Priority = priority;
			rule.PRL_RuleDefinition = "{}";
			return rule;
		}

		public ProductionRuleScheduleQueue CreateScheduledRuleQueue(ProductionRule rule, ZDateTime createTime)
		{
			var queue = Factory.New<ProductionRuleScheduleQueue>();
			queue.PRQ_PRL_Rule = rule.PK;
			queue.PRQ_SystemCreateTimeUtc = createTime;
			return queue;
		}

		public GenCustomColumnDefinition CreateUserDefinedProperty(string type, string propertyName, string factUniqueKey = "INV")
		{
			var factType = Factory.LoadTop1<ProductionRulesFactTypeView>(new ZQuery(ProductionRulesFactTypeViewSchema.PFV_FactTypeKey, factUniqueKey));
			var userProperty = Factory.New<GenCustomColumnDefinition>();
			userProperty.XC_Type = type;
			userProperty.XC_Name = propertyName;
			userProperty.XC_ParentTableCode = ProductionRulesFactTypeViewSchema.Constants.Prefix;
			userProperty.XC_ParentID = factType.PK;
			return userProperty;
		}
	}
}
