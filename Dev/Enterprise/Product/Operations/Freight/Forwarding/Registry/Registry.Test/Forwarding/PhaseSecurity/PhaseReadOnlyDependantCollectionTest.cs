using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseDependantCollection))]
	public class PhaseReadOnlyDependantCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PhaseDependantCollection>
	{
		#region Implementation

		protected override PhaseDependantCollection GetCollectionToTest()
		{
			return new PhaseDependantCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PhaseDependant();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new PhaseDependantCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
