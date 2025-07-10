using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Shared.Testing
{
	public class AutoCreatorTargetModulesTest : TestCase
	{
		public void TestCorrectCount()
		{
			var list = new AutoCreatorTargetModules().List;
			AssertEquals(2, list.Count);
		}

		public void TestHasCorrectElements()
		{
			var list = new AutoCreatorTargetModules().List;
			AssertCollectionContains(new CodeDescriptionPair("PTR", "Target the Port Transport Module"), list);
			AssertCollectionContains(new CodeDescriptionPair("GCN", "Target the Land Transport Module"), list);
		}
	}
}
