using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(RefDataForm))]
	sealed class RefDataFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefDataForm();
		}
	}
}
