using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCarrierFilterStripBusinessObject))]
	public class ZZRefCarrierFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ZZRefCarrierFilterStripBusinessObject();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(ZZRefCarrierAttributeCombinedSchema.Constants.TableName, Constants.ZZRefCarrierFilters.TransportMode));
			return result;
		}
	}
}
