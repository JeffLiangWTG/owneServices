using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(DocumentImportCargoForm))]
	public class DocumentImportCargoFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentImportCargoLabel doc = new DocumentImportCargoLabel(consol);

			return new DocumentImportCargoForm(doc);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
