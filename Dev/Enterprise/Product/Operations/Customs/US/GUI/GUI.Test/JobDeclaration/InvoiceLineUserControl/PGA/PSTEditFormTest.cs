using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(PSTEditForm))]
	sealed class PSTEditFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new PSTEditForm(Factory.New<Pesticide>());
	}
}
