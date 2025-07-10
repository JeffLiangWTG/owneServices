using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	static class PersonPatternMatchingExtentions
	{
		public static void InitData(this GlbPerson glbPerson, out List<ZGuid> pks, out List<GlbStaff> glbStaffs, out List<IHRJobApplicant> jobApplicants)
		{
			glbPerson.InitData(out pks, out _, out glbStaffs, out jobApplicants, false);
		}

		public static void InitData(this GlbPerson glbPerson, out List<ZGuid> pks, out List<OrgContact> orgContacts, out List<GlbStaff> glbStaffs, out List<IHRJobApplicant> jobApplicants)
		{
			glbPerson.InitData(out pks, out orgContacts, out glbStaffs, out jobApplicants, true);
		}

		static void InitData(this GlbPerson glbPerson, out List<ZGuid> pks, out List<OrgContact> orgContacts, out List<GlbStaff> glbStaffs, out List<IHRJobApplicant> jobApplicants, bool needContacts)
		{
			orgContacts = new List<OrgContact>();
			glbStaffs = new List<GlbStaff>();
			jobApplicants = new List<IHRJobApplicant>();
			pks = new List<ZGuid>()
			{
				glbPerson.PK
			};

			if (needContacts && glbPerson.ContactCollection != null && glbPerson.ContactCollection.Any())
			{
				foreach (var contact in glbPerson.ContactCollection.Cast<OrgContact>())
				{
					orgContacts.Add(contact);
					pks.Add(contact.PK);
				}
			}

			if (glbPerson.StaffCollection != null && glbPerson.StaffCollection.Any())
			{
				foreach (var staff in glbPerson.StaffCollection)
				{
					glbStaffs.Add(staff);
					pks.Add(staff.PK);
				}
			}

			if (glbPerson.ApplicantCollection != null && glbPerson.ApplicantCollection.Any())
			{
				foreach (var applicant in glbPerson.ApplicantCollection.Cast<IHRJobApplicant>())
				{
					jobApplicants.Add(applicant);
					pks.Add(applicant.PK);
				}
			}
		}
	}
}
