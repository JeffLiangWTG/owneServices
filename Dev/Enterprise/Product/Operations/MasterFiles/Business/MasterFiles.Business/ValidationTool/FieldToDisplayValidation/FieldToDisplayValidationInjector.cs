using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business;

sealed class FieldToDisplayValidationInjector(IBusiness job)
{
	ZString JobTableName => jobTableName ??= (job as BusinessObject)?.TableName ?? string.Empty;
	string jobTableName;

	public void Inject()
	{
		if (!ProcessTemplateValidationManager.CheckValidationToolIsSupported(job, ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation))
		{
			return;
		}

		var matchedRules = new ValidationToolLoader(job).GetMatchedRules(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation);
		var (rootRules, leafRules) = SplitByFieldToDisplayValidationTableName(matchedRules);
		InjectRootRules(rootRules);
		InjectLeafRules(leafRules);
	}

	(IReadOnlyList<RuleInfo> RootRules, IReadOnlyList<RuleInfo> LeafRules) SplitByFieldToDisplayValidationTableName(IReadOnlyList<ProcessTemplateValidation> matchedRules)
	{
		List<RuleInfo> rootRules = new();
		List<RuleInfo> leafRules = new();
		foreach (var rule in matchedRules)
		{
			var fieldToDisplayValidationStaticInfo = rule.FieldToDisplayValidationStaticInfo;
			if (fieldToDisplayValidationStaticInfo is null)
			{
				continue;
			}

			var tableName = fieldToDisplayValidationStaticInfo.GetTableName();
			if (tableName == JobTableName)
			{
				rootRules.Add(new RuleInfo(rule, fieldToDisplayValidationStaticInfo, tableName));
			}
			else
			{
				leafRules.Add(new RuleInfo(rule, fieldToDisplayValidationStaticInfo, tableName));
			}
		}
		return (rootRules, leafRules);
	}

	void InjectRootRules(IReadOnlyList<RuleInfo> rootRules)
	{
		foreach (var (rule, propertyInfoToDisplayValidation, _) in rootRules)
		{
			var zPropertyInfo = propertyInfoToDisplayValidation.GetZPropertyInfo(job);
			if (zPropertyInfo is null)
			{
				continue;
			}

			zPropertyInfo.AdditionalValidation += () =>
			{
				RuleFieldRule(rule, (BusinessObject)job);
			};
		}
	}

	void InjectLeafRules(IReadOnlyList<RuleInfo> leafRules)
	{
		var leafRuleRegistration = new Dictionary<ZPropertyInfo, List<ZGuid>>();
		foreach (var ruleInfo in leafRules)
		{
			var checker = ruleInfo.Rule.GetValidationToolChecker(job);
			var fieldToDisplayValidationZPropertyInfo = checker?.FieldToDisplayValidationZPropertyInfo;
			if (fieldToDisplayValidationZPropertyInfo is null)
			{
				continue;
			}

			InjectFieldRule(fieldToDisplayValidationZPropertyInfo, ruleInfo.Rule, fieldToDisplayValidationZPropertyInfo.BizObj);
		}

		if (job is null)
		{
			return;
		}

		job.HasChangesChanged += (_, args) =>
		{
			if (args.ObjectThatWasChanged is not BusinessObject { IsDeleted: false } objectThatWasChanged)
			{
				return;
			}

			var tableName = objectThatWasChanged is CustomBusinessObject customBusinessObject
				? customBusinessObject.Parent.TableName
				: objectThatWasChanged.TableName;
			if (tableName == JobTableName)
			{
				return;
			}

			foreach (var ruleInfo in leafRules.Where(x => x.TableName == tableName))
			{
				var zPropertyInfo = ruleInfo.FiledToDisplayValidationStaticInfo.GetZPropertyInfo(objectThatWasChanged);
				if (zPropertyInfo is null)
				{
					continue;
				}

				InjectFieldRule(zPropertyInfo, ruleInfo.Rule, objectThatWasChanged);
			}
		};

		return;

		void InjectFieldRule(ZPropertyInfo zPropertyInfo, ProcessTemplateValidation rule, BusinessObject target)
		{
			if (leafRuleRegistration.TryGetValue(zPropertyInfo, out var list) && list.Contains(rule.PK))
			{
				return;
			}

			zPropertyInfo.AdditionalValidation += () =>
			{
				RuleFieldRule(rule, target);
			};
			list ??= new List<ZGuid>();
			list.Add(rule.PK);
			leafRuleRegistration.Add(zPropertyInfo, list);
		}
	}

	void RuleFieldRule(ProcessTemplateValidation rule, BusinessObject target)
	{
		if (target is null)
		{
			return;
		}

		var checker = rule.GetValidationToolChecker(job);
		if (checker is null)
		{
			return;
		}

		var bizObj = GetRealBizObj();
		if (bizObj is null || target != bizObj)
		{
			return;
		}

		var result = checker.EvaluateRule();
		if (result.Passed)
		{
			return;
		}

		result.DisplayValidationOnField();

		return;

		BusinessObject GetRealBizObj()
		{
			var fieldToDisplayValidationZPropertyInfo = checker.FieldToDisplayValidationZPropertyInfo;
			var businessObject = fieldToDisplayValidationZPropertyInfo?.BizObj;
			businessObject = businessObject is CustomBusinessObject customBizObj ? customBizObj.Parent : businessObject;
			return businessObject;
		}
	}

	sealed record RuleInfo(
		ProcessTemplateValidation Rule,
		FiledToDisplayValidationStaticInfo FiledToDisplayValidationStaticInfo,
		ZString TableName)
	{
		public ProcessTemplateValidation Rule { get; } = Rule;
		public FiledToDisplayValidationStaticInfo FiledToDisplayValidationStaticInfo { get; } = FiledToDisplayValidationStaticInfo;
		public ZString TableName { get;  } = TableName;
	}
}
