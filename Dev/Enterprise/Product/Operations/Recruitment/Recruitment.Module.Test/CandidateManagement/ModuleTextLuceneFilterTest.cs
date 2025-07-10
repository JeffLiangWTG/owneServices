using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Recruitment.Module.CandidateManagement;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(ModuleTextLuceneFilter))]
	sealed class ModuleTextLuceneFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldReevaluateQuery()
		{
			var filter = new ModuleTextLuceneFilterForTest(
				"Resume Keywords",
				(SQLComparisonOperator comparisonOperator, ZString keywords) => ZQuery.NoResultQuery);

			Assert(!filter.ShouldReevaluateQuery_Exposed());
		}

		protected override BusinessObject GetNewBusinessObject()
			=> new ModuleTextLuceneFilter(
				"foo",
				(SQLComparisonOperator comparisonOperator, ZString keywords) => ZQuery.NoResultQuery);
	}
}
