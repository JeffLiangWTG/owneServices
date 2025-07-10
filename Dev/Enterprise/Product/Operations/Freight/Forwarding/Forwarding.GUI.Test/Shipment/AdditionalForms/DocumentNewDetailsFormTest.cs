using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(DocumentNewDetailsForm))]
	public class DocumentNewDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocumentShipment doc = new DocumentShipment(shipment, Core.Constants.DataContext.LetterOfIndemnity);
			return new DocumentNewDetailsForm(doc);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
