using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USImportATFAddInfoValidation : USATFAddInfoValidation
	{
		public USImportATFAddInfoValidation(ATFAddInfo parent)
			: base(parent)
		{
		}

		protected ATF Product
		{
			get { return (ATF)Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var product = Product;
				var invoiceLine = product == null ? null : product.InvoiceLine;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		protected override void CheckUS_Quantity()
		{
			base.CheckUS_Quantity();
			if (IsPGAValidation)
			{
				if (Parent.US_Quantity.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_QuantityInfo);
				}
				else if (Parent.US_Quantity < 0)
				{
					Parent.US_QuantityInfo.AddMessageError(EnterNumberGreaterThanZero);
				}
			}
		}
		internal const string EnterNumberGreaterThanZero = "Please enter a number greater than 0.";

		protected override void CheckUS_CategoryCode()
		{
			base.CheckUS_CategoryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CategoryCodeInfo, Parent.Lookups.CategoryCodeList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CategoryCodeInfo);
			}
			ValidateUS_FFLNumber();
			ValidateUS_FFLExemptionCode();
			ValidateUS_FELNumber();
			ValidateUS_FELExemptionCode();
			ValidateUS_PermitNumber();
			ValidateUS_PermitExemptionCode();
			ValidateUS_AECANumber();
			ValidateUS_AECAExemptionCode();
		}

		protected override void CheckUS_FFLNumber()
		{
			base.CheckUS_FFLNumber();
			if (ATFCategoryCodeList.IsFFLRequired(Parent.US_CategoryCode))
			{
				if (Parent.US_FFLExemptionCode.IsEmpty && IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FFLNumberInfo);
				}

				if (!Parent.US_FFLNumber.IsEmpty)
				{
					if (!Regex.IsMatch(Parent.US_FFLNumber, @"^[0-9]{1}-[0-9]{2}-[0-9]{3}-[0-9]{2}-[0-9]{1}[a-z]{1}-[0-9]{5}$", RegexOptions.IgnoreCase) &&
						!Regex.IsMatch(Parent.US_FFLNumber, @"^[0-9]{1}[0-9]{2}[0-9]{3}[0-9]{2}[0-9]{1}[a-z]{1}[0-9]{5}$", RegexOptions.IgnoreCase))
					{
						Parent.US_FFLNumberInfo.AddMessageError(FFLNumberFormat);
					}
				}
			}
			else if (!Parent.US_CategoryCode.IsEmpty && !Parent.US_FFLNumber.IsEmpty)
			{
				Parent.US_FFLNumberInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}
		internal const string NotRequiredDataByCategoryCode = "The selected category code does not require this data.";
		internal const string FFLNumberFormat = "FFL number format is incorrect.  Format should be N-NN-NNN-NN-NA-NNNNN or NNNNNNNNNANNNNN.";

		protected override void CheckUS_FFLExemptionCode()
		{
			base.CheckUS_FFLExemptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FFLExemptionCodeInfo, Parent.Lookups.ExemptionCodesList);

			if (!ATFCategoryCodeList.IsFFLRequired(Parent.US_CategoryCode) && !Parent.US_CategoryCode.IsEmpty && !Parent.US_FFLExemptionCode.IsEmpty)
			{
				Parent.US_FFLExemptionCodeInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}

		protected override void CheckUS_FELNumber()
		{
			base.CheckUS_FELNumber();
			if (ATFCategoryCodeList.IsFELRequired(Parent.US_CategoryCode))
			{
				if (Parent.US_FELExemptionCode.IsEmpty && IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FELNumberInfo);
				}
			}
			else if (!Parent.US_CategoryCode.IsEmpty && !Parent.US_FELNumber.IsEmpty)
			{
				Parent.US_FELNumberInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}

		protected override void CheckUS_FELExemptionCode()
		{
			base.CheckUS_FELExemptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FELExemptionCodeInfo, Parent.Lookups.ExemptionCodesList);

			if (!ATFCategoryCodeList.IsFELRequired(Parent.US_CategoryCode) && !Parent.US_CategoryCode.IsEmpty && !Parent.US_FELExemptionCode.IsEmpty)
			{
				Parent.US_FELExemptionCodeInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}

		protected override void CheckUS_PermitNumber()
		{
			base.CheckUS_PermitNumber();
			if (ATFCategoryCodeList.IsPermitRequired(Parent.US_CategoryCode))
			{
				if (Parent.US_PermitExemptionCode.IsEmpty && IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PermitNumberInfo);
				}

				if (!Parent.US_PermitNumber.IsEmpty)
				{
					if (!Regex.IsMatch(Parent.US_PermitNumber, @"^[0-9]{4}-[0-9]{5}$", RegexOptions.IgnoreCase) &&
						!Regex.IsMatch(Parent.US_PermitNumber, @"^[0-9]{4}[0-9]{5}$", RegexOptions.IgnoreCase))
					{
						Parent.US_PermitNumberInfo.AddMessageError(PermitNumberFormat);
					}
				}
			}
			else if (!Parent.US_CategoryCode.IsEmpty && !Parent.US_PermitNumber.IsEmpty)
			{
				Parent.US_PermitNumberInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}
		internal const string PermitNumberFormat = "Permit Number format is incorrect. Format should start with a complete year (YYYY). Format is YYYY-NNNNN or YYYYNNNNN.";

		protected override void CheckUS_PermitExemptionCode()
		{
			base.CheckUS_PermitExemptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PermitExemptionCodeInfo, Parent.Lookups.ExemptionCodesList);

			if (!ATFCategoryCodeList.IsPermitRequired(Parent.US_CategoryCode) && !Parent.US_CategoryCode.IsEmpty && !Parent.US_PermitExemptionCode.IsEmpty)
			{
				Parent.US_PermitExemptionCodeInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}

		protected override void CheckUS_AECANumber()
		{
			base.CheckUS_AECANumber();
			if (ATFCategoryCodeList.IsAECARequired(Parent.US_CategoryCode))
			{
				if (Parent.US_AECAExemptionCode.IsEmpty && IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AECANumberInfo);
				}

				if (!Parent.US_AECANumber.IsEmpty)
				{
					if (!Regex.IsMatch(Parent.US_AECANumber, @"^A-[0-9]{2}-[0-9]{3}-[0-9]{4}$", RegexOptions.IgnoreCase) &&
						!Regex.IsMatch(Parent.US_AECANumber, @"^A[0-9]{2}[0-9]{3}[0-9]{4}$", RegexOptions.IgnoreCase))
					{
						Parent.US_AECANumberInfo.AddMessageError(AECANumberFormat);
					}
				}
			}
			else if (!Parent.US_CategoryCode.IsEmpty && !Parent.US_AECANumber.IsEmpty)
			{
				Parent.US_AECANumberInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}
		internal const string AECANumberFormat = "AECA Number format is incorrect. AECA Numbers must start with the letter 'A'. Format is A-NN-NNN-NNNN or ANNNNNNNNN.";

		protected override void CheckUS_AECAExemptionCode()
		{
			base.CheckUS_AECAExemptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_AECAExemptionCodeInfo, Parent.Lookups.ExemptionCodesList);

			if (!ATFCategoryCodeList.IsAECARequired(Parent.US_CategoryCode) && !Parent.US_CategoryCode.IsEmpty && !Parent.US_AECAExemptionCode.IsEmpty)
			{
				Parent.US_AECAExemptionCodeInfo.AddMessageError(NotRequiredDataByCategoryCode);
			}
		}
	}
}
