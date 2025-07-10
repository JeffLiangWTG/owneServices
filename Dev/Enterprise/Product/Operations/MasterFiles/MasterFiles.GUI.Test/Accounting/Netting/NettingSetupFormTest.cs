using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingSetupForm))]
	sealed class NettingSetupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NettingSetupForm(new NettingSetupManager(Factory, new ZGuid[] { ZGuid.Empty }));
		}
	}
}
