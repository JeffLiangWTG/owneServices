using System.Windows.Forms;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(WhsPutawayGroupEntryForm))]
	public class WhsPutawayGroupEntryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new WhsPutawayGroupEntryForm(Factory.New<WhsPutawayGroup>());
		}
	}
}
