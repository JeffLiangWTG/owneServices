using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrganisationPatternMatchingNameRegenerator : PatternMatchingNameRegenerator<OrgHeader>
	{
		OrgAddress[] allOrgAddressArray;
		OrgHeader[] allOrgHeaderArray;
		OrgBrandOrRelatedName[] allOrgBrandOrRelatedNameArray;

		OrgAddress[] addressNeedAdd;
		OrgHeader[] orgHeaderNeedAdd;
		OrgBrandOrRelatedName[] orgBrandOrRelatedNameNeedAdd;

		public OrganisationPatternMatchingNameRegenerator(PatternMatchingRecalculator<OrgHeader> recalculator)
			: base(recalculator)
		{
		}

		protected override int GetActualAddCount()
		{
			return addressNeedAdd.Length + orgBrandOrRelatedNameNeedAdd.Length + orgHeaderNeedAdd.Length;
		}

		void CreateMatchedNameParentIDs(PatternMatchingName[] patternMatchingNames)
		{
			var matchNameparentIds = patternMatchingNames.Select(a => a.ParentId).ToList();
			addressNeedAdd = allOrgAddressArray.Where(s => !s.OA_CompanyNameOverride.IsEmpty && !matchNameparentIds.Contains(s.PK)).ToArray();
			orgBrandOrRelatedNameNeedAdd = allOrgBrandOrRelatedNameArray.Where(s => !s.P1_RelatedName.IsEmpty && !matchNameparentIds.Contains(s.PK)).ToArray();
			orgHeaderNeedAdd = allOrgHeaderArray.Where(s => !s.OH_FullName.IsEmpty && !matchNameparentIds.Contains(s.PK)).ToArray();
		}

		void ComputeDataToManipulate()
		{
			var orgAddressIds = allOrgAddressArray.Select(a => a.PK).ToList();
			var orgBrandOrRelatedNameIds = allOrgBrandOrRelatedNameArray.Select(a => a.PK).ToList();
			var orgHeaderArrayIds = allOrgHeaderArray.Select(a => a.PK).ToList();

			needDelete = patternMatchingNameArray.Where(s => !orgAddressIds.Contains(s.ParentId) && !orgBrandOrRelatedNameIds.Contains(s.ParentId) && !orgHeaderArrayIds.Contains(s.ParentId)).ToArray();
			needUpdate = patternMatchingNameArray.Where(s => orgAddressIds.Contains(s.ParentId) || orgBrandOrRelatedNameIds.Contains(s.ParentId) || orgHeaderArrayIds.Contains(s.ParentId)).ToArray();
		}

		protected override int AddCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			addressNeedAdd.ForEach((item) =>
			{
				AddCore(factory, item.OA_CompanyNameOverride, bizo.PK, item.TablePrefix, item.PK, item.OA_RN_NKCountryCode, nameof(item.OA_CompanyNameOverride));
			});

			orgBrandOrRelatedNameNeedAdd.ForEach((item) =>
			{
				AddCore(factory, item.P1_RelatedName, bizo.PK, item.TablePrefix, item.PK, bizo.CountryCode, nameof(item.P1_RelatedName));
			});

			orgHeaderNeedAdd.ForEach((item) =>
			{
				AddCore(factory, item.OH_FullName, bizo.PK, item.TablePrefix, item.PK, item.CountryCode, nameof(item.OH_FullName));
			});

			return GetActualAddCount();
		}

		protected override void SetUltimateParentPK(PatternMatchingName patternName, ZGuid pk)
		{
			patternName.PMN_OH = pk;
		}

		protected override int InitializeDataCountCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			allOrgHeaderArray = new OrgHeader[] { bizo };

			var query = new ZQuery(OrgAddressSchema.OA_OH, bizo.PK);
			allOrgAddressArray = factory.Load<OrgAddress>(query);

			query = new ZQuery(OrgBrandOrRelatedNameSchema.P1_OH, bizo.PK);
			allOrgBrandOrRelatedNameArray = factory.Load<OrgBrandOrRelatedName>(query);

			query = new ZQuery(PatternMatchingNameSchema.PMN_OH, bizo.PK);
			query.AddToFilter(PatternMatchingNameSchema.PMN_ParentTableCode, TablesPrefixList);
			patternMatchingNameArray = factory.Load<PatternMatchingName>(query);

			CreateMatchedNameParentIDs(patternMatchingNameArray);
			ComputeDataToManipulate();

			return GetActualAddCount() + needDelete.Length + needUpdate.Length;
		}

		protected override void UpdateCore(OrgHeader bizo, PatternMatchingName patternName)
		{
			if (patternName.ParentTableCode.Equals(OrgAddressSchema.Constants.Prefix))
			{
				var orgAddress = allOrgAddressArray.Where(oa => oa.PK.Equals(patternName.ParentId)).FirstOrDefault();
				ZString valueToHash = orgAddress.OA_CompanyNameOverride;

				UpdateCore(patternName, valueToHash, orgAddress.OA_IsActive, orgAddress.OA_RN_NKCountryCode, orgAddress.PK, nameof(orgAddress.OA_CompanyNameOverride));
			}
			else if (patternName.ParentTableCode.Equals(OrgBrandOrRelatedNameSchema.Constants.Prefix))
			{
				var brand = allOrgBrandOrRelatedNameArray.Where(oa => oa.PK.Equals(patternName.ParentId)).FirstOrDefault();
				ZString needHashValue = brand.P1_RelatedName;

				UpdateCore(patternName, needHashValue, ZBool.True, bizo.CountryCode, brand.PK, nameof(brand.P1_RelatedName));
			}
			else if (patternName.ParentTableCode.Equals(OrgHeaderSchema.Constants.Prefix))
			{
				var orgHeader = allOrgHeaderArray.Where(oa => oa.PK.Equals(patternName.ParentId)).FirstOrDefault();
				ZString needHashValue = orgHeader.OH_FullName;

				UpdateCore(patternName, needHashValue, orgHeader.OH_IsActive, orgHeader.CountryCode, orgHeader.PK, nameof(orgHeader.OH_FullName));
			}

			ReportProgress();
		}
	}
}
