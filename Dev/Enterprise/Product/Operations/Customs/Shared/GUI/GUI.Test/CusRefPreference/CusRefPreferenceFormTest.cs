using System.Windows.Forms;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusRefPreferenceForm))]
	sealed class CusRefPreferenceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusRefPreferenceForm(Factory.New<CusRefPreference>());
	}
}
