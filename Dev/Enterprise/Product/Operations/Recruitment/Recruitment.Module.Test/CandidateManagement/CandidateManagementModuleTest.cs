using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Recruitment.Testing.RecruitmentDataHelpers;

namespace Enterprise.Recruitment.Module.Testing
{
	[TestedType(typeof(CandidateManagementModule))]
	sealed class CandidateManagementModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.RecruitmentCandidateManagement;

		protected override void SetUp()
		{
			CreateCandidate(Factory, "Borris Johnson");
			Factory.Save();

			base.SetUp();
		}

		public void TestRecordsLabelVisible()
		{
			var minimumSupportedScreenSize = new Size(1366, 768);

			using (var form = GetFormToBash())
			{
				form.Size = minimumSupportedScreenSize;
				form.Show();
				Application.DoEvents();

				PerformSearch(form);

				var stripControl = GetStripControl(form);
				var recordsFoundLabel = (ZLabel)stripControl.Controls.Find("ToolStripRecordsFoundLabel", false).Single();

				Assert("PRE: Label should be visible", recordsFoundLabel.Visible);
				AssertInBounds(recordsFoundLabel, stripControl);

				var splitter = (SplitContainer)form.Controls.Find("pageSplitControl", true).Single();
				splitter.SplitterDistance = splitter.Width - splitter.Panel2MinSize - splitter.SplitterWidth;

				Application.DoEvents();
				AssertInBounds(recordsFoundLabel, stripControl);
			}

			void AssertInBounds(Control item, Control parent) => CombineAssertions("The label should be not be blocked by the parent", () =>
			{
				AssertEquals("PRE: The parent should be the parent", item.Parent, parent);

				Assert("Left edge", item.Left >= 0);
				Assert("Right edge", item.Right <= parent.Width);
				Assert("Top edge", item.Top >= 0);
				Assert("Bottom edge", item.Bottom <= parent.Height);
			});
		}

		public void TestPanelVisible()
		{
			using (var form = GetFormToBash())
			{
				var splitter = (SplitContainer)form.Controls.Find("pageSplitControl", true).Single();
				splitter.Size = new Size(1024, 768);
				form.Show();
				Application.DoEvents();

				Assert("Right panel is not in bounds of the parent", splitter.Panel2.Right <= splitter.Right);
			}
		}

		public void TestNewApplicationButton()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				Application.DoEvents();

				var toolStrip = (ZToolStrip)form.Controls.Find("moduleActionsToolbar", true).Single();
				var button = toolStrip.Items["New Candidate"];
				button.PerformClick();

				Application.DoEvents();

