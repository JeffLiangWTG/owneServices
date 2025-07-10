using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ControllingBranchesForm))]
	sealed class ControllingBranchesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ControllingBranchesForm(Factory.New<OrgHeader>());
		}
	}
}
