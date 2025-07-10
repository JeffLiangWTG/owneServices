using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	[TestedType(typeof(DemandeDeTracing))]
	sealed class DemandeDeTracingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DemandeDeTracing("ForwardingConsol", "C00001015")
			{
				Containers = new List<DemandeDeTracingContainer>()
				{
					new DemandeDeTracingContainer(ZGuid.Empty) { ContainerNumber = "ABCD0008" }
				}
			};
		}
	}
}
