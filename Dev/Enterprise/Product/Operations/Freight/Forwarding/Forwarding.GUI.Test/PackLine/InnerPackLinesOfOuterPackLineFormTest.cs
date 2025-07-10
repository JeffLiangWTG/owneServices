using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(InnerPackLinesOfOuterPackLineForm))]
	class InnerPackLinesOfOuterPackLineFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();

			Factory.Save();

			return new InnerPackLinesOfOuterPackLineForm(packline);
		}
	}
}
