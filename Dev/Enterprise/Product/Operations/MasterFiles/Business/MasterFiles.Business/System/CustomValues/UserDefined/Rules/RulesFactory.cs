using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using WF = CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	class RulesFactory : WF.RuleFactory<ICustomAddOnRule>
	{
		protected override ICustomAddOnRule NewCheckEnteredRule() => new CheckEnteredRule();

		protected override IRuleProvider<string> DateTimeFormatRuleProvider => new DateTimeFormatRuleProviderImpl();

		class DateTimeFormatRuleProviderImpl : IRuleProvider<string>
		{
			public ICustomAddOnRule Create(string args)
			{
				var result = new DateTimeFormatRule();
				if (Enum.TryParse<KDateTimeFormat>(args, out var kFormat))
				{
					result.Format = kFormat;
				}
				return result;
			}

			public string GetArgs(ICustomAddOnRule rule) => ((DateTimeFormatRule)rule).Format.ToString();
		}

		protected override IRuleProvider<CreateEventRuleArgs> CreateEventRuleProvider => new CreateEventRuleProviderImpl();

		class CreateEventRuleProviderImpl : IRuleProvider<CreateEventRuleArgs>
		{
			public ICustomAddOnRule Create(CreateEventRuleArgs args)
			{
				var result = new CreateEventRule();
				result.EventCode = args.EventCode ?? ZString.Empty;
				result.EventReference = args.EventReference ?? ZString.Empty;
				result.IsEstimate = args.IsEstimate;
				return result;
			}

			public CreateEventRuleArgs GetArgs(ICustomAddOnRule rule)
			{
				var typedRule = (CreateEventRule)rule;
				return new CreateEventRuleArgs
				{
					EventCode = typedRule.EventCode,
					EventReference = typedRule.EventReference,
					IsEstimate = typedRule.IsEstimate,
				};
			}
		}

		protected override IRuleProvider<IEnumerable<KeyValuePair<string, string>>> InvalidCodeRuleProvider => new InvalidCodeRuleProviderImpl();

		class InvalidCodeRuleProviderImpl : IRuleProvider<IEnumerable<KeyValuePair<string, string>>>
		{
			public ICustomAddOnRule Create(IEnumerable<KeyValuePair<string, string>> args)
			{
				var result = new InvalidCodeRule();
				if (args != null)
				{
					var list = new CodeDescriptionPairList();
					foreach (var pair in args)
					{
						list.AddPair(pair.Key, pair.Value);
					}
					result.List = list;
				}
				return result;
			}

			public IEnumerable<KeyValuePair<string, string>> GetArgs(ICustomAddOnRule rule)
			{
				var result = ((InvalidCodeRule)rule).List?.OfType<CodeDescriptionPair>().Select(x => new KeyValuePair<string, string>(x.Code, x.Description));
				return result;
			}
		}
	}
}
