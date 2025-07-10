using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ContainerLoadPlanContainer))]
	sealed class ContainerLoadPlanContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var loadPlan = new ContainerLoadPlan("", "", "");
			var container = new ContainerLoadPlanContainer(loadPlan, new DocDataObjects.CommonContext(new BusinessObjectFactory()), "cont111");

			loadPlan.Containers = new ContainerLoadPlanContainer[]
			{
				container
			};

			container.Groups = new List<ContainerLoadPlanSOGrouping>();

			return container;
		}
	}
}
