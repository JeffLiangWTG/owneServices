//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSAPermitAndLicenseAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNHTSAPermitAndLicenseAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USNHTSAPermitAndLicenseAddInfoValidation : AutoUSNHTSAPermitAndLicenseAddInfoValidation
	{
		public USNHTSAPermitAndLicenseAddInfoValidation(AutoUSNHTSAPermitAndLicenseAddInfo parent) : base(parent)
		{
		}

		new USNHTSAPermitAndLicenseAddInfo Parent
		{
			get { return (USNHTSAPermitAndLicenseAddInfo)base.Parent; }
		}

		NHTSAPermitAndLicenses PermitAndLicense
		{
			get { return Parent.PermitAndLicenses; }
		}

		protected override void CheckUS_NHTLPCOType()
		{
			base.CheckUS_NHTLPCOType();
			IdentityNumberAndLPCODetailsValidator.ValidateLPCOType(Parent.US_NHTLPCOTypeInfo, Parent.Lookups.LPCOTypes, Parent.US_NHTLPCONumber, Parent.US_NHTLPCODateType, Parent.US_NHTLPCODate, Parent.US_NHTLPCOQuantity);
			ValidateUS_NHTLPCONumber();

			if (PermitAndLicense.Details != null && PermitAndLicense.Details.Header != null)
			{
				PermitAndLicense.Details.Header.AddInfo.Validation.ValidateUS_NHTBoxNumber();
			}
		}

		protected override void CheckUS_NHTLPCONumber()
		{
			base.CheckUS_NHTLPCONumber();

			var details = PermitAndLicense.Details;
			var isPGAValidation = details != null && details.Header != null && details.Header.IsPGAValidationOn;
			IdentityNumberAndLPCODetailsValidator.ValidateLPCONumber(Parent.US_NHTLPCONumberInfo, isPGAValidation, Parent.US_NHTLPCOType, PermitAndLicense.IsRegisteredImporterNumber, PermitAndLicense.IsNHTSAImportPermissionLetter, PermitAndLicense.IsVehicleEligibilityNumber);
		}

		protected override void CheckUS_NHTLPCODateType()
		{
			base.CheckUS_NHTLPCODateType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTLPCODateTypeInfo, Parent.Lookups.LPCODateTypes);
			IdentityNumberAndLPCODetailsValidator.ValidateLPCODateType(Parent.US_NHTLPCODateTypeInfo, Parent.US_NHTLPCODate);

			ValidateUS_NHTLPCODate();
		}

		protected override void CheckUS_NHTLPCODate()
		{
			base.CheckUS_NHTLPCODate();
			IdentityNumberAndLPCODetailsValidator.ValidateLPCODate(Parent.US_NHTLPCODateInfo, Parent.US_NHTLPCODateType);

			ValidateUS_NHTLPCODateType();
		}
	}
}
