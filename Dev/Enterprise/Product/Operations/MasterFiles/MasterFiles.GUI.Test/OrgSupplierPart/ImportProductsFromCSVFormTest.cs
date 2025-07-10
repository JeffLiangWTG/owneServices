using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ImportProductsFromCSVForm))]
	sealed class ImportProductsFromCSVFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportProductsFromCSVForm();
		}

		protected override string CountryCode
		{
			get { return "ER"; }
		}

		public void TestFormHeading()
		{
			using (ImportProductsFromCSVForm testForm = new ImportProductsFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form text", "Import Product Data", testForm.Text);
			}
		}

		[RequiresSTA]
		public void TestAuditGroupBoxSecurityPermissions()
		{
			Env.Security.CustomsSupplierPartAuditImport.IsAllowed = false;
			Env.Security.CustomsSupplierPartAuditExport.IsAllowed = false;

			using (var form = new ImportProductsFromCSVForm())
			{
				Assert(!form.SetAuditStatusGroupBox.Enabled);
				Assert(!form.SetAuditStatusYesRadioButton.Enabled);
				Assert(!form.SetAuditStatusNoRadioButton.Enabled);
			}

			Env.Security.CustomsSupplierPartAuditImport.IsAllowed = true;
			Env.Security.CustomsSupplierPartAuditExport.IsAllowed = false;

			using (var form = new ImportProductsFromCSVForm())
			{
				Assert(form.SetAuditStatusGroupBox.Enabled);
				Assert(form.SetAuditStatusYesRadioButton.Enabled);
				Assert(form.SetAuditStatusNoRadioButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestAuditGroupBoxFunctionality()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Canada);

			using (var form = new ImportProductsFromCSVForm())
			{
				form.Show();
				Assert(!form.SetAuditStatusYesRadioButton.Checked);
				Assert(form.SetAuditStatusNoRadioButton.Checked);

				using (var csvFile = CreateSampleCsvFileForPartNumber("COTTON"))
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					form.FileNameTextBox.Text = csvFile.Filename;
					form.SetAuditStatusYesRadioButton.PerformClick();
					Assert(form.SetAuditStatusYesRadioButton.Checked);
					Assert(!form.SetAuditStatusNoRadioButton.Checked);

					var auditMessageForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertNotNull("Audit message box is shown", auditMessageForm);
					AssertType<AuditMessageForm>(auditMessageForm);
					((AuditMessageForm)auditMessageForm).ReferenceText = "My audit test message";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					form.StartButton.PerformClick();

					var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "COTTON"));
					var pivot = Factory.LoadTop1<IBaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
					Assert("Audited date is not empty", !pivot.CI_LastAuditedDate.IsEmpty);
					Assert("Audited user is not empty", !pivot.CI_LastAuditedUser.IsEmpty);
					var log = GetEvent((BusinessObject)pivot, Events.RecordAuditedCode);
					AssertNotNull("Has an audit log record.", log);
					AssertEquals("Has the audit message.", "My audit test message", log.SL_Reference);
				}

				using (var csvFile = CreateSampleCsvFileForPartNumber("SILK"))
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					form.FileNameTextBox.Text = csvFile.Filename;
					form.SetAuditStatusNoRadioButton.PerformClick();
					Assert(!form.SetAuditStatusYesRadioButton.Checked);
					Assert(form.SetAuditStatusNoRadioButton.Checked);

					var auditMessageForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertNull("Audit message box is not shown", auditMessageForm);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					form.StartButton.PerformClick();

					var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "SILK"));
					var pivot = Factory.LoadTop1<IBaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, part.PK));
					Assert("Audited date is empty", pivot.CI_LastAuditedDate.IsEmpty);
					Assert("Audited user is empty", pivot.CI_LastAuditedUser.IsEmpty);
					var log = GetEvent((BusinessObject)pivot, Events.RecordAuditedCode);
					AssertNull("No audit log record.", log);
				}
			}
		}

		[RequiresSTA]
		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile);

				using (ImportProductsFromCSVForm testForm = new ImportProductsFromCSVForm())
				{
					testForm.Show();
					testForm.FileNameTextBox.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButton.PerformClick();

					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only Products with valid Organization and Classification Lookup links will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButton.Enabled);
					Assert(testForm.CopyLogToClipboardButton.Visible);
					Assert(testForm.CloseButton.Enabled);
					Assert(testForm.CloseButton.Visible);
					Assert(testForm.OutputListBox.Items.Count > 0);

					AssertEquals("Progress Bar total has not been set", 4, testForm.ProgressBar.Maximum);
					AssertEquals("Progress Bar has not been updated", 4, testForm.ProgressBar.Value);

					string logData = testForm.GetLog();
					Assert("Log Data not as expected", logData.StartsWith("Products to Import = 3"));

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
			OrgHeader testOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier,Division,QtyinStock");
				sw.WriteLine("1,RUBBER GASKET,KG,XXX,Class-Lookup,," + testOrganisation.OH_Code + ",,0");
				sw.WriteLine("P1234-4848X,Test Part,,Exp Lookup,," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");
				sw.WriteLine("P1234-4848I,Test Part,,,Imp Lookup," + testOrganisation.OH_Code + "," + testSupplier1.OH_Code + ",Major Accounts,175850");

				sw.Flush();
			}
		}

		StmALog GetEvent(BusinessObject parent, string eventCode)
		{
			var query = new ZQuery(StmALogSchema.SL_Table, parent.TableName)
				.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, parent.PK))
				.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode));
			return Factory.LoadTop1<StmALog>(query);
		}

		IBaseCusClassification Classification => Factory.GetCachedValue("ImportProductsFromCSVFormTest|Classification", () =>
			{
				var result = Factory.New<IBaseCusClassification>();

				result[CusClassificationSchema.CC_ClassificationType.Name] = "IMP";
				result[CusClassificationSchema.CC_LookupCode.Name] = "LOOKUP";
				result[CusClassificationSchema.CC_TariffNum.Name] = "x";
				result[CusClassificationSchema.CC_Description.Name] = "DESCRIPTION";
				Factory.Save();

				return result;
			});

		OrgHeader Owner => Factory.GetCachedValue("ImportProductsFromCSVFormTest|Owner", () =>
			{
				return Factory.LoadTop1<OrgHeader>(new ZQuery());
			});

		TempFile CreateSampleCsvFileForPartNumber(string partNumber)
		{
			var result = TempFile.NewWithExtension("csv");

			using (var sw = new StreamWriter(result.Filename))
			{
				sw.WriteLine("Code,Description,UQ,ExportClassification,ImportClassification,Owner,Supplier");
				sw.WriteLine($"{partNumber},{partNumber}'s DESCRIPTION,,,{Classification.CC_LookupCode},{Owner.OH_Code},");
			}

			return result;
		}
	}
}
