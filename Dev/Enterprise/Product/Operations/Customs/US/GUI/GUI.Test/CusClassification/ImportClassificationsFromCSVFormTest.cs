using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(ImportClassificationsFromCSVForm))]
	sealed class ImportClassificationsFromCSVFormTest : MasterFiles.GUI.Testing.DataLoaderFormTestCase
	{
		public void TestFormHeading()
		{
			using (ImportClassificationsFromCSVForm testForm = new ImportClassificationsFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form Heading", "Import Lookup Code Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			using (TempFile tempFile = TempFile.NewWithExtension(".csv"))
			{
				PopulateTestFile(tempFile);
				using (var testForm = new ImportClassificationsFromCSVFormForTest())
				{
					testForm.Show();
					testForm.FileNameTextBoxInternal.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButtonInternal.PerformClick();
					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only classifications with valid tariff details will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButtonInternal.Enabled);
					Assert(testForm.CopyLogToClipboardButtonInternal.Visible);
					Assert(testForm.CloseButtonInternal.Enabled);
					Assert(testForm.CloseButtonInternal.Visible);
					Assert(testForm.OutputListBoxInternal.Items.Count > 0);
					string logData = testForm.GetLogInternal();
					Assert("Log Data not as expected", logData.StartsWith("Classifications to Import = 3"));
					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.CreateLogInDataDirectoryInternal(logData);
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

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore() => new ImportClassificationsFromCSVForm();

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF");
				sw.WriteLine("TEST,IMP,Test Lookup,6205900054,,,");
				sw.WriteLine("Lookup with concession,IMP,Lookup Description - code with concession,6205900054");
				sw.WriteLine("Joggers,EXP,,64051000");
				sw.Flush();
			}
		}

		sealed class ImportClassificationsFromCSVFormForTest : ImportClassificationsFromCSVForm
		{
			public ImportClassificationsFromCSVFormForTest() : base()
			{
			}

			internal ZButton CloseButtonInternal => CloseButton;
			internal ZButton CopyLogToClipboardButtonInternal => CopyLogToClipboardButton;
			internal KListBox OutputListBoxInternal => OutputListBox;
			internal ZTextBox FileNameTextBoxInternal => FileNameTextBox;
			internal ZButton StartButtonInternal => StartButton;
			internal string GetLogInternal() => GetLog();
			internal string CreateLogInDataDirectoryInternal(string logData) => CreateLogInDataDirectory(logData);
		}
	}
}
