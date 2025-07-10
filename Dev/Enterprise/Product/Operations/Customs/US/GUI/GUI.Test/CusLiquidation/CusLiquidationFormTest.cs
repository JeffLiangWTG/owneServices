using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(CusLiquidationForm))]
	sealed class CusLiquidationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusLiquidationForm(Factory.New<CusLiquidation>());
	}
}
