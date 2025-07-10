using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(ImportCustomsExchangeRatesFormForTest))]
	sealed class ImportCustomsExchangeRatesTest : ImportFromCSVFormBaseTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConfirmOKLoadData()
		{
			using (var tempFile = TempFile.NewFromFile(BaseSourcePath + TestFile))
			using (var testForm = new ImportCustomsExchangeRatesFormForTest())
			{
				testForm.Show();
				testForm.FileNameTextBox.Text = tempFile.Filename;
				testForm.StartButton.PerformClick();
				AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
				Assert(testForm.CopyLogToClipboardButton.Enabled);
				Assert(testForm.CopyLogToClipboardButton.Visible);
				Assert(testForm.CloseButton.Enabled);
				Assert(testForm.CloseButton.Visible);
				Assert(testForm.OutputListBox.Items.Count > 0);
				AssertEquals("Progress Bar total has not been set", 25, testForm.ProgressBar.Maximum);
				AssertEquals("Progress Bar has not been updated", 25, testForm.ProgressBar.Value);
				string logData = testForm.GetLog();
				Assert("Log Data not as expected", logData.StartsWith("Updating Customs exchange rates effective 23/10/2007 expiring 29/10/2007"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCorruptDataDisplaysErrorMessage()
		{
			using (var tempFile = TempFile.NewFromFile(BaseSourcePath + TestFileWrongDate))
			using (var testForm = new ImportCustomsExchangeRatesFormForTest())
			{
				testForm.Show();
				testForm.FileNameTextBox.Text = tempFile.Filename;
				testForm.StartButton.PerformClick();
				string logData = testForm.GetLog();
				Assert("Log should contain error notification", logData.Contains("The source file is corrupt. Please, verify the effective and expiry dates for the exchange rates."));
			}
		}

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore() => new ImportCustomsExchangeRatesFormForTest();

		protected override string CountryCode => Core.Constants.CountryCodes.Eritrea;

		const string TestFile = @"Enterprise\Product\Operations\Customs\SG\Core\Business\Business\RefCurrency\ExchangeRate.xls";
		const string TestFileWrongDate = @"Enterprise\Product\Operations\Customs\SG\Core\Business\Business\RefCurrency\ExchangeRateWrongDate.xls";
	}

	sealed class ImportCustomsExchangeRatesFormForTest : ImportCustomsExchangeRatesForm
	{
		internal new ZTextBox FileNameTextBox => base.FileNameTextBox;

		internal new ZButton StartButton => base.StartButton;

		internal new ZButton CopyLogToClipboardButton => base.CopyLogToClipboardButton;

		internal new ZButton CloseButton => base.CloseButton;

		internal new KListBox OutputListBox => base.OutputListBox;

		internal new KProgressBar ProgressBar => base.ProgressBar;

		internal new string GetLog() => base.GetLog();
	}
}
