using System.IO;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ImportVesselsFromCSVForm))]
	public class ImportVesselFromCSVFormTest : DataLoaderFormTestCase
	{
		public void TestFormHeading()
		{
			using (ImportVesselsFromCSVForm testForm = (ImportVesselsFromCSVForm)GetNewImportFromCSVFormCore())
			{
				testForm.Show();
				Assert("Form Heading", testForm.Text.Contains("Import Vessel Data"));
			}
		}

		public void TestConfirmOKLoadData()
		{
			SetCountryForThisTest();
			using (TempFile tempFile = TempFile.NewWithExtension(".csv"))
			{
				PopulateTestFile(tempFile);

				using (ImportVesselsFromCSVForm testForm = (ImportVesselsFromCSVForm)GetNewImportFromCSVFormCore())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();

					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals(GetConfirmOkMessageText(), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButton.Enabled);
					Assert(testForm.CopyLogToClipboardButton.Visible);
					Assert(testForm.CloseButton.Enabled);
					Assert(testForm.CloseButton.Visible);
					Assert(testForm.OutputListBox.Items.Count > 0);

					AssertEquals("Progress Bar total has not been set", 4, testForm.ProgressBar.Maximum);
					AssertEquals("Progress Bar has not been updated", 4, testForm.ProgressBar.Value);

					string logData = testForm.GetLog();
					Assert("Log Data not as expected", logData.StartsWith("Vessels to Import = 3"));

					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.CreateLogInDataDirectory(logData);
						Assert(logDataOutputFileName.Length > 0);
						Assert(logDataOutputFileName != "Not Created");
					}
					finally
					{
						File.Delete(logDataOutputFileName);
					}
				}
			}
		}

		#region Implementation

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportVesselsFromCSVForm();
		}

		protected override string CountryCode
		{
			get { return "ER"; }
		}

		protected virtual void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("NAME,COUNTRYOFREGO,LLOYDSID");
				sw.WriteLine("MSC SARISKA,AU,7107780");
				sw.WriteLine("MSC TERESA,,7320253");
				sw.WriteLine("MSC FEDERICA,,7347512");
				sw.Flush();
			}
		}

		protected virtual void SetCountryForThisTest()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
		}

		protected virtual string GetConfirmOkMessageText()
		{
			return "Please Note: Only vessels with names and Lloyds numbers will be loaded.";
		}

		#endregion
	}
}
