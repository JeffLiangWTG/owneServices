using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingRegCodeRegenerator<TBizo> : PatternMatchingRegenerator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Calculate Fields

		protected PatternMatchingRegCode[] allPatternMatchingRegCodeArray;

		protected PatternMatchingRegCode[] needDelete;
		protected PatternMatchingRegCode[] needUpdate;

		#endregion

		protected PatternMatchingRegCodeRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
			: base(recalculator)
		{
		}

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			return InitializeDataCountCore(bizo, factory);
		}

		protected void AddCore(BusinessObjectFactory factory, ZString regCode, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZBool isActive, ZString countryCode, ZString debuggerName)
		{
			var matchingRegCode = factory.New<PatternMatchingRegCode>();
			var valueToHash = Utils.RemoveAllWhiteSpaceCharacters(regCode);

			SetUltimateParentPK(matchingRegCode, superParentPk);
			matchingRegCode.PMR_ParentTableCode = tableCode;
			matchingRegCode.PMR_ParentId = parentPk;
			matchingRegCode.PMR_IsActive = isActive;
			matchingRegCode.PMR_RN_NKCountryCode = countryCode;
			matchingRegCode.HashedValue = TextStandardizerHelper.ComputeStringHashFast(valueToHash);

			patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
			{
				ColumnName = debuggerName,
				Hash = matchingRegCode.HashedValue,
				OriginalValue = regCode,
				PK = parentPk.ToGuid(),
				StandardizedValue = valueToHash,
				TableName = tableCode
			});
		}

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			return AddCore(bizo, factory);
		}

		protected override int Delete()
		{
			needDelete.ForEach((x) =>
			{
				x.Delete();

				ReportProgress();
			});

			return needDelete.Length;
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			needUpdate.ForEach((item) =>
			{
				UpdateCore(bizo, item, factory);
			});

			return needUpdate.Length;
		}

		protected abstract int InitializeDataCountCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract int AddCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void UpdateCore(TBizo bizo, PatternMatchingRegCode patternRegCode, BusinessObjectFactory factory);

		protected abstract void SetUltimateParentPK(PatternMatchingRegCode patternRegCode, ZGuid pk);
	}
}

