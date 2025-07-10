using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.NumberFountain;
using static System.FormattableString;

namespace Enterprise.eTail.ServiceTasks
{
	class NumberFountainFixer
	{
		public NumberFountainFixer(NumberFountainInfo fountainInfo, IHVLVReferenceInfo referenceInfo)
		{
			FountainInfo = Argument.NotNull(fountainInfo, nameof(fountainInfo));
			ReferenceInfo = Argument.NotNull(referenceInfo, nameof(referenceInfo));
		}

		protected NumberFountainInfo FountainInfo { get; }
		IHVLVReferenceInfo ReferenceInfo { get; }

		public void FixFountainToNextAvailableSpot()
		{
			FountainInfo.Fountain.GetMinAndMaxValues(Db.Connection, out _, out var maxValue);
			var peekedNumber = FountainInfo.Fountain.PeekPreliminary(Db.Connection);

			var lowerBound = peekedNumber;
			var upperBound = Math.Min(lowerBound + 1, maxValue);
			var checkCount = 1;
			var maxCheckCount = (int)Math.Log(int.MaxValue, 2);
			var found = false;

			while (!found && upperBound > lowerBound)
			{
				var occupiedCount = GetOccupiedCountForNumberRange(lowerBound, upperBound);

				if (IsNumberRangeFullyOccupied(lowerBound, upperBound, occupiedCount))
				{
					checkCount++;
					var range = checkCount > maxCheckCount ? int.MaxValue : (int)Math.Pow(2, checkCount);
					lowerBound = upperBound + 1;
					upperBound = Math.Min(lowerBound + range, maxValue);
				}
				else
				{
					var nextNumber = FindNextAvailableNumberInRange(lowerBound, upperBound);

					if (nextNumber != peekedNumber)
					{
						using (var transactionManager = Db.Connection.BeginTransactionWithManager())
						{
							FountainInfo.Fountain.SetNext(Db.Connection, nextNumber);
							transactionManager.CommitTransaction();
						}
					}

					found = true;
				}
			}

			if (!found)
			{
				throw new NumberFountainException("Unable to find unused fountain number.");
			}
		}

		bool IsNumberRangeFullyOccupied(long lowerBound, long upperBound, int occupiedCount) => occupiedCount == upperBound - lowerBound + 1;

		long FindNextAvailableNumberInRange(long lowerBound, long upperBound)
		{
			if (lowerBound >= upperBound)
			{
				return lowerBound;
			}

			var firstHalfLowerBound = lowerBound;
			var firstHalfUpperBound = (upperBound + lowerBound) / 2;
			var secondHalfLowerBound = firstHalfUpperBound + 1;
			var secondHalfUpperBound = upperBound;

			var firstHalfCount = GetOccupiedCountForNumberRange(firstHalfLowerBound, firstHalfUpperBound);

			if (firstHalfCount == 0)
			{
				return firstHalfLowerBound;
			}

			return !IsNumberRangeFullyOccupied(firstHalfLowerBound, firstHalfUpperBound, firstHalfCount)
				? FindNextAvailableNumberInRange(firstHalfLowerBound, firstHalfUpperBound)
				: FindNextAvailableNumberInRange(secondHalfLowerBound, secondHalfUpperBound);
		}

		int GetOccupiedCountForNumberRange(long lowerBound, long upperBound) => GetOccupiedCountForRange(GetExclusiveLowerBoundSearchValue(lowerBound), GetExclusiveUpperBoundSearchValue(upperBound));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int GetOccupiedCountForRange(string lowerBound, string upperBound)
		{
			const string searchLengthParam = "@searchLength";
			const string validIdLengthParam = "@validIdLength";
			const string lowerBoundParam = "@lowerBound";
			const string upperBoundParam = "@upperBound";

			var sql = Invariant($@"
SELECT COUNT(*)
FROM
(
	SELECT DISTINCT LEFT({ReferenceInfo.IdColumn.Name}, {searchLengthParam})
	FROM {ReferenceInfo.IdColumn.TableName}
	WHERE {ReferenceInfo.IdColumn.Name} > {lowerBoundParam} AND {ReferenceInfo.IdColumn.Name} < {upperBoundParam}
	AND LEN({ReferenceInfo.IdColumn.Name}) = {validIdLengthParam}
	AND {ReferenceInfo.IsValidatedForUniquenessColumn.Name} = 1
) Ids(Id)");

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter(searchLengthParam, SqlDbType.Int, lowerBound.Length);
				cmd.AddParameter(validIdLengthParam, SqlDbType.Int, GetFullIdLengthFromSearchValue(lowerBound));
				cmd.AddParameter(lowerBoundParam, SqlDbType.VarChar, lowerBound);
				cmd.AddParameter(upperBoundParam, SqlDbType.VarChar, upperBound);
				return (int)cmd.ExecuteScalar();
			}
		}

		protected virtual string GetExclusiveLowerBoundSearchValue(long number) => Format(number - 1, FountainInfo.Prefix, FountainInfo.FormatDigits);

		protected virtual string GetExclusiveUpperBoundSearchValue(long number) => Format(number + 1, FountainInfo.Prefix, FountainInfo.FormatDigits);

		protected virtual int GetFullIdLengthFromSearchValue(string searchValue) => searchValue.Length;

		static string Format(long number, string prefix, int formatDigits)
		{
			return prefix + number.ToString(CultureInfo.InvariantCulture).PadLeft(formatDigits, '0');
		}
	}
}
