using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module
{
	[TestedType(typeof(ValueAnalysisQuantityFilter))]
	class ValueAnalysisQuantityFilterNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ValueAnalysisQuantityFilter("moo", OrgTradeValueSchema.PAV_Revenue, true);
		}
	}
}
