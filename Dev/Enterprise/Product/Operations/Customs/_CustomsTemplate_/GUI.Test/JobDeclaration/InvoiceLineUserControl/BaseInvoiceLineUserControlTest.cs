using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs._CustomsTemplate_.GUI.Testing
{
	class BaseInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffColumnNotExists_UniversalTariffFalse()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
			}
		}
	}
}
