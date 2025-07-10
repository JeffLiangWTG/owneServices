using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class CarrierCompanyProvider : ICarrierCompany
	{
		public CarrierCompanyProvider(OrgAddress carrierAddress, ZDateTime effectiveDate)
		{
			this.carrierAddress = carrierAddress;
			this.carrierCompany = carrierAddress.Header;
			this.effectiveDate = effectiveDate;
		}
		readonly OrgAddress carrierAddress;
		readonly OrgHeader carrierCompany;
		readonly ZDateTime effectiveDate;

		public ZString CarrierName => carrierCompany != null ? carrierCompany.OH_FullName : ZString.Empty;
		public ZString StreetNo
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (carrierAddress != null)
				{
					returnValue = carrierAddress.Address1 + " " + carrierAddress.Address2;
				}
				return returnValue;
			}
		}
		public ZString Fax
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (carrierAddress != null)
				{
					returnValue = carrierAddress.FaxNumber.ToString();
				}
				return returnValue;
			}
		}
		public ZString ProvinceDistrict
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (carrierAddress != null)
				{
					returnValue = carrierAddress.City.ToString();
				}
				return returnValue;
			}
		}
		public ZString IdentificationNumber => ZString.Empty;
		public ZString IdentityType => TurkishConstants.IdentityType;
		public ZString PostCode
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (carrierAddress != null)
				{
					returnValue = carrierAddress.Postcode.ToString();
				}
				return returnValue;
			}
		}
		public ZString Phone
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (carrierAddress != null)
				{
					returnValue = carrierAddress.PhoneNumber.ToString();
				}
				return returnValue;
			}
		}
		public ZString CountryCode
		{
			get
			{
				ZString returnValue = ZString.Empty;
				var carrierCompany = this.carrierCompany;
				if (carrierCompany != null)
				{
					returnValue = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(carrierCompany.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, carrierAddress.Country.Code.SubstringSafe(0, 2), effectiveDate);
				}
				return returnValue;
			}
		}
		public ZString TaxOfficeCode => ZString.Empty;
	}
}
