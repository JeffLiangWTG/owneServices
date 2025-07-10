using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class RateCodeLoadCriteriaTest : TestCase
	{
		public void TestDefaultValues()
		{
			IRateCodeLoadCriteria loadCriteria = new RateCodeLoadCriteria();
			CombineAssertions(() =>
			{
				AssertNull("InternalUseRate", loadCriteria.InternalUseRate);
				AssertNull("RateTypesToInclude", loadCriteria.RateTypesToInclude);
				AssertNull("RateTypesToExclude", loadCriteria.RateTypesToExclude);
				var filter = new ZQuery();
				loadCriteria.AddToRateCodeFilter(filter);
				AssertEquals("Default RateCodeLoadCriteria AddToRateCodeFilter() does not add any filters.", true, filter.IsEmpty);
			}

			);
		}

		public void TestInterfaceMembers()
		{
			IRateCodeLoadCriteria loadCriteria = new RateCodeLoadCriteria()
			{ InternalUseRate = true, RateTypesToInclude = new ZString[] { "RT1", "RT2" }, RateTypesToExclude = new ZString[] { "RT3" } };
			CombineAssertions(() =>
			{
				AssertEquals("InternalUseRate", true, loadCriteria.InternalUseRate);
				AssertEquals("RateTypesToInclude[0]", "RT1", loadCriteria.RateTypesToInclude[0]);
				AssertEquals("RateTypesToInclude[1]", "RT2", loadCriteria.RateTypesToInclude[1]);
				AssertEquals("RateTypesToExclude[0]", "RT3", loadCriteria.RateTypesToExclude[0]);
				var filter = new ZQuery();
				loadCriteria.AddToRateCodeFilter(filter);
				AssertEquals("Filter", "ZY1_InternalUse = 1 and (ZY1_RateType in ('RT1', 'RT2')) and ZY1_RateType <> 'RT3'", filter.LiteralTextADO);
			}

			);
		}
	}
}
