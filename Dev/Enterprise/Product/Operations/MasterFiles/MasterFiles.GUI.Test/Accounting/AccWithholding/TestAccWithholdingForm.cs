using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccWithholdingForm))]
	sealed class TestAccWithholdingForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccWithholding withholding = Factory.New<AccWithholding>();
			return new AccWithholdingForm(withholding);
		}
	}
}
