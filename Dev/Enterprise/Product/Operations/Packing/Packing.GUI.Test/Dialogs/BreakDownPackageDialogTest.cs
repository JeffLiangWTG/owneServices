using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	[TestedType(typeof(BreakDownPackageDialog))]
	sealed class BreakDownPackageDialogTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BreakDownPackageDialog(new PkgPackageCollection((Factory.New<PkgPackageJob>())));
		}
	}
}
