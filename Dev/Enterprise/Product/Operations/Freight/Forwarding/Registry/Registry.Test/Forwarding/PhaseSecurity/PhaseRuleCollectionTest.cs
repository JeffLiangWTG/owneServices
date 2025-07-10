using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseRuleCollection))]
	public class PhaseRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PhaseRuleCollection>
	{
		public void TestParent()
		{
			PhaseRuleCollection collection = new PhaseRuleCollection();
			PhaseRule rule = collection.AddNew();
			AssertNull(collection.Parent);
			AssertNull(rule.Parent);

			Phase phase = new Phase();
			collection = new PhaseRuleCollection(phase);
			rule = collection.AddNew();
			AssertEquals(phase, collection.Parent);
			AssertEquals(phase, rule.Parent);

			Phase anotherPhase = new Phase();
			collection.Parent = anotherPhase;
			AssertEquals(anotherPhase, collection.Parent);
			AssertEquals(anotherPhase, rule.Parent);
		}

		#region Implementation

		protected override PhaseRuleCollection GetCollectionToTest()
		{
			return new PhaseRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PhaseRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new PhaseRuleCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
