using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class PatternMatchingTestHelper
	{
		public static T CreatePatternMatchingObject<T>(BusinessObjectFactory factory, BusinessObject parent) where T : BusinessObject, IPatternMatchingBusinessObjects
		{
			var patternMatchingBizo = factory.NewWithValidTestData<T>();
			patternMatchingBizo.ParentId = parent.PK;
			patternMatchingBizo.ParentTableCode = parent.TablePrefix;
			return patternMatchingBizo;
		}

		public static PatternMatchingResult CreatePatternMatchingResult(BusinessObjectFactory factory, BusinessObject parent, string status = null)
		{
			var result = factory.NewWithValidTestData<PatternMatchingResult>();
			result.PMT_FoundTimeUtc = ZDateTime.Now;
			result.PMT_GS_NKExcludeBy = "NA";
			result.PMT_Status = status;
			return result;
		}

		public static BusinessObject[] CreateCompleteSetOfPatternMatchingObjects(BusinessObjectFactory factory, BusinessObject parent)
		{
			PatternMatchingAddress pmAddress;
			PatternMatchingDomain pmDomain;
			PatternMatchingEmail pmEmail;
			PatternMatchingName pmName;
			PatternMatchingPhone pmPhone;
			PatternMatchingRegCode pmRegCode;

			if (parent is GlbPerson)
			{
				pmAddress = CreatePatternMatchingObject<PatternMatchingAddress>(factory, parent);
				pmEmail = CreatePatternMatchingObject<PatternMatchingEmail>(factory, parent);
				pmName = CreatePatternMatchingObject<PatternMatchingName>(factory, parent);
				pmPhone = CreatePatternMatchingObject<PatternMatchingPhone>(factory, parent);
				pmRegCode = CreatePatternMatchingObject<PatternMatchingRegCode>(factory, parent);
				pmAddress.PMA_PER = parent.PK;
				pmEmail.PME_PER = parent.PK;
				pmName.PMN_PER = parent.PK;
				pmPhone.PMP_PER = parent.PK;
				pmRegCode.PMR_PER = parent.PK;

				return new BusinessObject[] { pmAddress, pmEmail, pmName, pmPhone, pmRegCode };
			}

			if (parent is OrgHeader)
			{
				pmName = CreatePatternMatchingObject<PatternMatchingName>(factory, parent);
				pmName.PMN_OH = parent.PK;

				return new BusinessObject[] { pmName };
			}

			if (parent is OrgContact)
			{
				pmDomain = CreatePatternMatchingObject<PatternMatchingDomain>(factory, parent);
				pmEmail = CreatePatternMatchingObject<PatternMatchingEmail>(factory, parent);
				pmName = CreatePatternMatchingObject<PatternMatchingName>(factory, parent);
				pmPhone = CreatePatternMatchingObject<PatternMatchingPhone>(factory, parent);
				pmRegCode = CreatePatternMatchingObject<PatternMatchingRegCode>(factory, parent);

				return new BusinessObject[] { pmDomain, pmEmail, pmName, pmPhone, pmRegCode };
			}

			if (parent is GenRegCertAccredMaintList)
			{
				pmRegCode = CreatePatternMatchingObject<PatternMatchingRegCode>(factory, parent);

				return new BusinessObject[] { pmRegCode };
			}

			if (parent is GlbStaff)
			{
				pmAddress = CreatePatternMatchingObject<PatternMatchingAddress>(factory, parent);
				pmEmail = CreatePatternMatchingObject<PatternMatchingEmail>(factory, parent);
				pmName = CreatePatternMatchingObject<PatternMatchingName>(factory, parent);
				pmPhone = CreatePatternMatchingObject<PatternMatchingPhone>(factory, parent);
				pmRegCode = CreatePatternMatchingObject<PatternMatchingRegCode>(factory, parent);

				return new BusinessObject[] { pmAddress, pmEmail, pmName, pmPhone, pmRegCode };
			}

			pmAddress = CreatePatternMatchingObject<PatternMatchingAddress>(factory, parent);
			pmDomain = CreatePatternMatchingObject<PatternMatchingDomain>(factory, parent);
			pmEmail = CreatePatternMatchingObject<PatternMatchingEmail>(factory, parent);
			pmName = CreatePatternMatchingObject<PatternMatchingName>(factory, parent);
			pmPhone = CreatePatternMatchingObject<PatternMatchingPhone>(factory, parent);
			pmRegCode = CreatePatternMatchingObject<PatternMatchingRegCode>(factory, parent);

			return new BusinessObject[] { pmAddress, pmDomain, pmEmail, pmName, pmPhone, pmRegCode };
		}

		public static PatternMatchingResult[] CreateCompleteSetOfPatternMatchingResults(BusinessObjectFactory factory, BusinessObject parent)
		{
			var mkResult = CreatePatternMatchingResult(factory, parent, PatternMatchingResult.StatusCodes.NoDuplicates);
			mkResult.PMT_MasterPK = parent.PK;
			mkResult.PMT_MasterTableCode = parent.TablePrefix;
			var tkResult = CreatePatternMatchingResult(factory, parent, PatternMatchingResult.StatusCodes.PermanentIgnore);
			tkResult.PMT_TargetTableCode = parent.TablePrefix;
			tkResult.PMT_MasterTableCode = parent.TablePrefix;
			tkResult.PMT_TargetPK = parent.PK;
			var mkTkResult = CreatePatternMatchingResult(factory, parent, PatternMatchingResult.StatusCodes.PermanentIgnore);
			mkTkResult.PMT_TargetTableCode = parent.TablePrefix;
			mkTkResult.PMT_MasterTableCode = parent.TablePrefix;
			mkTkResult.PMT_MasterPK = parent.PK;
			mkTkResult.PMT_TargetPK = parent.PK;

			return new PatternMatchingResult[] { mkResult, tkResult, mkTkResult };
		}
	}
}
