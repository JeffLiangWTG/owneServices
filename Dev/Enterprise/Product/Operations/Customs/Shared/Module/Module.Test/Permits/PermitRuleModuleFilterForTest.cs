using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	class PermitRuleModuleFilterForTest : PermitRuleModuleFilter
	{
		public PermitRuleModuleFilterForTest(ZString description, GetPermitRuleQuery queryDelegate) : base(description, queryDelegate, GetCountries)
		{
		}

		static IList GetCountries()
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair("ZA", "South Africa")
			};
		}

		public new PermitRuleCodeList PermitRuleCodes => new PermitRuleCodes_ForModuleFilterTest();

		sealed class PermitRuleCodes_ForModuleFilterTest : PermitRuleCodeList
		{
			public PermitRuleCodes_ForModuleFilterTest()
			{
				AddRange(new CodeDescriptionPairList()
				{
					new CodeDescriptionPair("TAR", "Tariff"), new CodeDescriptionPair("PRD", "Product")
				});
			}
		}
	}
}
