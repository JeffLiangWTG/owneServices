using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class TemplateFileUserControlTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestImport()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (ZForm form = new ZForm(GlbCompany.CurrentCompany))
			{
				GlbCompany.CurrentCompany.TemplateFiles.RemoveAndDeleteAll();
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var testDocumentPath = resourceRetriever.SaveResourceToFile("Test.xslt");
					var selectedFile = testDocumentPath;
					using (TemplateFileUserControl userControl = new TemplateFileUserControlForTest(selectedFile))
					{
						form.Controls.Add(userControl);
						form.Show();

						var btnNews = userControl.Controls.Find("btnNew", true);
						AssertEquals(nameof(btnNews.Length), 1, btnNews.Length);
						var btnNew = btnNews[0] as ZButton;
						btnNew.PerformClick();
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles.Count, 1);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestImport_ChangeTemplateFile()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (ZForm form = new ZForm(GlbCompany.CurrentCompany))
			{
				GlbCompany.CurrentCompany.TemplateFiles.RemoveAndDeleteAll();
				var templateFile = Factory.New<AccTemplateFileStorage>();
				templateFile.TFS_Code = "XXX";
				GlbCompany.CurrentCompany.TemplateFiles.Add(templateFile);

				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var testDocumentPath = resourceRetriever.SaveResourceToFile("Test.xslt");
					var selectedFile = testDocumentPath;

					using (TemplateFileUserControl userControl = new TemplateFileUserControlForTest(selectedFile))
					{
						form.Controls.Add(userControl);
						form.Show();

						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles.Count, 1);
						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles[0].TFS_FileName, ZString.Empty);

						var grid = userControl.Controls.Find("TemplateFileGrid", true)[0] as ZGrid;
						grid.CurrentRowIndex = 0;
						var btnEditFile = userControl.Controls.Find("btnEditFile", true)[0] as ZButton;
						AssertNotNull(btnEditFile);
						btnEditFile.PerformClick();

						AssertEquals("File '" + selectedFile + "' should exist", true, File.Exists(selectedFile));
						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles.Count, 1);
						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles[0].TFS_FileName, ValidFileName);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestImport_DeleteTemplateFile()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (ZForm form = new ZForm(GlbCompany.CurrentCompany))
			{
				GlbCompany.CurrentCompany.TemplateFiles.RemoveAndDeleteAll();
				var templateFile = Factory.New<AccTemplateFileStorage>();
				templateFile.TFS_Code = "XXX";
				GlbCompany.CurrentCompany.TemplateFiles.Add(templateFile);

				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var testDocumentPath = resourceRetriever.SaveResourceToFile("Test.xslt");
					var selectedFile = testDocumentPath;
					using (TemplateFileUserControl userControl = new TemplateFileUserControlForTest(selectedFile))
					{
						form.Controls.Add(userControl);
						form.Show();

						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles.Count, 1);
						var grid = userControl.Controls.Find("TemplateFileGrid", true)[0] as ZGrid;
						grid.CurrentRowIndex = 0;
						var btnDelete = userControl.Controls.Find("btnDelete", true)[0] as ZButton;
						AssertNotNull(btnDelete);
						btnDelete.PerformClick();
						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles.Count, 0);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestImport_UnableToDeleteTemplateFile()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			using (ZForm form = new ZForm(GlbCompany.CurrentCompany))
			{
				GlbCompany.CurrentCompany.TemplateFiles.RemoveAndDeleteAll();
				var templateFile = Factory.New<AccTemplateFileStorage>();
				templateFile.TFS_Code = "XXX";
				GlbCompany.CurrentCompany.TemplateFiles.Add(templateFile);

				var config = Factory.NewWithValidTestData<AccEInvoicingTemplateFileView>();
				config.ETF_TemplateCode = templateFile.TFS_Code;

				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var testDocumentPath = resourceRetriever.SaveResourceToFile("Test.xslt");
					var selectedFile = testDocumentPath;
					using (TemplateFileUserControl userControl = new TemplateFileUserControlForTest(selectedFile))
					{
						form.Controls.Add(userControl);
						form.Show();

						AssertEquals(GlbCompany.CurrentCompany.TemplateFiles.Count, 1);
						var grid = userControl.Controls.Find("TemplateFileGrid", true)[0] as ZGrid;
						grid.CurrentRowIndex = 0;
						var btnDelete = userControl.Controls.Find("btnDelete", true)[0] as ZButton;
						AssertNotNull(btnDelete);
						btnDelete.PerformClick();
						AssertEquals(templateFile.ReasonForNotAbleToDelete,
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		const string ValidFileName = "Test.xslt";

		public class TemplateFileUserControlForTest : TemplateFileUserControl
		{
			public TemplateFileUserControlForTest(string selectedFile)
			{
				SelectedFile = selectedFile;
			}

			protected override void Import(bool changeCurrent = false)
			{
				if (File.Exists(SelectedFile))
				{
					ConvertFileDataToBytesAndAddToCollection(changeCurrent, SelectedFile);
				}
				else
				{
					Globals.Message.ShowError($"File '{SelectedFile}' Does Not Exist.");
				}
			}

			readonly string SelectedFile;
		}
	}
}
