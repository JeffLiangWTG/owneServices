using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SalesProductForm))]
	class SalesProductFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new SalesProductForm(salesProduct);
		}

		#endregion
	}
}
