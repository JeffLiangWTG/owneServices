using System;
using CargoWise.Types;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Possible results for ranking the similarity of two dimension values, like two RefContainer PKs
	/// </summary>
	internal enum Similarity
	{
		/// <summary>
		/// Values are an exact match.
		/// </summary>
		Exact = 9,

		/// <summary>
		/// There's not an exact match, and the line value is a generic value that matches everything.
		/// </summary>
		Generic = 1,

		/// <summary>
		/// Line value matches indirectly or is even less specific than the generic value.
		/// 
		/// Used when the part list being compared doesn't have the attribute, but it does exist elsewhere on the job,
		/// and one of those other values does match the rate.
		/// The rate is still considered to be similar.
		///
		/// Also used for empty line values such as an empty commodity, since the GEN value has the higher, Generic similarity.
		/// </summary>
		Lowest = 0,

		/// <summary>
		/// The values do not match, e.g, the line value is not empty, and doesn't equal the job value.
		/// </summary>
		None = -1,
	}

	internal static class SimilarityHelper
	{
		public static Similarity GetGuidSimilarity(Guid? rate, Guid? job)
		{
			if (rate == job)
			{
				return Similarity.Exact;
			}
			else if (!rate.HasValue)
			{
				return Similarity.Generic;
			}
			else
			{
				return Similarity.None;
			}
		}

		public static Similarity GetGuidSimilarity(ZGuid rate, Guid? job)
			=> GetGuidSimilarity(NullableHelper.ToNullable(rate), job);

		public static Similarity GetStringSimilarity(string lineValue, string partValue)
		{
			return lineValue == ZString.Empty
						? Similarity.Lowest
						: lineValue == partValue
							? Similarity.Exact
							: Similarity.None;
		}
	}
}
