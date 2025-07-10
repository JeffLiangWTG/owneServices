using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using ConvertApiDotNet.Exceptions;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.GUI;
using Enterprise.EConversation.GUI;
using Enterprise.Integration;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

using static Enterprise.Recruitment.Testing.RecruitmentDataHelpers;

namespace Enterprise.Recruitment.Testing.Module
{
	sealed class CandidateDetailsControlTest : TestCaseWithFactory
	{
		public void TestPostingButtons_Visibility()
		{
			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				var posting = (ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single();
				CombineAssertions("We can only use the save button", () =>
				{
					Assert("The SaveAndClose button should be hidden.", !posting.SaveAndCloseButton.Visible);
					Assert("The Close / Cancel button should be hidden.", !posting.CloseButton.Visible);
					Assert("We can only actually use the Save button so it should be visible.", posting.SaveButton.Visible);
				});
			}
		}

		public void TestPostingButtons_EnabledByBizo()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			var bill = CreateCandidate(Factory, "Bill Bobson", PdfWithDifferentColorsForEachPagePath);

			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;

				Assert("We haven't bound yet - Save is disabled", !saveButton.Enabled);

				bill.Application.HP_ApplicationOverallRating = RatingValues.Suitable.ToString();
				control.SetDataBinding(bill, string.Empty);
				Application.DoEvents();
				Assert("When binding to a bizo with existing changes, save button should be enabled", saveButton.Enabled);

				control.SetDataBinding(borris, string.Empty);
				Application.DoEvents();
				Assert("When binding to a bizo with no changes, save should be disabled", !saveButton.Enabled);

				borris.Application.HP_ApplicationOverallRating = RatingValues.Suitable.ToString();
				Application.DoEvents();
				Assert("When a bound bizo is changed, the save button should be enabled", saveButton.Enabled);

