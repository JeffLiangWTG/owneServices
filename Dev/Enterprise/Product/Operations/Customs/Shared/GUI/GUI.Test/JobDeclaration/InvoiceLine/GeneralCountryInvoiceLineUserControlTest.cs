using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class GeneralCountryInvoiceLineUserControlTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<GeneralCountryInvoiceLineUserControl>
	{
		protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;
		public void TestTaxOrFeeDropEditMaxLength()
		{
			using (var form = new ZForm())
			using (var control = new GeneralCountryInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var taxOrFeeDropEdit = (ZDropEdit)control.Controls.Find("TaxOrFeeDropEdit", true).First();
				AssertEquals(4, taxOrFeeDropEdit.PreBoundMaxLength);
			}
		}
	}
}
