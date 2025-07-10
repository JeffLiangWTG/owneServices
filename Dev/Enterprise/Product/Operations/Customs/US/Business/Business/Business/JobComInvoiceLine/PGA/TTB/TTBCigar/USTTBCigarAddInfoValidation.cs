//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSTTBCigarAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSTTBCigarAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.Types;

	public class USTTBCigarAddInfoValidation : AutoUSTTBCigarAddInfoValidation
	{
		public USTTBCigarAddInfoValidation(AutoUSTTBCigarAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Quantity()
		{
			base.CheckUS_Quantity();
			if (IsTobaccoPGAValidation)
			{
				if (Parent.US_Quantity <= ZInt.Zero)
				{
					Parent.US_QuantityInfo.AddMessageError(ValidationConstants.TTB.QuantityMustBeGreaterThanZero);
				}
				else if (Parent.US_Quantity > 999999)
				{
					Parent.US_QuantityInfo.AddMessageError(ValidationConstants.TTB.QuantityCannotBeGreaterThan999999);
				}
			}
		}

		protected override void CheckUS_UnitPrice()
		{
			base.CheckUS_UnitPrice();
			if (!Parent.US_IsSmall && IsTobaccoPGAValidation)
			{
				if (Parent.US_UnitPrice <= ZDecimal.Zero)
				{
					Parent.US_UnitPriceInfo.AddMessageError(ValidationConstants.TTB.UnitPriceMustBeGreaterThanZero);
				}
				else if (Parent.US_UnitPrice > TTBCigar.MaximuSalePrice)
				{
					Parent.US_UnitPriceInfo.AddMessageError(ValidationConstants.TTB.UnitPriceMaximumSalePrice);
				}
			}
		}

		protected override void CheckUS_IsSmall()
		{
			base.CheckUS_IsSmall();
			ValidateUS_UnitPrice();
		}

		protected new USTTBCigarAddInfo Parent
		{
			get { return (USTTBCigarAddInfo)base.Parent; }
		}

		protected TTBCigar Cigar
		{
			get { return Parent.Parent; }
		}

		protected TTBLine TTBLine
		{
			get
			{
				var cigar = Cigar;
				return cigar == null ? null : cigar.Parent;
			}
		}

		bool IsTobaccoPGAValidation
		{
			get
			{
				var result = false;

				var ttbLine = TTBLine;
				if (ttbLine != null && ttbLine.IsTobaccoProgramTypeAndNotPaperOrTube)
				{
					var invoiceLine = ttbLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
