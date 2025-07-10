using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(SplitForm))]
	sealed class SplitTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			PackLine line = Factory.New<PackLine>();
			CommonContainer container = Factory.New<CommonContainer>();

			PackLineSplitter splitter = new PackLineSplitter(line, container);
			return new SplitForm(splitter);
		}
	}
}
