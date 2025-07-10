using System;
using CargoWise.Common;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// An IRateablePart along with an IHasPartDimensions indicating what dimensions it has.
	/// 
	/// Implements IEquatable - instances of this class are equal if they have the same part.
	/// </summary>
	public struct PartWithDimensions : IEquatable<PartWithDimensions>
	{
		public PartWithDimensions(IHasPartDimensions hasDimensions, IRateablePart part)
		{
			Argument.NotNull(hasDimensions, nameof(hasDimensions));
			Argument.NotNull(part, nameof(part));

			HasDimensions = hasDimensions;
			Part = part;
		}

		public IHasPartDimensions HasDimensions { get; }
		public IRateablePart Part { get; }

		public bool Equals(PartWithDimensions other)
			=> ReferenceEquals(Part, other.Part);
	}
}

