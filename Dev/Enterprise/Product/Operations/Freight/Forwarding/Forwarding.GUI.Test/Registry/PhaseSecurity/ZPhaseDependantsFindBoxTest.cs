using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ZPhaseDependantsFindBoxTest : TestCase
	{
		public void TestSelectFromPopupForm()
		{
			using (ZPhaseDependantsFindBoxForTesting findBox = new ZPhaseDependantsFindBoxForTesting())
			{
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				findBox.SelectFromPopupForm();
				AssertNull("No dialogs shown", ZFormModaliser.LastFormShownDialogForTest);

				PhaseSecurity phaseSecurity = new PhaseSecurity(new ZArchitecture.Core.CodeDescriptionPairList(), new DummyPhaseDependantsProvider());
				PhaseRule phaseRule = phaseSecurity.Phases.AddNew().Rules.AddNew();
				phaseRule.Dependants.AddNew().Name = "Gandalf";
				AssertEquals("Precondition", 1, phaseRule.Dependants.Count);

				Assert("FindBox button should be clickable", !findBox.PopupButton.ReadOnly && findBox.PopupButtonReadonlyCanBeDifferent);

				findBox.PhaseRule = phaseRule;
				findBox.SelectFromPopupForm();
				AssertNotNull("Correct dialog was shown", ZFormModaliser.LastFormShownDialogForTest as PhaseDependantsSelectionDialog);

				ZString[] expectedDependants = new ZString[] { "Frodo", "Bilbo" };
				AssertContainsExactElementsInAnyOrder("Dependants were updated", expectedDependants, phaseRule.ReadOnlyDependants.Cast<IPhaseDependant>().Select(x => x.Name));
			}
		}

		public void TestNoMissingListExceptionIsThrown()
		{
			using (ZPhaseDependantsFindBox findBox = new ZPhaseDependantsFindBox())
			{
				findBox.SetDataBinding(new DummyForFindBoxTest(), "TheProperty");
				AssertNull("List is accessed w/o 'missing list' exception", findBox.List);
			}
		}

		#region Implementation

		public class ZPhaseDependantsFindBoxForTesting : ZPhaseDependantsFindBox
		{
			public ZPhaseDependantsFindBoxForTesting()
				: base()
			{
			}

			protected override PhaseDependantsWrapper GetPhaseDependantsWrapper()
			{
				PhaseDependantsWrapper wrapper = new PhaseDependantsWrapperForTesting(PhaseRule.Parent.Parent.DependantsProvider, ((IPhaseRule)PhaseRule).ReadOnlyDependants);
				AssertEquals("Precondition: wrapper has changes in dependants (as they were done in GUI)", true, wrapper.HasChangesInDependants);

				return wrapper;
			}
		}

		class PhaseDependantsWrapperForTesting : PhaseDependantsWrapper
		{
			public PhaseDependantsWrapperForTesting(PhaseDependantsProvider dependantsProvider, IEnumerable<IPhaseDependant> dependants)
				: base(dependantsProvider, dependants)
			{
				var selected = new List<PhaseDependantsProvider.PropertyDependant>();
				selected.Add(new PhaseDependantsProvider.PropertyDependant("Frodo", "Young hobbit") { IsReadOnly = true });
				selected.Add(new PhaseDependantsProvider.PropertyDependant("Bilbo", "Old hobbit") { IsReadOnly = true });

				SelectedDependants = selected.Cast<IPhaseDependant>();
			}
		}

		class DummyForFindBoxTest
		{
			public DummyForFindBoxTest()
			{
			}

			public string TheProperty { get; set; }
		}

		#endregion
	}
}
