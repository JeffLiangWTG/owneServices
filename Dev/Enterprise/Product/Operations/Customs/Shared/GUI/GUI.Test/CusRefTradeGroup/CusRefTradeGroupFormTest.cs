using System.Windows.Forms;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusRefTradeGroupForm))]
	sealed class CusRefTradeGroupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusRefTradeGroupForm(Factory.New<CusRefTradeGroup>());
	}
}
