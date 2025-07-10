using System;
using System.Collections.Generic;
using System.Globalization;
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
	public class PersonPatternMatchingRegCodeRegenerator : PatternMatchingRegCodeRegenerator<GlbPerson>
	{
		List<GlbStaff> staffList;
		List<IHRJobApplicant> applicantList;

		GlbPerson personNeedAdd;
		List<GlbStaff> staffNeedAddList;
		List<IHRJobApplicant> applicantNeedAddList;
		readonly DateTime GregorianStartDate = new DateTime(1753, 1, 1);

		public PersonPatternMatchingRegCodeRegenerator(PatternMatchingRecalculator<GlbPerson> recalculator)
			: base(recalculator)
		{
		}

		protected override int AddCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			var counter = 0;

			if (personNeedAdd != null)
			{
				AddCore(factory, bizo.PK, personNeedAdd.TablePrefix, personNeedAdd.PK, bizo.PER_IsActive, bizo.PER_RN_NKCountryInternal, personNeedAdd.PER_BirthDate, personNeedAdd.PER_Passport, personNeedAdd.PER_DriversLicenseNumber, ZString.Empty, ZString.Empty, ref counter);
			}

			foreach (var staffNeedAdd in staffNeedAddList)
			{
				AddCore(factory, bizo.PK, staffNeedAdd.TablePrefix, staffNeedAdd.PK, bizo.PER_IsActive, bizo.PER_RN_NKCountryInternal, staffNeedAdd.GS_Birthdate, staffNeedAdd.GS_Passport, ZString.Empty, staffNeedAdd.GS_EnterpriseCertificationID, ZString.Empty, ref counter);
			}

			foreach (var applicantNeedAdd in applicantNeedAddList)
			{
				AddCore(factory, bizo.PK, HRJobApplicantSchema.Constants.Prefix, applicantNeedAdd.PK, bizo.PER_IsActive, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, string.Empty, (ZString)(applicantNeedAdd as BusinessObject)?[HRJobApplicantSchema.Constants.HA_OtherIdentityDocument], ref counter);
			}

			return counter;
		}

		void AddCore(BusinessObjectFactory factory, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZBool isActive, ZString countryCode, ZDateTime birthday, ZString passport, ZString license, ZString certificate, ZString other, ref int counter)
		{
			var notNullPropList = new List<string>();

			if (!birthday.IsEmpty)
			{
				var birthdayToHash = (birthday.ToDateTime() - GregorianStartDate).Days.ToString(CultureInfo.InvariantCulture);
				notNullPropList.Add(birthdayToHash);
			}

			if (!passport.IsEmpty)
			{
				var passportToHash = CodeTypeIdentifier.Passport + TextStandardizerHelper.StandardizeRegCode(passport);
				notNullPropList.Add(passportToHash);
			}

			if (!license.IsEmpty)
			{
				var licenseToHash = CodeTypeIdentifier.License + TextStandardizerHelper.StandardizeRegCode(license);
				notNullPropList.Add(licenseToHash);
			}

			if (!certificate.IsEmpty)
			{
				var certificateToHash = CodeTypeIdentifier.Certificate + TextStandardizerHelper.StandardizeRegCode(certificate);
				notNullPropList.Add(certificateToHash);
			}

			if (!other.IsEmpty)
			{
				var otherToHash = CodeTypeIdentifier.Other + TextStandardizerHelper.StandardizeRegCode(other);
				notNullPropList.Add(otherToHash);
			}

			foreach (var propValue in notNullPropList)
			{
				AddCore(factory, propValue, superParentPk, tableCode, parentPk, isActive, countryCode, propValue);
				counter++;
				ReportProgress();
			}
		}

		protected override void SetUltimateParentPK(PatternMatchingRegCode patternRegCode, ZGuid pk)
		{
			patternRegCode.PMR_PER = pk;
		}

		protected override int GetActualAddCount()
		{
			throw new NotImplementedException();
		}

		protected override List<string> TablesPrefixList => new List<string>
		{
			GlbPersonSchema.Constants.Prefix,
			HRJobApplicantSchema.Constants.Prefix,
			GlbStaffSchema.Constants.Prefix
		};

		protected override int InitializeDataCountCore(GlbPerson bizo, BusinessObjectFactory factory)
		{
			bizo.InitData(out var pks, out staffList, out applicantList);
			personNeedAdd = null;
			staffNeedAddList = new List<GlbStaff>();
			applicantNeedAddList = new List<IHRJobApplicant>();
			hasUpdatedPerson = false;
			hasUpdatedStaff = new List<ZGuid>();
			hasUpdatedApplicant = new List<ZGuid>();

			var query = new ZQuery(PatternMatchingRegCodeSchema.PMR_PER, bizo.PK);
			query.AddToFilter(PatternMatchingRegCodeSchema.PMR_ParentTableCode, TablesPrefixList);
			allPatternMatchingRegCodeArray = factory.Load<PatternMatchingRegCode>(query);

			needDelete = allPatternMatchingRegCodeArray.Where(s => !pks.Contains(s.ParentId)).ToArray();
			needUpdate = allPatternMatchingRegCodeArray.Where(s => pks.Contains(s.ParentId)).ToArray();

			var addCount = 0;
			var matchNameparentIds = allPatternMatchingRegCodeArray.Select(a => a.ParentId).Distinct().ToList();

			if (!matchNameparentIds.Contains(bizo.PK) && ShouldAdd(bizo.PER_BirthDate, bizo.PER_Passport, bizo.PER_DriversLicenseNumber, ZString.Empty, ZString.Empty, ref addCount))
			{
				personNeedAdd = bizo;
			}

			foreach (var staff in staffList)
			{
				if (!matchNameparentIds.Contains(staff.PK) && ShouldAdd(staff.GS_Birthdate, staff.GS_Passport, ZString.Empty, staff.GS_EnterpriseCertificationID, ZString.Empty, ref addCount))
				{
					staffNeedAddList.Add(staff);
				}
			}

			foreach (var applicant in applicantList)
			{
				if (!matchNameparentIds.Contains(applicant.PK) && ShouldAdd(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, (ZString)(applicant as BusinessObject)?[HRJobApplicantSchema.Constants.HA_OtherIdentityDocument], ref addCount))
				{
					applicantNeedAddList.Add(applicant);
				}
			}

			return addCount + needDelete.Length + needUpdate.Length;
		}

		bool ShouldAdd(ZDateTime birthDate, ZString passport, ZString license, ZString certificate, ZString other, ref int addCount)
		{
			var rawCount = addCount;
			addCount += !birthDate.IsEmpty ? 1 : 0;
			addCount += !passport.IsEmpty ? 1 : 0;
			addCount += !license.IsEmpty ? 1 : 0;
			addCount += !certificate.IsEmpty ? 1 : 0;
			addCount += !other.IsEmpty ? 1 : 0;

			return addCount > rawCount;
		}

		bool hasUpdatedPerson;
		List<ZGuid> hasUpdatedStaff;
		List<ZGuid> hasUpdatedApplicant;

		protected override void UpdateCore(GlbPerson bizo, PatternMatchingRegCode patternRegCode, BusinessObjectFactory factory)
		{
			if (!hasUpdatedPerson && patternRegCode.ParentId == bizo.PK)
			{
				UpdateCore(factory, bizo.PK, bizo.TablePrefix, bizo.PK, bizo.PER_IsActive, bizo.PER_RN_NKCountryInternal, bizo.PER_BirthDate, bizo.PER_Passport, bizo.PER_DriversLicenseNumber, ZString.Empty, ZString.Empty, PersonPropNames);
				hasUpdatedPerson = true;
			}
			else
			{
				var staff = staffList?.FirstOrDefault(u => u.PK == patternRegCode.ParentId);
				if (staff != null && !hasUpdatedStaff.Contains(staff.PK))
				{
					UpdateCore(factory, bizo.PK, staff.TablePrefix, staff.PK, bizo.PER_IsActive, bizo.PER_RN_NKCountryInternal, staff.GS_Birthdate, staff.GS_Passport, ZString.Empty, staff.GS_EnterpriseCertificationID, ZString.Empty, StaffPropNames);
					hasUpdatedStaff.Add(staff.PK);
					return;
				}

				var applicant = applicantList?.FirstOrDefault(u => u.PK == patternRegCode.ParentId);
				if (applicant != null && !hasUpdatedApplicant.Contains(applicant.PK))
				{
					UpdateCore(factory, bizo.PK, HRJobApplicantSchema.Constants.Prefix, applicant.PK, bizo.PER_IsActive, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, (ZString)(applicant as BusinessObject)?[HRJobApplicantSchema.Constants.HA_OtherIdentityDocument], ApplicantPropNames);
					hasUpdatedApplicant.Add(applicant.PK);
				}
			}
		}

		readonly string[] PersonPropNames = { GlbPersonSchema.Constants.PER_BirthDate, GlbPersonSchema.Constants.PER_Passport, GlbPersonSchema.Constants.PER_DriversLicenseNumber, string.Empty, string.Empty };
		readonly string[] StaffPropNames = { GlbStaffSchema.Constants.GS_Birthdate, GlbStaffSchema.Constants.GS_Passport, string.Empty, GlbStaffSchema.Constants.GS_EnterpriseCertificationID, string.Empty };
		readonly string[] ApplicantPropNames = { string.Empty, string.Empty, string.Empty, string.Empty, HRJobApplicantSchema.Constants.HA_OtherIdentityDocument };

		void UpdateCore(BusinessObjectFactory factory, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZBool isActive, ZString countryCode, ZDateTime birthday, ZString passport, ZString license, ZString certificate, ZString other, string[] propNames)
		{
			var notNullPropValueList = new List<string>();
			var notNullPropNameList = new List<string>();
			var patternMatchingRegCodes = needUpdate.Where(u => !u.IsDeleted && u.ParentId == parentPk).ToList();

			if (!birthday.IsEmpty)
			{
				var birthdayToHash = (birthday.ToDateTime() - GregorianStartDate).Days.ToString(CultureInfo.InvariantCulture);
				notNullPropValueList.Add(birthdayToHash);
				notNullPropNameList.Add(propNames[0]);
			}

			if (!passport.IsEmpty)
			{
				var passportToHash = CodeTypeIdentifier.Passport + TextStandardizerHelper.StandardizeRegCode(passport);
				notNullPropValueList.Add(passportToHash);
				notNullPropNameList.Add(propNames[1]);
			}

			if (!license.IsEmpty)
			{
				var licenseToHash = CodeTypeIdentifier.License + TextStandardizerHelper.StandardizeRegCode(license);
				notNullPropValueList.Add(licenseToHash);
				notNullPropNameList.Add(propNames[2]);
			}

			if (!certificate.IsEmpty)
			{
				var certificateToHash = CodeTypeIdentifier.Certificate + TextStandardizerHelper.StandardizeRegCode(certificate);
				notNullPropValueList.Add(certificateToHash);
				notNullPropNameList.Add(propNames[3]);
			}

			if (!other.IsEmpty)
			{
				var otherToHash = CodeTypeIdentifier.Other + TextStandardizerHelper.StandardizeRegCode(other);
				notNullPropValueList.Add(otherToHash);
				notNullPropNameList.Add(propNames[4]);
			}

			while (patternMatchingRegCodes.Count > 0 && patternMatchingRegCodes.Count > notNullPropValueList.Count)
			{
				var patternMatchingRegCode = patternMatchingRegCodes[0];

				patternMatchingRegCodes.Remove(patternMatchingRegCode);
				patternMatchingRegCode.Delete();
			}

			while (patternMatchingRegCodes.Count > 0 && patternMatchingRegCodes.Count < notNullPropValueList.Count)
			{
				var matchingRegCode = factory.New<PatternMatchingRegCode>();

				matchingRegCode.PMR_PER = superParentPk;
				matchingRegCode.PMR_ParentTableCode = tableCode;
				matchingRegCode.PMR_ParentId = parentPk;
				matchingRegCode.PMR_IsActive = isActive;
				matchingRegCode.PMR_RN_NKCountryCode = countryCode;
				patternMatchingRegCodes.Add(matchingRegCode);
			}

			for (var i = 0; i < notNullPropValueList.Count; i++)
			{
				ZString needHashValue = notNullPropValueList[i];

				patternMatchingRegCodes[i].PMR_RN_NKCountryCode = countryCode;
				patternMatchingRegCodes[i].HashedValue = TextStandardizerHelper.ComputeStringHashFast(needHashValue);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = notNullPropNameList[i],
					Hash = patternMatchingRegCodes[i].HashedValue,
					OriginalValue = needHashValue,
					PK = parentPk.ToGuid(),
					StandardizedValue = needHashValue,
					TableName = patternMatchingRegCodes[i].TablePrefix
				});
			}

			ReportProgress();
		}
	}
}
