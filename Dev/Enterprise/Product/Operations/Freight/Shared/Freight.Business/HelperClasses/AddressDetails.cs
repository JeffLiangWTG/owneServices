using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class AddressDetails
	{
		AddressDetails(IDocAddress docAddress)
		{
			Argument.NotNull(docAddress, nameof(docAddress));

			CompanyName = docAddress.E2_CompanyName;
			CompanyNameTruncated = docAddress.E2_CompanyNameTruncated;
			Address1 = docAddress.E2_Address1;
			Address2 = docAddress.E2_Address2;
			City = docAddress.E2_City;
			State = docAddress.E2_State;
			Postcode = docAddress.E2_Postcode;
		}

		AddressDetails(OrgTranslatedAddress translatedAddress)
		{
			Argument.NotNull(translatedAddress, nameof(translatedAddress));

			CompanyName = translatedAddress.OTA_CompanyName.IsEmpty ? translatedAddress.ParentAddress.EffectiveCompanyName : translatedAddress.OTA_CompanyName;
			CompanyNameTruncated = translatedAddress.OTA_CompanyName.IsEmpty ? translatedAddress.ParentAddress.EffectiveCompanyNameTruncated : translatedAddress.OTA_CompanyName.Substring(0, OrgAddress.Schema.OA_CompanyNameOverrideTruncatedLength);
			Address1 = translatedAddress.OTA_Address1;
			Address2 = translatedAddress.OTA_Address2;
			City = translatedAddress.OTA_City;
			State = translatedAddress.OTA_State;
			Postcode = translatedAddress.OTA_PostCode;
		}

		public static AddressDetails Get(IDocAddress address, bool preferEnglish)
		{
			if (address == null)
			{
				return null;
			}

			if (preferEnglish)
			{
				var realAddress = GetRealAddress(address);
				if (realAddress != null && !realAddress.IsEnglish)
				{
					var englishAddress = realAddress.TranslatedAddresses.FirstOrDefault(a => a.IsEnglish);
					if (englishAddress != null)
					{
						return new AddressDetails(englishAddress);
					}
				}
			}

			return new AddressDetails(address);
		}

		static OrgAddress GetRealAddress(IDocAddress address) => address as OrgAddress ?? (address is JobDocAddress jobDocAddress && !jobDocAddress.E2_AddressOverride && jobDocAddress.HasRealAddress ? jobDocAddress.Address : null);

		public readonly ZString CompanyName;
		public readonly ZString CompanyNameTruncated;
		public readonly ZString Address1;
		public readonly ZString Address2;
		public readonly ZString City;
		public readonly ZString State;
		public readonly ZString Postcode;
	}
}
