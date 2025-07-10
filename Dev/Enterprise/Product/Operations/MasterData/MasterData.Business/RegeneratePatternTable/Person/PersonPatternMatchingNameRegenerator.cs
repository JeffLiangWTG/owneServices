using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PersonPatternMatchingNameRegenerator : PatternMatchingNameRegenerator<GlbPerson>
	{
		List<OrgContact> contactList;
		List<GlbStaff> staffList;

		GlbPerson personNeedAdd;
		List<OrgContact> contactNeedAddList;
		List<GlbStaff> staffNeedAddList;
		int addCount;

		public PersonPatternMatchingNameRegenerator(PatternMatchingRecalculator<GlbPerson> recalculator)
			: base(recalculator)
		{
		}

		protected override List<string> TablesPrefixList => new List<string>()
		{
			GlbPersonSchema.Constants.Prefix,
			HRJobApplicantSchema.Constants.Prefix,
			OrgContactSchema.Constants.Prefix,
			GlbStaffSchema.Constants.Prefix,
		};

		protected override int AddCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			if (personNeedAdd != null)
			{
				AddCore(factory, personNeedAdd.PER_FullName, bizo, personNeedAdd.TablePrefix, personNeedAdd.PK, nameof(personNeedAdd.PER_FullName));
			}

			foreach (var contactNeedAdd in contactNeedAddList)
			{
				AddCore(factory, contactNeedAdd.OC_ContactName, bizo, contactNeedAdd.TablePrefix, contactNeedAdd.PK, nameof(contactNeedAdd.OC_ContactName));
			}

			foreach (var staffNeedAdd in staffNeedAddList)
			{
				AddCore(factory, staffNeedAdd.GS_FullName, bizo, staffNeedAdd.TablePrefix, staffNeedAdd.PK, nameof(staffNeedAdd.GS_FullName));
			}

			return GetActualAddCount();
		}

		void AddCore(BusinessObjectFactory factory, ZString name, GlbPerson person, ZString tableCode, ZGuid parentPk, ZString debuggerName)
		{
			var nameToHash = name.Trim().Contains(' ') ? TextStandardizerHelper.StandardizePersonName(name) : string.Empty;

			if (!string.IsNullOrEmpty(nameToHash))
			{
				var matchingName = factory.New<PatternMatchingName>();
				matchingName.PMN_ParentTableCode = tableCode;
				matchingName.PMN_ParentId = parentPk;
				matchingName.PMN_PER = person.PK;
				matchingName.PMN_RN_NKCountryCode = person.PER_RN_NKCountryInternal;
				matchingName.PMN_IsActive = person.PER_IsActive;
				matchingName.HashedValue = TextStandardizerHelper.ComputeStringHashFast(nameToHash);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = debuggerName,
					Hash = matchingName.HashedValue,
					OriginalValue = name,
					PK = parentPk.ToGuid(),
					StandardizedValue = nameToHash,
					TableName = tableCode
				});
			}

			ReportProgress();
		}

		protected override void SetUltimateParentPK(PatternMatchingName patternName, ZGuid pk)
		{
			patternName.PMN_PER = pk;
		}

		protected override int GetActualAddCount()
		{
			return addCount;
		}

		protected override int InitializeDataCountCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			bizo.InitData(out var pks, out contactList, out staffList, out _);
			personNeedAdd = null;
			contactNeedAddList = new List<OrgContact>();
			staffNeedAddList = new List<GlbStaff>();
			addCount = 0;

			var query = new ZQuery(PatternMatchingNameSchema.PMN_PER, bizo.PK);
			query.AddToFilter(PatternMatchingNameSchema.PMN_ParentTableCode, TablesPrefixList);
			patternMatchingNameArray = factory.Load<PatternMatchingName>(query);

			needDelete = patternMatchingNameArray.Where(s => !pks.Contains(s.ParentId)).ToArray();
			needUpdate = patternMatchingNameArray.Where(s => pks.Contains(s.ParentId)).ToArray();

			var matchNameparentIds = patternMatchingNameArray.Select(a => a.ParentId).ToList();

			if (!bizo.PER_FullName.IsEmpty && !matchNameparentIds.Contains(bizo.PK) && !PersonNameComparator.IsDummyContact(bizo.PER_FullName))
			{
				personNeedAdd = bizo;
				addCount += 1;
			}

			foreach (var contact in contactList)
			{
				if (!contact.OC_ContactName.IsEmpty && !matchNameparentIds.Contains(contact.PK) && !PersonNameComparator.IsDummyContact(contact.OC_ContactName))
				{
					contactNeedAddList.Add(contact);
					addCount += 1;
				}
			}

			foreach (var staff in staffList)
			{
				if (!staff.GS_FullName.IsEmpty && !matchNameparentIds.Contains(staff.PK))
				{
					staffNeedAddList.Add(staff);
					addCount += 1;
				}
			}

			return addCount + needDelete.Length + needUpdate.Length;
		}

		protected override void UpdateCore(GlbPerson bizo, PatternMatchingName patternName)
		{
			if (patternName.PMN_ParentId == bizo.PK)
			{
				UpdateCore(patternName, bizo.PER_FullName, bizo.PK, bizo.TablePrefix, bizo.PK, bizo.PER_RN_NKCountryInternal, nameof(bizo.PER_FullName), bizo.PER_IsActive);
			}
			else
			{
				var contact = contactList?.FirstOrDefault(u => u.PK == patternName.PMN_ParentId);
				if (contact != null)
				{
					UpdateCore(patternName, contact.OC_ContactName, bizo.PK, contact.TablePrefix, contact.PK, bizo.PER_RN_NKCountryInternal, nameof(contact.OC_ContactName), bizo.PER_IsActive);
					return;
				}

				var staff = staffList?.FirstOrDefault(u => u.PK == patternName.PMN_ParentId);
				if (staff != null)
				{
					UpdateCore(patternName, staff.GS_FullName, bizo.PK, staff.TablePrefix, staff.PK, bizo.PER_RN_NKCountryInternal, nameof(staff.GS_FullName), bizo.PER_IsActive);
					return;
				}
			}
		}

		void UpdateCore(PatternMatchingName patternName, ZString name, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZString countryCode, ZString debuggerName, ZBool isActive)
		{
			ZString nameToHash = name.Trim().Contains(' ') ? TextStandardizerHelper.StandardizePersonName(name) : string.Empty;

			if (!nameToHash.IsEmpty)
			{
				patternName.PMN_IsActive = isActive;
				patternName.PMN_RN_NKCountryCode = countryCode;
				patternName.PMN_PER = superParentPk;
				patternName.HashedValue = TextStandardizerHelper.ComputeStringHashFast(nameToHash);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = debuggerName,
					Hash = patternName.HashedValue,
					OriginalValue = name,
					PK = parentPk.ToGuid(),
					StandardizedValue = nameToHash,
					TableName = tableCode
				});
			}
			else
			{
				patternName.Delete();
			}

			ReportProgress();
		}
	}
}
