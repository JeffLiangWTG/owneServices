using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business.Testing
{
	public static class AutomatedRejectionTestHelper
	{
		public static HRJobApplication CreateApplication(BusinessObjectFactory factory, string name, string resume = null, string email = null, bool addApplicationResume = false, bool addApplicantResume = false)
		{
			var applicant = CreateApplicant(factory, name, email);

			var application = applicant.Applications.AddNew();
			application.FillWithValidTestData();
			application.HP_HA = applicant.PK;

			var task = application.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "Read Resume";

			var capability = factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.G4_Code, "COD")).FirstOrDefault();
			if (capability == null)
			{
				capability = factory.New<GlbCapability>();
				capability.G4_Code = "COD";
				capability.G4_Description = "Test Capability";
				capability.G4_IsActive = true;
			}
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;

			if (resume != null)
			{
				if (addApplicationResume)
				{
					_ = AddResume(application, resume);
				}

				if (addApplicantResume)
				{
					_ = AddResume(applicant, resume);
				}
			}

			return application;
		}

		public static HRJobApplicant CreateApplicant(BusinessObjectFactory factory, string name, string email = null)
		{
			var applicant = factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = email ?? CreateDummyEmailFromName(name);
			applicant.HA_FullName = name;

			return applicant;
		}

		public static JobConversation CreateConversation(HRJobApplication application, params string[] participantEmails)
		{
			var conversation = JobConversation.CreateWithoutCheckingForExistingConversation(application, application.Factory);
			foreach (var p in participantEmails)
			{
				conversation.Participants.AddNewParticipant(p);
			}

			return conversation;
		}

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
