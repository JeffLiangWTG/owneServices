//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISLicenseAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISLicenseAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISLicenseAddInfoValidation : AutoUSAPHISLicenseAddInfoValidation
	{
		public USAPHISLicenseAddInfoValidation(AutoUSAPHISLicenseAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_RN_CountryCode()
		{
			base.CheckUS_RN_CountryCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_RN_CountryCodeInfo, Parent.Lookups.Countries);
			}
		}

		protected override void CheckUS_Date()
		{
			base.CheckUS_Date();
			if (IsPGAValidation)
			{
				if (Parent.US_Date.IsEmpty)
				{
					Parent.US_DateInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("License Date"));
				}
				else if (ZZCustomsFunctionality.IsAPHIS2024Effective)
				{
					if (Parent.US_DateQualifier == LPCODateQualifierList.Codes.ExpirationDate && Parent.US_Date.Date < ZDateTime.Today)
					{
						Parent.US_DateInfo.AddMessageError(ExpirationDateCannotBeInThePast);
					}
					else if (Parent.US_DateQualifier == LPCODateQualifierList.Codes.DateIssuedOrSigned && Parent.US_Date.Date > ZDateTime.Today.AddDays(10))
					{
						Parent.US_DateInfo.AddMessageError(DateIssuedCannotBeMoreThanTenDaysInTheFuture);
					}
				}
			}
		}
		internal const string ExpirationDateCannotBeInThePast = "Please note that the Expiration date cannot be in the past.";
		internal const string DateIssuedCannotBeMoreThanTenDaysInTheFuture = "Please note that the Date Issued or Signed cannot be more than 10 days in the future.";

		protected override void CheckUS_DateQualifier()
		{
			base.CheckUS_DateQualifier();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_DateQualifierInfo, Parent.Lookups.DateQualifiers);
			}
		}

		protected override void CheckUS_Type()
		{
			base.CheckUS_Type();
			var header = Header;
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TypeInfo, Parent.Lookups.LicenseTypes);
				if (Parent.US_Type == APHISLicenseTypeList.Codes.USCanadaGreenhouseGrownPlantExportCertificationLabel && header != null && !header.Sources.HasSourceCountryCA)
				{
					Parent.US_TypeInfo.AddMessageError(SourceCountryMustBeCA);
				}
			}
			if (header != null)
			{
				header.AddInfoValidation.ValidateUS_OA_PermittedAddress();
			}
		}
		internal const string SourceCountryMustBeCA = "For LPCO Type A37 (US/Canada Greenhouse Grown Plant Export Certification Label), the Source Country must be CA.";

		protected override void CheckUS_Quantity()
		{
			base.CheckUS_Quantity();
			if (IsPGAValidation)
			{
				if (Parent.US_Quantity.IsEmpty && !Parent.US_UnitOfMeasure.IsEmpty)
				{
					Parent.US_QuantityInfo.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity", "Unit Of Measure"));
				}
				ValidateUS_UnitOfMeasure();
			}
		}

		protected override void CheckUS_UnitOfMeasure()
		{
			base.CheckUS_UnitOfMeasure();
			if (IsPGAValidation)
			{
				if (Parent.US_UnitOfMeasure.IsEmpty)
				{
					if (!Parent.US_Quantity.IsEmpty)
					{
						Parent.US_UnitOfMeasureInfo.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Unit Of Measure", "Quantity"));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_UnitOfMeasureInfo, Parent.Lookups.UnitOfMeasureList);
				}
				ValidateUS_Quantity();
			}
		}

		protected override void CheckUS_Number()
		{
			base.CheckUS_Number();

			if (IsPGAValidation && Parent.US_Number.IsEmpty)
			{
				Parent.US_NumberInfo.AddMessageError(LicenseNumberOrNameRequired);
			}
		}
		internal const string LicenseNumberOrNameRequired = "License Number required. If not known, name of form is acceptable.";

		protected new USAPHISLicenseAddInfo Parent
		{
			get { return (USAPHISLicenseAddInfo)base.Parent; }
		}

		protected APHISLicense License
		{
			get { return Parent.Parent; }
		}

		protected APHISHeader Header
		{
			get
			{
				var license = License;
				return license == null ? null : license.Header;
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var aphisHeader = Header;
				if (aphisHeader != null)
				{
					var invoiceLine = aphisHeader.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
