using System.IO;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Organisation.OrgColdCallRegister.Test
{
	[TestedType(typeof(ImportInquiryFromCSVForm))]
	sealed class ImportInquiryFromCSVFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportInquiryFromCSVForm();
		}

		public void TestFormHeading()
		{
			using (var testForm = GetNewImportFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import Inquiry Data", testForm.Text);
			}
		}

		[RequiresSTA]
		public void TestLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			using (var tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (var testForm = new ImportInquiryFromCSVFormForTesting())
				{
					testForm.Show();
					testForm.GetFileNameTextBoxForTesting().Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					testForm.GetStartButtonForTesting().PerformClick();

					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.GetCopyLogToClipboardButtonForTesting().Enabled);
					Assert(testForm.GetCopyLogToClipboardButtonForTesting().Visible);
					Assert(testForm.GetCloseButtonForTesting().Enabled);
					Assert(testForm.GetCloseButtonForTesting().Visible);
					Assert(testForm.GetOutputListBoxForTesting().Items.Count > 0);

					string logData = testForm.GetLogForTesting();
					Assert("Log Data not as expected", logData.StartsWith("Inquiries to Import = 3"));
					string logDataOutputFileName = testForm.CreateLogInDataDirectoryForTesting(logData);

					try
					{
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
			using (var sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("COMPANY,ADDRESS1,ADDRESS2,CITY,STATE,POSTCODE,COUNTRY,BUS_REG_NO,CONTACT_NAME,PHONE,EMAIL,MOBILE,FAX,SOURCE,DETAILS,SALES_REP,NOTES,TYPE,REF_ORG_CODE,REF_CONTACT_NAME,LEAD_INTEREST");
				sw.WriteLine("TESTCOMPANYNAME1,3A Pound St,,WEST IPSWICH,QLD,4305,AUSYD,,Myrene,0738128363,,,,,,,-Transport Storage,INQ,ORGSYDCODE,Enerym,WRM");
				sw.WriteLine("TESTCOMPANYNAME2,Locked Bag 67,Unlock Key 76,WETHERILL PARK DC,NSW,1851,AU,23112936991,Stephen,02 9513 0300,sbrown@1stfleet.com.au,0412345678,02 9756 5370,TMK,Purchased List,F.L,Business Services|Road Freight|Storage|Transport,WEB,,CLD");
				sw.WriteLine("TESTCOMPANYNAME3,,,,,,,,,,,,,,,,,,,,");
				sw.Flush();
			}
		}

		#region Implementation

		class ImportInquiryFromCSVFormForTesting : ImportInquiryFromCSVForm
		{
			public ZTextBox GetFileNameTextBoxForTesting()
			{
				return FileNameTextBox;
			}

			public ZButton GetStartButtonForTesting()
			{
				return StartButton;
			}

			public ZButton GetCopyLogToClipboardButtonForTesting()
			{
				return CopyLogToClipboardButton;
			}

			public ZButton GetCloseButtonForTesting()
			{
				return CloseButton;
			}

			public ListBox GetOutputListBoxForTesting()
			{
				return OutputListBox;
			}

			public string GetLogForTesting()
			{
				return GetLog();
			}

			public string CreateLogInDataDirectoryForTesting(string logData)
			{
				return CreateLogInDataDirectory(logData);
			}
		}

		#endregion
	}
}
