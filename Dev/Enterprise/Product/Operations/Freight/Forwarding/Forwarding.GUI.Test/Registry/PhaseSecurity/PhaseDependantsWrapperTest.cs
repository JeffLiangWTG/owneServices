using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PhaseDependantsWrapper))]
	public class PhaseDependantsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasChangesInDependants()
		{
			var firstDependants = new List<PhaseDependantsProvider.PhaseReadOnlyDependantImpl>();
			firstDependants.Add(GetNewDependant("frodo", "baggins", true, false));
			firstDependants.Add(GetNewDependant("bilbo", "baggins", false, true));

			var firstDependantsReadOnlyAndMandatoryReversed = new List<PhaseDependantsProvider.PhaseReadOnlyDependantImpl>();
			firstDependantsReadOnlyAndMandatoryReversed.Add(GetNewDependant("frodo", "baggins", false, true));
			firstDependantsReadOnlyAndMandatoryReversed.Add(GetNewDependant("bilbo", "baggins", true, false));

			var firstExtendedDependants = new List<PhaseDependantsProvider.PhaseReadOnlyDependantImpl>(firstDependants);
			firstExtendedDependants.Add(GetNewDependant("samwise", "gamgee", true, false));

			var secondDependants = new List<PhaseDependantsProvider.PhaseReadOnlyDependantImpl>();
			secondDependants.Add(GetNewDependant("rachel", "berry", false, true));
			secondDependants.Add(GetNewDependant("sue", "sylvester", true, false));

			PhaseDependantsWrapper wrapper = new PhaseDependantsWrapper(new DummyPhaseDependantsProvider(), firstDependants.Cast<IPhaseDependant>());
			AssertEquals(false, wrapper.HasChangesInDependants);

			wrapper.SelectedDependants = firstDependants.Cast<IPhaseDependant>();
			AssertEquals(false, wrapper.HasChangesInDependants);

			wrapper.SelectedDependants = firstDependantsReadOnlyAndMandatoryReversed.Cast<IPhaseDependant>();
			AssertEquals(true, wrapper.HasChangesInDependants);

			wrapper.SelectedDependants = firstExtendedDependants.Cast<IPhaseDependant>();
			AssertEquals(true, wrapper.HasChangesInDependants);

			wrapper.SelectedDependants = secondDependants.Cast<IPhaseDependant>();
			AssertEquals(true, wrapper.HasChangesInDependants);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PhaseDependantsWrapper(new DummyPhaseDependantsProvider(), Enumerable.Empty<IPhaseDependant>());
		}

		PhaseDependantsProvider.PropertyDependant GetNewDependant(string name, string description, bool readOnly, bool mandatory)
		{
			var dependant = new PhaseDependantsProvider.PropertyDependant(name, description);
			dependant.IsReadOnly = readOnly;
			dependant.IsMandatory = mandatory;

			return dependant;
		}

		#endregion
	}
}
