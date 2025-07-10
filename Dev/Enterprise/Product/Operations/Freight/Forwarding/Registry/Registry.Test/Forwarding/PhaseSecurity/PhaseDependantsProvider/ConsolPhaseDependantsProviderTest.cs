using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public class ConsolPhaseDependantsProviderTest : PhaseDependantsProviderTestCase
	{
		public void TestGetChildDependants()
		{
			ZString[] expectedChildDependants = new ZString[]
			{
				"Containers",
				"Shipments",
				"Routing"
			};

			ConsolPhaseDependantsProvider provider = new ConsolPhaseDependantsProvider();
			AssertContainsExactElementsInAnyOrder(expectedChildDependants, provider.GetChildDependants().Select(x => x.Name));
		}

		protected override ZString[] ChildDependantsThatHaveDifferentExposedNames
		{
			get { return new ZString[] { "Routing" }; }
		}

		#region Implementation

		protected override PhaseDependantsProvider GetDependantsProvider()
		{
			return new ConsolPhaseDependantsProvider();
		}

		protected override Type GetParentType()
		{
			return ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>();
		}

		#endregion
	}
}
