//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDEAConstituentAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDEAConstituentAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;

	public class USDEAConstituentAddInfoValidation : AutoUSDEAConstituentAddInfoValidation
	{
		public USDEAConstituentAddInfoValidation(AutoUSDEAConstituentAddInfo parent) : base(parent)
		{
		}

		DEAConstituent Constituent
		{
			get { return (DEAConstituent)Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var declaration = Constituent?.Header?.InvoiceLine?.Declaration;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		bool IsExport
		{
			get
			{
				var invoiceLine = Constituent?.Header?.InvoiceLine;
				return invoiceLine != null && invoiceLine.IsExport;
			}
		}

		protected override void CheckUS_ProductCode()
		{
			base.CheckUS_ProductCode();
			if (IsPGAValidation || IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductCodeInfo);

				if (IsPGAValidation)
				{
					var productCode = Parent.US_ProductCode;
					if (!productCode.IsEmpty && (productCode.Length != 4 || !productCode.IsNumbersOnlyOrEmpty))
					{
						Parent.US_ProductCodeInfo.AddMessageError(ProductCodeFormat);
					}
				}
			}
		}
		internal const string ProductCodeFormat = "Product Code Number should be 4 digits.";

		protected override void CheckUS_Weight()
		{
			base.CheckUS_Weight();
			if (IsPGAValidation || IsExport)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.US_WeightInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.US_WeightInfo);

				if (IsPGAValidation && Parent.US_Weight > 9999999999.99m)
				{
					Parent.US_WeightInfo.AddMessageError(WeightShouldBeLessThanTenBillion);
				}
			}
		}
		internal const string WeightShouldBeLessThanTenBillion = "Weight should be less than 999,999,999.99. Please adjust value accordingly, otherwise Zero will be sent in message.";

		protected override void CheckUS_WeightUQ()
		{
			base.CheckUS_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_WeightUQInfo, Parent.Lookups.WeightUQList);

			if (IsPGAValidation || IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_WeightUQInfo);
			}
		}
	}
}
