using System.Windows.Forms;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(WhsSalesChannelEntryForm))]
	public class WhsSalesChannelEntryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new WhsSalesChannelEntryForm(Factory.New<WhsSalesChannel>());
		}
	}
}
