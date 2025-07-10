using System.IO;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DataLoaderForm))]
	sealed class DataLoaderFormTest : ZFormBasherTest
	{
		public void TestCanDownloadTemplate()
		{
			using (var form = new DataLoaderFormWithNoTemplateHeadingForTesting())
			{
				form.Show();
				AssertEquals(false, form.CanDownloadTemplate_Exposed);
			}

			using (var form = new DataLoaderFormWithMultilineTemplateHeadingForTesting())
			{
				form.Show();
				AssertEquals(true, form.CanDownloadTemplate_Exposed);
			}
		}

		[DeveloperOnlyTest]
		public void TestLoadSpecificDataType_InvalidHeader()
		{
			using (var testFile = TempFile.New())
			{
				using (var sw = new StreamWriter(testFile.Filename))
				{
					sw.WriteLine("Invalid Header");
					sw.Flush();
				}

				using (var form = new DataLoaderFormWithMultilineTemplateHeadingForTesting())
				{
					form.LoadSpecificDataType(testFile.Filename);

					AssertEquals("File Header information is incorrect. The import of TestDataType data requires a specific .CSV format file.", form.LastDataLoad.Log[1]);
					AssertEquals("Please check the format in the template file copied to the clipboard, and also available for Download above this message.", form.LastDataLoad.Log[2]);
					AssertMultilineASCIIEquals("Should have copied csv template text to Clipboard as tab separated", "TEST1\tTEST2\tTEST3\r\nTESTA\tTESTB", SafeClipboard.GetText());
					AssertGreaterThan("Logs are expected.", form.OutputListBox.Items.Count, 0);
					var nonEmptyLogs = form.OutputListBox.Items.OfType<string>().Where(log => !string.IsNullOrEmpty(log));
					var distinctNonEmptyCount = nonEmptyLogs.Distinct().Count();
					AssertEquals("Should only print logs once.", distinctNonEmptyCount, nonEmptyLogs.Count());
				}
			}
		}

		public void TestLoadSpecificDataType_InvalidHeader_AndClipboardUnaccessible()
		{
			using (var testFile = TempFile.New())
			{
				using (var sw = new StreamWriter(testFile.Filename))
				{
					sw.WriteLine("Invalid Header");
					sw.Flush();
				}

				using (var form = new DataLoaderFormWithMultilineTemplateHeadingForTesting())
				{
					form.SetClipboardTextResultOverrideForTest = false;
					form.LoadSpecificDataType(testFile.Filename);

					AssertEquals("File Header information is incorrect. The import of TestDataType data requires a specific .CSV format file.", form.LastDataLoad.Log[1]);
					AssertEquals("Template file could not be copied to the clipboard as it is not accessible at the moment. It is available for Download above this message.", form.LastDataLoad.Log[2]);
					AssertGreaterThan("Logs are expected.", form.OutputListBox.Items.Count, 0);
					var nonEmptyLogs = form.OutputListBox.Items.OfType<string>().Where(log => !string.IsNullOrEmpty(log));
					var distinctNonEmptyCount = nonEmptyLogs.Distinct().Count();
					AssertEquals("Should only print logs once.", distinctNonEmptyCount, nonEmptyLogs.Count());
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DataLoaderFormWithMultilineTemplateHeadingForTesting();
		}

		#region DataLoaderForm Classes

		class DataLoaderFormWithNoTemplateHeadingForTesting : DataLoaderFormForTesting
		{
			protected override DataLoad GetNewDataLoader()
			{
				return new DataLoadWithoutTemplateHeading();
			}
		}

		class DataLoaderFormWithMultilineTemplateHeadingForTesting : DataLoaderFormForTesting
		{
			protected override DataLoad GetNewDataLoader()
			{
				LastDataLoad = new DataLoadWithMultilineTemplateHeading();
				return LastDataLoad;
			}

			public DataLoad LastDataLoad;
		}

		abstract class DataLoaderFormForTesting : DataLoaderForm
		{
			protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
			{
				dataLoader.ImportData(dataToLoad, "TestDataType");
			}

			protected override bool SetClipboardText(string text)
			{
				return SetClipboardTextResultOverrideForTest ?? base.SetClipboardText(text);
			}
			public bool? SetClipboardTextResultOverrideForTest;

			public bool CanDownloadTemplate_Exposed
			{
				get { return CanDownloadTemplate; }
			}
		}

		#endregion

		#region DataLoad Classes

		sealed class DataLoadWithoutTemplateHeading : Business.Testing.DataLoadForTest
		{
			public override string CSVTemplateHeading
			{
				get { return ""; }
			}
		}

		sealed class DataLoadWithMultilineTemplateHeading : Business.Testing.DataLoadForTest
		{
			public override string CSVTemplateHeading
			{
				get
				{
					return @"TEST1,TEST2,TEST3
TESTA,TESTB";
				}
			}
		}

		#endregion

		#endregion
	}
}
