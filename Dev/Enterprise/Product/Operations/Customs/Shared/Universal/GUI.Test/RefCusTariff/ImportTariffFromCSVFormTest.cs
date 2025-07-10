using System.IO;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(ImportTariffFromCSVForm))]
	class ImportTariffFromCSVFormTest : MasterFiles.GUI.Testing.DataLoaderFormTestCase
	{
		public void TestFormHeading()
		{
			using (var testForm = new ImportTariffFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import Tariff Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			using (var tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);
				using (var testForm = new ImportTariffFromCSVFormForTest())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();
					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only tariffs with Country Code and Version and Tariff Type and Tariff Code will be loaded. If it is new Tariff to import, Description and UOM1 is necessary too.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButton.Enabled);
					Assert(testForm.CopyLogToClipboardButton.Visible);
					Assert(testForm.CloseButton.Enabled);
					Assert(testForm.CloseButton.Visible);
					Assert(testForm.OutputListBox.Items.Count > 0);
					var logData = testForm.GetLog();
					Assert("Log Data as expected", logData.StartsWith("Tariffs to Import = 3"));
					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.CreateLogInDataDirectory(logData);
						Assert(logDataOutputFileName.Length > 0);
						Assert(logDataOutputFileName != "Not Created");
					}
					finally
					{
						if (!string.IsNullOrEmpty(logDataOutputFileName))
						{
							File.Delete(logDataOutputFileName);
						}
					}
				}
			}
		}

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore() => new ImportTariffFromCSVForm();

		protected override string CountryCode => Core.Constants.CountryCodes.Eritrea;

		static void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Version,TariffType,TariffCode,Description,StartDate,EndDate,TaxOrFeeCode,CountryCode,UOM1,UOM2,UOM3");
				sw.WriteLine("111,HSN,VAT,Tariff Description1,2021-02-03,2022-02-03,TT1,AU,KG,PK,XX");
				sw.WriteLine("222,OTH,VAT,Tariff Description2,2021-02-03,2022-02-03,TT2,AU,KG,,XX");
				sw.WriteLine("333,,VAT,Tariff Description3,2021-02-03,2022-02-03,TT3,AU,KG,,");
				sw.Flush();
			}
		}

		class ImportTariffFromCSVFormForTest : ImportTariffFromCSVForm
		{
			public new ZArchitecture.ZTextBox FileNameTextBox => base.FileNameTextBox;
			public new ZArchitecture.GUI.ZButton StartButton => base.StartButton;
			public new ZArchitecture.GUI.ZButton CopyLogToClipboardButton => base.CopyLogToClipboardButton;
			public new ZArchitecture.GUI.ZButton CloseButton => base.CloseButton;
			public new CargoWise.Windows.UI.KListBox OutputListBox => base.OutputListBox;
			public new string GetLog() => base.GetLog();
			public new string CreateLogInDataDirectory(string logData) => base.CreateLogInDataDirectory(logData);
		}
	}
}
