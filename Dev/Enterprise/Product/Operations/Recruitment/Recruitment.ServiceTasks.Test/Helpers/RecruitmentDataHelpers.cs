using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	static class RecruitmentDataHelpers
	{
		public const string MiddleManAddressForTest = "example.com";

		public static Candidate CreateCandidate(BusinessObjectFactory factory, string name, string resume = null, string email = null, bool addApplicationResume = false, bool addApplicantResume = false)
		{
			var applicant = CreateApplicant(factory, name, email);

			var application = applicant.Applications.AddNew();
			application.FillWithValidTestData();
			application.HP_HA = applicant.PK;

			var task = application.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "Read Resume";
			task.P9_G4_RequiredCapability = factory.NewWithValidTestData<GlbCapability>().PK;
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;

			var candidate = new Candidate(application);
			if (resume != null)
			{
				if (addApplicationResume)
				{
					AddResume(candidate.Application, resume);
				}

				if (addApplicantResume)
				{
					AddResume(candidate.Applicant, resume);
				}
			}

			return candidate;
		}

		public static HRJobApplicant CreateApplicant(BusinessObjectFactory factory, string name, string email = null)
		{
			var applicant = factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = email ?? CreateDummyEmailFromName(name);
			applicant.HA_FullName = name;

			return applicant;
		}

		public static IeDoc GetMostRecentEDoc(Candidate candidate)
			=> candidate.Application.DocManagerInfo.AllEDocs.Cast<IeDoc>().MaxBy(edoc => edoc.DateAdded);

		public static JobConversation CreateConversation(Candidate candidate, params string[] participantEmails)
		{
			var conversation = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);
			foreach (var p in participantEmails)
			{
				conversation.Participants.AddNewParticipant(p);
			}

			return conversation;
		}

		public static string CreateConversationEmailAddress(Candidate candidate, params string[] participantEmails)
			=> EmailBizoEncoder.GetMailToForBizo(CreateConversation(candidate, participantEmails), MiddleManAddressForTest);

		public static StorageDocsBase AddResume(IDocManagerSupport host, string path)
		{
			var doc = (StorageDocsBase)host.DocManagerInfo.AddFileOrDocument(path, RecruiterDataRegistry.Instance.DocTypeCVCode);
			var hostFactory = ((BusinessObject)host).Factory;

			hostFactory.Saved += SaveOnParent;
			void SaveOnParent(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				host.DocManagerInfo.Save();
				hostFactory.Saved -= SaveOnParent;
			}

			return doc;
		}

		static string CreateDummyEmailFromName(string name) => Regex.Replace(name, "\\W", "") + "@dummy.com";
	}
}
