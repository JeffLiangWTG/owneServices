using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing
{
	[TestedType(typeof(Candidate))]
	sealed class CandidateTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new Candidate(Factory, Factory.NewWithValidTestData<HRJobApplication>().PK);

		public void TestCanBeCreatedFromApplication()
		{
			var applicant = Factory.New<HRJobApplicant>();
			var candidate = new Candidate(Factory, applicant.Applications.AddNew().PK);

			AssertNotNull("We should create an application for the applicant when one does not already exist", candidate.Application);
			AssertNull("Before we have parsed the resume we have no details to populate the document with", candidate.Document);
			AssertNull("We can't create a resume without any details, so it's safe to be null", candidate.Resume);
		}

		public void TestEditingApplicantsSetsHasChanges()
		{
			var otherFactory = new BusinessObjectFactory();
			var otherCandidate = RecruitmentDataHelpers.CreateCandidate(otherFactory, "John Smith");

			otherFactory.Save();

			var candidate = new Candidate(Factory, otherCandidate.Application.PK);
			Assert("PRE: We've just saved, so the candidate should not have any changes", !candidate.HasChanges);

			candidate.Applicant.HA_FullName = "Johno Smitty";
			Assert("After editing an Applicant's property the candidate's HasChanges should be true", candidate.HasChanges);
		}

		public void TestEditingApplicationSetsHasChanges()
		{
			var otherFactory = new BusinessObjectFactory();
			var otherCandidate = RecruitmentDataHelpers.CreateCandidate(otherFactory, "John Smith");
			otherCandidate.Application.HP_CurrentStatus = "SUP";

			otherFactory.Save();

			var candidate = new Candidate(Factory, otherCandidate.Application.PK);
			Assert("PRE: We've just saved, so the candidate should not have any changes", !candidate.HasChanges);

			candidate.Application.HP_CurrentStatus = "NUP";
			Assert("After editing an Application's property the candidate's HasChanges should be true", candidate.HasChanges);
		}

		public void TestWillNotOverwriteExistingApplication()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			Factory.Save();

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals("We should use the existing applicant when available", application.PK, candidate.Application.PK);
		}

		public void TestEConversation()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "John Smith");
			Factory.Save();

			AssertEquals(1, candidate.EConversation.Conversations.Count);
			AssertEquals("Candidate created", candidate.EConversation.RootConversation.Messages.First().JCM_Body);
			AssertEquals(candidate.EConversation.RootConversation, candidate.EConversation.Conversations.First());
		}

		public void TestAddingNotesApplicationSetsHasChanges()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "John Smith");
			Factory.Save();

			var groupedEconversation = candidate.EConversation;
			var rootConversation = groupedEconversation.RootConversation;
			AssertEquals("Candidate created", rootConversation.GetTimeOrderedMessages().LastOrDefault().Body);
			Assert("PRE: Before adding a note the HasChanges should be false", !rootConversation.HasChanges);
			Assert("PRE: Before adding a note the HasChanges should be false", !groupedEconversation.HasChanges);
			Assert("PRE: Before adding a note the HasChanges should be false", !candidate.Application.HasChanges);
			Assert("PRE: Before adding a note the HasChanges should be false", !candidate.Applicant.HasChanges);
			Assert("PRE: Before adding a note the HasChanges should be false", !candidate.HasChanges);

			rootConversation.AddMessageFromCurrentUser("Teetst", isInternal: true);

			Assert("After adding a note the HasChanges should be true", rootConversation.HasChanges);
			Assert("After adding a note the HasChanges should be true", groupedEconversation.HasChanges);
			Assert("After adding a note the HasChanges should be true", candidate.Application.HasChanges);
			Assert("After adding a note the HasChanges should be true", candidate.Applicant.HasChanges);
			Assert("After adding a note the HasChanges should be true", candidate.HasChanges);
		}

		public void TestCanFindResume()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var pdfTempFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfWithDifferentColorsForEachPage.pdf");
			var resume = application.DocManagerInfo.AddFileOrDocument(pdfTempFilePath, "PDF");
			resume.DocType = "RES";

			var otherDocument = application.DocManagerInfo.AddFileOrDocument(pdfTempFilePath, "PDF");
			otherDocument.DocType = "1RM";

			var candidate = new Candidate(Factory, application.PK);

			AssertEquals("We should locate the registry ", resume, candidate.Resume);
		}

		public void TestStageBinding_ShouldOnlySequenceWorkFlowTasks()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "HRA";
			template.P0_SubType1 = "AU";
			var templateTask1 = template.WorkflowItems.AddNew();
			templateTask1.P9_Description = "Task 1";
			templateTask1.P9_Sequence = 1;
			var templateTask2 = template.WorkflowItems.AddNew();
			templateTask2.P9_Description = "Task 2";
			templateTask2.P9_Sequence = 2;
			var templateTask3 = template.WorkflowItems.AddNew();
			templateTask3.P9_Description = "Task 3";
			templateTask3.P9_Sequence = 3;
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var triggerTask = application.WorkflowItems.Triggers.AddNew();
			triggerTask.P9_Sequence = 2;
			triggerTask.P9_Description = "TriggerTask 1";

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CLS";
			application.WorkflowItems.Tasks[0].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "ASN";
			application.WorkflowItems.Tasks[3].P9_Status = "ASN";

			AssertEquals("3 - Task 3", candidate.Stage.ToString());
		}

		public void TestStageBinding_JobCompleted_AllPassed()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CLS";
			application.WorkflowItems.Tasks[0].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "CLS";
			application.WorkflowItems.Tasks[2].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[3].P9_Status = "CLS";
			application.WorkflowItems.Tasks[3].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;

			AssertEquals("Job completed", candidate.Stage.ToString());
		}

		public void TestStageBinding_JobCompleted_SkippedLastTaskItem()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CLS";
			application.WorkflowItems.Tasks[0].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "CLS";
			application.WorkflowItems.Tasks[2].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[3].P9_Status = "CAN";

			AssertEquals("Job completed", candidate.Stage.ToString());
		}

		public void TestStageBinding_JobCompleted_SkippedAll()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CAN";
			application.WorkflowItems.Tasks[1].P9_Status = "CAN";
			application.WorkflowItems.Tasks[2].P9_Status = "CAN";
			application.WorkflowItems.Tasks[3].P9_Status = "CAN";

			AssertEquals("Job completed", candidate.Stage.ToString());
		}

		public void TestStageBinding_IncompleteTaskFirst()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CAN";
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "ASN";
			application.WorkflowItems.Tasks[3].P9_Status = "CLS";
			application.WorkflowItems.Tasks[3].P9_Outcome = ProcessTaskView.TaskP9OutcomeFail;

			AssertEquals("3 - Task 3", candidate.Stage.ToString());
		}

		public void TestStageBinding_FailedTaskFirst()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CAN";
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "CLS";
			application.WorkflowItems.Tasks[2].P9_Outcome = ProcessTaskView.TaskP9OutcomeFail;
			application.WorkflowItems.Tasks[3].P9_Status = "ASN";

			AssertEquals("3 - Task 3", candidate.Stage.ToString());
		}

		public void TestStageBinding_NoTask()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "HRA";
			template.P0_SubType1 = "AU";

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			AssertEquals(string.Empty, candidate.Stage.ToString());
		}

		public void TestStageBinding_NoIncompleteAndFailedTask()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CAN";
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "CAN";
			application.WorkflowItems.Tasks[3].P9_Status = "CLS";
			application.WorkflowItems.Tasks[3].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;

			AssertEquals("Job completed", candidate.Stage.ToString());
		}

		public void TestStageBinding_NoWorkflow()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			AssertEquals(string.Empty, candidate.Stage.ToString());
		}

		public void TestStageBinding_OPNStatus()
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
			var templateTask4 = template.WorkflowItems.AddNew();
			templateTask4.P9_Description = "Task 4";
			templateTask4.P9_Sequence = 4;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_RN_NKCountry = "AU";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);

			Factory.Save();

			application.WorkflowItems.Tasks.CreateItemsFromTemplate();

			application.WorkflowItems.Tasks[0].P9_Status = "CAN";
			application.WorkflowItems.Tasks[1].P9_Status = "CLS";
			application.WorkflowItems.Tasks[1].P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			application.WorkflowItems.Tasks[2].P9_Status = "OPN";
			application.WorkflowItems.Tasks[3].P9_Status = "ASN";

			AssertEquals("3 - Task 3", candidate.Stage.ToString());
		}

		public void TestRatingBinding()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var candidate = new Candidate(Factory, application.PK);

			AssertEquals(RatingValues.Unset, candidate.Rating);
			AssertEquals(RatingValues.Unset.ToString(), application.HP_ApplicationOverallRating);

			candidate.Rating = RatingValues.Potential;
			AssertEquals("2", application.HP_ApplicationOverallRating);

			application.HP_ApplicationOverallRating = "1";
			AssertEquals(RatingValues.Suitable, candidate.Rating);

			application.HP_ApplicationOverallRating = "abc";
			AssertEquals(RatingValues.Unset, candidate.Rating);

			application.HP_ApplicationOverallRating = null;
			AssertEquals(RatingValues.Unset, candidate.Rating);
		}

		public void TestRatingControl_ChangeFromGUI()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals(RatingValues.Unset, candidate.Rating);

			candidate.Rating_Suitable = true;
			AssertEquals(RatingValues.Suitable, candidate.Rating);
			AssertEquals(true, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating_Potential = true;
			AssertEquals(RatingValues.Potential, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(true, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating_Unsuitable = true;
			AssertEquals(RatingValues.Unsuitable, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(true, candidate.Rating_Unsuitable);
		}

		public void TestRatingControl_SetSameValueOnGUI()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals(RatingValues.Unset, candidate.Rating);

			candidate.Rating_Suitable = true;
			AssertEquals(RatingValues.Suitable, candidate.Rating);
			AssertEquals(true, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating_Suitable = true;
			AssertEquals(RatingValues.Unset, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating_Potential = true;
			AssertEquals(RatingValues.Potential, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(true, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating_Potential = true;
			AssertEquals(RatingValues.Unset, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating_Unsuitable = true;
			AssertEquals(RatingValues.Unsuitable, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(true, candidate.Rating_Unsuitable);

			candidate.Rating_Unsuitable = true;
			AssertEquals(RatingValues.Unset, candidate.Rating);
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);
		}

		public void TestRatingControl_ChangeFromBizo()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals(RatingValues.Unset, candidate.Rating);

			candidate.Rating = RatingValues.Suitable;
			AssertEquals(true, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Potential;
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(true, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Unsuitable;
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(true, candidate.Rating_Unsuitable);
		}

		public void TestRatingControl_SetSameValueOnBizo()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals(RatingValues.Unset, candidate.Rating);

			candidate.Rating = RatingValues.Suitable;
			AssertEquals(true, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Suitable;
			AssertEquals(true, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Potential;
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(true, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Potential;
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(true, candidate.Rating_Potential);
			AssertEquals(false, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Unsuitable;
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(true, candidate.Rating_Unsuitable);

			candidate.Rating = RatingValues.Unsuitable;
			AssertEquals(false, candidate.Rating_Suitable);
			AssertEquals(false, candidate.Rating_Potential);
			AssertEquals(true, candidate.Rating_Unsuitable);
		}

		public void TestRatingControl_OnCandidateRatingChangedEventRised()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var receivedEvents = new List<ZInt>();

			var candidate = new Candidate(Factory, application.PK);
			candidate.OnCandidateRatingChanged += delegate (object sender, CandidateRatingChangeEventArgs e)
			{
				receivedEvents.Add(e.Rating);
			};

			candidate.Rating = RatingValues.Suitable;
			candidate.Rating = RatingValues.Potential;

			AssertEquals(2, receivedEvents.Count);
			AssertEquals(RatingValues.Suitable, receivedEvents[0]);
			AssertEquals(RatingValues.Potential, receivedEvents[1]);
		}

		public void TestRatingSavesWhenChanged()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var candidate = new Candidate(Factory, application.PK);
			Factory.Save();
			AssertEquals(RatingValues.Unset, candidate.Rating);

			candidate.Rating = RatingValues.Unsuitable;

			var other = new BusinessObjectFactory { RefreshEnabled = false };
			var otherApplication = other.Load<HRJobApplication>(application.PK);
			Assert(int.TryParse(otherApplication.HP_ApplicationOverallRating, out var ratingInDb));
			AssertEquals("Value in DB was different to the saved value", RatingValues.Unsuitable, ratingInDb);
		}

		public void TestRatingSave_ConcurrencyException()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var candidate = new Candidate(Factory, application.PK);
			var borris = candidate;
			borris.Rating = RatingValues.Suitable;

			var other = new BusinessObjectFactory { RefreshEnabled = false };
			var otherApplication = other.Load<HRJobApplication>(borris.Applicant.PK);
			var otherCandidate = new Candidate(other, application.PK) { Rating = RatingValues.Unsuitable };

			AssertEquals(1, borris.Rating);
			AssertEquals(RatingValues.Unsuitable, otherCandidate.Rating);

			borris.Rating = RatingValues.Potential;

			CombineAssertions("Merge conflict should have blocked the save", () =>
			{
				Assert("Save occured. Bound object has no changes.", !borris.HasChanges);
				AssertContains("We should show the user an error message", "While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestStatusBinding()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals(string.Empty, candidate.Status);

			candidate.Status = "INP";
			AssertEquals("INP", application.HP_CurrentStatus);

			application.HP_CurrentStatus = "ST1";
			AssertEquals("ST1", candidate.Status);
		}

		public void TestStatusControl_SetSameValueOnBizo()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			application.HP_CurrentStatus = "DEF";

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals("DEF", candidate.Status);

			candidate.Status = "ST4";
			AssertEquals("ST4", application.HP_CurrentStatus);

			candidate.Status = "ACC";
			AssertEquals("ACC", application.HP_CurrentStatus);
		}

		public void TestCandidateLogEvents_StatusChanged()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");

			// act
			candidate.Status = "ACC";

			// assert
			var log = candidate.EventLogs.First();
			AssertEventLogIsValid(log, HRJobApplicationEvent.StatusChanged, " => ACC");
		}

		public void TestGetPreviousExperienceDetails_NoDocContentsAvailable()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var pdfTempFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfWithDifferentColorsForEachPage.pdf");

			var resume = application.DocManagerInfo.AddFileOrDocument(pdfTempFilePath, "PDF");
			resume.DocType = "RES";

			var candidate = new Candidate(Factory, application.PK);

			AssertEquals(ZString.Empty, candidate.PreviousExperienceDetails);
		}

		public void TestCandidateLogEvents()
		{
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");

			_ = candidate.Application.LogRCEEvent(HRJobApplicationEvent.RatingChanged, "some_info");

			AssertEquals(1, candidate.EventLogs.Count());
			var log = candidate.EventLogs.First();
			AssertEquals(false, log.IsCancelled);
			AssertEquals(AutoEvents.RecruitmentCandidateEvent.Code, log.Event.SE_Code);
			AssertNotNull(log.Parameters);

			Assert(log.Parameters.ContainsKey("EVT"));
			var eventName = log.Parameters["EVT"];
			AssertEquals(nameof(HRJobApplicationEvent.RatingChanged), eventName);
			AssertEquals((HRJobApplicationEvent)Enum.Parse(typeof(HRJobApplicationEvent), eventName), HRJobApplicationEvent.RatingChanged);

			Assert(log.Parameters.ContainsKey("DES"));
			var info = log.Parameters["DES"];
			AssertEquals("some_info", info);
		}

		public void TestCandidateLogEvents_RatingChanged()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");

			// act
			candidate.Rating = RatingValues.Suitable;

			// assert
			var log = candidate.EventLogs.First();
			AssertEventLogIsValid(log, HRJobApplicationEvent.RatingChanged, "Unset => Suitable");
		}

		public void TestCandidateLogEvents_CandidateCreated()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");

			// act
			factory.Save();

			// assert
			Assert(candidate.Logs.Any(log => log.Event.SE_Code == AutoEvents.AddedARecordToTheSystem.Code));
		}

		public void TestCandidateRatingChange_HandleSaveConcurrencyException()
		{
			// arrange
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var candidate = RecruitmentDataHelpers.CreateCandidate(factory1, "John Smith");
			var task1 = (HRJobApplicationProcessTask)candidate.Application.WorkflowItems.AddNew(typeof(HRJobApplicationProcessTask));
			task1.P9_GS_NKAssignedStaffMember = "ABC";
			task1.P9_SystemLastEditUser = "ZZZ";
			factory1.Save();

			var task2 = factory2.Load<HRJobApplicationProcessTask>(task1.PK);
			task2.P9_GS_NKAssignedStaffMember = "DEF";
			task2.P9_SystemLastEditUser = "USR";
			factory2.Save();

			// factory1 now out of sync with DB

			// trigger factory1.save() from rating change
			AssertNoExceptionThrown(() => candidate.Rating = RatingValues.Potential);
		}

		public void TestCandidateRatingChange_NoDuplicateEvents()
		{
			// arrange
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var candidate1 = RecruitmentDataHelpers.CreateCandidate(factory1, "John Smith");
			factory1.Save();

			var app2 = factory2.Load<HRJobApplication>(candidate1.Application.PK);
			var candidate2 = new Candidate(app2);
			factory2.Save();

			candidate1.Rating = RatingValues.Potential;
			candidate2.Rating = RatingValues.Potential;

			var factory3 = new BusinessObjectFactory();
			var app3 = factory3.Load<HRJobApplication>(candidate1.Application.PK);
			var candidate3 = new Candidate(app3);
			var logs = candidate3.EventLogs;
			AssertEquals(1, logs.Count());
		}

		public static void AssertEventLogIsValid(StmALog log, HRJobApplicationEvent expectedEvent, string expectedEventInfo)
		{
			AssertNotNull(log);
			AssertEquals(false, log.IsCancelled);
			AssertNotNull(log.Parameters);
			if (log.Event.SE_Code == "RCE")
			{
				var eventName = log.Parameters["EVT"];
				AssertEquals(expectedEvent.ToString(), eventName);

				var info = log.Parameters["DES"];
				AssertEquals(expectedEventInfo, info);
			}
			else
			{
				var eventName = expectedEvent.ToString();
				AssertEquals(expectedEvent.ToString(), eventName);
				AssertEquals(expectedEventInfo, expectedEventInfo);
			}
		}

		public void TestGetPreviousExperienceDetails()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var doc = application.Documents.AddNew();
			doc.HPD_Content = header + xml;

			var candidate = new Candidate(Factory, application.PK);
			ZString expectedString = string.Format("Consultant - ???{0}Senior Analyst - Accenture Inc.{0}??? - ???{0}??? - Accenture Inc.", System.Environment.NewLine);

			AssertEquals(expectedString, candidate.PreviousExperienceDetails);
		}

		public void TestGetPreviousExperienceDetails_XmlWithNoEmploymentData()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var doc = application.Documents.AddNew();
			doc.HPD_Content = header + xmlWithNoEmploymentData;

			var candidate = new Candidate(Factory, application.PK);

			AssertEquals(ZString.Empty, candidate.PreviousExperienceDetails);
		}

		public void TestGetPreviousExperienceDetails_Exceptions()
		{
			var expectedExceptionMessage = "Error: The previous experience details could not be read.";
			var applicant = Factory.New<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var doc = application.Documents.AddNew();
			doc.HPD_Content = header + xml;

			var candidate1 = new CandidatePreviousExperienceExceptions<XmlException>(Factory, application.PK);
			candidate1.GetPreviousExperienceDetails_Exposed();
			AssertEquals(expectedExceptionMessage, candidate1.PreviousExperienceDetails);
			AssertEquals("Unable to load the xml for this candidate", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var candidate2 = new CandidatePreviousExperienceExceptions<XPathException>(Factory, application.PK);
			candidate2.GetPreviousExperienceDetails_Exposed();
			AssertEquals(expectedExceptionMessage, candidate2.PreviousExperienceDetails);
			AssertEquals("Unable to parse the employment history for this candidate", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var candidate3 = new CandidatePreviousExperienceExceptions<Exception>(Factory, application.PK);
			candidate3.GetPreviousExperienceDetails_Exposed();
			AssertEquals(expectedExceptionMessage, candidate3.PreviousExperienceDetails);
			AssertEquals("Unknown exception while trying to read employment history for this candidate", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPreviousExperienceDetails_EditsArePersistent()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var doc = application.Documents.AddNew();
			doc.HPD_Content = header + xml;
			var candidate = new Candidate(Factory, application.PK);

			ZString expectedString = string.Format("Consultant - ???{0}Senior Analyst - Accenture Inc.{0}??? - ???{0}??? - Accenture Inc.", System.Environment.NewLine);

			AssertEquals(expectedString, candidate.PreviousExperienceDetails);

			candidate.PreviousExperienceDetails = "some previóus experience 😁";
			AssertEquals("some previóus experience 😁", candidate.PreviousExperienceDetails);

			candidate.PreviousExperienceDetails = string.Empty;
			AssertEquals(expectedString, candidate.PreviousExperienceDetails);
		}

		public void TestSourceBinding()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals("MAN", candidate.Source);

			candidate.Source = "WEB";
			AssertEquals("WEB", application.HP_SourceType);

			application.HP_SourceType = "AGT";
			AssertEquals("AGT", candidate.Source);
		}

		public void TestSourceControl_SetSameValueOnBizo()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			application.HP_SourceType = "WEB";

			var candidate = new Candidate(Factory, application.PK);
			AssertEquals("WEB", candidate.Source);

			candidate.Source = "STF";
			AssertEquals("STF", application.HP_SourceType);

			candidate.Source = "AGT";
			AssertEquals("AGT", application.HP_SourceType);
		}

		public void TestCandidateLogEvents_SourceChanged()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");

			// act
			candidate.Source = "AGT";

			// assert
			var log = candidate.EventLogs.First();
			AssertEventLogIsValid(log, HRJobApplicationEvent.SourceChanged, "MAN => AGT");
		}

		[UseSnapshotProtection]
		public void TestPairListFromWorkItemListArray()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");
			AssertEquals(string.Empty, candidate.PairListFromWorkItemList.ElementsAsString);
			var collection = new WorkItemTemplatePropertiesCollection();
			var template = CreateTemplate("test");
			collection.Add(template);

			// act
			using (RecruitmentDataRegistry.Instance.WorkItemTemplateProperties.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				// assert
				AssertEquals("test - template", candidate.PairListFromWorkItemList.ElementsAsString);
			}
		}

		public void TestCanAccessWorkItemsList()
		{
			var properties = new WorkItemTemplateProperties();
			AssertNotNull(properties.WorkflowTemplateTypes);
		}

		[UseSnapshotProtection]
		public void TestSelectedTemplate()
		{
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");
			var collection = new WorkItemTemplatePropertiesCollection();
			var template1 = CreateTemplate("test");
			collection.Add(template1);

			using (RecruitmentDataRegistry.Instance.WorkItemTemplateProperties.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(template1.FriendlyName, candidate.SelectedTemplate.FriendlyName);
			}

			var template2 = CreateTemplate("changed name");
			collection.Add(template2);
			candidate.SelectedTemplate = template2;

			using (RecruitmentDataRegistry.Instance.WorkItemTemplateProperties.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(candidate.SelectedTemplate.FriendlyName, template2.FriendlyName);
			}
		}

		[UseSnapshotProtection]
		public void TestSelectedTemplateDescription()
		{
			var factory = new BusinessObjectFactory();
			var candidate = RecruitmentDataHelpers.CreateCandidate(factory, "John Smith");
			var collection = new WorkItemTemplatePropertiesCollection();

			AssertEquals(string.Empty, candidate.SelectedTemplateDescription);

			var template = CreateTemplate("test");
			collection.Add(template);
			using (RecruitmentDataRegistry.Instance.WorkItemTemplateProperties.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(template.FriendlyName, candidate.SelectedTemplateDescription);
			}

			var template2 = CreateTemplate("changed");
			template2.FriendlyName = new ZString("changeddesc");
			collection.Add(template2);
			candidate.SelectedTemplate = template2;
			AssertEquals(template2.FriendlyName, candidate.SelectedTemplateDescription);
		}

		static WorkItemTemplateProperties CreateTemplate(ZString name)
		{
			var template = new WorkItemTemplateProperties();
			template.FriendlyName = name;
			template.WKI_PK = new BusinessObjectFactory().New<WorkItem>().PK;
			return template;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentScanning.Business.Test.DocumentUtilitiesTest).Assembly));

		const string header = @"<?xml version='1.0' encoding='utf-16'?>";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		const string xml = @"<Resume lang=""en"" src=""DAXTRA-CVX schema:2.0.29 release:0.26.0.60794 rdate:2019-01-28"">
   <StructuredXMLResume>
      <ContactInfo>
         <PersonName id=""169"" oids=""2 6 13 67 102 108 118 169 "">
            <FormattedName>Krisna S. Castor</FormattedName>
            <GivenName>Krisna</GivenName>
            <MiddleName>S.</MiddleName>
            <FamilyName>Castor</FamilyName>
         </PersonName>
         <ContactMethod>
            <Mobile id=""170"" oids=""3 103 170 "">
               <InternationalCountryCode>63</InternationalCountryCode>
               <SubscriberNumber>932 669 6657</SubscriberNumber>
               <FormattedNumber>+63 932 669 6657</FormattedNumber>
            </Mobile>
            <InternetEmailAddress id=""171"" oids=""4 104 171 "" type=""main"">krisnacastor@gmail.com</InternetEmailAddress>
            <PostalAddress id=""5"" type=""main"">
               <CountryCode>PH</CountryCode>
               <Municipality>Makati</Municipality>
            </PostalAddress>
         </ContactMethod>
      </ContactInfo>
      <Objective id=""7"">a BI Developer</Objective>
      <EmploymentHistory>
         <EmployerOrg id=""22"" employerOrgType=""soleEmployer"">
            <EmployerContactInfo>
               <LocationSummary id=""21"">
                  <CountryCode>PH</CountryCode>
               </LocationSummary>
            </EmployerContactInfo>
            <PositionHistory positionType=""PERMANENT"">
               <Title id=""23"">Consultant</Title>
               <StartDate id=""22"">
                  <YearMonth>2017-06</YearMonth>
               </StartDate>
               <EndDate id=""22"">
                  <YearMonth>2018-05</YearMonth>
               </EndDate>
               <JobLevelInfo>
                  <JobGrade>joblevel-nonmanager-low</JobGrade>
               </JobLevelInfo>
               <JobCategory>
                  <TaxonomyName>JOBAREA</TaxonomyName>
                  <CategoryCode>it</CategoryCode>
               </JobCategory>
               <UserArea>
                  <DaxPositionHistoryUserArea>
                     <TLSPAN first_id=""21"" last_id=""23"" border_id=""24"" />
                     <MonthsOfWork>12</MonthsOfWork>
                  </DaxPositionHistoryUserArea>
               </UserArea>
            </PositionHistory>
         </EmployerOrg>
         <EmployerOrg id=""25"" employerOrgType=""soleEmployer"">
            <EmployerOrgName id=""24"">Accenture Inc.</EmployerOrgName>
            <PositionHistory positionType=""PERMANENT"">
               <Title id=""26"">Senior Analyst</Title>
               <OrgName id=""24"">Accenture Inc.</OrgName>
               <StartDate id=""25"">
                  <YearMonth>2013-03</YearMonth>
               </StartDate>
               <EndDate id=""25"">
                  <YearMonth>2017-06</YearMonth>
               </EndDate>
               <JobLevelInfo>
                  <JobGrade>joblevel-nonmanager-mid</JobGrade>
               </JobLevelInfo>
               <JobCategory>
                  <TaxonomyName>JOBAREA</TaxonomyName>
                  <CategoryCode>senior</CategoryCode>
               </JobCategory>
               <UserArea>
                  <DaxPositionHistoryUserArea>
                     <TLSPAN first_id=""24"" last_id=""26"" border_id=""0"" />
                     <MonthsOfWork>52</MonthsOfWork>
                  </DaxPositionHistoryUserArea>
               </UserArea>
            </PositionHistory>
         </EmployerOrg>
         <EmployerOrg id=""34"" employerOrgType=""PROJECT"">
            <PositionHistory positionType=""PROJECT"">
               <Description>2012/2014    This internal NNIT team aims to provide insights into NNIT's services and performance
*    Microsoft Certified Professional    through deep analytical reports. Primarily, the team is responsible for maintaining and
*    Bachelor of Science in Electronics    enhancing an internal site called BIC, which stores reports, KPIs and analytics used by
    Engineering - Ateneo de Naga    the upper management, customers and all of NNIT employees.
    University</Description>
               <StartDate id=""34"">
                  <YearMonth>2018-06</YearMonth>
               </StartDate>
               <EndDate id=""34"">notApplicable</EndDate>
               <JobCategory>
                  <TaxonomyName>JOBAREA</TaxonomyName>
                  <CategoryCode>engineering</CategoryCode>
               </JobCategory>
               <UserArea>
                  <DaxPositionHistoryUserArea>
                     <Umbrella>19</Umbrella>
                     <TLSPAN first_id=""32"" last_id=""40"" border_id=""41"" />
                     <MonthsOfWork>5</MonthsOfWork>
                  </DaxPositionHistoryUserArea>
               </UserArea>
            </PositionHistory>
         </EmployerOrg>
         <EmployerOrg id=""50"" employerOrgType=""PROJECT"">
            <EmployerOrgName id=""24"">Accenture Inc.</EmployerOrgName>
            <PositionHistory positionType=""PROJECT"">
               <OrgName id=""49"">Microsoft</OrgName>
               <Description>SQL Server 2012/2014    BMGF is said to be the largest private foundation in the United States and one of the
*    70-461 - Querying Microsoft SQL    largest in the world. It is among the long-term clients of Accenture-Avanade. The EDW
    Server 2012/2014    team is part of the BMGF project that supports several Financial applications including
*    Azure Summit 2018 - Philippines    its Enterprise Data Warehouse, and its reporting and analysis services.
*    Business Intelligence Boot Camp</Description>
               <StartDate id=""50"">
                  <YearMonth>2015-10</YearMonth>
               </StartDate>
               <EndDate id=""50"">
                  <YearMonth>2017-06</YearMonth>
               </EndDate>
               <JobCategory>
                  <TaxonomyName>JOBAREA</TaxonomyName>
                  <CategoryCode>it</CategoryCode>
               </JobCategory>
               <UserArea>
                  <DaxPositionHistoryUserArea>
                     <Umbrella>25</Umbrella>
                     <TLSPAN first_id=""49"" last_id=""61"" border_id=""62"" />
                     <MonthsOfWork>21</MonthsOfWork>
                  </DaxPositionHistoryUserArea>
               </UserArea>
            </PositionHistory>
         </EmployerOrg>
         
      </EmploymentHistory>
      
      <RevisionDate id=""1"">2018-10-15</RevisionDate>
   </StructuredXMLResume>
   <UserArea>
      <DaxResumeUserArea>
         <AdditionalPersonalData>
            <ExperienceSummary>
               <TotalMonthsOfWorkExperience>69</TotalMonthsOfWorkExperience>
               <TotalYearsOfWorkExperience>6</TotalYearsOfWorkExperience>
               <ExecutiveBrief>Krisna S. Castor is a resident of Makati, PH. This candidate has been working in the IT occupational sector for more than 6 years. Currently this candidate is employed as an Advanced Developer. Krisna has extensive knowledge of SQL, Data Warehousing, Windows Server, Scrum Methodology &gt; Sprint Planning, Microsoft SQL Server, ETL, Data Warehousing &gt; EDW, Stored Procedures, Microsoft C-SHARP, Visual Basic, SQL Server Reporting Services, Microsoft PowerPivot, SQL Server Analysis Services, Scrum Methodology, Microsoft .NET Technology, MS Excel VBA, Waterfall Methodology. So far Krisna has not gained any managerial experience.</ExecutiveBrief>
            </ExperienceSummary>
         </AdditionalPersonalData>
         <ParserInfo>
            <ParserConfiguration>{max_len=50000} {tel_flag=} {send_zip=} {fast_conv=} {DEF_LOCAL=} {sdate=0} {no_email_body=0} {do_clever_zoning=0} {keep_zone_span=0} {keep_span=1} {complex=0} {accept_langs=} {not_accept_langs=} {prefer_lang_cv=} {pers_only=0} {projects_off=0} {tree_search_on=0} {all_skills=0} {turbo=0} {split_language=0} {picture=0} {picture_inline=0} {debug=0} {ocr_allowed=0} {name_space=0} {charset=} {hrxml_upgrade_edu_hist=0}{hrxml_add_languages_section=1}{spool=} {docID=} {user=wisetech_global}</ParserConfiguration>
            <ParserRelease>0.26.0.60794</ParserRelease>
            <ParserReleaseDate>2019-01-28</ParserReleaseDate>
            <ParserSchema>2.0.29</ParserSchema>
            <ParserSchemaLocation>http://cvxdemo.daxtra.com/cvx/cvx_schema/candidate/2.0.29/Resume.xsd</ParserSchemaLocation>
            <ConverterRelease>0.19.0.60794</ConverterRelease>
            <ConverterReleaseDate>2019-01-28</ConverterReleaseDate>
         </ParserInfo>
         <FileStruct filename=""/tmp/soap_154369941071735"">
            <attachment fname=""00748536962986144"" ftype=""pdf"" conv=""yes"" doc_type=""cv"" lang=""EN"">/tmp/soap_154369941071735</attachment>
         </FileStruct>
      </DaxResumeUserArea>
   </UserArea>
</Resume>";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		const string xmlWithNoEmploymentData = @"<Resume lang=""en"" src=""DAXTRA-CVX schema:2.0.29 release:0.26.0.60794 rdate:2019-01-28"">
   <StructuredXMLResume>
      <ContactInfo>
         <PersonName id=""169"" oids=""2 6 13 67 102 108 118 169 "">
            <FormattedName>Krisna S. Castor</FormattedName>
            <GivenName>Krisna</GivenName>
            <MiddleName>S.</MiddleName>
            <FamilyName>Castor</FamilyName>
         </PersonName>
         <ContactMethod>
            <Mobile id=""170"" oids=""3 103 170 "">
               <InternationalCountryCode>63</InternationalCountryCode>
               <SubscriberNumber>932 669 6657</SubscriberNumber>
               <FormattedNumber>+63 932 669 6657</FormattedNumber>
            </Mobile>
            <InternetEmailAddress id=""171"" oids=""4 104 171 "" type=""main"">krisnacastor@gmail.com</InternetEmailAddress>
            <PostalAddress id=""5"" type=""main"">
               <CountryCode>PH</CountryCode>
               <Municipality>Makati</Municipality>
            </PostalAddress>
         </ContactMethod>
      </ContactInfo>
      <Objective id=""7"">a BI Developer</Objective>
      <RevisionDate id=""1"">2018-10-15</RevisionDate>
   </StructuredXMLResume>
   <UserArea>
      <DaxResumeUserArea>
         <AdditionalPersonalData>
            <ExperienceSummary>
               <TotalMonthsOfWorkExperience>69</TotalMonthsOfWorkExperience>
               <TotalYearsOfWorkExperience>6</TotalYearsOfWorkExperience>
               <ExecutiveBrief>Krisna S. Castor is a resident of Makati, PH. This candidate has been working in the IT occupational sector for more than 6 years. Currently this candidate is employed as an Advanced Developer. Krisna has extensive knowledge of SQL, Data Warehousing, Windows Server, Scrum Methodology &gt; Sprint Planning, Microsoft SQL Server, ETL, Data Warehousing &gt; EDW, Stored Procedures, Microsoft C-SHARP, Visual Basic, SQL Server Reporting Services, Microsoft PowerPivot, SQL Server Analysis Services, Scrum Methodology, Microsoft .NET Technology, MS Excel VBA, Waterfall Methodology. So far Krisna has not gained any managerial experience.</ExecutiveBrief>
            </ExperienceSummary>
         </AdditionalPersonalData>
         <ParserInfo>
            <ParserConfiguration>{max_len=50000} {tel_flag=} {send_zip=} {fast_conv=} {DEF_LOCAL=} {sdate=0} {no_email_body=0} {do_clever_zoning=0} {keep_zone_span=0} {keep_span=1} {complex=0} {accept_langs=} {not_accept_langs=} {prefer_lang_cv=} {pers_only=0} {projects_off=0} {tree_search_on=0} {all_skills=0} {turbo=0} {split_language=0} {picture=0} {picture_inline=0} {debug=0} {ocr_allowed=0} {name_space=0} {charset=} {hrxml_upgrade_edu_hist=0}{hrxml_add_languages_section=1}{spool=} {docID=} {user=wisetech_global}</ParserConfiguration>
            <ParserRelease>0.26.0.60794</ParserRelease>
            <ParserReleaseDate>2019-01-28</ParserReleaseDate>
            <ParserSchema>2.0.29</ParserSchema>
            <ParserSchemaLocation>http://cvxdemo.daxtra.com/cvx/cvx_schema/candidate/2.0.29/Resume.xsd</ParserSchemaLocation>
            <ConverterRelease>0.19.0.60794</ConverterRelease>
            <ConverterReleaseDate>2019-01-28</ConverterReleaseDate>
         </ParserInfo>
         <FileStruct filename=""/tmp/soap_154369941071735"">
            <attachment fname=""00748536962986144"" ftype=""pdf"" conv=""yes"" doc_type=""cv"" lang=""EN"">/tmp/soap_154369941071735</attachment>
         </FileStruct>
      </DaxResumeUserArea>
   </UserArea>
</Resume>";
	}
}
