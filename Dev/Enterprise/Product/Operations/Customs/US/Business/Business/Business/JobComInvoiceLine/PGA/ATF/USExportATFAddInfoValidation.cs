using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USExportATFAddInfoValidation : USATFAddInfoValidation
	{
		public USExportATFAddInfoValidation(ATFAddInfo parent)
			: base(parent)
		{
		}

		new ATFAddInfo Parent
		{
			get { return (ATFAddInfo)base.Parent; }
		}

		protected override void CheckUS_FFLNumber()
		{
			base.CheckUS_FFLNumber();

			if (!Parent.US_FFLNumber.IsEmpty && !Parent.US_FFLExemptionCode.IsEmpty)
			{
				Parent.US_FFLNumberInfo.AddMessageError(EitherFFLNumberOrExemptionCodeCanBeSpecified);
			}

			ValidateUS_FFLExemptionCode();
		}

		protected override void CheckUS_FFLExemptionCode()
		{
			base.CheckUS_FFLExemptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FFLExemptionCodeInfo, Parent.Lookups.ExemptionCodesList);

			if (!Parent.US_FFLExemptionCode.IsEmpty && !Parent.US_FFLNumber.IsEmpty)
			{
				Parent.US_FFLExemptionCodeInfo.AddMessageError(EitherFFLNumberOrExemptionCodeCanBeSpecified);
			}

			ValidateUS_FFLNumber();
		}
		internal const string EitherFFLNumberOrExemptionCodeCanBeSpecified = "Either FFL Number or FFL Exemption Code can be specified, but not both.";

		protected override void CheckUS_PermitNumber()
		{
			base.CheckUS_PermitNumber();

			if (!Parent.US_PermitNumber.IsEmpty && !Parent.US_PermitExemptionCode.IsEmpty)
			{
				Parent.US_PermitNumberInfo.AddMessageError(EitherPermitNumberOrExemptionCodeCanBeSpecified);
			}

			ValidateUS_PermitExemptionCode();
		}

		protected override void CheckUS_PermitExemptionCode()
		{
			base.CheckUS_PermitExemptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PermitExemptionCodeInfo, Parent.Lookups.ExemptionCodesList);

			if (!Parent.US_PermitExemptionCode.IsEmpty && !Parent.US_PermitNumber.IsEmpty)
			{
				Parent.US_PermitExemptionCodeInfo.AddMessageError(EitherPermitNumberOrExemptionCodeCanBeSpecified);
			}

			ValidateUS_PermitNumber();
		}
		internal const string EitherPermitNumberOrExemptionCodeCanBeSpecified = "Either Permit Number or Permit Exemption Code can be specified, but not both.";

		protected override void CheckUS_Quantity()
		{
			base.CheckUS_Quantity();

			var invoiceLine = Parent.Parent?.InvoiceLine;
			if (invoiceLine != null && invoiceLine.IsATFDeclared)
			{
				if (Parent.US_Quantity.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_QuantityInfo);
				}
				else if (Parent.US_Quantity < 0m)
				{
					Parent.US_QuantityInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
				}
				else if (Parent.US_Quantity > 999999999m)
				{
					Parent.US_QuantityInfo.AddError(QuantityShouldBeLessThanOneBillion);
				}
			}
		}
		internal const string QuantityShouldBeLessThanOneBillion = "Permit Quantity should be less than 999,999,999. Please adjust value accordingly.";

		protected override void CheckUS_CategoryCode()
		{
			base.CheckUS_CategoryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CategoryCodeInfo, Parent.Lookups.CategoryCodeList);
		}
	}
}
