//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFSISLineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSFSISLineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USFSISLineAddInfoValidation : AutoUSFSISLineAddInfoValidation
	{
		public USFSISLineAddInfoValidation(AutoUSFSISLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_HealthCertificateNumber()
		{
			base.CheckUS_HealthCertificateNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_HealthCertificateNumberInfo);
		}

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UC_NKCountryOfOriginInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UC_NKCountryOfOriginInfo, Parent.Lookups.USCountries);
		}

		protected override void CheckUS_UC_NKCertificateIssuerCountry()
		{
			base.CheckUS_UC_NKCertificateIssuerCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UC_NKCertificateIssuerCountryInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UC_NKCertificateIssuerCountryInfo, Parent.Lookups.USCountries);
		}

		protected override void CheckUS_ImportingEstNo()
		{
			base.CheckUS_ImportingEstNo();
			var estNoInfo = Parent.US_ImportingEstNoInfo;
			USFSISLineAddInfoValidationHelper.CheckEstablishNumbers(estNoInfo, Parent.US_ImportingEstNo);
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ImportingEstNoInfo, Parent.Lookups.ImportEstablishments);
		}

		protected override void CheckUS_CommercialDescription()
		{
			base.CheckUS_CommercialDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CommercialDescriptionInfo);
		}

		protected override void CheckUS_ExportingEstNo()
		{
			base.CheckUS_ExportingEstNo();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ExportingEstNoInfo);
			}
			USFSISLineAddInfoValidationHelper.CheckEstablishNumbers(Parent.US_ExportingEstNoInfo, Parent.US_ExportingEstNo);
		}

		protected override void CheckUS_ProductID()
		{
			base.CheckUS_ProductID();
			if (!IsElectronicallyCertificated)
			{
				if (!Parent.US_ProductIDQualifier.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductIDInfo);
				}
			}
		}

		protected override void CheckUS_ProductIDQualifier()
		{
			base.CheckUS_ProductIDQualifier();
			if (!IsElectronicallyCertificated)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductIDQualifierInfo, Parent.Lookups.ProductIDQualifiers);
			}
		}

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();
			if (!IsElectronicallyCertificated)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, Parent.Lookups.ACEIntendedUseBaseCodes);
			}
		}

		protected override void CheckUS_DateOfInspection()
		{
			base.CheckUS_DateOfInspection();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DateOfInspectionInfo, "Date Of Inspection");
		}

		protected virtual bool IsPGAValidationOn
		{
			get
			{
				var result = false;
				var line = Line;
				if (line != null)
				{
					var invoiceLine = (JobComInvoiceLine)line.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		protected override void CheckUS_CertifyingIndividual()
		{
			base.CheckUS_CertifyingIndividual();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertifyingIndividualInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertifyingIndividualInfo, Parent.Lookups.FSISCertifyingIndividualList);

			ValidateUS_PGAContactName();
			ValidateUS_PGAContactPhoneNo();
			ValidateUS_PGAContactEmail();
		}

		protected override void CheckUS_PGAContactName()
		{
			base.CheckUS_PGAContactName();
			if (IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactNameInfo);
			}
		}

		protected override void CheckUS_PGAContactPhoneNo()
		{
			base.CheckUS_PGAContactPhoneNo();
			if (IsPGAValidationOn)
			{
				if (Parent.US_PGAContactPhoneNo.IsEmpty)
				{
					Parent.US_PGAContactPhoneNoInfo.AddMessageError(DomesticPhoneNoValidator.DomesticPhoneNoFormat);
				}
				else if (!Parent.US_PGAContactPhoneNo.IsEmpty)
				{
					ZString messageError = DomesticPhoneNoValidator.Validate(Parent.US_PGAContactPhoneNo);
					if (!messageError.IsEmpty)
					{
						Parent.US_PGAContactPhoneNoInfo.AddMessageError(messageError);
					}
				}
			}
		}

		protected override void CheckUS_PGAContactEmail()
		{
			base.CheckUS_PGAContactEmail();

			if (IsPGAValidationOn)
			{
				if (!Parent.US_PGAContactEmail.IsEmpty)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_PGAContactEmail))
					{
						Parent.US_PGAContactEmailInfo.AddWarning("Invalid email format");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PGAContactEmailInfo);
				}
			}
		}

		new internal USFSISLineAddInfo Parent => (USFSISLineAddInfo)base.Parent;

		USFSISLine Line => Parent.Parent;

		bool IsElectronicallyCertificated => Line.IsElectronicallyCertificated;
	}
}
