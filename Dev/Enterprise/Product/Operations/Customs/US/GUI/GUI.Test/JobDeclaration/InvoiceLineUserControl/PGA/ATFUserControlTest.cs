using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ATFUserControl>))]
	sealed class ATFUserControlTest : ZPGAFormBasherAbstractTest<ATFUserControl>
	{
		public void TestATFGroupBoxText()
		{
			using (var testForm = new ZChildForm())
			using (Control atfUserControl = new ATFUserControl())
			{
				testForm.Controls.Add(atfUserControl);
				testForm.Show();
				var correctText = "Bureau of Alcohol, Tobacco, Firearms and Explosives - ATF";
				var control = atfUserControl.Controls.Find("ATFGroupBox", true)[0];
				AssertNotNull("Expected:the ATFGroupBox is not null", control);
				Assert("Expected:the ATFGroupBox's text is correct", correctText.CompareTo(control.Text) == 0);
			}
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var atfLine = invoiceLine.ATFLines.AddNew();
			atfLine.US_CategoryCode = ATFCategoryCodeList.Codes.ESP;
			atfLine.US_Quantity = 5m;
			return atfLine;
		}

		protected override string BindMember => "FilteredInvoiceLines.ATFLines";
	}

	sealed class ATFGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<ATFUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.ATFLines;

		protected override ZGrid GetGrid(ATFUserControl control) => control.ATFGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((ATFCollection)collection).AddNew();
	}
}
