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
	[TestedType(typeof(ImportEDICodeMappingFromCSVForm))]
	sealed class ImportEDICodeMappingFromCSVFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportEDICodeMappingFromCSVForm(OrgProxy);
		}

		public void TestFormHeading()
		{
			using (ImportEDICodeMappingFromCSVForm testForm = new ImportEDICodeMappingFromCSVForm(OrgProxy))
			{
				testForm.Show();
				AssertEquals("Form text", "Import CSV Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (ImportEDICodeMappingFromCSVForm testForm = new ImportEDICodeMappingFromCSVForm(OrgProxy))
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();

					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					AssertEquals("Please Note: Only valid data mapping will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("Progress Bar total has been set", 3, testForm.ProgressBar.Maximum);

					string logData = testForm.GetLog();
					Assert("Log Data not as expected", logData.StartsWith("EDI Code Mappings to Import = 2"));

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
				sw.WriteLine("ForeignCode,Relationship,CargoWiseOneCode");
				sw.WriteLine("111,\"ORG\",\"" + TestOrg1.OH_Code + "\"");
				sw.WriteLine("222,\"ORG\",\"" + TestOrg1.OH_Code + "\"");
				sw.Flush();
			}
		}

		OrgHeader[] TestOrgs
		{
			get { return testOrgs ?? (testOrgs = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "JAC"))); }
		}
		OrgHeader[] testOrgs;

		OrgHeader OrgProxy
		{
			get { return orgProxy ?? (orgProxy = TestOrgs[0]); }
		}
		OrgHeader orgProxy;

		OrgHeader TestOrg1
		{
			get { return testOrg1 ?? (testOrg1 = TestOrgs[1]); }
		}
		OrgHeader testOrg1;
	}
}
