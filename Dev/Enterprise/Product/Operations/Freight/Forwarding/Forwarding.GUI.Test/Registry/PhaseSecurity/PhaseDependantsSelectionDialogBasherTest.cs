using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PhaseDependantsSelectionDialog))]
	internal class PhaseDependantsSelectionDialogBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var wrapper = new PhaseDependantsWrapper(new DummyPhaseDependantsProvider(), Enumerable.Empty<IPhaseDependant>());
			return new PhaseDependantsSelectionDialog(wrapper);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "DependantsTreeView")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		#endregion
	}
}
