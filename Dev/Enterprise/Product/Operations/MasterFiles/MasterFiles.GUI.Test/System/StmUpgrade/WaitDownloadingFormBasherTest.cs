using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(WaitDownloadingForm))]
	sealed class WaitDownloadingFormBasherTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new WaitDownloadingForm();
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}
	}
}
