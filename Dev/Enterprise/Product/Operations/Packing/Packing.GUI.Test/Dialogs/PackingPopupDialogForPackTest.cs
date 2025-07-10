using System.Windows.Forms;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	[TestedType(typeof(PackingPopupDialog))]
	public class PackingPopupDialogForPackTest : PackingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			return new PackingPopupDialog(packageJob);
		}
	}
}
