using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USImportDEAHeaderAddInfoValidation : USDEAHeaderAddInfoValidation
	{
		public USImportDEAHeaderAddInfoValidation(DEAHeaderAddInfo parent)
			: base(parent)
		{
		}

		DEAHeader Header
		{
			get { return (DEAHeader)Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var declaration = Header?.InvoiceLine?.Declaration;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		protected override void CheckUS_CountryOfShipment()
		{
			base.CheckUS_CountryOfShipment();

			if (!Parent.US_CountryOfShipment.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_CountryOfShipmentInfo, Header.AddInfoLookups.Countries);
			}
			else if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CountryOfShipmentInfo);
			}
		}

		protected override void CheckUS_PermitNumber()
		{
			base.CheckUS_PermitNumber();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PermitNumberInfo);
				var permitNumber = Parent.US_PermitNumber.KeepAlphanumericCharacters();
				if (!permitNumber.IsEmpty && permitNumber.Length != 7)
				{
					Parent.US_PermitNumberInfo.AddMessageError(PermitNumberFormat);
				}
			}
		}
		internal const string PermitNumberFormat = "Registration Number should be a 7 alpha numeric number (do not include the dash and amendment number).";

		protected override void CheckUS_RegistrationNumber()
		{
			base.CheckUS_RegistrationNumber();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RegistrationNumberInfo);
				var registrationNumber = Parent.US_RegistrationNumber;
				if (!registrationNumber.IsEmpty && (registrationNumber.Length != 9 || !registrationNumber.IsLettersAndNumbersOnlyOrEmpty))
				{
					Parent.US_RegistrationNumberInfo.AddMessageError(RegistrationNumberFormat);
				}
			}
		}
		internal const string RegistrationNumberFormat = "Registration Number should be a 9 alpha numeric number.";

		protected override void CheckUS_FormID()
		{
			base.CheckUS_FormID();

			if (!Parent.US_FormID.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_FormIDInfo, Header.AddInfoLookups.FormTypes);
			}
			else if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FormIDInfo);
			}
		}
	}
}
