using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount
{
	[Immutable]
	public abstract class DocAmount
	{
		#region Factory Methods

		public static DocAmount Create(ZDecimal amountAsDecimal, int decimals)
			=> new DecimalDigitDocAmount(amountAsDecimal, decimals);

		public static DocAmount Create(ZDecimal amountAsDecimal, int decimals, string amountDescription)
			=> new AmountWithDescriptionDocAmount(amountAsDecimal, decimals, amountDescription);

		public static DocAmount Create(MultilingualString amountAsString)
			=> new DescriptionDocAmount(amountAsString);

		public static DocAmount Create(ZDecimal amountAsDecimal, QuotationLineType type)
			=> new QuotationLineTypeDocAmount(amountAsDecimal, type);

		public static DocAmount[] Create(int count)
		{
			var docAmounts = new DocAmount[count];
			for (var i = 0; i < docAmounts.Length; i++)
			{
				docAmounts[i] = DocAmount.Empty;
			}
			return docAmounts;
		}

		#endregion

		public static readonly DocAmount Empty = new EmptyDocAmount();

		public static void Clear(DocAmount[] docAmounts, int index, int length)
		{
			for (var i = index; i < length; i++)
			{
				docAmounts[i] = DocAmount.Empty;
			}
		}

		public static bool IsEmpty(DocAmount docAmount) => docAmount is EmptyDocAmount;

		public abstract MultilingualString AmountAsString { get; }
	}
}
