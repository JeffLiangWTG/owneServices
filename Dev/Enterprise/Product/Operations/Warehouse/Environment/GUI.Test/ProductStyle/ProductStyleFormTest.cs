using System.Windows.Forms;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(ProductStyleForm))]
	public class ProductStyleFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ProductStyleForm(Factory.New<WhsProductStyle>());
		}
	}
}
