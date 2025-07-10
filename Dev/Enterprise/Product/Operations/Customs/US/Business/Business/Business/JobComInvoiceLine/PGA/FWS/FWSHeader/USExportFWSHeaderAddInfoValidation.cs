using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USExportFWSHeaderAddInfoValidation : USFWSHeaderAddInfoValidation
	{
		public USExportFWSHeaderAddInfoValidation(USFWSHeaderAddInfo parent)
			: base(parent)
		{
		}

		new USFWSHeaderAddInfo Parent
		{
			get { return (USFWSHeaderAddInfo)base.Parent; }
		}

		protected override void CheckUS_ConfirmationNum()
		{
			base.CheckUS_ConfirmationNum();

			if (IsFWSIndicatorDeclared)
			{
				if (!Parent.US_ConfirmationNum.IsEmpty && !IsConfirmationNumberValidFormat)
				{
					Parent.US_ConfirmationNumInfo.AddMessageError(ConfirmationNumberInvalidFormat);
				}
			}
		}
		internal const string ConfirmationNumberInvalidFormat = "Confirmation Number should be in the following format:CCYYAANNNNNNN where CC is century, YY is year, A is alphabetic and N is a number";

		protected override void CheckUS_TaxonomicSerialNumber()
		{
			base.CheckUS_TaxonomicSerialNumber();

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_TaxonomicSerialNumberInfo);
			}
		}

		protected override void CheckUS_PurposeCode()
		{
			base.CheckUS_PurposeCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PurposeCodeInfo, Parent.Lookups.PurposeCodeList);

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_PurposeCodeInfo);
			}
		}

		protected override void CheckUS_WildlifeDescriptionCode()
		{
			base.CheckUS_WildlifeDescriptionCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_WildlifeDescriptionCodeInfo, Parent.Lookups.WildlifeDescriptionCodes);

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_WildlifeDescriptionCodeInfo);
			}
		}

		protected override void CheckUS_SpeciesOrigin()
		{
			base.CheckUS_SpeciesOrigin();

			if (!Parent.US_SpeciesOrigin.EqualsIgnoringCase("ZZ"))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_SpeciesOriginInfo, Parent.Lookups.SpeciesOrigins);
			}

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_SpeciesOriginInfo);
			}
		}

		protected override void CheckUS_WildlifeSource()
		{
			base.CheckUS_WildlifeSource();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_WildlifeSourceInfo, Parent.Lookups.WildlifeSources);

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_WildlifeSourceInfo);
			}
		}

		protected override void CheckUS_CertificationCode()
		{
			base.CheckUS_CertificationCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertificationCodeInfo, Parent.Lookups.CertificationCodeList);

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_CertificationCodeInfo);
			}
		}

		protected override void CheckUS_WildlifeCategoryCode()
		{
			base.CheckUS_WildlifeCategoryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_WildlifeCategoryCodeInfo, Parent.Lookups.WildlifeCategoryCodes);

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_WildlifeCategoryCodeInfo);
			}
		}

		protected override void CheckUS_USState()
		{
			base.CheckUS_USState();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_USStateInfo, Parent.Lookups.GetCachedUSStateList);

			if (IsFWSIndicatorDeclared)
			{
				CheckIfValureIsRequired(Parent.US_USStateInfo);
			}
		}

		void CheckIfValureIsRequired(ZPropertyInfo propertyInfo)
		{
			if (!Parent.US_ConfirmationNum.IsEmpty && !propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddMessageError(ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);
			}
			else if (Parent.US_ConfirmationNum.IsEmpty && propertyInfo.Value.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}
		internal const string ValueShouldNotBeEnteredIfConfirmationNumberIsEntered = "Value is not required if confirmation number is entered.";

		ZBool IsFWSIndicatorDeclared
		{
			get
			{
				var invoiceLine = Parent?.Parent?.InvoiceLine;
				var pivot = Parent?.Parent?.Pivot;
				return (invoiceLine != null && invoiceLine.IsFWSDeclared) || (pivot != null && pivot.IsFWSDeclared);
			}
		}

		ZBool IsConfirmationNumberValidFormat
		{
			get
			{
				return Parent.US_ConfirmationNum.Length == 13
					&& Parent.US_ConfirmationNum.SubstringSafe(0, 4).IsNumbersOnlyOrEmpty
					&& Parent.US_ConfirmationNum.SubstringSafe(4, 2).IsLettersOnlyOrEmpty
					&& Parent.US_ConfirmationNum.SubstringSafe(6).IsNumbersOnlyOrEmpty;
			}
		}
	}
}