				using (var newAppForm = ZApplication.GetOpenForms().OfType<HRJobApplicationForm>().SingleOrDefault())
				{
					AssertNotNull("Expected a new form for HRJobApplication to be opened", newAppForm);
					Assert("Should be a new HRJobApplication, not an existing one", !newAppForm.BusinessEntity.IsInDatabase);
				}
			}
		}

		public void TestEmptyModuleDoesntBlowUp()
		{
			TestCaseHelper.ClearTable(HRJobApplicationDocumentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(HRJobApplicationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(HRJobApplicantSchema.Constants.TableName);

			using (var form = GetFormToBash())
			{
				AssertNoExceptionThrown(() =>
				{
					form.Show();
					Application.DoEvents();

					var details = GetDetailsControl(form);
					var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();

					foreach (var page in tabControl.AllTabPages)
					{
						tabControl.SelectedTab = (ZTabPage)page;
						Application.DoEvents();
					}
				});

				AssertNotNull("It should have been created & displayed", GetDetailsControl(form));
			}
		}

		public void TestSelectingCandidateUpdatesDetails()
		{
			var john = CreateCandidate(Factory, "John Smith", resume: PdfWithDifferentColorsForEachPagePath);
			var jack = CreateCandidate(Factory, "Jack Jones", resume: PdfWithDifferentColorsForEachPagePath);
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();
				AssertGreaterThanOrEqualTo("PRE: We made two candidates, they should be there (at the very least)", grid.List.Count, 2);

				SelectCandidate(grid, john);

				var details = GetDetailsControl(form);

				AssertEquals("When a candidate is selected we should rebind the details tab", details.CurrentDataItem.Applicant.Name, john.Applicant.Name);

				SelectCandidate(grid, jack);
				AssertEquals("When a candidate is selected we should rebind the details tab", details.CurrentDataItem.Applicant.Name, jack.Applicant.Name);
			}
		}

		Point AddMargin(Point p) => p + ControlDpiScalingHelper.NewScaledSize(5, 5);

		public void TestEDocsUpdated()
		{
			var candidateDetailTestApplicantPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.CandidateDetailTestApplicant.pdf", "CandidateDetailTestApplicant.pdf");
			var scott = CreateCandidate(Factory, "Scott Fakename", candidateDetailTestApplicantPath, addApplicationResume: false, addApplicantResume: true);
			var candidateDetailTestApplicationPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.CandidateDetailTestApplication.pdf", "CandidateDetailTestApplication.pdf");
			_ = AddResume(scott.Application, candidateDetailTestApplicationPath);
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var candidateDetailsControl = GetDetailsControl(form);
				var tabControl = (ZTabControl)candidateDetailsControl.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "edocsTabPage");
				Application.DoEvents();

				var eDocsUserControl = (eDocsUserControl)form.Controls.Find("eDocsUserControl", true).Single();
				var relatedParentsGrid = (ZGrid)eDocsUserControl.Controls.Find("RelatedParentsGrid", true).First();
				var storageDocsGrid = (DocumentsZGrid)eDocsUserControl.Controls.Find("StorageDocsGrid", true).First();

				var pos0 = AddMargin(relatedParentsGrid.GetCellBounds(0, 0).Location);
				var pos1 = AddMargin(relatedParentsGrid.GetCellBounds(1, 0).Location);

				relatedParentsGrid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos0.X, pos0.Y, 0));
				AssertEquals("Current eDocs file should be CandidateDetailTestApplication.pdf", "CandidateDetailTestApplication.pdf", storageDocsGrid.CurrentElement.GetFileNameOnlyWithExtension());
				relatedParentsGrid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos1.X, pos1.Y, 0));
				AssertEquals("Current eDocs file should be CandidateDetailTestApplicant.pdf", "CandidateDetailTestApplicant.pdf", storageDocsGrid.CurrentElement.GetFileNameOnlyWithExtension());
				relatedParentsGrid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos0.X, pos0.Y, 0));
				AssertEquals("Current eDocs file should be CandidateDetailTestApplication.pdf", "CandidateDetailTestApplication.pdf", storageDocsGrid.CurrentElement.GetFileNameOnlyWithExtension());
			}
		}

		public void TestDoubleClickCandidate()
		{
			var john = CreateCandidate(Factory, "John Smith", resume: PdfWithDifferentColorsForEachPagePath);
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();
				AssertGreaterThanOrEqualTo("PRE: We made two candidates, they should be there (at the very least)", grid.List.Count, 2);

				var rowNum = SelectCandidate(grid, john);
				grid.PerformMouseDoubleClickForTest(rowNum);
				Application.DoEvents();

				using (var jobApplicantForm = Application.OpenForms.OfType<HRJobApplicantForm>().First())
				{
					AssertNotNull("Tag should be double-clicked and editing form opened successfully", jobApplicantForm);
				}

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		public void TestRemovingApplicationFromChildApplicantForm()
		{
			var john = CreateCandidate(Factory, "John Smith", resume: PdfWithDifferentColorsForEachPagePath);
			john.Application.HP_HV = Factory.New<HRRecruitmentJobCampaign>().PK;
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();
				var source = grid.DataSource as CandidateModuleBusinessObject;
				AssertEquals(2, source.Collection.Count);
				AssertGreaterThanOrEqualTo("PRE: We made two candidates, they should be there (at the very least)", grid.List.Count, 2);

				var rowNum = SelectCandidate(grid, john);
				grid.PerformMouseDoubleClickForTest(rowNum);

				Application.DoEvents();

				using (var jobApplicantForm = Application.OpenForms.OfType<HRJobApplicantForm>().First())
				{
					AssertNotNull("Tag should be double-clicked and editing form opened successfully", jobApplicantForm);

					var tabControl = (ZTemplateTabControl)jobApplicantForm.Controls.Find("MainTabControl", true).Where(i => i is ZTemplateTabControl).Single();
					tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "ApplicationsTabPage");
					Application.DoEvents();

					var applicationGrid = (ZGrid)jobApplicantForm.Controls.Find("ApplicationsGrid", true).Single();
					AssertGreaterThanOrEqualTo("PRE: We made one application, it should be there (at the very least)", applicationGrid.List.Count, 1);
					var applicationRowNum = SelectApplication(applicationGrid, john.Application.HP_HV);
					applicationGrid.Select(applicationRowNum);
					applicationGrid.DeleteMenuItem.PerformClick();
					Application.DoEvents();

					AssertGreaterThanOrEqualTo("The application should be removed", grid.List.Count, 0);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

					jobApplicantForm.FireSaveButton();
					Application.DoEvents();

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("Msg shown for saving changes", "This application has been deleted.\r\nAny unsaved candidate changes have been discarded.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNoExceptionThrown("Should not throw exception", () => jobApplicantForm.Close());
				}

				AssertGreaterThanOrEqualTo("One candidate should be removed", grid.List.Count, 1);
				AssertEquals(1, source.Collection.Count);
				AssertNoExceptionThrown("Should not throw exception", () => form.Close());
			}
		}

		public void TestCheckBoxWithNoEdocs_PreviewDocuments()
		{
			var john = CreateCandidate(Factory, "John Smith");
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();
				AssertGreaterThanOrEqualTo("PRE: We made two candidates, they should be there (at the very least)", grid.List.Count, 2);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "edocsTabPage");
				Application.DoEvents();

				var userControl = (eDocsUserControl)form.Controls.Find("eDocsUserControl", true).Single();
				var documentPreview = userControl.DocumentPreview;

				var previewDocumentsCheckBox = (ZCheckBox)form.Controls.Find("PreviewDocumentsCheckBox", true).Single();
				previewDocumentsCheckBox.Checked = true;
				Application.DoEvents();

				AssertNull("The graphical display is cleared", documentPreview.PageSelector);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		public void TestCheckBoxWithNoEdocs_CompanySpecific()
		{
			AssertCheckBox("CompanySpecificCheckBox");
		}

		public void TestCheckBoxWithNoEdocs_BranchSpecific()
		{
			AssertCheckBox("BranchSpecificCheckBox");
		}

		public void TestCheckBoxWithNoEdocs_DepartmentSpecific()
		{
			AssertCheckBox("DepartmentSpecificCheckBox");
		}

		public void TestCheckBoxWithNoEdocs_ShowDeletedDocuments()
		{
			AssertCheckBox("ShowDeletedDocumentsCheckBox");
		}

		public void TestModuleIsZPopupModule()
		{
			var module = new CandidateManagementModule();
			Assert(module is ZPopupModule);
			module.Dispose();
		}

		public void TestFormIsZForm()
		{
			var module = new CandidateManagementModule();
			var controller = (CandidateManagementPopupFormController)module.GetNewControllerForTest();
			var collection = new CandidateBusinessObjectCollection(Factory);
			var businessObject = new CandidateModuleBusinessObject(collection);
			var form = controller.GetFormForTest(businessObject);
			Assert(form is ZForm);
			form.Dispose();
			module.Dispose();
		}

		public void TestNotSupportsHyperlinking()
		{
			using (var module = new CandidateManagementModule())
			{
				var controller = (CandidateManagementPopupFormController)module.GetNewControllerForTest();
				AssertEquals(controller.SupportsHyperlinking, false);
			}
		}

		void AssertCheckBox(string checkBoxName)
		{
			var john = CreateCandidate(Factory, "John Smith");
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();
				AssertGreaterThanOrEqualTo("PRE: We made two candidates, they should be there (at the very least)", grid.List.Count, 2);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "edocsTabPage");
				Application.DoEvents();

				var userControl = (eDocsUserControl)form.Controls.Find("eDocsUserControl", true).Single();
				var storageMain = userControl.CurrentDataItem as StorageMain;

				var checkBox = (ZCheckBox)form.Controls.Find(checkBoxName, true).Single();
				checkBox.Checked = true;
				Application.DoEvents();

				AssertEquals(0, storageMain.eDocs.Count);
			}
		}

		public void TestFormDisposesWithoutThrowingExceptionInThumbnailPreviewMode()
		{
			var john = CreateCandidate(Factory, "John Smith", resume: PdfWithDifferentColorsForEachPagePath);
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();
				PerformSearch(form);

				FindDocumentPreviewControl(form).SetThumbnailVisible(true);

				form.Close();
				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestStageColumnSorting()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "HRA";
			template.P0_SubType1 = "AU";
			var templateTask1 = template.WorkflowItems.AddNew();
			templateTask1.P9_Sequence = 1;
			templateTask1.P9_Description = "Task 1";
			var templateTask2 = template.WorkflowItems.AddNew();
			templateTask2.P9_Description = "Task 2";
			templateTask2.P9_Sequence = 2;
			var templateTask3 = template.WorkflowItems.AddNew();
			templateTask3.P9_Description = "Task 3";
			templateTask3.P9_Sequence = 3;

			var campaign1 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application1 = Factory.New<HRJobApplication>();
			var applicant1 = Factory.New<HRJobApplicant>();
			applicant1.HA_FullName = "applicant 1";
			applicant1.HA_EmailAddress = "applicant1@gmail.com";
			applicant1.HA_RN_NKCountry = "AU";
			application1.HP_HA = applicant1.PK;
			application1.HP_HV = campaign1.PK;
			application1.SubmissionTimeLocal = ZDateTime.Today.AddDays(-2);

			var application2 = Factory.New<HRJobApplication>();
			var applicant2 = Factory.New<HRJobApplicant>();
			applicant2.HA_FullName = "applicant 2";
			applicant2.HA_EmailAddress = "applicant2@gmail.com";
			applicant2.HA_RN_NKCountry = "AU";
			application2.HP_HA = applicant2.PK;
			application2.HP_HV = campaign1.PK;
			application2.SubmissionTimeLocal = ZDateTime.Today.AddDays(-10);

			var application3 = Factory.New<HRJobApplication>();
			var applicant3 = Factory.New<HRJobApplicant>();
			applicant3.HA_FullName = "applicant 3";
			applicant3.HA_EmailAddress = "applicant3@gmail.com";
			applicant3.HA_RN_NKCountry = "AU";
			application3.HP_HA = applicant3.PK;
			application3.HP_HV = campaign1.PK;
			application3.SubmissionTimeLocal = ZDateTime.Today.AddDays(-6);

			var campaign2 = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var application4 = Factory.New<HRJobApplication>();
			application4.HP_HA = applicant1.PK;
			application4.HP_HV = campaign2.PK;
			application4.SubmissionTimeLocal = ZDateTime.Today.AddDays(-8);

			Factory.Save();

			application1.WorkflowItems.Tasks.CreateItemsFromTemplate();
			application2.WorkflowItems.Tasks.CreateItemsFromTemplate();
			application4.WorkflowItems.Tasks.CreateItemsFromTemplate();

			// application1's stage is sequence 1
			application1.WorkflowItems.Tasks[0].P9_Status = "CLS";
			application1.WorkflowItems.Tasks[0].P9_Outcome = ProcessTaskView.TaskP9OutcomeFail;
			application1.WorkflowItems.Tasks[1].P9_Status = "ASN";
			application1.WorkflowItems.Tasks[2].P9_Status = "ASN";

			// application2's stage is sequence 2
			application2.WorkflowItems.Tasks[0].P9_Status = "CLS";
			application2.WorkflowItems.Tasks[0].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application2.WorkflowItems.Tasks[1].P9_Status = "ASN";
			application2.WorkflowItems.Tasks[2].P9_Status = "ASN";

			// application3's stage is sequence -1

			// application4's stage is sequnce 3
			application4.WorkflowItems.Tasks[0].P9_Status = "CLS";
			application4.WorkflowItems.Tasks[0].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application4.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application4.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application4.WorkflowItems.Tasks[2].P9_Status = "ASN";

			Factory.Save();

			ZGuid[] ascendingOrder = { application1.PK, application2.PK, application4.PK, application3.PK };
			ZGuid[] descendingOrder = { application3.PK, application4.PK, application2.PK, application1.PK };
			var allApplications = new HashSet<ZGuid>() { application1.PK, application2.PK, application3.PK, application4.PK };

			ZGuid[] ascedingTimeOrder = { application2.PK, application4.PK, application3.PK, application1.PK };

			using (var form = GetFormToBash())
			{
				form.Show();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				grid.List.ApplySort(TypeDescriptor.GetProperties(grid.List[0])["Stage"], ListSortDirection.Ascending);
				AssertContainsExactElementsInExactOrder(ascendingOrder, GetCandidates(grid.List, allApplications));

				grid.List.ApplySort(TypeDescriptor.GetProperties(grid.List[0])["Stage"], ListSortDirection.Descending);
				AssertContainsExactElementsInExactOrder(descendingOrder, GetCandidates(grid.List, allApplications));

				grid.List.ApplySort(TypeDescriptor.GetProperties(grid.List[0])["Stage"], ListSortDirection.Ascending);
				AssertContainsExactElementsInExactOrder(ascendingOrder, GetCandidates(grid.List, allApplications));

				grid.List.ApplySort(TypeDescriptor.GetProperties(grid.List[0])["Application+SubmissionTimeLocal"], ListSortDirection.Ascending);
				AssertContainsExactElementsInExactOrder(ascedingTimeOrder, GetCandidates(grid.List, allApplications));

				grid.List.ApplySort(TypeDescriptor.GetProperties(grid.List[0])["Stage"], ListSortDirection.Ascending);
				AssertContainsExactElementsInExactOrder(ascendingOrder, GetCandidates(grid.List, allApplications));

				form.Close();
			}
		}

		List<ZGuid> GetCandidates(IBindingList list, HashSet<ZGuid> allApplications)
		{
			List<ZGuid> result = new List<ZGuid>();

			foreach (var element in list)
			{
				if (allApplications.Contains(((Candidate)element).Application.PK))
				{
					result.Add(((Candidate)element).Application.PK);
				}
			}
			return result;
		}

		public void TestFormCloseWarning_WithoutChanges()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "email1@gmail.com";
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Close();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestFormCloseWarning_ChangesNotSave()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "email1@gmail.com";
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.Close();
				AssertEquals("Msg shown for saving changes", "This record has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("email1@gmail.com", scott.Applicant.HA_EmailAddress);
			}
		}

		public void TestFormCloseWarning_ChangesCancel()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "email1@gmail.com";
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.Close();
				AssertEquals(false, form.IsDisposed);
				AssertEquals("Msg shown for saving changes", "This record has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("emailchanged@gmail.com", emailTextBox.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.Close();
				AssertEquals(true, form.IsDisposed);
				AssertEquals("Msg shown for saving changes", "This record has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormCloseWarning_ChangesSave()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "email1@gmail.com";
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.Close();
				AssertEquals(true, form.IsDisposed);
				AssertEquals("Msg shown for saving changes", "This record has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("emailchanged@gmail.com", scott.Applicant.HA_EmailAddress);
			}
		}

		public void TestFormCloseWarning_ChangesSaveValidationFails()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "email1@gmail.com";
			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Close();
				AssertEquals(false, form.IsDisposed);

				AssertEquals("Msg shown for saving changes", "This record has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("There are errors that need to be corrected before this Candidate can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("@gmail.com", emailTextBox.Text);
				AssertEquals("email1@gmail.com", scott.Applicant.HA_EmailAddress);
			}
		}

		public void TestReselectCandidateWarning_SameCandidate()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailscottchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(scott.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
				AssertEquals("emailscottchanged@gmail.com", emailTextBox.Text);
			}
		}

		public void TestReselectCandidateWarning_WithoutChanges()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(john.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
			}
		}

		public void TestReselectCandidateWarning_ChangesNotSave()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailscottchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Msg shown for saving changes", "You have unsaved changes.\r\nWould you like to save these changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(john.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
				AssertEquals("emailscott@gmail.com", scott.Applicant.HA_EmailAddress);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Close();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestReselectCandidateWarning_ChangesCancel()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailscottchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Msg shown for saving changes", "You have unsaved changes.\r\nWould you like to save these changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(scott.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
				AssertEquals("emailscottchanged@gmail.com", emailTextBox.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Msg shown for saving changes", "You have unsaved changes.\r\nWould you like to save these changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReselectCandidateWarning_ChangesSave()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailscottchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Msg shown for saving changes", "You have unsaved changes.\r\nWould you like to save these changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(john.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
				AssertEquals("emailscottchanged@gmail.com", scott.Applicant.HA_EmailAddress);
			}
		}

		public void TestReselectCandidateWarning_ChangesSaveValidationFails()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				AssertEquals("Msg shown for saving changes", "You have unsaved changes.\r\nWould you like to save these changes?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("There are errors that need to be corrected before this Candidate can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(scott.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
				AssertEquals("@gmail.com", emailTextBox.Text);
			}
		}

		public void TestReselectCandidateWarning_CancelFormClose()
		{
			var scott = CreateCandidate(Factory, "Scott Fakename");
			scott.Applicant.HA_EmailAddress = "emailscott@gmail.com";

			var john = CreateCandidate(Factory, "John Fakename");
			john.Applicant.HA_EmailAddress = "emailjohn@gmail.com";

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();

				Application.DoEvents();
				PerformSearch(form);

				var grid = (ZGrid)form.Controls.Find("grid", true).Single();

				_ = SelectCandidate(grid, scott);
				Application.DoEvents();

				var details = GetDetailsControl(form);
				var tabControl = (ZTabControl)details.Controls.Find("tabControl", true).Single();
				tabControl.SelectedTab = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "profileTabPage");
				Application.DoEvents();

				var emailTextBox = (ZTextBox)form.Controls.Find("emailTextBox", true).Single();
				emailTextBox.Text = "emailscottchanged@gmail.com";
				grid.Focus();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.Close();
				AssertEquals(false, form.IsDisposed);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				_ = SelectCandidate(grid, john);
				Application.DoEvents();

				AssertEquals("Msg shown for saving changes", "You have unsaved changes.\r\nWould you like to save these changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(john.Applicant.PK, ((Candidate)grid.GetCurrent()).Applicant.PK);
				AssertEquals("emailscottchanged@gmail.com", scott.Applicant.HA_EmailAddress);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentScanning.Business.Test.TestUtils).Assembly));

		string pdfWithDifferentColorsForEachPagePath;
		string PdfWithDifferentColorsForEachPagePath
		{
			get
			{
				if (string.IsNullOrEmpty(pdfWithDifferentColorsForEachPagePath))
				{
					pdfWithDifferentColorsForEachPagePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfWithDifferentColorsForEachPage.pdf");
				}
				return pdfWithDifferentColorsForEachPagePath;
			}
		}

		static void PerformSearch(Form form)
		{
			FindStripControl(form).FirePerformSearch();
			Application.DoEvents();
		}

		static ZFilterStripBaseControl FindStripControl(Form form)
			=> (ZFilterStripBaseControl)form.Controls.Find("stripControl", true).Single();

		static GraphicalDisplayControl FindDocumentPreviewControl(Form form)
			=> (GraphicalDisplayControl)form.Controls.Find("documentPreview", true).Single();

		static ZFilterStripBaseControl GetStripControl(Form form)
			=> (ZFilterStripBaseControl)form.Controls.Find("stripControl", true).Single();

		static int SelectCandidate(ZGrid grid, Candidate seek)
			=> SelectCandidate(grid, seek.Applicant.Name);

		static int SelectCandidate(ZGrid grid, string name)
		{
			for (var i = 0; i < grid.List.Count; i++)
			{
				if (grid.List[i] is Candidate c && c.Applicant.Name == name)
				{
					grid.CurrentCell = new DataGridCell(i, 0);
					grid.Select(i);

					Application.DoEvents();
					return i;
				}
			}

			throw new ArgumentException("The user is not in the grid");
		}

		static int SelectApplication(ZGrid grid, ZGuid campaignPK)
		{
			for (var i = 0; i < grid.List.Count; i++)
			{
				if (grid.List[i] is HRJobApplication c && c.HP_HV == campaignPK)
				{
					grid.CurrentCell = new DataGridCell(i, 0);
					grid.Select(i);

					Application.DoEvents();
					return i;
				}
			}

			throw new ArgumentException("The application is not in the grid");
		}

		static CandidateDetailsControl GetDetailsControl(Control root)
			=> (CandidateDetailsControl)root.Controls.Find("candidateDetailsControl", true).Single();
	}
}
