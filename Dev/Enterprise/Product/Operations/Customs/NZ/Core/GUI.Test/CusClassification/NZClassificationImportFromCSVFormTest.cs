using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI
{
	[TestedType(typeof(NZClassificationImportFromCSVForm))]
	public class NZClassificationImportFromCSVFormTest : Enterprise.MasterFiles.GUI.Testing.DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new NZClassificationImportFromCSVForm();
		}

		protected override string CountryCode
		{
			get
			{
				return "ER";
			}
		}

		public void TestFormHeading()
		{
			using (NZClassificationImportFromCSVForm testForm = new NZClassificationImportFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form Heading", "Import Classification Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			using (TempFile tempFile = TempFile.NewWithExtension(".csv"))
			{
				PopulateTestFile(tempFile);
				using (NZClassificationImportFromCSVForm_ForTest testForm = new NZClassificationImportFromCSVForm_ForTest())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();
					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only classifications with valid tariff details will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButton.Enabled);
					Assert(testForm.CopyLogToClipboardButton.Visible);
					Assert(testForm.CloseButton.Enabled);
					Assert(testForm.CloseButton.Visible);
					Assert(testForm.OutputListBox.Items.Count > 0);
					AssertEquals("Progress Bar total has not been set", 5, testForm.ProgressBar.Maximum);
					AssertEquals("Progress Bar has not been updated", 5, testForm.ProgressBar.Value);
					string logData = testForm.GetLog();
					Assert("Log Data not as expected", logData.StartsWith("Classifications to Import = 4"));
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

		void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Code,Description,Tariff,Concession,PartsOfTariff,PermitCode1,PermitNo1,PermitCode2,PermitNo2,PermitCode3,PermitNo3,ProhibitedCode1,ProhibitedCode2");
				sw.WriteLine("Lookup with concession,LAMINATING POUCH FILM,3926.10.90.19B,984581L,,,,,,,,,");
				sw.WriteLine("PVC SHEETING,RIGID PVC SHEETING NOT EXC 1MM,3920.43.00.11C,740403E,,,,,,,,,");
				sw.WriteLine("FOUR,OVERHEAD TRANSPARENCY FILM (OHP FILM),3926.10.90.19B,504017F,,,,,,,,,");
				sw.WriteLine("FIVE,SYNTHETIC STITCHBONDED DYED FABRIC SAMPLES CARDS (FABRICS) FOR CLIENTS,6005.32.11.00G,,,,,,,,,,");
				sw.Flush();
			}
		}

		class NZClassificationImportFromCSVForm_ForTest : NZClassificationImportFromCSVForm
		{
			public new ZTextBox FileNameTextBox => base.FileNameTextBox;

			public new ZButton StartButton => base.StartButton;

			public new ZButton CopyLogToClipboardButton => base.CopyLogToClipboardButton;

			public new ZButton CloseButton => base.CloseButton;

			public new KListBox OutputListBox => base.OutputListBox;

			public new KProgressBar ProgressBar => base.ProgressBar;

			public new string GetLog()
			{
				return base.GetLog();
			}

			public new string CreateLogInDataDirectory(string logData)
			{
				return base.CreateLogInDataDirectory(logData);
			}
		}
	}
}
