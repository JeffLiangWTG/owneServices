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
	public class OrganisationPatternMatchingRegCodeRegenerator : PatternMatchingRegCodeRegenerator<OrgHeader>
	{
		OrgCusCode[] allOrgCusCodeArray;
		OrgCusCode[] CusCodeNeedAdd;

		public OrganisationPatternMatchingRegCodeRegenerator(PatternMatchingRecalculator<OrgHeader> recalculator)
			: base(recalculator)
		{
		}

		protected override List<string> TablesPrefixList
		{
			get
			{
				return new List<string>()
				{
					OrgCusCodeSchema.Constants.Prefix
				};
			}
		}

		protected override int GetActualAddCount()
		{
			return CusCodeNeedAdd.Length;
		}

		protected override int AddCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			CusCodeNeedAdd.ForEach((item) =>
			{
				if (!TextStandardizerHelper.IsPlaceholderRegCode(item.OK_CustomsRegNo))
				{
					AddCore(factory, item.OK_CustomsRegNo, bizo.PK, item.TablePrefix, item.PK, ZBool.True, item.OK_RN_NKCodeCountry, nameof(item.OK_CustomsRegNo));
				}

				ReportProgress();
			});

			return GetActualAddCount();
		}

		protected override void SetUltimateParentPK(PatternMatchingRegCode patternRegCode, ZGuid pk)
		{
			patternRegCode.PMR_OH = pk;
		}

		protected override int InitializeDataCountCore(OrgHeader bizo, BusinessObjectFactory factory)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_OH, bizo.PK);
			allOrgCusCodeArray = factory.Load<OrgCusCode>(query);

			query = new ZQuery(PatternMatchingRegCodeSchema.PMR_OH, bizo.PK);
			query.AddToFilter(PatternMatchingRegCodeSchema.PMR_ParentTableCode, TablesPrefixList);
			allPatternMatchingRegCodeArray = factory.Load<PatternMatchingRegCode>(query);

			var matchCusCodeparentIds = allPatternMatchingRegCodeArray.Select(a => a.ParentId).ToList();
			CusCodeNeedAdd = allOrgCusCodeArray.Where(s => !s.OK_CustomsRegNo.IsEmpty && !matchCusCodeparentIds.Contains(s.PK)).ToArray();

			var orgCusCodeIds = allOrgCusCodeArray.Select(a => a.PK).ToList();

			needDelete = allPatternMatchingRegCodeArray.Where(s => !orgCusCodeIds.Contains(s.ParentId)).ToArray();
			needUpdate = allPatternMatchingRegCodeArray.Where(s => orgCusCodeIds.Contains(s.ParentId)).ToArray();

			return GetActualAddCount() + needDelete.Length + needUpdate.Length;
		}

		protected override void UpdateCore(OrgHeader bizo, PatternMatchingRegCode patternRegCode, BusinessObjectFactory factory)
		{
			var orgCusCode = allOrgCusCodeArray.Where(oa => oa.PK.Equals(patternRegCode.ParentId)).FirstOrDefault();

			if (!orgCusCode.OK_CustomsRegNo.IsEmpty && !TextStandardizerHelper.IsPlaceholderRegCode(orgCusCode.OK_CustomsRegNo))
			{
				var valueToHash = Utils.RemoveAllWhiteSpaceCharacters(orgCusCode.OK_CustomsRegNo);

				patternRegCode.PMR_RN_NKCountryCode = orgCusCode.OK_RN_NKCodeCountry;
				patternRegCode.PMR_IsActive = ZBool.True;
				patternRegCode.HashedValue = TextStandardizerHelper.ComputeStringHashFast(valueToHash);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = nameof(orgCusCode.OK_CustomsRegNo),
					Hash = patternRegCode.HashedValue,
					OriginalValue = orgCusCode.OK_CustomsRegNo,
					PK = orgCusCode.PK.ToGuid(),
					StandardizedValue = valueToHash,
					TableName = patternRegCode.TablePrefix
				});
			}
			else
			{
				patternRegCode.Delete();
			}

			ReportProgress();
		}
	}
}
