using System.Windows.Forms;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusRefRateCodeForm))]
	sealed class CusRefRateCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusRefRateCodeForm(Factory.NewWithValidTestData<CusRefRateCodeForTesting>());
	}
}
