using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.ImportFromCSVForm;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ImportFromCSVForm))]
	sealed class ImportFromCSVFormTest : ImportFromCSVFormBaseTest
	{
		public void TestFileLocationIsEntered()
		{
			using (var testForm = GetNewImportFromCSVForm())
			{
				testForm.Show();
				testForm.StartButton.PerformClick();

				AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please enter the location of the file you wish to import data from.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFileDoesNotExist()
		{
			using (var testForm = GetNewImportFromCSVForm())
			{
				testForm.Show();
				testForm.FileNameTextBox.Text = @"d:\filedoesnotexist.txt";
				testForm.StartButton.PerformClick();

				AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(@"File D:\FILEDOESNOTEXIST.TXT doesn't exist!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormIsClosed()
		{
			using (var testForm = GetNewImportFromCSVForm())
			{
				testForm.Show();
				testForm.CloseButton.Enabled = true;
				testForm.CloseButton.Visible = true;
				testForm.CloseButton.PerformClick();

				Assert(!testForm.Visible);
			}
		}

		public void TestFormHeading()
		{
			using (var testForm = GetNewImportFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import CSV Data", testForm.Text);
			}
		}

		public void TestConfirmCancelLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (var testForm = GetNewImportFromCSVForm())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					testForm.StartButton.PerformClick();

					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only test records will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!testForm.CopyLogToClipboardButton.Enabled);
					Assert(!testForm.CopyLogToClipboardButton.Visible);
					Assert(!testForm.CloseButton.Enabled);
					Assert(!testForm.CloseButton.Visible);
					AssertEquals(0, testForm.OutputListBox.Items.Count);
				}
			}
		}

		public void TestDownloadTemplate()
		{
			using (var form = GetNewImportFromCSVForm())
			{
				form.Show();

				var tempFileName = Env.GetTempFileName();

				try
				{
					form.DownloadTemplateDialogShowing += (sender, e) =>
					{
						AssertEquals("Comma delimited (*.csv)|*.csv", e.DownloadTemplateDialog.Filter);

						e.DownloadTemplateDialog.FileName = tempFileName;
						e.DialogResultOverride = DialogResult.OK;
					};

					form.DownloadTemplateButton.PerformClick();

					Assert("Should have created file", File.Exists(tempFileName));
					AssertEquals(string.Format("Template has been saved to {0}", tempFileName), UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					using (File.OpenWrite(tempFileName))
					{
						form.DownloadTemplateButton.PerformClick();

						AssertContains("Cannot write the file to the disk.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		#region Implementation

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new TestCsvLoadForm();
		}

		protected override string CountryCode
		{
			get { return "ER"; }
		}

		void PopulateTestFile(TempFile tempFile)
		{
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Code,Description,UQ,TariffLookup,AltTariffLookup,Owner,Supplier,Division,QtyinStock");
				sw.WriteLine("1,RUBBER GASKET,KG,XXX,Class-Lookup,," + testOrganisation.OH_Code + ",,0");
				sw.WriteLine("P1234-4848X,Test Part,,Exp Lookup,," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");
				sw.WriteLine("P1234-4848I,Test Part,,,Imp Lookup," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");

				sw.Flush();
			}
		}

		#endregion
	}
}
