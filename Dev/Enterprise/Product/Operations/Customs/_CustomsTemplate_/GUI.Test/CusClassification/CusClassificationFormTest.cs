using System.Windows.Forms;
using Enterprise.Customs._CustomsTemplate_.Business;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.GUI.Testing
{
	[TestedType(typeof(CusClassificationForm))]
	sealed class CusClassificationFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusClassificationForm(Factory.New<CusClassification>());
	}
}
