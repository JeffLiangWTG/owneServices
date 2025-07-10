using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCACCaseForm))]
	sealed class USCACCaseFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new USCACCaseForm(Factory.New<USCACCase>());
	}
}
