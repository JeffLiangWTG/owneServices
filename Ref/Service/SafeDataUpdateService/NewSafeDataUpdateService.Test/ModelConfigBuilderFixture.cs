using System.Linq;
using Microsoft.OData.ModelBuilder;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class ModelConfigBuilderFixture
	{
		[TestCase("GetWithOptimizedExpand", "Collection(CargoWise.RefDbRepo.Service.Schema_0_9_New.RefAccTaxRate)")]
		public void IsProcedureAvailable(string procedureName, string returnType)
		{
			Assert.True(builder.Operations.Any(x => x.Name == procedureName && x.ReturnType.Name == returnType));
		}

		[SetUp]
		public void Setup()
		{
			builder = new ODataConventionModelBuilder();
			ModelConfig.RegisterModels(builder);
		}

		ODataConventionModelBuilder builder;
	}
}
