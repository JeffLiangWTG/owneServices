using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ProcessManagement.Business.JiraConstants;

namespace Enterprise.ProcessManagement.GUI.Test
{
	public class JiraProjectSelectorUserControlTest : TestCaseWithFactory
	{
		public void TestImportFromJiraMenuItem_WhenSuccessful_ShouldNotShowError()
		{
			var dummyViewModel = CreateDummyViewModelWithCustomImportResult(new ProjectImportReport(true, ProjectImportReport.Success()));

			CreateSelectorFormAndBeginImport(dummyViewModel);

			AssertEquals("This text should be the text of the dialog shown after an unsuccessful data import.", ProjectImportReport.Success(), UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("A successful import tells the user, not errors the user", UnitTestUserNotification.Instance.LastMessage.WasInformation);
		}

		public void TestBeginImportOnAnIncorrectJiraUrl_ShouldInformUser()
		{
			var dummyViewModel = CreateDummyViewModelWithCustomImportResult(new ProjectImportReport(false, ProjectImportReport.IncorrectJiraUrl));
			CreateSelectorFormAndBeginImport(dummyViewModel);

			AssertEquals("This text should be the text of the dialog shown after an unsuccessful data import.", ProjectImportReport.IncorrectJiraUrl, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("An import from an incorrect URL shows an error to the user", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestImportFromJiraMenuItem_WithBadCredentials_ShouldInformUser()
		{
			var dummyViewModel = CreateDummyViewModelWithCustomImportResult(new ProjectImportReport(false, ProjectImportReport.BadCredentials));

			CreateSelectorFormAndBeginImport(dummyViewModel);

			AssertEquals("This text should be the text of the dialog shown after an unsuccessful data import.", ProjectImportReport.BadCredentials, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("A successful import tells the user that there was an error", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestImportFromJiraMenuItem_WithErrorsAndSavedData_ShouldInformUser()
		{
			var dummyViewModel = CreateDummyViewModelWithCustomImportResult(new ProjectImportReport(false, ProjectImportReport.BadCredentials + "\r\n\t" + ProjectImportReport.PartiallySavedImport));

			CreateSelectorFormAndBeginImport(dummyViewModel);

			AssertEquals("This text should be the text of the dialog shown after an unsuccessful data import where some data was saved.", ProjectImportReport.BadCredentials + "\r\n\t" + ProjectImportReport.PartiallySavedImport, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("A successful import tells the user that there was an error", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestImport_ShouldDisposeProgressForm()
		{
			var dummyViewModel = CreateDummyViewModelWithCustomImportResult(new ProjectImportReport(false, ProjectImportReport.BadCredentials + "\r\n\t" + ProjectImportReport.PartiallySavedImport));

			using (var selectorForm = new JiraProjectSelectorForm(Factory))
			{
				selectorForm.SetDataBinding(dummyViewModel, "");

				using (var importUserControl = new DummyJiraProjectSelectorUserControl())
				{
					AssertEquals("Progress form is uncreated and so undisposed", false, importUserControl.ProgressFormWasDisposed);

					importUserControl.BeginImport_ForTest(selectorForm);

					AssertEquals("Progress form was created and was disposed of automatically, and so is disposed", true, importUserControl.ProgressFormWasDisposed);
				}
			}
		}

		public void TestImport_JiraSystemCodeDropList_CaptionFullDescription_ContainsCorrectPath()
		{
			var dummyViewModel = CreateDummyViewModelWithCustomImportResult(new ProjectImportReport(false, ""));

			CreateSelectorFormAndBeginImport(dummyViewModel);

			using (var selectorForm = new JiraProjectSelectorForm(Factory))
			{
				selectorForm.SetDataBinding(dummyViewModel, "");

				using (var importUserControl = new DummyJiraProjectSelectorUserControl())
				{
					var control = selectorForm.FindSingle<ZDropEdit>("JiraSystemCodeDropList");
					AssertContains(ProcessManagementRegistry.Instance.JiraSiteUrls.Inner.Location, control.CaptionResourceString.FullDescription);
				}
			}
		}

		#region Implementation

		DummyProjectsToImportViewModel CreateDummyViewModelWithCustomImportResult(ProjectImportReport importResult)
		{
			return new DummyProjectsToImportViewModel(Factory, JiraAPIVersions.JiraRequestAPI_3)
			{
				CustomImportResult = importResult,
				JiraUserName = "WhoAmI?",
				JiraAuthToken = "NoneOfYourBusiness!"
			};
		}

		void CreateSelectorFormAndBeginImport(DummyProjectsToImportViewModel viewModel)
		{
			using (var selectorForm = new JiraProjectSelectorForm(Factory))
			{
				selectorForm.SetDataBinding(viewModel, "");

				using (var importUserControl = new DummyJiraProjectSelectorUserControl())
				{
					importUserControl.BeginImport_ForTest(selectorForm);
				}

				AssertEquals("An import should always close the progress form", false, Application.OpenForms.OfType<IProgressForm>().Any());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem();
		}

		#endregion
	}
}
