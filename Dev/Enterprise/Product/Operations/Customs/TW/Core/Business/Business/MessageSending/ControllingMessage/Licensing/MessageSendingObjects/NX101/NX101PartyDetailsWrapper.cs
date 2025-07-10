using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX101PartyDetailsWrapper : LicensingMessagePartyDetailsWrapper
	{
		public NX101PartyDetailsWrapper(TWJobDocAddress jobDocAddress, ZBool isCertificate15)
			: base(jobDocAddress, isCertificate15)
		{
		}

		protected override ZString TypeCodeCore => GetTypeCodeBasedOnOrgCusCode(JobDocumentaryAddress.IDCodeType);

		protected override ZString ChineseNameCore
		{
			get
			{
				ZString result;
				if (JobDocumentaryAddress.E2_AddressOverride)
				{
					result = JobDocumentaryAddress.CompanyChineseName;
				}
				else if (orgAddress != null && orgAddress.Language == Core.SharedConstants.Languages.ChineseTraditional)
				{
					result = orgAddress.OA_CompanyNameOverride;
				}
				else
				{
					result = JobDocumentaryAddress.ChineseTranslatedAddress?.CompanyName ?? ZString.Empty;
				}
				return result;
			}
		}

		protected override IAddress AddressCore => new AddressWrapper(AddressLine, ChineseAddressLine, "", "", "");

		ZString ChineseAddressLine
		{
			get
			{
				var address1 = ZString.Empty;
				var address2 = ZString.Empty;
				var additionalAddressInfomation = ZString.Empty;
				var city = ZString.Empty;
				var state = ZString.Empty;
				var postcode = ZString.Empty;
				var countryName = ZString.Empty;
				if (JobDocumentaryAddress.E2_AddressOverride)
				{
					if (JobDocumentaryAddress.LocalAddress is JobDocAddress localAddress && !localAddress.E2_Address1.IsEmpty)
					{
						address1 = localAddress.E2_Address1;
						address2 = localAddress.E2_Address2;
						additionalAddressInfomation = localAddress.E2_AdditionalAddressInformation;
						city = localAddress.E2_City;
						state = localAddress.E2_State;
						if (!isCertificate15)
						{
							postcode = localAddress.E2_Postcode;
							countryName = (ZString?)localAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional) ?? ZString.Empty;
						}
					}
				}
				else if (orgAddress != null)
				{
					if (orgAddress.Language == Core.SharedConstants.Languages.ChineseTraditional)
					{
						address1 = orgAddress.OA_Address1;
						if (!address1.IsEmpty)
						{
							address2 = orgAddress.OA_Address2;
							city = orgAddress.OA_City;
							state = orgAddress.OA_State;
							if (!isCertificate15)
							{
								postcode = orgAddress.OA_PostCode;
								countryName = (ZString?)orgAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional) ?? ZString.Empty;
							}
						}
					}
					else
					{
						var translatedAddress = orgAddress.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.ChineseTraditional);
						if (translatedAddress != null)
						{
							address1 = translatedAddress.OTA_Address1;
							if (!address1.IsEmpty)
							{
								address2 = translatedAddress.OTA_Address2;
								city = translatedAddress.OTA_City;
								state = translatedAddress.OTA_State;
								if (!isCertificate15)
								{
									postcode = translatedAddress.OTA_PostCode;
									countryName = (ZString?)translatedAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional) ?? ZString.Empty;
								}
							}
						}
					}
				}

				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(postcode);
				stringBuilder.AppendIfNotEmpty(countryName);
				stringBuilder.AppendIfNotEmpty(state);
				stringBuilder.AppendIfNotEmpty(city);
				stringBuilder.AppendIfNotEmpty(address1);
				stringBuilder.AppendIfNotEmpty(address2);
				stringBuilder.AppendIfNotEmpty(additionalAddressInfomation);
				return stringBuilder.ToString().ToUpperInvariant();
			}
		}

		ZString AddressLine
		{
			get
			{
				var address1 = ZString.Empty;
				var address2 = ZString.Empty;
				var additionalAddressInfomation = ZString.Empty;
				var city = ZString.Empty;
				var state = ZString.Empty;
				var postcode = ZString.Empty;
				var countryName = ZString.Empty;
				if (JobDocumentaryAddress.E2_AddressOverride)
				{
					address1 = JobDocumentaryAddress.E2_Address1;
					if (!address1.IsEmpty)
					{
						address2 = JobDocumentaryAddress.E2_Address2;
						additionalAddressInfomation = JobDocumentaryAddress.E2_AdditionalAddressInformation;
						city = JobDocumentaryAddress.E2_City;
						state = JobDocumentaryAddress.E2_State;
						postcode = JobDocumentaryAddress.E2_Postcode;
						countryName = EnglishCountryName;
					}
				}
				else if (orgAddress != null && SharedHelper.GetEnglishLanguageCodes().Contains(orgAddress.Language))
				{
					address1 = orgAddress.OA_Address1;
					if (!address1.IsEmpty)
					{
						address2 = orgAddress.OA_Address2;
						city = orgAddress.OA_City;
						state = orgAddress.OA_State;
						postcode = orgAddress.OA_PostCode;
						countryName = (ZString?)orgAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(orgAddress.Language) ?? ZString.Empty;
					}
				}

				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(address1);
				stringBuilder.AppendIfNotEmpty(address2);
				stringBuilder.AppendIfNotEmpty(additionalAddressInfomation);
				stringBuilder.AppendIfNotEmpty(city);
				stringBuilder.AppendIfNotEmpty(state);
				stringBuilder.AppendIfNotEmpty(postcode);
				stringBuilder.AppendIfNotEmpty(countryName);
				return stringBuilder.ToStringWithDelimiterBetweenAppends(" ").ToUpperInvariant();
			}
		}

		ZString EnglishCountryName
		{
			get
			{
				var name = ZString.Empty;
				foreach (var language in SharedHelper.GetEnglishLanguageCodes())
				{
					name = (ZString?)JobDocumentaryAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(language) ?? ZString.Empty;
					if (!name.IsEmpty)
					{
						break;
					}
				}
				return name;
			}
		}
	}
}
