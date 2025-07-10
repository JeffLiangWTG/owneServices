using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingAddressRegenerator<TBizo> : PatternMatchingRegenerator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Calculate Fields

		protected PatternMatchingAddress[] allPatternMatchingAddressArray;
		protected PatternMatchingAddress[] patternMatchingAddressesToDelete;
		protected PatternMatchingAddress[] patternMatchingAddressesToUpdate;

		#endregion

		protected PatternMatchingAddressRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
			: base(recalculator)
		{
		}

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			return InitializeDataCountCore(bizo, factory);
		}

		protected override int Delete()
		{
			patternMatchingAddressesToDelete.ForEach((x) =>
			{
				x.Delete();
				ReportProgress();
			});

			return patternMatchingAddressesToDelete.Length;
		}

		protected void AddCore(BusinessObjectFactory factory, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZString countryCode, ZString valueToHash, ZString address1, ZString address2)
		{
			if (!valueToHash.IsEmpty && (!TextStandardizerHelper.IsPlaceholderAddress(address1) || address1.IsEmpty && !TextStandardizerHelper.IsPlaceholderAddress(address2)))
			{
				var matchingAddress = factory.New<PatternMatchingAddress>();

				SetUltimateParentPK(matchingAddress, superParentPk);
				matchingAddress.PMA_ParentTableCode = tableCode;
				matchingAddress.PMA_ParentId = parentPk;
				matchingAddress.PMA_RN_NKCountryCode = countryCode;
				matchingAddress.PMA_IsActive = ZBool.True;
				matchingAddress.HashedValue = TextStandardizerHelper.ComputeStringHashFast(valueToHash);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = ColumnsToHash,
					Hash = matchingAddress.HashedValue,
					OriginalValue = string.Empty,
					PK = parentPk.ToGuid(),
					StandardizedValue = valueToHash,
					TableName = matchingAddress.TablePrefix
				});
			}

			ReportProgress();
		}

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			return AddCore(bizo, factory);
		}

		protected void UpdateCore(PatternMatchingAddress patternAddress, ZGuid targetPk, ZString address1, ZString address2, ZString valueToHash, ZBool isActive, ZString countryCode)
		{
			if (!valueToHash.IsEmpty && (!TextStandardizerHelper.IsPlaceholderAddress(address1) || address1.IsEmpty && !TextStandardizerHelper.IsPlaceholderAddress(address2)))
			{
				patternAddress.PMA_IsActive = isActive;
				patternAddress.PMA_RN_NKCountryCode = countryCode;
				patternAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(valueToHash);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = ColumnsToHash,
					Hash = patternAddress.HashedValue,
					OriginalValue = valueToHash,
					PK = targetPk.ToGuid(),
					StandardizedValue = valueToHash,
					TableName = patternAddress.TablePrefix
				});
			}
			else
			{
				patternAddress.Delete();
			}

			ReportProgress();
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			patternMatchingAddressesToUpdate.ForEach((item) =>
			{
				UpdateCore(bizo, item);
			});

			return patternMatchingAddressesToUpdate.Length;
		}

		string ColumnsToHash
		{
			get
			{
				return (NoResString)"Address1, Address2, City, PostCode, State";
			}
		}

		protected string GetValueToUpper(ISupportWebAddressValidation addressKeeper)
		{
			var valueToHash = addressKeeper.Address1 + addressKeeper.Address2 + addressKeeper.City + addressKeeper.Postcode + addressKeeper.StateCode;
			return valueToHash.ToUpperInvariant();
		}

		protected abstract int InitializeDataCountCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract int AddCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void SetUltimateParentPK(PatternMatchingAddress patternAddress, ZGuid pk);

		protected abstract void UpdateCore(TBizo bizo, PatternMatchingAddress patternAddress);
	}
}
