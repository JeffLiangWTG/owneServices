using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PackProductsForm))]
	public class PackProductsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ForwardingPackLine packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			packline.Products.AddNew();
			Factory.Save();
			return new PackProductsForm(packline);
		}
	}
}
