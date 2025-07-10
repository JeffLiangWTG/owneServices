using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseDependant))]
	public class PhaseReadOnlyDependantTest : RegistryBusinessObjectTemplateTestCase<PhaseDependant>
	{
		public void TestCtor()
		{
			var dependant = new PhaseDependant(null);
			AssertEquals("", dependant.Name);
			AssertEquals("", dependant.Description);
			AssertEquals("", dependant.DependantType);

			dependant.Name = "hello";
			dependant.Description = "world";
			dependant.DependantType = "!";
			dependant.IsReadOnly = true;

			var anotherDependant = new PhaseDependant(dependant);
			AssertEquals("hello", anotherDependant.Name);
			AssertEquals("world", anotherDependant.Description);
			AssertEquals("!", anotherDependant.DependantType);
			Assert(anotherDependant.IsReadOnly);
			Assert(!anotherDependant.IsMandatory);

			dependant.IsReadOnly = false;
			dependant.IsMandatory = true;

			anotherDependant = new PhaseDependant(dependant);
			Assert(!anotherDependant.IsReadOnly);
			Assert(anotherDependant.IsMandatory);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PhaseDependant();
		}

		protected override PhaseDependant GetBusinessObjectToClone()
		{
			return (PhaseDependant)GetNewBusinessObject();
		}

		protected override PhaseDependant GetBusinessObjectToSerialise()
		{
			return (PhaseDependant)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
