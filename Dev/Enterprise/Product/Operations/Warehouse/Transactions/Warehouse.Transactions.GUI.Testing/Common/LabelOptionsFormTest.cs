using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(LabelOptionsForm))]
	public class LabelOptionsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			WhsDocketLabelControl docketLabel = new WhsDocketLabelControl(order, 1);
			return new LabelOptionsForm(docketLabel);
		}
	}
}
