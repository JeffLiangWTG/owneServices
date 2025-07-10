using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ContainerLoadPlanSOGrouping))]
	class ContainerLoadPlanSOGroupingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var result = new ContainerLoadPlanSOGrouping("0001");
			result.DangerousGoods = new List<DangerousGood>();

			return result;
		}
	}
}