				control.ValidateAndSave();
				Application.DoEvents();
				Assert("After a save occurs, the bizo should have no changes and save is disabled", !saveButton.Enabled);
			}
		}

		public void TestPostingButtons_EnabledByEDocs()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash<TestCandidateDetailsControl>(out var control))
			{
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;

				Assert("We haven't bound yet - Save is disabled", !saveButton.Enabled);

				control.SetDataBinding(borris, string.Empty);
				Assert("Bound object has no changes - Save is disabled", !saveButton.Enabled);

				_ = AddResume(borris.Application, PdfWithDifferentColorsForEachPagePath);

				Assert("eDocs control should have changes", control.eDocsControl_Exposed.HasChanges);
				Assert("Bound object's eDocs has changes - Save is enabled", saveButton.Enabled);

				control.ValidateAndSave();

				Assert("Save occured. Bound object has no changes. Save is disabled", !saveButton.Enabled);
			}
		}

		public void TestPostingButtons_SaveButtonEnabledWhenALogIsAdded()
		{
			//arrange 
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash<CandidateDetailsControl>(out var control))
			{
				control.SetDataBinding(borris, string.Empty);
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;
				Assert("Save button is Disabled at start", !saveButton.Enabled);
				var eventValue = new EventValue(AutoEvents.AddedARecordToTheSystem, ZDateTimeOffset.UtcNow.AddMinutes(-30));
				//act
				borris.Application.Logs.AddNew(eventValue);
				Application.DoEvents();

				//assert
				Assert("Candidate should have changes", borris.HasChanges);
				Assert("Application should have changes", borris.Application.HasChanges);
				Assert("Bound object's eDocs has changes - Save is enabled", saveButton.Enabled);
			}
		}

		public void TestRebindingUpdatedEdocsGridAsExpected()
		{
			var john = CreateCandidate(Factory, "John Smith", PdfWithDifferentColorsForEachPagePath, "john@me.hoff", false, true);
			var jack = CreateCandidate(Factory, "Jack Jones", PdfWithDifferentColorsForEachPagePath, "jack@me.hoff", false, true);
			Factory.Save();

			var path1 = AddEdocToCandidate(john);
			var path2 = AddEdocToCandidate(jack);

			try
			{
				Factory.Save();

				using (var form = GetFormToBash<TestCandidateDetailsControl>(out var control))
				{
					form.Show();

					// arrange
					control.SetDataBinding(john, string.Empty);
					var johneDocsControlStorage = (IStorageMain)control.eDocsControl_Exposed.eDocsUserControl.CurrentDataItem;
					var johnDocStorage = john.Application.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain((BusinessObject)(object)john.Application, john.Application.DocManagerInfo.DocManagerCode);

					//assert
					AssertEquals(johnDocStorage.Files[0].FileName, johneDocsControlStorage.Files[0].FileName);
					AssertEquals(Path.GetFileName(path1), johneDocsControlStorage.Files[0].FileName);

					// arrange
					control.SetDataBinding(jack, string.Empty);
					var jackeDocsControlStorage = (IStorageMain)control.eDocsControl_Exposed.eDocsUserControl.CurrentDataItem;
					var jackDocStorage = jack.Application.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain((BusinessObject)(object)jack.Application, jack.Application.DocManagerInfo.DocManagerCode);

					//assert
					AssertEquals(jackeDocsControlStorage.Files[0].FileName, jackeDocsControlStorage.Files[0].FileName);
					AssertEquals(Path.GetFileName(path2), jackeDocsControlStorage.Files[0].FileName);

					AssertNotEquals(johneDocsControlStorage, jackeDocsControlStorage);
				}
			}
			finally
			{
				File.Delete(path1);
				File.Delete(path2);
			}
		}

		public string AddEdocToCandidate(Candidate c)
		{
			var tmp = TempForTest.GetTempFileName();
			File.WriteAllText(tmp, "_test_" + c.PK);
			_ = c.Application.DocManagerInfo.AddFileOrDocument(tmp, RecruiterDataRegistry.Instance.DocTypeCVCode);
			return tmp;
		}

		public void TestSaveButton()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(borris, string.Empty);

				borris.Application.HP_ApplicationOverallRating = "-1";

				var saveButton = FindSaveButton(control);
				saveButton.PerformClick();

				CombineAssertions("Save occured.", () =>
				{
					Assert("Bound object has no changes", !borris.HasChanges);
					Assert("Save is disabled", !saveButton.Enabled);
				});
			}
		}

		public void TestSaveButton_ConcurrencyException()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;
				control.SetDataBinding(borris, string.Empty);

				var other = new BusinessObjectFactory { RefreshEnabled = false };
				other.Load<HRJobApplication>(borris.Application.PK).HP_ApplicationOverallRating = RatingValues.Potential.ToString();
				other.Save();

				borris.Application.HP_ApplicationOverallRating = RatingValues.Unsuitable.ToString();

				Application.DoEvents();

				saveButton.PerformClick();

				AssertContains("We should show the user an error message", "While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveButton_ValidationError()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;
				control.SetDataBinding(borris, string.Empty);

				borris.Application.HP_ApplicationOverallRating = "7";

				var saveCount = BusinessObjectFactory.GlobalSaveCount;
				saveButton.PerformClick();

				AssertEquals("Save should not occur when there are validation errors", saveCount, BusinessObjectFactory.GlobalSaveCount);
				AssertContains("We should show the user an error message", "There are errors that need to be corrected before this Candidate can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApplicationOverallRatingChanged_RejectionEmailQueued()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(borris, string.Empty);

				borris.Application.HP_ApplicationOverallRating = "3";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				var saveButton = FindSaveButton(control);
				saveButton.PerformClick();

				CombineAssertions("Save occurred.", () =>
				{
					var logs = borris.RejectionLogs.First();

					AssertEventLogIsValid(logs, HRJobApplicationEvent.RejectionEmailQueued, "Rejection Email Queued");
				});
			}
		}

		public static void AssertEventLogIsValid(StmALog log, HRJobApplicationEvent expectedEvent, string expectedEventInfo)
		{
			AssertNotNull(log);
			AssertEquals(false, log.IsCancelled);
			AssertNotNull(log.Parameters);
			if (log.Event.SE_Code == "RCE")
			{
				var eventName = log.Parameters["Event"];
				AssertEquals(expectedEvent.ToString(), eventName);

				var info = log.Parameters["Info"];
				AssertEquals(expectedEventInfo, info);
			}
			else
			{
				var eventName = expectedEvent.ToString();
				AssertEquals(expectedEvent.ToString(), eventName);
				AssertEquals(expectedEventInfo, expectedEventInfo);
			}
		}

		public void TestApplicationOverallRatingChanged_RejectionEmailCanceled()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(borris, string.Empty);

				borris.Application.HP_ApplicationOverallRating = "3";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				var saveButton = FindSaveButton(control);
				saveButton.PerformClick();

				CombineAssertions("Save occurred.", () =>
				{
					var logs = borris.RejectionLogs.First();

					AssertEventLogIsValid(logs, HRJobApplicationEvent.RejectionEmailQueued, "Rejection Email Queued");
				});

				borris.Application.HP_ApplicationOverallRating = "-1";

				saveButton.PerformClick();

				var rejectionLogs = borris.RejectionLogs;
				var log = rejectionLogs.First();
				AssertEquals(true, log.IsCancelled);
				AssertEquals(1, rejectionLogs.Count());
			}
		}

		public void TestPreviewPaneUpdatedOnRebind_AddToApplication()
		{
			var ava = CreateCandidate(Factory, "Ava Mia", PdfWithDifferentColorsForEachPagePath, addApplicationResume: true, addApplicantResume: false);
			var isabella = CreateCandidate(Factory, "Isabella Emily", PdfWithDifferentColorsForEachPagePath, addApplicationResume: true, addApplicantResume: false);
			var thisGuyForgotHisCV = CreateCandidate(Factory, "Homer Simpson", resume: null, addApplicationResume: true, addApplicantResume: false);

			Factory.Save();
			AssertCandidateResumes(ava, isabella, thisGuyForgotHisCV);
		}

		public void TestPreviewPaneUpdatedOnRebind_AddToApplicant()
		{
			var ava = CreateCandidate(Factory, "Ava Mia", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			var isabella = CreateCandidate(Factory, "Isabella Emily", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			var thisGuyForgotHisCV = CreateCandidate(Factory, "Homer Simpson", resume: null, addApplicationResume: false, addApplicantResume: true);

			Factory.Save();
			AssertCandidateResumes(ava, isabella, thisGuyForgotHisCV);
		}

		public void TestPreviewPaneUpdatedOnRebind_FallbackWithSameDate()
		{
			var john = CreateCandidate(Factory, "John Smith");
			var applicantResume = AddResume(john.Applicant, PdfWithDifferentColorsForEachPagePath);
			var applicationResume = AddResume(john.Application, PdfWithDifferentColorsForEachPagePath);

			var identicalDate = new ZDateTime(2015, 11, 12, 12, 0, 0);
			applicantResume.SC_Date = identicalDate;
			applicationResume.SC_Date = identicalDate;

			Factory.Save();

			AssertEquals(applicantResume.SC_Date.ToString(), applicationResume.SC_Date.ToString());
			AssertResume(john, applicationResume);
		}

		[TestDate(2015, 11, 12, 12, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestPreviewPaneUpdatedOnRebind_ApplicationFallbackWithDiffDate()
		{
			var ava = CreateCandidate(Factory, "Ava Mia");
			var applicationResume1 = AddResume(ava.Application, PdfWithDifferentColorsForEachPagePath);
			Factory.Save();

			AssertResume(ava, applicationResume1);

			var applicationResume2 = AddResume(ava.Application, PdfWithDifferentColorsForEachPagePath);
			applicationResume2.SC_Date = ZDateTime.UtcNow.AddMinutes(2);
			Factory.Save();

			AssertNotEquals(applicationResume1.PK, applicationResume2.PK);
			AssertResume(ava, applicationResume2);

			var applicantResume = AddResume(ava.Applicant, PdfWithDifferentColorsForEachPagePath);
			applicantResume.SC_Date = ZDateTime.UtcNow.AddMinutes(2);
			Factory.Save();

			AssertResume(ava, applicationResume2);
		}

		[TestDate(2015, 11, 12, 12, 0, 0)]
		[TestUtcOffset(11, 0, 0)]
		public void TestPreviewPaneUpdatedOnRebind_ApplicantFallbackWithDiffDate()
		{
			var ava = CreateCandidate(Factory, "Ava Mia");
			var applicantResume1 = AddResume(ava.Applicant, PdfWithDifferentColorsForEachPagePath);
			Factory.Save();

			AssertResume(ava, applicantResume1);

			var applicantResume2 = AddResume(ava.Applicant, PdfWithDifferentColorsForEachPagePath);
			applicantResume2.SC_Date = ZDateTime.UtcNow.AddMinutes(2);
			Factory.Save();

			AssertNotEquals(applicantResume1.PK, applicantResume2.PK);
			AssertResume(ava, applicantResume2);

			var applicationResume = AddResume(ava.Application, PdfWithDifferentColorsForEachPagePath);
			applicationResume.SC_Date = ZDateTime.UtcNow.AddMinutes(2);
			Factory.Save();

			AssertResume(ava, applicationResume);
		}

		void AssertResume(Candidate candidate, DocumentScanning.Business.StorageDocsBase applicationResume)
		{
			using (var form = GetFormToBash(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				detailsControl.SetDataBinding(candidate, string.Empty);
				Application.DoEvents();

				AssertEquals(candidate.Resume.PK, applicationResume.PK);
				AssertEquals("We should automatically display the candidates resume when they are selected", previewControl.Document.PK, candidate.Resume.PK);
			}
		}

		void AssertCandidateResumes(Candidate candidate1, Candidate candidate2, Candidate candidateWithoutCv)
		{
			using (var form = GetFormToBash(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				detailsControl.SetDataBinding(candidate1, string.Empty);
				Application.DoEvents();

				AssertEquals("We should automatically display the candidates resume when they are selected", previewControl.Document.PK, candidate1.Resume.PK);

				detailsControl.SetDataBinding(candidate2, string.Empty);
				Application.DoEvents();

				AssertEquals("We should update the resume preview when changing candidates", previewControl.Document.PK, candidate2.Resume.PK);

				detailsControl.SetDataBinding(candidateWithoutCv, string.Empty);
				AssertNull("When a CV cant be found for a user we should clear the preview pane", previewControl.Document);

				var dummyPreviewable = previewControl.PreviewableDocument as DummyPreviewable;
				AssertEquals("A resume in a supported format could not be found", dummyPreviewable.ErrorMessage);
			}
		}

		public void TestConvertUnsupportedDocOntheFly()
		{
			var john = CreateCandidate(Factory, "John Smith");
			var johnDoc = AddResume(john.Application, UnsupportedFormatDocxPath);
			Factory.Save();

			using (var form = GetFormToBashForAsync(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				var converter = new ResumeConverterForDelayedTest();
				using (ObjectFactory.Substitute<IResumeConverter>(converter))
				{
					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					AssertNull(john.Resume);
					AssertNull(previewControl.Document);

					var dummyPreviewable = previewControl.PreviewableDocument as DummyPreviewable;
					AssertEquals("Resume is converting to correct format", dummyPreviewable.ErrorMessage);

					Task.WaitAll(converter.Conversions.ToArray());
					Application.DoEvents();

					AssertNotNull(john.Resume);
					AssertNotEquals(john.Resume.PK, johnDoc.PK);
					AssertNotNull(previewControl.Document);
					AssertEquals("UnsupportedFormat.pdf", previewControl.Document.SC_FileNameWithExtension);
				}
			}
		}

		public void TestConvertUnsupportedDocOntheFly_ShowingCorrectMessages()
		{
			var john = CreateCandidate(Factory, "John Smith");
			var johnDoc = AddResume(john.Application, UnsupportedFormatDocxPath);
			var chris = CreateCandidate(Factory, "Chris Smith");

			Factory.Save();

			using (var form = GetFormToBashForAsync(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				var converter = new ResumeConverterForDelayedTest(delayForConversion: 2000);
				using (ObjectFactory.Substitute<IResumeConverter>(converter))
				{
					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					AssertNull(john.Resume);
					AssertNull(previewControl.Document);

					var dummyPreviewable = previewControl.PreviewableDocument as DummyPreviewable;
					AssertEquals("Resume is converting to correct format", dummyPreviewable.ErrorMessage);

					detailsControl.SetDataBinding(chris, string.Empty);
					Application.DoEvents();

					previewControl = FindPreviewControl(detailsControl);
					dummyPreviewable = previewControl.PreviewableDocument as DummyPreviewable;
					AssertEquals("A resume in a supported format could not be found", dummyPreviewable.ErrorMessage);

					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					previewControl = FindPreviewControl(detailsControl);
					dummyPreviewable = previewControl.PreviewableDocument as DummyPreviewable;
					AssertEquals("Resume is converting to correct format", dummyPreviewable.ErrorMessage);

					Task.WaitAll(converter.Conversions.ToArray());
					Application.DoEvents();

					AssertNotNull(john.Resume);
					AssertEquals(2, john.Application.DocManagerInfo.AllEDocs.Count);
					AssertNotEquals(john.Resume.PK, johnDoc.PK);
					AssertNotNull(previewControl.Document);
					AssertEquals("UnsupportedFormat.pdf", previewControl.Document.SC_FileNameWithExtension);
				}
			}
		}

		public void TestConvertUnsupportedDocOntheFly_SwichingCandidateMultipleTimes()
		{
			var john = CreateCandidate(Factory, "John Smith");
			var johnDoc = AddResume(john.Application, UnsupportedFormatDocxPath);
			var chris = CreateCandidate(Factory, "Chris Smith");

			Factory.Save();

			using (var form = GetFormToBashForAsync(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				var converter = new ResumeConverterForDelayedTest(delayForConversion: 2000);
				using (ObjectFactory.Substitute<IResumeConverter>(converter))
				{
					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					AssertNull(john.Resume);
					AssertNull(previewControl.Document);

					detailsControl.SetDataBinding(chris, string.Empty);
					Application.DoEvents();

					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					detailsControl.SetDataBinding(chris, string.Empty);
					Application.DoEvents();

					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					detailsControl.SetDataBinding(chris, string.Empty);
					Application.DoEvents();

					detailsControl.SetDataBinding(john, string.Empty);
					Application.DoEvents();

					Task.WaitAll(converter.Conversions.ToArray());
					Application.DoEvents();

					AssertNotNull(john.Resume);
					AssertEquals(2, john.Application.DocManagerInfo.AllEDocs.Count);
					AssertNotEquals(john.Resume.PK, johnDoc.PK);
					AssertNotNull(previewControl.Document);
					AssertEquals("UnsupportedFormat.pdf", previewControl.Document.SC_FileNameWithExtension);
				}
			}
		}

		public void TestConvertUnsupportedDocOntheFly_ConvertApiException()
		{
			var john = CreateCandidate(Factory, "John Smith");
			_ = AddResume(john.Application, UnsupportedFormatDocxPath);
			Factory.Save();

			using (var form = GetFormToBashForAsync(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				var converter = new ResumeConverterForConvertApiException();
				using (ObjectFactory.Substitute<IResumeConverter>(converter))
				{
					detailsControl.SetDataBinding(john, string.Empty);

					ThreadSleepTillDummyPreviewableAvailable(previewControl, converter);
					AssertEquals("ConvertApiException thrown", (previewControl.PreviewableDocument as DummyPreviewable).ErrorMessage);
				}
			}
		}

		public void TestConvertUnsupportedDocOntheFly_NonCriticalException()
		{
			var john = CreateCandidate(Factory, "John Smith");
			_ = AddResume(john.Application, UnsupportedFormatDocxPath);
			Factory.Save();

			using (var form = GetFormToBashForAsync(out var detailsControl))
			{
				form.Show();
				Application.DoEvents();

				var previewControl = FindPreviewControl(detailsControl);

				var converter = new ResumeConverterForNonCriticalException();
				using (ObjectFactory.Substitute<IResumeConverter>(converter))
				{
					detailsControl.SetDataBinding(john, string.Empty);

					ThreadSleepTillDummyPreviewableAvailable(previewControl, converter);
					AssertEquals("UnknownErrorConvertAttachmentToPDF", ErrorReporter.LastKeyReported);
					AssertEquals("An exception caught while converting to PDF", ErrorReporter.LastMessageReported);
					AssertEquals("Non Critical Message thrown", ErrorReporter.LastExceptionReported.Message);
					AssertEquals("Error converting resume", (previewControl.PreviewableDocument as DummyPreviewable).ErrorMessage);
					ErrorReporter.Clear();
				}
			}
		}

		static void ThreadSleepTillDummyPreviewableAvailable(GraphicalDisplayControl previewControl, ResumeConverterForDelayedTest converter)
		{
			DummyPreviewable dummyPreviewable = null;
			for (int i = 1; i < 5; i++)
			{
				if (dummyPreviewable == null)
				{
					converter.Conversions.First().ContinueWith(t => Thread.Sleep(1000)).Wait();
					Application.DoEvents();
				}
				dummyPreviewable = previewControl.PreviewableDocument as DummyPreviewable;
			}
		}

		public void TestRebindingWorksAsExpected()
		{
			var john = CreateCandidate(Factory, "John Smith", email: "john@me.off");
			var jack = CreateCandidate(Factory, "Jack Jones", email: "jack@me.off");

			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				NavigateToTabPage(control, "profileTabPage");

				var email = GetEmailTextBox(control);

				control.SetDataBinding(john, string.Empty);
				Application.DoEvents();

				AssertEquals("Should contain the bound objects email", email.Text, "john@me.off");

				control.SetDataBinding(jack, string.Empty);
				Application.DoEvents();
				AssertEquals("Should contain the newly bound objects email", email.Text, "jack@me.off");
			}
		}

		public void TestNotes_SaveButtonOnAddNote()
		{
			var otherFactory = new BusinessObjectFactory();
			var scott = CreateCandidate(otherFactory, "Scott Fakename");
			otherFactory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;
				Assert("We haven't bound yet - Save is disabled", !saveButton.Enabled);

				control.SetDataBinding(scott, string.Empty);
				Assert("Bound object has no changes - Save is disabled", !saveButton.Enabled);

				NavigateToTabPage(control, "notesTabPage");
				var noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();

				var noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				var columnStyle = (ZTextBoxColumnStyle)noteGrid.Columns["ST_Description"].ColumnStyle;
				noteGrid.BeginEdit(columnStyle, 0);
				columnStyle.EditControl.Text = "ABC";
				noteGrid.EndEdit(columnStyle, 0, false);
				Application.DoEvents();

				var richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();

				KeySender.SendKeyPress(richTextBox, Keys.B);
				Application.DoEvents();

				var pos0 = noteGrid.GetCellBounds(0, 0).Location + ControlDpiScalingHelper.NewScaledSize(-5, -5);

				noteGrid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos0.X, pos0.Y, 0));

				Assert("Bound object has changes - Save is enabled", saveButton.Enabled);

				control.ValidateAndSave();
				Assert("Save occured. Bound object has no changes. Save is disabled", !saveButton.Enabled);
			}
		}

		public void TestNotes_SaveButtonOnModifyNote()
		{
			var otherFactory = new BusinessObjectFactory();
			var scott = CreateCandidate(otherFactory, "Scott Fakename");
			otherFactory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;
				Assert("We haven't bound yet - Save is disabled", !saveButton.Enabled);

				control.SetDataBinding(scott, string.Empty);

				NavigateToTabPage(control, "notesTabPage");
				var noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();

				var noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				var columnStyle = (ZTextBoxColumnStyle)noteGrid.Columns["ST_Description"].ColumnStyle;
				noteGrid.BeginEdit(columnStyle, 0);
				columnStyle.EditControl.Text = "ABC";
				noteGrid.EndEdit(columnStyle, 0, false);
				Application.DoEvents();

				var richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();

				KeySender.SendKeyPress(richTextBox, Keys.B);
				Application.DoEvents();
				control.ValidateAndSave();

				Assert("Save occured. Bound object has no changes. Save is disabled", !saveButton.Enabled);

				noteGrid.BeginEdit(columnStyle, 0);
				columnStyle.EditControl.Text = "C";
				noteGrid.EndEdit(columnStyle, 0, false);
				Application.DoEvents();

				Assert("Bound object has changes - Save is enabled", saveButton.Enabled);

				control.ValidateAndSave();
				Assert("Save occured. Bound object has no changes. Save is disabled", !saveButton.Enabled);
			}
		}

		public void TestNotes_RebindWorksAsExpected()
		{
			var scott1 = CreateCandidate(Factory, "Scott Fake 1");
			var scott2 = CreateCandidate(Factory, "Scott Fake 2");

			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(scott1, string.Empty);

				NavigateToTabPage(control, "notesTabPage");
				var noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();

				var noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				var columnStyle = (ZTextBoxColumnStyle)noteGrid.Columns["ST_Description"].ColumnStyle;
				noteGrid.BeginEdit(columnStyle, 0);
				columnStyle.EditControl.Text = "scott 1";
				noteGrid.EndEdit(columnStyle, 0, false);
				Application.DoEvents();

				var richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();

				KeySender.SendKeyPress(richTextBox, Keys.A);
				Application.DoEvents();
				control.ValidateAndSave();

				control.SetDataBinding(scott2, string.Empty);

				NavigateToTabPage(control, "notesTabPage");
				noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();

				noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				columnStyle = (ZTextBoxColumnStyle)noteGrid.Columns["ST_Description"].ColumnStyle;
				noteGrid.BeginEdit(columnStyle, 0);
				columnStyle.EditControl.Text = "scott 2";
				noteGrid.EndEdit(columnStyle, 0, false);
				Application.DoEvents();

				richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();

				KeySender.SendKeyPress(richTextBox, Keys.B);
				Application.DoEvents();
				control.ValidateAndSave();

				control.SetDataBinding(scott1, string.Empty);
				Application.DoEvents();

				noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();
				noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				AssertEquals("scott 1", ((StmNote)noteGrid.List[0]).ST_Description);
				richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();
				AssertEquals("A", richTextBox.Text);

				control.SetDataBinding(scott2, string.Empty);
				Application.DoEvents();

				noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();
				noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				AssertEquals("scott 2", ((StmNote)noteGrid.List[0]).ST_Description);
				richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();
				AssertEquals("B", richTextBox.Text);
			}
		}

		public void TestNotes_NotesControlWorksAsExpectedWhenNoBindingCandidate()
		{
			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				NavigateToTabPage(control, "notesTabPage");
				var noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();
				Assert("Note control should not be enabled when nothing is bound to it", !noteUserControl.Enabled);
			}
		}

		public void TestNotes_SavedNotesParentIsApplication()
		{
			var otherFactory = new BusinessObjectFactory();
			var scott = CreateCandidate(otherFactory, "Scott Fakename");
			otherFactory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(scott, string.Empty);

				NavigateToTabPage(control, "notesTabPage");
				var noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();

				var noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				var columnStyle = (ZTextBoxColumnStyle)noteGrid.Columns["ST_Description"].ColumnStyle;
				noteGrid.BeginEdit(columnStyle, 0);
				columnStyle.EditControl.Text = "ABC";
				noteGrid.EndEdit(columnStyle, 0, false);
				Application.DoEvents();

				var richTextBox = noteUserControl.Controls.Find("RichEdit", true).Single();

				KeySender.SendKeyPress(richTextBox, Keys.A);
				Application.DoEvents();

				control.ValidateAndSave();
				AssertEquals("HRJobApplication", ((StmNote)noteGrid.List[0]).Master.NotesParentTableName);
			}
		}

		public void TestNotes_NotesForApplicationAreShown()
		{
			var otherFactory = new BusinessObjectFactory();
			var scott = CreateCandidate(otherFactory, "Scott Fakename");
			otherFactory.Save();
			var applicationNote = scott.Application.Notes.AddNew();
			applicationNote.ST_Description = "application note";

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(scott, string.Empty);

				NavigateToTabPage(control, "notesTabPage");
				var noteUserControl = (ZStmNoteUserControl)control.Controls.Find("notesControl", true).Single();

				var noteGrid = (ZStmNoteGrid)noteUserControl.Controls.Find("NoteGrid", true).Single();
				AssertEquals("application note", ((StmNote)noteGrid.List[0]).ST_Description);
			}
		}

		public void TestEConversation_SaveButtonOnAddNote()
		{
			var otherFactory = new BusinessObjectFactory();
			var otherBoris = CreateCandidate(otherFactory, "Boris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			otherFactory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				var saveButton = ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;
				Assert("We haven't bound yet - Save is disabled", !saveButton.Enabled);

				var boris = new Candidate(Factory, otherBoris.Application.PK);
				control.SetDataBinding(boris, string.Empty);
				Assert("Bound object has no changes - Save is disabled", !saveButton.Enabled);

				NavigateToTabPage(control, "eConversationTabPage");
				var textbox = (ZAutoCompleteTextBox)control.Controls.Find("econversationMessageTextBox", true).Single();
				textbox.Text = "test string";

				var sendButton = (ZButton)control.Controls.Find("AddInternalCommentButton", true).Single();
				sendButton.PerformClick();

				Assert("Bound object has changes - Save is enabled", saveButton.Enabled);

				control.ValidateAndSave();
				Assert("Save occured. Bound object has no changes. Save is disabled", !saveButton.Enabled);
			}
		}

		public void TestEConversation_RebindWorksAsExpected()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson");
			var jack = CreateCandidate(Factory, "Jack Jones");
			jack.EConversation.RootConversation.AddMessageFromCurrentUser("test", true, false);

			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				NavigateToTabPage(control, "eConversationTabPage");
				var messageListControl = GetEConversationMessageListControl(control);

				control.SetDataBinding(borris, string.Empty);
				Application.DoEvents();
				AssertEquals(1, messageListControl.messagesLayoutPanel.Controls.OfType<ConversationMessageUserControl>().Count());

				control.SetDataBinding(jack, string.Empty);
				Application.DoEvents();
				AssertEquals(2, messageListControl.messagesLayoutPanel.Controls.OfType<ConversationMessageUserControl>().Count());
			}
		}

		public void TestControlsOnTabPagesAreFullyContainedInParentTab()
		{
			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = control.Controls.Find("tabControl", true).SingleOrDefault() as TabControl;
				AssertNotNull("Unable to find the TabControl in this form", tabControl);

				foreach (var tabPage in tabControl.TabPages.Cast<TabPage>())
				{
					foreach (var ctrl in tabPage.Controls.Cast<Control>())
					{
						var res = tabControl.ClientRectangle.Contains(ctrl.Bounds);
						Assert($"[TabPage:{tabPage.Name} Bounds:{tabPage.Bounds}][Control:{ctrl.Name} Bounds:{ctrl.Bounds}] must be fully contained in the parent tab", res);
					}
				}
			}
		}

		public void CallOccurredHelper(string controlName, bool isMobilePhoneNumber, string expectedEventDescription)
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = CreateCandidate(factory, "John Smith");

			if (isMobilePhoneNumber)
			{
				candidate.Applicant.HA_MobilePhone = "+610407081440";
			}
			else
			{
				candidate.Applicant.HA_HomePhone = "+610407081440";
			}

			using (var form = GetFormToBash(out var ctrl))
			{
				form.Show();
				ctrl.SetDataBinding(candidate, string.Empty);
				NavigateToTabPage(ctrl, "profileTabPage");
				var phoneNumberUserControl = (PhoneNumberUserControl)ctrl.Controls.Find(controlName, true).First();
				var phoneDiallerUserControl = (PhoneDiallerUserControl)phoneNumberUserControl.Controls.Find("PhoneDiallerControl", true).First();

				// act
				phoneDiallerUserControl.CallButton.PerformClick();
				Application.DoEvents();
			}

			// assert
			Assert(candidate.Logs.Any(log => log.Event.SE_Code == AutoEvents.MeetingOccurred.Code));
		}

		[GuiTest]
		public void TestCandidateLogEvents_AlternatePhoneCalled()
		{
			CallOccurredHelper("alternatePhoneControl", false, "Candidate was called on their alternate phone number");
		}

		[GuiTest]
		public void TestCandidateLogEvents_MainPhoneCalled()
		{
			CallOccurredHelper("mobileNumberControl", true, "Candidate was called on their mobile phone number");
		}

		[GuiTest]
		public void CandidateLogEvents_ReferenceContactHelper(bool isCheckBoxChecked, string eventLogMessage)
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = CreateCandidate(factory, "John Smith");
			using (var form = GetFormToBash(out var ctrl))
			{
				form.Show();
				ctrl.SetDataBinding(candidate, string.Empty);
				NavigateToTabPage(ctrl, "peopleAndCommunicationTabPage");
				var checkBox = (CheckBox)ctrl.Controls.Find("permittedToContactReferencesCheckBox", true).First();

				// set initial state, may be true already
				checkBox.Checked = true;

				// act
				checkBox.Checked = false;
				// assert
				Assert(candidate.Logs.Any(log => log.Event.SE_Code == AutoEvents.AuthorisationWithdrawn.Code));

				// act
				checkBox.Checked = true;
				// assert
				Assert(candidate.Logs.Any(log => log.Event.SE_Code == AutoEvents.Authorised.Code));
			}
		}

		[GuiTest]
		public void TestCandidateLogEvents_ReferenceContactApproval()
		{
			CandidateLogEvents_ReferenceContactHelper(true, "Permission to contact references granted");
		}

		[GuiTest]
		public void TestCandidateLogEvents_ReferenceContactRevoked()
		{
			CandidateLogEvents_ReferenceContactHelper(false, "Permission to contact references revoked");
		}

		static ZToolStripButton FindSaveButton(CandidateDetailsControl control)
			=> ((ZPostingButtonsUserControl)control.Controls.Find("postingButtons", true).Single()).SaveButton;

		static GraphicalDisplayControl FindPreviewControl(CandidateDetailsControl control)
			=> (GraphicalDisplayControl)control.Controls.Find("documentPreview", true).Single();

		static ZTextBox GetEmailTextBox(CandidateDetailsControl control) => (ZTextBox)control.Controls.Find("emailTextBox", true).Single();
		static CandidateEConversationMessageListUserControl GetEConversationMessageListControl(CandidateDetailsControl control) => (CandidateEConversationMessageListUserControl)control.Controls.Find("chatboxControl", true).Single();

		static void NavigateToTabPage(CandidateDetailsControl control, string tabPageName)
		{
			var tabControl = control.Controls.Find("tabControl", true).First() as ZTabControl;
			tabControl.SelectedTab = (ZTabPage)tabControl.TabPages[tabPageName];
			Application.DoEvents();
		}

		Form GetFormToBash(out CandidateDetailsControl control) => GetFormToBash<CandidateDetailsControl>(out control);
		Form GetFormToBashForAsync(out CandidateDetailsControlAsnycTest control) => GetFormToBash(out control);

		Form GetFormToBash<T>(out T control) where T : Control, new()
		{
			var form = new ZChildForm();
			form.Controls.Add(control = new T { Dock = DockStyle.Fill });
			form.MinimumSize = control.MinimumSize;
			return form;
		}

		public void TestRefreshEdocs()
		{
			var john = CreateCandidate(Factory, "John Smith", PdfWithDifferentColorsForEachPagePath, "john@me.hoff", false, true);
			Factory.Save();

			using (var form = GetFormToBash<TestCandidateDetailsControl>(out var control))
			{
				form.Show();
				NavigateToTabPage(control, "edocsTabPage");
				control.SetDataBinding(john, string.Empty);

				var c = control.eDocsControl_Exposed.eDocsUserControl;
				Assert("eDocsControl should be visible when there is a Candidate bound" + System.Environment.NewLine + PrintVisibleControlHierarchy(c), c.Visible);

				control.SetDataBinding(null, string.Empty);
				Assert("eDocsControl should not be visible when there is no Candidate bound" + System.Environment.NewLine + PrintVisibleControlHierarchy(c), !c.Visible);
			}
		}

		static string PrintVisibleControlHierarchy(Control c)
		{
			var path = string.Empty;
			while (c != null)
			{
				path = c.Name + $"[{c.Visible}]." + path;
				c = c.Parent;
			}
			return path;
		}

		public void TestRefreshEdocsNoExceptions()
		{
			using (var form = GetFormToBash<TestCandidateDetailsControl>(out var control))
			{
				AssertNoExceptionThrown(() => control.RefreshEdocs_Exposed());
			}
		}

		public void TestEmailTabDisabled()
		{
			var john = CreateCandidate(Factory, "John Smith", email: "john@me.off");
			using (var ctrl = new CandidateDetailsControl())
			{
				var tabPage = ctrl.Controls.Find("peopleAndCommunicationControl", true).First();
				var dummyForBinding = new Candidate(Factory.GetNull<HRJobApplication>()) { IsNull = true };

				AssertEquals(false, tabPage.Enabled);

				ctrl.SetDataBinding(john, string.Empty);
				AssertEquals(true, tabPage.Enabled);

				ctrl.SetDataBinding(null, string.Empty);
				AssertEquals(false, tabPage.Enabled);

				ctrl.SetDataBinding(john, string.Empty);
				AssertEquals(true, tabPage.Enabled);

				ctrl.SetDataBinding(dummyForBinding, string.Empty);
				AssertEquals(false, tabPage.Enabled);
			}
		}

		public void TestDummyBindingIsNull()
		{
			var john = CreateCandidate(Factory, "John Smith", email: "john@me.off");
			using (var ctrl = new CandidateDetailsControl())
			{
				var tasksGrid = ctrl.Controls.Find("tasksGrid", true).First();
				var dummyForBinding = new Candidate(Factory.GetNull<HRJobApplication>()) { IsNull = true };

				AssertEquals(false, tasksGrid.Enabled);

				ctrl.SetDataBinding(john, string.Empty);
				AssertEquals(true, tasksGrid.Enabled);

				ctrl.SetDataBinding(null, string.Empty);
				AssertEquals(false, tasksGrid.Enabled);

				ctrl.SetDataBinding(john, string.Empty);
				AssertEquals(true, tasksGrid.Enabled);

				ctrl.SetDataBinding(dummyForBinding, string.Empty);
				AssertEquals(false, tasksGrid.Enabled);
			}
		}

		public void TestWorkflowItemsOnDummyIsEmpty()
		{
			var dummyForBinding = new Candidate(Factory.GetNull<HRJobApplication>()) { IsNull = true };
			var tasks = dummyForBinding.Application.WorkflowItems.Tasks;

			AssertEquals(0, tasks.Count);
		}

		public void TestStateZDropEditHasCorrectAnchoring()
		{
			using (var ctrl = new CandidateDetailsControl())
			{
				var stateDropEdit = ctrl.Controls.Find("stateDropEdit", true).First();
				AssertNotNull("HA_State ZDropEdit control was not found", stateDropEdit);
				AssertEquals("Anchoring for stateDropEdit was not correct", AnchorStyles.Top | AnchorStyles.Left, stateDropEdit.Anchor);
			}
		}

		public void TestCreateWorkItemButton_EnabledWhenBound()
		{
			TestButtonEnabledWhenBound("createWorkItemButton");
		}

		[UseSnapshotProtection]
		public void TestCreateWorkItemButton_NoTemplateAppliedAndSaved()
		{
			// arrange
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				control.SetDataBinding(borris, string.Empty);
				Application.DoEvents();

				var createWorkItemButton = form.GetControl<ZButton>("createWorkItemButton", true);
				AssertNotNull(createWorkItemButton);
				createWorkItemButton.PerformClick();
				Application.DoEvents();

				// WI form is now shown

				// assert
				var formList = Enumerable
					.Range(0, Application.OpenForms.Count)
					.Select(f => Application.OpenForms[f]);

				var wiForm = formList.SingleOrDefault(f => f.Name == "WorkItemForm");
				AssertNotNull("WI form didn't exist", wiForm);
				Assert("WI form wasn't visible", wiForm.Visible);

				// arrange
				var text = "test summary";
				wiForm.GetControl<ZTextBox>("SummaryTextBox").Text = text;

				// act
				var toolStrip = wiForm.GetControl<ZToolStrip>("toolStrip", true);
				var button = toolStrip.Items.Cast<ZToolStripButton>().First(x => x.Text == "S&ave && Close");
				button.PerformClick();
				Application.DoEvents();

				// WI form closed

				// assert
				Assert("WI form was't closed", !wiForm.Visible);
				Assert("WI wasn't in the database", Factory.ExistsInDatabase(WorkItemSchema.Constants.TableName, new ZQuery().AddToFilter(WorkItemSchema.WKI_Summary, text)));
			}
		}

		[UseSnapshotProtection]
		public void TestCreateWorkItemButton_WorkItemNotSaved()
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(borris, string.Empty);
				Application.DoEvents();

				ClickCreateWorkItemButton(control);

				var formList = Enumerable.Range(0, Application.OpenForms.Count).Select(f => Application.OpenForms[f]);

				var wiForm = formList.SingleOrDefault(f => f.Name == "WorkItemForm");
				AssertNotNull("WI form didn't exist", wiForm);

				wiForm.GetControl<ZTextBox>("SummaryTextBox").Text = "random";

				wiForm.Close();
				Application.DoEvents();

				Assert("WI wasn't in the database", !Factory.ExistsInDatabase(WorkItemSchema.Constants.TableName, new ZQuery().AddToFilter(WorkItemSchema.WKI_Summary, "random")));
			}
		}

		public void ClickCreateWorkItemButton(CandidateDetailsControl control)
		{
			var createWorkItemButton = control.GetControl<ZButton>("createWorkItemButton", true);
			createWorkItemButton.PerformClick();
			Application.DoEvents();
		}

		public void TestCreateWorkItemDropDownBinding_AsExpected()
		{
			var john = CreateCandidate(Factory, "John Smith");
			var witp = new WorkItemTemplateProperties();
			witp.FriendlyName = "hello world";
			john.SelectedTemplate = witp;

			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(john, string.Empty);
				Application.DoEvents();

				var chosenTemplate = control.GetControl<ZDropEdit>("createWorkItemDropDown", true);

				AssertEquals("Should contain the bound objects status", chosenTemplate.Text, "HELLO WORLD");
			}
		}

		public void TestHiringRequestButton_EnabledWhenBound()
		{
			TestButtonEnabledWhenBound("HiringRequestButton");
		}

		public void TestHiringRequestButtonClick()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				var button = control.GetControl<ZButton>("HiringRequestButton", true);
				control.SetDataBinding(borris, string.Empty);
				button.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				AssertEquals("/goto/hiringRequest", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", queryKeyValuePairs["sso_otp"]);
				AssertEquals(borris.Applicant.PK.ToString(), queryKeyValuePairs["applicantPK"]);
				AssertEquals(null, queryKeyValuePairs["jobTitle"]);
			}
		}

		public void TestHiringRequestWithJobRoleButtonClick()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			var role = Factory.NewWithValidTestData<HRJobRole>();
			role.HJ_JobTitle = "Intern";
			var opening = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			opening.HV_HJ_JobRole = role.PK;
			borris.Application.HP_HV = opening.PK;
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				var button = control.GetControl<ZButton>("HiringRequestButton", true);
				control.SetDataBinding(borris, string.Empty);
				button.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				AssertEquals("/goto/hiringRequest", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", queryKeyValuePairs["sso_otp"]);
				AssertEquals(borris.Applicant.PK.ToString(), queryKeyValuePairs["applicantPK"]);
				AssertEquals("Intern", queryKeyValuePairs["jobTitle"]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NotBlank");
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
					pdfWithDifferentColorsForEachPagePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfWithDifferentColorsForEachPage.pdf", "PdfWithDifferentColorsForEachPage.pdf");
				}
				return pdfWithDifferentColorsForEachPagePath;
			}
		}

		string unsupportedFormatDocxPath;
		string UnsupportedFormatDocxPath
		{
			get
			{
				if (string.IsNullOrEmpty(unsupportedFormatDocxPath))
				{
					unsupportedFormatDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.UnsupportedFormat.docx", "UnsupportedFormat.docx");
				}
				return unsupportedFormatDocxPath;
			}
		}

		void TestButtonEnabledWhenBound(string buttonName)
		{
			var borris = CreateCandidate(Factory, "Borris Johnson", PdfWithDifferentColorsForEachPagePath, addApplicationResume: false, addApplicantResume: true);
			Factory.Save();

			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				var button = control.GetControl<ZButton>(buttonName, true);

				Assert("We haven't bound yet", !button.Enabled);

				control.SetDataBinding(borris, string.Empty);
				Assert("Bound object found button is Enabled", button.Enabled);
			}
		}

		public class TestCandidateDetailsControl : CandidateDetailsControl
		{
			public void RefreshEdocs_Exposed() => RefreshEdocs();
			public EdocsSwappableControl eDocsControl_Exposed => eDocsControl;
		}

		class CandidateDetailsControlAsnycTest : CandidateDetailsControl
		{
			public CandidateDetailsControlAsnycTest() : base()
			{ }

			protected override void AttachEdocConverApiResponse(HRJobApplication application, Task<Stream> res, IResumeConverter converter, IeDoc toConvert)
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocumentScanning.Business.Test.TestUtils).Assembly))
				{
					var pdfWithDifferentColorsForEachPageStream = resourceRetriever.GetStream("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfWithDifferentColorsForEachPage.pdf");
					converter.AttachEdocCore(application, Path.ChangeExtension(toConvert.FileName, converter.ConvertedFileExtension), pdfWithDifferentColorsForEachPageStream, RecruiterDataRegistry.Instance.DocTypeCVCode);
				}
			}
		}

		class ResumeConverterForConvertApiException : ResumeConverterForDelayedTest
		{
			protected override async Task<Stream> ConvertToPdfAsyncCore(string filename, Stream input)
			{
				await Task.Delay(delayForConversion).ConfigureAwait(false);
				throw new ConvertApiException(System.Net.HttpStatusCode.BadRequest, "ConvertApiException thrown", "Response");
			}
		}

		class ResumeConverterForNonCriticalException : ResumeConverterForDelayedTest
		{
			protected override async Task<Stream> ConvertToPdfAsyncCore(string filename, Stream input)
			{
				await Task.Delay(delayForConversion).ConfigureAwait(false);
				throw new NotImplementedException("Non Critical Message thrown");
			}
		}

		class ResumeConverterForDelayedTest : IResumeConverter
		{
			readonly ILogger logger;
			protected readonly int delayForConversion;

			public List<Task<Stream>> Conversions { get; } = new List<Task<Stream>>();

			public ResumeConverterForDelayedTest(ILogger logger = null, int delayForConversion = 1000)
			{
				this.logger = logger;
				this.delayForConversion = delayForConversion;
			}

			public void AttachEdocCore(HRJobApplication jobApplication, string filename, Stream input, string docType)
			{
				TestConvertApiResumeConverter.AttachEdocCore(jobApplication, filename, input, docType);
			}

			public bool CanConvert(string ext) => TestConvertApiResumeConverter.CanConvert(ext);

			public void ConvertAvailableResumes(HRJobApplication host)
			{
			}

			public Task<Stream> ConvertToPdfAsync(string inputFileExtension, Stream input)
			{
				var task = ConvertToPdfAsyncCore(inputFileExtension, input);
				Conversions.Add(task);
				return task;
			}

			protected virtual async Task<Stream> ConvertToPdfAsyncCore(string inputFileExtension, Stream input)
			{
				AssertNotContains("A file name was passed in as the file extension", ".", inputFileExtension);

				await Task.Delay(delayForConversion).ConfigureAwait(false);
				return new MemoryStream(Encoding.ASCII.GetBytes($"converted.{inputFileExtension}"));
			}

			public IeDoc GetResumeToConvert(HRJobApplication host) => TestConvertApiResumeConverter.GetResumeToConvert(host);

			ConvertApiResumeConverter TestConvertApiResumeConverter
			{
				get { return testConvertApiResumeConverter ?? (testConvertApiResumeConverter = new ConvertApiResumeConverter(logger)); }
			}
			ConvertApiResumeConverter testConvertApiResumeConverter;

			public string ConvertedFileExtension => "PDF";
		}
	}
}
