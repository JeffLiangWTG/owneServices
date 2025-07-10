using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(CostingDataContextManager))]
	public class CostingDataContextManagerTestCase : RatingHeaderDataContextManagerTestCase<CostingDataContextManager, Costing>
	{
		protected override Costing GetBusinessObjectForTesting() => Factory.NewWithValidTestData<Costing>();

		protected override RatingHeaderDataContextManager<Costing> GetNewContextManagerForTesting() => new CostingDataContextManager();
	}
}
