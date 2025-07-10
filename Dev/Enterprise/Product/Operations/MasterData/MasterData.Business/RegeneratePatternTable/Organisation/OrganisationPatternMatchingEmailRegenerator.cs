using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrganisationPatternMatchingEmailRegenerator : PatternMatchingEmailRegenerator<OrgHeader>
	{
		OrgAddress[] allOrgAddressArray;
		OrgContact[] allOrgContactArray;

		OrgAddress[] addressNeedAdd;
		OrgContact[] contactNeedAdd;

		public OrganisationPatternMatchingEmailRegenerator(PatternMatchingRecalculator<OrgHeader> recalculator)
			: base(recalculator)
		{
		}

		protected override List<string> TablesPrefixList
		{
			get
			{
				return new List<string>()
				{
					OrgAddressSchema.Constants.Prefix,
					OrgContactSchema.Constants.Prefix,
				};
			}
		}

		protected override int GetActualAddCount()
		{
			return addressNeedAdd.Length + contactNeedAdd.Length;
		}

		protected override int InitializeDataCountCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			var query = new ZQuery(OrgAddressSchema.OA_OH, bizo.PK);
			allOrgAddressArray = factory.Load<OrgAddress>(query);

			query = new ZQuery(OrgContactSchema.OC_OH, bizo.PK);
			allOrgContactArray = factory.Load<OrgContact>(query);

			query = new ZQuery(PatternMatchingEmailSchema.PME_OH, bizo.PK);
			query.AddToFilter(PatternMatchingEmailSchema.PME_ParentTableCode, TablesPrefixList);
			patternMatchingEmailArray = factory.Load<PatternMatchingEmail>(query);

			var matchEmailparentIds = patternMatchingEmailArray.Select(a => a.ParentId).ToList();

			addressNeedAdd = allOrgAddressArray.Where(s => !s.OA_Email.IsEmpty && !matchEmailparentIds.Contains(s.PK)).ToArray();
			contactNeedAdd = allOrgContactArray.Where(s => !s.OC_Email.IsEmpty && !matchEmailparentIds.Contains(s.PK)).ToArray();

			var orgAddressIds = allOrgAddressArray.Select(a => a.PK).ToList();
			var orgContactIds = allOrgContactArray.Select(a => a.PK).ToList();

			needDelete = patternMatchingEmailArray.Where(s => !orgAddressIds.Contains(s.ParentId) && !orgContactIds.Contains(s.ParentId)).ToArray();
			needUpdate = patternMatchingEmailArray.Where(s => orgAddressIds.Contains(s.ParentId) || orgContactIds.Contains(s.ParentId)).ToArray();

			return GetActualAddCount() + needDelete.Length + needUpdate.Length;
		}

		protected override void SetUltimateParentPK(PatternMatchingEmail patternEmail, ZGuid pk, ZGuid parentPK)
		{
			patternEmail.PME_OH = pk;
		}

		protected override int AddCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			addressNeedAdd.ForEach((item) =>
			{
				if (!item.OA_Email.IsEmpty)
				{
					AddCore(factory, TextStandardizerHelper.StandardizeEmail(item.OA_Email), bizo.PK, item.TablePrefix, item.PK, item.OA_RN_NKCountryCode, nameof(item.OA_Email));
				}
			});

			contactNeedAdd.ForEach((item) =>
			{
				if (!item.OC_Email.IsEmpty)
				{
					AddCore(factory, item.OC_Email, bizo.PK, item.TablePrefix, item.PK, bizo.CountryCode, nameof(item.OC_Email));
				}
			});

			return GetActualAddCount();
		}

		protected override void UpdateCore(OrgHeader bizo, PatternMatchingEmail patternEmail, BusinessObjectFactory factory)
		{
			if (patternEmail.ParentTableCode.Equals(OrgAddressSchema.Constants.Prefix))
			{
				var orgAddress = allOrgAddressArray.Where(oa => oa.PK.Equals(patternEmail.ParentId)).FirstOrDefault();

				patternEmail.PME_IsActive = orgAddress.OA_IsActive;
				UpdateCore(patternEmail, orgAddress.OA_Email, orgAddress.OA_RN_NKCountryCode, orgAddress.PK, nameof(orgAddress.OA_Email));
			}
			else if (patternEmail.ParentTableCode.Equals(OrgContactSchema.Constants.Prefix))
			{
				var orgContact = allOrgContactArray.Where(oc => oc.PK.Equals(patternEmail.ParentId)).FirstOrDefault();

				patternEmail.PME_IsActive = orgContact.OC_IsActive;
				UpdateCore(patternEmail, orgContact.OC_Email, bizo.CountryCode, orgContact.PK, nameof(orgContact.OC_Email));
			}
		}
	}
}
