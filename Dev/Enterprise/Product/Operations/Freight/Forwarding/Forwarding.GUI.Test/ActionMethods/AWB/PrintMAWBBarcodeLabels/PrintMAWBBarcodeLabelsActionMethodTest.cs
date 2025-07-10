using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PrintMAWBBarcodeLabelsActionMethod))]
	public class PrintMAWBBarcodeLabelsActionMethodTest : OperationalActionMethodTest<PrintMAWBBarcodeLabelsActionMethod>
	{
		public void TestOverrides()
		{
			AssertEquals("Print MAWB Barcode Labels", Method.Name);
			AssertEquals("Allows for the printing of Master Air Waybill barcode labels", Method.Description);
			AssertEquals(typeof(PrintMAWBBarcodeLabelsMethodApplicator), Method.NewApplicator(Factory, new AWBPrintSettings()).GetType());
			Assert(Method.HasSettings);
			AssertEquals(typeof(AWBPrintSettings), Method.NewSetting(Factory).GetType());
			using (Control control = (Control)Method.NewSettingsControl())
			{
				AssertEquals(typeof(AWBPrintSettingsControl), control.GetType());
			}
		}

		#region Implementation

		protected override PrintMAWBBarcodeLabelsActionMethod NewMethod()
		{
			return new PrintMAWBBarcodeLabelsActionMethod();
		}

		#endregion
	}
}
