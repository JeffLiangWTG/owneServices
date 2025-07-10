using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class ForwardingConsolSpecificMatcher : ForwardingConsolMatcher
	{
		readonly ForwardingConsol[] consols;

		readonly List<ConsolFetchRule.MatchRule> usedRules;

		readonly int score;

		public ForwardingConsolSpecificMatcher(ForwardingConsol[] consols, int score, List<ConsolFetchRule.MatchRule> usedRules, BusinessObjectFactory factory, CommonConsolReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, references, logger, helper)
		{
			this.consols = consols;
			this.score = score;
			this.usedRules = usedRules;
		}

		protected override void BuildMatchingQueryAndMatchDelegates(CommonConsolReferences referencesParent)
		{
			var nonUsageJustToMakeSureQueryIsNotEmpty = new ZQuery();
			for (int i = 0; i < usedRules.Count; i++)
			{
				nonUsageJustToMakeSureQueryIsNotEmpty.AddToFilter(new ZQuery(usedRules[i].MatchKeyName, usedRules[i].MatchKeyValue));
			}

			AddPossibleMatch(nonUsageJustToMakeSureQueryIsNotEmpty, consol => score * 10000);

			base.BuildMatchingQueryAndMatchDelegates(referencesParent);
		}

		protected override ForwardingConsol[] GetBusinessObjectUsingModuleSpecificBusinessRules(CommonConsolReferences parent)
		{
			return consols;
		}

		protected override ForwardingConsol GetLatestParentIfApplicable(List<ForwardingConsol> parentsToLookThrough)
		{
			return parentsToLookThrough.OrderByDescending(consol => consol.JK_SystemCreateTimeUtc).First();
		}
	}
}
