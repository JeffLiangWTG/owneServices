using System.Windows.Forms;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.GUI.Testing
{
	[TestedType(typeof(CartageTypeForm))]
	public class CartageTypeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonCartageType localCartageJobType = Factory.New<CommonCartageType>();
			return new CartageTypeForm(localCartageJobType);
		}
	}
}
