using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefPremisesGateCodeForm))]
	sealed class RefPremisesGateCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefPremisesGateCodeForm(Factory.New<RefPremisesGateCode>());
		}
	}
}
