using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(DummyPackLine))]
	sealed class DummyPackLineTest : NonPersistentBusinessObjectTestCase
	{
	}
}
