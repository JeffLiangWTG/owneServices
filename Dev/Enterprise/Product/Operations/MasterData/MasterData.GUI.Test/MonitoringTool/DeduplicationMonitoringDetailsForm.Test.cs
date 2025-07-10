using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(DeduplicationMonitoringDetailsForm))]
	public class DeduplicationMonitoringDetailsFormTest : ZFormBasherTest
	{
		#region Implementation

		protected sealed override Form GetFormToBashCore()
		{
			return new DeduplicationMonitoringDetailsForm(new DeduplicationDebugReporter("Blah"));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "detailsTextBox";
		}

		#endregion
	}
}
