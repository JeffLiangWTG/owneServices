using System.Windows.Forms;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Orders.DataTransfer.Testing
{
	[TestedType(typeof(CsvOrderDataImporterForm))]
	public class CsvOrderDataImporterFormTest : DataImporterFormTest
	{
		protected override DataImporterForm NewDataImporterForm()
		{
			return CsvOrderDataImporterForm.Create(BillingInterfaceName.Test);
		}

		protected override Form GetFormToBashCore()
		{
			return NewDataImporterForm();
		}

		protected override string ExpectedFormCaption
		{
			get { return "Order CSV Data Importer"; }
		}
	}
}
