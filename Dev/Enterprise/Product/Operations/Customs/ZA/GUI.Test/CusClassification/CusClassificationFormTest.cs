using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(CusClassificationForm))]
	sealed class CusClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<CusClassification>());
	}
}
