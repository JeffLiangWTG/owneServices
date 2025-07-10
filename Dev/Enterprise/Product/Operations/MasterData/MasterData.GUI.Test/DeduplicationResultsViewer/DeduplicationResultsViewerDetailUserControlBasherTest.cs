using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	public class DeduplicationResultsViewerDetailUserControlBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new ZEmptyFormForBasherTest();
			var resultsViewer = new DeduplicationResultsViewerDetailsUserControl();
			resultsViewer.Dock = DockStyle.Fill;
			form.CaptionRenderingEnabled = true;
			form.Size = ControlDpiScalingHelper.NewScaledSize(1200, 1200);
			form.Controls.Add(resultsViewer);
			form.ControllerID = DummyControllerIDs.Dummy;
			return form;
		}
	}
}
