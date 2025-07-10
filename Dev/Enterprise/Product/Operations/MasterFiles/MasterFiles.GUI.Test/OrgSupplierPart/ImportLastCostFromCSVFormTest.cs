using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ImportLastCostFromCSVForm))]
	sealed class ImportLastCostFromCSVFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportLastCostFromCSVForm();
		}

		protected override string CountryCode
		{
			get { return "ER"; }
		}

		public void TestFormHeading()
		{
			using (ImportLastCostFromCSVForm testForm = new ImportLastCostFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import Products Last Cost Data", testForm.Text);
			}
		}

		[RequiresSTA]
		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (ImportLastCostFromCSVForm testForm = new ImportLastCostFromCSVForm())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();

					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButton.Enabled);
					Assert(testForm.CopyLogToClipboardButton.Visible);
					Assert(testForm.CloseButton.Enabled);
					Assert(testForm.CloseButton.Visible);
					Assert(testForm.OutputListBox.Items.Count > 0);

					AssertEquals("Progress Bar total has not been set", 4, testForm.ProgressBar.Maximum);
					AssertEquals("Progress Bar has not been updated", 4, testForm.ProgressBar.Value);

					string logData = testForm.GetLog();
					Assert("Log Data not as expected", logData.StartsWith("Part Last Costs to Import = 3"));

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

		void PopulateTestFile(TempFile tempFile)
		{
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Code,Owner,Supplier,Last_Cost");
				sw.WriteLine("1," + testOrganisation.OH_Code + ",,100");
				sw.WriteLine("P1234-4848X," + testOrganisation.OH_Code + "," + testSupplier.OH_Code + "," + "1758.50");
				sw.WriteLine("P1234-4848I," + testOrganisation.OH_Code + "," + testSupplier.OH_Code + "," + "1758.50");

				sw.Flush();
			}
		}

		#endregion
	}
}
