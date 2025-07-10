using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(StatDescriptionForm))]
	sealed class StatDescriptionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new StatDescriptionForm("NAFTA");
	}
}
