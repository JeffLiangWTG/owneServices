using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(DOTForm))]
	sealed class DOTFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new DOTForm(Factory.New<DOT>());
	}
}
