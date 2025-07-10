using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ShipmentsOfConsolFilter))]
	class ShipmentsOfConsolFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentsOfConsolFilter("moo", () => new ArrayList());
		}
	}
}
