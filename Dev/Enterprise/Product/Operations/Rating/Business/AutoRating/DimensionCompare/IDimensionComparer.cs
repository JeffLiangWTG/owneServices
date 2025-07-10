using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Interface for comparing a dimension (attribute) of a job with the corresponding value on a rate.
	/// E.g, job container type vs entry container type.
	/// </summary>
	internal interface IDimensionComparer
	{
		Similarity GetSimilarity(
			FastLine line,
			IDistinctPartRateDimensions measures,
			IHasPartDimensions partList,
			IRateablePart part,
			IPointMatchingLog pointMatchingLog = null);

		/// <summary>
		/// Returns true if the given part list has the dimension that can be compared.
		/// </summary>
		bool HasDimension(IHasPartDimensions partList);

		/// <summary>
		/// Returns true if all the dimension values on the job
		/// that are similar to line1 are also similar to line2.
		/// E.g., if all the container types that match line1 also match line 2.
		/// This means that the lines apply to the same parts.
		/// 
		/// One use case for this is when two rate entries with different container types
		/// have the same charge code and flat calculator, such as a document fee.
		/// The measure type for a flat charge is Unidentified so the charges will match at the job level and be calculated simultaneously.
		/// Since they have different container types they are not compatible for simultaneous usage.
		/// They will have to be the same amount to be rated without error (only one amount is used).
		/// If they had different amounts the rates conflict.
		///
		/// By contrast, consider two rates per-container, one for 20GP and one for 40GP.
		/// The measure type is ContainerType and so the rates will match different container parts.
		/// They will not be calculated together and this method will not be called.
		/// </summary>
		bool AreRatelinesCompatibleForSimultaneousUsage(RateableMeasureSet measures, FastLine line1, FastLine line2);
	}
}
