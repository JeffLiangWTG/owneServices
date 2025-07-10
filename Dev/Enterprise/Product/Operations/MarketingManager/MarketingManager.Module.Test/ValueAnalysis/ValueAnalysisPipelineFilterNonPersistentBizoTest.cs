using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module
{
	[TestedType(typeof(ValueAnalysisPipelineFilter))]
	class ValueAnalysisPipelineFilterNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ValueAnalysisPipelineFilter("moo", OrgTradeValueSchema.PAV_Revenue);
		}
	}
}
