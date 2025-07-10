using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(EPAEditForm))]
	sealed class EPAEditFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new EPAEditForm(Factory.New<Vehicle>());
	}
}
