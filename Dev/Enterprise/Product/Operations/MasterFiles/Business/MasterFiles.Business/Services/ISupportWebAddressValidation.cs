using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business
{
	public interface ISupportWebAddressValidation
	{
		[List("LanguageList")]
		ZString Language { get; set; }
		ZPropertyInfo LanguageInfo { get; }
		int Language_MaxLength { get; }
		CodeDescriptionPairList LanguageList { get; }

		[List("AdditionalAddressInfoList")]
		ZString UnrestrictedAdditionalAddressInformation { get; set; }
		ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo { get; }
		CodeDescriptionPairList AdditionalAddressInfoList { get; }

		ZString AddressCode { get; set; }
		ZString Address1 { get; set; }
		ZPropertyInfo Address1Info { get; }
		int Address1_MaxLength { get; }

		ZString Address2 { get; set; }
		ZPropertyInfo Address2Info { get; }
		int Address2_MaxLength { get; }

		ZString City { get; set; }
		ZPropertyInfo CityInfo { get; }
		int City_MaxLength { get; }

		ZString Postcode { get; set; }
		ZPropertyInfo PostcodeInfo { get; }
		int Postcode_MaxLength { get; }

		ZString CompanyName { get; set; }
		ZPropertyInfo CompanyNameInfo { get; }
		int CompanyName_MaxLength { get; }

		[List("StateCodeList")]
		ZString StateCode { get; set; }
		ZPropertyInfo StateCodeInfo { get; }
		int StateCode_MaxLength { get; }
		CodeDescriptionPairList StateCodeList { get; }

		ZString State { get; set; }
		int State_MaxLength { get; }

		[List("CountryCodeList")]
		ZString CountryCodeISO2 { get; set; }

		int CountryCodeISO2_MaxLength { get; }
		RefCountry Country { get; }
		RefCountryCollection CountryCodeList { get; }

		ZString DisplayText { get; set; }
		ZGuid EntityPK { get; }
		bool GetReadOnlySecurity(PropertyDescriptor property);
		ZString AddressRecordGUID { get; }
		ZString AddressSourceTable { get; }
		ZString ValidationStatus { get; set; }
		ZString AddressMap { get; set; }
		ZString Addressee { get; }
		ZGeography GeoLocation { get; set; }
		ZString ClosestPort { get; set; }
		bool NeedValidation { get; }
		bool IsUpdatingCityTown { get; set; }
		bool IsValidatingAddress { get; set; }
		bool IsExactPointFound { get; set; }

		bool IsErrorSuppressed { get; }

		bool IsTSAKnownAddress { get; }

		bool IsMIDAddress { get; }

		bool IsJobDocAddress { get; }

		bool IsRowDeletedOrDetachedOrNull { get; }

		bool IsValidatedByBackgroundService { get; set; }

		event EventHandler TriggerWebAddressValidation;
		event EventHandler AddressValidationStatusChanged;
		event EventHandler TriggerWebGetCityTown;

		Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest);
		Task<CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken);
		void ResetValidationStatus(ZPropertyInfo propertyInfo);
		void PreValidationForAddressValidationService();
		void ValidatePostcodeAndStateForAddress();
		void ClearWebAddressValidationHandler();
		void ClearWebGetCityTownHandler();
	}
}
