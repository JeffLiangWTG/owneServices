using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PersonPatternMatchingPhoneRegenerator : PatternMatchingPhoneRegenerator<GlbPerson>
	{
		List<OrgContact> contactList;
		List<GlbStaff> staffList;
		List<IHRJobApplicant> applicantList;

		GlbPerson personNeedAdd;
		List<OrgContact> contactNeedAddList;
		List<GlbStaff> staffNeedAddList;
		List<IHRJobApplicant> applicantNeedAddList;

		readonly List<string> propPersonList = new List<string> { GlbPersonSchema.Constants.PER_HomePhone, GlbPersonSchema.Constants.PER_FaxNumber, GlbPersonSchema.Constants.PER_MobilePhone, GlbPersonSchema.Constants.PER_MobilePhone2 };

		readonly List<string> propContactList = new List<string> { OrgContactSchema.Constants.OC_Phone, OrgContactSchema.Constants.OC_HomePhone, OrgContactSchema.Constants.OC_Mobile, OrgContactSchema.Constants.OC_OtherPhone, OrgContactSchema.Constants.OC_Fax };

		readonly List<string> propStaffList = new List<string> { GlbStaffSchema.Constants.GS_FaxNum, GlbStaffSchema.Constants.GS_HomePhone, GlbStaffSchema.Constants.GS_MobilePhone, GlbStaffSchema.Constants.GS_WorkPhone };

		readonly List<string> propApplicantList = new List<string> { HRJobApplicantSchema.Constants.HA_WorkPhone };

		protected override List<string> TablesPrefixList => new List<string>()
		{
			GlbPersonSchema.Constants.Prefix,
			HRJobApplicantSchema.Constants.Prefix,
			OrgContactSchema.Constants.Prefix,
			GlbStaffSchema.Constants.Prefix,
		};

		public PersonPatternMatchingPhoneRegenerator(PatternMatchingRecalculator<GlbPerson> recalculator)
			: base(recalculator)
		{
		}

		#region Create New Record

		int CreateNewMatchingPhoneFromPerson(GlbPerson person, BusinessObjectFactory factory)
		{
			var addCount = 0;

			if (personNeedAdd != null)
			{
				addCount = AddCore(propPersonList, person, factory, person.PK, person.PER_RN_NKCountryInternal, person.PER_IsActive);
			}

			return addCount;
		}

		int CreateNewMatchingPhoneFromStaff(GlbPerson person, BusinessObjectFactory factory)
		{
			var addCount = 0;

			foreach (var staffNeedAdd in staffNeedAddList)
			{
				addCount = AddCore(propStaffList, staffNeedAdd, factory, person.PK, person.PER_RN_NKCountryInternal, person.PER_IsActive);
			}

			return addCount;
		}

		int CreateNewMatchingPhoneFromContact(GlbPerson person, BusinessObjectFactory factory)
		{
			var addCount = 0;

			foreach (var contactNeedAdd in contactNeedAddList)
			{
				var countryCode = contactNeedAdd?.Header?.CountryCode ?? ZString.Empty;
				addCount = AddCore(propContactList, contactNeedAdd, factory, person.PK, countryCode, person.PER_IsActive);
			}

			return addCount;
		}

		int CreateNewMatchingPhoneFromApplicant(GlbPerson person, BusinessObjectFactory factory)
		{
			var addCount = 0;

			foreach (var applicantNeedAdd in applicantNeedAddList)
			{
				var applicantBizO = applicantNeedAdd as BusinessObject;
				if (applicantBizO != null)
				{
					addCount = AddCore(propApplicantList, applicantBizO, factory, person.PK, person.PER_RN_NKCountryInternal, person.PER_IsActive);
				}
			}

			return addCount;
		}

		#endregion

		#region Update Exist Record

		void UpdateMatchingPhoneFromPerson(GlbPerson person, BusinessObjectFactory factory, ZGuid[] matchPhoneParentIds, ref int counter)
		{
			if (matchPhoneParentIds.Contains(person.PK))
			{
				UpdateCore(propPersonList, person, factory, person.PK, person.PER_RN_NKCountryInternal, person.PER_IsActive);
				counter++;
			}
		}

		void UpdateMatchingPhoneFromStaff(GlbPerson person, BusinessObjectFactory factory, ZGuid[] matchPhoneParentIds, ref int counter)
		{
			if (staffList != null)
			{
				foreach (var staff in staffList)
				{
					if (matchPhoneParentIds.Contains(staff.PK))
					{
						UpdateCore(propStaffList, staff, factory, person.PK, person.PER_RN_NKCountryInternal, person.PER_IsActive);
						counter++;
					}
				}
			}
		}

		void UpdateMatchingPhoneFromContact(GlbPerson person, BusinessObjectFactory factory, ZGuid[] matchPhoneParentIds, ref int counter)
		{
			if (contactList != null)
			{
				foreach (var contact in contactList)
				{
					if (matchPhoneParentIds.Contains(contact.PK))
					{
						var countryCode = contact.Header?.CountryCode ?? ZString.Empty;
						UpdateCore(propContactList, contact, factory, person.PK, countryCode, person.PER_IsActive);
						counter++;
					}
				}
			}
		}

		void UpdateMatchingPhoneFromApplicant(GlbPerson person, BusinessObjectFactory factory, ZGuid[] matchPhoneParentIds, ref int counter)
		{
			if (applicantList != null)
			{
				foreach (var applicant in applicantList)
				{
					var applicantBizO = applicant as BusinessObject;

					if (applicantBizO != null && matchPhoneParentIds.Contains(applicantBizO.PK))
					{
						UpdateCore(propApplicantList, applicantBizO, factory, person.PK, person.PER_RN_NKCountryInternal, person.PER_IsActive);
						counter++;
					}
				}
			}
		}

		#endregion

		protected override int AddCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			var addCount = 0;

			addCount += CreateNewMatchingPhoneFromPerson(bizo, factory);

			addCount += CreateNewMatchingPhoneFromStaff(bizo, factory);

			addCount += CreateNewMatchingPhoneFromContact(bizo, factory);

			addCount += CreateNewMatchingPhoneFromApplicant(bizo, factory);

			return addCount;
		}

		protected override int GetActualAddCount()
		{
			return 0;
		}

		protected override int InitializeDataCountCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			bizo.InitData(out var pks, out contactList, out staffList, out applicantList);
			personNeedAdd = null;
			contactNeedAddList = new List<OrgContact>();
			staffNeedAddList = new List<GlbStaff>();
			applicantNeedAddList = new List<IHRJobApplicant>();

			var query = new ZQuery(PatternMatchingPhoneSchema.PMP_PER, bizo.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, TablesPrefixList);
			allPatternMatchingPhoneArray = factory.Load<PatternMatchingPhone>(query);

			var matchPhoneParentIds = allPatternMatchingPhoneArray.Select(a => a.ParentId).Distinct().ToArray();
			var addCount = 0;
			var updateCount = matchPhoneParentIds.Count(u => pks.Contains(u));

			personNeedAdd = InitializeAddInfo(propPersonList, matchPhoneParentIds, bizo, ref addCount);
			InitializeAddInfo(contactNeedAddList, propContactList, matchPhoneParentIds, contactList, ref addCount);
			InitializeAddInfo(staffNeedAddList, propStaffList, matchPhoneParentIds, staffList, ref addCount);

			foreach (var applicant in applicantList)
			{
				var applicantNeedAdd = InitializeAddInfo(propApplicantList, matchPhoneParentIds, applicant as BusinessObject, ref addCount) as IHRJobApplicant;
				if (applicantNeedAdd != null)
				{
					applicantNeedAddList.Add(applicantNeedAdd);
				}
			}

			needDelete = allPatternMatchingPhoneArray.Where(s => !pks.Contains(s.ParentId)).ToArray();

			return addCount + needDelete.Length + updateCount;
		}

		T InitializeAddInfo<T>(List<string> propList, ZGuid[] matchPhoneParentIds, T bizO, ref int addCount) where T : BusinessObject
		{
			T bizONeedAdd;

			if (bizO != null && !matchPhoneParentIds.Contains(bizO.PK))
			{
				bizONeedAdd = bizO;

				foreach (var prop in propList)
				{
					var propValue = GetObjectPropertyValue(bizO, prop);

					if (!propValue.IsEmpty && !TextStandardizerHelper.IsPlaceholderPhone(propValue))
					{
						addCount += 1;
					}
				}
			}
			else
			{
				bizONeedAdd = null;
			}

			return bizONeedAdd;
		}

		void InitializeAddInfo<T>(List<T> needAddList, List<string> propList, ZGuid[] matchPhoneParentIds, List<T> bizOList, ref int addCount) where T : BusinessObject
		{
			foreach (var bizO in bizOList)
			{
				var addInfo = InitializeAddInfo(propList, matchPhoneParentIds, bizO, ref addCount);
				if (addInfo != null)
				{
					needAddList.Add(addInfo);
				}
			}
		}

		protected override int UpdateCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			var matchPhoneParentIds = allPatternMatchingPhoneArray.Where(a => !a.IsDeleted).Select(a => a.ParentId).Distinct().ToArray();
			var counter = 0;

			UpdateMatchingPhoneFromPerson(bizo, factory, matchPhoneParentIds, ref counter);

			UpdateMatchingPhoneFromStaff(bizo, factory, matchPhoneParentIds, ref counter);

			UpdateMatchingPhoneFromContact(bizo, factory, matchPhoneParentIds, ref counter);

			UpdateMatchingPhoneFromApplicant(bizo, factory, matchPhoneParentIds, ref counter);

			return counter;
		}

		protected override void SetUltimateParentPK(PatternMatchingPhone patternPhone, ZGuid pk, ZGuid parentPK)
		{
			patternPhone.PMP_PER = pk;

			var contact = contactList?.FirstOrDefault(u => u.PK == parentPK);
			if (contact != null)
			{
				patternPhone.PMP_OH = contact.Header?.PK ?? patternPhone.PMP_OH;
			}
		}
	}
}
