using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Business
{
	[Immutable]
	public class PostcodeFormattingRulesProvider : IPostcodeFormattingRulesProvider
	{
		const string JsonFileName = "Enterprise.MasterData.Business.PostcodeValidation.PostcodeFormattingRules.json";
		static readonly Lazy<ImmutableDictionary<string, PostcodeFormattingRule>> rules = new Lazy<ImmutableDictionary<string, PostcodeFormattingRule>>(ReadRules);

		PostcodeFormattingRulesProvider()
		{
		}

		public PostcodeFormattingRule GetRuleFromIso(string iso)
		{
			rules.Value.TryGetValue(iso, out var postcodeFormattingRule);

			return postcodeFormattingRule;
		}

		static ImmutableDictionary<string, PostcodeFormattingRule> ReadRules()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream(JsonFileName))
			{
				var reader = new StreamReader(stream);
				var jsonString = reader.ReadToEnd();
				return JsonConvert.DeserializeObject<PostcodeFormattingRule[]>(jsonString).ToImmutableDictionary(u => u.Iso);
			}
		}

#if DEBUG
		public List<PostcodeFormattingRule> Rules => rules.Value.Select(u => u.Value).ToList();
#endif
	}
}
