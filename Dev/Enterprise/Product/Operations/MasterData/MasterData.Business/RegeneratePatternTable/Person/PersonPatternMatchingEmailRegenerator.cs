using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PersonPatternMatchingEmailRegenerator : PatternMatchingEmailRegenerator<GlbPerson>
	{
		List<OrgContact> contactList;
		List<GlbStaff> staffList;
		List<IHRJobApplicant> applicantList;

		GlbPerson personNeedAdd;
		List<OrgContact> contactNeedAddList;
		List<GlbStaff> staffNeedAddList;
		List<IHRJobApplicant> applicantNeedAddList;
		int addCount;

		readonly List<string> propPersonList = new List<string>() { GlbPersonSchema.Constants.PER_EmailAddress, GlbPersonSchema.Constants.PER_EmailAddress2 };

		public PersonPatternMatchingEmailRegenerator(PatternMatchingRecalculator<GlbPerson> recalculator)
			: base(recalculator)
		{
		}

		ZString GetObjectPropertyValue(BusinessObject bizo, string propertyname)
		{
			var property = bizo.GetType().GetProperty(propertyname);

			if (property == null)
			{
				return ZString.Empty;
			}

			object objValue = property.GetValue(bizo, null);

			return string.IsNullOrEmpty(objValue.ToString()) ? string.Empty : TextStandardizerHelper.StandardizeEmail(objValue.ToString());
		}

		protected override int AddCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			if (personNeedAdd != null)
			{
				foreach (var prop in propPersonList)
				{
					var propValue = GetObjectPropertyValue(bizo, prop);

					if (!propValue.IsEmpty)
					{
						AddCore(factory, propValue, bizo.PK, bizo.TablePrefix, bizo.PK, bizo.PER_RN_NKCountryInternal, prop);
					}
				}
			}

			foreach (var contactNeedAdd in contactNeedAddList)
			{
				var countryCode = contactNeedAdd.Header?.CountryCode ?? ZString.Empty;
				AddCore(factory, TextStandardizerHelper.StandardizeEmail(contactNeedAdd.OC_Email), bizo.PK, contactNeedAdd.TablePrefix, contactNeedAdd.PK, countryCode, nameof(contactNeedAdd.OC_Email));
			}

			foreach (var staffNeedAdd in staffNeedAddList)
			{
				AddCore(factory, TextStandardizerHelper.StandardizeEmail(staffNeedAdd.GS_EmailAddress), bizo.PK, staffNeedAdd.TablePrefix, staffNeedAdd.PK, bizo.PER_RN_NKCountryInternal, nameof(staffNeedAdd.GS_EmailAddress));
			}

			foreach (var applicantNeedAdd in applicantNeedAddList)
			{
				AddCore(factory, TextStandardizerHelper.StandardizeEmail(applicantNeedAdd.HA_EmailAddress), bizo.PK, HRJobApplicantSchema.Constants.Prefix, applicantNeedAdd.PK, bizo.PER_RN_NKCountryInternal, nameof(applicantNeedAdd.HA_EmailAddress));
			}

			return GetActualAddCount();
		}

		protected override void SetUltimateParentPK(PatternMatchingEmail patternEmail, ZGuid pk, ZGuid parentPK)
		{
			patternEmail.PME_PER = pk;

			var contact = contactList?.FirstOrDefault(u => parentPK == u?.PK);
			if (contact != null)
			{
				patternEmail.PME_OH = contact.Header?.PK ?? patternEmail.PME_OH;
			}
		}

		protected override int GetActualAddCount()
		{
			return addCount;
		}

		protected override List<string> TablesPrefixList => new List<string>()
		{
			GlbPersonSchema.Constants.Prefix,
			HRJobApplicantSchema.Constants.Prefix,
			OrgContactSchema.Constants.Prefix,
			GlbStaffSchema.Constants.Prefix,
		};

		protected override int InitializeDataCountCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			bizo.InitData(out var pks, out contactList, out staffList, out applicantList);
			personNeedAdd = null;
			contactNeedAddList = new List<OrgContact>();
			staffNeedAddList = new List<GlbStaff>();
			applicantNeedAddList = new List<IHRJobApplicant>();
			hasUpdatedPerson = false;
			addCount = 0;

			var query = new ZQuery(PatternMatchingEmailSchema.PME_PER, bizo.PK);
			query.AddToFilter(PatternMatchingEmailSchema.PME_ParentTableCode, TablesPrefixList);
			patternMatchingEmailArray = factory.Load<PatternMatchingEmail>(query);

			var matchEmailparentIds = patternMatchingEmailArray.Select(a => a.ParentId).Distinct().ToList();
			needDelete = patternMatchingEmailArray.Where(s => !pks.Contains(s.ParentId)).ToArray();
			needUpdate = patternMatchingEmailArray.Where(s => pks.Contains(s.ParentId)).ToArray();

			if (!matchEmailparentIds.Contains(bizo.PK))
			{
				personNeedAdd = bizo;

				var email1 = bizo.PER_EmailAddress;
				var email2 = bizo.PER_EmailAddress2;

				var email1ToHash = TextStandardizerHelper.StandardizeEmail(email1);
				var email2ToHash = TextStandardizerHelper.StandardizeEmail(email2);

				if (!email1ToHash.IsNullOrEmpty())
				{
					addCount += 1;
				}
				if (!email2ToHash.IsNullOrEmpty())
				{
					addCount += 1;
				}
			}

			foreach (var contact in contactList)
			{
				if (!string.IsNullOrEmpty(TextStandardizerHelper.StandardizeEmail(contact.OC_Email)) && !matchEmailparentIds.Contains(contact.PK))
				{
					contactNeedAddList.Add(contact);
					addCount += 1;
				}
			}

			foreach (var staff in staffList)
			{
				if (!string.IsNullOrEmpty(TextStandardizerHelper.StandardizeEmail(staff.GS_EmailAddress)) && !matchEmailparentIds.Contains(staff.PK))
				{
					staffNeedAddList.Add(staff);
					addCount += 1;
				}
			}

			foreach (var applicant in applicantList)
			{
				if (applicant != null && !string.IsNullOrEmpty(TextStandardizerHelper.StandardizeEmail(applicant.HA_EmailAddress)) && !matchEmailparentIds.Contains(applicant.PK))
				{
					applicantNeedAddList.Add(applicant);
					addCount += 1;
				}
			}

			return addCount + needDelete.Length + needUpdate.Length;
		}

		bool hasUpdatedPerson;

		protected override void UpdateCore(GlbPerson bizo, PatternMatchingEmail patternEmail, BusinessObjectFactory factory)
		{
			if (!hasUpdatedPerson && patternEmail.ParentId == bizo.PK)
			{
				#region Update Person

				var patternMatchingPersonEmails = needUpdate.Where(u => !u.IsDeleted && u.ParentId == bizo.PK).ToList();
				var notNullPropList = new List<string>();

				foreach (var prop in propPersonList)
				{
					if (!GetObjectPropertyValue(bizo, prop).IsEmpty)
					{
						notNullPropList.Add(prop);
					}
				}

				while (patternMatchingPersonEmails.Count > 0 && patternMatchingPersonEmails.Count > notNullPropList.Count)
				{
					var patternMatchingEmail = patternMatchingPersonEmails[0];

					patternMatchingPersonEmails.Remove(patternMatchingEmail);
					patternMatchingEmail.Delete();
				}

				while (patternMatchingPersonEmails.Count > 0 && patternMatchingPersonEmails.Count < notNullPropList.Count)
				{
					var matchingEmail = factory.New<PatternMatchingEmail>();

					matchingEmail.PME_ParentTableCode = bizo.TablePrefix;
					matchingEmail.PME_ParentId = bizo.PK;
					matchingEmail.PME_PER = bizo.PK;
					matchingEmail.PME_IsActive = bizo.PER_IsActive;
					matchingEmail.PME_RN_NKCountryCode = bizo.PER_RN_NKCountryInternal;
					patternMatchingPersonEmails.Add(matchingEmail);
				}

				for (var i = 0; i < notNullPropList.Count; i++)
				{
					var prop = notNullPropList[i];
					var needHashValue = GetObjectPropertyValue(bizo, prop);

					patternMatchingPersonEmails[i].PME_RN_NKCountryCode = bizo.PER_RN_NKCountryInternal;
					patternMatchingPersonEmails[i].HashedValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue);

					patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
					{
						ColumnName = prop,
						Hash = patternMatchingPersonEmails[i].HashedValue,
						OriginalValue = needHashValue,
						PK = bizo.PK.ToGuid(),
						StandardizedValue = needHashValue,
						TableName = patternMatchingPersonEmails[i].TablePrefix
					});
				}

				#endregion

				hasUpdatedPerson = true;
			}
			else
			{
				var contact = contactList?.FirstOrDefault(u => u.PK == patternEmail.PME_ParentId);
				if (contact != null)
				{
					var countryCode = contact?.Header?.CountryCode ?? ZString.Empty;
					UpdateCore(patternEmail, TextStandardizerHelper.StandardizeEmail(contact.OC_Email), countryCode, contact.PK, nameof(contact.OC_ContactName));
					return;
				}

				var staff = staffList?.FirstOrDefault(u => u.PK == patternEmail.PME_ParentId);
				if (staff != null)
				{
					UpdateCore(patternEmail, TextStandardizerHelper.StandardizeEmail(staff.GS_EmailAddress), bizo.PER_RN_NKCountryInternal, staff.PK, nameof(staff.GS_FullName));
					return;
				}

				var applicant = applicantList?.FirstOrDefault(u => u.PK == patternEmail.PME_ParentId);
				if (applicant != null)
				{
					UpdateCore(patternEmail, TextStandardizerHelper.StandardizeEmail(applicant.HA_EmailAddress), bizo.PER_RN_NKCountryInternal, applicant.PK, nameof(applicant.HA_FullName));
				}
			}
		}
	}
}
