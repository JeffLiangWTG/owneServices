using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ConsolsOfShipmentFilter))]
	class ConsolsOfShipmentFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolsOfShipmentFilter("moo", () => new ArrayList());
		}
	}
}
