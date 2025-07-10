using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(DocumentContainerForm))]
	public class DocumentContainerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cartage = Factory.New<CommonCartage>();
			DocumentContainerOptions options = new DocumentContainerOptions(new DocumentContainerCollection(cartage.Containers, Factory));
			return new DocumentContainerForm(options);
		}
	}
}
