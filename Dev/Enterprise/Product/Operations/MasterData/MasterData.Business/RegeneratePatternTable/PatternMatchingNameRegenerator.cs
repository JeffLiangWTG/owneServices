using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingNameRegenerator<TBizo> : PatternMatchingRegenerator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Calculate Fields

		protected PatternMatchingName[] patternMatchingNameArray;

		protected PatternMatchingName[] needDelete;
		protected PatternMatchingName[] needUpdate;

		#endregion

		protected PatternMatchingNameRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
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
					OrgHeaderSchema.Constants.Prefix,
					OrgBrandOrRelatedNameSchema.Constants.Prefix,
				};
			}
		}

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			return InitializeDataCountCore(bizo, factory);
		}

		protected override int Delete()
		{
			needDelete.ForEach((item) =>
			{
				item.Delete();

				ReportProgress();
			});

			return needDelete.Length;
		}

		protected void AddCore(BusinessObjectFactory factory, ZString name, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZString countryCode, ZString debuggerName)
		{
			if (!name.IsEmpty)
			{
				var matchingName = factory.New<PatternMatchingName>();
				var standardName = TextStandardizerHelper.StandardizeCompanyName(name, countryCode);

				SetUltimateParentPK(matchingName, superParentPk);

				matchingName.PMN_ParentTableCode = tableCode;
				matchingName.PMN_ParentId = parentPk;
				matchingName.PMN_RN_NKCountryCode = countryCode;
				matchingName.PMN_IsActive = ZBool.True;
				matchingName.HashedValue = TextStandardizerHelper.ComputeStringHashFast(standardName);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = debuggerName,
					Hash = matchingName.HashedValue,
					OriginalValue = name,
					PK = parentPk.ToGuid(),
					StandardizedValue = standardName,
					TableName = tableCode
				});

				ReportProgress();
			}
		}

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			return AddCore(bizo, factory);
		}

		protected void UpdateCore(PatternMatchingName patternName, ZString valueToHash, ZBool isActive, ZString countryCode, ZGuid parentPk, ZString debuggerName)
		{
			if (!valueToHash.IsEmpty)
			{
				var standardName = TextStandardizerHelper.StandardizeCompanyName(valueToHash, countryCode);

				patternName.PMN_IsActive = isActive;
				patternName.HashedValue = TextStandardizerHelper.ComputeStringHashFast(standardName);
				patternName.PMN_RN_NKCountryCode = countryCode;

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = debuggerName,
					Hash = patternName.HashedValue,
					OriginalValue = valueToHash,
					PK = parentPk.ToGuid(),
					StandardizedValue = standardName,
					TableName = patternName.TablePrefix
				});
			}
			else
			{
				patternName.Delete();
			}
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			needUpdate.ForEach((item) =>
			{
				UpdateCore(bizo, item);
			});

			return needUpdate.Length;
		}

		protected abstract int InitializeDataCountCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void SetUltimateParentPK(PatternMatchingName patternName, ZGuid pk);

		protected abstract int AddCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void UpdateCore(TBizo bizo, PatternMatchingName patternName);
	}
}

