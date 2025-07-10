using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationRelatedShipmentFilter))]
	class JobDeclarationRelatedShipmentFilterTest : ModuleFilterTestCase<JobDeclarationRelatedShipmentFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			Assert(!Filter.Query.IsEmpty);
		}

		protected override JobDeclarationRelatedShipmentFilter GetNewModuleFilter()
		{
			return new JobDeclarationRelatedShipmentFilter(Factory);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ZString ExpectedDescription => DeclarationFilterConstants.RelatedShipment;
	}
}
