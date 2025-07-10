using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACCaseQueryForm))]
	sealed class ACCaseQueryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ACCaseQueryForm(new ACEACCaseQuery(Factory));
	}
}
