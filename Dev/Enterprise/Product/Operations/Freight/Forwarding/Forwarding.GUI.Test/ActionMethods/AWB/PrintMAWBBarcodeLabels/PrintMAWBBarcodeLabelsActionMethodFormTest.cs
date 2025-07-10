using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(PrintMAWBBarcodeLabelsActionMethodForm))]
	public class PrintMAWBBarcodeLabelsActionMethodFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			BulkMAWBLabelActions actions = new BulkMAWBLabelActions(Factory);
			return new PrintMAWBBarcodeLabelsActionMethodForm(actions);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
