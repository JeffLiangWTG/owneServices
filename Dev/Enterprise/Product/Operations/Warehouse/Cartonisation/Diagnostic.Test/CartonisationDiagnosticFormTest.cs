using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	[TestedType(typeof(CartonisationDiagnosticForm))]
	internal class CartonisationDiagnosticFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CartonisationDiagnosticForm();
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "ResultsTextBox" || base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
