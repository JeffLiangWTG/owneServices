using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseCollection))]
	public class PhaseCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PhaseCollection>
	{
		public void TestParent()
		{
			PhaseCollection collection = new PhaseCollection();
			Phase phase = collection.AddNew();
			AssertNull(collection.Parent);
			AssertNull(phase.Parent);

			PhaseSecurity security = new PhaseSecurity();
			collection = new PhaseCollection(security);
			phase = collection.AddNew();
			AssertEquals(security, collection.Parent);
			AssertEquals(security, phase.Parent);

			PhaseSecurity anotherSecurity = new PhaseSecurity();
			collection.Parent = anotherSecurity;
			AssertEquals(anotherSecurity, collection.Parent);
			AssertEquals(anotherSecurity, phase.Parent);
		}

		#region Implementation

		protected override PhaseCollection GetCollectionToTest()
		{
			return new PhaseCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Phase();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new PhaseCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
