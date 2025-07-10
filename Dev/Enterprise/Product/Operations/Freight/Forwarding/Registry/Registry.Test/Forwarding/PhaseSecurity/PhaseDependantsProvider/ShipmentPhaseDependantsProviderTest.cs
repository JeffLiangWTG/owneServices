using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public class ShipmentPhaseDependantsProviderTest : PhaseDependantsProviderTestCase
	{
		public void TestGetIZTypeProperties()
		{
			ShipmentPhaseDependantsProvider provider = new ShipmentPhaseDependantsProvider();
			CombineAssertions("Contains shipment properties", () =>
			{
				Assert(provider.GetIZTypeProperties().Any(x => x.Name == "JS_TransportMode"));
				Assert(provider.GetIZTypeProperties().Any(x => x.Name == "JS_ShipmentType"));
				Assert(provider.GetIZTypeProperties().Any(x => x.Name == "JS_HouseBill"));
			});

			int propertiesCount = provider.GetIZTypeProperties().Count();
			AssertEquals("Expecting more than 100 public writeable IZType properties on ForwardingShipment", true, propertiesCount > 100);
		}

		public void TestGetChildDependants()
		{
			ZString[] expectedChildDependants = new ZString[]
			{
				"InnerPackLines",
				"OuterPackLines",
				"PickupConfirms",
				"DeliveryConfirms"
			};

			ShipmentPhaseDependantsProvider provider = new ShipmentPhaseDependantsProvider();
			AssertContainsExactElementsInAnyOrder(expectedChildDependants, provider.GetChildDependants().Select(x => x.Name));
		}

		public void TestGetChildExpandableDependants()
		{
			var expandableDependants = new ShipmentPhaseDependantsProvider().GetChildExpandableDependants();
			AssertEquals(1, expandableDependants.Count);

			IPhaseDependant docsAndCartageKey = expandableDependants.Keys.FirstOrDefault(key => key.Name == "DocsAndCartage");
			AssertNotNull("Contains DocsAndCartage", docsAndCartageKey);
			AssertEquals("Contains DocsAndCartage properties", true, expandableDependants[docsAndCartageKey].Any(x => x.Name.Contains("JP_")));
		}

		#region Implementation

		protected override PhaseDependantsProvider GetDependantsProvider()
		{
			return new ShipmentPhaseDependantsProvider();
		}

		protected override Type GetParentType()
		{
			return ObjectFactory.GetType<Integration.Forwarding.IForwardingShipment>();
		}

		#endregion
	}
}
