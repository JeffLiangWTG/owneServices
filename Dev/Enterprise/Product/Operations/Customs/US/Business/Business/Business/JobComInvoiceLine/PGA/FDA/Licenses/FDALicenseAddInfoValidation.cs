using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class FDALicenseAddInfoValidation : AutoFDALicenseAddInfoValidation
	{
		public FDALicenseAddInfoValidation(AutoFDALicenseAddInfo parent) : base(parent)
		{
		}

		protected new FDALicenseAddInfo Parent
		{
			get { return (FDALicenseAddInfo)base.Parent; }
		}

		protected FDALicense License
		{
			get { return Parent.Parent; }
		}

		protected override void CheckUS_StateCode()
		{
			base.CheckUS_StateCode();
			if (IsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_StateCodeInfo, Parent.Lookups.StateList);
			}
		}

		protected override void CheckUS_CountryCode()
		{
			base.CheckUS_CountryCode();
			if (IsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CountryCodeInfo, Parent.Lookups.Countries);
				ValidateUS_StateCode();
			}
		}

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var result = false;
				var license = License;
				if (license != null)
				{
					var fda = license.Parent as ACEFDA;
					var invoiceLine = fda != null ? fda.InvoiceLine : null;
					result = invoiceLine != null && invoiceLine.IsACECargoReleaseValidationMode;
				}
				return result;
			}
		}
	}
}
