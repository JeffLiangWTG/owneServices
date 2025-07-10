using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(ContainerPackingInfo))]
	sealed class ContainerPackingInfoTest : NonPersistentBusinessObjectTestCase
	{
	}
}
