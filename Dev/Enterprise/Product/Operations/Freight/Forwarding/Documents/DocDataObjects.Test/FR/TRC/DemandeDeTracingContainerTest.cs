using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	[TestedType(typeof(DemandeDeTracingContainer))]
	sealed class DemandeDeTracingContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DemandeDeTracingContainer(ZGuid.Empty)
			{
				ContainerNumber = "ABCD0007",
				IsNonOperativeReefer = false
			};
		}
	}
}
