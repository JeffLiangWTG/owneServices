using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusVehicleValidation : EU.Business.CusVehicleValidation
	{
		public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
		{
		}

		public new CusVehicle Parent => (CusVehicle)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBrandValue();
			ValidateBrandValueInTRY();
		}

		protected override void CheckCVH_RegistrationNumber()
		{
			base.CheckCVH_RegistrationNumber();
			if (!Parent.CVH_RegistrationNumber.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.CVH_RegistrationNumberInfo.AddMessageError(Res.GetString("000FCCF3-10C3-4D93-889A-0469CAC38EFD", "Registration No should consist of alphanumeric characters."));
			}
		}

		protected override void CheckCVH_SerialNumber()
		{
			base.CheckCVH_SerialNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_SerialNumberInfo);
		}

		protected override void CheckCVH_ModelYear()
		{
			base.CheckCVH_ModelYear();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_ModelYearInfo);

			if (!Regex.IsMatch(Parent.CVH_ModelYear, @"^[12][0-9]{3}$", RegexOptions.IgnoreCase))
			{
				Parent.CVH_ModelYearInfo.AddMessageError(Res.GetString("48186526-1f5e-48d4-9449-1b8fec5ffe12", "Please enter a valid Model Year."));
			}
		}

		protected override void CheckCVH_ModelName()
		{
			base.CheckCVH_ModelName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_ModelNameInfo);
		}

		protected override void CheckCVH_Color()
		{
			base.CheckCVH_Color();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_ColorInfo);
		}

		protected override void CheckCVH_VehicleIdentificationNumber()
		{
			base.CheckCVH_VehicleIdentificationNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_VehicleIdentificationNumberInfo);

			var vin = Parent.CVH_VehicleIdentificationNumber;
			if (vin.Length != 17 || !vin.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.CVH_VehicleIdentificationNumberInfo.AddMessageError(Res.GetString("044880FE-EE23-45AE-B0B4-E6DBBBD33C64", "VIN should be 17 alphanumeric characters."));
			}

			var invoiceHeader = Parent?.InvoiceLine?.InvoiceHeader;
			if (invoiceHeader != null)
			{
				var parentPK = Parent.PK;
				foreach (JobComInvoiceLine line in invoiceHeader.InvoiceLines)
				{
					if (line.Vehicles.Cast<CusVehicle>().Any(x => x.CVH_VehicleIdentificationNumber == vin && x.PK != parentPK))
					{
						Parent.CVH_VehicleIdentificationNumberInfo.AddMessageError(Res.GetString("00BCC643-5451-49D6-AA57-1AF816B388A0", "This VIN Number already exists in Invoice."));
					}
				}
			}
		}

		public void ValidateBrandValue()
		{
			ValidateCalculatedProperty(Parent.BrandValueInfo);
		}

		protected void CheckBrandValue()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BrandValueInfo);
		}

		public void ValidateBrandValueInTRY()
		{
			ValidateCalculatedProperty(Parent.BrandValueInTRYInfo);
		}

		protected void CheckBrandValueInTRY()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BrandValueInTRYInfo);
		}

		protected override void CheckCVH_BrandName()
		{
			base.CheckCVH_BrandName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_BrandNameInfo);
		}
	}
}
