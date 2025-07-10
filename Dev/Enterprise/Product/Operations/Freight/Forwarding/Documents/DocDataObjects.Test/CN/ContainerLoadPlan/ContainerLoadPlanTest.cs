using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ContainerLoadPlan))]
	sealed class ContainerLoadPlanTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var loadPlan = new ContainerLoadPlan("", "", "")
			{
				Containers = System.Array.Empty<ContainerLoadPlanContainer>()
			};

			return loadPlan;
		}
	}
}
