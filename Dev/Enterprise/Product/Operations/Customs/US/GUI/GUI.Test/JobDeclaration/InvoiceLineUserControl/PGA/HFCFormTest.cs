using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(HFCForm))]
	sealed class HFCFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new HFCForm(Factory.New<USHFCHeader>());
	}
}
