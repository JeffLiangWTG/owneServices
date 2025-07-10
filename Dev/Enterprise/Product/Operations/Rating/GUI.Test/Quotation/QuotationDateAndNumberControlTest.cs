using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class QuotationDateAndNumberControlTest : TestCase
	{
		public void TestControlsVisibility()
		{
			using (var ctrl = new QuotationDateAndNumberControl())
			{
				Assert(ctrl.Controls.Cast<Control>().Count(x => x.Visible) == 2);
				ctrl.ShowQuoteCancellationReasonDropEdit();
				Assert(ctrl.Controls.Cast<Control>().Count(x => x.Visible) == 1);
			}
		}
	}
}
