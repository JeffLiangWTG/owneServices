using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseSecurityRegistryItem))]
	public class PhaseSecurityRegistryItemTest : StronglyTypedRegistryItemTestCase<PhaseSecurity>
	{
		protected override StronglyTypedRegistryItem<PhaseSecurity, PhaseSecurity> GetNewRegistryItem()
		{
			return new PhaseSecurityRegistryItem(string.Empty, null, null, null, new CodeDescriptionPairList(), null);
		}
	}
}
