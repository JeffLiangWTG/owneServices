using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount
{
	internal sealed class DescriptionDocAmount : DocAmount
	{
		internal DescriptionDocAmount(MultilingualString description)
		{
			Description = description;
		}

		public override MultilingualString AmountAsString => Description;

		MultilingualString Description { get; }
	}
}
