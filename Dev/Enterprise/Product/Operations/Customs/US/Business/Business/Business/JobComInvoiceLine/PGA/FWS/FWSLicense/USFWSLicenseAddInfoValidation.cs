//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFWSLicenseAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSFWSLicenseAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;

	public class USFWSLicenseAddInfoValidation : AutoUSFWSLicenseAddInfoValidation
	{
		public USFWSLicenseAddInfoValidation(AutoUSFWSLicenseAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Number()
		{
			base.CheckUS_Number();
			var parent = Parent;
			if (parent.US_Number.IsEmpty && !parent.US_Type.IsEmpty && Header?.Parent != null)
			{
				parent.US_NumberInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("License Number"));
			}
			else if (parent.US_Type == FWSLicenseTypeList.Codes.FWSeDecsConfirmationNumber && (!parent.US_Number.IsLettersAndNumbersOnlyOrEmpty || parent.US_Number.Length > 18))
			{
				parent.US_NumberInfo.AddMessageError(FWCMaximumLength);
			}
		}
		internal const string FWCMaximumLength = "FWC licence numbers are limited to 18 alphanumic characters maximum.";

		protected override void CheckUS_Type()
		{
			base.CheckUS_Type();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.US_TypeInfo, parent.Lookups.LicenseTypes);

			var fwsHeader = Header;
			if (fwsHeader != null && FWSProcessingCodeList.IsEDS(fwsHeader.US_ProcessingCode) && parent.US_Type == FWSLicenseTypeList.Codes.FWSeDecsConfirmationNumber)
			{
				parent.US_TypeInfo.AddMessageError(ValidationConstants.FWS.FWCShouldOnlyBeEnteredWhenProcessingCodeIsLDS);
			}
		}

		protected new USFWSLicenseAddInfo Parent
		{
			get { return (USFWSLicenseAddInfo)base.Parent; }
		}

		protected FWSLicense License
		{
			get { return Parent.Parent; }
		}

		protected FWSHeader Header
		{
			get
			{
				var license = License;
				return license == null ? null : license.Header;
			}
		}
	}
}
