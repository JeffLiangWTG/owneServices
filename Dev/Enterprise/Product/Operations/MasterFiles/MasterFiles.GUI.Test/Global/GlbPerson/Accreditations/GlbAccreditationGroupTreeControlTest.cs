using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DummyAccreditationForm))]
	sealed class GlbAccreditationGroupTreeControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new DummyAccreditationForm();
			form.Controls.Add(new GlbAccreditationGroupTreeControlBase());
			return form;
		}

		public override void TestBashingForm()
		{
			// Ignore this test
		}
	}
}
