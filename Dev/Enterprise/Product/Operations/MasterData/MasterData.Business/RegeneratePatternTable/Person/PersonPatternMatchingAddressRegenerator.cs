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
	public class PersonPatternMatchingAddressRegenerator : PatternMatchingAddressRegenerator<GlbPerson>
	{
		List<GlbStaff> staffList;

		GlbPerson personNeedAdd;
		List<GlbStaff> staffNeedAddList;
		int addCount;

		public PersonPatternMatchingAddressRegenerator(PatternMatchingRecalculator<GlbPerson> recalculator)
			: base(recalculator)
		{
		}

		protected override int AddCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			if (personNeedAdd != null)
			{
				AddCore(factory, bizo.PK, personNeedAdd.TablePrefix, personNeedAdd.PK, bizo.PER_RN_NKCountryInternal, GetValueToUpper(personNeedAdd), personNeedAdd.Address1, personNeedAdd.Address2);
			}

			foreach (var staffNeedAdd in staffNeedAddList)
			{
				AddCore(factory, bizo.PK, staffNeedAdd.TablePrefix, staffNeedAdd.PK, bizo.PER_RN_NKCountryInternal, GetValueToUpper(staffNeedAdd), staffNeedAdd.Address1, staffNeedAdd.Address2);
			}

			return GetActualAddCount();
		}

		protected override void SetUltimateParentPK(PatternMatchingAddress patternAddress, ZGuid pk)
		{
			patternAddress.PMA_PER = pk;
		}

		protected override int InitializeDataCountCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			bizo.InitData(out var pks, out staffList, out _);
			personNeedAdd = null;
			staffNeedAddList = new List<GlbStaff>();
			addCount = 0;

			var query = new ZQuery(PatternMatchingAddressSchema.PMA_PER, bizo.PK);
			query.AddToFilter(PatternMatchingAddressSchema.PMA_ParentTableCode, TablesPrefixList);
			allPatternMatchingAddressArray = factory.Load<PatternMatchingAddress>(query);

			var matchAddressParentIds = allPatternMatchingAddressArray.Select(a => a.ParentId).ToList();
			patternMatchingAddressesToDelete = allPatternMatchingAddressArray.Where(s => !pks.Contains(s.ParentId)).ToArray();
			patternMatchingAddressesToUpdate = allPatternMatchingAddressArray.Where(s => pks.Contains(s.ParentId)).ToArray();

			if (ShouldAdd(bizo, matchAddressParentIds))
			{
				personNeedAdd = bizo;
				addCount += 1;
			}

			foreach (var staff in staffList)
			{
				if (ShouldAdd(staff, matchAddressParentIds))
				{
					staffNeedAddList.Add(staff);
					addCount += 1;
				}
			}

			return addCount + patternMatchingAddressesToDelete.Length + patternMatchingAddressesToUpdate.Length;
		}

		bool ShouldAdd(ISupportWebAddressValidation bizo, List<ZGuid> matchAddressParentIds)
		{
			return bizo != null
				&& (!TextStandardizerHelper.IsPlaceholderAddress(bizo.Address1) || (bizo.Address1.IsEmpty && !TextStandardizerHelper.IsPlaceholderAddress(bizo.Address2)))
				&& !string.IsNullOrEmpty(GetValueToUpper(bizo))
				&& !matchAddressParentIds.Contains(bizo.EntityPK);
		}

		protected override int GetActualAddCount()
		{
			return addCount;
		}

		protected override List<string> TablesPrefixList => new List<string>()
		{
			GlbPersonSchema.Constants.Prefix,
			HRJobApplicantSchema.Constants.Prefix,
			GlbStaffSchema.Constants.Prefix,
		};

		protected override void UpdateCore(GlbPerson bizo, PatternMatchingAddress patternAddress)
		{
			if (patternAddress.PMA_ParentId == bizo.PK)
			{
				UpdateCore(patternAddress, bizo.PK, bizo.Address1, bizo.Address2, GetValueToUpper(bizo), bizo.PER_IsActive, bizo.PER_RN_NKCountryInternal);
			}
			else
			{
				var staff = staffList?.FirstOrDefault(u => patternAddress.PMA_ParentId == u.PK);
				if (staff != null)
				{
					UpdateCore(patternAddress, staff.PK, staff.Address1, staff.Address2, GetValueToUpper(staff), bizo.PER_IsActive, bizo.PER_RN_NKCountryInternal);
					return;
				}
			}
		}
	}
}
