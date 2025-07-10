using Enterprise.Freight.Agency.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Test
{
	[TestedType(typeof(DummyDetentionStrategy))]
	[UseDummyDetentionStrategy]
	internal class DummyDetentionStrategyTest : DetentionStrategyBaseTest<DummyDetentionStrategy>
	{
		#region Implementation
		protected override string[] GetDetentionTypes()
		{
			return new string[] { "" };
		}
		#endregion
	}
}
