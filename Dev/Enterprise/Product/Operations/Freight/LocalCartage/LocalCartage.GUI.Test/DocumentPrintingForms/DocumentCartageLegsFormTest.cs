using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(DocumentCartageLegsForm))]
	public class DocumentCartageLegsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DocumentCartageLegOptions options = new DocumentCartageLegOptions(new DocumentCartageLegCollection(new CommonCartageLegCollection(Factory)));
			return new DocumentCartageLegsForm(options);
		}
	}
}
